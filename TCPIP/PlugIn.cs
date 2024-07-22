using CommunicationModule;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WH.Entity;

namespace TCPIP
{
    public class PlugIn : ICommunicate
    {
        /// <summary>
        /// 2024.7.21 李焕彬
        /// 创建新通讯实例
        /// </summary>
        /// <param name="com">输出通讯对象</param>
        /// <returns>通讯参数对象</returns>
        public CCommunicationSettingBase CreateNewCom(out CCommunicationBase com)
        {
            CTcpIpCommunicationSetting param = new CTcpIpCommunicationSetting();
            CTcpIpCommPart Tcp = new CTcpIpCommPart(param);
            com = Tcp;
            return param;
        }
        /// <summary>
        /// 2024.7.21 李焕彬
        /// 根据json路径初始化通讯实例
        /// </summary>
        /// <param name="path">配置路径</param>
        /// <param name="index">索引</param>
        /// <param name="com">输出通讯对象</param>
        /// <returns>通讯参数对象</returns>
        public CCommunicationSettingBase Init(string path, int index, out CCommunicationBase com)
        {
            CTcpIpCommunicationSetting param = ConfigAPI.LoadDeserialize<List<CTcpIpCommunicationSetting>>(path)[index];
            CTcpIpCommPart Tcp = new CTcpIpCommPart(param);
            foreach (var alarm in param.AlarmProtocols)
            {
                ObservableCollection<CDataInfo> elems = new ObservableCollection<CDataInfo>();
                foreach (var item in alarm.Protocol)
                {
                    elems.Add(JsonConvert.DeserializeObject<CDataInfo>(JsonConvert.SerializeObject(item)));
                }
                alarm.Protocol = elems;
            }
            com = Tcp;
            return param;
        }
    }
}
