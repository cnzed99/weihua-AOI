using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Media;
using XinGearInfo;
using WH.Controls;
using WH.DetectSystem.DetectSystem.MainModel;
using WH.RecipeCellRootBase;
using WH.RunCell;

namespace WH.DetectSystem.Models
{
    /// <summary>
    /// 【新兴盘齿方案4-注释】 新兴制程运行时：取图绑 ID、XinGearImages 分张、侧面 2x3、组结果回写。
    /// 与 MainVM 同为 CMainModel 部分类；复用盘齿私有字段 _lastBoundProductId / _photoCounter。
    /// 禁止写入 GearImages；无上齿面转发。
    /// </summary>
    public partial class CMainModel
    {
        private Cell _lastXinGearShotCell;
        private ObservableCollection<XinGearShotTileVM> _shotTiles;

        /// <summary>
        /// 【新兴盘齿方案0.6-注释】页2 侧面分格（张数=本制程 PhotoTotalCount）。不进 .burrproj。
        /// </summary>
        public ObservableCollection<XinGearShotTileVM> ShotTiles
        {
            get
            {
                if (_shotTiles == null || _shotTiles.Count != this.PhotoTotalCount)
                {
                    RebuildXinGearShotTiles();
                }
                return _shotTiles;
            }
        }

        /// <summary>
        /// 【新兴盘齿方案0.6-注释】格数跟随 PhotoTotalCount。已绑定则 Clear 重填，避免同会话改 N 仍只有侧面1。
        /// </summary>
        void RebuildXinGearShotTiles()
        {
            if (_shotTiles == null)
            {
                _shotTiles = new ObservableCollection<XinGearShotTileVM>();
            }
            else
            {
                _shotTiles.Clear();
            }
            for (int i = 1; i <= this.PhotoTotalCount; i++)
            {
                XinGearShotTileVM tile = new XinGearShotTileVM(i);
                tile.WhenViewReady = vm =>
                {
                    if (_lastXinGearShotCell != null)
                    {
                        DrawXinGearShotTile(vm, _lastXinGearShotCell);
                    }
                };
                _shotTiles.Add(tile);
            }
        }

        void SyncXinGearShotTilesAfterPhotoCountChanged()
        {
            if (_shotTiles == null)
            {
                return;
            }
            RebuildXinGearShotTiles();
        }

        /// <summary>
        /// 正式自动取图：绑缓存 ProductID、本制程张号 1..N。无 PLC / ID&lt;=0 仍放行（离线可点开始），件号用 0。
        /// 返回 true 表示 cell 已处理完，取图线程 continue。
        /// </summary>
        private bool TryConsumeXinGearFormalCapture(Cell cell, ref bool IDisRight)
        {
            int id = CXinGearCommunicate.GetProductID();
            string productID = id > 0 ? id.ToString(CultureInfo.InvariantCulture) : "0";

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

            if (_photoCounter > cell.PhotoTatolCount)
            {
                IDisRight = false;
                SysLog.Warn($"{Name}-张号过多丢弃：产品ID:{cell.ID},PhotoIndex:{_photoCounter}>PhotoTatolCount:{cell.PhotoTatolCount}");
                cell.Dispose();
                return true;
            }

            IDisRight = true;
            return false;
        }

        private void ApplyXinGearMergedDisplay(Cell newCell, List<CImage> img)
        {
            if (img == null || img.Count == 0)
            {
                return;
            }
            newCell.Image = img[0];
            newCell.MergedPanorama = img[0];
        }

        private List<CImage> CollectXinGearCImages(List<Cell> cells)
        {
            List<CImage> cImages = new List<CImage>();
            List<Cell> ordered = cells.OrderBy(c => c.PhotoIndex).ToList();
            CImage merged = GetMergeImage2x2(ordered);
            if (merged != null)
            {
                cImages.Add(merged);
            }
            return cImages;
        }

