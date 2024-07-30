using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using Autofac;
using CameraModule;
using CommunityToolkit.Mvvm.ComponentModel;
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
            var mainModel = App.Container.Resolve<CMainModelsModelVM>().CMainVMs[0];
            if (CCameraManagement.CameraDict.ContainsKey(mainModel.CameraSerial))
            {
                TimerCamTriggerVM triggerVM = new TimerCamTriggerVM(
                    CCameraManagement.CameraDict[mainModel.CameraSerial],
                    mainModel.Name
                );
                Timers.Add(triggerVM);
            }
        }
    }

    public partial class TimerCamTriggerVM : ObservableObject
    {
        public string Name { get; internal set; }

        [ObservableProperty]
        bool isTriggerStart;

        [ObservableProperty]
        CCameraBase cam;

        [ObservableProperty]
        DispatcherTimer triggerTimer;

        [ObservableProperty]
        int interval = 50;

        partial void OnIntervalChanged(int value)
        {
            TriggerTimer.Interval = TimeSpan.FromMilliseconds(value);
        }

        public TimerCamTriggerVM(CCameraBase cam, string name)
        {
            Cam = cam;
            TriggerTimer = new DispatcherTimer();
            TriggerTimer.Tick += TimerCallBack;
            TriggerTimer.Start();
            Name = name;
        }

        private void TimerCallBack(object sender, EventArgs e)
        {
            if (IsTriggerStart && Cam.Connected)
            {
                Cam.ExecuteSoftwareTrigger();
            }
        }
    }
}
