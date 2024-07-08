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
    public enum EMDETECTRESULT
    {
        EMDR_OK = 0,//检测OK
        EMDR_NG_LIGHTEDGE = 1,//毛刺NG
        EMDR_NG_DARKEDGE = 2,//料区NG
        EMDR_NG_EMPTY = 3,//空白NG
    };

    /// <summary>
    /// 2024.6.25 李焕彬
    /// 区域类型
    /// </summary>
    public enum EMREGIONTYPE
    {
        EMRT_DARKTOP = 0,
        EMRT_DARKBOT = 1,
        EMRT_LIGHTTOP = 2,
        EMRT_LIGHTBOT = 3,
        EMRT_MAOCIREGION = 4,
        EMRT_THICKREGION = 5,
    };
    /// <summary>
    /// 2024.6.25 李焕彬
    /// 区域信息
    /// </summary>
    public struct SRegionInfo
    {
        /// <summary>
        /// 2024.7.4 李焕彬
        /// 像素矩形X
        /// </summary>
        public int X = 0;
        /// <summary>
        /// 2024.7.4 李焕彬
        /// 像素矩形Y
        /// </summary>
        public int Y = 0;
        /// <summary>
        /// 2024.7.4 李焕彬
        /// 像素矩形宽
        /// </summary>
        public int Width = 100;
        /// <summary>
        /// 2024.7.4 李焕彬
        /// 像素矩形高
        /// </summary>
        public int Height = 100;
        /// <summary>
        /// 2024.7.4 李焕彬
        /// um垂直宽度
        /// </summary>
        public double WidthBound = 0;
        /// <summary>
        /// 2024.7.4 李焕彬
        /// um垂直高度
        /// </summary>
        public double HeightBound = 0;
        /// <summary>
        /// 2024.7.4 李焕彬
        /// um直角高度
        /// </summary>
        public double PeakHeight = 0;
        /// <summary>
        /// 2024.7.4 李焕彬
        /// um长边长度
        /// </summary>
        public double LongLen = 0;
        /// <summary>
        /// 2024.7.4 李焕彬
        /// um短边长度
        /// </summary>
        public double ShorLen = 0;
        /// <summary>
        /// 2024.7.4 李焕彬
        /// 角度
        /// </summary>
        public double Phi = 0;
        /// <summary>
        /// 2024.7.4 李焕彬
        /// um周长
        /// </summary>
        public double ContLen = 0;
        /// <summary>
        /// 2024.7.4 李焕彬
        /// um²面积
        /// </summary>
        public double Area = 0;
        public SRegionInfo()
        {

        }

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 复制
        /// </summary>
        /// <param name="regionInfo">复制源</param>
        public void Copy(SRegionInfo regionInfo)
        {
            X = regionInfo.X;
            Y = regionInfo.Y;
            Width = regionInfo.Width;
            Height = regionInfo.Height;
            WidthBound = regionInfo.WidthBound;
            HeightBound = regionInfo.HeightBound;
            PeakHeight = regionInfo.PeakHeight;
            LongLen = regionInfo.LongLen;
            ShorLen = regionInfo.ShorLen;
            Phi = regionInfo.Phi;
            ContLen = regionInfo.ContLen;
            Area = regionInfo.Area;
        }
    };

    /// <summary>
    /// 2024.6.25 李焕彬
    /// 区域
    /// </summary>
    public struct SRegion
    {
        /// <summary>
        /// 2024.7.4 李焕彬
        /// 区域信息
        /// </summary>
        public SRegionInfo RegionInfo;

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 区域点集
        /// </summary>
        public List<Point> points1;

        public SRegion()
        {
            RegionInfo = new SRegionInfo();
            points1 = new List<Point>();
        }

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 获取矩形
        /// </summary>
        /// <returns></returns>
        public Rect GetRect()
        {
            return new Rect(RegionInfo.X, RegionInfo.Y, RegionInfo.Width, RegionInfo.Height);
        }
    }
    /// <summary>
    /// 2024.6.25 李焕彬
    /// PC算法参数
    /// </summary>
    public struct SMaociAlgorParam
    {
        /// <summary>
        /// 2024.7.4 李焕彬
        /// 自适应阈值邻域大小
        /// </summary>
        public uint AdaptiveSize = 14;

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 自适应阈值增加值
        /// </summary>
        public int AdaptiveAddGray = 20;

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 过滤矩阵邻域大小
        /// </summary>
        public uint NeighbSize = 5;

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 过滤矩阵邻域点数量限制
        /// </summary>
        public uint NeighbLightPoint = 30;

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 料区阈值
        /// </summary>
        public uint DarkThresh = 30;

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 铝层阈值
        /// </summary>
        public uint LightThresh = 80;

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 铝层厚度
        /// </summary>
        public uint LightThick = 6;

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 构造
        /// </summary>
        /// <param name="param">算法参数类</param>
        public SMaociAlgorParam(MaociAlgorParam param)
        {
            this.AdaptiveSize = param.AdaptiveSize;
            this.AdaptiveAddGray = param.AdaptiveAddGray;
            this.NeighbSize = param.NeighbSize;
            this.NeighbLightPoint = param.NeighbLightPoint;
            this.DarkThresh = param.DarkThresh;
            this.LightThresh = param.LightThresh;
            this.LightThick = param.LightThick;
        }
    };

    /// <summary>
    /// 2024.6.25 李焕彬
    /// Fpga算法参数
    /// </summary>
    public struct SMaociAlgorParamFpga
    {
        /// <summary>
        /// 2024.7.4 李焕彬
        /// //自适应阈值邻域大小
        /// </summary>
        public uint AdaptiveSize = 14;

        /// <summary>
        /// 2024.7.4 李焕彬
        /// //自适应阈值增加值
        /// </summary>
        public int AdaptiveAddGray = 20;

        /// <summary>
        /// 2024.7.4 李焕彬
        /// //过滤矩阵邻域大小
        /// </summary>
        public uint NeighbSize = 5;

        /// <summary>
        /// 2024.7.4 李焕彬
        /// //过滤矩阵邻域点数量限制
        /// </summary>
        public uint NeighbLightPoint = 30;

        /// <summary>
        /// 2024.7.4 李焕彬
        /// //料区阈值
        /// </summary>
        public uint DarkThresh = 30;

        /// <summary>
        /// 2024.7.4 李焕彬
        /// //铝层阈值
        /// </summary>
        public uint LightThresh = 80;

        /// <summary>
        /// 2024.7.4 李焕彬
        /// //料区厚度限制，掉料检测
        /// </summary>
        public uint DarkThickLimit = 30;

        /// <summary>
        /// 2024.7.4 李焕彬
        /// //料区厚度NG连续长度限制
        /// </summary>
        public uint DarkThickContinueLen = 5;

        /// <summary>
        /// 2024.7.4 李焕彬
        /// //料区厚度
        /// </summary>
        public uint DarkThick = 84;

        /// <summary>
        /// 2024.7.4 李焕彬
        /// //铝层厚度限制，毛刺检测
        /// </summary>
        public uint LightThickLimit = 7;

        /// <summary>
        /// 2024.7.4 李焕彬
        /// //铝层厚度NG连续长度限制
        /// </summary>
        public uint LightThickContinueLen = 0;

        /// <summary>
        /// 2024.7.4 李焕彬
        /// //铝层厚度
        /// </summary>
        public uint LightThick = 6;

        /// <summary>
        /// 2024.7.4 李焕彬
        /// //铝层在料区中心位置限制上
        /// </summary>
        public uint PosLimitT = 20;

        /// <summary>
        /// 2024.7.4 李焕彬
        /// //铝层在料区中心位置限制下
        /// </summary>
        public uint PosLimitB = 20;

        /// <summary>
        /// 2024.7.4 李焕彬
        /// //铝层位置偏移值
        /// </summary>
        public int LightPosOffest = 0;

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 构造
        /// </summary>
        /// <param name="param">FPGA算法参数类</param>
        public SMaociAlgorParamFpga(MaociAlgorParamFpga param)
        {
            this.AdaptiveSize = param.AdaptiveSize;
            this.AdaptiveAddGray = param.AdaptiveAddGray;
            this.NeighbSize = param.NeighbSize;
            this.NeighbLightPoint = param.NeighbLightPoint;
            this.DarkThresh = param.DarkThresh;
            this.LightThresh = param.LightThresh;
            this.DarkThickLimit = param.DarkThickLimit;
            this.DarkThickContinueLen = param.DarkThickContinueLen;
            this.DarkThick = param.DarkThick;
            this.LightThickLimit = param.LightThickLimit;
            this.LightThickContinueLen = param.LightThickContinueLen;
            this.LightThick = param.LightThick;
            this.PosLimitT = param.PosLimitT;
            this.PosLimitB = param.PosLimitB;
            this.LightPosOffest = param.LightPosOffest;
        }
    };

    /// <summary>
    /// 2024.6.25 李焕彬
    /// 毛刺检测算法接口
    /// </summary>
    public class MaociTest
    {
        #region 获取区域边缘点集合
        /// <summary>
        /// 2024.7.4 李焕彬
        /// 获取区域点
        /// </summary>
        /// <param name="type">区域类型</param>
        /// <returns></returns>
        [DllImport("MaociAlg.dll")]
        private extern static int GetEdgeCount(EMREGIONTYPE type);

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 获取区域
        /// </summary>
        /// <param name="type">类型</param>
        /// <param name="ptrX">输出指针X</param>
        /// <param name="ptrY">输出指针Y</param>
        [DllImport("MaociAlg.dll")]
        private extern static void GetEdge(EMREGIONTYPE type, IntPtr ptrX, IntPtr ptrY);

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 获取区域点数
        /// </summary>
        /// <param name="type">区域类型</param>
        /// <returns></returns>
        [DllImport("MaociAlg.dll")]
        private extern static int GetRegionCount(EMREGIONTYPE type);

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 获取区域信息
        /// </summary>
        /// <param name="type">类型</param>
        /// <param name="nIndex">索引</param>
        /// <param name="sRegionInfo">输出区域信息</param>
        /// <returns></returns>
        [DllImport("MaociAlg.dll")]
        private extern static int GetRegionInfo(EMREGIONTYPE type, int nIndex, ref SRegionInfo sRegionInfo);

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 获取区域点集
        /// </summary>
        /// <param name="type">类型</param>
        /// <param name="nIndex">索引</param>
        /// <param name="pX">输出指针X</param>
        /// <param name="pY">输出指针Y</param>
        [DllImport("MaociAlg.dll")]
        private extern static void GetRegionPoints(EMREGIONTYPE type, int nIndex, IntPtr pX, IntPtr pY);

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 获取区域
        /// </summary>
        /// <param name="type">类型</param>
        /// <returns></returns>
        public static List<Point> GetRegion(EMREGIONTYPE type)
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

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 获取区域集
        /// </summary>
        /// <param name="type">类型</param>
        /// <returns></returns>
        public static List<SRegion> GetRegions(EMREGIONTYPE type)
        {
            List<SRegion> regions = new List<SRegion>();
            int regionCount = MaociTest.GetRegionCount(type);
            double dMmPerPixel = 2.25d;
            for (int k = 0; k < regionCount; k++)
            {
                SRegion sRegion = new SRegion();
                int size = MaociTest.GetRegionInfo(type, k, ref sRegion.RegionInfo);
                sRegion.RegionInfo.WidthBound *= dMmPerPixel;
                sRegion.RegionInfo.HeightBound *= dMmPerPixel;
                sRegion.RegionInfo.PeakHeight *= dMmPerPixel;
                sRegion.RegionInfo.LongLen *= dMmPerPixel;
                sRegion.RegionInfo.ShorLen *= dMmPerPixel;
                sRegion.RegionInfo.ContLen *= dMmPerPixel;
                sRegion.RegionInfo.Area *= dMmPerPixel;
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

        /// <summary>
        /// 2024.7.4 李焕彬
        /// PC算法测试
        /// </summary>
        /// <param name="width">宽度</param>
        /// <param name="height">高度</param>
        /// <param name="nLine">行宽</param>
        /// <param name="data">图像数据</param>
        /// <param name="detectParam">PC算法</param>
        /// <returns>检测结果</returns>
        #region 毛刺算法
        [DllImport("MaociAlg.dll")]
        public extern static EMDETECTRESULT Test(int width, int height, int nLine, IntPtr data, SMaociAlgorParam detectParam);

        /// <summary>
        /// 2024.7.4 李焕彬
        /// FPGA算法测试
        /// </summary>
        /// <param name="width">宽度</param>
        /// <param name="height">高度</param>
        /// <param name="nLine">行宽</param>
        /// <param name="data">图像数据</param>
        /// <param name="sDetectParamFpga">FPGA算法</param>
        /// <returns>检测结果</returns>
        [DllImport("MaociAlg.dll")]
        public extern static EMDETECTRESULT TestFpga(int width, int height, int nLine, IntPtr data, SMaociAlgorParamFpga sDetectParamFpga);

        #endregion
        /// <summary>
        /// 2024.7.4 李焕彬
        /// 料区上边缘轮廓
        /// </summary>
        public List<Point> DarkTopRegion { get; set; } = new();
        /// <summary>
        /// 2024.7.4 李焕彬
        /// 料区下边缘轮廓
        /// </summary>
        public List<Point> DarkBotRegion { get; set; } = new();
        /// <summary>
        /// 2024.7.4 李焕彬
        /// 铝层上边缘轮廓
        /// </summary>
        public List<Point> LightTopRegion { get; set; } = new();
        /// <summary>
        /// 2024.7.4 李焕彬
        /// 铝层下边缘轮廓
        /// </summary>
        public List<Point> LightBotRegion { get; set; } = new();
        /// <summary>
        /// 2024.7.4 李焕彬
        /// 毛刺区域集
        /// </summary>
        public List<SRegion> MaociRegions { get; set; } = new();
        /// <summary>
        /// 2024.7.4 李焕彬
        /// 厚度Ng区域集
        /// </summary>
        public List<SRegion> ThickRegions { get; set; } = new();
        /// <summary>
        /// 2024.7.4 李焕彬
        /// 算法输出
        /// </summary>
        public AlgorithmOut AlgorithmOut { get; set; } = new();
        /// <summary>
        /// 2024.6.20 李焕彬
        /// 检测图像
        /// </summary>
        /// <param name="width">图像宽度</param>
        /// <param name="height">图像高度</param>
        /// <param name="stride">一行宽度</param>
        /// <param name="data">图像指针</param>
        public EMDETECTRESULT DetectImage(int width, int height, int stride, IntPtr data, SMaociAlgorParam detectParam)
        {
            EMDETECTRESULT result = MaociTest.Test(width, height, stride, data, detectParam);
            DarkTopRegion = MaociTest.GetRegion(EMREGIONTYPE.EMRT_DARKTOP);
            DarkBotRegion = MaociTest.GetRegion(EMREGIONTYPE.EMRT_DARKBOT);
            LightTopRegion = MaociTest.GetRegion(EMREGIONTYPE.EMRT_LIGHTTOP);
            LightBotRegion = MaociTest.GetRegion(EMREGIONTYPE.EMRT_LIGHTBOT);
            MaociRegions = MaociTest.GetRegions(EMREGIONTYPE.EMRT_MAOCIREGION);
            ThickRegions = MaociTest.GetRegions(EMREGIONTYPE.EMRT_THICKREGION);

            AlgorithmOut[AlgorithmOut.c_SpMaoci][AlgorithmOut.c_DeMaoci].Region = MaociRegions;
            AlgorithmOut[AlgorithmOut.c_SpThick][AlgorithmOut.c_DeThick].Region = ThickRegions;

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
        public EMDETECTRESULT DetectFpga(int width, int height, int stride, IntPtr data, SMaociAlgorParamFpga detectParamFpga)
        {
            EMDETECTRESULT result = MaociTest.TestFpga(width, height, stride, data, detectParamFpga);
            DarkTopRegion = MaociTest.GetRegion(EMREGIONTYPE.EMRT_DARKTOP);
            DarkBotRegion = MaociTest.GetRegion(EMREGIONTYPE.EMRT_DARKBOT);
            LightTopRegion = MaociTest.GetRegion(EMREGIONTYPE.EMRT_LIGHTTOP);
            LightBotRegion = MaociTest.GetRegion(EMREGIONTYPE.EMRT_LIGHTBOT);
            MaociRegions = MaociTest.GetRegions(EMREGIONTYPE.EMRT_MAOCIREGION);
            ThickRegions = MaociTest.GetRegions(EMREGIONTYPE.EMRT_THICKREGION);

            AlgorithmOut[AlgorithmOut.c_SpMaoci][AlgorithmOut.c_DeMaoci].Region = MaociRegions;
            AlgorithmOut[AlgorithmOut.c_SpThick][AlgorithmOut.c_DeThick].Region = ThickRegions;

            return result;
        }
    }
}
