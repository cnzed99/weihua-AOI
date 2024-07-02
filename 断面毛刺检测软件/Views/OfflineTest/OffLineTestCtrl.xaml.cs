using Autofac;
using Microsoft.Win32;
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
using WH.Entity.CommonLib;
using WH.Entity.LogRecord;
using WH.RunCell;
using 断面毛刺检测软件.ViewModels;


namespace 断面毛刺检测软件.Views
{
    
    /// <summary>
    /// OffLineTestCtrl.xaml 的交互逻辑
    /// </summary>
    public partial class OffLineTestCtrl : System.Windows.Controls.UserControl,INotifyPropertyChanged
    {

        private MainVM m_mainVM;
        public MainVM MMainVM
        {
            get => m_mainVM;
            set
            {
                m_mainVM = value;
                OnPropertyChanged();
            }
        }

        CLogRec SysLog;
        /// <summary>
        /// 取消令牌
        /// </summary>
        private CancellationTokenSource CancelToken { get; set; } = new CancellationTokenSource();

        /// <summary>
        /// 图片文件筛选
        /// </summary>
        private readonly Regex _imgRegex = new Regex("(.*?)(jpg|png|bmp|tiff|tif)$",RegexOptions.IgnoreCase);

        /// <summary>
        /// 信号锁，调试时使用。
        /// </summary>
        private AutoResetEvent WaitSignal;
        OpenFileDialog ImgFileDialog = new OpenFileDialog();
        OpenFolderDialog ImgFolderDialog = new OpenFolderDialog();

        
        IEnumerator<KnownColor> brushes = new BrushPro().KnownColors.GetEnumerator();
        Random random = new Random(50);
        public OffLineTestCtrl(MainVM vm)
        {
            InitializeComponent();
            MMainVM = vm;
            SysLog = vm.SysLog;
            WaitSignal = vm.WaitSignal;
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
            this.ImgFileDialog.Filter = "图像文件|*.*|png|*.png|jpg|*.jpg|bmp|*.bmp|jpeg|*.jpeg|tiff|*.tiff";
            this.ImgFileDialog.Multiselect = true;
            this.ImgFileDialog.Title = "选择图像文件";
            // 
            // ImgFolderDialog
            // 
            this.ImgFolderDialog.Title = "选择图像文件夹";
            this.ImgFolderDialog.DefaultDirectory = "D:\\";
            this.DataContext = this;
            //cbAngle.SelectedIndex = cbAngle.Items.IndexOf(_projConfig.ProcessSet.Angle.ToString());
        }
       
        /// <summary>
        /// 当前图像序号
        /// </summary>
        private int _imgIndex = -110;

        /// <summary>
        /// 当前图像序号
        /// </summary>
        public int ImgIndex
        {
            get => _imgIndex;
            set
            {
                if (_imgIndex != -110)
                {
                    _imgIndex = value;
                    if (_imgIndex >= _imgFiles.Count)
                    {
                        _imgIndex = 0;
                    }
                    else if (_imgIndex <= -1)
                    {
                        _imgIndex = _imgFiles.Count - 1;
                    }
                    OnPropertyChanged();
                }
            }
        }

        private bool _stop;

        /// <summary>
        /// 离线测试 下一张 按钮按下
        /// </summary>
        private bool _nextPressed = false;

        /// <summary>
        /// 离线测试 上一张 按钮按下
        /// </summary>
        private bool _prePressed = false;

        /// <summary>
        /// 图像文件列表
        /// </summary>
        private List<string> _imgFiles = new List<string>();

        /// <summary>
        /// 图像文件列表
        /// </summary>
        private List<string> ImgFiles
        {
            get => _imgFiles;
            set
            {
                if (value.Count > 0)
                {
                    _imgFiles = value;
                    MMainVM.TestImgFiles = value;
                    ImgNames.Clear();
                    string[] fileNames = Array.ConvertAll<string, string>(_imgFiles.ToArray(), System.IO.Path.GetFileNameWithoutExtension);
                   
                    _imgIndex = 0;
                    ImgNames = new List<string>(fileNames);
                    OnPropertyChanged();
                }
                else
                {
                    _imgIndex = -110;
                }
            }
        }
        private List<string> imgNames = new List<string>();
        public List<string> ImgNames { get=>imgNames; set { imgNames = value;OnPropertyChanged(); } } 

        private void BtnPreImg_Click(object sender, RoutedEventArgs e)
        {
            _prePressed = true;
            _nextPressed = false;
            // CancelToken = new CancellationTokenSource();
            ImgIndex--;
        }

        private void BtnNextImg_Click(object sender, RoutedEventArgs e)
        {
            _prePressed = false;
            _nextPressed = true;
            // CancelToken = new CancellationTokenSource();
            ImgIndex++;
        }

