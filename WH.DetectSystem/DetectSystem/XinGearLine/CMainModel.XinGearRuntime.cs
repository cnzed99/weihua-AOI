using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using XinGearInfo;
using WH.Controls;
using WH.DetectSystem.DetectSystem.MainModel;
using WH.DetectSystem.ViewModels;
using SDFilter;
using WH.RecipeCellRootBase;
using WH.RunCell;

namespace WH.DetectSystem.Models
{
    /// <summary>
    /// 新兴制程运行时：取图绑 ID、XinGearImages 分张、侧面按型号分格及拼图、组结果回写。
    /// 与 MainVM 同为 CMainModel 部分类；复用盘齿私有字段 _lastBoundProductId / _photoCounter。
    /// </summary>
    public partial class CMainModel
    {
        private Cell _xinGearFilterPreviewCell;
        private ObservableCollection<XinGearShotTileVM> _shotTiles;
        private readonly object _xinGearPhotoCountLock = new object();
        private int _activeXinGearPhotoCount;
        private int _pendingXinGearPhotoCount;
        private int _switchAfterXinGearId;
        private readonly Dictionary<int, Cell> _xinGearLiveShotCells = new Dictionary<int, Cell>();
        private string _xinGearLiveProductId;

        public int XinGearShotColumns => Math.Min(PhotoTotalCount,
            Math.Max(3, (int)Math.Ceiling(Math.Sqrt(PhotoTotalCount * 1.5))));

        private static int GetXinGearMosaicColumns(int count) =>
            count <= 6 ? 2 : (int)Math.Ceiling(Math.Sqrt(count));

        private bool UseXinGearProgressiveShotDisplay =>
            COpenProjectLine.IsXinGear && Name == COpenProjectLine.XinGearFixedProcessNames[2];

        public void InitializeXinGearPhotoCount(int count)
        {
            lock (_xinGearPhotoCountLock)
            {
                _activeXinGearPhotoCount = count;
                _pendingXinGearPhotoCount = 0;
                _switchAfterXinGearId = 0;
            }
            PhotoTotalCount = count;
        }

        public void QueueXinGearPhotoCount(int count, int currentId, bool defer)
        {
            bool applyImmediately;
            lock (_xinGearPhotoCountLock)
            {
                if (defer && currentId > 0)
                {
                    // 运行中只记录下一件张数；当前件继续使用旧格数和旧显示。
                    _pendingXinGearPhotoCount = count;
                    _switchAfterXinGearId = currentId;
                    applyImmediately = false;
                }
                else
                {
                    _activeXinGearPhotoCount = count;
                    _pendingXinGearPhotoCount = 0;
                    _switchAfterXinGearId = 0;
                    applyImmediately = true;
                }
            }

            if (applyImmediately)
            {
                ResetXinGearShotTiles(null, count, true);
            }
        }

        private bool ShouldDisplayXinGearCell(Cell cell)
        {
            if (cell == null)
            {
                return false;
            }
            return !UseXinGearProgressiveShotDisplay
                || !IsStart
                || string.Equals(cell.ID, _lastBoundProductId, StringComparison.Ordinal);
        }

        private static void ClearXinGearShotTile(XinGearShotTileVM tileVm)
        {
            if (tileVm == null)
            {
                return;
            }
            tileVm.ModelImage = null;
            if (tileVm.CurView == null)
            {
                return;
            }
            // ImageView 的 Source=null 不会主动清内部底图，必须显式置空。
            tileVm.CurView.UpdateImg(null);
            tileVm.CurView.Clear();
        }

        /// <summary>
        /// 停机切型或新产品 ID 到达时，原子切换格数并清除上一件逐格画面。
        /// </summary>
        private void ResetXinGearShotTiles(string productID, int photoCount, bool resetSelection)
        {
            if (!UseXinGearProgressiveShotDisplay)
            {
                return;
            }

            Action clearAction = () =>
            {
                _xinGearFilterPreviewCell = null;
                _xinGearLiveShotCells.Clear();
                _xinGearLiveProductId = productID;
                bool countChanged = photoCount > 0 && PhotoTotalCount != photoCount;
                if (resetSelection || countChanged)
                {
                    _xinGearFilterPreviewPhotoIndex = 1;
                }
                if (countChanged)
                {
                    PhotoTotalCount = photoCount;
                }
                foreach (XinGearShotTileVM tileVm in ShotTiles)
                {
                    ClearXinGearShotTile(tileVm);
                }
                ApplyXinGearShotTileFilterPreview(_xinGearFilterPreviewPhotoIndex);
            };

            var dispatcher = CMainModelsModelVM.Dispatcher;
            if (dispatcher == null || dispatcher.CheckAccess())
            {
                clearAction();
            }
            else
            {
                dispatcher.Invoke(clearAction);
            }
        }

