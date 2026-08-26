using WH.Entity.LogRecord;

namespace WH.DetectSystem.DetectSystem.CrankLine
{
    /// <summary>
    /// 【曲轴方案11-注释】曲轴工程挂接空壳。方案2 未就绪，Attach/Detach 不绑协议、不引用 CrankInfo / CGearCommunicate。
    /// </summary>
    public static class CCrankLineHost
    {
        public static void Detach()
        {
            CLogRec.Default?.Info("CCrankLineHost.Detach：方案2 未就绪，空壳");
        }

        public static void Attach()
        {
            CLogRec.Default?.Info("CCrankLineHost.Attach：方案2 未就绪，空壳（不绑协议、不下发 HD1200）");
        }
    }
}
