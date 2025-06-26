using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZipperInfo
{
    public partial class CAutomaticModel: ObservableObject
    {
        /// <summary>
        /// 拉链长度
        /// </summary>
        [ObservableProperty]
        float zipperLenght = 0;
        /// <summary>
        /// 相机中心到切刀的中心距
        /// </summary>
        [ObservableProperty]
        float daoDitance = 782;
        /// <summary>
        /// 相机视野
        /// </summary>
        [ObservableProperty]
        float ccdWidth = 114;
    }
}
