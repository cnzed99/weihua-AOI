using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CameraModule;
using CommunityToolkit.Mvvm.ComponentModel;
using MVSDK;
using WH.Controls;
using WH.Entity.Attribute;

namespace MindVisionCam
{
    /// <summary>
    /// 2024.7.23 李焕彬
    /// 相机参数派生类
    /// </summary>
    public partial class CMindVSParameterSetting : CCameraParameterBase
    {
        public CMindVSParameterSetting()
            : base() { }

        public CMindVSParameterSetting(string serialnumber, string cameraSupplier)
            : base(serialnumber, cameraSupplier) { }
    }
}
