using System.Collections.Generic;
using System.Linq;
using WH.RecipeCellRootBase;
using WH.RunCell;
using ZipperInfo;

namespace WH.DetectSystem.Models
{
    /// <summary>
    /// 拉链制程运行时：取图绑 ID、算法前灌参、ZipperImages 高低曝光合并、组结果回写。
    /// 与 MainVM 同为 CMainModel 分文件，以便访问制程私有字段。
    /// </summary>
    public partial class CMainModel
    {
        /// <summary>
        /// 【盘齿方案11-注释】拉链取图绑 ID。返回 true 表示 cell 已丢弃，调用方 continue。
        /// </summary>
        private bool TryConsumeZipperCaptureReject(Cell cell, ref bool IDisRight)
        {
            int productID = -1;
            if (Name == "正面" || Name == "反面")
            {
                CZipperCommunicate.GetID(out productID);
                m_WaitIDChannel.Reader.TryRead(out ZipperID zipperID);
                if (zipperID.ProductID > 0)
                {
                    bool bnext = zipperID.ProductID < productID;
                    while (bnext && zipperID.ProductID > 0)
                    {
                        m_WaitIDChannel.Reader.TryRead(out zipperID);
                        bnext = zipperID.ProductID < productID;
                        if (bnext)
                        {
                            SysLog.Info($"{Name}-变化的产品ID:{zipperID.ProductID}小于当前{productID}，抛弃{zipperID.ProductID}-{zipperID.PhotoID}");
                            continue;
                        }
                    }
                    cell.ID = zipperID.ProductID.ToString();
                    cell.PhotoIndex = zipperID.PhotoID;
                    IDisRight = true;
                    SysLog.Info($"{Name}-接收到产品ID:{zipperID.ProductID},图片ID:{zipperID.PhotoID}");
                    return false;
                }
                IDisRight = false;
                SysLog.Info($"{Name}-接收到产品ID:{zipperID.ProductID},抛弃");
                cell.Dispose();
                return true;
            }
            if (Name == "上止")
            {
                IDisRight = true;
                cell.ID = (ProcessGroup.MaociDefectsProduce.Total + 1).ToString();
                cell.PhotoIndex = 1;
                cell.PhotoTatolCount = 1;
                return false;
            }
            CZipperCommunicate.GetID2(out productID);
            if (productID != -1)
            {
                IDisRight = true;
                cell.ID = productID.ToString();
                cell.PhotoIndex = 1;
                cell.PhotoTatolCount = 1;
            }
            return false;
        }

        /// <summary>
        /// 【盘齿方案11-注释】拉链算法前灌参。copyGeometryAndCount 仅正式自动取图。
        /// </summary>
        private void CopyZipperInfoOntoCell(Cell cell, bool copyGeometryAndCount)
        {
            var info = CZipperAutomaticAlgorithm.Instance.ZipperInfo;
            if (copyGeometryAndCount)
            {
                cell.ZipperPullerCX = info.TempData1.ZipperPullerCX;
                cell.ZipperPullerCY = info.TempData1.ZipperPullerCY;
                cell.PullOrgContours = info.TempData1.OrgContours;
                cell.PullHoldOrgContours = info.TempData1.HoleOrgContours;
                cell.PullsOrgHvalue = info.TempData1.PullsMeanH;
                cell.PullsOrgSvalue = info.TempData1.PullsMeanS;
                cell.PullsOrgVvalue = info.TempData1.PullsMeanV;
                cell.PullerOrgHvalue = info.TempData1.PullerMeanH;
                cell.PullerOrgSvalue = info.TempData1.PullerMeanS;
                cell.PullerOrgVvalue = info.TempData1.PullerMeanV;
                cell.ModelID_Pull = info.TempData1.ModelID_Pull;
                cell.ModelID_Logo = info.TempData1.ModelID_Logo;
                cell.PullModelRow = info.TempData1.PullModelRow;
                cell.PullModelCol = info.TempData1.PullModelCol;
                cell.BackRectangle = info.TempData1.BackRectangle;
                cell.PullSegOrgArea = info.TempData1.PullSegOrgArea;
                cell.UpMass_1_MeanH = info.TempData1.UpMass_1_MeanH;
                cell.UpMass_1_MeanS = info.TempData1.UpMass_1_MeanS;
                cell.UpMass_1_MeanV = info.TempData1.UpMass_1_MeanV;
                cell.UpMass_2_MeanH = info.TempData1.UpMass_2_MeanH;
                cell.UpMass_2_MeanS = info.TempData1.UpMass_2_MeanS;
                cell.UpMass_2_MeanV = info.TempData1.UpMass_2_MeanV;
                int photoTotalCount = 0;
                if (Name != "正面" && Name != "反面")
                {
                    photoTotalCount = 1;
                }
                else
                {
                    photoTotalCount = info.TempData1.ZipperImagesCount * 2;
                }
                cell.PhotoTatolCount = photoTotalCount;
            }
            cell.PullMaterlsType = info.PullMaterlsType.ToString();
            cell.DownStopMassType = info.ZipperDownMassType.ToString();
            cell.UpStopMassType = info.ZipperUpMassType.ToString();
            cell.BoltDiretion = info.TempData1.AutoData?.BoltDiretion.ToString();
            cell.ZipperLogoType = info.ZipperLogoType;
        }

