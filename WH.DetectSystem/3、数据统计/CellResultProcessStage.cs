using ProjProduceData;
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
    /// cell检测结果 汇报 统计 存储
    /// </summary>
    public static class CellResultProcessStage
    {
        public static void CellResultExcute(this DefectsProduce produce, Cell cell)
        {
            produce.AddDefectProduce(cell);
        }
    }
}
