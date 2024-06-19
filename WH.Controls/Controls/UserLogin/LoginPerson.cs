using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace WH.Controls
{
    public partial class LoginPerson : ObservableObject
    {
        [ObservableProperty]
        private string userName = string.Empty;


        /// <summary>
        /// 密码
        /// </summary>
        [ObservableProperty]
        private string passWord = string.Empty;



        /// <summary>
        /// 权限等级
        /// </summary>
       
        private PRIVILEGE _privileageLevel;
        
        public PRIVILEGE PrivileageLevel
        {
            get { return _privileageLevel; }
            set
            {

               
                switch (value)
                {
                    case PRIVILEGE.售后:
                        LogoImage = new BitmapImage(new Uri("pack://application:,,,/WH.Controls;component/Controls/UserLogin/Imgs/售后.png"));
                        break;
                    case PRIVILEGE.无权限:
                        LogoImage = new BitmapImage(new Uri("pack://application:,,,/WH.Controls;component/Controls/UserLogin/Imgs/未登录.png"));
                        break;
                    case PRIVILEGE.工程师:
                        LogoImage = new BitmapImage(new Uri("pack://application:,,,/WH.Controls;component/Controls/UserLogin/Imgs/工程师.png"));
                        break;
                    case PRIVILEGE.工艺员:
                        LogoImage = new BitmapImage(new Uri("pack://application:,,,/WH.Controls;component/Controls/UserLogin/Imgs/工艺.png"));
                        break;
                    case PRIVILEGE.操作员:
                        LogoImage = new BitmapImage(new Uri("pack://application:,,,/WH.Controls;component/Controls/UserLogin/Imgs/操作员.png"));
                        break;
                    case PRIVILEGE.管理员:
                        LogoImage = new BitmapImage(new Uri("pack://application:,,,/WH.Controls;component/Controls/UserLogin/Imgs/管理员.png"));
                        break;
                    default:
                        LogoImage = new BitmapImage(new Uri("pack://application:,,,/WH.Controls;component/Controls/UserLogin/Imgs/未登录.png"));
                        break;
                }
                SetProperty(ref _privileageLevel, value);
                OnPropertyChanged(nameof(LogoImage));
            }
        }


        public ImageSource LogoImage = new BitmapImage(new Uri("pack://application:,,,/WH.Controls;component/Controls/UserLogin/Imgs/未登录.png"));

    }

    public enum PRIVILEGE
    {
        /// <summary>
        /// 无权限
        /// </summary>
        无权限,
        /// <summary>
        /// 普通操作员
        /// </summary>
        操作员,
        /// <summary>
        /// 技术员
        /// </summary>
        工艺员,
        /// <summary>
        /// 工程师
        /// </summary>
        工程师,
        /// <summary>
        /// 售后
        /// </summary>
        售后,
        /// <summary>
        /// 管理员
        /// </summary>
        管理员
    }
}