        /// <summary>
        /// 【盘齿方案11-注释】拉链组齐套后按制程名回写 SendResult/2/3。
        /// </summary>
        private void SendZipperProcessResult(CCellPro CellOut)
        {
            if (CellOut.Cell.IsOK && CellOut.Cell.ID != "0")
            {
                if (Name == "正面" || Name == "反面")
                {
                    CZipperCommunicate.SendResult(CellOut.Cell.ID, ZIPPERESULT.OK);
                }
                else if (Name == "上止")
                {
                    CZipperCommunicate.SendResult3(CellOut.Cell.ID, ZIPPERESULT.OK);
                }
                else
                {
                    CZipperCommunicate.SendResult2(CellOut.Cell.ID, ZIPPERESULT.OK);
                }
            }
            else
            {
                if (Name == "正面" || Name == "反面")
                {
                    if (CellOut.Cell.Detection.DefectFilter.Name.Contains("大接头") || CellOut.Cell.Detection.DefectFilter.Name.Contains("大破损")
                     || CellOut.Cell.Detection.DefectFilter.Name.Contains("大起毛"))
                    {
                        CZipperCommunicate.SendResult(CellOut.Cell.ID, ZIPPERESULT.NG);
                    }
                    else
                    {
                        CZipperCommunicate.SendResult(CellOut.Cell.ID, ZIPPERESULT.NG2);
                    }
                }
                else if (Name == "上止")
                {
                    CZipperCommunicate.SendResult3(CellOut.Cell.ID, ZIPPERESULT.NG);
                }
                else
                {
                    CZipperCommunicate.SendResult2(CellOut.Cell.ID, ZIPPERESULT.NG);
                }
            }
        }

        private void CopyZipperMergeSideFields(Cell newCell, Cell src)
        {
            if (src.ZipperPullPartImg != null)
            {
                newCell.ZipperPullPartImg = src.ZipperPullPartImg;
            }
            if (src.UpMassMatImg != null && src.UpMassMatImg.Count > 0)
            {
                for (int j = 0; j < src.UpMassMatImg.Count; j++)
                {
                    newCell.UpMassMatImg.Add(src.UpMassMatImg[j]);
                }
            }
            if (src.FourCutMatImg != null && src.FourCutMatImg.Count > 0)
            {
                for (int j = 0; j < src.FourCutMatImg.Count; j++)
                {
                    newCell.FourCutMatImg.Add(src.FourCutMatImg[j]);
                }
            }
            if (src.DownMassMatImg != null)
            {
                newCell.DownMassMatImg = src.DownMassMatImg;
            }
        }

        private void AppendZipperMergedStationImages(Cell newCell, List<Cell> currentCells)
        {
            for (int i = 0; i < currentCells.Count; i++)
            {
                newCell.ZipperImages.Add((currentCells[i].Image, currentCells[i].PhotoIndex,
                    currentCells[i].CreateTime, currentCells[i].RecipeTime));
            }
        }

        private void ApplyZipperMergedDisplay(Cell newCell, List<CImage> img)
        {
            if (img == null || img.Count == 0)
            {
                return;
            }
            if (img.Count == 1)
            {
                newCell.Image = img[0];
            }
            else
            {
                newCell.Image = img[0];
                newCell.ChangleImgae = img[1];
            }
        }

        private List<CImage> CollectZipperCImages(List<Cell> cells)
        {
            List<CImage> cImages = new List<CImage>();
            List<Cell> lowIndexGroup = cells.Where(c => c.PhotoIndex >= 100).ToList();
            List<Cell> highIndexGroup = cells.Where(c => c.PhotoIndex < 100).ToList();
            lowIndexGroup.Sort((a, b) => a.PhotoIndex.CompareTo(b.PhotoIndex));
            highIndexGroup.Sort((a, b) => a.PhotoIndex.CompareTo(b.PhotoIndex));
            if (lowIndexGroup.Count > 0)
            {
                CImage lowimage = GetMergeImage(lowIndexGroup);
                cImages.Add(lowimage);
            }
            if (highIndexGroup.Count > 0)
            {
                CImage heightimage = GetMergeImage(highIndexGroup);
                cImages.Add(heightimage);
            }
            return cImages;
        }
    }
}
