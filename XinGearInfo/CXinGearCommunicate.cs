using Modbus;

namespace XinGearInfo
{
    /// <summary>
    /// 【新兴盘齿方案11-注释】新兴协议静态类空壳。波次1 只赋/清 com，不写 PLC、不起心跳/ID、不加载点位 JSON。
    /// 禁止抄 Gear Bore/Shaft 四路张数。点位正文见方案2（波次2）。
    /// </summary>
    public static class CXinGearCommunicate
    {
        public static CModbusCommPart com;

        public static void OnComAttached()
        {
            // 波次1：空实现。com 可为 null（离线）。
        }

        public static void Detach()
        {
            com = null;
        }
    }
}
