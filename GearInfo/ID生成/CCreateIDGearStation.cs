using System;
using WH.Entity.LogRecord;

namespace GearInfo
{
    /// <summary>
    /// 盘齿 ID 缓存轮询：读 ProductID 寄存器，>0 且变化才更新 CurrentProductID。
    /// </summary>
    public class CCreateIDGearStation : CGearCreateIDBase
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
                if (CGearCommunicate.com == null)
                {
                    return;
                }

                CGearProtocolPoints points = CGearCommunicate.Points;
                if (points == null || !points.TryGetPoint("ProductID", out GearPointDef def) || def == null)
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

                if (!CGearCommunicate.TryReadProductIdRegister((ushort)def.Address, out int productID))
                {
                    return;
                }
                UpdateCacheIfChanged(productID);
            }
            catch (Exception ex)
            {
                try
                {
                    CLogRec.Default.Warn("Gear 轮询读 ID 失败: " + ex.Message);
                }
                catch
                {
                }
            }
        }
    }
}