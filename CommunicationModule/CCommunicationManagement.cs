//using System.Text;
//using log4net;

//namespace CommunicationModule
//{
//    public class CCommunicationManagement
//    {
//        private CCommunicationBase _communication;

//        /// <summary>
//        /// 通讯模块日志
//        /// </summary>
//        public static ILog ComLogger = new ClogSetting("Com", "Com").Create();

//        /// <summary>
//        /// 通讯操作字典
//        /// </summary>
//        public static Dictionary<string, CCommunicationBase> CommDic =
//            new Dictionary<string, CCommunicationBase>();
//        public static Dictionary<string, ICommunicate> ComHelper =
//            new Dictionary<string, ICommunicate>();

//        /// <summary>
//        /// 通讯参数操作字典
//        /// </summary>
//        public static Dictionary<string, CCommunicationSettingBase> CommParamDic =
//            new Dictionary<string, CCommunicationSettingBase>();

//        public CCommunicationManagement(List<CCommunicationSettingBase> param, string paramPath)
//        {
//            LoadComPlugs.LoadCom();
//            if (param != null)
//            {
//                for (int i = 0; i < param.Count; i++)
//                {
//                    ComLogger.Info($"开始读取{param[i].CommType}:" + param[i].ParamName + "参数");
//                    var Param = ComHelper[param[i].CommType].Init(paramPath, i, out _communication);
//                    if (Param != null)
//                    {
//                        ComLogger.Info($"添加{param[i].CommType}:" + param[i].ParamName + "到字典中");
//                        CommParamDic.Add(param[i].Guid, Param);
//                        CommDic.Add(param[i].Guid, _communication);
//                    }
//                    //    switch (_commParam[i].CommType)
//                    //    {
//                    //        case COMMUNCATIONTYPE.UDPIP:
//                    //            try
//                    //            {
//                    //                ComLogger.Info("开始读取UDP:" + name + "参数");
//                    //                List<CUdpIpCommunicationSetting> udpParam = new List<CUdpIpCommunicationSetting>();
//                    //                udpParam = ConfigAPI.Load<List<CUdpIpCommunicationSetting>>(paramPath);
//                    //                if (udpParam != null)
//                    //                {
//                    //                    ComLogger.Info("添加UDP:" + name + "到字典中");
//                    //                    CommParamDic.Add(name, (CUdpIpCommunicationSetting)udpParam[i]);
//                    //                    _communication = new UdpIpCommPart(udpParam[i]);
//                    //                    CommDic.Add(name, _communication);
//                    //                }
//                    //            }
//                    //            catch (Exception ex)
//                    //            {
//                    //                ComLogger.Error("读取UDP参数出错:"+ex.Message);
//                    //            }

//                    //            break;

//                    //        case COMMUNCATIONTYPE.TCPIP:
//                    //            try
//                    //            {
//                    //                ComLogger.Info("开始读取TCP:" + name + "参数");
//                    //                List<CTcpIpCommunicationSetting> tcpParam = new List<CTcpIpCommunicationSetting>();
//                    //                tcpParam = ConfigAPI.Load<List<CTcpIpCommunicationSetting>>(paramPath);
//                    //                if (tcpParam != null)
//                    //                {
//                    //                    ComLogger.Info("添加TCP:" + name + "到字典中");
//                    //                    CommParamDic.Add(name, (CTcpIpCommunicationSetting)tcpParam[i]);
//                    //                    _communication = new TcpIpCommPart(tcpParam[i]);
//                    //                    CommDic.Add(name, _communication);
//                    //                }
//                    //            }
//                    //            catch (Exception ex)
//                    //            {
//                    //                ComLogger.Error("读取TCP参数出错:" + ex.Message);
//                    //            }

//                    //            break;
//                    //    }
//                }
//            }
//        }

//        /// <summary>
//        /// 打开所有通讯
//        /// </summary>
//        public bool OpenAllComm()
//        {
//            ComLogger.Info("连接所有通讯:");
//            bool result = true;
//            if (CommDic.Count > 0)
//            {
//                if (CommDic != null)
//                {
//                    foreach (var Comm in CommDic)
//                    {
//                        ComLogger.Info("连接:" + CommParamDic[Comm.Key].ParamName);
//                        try
//                        {
//                            Comm.Value.Connect();
//                        }
//                        catch (Exception ex)
//                        {
//                            ComLogger.Info(
//                                $"{CommParamDic[Comm.Key].ParamName}连接服务器:{CommParamDic[Comm.Key].RemoteIP}失败:{ex.Message}"
//                            );
//                            // this.AddLogToListBox($"{CommParamDic[Comm.Key].ParamName}连接服务器:{CommParamDic[Comm.Key].RemoteIP}失败:{ex.Message}", LOG.LOG_WARN);
//                            result = false;
//                        }
//                    }
//                }
//            }

