using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using AlgorithmDll;
using WH.RecipeCellRootBase;

namespace FrontAlgorithm
{
    /// <summary>
    /// 2024.6.25 李焕彬
    /// PC算法参数
    /// </summary>
    public struct SMaociAlgorParam
    {
        /// <summary>
        /// 2024.7.30 李焕彬
        /// 计算超时时间，单位ms
        /// </summary>
        public uint TimeOut = 3000;

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
        public double LightThick = 6;

        /// <summary>
        /// 2024.8.19 李焕彬
        /// 最小清晰度
        /// </summary>
        public float MinDistinct = 25.0f;

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 构造
        /// </summary>
        /// <param name="param">算法参数类</param>
        public SMaociAlgorParam(CPcParam param)
        {
            this.AdaptiveSize = param.AdaptiveSize;
            this.AdaptiveAddGray = param.AdaptiveAddGray;
            this.NeighbSize = param.NeighbSize;
            this.NeighbLightPoint = param.NeighbLightPoint;
            this.DarkThresh = param.DarkThresh;
            this.LightThresh = param.LightThresh;
            this.LightThick = param.LightThick;
            this.TimeOut = param.TimeOut;
            this.MinDistinct = param.MinDistinct;
        }

        /// <summary>
        /// 2024.8.29 李焕彬
        /// 获取像素级的算法参数
        /// </summary>
        /// <param name="umPerPixel">像素当量</param>
        /// <returns></returns>
        public SMaociAlgorParam(CPcParam param, double umPerPixel)
        {
            this.AdaptiveSize = param.AdaptiveSize;
            this.AdaptiveAddGray = param.AdaptiveAddGray;
            this.NeighbSize = param.NeighbSize;
            this.NeighbLightPoint = param.NeighbLightPoint;
            this.DarkThresh = param.DarkThresh;
            this.LightThresh = param.LightThresh;
            this.LightThick = param.LightThick / umPerPixel;
            this.TimeOut = param.TimeOut;
            this.MinDistinct = param.MinDistinct;
        }
    };

    /// <summary>
    /// 2024.6.25 李焕彬
    /// Fpga算法参数
    /// </summary>
    public struct SMaociAlgorParamFpga
    {
        /// <summary>
        /// 2024.7.30 李焕彬
        /// 计算超时时间，单位ms
        /// </summary>
        public uint TimeOut = 3000;

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
        public double DarkThickLimit = 30;

        /// <summary>
        /// 2024.7.4 李焕彬
        /// //料区厚度NG连续长度限制
        /// </summary>
        public double DarkThickContinueLen = 5;

        /// <summary>
        /// 2024.7.4 李焕彬
        /// //料区厚度
        /// </summary>
        public double DarkThick = 84;

        /// <summary>
        /// 2024.7.4 李焕彬
        /// //铝层厚度限制，毛刺检测
        /// </summary>
        public double LightThickLimit = 7;

        /// <summary>
        /// 2024.7.4 李焕彬
        /// //铝层厚度NG连续长度限制
        /// </summary>
        public double LightThickContinueLen = 0;

        /// <summary>
        /// 2024.7.4 李焕彬
        /// //铝层厚度
        /// </summary>
        public double LightThick = 6;

        /// <summary>
        /// 2024.7.4 李焕彬
        /// //铝层在料区中心位置限制上
        /// </summary>
        public double PosLimitT = 20;

        /// <summary>
        /// 2024.7.4 李焕彬
        /// //铝层在料区中心位置限制下
        /// </summary>
        public double PosLimitB = 20;

        /// <summary>
        /// 2024.7.4 李焕彬
        /// //铝层位置偏移值
        /// </summary>
        public double LightPosOffest = 0;

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 构造
        /// </summary>
        /// <param name="param">FPGA算法参数类</param>
        public SMaociAlgorParamFpga(CFpgaParam param)
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
            this.TimeOut = param.TimeOut;
        }

