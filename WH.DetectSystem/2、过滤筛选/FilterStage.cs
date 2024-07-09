using SDFilter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WH.RunCell;

namespace WH.DetectSystem
{
    /// <summary>
    /// 20240704 TCG
    /// 过滤阶段执行
    /// </summary>
    public static class CFilterStage
    {
        /// <summary>
        /// 20240704 TCG
        /// 对cell 中的缺陷进行过滤 得到最终的定级缺陷你，写入Cell中
        /// </summary>
        /// <param name="filter">过滤参数</param>
        /// <param name="cell">检测对象</param>
        public static void FilterExute(this CFilterConfig filter,Cell cell)
        {
            filter.Excute(cell);
        }
    }
}
