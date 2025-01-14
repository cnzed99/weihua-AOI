global using static IKapVisionCam.CIKapStatics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using CameraModule;
using IKapBoardClassLibrary;
using IKapC.NET;

namespace IKapVisionCam
{
    /// <summary>
    /// 2025.1.14 李焕彬
    /// 初筛算法参数
    /// </summary>
    public struct SMaociAlgorParam
    {
        /// <summary>
        /// 2025.1.14 李焕彬
        /// 计算超时时间，单位ms
        /// </summary>
        public uint TimeOut = 3000;

        /// <summary>
        /// 2025.1.14 李焕彬
        /// //自适应阈值邻域大小
        /// </summary>
        public uint AdaptiveSize = 14;

        /// <summary>
        /// 2025.1.14 李焕彬
        /// //自适应阈值增加值
        /// </summary>
        public int AdaptiveAddGray = 20;

        /// <summary>
        /// 2025.1.14 李焕彬
        /// //过滤矩阵邻域大小
        /// </summary>
        public uint NeighbSize = 5;

        /// <summary>
        /// 2025.1.14 李焕彬
        /// //过滤矩阵邻域点数量限制
        /// </summary>
        public uint NeighbLightPoint = 30;

        /// <summary>
        /// 2025.1.14 李焕彬
        /// //料区阈值
        /// </summary>
        public uint DarkThresh = 30;

        /// <summary>
        /// 2025.1.14 李焕彬
        /// //铝层阈值
        /// </summary>
        public uint LightThresh = 80;

        /// <summary>
        /// 2025.1.14 李焕彬
        /// //料区厚度限制，掉料检测
        /// </summary>
        public double DarkThickLimit = 30;

        /// <summary>
        /// 2025.1.14 李焕彬
        /// //料区厚度NG连续长度限制
        /// </summary>
        public double DarkThickContinueLen = 5;

        /// <summary>
        /// 2025.1.14 李焕彬
        /// //料区厚度
        /// </summary>
        public double DarkThick = 84;

        /// <summary>
        /// 2025.1.14 李焕彬
        /// //铝层厚度限制，毛刺检测
        /// </summary>
        public double LightThickLimit = 7;

        /// <summary>
        /// 2025.1.14 李焕彬
        /// //铝层厚度NG连续长度限制
        /// </summary>
        public double LightThickContinueLen = 0;

        /// <summary>
        /// 2025.1.14 李焕彬
        /// //铝层厚度
        /// </summary>
        public double LightThick = 6;

        /// <summary>
        /// 2025.1.14 李焕彬
        /// //铝层在料区中心位置限制上
        /// </summary>
        public double PosLimitT = 20;

        /// <summary>
        /// 2025.1.14 李焕彬
        /// //铝层在料区中心位置限制下
        /// </summary>
        public double PosLimitB = 20;

        /// <summary>
        /// 2025.1.14 李焕彬
        /// //铝层位置偏移值
        /// </summary>
        public double LightPosOffest = 0;

        /// <summary>
        /// 2025.1.14 李焕彬
        /// 构造
        /// </summary>
        /// <param name="param">FPGA算法参数类</param>
        public SMaociAlgorParam(CParameterSetting param)
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
            this.TimeOut = param.PreTimeOut;
        }

