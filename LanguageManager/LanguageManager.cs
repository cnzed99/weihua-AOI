using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Resources;

namespace LanguageManager
{
    public class LanguageManager : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private ResourceManager resourceManager;

        private static List<LanguageManager> s_LanguageManagers = new List<LanguageManager>();

        public LanguageManager(string resourcePath, Assembly assembly)
        {
            resourceManager = new ResourceManager(resourcePath, assembly);
            s_LanguageManagers.Add(this);
        }

        /// <summary>
        /// 索引器的写法，传入字符串的下标
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public string this[string name]
        {
            get
            {
                if (name == null)
                {
                    throw new ArgumentNullException(nameof(name));
                }
                return resourceManager?.GetString(name)??string.Empty;
            }
        }

        public static void ChangeLanguage(CultureInfo cultureInfo)
        {
            CultureInfo.CurrentCulture = cultureInfo;
            CultureInfo.CurrentUICulture = cultureInfo;
            foreach (var item in s_LanguageManagers)
            {
                item.PropertyChanged?.Invoke(item, new PropertyChangedEventArgs(""));
            }
        }
    }
}