        /// <summary>
        /// 2024.8.29 李焕彬
        /// 获取像素级的算法参数
        /// </summary>
        /// <param name="umPerPixel">像素当量</param>
        /// <returns></returns>
        public SMaociAlgorParamFpga(CFpgaParam param, double umPerPixel)
        {
            this.AdaptiveSize = param.AdaptiveSize;
            this.AdaptiveAddGray = param.AdaptiveAddGray;
            this.NeighbSize = param.NeighbSize;
            this.NeighbLightPoint = param.NeighbLightPoint;
            this.DarkThresh = param.DarkThresh;
            this.LightThresh = param.LightThresh;
            this.DarkThickLimit = param.DarkThickLimit / umPerPixel;
            this.DarkThickContinueLen = param.DarkThickContinueLen / umPerPixel;
            this.DarkThick = param.DarkThick / umPerPixel;
            this.LightThickLimit = param.LightThickLimit / umPerPixel;
            this.LightThickContinueLen = param.LightThickContinueLen / umPerPixel;
            this.LightThick = param.LightThick / umPerPixel;
            this.PosLimitT = param.PosLimitT / umPerPixel;
            this.PosLimitB = param.PosLimitB / umPerPixel;
            this.LightPosOffest = param.LightPosOffest / umPerPixel;
            this.TimeOut = param.TimeOut;
        }
    };

    /// <summary>
    /// 2024.9.11 李焕彬
    /// DLL传输区域用
    /// 结构体顺序要跟DLL结构体顺序一致，不能更改
    /// </summary>
    public struct SRegionEdge
    {
        /// <summary>
        /// 2024.9.11 李焕彬
        /// 点数
        /// </summary>
        public int count;

        /// <summary>
        /// 2024.9.11 李焕彬
        /// 区域点集
        /// </summary>
        public int start;
    };

    /// <summary>
    /// 2024.9.11 李焕彬
    /// DLL传输区域用
    /// 结构体顺序要跟DLL结构体顺序一致，不能更改
    /// </summary>
    public struct SRegionTransform
    {
        /// <summary>
        /// 2024.9.11 李焕彬
        /// 区域信息
        /// </summary>
        public SRegionInfo regionInfo;

        /// <summary>
        /// 2024.9.12 李焕彬
        /// 区域点
        /// </summary>
        public SRegionEdge regionEdge;
    }

    /// <summary>
    /// 2024.9.11 李焕彬
    /// 检测信息，dll检测结果
    /// 结构体顺序要跟DLL结构体顺序一致，不能更改
    /// </summary>
    public struct SDetectInfo
    {
        /// <summary>
        /// 2024.9.11 李焕彬
        /// 料区边缘上
        /// </summary>
        public SRegionEdge regionDarkTop = new SRegionEdge();

        /// <summary>
        /// 2024.9.11 李焕彬
        /// 料区边缘下
        /// </summary>
        public SRegionEdge regionDarkBot = new SRegionEdge();

        /// <summary>
        /// 2024.9.11 李焕彬
        /// 铝层边缘上
        /// </summary>
        public SRegionEdge regionLightTop = new SRegionEdge();

        /// <summary>
        /// 2024.9.11 李焕彬
        /// 铝层边缘下
        /// </summary>
        public SRegionEdge regionLightBot = new SRegionEdge();

        /// <summary>
        /// 2024.9.11 李焕彬
        /// 毛刺区域数
        /// </summary>
        public int maociCount = 0;

        /// <summary>
        /// 2024.9.11 李焕彬
        /// 毛刺区域集
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 100)]
        public SRegionTransform[] maociRegion = new SRegionTransform[100];

        /// <summary>
        /// 2024.9.11 李焕彬
        /// 掉料区域数
        /// </summary>
        public int thickCount = 0;

        /// <summary>
        /// 2024.9.11 李焕彬
        /// 掉料区域集
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 100)]
        public SRegionTransform[] thickRegion = new SRegionTransform[100];

        /// <summary>
        /// 2024.9.12 李焕彬
        /// X坐标数据指针
        /// </summary>
        public IntPtr ptrDataX = IntPtr.Zero;

        /// <summary>
        /// 2024.9.12 李焕彬
        /// Y坐标数据指针
        /// </summary>
        public IntPtr ptrDataY = IntPtr.Zero;

        public SDetectInfo()
        {
            ptrDataX = Marshal.AllocHGlobal(sizeof(short) * 20000);
            ptrDataY = Marshal.AllocHGlobal(sizeof(short) * 20000);
        }

        /// <summary>
        /// 2024.9.12 李焕彬
        /// 分解输出区域,初始化后必须执行且只能执行一次
        /// </summary>
        /// <param name="edgeDarkTop">料区上边缘</param>
        /// <param name="edgeDarkBot">料区下边缘</param>
        /// <param name="edgeLightTop">铝层上边缘</param>
        /// <param name="edgeLightBot">铝层下边缘</param>
        /// <param name="maociRegion">毛刺区域</param>
        /// <param name="thickRegion">掉料区域</param>
        public void DeComposeEdge(
            double umPerPixel,
            out List<Point> edgeDarkTop,
            out List<Point> edgeDarkBot,
            out List<Point> edgeLightTop,
            out List<Point> edgeLightBot,
            out List<SRegion> maociRegion,
            out List<SRegion> thickRegion
        )
        {
            short[] Xs = new short[20000];
            short[] Ys = new short[20000];
            Marshal.Copy(ptrDataX, Xs, 0, 20000);
            Marshal.Copy(ptrDataY, Ys, 0, 20000);
            //Marshal.FreeHGlobal(ptrDataX);
            //Marshal.FreeHGlobal(ptrDataY);
            edgeDarkTop = new List<Point>();
            for (int i = regionDarkTop.start; i < regionDarkTop.start + regionDarkTop.count; i++)
            {
                edgeDarkTop.Add(new(Xs[i], Ys[i]));
            }
            edgeDarkBot = new List<Point>();
            for (int i = regionDarkBot.start; i < regionDarkBot.start + regionDarkBot.count; i++)
            {
                edgeDarkBot.Add(new(Xs[i], Ys[i]));
            }
            edgeLightTop = new List<Point>();
            for (int i = regionLightTop.start; i < regionLightTop.start + regionLightTop.count; i++)
            {
                edgeLightTop.Add(new(Xs[i], Ys[i]));
            }
            edgeLightBot = new List<Point>();
            for (int i = regionLightBot.start; i < regionLightBot.start + regionLightBot.count; i++)
            {
                edgeLightBot.Add(new(Xs[i], Ys[i]));
            }
            maociRegion = new List<SRegion>();
            for (int j = 0; j < maociCount; j++)
            {
                var lsPoint = new List<Point>();
                for (
                    int i = this.maociRegion[j].regionEdge.start;
                    i < this.maociRegion[j].regionEdge.start + this.maociRegion[j].regionEdge.count;
                    i++
                )
                {
                    lsPoint.Add(new(Xs[i], Ys[i]));
                }
                this.maociRegion[j].regionInfo.WidthBound *= umPerPixel;
                this.maociRegion[j].regionInfo.HeightBound *= umPerPixel;
                this.maociRegion[j].regionInfo.PeakHeight *= umPerPixel;
                this.maociRegion[j].regionInfo.LongLen *= umPerPixel;
                this.maociRegion[j].regionInfo.ShorLen *= umPerPixel;
                this.maociRegion[j].regionInfo.ContLen *= umPerPixel;
                this.maociRegion[j].regionInfo.Area *= umPerPixel;
                maociRegion.Add(new SRegion(this.maociRegion[j].regionInfo, lsPoint));
            }
            thickRegion = new List<SRegion>();
            for (int j = 0; j < thickCount; j++)
            {
                var lsPoint = new List<Point>();
                for (
                    int i = this.thickRegion[j].regionEdge.start;
                    i < this.thickRegion[j].regionEdge.start + this.thickRegion[j].regionEdge.count;
                    i++
                )
                {
                    lsPoint.Add(new(Xs[i], Ys[i]));
                }
                this.thickRegion[j].regionInfo.WidthBound *= umPerPixel;
                this.thickRegion[j].regionInfo.HeightBound *= umPerPixel;
                this.thickRegion[j].regionInfo.PeakHeight *= umPerPixel;
                this.thickRegion[j].regionInfo.LongLen *= umPerPixel;
                this.thickRegion[j].regionInfo.ShorLen *= umPerPixel;
                this.thickRegion[j].regionInfo.ContLen *= umPerPixel;
                this.thickRegion[j].regionInfo.Area *= umPerPixel;
                thickRegion.Add(new SRegion(this.thickRegion[j].regionInfo, lsPoint));
            }
        }
    }

    /// <summary>
    /// 2024.9.11 李焕彬
    /// 算法dll调用类
    /// </summary>
    public class CAlgorithmDll
    {
        #region 获取区域边缘点集合
        /// <summary>
        /// 2024.7.4 李焕彬
        /// 获取区域点
        /// </summary>
        /// <param name="type">区域类型</param>
        /// <returns></returns>
        [DllImport("MaociAlg.dll")]
        private static extern int GetEdgeCount(EMREGIONTYPE type, string GUID);

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 获取区域
        /// </summary>
        /// <param name="type">类型</param>
        /// <param name="ptrX">输出指针X</param>
        /// <param name="ptrY">输出指针Y</param>
        [DllImport("MaociAlg.dll")]
        private static extern void GetEdge(
            EMREGIONTYPE type,
            IntPtr ptrX,
            IntPtr ptrY,
            string GUID
        );

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 获取区域点数
        /// </summary>
        /// <param name="type">区域类型</param>
        /// <returns></returns>
        [DllImport("MaociAlg.dll")]
        private static extern int GetRegionCount(EMREGIONTYPE type, string GUID);

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 获取区域信息
        /// </summary>
        /// <param name="type">类型</param>
        /// <param name="nIndex">索引</param>
        /// <param name="sRegionInfo">输出区域信息</param>
        /// <returns></returns>
        [DllImport("MaociAlg.dll")]
        private static extern int GetRegionInfo(
            EMREGIONTYPE type,
            int nIndex,
            ref SRegionInfo sRegionInfo,
            string GUID
        );

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 获取区域点集
        /// </summary>
        /// <param name="type">类型</param>
        /// <param name="nIndex">索引</param>
        /// <param name="pX">输出指针X</param>
        /// <param name="pY">输出指针Y</param>
        [DllImport("MaociAlg.dll")]
        private static extern void GetRegionPoints(
            EMREGIONTYPE type,
            int nIndex,
            IntPtr pX,
            IntPtr pY,
            string GUID
        );

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 获取区域
        /// </summary>
        /// <param name="type">类型</param>
        /// <returns></returns>
        public static List<Point> GetRegion(EMREGIONTYPE type, string GUID)
        {
            int size = GetEdgeCount(type, GUID);
            IntPtr ptrX = Marshal.AllocHGlobal(size * sizeof(int));
            IntPtr ptrY = Marshal.AllocHGlobal(size * sizeof(int));
            GetEdge(type, ptrX, ptrY, GUID);
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
        public static List<SRegion> GetRegions(EMREGIONTYPE type, string GUID)
        {
            List<SRegion> regions = new List<SRegion>();
            int regionCount = GetRegionCount(type, GUID);
            double dMmPerPixel = 2.25d;
            for (int k = 0; k < regionCount; k++)
            {
                SRegion sRegion = new SRegion();
                int size = GetRegionInfo(type, k, ref sRegion.RegionInfo, GUID);
                sRegion.RegionInfo.WidthBound *= dMmPerPixel;
                sRegion.RegionInfo.HeightBound *= dMmPerPixel;
                sRegion.RegionInfo.PeakHeight *= dMmPerPixel;
                sRegion.RegionInfo.LongLen *= dMmPerPixel;
                sRegion.RegionInfo.ShorLen *= dMmPerPixel;
                sRegion.RegionInfo.ContLen *= dMmPerPixel;
                sRegion.RegionInfo.Area *= dMmPerPixel;
                IntPtr ptrX = Marshal.AllocHGlobal(size * sizeof(int));
                IntPtr ptrY = Marshal.AllocHGlobal(size * sizeof(int));
                GetRegionPoints(type, k, ptrX, ptrY, GUID);
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
        /// <param name="detectInfo">检测结果信息</param>
        /// <param name="dataOutX">输出点X集合</param>
        /// <param name="dataOutY">输出点Y集合</param>
        /// <returns>检测结果</returns>
        #region 毛刺算法
        [DllImport("MaociAlg.dll")]
        public static extern EMDETECTRESULT Test(
            int width,
            int height,
            int nLine,
            IntPtr data,
            ref SMaociAlgorParam detectParam,
            ref SDetectInfo detectInfo
        );

        /// <summary>
        /// 2024.7.4 李焕彬
        /// FPGA算法测试
        /// </summary>
        /// <param name="width">宽度</param>
        /// <param name="height">高度</param>
        /// <param name="nLine">行宽</param>
        /// <param name="data">图像数据</param>
        /// <param name="sDetectParamFpga">FPGA算法</param>
        /// <param name="detectInfo">检测结果信息</param>
        /// <param name="dataOutX">输出点X集合</param>
        /// <param name="dataOutY">输出点Y集合</param>
        /// <returns>检测结果</returns>
        [DllImport("MaociAlg.dll")]
        public static extern EMDETECTRESULT TestFpga(
            int width,
            int height,
            int nLine,
            IntPtr data,
            ref SMaociAlgorParamFpga sDetectParamFpga,
            ref SDetectInfo detectInfo
        );

        #endregion

        /// <summary>
        /// 2024.8.28 李焕彬
        /// 计算对焦清晰度
        /// </summary>
        /// <param name="width">图像宽度</param>
        /// <param name="height">图像高度</param>
        /// <param name="nLine">图像行宽</param>
        /// <param name="data">图像数据</param>
        /// <param name="algType">算法类型，0能量梯度，1Laplacian方差</param>
        /// <param name="nThresh">目标料区阈值，算法1使用</param>
        /// <returns></returns>
        [DllImport("MaociAlg.dll")]
        public static extern float CalcDistinct(
            int width,
            int height,
            int nLine,
            IntPtr data,
            int algType,
            int nThresh
        );
    }
}
