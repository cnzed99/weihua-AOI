using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using CommunicationModule;
using HandyControl.Controls;
using LanguageManager;

namespace TCPIP
{
    /// <summary>
    /// 2024.7.21 李焕彬
    /// TCP通讯
    /// </summary>
    public class CTcpIpCommPart : CCommunicationBase
    {
        /// <summary>
        /// 2024.7.21 李焕彬
        /// TCP通讯参数
        /// </summary>
        public CTcpIpCommunicationSetting setting;

        /// <summary>
        /// 2024.7.21 李焕彬
        ///soket对象
        /// </summary>
        private Socket _socket;

        /// <summary>
        /// 2024.7.21 李焕彬
        ///远程对象
        /// </summary>
        private IPEndPoint Sever;

        /// <summary>
        /// 2024.7.21 李焕彬
        ///读取数据线程
        /// </summary>
        private Task taskRecv;

        /// <summary>
        /// 2024.7.21 李焕彬
        /// 连接后触发事件
        /// </summary>
        public override event Action<bool, string> ConnectedEventArgs;

        public CTcpIpCommPart(CTcpIpCommunicationSetting prama)
        {
            setting = prama;
        }

        /// <summary>
        /// 2024.7.21 李焕彬
        /// 发送数据
        /// </summary>
        public void SendData(byte[] data)
        {
            try
            {
                if (IsConnected)
                {
                    int len = data.Length;

                    int count = _socket.Send(data, 0, data.Length, SocketFlags.None);
                    if (count != data.Length)
                    {
                        CCommunicationManagement.ComLogger.Error("TCP发送数据不完整,数据丢失,发送数据数为:" + count);
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 2024.7.21 李焕彬
        /// 接收数据
        /// </summary>
        public void ReceiveData()
        {
            Thread.CurrentThread.Priority = ThreadPriority.Highest;
            while (IsConnected)
            {
                try
                {
                    byte[] data = new byte[1024];
                    int len = 0;
                    len = _socket.Receive(data, SocketFlags.None);
                    byte[] buff = new byte[len];
                    Array.Copy(data, buff, len);
                    base.AfterReceive(setting.Guid, buff);
                }
                catch (Exception ex)
                {
                    if (!_socket.Connected)
                    {
                        CCommunicationManagement.ComLogger.Error("TCP连接断开" + ex.Message);
                    }
                }
            }
        }

        /// <summary>
        /// 2024.7.21 李焕彬
        /// 连接
        /// </summary>
        public override void Connect()
        {
            try
            {
                if (setting.Enable) //是否启用
                {
                    CCommunicationManagement.ComLogger.Info(
                        "TCP连接线程开启,准备连接服务器:" + this.setting.RemoteIP
                    );
                    Close();
                    this._socket = new Socket(
                        AddressFamily.InterNetwork,
                        SocketType.Stream,
                        ProtocolType.Tcp
                    );
                    this.Sever = new IPEndPoint(
                        IPAddress.Parse(this.setting.RemoteIP),
                        Convert.ToInt32(this.setting.RemotePort)
                    );

                    IPEndPoint localEndPoint = new IPEndPoint(
                        IPAddress.Parse(this.setting.LocalIP),
                        Convert.ToInt32(this.setting.LocalPort)
                    );
                    this._socket.Bind(localEndPoint);

                    //设置KeepAlive
                    _socket.SetSocketOption(
                        SocketOptionLevel.Socket,
                        SocketOptionName.KeepAlive,
                        true
                    );
                    _socket.IOControl(IOControlCode.KeepAliveValues, GetKeepAliveData(), null);

                    this._socket.Connect(Sever);
                    this._socket.SetSocketOption(
                        SocketOptionLevel.Socket,
                        SocketOptionName.ReceiveTimeout,
                        500
                    );
                    IsConnected = true;
                    CCommunicationManagement.ComLogger.Info(
                        "TCP连接服务器:" + this.setting.RemoteIP + "成功!"
                    );
                    ConnectedEventArgs?.Invoke(IsConnected, "TCP连接状态:已连接");
                    taskRecv = Task.Factory.StartNew(ReceiveData);
                }
                else
                {
                    ConnectedEventArgs?.Invoke(false, "连接状态:本通讯未启用");
                }
            }
            catch (Exception ex)
            {
                CCommunicationManagement.ComLogger.Error(
                    "TCP连接服务器:" + this.setting.RemoteIP + "出现异常:" + ex.Message
                );
                Close();
                throw;
            }
        }

        /// <summary>
        /// 2024.7.21 李焕彬
        /// 关闭
        /// </summary>
        /// <returns>true关闭成功，false关闭失败</returns>
        public override bool Close()
        {
            try
            {
                if (_socket != null)
                {
                    IsConnected = false;
                    while (taskRecv != null && !taskRecv.IsCompleted)
                    {
                        Thread.Sleep(50);
                    }
                    this._socket.Close();
                    this._socket.Dispose();
                    this._socket = null;
                    taskRecv = null;
                    ConnectedEventArgs?.Invoke(IsConnected, "TCP连接状态:已断开连接");
                    CCommunicationManagement.ComLogger.Info("TCP关闭连接:" + this.setting.RemoteIP);
                }
                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 2024.7.21 李焕彬
        /// 设置KeepAlive
        /// </summary>
        /// <returns>KeepAliveData</returns>
        private byte[] GetKeepAliveData()
        {
            uint dummy = 0;
            byte[] inOptionValues = new byte[Marshal.SizeOf(dummy) * 3];
            BitConverter.GetBytes((uint)1).CopyTo(inOptionValues, 0);
            BitConverter.GetBytes((uint)3000).CopyTo(inOptionValues, Marshal.SizeOf(dummy)); //keep-alive间隔
            BitConverter.GetBytes((uint)500).CopyTo(inOptionValues, Marshal.SizeOf(dummy) * 2); // 尝试间隔
            return inOptionValues;
        }

        /// <summary>
        /// 2024.7.21 李焕彬
        /// 获取测试控件
        /// </summary>
        /// <returns>测试控件对象</returns>
        public override UserControl GetTestControl()
        {
            return new TestControl(this);
        }

        /// <summary>
        /// 2024.7.21 李焕彬
        /// 获取语言包
        /// </summary>
        /// <returns>测试控件对象</returns>
        public override CLanguageManager GetLanguage()
        {
            return CLang.s_Instance;
        }

        /// <summary>
        /// 2024.7.21 李焕彬
        /// 创建协议
        /// </summary>
        /// <returns>协议指令集</returns>
        public override IList CreateProtocol()
        {
            return new ObservableCollection<CDataInfo>();
        }

        /// <summary>
        /// 2024.7.19 李焕彬
        /// 通讯最大数据长度
        /// </summary>
        public static readonly int MaxbuffLength = 1024;

        /// <summary>
        /// 2024.7.21 李焕彬
        /// 发送数据
        /// </summary>
        /// <param name="list">协议指令集</param>
        public override void Send(IList list)
        {
            ObservableCollection<CDataInfo> elements = list as ObservableCollection<CDataInfo>;
            byte[] buff = new byte[MaxbuffLength];
            int maxLength = 0;
            foreach (var c in elements)
            {
                byte[] convert = c.GetBytes(c.DataValue);
                Array.Copy(convert, 0, buff, c.DataIndex, convert.Length);
                maxLength = Math.Max(maxLength, c.DataIndex + c.DataLength);
            }
            byte[] buff2 = new byte[maxLength + 1];
            Array.Copy(buff, buff2, maxLength + 1);
            SendData(buff2);
        }
    }
}
