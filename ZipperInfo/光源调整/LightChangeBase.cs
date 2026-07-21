using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZipperInfo
{
    public  class LightChangeBase
    {
        public string PortName {  get; set; }

        public  int MaxTimeOutCount {  get; set; }
        public  int  MinTimeOutCount { get; set; }

        public int TempLightValue_Change1 { get; set; } = 30;
        public virtual void ChangeLineValue1(bool saveTemp,int lightValue) { }
        public virtual void ChangeLineValue2(int lightValue) { }
        public virtual void LineValueReset() { }


    }
}
