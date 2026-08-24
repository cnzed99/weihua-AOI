using AlgorithmDll;

namespace GearTestAlgorihm
{
    public class PlugIn : IAlgorithm
    {
        /// <summary>
        /// 【盘齿方案3.4-注释】user = 制程名。
        /// </summary>
        public CAlgorithmParamBase CreateNewAlgorithm(string user)
        {
            return new CGearTestAlgorihmParam(user);
        }
    }
}
