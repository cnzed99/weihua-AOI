using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using LanguageManager;

namespace CommunicationModule
{
    /// <summary>
    /// 2024.7.17 李焕彬
    /// 通讯抽象基类
    /// </summary>
    public abstract partial class CCommunicationBase : ObservableObject
    {
        /// <summary>
        /// 2024.9.29 李焕彬
        /// 测试控件
        /// </summary>
        [ObservableProperty]
        UserControl testControl;

        /// <summary>
        /// 2024.7.17 李焕彬
        /// 数据传出委托
        /// </summary>
        public Action<byte[]> ReceivedEvent;

        /// <summary>
        /// 2024.7.17 李焕彬
        /// 连接后触发事件
        /// </summary>
        public abstract event Action<bool, string> ConnectedEventArgs;

        /// <summary>
        /// 2024.7.17 李焕彬
        /// 是否已连接
        /// </summary>
        [ObservableProperty]
        private bool isConnected;

        /// <summary>
        /// 2024.7.21 李焕彬
        /// 发送数据
        /// </summary>
        /// <param name="list">协议指令集</param>
        public abstract void Send(IList list);

        /// <summary>
        /// 2024.7.17 李焕彬
        /// 连接
        /// </summary>
        public abstract void Connect();

        /// <summary>
        /// 2024.7.17 李焕彬
        /// 关闭
        /// </summary>
        /// <returns></returns>
        public abstract bool Close();

        /// <summary>
        /// 2024.7.19 李焕彬
        /// 接收事件处理
        /// </summary>
        /// <param name="Guid">guid</param>
        /// <param name="buff">数据</param>
        public void AfterReceive(string Guid, byte[] buff)
        {
            ReceivedEvent?.Invoke(buff);
        }

        /// <summary>
        /// 2024.7.21 李焕彬
        /// 获取语言包
        /// </summary>
        /// <returns>测试控件对象</returns>
        public abstract CLanguageManager GetLanguage();

        /// <summary>
        /// 2024.7.21 李焕彬
        /// 创建协议
        /// </summary>
        /// <returns>协议</returns>
        public abstract IList CreateProtocol();
    }
}
