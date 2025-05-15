using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CameraModule;
using CommunityToolkit.Mvvm.ComponentModel;
using HandyControl.Controls;
using NetSDKCS;
using WH.Controls;
using WH.Entity.Attribute;
using WH.Entity.CommonLib;

namespace DahuaThermalCam
{
    /// <summary>
    /// 2025.5.13 李焕彬
    /// 相机参数派生类
    /// </summary>
    public partial class CParameterSetting : CCameraParameterBase
    {
        public CParameterSetting()
            : base() { }

        public CParameterSetting(string serialnumber, string cameraSupplier)
            : base(serialnumber, cameraSupplier) { }

        /// <summary>
        /// 2025.5.13 李焕彬
        /// ip
        /// </summary>
        [ObservableProperty]
        [property: Category("登录信息")]
        [property: DisplayName("IP")]
        [property: Description("IP")]
        private string ip = "192.168.1.108";

        /// <summary>
        /// 2025.5.13 李焕彬
        /// port
        /// </summary>
        [ObservableProperty]
        [property: Category("登录信息")]
        [property: DisplayName("Port")]
        [property: Description("Port")]
        private ushort port = 37777;

        /// <summary>
        /// 2025.5.13 李焕彬
        /// User
        /// </summary>
        [ObservableProperty]
        [property: Category("登录信息")]
        [property: DisplayName("User")]
        [property: Description("User")]
        private string user = "admin";

        /// <summary>
        /// 2025.5.13 李焕彬
        /// Password
        /// </summary>
        [ObservableProperty]
        [property: Category("登录信息")]
        [property: DisplayName("Password")]
        [property: Description("Password")]
        private string password = "q123456789";

        /// <summary>
        /// 2025.5.13 李焕彬
        /// 调色板
        /// </summary>
        [ObservableProperty]
        [property: Category("温宽调节")]
        [property: DisplayName("1.调色板")]
        [property: Description("1.调色板")]
        private EM_PALETTE palette = EM_PALETTE.彩虹;

        /// <summary>
        /// 2025.5.13 李焕彬
        /// 启用温宽调节
        /// </summary>
        [ObservableProperty]
        [property: Category("温宽调节")]
        [property: DisplayName("1.启用温宽调节")]
        [property: Description("1.启用温宽调节")]
        bool enableTempLimit = false;

        /// <summary>
        /// 2025.5.13 李焕彬
        /// 低温
        /// </summary>
        [ObservableProperty]
        [property: Category("温宽调节")]
        [property: DisplayName("2.低温")]
        [property: Description("2.低温，单位为C")]
        float tempLower = 20;

        /// <summary>
        /// 2025.5.13 李焕彬
        /// 高温
        /// </summary>
        [ObservableProperty]
        [property: Category("温宽调节")]
        [property: DisplayName("3.高温")]
        [property: Description("3.高温，单位为C")]
        float tempHigher = 40;
    }
}
