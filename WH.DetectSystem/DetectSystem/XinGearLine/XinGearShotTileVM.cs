using System;
using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using WH.Controls;

namespace WH.DetectSystem.Models
{
    /// <summary>
    /// 【新兴盘齿方案0.6-注释】拍摄分页页2 一格。CurView 仅该格 Operator，不抢侧面制程格的 CurView。
    /// </summary>
    public partial class XinGearShotTileVM : ObservableObject
    {
        public XinGearShotTileVM(int photoIndex)
        {
            PhotoIndex = photoIndex;
        }

        public int PhotoIndex { get; }

        public string Title => "侧面" + PhotoIndex;

        public int GridRow => (PhotoIndex - 1) / 3;

        public int GridCol => (PhotoIndex - 1) % 3;

        [ObservableProperty]
        private BitmapSource modelImage;

        internal Action<XinGearShotTileVM> WhenViewReady;

        private ImageView _curView;

        public ImageView CurView
        {
            get => _curView;
            set
            {
                _curView = value;
                WhenViewReady?.Invoke(this);
            }
        }
    }
}
