using System.Globalization;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Mysqlx.Crud;
using SaveImageManage;
using SDFilter;
using WH.DetectSystem.Models;
using WH.Entity.DiskSpace;
using WH.Entity.LogRecord;
using WH.RecipeCellRootBase;
using WH.RunCell;
using Path = System.IO.Path;

namespace WH.DetectSystem._5_存图操作
{
    public static class SaveImageStage
    {
        /// <summary>
        /// 2024.7.19 李焕彬
        /// 运行日志
        /// </summary>
        public static CLogRec s_SysLog = CLogRec.Create("Info", "D:/Data");

        private static StringBuilder s_NameBuilder = new StringBuilder();

        /// <summary>
        /// 路径锁 防止多线程访问导致路径不一致
        /// </summary>
        private static readonly object s_PathLock = new object();

        /// <summary>
        /// 2024.8.1 李焕彬
        /// 清理图片线程
        /// </summary>
        private static Task s_TaskClear;

        /// <summary>
        /// 2024.8.2 李焕彬
        /// 清理线程开始日期
        /// </summary>
        private static DateTime s_DateTaskClear;

        /// <summary>
        /// 2024.8.2 李焕彬
        /// 停止存图标志
        /// </summary>
        private static bool s_StopSave = false;

        /// <summary>
        /// 2024.8.2 李焕彬
        /// 总存图计数（清理图片线程开启间隔）
        /// </summary>
        private static int s_SaveCountAll = 0;

        /// <summary>
        /// 2024.8.2 李焕彬
        /// 当前存图的制程存图文件夹集合，防止清理图片时删除制程存图文件夹
        /// </summary>
        private static List<string> s_ProjSavePaths;

