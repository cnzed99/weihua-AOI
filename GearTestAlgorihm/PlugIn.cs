using AlgorithmDll;

namespace GearTestAlgorihm
{
    public class PlugIn : IAlgorithm
    {
        /// <summary>
        /// 2026.08.17 盘齿方案3.4
        /// 创建新算法。user = 制程名（下端面/上齿面/…）
        /// </summary>
        public CAlgorithmParamBase CreateNewAlgorithm(string user)
        {
            return new CGearTestAlgorihmParam(user);
        }
    }
}
