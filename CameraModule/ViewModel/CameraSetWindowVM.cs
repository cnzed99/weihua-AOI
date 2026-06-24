using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HandyControl.Controls;
using WH.RecipeCellRootBase;
using WH.RunCell;

namespace CameraModule
{
    /// <summary>
    /// 2024.7.22 李焕彬
    /// VM
    /// </summary>
    public partial class CCameraSetWindowVM : ObservableObject
    {
        public CCameraSetWindowVM()
        {
            timer.Elapsed += Timer_Elapsed;
            foreach (var item in CCameraManagement.CamParamDict)
            {
                CamParamList.Add(item.Value);
            }
            if (CamParamList.Count > 0)
            {
                CamParamSelect = CamParamList[0];
            }
        }

        /// <summary>
        /// 2024.7.22 李焕彬
        /// 定时器，触发拍照
        /// </summary>
        private System.Timers.Timer timer = new(250);

        /// <summary>
        /// 2024.7.22 李焕彬
        /// 选中相机操作对象
        /// </summary>
        private CCameraBase camSelect;

        /// <summary>
        /// 2024.7.22 李焕彬
        /// 选中相机参数
        /// </summary>
        private CCameraParameterBase camParamSelect;

        /// <summary>
        /// 2024.7.22 李焕彬
        /// 当前选中相机
        /// </summary>
        public CCameraParameterBase CamParamSelect
        {
            get { return camParamSelect; }
            set
            {
                SetProperty(ref camParamSelect, value);
                if (camParamSelect != null)
                {
                    foreach (var item in CCameraManagement.CameraDict)
                    {
                        item.Value.IsSetWindowShowed = false;
                    }
                    camSelect = CCameraManagement.CameraDict[camParamSelect.SerialNumber];
                    camSelect.IsSetWindowShowed = true;
                    camSelect.GrabFinishEvent = ShowImage;
                }
                else
                {
                    foreach (var item in CCameraManagement.CameraDict)
                    {
                        item.Value.IsSetWindowShowed = false;
                    }
                    camSelect.IsSetWindowShowed = false;
                    camSelect.GrabFinishEvent -= ShowImage;
                    camSelect = null;
                }
            }
        }

        /// <summary>
        /// 2024.7.22 李焕彬
        /// 相机集合
        /// </summary>
        [ObservableProperty]
        private ObservableCollection<CCameraParameterBase> camParamList = new();

        /// <summary>
        /// 2024.7.22 李焕彬
        /// 是否连续拍照
        /// </summary>
        [ObservableProperty]
        private bool isContinuous = false;

        /// <summary>
        /// 2024.7.22 李焕彬
        /// 显示图像
        /// </summary>
        [ObservableProperty]
        private BitmapSource imageShow;

        /// <summary>
        /// 2024.7.22 李焕彬
        /// 接收cell
        /// </summary>
        private Cell cellRecv;

        /// <summary>
        /// 2024.7.22 李焕彬
        /// 当前图片清晰度值
        /// </summary>
        [ObservableProperty]
        private float distinct;

        /// <summary>
        /// 2024.7.22 李焕彬
        /// 关闭窗口
        /// </summary>
        [RelayCommand]
        public void Close()
        {
            foreach (var item in CamParamList)
            {
                CCameraManagement.CameraDict[item.SerialNumber].IsSetWindowShowed = false;
            }
            if (camSelect != null)
            {
                //camSelect.IsSetWindowShowed = false;
                camSelect.GrabFinishEvent -= ShowImage;
                camSelect = null;
            }
        }

        /// <summary>
        /// 2024.7.22 李焕彬
        /// 打开搜索相机列表
        /// </summary>
        [RelayCommand]
        public void Search()
        {
            CameraListWindow cameraListWindow = new CameraListWindow();
            cameraListWindow.ShowDialog();

            foreach (var item in CCameraManagement.CamParamDict)
            {
                if (!CamParamList.Contains(item.Value))
                {
                    CamParamList.Add(item.Value);
                }
            }
            for (int i = CamParamList.Count - 1; i >= 0; i--)
            {
                if (!CCameraManagement.CamParamDict.ContainsKey(CamParamList[i].SerialNumber))
                {
                    CamParamList.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// 2024.7.22 李焕彬
        /// 软触发
        /// </summary>
        [RelayCommand]
        public void SoftWareTrigger()
        {
            if (camSelect == null)
                return;
            if (camSelect?.Setting.TriggerMode == EMTRIGGERMODE.EMTRIGGERSOFTWARE)
            {
                camSelect?.ExecuteSoftwareTrigger();
            }
            else
            {
                Growl.WarningGlobal(Properties.Resources.软触发失败);
            }
        }

        /// <summary>
        /// 2024.7.22 李焕彬
        /// 连续触发
        /// </summary>
        [RelayCommand]
        public void Continuous()
        {
            if (camSelect == null)
                return;
            if (IsContinuous)
            {
                timer.Enabled = false;
            }
            else
            {
                if (camSelect?.Setting.TriggerMode != EMTRIGGERMODE.EMTRIGGERSOFTWARE)
                {
                    Growl.WarningGlobal(Properties.Resources.软触发失败);
                    return;
                }
                timer.Enabled = true;
            }
            IsContinuous = !IsContinuous;
        }

        /// <summary>
        /// 2024.7.22 李焕彬
        /// 删除相机
        /// </summary>
        [RelayCommand]
        public void DelCam()
        {
            if (CamParamSelect != null)
            {
                if (CCameraManagement.CameraDict.ContainsKey(CamParamSelect.SerialNumber))
                {
                    try
                    {
                        CCameraManagement.OperateLog.Info(
                            $"{Properties.Resources.InfoDelCam}" + CamParamSelect.SerialNumber
                        );
                        CCameraManagement.CameraDict[CamParamSelect.SerialNumber].EndCamera();
                        CCameraManagement.CameraDict.Remove(CamParamSelect.SerialNumber);
                        CCameraManagement.CamParamDict.Remove(CamParamSelect.SerialNumber);
                        CamParamList.Remove(CamParamSelect);
                    }
                    catch (Exception ex)
                    {
                        CCameraManagement.CamLogger.Error(
                            $"{Properties.Resources.ErrorCloseCam}-{CamParamSelect.SerialNumber}"
                                + ex.Message
                        );
                    }
                }
            }
        }

        /// <summary>
        /// 2024.7.22 李焕彬
        /// 保存相机参数
        /// </summary>
        [RelayCommand]
        public void Save()
        {
            CCameraManagement.SaveAllCamConfig();
        }

        /// <summary>
        /// 2024.7.22 李焕彬
        /// 定时触发
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            camSelect?.ExecuteSoftwareTrigger();
        }

        object lockObject = new object();

        /// <summary>
        /// 2024.7.22 李焕彬
        /// 显示图像
        /// </summary>
        /// <param name="cell">cell</param>
        public void ShowImage(Cell cell)
        {
            lock (lockObject)
            {
                CImage image = cell.Image;
                if (image != null)
                {
                    try
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            ImageShow = image.ToBitmapSource();
                            cell.Dispose();
                        });
                        //if (camSelect?.FuncDistinct != null)
                        //{
                        //    Distinct = camSelect.FuncDistinct(image);
                        //}
                        //else
                        //{
                        //    Distinct = 0;
                        //}
                       // cellRecv?.Dispose();
                        //cellRecv = cell;
                    }
                    catch (Exception ex)
                    {
                        CCameraManagement.CamLogger.Error(
                            Properties.Resources.ErrorShowImage + ex.Message
                        );
                    }
                }
            }
        }
    }
}
