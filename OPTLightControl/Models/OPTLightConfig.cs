using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using WH.LightControl;

namespace OPTLightControl
{
    /// <summary>
    /// 20260129 龚伟东
    /// 奥普特光源参数类
    /// </summary>
    public partial class OPTLightConfig : CLightParamsBase
    {
        public OPTLightConfig()
            : base()
        {
            if (LightChannelList == null)
            {
                LightChannelList = new()
                {
                    new CLight("A"),
                    new CLight("B"),
                    new CLight("C"),
                    new CLight("D"),
                    new CLight("E"),
                    new CLight("F"),
                    new CLight("G"),
                    new CLight("H")
                }; //通道亮度
            }
        }

        /// <summary>
        /// 20260129 龚伟东
        /// 高低电平触发 true：高电平 false：低电平
        /// </summary>
        [property: DisplayName("高低电平触发")]
        [ObservableProperty]
        bool triggerMode = true;

        /// <summary>
        /// 20260129 龚伟东
        /// 边沿触发 true：上升沿 false：下降沿
        /// </summary>
        [property: DisplayName("边沿触发")]
        [ObservableProperty]
        bool triggerEdge = true;

        /// <summary>
        /// 20260129 龚伟东
        /// 工作模式 true：增亮模式 false：调试模式
        /// 0 代表常亮模式，1 代表常用触发模式，2 代表高亮触发模式，3 代表切换为硬件工作模式设置
        /// </summary>
        [property: DisplayName("工作模式")]
        [ObservableProperty]
        bool workMode = true;
    }
}
