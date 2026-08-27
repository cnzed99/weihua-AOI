using System;
using System.Threading;
using Modbus;
using WH.Entity.LogRecord;

namespace XinGearInfo
{
    /// <summary>
    /// 【新兴盘齿方案2-注释】协议静态类。挂 com、加载点位、锁与短超时、ID 轮询/心跳；张数由 Host 开机写一次，不在 OnComAttached 写。
    /// </summary>
    public static class CXinGearCommunicate
    {
        public static CModbusCommPart com;

        /// <summary>
        /// 工件 ID 静态缓存。默认 0；由轮询器在读到新 ID 且 >0 时更新。
        /// </summary>
        public static int CurrentProductID = 0;

        public static CXinGearProtocolPoints Points { get; private set; } = new CXinGearProtocolPoints();

        //【新兴盘齿方案2-注释】原因：D6 串行化全部 Modbus 读写（NModbus master 非线程安全）
        static readonly object _protocolLock = new object();

        static CCreateIDXinGearStation _idPoller;
        static Timer _heartBeatTimer;
        static bool _pollStarted;
        static bool _heartBeatStarted;

        const int XinGearTransportTimeoutMs = 150;
        const int XinGearTransportRetries = 2;

        /// <summary>
        /// F1：读缓存，零次 Modbus。
        /// </summary>
        public static int GetProductID()
        {
            return CurrentProductID;
        }

        /// <summary>
        /// F1：out 重载，对齐方案用词。
        /// </summary>
        public static void GetProductID(out int productID)
        {
            productID = CurrentProductID;
        }

        /// <summary>
        /// OpenProj 识别为新兴盘齿并挂 com 之后调用：无论 com 是否 null 都尝试加载 JSON；com==null 不起轮询/心跳。不抛。
        /// </summary>
        public static void OnComAttached()
        {
            try
            {
                TryLoadProtocolPoints();

                if (com == null)
                {
                    TryLogInfo("新兴盘齿 com 为空，跳过轮询/心跳");
                    return;
                }

                ApplyShortModbusTimeouts();
                StartIdPolling();
                StartHeartBeat();
            }
            catch (Exception ex)
            {
                TryLogError("新兴盘齿 初始化失败: " + ex.Message);
            }
        }

        /// <summary>
        /// 【新兴盘齿方案2-注释】换工程/关闭时停心跳与 ID 轮询，清空 com，不关底层 OpenAllComm。
        /// </summary>
        public static void Detach()
        {
            try
            {
                _heartBeatStarted = false;
                if (_heartBeatTimer != null)
                {
                    _heartBeatTimer.Dispose();
                    _heartBeatTimer = null;
                }
                if (_idPoller != null)
                {
                    _idPoller.StopThread();
                    _idPoller = null;
                }
                _pollStarted = false;
                com = null;
                CurrentProductID = 0;
                TryLogInfo("新兴盘齿 已卸载协议");
            }
            catch (Exception ex)
            {
                TryLogWarn("新兴盘齿 Detach 失败: " + ex.Message);
            }
        }

        /// <summary>
        /// 加载点位 JSON（相对运行目录 ..\SystemConfig\XinGearProtocolPoints.json）。失败不抛。
        /// </summary>
        public static bool TryLoadProtocolPoints()
        {
            try
            {
                bool ok = CXinGearProtocolPoints.TryLoad(out CXinGearProtocolPoints loaded);
                Points = loaded ?? new CXinGearProtocolPoints();
                string path = CXinGearProtocolPoints.GetResolvedPointsPath();
                if (ok)
                {
                    TryLogInfo("新兴盘齿 点位已加载: " + path);
                }
                else
                {
                    TryLogWarn("新兴盘齿 点位加载失败: " + path);
                }
                return ok;
            }
            catch (Exception ex)
            {
                Points = new CXinGearProtocolPoints();
                TryLogError("新兴盘齿 点位加载失败: " + CXinGearProtocolPoints.GetResolvedPointsPath() + " " + ex.Message);
                return false;
            }
        }

        static void ApplyShortModbusTimeouts()
        {
            try
            {
                if (com == null)
                {
                    return;
                }
                com.SetTransportTimeoutAndRetries(XinGearTransportTimeoutMs, XinGearTransportRetries);
            }
            catch (Exception ex)
            {
                TryLogWarn("新兴盘齿 设置 Modbus 超时失败: " + ex.Message);
            }
        }

        static void StartIdPolling()
        {
            if (_pollStarted)
            {
                return;
            }
            _idPoller = new CCreateIDXinGearStation();
            _idPoller.IntThread();
            _pollStarted = true;
            TryLogInfo("新兴盘齿 轮询已启动");
        }

        static void StartHeartBeat()
        {
            if (_heartBeatStarted)
            {
                return;
            }
            _heartBeatTimer = new Timer(HeartBeatTick, null, 1000, 1000);
            _heartBeatStarted = true;
            TryLogInfo("新兴盘齿 心跳已启动");
        }

        static void HeartBeatTick(object state)
        {
            try
            {
                SendHeartBeat();
            }
            catch (Exception ex)
            {
                TryLogWarn("新兴盘齿 心跳写失败: " + ex.Message);
            }
        }

