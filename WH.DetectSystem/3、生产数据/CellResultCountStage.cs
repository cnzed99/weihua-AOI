using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mysqlx;
using ProjProduceData;
using WH.Entity;
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
            
            if (AppConfig.DefectTotalOnlyConfig())
            {
                int defectCounttotal = 0;
                if (cell.Detection != null)
                {
                    foreach (var algorithmOut in cell.AlgorithmOut)
                    {
                        defectCounttotal += algorithmOut.regionOut.Count;
                    }
                    produce.Ng += defectCounttotal;
                    // produce.Ng += 1;
                    var currentDefect = produce[cell.Detection.DefectFilter.Name];
                    // currentDefect.Number += 1;
                    currentDefect.Number += defectCounttotal;
                    //cell.Detection.DefectFilter.Number += 1;
                    foreach (var defect in produce.DefectNumbersList)
                    {
                        defect.Percent = (double)defect.Number / produce.Ng;
                    }
                }
                else
                {
                    produce.OK += 1;
                }
                produce.QualityNumbersList.First(o => o.Name == cell.Quality.Name).Number += defectCounttotal;
                //cell.Quality.Number += 1;
                // produce.Total += 1;
                produce.Total += defectCounttotal;
                foreach (var defect in produce.DefectNumbersList)
                {
                    defect.PercentofAll = (double)defect.Number / produce.Total;
                }
            }
            else
            {
                if (cell.Detection != null)
                {
                    produce.Ng += 1;
                    var currentDefect = produce[cell.Detection.DefectFilter.Name];
                    currentDefect.Number += 1;
                    //cell.Detection.DefectFilter.Number += 1;
                    foreach (var defect in produce.DefectNumbersList)
                    {
                        defect.Percent = (double)defect.Number / produce.Ng;
                    }
                }
                else
                {
                    produce.OK += 1;
                }
                produce.QualityNumbersList.First(o => o.Name == cell.Quality.Name).Number += 1;
                //cell.Quality.Number += 1;
                produce.Total += 1;
                foreach (var defect in produce.DefectNumbersList)
                {
                    defect.PercentofAll = (double)defect.Number / produce.Total;
                }
            }

          
        }
    }
}
