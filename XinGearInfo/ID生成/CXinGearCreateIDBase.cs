using System.Threading;

namespace XinGearInfo
{
    /// <summary>
    /// 【新兴盘齿方案2-注释】 ID 缓存轮询基类。1ms 后台线程。无 IDSendEvent。
    /// </summary>
    public class CXinGearCreateIDBase
    {
        Thread WaitIDThread = null;

        public bool Connend = false;
        public int TempproductID = 0;

        /// <summary>
        /// 【新兴盘齿方案启动 1ms 后台轮询线程。
        /// </summary>
        public void IntThread()
        {
            if (WaitIDThread != null && WaitIDThread.IsAlive)
            {
                return;
            }
            Connend = true;
            WaitIDThread = new Thread(MonitoringID);
            WaitIDThread.IsBackground = true;
            WaitIDThread.Start();
        }

        /// <summary>
        /// 【新兴盘齿方案停轮询：Connend=false，后台线程自行退出。
        /// </summary>
        public void StopThread()
        {
            Connend = false;
        }

        /// <summary>
        /// 轮询入口。基类 1ms 循环；子类覆盖 PollOnce。
        /// </summary>
        public virtual void MonitoringID()
        {
            while (Connend)
            {
                try
                {
                    PollOnce();
                }
                catch
                {
                }
                Thread.Sleep(1);
            }
        }

        /// <summary>
        /// 单次轮询。子类覆盖。
        /// </summary>
        protected virtual void PollOnce()
        {
        }

        /// <summary>
        /// 仅当新 ProductID 且大于 0 才更新静态缓存。≤0 无效，不更新。
        /// </summary>
        protected virtual void UpdateCacheIfChanged(int productID)
        {
            if (productID <= 0)
            {
                return;
            }
            if (productID != TempproductID)
            {
                TempproductID = productID;
                CXinGearCommunicate.CurrentProductID = productID;
            }
        }
    }
}
