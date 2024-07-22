using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace CommunicationModule
{
    /// <summary>
    /// 2024.7.17 李焕彬
    /// OpenCommSelect.xaml 的交互逻辑
    /// </summary>
    public partial class OpenCommSelect : HandyControl.Controls.Window
    {
        public OpenCommSelect()
        {
            InitializeComponent();
            this.DataContext = selectVM;
        }

        /// <summary>
        /// 2024.7.17 李焕彬
        /// VM
        /// </summary>
        public COpenCommSelectVM selectVM = new COpenCommSelectVM();
    }

    /// <summary>
    /// 2024.7.17 李焕彬
    /// 通讯名称转换器，根据已有通讯自动递增命名
    /// </summary>
    public class NameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null && value is string key)
            {
                List<string> names = new List<string>();
                CCommunicationManagement
                    .CommParamDic.Values.ToList()
                    .ForEach(c => names.Add(c.Name));
                int count = 1;
                for (int i = 0; i < names.Count; i++)
                {
                    string[] spkey = names[i].Split('_');
                    if (spkey[0].Contains(key))
                    {
                        count++;
                    }
                }
                return key + "_" + count;
            }
            return Binding.DoNothing;
        }

        public object ConvertBack(
            object value,
            Type targetType,
            object parameter,
            CultureInfo culture
        )
        {
            throw new NotImplementedException();
        }
    }
}