        /// <summary>
        /// 保存原图和截图 并返回截图路径
        /// </summary>
        /// <param name="saveImageConfig">存图配置</param>
        /// <param name="systemSettings">系统设置</param>
        /// <param name="cell">cell</param>
        /// <param name="saveCountOk">OK存图间隔计数</param>
        /// <returns>截图路径</returns>
        public static string Excute(
            this CSaveImageConfig saveImageConfig,
            CSystemSettingsModel systemSettings,
            Cell cell,
            ref int saveCountOk
        )
        {
            //总存图计数达到500或停止存图时执行清理存图线程
            if (++s_SaveCountAll > 500 || s_StopSave)
            {
                s_SaveCountAll = 0;
                if (s_TaskClear == null || s_TaskClear.IsCompleted)
                {
                    s_ProjSavePaths = new List<string>();
                    s_ProjSavePaths.Add(saveImageConfig.SaveImagePath + "\\" + cell.ProjName);
                    s_DateTaskClear = DateTime.Now;
                    s_TaskClear = Task.Run(() =>
                    {
                        ClearHardDiskSpace(saveImageConfig);
                    });
                }
                if (s_StopSave)
                    return null;
            }
            string savePath = string.Empty;
            try
            {
                string classPath;
                string cropPath;
                string cropName;
                saveImageConfig.GetSavePath(
                    cell,
                    systemSettings.NowShift,
                    out classPath,
                    out cropPath,
                    out cropName
                );
                if (!cell.IsOK || saveImageConfig.OKScreenShot)
                {
                    if (saveImageConfig.PiantScreenEnable)
                    {
                        savePath = SaveDumpImage(
                            cell,
                            classPath,
                            saveImageConfig.SaveImageFormat,
                            systemSettings.ShowAllDefect
                        );
                    }
                }

                if (saveImageConfig.SaveImageEnable) //开启存原图
                {
                    string fileName = classPath;
                    if (Directory.Exists(Directory.GetParent(fileName).FullName))
                    {
                        Directory.CreateDirectory(Directory.GetParent(fileName).FullName);
                    }
                    switch (saveImageConfig.SaveSelect)
                    {
                        case "0": //存所有图
                            if (cell.IsOK)
                            {
                                saveCountOk++;
                                if (saveCountOk >= saveImageConfig.OkIntervalCount)
                                {
                                    saveCountOk = 0;
                                    WriteImage(
                                        cell,
                                        fileName,
                                        saveImageConfig.SaveImageFormat
                                    );
                                }
                            }
                            else
                            {
                                WriteImage(cell, fileName, saveImageConfig.SaveImageFormat);
                            }

                            break;

                        case "1": //只存不良图
                            if (!cell.IsOK)
                            {
                                WriteImage(cell, fileName, saveImageConfig.SaveImageFormat);
                            }
                            break;

                        case "2": //只存合格图
                            if (cell.IsOK)
                            {
                                saveCountOk++;
                                if (saveCountOk >= saveImageConfig.OkIntervalCount)
                                {
                                    saveCountOk = 0;
                                    WriteImage(
                                        cell,
                                        fileName,
                                        saveImageConfig.SaveImageFormat
                                    );
                                }
                            }
                            break;
                    }
                }
                return savePath;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        ///20240717 TCG
        ///保存截图并返回其路径
        /// </summary>
        /// <param name="cell"></param>
        /// <param name="dumpImagePath"></param>
        /// <param name="format">格式</param>
        /// <returns>保存路径</returns>
        private static string SaveDumpImage(
            Cell cell,
            string dumpImagePath,
            string format,
            bool showAllDefect
        )
        {
            DrawingVisual drawingVisual = new DrawingVisual();
            DrawingContext drawingContext = drawingVisual.RenderOpen();
            drawingContext.DrawImage(
                cell.Image.ToBitmapSource(),
                new Rect(0, 0, cell.Image.ImageWidth, cell.Image.ImageHeight)
            );
            foreach (var edge in cell.DrawEdges)
            {
                Pen pen = new Pen(edge.BrushDraw, 1);

                switch (edge.DrawType)
                {

                    case EMDRAWTYPE.EMDRAWTYPE_POINTS:
                        //drawView.SetPen(edge.BrushDraw);
                        //drawView.ImgDrawPoints(edge.Points, false);

                        DrawPoints(edge.Points, pen);
                        break;

                    case EMDRAWTYPE.EMDRAWTYPE_REGION:

                        DrawPoints(edge.Points, pen);
                        break;

                    case EMDRAWTYPE.EMDRAWTYPE_Text:
                        DrawText(edge.Text, edge.TextPos, edge.BrushDraw, cell.Image.ImageWidth / 20);
                        break;
                }

            }
            if (!cell.IsOK)
            {
                DefectFilter dstFilter = cell.Detection.DefectFilter;
                StringBuilder textBuilder = new StringBuilder();
                textBuilder.AppendLine(dstFilter.Name);
                textBuilder.Append(cell.Quality.Name);
                DrawTextAlignment(
                    textBuilder.ToString(),
                    AlignmentX.Right,
                    AlignmentY.Top,
                    cell.Quality.ShowColor.Brush, cell.Image.ImageWidth / 20
                );
                //显示所有Region缺陷
                if (showAllDefect)
                {
                    foreach (var detection in cell.Detections)
                    {
                        if (
                            detection.Result
                            || detection.Category != Category.区域
                            || detection.regionOut.Count == 0
                        )
                            continue;
                        DefectFilter defectFilter = detection.DefectFilter;
                        Pen penDraw = new Pen(defectFilter.ShowColor.Brush, 1);
                        for (int i = 0; i < detection.regionOut.Count; i++)
                        {
                            DrawPoints(detection.regionOut[i].points, penDraw);
                            DrawText(
                                    detection.DetectLog[i].ToString(),
                                    detection.regionOut[i].GetCenter(),
                                    defectFilter.ShowColor.Brush, cell.Image.ImageWidth / 20
                                );
                            //if (i == detection.regionOut.Count - 1)
                            //{
                            //    DrawText(
                            //        detection.DetectLog.ToString(),
                            //        detection.regionOut[i].GetCenter(),
                            //        defectFilter.ShowColor.Brush
                            //    );
                            //}
                        }
                    }
                }
                else
                {
                    DefectFilter defectFilter = cell.Detection.DefectFilter;
                    if (
                        !(
                            cell.Detection.Result
                            || cell.Detection.Category != Category.区域
                            || cell.Detection.regionOut.Count == 0
                        )
                    )
                    {
                        Pen penDraw = new Pen(defectFilter.ShowColor.Brush, 1);
                        for (int i = 0; i < cell.Detection.regionOut.Count; i++)
                        {
                            DrawPoints(cell.Detection.regionOut[i].points, penDraw);
                            DrawText(
                                   cell.Detection.DetectLog[i].ToString(),
                                   cell.Detection.regionOut[i].GetCenter(),
                                   defectFilter.ShowColor.Brush, cell.Image.ImageWidth / 20
                               );
                            //if (i == cell.Detection.regionOut.Count - 1)
                            //{
                            //    DrawText(
                            //        cell.Detection.DetectLog.ToString(),
                            //        cell.Detection.regionOut[i].GetCenter(),
                            //        defectFilter.ShowColor.Brush
                            //    );
                            //}
                        }
                    }
                }
            }
            else
            {
                DrawTextAlignment(
                    "OK",
                    AlignmentX.Right,
                    AlignmentY.Top,
                    cell.Quality.ShowColor.Brush, cell.Image.ImageWidth / 20
                );
            }
            drawingContext.Close();
            RenderTargetBitmap renderTargetBitmap =
                new(cell.Image.ImageWidth, cell.Image.ImageHeight, 96, 96, PixelFormats.Default);
            renderTargetBitmap.Render(drawingVisual);
            renderTargetBitmap.Freeze();

            string directory = Path.GetDirectoryName(dumpImagePath);
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(dumpImagePath);
            string dirPath = $"{directory}{"\\Jpg\\"}";
            string path = $"{dirPath}{fileNameWithoutExtension}{".jpg"}";
            if (!Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
            }
            WriteImage(renderTargetBitmap, path, ".jpg");

            #region 绘制区域、文字方法
            void DrawGeometry(List<Point> points, Pen pen)
            {
                PathGeometry geometry = new PathGeometry();
                PolyLineSegment polyLineSegment = new PolyLineSegment();
                polyLineSegment.Points = new PointCollection(points);
                PathFigure figure = new PathFigure(points[0], new[] { polyLineSegment }, false);
                geometry.Figures.Add(figure);

                drawingContext.DrawGeometry(Brushes.Transparent, pen, geometry);
            }

            void DrawPoints(List<Point> points, Pen pen)
            {
                List<Point> region = new List<Point>();
                foreach (var item in points)
                {
                    if (
                        region.Count > 0
                        && Math.Sqrt(
                            (region.Last().X - item.X) * (region.Last().X - item.X)
                                + (region.Last().Y - item.Y) * (region.Last().Y - item.Y)
                        ) > 2
                    )
                    {
                        DrawGeometry(region, pen);
                        // region = new List<Point>();
                    }
                    region.Add(item);
                }
                if (region.Count > 0)
                {
                    DrawGeometry(region, pen);
                }
            }

            void DrawTextAlignment(
                string text,
                AlignmentX alignmentX,
                AlignmentY alignmentY,
                Brush fontBrush, int fontSize
            )
            {
                // int fontSize = 50;
                FontFamily fontFamily = new FontFamily("宋体");
                FontStyle fontStyle = FontStyles.Normal;
                FontWeight fontWeight = FontWeights.Normal;
                FormattedText formattedText = new FormattedText(
                    text,
                    CultureInfo.InvariantCulture,
                    FlowDirection.LeftToRight,
                    new Typeface(fontFamily, fontStyle, fontWeight, FontStretches.Normal),
                    fontSize,
                    fontBrush,
                    1
                );
                double x = 20;
                double y = 20;
                switch (alignmentX)
                {
                    case AlignmentX.Center:
                        x = cell.Image.ImageWidth / 2 - formattedText.Width / 2;
                        break;
                    case AlignmentX.Right:
                        x = cell.Image.ImageWidth - formattedText.Width - 20;
                        break;
                }
                switch (alignmentY)
                {
                    case AlignmentY.Center:
                        y = cell.Image.ImageHeight / 2 - formattedText.Height / 2;
                        break;
                    case AlignmentY.Bottom:
                        y = cell.Image.ImageHeight - formattedText.Height - 20;
                        break;
                }
                drawingContext.DrawText(formattedText, new Point(x, y));
            }

            void DrawText(string text, Point origin, Brush fontBrush, int fontSize)
            {
                //int fontSize = 50;
                FontFamily fontFamily = new FontFamily("宋体");
                FontStyle fontStyle = FontStyles.Normal;
                FontWeight fontWeight = FontWeights.Normal;
                FormattedText formattedText = new FormattedText(
                    text,
                    CultureInfo.InvariantCulture,
                    FlowDirection.LeftToRight,
                    new Typeface(fontFamily, fontStyle, fontWeight, FontStretches.Normal),
                    fontSize,
                    fontBrush,
                    1
                );
                drawingContext.DrawText(formattedText, origin);
            }
            #endregion

            return path;
        }

        /// <summary>
        /// 获取存图路径, 源路径\\按工程名\\按时间白晚班\\按小时\\按相机名\\OK/NG\\检测类\\缺陷名
        /// </summary>
        /// <param name="cell">cell</param>
        /// <param name="classPath">存图路径</param>
        /// <param name="cropPath">存图文件夹路径</param>
        /// <param name="cropName">截图名称</param>
        public static void GetSavePath(
            this CSaveImageConfig saveImageConfig,
            Cell cell,
            string nowShift,
            out string classPath,
            out string cropPath,
            out string cropName
        )
        {
            lock (s_PathLock)
            {
                try
                {
                    // 图片命名:时间-流水ID-质量等级-缺陷名-处理时间

                    cropPath = "";
                    cropName = "";
                    string filename = "";
                    s_NameBuilder.Clear();
                    s_NameBuilder.Append("\\");

                    s_NameBuilder.Append(cell.ID); //ID号
                    s_NameBuilder.Append("_");

                    s_NameBuilder.Append(cell.PhotoIndex); //图片编号
                    s_NameBuilder.Append("_");

                    cropName = s_NameBuilder.ToString();

                    //s_NameBuilder.Append(string.Format("{0:HHmmssfff}", cell.CreateTime)); //时间
                    //s_NameBuilder.Append("-");

                    s_NameBuilder.Append(cell.Quality?.Signal); //质量信号值
                    s_NameBuilder.Append("_");
                    s_NameBuilder.Append(cell.Quality?.Name); //质量等级名称
                    s_NameBuilder.Append("_");

                    s_NameBuilder.Append(cell.Detection?.DefectFilter?.Name ?? "OK"); //缺陷名称
                   // s_NameBuilder.Append("-");

                    //s_NameBuilder.Append(cell.ProcessTime.TotalMilliseconds.ToString("F0")); //耗时
                    s_NameBuilder.Append(saveImageConfig.SaveImageFormat); //格式
                    filename = s_NameBuilder.ToString();

                    classPath = saveImageConfig.SaveImagePath;
                    if (saveImageConfig.SavebyProjName)
                    {
                        classPath = classPath + "\\" + cell.ProjName;
                    }
                    classPath = classPath + "\\" + nowShift;
                    //if (saveImageConfig.SavebyHour)
                    //{
                    //    string hourNow = cell.CreateTime.Hour.ToString("D2");
                    //    classPath = classPath + "\\" + hourNow;
                    //}
                    //if (saveImageConfig.SavebyCamName)
                    //{
                    //    if (!string.IsNullOrEmpty(cell.CamName))
                    //    {
                    //        classPath = classPath + "\\" + cell.CamName;
                    //    }
                    //}
                    if (cell.IsOK)
                    {
                        classPath = classPath + "\\OK";
                    }
                    else
                    {
                        classPath = classPath + "\\NG";
                        cropPath = classPath;
                        if (saveImageConfig.SavebyDefectName)
                        {
                            if (!(cell.Detection is null))
                            {
                                classPath =
                                    classPath
                                    + "\\"
                                    + cell.Detection.Type
                                    + "\\"
                                    + cell.Detection.DefectFilter.Name;
                                ;
                            }
                        }
                    }
                    //if (saveImageConfig.SavebyID)
                    //{
                    classPath = classPath + "\\" + cell.ID;
                    cropPath = classPath;
                    //}

                    if (!Directory.Exists(classPath))
                    {
                        Directory.CreateDirectory(classPath);
                    }

                    classPath = classPath + filename;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }

        /// <summary>
        /// 2024.7.29 李焕彬
        /// 保存图片
        /// </summary>
        /// <param name="bitImage">图片</param>
        /// <param name="filepath">存图路径</param>
        /// <param name="format">图片格式</param>
        private static void WriteImage(CImage image, string filepath, string format)
        {
            if (image != null)
            {
                using (FileStream stream = new FileStream(filepath, FileMode.Create))
                {

                    BitmapEncoder encoder = GetEncoder(format);
                    encoder.Frames.Add(BitmapFrame.Create(image.ToBitmapSource()));
                    encoder.Save(stream);
                }
            }
        }

        /// <summary>
        /// 2025.6.5 鲍赞宝
        /// 保存图片
        /// </summary>
        /// <param name="bitImage">图片</param>
        /// <param name="filepath">存图路径</param>
        /// <param name="format">图片格式</param>
        private static void WriteImage(Cell cell, string filepath, string format)
        {
            if (cell != null && cell.ZipperImages != null)
            {
                string[] filenames = filepath.Split('.');
                if (filenames.Length >= 2)
                {
                    foreach ((CImage, int) img in cell.ZipperImages)
                    {
                        string[] namesplits = filenames[0].Split('_');
                        if (namesplits.Length >= 2)
                        {
                            namesplits[1] = img.Item2.ToString();
                            filenames[0] = string.Join("_", namesplits);
                            string createtime = string.Format("{0:HHmmssfff}", cell.CreateTime);
                            string filename = $"{filenames[0]}_{createtime}_{cell.ProcessTime.TotalMilliseconds.ToString("F0")}.{filenames[1]}";

                            using (FileStream stream = new FileStream(filename, FileMode.Create))
                            {
                                BitmapEncoder encoder = GetEncoder(format);
                                encoder.Frames.Add(BitmapFrame.Create(img.Item1.ToBitmapSource()));
                                encoder.Save(stream);
                            }
                        }
                    }

                }


            }
        }

        /// <summary>
        /// 2024.7.29 李焕彬
        /// 保存图片
        /// </summary>
        /// <param name="bitImage">图片</param>
        /// <param name="filepath">存图路径</param>
        /// <param name="format">图片格式</param>
        private static void WriteImage(BitmapSource bitmap, string filepath, string format)
        {
            if (bitmap != null)
            {
                using (FileStream stream = new FileStream(filepath, FileMode.Create))
                {
                    BitmapEncoder encoder = GetEncoder(format);
                    encoder.Frames.Add(BitmapFrame.Create(bitmap));
                    encoder.Save(stream);
                }
            }
        }

        /// <summary>
        /// 获取格式
        /// </summary>
        /// <returns></returns>
        private static BitmapEncoder GetEncoder(string format)
        {
            BitmapEncoder encoder;
            switch (format)
            {
                case ".bmp":
                    encoder = new BmpBitmapEncoder();
                    break;
                case ".jpg":
                    encoder = new JpegBitmapEncoder();
                    break;
                case ".png":
                    encoder = new PngBitmapEncoder();
                    break;
                case ".tiff":
                    encoder = new TiffBitmapEncoder();
                    break;
                default:
                    encoder = new BmpBitmapEncoder();
                    break;
            }
            return encoder;
        }

        /// <summary>
        /// 2024.8.1 李焕彬
        /// 按设置时间、空间清理磁盘空间
        /// </summary>
        /// <param name="saveImageConfig">存图配置</param>
        public static void ClearHardDiskSpace(this CSaveImageConfig saveImageConfig)
        {
            try
            {
                if (Directory.Exists(saveImageConfig.SaveImagePath))
                {
                    DelOverTimeFiles(saveImageConfig);
                    DelOverSpaceFiles(saveImageConfig);
                }
            }
            catch (Exception ex)
            {
                s_SysLog.Error("存图线程出错：" + ex.Message + ex.StackTrace);
            }
        }

        /// <summary>
        /// 2024.8.1 李焕彬
        /// 删除超过设置天数的图像文件夹
        /// </summary>
        /// <param name="saveImageConfig">存图配置</param>
        private static void DelOverTimeFiles(CSaveImageConfig saveImageConfig)
        {
            DelOverTimeFiles(
                saveImageConfig.SaveImagePath,
                saveImageConfig.OkDays,
                saveImageConfig.NgDays
            );
        }

        /// <summary>
        /// 2024.8.1 李焕彬
        /// 删除超过设置天数的图像文件夹，递归
        /// </summary>
        /// <param name="dir">父文件夹</param>
        /// <param name="overDaysOk">Ok存储天数</param>
        /// <param name="overDaysNg">Ng存储天数</param>
        private static void DelOverTimeFiles(string dir, int overDaysOk, int overDaysNg)
        {
            //判断是否包含OK/NG文件夹，是则删除后退出,否则向下查找
            if (
                Directory
                    .GetDirectories(dir)
                    .FirstOrDefault(o => Path.GetFileName(o) == "OK" || Path.GetFileName(o) == "NG")
                != null
            )
            {
                DeleteOkNgPathByDate(dir, overDaysOk, overDaysNg);
            }
            else
            {
                foreach (var subDir in Directory.GetDirectories(dir))
                {
                    DelOverTimeFiles(subDir, overDaysOk, overDaysNg);
                }
                //不删除工程名文件夹
                if (Directory.GetDirectories(dir).Length == 0 && !s_ProjSavePaths.Contains(dir))
                {
                    Directory.Delete(dir, true);
                }
            }
        }

        /// <summary>
        /// 2024.8.1 李焕彬
        /// 超出剩余可用空间时删除文件夹
        /// </summary>
        /// <param name="saveImageConfig">存图配置</param>
        private static void DelOverSpaceFiles(CSaveImageConfig saveImageConfig)
        {
            if (
                DiskSpace.GetHardDiskSpace(saveImageConfig.SaveImagePath)
                <= saveImageConfig.FreeSpaceLimit
            )
            {
                var subDirs = Directory.GetDirectories(saveImageConfig.SaveImagePath).ToList();
                if (subDirs.Count > 0)
                {
                    subDirs.Sort(
                        delegate (string l, string r)
                        {
                            return File.GetCreationTime(l).CompareTo(File.GetCreationTime(r));
                        }
                    );
                    foreach (var subDir in subDirs)
                    {
                        DelOverSpaceFiles(subDir, saveImageConfig.FreeSpaceLimit);
                        if (DiskSpace.GetHardDiskSpace(subDir) > saveImageConfig.FreeSpaceLimit)
                            break;
                    }
                }
            }
            if (
                DiskSpace.GetHardDiskSpace(saveImageConfig.SaveImagePath)
                <= saveImageConfig.FreeSpaceLimit
            )
            {
                s_StopSave = true;
            }
            else
            {
                s_StopSave = false;
            }
        }

        /// <summary>
        /// 2024.8.1 李焕彬
        /// 超出剩余可用空间时删除文件夹，递归
        /// </summary>
        /// <param name="dir">目标文件夹</param>
        /// <param name="freeSpaceLimit">可用空间</param>
        private static void DelOverSpaceFiles(string dir, int freeSpaceLimit)
        {
            //判断是否包含OK/NG文件夹，是则删除后退出,否则向下查找
            if (
                Directory
                    .GetDirectories(dir)
                    .FirstOrDefault(o => Path.GetFileName(o) == "OK" || Path.GetFileName(o) == "NG")
                != null
            )
            {
                DeleteOkNgPathByDate(dir, 1, 1);
            }
            else
            {
                var subDirs = Directory.GetDirectories(dir).ToList();
                if (subDirs.Count > 0)
                {
                    subDirs.Sort(
                        delegate (string l, string r)
                        {
                            return File.GetCreationTime(l).CompareTo(File.GetCreationTime(r));
                        }
                    );
                    foreach (var subDir in subDirs)
                    {
                        DelOverSpaceFiles(subDir, freeSpaceLimit);
                        if (DiskSpace.GetHardDiskSpace(dir) > freeSpaceLimit)
                            break;
                    }
                }
                //不删除工程名文件夹
                if (Directory.GetDirectories(dir).Length == 0 && !s_ProjSavePaths.Contains(dir))
                {
                    Directory.Delete(dir, true);
                }
            }
        }

        /// <summary>
        /// 2024.8.2 李焕彬
        /// 删除超过天数的OK/NG文件夹，文件夹为空时自动删除
        /// </summary>
        /// <param name="parent">父文件夹</param>
        /// <param name="overDaysOk">ok存储天数</param>
        /// <param name="overDaysNg">ng存储天数</param>
        private static void DeleteOkNgPathByDate(string parent, int overDaysOk, int overDaysNg)
        {
            string pathOk = parent + "\\OK";
            if (
                Directory.Exists(pathOk)
                && (s_DateTaskClear.Day - Directory.GetCreationTime(pathOk).Day) >= overDaysOk
            )
            {
                Directory.Delete(pathOk, true);
            }
            string pathNg = parent + "\\NG";
            if (
                Directory.Exists(pathNg)
                && (s_DateTaskClear.Day - Directory.GetCreationTime(pathNg).Day) >= overDaysNg
            )
            {
                Directory.Delete(pathNg, true);
            }
            if (Directory.GetDirectories(parent).Length == 0)
            {
                Directory.Delete(parent, true);
            }
        }
    }
}
