using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using AlgorithmDll;
using WH.RecipeCellRootBase;
using HalconDotNet;

namespace BottleInspectionAlgorithm
{
    /// <summary>
    /// 2025.01.09 易群生
    /// PC算法参数
    /// </summary>
    public struct SOCRDateAlgorParam
    {
        /// <summary>
        /// 2025.01.09 易群生
        /// 标准日期的来源：1 固定（由UI界面输入）；2 其他来源(从客户服务器读取)
        /// </summary>
        //public int StandardDateSource = 1;

        /// <summary>
        /// 标准日期
        /// </summary>
        public String StandardDate = "112412112024/12/232026/12/22B";

        /// <summary>
        /// 2025.03.20 易群生
        /// 标签位置最上面的行位置
        /// </summary>
        public int LabelMinRow = 00;

        /// <summary>
        /// 2025.03.20 易群生
        /// 标签位置最下面的行位置
        /// </summary>
        public int LabelMaxRow = 1000;

        /// <summary>
        /// 2025.03.20 易群生
        /// 三期位置最上面的行位置
        /// </summary>
        public int DateMinRow = 00;

        /// <summary>
        /// 2025.03.20 易群生
        /// 三期位置最下面的行位置
        /// </summary>
        public int DateMaxRow = 1000;

        /// <summary>
        /// 2025.03.20 易群生
        /// 瓶盖位置最上面的行位置
        /// </summary>
        public int CapMinRow = 00;

        /// <summary>
        /// 2025.03.20 易群生
        /// 瓶盖位置最下面的行位置
        /// </summary>
        public int CapMaxRow = 1000;

        /// <summary>
        /// /// 2025.03.10 易群生
        /// 0 代表0度方向，90代表顺时针旋转90度方向，180代表顺时针旋转180方向，270代表顺时针旋转270度方向
        /// </summary>
        public int DateAngleIndex = 0;

        /// <summary>
        /// 2025.03.15 易群生
        /// 柱形物体的半径，单位是mm
        /// </summary>
        public double CylinderRadiusMM = 11;

        /// <summary>
        /// 2025.03.15 易群生
        /// 图像的像素实际尺寸，单位是mm
        /// </summary>
        public double PixelSizeMM = 0.058;



        /// <summary>
        /// 2025.01.09 易群生
        /// 构造
        /// </summary>
        /// <param name="param">算法参数类</param>
        public SOCRDateAlgorParam(CPcParam param)
        {
            this.StandardDate = param.StandardDate;
            this.LabelMinRow = param.LabelMinRow;
            this.LabelMaxRow = param.LabelMaxRow;
            this.DateMinRow = param.DateMinRow;
            this.DateMaxRow = param.DateMaxRow;
            this.DateAngleIndex = param.DateAngleIndex;
            this.CylinderRadiusMM = param.CylinderRadiusMM;
            this.PixelSizeMM = param.PixelSizeMM;

            this.CapMinRow = param.CapMinRow;
            this.CapMaxRow = param.CapMaxRow;
        }

        /// <summary>
        /// 2025.01.09 易群生
        /// 获取像素级的算法参数
        /// </summary>
        /// <param name="umPerPixel">像素当量</param>
        /// <returns></returns>
        public SOCRDateAlgorParam(CPcParam param, double umPerPixel)
        {
            this.StandardDate = param.StandardDate;
            this.LabelMinRow = param.LabelMinRow;
            this.LabelMaxRow = param.LabelMaxRow;
            this.DateMinRow = param.DateMinRow;
            this.DateMaxRow = param.DateMaxRow;
            this.DateAngleIndex = param.DateAngleIndex;
            this.CylinderRadiusMM = param.CylinderRadiusMM;
            this.PixelSizeMM = param.PixelSizeMM;

            this.CapMinRow = param.CapMinRow;
            this.CapMaxRow = param.CapMaxRow;
        }
    };
}
