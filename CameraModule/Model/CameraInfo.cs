using CommunityToolkit.Mvvm.ComponentModel;

namespace CameraModule
{
    /// <summary>
    /// 2024.7.23 李焕彬
    /// 相机信息 用于显示
    /// </summary>
    public partial class WHCameraInfo : ObservableObject
    {
        /// <summary>
        /// 2024.7.23 李焕彬
        /// 相机序列号
        /// </summary>
        [ObservableProperty]
        private string serialNumber;
        /// <summary>
        /// 2024.7.23 李焕彬
        /// 相机品牌
        /// </summary>
        [ObservableProperty]
        private string vender;
        /// <summary>
        /// 2024.7.23 李焕彬
        /// 相机类型
        /// </summary>
        [ObservableProperty]
        private string camType;
        /// <summary>
        /// 2024.7.23 李焕彬
        /// 相机IP
        /// </summary>
        [ObservableProperty]
        private string camIp;

        /// <summary>
        /// 2024.7.23 李焕彬
        /// 是否使用
        /// </summary
        [ObservableProperty]
        private bool isUse;
    }
}
