using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Autofac;
using CameraModule;
using CommunicationModule;
using HandyControl.Controls;
using Microsoft.Win32;
using WH.DetectSystem.Models;
using WH.DetectSystem.ViewModels;
using WH.Entity.CommonLib;
using WH.Entity.LogRecord;
using WH.RunCell;

namespace 断面毛刺检测软件.Views
{
    /// <summary>
    /// OffLineTestCtrl.xaml 的交互逻辑
    /// </summary>
    public partial class OffLineTestCtrl
        : System.Windows.Controls.UserControl,
            INotifyPropertyChanged
    {
        private CMainModel mainVM;

        /// <summary>
        /// 2024.9.6 李焕彬
        /// 制程
        /// </summary>
        public CMainModel MMainVM
        {
            get => mainVM;
            set
            {
                mainVM = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// 2024.9.2 李焕彬
        /// 工程视图模型
        /// </summary>
        CMainModelsModelVM mainModelsModelVM;

        CLogRec sysLog;

        /// <summary>
        /// 取消令牌
        /// </summary>
        public CancellationTokenSource CancelToken { get; set; } = new CancellationTokenSource();

        /// <summary>
        /// 图片文件筛选
        /// </summary>
        private readonly Regex imgRegex = new Regex(
            "(.*?)(jpg|png|bmp|tiff|tif)$",
            RegexOptions.IgnoreCase
        );

        /// <summary>
        /// 信号锁，调试时使用。
        /// </summary>
        private AutoResetEvent waitSignal;
        OpenFileDialog imgFileDialog = new OpenFileDialog();
#if NET8_0_OR_GREATER
        OpenFolderDialog imgFolderDialog = new OpenFolderDialog();
#else
        System.Windows.Forms.FolderBrowserDialog imgFolderDialog =
            new System.Windows.Forms.FolderBrowserDialog();
#endif
        IEnumerator<CKnownColor> brushes = new CBrushPro().KnownColors.GetEnumerator();
        Random random = new Random(50);

        public OffLineTestCtrl(CMainModel vm)
        {
            InitializeComponent();
            mainModelsModelVM = App.Container.Resolve<CMainModelsModelVM>();
            MMainVM = vm;
            sysLog = vm.SysLog;
            waitSignal = MMainVM.WaitSignal;
            if (MMainVM.TestImgFiles.Count == 0)
            {
                MMainVM.TestImgFiles = ImgFiles;
            }
            else
            {
                ImgFiles = MMainVM.TestImgFiles;
            }
            //
            // ImgFileDialog
            //
            this.imgFileDialog.Filter =
                "图像文件|*.*|png|*.png|jpg|*.jpg|bmp|*.bmp|jpeg|*.jpeg|tiff|*.tiff";
            this.imgFileDialog.Multiselect = true;
            this.imgFileDialog.Title = "选择图像文件";
            //
            // ImgFolderDialog
            //
#if NET8_0_OR_GREATER
            this.imgFolderDialog.Title = "选择图像文件夹";
            this.imgFolderDialog.DefaultDirectory = "D:\\";
#else

            this.imgFolderDialog.SelectedPath = "D:\\";
#endif
            this.DataContext = this;
            //cbAngle.SelectedIndex = cbAngle.Items.IndexOf(_projConfig.ProcessSet.Angle.ToString());
        }

        /// <summary>
        /// 当前图像序号
        /// </summary>
        private int imgIndex = -110;

        /// <summary>
        /// 当前图像序号
        /// </summary>
        public int ImgIndex
        {
            get => imgIndex;
            set
            {
                if (imgIndex != -110)
                {
                    imgIndex = value;
                    if (imgIndex >= imgFiles.Count)
                    {
                        imgIndex = 0;
                    }
                    else if (imgIndex <= -1)
                    {
                        imgIndex = imgFiles.Count - 1;
                    }
                    OnPropertyChanged();
                }
            }
        }

        public bool Stop;

        /// <summary>
        /// 图像文件列表
        /// </summary>
        private List<string> imgFiles = new List<string>();

        /// <summary>
        /// 图像文件列表
        /// </summary>
        private List<string> ImgFiles
        {
            get => imgFiles;
            set
            {
                if (value.Count > 0)
                {
                    imgFiles = value;
                    MMainVM.TestImgFiles = value;
                    ImgNames.Clear();
                    string[] fileNames = Array.ConvertAll<string, string>(
                        imgFiles.ToArray(),
                        System.IO.Path.GetFileNameWithoutExtension
                    );

                    imgIndex = 0;
                    ImgNames = new List<string>(fileNames);
                    OnPropertyChanged();
                }
                else
                {
                    imgIndex = -110;
                }
            }
        }
        private List<string> imgNames = new List<string>();
        public List<string> ImgNames
        {
            get => imgNames;
            set
            {
                imgNames = value;
                OnPropertyChanged();
            }
        }

        private void BtnPreImg_Click(object sender, RoutedEventArgs e)
        {
            // CancelToken = new CancellationTokenSource();
            ImgIndex--;
        }

        private void BtnNextImg_Click(object sender, RoutedEventArgs e)
        {
            // CancelToken = new CancellationTokenSource();
            ImgIndex++;
        }

        private async void BtnStartOnce_Click(object sender, RoutedEventArgs e)
        {
            DisableButtons();
            Stop = false;
            waitSignal.Set();

            CancelToken = new CancellationTokenSource();
            Task task = Task.Run(
                () =>
                {
                    imgIndex = -1;
                    while (!CancelToken.IsCancellationRequested)
                    {
                        if (Stop)
                        {
                            break;
                        }
                        waitSignal.WaitOne();
                        imgIndex++;
                        if (imgIndex >= ImgFiles.Count)
                        {
                            imgIndex = 0;
                            break; //退出
                        }
                        //this.BeginInvoke(new Action(() =>
                        //{
                        //    this.AddLogToListBox($"单次遍历:{ImgIndex + 1}/{ImgFiles.Count}：" + _imgFiles[ImgIndex]);
                        //}));

                        PreDllExcute();
                    }
                },
                CancelToken.Token
            );
            await task;
            EnableButtons();
            task.Dispose();
        }

        private async void BtnStartCircle_Click(object sender, RoutedEventArgs e)
        {
            mainModelsModelVM.DeviceSeting = false;
            DisableButtons();
            Stop = false;
            waitSignal.Set();
            CancelToken = new CancellationTokenSource();
            await Task.Run(
                    () =>
                    {
                        imgIndex = -1;
                        int numTimes = 0;
                        while (!CancelToken.IsCancellationRequested)
                        {
                            if (Stop)
                            {
                                break;
                            }
                            waitSignal.WaitOne();
                            imgIndex++;

                            if (imgIndex >= ImgFiles.Count - 1)
                            {
                                if (imgIndex == ImgFiles.Count)
                                {
                                    imgIndex = 0;
                                }

                                numTimes++;
                                var times = numTimes;
                                this.Dispatcher.Invoke(
                                    new Action(() =>
                                    {
                                        if (imgIndex > 0)
                                        {
                                            sysLog.Info(
                                                $"循环遍历了{times}次:{ImgIndex + 1}/{ImgFiles.Count}："
                                                    + imgFiles[ImgIndex]
                                            );
                                        }
                                    })
                                );
                            }

                            PreDllExcute();
                        }
                    },
                    CancelToken.Token
                )
                .ContinueWith(t =>
                {
                    sysLog.Info(@"循环遍历已取消！");
                });
            EnableButtons();
        }

        /// <summary>
        /// 自动根据 ImgIndex 及预处理库选择项进行读图处理
        /// 另开线程 调试线程
        /// </summary>
        private async void PreDllExcute(bool once = false)
        {
            if (imgFiles.Count > ImgIndex && File.Exists(imgFiles[ImgIndex]))
            {
                try
                {
                    Cell cell = new Cell()
                    {
                        ID = (mainVM.MaociDefectsProduce.Total + 1).ToString(),
                        isOnce = once,
                        Quality = mainVM.MaociQualityConfig.Qualities[0],
                        ImageFile = ImgFiles[ImgIndex],
                        CancelSource = this.CancelToken,
                        ProjGuid = MMainVM.GUID,
                        CamSerial = MMainVM.CameraSerial,
                        ComGuid = "com"
                    };
                    if (
                        !string.IsNullOrEmpty(cell.CamSerial)
                        && CCameraManagement.CamParamDict.ContainsKey(cell.CamSerial)
                    )
                    {
                        cell.CamName = CCameraManagement.CamParamDict[cell.CamSerial].Name;
                        cell.MmPerPixel = CCameraManagement.CamParamDict[cell.CamSerial].MmPerPixel;
                    }
                    if (random.Next(10) > 5)
                        cell.IsOK = true;
                    //  _infoLog.Enqueue($"{$"[{_waitTriggerImageQueue.s_Name}]",-10}{cell.ID,-8}{"离线触发",-20}");
                    // _waitTriggerImageQueue.Enqueue(cell);
                    cell.GetImageExcute(true, 0);

                    //await CCameraBase.waitGetImageChannel.Writer.WriteAsync(cell);
                    await MMainVM.m_WaitImgChannel.Writer.WriteAsync(cell);
                }
                catch (TaskCanceledException ex)
                {
                    sysLog.Info("任务被取消！" + ex.Message);
                }
                catch (Exception e)
                {
                    sysLog.Error("读取图片发生错误:" + e.Message);
                }
            }
        }

        private void BtnStopOffLine_Click(object sender, RoutedEventArgs e)
        {
            Stop = true;
            EnableButtons();
            // DetectProgress.Value = 0;
        }

        #region 按钮使能

        private void DisableButtons()
        {
            //this.BtnStartCircle.Enabled = false;
            this.BtnStartCircle.IsEnabled = false;
            this.BtnStartOnce.IsEnabled = false;
            this.BtnPreImg.IsEnabled = false;
            this.BtnNextImg.IsEnabled = false;
            this.cb_CurImgFile.IsEnabled = false;
            this.btn_ImgDir.IsEnabled = false;
            this.btn_ImgFiles.IsEnabled = false;
            this.btn_FromCam.IsEnabled = false;
            this.cb_RoAngle.IsEnabled = false;
        }

        private void EnableButtons()
        {
            //this.BtnStartCircle.Enabled = true;
            this.BtnStartCircle.IsEnabled = true;
            this.BtnStartOnce.IsEnabled = true;
            this.BtnPreImg.IsEnabled = true;
            this.BtnNextImg.IsEnabled = true;
            this.cb_CurImgFile.IsEnabled = true;
            this.btn_ImgDir.IsEnabled = true;
            this.btn_ImgFiles.IsEnabled = true;
            this.btn_FromCam.IsEnabled = true;
            this.cb_RoAngle.IsEnabled = true;
        }

        #endregion 按钮使能

        private void btn_ImgFiles_Click(object sender, RoutedEventArgs e)
        {
            if (imgFileDialog.ShowDialog() == true)
            {
                List<string> imgs = new List<string>();
                foreach (var file in imgFileDialog.FileNames)
                {
                    if (imgRegex.IsMatch(file))
                    {
                        imgs.Add(file);
                    }
                }
                if (imgs.Count > 0)
                {
                    ImgFiles.Clear();
                    ImgFiles = imgs;
                    ImgFiles.TrimExcess();

                    StringBuilder files = new StringBuilder();
                    foreach (var file in ImgFiles)
                    {
                        files.AppendLine(file);
                    }
                    //  ZzMessageBox.Show(files.ToString());
                }
                else
                {
                    sysLog.Info("没有有效图像文件 jpg|png|bmp|tiff|tif");
                }
            }
        }

        private void btn_ImgDir_Click(object sender, RoutedEventArgs e)
        {
#if NET8_0_OR_GREATER
            if (imgFolderDialog.ShowDialog() is true)
            {
                string[] Allfiles = Directory.GetFiles(imgFolderDialog.FolderName);
#else
            if (imgFolderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string[] Allfiles = Directory.GetFiles(imgFolderDialog.SelectedPath);
#endif
                List<string> imgs = new List<string>();
                foreach (var file in Allfiles)
                {
                    if (imgRegex.IsMatch(file))
                    {
                        imgs.Add(file);
                    }
                }
                if (imgs.Count > 0)
                {
                    ImgFiles.Clear();
                    ImgFiles = imgs;
                    ImgFiles.TrimExcess();
                    StringBuilder files = new StringBuilder();
                    foreach (var file in ImgFiles)
                    {
                        files.AppendLine(file);
                    }
                    //MessageBox.Show(files.ToString());
                }
                else
                {
                    sysLog.Info("没有有效图像文件 " + imgRegex.ToString());
                }
            }
        }

        private void cb_CurImgFile_SelectedIndexChanged(object sender, SelectionChangedEventArgs e)
        {
            DisableButtons();
            CancelToken = new CancellationTokenSource();

            if (File.Exists(imgFiles[ImgIndex]))
            {
                sysLog.Info("正在预处理：" + imgFiles[ImgIndex]);
                PreDllExcute(true);
            }
            else
            {
                sysLog.Info("图像不存在！");
            }
            EnableButtons();
        }

        private void btn_FromCam_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                CCameraManagement.CameraDict[MMainVM.CameraSerial].ExecuteSoftwareTrigger();
                //CCommunicationManagement.GetComFromCam(_projConfig.CamSerial, out string com);
                //int num = _projConfig.Config.LevelProduce.TotalNum + 1;
                //byte[] data = CCommunicationManagement.GetAutoID(_projConfig.CamSerial, num);
                //SystemStatic._isRuning = true;
                //await CCommunicationBase.DataChannel.Writer.WriteAsync(((_projConfig.CamSerial, _projConfig.CamSplitGroup), com, data));
                //Thread.Sleep(10);
                //SystemStatic._isRuning = false;
            }
            catch (Exception ex)
            {
                //SystemStatic._isRuning = false;
                sysLog.Error($"发生错误:{ex.Message}");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
            if (propertyChanged == null)
                return;
            propertyChanged((object)this, new PropertyChangedEventArgs(propertyName));
        }

        private void BtnRunAgain_Click(object sender, RoutedEventArgs e)
        {
            if (imgFiles.Count == 0)
            {
                Growl.Error("图像不存在！请选择图像！");
                return;
            }
            cb_CurImgFile_SelectedIndexChanged(null, null);
        }
    }
}
