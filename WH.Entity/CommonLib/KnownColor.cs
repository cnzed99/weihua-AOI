using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace WH.Entity.CommonLib
{
    /// <summary>
    /// 2024.7.1 李焕彬
    /// Known颜色类，含颜色和颜色名
    /// </summary>
    public class KnownColor:IEquatable<KnownColor>
    {
        public KnownColor()
        {
            
        }
        public KnownColor(string name, Brush brush)
        {
            this.Name = name;
            this.Brush = brush;
        }

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 颜色名
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 颜色画刷
        /// </summary>
        public Brush Brush { get; set; }

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 重载等于
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public bool Equals(KnownColor other)
        {
            if (other == null) throw new ArgumentNullException(nameof(other));
            var kcolor = (KnownColor)other;
            if (kcolor.Name == Name) return true;
            else return false;
        }

        /// <summary>
        /// 2024.7.4 李焕彬
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Name;
        }
    }

    /// <summary>
    /// 2024.6.26 李焕彬
    /// Known颜色集，含静态对象
    /// </summary>
    public class BrushPro
    {
        /// <summary>
        /// 2024.7.4 李焕彬
        /// Known颜色实例
        /// </summary>
        public static BrushPro s_Instance = new BrushPro();

        /// <summary>
        /// 2024.7.4 李焕彬
        /// Known颜色集
        /// </summary>
        public List<KnownColor> KnownColors { get; set; } = new List<KnownColor>();

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 初始化Known颜色集
        /// </summary>
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
