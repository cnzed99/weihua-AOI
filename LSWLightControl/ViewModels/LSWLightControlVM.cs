using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Documents;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WH.LightControl;
using WH.Entity;

namespace LSWLightControl
{
    /// <summary>
    /// 20240724 TCG
    /// 立实为光源控制视图模型
    /// </summary>
    public partial class LSWLightControlVM : LightControlBase
    {
        /// <summary>
        /// 20240724 TCG
        /// 触发模式改变
        /// </summary>
        [RelayCommand]
        void OnTriggerModeChanged()
        {
            if (IsOpen())
            {
                if (Config.TriggerMode)
                {
                    Write("$TH#");
                }
                else
                {
                    Write("$TL#");
                }
            }
        }

        /// <summary>
        /// 20240724 TCG
        /// 触发沿改变
        /// </summary>
        [RelayCommand]
        void OnTriggerEdgeChanged()
        {
            if (IsOpen())
            {
                if (Config.TriggerEdge)
                {
                    Write("$HF#");
                }
                else
                {
                    Write("$LF#");
                }
            }
        }

        /// <summary>
        /// 20240724 TCG
        /// 工作模式改变
        /// </summary>
        [RelayCommand]
        void OnWorkModeChanged()
        {
            if (IsOpen())
            {
                if (Config.WorkMode)
                {
                    Write("$WK#");
                }
                else
                {
                    Write("$DB#");
                }
            }
        }

        /// <summary>
        /// 20240724 TCG
        /// 串口是否打开
        /// </summary>
        [ObservableProperty]
        bool isSerialPortOpen;

        /// <summary>
        /// 20240724 TCG
        /// 光源配置
        /// </summary>
        [ObservableProperty]
        LSWLightConfig config;

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

        [ObservableProperty]
        string errorMessage;

        public LSWLightControlVM()
            : base()
        {
            //if (File.Exists(LightParamsBase.s_LightConfigPath))
            //{
            //    this.Config = ConfigAPI.Load<LSWLightConfig>(LightParamsBase.s_LightConfigPath);
            //}
            //else
            //{
            //    this.Config = new LSWLightConfig();
            //}
            //_ = this.Open();
        }

        public override bool Close()
        {
            if (this.SerialPort.IsOpen)
            {
                this.SerialPort.Close();
            }
            return true;
        }

        public override void GetLightValues()
        {
            if (slim.Wait(2000))
            {
                message.Clear();
                StringBuilder sb = new StringBuilder("$");
                foreach (var light in Config.LightChannelList)
                {
                    sb.Append("S");
                    sb.Append(light.Channel);
                    sb.Append("R&");
                }
                sb.Remove(sb.Length - 1, 1);
                sb.Append("#");
                this.action = ReadLightValues;
                Write(sb.ToString());
            }
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
                GetLightValues();
                GetTriggerMode();
                await Task.Delay(50);
                OnTriggerModeChanged();
                await Task.Delay(50);
                OnTriggerEdgeChanged();
                await Task.Delay(50);
                OnWorkModeChanged();
            }
        }

        [RelayCommand]
        protected override void SetChannelValue(CLight light)
        {
            if (!IsOpen())
                return;
            var str = $"$S{light.Channel}{light.Value:D3}#";
            Write(str);
        }

        [RelayCommand]
        protected override void ReSet()
        {
            if (!IsOpen())
                return;
            Write("$REC#");
            GetLightValues();
            GetTriggerMode();
        }

        [RelayCommand]
        protected override void Save()
        {
            ConfigAPI.Save(Config, LightParamsBase.s_LightConfigPath);
        }

        /// <summary>
        /// 20240724 TCG
        /// 读取触发模式
        /// </summary>
        protected void GetTriggerMode()
        {
            if (slim.Wait(2000))
            {
                message.Clear();
                action = s =>
                {
                    switch (s)
                    {
                        case "$TH#":
                            Config.TriggerMode = true;
                            break;
                        case "$TL#":
                            Config.TriggerMode = false;
                            break;
                    }
                    slim.Release();
                    action = null;
                };
                Write("$TR#");
            }
        }

        protected override void DataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
        {
            string indata = this.SerialPort.ReadExisting();
            message.Append(indata);
            if (!indata.EndsWith("#"))
                return;

            action?.Invoke(message.ToString());
        }

        protected async void Write(string msg)
        {
            try
            {
                ErrorMessage = string.Empty;
                await Task.Run(() =>
                {
                    try
                    {
                        this.SerialPort.Write(msg);
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                });
            }
            catch (TimeoutException e)
            {
                ErrorMessage = e.Message;
            }
            catch (Exception e)
            {
                ErrorMessage = e.Message;
            }
        }
    }
}
