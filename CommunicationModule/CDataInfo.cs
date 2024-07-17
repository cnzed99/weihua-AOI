//using System;
//using System.Collections.Generic;
//using System.ComponentModel;
//using System.Globalization;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using static CommunicationModule.CCommunicationSettingBase;

//namespace CommunicationModule
//{
//    public class CDataInfo<T>
//        where T : Enum
//    {
//        public CDataInfo()
//        {
//            _SignalName = _SignalType.ToString();
//            if (
//                OpenCommunication._Name != null
//                && CCommunicationManagement.CommParamDic.ContainsKey(OpenCommunication._Name)
//            )
//            {
//                _sysConvertType = CCommunicationManagement
//                    .CommParamDic[OpenCommunication._Name]
//                    .SysConvertType;
//                _encodingType = CCommunicationManagement
//                    .CommParamDic[OpenCommunication._Name]
//                    .EncodingType;
//            }
//        }

//        private T _SignalType;

//        [Category("基本参数")]
//        [Description("字段类型")]
//        [DisplayName("1.字段类型")]
//        public T SignalType
//        {
//            get { return _SignalType; }
//            set
//            {
//                _SignalType = value;
//                _SignalName = _SignalType.ToString();
//            }
//        }

//        private string _SignalName = "";

//        [Category("基本参数")]
//        [Description("字段名")]
//        [DisplayName("2.字段名")]
//        public string SignalName
//        {
//            get { return _SignalName; }
//            set
//            {
//                if (_SignalType.ToString() == "可编辑信号")
//                {
//                    _SignalName = value;
//                }
//            }
//        }

//        private int _index = 0;

//        [Category("基本参数")]
//        [Description("字段索引")]
//        [DisplayName("3.字段索引")]
//        public int DataIndex
//        {
//            get { return _index; }
//            set
//            {
//                if (value + DataLength >= CCommunicationManagement.MaxbuffLength)
//                    ClsNotification.NotifyError(
//                        $"索引加长度超出最大数据长度{CCommunicationManagement.MaxbuffLength}!",
//                        3000
//                    );
//                else
//                    _index = value;
//            }
//        }

//        private string _value = "0";

//        [Category("基本参数")]
//        [Description("字段值")]
//        [DisplayName("4.字段值")]
//        public string DataValue
//        {
//            get { return _value; }
//            set { _value = value; }
//        }

//        private int _length = 1;

//        [Category("基本参数")]
//        [Description("字段长度")]
//        [DisplayName("5.字段长度")]
//        public int DataLength
//        {
//            get { return _length; }
//            set
//            {
//                if (value + DataIndex >= CCommunicationManagement.MaxbuffLength)
//                    ClsNotification.NotifyError(
//                        $"索引加长度超出最大数据长度{CCommunicationManagement.MaxbuffLength}!",
//                        3000
//                    );
//                else
//                    _length = value;
//            }
//        }

//        private ENCODING _encodingType = ENCODING.ASCII;

//        [Category("基本参数")]
//        [DisplayName("6.转码类型")]
//        [Description("转码类型")]
//        public ENCODING EncodingType
//        {
//            get { return _encodingType; }
//            set { _encodingType = value; }
//        }

//        private SYSCONVERT _sysConvertType = SYSCONVERT.十进制;

//        [Category("基本参数")]
//        [DisplayName("7.进制转换")]
//        [Description("进制转换")]
//        public SYSCONVERT SysConvertType
//        {
//            get { return _sysConvertType; }
//            set { _sysConvertType = value; }
//        }

//        private string _note = "";

//        [Category("基本参数")]
//        [Description("说明")]
//        [DisplayName("8.说明")]
//        public string Note
//        {
//            get { return _note; }
//            set { _note = value; }
//        }

//        private string _cam = "所有相机";

