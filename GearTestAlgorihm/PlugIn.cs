using AlgorithmDll;

namespace GearTestAlgorihm
{
    public class PlugIn : IAlgorithm
    {
        /// <summary>
        /// user = 制程名。
        /// </summary>
        public CAlgorithmParamBase CreateNewAlgorithm(string user)
        {
            return new CGearTestAlgorihmParam(user);
        }
    }
}
