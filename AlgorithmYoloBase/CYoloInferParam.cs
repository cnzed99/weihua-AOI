using System.ComponentModel;
using AlgorithmDll;
using WH.Entity.CommonLib;

namespace AlgorithmYoloBase
{
    /// CParam 的公共字段
    public class CYoloInferParam : CParamBase
    {
        public CYoloInferParam() { }

        public CYoloInferParam(string name, Token token) : base(name, token) { }

        [Category("算法参数")]
        [DisplayName("01、设备")]
        [Description("推理设备")]
        public string CurrentDevice { get; set; } = "CPU";

        [Category("算法参数")]
        [DisplayName("01 分数阈值")]
        [Description("分数阈值")]
        public float Score { get; set; } = 0.3f;

        public float Nms { get; set; } = 0.5f;
    }
}
