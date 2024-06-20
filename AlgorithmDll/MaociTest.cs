using System.Runtime.InteropServices;
using System.Windows;

namespace AlgorithmDll
{
    public enum DetectResult
    {
        TEST_OK = 0,//检测OK
        Ng_LightEdge = 1,//毛刺NG
        Ng_DarkEdge = 2,//料区NG
        Ng_Empty = 3,//空白NG
    };


    public enum RegionType
    {
        DarkTop = 0,
        DarkBot = 1,
        LightTop = 2,
        LightBot = 3,
        MaociRegion = 4,
        ThickRegion = 5,
    };

    public struct SRegionInfo
    {
        public int nX;//矩形X
        public int nY;//矩形Y
        public int nWidth;//矩形宽
        public int nHeight;//矩形高
        public double dHeight;//毛刺高度
        public int nArea;//毛刺面积
    };

    public struct SRegion
    {
        public SRegionInfo regionInfo;
        public List<Point> points1;

        public SRegion()
        {
            regionInfo = new SRegionInfo();
            points1 = new List<Point>();
        }
        public Rect GetRect()
        {
            return new Rect(regionInfo.nX, regionInfo.nY, regionInfo.nWidth, regionInfo.nHeight);
        }
    }

    public struct SDetectParam
    {
        public int nAdaptiveSize = 14;
        public int nAdaptiveAddGray = 20;
        public int nNeighbSize = 5;
        public int nNeighbLightPoint = 30;//邻域点数量限制
        public int nDarkThresh = 30;//料区阈值
        public int nLightThresh = 80;//铝层阈值
        public double dDarkThickLimit = 30;//料区厚度限制，掉料检测
        public double dDarkThick = 84;//料区厚度
        public double dLightThickLimit = 7;//铝层厚度限制，毛刺检测
        public double dLightThick = 6;//铝层厚度
        public double dPosLimit = 20;//铝层在料区中心位置限制

        public SDetectParam()
        {

        }
    };

    public struct SDetectParamFpga
    {
        public int nAdaptiveSize = 14;
        public int nAdaptiveAddGray = 20;
        public int nNeighbSize = 5;
        public int nNeighbLightPoint = 30;//邻域点数量限制
        public int nDarkThresh = 30;//料区阈值
        public int nLightThresh = 80;//铝层阈值
        public double dDarkThickLimit = 30;//料区厚度限制，掉料检测
        public double dDarkThick = 84;//料区厚度
        public double dLightThickLimit = 7;//铝层厚度限制，毛刺检测
        public double dLightThick = 6;//铝层厚度
        public double dPosLimit = 20;//铝层在料区中心位置限制

        public SDetectParamFpga()
        {
        }
    };

    public class MaociTest
    {
        #region 获取区域边缘点集合

        [DllImport("MaociAlg.dll")]
        private extern static int GetEdgeCount(RegionType type);

        [DllImport("MaociAlg.dll")]
        private extern static void GetEdge(RegionType type, IntPtr ptrX, IntPtr ptrY);

        [DllImport("MaociAlg.dll")]
        private extern static int GetRegionCount(RegionType type);

        [DllImport("MaociAlg.dll")]
        private extern static int GetRegionInfo(RegionType type, int nIndex, ref SRegionInfo sRegionInfo);

        [DllImport("MaociAlg.dll")]
        private extern static void GetRegionPoints(RegionType type, int nIndex, IntPtr pX, IntPtr pY);

        public static List<Point> GetRegion(RegionType type)
        {
            int size = MaociTest.GetEdgeCount(type);
            IntPtr ptrX = Marshal.AllocHGlobal(size * sizeof(int));
            IntPtr ptrY = Marshal.AllocHGlobal(size * sizeof(int));
            MaociTest.GetEdge(type, ptrX, ptrY);
            int[] Xs = new int[size];
            int[] Ys = new int[size];
            Marshal.Copy(ptrX, Xs, 0, size);
            Marshal.Copy(ptrY, Ys, 0, size);
            Marshal.FreeHGlobal(ptrX);
            Marshal.FreeHGlobal(ptrY);

            List<Point> points = new List<Point>();
            for (int i = 0; i < size; i++)
            {
                points.Add(new Point(Xs[i], Ys[i]));
            }

            return points;
        }

