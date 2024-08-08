using CommunityToolkit.Mvvm.Messaging;
using HandyControl.Controls;
using WH.Entity;
using WH.Entity.CommonLib;
using WH.Entity.LogRecord;

namespace CameraModule
{
    public class CCameraManagement : IRecipient<OperateMessage>
    {
        /// <summary>
        /// 2024.7.23 李焕彬
        /// 相机参数保存路径
        /// </summary>
        public static string s_CamPath = "..\\SystemConfig\\CamConfig.Json";

        /// <summary>
        /// 2024.7.23 李焕彬
        /// 模块日志
        /// </summary>
        public static CLogRec CamLogger { get; set; } = CLogRec.Create("Cam", "D:/Data");

        /// <summary>
        /// 2024.7.23 李焕彬
        /// 操作日志
        /// </summary>
        public static CLogRec OperateLog { get; set; } = CLogRec.Create("Operate", "D:/Data");

        /// <summary>
        /// 2024.7.19 李焕彬
        /// 运行日志
        /// </summary>
        public static CLogRec SysLog = CLogRec.Create("Info", "D:/Data");

        /// <summary>
        /// 2024.7.23 李焕彬
        /// 相机操作类字典,从文件打开软件时使用
        /// key:相机序列号 Value:相机操作对象
        /// </summary>
        public static Dictionary<string, CCameraBase> CameraDict =
            new Dictionary<string, CCameraBase>();

        /// <summary>
        /// 2024.7.23 李焕彬
        /// 相机插件字典
        /// key:相机品牌 Value:相机插件操作对象
        /// </summary>
        public static Dictionary<string, ICamera> CameraHelpers = new Dictionary<string, ICamera>();

        /// <summary>
        /// 2024.7.23 李焕彬
        /// 相机参数类字典,从文件打开软件时使用
        /// key:相机序列号 Value:相机参数
        /// </summary>
        public static Dictionary<string, CCameraParameterBase> CamParamDict =
            new Dictionary<string, CCameraParameterBase>();

        /// <summary>
        /// 2024.7.23 李焕彬
        /// 相机类型
        /// </summary>
        public static Dictionary<string, Type> CameraTypes = new Dictionary<string, Type>();

        /// <summary>
        /// 2024.7.23 李焕彬
        /// 是否正在加载Json参数
        /// </summary>
        public static bool s_IsLoadParam = false;

        public CCameraManagement(List<CCameraParameterBase> camparameters, string path)
        {
            try
            {
                LoadCamPlugs.LoadCam();
                s_IsLoadParam = true;
                if (camparameters != null)
                {
                    for (int i = 0; i < camparameters.Count; i++)
                    {
                        string serialnumber = camparameters[i].SerialNumber;
                        var param = CCameraManagement
                            .CameraHelpers[camparameters[i].CameraSupplier]
                            .Init(path, i, out CCameraBase obj);
                        CamParamDict.Add(serialnumber, param);
                        CameraDict.Add(serialnumber, obj);
                    }
                }
                s_IsLoadParam = false;
                InitializeAllCamera();
            }
            catch (Exception)
            {
                throw;
            }

            WeakReferenceMessenger.Default.Register<OperateMessage, Token>(
                this,
                new Token("", this.GetType().Namespace)
            );
        }

        /// <summary>
        /// 2024.7.23 李焕彬
        /// 初始化所有相机
        /// </summary>
        /// <returns>成功打开的相机集合</returns>
        public void InitializeAllCamera()
        {
            List<(string serialnumber, bool connected)> connects =
                new List<(string serialnumber, bool connected)>();
            if (CameraDict.Count > 0)
            {
                if (CameraDict != null)
                {
                    foreach (var Cam in CameraDict)
                    {
                        if (Cam.Value.Setting.Enable && !Cam.Value.Connected)
                        {
                            Cam.Value.InitializeCamera();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 2024.7.23 李焕彬
        /// 关闭所有相机
        /// </summary>
        public static void CloseAllCameras()
        {
            if (CameraDict.Count > 0)
            {
                if (CameraDict != null)
                {
                    foreach (var Cam in CameraDict)
                    {
                        if (Cam.Value.Connected)
                        {
                            Cam.Value.EndCamera();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 2024.7.23 李焕彬
        /// 保存所有相机参数
        /// </summary>
        /// <param name="campath"></param>
        public static void SaveAllCamConfig()
        {
            try
            {
                Dictionary<string, CCameraParameterBase>.ValueCollection values = CCameraManagement
                    .CamParamDict
                    .Values;
                List<CCameraParameterBase> list = new List<CCameraParameterBase>();
                foreach (var item in values)
                {
                    list.Add(item);
                }
                ConfigAPI.Save(list, s_CamPath);
            }
            catch (Exception ex)
            {
                CCameraManagement.CamLogger.Error(Properties.Resources.ErrorSave + ex.Message);
            }
        }

        /// <summary>
        /// 2024.7.23 李焕彬
        /// 获取相机名
        /// </summary>
        /// <param name="CamSerial">序列号</param>
        /// <returns>相机名</returns>
        public static string GetCamStationName(string CamSerial)
        {
            return CamParamDict.ContainsKey(CamSerial) ? CamParamDict[CamSerial].Name : "未找到相机";
        }

        /// <summary>
        /// 2024.7.23 李焕彬
        /// 日志消息处理
        /// </summary>
        /// <param name="message">消息</param>
        public void Receive(OperateMessage message)
        {
            if (message.obj.GetType() == typeof(CCameraManagement))
            {
                OperateLog.Info($"相机模块-{message.message}");
                return;
            }
            foreach (var cam in CamParamDict)
            {
                if (typeof(CCameraParameterBase).IsAssignableFrom(message.obj.GetType()))
                {
                    if (cam.Value == message.obj)
                    {
                        OperateLog.Info($"相机模块-{cam.Value.Name}:{message.message}");
                        return;
                    }
                    continue;
                }
            }
        }

        //public static void StopImaging(string ProjGuid, string CamSerial)
        //{
        //    if (!CameraDict.ContainsKey(CamSerial))
        //    {
        //        CameraDict[CamSerial].IsSetWindowShowed = false;
        //    }
        //}

        //public static void StartImaging(string ProjGuid, string CamSerial)
        //{
        //    if (!CameraDict.ContainsKey(CamSerial))
        //    {
        //        CameraDict[CamSerial].IsSetWindowShowed = true;
        //    }
        //}
    }
}
