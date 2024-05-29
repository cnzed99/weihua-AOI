using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static System.Formats.Asn1.AsnWriter;

namespace WH.Controls
{
    /// <summary>
    /// ImageView.xaml 的交互逻辑
    /// </summary>
    public partial class ImageView : UserControl
    {
        public ImageView()
        {
            InitializeComponent();
            this.Loaded += ImageView_Loaded;
        }

        private void ImageView_Loaded(object sender, RoutedEventArgs e)
        {
            ImageDraw = this.Image;
            WinDraw = this.Canvas;
        }

        public BitmapSource Source
        {
            get { return (BitmapSource)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for BitmapSource.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register("Source", typeof(BitmapSource), typeof(ImageView), new PropertyMetadata(null, (d, e) =>
            {
                if (e.NewValue != null)
                {
                    ImageView view = (ImageView)d;
                    view.Image.Source = (BitmapSource)e.NewValue;
                    view.Canvas.Source = (BitmapSource)e.NewValue;
                }
            }));



        public ImagePro ImageDraw
        {
            get { return (ImagePro)GetValue(ImageDrawProperty); }
            set { SetValue(ImageDrawProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ImageDrawProperty =
            DependencyProperty.Register("ImageDraw", typeof(ImagePro), typeof(ImageView), new PropertyMetadata(null));



        public CanvasPro WinDraw
        {
            get { return (CanvasPro)GetValue(WinDrawProperty); }
            set { SetValue(WinDrawProperty, value); }
        }

        // Using a DependencyProperty as the backing store for WinDraw.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty WinDrawProperty =
            DependencyProperty.Register("WinDraw", typeof(CanvasPro), typeof(ImageView), new PropertyMetadata(null));


        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            ImageDraw.Clear();
            WinDraw.Clear();
        }
    }
}
