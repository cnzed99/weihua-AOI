using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunicationModule;
using Newtonsoft.Json;
using WH.Entity;

namespace Modbus
{
    /// <summary>
    /// 2024.7.21 李焕彬
    /// Modbus通讯插口
    /// </summary>
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
            CModbusSetting param = new CModbusSetting();
            CModbusCommPart modbus = new CModbusCommPart(param);
            com = modbus;
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
            CModbusSetting param = ConfigAPI.LoadDeserialize<List<CModbusSetting>>(path)[index];
            foreach (var alarm in param.AlarmProtocols)
            {
                ObservableCollection<CElement> elems = new ObservableCollection<CElement>();
                foreach (var item in alarm.Protocol)
                {
                    elems.Add(
                        JsonConvert.DeserializeObject<CElement>(JsonConvert.SerializeObject(item))
                    );
                }
                alarm.Protocol = elems;
            }
            CModbusCommPart modbus = new CModbusCommPart(param);
            com = modbus;
            return param;
        }
    }
}
