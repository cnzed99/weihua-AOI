using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Autofac;
using CameraModule;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Xaml.Behaviors.Core;
using WH.DetectSystem.ViewModels;

namespace 断面毛刺检测软件.Views
{
    public partial class TimeCamTriggerVMs : ObservableObject
    {
        [ObservableProperty]
        ObservableCollection<TimerCamTriggerVM> timers = new();

        public TimeCamTriggerVMs()
        {
            foreach (var mainModel in App.Container.Resolve<CMainModelsModelVM>().CMainVMs)
            {
                if (
                    !string.IsNullOrEmpty(mainModel.CameraSerial)
                    && CCameraManagement.CameraDict.ContainsKey(mainModel.CameraSerial)
                )
                {
                    TimerCamTriggerVM triggerVM = new TimerCamTriggerVM(
                        CCameraManagement.CameraDict[mainModel.CameraSerial],
                        mainModel.Name
                    );
                    Timers.Add(triggerVM);
                }
            }
        }
    }

    public partial class TimerCamTriggerVM : ObservableObject
    {
        public string Name { get; internal set; }

        [ObservableProperty]
        bool isTriggerStart;

        partial void OnIsTriggerStartChanged(bool value)
        {
            if (IsTriggerStart)
            {
                TriggerTimer.Start();
            }
            else
            {
                TriggerTimer.Stop();
            }
        }

        [ObservableProperty]
        bool softTriggerEnable;
        partial void OnSoftTriggerEnableChanged(bool value)
        {
            if (value)
            {
                cam.Setting.TriggerMode = EMTRIGGERMODE.EMTRIGGERSOFTWARE;
                Cam.SetTriggerModePro(EMTRIGGERMODE.EMTRIGGERSOFTWARE);
            }
            else
            {
                cam.Setting.TriggerMode = EMTRIGGERMODE.EMTRIGGERHARDWARE;
                Cam.SetTriggerModePro(EMTRIGGERMODE.EMTRIGGERHARDWARE);
            }
        }

        [ObservableProperty]
        CCameraBase cam;

        [ObservableProperty]
        DispatcherTimer triggerTimer;

        [ObservableProperty]
        [property: MinLength(3)]
        int interval = 50;

        private uint exposureValue;

        public uint ExposureValue
        {
            get 
            {
                //return exposureValue; 
                if (Cam != null && cam.Connected)
                {
                     cam.GetExposureTime(out uint ex);
                    return ex;
                }
                else { return 0; }
            }
            set 
            {
                exposureValue = value;
                if (Cam!=null&&cam.Connected)
                {
                    cam.SetExposureTime(exposureValue);
                }
                OnPropertyChanged();
            }
        }

        [RelayCommand]
        void SoftTrigger()
        {
            if (Cam.Connected)
            {
                Cam.ExecuteSoftwareTrigger();
            }
        }

        partial void OnIntervalChanged(int value)
        {
            TriggerTimer.Interval = TimeSpan.FromMilliseconds(value);
        }

        public TimerCamTriggerVM(CCameraBase cam, string name)
        {
            Cam = cam;
            TriggerTimer = new DispatcherTimer();
            TriggerTimer.Interval = TimeSpan.FromMilliseconds(Interval);
            TriggerTimer.Tick += TimerCallBack;
            Name = name;
        }

        private void TimerCallBack(object sender, EventArgs e)
        {
            if (Cam.Connected)
            {
                Cam.ExecuteSoftwareTrigger();
            }
        }
    }
}
