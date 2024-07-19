using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using SaveImageManage;
using WH.RunCell;

namespace WH.DetectSystem._5_存图操作
{
    public static class SaveImageStage
    {
        private static StringBuilder nameBuilder = new StringBuilder();

        /// <summary>
        /// 路径锁 防止多线程访问导致路径不一致
        /// </summary>
        private static readonly object pathLock = new object();

        /// <summary>
        /// 间隔存图计数
        /// </summary>
        static int imageCount = -1;

        /// <summary>
        /// 保存原图和截图 并返回截图路径
        /// </summary>
        /// <param name="saveImageConfig"></param>
        /// <param name="cell"></param>
        /// <returns></returns>
        public static string Excute(this CSaveImageConfig saveImageConfig, Cell cell)
        {
            string savePath = string.Empty;
            try
            {
                string classPath;
                string cropPath;
                string cropName;
                saveImageConfig.GetSavePath(cell, out classPath, out cropPath, out cropName);
                //从预处理库里读出来的图片可能经过旋转平移  导致缺陷和图片对不上 ,所以要调用预处理传出来的图
                if (!cell.IsOK || saveImageConfig.OKScreenShot)
                {
                    if (saveImageConfig.PiantScreenEnable)
                    {
                        foreach (CellDetection detection in cell.Detections) //存缺陷小截图
                        {
                            if (detection.Result)
                                continue;
                            string dirPath;
                            if (saveImageConfig.SavebyDefectName)
                            {
                                dirPath =
                                    cropPath
                                    + "\\"
                                    + detection.Type
                                    + "\\"
                                    + detection.DefectFilter.Name
                                    + "\\ErrPart"; //存缺陷截图的文件夹
                            }
                            else
                            {
                                dirPath = cropPath + "\\ErrPart"; //存缺陷截图的文件夹
                            }

                            if (!Directory.Exists(dirPath))
                            {
                                Directory.CreateDirectory(dirPath);
                            }
                            string filecropName = dirPath + cropName + detection.DefectFilter.Name;
                        }

                        savePath = SaveDumpImage(cell, classPath); //存窗口截图 李工还未做
                    }
                }

                if (saveImageConfig.SaveImageEnable) //开启存原图
                {
                    string fileName = classPath;
                    switch (saveImageConfig.SaveSelect)
                    {
                        case "0": //存所有图
                            if (cell.IsOK)
                            {
                                imageCount++;
                                if (imageCount >= saveImageConfig.OkIntervalCount)
                                {
                                    imageCount = -1;
                                    WriteImage(
                                        cell.Image,
                                        fileName,
                                        saveImageConfig.SaveImageFormat
                                    );
                                }
                            }
                            else
                            {
                                WriteImage(cell.Image, fileName, saveImageConfig.SaveImageFormat);
                            }

                            break;

                        case "1": //只存不良图
                            if (!cell.IsOK)
                            {
                                WriteImage(cell.Image, fileName, saveImageConfig.SaveImageFormat);
                            }
                            break;

                        case "2": //只存合格图
                            if (cell.IsOK)
                            {
                                imageCount++;
                                if (imageCount >= saveImageConfig.OkIntervalCount)
                                {
                                    imageCount = -1;
                                    WriteImage(
                                        cell.Image,
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
        /// <returns>保存路径</returns>
        private static string SaveDumpImage(Cell cell, string dumpImagePath)
        {
            string directory = Path.GetDirectoryName(dumpImagePath);
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(dumpImagePath);
            string Dirpath = $"{directory}{"\\Jpg\\"}";
            string path = $"{Dirpath}{fileNameWithoutExtension}{".jpg"}";
            //保存截图。。。
            return path;
        }

        /// <summary>
        /// 获取存图路径
        /// </summary>
        /// <param name="cell">cell</param>
        /// <param name="classPath">存图路径</param>
        /// <param name="cropPath">存图文件夹路径</param>
        /// <param name="cropName">截图名称</param>
        public static void GetSavePath(
            this CSaveImageConfig saveImageConfig,
            Cell cell,
            out string classPath,
            out string cropPath,
            out string cropName
        )
        {
            lock (pathLock)
            {
                try
                {
                    // 图片命名:时间-流水ID-质量等级-缺陷名-处理时间

                    cropPath = "";
                    cropName = "";
                    string filename = "";
                    nameBuilder.Clear();
                    nameBuilder.Append("\\");

                    nameBuilder.Append(string.Format("{0:HHmmssfff}", cell.CreateTime)); //时间
                    nameBuilder.Append("-");

                    nameBuilder.Append(cell.ID); //ID号
                    nameBuilder.Append("-");

                    cropName = nameBuilder.ToString();

                    nameBuilder.Append(cell.Quality?.Signal); //质量信号值
                    nameBuilder.Append("-");

                    nameBuilder.Append(cell.Quality?.Name); //质量等级名称
                    nameBuilder.Append("-");

                    nameBuilder.Append(cell.Detection?.DefectFilter?.Name); //缺陷名称
                    nameBuilder.Append("-");

                    nameBuilder.Append(cell.ProcessTime.TotalMilliseconds.ToString("F0")); //耗时
                    nameBuilder.Append(saveImageConfig.SaveImageFormat); //格式
                    filename = nameBuilder.ToString();

                    //if (!SystemStatic._isRuning)
                    //{
                    //    string oldfilename = Path.GetFileNameWithoutExtension(cell.ImageFile);

                    //    if (oldfilename != null)
                    //    {
                    //        filename = "\\" + oldfilename + ".tiff";
                    //        cropName = "\\" + oldfilename.Split('-')[0] + "-";
                    //    }
                    //}

                    classPath = saveImageConfig.SaveImagePath;
                    if (saveImageConfig.SavebyProjName)
                    {
                        classPath = classPath + "\\" + cell.ProjName;
                    }
                    classPath = classPath + "\\" + "白班"; //CSystemParamJson.SystemSetParam.NowShift;
                    if (saveImageConfig.SavebyCamName)
                    {
                        if (!string.IsNullOrEmpty(cell.CamName))
                        {
                            classPath = classPath + "\\" + cell.CamName;
                        }
                    }

                    if (saveImageConfig.SavebyHour)
                    {
                        string hourNow = cell.CreateTime.Hour.ToString("D2");
                        classPath = classPath + "\\" + hourNow;
                    }
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
                                string detectionName =
                                    cell.Detection.Type + "\\" + cell.Detection.DefectFilter.Name;
                                classPath = classPath + "\\" + detectionName;
                            }
                        }
                    }
                    if (saveImageConfig.SavebyID)
                    {
                        classPath = classPath + "\\" + cell.ID;
                        cropPath = classPath;
                    }

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
        /// 保存图片
        /// </summary>
        /// <param name="bitImage">图片</param>
        /// <param name="filepath">存图路径</param>
        private static void WriteImage(MemoryStream msImage, string filepath, string format)
        {
            if (msImage != null)
            {
                using (FileStream stream = new FileStream(filepath, FileMode.Create))
                {
                    BitmapEncoder encoder = GetEncoder(format);
                    encoder.Frames.Add(BitmapFrame.Create(msImage));
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
    }
}
