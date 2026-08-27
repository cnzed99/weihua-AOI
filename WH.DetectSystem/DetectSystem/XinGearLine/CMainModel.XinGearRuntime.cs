using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using XinGearInfo;
using WH.DetectSystem.DetectSystem.MainModel;
using WH.RecipeCellRootBase;
using WH.RunCell;

namespace WH.DetectSystem.Models
{
    /// <summary>
    /// 【新兴盘齿方案4-注释】 新兴制程运行时：取图绑 ID、XinGearImages 分张、侧面 2x2、组结果回写。
    /// 与 MainVM 同为 CMainModel 部分类；复用盘齿私有字段 _lastBoundProductId / _photoCounter。
    /// 禁止写入 GearImages；无上齿面转发。
    /// </summary>
    public partial class CMainModel
    {
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

        private void AppendXinGearMergedStationImages(Cell newCell, List<Cell> currentCells)
        {
            for (int i = 0; i < currentCells.Count; i++)
            {
                newCell.XinGearImages.Add((currentCells[i].Image, currentCells[i].PhotoIndex,
                    currentCells[i].CreateTime, currentCells[i].RecipeTime));
            }
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
        /// 【新兴盘齿方案4-注释】 侧面 4 张按 PhotoIndex 1..4 拼 2x2（左上/右上/左下/右下）。缺张留黑。
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

            int dstW = tileW * 2;
            int dstH = tileH * 2;
            int dstStride = dstW * bytesPerPixel;
            int dstSize = dstStride * dstH;
            IntPtr dstPtr = Marshal.AllocHGlobal(dstSize);
            try
            {
                new Span<byte>((void*)dstPtr, dstSize).Clear();
                for (int i = 0; i < 4; i++)
                {
                    Cell srcCell = i < cells.Count ? cells[i] : null;
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

                    int col = i % 2;
                    int row = i / 2;
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

        private void SendXinGearGroupResult(CCellPro CellOut)
        {
            CXinGearCommunicate.SendGroupResult(
                ProcessGroup.Name,
                CellOut.Cell.ID,
                CellOut.Cell.IsOK ? XinGearResult.OK : XinGearResult.NG);
        }
    }
}