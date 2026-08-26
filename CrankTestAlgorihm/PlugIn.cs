using AlgorithmDll;

namespace CrankTestAlgorihm
{
    /// <summary>
    /// 【曲轴方案3.4-注释】插件入口。user = 制程冻结名（端面 / 底部光滑面 / 杆面 / 底盘侧面 / 顶面 / 底面）。
    /// </summary>
    public class PlugIn : IAlgorithm
    {
        public CAlgorithmParamBase CreateNewAlgorithm(string user)
        {
            return new CCrankTestAlgorihmParam(user);
        }
    }
}