        /// <summary>
        /// 页2 侧面分格（张数=本制程 PhotoTotalCount）。不进 .burrproj。
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
        /// 格数跟随 PhotoTotalCount。已绑定则 Clear 重填，避免同会话改 N 仍只有侧面1。
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
                    if (_xinGearLiveShotCells.TryGetValue(vm.PhotoIndex, out Cell liveCell))
                    {
                        DrawXinGearLiveShotTile(vm, liveCell);
                    }
                    else
                    {
                        ClearXinGearShotTile(vm);
                    }
                };
                _shotTiles.Add(tile);
            }
        }

        /// <summary>
        /// N 跟格只在新兴运行时收口。PhotoTotalCount 是全产线字段，禁止写进其 setter。
        /// 仅当页2 已物化 ShotTiles（拉链/盘齿/曲轴/_shotTiles 未建则为空操作）。
        /// </summary>
        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if (_shotTiles == null || e == null || e.PropertyName != nameof(PhotoTotalCount))
            {
                return;
            }
            RebuildXinGearShotTiles();
            OnPropertyChanged(nameof(XinGearShotColumns));
        }

        /// <summary>
        /// 正式自动取图：绑缓存 ProductID、本制程张号 1..N。无 PLC / ID&lt;=0 仍放行（离线可点开始），件号用 0。
        /// 返回 true 表示 cell 已处理完，取图线程 continue。
        /// </summary>
        private bool TryConsumeXinGearFormalCapture(Cell cell, ref bool IDisRight)
        {
            int id = CXinGearCommunicate.GetProductID();
            string productID = id > 0 ? id.ToString(CultureInfo.InvariantCulture) : "0";

            int expectedCount;
            int previousCount;
            lock (_xinGearPhotoCountLock)
            {
                previousCount = _activeXinGearPhotoCount > 0 ? _activeXinGearPhotoCount : PhotoTotalCount;
                if (productID != _lastBoundProductId && _pendingXinGearPhotoCount > 0
                    && id > _switchAfterXinGearId)
                {
                    _activeXinGearPhotoCount = _pendingXinGearPhotoCount;
                    _pendingXinGearPhotoCount = 0;
                }
                expectedCount = _activeXinGearPhotoCount > 0 ? _activeXinGearPhotoCount : PhotoTotalCount;
            }
            if (productID != _lastBoundProductId)
            {
                if (_photoCounter > 0 && _photoCounter < previousCount)
                {
                    SysLog.Warn($"{Name}-残图告警：制程/{_lastBoundProductId}/已收{_photoCounter}/应收{previousCount}");
                }
                _photoCounter = 0;
                _lastBoundProductId = productID;
                ResetXinGearShotTiles(productID, expectedCount, false);
            }

            _photoCounter++;
            cell.ID = productID;
            cell.PhotoIndex = _photoCounter;
            cell.PhotoTatolCount = expectedCount;

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
        /// 侧面按本件张数拼图；原六张配方继续使用 2 列 3 行。缺张留黑。
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

            int count = cells[0].PhotoTatolCount > 0 ? cells[0].PhotoTatolCount : cells.Count;
            int cols = GetXinGearMosaicColumns(count);
            int rows = (count + cols - 1) / cols;
            int dstW = tileW * cols;
            int dstH = tileH * rows;
            int dstStride = dstW * bytesPerPixel;
            int dstSize = dstStride * dstH;
            IntPtr dstPtr = Marshal.AllocHGlobal(dstSize);
            try
            {
                new Span<byte>((void*)dstPtr, dstSize).Clear();
                for (int photo = 1; photo <= count; photo++)
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
        /// 合并前深拷贝分张（图+时间+图内框）。Clone() 的 regionOut.points 仍是同一 List，offset 会污染分张。
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

        private static List<SRegion> CreateXinGearOffsetRegions(Cell source, CellDetection detection)
        {
            List<SRegion> regions = new List<SRegion>();
            if (source == null || detection?.regionOut == null)
            {
                return regions;
            }
            int idx = Math.Max(0, source.PhotoIndex - 1);
            int cols = GetXinGearMosaicColumns(source.PhotoTatolCount);
            int w = source.Image?.ImageWidth ?? 0;
            int h = source.Image?.ImageHeight ?? 0;
            double dx = (idx % cols) * w;
            double dy = (idx / cols) * h;
            foreach (SRegion region in detection.regionOut)
            {
                regions.Add(OffsetSRegion(region, dx, dy));
            }
            return regions;
        }

        /// <summary>
        /// 仅为合并 Cell 创建拼图坐标副本，不修改逐张 Cell 的局部坐标。
        /// </summary>
        private static List<CellDetection> CreateXinGearMergedAlgorithmOut(List<Cell> cells)
        {
            if (cells == null)
            {
                return new List<CellDetection>();
            }
            return cells
                .Where(source => source?.AlgorithmOut != null)
                .SelectMany(source => source.AlgorithmOut.Select(detection => (source, detection)))
                .GroupBy(item => item.detection.RecipeDefectName)
                .Select(group => new CellDetection
                {
                    RecipeDefectName = group.Key,
                    regionOut = group.SelectMany(item =>
                        CreateXinGearOffsetRegions(item.source, item.detection)).ToList(),
                    Category = group.First().detection.Category,
                    Value = group.SelectMany(item => item.detection.Value).ToList(),
                    Type = group.First().detection.Type,
                    Index = group.Max(item => item.detection.Index),
                    ShowInView = group.Max(item => item.detection.ShowInView)
                })
                .ToList();
        }

        /// <summary>
        /// 新兴侧面逐张预览：算法每完成一张就按 PhotoIndex 刷新一格，不等待整件 Merge。
        /// 这里只负责显示；整件 Filter/PLC/存图仍在收齐后沿用公共 Merge 流程。
        /// </summary>
        private void RefreshXinGearCurrentShotTiles(IReadOnlyList<Cell> currentCells)
        {
            if (!UseXinGearProgressiveShotDisplay || currentCells == null || currentCells.Count == 0)
            {
                return;
            }

            Cell latest = currentCells[currentCells.Count - 1];
            if (!ShouldDisplayXinGearCell(latest))
            {
                return;
            }
            // 新 ID 已在取图线程绑定后，迟到的旧件算法结果不得恢复上一件画面。
            if (!string.Equals(latest.ID, _lastBoundProductId, StringComparison.Ordinal))
            {
                return;
            }
            if (!string.Equals(_xinGearLiveProductId, latest.ID, StringComparison.Ordinal))
            {
                _xinGearLiveShotCells.Clear();
                _xinGearLiveProductId = latest.ID;
                _xinGearFilterPreviewCell = null;
            }

            foreach (Cell shotCell in currentCells.OrderBy(c => c.CreateTime))
            {
                if (shotCell.PhotoIndex >= 1 && shotCell.PhotoIndex <= PhotoTotalCount)
                {
                    // 同一张号重复到达时保留最新算法结果。
                    _xinGearLiveShotCells[shotCell.PhotoIndex] = shotCell;
                }
            }

            foreach (XinGearShotTileVM tileVm in ShotTiles)
            {
                _xinGearLiveShotCells.TryGetValue(tileVm.PhotoIndex, out Cell shotCell);
                DrawXinGearLiveShotTile(tileVm, shotCell);
            }
        }
        /// <summary>
        /// 兼容最终结果手动回显；逐张运行流程不调用此入口。
        /// </summary>
        public void RefreshXinGearShotTiles(Cell cell)
        {
            if (!ShouldDisplayXinGearCell(cell)) return;
            foreach (XinGearShotTileVM tileVm in ShotTiles)
            {
                DrawXinGearShotTile(tileVm, cell);
            }
            ApplyXinGearShotTileFilterPreview(_xinGearFilterPreviewPhotoIndex, cell);
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

            int w = tileImg?.ImageWidth ?? 0;
            int h = tileImg?.ImageHeight ?? 0;
            int idx = Math.Max(0, tileVm.PhotoIndex - 1);
            int cols = GetXinGearMosaicColumns(cell?.PhotoTatolCount ?? PhotoTotalCount);
            double dx = (idx % cols) * w;
            double dy = (idx / cols) * h;
            DrawXinGearShotTileCore(tileVm, tileImg, cell, dx, dy);
        }

        private void DrawXinGearLiveShotTile(XinGearShotTileVM tileVm, Cell shotCell)
        {
            DrawXinGearShotTileCore(tileVm, shotCell?.Image, shotCell, 0, 0);
        }

        private void DrawXinGearShotTileCore(
            XinGearShotTileVM tileVm,
            CImage tileImg,
            Cell detectionCell,
            double dx,
            double dy)
        {
            if (tileVm == null)
            {
                return;
            }

            // 空格必须保持真正空白，不能在残留底图上绘制 OK。
            if (tileImg == null)
            {
                ClearXinGearShotTile(tileVm);
                return;
            }
            // ToBitmapSource 不拷像素；Clone 后由 UI 独立持有，避免原 Cell 后续释放影响分格。
            BitmapSource raw = tileImg.ToBitmapSource();
            if (raw == null)
            {
                ClearXinGearShotTile(tileVm);
                return;
            }
            tileVm.ModelImage = raw.Clone();
            if (tileVm.CurView == null)
            {
                return;
            }
            tileVm.CurView.Clear(false);

            int w = tileImg?.ImageWidth ?? 0;
            int h = tileImg?.ImageHeight ?? 0;
            string cornerName = null;
            if (detectionCell?.AlgorithmOut != null && w > 0 && h > 0)
            {
                foreach (CellDetection det in detectionCell.AlgorithmOut)
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
            if (!string.IsNullOrEmpty(cornerName))
            {
                Brush resultBrush = detectionCell?.Quality?.ShowColor.Brush ?? Brushes.Red;
                string resultName = detectionCell?.Quality?.Name ?? "NG";
                tileVm.CurView.SetFontBrush(resultBrush);
                tileVm.CurView.WinDrawText(resultName + ":" + cornerName, AlignmentX.Right, AlignmentY.Top, false);
            }
            else
            {
                tileVm.CurView.SetFontBrush(MaociQualityConfig?.GetBest()?.ShowColor.Brush ?? Brushes.Lime);
                tileVm.CurView.WinDrawText("OK", AlignmentX.Right, AlignmentY.Top, false);
            }

            if (detectionCell?.AlgorithmOut != null && w > 0 && h > 0)
            {
                tileVm.CurView.SetPen(Brushes.Red);
                tileVm.CurView.SetFontBrush(Brushes.Red);
                tileVm.CurView.SetFontSize(15);
                tileVm.CurView.SetFontWeight(System.Windows.FontWeights.Normal);
                foreach (CellDetection det in detectionCell.AlgorithmOut)
                {
                    if (det?.regionOut == null)
                    {
                        continue;
                    }
                    foreach (SRegion region in det.regionOut)
                    {
                        if (!XinGearRegionInMosaicTile(region, dx, dy, w, h))
                        {
                            continue;
                        }
                        SRegion local = OffsetSRegion(region, -dx, -dy);
                        tileVm.CurView.ImgDrawRegion(local.points, false);
                        if (!string.IsNullOrEmpty(det.RecipeDefectName))
                        {
                            tileVm.CurView.ImgDrawText(det.RecipeDefectName, local.GetBottomRight(), false);
                        }
                    }
                }
            }
            tileVm.CurView.Invalidate();
        }

        int _xinGearFilterPreviewPhotoIndex = 1;

        /// <summary>
        /// 检测区展示按格切片：只改 ResultList 数字
        /// </summary>
        public void ApplyXinGearShotTileFilterPreview(int photoIndex, Cell sourceCell = null)
        {
            if (photoIndex < 1)
            {
                photoIndex = 1;
            }
            _xinGearFilterPreviewPhotoIndex = photoIndex;
            if (MaociFilterConfig == null)
            {
                return;
            }
            if (sourceCell != null)
            {
                if (!ShouldDisplayXinGearCell(sourceCell))
                {
                    return;
                }
                _xinGearFilterPreviewCell = sourceCell;
            }
            Cell sliceCell = sourceCell ?? _xinGearFilterPreviewCell;
            // 禁止 FilterExute 做预览。检测区 Result/数字只按选中格覆盖，整件判定已在 FilterExute(newCell)。
            ResetXinGearFilterResultDisplay();
            List<CellDetection> sliced = SliceXinGearAlgorithmOutToPhoto(sliceCell, photoIndex);
            FillXinGearFilterResultDisplay(sliced);
        }

        void FillXinGearFilterResultDisplay(List<CellDetection> sliced)
        {
            if (sliced == null || MaociFilterConfig == null)
            {
                return;
            }
            foreach (CellDetection det in sliced)
            {
                if (det?.regionOut == null || det.regionOut.Count == 0 || string.IsNullOrEmpty(det.Type) || string.IsNullOrEmpty(det.RecipeDefectName))
                {
                    continue;
                }
                var species = MaociFilterConfig[det.Type];
                if (species == null)
                {
                    continue;
                }
                var recipe = species[det.RecipeDefectName];
                if (recipe?.DefectFilters == null || recipe.DefectFilters.Count == 0)
                {
                    continue;
                }
                var de = recipe.DefectFilters[0];
                de.Result = false;
                species.Result = false;
                if (de.ResultList == null)
                {
                    continue;
                }
                foreach (var item in de.ResultList)
                {
                    if (item.Feature == CFeacture.FeactureCount)
                    {
                        item.Value = det.regionOut.Count;
                    }
                    else
                    {
                        double v = 0;
                        foreach (SRegion region in det.regionOut)
                        {
                            if (region.regionInfo != null)
                            {
                                v = region.regionInfo.GetValue(item.Feature, region);
                            }
                        }
                        item.Value = v;
                    }
                }
            }
        }

        void ResetXinGearFilterResultDisplay()
        {
            if (MaociFilterConfig.SpeciesFilters == null)
            {
                return;
            }
            foreach (var sp in MaociFilterConfig.SpeciesFilters)
            {
                sp.Result = true;
                if (sp.RecipeDefects == null)
                {
                    continue;
                }
                foreach (var rd in sp.RecipeDefects)
                {
                    if (rd.DefectFilters == null)
                    {
                        continue;
                    }
                    foreach (var de in rd.DefectFilters)
                    {
                        de.Result = true;
                        if (de.ResultList == null)
                        {
                            continue;
                        }
                        foreach (var item in de.ResultList)
                        {
                            item.Value = 0;
                        }
                    }
                }
            }
        }

        List<CellDetection> SliceXinGearAlgorithmOutToPhoto(Cell cell, int photoIndex)
        {
            List<CellDetection> sliced = new List<CellDetection>();
            if (cell?.AlgorithmOut == null)
            {
                return sliced;
            }
            int w = 0;
            int h = 0;
            if (cell.XinGearImages != null)
            {
                foreach ((CImage img, int itemPhoto, DateTime t, TimeSpan cost) item in cell.XinGearImages)
                {
                    if (item.itemPhoto == photoIndex && item.img != null)
                    {
                        w = item.img.ImageWidth;
                        h = item.img.ImageHeight;
                        break;
                    }
                }
            }
            if (w <= 0 || h <= 0)
            {
                return sliced;
            }
            int idx = photoIndex - 1;
            if (idx < 0)
            {
                idx = 0;
            }
            int cols = GetXinGearMosaicColumns(cell?.PhotoTatolCount ?? PhotoTotalCount);
            double dx = (idx % cols) * w;
            double dy = (idx / cols) * h;
            foreach (CellDetection det in cell.AlgorithmOut)
            {
                if (det?.regionOut == null)
                {
                    continue;
                }
                List<SRegion> kept = new List<SRegion>();
                foreach (SRegion region in det.regionOut)
                {
                    if (XinGearRegionInMosaicTile(region, dx, dy, w, h))
                    {
                        kept.Add(region);
                    }
                }
                if (kept.Count == 0)
                {
                    continue;
                }
                CellDetection copy = det.Clone();
                copy.regionOut = kept;
                sliced.Add(copy);
            }
            return sliced;
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
