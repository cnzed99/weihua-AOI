using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using HandyControl.Controls;
using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WH.Entity;
using WH.Entity.Attribute;
using WH.Entity.CommonLib;
using WH.Entity.LogRecord;
using WH.Entity.Messages;

namespace CommunicationModule
{
    /// <summary>
    /// 2024.7.17 李焕彬
    /// 通讯管理类
    /// </summary>
    public class CCommunicationManagement : IRecipient<OperateMessage>
    {
        /// <summary>
        /// 2024.7.17 李焕彬
        /// 主通讯参数保存路径
        /// </summary>
        public static string s_CommPath = "..\\SystemConfig\\CommConfig.Json";

        /// <summary>
        /// 2024.7.19 李焕彬
        /// 通讯模块日志
        /// </summary>
        public static CLogRec ComLogger { get; set; } = CLogRec.Create("Com", "D:/Data");

        /// <summary>
        /// 2024.7.18 李焕彬
        /// 操作日志
        /// </summary>
        public static CLogRec OperateLog { get; set; } = CLogRec.Create("Operate", "D:/Data");

        /// <summary>
        /// 2024.7.19 李焕彬
        /// 运行日志
        /// </summary>
        public static CLogRec SysLog = CLogRec.Create("Info", "D:/Data");

        /// <summary>
        /// 2024.7.19 李焕彬
        /// 通讯操作字典
        /// </summary>
        public static Dictionary<string, CCommunicationBase> CommDic =
            new Dictionary<string, CCommunicationBase>();
        public static Dictionary<string, ICommunicate> ComHelper =
            new Dictionary<string, ICommunicate>();

        /// <summary>
        /// 2024.7.19 李焕彬
        /// key:通讯GUID,value:通讯实例 通讯参数操作字典
        /// </summary>
        public static Dictionary<string, CCommunicationSettingBase> CommParamDic =
            new Dictionary<string, CCommunicationSettingBase>();
        private static ObservableCollection<CCommunicationSettingBase> comparams;

        public CCommunicationManagement(
            ObservableCollection<CCommunicationSettingBase> param,
            string paramPath
        )
        {
            CLoadComPlugs.LoadCom();
            comparams = param;
            if (param != null)
            {
                for (int i = 0; i < param.Count; i++)
                {
                    ComLogger.Info($"开始读取{param[i].CommType}:" + param[i].Name + "参数");
                    var Param = ComHelper[param[i].CommType]
                        .Init(paramPath, i, out CCommunicationBase _communication);
                    if (Param != null)
                    {
                        ComLogger.Info($"添加{param[i].CommType}:" + param[i].Name + "到字典中");
                        CommParamDic.Add(param[i].Guid, Param);
                        CommDic.Add(param[i].Guid, _communication);
                    }
                }
            }

            WeakReferenceMessenger.Default.Register<OperateMessage, Token>(
                this,
                new Token("", this.GetType().Namespace)
            );
        }

        /// <summary>
        /// 2024.7.18 李焕彬
        /// 添加通讯
        /// </summary>
        /// <param name="settingBase">通讯参数对象</param>
        /// <param name="com">通讯操作对象</param>
        public static void AddComm(CCommunicationSettingBase settingBase, CCommunicationBase com)
        {
            CCommunicationManagement.CommParamDic.Add(settingBase.Guid, settingBase);
            CCommunicationManagement.CommDic.Add(settingBase.Guid, com);
            UpdateParamList();
            OperateLog.Info($"通讯模块-通讯增加{settingBase.ToString()}!");
        }

        /// <summary>
        /// 2024.7.18 李焕彬
        /// 删除通讯
        /// </summary>
        /// <param name="guid">guid</param>
        public static void DelComm(string guid)
        {
            OperateLog.Info($"通讯模块-通讯删除{CommParamDic[guid].ToString()}!");

            CCommunicationManagement.CommDic.Remove(guid);

            CCommunicationManagement.CommParamDic.Remove(guid);
            UpdateParamList();
        }

        private static void UpdateParamList()
        {
            comparams.Clear();
            foreach (var item in CommParamDic.Values)
            {
                comparams.Add(item);
            }
        }

