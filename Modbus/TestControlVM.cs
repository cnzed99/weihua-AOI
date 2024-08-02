using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunicationModule;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HandyControl.Controls;

namespace Modbus
{
    /// <summary>
    /// 2024.7.21 李焕彬
    /// 通讯控件VM
    /// </summary>
    public partial class TestControlVM : ObservableObject
    {
        public TestControlVM() { }

        public TestControlVM(CModbusCommPart com)
        {
            this.com = com;
            this.Config = com.setting;
            Com.isReadTestElem = true;
            Com.ConnectedEventArgs += Connected;
        }

        /// <summary>
        /// 2024.7.17 l李焕彬
        /// 关闭窗口时关闭事件
        /// </summary>
        public void Close()
        {
            Com.ConnectedEventArgs -= Connected;
            Com.isReadTestElem = false;
        }

        /// <summary>
        /// 2024.7.21 李焕彬
        /// 通讯对象
        /// </summary>
        [ObservableProperty]
        CModbusCommPart com;

        /// <summary>
        /// 2024.7.21 李焕彬
        /// 通讯配置
        /// </summary>
        [ObservableProperty]
        CModbusSetting config;

        /// <summary>
        /// 2024.7.12 李焕彬
        /// 删除元件
        /// </summary>
        /// <param name="elem">元件</param>
        [RelayCommand]
        public void Del(CElement elem)
        {
            Config.TestElems.Remove(elem);
        }

        /// <summary>
        /// 2024.7.12 李焕彬
        /// 增加元件
        /// </summary>
        [RelayCommand]
        public void Add()
        {
            Config.TestElems.Add(new CElement(Config.token, "Y1", 1));
        }

        /// <summary>
        /// 2024.7.12 李焕彬
        /// 写入元件
        /// </summary>
        [RelayCommand]
        public void Write()
        {
            foreach (var elem in Config.TestElems)
            {
                switch (elem.Type)
                {
                    case EMELEMTYPE.EMELEMM:
                        Com.WriteSingleCoil(elem.Addr, elem.WriteValue == 1 ? true : false);
                        break;
                    case EMELEMTYPE.EMELEMD:
                        Com.WriteSingleRegister(elem.Addr, elem.WriteValue);
                        break;
                }
            }
        }

        /// <summary>
        /// 2024.7.12 李焕彬
        /// 写入单个元件
        /// </summary>
        /// <param name="elem">元件</param>
        [RelayCommand]
        public void WriteSingle(CElement elem)
        {
            switch (elem.Type)
            {
                case EMELEMTYPE.EMELEMM:
                    Com.WriteSingleCoil(elem.Addr, elem.WriteValue == 1 ? true : false);
                    break;
                case EMELEMTYPE.EMELEMD:
                    Com.WriteSingleRegister(elem.Addr, elem.WriteValue);
                    break;
            }
        }

        /// <summary>
        /// 2024.7.17 l李焕彬
        /// 连接事件
        /// </summary>
        private void Connected(bool enable, string msg)
        {
            CCommunicationManagement.ComLogger.Info(msg);
            Growl.Info(msg);
        }
    }
}
