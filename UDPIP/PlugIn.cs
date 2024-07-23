using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunicationModule;
using Newtonsoft.Json;
using WH.Entity;

namespace UDPIP
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
            CUdpIpCommunicationSetting param = new CUdpIpCommunicationSetting();
            CUdpIpCommPart Udp = new CUdpIpCommPart(param);
            com = Udp;
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
            CUdpIpCommunicationSetting param = ConfigAPI.LoadDeserialize<
                List<CUdpIpCommunicationSetting>
            >(path)[index];
            CUdpIpCommPart Udp = new CUdpIpCommPart(param);
            foreach (var alarm in param.AlarmAgreements)
            {
                ObservableCollection<CDataInfo> elems = new ObservableCollection<CDataInfo>();
                foreach (var item in alarm.Protocol)
                {
                    elems.Add(
                        JsonConvert.DeserializeObject<CDataInfo>(JsonConvert.SerializeObject(item))
                    );
                }
                alarm.Protocol = elems;
            }
            com = Udp;
            return param;
        }
    }
}
