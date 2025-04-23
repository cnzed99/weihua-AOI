using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using Newtonsoft.Json;

namespace WH.Entity.CommonLib
{
    /// <summary>
    /// 2024.7.1 李焕彬
    /// Known颜色类，含颜色和颜色名
    /// </summary>
    public class OpenSetWindowProperty
    {
        public OpenSetWindowProperty()
        { }

        public OpenSetWindowProperty(string name, bool isopened)
        {
            this.Name = name;
            this.IsOpened = isopened;
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
        [JsonIgnore]
        public bool IsOpened { get; set; }
    }
}