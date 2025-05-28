using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZipperInfo
{
    public class ZipperInfoVM
    {
        public CZipperInfo ZipperInfo  { get; set; }
        public ZipperInfoVM() 
        {
            ZipperInfo=new CZipperInfo();
        }
    }
}