//            return result;
//        }

//        /// <summary>
//        /// 关闭所有通讯
//        /// </summary>
//        public void CloseAllComm()
//        {
//            if (CommDic.Count > 0)
//            {
//                ComLogger.Info("关闭所有通讯:");
//                if (CommDic != null)
//                {
//                    foreach (var Comm in CommDic)
//                    {
//                        ComLogger.Info("关闭:" + CommParamDic[Comm.Key].ParamName);
//                        try
//                        {
//                            Comm.Value.Close();
//                        }
//                        catch (Exception ex)
//                        {
//                            ComLogger.Info(
//                                $"{CommParamDic[Comm.Key].ParamName}关闭服务器:{CommParamDic[Comm.Key].RemoteIP}失败:{ex.Message}"
//                            );
//                            // this.AddLogToListBox($"{CommParamDic[Comm.Key].ParamName}关闭服务器:{CommParamDic[Comm.Key].RemoteIP}失败:{ex.Message}", LOG.LOG_WARN);
//                        }
//                    }
//                }
//            }
//        }

//        /// <summary>
//        /// 通讯最大数据长度
//        /// </summary>
//        public static readonly int MaxbuffLength = 1024;

//        /// <summary>
//        /// 获取触发拍照用的报文
//        /// </summary>
//        /// <param name="cam">触发的相机</param>
//        /// <returns></returns>
//        public static byte[] GetTriggerSignalByte(string cam)
//        {
//            byte[] result = new byte[MaxbuffLength];
//            if (GetComFromCam(cam, out string comFromCam))
//            {
//                List<CDataInfo<SignalTrigger>> protocolFind = CommParamDic[comFromCam]
//                    .TriggerProtocol.FindAll(c => c.Cam == "所有相机" || c.Cam == cam);

//                if (protocolFind.Count > 0)
//                {
//                    foreach (var item in protocolFind)
//                    {
//                        byte[] convert = item.GetBytes(item.DataValue);
//                        Array.Copy(convert, 0, result, item.DataIndex, convert.Length);
//                    }
//                }
//            }

//            return result;
//        }

//        /// <summary>
//        /// 获取自增的ID
//        /// </summary>
//        /// <param name="cam">触发的相机</param>
//        /// <returns></returns>
//        public static byte[] GetAutoID(string cam, int IDint)
//        {
//            byte[] result = new byte[128];
//            if (GetComFromCam(cam, out string comFromCam))
//            {
//                List<CDataInfo<SignalTrigger>> protocolFind = CommParamDic[comFromCam]
//                    .TriggerProtocol.FindAll(c => c.Cam == "所有相机" || c.Cam == cam);
//                if (protocolFind.Count > 0)
//                {
//                    CDataInfo<SignalTrigger> findidTrigger = protocolFind.Find(c =>
//                        c.SignalName == "相机触发信号"
//                    );
//                    if (findidTrigger != null)
//                    {
//                        byte[] convert = findidTrigger.GetBytes(findidTrigger.DataValue);
//                        Array.Copy(convert, 0, result, findidTrigger.DataIndex, convert.Length);
//                    }

//                    CDataInfo<SignalTrigger> findid = protocolFind.Find(c => c.SignalName == "流水号");
//                    if (findid != null)
//                    {
//                        string formatstr;
//                        if (findid.EncodingType == ENCODING.Unicode)
//                        {
//                            int Len = findid.DataLength / 2;
//                            formatstr = $"D{Len}";
//                        }
//                        else
//                        {
//                            formatstr = $"D{findid.DataLength}";
//                        }
//                        string convstr = IDint.ToString(formatstr);
//                        string sendstr = string.Empty;
//                        if (findid.SysConvertType == SYSCONVERT.十六进制)
//                        {
//                            byte[] hexbyte = Encoding.ASCII.GetBytes(convstr);
//                            string hexstr = HexConvert.ByteToHexString(hexbyte);
//                            sendstr = hexstr;
//                        }
//                        else
//                        {
//                            if (findid.EncodingType == ENCODING.NONE)
//                            {
//                                sendstr = string.Join(" ", convstr.Select(c => c.ToString()));
//                            }
//                            else
//                            {
//                                sendstr = convstr;
//                            }
//                        }
//                        byte[] byteArr = ConvertBytes.GetBytes(
//                            sendstr,
//                            findid.SysConvertType,
//                            findid.EncodingType
//                        );
//                        string ss = "";
//                        for (int i = 0; i < byteArr.Length; i++)
//                        {
//                            ss = ss + byteArr[i].ToString();
//                        }
//                        Console.WriteLine($"******************coypy前{ss}+++++++++++++");
//                        Array.Copy(byteArr, 0, result, findid.DataIndex, findid.DataLength);
//                    }
//                }
//            }