        public static List<SRegion> GetRegions(RegionType type)
        {
            List<SRegion> regions = new List<SRegion>();
            int regionCount = MaociTest.GetRegionCount(type);
            for (int k = 0; k < regionCount; k++)
            {
                SRegion sRegion = new SRegion();
                int size = MaociTest.GetRegionInfo(type, k, ref sRegion.regionInfo);
                IntPtr ptrX = Marshal.AllocHGlobal(size * sizeof(int));
                IntPtr ptrY = Marshal.AllocHGlobal(size * sizeof(int));
                MaociTest.GetRegionPoints(type, k, ptrX, ptrY);
                int[] Xs = new int[size];
                int[] Ys = new int[size];
                Marshal.Copy(ptrX, Xs, 0, size);
                Marshal.Copy(ptrY, Ys, 0, size);
                Marshal.FreeHGlobal(ptrX);
                Marshal.FreeHGlobal(ptrY);

                for (int i = 0; i < size; i++)
                {
                    sRegion.points1.Add(new Point(Xs[i], Ys[i]));
                }
                regions.Add(sRegion);
            }

            return regions;
        }
        #endregion

        #region 毛刺算法
        [DllImport("MaociAlg.dll")]
        public extern static DetectResult Test(int width, int height, int nLine, IntPtr data, SDetectParam detectParam);

        [DllImport("MaociAlg.dll")]
        public extern static DetectResult TestFpga(int width, int height, int nLine, IntPtr data, SDetectParamFpga sDetectParamFpga);

        #endregion
        /// <summary>
        /// 料区上边缘轮廓
        /// </summary>
        public List<Point> DarkTopRegion { get; set; } = new();
        /// <summary>
        /// 料区下边缘轮廓
        /// </summary>
        public List<Point> DarkBotRegion { get; set; } = new();
        /// <summary>
        /// 铝层上边缘轮廓
        /// </summary>
        public List<Point> LightTopRegion { get; set; } = new();
        /// <summary>
        /// 铝层下边缘轮廓
        /// </summary>
        public List<Point> LightBotRegion { get; set; } = new();
        /// <summary>
        /// 毛刺区域集
        /// </summary>
        public List<SRegion> MaociRegions { get; set; } = new();
        /// <summary>
        /// 厚度Ng区域集
        /// </summary>
        public List<SRegion> ThickRegions { get; set; } = new();

        /// <summary>
        /// 2024.6.20 李焕彬
        /// 检测图像
        /// </summary>
        /// <param name="width">图像宽度</param>
        /// <param name="height">图像高度</param>
        /// <param name="stride">一行宽度</param>
        /// <param name="data">图像指针</param>
        public DetectResult DetectImage(int width, int height, int stride, IntPtr data, SDetectParam detectParam)
        {
            DetectResult result = MaociTest.Test(width, height, stride, data, detectParam);
            DarkTopRegion = MaociTest.GetRegion(RegionType.DarkTop);
            DarkBotRegion = MaociTest.GetRegion(RegionType.DarkBot);
            LightTopRegion = MaociTest.GetRegion(RegionType.LightTop);
            LightBotRegion = MaociTest.GetRegion(RegionType.LightBot);
            MaociRegions = MaociTest.GetRegions(RegionType.MaociRegion);
            ThickRegions = MaociTest.GetRegions(RegionType.ThickRegion);

            return result;
        }

        /// <summary>
        /// 2024.6.20 李焕彬
        /// 检测Fpga
        /// </summary>
        /// <param name="width">图像宽度</param>
        /// <param name="height">图像高度</param>
        /// <param name="stride">一行宽度</param>
        /// <param name="data">图像指针</param>
        public DetectResult DetectFpga(int width, int height, int stride, IntPtr data, SDetectParamFpga detectParamFpga)
        {
            DetectResult result = MaociTest.TestFpga(width, height, stride, data, detectParamFpga);
            DarkTopRegion = MaociTest.GetRegion(RegionType.DarkTop);
            DarkBotRegion = MaociTest.GetRegion(RegionType.DarkBot);
            LightTopRegion = MaociTest.GetRegion(RegionType.LightTop);
            LightBotRegion = MaociTest.GetRegion(RegionType.LightBot);
            MaociRegions = MaociTest.GetRegions(RegionType.MaociRegion);
            ThickRegions = MaociTest.GetRegions(RegionType.ThickRegion);

            return result;
        }
    }
}
