using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mysqlx;
using ProjProduceData;
using WH.RunCell;

namespace WH.DetectSystem
{
    /// <summary>
    /// 20240704 TCG
    /// cell检测结果 汇报 统计 存储
    /// </summary>
    public static class CellResultCountStage
    {
        public static void Excute(this CDefectsProduce produce, Cell cell)
        {
            if (cell.Detection != null)
            {
                produce.Ng += 1;
                //var currentDefect = this[cell.Detection.Name];
                //currentDefect.Number += 1;
                cell.Detection.DefectFilter.Number += 1;
                foreach (var defect in produce.DefectNumbersList)
                {
                    defect.Percent = (double)defect.Number / produce.Ng;
                }
            }
            else
            {
                produce.OK += 1;
            }
            cell.Quality.Number += 1;
            produce.Total += 1;
            foreach (var defect in produce.DefectNumbersList)
            {
                defect.PercentofAll = (double)defect.Number / produce.Total;
            }
        }
    }
}
