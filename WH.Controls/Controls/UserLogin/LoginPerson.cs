using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WH.Entity.Attribute;

namespace WH.Controls
{
    public partial class CLoginPerson : ObservableObject
    {
        [ObservableProperty]
        private string userName = string.Empty;


        /// <summary>
        /// 密码
        /// </summary>
        [ObservableProperty]
        private string passWord = string.Empty;

        public void Init()
        {
            UserName = string.Empty;
            PassWord = string.Empty;
            PrivileageLevel = 0;
        }

        /// <summary>
        /// 权限等级
        /// </summary>
       
        private PRIVILEGE privileageLevel;
        
        public PRIVILEGE PrivileageLevel
        {
            get { return privileageLevel; }
            set
            {

               
                switch (value)
                {
                    case PRIVILEGE.AFTER_SALE:
                        LogoImage = new BitmapImage(new Uri("pack://application:,,,/WH.Controls;component/Controls/UserLogin/Imgs/售后.png"));
                        break;
                    case PRIVILEGE.NOPERMISSION:
                        LogoImage = new BitmapImage(new Uri("pack://application:,,,/WH.Controls;component/Controls/UserLogin/Imgs/未登录.png"));
                        break;
                    case PRIVILEGE.ENGINEER:
                        LogoImage = new BitmapImage(new Uri("pack://application:,,,/WH.Controls;component/Controls/UserLogin/Imgs/工程师.png"));
                        break;
                    case PRIVILEGE.TECHNOLOGIST:
                        LogoImage = new BitmapImage(new Uri("pack://application:,,,/WH.Controls;component/Controls/UserLogin/Imgs/工艺.png"));
                        break;
                    case PRIVILEGE.OPERATOR:
                        LogoImage = new BitmapImage(new Uri("pack://application:,,,/WH.Controls;component/Controls/UserLogin/Imgs/操作员.png"));
                        break;
                    case PRIVILEGE.ADMINISTRATOR:
                        LogoImage = new BitmapImage(new Uri("pack://application:,,,/WH.Controls;component/Controls/UserLogin/Imgs/管理员.png"));
                        break;
                    default:
                        LogoImage = new BitmapImage(new Uri("pack://application:,,,/WH.Controls;component/Controls/UserLogin/Imgs/未登录.png"));
                        break;
                }
                SetProperty(ref privileageLevel, value);
                OnPropertyChanged(nameof(LogoImage));
            }
        }


        public ImageSource LogoImage { get; set; } = new BitmapImage(new Uri("pack://application:,,,/WH.Controls;component/Controls/UserLogin/Imgs/未登录.png"));

    }

    public enum PRIVILEGE
    {
        /// <summary>
        /// 无权限
        /// </summary>
        [EnumString("无权限", "NO PERMISSION")]
        NOPERMISSION,
        /// <summary>
        /// 普通操作员
        /// </summary>
        [EnumString("操作员", "OPERATOR")]
        OPERATOR,
        /// <summary>
        /// 技术员
        /// </summary>
        [EnumString("技术员", "TECHNOLOGIST")]
        TECHNOLOGIST,
        /// <summary>
        /// 工程师
        /// </summary>
        [EnumString("工程师", "ENGINEER")]
        ENGINEER,
        /// <summary>
        /// 售后
        /// </summary>
        [EnumString("售后", "AFTER_SALE")]
        AFTER_SALE,
        /// <summary>
        /// 管理员
        /// </summary>
        [EnumString("管理员", "ADMINISTRATOR")]
        ADMINISTRATOR
    }
   
}
