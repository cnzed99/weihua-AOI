using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using WH.Entity.Attribute;

namespace WH.Controls
{
    /// <summary>
    /// 20240704 TCG
    /// 用户 包含用户名、密码、权限
    /// </summary>
    public partial class CLoginPerson : ObservableObject
    {
        /// <summary>
        /// 20240801 TCG
        /// 当前用户名
        /// </summary>
        [ObservableProperty]
        private string userName = string.Empty;

        /// <summary>
        /// 20240801 TCG
        /// 是否最高权限
        /// </summary>
        [ObservableProperty]
        private bool isAdministrator;

        partial void OnIsAdministratorChanged(bool value)
        {
            if (value)
            {
                IsAfterSale = true;
                //IsEngineer = true;
                //IsTechnologist = true;
                //IsOperator = true;
                //IsNoPermission = false;
            }
        }

        /// <summary>
        /// 20240801 TCG
        /// 是否售后
        /// </summary>
        [ObservableProperty]
        private bool isAfterSale;

        partial void OnIsAfterSaleChanged(bool value)
        {
            if (value)
            {
                //IsAdministrator = false;
                IsEngineer = true;
                //IsTechnologist = true;
                //IsOperator = true;
                //IsNoPermission = false;
            }
        }

        /// <summary>
        /// 20240801 TCG
        /// 是否工程师
        /// </summary>
        [ObservableProperty]
        private bool isEngineer;

        partial void OnIsEngineerChanged(bool value)
        {
            if (value)
            {
                //IsAdministrator = false;
                //IsAfterSale = true;
                IsTechnologist = true;
                //IsOperator = true;
                //IsNoPermission = false;
            }
        }

        /// <summary>
        /// 20240801 TCG
        /// 是否技术员
        /// </summary>
        [ObservableProperty]
        private bool isTechnologist;

        partial void OnIsTechnologistChanged(bool value)
        {
            if (value)
            {
                //IsAdministrator = false;
                //IsEngineer = false;
                //IsAfterSale = false;
                IsOperator = true;
                //IsNoPermission = false;
            }
        }

        /// <summary>
        /// 20240801 TCG
        /// 是否售后
        /// </summary>
        [ObservableProperty]
        private bool isOperator;

        partial void OnIsOperatorChanged(bool value)
        {
            if (value)
            {
                //IsAdministrator = false;
                //IsEngineer = false;
                //IsAfterSale = false;
                //IsTechnologist = false;
                IsNoPermission = false;
            }
        }

        /// <summary>
        /// 20240801 TCG
        /// 是否无权限
        /// </summary>
        [ObservableProperty]
        private bool isNoPermission;

        partial void OnIsNoPermissionChanged(bool value)
        {
            if (value)
            {
                IsAdministrator = false;
                IsEngineer = false;
                IsAfterSale = false;
                IsTechnologist = false;
                IsOperator = false;
            }
        }

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

        /// <summary>
        /// 权限等级
        /// </summary>
        public PRIVILEGE PrivileageLevel
        {
            get { return privileageLevel; }
            set
            {
                IsNoPermission = true;
                switch (value)
                {
                    case PRIVILEGE.AFTER_SALE:
                        LogoImage = new BitmapImage(
                            new Uri(
                                "pack://application:,,,/WH.Controls;component/Controls/UserLogin/Imgs/售后.png"
                            )
                        );
                        IsAfterSale = true;

                        break;
                    case PRIVILEGE.NOPERMISSION:
                        LogoImage = new BitmapImage(
                            new Uri(
                                "pack://application:,,,/WH.Controls;component/Controls/UserLogin/Imgs/未登录.png"
                            )
                        );
                        IsNoPermission = true;
                        break;
                    case PRIVILEGE.ENGINEER:
                        LogoImage = new BitmapImage(
                            new Uri(
                                "pack://application:,,,/WH.Controls;component/Controls/UserLogin/Imgs/工程师.png"
                            )
                        );
                        IsEngineer = true;
                        break;
                    case PRIVILEGE.TECHNOLOGIST:
                        LogoImage = new BitmapImage(
                            new Uri(
                                "pack://application:,,,/WH.Controls;component/Controls/UserLogin/Imgs/工艺.png"
                            )
                        );
                        IsTechnologist = true;
                        break;
                    case PRIVILEGE.OPERATOR:
                        LogoImage = new BitmapImage(
                            new Uri(
                                "pack://application:,,,/WH.Controls;component/Controls/UserLogin/Imgs/操作员.png"
                            )
                        );
                        IsOperator = true;
                        break;
                    case PRIVILEGE.ADMINISTRATOR:
                        LogoImage = new BitmapImage(
                            new Uri(
                                "pack://application:,,,/WH.Controls;component/Controls/UserLogin/Imgs/管理员.png"
                            )
                        );
                        IsAdministrator = true;
                        break;
                    default:
                        LogoImage = new BitmapImage(
                            new Uri(
                                "pack://application:,,,/WH.Controls;component/Controls/UserLogin/Imgs/未登录.png"
                            )
                        );
                        break;
                }
                SetProperty(ref privileageLevel, value);
                OnPropertyChanged(nameof(LogoImage));
            }
        }

        public ImageSource LogoImage { get; set; } =
            new BitmapImage(
                new Uri(
                    "pack://application:,,,/WH.Controls;component/Controls/UserLogin/Imgs/未登录.png"
                )
            );
    }

    /// <summary>
    /// 20240704 TCG
    /// 权限枚举
    /// </summary>
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
