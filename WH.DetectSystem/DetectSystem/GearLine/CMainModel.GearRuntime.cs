using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using GearInfo;
using WH.DetectSystem.DetectSystem.MainModel;
using WH.RecipeCellRootBase;
using WH.RunCell;

namespace WH.DetectSystem.Models
{
    /// <summary>
    /// 盘齿制程运行时：取图绑 ID、上齿面转发、GearImages 合并、组结果回写。
    /// 与 MainVM 同为 CMainModel 分文件，以便访问制程私有字段。
    /// </summary>
    public partial class CMainModel
    {
        /// <summary>
        /// 【盘齿方案4】改动A：本制程上次绑定的 ProductID（取图线程维护，禁止静态全局）
        /// 【盘齿方案4-注释】string；空/null 表示尚未绑定。占位 ID 与拉链一样来自产量+1，字符串比较。
        /// </summary>
        private string _lastBoundProductId;

        /// <summary>
        /// 【盘齿方案4】改动A：本制程当前 ID 已收张数（本地 PhotoIndex 1..N）
        /// </summary>
        private int _photoCounter;

        /// <summary>
        /// 正式自动取图：绑 ID、过张丢弃、上齿面转发。返回 true 表示 cell 已处理完，取图线程 continue。
        /// </summary>
        private bool TryConsumeGearFormalCapture(Cell cell, ref bool IDisRight)
        {
            if (!TryBindFormalProductId(cell))
            {
                IDisRight = false;
                cell.Dispose();
                return true;
            }

            if (_photoCounter > cell.PhotoTatolCount && Name != "上齿面")
            {
                IDisRight = false;
                SysLog.Warn($"{Name}-过张丢弃：产品ID:{cell.ID},PhotoIndex:{_photoCounter}>PhotoTatolCount:{cell.PhotoTatolCount}");
                cell.Dispose();
                return true;
            }
            IDisRight = true;

            if (Name == "上齿面" && TryDispatchToothTopByIndex(cell, _photoCounter))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 离线/手动：上齿面按文件名 PhotoIndex 分流。k==1 时把张数改成制程配置，避免误等第二张。
        /// 返回 true 表示已转发或丢弃，取图线程 continue。
        /// </summary>
        private bool TryConsumeGearOfflineToothTop(Cell cell)
        {
            if (Name != "上齿面")
            {
                return false;
            }
            if (TryDispatchToothTopByIndex(cell, cell.PhotoIndex))
            {
                return true;
            }
            cell.PhotoTatolCount = this.PhotoTotalCount;
            return false;
        }

        /// <summary>
        /// 【盘齿方案4】改动B：上齿面第 k 张分流。k==1 返回 false 入本制程算法；k==2 转发上端面；k>=3 丢弃。
        /// 返回 true 表示本张已处理完，取图线程应 continue，禁止写入本制程 m_AlgorithmChannel。
        /// CloneExecptImg 不拷贝 Image（方法内 WriteTo 已注释），此处移交 Image 所有权后再 Dispose 原 cell。
        /// </summary>
        private bool TryDispatchToothTopByIndex(Cell cell, int k)
        {
            if (!COpenProjectLine.IsGear)
            {
                return false;
            }
            if (k <= 1)
            {
                return false;
            }
            if (k >= 3)
            {
                SysLog.Warn($"{Name}-工位1多余张丢弃：产品ID:{cell.ID},PhotoIndex:{k}>=3");
                cell.Dispose();
                return true;
            }

            CMainModel outerVm = ProcessGroup?.CMainModels?.FirstOrDefault(m => m.Name == "上端面");
            if (outerVm == null)
            {
                SysLog.Error($"{Name}-工位1转发失败：同组未找到制程「上端面」，丢弃 ID:{cell.ID}");
                cell.Dispose();
                return true;
            }

            Cell fwd = cell.CloneExecptImg();
            fwd.Image = cell.Image;
            cell.Image = null;
            fwd.ID = cell.ID;
            fwd.PhotoIndex = 1;
            fwd.PhotoTatolCount = 1;
            fwd.ProjName = "上端面";
            fwd.ProjGuid = outerVm.GUID;
            fwd.IsPreBound = true;
            if (!outerVm.m_WaitImgChannel.Writer.TryWrite(fwd))
            {
                SysLog.Error($"{Name}-工位1转发失败：上端面通道写入失败，丢弃 ID:{fwd.ID}");
                fwd.Dispose();
            }
            else
            {
                SysLog.Info($"{Name}-工位1转发第2张到上端面：ID:{fwd.ID},PhotoIndex:{fwd.PhotoIndex},IsPreBound:{fwd.IsPreBound}");
            }
            cell.Dispose();
            return true;
        }

        /// <summary>
        /// 【盘齿方案2】P2-3 策略B：正式路径只读 GetProductID 缓存绑 ID。com==null 或 ID&lt;=0 返回 false（调用方丢弃）。
        /// 禁止在取图线程 ReadHoldingRegister。成功则写 cell.ID/PhotoIndex/PhotoTatolCount 并维护换 ID 计数。
        /// </summary>
        private bool TryBindFormalProductId(Cell cell)
        {
            if (CGearCommunicate.com == null)
            {
                SysLog.Warn($"{Name}-无PLC，不绑ID，丢弃");
                return false;
            }
            int id = CGearCommunicate.GetProductID();
            if (id <= 0)
            {
                SysLog.Warn($"{Name}-产品ID无效:{id}，丢弃");
                return false;
            }
            string productID = id.ToString(CultureInfo.InvariantCulture);

            if (productID != _lastBoundProductId)
            {
                if (_photoCounter > 0 && _photoCounter < this.PhotoTotalCount)
                {
                    SysLog.Warn($"{Name}-残图告警：制程/{_lastBoundProductId}/已收{_photoCounter}/应收{this.PhotoTotalCount}");
                }
                _photoCounter = 0;
                _lastBoundProductId = productID;
            }

            _photoCounter++;
            cell.ID = productID;
            cell.PhotoIndex = _photoCounter;
            cell.PhotoTatolCount = this.PhotoTotalCount;
            return true;
        }

        private void AppendGearMergedStationImages(Cell newCell, List<Cell> currentCells)
        {
            for (int i = 0; i < currentCells.Count; i++)
            {
                newCell.GearImages.Add((currentCells[i].Image, currentCells[i].PhotoIndex,
                    currentCells[i].CreateTime, currentCells[i].RecipeTime));
            }
        }

        private void ApplyGearMergedDisplay(Cell newCell, List<CImage> img)
        {
            if (img == null || img.Count == 0)
            {
                return;
            }
            newCell.Image = img[0];
            newCell.MergedPanorama = img[0];
        }

        private List<CImage> CollectGearCImages(List<Cell> cells)
        {
            List<CImage> cImages = new List<CImage>();
            List<Cell> ordered = cells.OrderBy(c => c.PhotoIndex).ToList();
            CImage merged = GetMergeImage(ordered);
            if (merged != null)
            {
                cImages.Add(merged);
            }
            return cImages;
        }

        private void SendGearGroupResult(CCellPro CellOut)
        {
            CGearCommunicate.SendGroupResult(
                ProcessGroup.Name,
                CellOut.Cell.ID,
                CellOut.Cell.IsOK ? GearResult.OK : GearResult.NG);
        }
    }
}
