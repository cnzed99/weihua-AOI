using AlgorithmDll;

namespace PlaneGearTestAlgorihm
{
    /// <summary>
    /// 插件入口。user = 制程冻结名（齿底 / 齿顶 / 侧面）。
    /// </summary>
    public class PlugIn : IAlgorithm
    {
        public CAlgorithmParamBase CreateNewAlgorithm(string user)
        {
            return new CPlaneGearTestAlgorihmParam(user);
        }
    }
}
