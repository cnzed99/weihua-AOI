using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace WH.Controls
{
    /// <summary>
    /// 20240725 TCG
    /// 状态指示灯
    /// </summary>
    public class StatusLight : Shape
    {
        public double Radius
        {
            get { return (double)GetValue(RadiusProperty); }
            set { SetValue(RadiusProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Radius.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty RadiusProperty = DependencyProperty.Register(
            "Radius",
            typeof(double),
            typeof(StatusLight),
            new PropertyMetadata(RadiusChanged)
        );

        private static void RadiusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            StatusLight statusLight = (StatusLight)d;
            statusLight.Width = (double)e.NewValue;
            statusLight.Height = (double)e.NewValue;
        }

        protected override Geometry DefiningGeometry
        {
            get
            {
                EllipseGeometry geometry = new EllipseGeometry();
                geometry.Center = new Point(Width / 2, Height / 2);
                geometry.RadiusX = Radius / 2;
                geometry.RadiusY = Radius / 2;

                return geometry;
            }
        }

        public StatusLight() { }
    }
}
