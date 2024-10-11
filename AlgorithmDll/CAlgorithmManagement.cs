using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgorithmDll
{
    /// <summary>
    /// 2024.09.04 李焕彬
    /// 算法插件静态管理类
    /// </summary>
    public class CAlgorithmManagement
    {
        public CAlgorithmManagement()
        {
            AlgorithmHeper = CLoadAlgorithmPlugs.LoadAlgorithm();
        }

        /// <summary>
        /// 2024.9.4 李焕彬
        /// 算法字典
        /// </summary>
        public static Dictionary<string, IAlgorithm> AlgorithmHeper { get; set; }
    }
}
