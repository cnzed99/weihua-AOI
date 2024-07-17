//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Channels;
//using System.Threading.Tasks;

//namespace CommunicationModule
//{
//    public abstract class CCommunicationBase
//    {
//        private static readonly BoundedChannelOptions channelOptions = new BoundedChannelOptions(
//            1000
//        )
//        {
//            FullMode = BoundedChannelFullMode.Wait
//        };

//        /// <summary>
//        /// 通讯消息队列
//        /// </summary>
//        public static Channel<(string Cam, string Com, byte[] data)> DataChannel =
//            Channel.CreateBounded<(string Cam, string Com, byte[] data)>(channelOptions);

//        /// <summary>
//        /// 检测结果存储（相机名，流水号，质量等级，颜色等级，其他发送信息）
//        /// </summary>
//        public List<(
//            string cam,
//            string waferID,
//            int qualityLevel,
//            int colorLevel,
//            Dictionary<string, string> otherSendInfo
//        )> _lsdefectResult =
//            new List<(
//                string cam,
//                string waferID,
//                int qualityLevel,
//                int colorLevel,
//                Dictionary<string, string> otherSendInfo
//            )>();

//        /// <summary>
//        /// 数据传出委托
//        /// </summary>
//        public Action<byte[]> ReceivedEvent;

//        /// <summary>
//        /// 连接后触发事件
//        /// </summary>
//        public abstract event Action<bool, string> ConnectedEventArgs;

//        /// <summary>
//        /// 是否已连接
//        /// </summary>
//        public bool IsConnected { get; set; }

//        /// <summary>
//        /// 发送数据
//        /// </summary>
//        public abstract void SendData(byte[] data);

//        /// <summary>
//        /// 接收数据
//        /// </summary>
//        public abstract void ReceiveData();

//        /// <summary>
//        /// 连接
//        /// </summary>
//        public abstract void Connect();

//        /// <summary>
//        /// 关闭
//        /// </summary>
//        /// <returns></returns>
//        public abstract bool Close();

//        public void AfterReceive(string Guid, byte[] buff)
//        {
//            if (!SystemStatic._deviceSeting) //自动运行状态下
//            {
//                CCommunicationManagement
//                    .GetTriggerCam(Guid, buff)
//                    .ForEach(c => DataChannel.Writer.WriteAsync((c, Guid, buff)));
//            }
//            else
//            {
//                ReceivedEvent?.Invoke(buff);
//            }
//        }
//    }
//}
