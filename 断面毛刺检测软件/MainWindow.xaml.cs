using CommunityToolkit.Mvvm.Messaging;
using HandyControl.Controls;
using HandyControl.Data;
using Newtonsoft.Json;
using System.Reactive;
using System.IO;
using System.Text;
using System.Text.Json.Nodes;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using 断面毛刺检测软件.Models;
using 断面毛刺检测软件.ViewModels;
using MessageBox = HandyControl.Controls.MessageBox;
using System.Reactive.Linq;

namespace 断面毛刺检测软件
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : HandyControl.Controls.Window
    {
        IObservable<Unit>? StartStopSource;
        MainVM mainVM = new MainVM();
        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = mainVM;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //半秒之内防止多次点击
            StartStopSource = Observable
                .FromEventPattern<RoutedEventHandler, RoutedEventArgs>(h => Btn_StartStop.Click += h, h => Btn_StartStop.Click -= h)
                .Select(x => Unit.Default)
                .StartWith(Unit.Default);

            StartStopSource
                .Throttle(TimeSpan.FromMilliseconds(500))
                .Subscribe(_ => 
                {
                    if (mainVM.isStart == mainVM.StartStop) return;
                    mainVM.isStart = mainVM.StartStop;
                }
                );
        }

        #region 主题色系
        private void ButtonConfig_OnClick(object sender, RoutedEventArgs e) => PopupConfig.IsOpen = true;

        private void ButtonSkins_OnClick(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource is Button { Tag: SkinType skinType })
            {
                PopupConfig.IsOpen = false;
                if (skinType.Equals(GlobalData.Config.Skin))
                {
                    return;
                }

                GlobalData.Config.Skin = skinType;
                GlobalData.Save();
                ((App)Application.Current).UpdateSkin(skinType);
                Application.Current.MainWindow.ApplyTemplate();
            }
        }

        #endregion

        #region 最大最小拖动 关闭

        private void MainWindow_Close(object sender, RoutedEventArgs e)
        {
            try
            {
                var result = MessageBox.Show("是否需要保存项目？\r\n Do you want to save it ?", "提示|Tips",MessageBoxButton.YesNoCancel,MessageBoxImage.Question,MessageBoxResult.OK);
                if (result == MessageBoxResult.Cancel)
                {
                    e.Handled = true;
                    Growl.Success("取消操作！");
                    return;
                }
                else if (result == MessageBoxResult.Yes)
                {
                    mainVM.ApplyChanges();
                    string json = JsonConvert.SerializeObject(mainVM.Model);
                    using (FileStream fs = new FileStream(mainVM.ProjPath, FileMode.Create, FileAccess.ReadWrite))
                    {
                        byte[] bytes = Encoding.UTF8.GetBytes(json);
                        fs.Write(bytes, 0, bytes.Length);
                        fs.Flush();
                    }
                }

            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                //CLogRec.Error(exception.Message);
            }
            finally
            {
                //CLoading.Close();
            }
            Application.Current.MainWindow.Close();
        }

        private void MainWindow_Maxmum(object sender, RoutedEventArgs e)
        {
            if (WindowState == WindowState.Maximized) WindowState = WindowState.Normal;
            else WindowState = WindowState.Maximized;
            //Growl.Success("文件保存成功！");
            
        }

        private void MainWindow_Minimum(object sender, RoutedEventArgs e)
        {
            Application.Current.MainWindow.WindowState = WindowState.Minimized;
        }

        private void Menu_MouseMove(object sender, MouseEventArgs e)
        {
            if(e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        #endregion

        private void Recent_Click(object sender, RoutedEventArgs e)
        {
            if(e.OriginalSource is MenuItem { Header:string header })
            {
                try
                {
                    var result = MessageBox.Show("是否需要保存当前项目？\r\n Do you want to save it ?", "提示|Tips", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Yes);
                    if (result == MessageBoxResult.Yes)
                    {
                        mainVM.ApplyChanges();
                        string json = JsonConvert.SerializeObject(mainVM.Model);
                        using (FileStream fs = new FileStream(mainVM.ProjPath, FileMode.Create, FileAccess.ReadWrite))
                        {
                            byte[] bytes = Encoding.UTF8.GetBytes(json);
                            fs.Write(bytes, 0, bytes.Length);
                            fs.Flush();
                        }
                        Growl.Success("已保存："+mainVM.Name);
                    }
                    mainVM.Model = JsonConvert.DeserializeObject<MainModel>(File.ReadAllText(header))??new MainModel();
                    mainVM.SystemSettings.RecentProjs.Remove(header);
                    mainVM.SystemSettings.RecentProjs.Insert(0, header);
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception);
                    //CLogRec.Error(exception.Message);
                }
                finally
                {
                    //CLoading.Close();
                }
                
            }
        }

     
    }
}