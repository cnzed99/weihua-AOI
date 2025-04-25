using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AlgorithmDll;
using Newtonsoft.Json;

namespace GeneralML
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
            CYoloAlgorithmParam cAlgorithmParam = new CYoloAlgorithmParam();
            return cAlgorithmParam;
        }

        ///// <summary>
        ///// 初始化算法
        ///// 2024.09.04 李焕彬
        ///// </summary>
        ///// <param name="algorithmParamBase">算法参数基类</param>
        ///// <returns>算法参数</returns>
        //public CAlgorithmParamBase Init(CAlgorithmParamBase algorithmParamBase)
        //{
        //    CAlgorithmParam algorithmParam = new CAlgorithmParam();
        //    if (algorithmParamBase != null)
        //    {
        //        algorithmParam = JsonConvert.DeserializeObject<CAlgorithmParam>(
        //            JsonConvert.SerializeObject(algorithmParamBase)
        //        );
        //        algorithmParam.PcParams = new ObservableCollection<CPcParamBase>();
        //        foreach (var para in algorithmParamBase.PcParams)
        //        {
        //            algorithmParam.PcParams.Add(para as CPcParam);
        //        }
        //        algorithmParam.FpgaParams = new ObservableCollection<CFpgaParamBase>();
        //        foreach (var para in algorithmParamBase.FpgaParams)
        //        {
        //            algorithmParam.FpgaParams.Add(para as CFpgaParam);
        //        }
        //    }
        //    return algorithmParam;
        //}
    }
}