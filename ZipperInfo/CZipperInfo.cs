using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace ZipperInfo
{
    public partial class CZipperInfo: ObservableObject
    {
        /// <summary>
        /// 拉链长度
        /// 2025.05.24 鲍赞宝
        /// </summary>
        [property: Category("拉链信息")]
        [property: DisplayName("01.拉链长度(cm)")]
        [property: Description("拉链长度,单位cm")]
        [property: Browsable(true)]
        [property: ReadOnly(true)]
        [ObservableProperty]
        float zipperLneght = 0.0f;
        /// <summary>
        /// 链牙型号(大小)
        /// 2025.05.24 鲍赞宝
        /// </summary>
        [property: Category("拉链信息")]
        [property: DisplayName("02.链牙型号")]
        [property: Description("链牙型号(大小)")]
        [property: Browsable(true)]
        [property: ReadOnly(true)]
        [ObservableProperty]
        TOOTHTYPE zipperToothType = TOOTHTYPE._3号;
        /// <summary>
        /// 穿拉头方式
        /// 2025.05.24 鲍赞宝
        /// </summary>
        [property: Category("拉链信息")]
        [property: DisplayName("03.穿拉头方式")]
        [property: Description("穿拉头方式")]
        [property: Browsable(true)]
        [property: ReadOnly(true)]
        [ObservableProperty]
        PULLTYPE zipperSliderType=PULLTYPE.正穿;
        /// <summary>
        /// 穿拉头方式
        /// 2025.05.24 鲍赞宝
        /// </summary>
        [property: Category("拉链信息")]
        [property: DisplayName("04.上止类型")]
        [property: Description("上止类型")]
        [property: Browsable(true)]
        [property: ReadOnly(true)]
        [ObservableProperty]
        STOPMASS zipperUpMass= STOPMASS.注塑;
        /// <summary>
        /// 穿拉头方式
        /// 2025.05.24 鲍赞宝
        /// </summary>
        [property: Category("拉链信息")]
        [property: DisplayName("05.下止类型")]
        [property: Description("下止类型")]
        [property: Browsable(true)]
        [property: ReadOnly(true)]
        [ObservableProperty]
        STOPMASS zipperDownMass = STOPMASS.注塑;
    }


    public enum TOOTHTYPE
    {
        _3号=0,
        _5号 = 1,
        _7号 = 2,
        _8号 = 3
    }
    public enum PULLTYPE
    {
        正穿=0,
        反穿= 1
    }

    public enum STOPMASS
    { 
        注塑=0,
        白铝=1,
        烤漆=2,
        透明=3,
        U型尼龙=4,
        融止=5,
        隐形=6

    }

}