        /// <summary>
        /// 【新兴盘齿方案0.6-注释】侧面 6 张按 PhotoIndex 1..6 拼 2 列 3 行。缺张留黑。
        /// </summary>
        private unsafe CImage GetMergeImage2x2(List<Cell> cells)
        {
            if (cells == null || cells.Count == 0 || cells[0].Image == null)
            {
                return null;
            }
            if (cells.Count == 1)
            {
                return (CImage)cells[0].Image.Clone();
            }

            int tileW = cells[0].Image.ImageWidth;
            int tileH = cells[0].Image.ImageHeight;
            int bytesPerPixel = cells[0].Image.PixelFormat.BitsPerPixel / 8;
            if (tileW <= 0 || tileH <= 0 || bytesPerPixel <= 0)
            {
                return null;
            }

            int cols = 2;
            int rows = (this.PhotoTotalCount + cols - 1) / cols;
            int dstW = tileW * cols;
            int dstH = tileH * rows;
            int dstStride = dstW * bytesPerPixel;
            int dstSize = dstStride * dstH;
            IntPtr dstPtr = Marshal.AllocHGlobal(dstSize);
            try
            {
                new Span<byte>((void*)dstPtr, dstSize).Clear();
                for (int photo = 1; photo <= this.PhotoTotalCount; photo++)
                {
                    Cell srcCell = null;
                    for (int i = 0; i < cells.Count; i++)
                    {
                        if (cells[i].PhotoIndex == photo)
                        {
                            srcCell = cells[i];
                            break;
                        }
                    }
                    if (srcCell == null || srcCell.Image == null || srcCell.Image.ImageData == IntPtr.Zero)
                    {
                        continue;
                    }

                    int copyW = Math.Min(tileW, srcCell.Image.ImageWidth);
                    int copyH = Math.Min(tileH, srcCell.Image.ImageHeight);
                    if (copyW <= 0 || copyH <= 0)
                    {
                        continue;
                    }

                    int idx = photo - 1;
                    int col = idx % cols;
                    int row = idx / cols;
                    int dstX = col * tileW;
                    int dstY = row * tileH;
                    int srcStride = GetImageStride(srcCell.Image.ImageWidth, bytesPerPixel);
                    byte* srcBuffer = (byte*)srcCell.Image.ImageData.ToPointer();
                    byte* dstBuffer = (byte*)dstPtr.ToPointer();

                    for (int y = 0; y < copyH; y++)
                    {
                        Buffer.MemoryCopy(
                            srcBuffer + y * srcStride,
                            dstBuffer + (dstY + y) * dstStride + dstX * bytesPerPixel,
                            copyW * bytesPerPixel,
                            copyW * bytesPerPixel);
                    }
                }

                return new CImage(dstW, dstH, dstPtr, cells[0].Image.PixelFormat);
            }
            catch
            {
                Marshal.FreeHGlobal(dstPtr);
                throw;
            }
        }


        /// <summary>
        /// 【新兴盘齿方案0.6-注释】合并前深拷贝分张（图+时间+图内框）。Clone() 的 regionOut.points 仍是同一 List，offset 会污染分张。
        /// </summary>
        private List<(CImage img, int photoIndex, DateTime t, TimeSpan cost)> CopyXinGearImagesFromCells(List<Cell> cells)
        {
            List<(CImage img, int photoIndex, DateTime t, TimeSpan cost)> tiles = new List<(CImage img, int photoIndex, DateTime t, TimeSpan cost)>();
            if (cells == null)
            {
                return tiles;
            }
            foreach (Cell src in cells.OrderBy(c => c.PhotoIndex))
            {
                tiles.Add((src.Image, src.PhotoIndex, src.CreateTime, src.RecipeTime));
            }
            return tiles;
        }

        private static SRegion OffsetSRegion(SRegion src, double dx, double dy)
        {
            List<System.Windows.Point> pts = new List<System.Windows.Point>();
            if (src.points != null)
            {
                foreach (System.Windows.Point p in src.points)
                {
                    pts.Add(new System.Windows.Point(p.X + dx, p.Y + dy));
                }
            }
            return new SRegion(src.regionInfo, pts);
        }

        /// <summary>
        /// 【新兴盘齿方案0.6-注释】仅新兴 N>1：按 PhotoIndex 把图内框平移到 2 列 3 行。不改拉链/盘齿横拼。
        /// </summary>
        private void OffsetXinGearCellDetectionsTo2x2(List<Cell> cells)
        {
            if (cells == null)
            {
                return;
            }
            foreach (Cell src in cells)
            {
                int idx = src.PhotoIndex - 1;
                if (idx < 0)
                {
                    idx = 0;
                }
                int col = idx % 2;
                int row = idx / 2;
                int w = src.Image?.ImageWidth ?? 0;
                int h = src.Image?.ImageHeight ?? 0;
                double dx = col * w;
                double dy = row * h;
                if ((dx == 0 && dy == 0) || src.AlgorithmOut == null)
                {
                    continue;
                }
                foreach (CellDetection det in src.AlgorithmOut)
                {
                    if (det.regionOut == null)
                    {
                        continue;
                    }
                    List<SRegion> regions = new List<SRegion>();
                    foreach (SRegion region in det.regionOut)
                    {
                        regions.Add(OffsetSRegion(region, dx, dy));
                    }
                    det.regionOut = regions;
                }
            }
        }

