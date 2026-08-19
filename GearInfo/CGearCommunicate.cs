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
        /// F2：按制程组结果回写。P2-4 再接线。com==null 直接返回。
        /// </summary>
        public static void SendGroupResult(string groupName, string id, GearResult result)
        {
            if (com == null)
            {
                return;
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
        /// F8：开机/换料写四路张数 + 两焦位。com==null 直接返回。P2-5 再实现。
        /// </summary>
        public static void SendRecipePhotoAndFocus()
        {
            if (com == null)
            {
                return;
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