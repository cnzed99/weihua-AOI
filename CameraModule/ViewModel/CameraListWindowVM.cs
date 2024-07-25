using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WH.Entity;

namespace CameraModule
{
    /// <summary>
    /// 2024.7.22 李焕彬
    /// VM
    /// </summary>
    public partial class CCameraListWindowVM : ObservableObject
    {
        public CCameraListWindowVM()
        {
            foreach (var camFunc in CCameraManagement.CameraHelpers)
            {
                try
                {
                    foreach (var item in camFunc.Value.EnumCamrea())
                    {
                        item.IsUse =
                            CCameraManagement.CamParamDict.ContainsKey(item.SerialNumber)
                            && item.Vender
                                == CCameraManagement.CamParamDict[item.SerialNumber].CameraSupplier;
                        CameraInfos.Add(item);
                    }
                }
                catch (Exception ex)
                {
                    CCameraManagement.CamLogger.Error(
                        $"{Properties.Resources.ErrorEnumCam}{camFunc.Key}" + ex.Message
                    );
                }
            }
        }

        /// <summary>
        /// 2024.7.22 李焕彬
        /// Close命令，添加相机
        /// </summary>
        /// <param name="cameraListWindow">窗口对象</param>
        [RelayCommand]
        public void Close()
        {
            foreach (var info in CameraInfos)
            {
                if (info.IsUse)
                {
                    if (!CCameraManagement.CameraDict.ContainsKey(info.SerialNumber))
                    {
                        CCameraManagement.CamLogger.Info(
                            $"{Properties.Resources.InfoAddCam}{info.Vender}" + info.SerialNumber
                        );
                        var _par = CCameraManagement
                            .CameraHelpers[info.Vender]
                            .CreatNewCam(info.SerialNumber, out CCameraBase camera);
                        CCameraManagement.CameraDict.Add(info.SerialNumber, camera);
                        CCameraManagement.CamParamDict.Add(info.SerialNumber, _par);
                        try
                        {
                            if (!CCameraManagement.CameraDict[info.SerialNumber].Connected)
                            {
                                CCameraManagement.CameraDict[info.SerialNumber].InitializeCamera();
                            }
                        }
                        catch (Exception ex)
                        {
                            CCameraManagement.CamLogger.Error(
                                $"{Properties.Resources.ErrorInit}{info.Vender}-{info.SerialNumber}"
                                    + ex.Message
                            );
                        }
                    }
                }
                else
                {
                    if (CCameraManagement.CameraDict.ContainsKey(info.SerialNumber))
                    {
                        try
                        {
                            CCameraManagement.CameraDict[info.SerialNumber].EndCamera();
                            CCameraManagement.CameraDict.Remove(info.SerialNumber);
                            CCameraManagement.CamParamDict.Remove(info.SerialNumber);
                        }
                        catch (Exception ex)
                        {
                            CCameraManagement.CamLogger.Error(
                                $"{Properties.Resources.ErrorCloseCam}{info.Vender}-{info.SerialNumber}"
                                    + ex.Message
                            );
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 2024.7.22 李焕彬
        /// 相机集合
        /// </summary>
        [ObservableProperty]
        ObservableCollection<WHCameraInfo> cameraInfos = new();
    }
}