//            return result;
//        }

//        /// <summary>
//        /// 获取相机对应的通讯的流水号索引
//        /// </summary>
//        /// <param name="cam"></param>
//        /// <returns></returns>
//        public static int GetIdIndexFromCam(string cam)
//        {
//            if (GetComFromCam(cam, out string comFromCam))
//            {
//                var protocolFind = CommParamDic[comFromCam]
//                    .TriggerProtocol.FindAll(c =>
//                        (c.Cam == "所有相机" || c.Cam == cam) && c.SignalType == SignalTrigger.流水号
//                    );
//                if (protocolFind.Count > 0)
//                    return protocolFind[0].DataIndex;
//            }

//            return -1;
//        }

//        /// <summary>
//        /// 根据相机名获取通讯名
//        /// </summary>
//        /// <param name="cam">输入相机名</param>
//        /// <param name="com">返回通讯名</param>
//        /// <returns>获取成功返回true，不存在返回false</returns>
//        public static bool GetComFromCam(string cam, out string com)
//        {
//            com = "";
//            foreach (var Comm in CommParamDic)
//            {
//                if (Comm.Value.ControledCamID.Exists(c => c.Cam == cam))
//                {
//                    com = Comm.Key;
//                    return true;
//                }
//            }
//            return false;
//        }

//        public static List<string> GetTriggerCam(string com, byte[] buff)
//        {
//            List<string> resultCam = new List<string>();
//            foreach (var item in CommParamDic[com].ControledCamID)
//            {
//                if (CCommunicationManagement.CheckTriggerSignal(com, item.Cam, buff, out _, out _))
//                    resultCam.Add(item.Cam);
//            }
//            return resultCam;
//        }

//        /// <summary>
//        /// 检查是否收到触发信号
//        /// </summary>
//        /// <param name="com">通讯名</param>
//        /// <param name="cam">相机名</param>
//        /// <param name="buff"></param>
//        /// <param name="waferID">流水号</param>
//        /// <param name="otherRecvInfo">其他接收信息</param>
//        /// <returns>返回true，则收到触发信号，false，则没有收到</returns>
//        public static bool CheckTriggerSignal(
//            string com,
//            string cam,
//            byte[] buff,
//            out string waferID,
//            out Dictionary<string, string> otherRecvInfo
//        )
//        {
//            waferID = "";
//            otherRecvInfo = new Dictionary<string, string>();
//            bool bTrriger = false;
//            if (GetComFromCam(cam, out string comFromCam) && comFromCam == com)
//            {
//                var protocolFind = CommParamDic[com]
//                    .TriggerProtocol.FindAll(c => c.Cam == "所有相机" || c.Cam == cam);
//                if (protocolFind.Count > 0)
//                {
//                    foreach (var c in protocolFind)
//                    {
//                        switch (c.SignalName)
//                        {
//                            case "相机触发信号":
//                                if (buff.Length >= c.DataIndex + c.DataLength)
//                                    bTrriger =
//                                        c.GetString(
//                                            buff.ToList()
//                                                .GetRange(c.DataIndex, c.DataLength)
//                                                .ToArray()
//                                        ) == c.DataValue;
//                                break;
//                            case "流水号":
//                                if (buff.Length >= c.DataIndex + c.DataLength)
//                                    waferID = c.GetString(
//                                            buff.ToList()
//                                                .GetRange(c.DataIndex, c.DataLength)
//                                                .ToArray()
//                                        )
//                                        .Replace("\0", "");
//                                break;
//                            default:
//                                if (buff.Length >= c.DataIndex + c.DataLength)
//                                    otherRecvInfo.Add(
//                                        c.SignalName,
//                                        c.GetString(
//                                                buff.ToList()
//                                                    .GetRange(c.DataIndex, c.DataLength)
//                                                    .ToArray()
//                                            )
//                                            .Replace("\0", "")
//                                    );
//                                break;
//                        }
//                    }
//                }
//            }
//            return bTrriger;
//        }

