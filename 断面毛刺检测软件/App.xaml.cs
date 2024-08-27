using System;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using Autofac;
using CameraModule;
using CommunicationModule;
using DataQuery;
using HandyControl.Data;
using HandyControl.Properties.Langs;
using HandyControl.Tools;
using MySqlOperatesApi;
using Newtonsoft.Json;
using SaveImageManage;
using WH.Controls.SingleInstance;
using WH.DetectSystem;
using WH.DetectSystem.ViewModels;
using WH.Entity.LogRecord;
using WH.Load;
using 断面毛刺检测软件.Views;
using WH.LightControl;

#if !NET40
using System.Runtime;
#endif

namespace 断面毛刺检测软件
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
#pragma warning disable IDE0052
        [SuppressMessage("ReSharper", "NotAccessedField.Local")]
        private static Mutex AppMutex;
#pragma warning restore IDE0052
        public App()
        {
#if !NET40
            var cachePath = $"{AppDomain.CurrentDomain.BaseDirectory}Cache";
            if (!Directory.Exists(cachePath))
            {
                Directory.CreateDirectory(cachePath);
            }
            ProfileOptimization.SetProfileRoot(cachePath);
            ProfileOptimization.StartProfile("Profile");
#endif
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            Loadkey.GetNumber();
            AppMutex = new Mutex(true, "Metal_Burr", out var createdNew);

            if (!createdNew)
            {
                var current = Process.GetCurrentProcess();

                foreach (var process in Process.GetProcessesByName(current.ProcessName))
                {
                    if (process.Id != current.Id)
                    {
                        Win32Helper.SetForegroundWindow(process.MainWindowHandle);
                        break;
                    }
                }
                Shutdown();
            }
            else
            {
                base.OnStartup(e);

                //UpdateRegistry();

                ShutdownMode = ShutdownMode.OnMainWindowClose;
                GlobalData.Init();

                if (GlobalData.Config.Skin != SkinType.Dark) //默认暗色系
                {
                    UpdateSkin(GlobalData.Config.Skin);
                }
                ConfigHelper.Instance.SetWindowDefaultStyle();
                ConfigHelper.Instance.SetNavigationWindowDefaultStyle();
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
            GlobalData.Save();
            //var lightProcess = App.Container.ResolveKeyed<Process>("LightControl");
            //try
            //{
            //    if (lightProcess != null && lightProcess.Threads != null)
            //        lightProcess?.Kill();
            //}
            //catch (Exception)
            //{
            //    //退出程序
            //}
        }

        internal void UpdateSkin(SkinType skin)
        {
            var skins0 = Resources.MergedDictionaries[0];
            skins0.Source = new Uri(
                $"pack://application:,,,/HandyControl;component/Themes/Skin{skin}.xaml"
            );
            skins0.MergedDictionaries.Clear();
            skins0.MergedDictionaries.Add(
                new ResourceDictionary
                {
                    Source = new Uri(
                        "pack://application:,,,/HandyControl;component/Themes/Theme.xaml"
                    )
                }
            );
            skins0.MergedDictionaries.Add(
                new ResourceDictionary
                {
                    Source = new Uri(
                        $"pack://application:,,,/HandyControl;component/Themes/Skin{skin}.xaml"
                    )
                }
            );
            var skins1 = Resources.MergedDictionaries[1];
            skins1.Source = new Uri(
                $"pack://application:,,,/HandyControl;component/Themes/Skin{skin}.xaml"
            );
            skins1.MergedDictionaries.Clear();
            skins1.MergedDictionaries.Add(
                new ResourceDictionary
                {
                    Source = new Uri(
                        "pack://application:,,,/HandyControl;component/Themes/Theme.xaml"
                    )
                }
            );

            Current.MainWindow?.OnApplyTemplate();
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            ConfigureServices();

            var viewModel = Container.Resolve<CMainModelsModelVM>();
            var cmodel = new WH.DetectSystem.Models.CMainModel();
            viewModel.CMainMModel.CMainModels.Add(cmodel);
            CMainVM mainVM = new CMainVM();
            //mainVM.Model = cmodel;
            viewModel.CMainVMs.Add(mainVM);
            var mainWindow = Container.Resolve<MainWindow>();
            mainWindow.DataContext = viewModel;
            mainWindow?.Show();
        }

        public static IContainer Container { get; set; }

        private static void ConfigureServices()
        {
            var builder = CPublicServices.ConfigureServices();
            builder.Register(c => CSysSet.LoadParameter()).SingleInstance();
            builder.RegisterType<CMainModelsModelVM>().SingleInstance();
            builder.RegisterType<MainWindow>().SingleInstance();

            //系统设置
            builder
                .Register(c =>
                    SingleInstance.Create<Lazy<SystemSettingWindow>, SystemSettingWindow>()
                )
                .InstancePerDependency();
            //新建
            builder
                .Register(c => SingleInstance.Create<Lazy<NewProjWindow>, NewProjWindow>())
                .InstancePerDependency();
            //修改
            builder
                .Register(c => SingleInstance.Create<Lazy<ModifyProjWindow>, ModifyProjWindow>())
                .InstancePerDependency();
            //离线测试
            builder
                .Register(c => SingleInstance.Create<Lazy<OffLineTestWindow>, OffLineTestWindow>())
                .InstancePerDependency();
            //修改工程
            builder
                .Register(c => SingleInstance.Create<Lazy<ModifyProjWindow>, ModifyProjWindow>())
                .InstancePerDependency();
            //存图设置
            builder
                .Register(c => SingleInstance.Create<Lazy<CSaveImageSetFrm>, CSaveImageSetFrm>())
                .InstancePerDependency();
            //数据库设置
            builder
                .Register(c => SingleInstance.Create<Lazy<SQLSetWindow>, SQLSetWindow>())
                .InstancePerDependency();
            //数据查看
            builder
                .Register(c => SingleInstance.Create<Lazy<DataQueryWindow>, DataQueryWindow>())
                .InstancePerDependency();
            //通讯配置
            builder
                .Register(c =>
                    SingleInstance.Create<Lazy<OpenCommunicationList>, OpenCommunicationList>()
                )
                .InstancePerDependency();
            //相机配置
            builder
                .Register(c => SingleInstance.Create<Lazy<CameraSetWindow>, CameraSetWindow>())
                .InstancePerDependency();
            //光源控制
            //var lightProcess = Invoke("./WH.LightControl.exe");
            //builder.RegisterInstance(lightProcess).Keyed<Process>("LightControl").SingleInstance();
            builder
             .Register(c => SingleInstance.Create<Lazy<LightSetWindow>, LightSetWindow>())
             .InstancePerDependency();

            builder
           .Register(c => SingleInstance.Create<Lazy<AddLightWindow>, AddLightWindow>())
           .InstancePerDependency();

            //手动调试
            builder
                .Register(c =>
                    SingleInstance.Create<Lazy<TimeTriggerTestWindow>, TimeTriggerTestWindow>()
                )
                .InstancePerDependency();

            Container = builder.Build();
            CPublicServices.Container = Container;
        }

        public static Process Invoke(string file)
        {
            if (file != null && File.Exists(file))
            {
                Process Opener = new Process();
                Opener.StartInfo.FileName = file;
                Opener.StartInfo.UseShellExecute = false;
                return Opener;
            }
            return null;
        }
    }

    internal class GlobalData
    {
        public static void Init()
        {
            if (File.Exists(AppConfig.SavePath))
            {
                try
                {
                    var json = File.ReadAllText(AppConfig.SavePath);
                    Config =
                        (
                            string.IsNullOrEmpty(json)
                                ? new AppConfig()
                                : JsonConvert.DeserializeObject<AppConfig>(json)
                        ) ?? new AppConfig();
                }
                catch
                {
                    Config = new AppConfig();
                }
            }
            else
            {
                Config = new AppConfig();
            }
        }

        public static void Save()
        {
            var json = JsonConvert.SerializeObject(Config);
            File.WriteAllText(AppConfig.SavePath, json);
        }

        public static AppConfig Config { get; set; } = new AppConfig();

        public static bool NotifyIconIsShow { get; set; } = true;
    }

    internal class AppConfig
    {
        public static readonly string SavePath =
            $"{AppDomain.CurrentDomain.BaseDirectory}AppConfig.json";

        public SkinType Skin { get; set; }
    }

    internal class Win32Helper
    {
        [DllImport("user32.dll", ExactSpelling = true, CharSet = CharSet.Auto)]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("winmm.dll", EntryPoint = "mciSendString", CharSet = CharSet.Auto)]
        public static extern int MciSendString(
            string lpstrCommand,
            string lpstrReturnString,
            int uReturnLength,
            int hwndCallback
        );
    }
}
