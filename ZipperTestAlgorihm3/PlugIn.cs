using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AlgorithmDll;
using Newtonsoft.Json;


namespace ZipperTestAlgorihm3
{
    public class PlugIn : IAlgorithm
    {
        /// <summary>
        /// 2024.09.04 李焕彬
        /// 创建新算法
        /// </summary>
        /// <returns>算法参数</returns>
        public CAlgorithmParamBase CreateNewAlgorithm(string user)
        {
            CZipperTestAlgorihmParam3 cAlgorithmParam = new CZipperTestAlgorihmParam3(user);
            return cAlgorithmParam;
        }

    }
}
