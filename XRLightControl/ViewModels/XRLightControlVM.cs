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

namespace XRLightControl
{
    /// <summary>
    /// 20240724 TCG
    /// 立实为光源控制视图模型
    /// </summary>
    public partial class XRLightControlVM : CLightControlBase
    {
        /// <summary>
        /// 20240724 TCG
        /// 触发模式改变
        /// </summary>
        [RelayCommand]
        void OnTriggerModeChanged()
        {
        }

        /// <summary>
        /// 20240724 TCG
        /// 触发沿改变
        /// </summary>
        [RelayCommand]
        void OnTriggerEdgeChanged()
        {
        }

        /// <summary>
        /// 20240724 TCG
        /// 工作模式改变
        /// </summary>
        [RelayCommand]
        void OnWorkModeChanged()
        {
        }

        ///// <summary>
        ///// 20240724 TCG
        ///// 串口是否打开
        ///// </summary>
        //[ObservableProperty]
        //bool isSerialPortOpen;

        /// <summary>
        /// 20240724 TCG
        /// 光源配置
        /// </summary>
        [ObservableProperty]
        XRLightConfig config;

        /// <summary>
        /// 20240724 TCG
        /// 串口消息
        /// </summary>
        StringBuilder message = new();

        /// <summary>
        /// 20240724 TCG
        /// 读取通道数值解析匹配
        /// </summary>
        Regex lightReadRegex = new Regex("S(.*?)#");

        /// <summary>
        /// 20240724 TCG
        /// 不同方法的解析方式委托
        /// </summary>
        Action<string> action;

        /// <summary>
        /// 20240724 TCG
        /// 读取操作完成后才能进行下一次读取
        /// </summary>
        SemaphoreSlim slim = new SemaphoreSlim(1);

        CXRPBL15048Controller Controller;


        public XRLightControlVM()
            : base()
        {
            Controller = new CXRPBL15048Controller();
        }

        public override bool Close()
        {
            if (this.SerialPort.IsOpen)
            {
                this.SerialPort.Close();
                IsSerialPortOpen = false;
            }
            return true;
        }

        public override void GetLightValues()
        {
            //if (slim.Wait(2000))
            //{
            //    message.Clear();
            //    StringBuilder sb = new StringBuilder("$");
            //    foreach (var light in Config.LightChannelList)
            //    {
            //        sb.Append("S");
            //        sb.Append(light.Channel);
            //        sb.Append("R&");
            //    }
            //    sb.Remove(sb.Length - 1, 1);
            //    sb.Append("#");
            //    this.action = ReadLightValues;
            //    Write(sb.ToString());
            //}
        }

        /// <summary>
        /// 20240724 TCG
        /// 解析读取到的四通道光源值
        /// </summary>
        /// <param name="message"></param>
        protected void ReadLightValues(string message)
        {
            var collection = lightReadRegex.Matches(message);
            var lightList = Config.LightChannelList.ToList();
            foreach (Match item in collection)
            {
                var channel = item.Groups[1].Value.Substring(0, 1);
                var value = int.Parse(item.Groups[1].Value.Substring(1));
                int index = lightList.FindIndex(l => l.Channel == channel);
                if (index >= 0)
                {
                    lightList[index].Value = value;
                }
            }
            slim.Release();
            action = null;
        }

        public override bool IsOpen()
        {
            IsSerialPortOpen = this.SerialPort.IsOpen;
            return IsSerialPortOpen;
        }

        /// <summary>
        /// 20240724 TCG
        /// 打开串口，读写配置
        /// </summary>
        [RelayCommand]
        private async Task Open()
        {
            if (base.Open(Config))
            {
                IsSerialPortOpen = true;
                ErrorMessage = string.Empty;
                GetLightValues();
                //GetTriggerMode();
                //await Task.Delay(50);
                //OnTriggerModeChanged();
                //await Task.Delay(50);
                //OnTriggerEdgeChanged();
                //await Task.Delay(50);
                //  OnWorkModeChanged();
            }
        }

        [RelayCommand]
        public override void SetChannelValue(CLight light)
        {
            light.Lightvalue = light.Value;
            if (!IsOpen())
                return;

            //var str = $"$S{light.Channel}{light.Lightvalue:D3}#";
            List<byte[]> bytes = Controller.SetExposureTime(light);
            for (int i = 0; i < bytes.Count; i++)
            {
                Write(bytes[i]);
                Thread.Sleep(20);
            }
        }

        [RelayCommand]
        protected override void ReSet()
        {
            //if (!IsOpen())
            //    return;
            //Write("$REC#");
            //GetLightValues();
            //GetTriggerMode();
        }

        //[RelayCommand]
        //protected override void Save()
        //{
        //    //ConfigAPI.Save(Config, CLightParamsBase.s_LightConfigPath);
        //    CLinghtManagement.SaveConfigParams();
        //}

        /// <summary>
        /// 20240724 TCG
        /// 读取触发模式
        /// </summary>
        protected void GetTriggerMode()
        {
            //if (slim.Wait(2000))
            //{
            //    message.Clear();
            //    action = s =>
            //    {
            //        switch (s)
            //        {
            //            case "$TH#":
            //                Config.TriggerMode = true;
            //                break;
            //            case "$TL#":
            //                Config.TriggerMode = false;
            //                break;
            //        }
            //        slim.Release();
            //        action = null;
            //    };
            //    Write("$TR#");
            //}
        }

        protected override void DataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
        {
           // int indata = this.SerialPort.ReadByte();
            //message.Append(indata);
            //if (!indata.EndsWith("#"))
            //    return;

            //action?.Invoke(message.ToString());
        }

        protected void Write(byte[] msg)
        {
            try
            {
                ErrorMessage = string.Empty;

                this.SerialPort.DiscardInBuffer(); // 清空输入缓冲区
                this.SerialPort.Write(msg, 0, msg.Length);


            }
            catch (Exception e)
            {
                ErrorMessage = e.Message;
            }
        }
    }
}
