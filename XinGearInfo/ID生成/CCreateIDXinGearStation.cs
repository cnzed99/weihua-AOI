using System;
using WH.Entity.LogRecord;

namespace XinGearInfo
{
    /// <summary>
    /// 【新兴盘齿方案2-注释】 ID 缓存轮询：读 ProductID 寄存器，>0 且变化才更新 CurrentProductID。
    /// </summary>
    public class CCreateIDXinGearStation : CXinGearCreateIDBase
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
                if (CXinGearCommunicate.com == null)
                {
                    return;
                }

                CXinGearProtocolPoints points = CXinGearCommunicate.Points;
                if (points == null || !points.TryGetPoint("ProductID", out XinGearPointDef def) || def == null)
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

                if (!CXinGearCommunicate.TryReadProductIdRegister((ushort)def.Address, out int productID))
                {
                    return;
                }
                UpdateCacheIfChanged(productID);
            }
            catch (Exception ex)
            {
                try
                {
                    CLogRec.Default.Warn("新兴盘齿 轮询读 ID 失败: " + ex.Message);
                }
                catch
                {
                }
            }
        }
    }
}
