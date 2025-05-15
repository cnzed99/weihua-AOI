using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DahuaThermalCam
{
    public enum EM_PALETTE
    {
        彩虹,
        热金属,
        冷热,
        红外,
        灰度,
        海洋陆地,
        科学可视化
    }

    public class TemperatureColorMap
    {
        // 彩虹调色板 (Rainbow)
        public static readonly Color[] RainbowPalette = new Color[]
        {
            Color.FromArgb(0, 0, 255), // 深蓝 (低温)
            Color.FromArgb(0, 255, 255), // 青色
            Color.FromArgb(0, 255, 0), // 绿色
            Color.FromArgb(255, 255, 0), // 黄色
            Color.FromArgb(255, 128, 0), // 橙色
            Color.FromArgb(255, 0, 0) // 红色 (高温)
        };

        //热金属调色板 (Hot Metal)
        public static readonly Color[] HotMetalPalette = new Color[]
        {
            Color.FromArgb(0, 0, 0), // 黑色 (低温)
            Color.FromArgb(128, 0, 0), // 暗红
            Color.FromArgb(255, 0, 0), // 红色
            Color.FromArgb(255, 128, 0), // 橙色
            Color.FromArgb(255, 255, 0), // 黄色
            Color.FromArgb(255, 255, 255) // 白色 (高温)
        };

        //冷热对比调色板 (Cool-Warm)
        public static readonly Color[] CoolWarmPalette = new Color[]
        {
            Color.FromArgb(59, 76, 192), // 深蓝 (低温)
            Color.FromArgb(144, 178, 255), // 浅蓝
            Color.FromArgb(220, 220, 220), // 灰色 (中间值)
            Color.FromArgb(255, 170, 170), // 浅红
            Color.FromArgb(192, 48, 48) // 深红 (高温)
        };

        //红外调色板 (Infrared)
        public static readonly Color[] InfraredPalette = new Color[]
        {
            Color.FromArgb(0, 0, 0), // 黑色 (低温)
            Color.FromArgb(128, 0, 128), // 紫色
            Color.FromArgb(255, 0, 0), // 红色
            Color.FromArgb(255, 128, 0), // 橙色
            Color.FromArgb(255, 255, 0), // 黄色
            Color.FromArgb(255, 255, 255) // 白色 (高温)
        };

        //灰度调色板 (Grayscale)
        public static readonly Color[] GrayscalePalette = new Color[]
        {
            Color.FromArgb(0, 0, 0), // 黑色 (低温)
            Color.FromArgb(64, 64, 64),
            Color.FromArgb(128, 128, 128),
            Color.FromArgb(192, 192, 192),
            Color.FromArgb(255, 255, 255) // 白色 (高温)
        };

        //海洋陆地调色板 (Ocean-Land)
        public static readonly Color[] OceanLandPalette = new Color[]
        {
            Color.FromArgb(0, 0, 128), // 深蓝 (海洋低温)
            Color.FromArgb(0, 0, 255), // 蓝色
            Color.FromArgb(0, 128, 255), // 浅蓝
            Color.FromArgb(0, 255, 255), // 青色
            Color.FromArgb(128, 255, 128), // 浅绿 (陆地)
            Color.FromArgb(255, 255, 0), // 黄色
            Color.FromArgb(255, 128, 0), // 橙色
            Color.FromArgb(255, 0, 0) // 红色 (高温)
        };

        //科学可视化调色板 (Viridis)
        public static readonly Color[] ViridisPalette = new Color[]
        {
            Color.FromArgb(68, 1, 84), // 深紫
            Color.FromArgb(71, 44, 122),
            Color.FromArgb(59, 81, 139),
            Color.FromArgb(44, 113, 142),
            Color.FromArgb(33, 144, 141),
            Color.FromArgb(39, 173, 129),
            Color.FromArgb(92, 200, 99),
            Color.FromArgb(170, 220, 50),
            Color.FromArgb(253, 231, 37) // 亮黄
        };

        public static Bitmap CreateTemperatureImageOptimized(
            float[] temperatureData,
            int width,
            int height,
            float minTemp,
            float maxTemp,
            EM_PALETTE palette
        )
        {
            Bitmap bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);

            // 锁定位图数据
            BitmapData bitmapData = bitmap.LockBits(
                new Rectangle(0, 0, width, height),
                ImageLockMode.WriteOnly,
                bitmap.PixelFormat
            );

            unsafe
            {
                byte* scan0 = (byte*)bitmapData.Scan0;
                int stride = bitmapData.Stride;

                for (int y = 0; y < height; y++)
                {
                    byte* row = scan0 + (y * stride);
                    for (int x = 0; x < width; x++)
                    {
                        Color color = GetColorForTemperature(
                            temperatureData[y * width + x],
                            minTemp,
                            maxTemp,
                            palette
                        );

                        // 32bppArgb格式: 每个像素4字节 (B, G, R, A)
                        row[x * 4] = color.B; // Blue
                        row[x * 4 + 1] = color.G; // Green
                        row[x * 4 + 2] = color.R; // Red
                        row[x * 4 + 3] = 255; // Alpha (不透明)
                    }
                }
            }

            bitmap.UnlockBits(bitmapData);
            return bitmap;
        }

        private static Color GetColorForTemperature(
            float temp,
            float minTemp,
            float maxTemp,
            EM_PALETTE palette
        )
        {
            Color[] ColorPalette = new Color[5];
            switch (palette)
            {
                case EM_PALETTE.彩虹:
                    ColorPalette = RainbowPalette;
                    break;
                case EM_PALETTE.热金属:
                    ColorPalette = HotMetalPalette;
                    break;
                case EM_PALETTE.冷热:
                    ColorPalette = CoolWarmPalette;
                    break;
                case EM_PALETTE.红外:
                    ColorPalette = InfraredPalette;
                    break;
                case EM_PALETTE.灰度:
                    ColorPalette = GrayscalePalette;
                    break;
                case EM_PALETTE.海洋陆地:
                    ColorPalette = OceanLandPalette;
                    break;
                case EM_PALETTE.科学可视化:
                    ColorPalette = ViridisPalette;
                    break;
                default:
                    break;
            }
            // 确保温度在范围内
            temp = Math.Max(minTemp, Math.Min(maxTemp, temp));

            // 计算温度在颜色数组中的位置
            float ratio = (temp - minTemp) / (maxTemp - minTemp);
            float position = ratio * (ColorPalette.Length - 1);

            int index1 = (int)Math.Floor(position);
            int index2 = Math.Min(index1 + 1, ColorPalette.Length - 1);
            float fraction = position - index1;

            // 在两个颜色之间插值
            Color color1 = ColorPalette[index1];
            Color color2 = ColorPalette[index2];

            int r = (int)(color1.R + (color2.R - color1.R) * fraction);
            int g = (int)(color1.G + (color2.G - color1.G) * fraction);
            int b = (int)(color1.B + (color2.B - color1.B) * fraction);

            return Color.FromArgb(r, g, b);
        }
    }
}