        /// <summary>
        /// 2024.8.29 李焕彬
        /// 获取像素级的算法参数
        /// </summary>
        /// <param name="umPerPixel">像素当量</param>
        /// <returns></returns>
        public SMaociAlgorParam(CParameterSetting param, double umPerPixel)
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
            this.TimeOut = param.PreTimeOut;
        }
    };

    public struct SSideMaociAlgorParam
    {
        /// <summary>
        /// 2025.1.14 李焕彬
        /// 计算超时时间，单位ms
        /// </summary>
        public uint TimeOut = 3000;

        /// <summary>
        /// 2025.1.14 李焕彬
        /// 过滤矩阵邻域大小
        /// </summary>
        public uint NeighbSize = 5;

        /// <summary>
        /// 2025.1.14 李焕彬
        /// 料区阈值
        /// </summary>
        public uint DarkThresh = 140;

        /// <summary>
        /// 2025.1.14 李焕彬
        /// //毛刺斜率限制
        /// </summary>
        public double maociLimit = 7;

        public SSideMaociAlgorParam(CParameterSetting param, double umPerPixel)
        {
            this.NeighbSize = param.NeighbSizeSide;
            this.DarkThresh = param.DarkThreshSide;
            this.TimeOut = param.PreTimeOut;
            this.maociLimit = param.MaociLimit / umPerPixel;
        }
    }

    public static class CIKapStatics
    {
        /// <summary>
        /// 2025.1.14 李焕彬
        /// Ikap函数返回值判断
        /// </summary>
        /// <param name="res"></param>
        /// <returns></returns>
        public static bool CheckIKapC(uint res)
        {
            if (res != (uint)ItkStatusErrorId.ITKSTATUS_OK)
            {
                CCameraManagement.CamLogger.Error($"Error Code: {res.ToString("x8")}");
                return false;
            }
            return true;
        }

        /// <summary>
        /// 2025.1.14 李焕彬
        /// IkapBoard函数返回值判断
        /// </summary>
        /// <param name="ret"></param>
        /// <returns></returns>
        public static bool CheckIKapBoard(int ret)
        {
            if (ret != (int)ErrorCode.IK_RTN_OK)
            {
                string sErrMsg = "";
                IKapBoard.IKAPERRORINFO tIKei = new IKapBoardClassLibrary.IKapBoard.IKAPERRORINFO();

                // 获取错误码信息。
                //
                // Get error code message.
                IKapBoard.IKapGetLastError(ref tIKei, true);

                // 打印错误信息。
                //
                // Print error message.
                sErrMsg = string.Concat(
                    "Error",
                    sErrMsg,
                    "Board Type\t = 0x",
                    tIKei.uBoardType.ToString("X4"),
                    "\n",
                    "Board Index\t = 0x",
                    tIKei.uBoardIndex.ToString("X4"),
                    "\n",
                    "Error Code\t = 0x",
                    tIKei.uErrorCode.ToString("X4"),
                    "\n"
                );

                CCameraManagement.CamLogger.Error(sErrMsg);
                return false;
            }
            return true;
        }

        /// <summary>
        /// 2025.1.14 李焕彬
        /// 图像数据复制
        /// </summary>
        /// <param name="src"></param>
        /// <param name="dst"></param>
        /// <param name="size"></param>
        [DllImport("GeneralAlg.dll")]
        public static extern void CopyMemory1(IntPtr src, IntPtr dst, int size);

        /// <summary>
        /// 2025.1.14 李焕彬
        /// 断面毛刺初筛算法接口
        /// </summary>
        /// <param name="srcs">图像数据集</param>
        /// <param name="size">图像个数</param>
        /// <param name="width">宽</param>
        /// <param name="height">高</param>
        /// <param name="nLine">行宽</param>
        /// <param name="algParams">算法参数组</param>
        /// <param name="result">检测结果</param>
        [DllImport("MaociAlg.dll")]
        public static extern void PreTest(
            IntPtr[] srcs,
            int size,
            int width,
            int height,
            int nLine,
            SMaociAlgorParam[] algParams,
            IntPtr result
        );

        /// <summary>
        /// 2025.1.14 李焕彬
        /// 侧面毛刺初筛算法接口
        /// </summary>
        /// <param name="srcs">图像数据集</param>
        /// <param name="size">图像个数</param>
        /// <param name="width">宽</param>
        /// <param name="height">高</param>
        /// <param name="nLine">行宽</param>
        /// <param name="algParams">算法参数组</param>
        /// <param name="result">检测结果</param>
        [DllImport("MaociAlg.dll")]
        public static extern void PreTestSide(
            IntPtr[] srcs,
            int size,
            int width,
            int height,
            int nLine,
            SSideMaociAlgorParam[] algParams,
            IntPtr result
        );
    }
}
