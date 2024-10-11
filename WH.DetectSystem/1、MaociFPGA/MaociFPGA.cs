using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using AlgorithmDll;
using WH.RunCell;

namespace WH.DetectSystem
{
    /// <summary>
    /// 20240704 TCG
    /// PC算法阶段 算法执行控制
    /// </summary>
    public static class CMaociStage
    {
        /// <summary>
        /// 20240704 TCG
        /// 毛刺FPGA算法执行，FPGA参数扩展方法
        /// </summary>
        /// <param name="paramFpga">FPGA参数</param>
        /// <param name="cell">检测对象</param>
        public static void MaociFPGAExcute(this CAlgorithmParamBase paramMaoci, Cell cell)
        {
            cell.AlgoriDetectResult = paramMaoci.DetectFpga(cell);
        }

        /// <summary>
        /// 20240704 TCG
        /// 毛刺PC算法执行，毛刺检测算法扩展方法
        /// </summary>
        /// <param name="paramMaoci">毛刺算法参数</param>
        /// <param name="cell">检测对象</param>
        public static void MaociExcute(this CAlgorithmParamBase paramMaoci, Cell cell)
        {
            cell.AlgoriDetectResult = paramMaoci.DetectImage(cell);
        }
    }
}