        /// <summary>
        /// 【新兴盘齿方案0.6-注释】页2 六格：原图 + 图内框。CurView 晚到时用 _lastXinGearShotCell 补画。
        /// </summary>
        public void RefreshXinGearShotTiles(Cell cell)
        {
            _lastXinGearShotCell = cell;
            foreach (XinGearShotTileVM tileVm in ShotTiles)
            {
                DrawXinGearShotTile(tileVm, cell);
            }
        }

        private void DrawXinGearShotTile(XinGearShotTileVM tileVm, Cell cell)
        {
            if (tileVm == null)
            {
                return;
            }
            CImage tileImg = null;
            if (cell?.XinGearImages != null)
            {
                foreach ((CImage img, int photoIndex, DateTime t, TimeSpan cost) item in cell.XinGearImages)
                {
                    if (item.photoIndex == tileVm.PhotoIndex)
                    {
                        tileImg = item.img;
                        break;
                    }
                }
            }
            tileVm.ModelImage = tileImg?.ToBitmapSource();
            if (tileVm.CurView == null)
            {
                return;
            }
            tileVm.CurView.Clear(false);

            int w = tileImg != null ? tileImg.ImageWidth : 0;
            int h = tileImg != null ? tileImg.ImageHeight : 0;
            int idx = tileVm.PhotoIndex - 1;
            if (idx < 0)
            {
                idx = 0;
            }
            double dx = (idx % 2) * w;
            double dy = (idx / 2) * h;

            string cornerName = null;
            if (cell?.AlgorithmOut != null && w > 0 && h > 0)
            {
                foreach (CellDetection det in cell.AlgorithmOut)
                {
                    if (det?.regionOut == null)
                    {
                        continue;
                    }
                    foreach (SRegion region in det.regionOut)
                    {
                        if (XinGearRegionInMosaicTile(region, dx, dy, w, h))
                        {
                            cornerName = det.RecipeDefectName;
                            break;
                        }
                    }
                    if (cornerName != null)
                    {
                        break;
                    }
                }
            }
            tileVm.CurView.SetFontSize(25);
            tileVm.CurView.SetFontWeight(System.Windows.FontWeights.Bold);
            if (!string.IsNullOrEmpty(cornerName) && cell?.Quality != null)
            {
                tileVm.CurView.SetFontBrush(cell.Quality.ShowColor.Brush);
                tileVm.CurView.WinDrawText(cell.Quality.Name + ":" + cornerName, AlignmentX.Right, AlignmentY.Top, false);
            }
            else
            {
                tileVm.CurView.SetFontBrush(MaociQualityConfig?.GetBest()?.ShowColor.Brush ?? Brushes.Lime);
                tileVm.CurView.WinDrawText("OK", AlignmentX.Right, AlignmentY.Top, false);
            }

            if (cell?.AlgorithmOut != null && w > 0 && h > 0)
            {
                tileVm.CurView.SetPen(Brushes.Red);
                tileVm.CurView.SetFontBrush(Brushes.Red);
                tileVm.CurView.SetFontSize(15);
                tileVm.CurView.SetFontWeight(System.Windows.FontWeights.Normal);
                foreach (CellDetection det in cell.AlgorithmOut)
                {
                    if (det?.regionOut == null)
                    {
                        continue;
                    }
                    for (int i = 0; i < det.regionOut.Count; i++)
                    {
                        if (!XinGearRegionInMosaicTile(det.regionOut[i], dx, dy, w, h))
                        {
                            continue;
                        }
                        SRegion local = OffsetSRegion(det.regionOut[i], -dx, -dy);
                        tileVm.CurView.ImgDrawRegion(local.points, false);
                        string name = det.RecipeDefectName;
                        if (!string.IsNullOrEmpty(name))
                        {
                            tileVm.CurView.ImgDrawText(name, local.GetBottomRight(), false);
                        }
                    }
                }
            }
            tileVm.CurView.Invalidate();
        }

        private static bool XinGearRegionInMosaicTile(SRegion region, double dx, double dy, int w, int h)
        {
            if (region.points == null || w <= 0 || h <= 0)
            {
                return false;
            }
            foreach (System.Windows.Point p in region.points)
            {
                if (p.X >= dx && p.X < dx + w && p.Y >= dy && p.Y < dy + h)
                {
                    return true;
                }
            }
            return false;
        }

        private void SendXinGearGroupResult(CCellPro CellOut)
        {
            CXinGearCommunicate.SendGroupResult(
                ProcessGroup.Name,
                CellOut.Cell.ID,
                CellOut.Cell.IsOK ? XinGearResult.OK : XinGearResult.NG);
        }
    }
}
