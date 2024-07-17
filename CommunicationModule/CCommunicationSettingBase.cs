//using System.ComponentModel;

//namespace CommunicationModule
//{
//    // [TypeConverter(typeof(PropertySorter))] //使用后无法Json序列化  弃用
//    public class CCommunicationSettingBase
//    {
//        public CCommunicationSettingBase()
//        {
//            Guid = System.Guid.NewGuid().ToString();
//        }

//        #region 信息

//        private string _paramName;

//        [Category("通讯基础信息")]
//        [DisplayName("1.名称")]
//        [Description("通讯名称")]
//        // [PropertyOrder(0)]
//        public string ParamName
//        {
//            get => _paramName;
//            set => _paramName = value;
//        }
//        private List<CtrlCam> _ctrlCamID = new List<CtrlCam>();

//        [Category("受控相机")]
//        [DisplayName("受控相机")]
//        [Description("添加受控相机及选择相机的合并关系")]
//        public List<CtrlCam> ControledCamID
//        {
//            get { return _ctrlCamID; }
//            set
//            {
//                if (value != null)
//                    _ctrlCamID = value;
//            }
//        }

//        private string _commType = "";

//        [Category("通讯基础信息")]
//        [DisplayName("2.通讯类型")]
//        [Description("通讯类型")]
//        [ReadOnly(true)]
//        //  [PropertyOrder(1)]
//        public string CommType
//        {
//            get { return _commType; }
//            set { _commType = value; }
//        }

//        private string _versions = "V1.0";

//        [Category("通讯基础信息")]
//        [DisplayName("3.当前版本")]
//        [Description("当前版本")]
//        [ReadOnly(true)]
//        //  [PropertyOrder(2)]
//        public string Versions
//        {
//            get { return _versions; }
//            set { _versions = value; }
//        }

//        /// <summary>
//        /// 唯一标识符
//        /// </summary>
//        public string Guid;

//        #endregion

//        #region 设置

//        private MACHINETYPE _connectDevice;

//        [Category("通讯基础设置")]
//        [DisplayName("1.连接设备")]
//        [Description("连接的设备")]
//        public MACHINETYPE ConnectDevice
//        {
//            get { return _connectDevice; }
//            set { _connectDevice = value; }
//        }

//        private string _remoteIP = "192.168.250.75";

//        [Category("通讯基础设置")]
//        [DisplayName("2.目标IP")]
//        [Description("目标IP地址")]
//        public string RemoteIP
//        {
//            get { return _remoteIP; }
//            set { _remoteIP = value; }
//        }

//        private uint _remotePort = 9702;

//        [Category("通讯基础设置")]
//        [DisplayName("3.目标端口")]
//        [Description("目标端口号")]
//        public uint RemotePort
//        {
//            get { return _remotePort; }
//            set { _remotePort = value; }
//        }

//        private bool _enable = true;

//        [Category("通讯基础设置")]
//        [DisplayName("4.是否启用")]
//        [Description("是否启用本通讯协议")]
//        public bool Enable
//        {
//            get { return _enable; }
//            set { _enable = value; }
//        }

//        private ENCODING _encodingType = ENCODING.NONE;

//        [Category("通讯基础设置")]
//        [DisplayName("5.默认转码类型")]
//        [Description("默认转码类型")]
//        public ENCODING EncodingType
//        {
//            get { return _encodingType; }
//            set { _encodingType = value; }
//        }

//        private SYSCONVERT _sysConvertType = SYSCONVERT.十进制;

//        [Category("通讯基础设置")]
//        [DisplayName("6.默认进制转换")]
//        [Description("默认进制转换")]
//        public SYSCONVERT SysConvertType
//        {
//            get { return _sysConvertType; }
//            set { _sysConvertType = value; }
//        }
//        #endregion

//        #region 数据协议
//        private List<CDataInfo<SignalTrigger>> _triggerProtocol =
//            new List<CDataInfo<SignalTrigger>>();

//        [Category("数据协议(接收)")]
//        [DisplayName("1.触发拍照")]
//        [Description("设置触发拍照时接收的数据协议")]
//        public List<CDataInfo<SignalTrigger>> TriggerProtocol
//        {
//            get => _triggerProtocol;
//            set => _triggerProtocol = value;
//        }

//        private List<CDataInfo<SignalReady>> _readyProtocol = new List<CDataInfo<SignalReady>>();

//        [Category("数据协议(发送)")]
//        [DisplayName("1.就绪清空")]
//        [Description("设置就绪清空时发送的数据协议")]
//        public List<CDataInfo<SignalReady>> ReadyProtocol
//        {
//            get => _readyProtocol;
//            set => _readyProtocol = value;
//        }

//        private List<CDataInfo<SignalReady>> _grabbingProtocol = new List<CDataInfo<SignalReady>>();

//        [Category("数据协议(发送)")]
//        [DisplayName("2.正在拍照中")]
//        [Description("设置正在拍照中发送的数据协议")]
//        public List<CDataInfo<SignalReady>> GrabbingProtocol
//        {
//            get => _grabbingProtocol;
//            set => _grabbingProtocol = value;
//        }

//        private List<CDataInfo<SignalReady>> _grabFinishProtocol =
//            new List<CDataInfo<SignalReady>>();

//        [Category("数据协议(发送)")]
//        [DisplayName("3.拍照完成")]
//        [Description("设置拍照完成后发送的数据协议")]
//        public List<CDataInfo<SignalReady>> GrabFinishProtocol
//        {
//            get => _grabFinishProtocol;
//            set => _grabFinishProtocol = value;
//        }

//        private List<CDataInfo<SignalDetectFinish>> _defectFinishProtocol =
//            new List<CDataInfo<SignalDetectFinish>>();

//        [Category("数据协议(发送)")]
//        [DisplayName("4.检测完成")]
//        [Description("设置检测完成后发送的数据协议")]
//        public List<CDataInfo<SignalDetectFinish>> DefectFinishProtocol
//        {
//            get => _defectFinishProtocol;
//            set => _defectFinishProtocol = value;
//        }
//        #endregion

//        public class CtrlCam
//        {
//            private string _cam = "";

//            [DisplayName("1.相机名")]
//            [Description("选择相机名")]
//            [TypeConverter(typeof(CamNameType))]
//            public string Cam
//            {
//                get => _cam;
//                set
//                {
//                    if (value.Contains("["))
//                    {
//                        value = value.Substring(0, value.IndexOf("["));
//                    }
//                    _cam = value;
//                }
//            }

//            private string _group = "分组0";

//            [DisplayName("2.结果分组")]
//            [Description("选择结果分组,同一组的检测结果合并发送")]
//            [TypeConverter(typeof(CamNameType))]
//            public string Group
//            {
//                get => _group;
//                set => _group = value;
//            }

//            public override string ToString()
//            {
//                string CamShow = CCameraManagement.CamParamDict.ContainsKey(Cam)
//                    ? $"{CCameraManagement.CamParamDict[Cam].SerialNumber}[{CCameraManagement.CamParamDict[Cam].CamStationName}]"
//                    : Cam;
//                return CamShow.IsEmpty()
//                    ? "请选择相机名..."
//                    : (Group.IsEmpty() ? CamShow : $"{CamShow}:{Group}");
//            }
//        }
//    }
//}
