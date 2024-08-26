using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using WH.LightControl;


namespace LSWLightControl
{
    public partial class LSWLightConfig : CLightParamsBase
    {
        public LSWLightConfig()
        {
            this.token = new WH.Entity.CommonLib.Token("", this.GetType().Namespace);
            LightChannelList = new()
            {
                new CLight("A"),
                new CLight("B"),
                new CLight("C"),
                new CLight("D")
            }; //通道亮度
        }

        /// <summary>
        /// 20240724 TCG
        /// 高低电平触发 true：高电平 false：低电平
        /// </summary>
        [property: DisplayName("高低电平触发")]
        [ObservableProperty]
        bool triggerMode = true;

        /// <summary>
        /// 20240724 TCG
        /// 边沿触发 true：上升沿 false：下降沿
        /// </summary>
        [property: DisplayName("边沿触发")]
        [ObservableProperty]
        bool triggerEdge = true;

        /// <summary>
        /// 20240724 TCG
        /// 工作模式 true：增亮模式 false：调试模式
        /// </summary>
        [property: DisplayName("工作模式")]
        [ObservableProperty]
        bool workMode = true;
    }
}