//        [Category("应用相机")]
//        [Description("选择应用相机名")]
//        [DisplayName("应用到")]
//        [TypeConverter(typeof(CamNameType))]
//        public string Cam
//        {
//            get { return _cam; }
//            set
//            {
//                if (value.Contains("["))
//                {
//                    value = value.Substring(0, value.IndexOf("["));
//                }
//                _cam = value;
//            }
//        }

//        public override string ToString()
//        {
//            string CamShow = CCameraManagement.CamParamDict.ContainsKey(Cam)
//                ? CCameraManagement.CamParamDict[Cam].CamStationName
//                : Cam;
//            return CamShow
//                + ":"
//                + this.SignalName
//                + ":"
//                + DataIndex.ToString()
//                + ":"
//                + this.DataValue.ToString();
//        }

//        public byte[] GetBytes(string str)
//        {
//            return ConvertBytes.GetBytes(str, SysConvertType, EncodingType);
//        }

//        public string GetString(byte[] bytes)
//        {
//            return ConvertBytes.GetString(bytes, SysConvertType, EncodingType);
//        }
//    }

//    public enum SignalTrigger
//    {
//        相机触发信号 = 0,
//        流水号 = 1,
//        可编辑信号 = 2
//    }

//    public enum SignalReady
//    {
//        指令信号 = 0,
//        流水号 = 1,
//        可编辑信号 = 2
//    }

//    public enum SignalDetectFinish
//    {
//        指令信号 = 0,
//        流水号 = 1,
//        质量信号 = 2,
//        颜色信号 = 3,
//        可编辑信号 = 4
//    }

//    public class CamNameType : StringConverter
//    {
//        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
//        {
//            return true;
//        }

//        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
//        {
//            if (context.Instance is CtrlCam)
//            {
//                if (context.PropertyDescriptor.Name == "Cam")
//                {
//                    return new StandardValuesCollection(CCameraManagement.CamParamDict.Keys);
//                }
//                if (context.PropertyDescriptor.Name == "Group")
//                {
//                    List<string> groups = new List<string>();
//                    for (int i = 0; i < CCameraManagement.CamParamDict.Count; i++)
//                    {
//                        groups.Add($"分组{i}");
//                    }
//                    return new StandardValuesCollection(groups);
//                }
//            }
//            else
//            {
//                if (context.PropertyDescriptor.Name == "Cam")
//                {
//                    List<string> cams = new List<string>();
//                    if (CCommunicationManagement.CommParamDic.ContainsKey(OpenCommunication._Name))
//                        CCommunicationManagement
//                            .CommParamDic[OpenCommunication._Name]
//                            .ControledCamID.ForEach(c => cams.Add(c.Cam));
//                    cams.Add("所有相机");
//                    return new StandardValuesCollection(cams);
//                }
//            }
//            return base.GetStandardValues(context);
//        }

//        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
//        {
//            return true;
//        }

//        public override object ConvertFrom(
//            ITypeDescriptorContext context,
//            CultureInfo culture,
//            object value
//        )
//        {
//            if (context != null && context.PropertyDescriptor.Name == "Cam" && value is string)
//            {
//                string CamStationName = value as string;
//                foreach (var item in CCameraManagement.CamParamDict.Values)
//                {
//                    if (item.CamStationName == CamStationName)
//                    {
//                        return $"{item.SerialNumber}[{item.CamStationName}]";
//                    }
//                }
//            }
//            return base.ConvertFrom(context, culture, value);
//        }

//        public override object ConvertTo(
//            ITypeDescriptorContext context,
//            CultureInfo culture,
//            object value,
//            Type destinationType
//        )
//        {
//            if (context != null && context.PropertyDescriptor.Name == "Cam" && value is string)
//            {
//                string SerialNumber = value as string;
//                foreach (var item in CCameraManagement.CamParamDict.Values)
//                {
//                    if (item.SerialNumber == SerialNumber)
//                    {
//                        return $"{item.SerialNumber}[{item.CamStationName}]";
//                    }
//                }
//            }
//            return base.ConvertFrom(context, culture, value);
//        }
//    }
//}