        /// <summary>
        /// 按制程组名回写组结果。查 GroupResults 得点位名再查 Address；com==null 只打日志不抛。
        /// G1=制程组1（端面+底部光滑面）；G2=制程组2（四面）。
        /// </summary>
        public static void SendGroupResult(string groupName, string id, XinGearResult result)
        {
            if (com == null)
            {
                TryLogWarn("无PLC，跳过组结果回写 groupName=" + groupName + " id=" + id + " result=" + result);
                return;
            }

            try
            {
                if (Points == null || Points.GroupResults == null)
                {
                    TryLogWarn("组结果点位未加载，跳过回写 groupName=" + groupName + " id=" + id + " result=" + result);
                    return;
                }

                if (string.IsNullOrEmpty(groupName)
                    || !Points.GroupResults.TryGetValue(groupName, out string pointName)
                    || string.IsNullOrEmpty(pointName))
                {
                    TryLogWarn("未找到制程组结果映射，跳过回写 groupName=" + groupName + " id=" + id + " result=" + result);
                    return;
                }

                if (!Points.TryGetPoint(pointName, out XinGearPointDef def) || def == null)
                {
                    TryLogWarn("未找到组结果点位定义，跳过回写 groupName=" + groupName + " point=" + pointName + " id=" + id + " result=" + result);
                    return;
                }

                if (def.Address < 0 || def.Address > ushort.MaxValue)
                {
                    TryLogWarn("组结果点位地址无效，跳过回写 groupName=" + groupName + " point=" + pointName + " addr=" + def.Address);
                    return;
                }

                WriteHoldingInt32Locked((ushort)def.Address, (int)result);
            }
            catch (Exception ex)
            {
                TryLogWarn("组结果回写失败 groupName=" + groupName + " id=" + id + " result=" + result + " " + ex.Message);
            }
        }

        /// <summary>
        /// F3：心跳写 HeartBeat 点位为 1。com==null 直接返回。走协议锁。
        /// </summary>
        public static void SendHeartBeat()
        {
            if (com == null)
            {
                return;
            }

            try
            {
                if (Points == null || !Points.TryGetPoint("HeartBeat", out XinGearPointDef def) || def == null)
                {
                    return;
                }
                if (!def.Enabled)
                {
                    return;
                }
                if (def.Address < 0 || def.Address > ushort.MaxValue)
                {
                    return;
                }

                WriteHoldingInt32Locked((ushort)def.Address, 1);
            }
            catch (Exception ex)
            {
                TryLogWarn("新兴盘齿 心跳写失败: " + ex.Message);
            }
        }

        /// <summary>
        /// 【新兴盘齿方案2-注释】 开机/换工程写三路张数一次：齿底/齿顶/侧面 -> PhotoCount_Bottom/Top/Side。每件不写。无 PLC 只日志。
        /// </summary>
        public static void SendRecipePhotoCount(IEnumerable<(string processName, int photoTotalCount)> processes)
        {
            if (com == null)
            {
                TryLogInfo("无PLC，跳过配方张数下发");
                return;
            }

            try
            {
                Dictionary<string, int> byName = new Dictionary<string, int>();
                if (processes != null)
                {
                    foreach ((string processName, int photoTotalCount) item in processes)
                    {
                        if (string.IsNullOrEmpty(item.processName))
                        {
                            continue;
                        }
                        byName[item.processName] = item.photoTotalCount;
                    }
                }

                TryWriteProcessPhotoCount(byName, "齿底", "PhotoCount_Bottom", 1);
                TryWriteProcessPhotoCount(byName, "齿顶", "PhotoCount_Top", 1);
                TryWriteProcessPhotoCount(byName, "侧面", "PhotoCount_Side", 4);
            }
            catch (Exception ex)
            {
                TryLogWarn("配方张数下发失败: " + ex.Message);
            }
        }

        static void TryWriteProcessPhotoCount(Dictionary<string, int> byName, string processName, string pointName, int fallback)
        {
            int count = fallback;
            if (byName != null && byName.TryGetValue(processName, out int configured) && configured > 0)
            {
                count = configured;
            }
            if (Points == null || !Points.TryGetPoint(pointName, out XinGearPointDef def) || def == null)
            {
                TryLogWarn("未找到点位「" + pointName + "」，跳过制程「" + processName + "」张数下发");
                return;
            }
            if (!def.Enabled)
            {
                TryLogWarn("点位「" + pointName + "」未启用，跳过制程「" + processName + "」张数下发");
                return;
            }
            if (def.Address < 0 || def.Address > ushort.MaxValue)
            {
                TryLogWarn("点位「" + pointName + "」地址无效 addr=" + def.Address + "，跳过张数下发");
                return;
            }
            WriteHoldingInt32Locked((ushort)def.Address, count);
        }

        /// <summary>
        /// 查询读 ProductID 寄存器。走协议锁。失败不抛。不改已缓存的旧值。
        /// </summary>
internal static bool TryReadProductIdRegister(ushort address, out int productID)
        {
            productID = 0;
            try
            {
                lock (_protocolLock)
                {
                    if (com == null)
                    {
                        return false;
                    }
                    productID = com.ReadHoldingRegisterInt32(address);
                    return true;
                }
            }
            catch (Exception ex)
            {
                TryLogWarn("新兴盘齿 轮询读 ID 失败: " + ex.Message);
                return false;
            }
        }

        internal static void WriteHoldingInt32Locked(ushort address, int value)
        {
            lock (_protocolLock)
            {
                if (com == null)
                {
                    return;
                }
                com.WriteSingleRegisterInt32(address, value);
            }
        }

        static void TryLogInfo(string message)
        {
            try
            {
                CLogRec.Default.Info(message);
            }
            catch
            {
            }
        }

        static void TryLogWarn(string message)
        {
            try
            {
                CLogRec.Default.Warn(message);
            }
            catch
            {
            }
        }

        static void TryLogError(string message)
        {
            try
            {
                CLogRec.Default.Error(message);
            }
            catch
            {
            }
        }
    }
}
