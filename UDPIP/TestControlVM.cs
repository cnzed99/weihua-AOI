using System.Collections.ObjectModel;
using CommunicationModule;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HandyControl.Controls;

namespace UDPIP
{
    /// <summary>
    /// 2024.7.21 李焕彬
    /// 通讯控件VM
    /// </summary>
    public partial class TestControlVM : ObservableObject
    {
        public TestControlVM() { }

        public TestControlVM(CUdpIpCommPart com)
        {
            this.com = com;
            this.Config = com.setting;
            Com.ReceivedEvent += ReceiveShow; //显示委托
            Com.ConnectedEventArgs += Connected; //连接后响应事件
        }

        /// <summary>
        /// 2024.7.17 l李焕彬
        /// 关闭窗口时关闭事件
        /// </summary>
        public void Close()
        {
            Com.ReceivedEvent -= ReceiveShow;
            Com.ConnectedEventArgs -= Connected;
        }

        /// <summary>
        /// 2024.7.21 李焕彬
        /// 通讯对象
        /// </summary>
        [ObservableProperty]
        CUdpIpCommPart com;

        /// <summary>
        /// 2024.7.21 李焕彬
        /// 通讯配置
        /// </summary>
        [ObservableProperty]
        CUdpIpCommunicationSetting config;

        /// <summary>
        /// 2024.7.17 l李焕彬
        /// 接收数据
        /// </summary>
        [ObservableProperty]
        private string txtRecv;

        /// <summary>
        /// 2024.7.17 l李焕彬
        /// 转码后数据
        /// </summary>
        [ObservableProperty]
        private string txtDecode;

        /// <summary>
        /// 2024.7.17 l李焕彬
        /// 信息提示
        /// </summary>
        [ObservableProperty]
        private ObservableCollection<string> listTips = new ObservableCollection<string>();

        /// <summary>
        /// 2024.7.17 l李焕彬
        /// 发送数据
        /// </summary>
        /// <param name="send">字符串</param>
        [RelayCommand]
        public void Send(string send)
        {
            try
            {
                CCommunicationManagement.ComLogger.Info(Properties.Resources.SendInfo + send);
                byte[] txtByte = CConvertBytes.GetBytes(
                    send.Trim(),
                    Config.SysConvertType,
                    Config.EncodingType
                );
                Com.SendData(txtByte);
            }
            catch (Exception ex)
            {
                Growl.Error(Properties.Resources.SendError + ex.Message);
            }
        }

        /// <summary>
        /// 2024.7.17 l李焕彬
        /// Ping
        /// </summary>
        [RelayCommand]
        public void Ping()
        {
            CCommunicationManagement.ComLogger.Info(Properties.Resources.PingInfo);
            TxtRecv = "";
            try
            {
                using (System.Diagnostics.Process pc = new System.Diagnostics.Process())
                {
                    pc.StartInfo.FileName = "cmd.exe";
                    pc.StartInfo.UseShellExecute = false; //是否使用操作系统shell启动
                    pc.StartInfo.RedirectStandardInput = true; //接受来自调用程序的输入信息
                    pc.StartInfo.RedirectStandardOutput = true; //由调用程序获取输出信息
                    pc.StartInfo.RedirectStandardError = true; //重定向标准错误输出
                    pc.StartInfo.CreateNoWindow = true; //不显示程序窗口
                    pc.Start(); //启动程序

                    string cmdStr = "Ping " + Config.RemoteIP + "& exit";
                    //向cmd窗口发送输入信息
                    pc.StandardInput.WriteLine(cmdStr);
                    // pc.StandardInput.WriteLine("exit");
                    pc.StandardInput.AutoFlush = false;
                    string output = pc.StandardOutput.ReadToEnd();
                    TxtRecv = output;
                    // pc.WaitForExit();
                    pc.Close();
                }
            }
            catch (Exception ex)
            {
                Growl.Error(Properties.Resources.PingError + ex.Message);
            }
        }

        /// <summary>
        /// 2024.7.17 l李焕彬
        /// 数据接收事件
        /// </summary>
        /// <param name="data">接收数据</param>
        private void ReceiveShow(byte[] data)
        {
            try
            {
                System.Windows.Application.Current.Dispatcher.Invoke(
                    new Action(() =>
                    {
                        TxtRecv = CConvertBytes.GetString(
                            data,
                            Config.SysConvertType,
                            Config.EncodingType
                        );
                        string message =
                            DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "  Receive!";
                        if (ListTips.Count > 50)
                        {
                            ListTips.RemoveAt(ListTips.Count - 1);
                        }
                        ListTips.Insert(0, message);
                        CCommunicationManagement.ComLogger.Info(
                            Properties.Resources.CommInfo + message
                        );
                    })
                );
            }
            catch (Exception ex)
            {
                Growl.Error(Properties.Resources.RecvError + ex.Message);
            }
        }

        /// <summary>
        /// 2024.7.17 l李焕彬
        /// 连接事件
        /// </summary>
        /// <param name="enable">连接状态</param>
        /// <param name="msg">消息</param>
        private void Connected(bool enable, string msg)
        {
            try
            {
                System.Windows.Application.Current.Dispatcher.Invoke(
                    new Action(() =>
                    {
                        if (ListTips.Count > 50)
                        {
                            ListTips.RemoveAt(ListTips.Count - 1);
                        }
                        ListTips.Insert(0, msg);
                        CCommunicationManagement.ComLogger.Info(
                            Properties.Resources.CommInfo + msg
                        );
                    })
                );
            }
            catch (Exception ex)
            {
                CCommunicationManagement.ComLogger.Error(
                    Properties.Resources.ConnectError + ex.Message
                );
                Growl.Error(Properties.Resources.ConnectError + ex.Message);
            }
        }
    }
}
