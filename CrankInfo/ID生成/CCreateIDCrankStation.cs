using System;
using WH.Entity.LogRecord;

namespace CrankInfo
{
    /// <summary>
    /// 【曲轴方案2-注释】曲轴 ID 缓存轮询：读 ProductID 寄存器，>0 且变化才更新 CurrentProductID。
    /// </summary>
    public class CCreateIDCrankStation : CCrankCreateIDBase
    {
        protected override void PollOnce()
        {
            PollProductIDOnce();
        }

        /// <summary>
        /// 单次读取。com==null 或点位未配置则返回。读失败不更新缓存。
        /// </summary>
        public void PollProductIDOnce()
        {
            try
            {
                if (CCrankCommunicate.com == null)
                {
                    return;
                }

                CCrankProtocolPoints points = CCrankCommunicate.Points;
                if (points == null || !points.TryGetPoint("ProductID", out CrankPointDef def) || def == null)
                {
                    return;
                }
                if (!def.Enabled)
                {
                    return;
                }
                if (def.Address < 0 || def.Address > ushort.MaxValue)
                {
                    return;
                }

                if (!CCrankCommunicate.TryReadProductIdRegister((ushort)def.Address, out int productID))
                {
                    return;
                }
                UpdateCacheIfChanged(productID);
            }
            catch (Exception ex)
            {
                try
                {
                    CLogRec.Default.Warn("Crank 轮询读 ID 失败: " + ex.Message);
                }
                catch
                {
                }
            }
        }
    }
}
