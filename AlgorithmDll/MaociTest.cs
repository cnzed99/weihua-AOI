using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using System.Windows;

namespace AlgorithmDll
{
    /// <summary>
    /// 2024.6.25 李焕彬
    /// 检测结果
    /// </summary>
    public enum DetectResult
    {
        DR_OK = 0,//检测OK
        DR_Ng_LightEdge = 1,//毛刺NG
        DR_Ng_DarkEdge = 2,//料区NG
        DR_Ng_Empty = 3,//空白NG
    };

    /// <summary>
    /// 2024.6.25 李焕彬
    /// 区域类型
    /// </summary>
    public enum RegionType
    {
        RT_DarkTop = 0,
        RT_DarkBot = 1,
        RT_LightTop = 2,
        RT_LightBot = 3,
        RT_MaociRegion = 4,
        RT_ThickRegion = 5,
    };
    /// <summary>
    /// 2024.6.25 李焕彬
    /// 区域信息
    /// </summary>
    public struct SRegionInfo
    {
        public int nX = 0;//矩形X
        public int nY = 0;//矩形Y
        public int nWidth = 100;//矩形宽
        public int nHeight = 100;//矩形高
        public double dPeakHeight = 0;//直角高度
        public double dLongLen = 0;//长边长度
        public double dShorLen = 0;//短边长度
        public double dPhi = 0;//角度
        public double dContLen = 0;//周长
        public int nArea = 0;//面积
        public SRegionInfo()
        {

        }
    };

    /// <summary>
    /// 2024.6.25 李焕彬
    /// 区域
    /// </summary>
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
    /// <summary>
    /// 2024.6.25 李焕彬
    /// PC算法参数
    /// </summary>
    public struct SMaociAlgorParam
    {
        public uint nAdaptiveSize = 14;
        public int nAdaptiveAddGray = 20;
        public uint nNeighbSize = 5;
        public uint nNeighbLightPoint = 30;
        public uint nDarkThresh = 30;
        public uint nLightThresh = 80;
        public uint nLightThick = 6;

        public SMaociAlgorParam(MaociAlgorParam param)
        {
            this.nAdaptiveSize = param.AdaptiveSize;
            this.nAdaptiveAddGray = param.AdaptiveAddGray;
            this.nNeighbSize = param.NeighbSize;
            this.nNeighbLightPoint = param.NeighbLightPoint;
            this.nDarkThresh = param.DarkThresh;
            this.nLightThresh = param.LightThresh;
            this.nLightThick = param.LightThick;
        }
    };

    /// <summary>
    /// 2024.6.25 李焕彬
    /// Fpga算法参数
    /// </summary>
    public struct SMaociAlgorParamFpga
    {
        public uint nAdaptiveSize = 14;//自适应阈值邻域大小
        public int nAdaptiveAddGray = 20;//自适应阈值增加值
        public uint nNeighbSize = 5;//过滤矩阵邻域大小
        public uint nNeighbLightPoint = 30;//过滤矩阵邻域点数量限制
        public uint nDarkThresh = 30;//料区阈值
        public uint nLightThresh = 80;//铝层阈值
        public uint nDarkThickLimit = 30;//料区厚度限制，掉料检测
        public uint nDarkThickContinueLen = 5;//料区厚度NG连续长度限制
        public uint nDarkThick = 84;//料区厚度
        public uint nLightThickLimit = 7;//铝层厚度限制，毛刺检测
        public uint nLightThickContinueLen = 0;//铝层厚度NG连续长度限制
        public uint nLightThick = 6;//铝层厚度
        public uint nPosLimitT = 20;//铝层在料区中心位置限制上
        public uint nPosLimitB = 20;//铝层在料区中心位置限制下
        public int nLightPosOffest = 0;//铝层位置偏移值

        public SMaociAlgorParamFpga(MaociAlgorParamFpga param)
        {
            this.nAdaptiveSize = param.AdaptiveSize;
            this.nAdaptiveAddGray = param.AdaptiveAddGray;
            this.nNeighbSize = param.NeighbSize;
            this.nNeighbLightPoint = param.NeighbLightPoint;
            this.nDarkThresh = param.DarkThresh;
            this.nLightThresh = param.LightThresh;
            this.nDarkThickLimit = param.DarkThickLimit;
            this.nDarkThickContinueLen = param.DarkThickContinueLen;
            this.nDarkThick = param.DarkThick;
            this.nLightThickLimit = param.LightThickLimit;
            this.nLightThickContinueLen = param.LightThickContinueLen;
            this.nLightThick = param.LightThick;
            this.nPosLimitT = param.PosLimitT;
            this.nPosLimitB = param.PosLimitB;
            this.nLightPosOffest = param.LightPosOffest;
        }
    };

    /// <summary>
    /// 2024.6.25 李焕彬
    /// 毛刺检测算法接口
    /// </summary>
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
        public extern static DetectResult Test(int width, int height, int nLine, IntPtr data, SMaociAlgorParam detectParam);

        [DllImport("MaociAlg.dll")]
        public extern static DetectResult TestFpga(int width, int height, int nLine, IntPtr data, SMaociAlgorParamFpga sDetectParamFpga);

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

        public AlgorithmOut AlgorithmOut { get; set; } = new AlgorithmOut();
        /// <summary>
        /// 2024.6.20 李焕彬
        /// 检测图像
        /// </summary>
        /// <param name="width">图像宽度</param>
        /// <param name="height">图像高度</param>
        /// <param name="stride">一行宽度</param>
        /// <param name="data">图像指针</param>
        public DetectResult DetectImage(int width, int height, int stride, IntPtr data, SMaociAlgorParam detectParam)
        {
            DetectResult result = MaociTest.Test(width, height, stride, data, detectParam);
            DarkTopRegion = MaociTest.GetRegion(RegionType.RT_DarkTop);
            DarkBotRegion = MaociTest.GetRegion(RegionType.RT_DarkBot);
            LightTopRegion = MaociTest.GetRegion(RegionType.RT_LightTop);
            LightBotRegion = MaociTest.GetRegion(RegionType.RT_LightBot);
            MaociRegions = MaociTest.GetRegions(RegionType.RT_MaociRegion);
            ThickRegions = MaociTest.GetRegions(RegionType.RT_ThickRegion);

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
        public DetectResult DetectFpga(int width, int height, int stride, IntPtr data, SMaociAlgorParamFpga detectParamFpga)
        {
            DetectResult result = MaociTest.TestFpga(width, height, stride, data, detectParamFpga);
            DarkTopRegion = MaociTest.GetRegion(RegionType.RT_DarkTop);
            DarkBotRegion = MaociTest.GetRegion(RegionType.RT_DarkBot);
            LightTopRegion = MaociTest.GetRegion(RegionType.RT_LightTop);
            LightBotRegion = MaociTest.GetRegion(RegionType.RT_LightBot);
            MaociRegions = MaociTest.GetRegions(RegionType.RT_MaociRegion);
            ThickRegions = MaociTest.GetRegions(RegionType.RT_ThickRegion);

            return result;
        }
    }
}
