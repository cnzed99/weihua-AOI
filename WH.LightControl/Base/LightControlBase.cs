using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Newtonsoft.Json;
using WH.Entity.CommonLib;

namespace WH.LightControl
{
    public abstract partial class LightControlBase : ObservableObject
    {
        /// <summary>
        /// 20240724 TCG
        /// 串口号
        /// </summary>
        [ObservableProperty]
        [property: JsonIgnore]
        private SerialPort serialPort = new SerialPort();

        protected LightControlBase()
        {
            SerialPort.ReadBufferSize = 1024;
            SerialPort.WriteBufferSize = 1024;
            SerialPort.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandler); // 接收到数据时的事件
        }

        /// <summary>
        /// 20240724 TCG
        /// 串口数据接受回调
        /// </summary>
        protected abstract void DataReceivedHandler(object sender, SerialDataReceivedEventArgs e);

        /// <summary>
        /// 20240724 TCG
        /// 打开光源串口
        /// </summary>
        /// <param name="setParam">是否设置亮度</param>
        /// <param name="port">串口号</param>
        /// <returns></returns>
        public virtual bool Open(LightParamsBase lightParams)
        {
            if (this.SerialPort.IsOpen)
            {
                this.SerialPort.Close();
            }
            this.SerialPort.PortName = "COM" + (int)lightParams.Port; //lightParams.Port;
            this.SerialPort.BaudRate = (int)lightParams.BaudRate;
            this.SerialPort.Parity = lightParams.Parity;
            this.SerialPort.DataBits = (int)lightParams.DataBits;
            this.SerialPort.StopBits = lightParams.StopBits;
            this.SerialPort.Handshake = lightParams.HandShake;
            this.SerialPort.Open();
            return true;
        }

        /// <summary>
        /// 20240724 TCG
        /// 关闭光源串口
        /// </summary>
        public abstract bool Close();

        /// <summary>
        /// 20240724 TCG
        /// 串口是否已打开
        /// </summary>
        public abstract bool IsOpen();

        /// <summary>
        /// 20240724 TCG
        /// 设置通道亮度
        /// </summary>
        /// <param name="light">-1设置四个通道，1、2、3、4设置对应通道</param>
        protected abstract void SetChannelValue(CLight light);

        /// <summary>
        /// 20240724 TCG
        /// 获取通道亮度至数组
        /// </summary>
        public abstract void GetLightValues();

        /// <summary>
        /// 20240724 TCG
        /// 恢复出厂
        /// </summary>
        protected abstract void ReSet();

        /// <summary>
        /// 20240724 TCG
        /// 保存配置
        /// </summary>
        protected abstract void Save();
    }
}
