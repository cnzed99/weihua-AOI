using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Net;
using System.Windows;
using HandyControl.Data;
using HandyControl.Tools;

namespace WH.LightControl
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static Mutex AppMutex;

        protected override void OnStartup(StartupEventArgs e)
        {
            AppMutex = new Mutex(true, "LightControl", out var createdNew);

            if (!createdNew)
            {
                Shutdown();
            }
            else
            {
                base.OnStartup(e);
            }
        }
    }
}
