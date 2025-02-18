using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using AlgorithmDll;
using WH.RecipeCellRootBase;

namespace SideAlgorithm
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
        /// 过滤矩阵邻域大小
        /// </summary>
        public uint NeighbSize = 7;

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 料区阈值
        /// </summary>
        public uint DarkThresh = 200;

        /// <summary>
        /// 2024.8.19 李焕彬
        /// 最小清晰度
        /// </summary>
        public float MinDistinct = 4.0f;

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 构造
        /// </summary>
        /// <param name="param">算法参数类</param>
        public SMaociAlgorParam(CPcParam param)
        {
            this.NeighbSize = param.NeighbSize;
            this.DarkThresh = param.DarkThresh;
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
            this.NeighbSize = param.NeighbSize;
            this.DarkThresh = param.DarkThresh;
            this.TimeOut = param.TimeOut;
            this.MinDistinct = param.MinDistinct;
        }
    };

    /// <summary>
    /// 2024.6.25 李焕彬
    /// 初筛算法参数
    /// </summary>
    public struct SMaociAlgorPreParam
    {
        /// <summary>
        /// 2024.7.30 李焕彬
        /// 计算超时时间，单位ms
        /// </summary>
        public uint TimeOut = 3000;

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 过滤矩阵邻域大小
        /// </summary>
        public uint NeighbSize = 7;

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 料区阈值
        /// </summary>
        public uint DarkThresh = 200;

        /// <summary>
        /// 2024.8.19 李焕彬
        /// 毛刺斜率限制
        /// </summary>
        public double maociLimit = 2.5;

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 构造
        /// </summary>
        /// <param name="param">算法参数类</param>
        public SMaociAlgorPreParam(CPcParam param)
        {
            this.NeighbSize = param.NeighbSize;
            this.DarkThresh = param.DarkThresh;
            this.TimeOut = param.TimeOut;
            this.maociLimit = param.MinDistinct;
        }

        /// <summary>
        /// 2024.8.29 李焕彬
        /// 获取像素级的算法参数
        /// </summary>
        /// <param name="umPerPixel">像素当量</param>
        /// <returns></returns>
        public SMaociAlgorPreParam(CPcParam param, double umPerPixel)
        {
            this.NeighbSize = param.NeighbSize;
            this.DarkThresh = param.DarkThresh;
            this.TimeOut = param.TimeOut;
            this.maociLimit = param.MaociLimit / umPerPixel;
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
            ptrDataX = Marshal.AllocHGlobal(sizeof(short) * 50000);
            ptrDataY = Marshal.AllocHGlobal(sizeof(short) * 50000);
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
            short[] Xs = new short[50000];
            short[] Ys = new short[50000];
            Marshal.Copy(ptrDataX, Xs, 0, 50000);
            Marshal.Copy(ptrDataY, Ys, 0, 50000);
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
                this.maociRegion[j].regionInfo.BotHeight *= umPerPixel;
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
                this.thickRegion[j].regionInfo.BotHeight *= umPerPixel;
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
        #region 毛刺算法
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
        [DllImport("MaociAlg.dll", EntryPoint = "TestSide")]
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
        [DllImport("MaociAlg.dll", EntryPoint = "TestFpgaSide")]
        public static extern EMDETECTRESULT TestFpga(
            int width,
            int height,
            int nLine,
            IntPtr data,
            ref SMaociAlgorPreParam detectParam,
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
        /// <param name="algType">算法类型，2背光专用</param>
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

    /// <summary>
    /// 2024.6.25 李焕彬
    /// 检测结果
    /// </summary>
    public enum EMDETECTRESULT
    {
        EMDR_OK = 0, //检测OK
        EMDR_NG_EDGE = 1, //边缘NG
        EMDR_NG_EMPTY = 3, //空白NG
        EMDR_TIMEOUT = 4, //检测超时
        EMDR_LOSEFOCUS = 5, //失焦异常
    };
}
