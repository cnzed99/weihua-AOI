using System;
using System.Threading;
using Modbus;
using WH.Entity.LogRecord;

namespace GearInfo
{
    /// <summary>
    /// 盘齿协议静态类。P2-2：挂 com、加载点位、G5 超时、轮询/心跳；F4 空实现。
    /// </summary>
    public static class CGearCommunicate
    {
        public static CModbusCommPart com;

        /// <summary>
        /// 工件 ID 静态缓存。默认 0；由轮询器在读到新 ID 且 >0 时更新。
        /// </summary>
        public static int CurrentProductID = 0;

        public static CGearProtocolPoints Points { get; private set; } = new CGearProtocolPoints();

        //【盘齿方案2-注释】原因：D6/G5 串行化全部 Modbus 读写（NModbus master 非线程安全）
        static readonly object _protocolLock = new object();

        static CCreateIDGearStation _idPoller;
        static Timer _heartBeatTimer;
        static bool _pollStarted;
        static bool _heartBeatStarted;

        const int GearTransportTimeoutMs = 150;
        const int GearTransportRetries = 2;

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
        /// LoadAsync 挂 com 之后调用：无论 com 是否 null 都尝试加载 JSON；com==null 不起轮询/心跳。不抛。
        /// </summary>
        public static void OnComAttached()
        {
            try
            {
                TryLoadProtocolPoints();

                if (com == null)
                {
                    TryLogInfo("Gear com 为空，跳过轮询/心跳");
                    return;
                }

                ApplyShortModbusTimeouts();
                StartIdPolling();
                StartHeartBeat();
            }
            catch (Exception ex)
            {
                TryLogError("Gear 初始化失败: " + ex.Message);
            }
        }

        /// <summary>
        /// 加载点位 JSON（相对运行目录 ..\SystemConfig\GearProtocolPoints.json）。失败不抛。
        /// </summary>
        public static bool TryLoadProtocolPoints()
        {
            try
            {
                bool ok = CGearProtocolPoints.TryLoad(out CGearProtocolPoints loaded);
                Points = loaded ?? new CGearProtocolPoints();
                string path = CGearProtocolPoints.GetResolvedPointsPath();
                if (ok)
                {
                    TryLogInfo("Gear 点位已加载: " + path);
                }
                else
                {
                    TryLogWarn("Gear 点位加载失败: " + path);
                }
                return ok;
            }
            catch (Exception ex)
            {
                Points = new CGearProtocolPoints();
                TryLogError("Gear 点位加载失败: " + CGearProtocolPoints.GetResolvedPointsPath() + " " + ex.Message);
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
                com.SetTransportTimeoutAndRetries(GearTransportTimeoutMs, GearTransportRetries);
            }
            catch (Exception ex)
            {
                TryLogWarn("Gear 设置 Modbus 超时失败: " + ex.Message);
            }
        }

        static void StartIdPolling()
        {
            if (_pollStarted)
            {
                return;
            }
            _idPoller = new CCreateIDGearStation();
            _idPoller.IntThread();
            _pollStarted = true;
            TryLogInfo("Gear 轮询已启动");
        }

        static void StartHeartBeat()
        {
            if (_heartBeatStarted)
            {
                return;
            }
            _heartBeatTimer = new Timer(HeartBeatTick, null, 1000, 1000);
            _heartBeatStarted = true;
            TryLogInfo("Gear 心跳已启动");
        }

        static void HeartBeatTick(object state)
        {
            try
            {
                SendHeartBeat();
            }
            catch (Exception ex)
            {
                TryLogWarn("Gear 心跳写失败: " + ex.Message);
            }
        }

        /// <summary>
        /// 按制程组名回写组结果。查 GroupResults 得点位名再查 Address；com==null 只打日志不抛。
        /// </summary>
        public static void SendGroupResult(string groupName, string id, GearResult result)
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

                if (!Points.TryGetPoint(pointName, out GearPointDef def) || def == null)
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
                if (Points == null || !Points.TryGetPoint("HeartBeat", out GearPointDef def) || def == null)
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
                TryLogWarn("Gear 心跳写失败: " + ex.Message);
            }
        }

        /// <summary>
        /// F4：自动测试开始。一期无线圈点位，空实现。
        /// </summary>
        public static void TestStart()
        {
            if (com == null)
            {
                return;
            }
        }

        /// <summary>
        /// F4：自动测试结束。一期无线圈点位，空实现。
        /// </summary>
        public static void TestFinish()
        {
            if (com == null)
            {
                return;
            }
        }

        /// <summary>
        /// 开机只写 HD1200 四路旋转张数。张数取工程制程 PhotoTotalCount。焦位不进点位；换料信号属方案8。每件不写。
        /// </summary>
        public static void SendRecipePhotoAndFocus(IEnumerable<(string processName, int photoTotalCount)> processes)
        {
            //【盘齿方案2-注释】只写 HD1200 四路；焦位不进点位；换料信号属方案8
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

                TryWriteProcessPhotoCount(byName, "内孔", "PhotoCount_Bore");
                TryWriteProcessPhotoCount(byName, "上轴侧面", "PhotoCount_ShaftTop");
                TryWriteProcessPhotoCount(byName, "下轴侧面", "PhotoCount_ShaftBottom");
                TryWriteProcessPhotoCount(byName, "整轴侧面", "PhotoCount_ShaftFull");

            }
            catch (Exception ex)
            {
                TryLogWarn("配方张数下发失败: " + ex.Message);
            }
        }

        static void TryWriteProcessPhotoCount(Dictionary<string, int> byName, string processName, string pointName)
        {
            if (byName == null || !byName.TryGetValue(processName, out int count))
            {
                TryLogWarn("找不到制程「" + processName + "」，跳过张数点 " + pointName);
                return;
            }
            if (Points == null || !Points.TryGetPoint(pointName, out GearPointDef def) || def == null)
            {
                TryLogWarn("找不到点位「" + pointName + "」，跳过制程「" + processName + "」张数下发");
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
                TryLogWarn("Gear 轮询读 ID 失败: " + ex.Message);
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