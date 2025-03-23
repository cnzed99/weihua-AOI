using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AlgorithmDll;

namespace OcrAlgorithm
{
    public class PlugIn : IAlgorithm
    {
        /// <summary>
        /// 2024.09.04 李焕彬
        /// 创建新算法
        /// </summary>
        /// <returns>算法参数</returns>
        public CAlgorithmParamBase CreateNewAlgorithm()
        {
            CAlgorithmParam cAlgorithmParam = new CAlgorithmParam();
            return cAlgorithmParam;
        }
    }
}
