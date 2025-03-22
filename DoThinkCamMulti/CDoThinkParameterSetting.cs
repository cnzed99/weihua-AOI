using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DVPCameraType;
using CameraModule;

namespace DoThinkCamMulti
{


    public class CDoThinkParameterSetting : CCameraParameterBase
    {

        public CDoThinkParameterSetting(string serialnumber, string cameraSupplier) : base(serialnumber, cameraSupplier)
        {

        }

        private dvpTriggerSource _triggersource = dvpTriggerSource.TRIGGER_SOURCE_SOFTWARE;
        [Category("度申相机专用")]
        [DisplayName("1.触发源")]
        [Description("触发源")]
        [Browsable(true)]
        public dvpTriggerSource TriggerSource
        {
            get { return _triggersource; }
            set
            {
                _triggersource = value;
                //if (CCameraManagement.CameraDict.ContainsKey(SerialNumber))
                //{
                //    try
                //    {
                //        CCameraManagement.CameraDict[SerialNumber].SetTriggerSource(value);
                //    }
                //    catch (Exception)
                //    {
                //    }

                //}

            }
        }

        private dvpLine _linesselect = dvpLine.LINE_1;
        [Category("度申相机专用")]
        [DisplayName("2.输入输出引脚")]
        [Description("设置输入输出引脚")]
        [Browsable(true)]
        public dvpLine LineSelect
        {
            get { return _linesselect; }
            set
            {
                _linesselect = value;
                //if (CCameraManagement.CameraDict.ContainsKey(SerialNumber))
                //{

                //    try
                //    {
                //        CCameraManagement.CameraDict[SerialNumber].SetLineSelector(value);
                //    }
                //    catch (Exception)
                //    {
                //    }
                //}


            }
        }

        private dvpLineMode _linemode = dvpLineMode.LINE_MODE_INPUT;
        [Category("度申相机专用")]
        [DisplayName("3.选择输入输出")]
        [Description("设置选择输入输出模式")]
        [Browsable(true)]
        public dvpLineMode LineMode
        {
            get { return _linemode; }
            set
            {
                _linemode = value;
                //if (CCameraManagement.CameraDict.ContainsKey(SerialNumber))
                //{
                //    try
                //    {
                //        CCameraManagement.CameraDict[SerialNumber].SetLineMode(value);
                //    }
                //    catch (Exception)
                //    {
                //    }

                //}

            }
        }


        private dvpLineSource _linesource = dvpLineSource.OUTPUT_SOURCE_STROBE;
        [Category("度申相机专用")]
        [DisplayName("4.引脚触发模式")]
        [Description("引脚触发模式")]
        [Browsable(true)]
        public dvpLineSource LineSource
        {
            get { return _linesource; }
            set
            {
                _linesource = value;
                //if (CCameraManagement.CameraDict.ContainsKey(SerialNumber))
                //{

                //    try
                //    {
                //        CCameraManagement.CameraDict[SerialNumber].SetLineSource(value);
                //    }
                //    catch (Exception)
                //    {
                //    }
                //}

            }
        }

    }
}