        private async void BtnStartOnce_Click(object sender, RoutedEventArgs e)
        {
            DisableButtons();
            _stop = false;
            WaitSignal.Set();

            CancelToken = new CancellationTokenSource();
            Task task = Task.Run(() =>
            {
                _imgIndex = -1;
                while (!CancelToken.IsCancellationRequested)
                {
                    if (_stop)
                    {
                        break;
                    }
                    WaitSignal.WaitOne();
                    _imgIndex++;
                    if (_imgIndex >= ImgFiles.Count)
                    {
                        _imgIndex = 0;
                        break;//退出
                    }
                    //this.BeginInvoke(new Action(() =>
                    //{
                    //    this.AddLogToListBox($"单次遍历:{ImgIndex + 1}/{ImgFiles.Count}：" + _imgFiles[ImgIndex]);
                    //}));

                    PreDllExcute();
                }
            }, CancelToken.Token);
            await task;
            EnableButtons();
            task.Dispose();
        }

        private async void BtnStartCircle_Click(object sender, RoutedEventArgs e)
        {
            MMainVM.DeviceSeting = false;
            DisableButtons();
            _stop = false;
            WaitSignal.Set();
            CancelToken = new CancellationTokenSource();
            await Task.Run(() =>
            {
                _imgIndex = -1;
                int numTimes = 0;
                while (!CancelToken.IsCancellationRequested)
                {
                    if (_stop)
                    {
                        break;
                    }
                    WaitSignal.WaitOne();
                    _imgIndex++;

                    if (_imgIndex >= ImgFiles.Count - 1)
                    {
                        if (_imgIndex == ImgFiles.Count)
                        {
                            _imgIndex = 0;
                        }

                        numTimes++;
                        var times = numTimes;
                        this.Dispatcher.Invoke(new Action(() =>
                        {
                            if (_imgIndex > 0)
                            {
                                SysLog.Info($"循环遍历了{times}次:{ImgIndex + 1}/{ImgFiles.Count}：" +
                                                               _imgFiles[ImgIndex]);
                            }
                        }));
                    }

                    PreDllExcute();
                }
            }, CancelToken.Token).ContinueWith(t =>
            {
                SysLog.Info(@"循环遍历已取消！");
            });
            EnableButtons();
        }

        /// <summary>
        /// 自动根据 ImgIndex 及预处理库选择项进行读图处理
        /// 另开线程 调试线程
        /// </summary>
        private async void PreDllExcute(bool once = false)
        {
            MMainVM.isStart = false;
            if (_imgFiles.Count > ImgIndex && File.Exists(_imgFiles[ImgIndex]))
            {
                try
                {
                    if (!MMainVM.isStart)
                    {
                        if (!once)//连续离线
                        {
                            Thread.Sleep(30);
                        }
                        else//单张离线
                        {
                        }
                    }
                   
                    if (!brushes.MoveNext())
                    {
                        brushes.Reset();
                        brushes.MoveNext();
                    }
                    Cell cell = new Cell()
                    {
                        ID = "002",
                        isOnce = once,
                        QualityColor = brushes.Current.brush,
                        ImageFile = ImgFiles[ImgIndex],
                        CancelSource = this.CancelToken,
                        ProjGuid = "001",
                        CamSerial = "002",
                        ComGuid = "com"
                    };
                    if (random.Next(10) > 5) cell.IsOK = true;
                    //  _infoLog.Enqueue($"{$"[{_waitTriggerImageQueue.Name}]",-10}{cell.ID,-8}{"离线触发",-20}");
                    // _waitTriggerImageQueue.Enqueue(cell);
                    cell.GetImageExcute(!MMainVM.isStart, 0);

                    //await CCameraBase.WaitGetImageChannel.Writer.WriteAsync(cell);
                    await MainVM.m_WaitImgChannel.Writer.WriteAsync(cell);
                }
                catch (TaskCanceledException ex)
                {

                    SysLog.Info("任务被取消！" + ex.Message);
                }
                catch (Exception e)
                {
                    SysLog.Error("读取图片发生错误:" + e.Message);
                }
            }
        }

        private void BtnStopOffLine_Click(object sender, RoutedEventArgs e)
        {
            _stop = true;
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
            if (ImgFileDialog.ShowDialog() == true)
            {
                List<string> imgs = new List<string>();
                foreach (var file in ImgFileDialog.FileNames)
                {
                    if (_imgRegex.IsMatch(file))
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
                    SysLog.Info("没有有效图像文件 jpg|png|bmp|tiff|tif");
                    
                }
            }
        }
        
        

        private void btn_ImgDir_Click(object sender, RoutedEventArgs e)
        {
            if (ImgFolderDialog.ShowDialog() is true)
            {
                string[] Allfiles = Directory.GetFiles(ImgFolderDialog.FolderName);
                List<string> imgs = new List<string>();
                foreach (var file in Allfiles)
                {
                    if (_imgRegex.IsMatch(file))
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
                    SysLog.Info("没有有效图像文件 " + _imgRegex.ToString());
                    
                }
            }
        }

        private void cb_CurImgFile_SelectedIndexChanged(object sender, SelectionChangedEventArgs e)
        {
            DisableButtons();
            CancelToken = new CancellationTokenSource();
           
            if (File.Exists(_imgFiles[ImgIndex]))
            {
               SysLog.Info("正在预处理：" + _imgFiles[ImgIndex]);
                PreDllExcute(true);
            }
            else
            {
                SysLog.Info("图像不存在！");
            }
            EnableButtons();
        }

        
        private async void btn_FromCam_Click(object sender, RoutedEventArgs e)  
        {
            try
            {
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
                //CLogRec.Error($"发生错误:{ex.Message}");
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

    }
    
}
