using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace WH.Entity.CommonLib
{
    public record KnownColor(string name, Brush brush);
    /// <summary>
    /// 2024.6.26 李焕彬
    /// Known颜色集，含颜色和颜色名
    /// </summary>
    public class BrushPro
    {
        public static BrushPro instance = new BrushPro();

        public List<KnownColor> KnownColors { get; set; } = new List<KnownColor>();
        public BrushPro() 
        {
            PropertyInfo[] properties = typeof(Brushes).GetProperties();
            foreach (var property in properties)
            {
                if (typeof(Brush).IsAssignableFrom(property.PropertyType))
                {
                    KnownColors.Add(new KnownColor(property.Name, (Brush)property.GetValue(null)));
                }
            }
        }
    }

    
}
