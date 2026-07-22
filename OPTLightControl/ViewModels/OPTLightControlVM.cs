using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HandyControl.Controls;
using WH.Entity;
using WH.LightControl;
using CSharp_OPTControllerAPI;

namespace OPTLightControl
{
    /// <summary>
    /// 20260129 龚伟东
    /// 奥普特光源控制视图模型
    /// </summary>
    public partial class OPTLightControlVM : CLightControlBase
    {
        private OPTControllerAPI OPTController;

        //private bool isConnected = false;  // 添加私有字段记录连接状态


        /// <summary>
        /// 20260129 龚伟东
        /// 光源配置
        /// </summary>
        [ObservableProperty]
        OPTLightConfig config;

        /// <summary>
        /// 20260129 龚伟东
        /// 串口消息
        /// </summary>
        StringBuilder message = new();

        /// <summary>
        /// 20260129 龚伟东
        /// 读取通道数值解析匹配
        /// </summary>
        Regex lightReadRegex = new Regex("S(.*?)#");

        /// <summary>
        /// 20260129 龚伟东
        /// 不同方法的解析方式委托
        /// </summary>
        Action<string> action;

        /// <summary>
        /// 20260129 龚伟东
        /// 读取操作完成后才能进行下一次读取
        /// </summary>
        SemaphoreSlim slim = new SemaphoreSlim(1);

        //[ObservableProperty]
        //string errorMessage;

        public OPTLightControlVM()
            : base()
        {
            OPTController = new OPTControllerAPI();
        }

        public override bool Close()
        {
            try
            {
                //if (IsSerialPortOpen)
                //{
                int result = OPTController.ReleaseSerialPort();
                if (result == 0)
                {
                    // isConnected = false;
                    IsSerialPortOpen = false;
                    ErrorMessage = string.Empty;
                    Growl.Info("光源已断开");
                    return true;
                }
                else
                {
                    ErrorMessage = $"断开连接失败，错误码: {result}";
                    return false;
                }
                //}
               // return true;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"断开连接异常: {ex.Message}";
                return false;
            }


            //if (IsOpen())
            //{
            //    OPTController.ReleaseSerialPort();
            //}
            //return true;
        }

        [RelayCommand]
        private void CloseConnect()
        {
            Close();
        }

        /// <summary>
        /// 20260129 龚伟东
        /// 从光源设备读取所有通道的当前亮度值
        /// </summary>
        public override void GetLightValues()
        {
            try
            {
                if (slim.Wait(2000))
                {
                    message.Clear();
                    var lightList = Config.LightChannelList?.ToList();
                    // 调用奥普特SDK读取亮度
                    if (lightList != null)
                    {
                        foreach (var light in lightList)
                        {
                            int intensity = 0;
                            //long result = OPTController.ReadIntensity(channel, ref intensity);
                            int channel = ConvertToOptChannelNumber(light.Channel);
                            long result = OPTController.ReadTriggerWidth(channel, ref intensity);
                            light.Value = intensity;
                        }
                    }
                    slim.Release();
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"读取亮度异常: {ex.Message}";

            }
        }


        /// <summary>
        /// 20260129 龚伟东
        /// 判断串口是否打开
        /// </summary>
        public override bool IsOpen()
        {
            // return isConnected;
            // return IsSerialPortOpen; // 使用属性
            return OPTController.IsConnect() == 0 ? true : false;
        }


        public override bool Open(CLightParamsBase lightParams)
        {
            //if (this.SerialPort.IsOpen)
            //{
            //    this.SerialPort.Close();
            //}
            //if (string.IsNullOrEmpty(lightParams.Port?.Name))
            //    return false;
            //this.SerialPort.PortName = lightParams.Port.Name; //lightParams.Port;
            //this.SerialPort.BaudRate = (int)lightParams.BaudRate;
            //this.SerialPort.Parity = lightParams.Parity;
            //this.SerialPort.DataBits = (int)lightParams.DataBits;
            //this.SerialPort.StopBits = lightParams.StopBits;
            //this.SerialPort.Handshake = lightParams.HandShake;
            //string[] ports = SerialPort.GetPortNames();
            _ = Open();
            return true;
        }

        /// <summary>
        /// 20260129 龚伟东
        /// 打开串口，读写配置
        /// </summary>
        [RelayCommand]
        private async Task Open()
        {
            try
            {
                // 如果已经连接，先断开
                int re = OPTController.IsConnect();
                if (re == 0)
                {
                    Close();
                }
                // 检查配置
                //if (Config == null || Config.Port == null || string.IsNullOrEmpty(Config.Port.Name))
                //{
                //    ErrorMessage = "请选择串口号";
                //    Growl.Warning("请选择串口号");
                //    return;
                //}

                long lRet;
                lRet = OPTController.InitSerialPort(Config.Port.Name);
                if (lRet == 0)
                {
                    // isConnected = true; // 设置连接状态为已连接
                    IsSerialPortOpen = true;
                    ErrorMessage = string.Empty;
                    GetLightValues();
                }
                else
                {
                    // isConnected = false; // 设置连接状态为未连接
                    IsSerialPortOpen = false;
                }

            }
            catch (Exception ex)
            {
                // isConnected = false;
                IsSerialPortOpen = false;
                ErrorMessage = $"连接异常: {ex.Message}";
                Growl.Error($"连接异常: {ex.Message}");
            }
        }

        [RelayCommand]
        //protected override void SetChannelValue(CLight light)
        public override void SetChannelValue(CLight light)
        {
            try
            {
                // 先保存设置值
                light.Lightvalue = light.Value;
                //if (!IsOpen())
                //    return;

                // 检查连接状态
                if (!IsOpen())
                {
                    ErrorMessage = "请先连接光源";
                    Growl.Warning("请先连接光源");
                    return;
                }

                //OPTController.SetIntensity(ConvertToOptChannelNumber(light.Channel), light.Lightvalue);
                int channel = ConvertToOptChannelNumber(light.Channel);
               // OPTController.TurnOnChannel(channel);
                OPTController.SetTriggerWidth(channel, light.Lightvalue);

            }
            catch (Exception ex)
            {
                ErrorMessage = $"设置亮度异常: {ex.Message}";
                Growl.Error($"设置异常: {ex.Message}");
            }


        }

        public int ConvertToOptChannelNumber(string channel)
        {
            if (string.IsNullOrEmpty(channel))
                return 0;
            // 如果是单个字母
            if (channel.Length == 1 && char.IsLetter(channel[0]))
            {
                char upperChar = char.ToUpper(channel[0]);
                int channelNumber = upperChar - 'A' + 1;

                // 验证范围
                if (channelNumber >= 1 && channelNumber <= 36)
                    return channelNumber;
                else
                    return 0;
            }
            return 0;  // 转换失败
        }


        [RelayCommand]
        protected override void ReSet()
        {
            if (!IsOpen())
                return;
        }



        protected override void DataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
        {
            string indata = this.SerialPort.ReadExisting();
            message.Append(indata);
            if (!indata.EndsWith("#"))
                return;

            action?.Invoke(message.ToString());
        }


    }
}
