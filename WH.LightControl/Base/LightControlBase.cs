using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HandyControl.Controls;
using Newtonsoft.Json;
using WH.Entity.CommonLib;

namespace WH.LightControl
{
    /// <summary>
    /// 光源操作基类
    /// 2024.08.25 鲍赞宝
    /// </summary>
    public abstract partial class CLightControlBase : ObservableObject
    {
        /// <summary>
        /// 20260129 龚伟东
        /// 串口是否打开
        /// </summary>
        [ObservableProperty]
        bool isSerialPortOpen;

        /// <summary>
        /// 20260307 鲍赞宝
        /// 错误消息提示
        /// </summary>
        [ObservableProperty]
        string errorMessage;
        /// <summary>
        /// 20240825 鲍赞宝
        /// 光源主配置
        /// </summary>
        [ObservableProperty]
        CLightParamsBase baseConfig;

        /// <summary>
        /// 20240724 TCG
        /// 串口号
        /// </summary>
        [ObservableProperty]
        [property: JsonIgnore]
        private SerialPort serialPort = new SerialPort();

        protected CLightControlBase()
        {
            SerialPort.ReadBufferSize = 1024;
            SerialPort.WriteBufferSize = 1024;
            SerialPort.WriteTimeout = 2000;
            SerialPort.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandler); // 接收到数据时的事件
        }
        /// <summary>
        /// 更新设置基类的排至参数
        /// 2024.08.25 鲍赞宝
        /// </summary>
        /// <param name="baseparam"></param>
        public void SetBaseParam(CLightParamsBase baseparam)
        {
            BaseConfig = baseparam;
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
        public virtual bool Open(CLightParamsBase lightParams)
        {
            try
            {
                if (this.SerialPort.IsOpen)
                {
                    this.SerialPort.Close();
                }
                if (string.IsNullOrEmpty(lightParams.Port?.Name))
                    return false;
                this.SerialPort.PortName = lightParams.Port.Name; //lightParams.Port;
                this.SerialPort.BaudRate = (int)lightParams.BaudRate;
                this.SerialPort.Parity = lightParams.Parity;
                this.SerialPort.DataBits = (int)lightParams.DataBits;
                this.SerialPort.StopBits = lightParams.StopBits;
                this.SerialPort.Handshake = lightParams.HandShake;
                string[] ports = SerialPort.GetPortNames();
                if (ports.Contains(this.SerialPort.PortName))
                {
                    this.SerialPort.Open();
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.ToString();
                return false;
            }

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
        public abstract void SetChannelValue(CLight light);

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
        //  protected abstract void Save();


        /// <summary>
        ///  2024.08.26 鲍赞宝
        /// 保存所有配置参数
        /// </summary>
        [RelayCommand]        
        protected virtual void Save()
        {
            CLinghtManagement.SaveLightParams();
        }


        /// <summary>
        ///  2024.08.26 鲍赞宝
        /// 删除光源控制器
        /// </summary>
        /// <param name="paramobj"></param>
        [RelayCommand]
        public virtual void Delete(object paramobj)
        {
            Growl.AskGlobal(Properties.Resources.删除光源询问, b =>
            {
                if (b)
                {
                    if (paramobj is CLightParamsBase param)
                    {
                        string lightkey = param.LightBrandName + "&" + param.LightStationName;
                        CLinghtManagement.LightControlDict.Remove(lightkey);
                        // CLinghtManagement.LightParamDict.Remove(lightkey);

                        CLinghtManagement.SaveLightParams();
                    }
                }
                return true;
            });
        }
    }
}
