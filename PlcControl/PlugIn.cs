using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Motion;
using Newtonsoft.Json;

namespace PlcControl
{
    public class PlugIn : IMotion
    {
        /// <summary>
        /// 2025.3.6 李焕彬
        /// 创建新控制
        /// </summary>
        public CMotionVMBase CreateNewMotion()
        {
            CMotionVMBase motioVM = new CMotionCtrlVM();
            return motioVM;
        }
    }
}
