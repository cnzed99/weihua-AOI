using AlgorithmDll;

namespace CrankTestAlgorihm
{
    /// <summary>
    /// 插件入口
    /// </summary>
    public class PlugIn : IAlgorithm
    {
        public CAlgorithmParamBase CreateNewAlgorithm(string user)
        {
            return new CCrankTestAlgorihmParam(user);
        }
    }
}
