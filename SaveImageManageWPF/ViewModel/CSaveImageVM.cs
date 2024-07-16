using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using Newtonsoft.Json;
using WH.Entity;
using WH.RunCell;

namespace SaveImageManage
{
    public partial class CSaveImageVM : ObservableValidator
    {
        private StringBuilder nameBuilder = new StringBuilder();

        /// <summary>
        /// 路径锁 防止多线程访问导致路径不一致
        /// </summary>
        private static readonly object pathLock = new object();

        /// <summary>
        /// 存图参数
        /// </summary>
        public CSaveImageConfig Param { get; set; }

        public CSaveImageVM()
        {
            Param = SaveImageManagement.LoadParameter();
        }

        [RelayCommand]
        private void Close(System.ComponentModel.CancelEventArgs e)
        {
            if (!HasErrors)
            {
                SaveImageManagement.SaveParameter(Param);
                // e.Cancel = true;
                //    WeakReferenceMessenger.Default.Send<CloseWindowMessage>(new CloseWindowMessage() { Sender = new WeakReference(this) });
            }
        }

        /// <summary>
        /// 将NG截图
        /// </summary>
        [JsonIgnore]
        public Action<ObservableCollection<string>> TransferPathDelegate { get; set; }

        [RelayCommand]
        void SelectPath()
        {
            try
            {
                var dialog = new OpenFolderDialog();

                if (dialog.ShowDialog() is true)
                {
                    Param.SaveImagePath = dialog.FolderName;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 间隔存图计数
        /// </summary>
        int imageCount = -1;

        /// <summary>
        /// 保存原图和截图
        /// </summary>
        /// <param name="cell">cell</param>
        public void SaveFullImage(Cell cell)
        {
            try
            {
                string classPath;
                string cropPath;
                string cropName;
                GetSavePath(cell, out classPath, out cropPath, out cropName);
                //从预处理库里读出来的图片可能经过旋转平移  导致缺陷和图片对不上 ,所以要调用预处理传出来的图
                if (!cell.IsOK || Param.OKScreenShot)
                {
                    if (Param.PiantScreenEnable)
                    {
                        foreach (CellDetection detection in cell.Detections) //存缺陷小截图
                        {
                            if (detection.Result)
                                continue;
                            string dirPath;
                            if (Param.SavebyDefectName)
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

                        SaveDumpImage(cell, classPath); //存窗口截图
                    }
                }

                if (Param.SaveImageEnable) //开启存原图
                {
                    string fileName = classPath;
                    switch (Param.SaveSelect)
                    {
                        case "0": //存所有图
                            if (cell.IsOK)
                            {
                                imageCount++;
                                if (imageCount >= Param.OkIntervalCount)
                                {
                                    imageCount = -1;
                                    WriteImage(cell.Image, fileName);
                                }
                            }
                            else
                            {
                                WriteImage(cell.Image, fileName);
                            }

                            break;

                        case "1": //只存不良图
                            if (!cell.IsOK)
                            {
                                WriteImage(cell.Image, fileName);
                            }
                            break;

                        case "2": //只存合格图
                            if (cell.IsOK)
                            {
                                imageCount++;
                                if (imageCount >= Param.OkIntervalCount)
                                {
                                    imageCount = -1;
                                    WriteImage(cell.Image, fileName);
                                }
                            }
                            break;
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="cell">cell</param>
        /// <param name="dumpImagePath">NG截图路径</param>
        /// <param name="showAllDefects">是否显示所有缺陷</param>
        private void SaveDumpImage(Cell cell, string dumpImagePath)
        {
            string directory = Path.GetDirectoryName(dumpImagePath);
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(dumpImagePath);
            string Dirpath = $"{directory}{"\\Jpg\\"}";
            string path = $"{Dirpath}{fileNameWithoutExtension}{".jpg"}";

            if (Param.NgImagePaths != null)
            {
                Param.NgImagePaths.Insert(0, path);
                if (Param.NgImagePaths.Count >= 1000)
                {
                    Param.NgImagePaths.RemoveAt(Param.NgImagePaths.Count - 1);
                }
                TransferPathDelegate?.Invoke(Param.NgImagePaths);
            }
        }

        /// <summary>
        /// 获取存图路径
        /// </summary>
        /// <param name="cell">cell</param>
        /// <param name="classPath">存图路径</param>
        /// <param name="cropPath">存图文件夹路径</param>
        /// <param name="cropName">截图名称</param>
        public void GetSavePath(
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

                    nameBuilder.Append(cell.Quality?.QualitySignal); //质量信号值
                    nameBuilder.Append("-");

                    nameBuilder.Append(cell.Quality?.QualityName); //质量等级名称
                    nameBuilder.Append("-");

                    nameBuilder.Append(cell.Detection?.DefectFilter?.Name); //缺陷名称
                    nameBuilder.Append("-");

                    nameBuilder.Append(cell.ProcessTime.TotalMilliseconds.ToString("F0")); //耗时
                    nameBuilder.Append(Param.SaveImageFormat); //格式
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

                    classPath = Param.SaveImagePath;
                    if (Param.SavebyProjName)
                    {
                        classPath = classPath + "\\" + cell.ProjName;
                    }
                    classPath = classPath + "\\" + "白班"; //CSystemParamJson.SystemSetParam.NowShift;
                    if (Param.SavebyCamName)
                    {
                        if (!string.IsNullOrEmpty(cell.CamName))
                        {
                            classPath = classPath + "\\" + cell.CamName;
                        }
                    }

                    if (Param.SavebyHour)
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
                        if (Param.SavebyDefectName)
                        {
                            if (!(cell.Detection is null))
                            {
                                string detectionName =
                                    cell.Detection.Type + "\\" + cell.Detection.DefectFilter.Name;
                                classPath = classPath + "\\" + detectionName;
                            }
                        }
                    }
                    if (Param.SavebyID)
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
        private void WriteImage(MemoryStream msImage, string filepath)
        {
            if (msImage != null)
            {
                using (FileStream stream = new FileStream(filepath, FileMode.Create))
                {
                    BitmapEncoder encoder = GetEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(msImage));
                    encoder.Save(stream);
                }
            }
        }

        /// <summary>
        /// 获取格式
        /// </summary>
        /// <returns></returns>
        private BitmapEncoder GetEncoder()
        {
            BitmapEncoder encoder;
            switch (Param.SaveImageFormat)
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

    /// <summary>
    /// 保存存图参数类
    /// </summary>
    public static class SaveImageManagement
    {
        /// <summary>
        /// 存图参数保存的路径
        /// </summary>
        public static string ParameterPath = "..\\SystemConfig\\SaveImageParam.Json";

        #region 保存参数

        public static void SaveParameter(CSaveImageConfig param)
        {
            try
            {
                ConfigAPI.Save(param, ParameterPath);
            }
            catch (Exception) { }
        }
        #endregion

        #region 读取参数

        public static CSaveImageConfig LoadParameter()
        {
            CSaveImageConfig settingsModel = new CSaveImageConfig();
            try
            {
                if (File.Exists(ParameterPath))
                {
                    settingsModel = ConfigAPI.Load<CSaveImageConfig>(ParameterPath);
                    if (settingsModel == null)
                    {
                        settingsModel = new CSaveImageConfig();
                    }
                }
                else
                {
                    settingsModel = new CSaveImageConfig();
                }
            }
            catch (Exception)
            {
                settingsModel = new CSaveImageConfig();
            }
            return settingsModel;
        }

        #endregion
    }
}
