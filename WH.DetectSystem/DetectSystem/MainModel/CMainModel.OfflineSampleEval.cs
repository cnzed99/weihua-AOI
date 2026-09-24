using System;
using WH.DetectSystem;
using WH.DetectSystem.DetectSystem.MainModel;
using WH.RunCell;

namespace WH.DetectSystem.Models
{
    /// <summary>【方案7.1-注释】离线评测挂在制程上，跟当前打开工程走。</summary>
    public partial class CMainModel
    {
        OfflineSampleEval _offlineSampleEval = new OfflineSampleEval();

        public void BeginOfflineSampleEval()
        {
            if (!COpenProjectLine.UsesOfflineSampleEval)
            {
                return;
            }

            if (_offlineSampleEval == null)
            {
                _offlineSampleEval = new OfflineSampleEval();
            }

            _offlineSampleEval.Begin();
        }

        public void FinishOfflineSampleEval()
        {
            if (!COpenProjectLine.UsesOfflineSampleEval || _offlineSampleEval == null)
            {
                return;
            }

            string fullName = (ProcessGroup?.Name ?? string.Empty) + "-" + (Name ?? string.Empty);
            _offlineSampleEval.TryFinish(fullName, OperateLog, out _);
        }

        public void TryRecordOfflineSampleEval(Cell cell)
        {
            if (!COpenProjectLine.UsesOfflineSampleEval
                || _offlineSampleEval == null
                || !_offlineSampleEval.IsRunning
                || cell == null
                || cell.Skipthis)
            {
                return;
            }

            try
            {
                Cell evalCell = new Cell();
                evalCell.CancelSource = cell.CancelSource;
                evalCell.Skipthis = cell.Skipthis;
                evalCell.ImageFile = cell.ImageFile;
                if (cell.AlgorithmOut != null)
                {
                    evalCell.AlgorithmOut = cell.AlgorithmOut;
                }

                MaociFilterConfig.FilterExute(evalCell);
                _offlineSampleEval.Record(cell, evalCell);
            }
            catch (Exception ex)
            {
                SysLog.Warn("评测记账失败: " + ex.Message);
            }
        }
    }
}
