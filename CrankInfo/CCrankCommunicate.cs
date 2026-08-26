using System;
using System.Threading;
using Modbus;
using WH.Entity.LogRecord;

namespace CrankInfo
{
    /// <summary>
    /// 【曲轴方案2-注释】曲轴协议静态类。挂 com、加载点位、锁与短超时、ID 轮询/心跳；不写 HD1200。
    /// </summary>
    public static class CCrankCommunicate
    {
        public static CModbusCommPart com;

        /// <summary>
        /// 工件 ID 静态缓存。默认 0；由轮询器在读到新 ID 且 >0 时更新。
        /// </summary>
        public static int CurrentProductID = 0;

        public static CCrankProtocolPoints Points { get; private set; } = new CCrankProtocolPoints();

        //【曲轴方案2-注释】原因：D6 串行化全部 Modbus 读写（NModbus master 非线程安全）
        static readonly object _protocolLock = new object();

        static CCreateIDCrankStation _idPoller;
        static Timer _heartBeatTimer;
        static bool _pollStarted;
        static bool _heartBeatStarted;

        const int CrankTransportTimeoutMs = 150;
        const int CrankTransportRetries = 2;

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
        /// OpenProj 识别为曲轴并挂 com 之后调用：无论 com 是否 null 都尝试加载 JSON；com==null 不起轮询/心跳。不抛。
        /// </summary>
        public static void OnComAttached()
        {
            try
            {
                TryLoadProtocolPoints();

                if (com == null)
                {
                    TryLogInfo("Crank com 为空，跳过轮询/心跳");
                    return;
                }

                ApplyShortModbusTimeouts();
                StartIdPolling();
                StartHeartBeat();
            }
            catch (Exception ex)
            {
                TryLogError("Crank 初始化失败: " + ex.Message);
            }
        }

        /// <summary>
        /// 【曲轴方案2-注释】换工程/关闭时停心跳与 ID 轮询，清空 com，不关底层 OpenAllComm。
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
                TryLogInfo("Crank 已卸载协议");
            }
            catch (Exception ex)
            {
                TryLogWarn("Crank Detach 失败: " + ex.Message);
            }
        }

        /// <summary>
        /// 加载点位 JSON（相对运行目录 ..\SystemConfig\CrankProtocolPoints.json）。失败不抛。
        /// </summary>
        public static bool TryLoadProtocolPoints()
        {
            try
            {
                bool ok = CCrankProtocolPoints.TryLoad(out CCrankProtocolPoints loaded);
                Points = loaded ?? new CCrankProtocolPoints();
                string path = CCrankProtocolPoints.GetResolvedPointsPath();
                if (ok)
                {
                    TryLogInfo("Crank 点位已加载: " + path);
                }
                else
                {
                    TryLogWarn("Crank 点位加载失败: " + path);
                }
                return ok;
            }
            catch (Exception ex)
            {
                Points = new CCrankProtocolPoints();
                TryLogError("Crank 点位加载失败: " + CCrankProtocolPoints.GetResolvedPointsPath() + " " + ex.Message);
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
                com.SetTransportTimeoutAndRetries(CrankTransportTimeoutMs, CrankTransportRetries);
            }
            catch (Exception ex)
            {
                TryLogWarn("Crank 设置 Modbus 超时失败: " + ex.Message);
            }
        }

        static void StartIdPolling()
        {
            if (_pollStarted)
            {
                return;
            }
            _idPoller = new CCreateIDCrankStation();
            _idPoller.IntThread();
            _pollStarted = true;
            TryLogInfo("Crank 轮询已启动");
        }

        static void StartHeartBeat()
        {
            if (_heartBeatStarted)
            {
                return;
            }
            _heartBeatTimer = new Timer(HeartBeatTick, null, 1000, 1000);
            _heartBeatStarted = true;
            TryLogInfo("Crank 心跳已启动");
        }

        static void HeartBeatTick(object state)
        {
            try
            {
                SendHeartBeat();
            }
            catch (Exception ex)
            {
                TryLogWarn("Crank 心跳写失败: " + ex.Message);
            }
        }

        /// <summary>
        /// 按制程组名回写组结果。查 GroupResults 得点位名再查 Address；com==null 只打日志不抛。
        /// G1=制程组1（端面+底部光滑面）；G2=制程组2（四面）。
        /// </summary>
        public static void SendGroupResult(string groupName, string id, CrankResult result)
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

                if (!Points.TryGetPoint(pointName, out CrankPointDef def) || def == null)
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
                if (Points == null || !Points.TryGetPoint("HeartBeat", out CrankPointDef def) || def == null)
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
                TryLogWarn("Crank 心跳写失败: " + ex.Message);
            }
        }

        /// <summary>
        /// 轮询读 ProductID 寄存器。走协议锁。失败不抛、不把缓存改成垃圾值。
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
                TryLogWarn("Crank 轮询读 ID 失败: " + ex.Message);
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
