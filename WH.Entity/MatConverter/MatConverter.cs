using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using OpenCvSharp.Extensions;
using OpenCvSharp;

namespace WH.Entity.MatConverter
{
    public class MatConverter
    {
        /// <summary>
        /// opencv Mat 类型转成BitmapSource
        /// </summary>
        /// <param name="img"></param>
        /// <returns></returns>
        public static BitmapSource Mat2BitmapSource(Mat img)
        {
            // 1. 检查输入有效性
            if (img == null || img.IsDisposed || img.Empty())
            {
                return null;
            }

            IntPtr hBitmap = IntPtr.Zero; // 声明位图句柄变量，初始化为零
            Bitmap bitmap = null; // 声明 Bitmap 变量

            try
            {
                // 2. 确保 Mat 对象是 8 位无符号整型，这是 Bitmap 的常见要求
                Mat convertedMat = new Mat();
                if (img.Type() != MatType.CV_8UC3 && img.Type() != MatType.CV_8UC1)
                {
                    img.ConvertTo(convertedMat, MatType.CV_8U);
                }
                else
                {
                    convertedMat = img; // 如果已经是所需类型，则直接使用
                }

                // 3. 颜色空间转换：OpenCV 默认 BGR，WPF 需要 RGB
                //Mat colorMat = new Mat();
                //if (convertedMat.Channels() == 3)
                //{
                //    Cv2.CvtColor(convertedMat, colorMat, ColorConversionCodes.BGR2RGB);
                //}
                //else if (convertedMat.Channels() == 1)
                //{
                //  Cv2.CvtColor(convertedMat, colorMat, ColorConversionCodes.GRAY2RGB);
                //}
                //else
                //{
                //    // 处理其他通道数的情况，例如转换为3通道
                //    Cv2.CvtColor(convertedMat, colorMat, ColorConversionCodes.BGRA2RGB); // 假设是4通道
                //}

                // 4. 转换为 System.Drawing.Bitmap
                bitmap = convertedMat.ToBitmap();

                // 5. 获取位图的句柄 (这是一个非托管资源)
                hBitmap = bitmap.GetHbitmap();

                // 6. 创建 BitmapSource
                BitmapSource bitmapSource = Imaging.CreateBitmapSourceFromHBitmap(
                    hBitmap,
                    IntPtr.Zero,
                    Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions());

                // 7. 冻结 BitmapSource（如果需要在其他线程使用）
                bitmapSource.Freeze();

                return bitmapSource;
            }
            catch (Exception ex)
            {
                // 记录或处理异常
                return null;
            }
            finally
            {
                // 8. 关键步骤：释放非托管资源
                if (hBitmap != IntPtr.Zero)
                {
                    DeleteObject(hBitmap); // 删除 GDI 位图对象
                }
                bitmap?.Dispose(); // 释放 Bitmap 资源
            }
        }

        // 导入 DeleteObject API 用于释放 GDI 对象
        [DllImport("gdi32.dll", EntryPoint = "DeleteObject")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DeleteObject(IntPtr hObject);
    }
}
