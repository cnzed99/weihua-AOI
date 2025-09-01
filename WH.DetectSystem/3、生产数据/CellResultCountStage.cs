using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
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
                DefectNumber currentDefect = produce[cell.Detection.DefectFilter.Name];
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

            //ObservableCollection<DefectNumber> defectNumbers =(ObservableCollection<DefectNumber>) produce.DefectNumbersList.Where(d => (d.Number != 0));
            //produce.DefectNumbersSortList = (ObservableCollection < DefectNumber > )defectNumbers.OrderByDescending(d => d.Number);

            //var viewSource = new CollectionViewSource { Source = produce.DefectNumbersList };
            //viewSource.SortDescriptions.Add(new SortDescription("Number", ListSortDirection.Descending));
            //var viewSource = new CollectionViewSource { Source = produce.DefectNumbersList };
            produce.SortedView = CollectionViewSource.GetDefaultView(produce.DefectNumbersList);

        }
    }
}