        /// <summary>
        /// 2024.7.18 李焕彬
        /// 日志消息处理
        /// </summary>
        /// <param name="message">消息</param>
        public void Receive(OperateMessage message)
        {
            if (message.obj.GetType() == typeof(CCommunicationManagement))
            {
                OperateLog.Info($"通讯模块-{message.message}");
                return;
            }
            foreach (var com in CommParamDic)
            {
                if (typeof(CCommunicationSettingBase).IsAssignableFrom(message.obj.GetType()))
                {
                    if (com.Value == message.obj)
                    {
                        OperateLog.Info($"通讯模块-{com.Value.Name}:{message.message}");
                        return;
                    }
                    continue;
                }
                foreach (var protocol in com.Value.AlarmAgreements)
                {
                    if (message.obj.GetType() == typeof(CAlarmAgreement))
                    {
                        if (protocol == message.obj)
                        {
                            OperateLog.Info(
                                $"通讯模块-{com.Value.Name}-{protocol.Name}:{message.message}"
                            );
                            return;
                        }
                        continue;
                    }
                    foreach (var dataInfo in protocol.Protocol)
                    {
                        if (message.obj.GetType() == dataInfo.GetType())
                        {
                            if (dataInfo == message.obj)
                            {
                                OperateLog.Info(
                                    $"通讯模块-{com.Value.Name}-{protocol.Name}-{dataInfo}:{message.message}"
                                );
                                return;
                            }
                            continue;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 2024.7.19 李焕彬
        /// 打开所有通讯
        /// </summary>
        public bool OpenAllComm()
        {
            ComLogger.Info("连接所有通讯:");
            bool result = true;
            if (CommDic.Count > 0)
            {
                if (CommDic != null)
                {
                    foreach (var Comm in CommDic)
                    {
                        ComLogger.Info("连接:" + CommParamDic[Comm.Key].Name);
                        try
                        {
                            Comm.Value.Connect();
                        }
                        catch (Exception ex)
                        {
                            ComLogger.Error(
                                $"{CommParamDic[Comm.Key].Name}连接服务器:{CommParamDic[Comm.Key].RemoteIP}失败:{ex.Message}"
                            );
                            result = false;
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// 2024.7.19 李焕彬
        /// 关闭所有通讯
        /// </summary>
        public static void CloseAllComm()
        {
            if (CommDic.Count > 0)
            {
                ComLogger.Info("关闭所有通讯:");
                if (CommDic != null)
                {
                    foreach (var Comm in CommDic)
                    {
                        ComLogger.Info("关闭:" + CommParamDic[Comm.Key].Name);
                        try
                        {
                            Comm.Value.Close();
                        }
                        catch (Exception ex)
                        {
                            ComLogger.Error(
                                $"{CommParamDic[Comm.Key].Name}关闭服务器:{CommParamDic[Comm.Key].RemoteIP}失败:{ex.Message}"
                            );
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 2024.7.23 李焕彬
        /// 保存所有通讯参数
        /// </summary>
        /// <param name="campath"></param>
        public static void SaveAllComConfig()
        {
            try
            {
                CCommunicationManagement.ComLogger.Info("保存所有通讯参数！");
                ObservableCollection<CCommunicationSettingBase> listparam =
                    new ObservableCollection<CCommunicationSettingBase>();
                Dictionary<string, CCommunicationSettingBase>.ValueCollection Values =
                    CCommunicationManagement.CommParamDic.Values;
                foreach (var value in Values)
                {
                    listparam.Add(value);
                }

                ConfigAPI.Save(listparam, s_CommPath);
                CCommunicationManagement.ComLogger.Info("保存所有通讯参数成功！");
            }
            catch (Exception ex)
            {
                CCommunicationManagement.ComLogger.Error("相机设置界面=>保存所有通讯参数错误:" + ex.Message);
            }
        }

        /// <summary>
        /// 2024.7.19 李焕彬
        /// 发送报警信息
        /// </summary>
        /// <param name="com">通讯名</param>
        /// <param name="alarmName">报警名</param>
        public static void SendAlarmSignal(CAlarmAgreement alarm)
        {
            if (alarm != null)
            {
                CommDic[alarm.GUID].Send(alarm.Protocol);
            }
        }
    }
}
