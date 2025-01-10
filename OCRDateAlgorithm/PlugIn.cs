using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AlgorithmDll;
using Newtonsoft.Json;

namespace OCRDateAlgorithm
{
    public class PlugIn : IAlgorithm
    {
        /// <summary>
        /// 2025.01.09 易群生
        /// 创建新算法参数
        /// </summary>
        /// <returns>算法参数</returns>
        public CAlgorithmParamBase CreateNewAlgorithm()
        {
            CAlgorithmParam cAlgorithmParam = new CAlgorithmParam();
            return cAlgorithmParam;
        }
    }

}
