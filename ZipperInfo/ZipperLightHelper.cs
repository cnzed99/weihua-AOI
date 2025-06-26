using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZipperInfo
{
    public class ZipperLightHelper
    {
        public static ZipperLightHelper Instance => _instance.Value;
        private static readonly Lazy<ZipperLightHelper> _instance = new Lazy<ZipperLightHelper>(() => new ZipperLightHelper());

        private ZipperLightHelper()
        {
        }

        private const int EmphaMaskValue = 7;
        private const double EmphaFactorValue = 0.7;
        private const int MinV = 80;
        private const int MaxV = 220;

        /// <summary>
        /// 拉链图像亮度检测
        /// </summary>
        /// <param name="inputImage">图像输入</param>
        /// <param name="BrightnessDiff">亮度保留差值（计算值与预测值小于此值则无需调整直接返回），默认值：10，推荐值：8-15</param>
        /// <param name="strideRate">步幅调整灵敏度比率，默认值：0.5（根据实际光源控制器调整）</param>
        /// <param name="VState">亮度评估状态，0：保持亮度、1：需增加光源亮度、2：需减少亮度</param>
        /// <param name="VStride">推荐调整光源步幅值（供上层应用快速调整至合适的光源值使用）</param>
        public void ZipperLightDetection(Mat inputImage, int brightnessDiff, double strideRate,
        out int VState, out int VStride)
        {
            // 初始化输出变量
            VState = 0;
            VStride = 0;

            // 步骤1: 转换为灰度图
            Mat grayImage = new Mat();
            Cv2.CvtColor(inputImage, grayImage, ColorConversionCodes.BGR2GRAY);

            // 步骤2: 图像增强 (emphasize)
            Mat imageEmphasize = EmphasizeImage(grayImage, EmphaMaskValue, EmphaFactorValue);

            // 步骤3: 计算均值图像
            Mat imageMean = new Mat();
            Cv2.Blur(grayImage, imageMean, new Size(15, 15));

            // 步骤4: 动态阈值分割
            Mat diff = new Mat();
            Cv2.Subtract(imageEmphasize, imageMean, diff);
            Mat regionDynThresh = new Mat();
            Cv2.Threshold(diff, regionDynThresh, 35, 255, ThresholdTypes.Binary);

            // 步骤5: 形态学操作 - 闭圆
            Mat regionClosing1 = new Mat();
            Mat kernelCircle = Cv2.GetStructuringElement(MorphShapes.Ellipse, new Size(31, 31));
            Cv2.MorphologyEx(regionDynThresh, regionClosing1, MorphTypes.Close, kernelCircle);

            // 步骤6: 形态学操作 - 开圆
            Mat regionOpening1 = new Mat();
            Mat kernelSmall = Cv2.GetStructuringElement(MorphShapes.Ellipse, new Size(3, 3));
            Cv2.MorphologyEx(regionClosing1, regionOpening1, MorphTypes.Open, kernelSmall);

            // 步骤7: 矩形闭操作和填充
            Mat regionClosing = new Mat();
            Mat kernelRect = Cv2.GetStructuringElement(MorphShapes.Rect, new Size(60, 60));
            Cv2.MorphologyEx(regionOpening1, regionClosing, MorphTypes.Close, kernelRect);

            // 修复的填充孔洞函数
            Mat regionFillUp = FillHoles(regionClosing);

            // 步骤8: 矩形开操作
            Mat regionOpening = new Mat();
            Mat kernelLarge = Cv2.GetStructuringElement(MorphShapes.Rect, new Size(200, 200));
            Cv2.MorphologyEx(regionFillUp, regionOpening, MorphTypes.Open, kernelLarge);

            // 步骤9: 矩形腐蚀
            Mat regionErosion = new Mat();
            Mat kernelErode = Cv2.GetStructuringElement(MorphShapes.Rect, new Size(5, 5));
            Cv2.Erode(regionOpening, regionErosion, kernelErode);
            Mat selectROI = regionErosion;

            // 步骤10: 颜色空间转换修复 - 使用HSV_FULL模式
            Mat imageReduced = new Mat();
            inputImage.CopyTo(imageReduced, selectROI);

            //Mat hsv = new Mat();
            // 关键修改：使用HSV_FULL模式确保H值范围0-255
            //Cv2.CvtColor(imageReduced, hsv, ColorConversionCodes.BGR2HSV_FULL);
            //Mat[] hsvChannels = Cv2.Split(hsv);

            //// 步骤11: 计算通道均值 (只在ROI区域内)
            //Scalar hMean = Cv2.Mean(hsvChannels[0], selectROI);
            //Scalar sMean = Cv2.Mean(hsvChannels[1], selectROI);
            //Scalar vMean = Cv2.Mean(hsvChannels[2], selectROI);

            ConvertToHalconHSVFast(imageReduced, out Mat hImg, out Mat sImg, out Mat vImg);
            Scalar hMean = Cv2.Mean(hImg, selectROI);
            Scalar sMean = Cv2.Mean(sImg, selectROI);
            Scalar vMean = Cv2.Mean(vImg, selectROI);

            // 使用HSV_FULL后H值已经是0-255范围，不需要转换
            double HValue = hMean.Val0;
            double SValue = sMean.Val0;
            double VValue = vMean.Val0;

            // 步骤12: 预测V值
            double V_predicted = PredictVFromColor(HValue, SValue);

            // 步骤13: 计算亮度差异
            double CValue = Math.Abs(VValue - V_predicted);
            VStride = (int)(CValue / 2.0 * strideRate);

            // 调试输出
            string debugInfo = $"H: {HValue:F2}, S: {SValue:F2}, V: {VValue:F2}, Predicted V: {V_predicted:F2}, CValue: {CValue:F2}";
            Console.WriteLine(debugInfo);

            // 步骤14: 状态判断
            if (CValue <= brightnessDiff)
            {
                VState = 0;
                VStride = 0;
            }
            else if (VValue < V_predicted)
            {
                VState = 1;
            }
            else if (VValue > V_predicted)
            {
                VState = 2;
            }
        }

        // 图像增强函数
        private Mat EmphasizeImage(Mat input, int maskSize, double factor)
        {
            Mat blurred = new Mat();
            Cv2.Blur(input, blurred, new Size(maskSize, maskSize));

            Mat emphasized = new Mat();
            Cv2.AddWeighted(input, 1 + factor, blurred, -factor, 0, emphasized);
            return emphasized;
        }


        /// <summary>
        /// 将 BGR 像素转换为 HSV（全部范围 0~255，模拟 Halcon 的方式）
        /// </summary>
        /// <param name="b">B通道（0-255）</param>
        /// <param name="g">G通道（0-255）</param>
        /// <param name="r">R通道（0-255）</param>
        /// <param name="h">输出 H（0-255）</param>
        /// <param name="s">输出 S（0-255）</param>
        /// <param name="v">输出 V（0-255）</param>
        private static void BGR2HSV_HalconStyle(byte b, byte g, byte r, out byte h, out byte s, out byte v)
        {
            double rNorm = r / 255.0;
            double gNorm = g / 255.0;
            double bNorm = b / 255.0;

            double max = Math.Max(rNorm, Math.Max(gNorm, bNorm));
            double min = Math.Min(rNorm, Math.Min(gNorm, bNorm));
            double delta = max - min;

            // Value
            v = (byte)(max * 255);

            // Saturation
            if (max == 0)
                s = 0;
            else
                s = (byte)((delta / max) * 255);

            // Hue
            double hTemp;
            if (delta == 0)
            {
                hTemp = 0;
            }
            else if (max == rNorm)
            {
                hTemp = (60 * ((gNorm - bNorm) / delta) + 360) % 360;
            }
            else if (max == gNorm)
            {
                hTemp = (60 * ((bNorm - rNorm) / delta) + 120);
            }
            else // max == bNorm
            {
                hTemp = (60 * ((rNorm - gNorm) / delta) + 240);
            }

            // Map hue from 0~360 to 0~255 (Halcon-style)
            h = (byte)(hTemp / 360.0 * 255.0);
        }

        //实现BGR转HSV（与halcon中的trans_from_rgb算法保持一至）
        public void ConvertToHalconHSV(Mat bgr, out Mat hImg, out Mat sImg, out Mat vImg)
        {
            hImg = new Mat(bgr.Size(), MatType.CV_8UC1);
            sImg = new Mat(bgr.Size(), MatType.CV_8UC1);
            vImg = new Mat(bgr.Size(), MatType.CV_8UC1);

            for (int y = 0; y < bgr.Rows; y++)
            {
                for (int x = 0; x < bgr.Cols; x++)
                {
                    Vec3b pixel = bgr.At<Vec3b>(y, x);
                    BGR2HSV_HalconStyle(pixel.Item0, pixel.Item1, pixel.Item2, out byte h, out byte s, out byte v);
                    hImg.Set(y, x, h);
                    sImg.Set(y, x, s);
                    vImg.Set(y, x, v);
                }
            }
        }

        //实现BGR转HSV（与halcon中的trans_from_rgb算法保持一至）
        public unsafe void ConvertToHalconHSVFast(Mat bgr, out Mat hMat, out Mat sMat, out Mat vMat)
        {
            int rows = bgr.Rows;
            int cols = bgr.Cols;

            hMat = new Mat(rows, cols, MatType.CV_8UC1);
            sMat = new Mat(rows, cols, MatType.CV_8UC1);
            vMat = new Mat(rows, cols, MatType.CV_8UC1);

            var srcIndexer = bgr.GetGenericIndexer<Vec3b>();

            byte* hPtr = (byte*)hMat.DataPointer;
            byte* sPtr = (byte*)sMat.DataPointer;
            byte* vPtr = (byte*)vMat.DataPointer;

            int stride = cols;

            Parallel.For(0, rows, y =>
            {
                for (int x = 0; x < cols; x++)
                {
                    Vec3b bgrPixel = srcIndexer[y, x];

                    double r = bgrPixel.Item2 / 255.0;
                    double g = bgrPixel.Item1 / 255.0;
                    double b = bgrPixel.Item0 / 255.0;

                    double max = Math.Max(r, Math.Max(g, b));
                    double min = Math.Min(r, Math.Min(g, b));
                    double delta = max - min;

                    double h, s, v;
                    v = max;
                    s = max == 0 ? 0 : delta / max;

                    if (delta == 0)
                    {
                        h = 0;
                    }
                    else if (max == r)
                    {
                        h = (60 * ((g - b) / delta) + 360) % 360;
                    }
                    else if (max == g)
                    {
                        h = 60 * ((b - r) / delta) + 120;
                    }
                    else // max == b
                    {
                        h = 60 * ((r - g) / delta) + 240;
                    }

                    int idx = y * stride + x;
                    hPtr[idx] = (byte)(h / 360.0 * 255.0);
                    sPtr[idx] = (byte)(s * 255.0);
                    vPtr[idx] = (byte)(v * 255.0);
                }
            });
        }


        // 修复的填充孔洞函数 - 使用连通组件分析
        private Mat FillHoles(Mat binaryImage)
        {
            // 创建输出图像
            Mat filled = new Mat(binaryImage.Size(), MatType.CV_8UC1, Scalar.Black);

            // 查找所有轮廓
            Point[][] contours;
            HierarchyIndex[] hierarchy;
            Cv2.FindContours(binaryImage, out contours, out hierarchy,
                RetrievalModes.CComp, ContourApproximationModes.ApproxSimple);

            // 如果没有找到轮廓，直接返回原始图像
            if (contours.Length == 0)
                return binaryImage.Clone();

            // 创建一个掩码用于绘制填充的轮廓
            Mat mask = new Mat(binaryImage.Size(), MatType.CV_8UC1, Scalar.Black);

            // 遍历所有轮廓
            for (int i = 0; i < contours.Length; i++)
            {
                // 只填充内部轮廓（孔洞）
                if (hierarchy[i].Parent < 0) // 外部轮廓
                {
                    // 绘制外部轮廓（白色填充）
                    Cv2.DrawContours(mask, contours, i, Scalar.White, -1);
                }
                else // 内部轮廓（孔洞）
                {
                    // 绘制孔洞（黑色填充）
                    Cv2.DrawContours(mask, contours, i, Scalar.Black, -1);
                }
            }

            // 对原始图像进行按位与操作，填充孔洞
            Cv2.BitwiseAnd(binaryImage, mask, filled);

            return filled;
        }

        // 预测V值函数 (使用Halcon的0-255范围)
        private double PredictVFromColor(double H, double S)
        {
            double V_predicted = 128; // 默认值

            // 计算色相环位置 (0-11)
            int hue_zone = (int)(H * 12.0 / 255.0);

            // 高饱和度区域 (S ≥ 150)
            if (S >= 150)
            {
                // 红色/橙色区 (色相区0,3,9-11)
                if (hue_zone == 0 || hue_zone == 3 || (hue_zone >= 9 && hue_zone <= 11))
                {
                    // 橙红色核心区 (H:75-85)
                    if (H >= 75 && H <= 85)
                    {
                        V_predicted = 200;
                    }
                    // 黄绿色过渡区 (H:35-75)
                    else if (H >= 35 && H <= 75)
                    {
                        double transition = Math.Min(Math.Max((H - 35) / 40.0, 0.0), 1.0);
                        V_predicted = 165 + 45 * transition;
                    }
                    // 深红/玫红色系 (H:240-255)
                    else if (H >= 240)
                    {
                        V_predicted = 130 + 0.22 * S;
                    }
                    else
                    {
                        V_predicted = 190;
                    }
                }
                // 黄绿色系 (色相区1)
                else if (hue_zone >= 1 && hue_zone <= 2)
                {
                    V_predicted = 145 + 0.15 * S;
                }
                // 绿色/青绿色区 (色相区4-6)
                else if (hue_zone >= 4 && hue_zone <= 6)
                {
                    // 草绿色核心区 (H:85-95)
                    if (H >= 85 && H <= 95)
                    {
                        V_predicted = 158;
                    }
                    // 青蓝色区 (H:140-175)
                    else if (H >= 140 && H <= 175)
                    {
                        V_predicted = 0.396 * H + 0.218 * S + 81.804;
                    }
                    else
                    {
                        double transition = Math.Min(Math.Max((H - 95) / 45.0, 0.0), 1.0);
                        double base_value = (hue_zone == 4) ? 158 : 165;
                        V_predicted = base_value + 10 * transition;
                    }
                }
                // 蓝色/紫色区 (色相区7-10)
                else if (hue_zone >= 7 && hue_zone <= 10)
                {
                    // 深红色系 (H:240-255)
                    if (H >= 240)
                    {
                        V_predicted = 130 + 0.2 * S;
                    }
                    else
                    {
                        V_predicted = 185;
                    }
                }
                // 其他高饱和颜色
                else
                {
                    V_predicted = 185;
                }
            }
            // 中饱和度区域 (50 ≤ S < 150)
            else if (S >= 50)
            {
                // 青灰色系 (H:130-170)
                if (H >= 130 && H <= 170)
                {
                    V_predicted = 80 + 0.15 * H + 0.35 * S;
                }
                // 黄/橙色区 (色相区1-3)
                else if (hue_zone >= 1 && hue_zone <= 3)
                {
                    V_predicted = 90 + 0.15 * H + 0.45 * S;
                }
                // 绿/青色区 (色相区4-6)
                else if (hue_zone >= 4 && hue_zone <= 6)
                {
                    // 计算饱和度权重
                    double s_weight = Math.Min(Math.Max((S - 40) / 30.0, 0.0), 1.0);
                    double base_value = 110 + 0.2 * H + 0.5 * S;
                    V_predicted = base_value - 20 * (1 - s_weight);
                }
                // 蓝/紫色区 (色相区7-10)
                else if (hue_zone >= 7 && hue_zone <= 10)
                {
                    V_predicted = 100 + 0.15 * H + 0.4 * S;
                }
                // 其他中饱和颜色
                else
                {
                    V_predicted = 110 + 0.2 * H + 0.55 * S;
                }
            }
            // 低饱和度区域 (S < 50)
            else
            {
                // 米黄色区 (色相区1-2, H:25-60)
                if (hue_zone >= 1 && hue_zone <= 3 && H >= 25 && H <= 70)
                {
                    // 米灰色
                    if (H >= 50 && S <= 15)
                    {
                        V_predicted = 160;
                    }
                    // 橄榄绿
                    else if (H >= 50)
                    {
                        // 计算饱和度权重
                        double s_weight = Math.Min(Math.Max((S - 40) / 30.0, 0.0), 1.0);
                        V_predicted = 160 - 35 * (1 - s_weight);
                    }
                    else
                    {
                        // 计算饱和度权重
                        double s_weight = Math.Min(Math.Max((S - 40) / 30.0, 0.0), 1.0);
                        V_predicted = 160 - 20 * (1 - s_weight);
                    }
                }
                // 青蓝色系 (H:140-175)
                else if (H >= 140 && H <= 175)
                {
                    V_predicted = 100 + 0.38 * S;
                }
                // 极低饱和区 (S < 15)
                else if (S < 15)
                {
                    V_predicted = 95;
                }
                // 低饱和过渡区 (15 ≤ S < 35)
                else if (S < 35)
                {
                    double base_value = (hue_zone >= 4 && hue_zone <= 6) ? 85 : 90;
                    V_predicted = base_value + 10 * (S - 15) / 20.0;
                }
                // 中低饱和区 (35 ≤ S < 50)
                else
                {
                    V_predicted = 95 + 0.5 * S;
                }
            }

            // 范围保护 (80-220) 使用平滑过渡
            if (V_predicted < MinV)
            {
                V_predicted = MinV + 0.5 * (MinV - V_predicted);
            }
            else if (V_predicted > MaxV)
            {
                V_predicted = MaxV - 0.5 * (V_predicted - MaxV);
            }

            // 最终取整
            return Math.Round(V_predicted);
        }
    }
}