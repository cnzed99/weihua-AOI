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

namespace XRDigitalLightControl
{
    /// <summary>
    /// 20240724 TCG
    /// 立实为光源控制视图模型
    /// </summary>
    public partial class XRDigitalLightControlVM : CLightControlBase
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
        XRDigitalLightConfig config;

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

        CXRPBL12024Controller Controller;


        public XRDigitalLightControlVM()
            : base()
        {
            Controller = new CXRPBL12024Controller();
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

        [RelayCommand]
        private void CloseConnect()
        {
            Close();
        }
        public override void GetLightValues()
        {
            //try
            //{
            //    var linghtvalues = ReadAllChannelsBrightness(0);
            //    if (linghtvalues != null && linghtvalues.Count() == 4)
            //    {
            //        var lightList = Config.LightChannelList.ToList();
            //        foreach (var item in linghtvalues)
            //        {
            //            var channel = item.respChannel;
            //            int value = item.brightness;
            //            int index = lightList.FindIndex(l => l.Channel == channel.ToString());
            //            if (index >= 0)
            //            {
            //                lightList[index].Value = value;
            //            }
            //        }


            //    }
            //}
            //catch (Exception ex)
            //{
            //    ErrorMessage=ex.Message;
            //}



        }

        /// <summary>
        /// 读取所有4个通道的亮度值和ON/OFF状态
        /// </summary>
        /// <param name="deviceId">设备ID</param>
        /// <returns>长度为4的数组，每个元素为(亮度, 是否开启)</returns>
        public (byte respChannel, byte brightness, bool isOn)[] ReadAllChannelsBrightness(byte deviceId)
        {
            var results = new (byte,byte, bool)[4];
            for (byte ch = 0; ch < 4; ch++)
            {
                results[ch] = ReadChannelBrightness(deviceId, ch);
            }
            return results;
        }

        /// <summary>
        /// 读取指定通道的亮度值和ON/OFF状态
        /// </summary>
        /// <param name="deviceId">设备ID</param>
        /// <param name="channel">通道号（0~3）</param>
        /// <returns>(亮度, ON/OFF状态)</returns>
        public (byte respChannel,byte brightness, bool isOn) ReadChannelBrightness(byte deviceId, byte channel)
        {
            if (channel > 3)
                throw new ArgumentOutOfRangeException(nameof(channel), "通道号必须为0~3");

            byte[] request = Controller.BuildReadRequestPacket(deviceId, channel);
            SendAndReceive(request, out byte[] response);

            // 解析回复（至少7字节：标识符+长度+设备CODE+设备ID+通道号+亮度+ON/OFF+校验和）
            if (response.Length < 7)
                throw new Exception($"回复数据长度不足，期望至少7字节，实际{response.Length}");

            if (response[0] != 0x40)
                throw new Exception("回复标识符错误");

            byte respLength = response[1];
            if (respLength != 5) // 正常回复长度为5（不含标识符和长度自身）
                throw new Exception($"回复长度字段异常: {respLength}");

            byte respDeviceCode = response[2];
            byte respDeviceId = response[3];
            byte respChannel = response[4];
            byte brightness = response[5];
            byte onOff = response[6];

            // 验证校验和（可选）
            // ...

            return (respChannel,brightness, onOff == 0x01);
        }


        /// <summary>
        /// 发送数据并等待接收回复
        /// </summary>
        private void SendAndReceive(byte[] sendData, out byte[] response)
        {
            lock (this.SerialPort) // 防止多线程冲突
            {
                // 清空缓冲区
                this.SerialPort.DiscardInBuffer();

                // 发送
                this.SerialPort.Write(sendData, 0, sendData.Length);

                // 等待设备回复（典型延迟<50ms）
                System.Threading.Thread.Sleep(100);

                // 读取回复
                int bytesToRead = this.SerialPort.BytesToRead;
                if (bytesToRead == 0)
                    throw new TimeoutException("设备无回复");

                response = new byte[bytesToRead];
                this.SerialPort.Read(response, 0, bytesToRead);
            }
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
            }
        }

        [RelayCommand]
        public override void SetChannelValue(CLight light)
        {
            light.Lightvalue = light.Value;
            if (!IsOpen())
                return;

            if (light.Lightvalue < 0 || light.Lightvalue > 255)
            {
                ErrorMessage = "光源值必须在0-255之间";
                return;
            }
            byte lightValue = (byte)light.Lightvalue;
            byte.TryParse(light.Channel, out byte channel);
            if (channel >= 1)
            {
                channel = (byte)(channel - 1); // 将通道号减1，转换为0-3的范围
                byte[] bytes = Controller.BuildSetBrightnessPacket(channel, lightValue);

                Write(bytes);
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

        }

        protected override void DataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
        {
            //int indata = this.SerialPort.ReadByte();
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