//        private static void BaseSignalSend(
//            List<CDataInfo<SignalReady>> protocol,
//            string com,
//            string cam,
//            string waferID,
//            Dictionary<string, string> otherSendInfo
//        )
//        {
//            var protocolFind = protocol.FindAll(c => c.Cam == "所有相机" || c.Cam == cam);
//            if (protocolFind.Count > 0)
//            {
//                byte[] buff = new byte[MaxbuffLength];

//                int maxLength = 0;
//                byte[] convert;
//                foreach (var c in protocolFind)
//                {
//                    switch (c.SignalName)
//                    {
//                        case "指令信号":
//                            convert = c.GetBytes(c.DataValue);
//                            Array.Copy(convert, 0, buff, c.DataIndex, convert.Length);
//                            break;
//                        case "流水号":
//                            convert = c.GetBytes(waferID);
//                            Array.Copy(convert, 0, buff, c.DataIndex, convert.Length);
//                            break;
//                        default:
//                            convert = c.GetBytes(
//                                otherSendInfo.ContainsKey(c.SignalName)
//                                    ? otherSendInfo[c.SignalName]
//                                    : "0"
//                            );
//                            Array.Copy(convert, 0, buff, c.DataIndex, convert.Length);
//                            break;
//                    }
//                    maxLength = Math.Max(maxLength, c.DataIndex + c.DataLength);
//                }
//                byte[] buff2 = new byte[maxLength + 1];
//                Array.Copy(buff, buff2, maxLength + 1);
//                CommDic[com].SendData(buff2);
//            }
//        }

//        /// <summary>
//        /// 发送就绪信号
//        /// </summary>
//        /// <param name="cam">相机名</param>
//        /// <param name="waferID">流水号</param>
//        /// /// <param name="otherSendInfo">其他发送信息</param>
//        public static void SendReadySignal(
//            string cam,
//            string waferID,
//            Dictionary<string, string> otherSendInfo
//        )
//        {
//            if (GetComFromCam(cam, out string com))
//                BaseSignalSend(CommParamDic[com].ReadyProtocol, com, cam, waferID, otherSendInfo);
//        }

//        /// <summary>
//        /// 发送拍照中信号
//        /// </summary>
//        /// <param name="cam">相机名</param>
//        /// <param name="waferID">流水号</param>
//        /// <param name="otherSendInfo">其他发送信息</param>
//        public static void SendGrabbingSignal(
//            string cam,
//            string waferID,
//            Dictionary<string, string> otherSendInfo
//        )
//        {
//            if (GetComFromCam(cam, out string com))
//                BaseSignalSend(
//                    CommParamDic[com].GrabbingProtocol,
//                    com,
//                    cam,
//                    waferID,
//                    otherSendInfo
//                );
//        }

//        /// <summary>
//        /// 发送拍照完成信号
//        /// </summary>
//        /// <param name="cam">相机名</param>
//        /// <param name="waferID">流水号</param>
//        /// /// <param name="otherSendInfo">其他发送信息</param>
//        public static void SendGrabFinishSignal(
//            string cam,
//            string waferID,
//            Dictionary<string, string> otherSendInfo
//        )
//        {
//            if (GetComFromCam(cam, out string com))
//                BaseSignalSend(
//                    CommParamDic[com].GrabFinishProtocol,
//                    com,
//                    cam,
//                    waferID,
//                    otherSendInfo
//                );
//        }

//        /// <summary>
//        /// 发送检测完成信号
//        /// </summary>
//        /// <param name="cam">相机名</param>
//        /// <param name="waferID">流水号</param>
//        /// <param name="qualityLevel">质量信号</param>
//        /// <param name="colorLevel">颜色信号</param>
//        /// /// <param name="otherSendInfo">其他发送信息</param>
//        public static void SendDetectionFinishSignal(
//            string cam,
//            string waferID,
//            int qualityLevel,
//            int colorLevel,
//            Dictionary<string, string> otherSendInfo
//        )
//        {
//            if (GetComFromCam(cam, out string com))
//            {
//                try
//                {
//                    string group = CommParamDic[com].ControledCamID.Find(c => c.Cam == cam).Group;
//                    List<string> cams = new List<string>();
//                    foreach (var item in CommParamDic[com].ControledCamID)
//                    {
//                        if (item.Group == group)
//                            cams.Add(item.Cam);
//                    }

