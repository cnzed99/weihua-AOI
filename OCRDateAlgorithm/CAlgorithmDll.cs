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

namespace OCRDateAlgorithm
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
        /// 2025.01.09 易群生
        /// 日期背景区域分割阈值
        /// </summary>
        public int DarkThresh = 50;

        /// <summary>
        /// 2025.01.09 易群生
        /// 构造
        /// </summary>
        /// <param name="param">算法参数类</param>
        public SOCRDateAlgorParam(CPcParam param)
        {
            this.StandardDate = param.StandardDate;
            this.DarkThresh = param.DarkThresh;
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
            this.DarkThresh = param.DarkThresh;
        }
    };
}
