using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WH.Entity.DiskSpace
{
    public static class DiskSpace
    {
        /// <summary>
        /// 获取磁盘剩余空间 磁盘不存在时返回0
        /// 单位GＢ
        /// </summary>
        /// <param name="str_HardDiskName"></param>
        /// <returns></returns>

        public static long GetHardDiskSpace(string str_HardDiskName)
        {
            long totalSize = 0;
            if (File.Exists(str_HardDiskName) || Directory.Exists(str_HardDiskName))
            {
                string diskName = Path.GetPathRoot(str_HardDiskName);
                System.IO.DriveInfo[] drives = System.IO.DriveInfo.GetDrives();
                foreach (System.IO.DriveInfo drive in drives)
                {
                    if (drive.Name == diskName)
                    {
                        totalSize = drive.AvailableFreeSpace / (1024 * 1024 * 1024);
                    }
                }
            }
            return totalSize;
        }
    }
}
