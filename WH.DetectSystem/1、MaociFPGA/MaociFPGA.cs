using AlgorithmDll;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using WH.RunCell;

namespace WH.DetectSystem
{
    /// <summary>
    /// 20240704 TCG
    /// PC算法阶段 算法执行控制
    /// </summary>
    public static class CMaociStage
    {
        /// <summary>
        /// 20240704 TCG
        /// 毛刺FPGA算法执行，FPGA参数扩展方法
        /// </summary>
        /// <param name="paramFpga">FPGA参数</param>
        /// <param name="cell">检测对象</param>
        public static void MaociFPGAExcute(this MaociAlgorParamConfig paramMaoci, Cell cell)
        {
            BitmapImage bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.StreamSource = new MemoryStream();
            cell.Image.WriteTo(bitmapImage.StreamSource);
            bitmapImage.EndInit();
            int stride = bitmapImage.PixelWidth * ((bitmapImage.Format.BitsPerPixel + 7) / 8);
            IntPtr ptr = Marshal.AllocHGlobal(bitmapImage.PixelHeight * stride);
            bitmapImage.CopyPixels(new Int32Rect(0, 0, bitmapImage.PixelWidth, bitmapImage.PixelHeight), ptr, bitmapImage.PixelHeight * stride, stride);
            //处理结果放到cell中
            cell.MaociTestOut.DetectFpga(bitmapImage.PixelWidth, bitmapImage.PixelHeight, stride, ptr, paramMaoci.MaociAlgorParamFpgaUse);
            Marshal.FreeHGlobal(ptr);
        }
        /// <summary>
        /// 20240704 TCG
        /// 毛刺PC算法执行，毛刺检测算法扩展方法
        /// </summary>
        /// <param name="paramMaoci">毛刺算法参数</param>
        /// <param name="cell">检测对象</param>
        public static void MaociExcute(this MaociAlgorParamConfig paramMaoci, Cell cell)
        {
            //处理结果放到cell中
            BitmapImage bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.StreamSource = new MemoryStream();
            cell.Image.WriteTo(bitmapImage.StreamSource);
            bitmapImage.EndInit();
            int stride = bitmapImage.PixelWidth * ((bitmapImage.Format.BitsPerPixel + 7) / 8);
            IntPtr ptr = Marshal.AllocHGlobal(bitmapImage.PixelHeight * stride);
            bitmapImage.CopyPixels(new Int32Rect(0, 0, bitmapImage.PixelWidth, bitmapImage.PixelHeight), ptr, bitmapImage.PixelHeight * stride, stride);
            //处理结果放到cell中
            cell.MaociTestOut.DetectImage(bitmapImage.PixelWidth, bitmapImage.PixelHeight, stride, ptr, paramMaoci.MaociAlgorParamUse);
            Marshal.FreeHGlobal(ptr);
        }
    }
}
