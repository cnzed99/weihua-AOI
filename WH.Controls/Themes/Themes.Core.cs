using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace WH.Controls.Themes.Core
{
    /// <summary>
    /// 20240916 TCG
    /// 颜色主题管理
    /// </summary>
    public class ThemesManager
    {
        private Dictionary<string, ResourceDictionary> _themes = new();

        public void AddTheme(string name, string assemblyName, string resourcePath)
        {
            string uri = $"/{assemblyName};component/{resourcePath}";
            ResourceDictionary resource = new ResourceDictionary();
            resource.Source = new Uri(uri, UriKind.RelativeOrAbsolute);
            _themes.Add(name, resource);
        }

        public void ApplyTheme(string themeName)
        {
            ResourceDictionary resource = _themes[themeName];
            foreach (KeyValuePair<string, ResourceDictionary> kvp in _themes)
            {
                Application.Current.Resources.MergedDictionaries.Remove(kvp.Value);
            }
            Application.Current.Resources.MergedDictionaries.Add(resource);
        }
    }
}
