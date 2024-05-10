using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Color = System.Windows.Media.Color;

namespace ImageWinCtrl
{
    public partial class ImageViewViewModel : ObservableObject
    {
        public ImageViewViewModel()
        {
        }

        [ObservableProperty]
        private Bitmap? bitmapSource;

        /// <summary>
        /// Image上画图
        /// </summary>
        [ObservableProperty]
        private ImagePro? imageOne;

        /// <summary>
        /// Window上画图
        /// </summary>
        [ObservableProperty]
        private CanvasPro? canvasOne;
    }
}
