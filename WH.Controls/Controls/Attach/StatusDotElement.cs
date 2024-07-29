using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace WH.Controls.Controls.Attach
{
    /// <summary>
    /// 20240726 TCG
    /// 状态灯 附加属性，配合Style:StatusDotButton,StatusDotLabel使用
    /// </summary>
    public static class StatusDotElement
    {
        public static int GetRadius(DependencyObject obj)
        {
            return (int)obj.GetValue(RadiusProperty);
        }

        public static void SetRadius(DependencyObject obj, int value)
        {
            obj.SetValue(RadiusProperty, value);
        }

        // Using a DependencyProperty as the backing store for Radius.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty RadiusProperty =
            DependencyProperty.RegisterAttached(
                "Radius",
                typeof(int),
                typeof(StatusDotElement),
                new FrameworkPropertyMetadata(10, FrameworkPropertyMetadataOptions.Inherits)
            );

        public static Brush GetDotBrush(DependencyObject obj)
        {
            return (Brush)obj.GetValue(DotBrushProperty);
        }

        public static void SetDotBrush(DependencyObject obj, Brush value)
        {
            obj.SetValue(DotBrushProperty, value);
        }

        // Using a DependencyProperty as the backing store for DotBrush.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DotBrushProperty =
            DependencyProperty.RegisterAttached(
                "DotBrush",
                typeof(Brush),
                typeof(StatusDotElement),
                new FrameworkPropertyMetadata(
                    default(Brush),
                    FrameworkPropertyMetadataOptions.Inherits
                )
            );
    }
}