//                    _evSendDetectionFinishSignal.WaitOne();
//                    CommDic[com]
//                        ._lsdefectResult.Add(
//                            (cam, waferID, qualityLevel, colorLevel, otherSendInfo)
//                        );

//                    foreach (var item in cams)
//                    {
//                        if (
//                            !CommDic[com]
//                                ._lsdefectResult.Exists(c => c.cam == item && c.waferID == waferID)
//                        )
//                            return;
//                    }
//                    byte[] buff = new byte[MaxbuffLength];
//                    int maxLength = 0;
//                    byte[] convert;
//                    var protocolFind = CommParamDic[com]
//                        .DefectFinishProtocol.FindAll(c => c.Cam == "所有相机");
//                    foreach (var c in protocolFind)
//                    {
//                        switch (c.SignalName)
//                        {
//                            case "指令信号":
//                                convert = c.GetBytes(c.DataValue);
//                                Array.Copy(convert, 0, buff, c.DataIndex, convert.Length);
//                                break;
//                            case "流水号":
//                                convert = c.GetBytes(waferID);
//                                Array.Copy(convert, 0, buff, c.DataIndex, convert.Length);
//                                break;
//                            case "质量信号":
//                                convert = c.GetBytes(qualityLevel.ToString());
//                                Array.Copy(convert, 0, buff, c.DataIndex, convert.Length);
//                                break;
//                            case "颜色信号":
//                                convert = c.GetBytes(colorLevel.ToString());
//                                Array.Copy(convert, 0, buff, c.DataIndex, convert.Length);
//                                break;
//                            default:
//                                convert = c.GetBytes(
//                                    otherSendInfo.ContainsKey(c.SignalName)
//                                        ? otherSendInfo[c.SignalName]
//                                        : "0"
//                                );
//                                Array.Copy(convert, 0, buff, c.DataIndex, convert.Length);
//                                break;
//                        }
//                        maxLength = Math.Max(maxLength, c.DataIndex + c.DataLength);
//                    }

//                    foreach (var item in cams)
//                    {
//                        var result = CommDic[com]
//                            ._lsdefectResult.Find(c =>
//                                (string)c.cam == item && c.waferID == waferID
//                            );
//                        protocolFind = CommParamDic[com]
//                            .DefectFinishProtocol.FindAll(c => c.Cam == item);
//                        foreach (var c in protocolFind)
//                        {
//                            switch (c.SignalName)
//                            {
//                                case "指令信号":
//                                    convert = c.GetBytes(c.DataValue);
//                                    Array.Copy(convert, 0, buff, c.DataIndex, convert.Length);
//                                    break;
//                                case "流水号":
//                                    convert = c.GetBytes(result.waferID);
//                                    Array.Copy(convert, 0, buff, c.DataIndex, convert.Length);
//                                    break;
//                                case "质量信号":
//                                    convert = c.GetBytes(result.qualityLevel.ToString());
//                                    Array.Copy(convert, 0, buff, c.DataIndex, convert.Length);
//                                    break;
//                                case "颜色信号":
//                                    convert = c.GetBytes(result.colorLevel.ToString());
//                                    Array.Copy(convert, 0, buff, c.DataIndex, convert.Length);
//                                    break;
//                                default:
//                                    convert = c.GetBytes(
//                                        otherSendInfo.ContainsKey(c.SignalName)
//                                            ? otherSendInfo[c.SignalName]
//                                            : "0"
//                                    );
//                                    Array.Copy(convert, 0, buff, c.DataIndex, convert.Length);
//                                    break;
//                            }
//                            maxLength = Math.Max(maxLength, c.DataIndex + c.DataLength);
//                        }
//                        CommDic[com]._lsdefectResult.Remove(result);
//                    }

//                    byte[] buff2 = new byte[maxLength + 1];
//                    Array.Copy(buff, buff2, maxLength + 1);
//                    CommDic[com].SendData(buff2);
//                }
//                catch (Exception)
//                {
//                    throw;
//                }
//                finally
//                {
//                    _evSendDetectionFinishSignal.Set();
//                }
//            }
//        }

//        /// <summary>
//        /// 防止多个制程同时访问
//        /// </summary>
//        private static AutoResetEvent _evSendDetectionFinishSignal = new AutoResetEvent(true);
//    }
//}
