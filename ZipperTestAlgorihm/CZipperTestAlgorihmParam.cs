
using AlgorithmDll;
using WH.RecipeCellRootBase;
using WH.RunCell;

namespace ZipperTestAlgorihm
{
    public class CZipperTestAlgorihmParam: CAlgorithmParamBase
    {

        public CZipperTestAlgorihmParam():base()
        {
            DefectSpecies = new()
            {
                new("上止类", new() { new("上止有无", Category.区域) }),
                new("下止类",new() {  new("下止有无", Category.区域) }),
                new("拉头类",new() {  new("下止有无", Category.区域) }),
            };
            DefectFeatures = new();

            DefectFeatures.Add(new("ShortLength", "短边", "ShortLength", "um"));
            DefectFeatures.Add(new("LongLength", "长边", "LongLength", "um"));
            DefectFeatures.Add(new("Area", "面积", "Area", "um²"));
            DefectFeatures.Add(new("Score", "分数", "Score", ""));
            DefectFeatures.Add(new("Angle", "角度", "Angle", "°"));
            DefectFeatures.Add(new("Height", "高度", "Height", "um"));
            DefectFeatures.Add(new("Width", "宽度", "Width", "um"));
        }
        /// <summary>
        /// 2024.9.11 李焕彬
        /// 增加参数
        /// </summary>
        /// <param name="name">名称</param>
        public override void AddParam(string name)
        {
            this.AlgorParams.Add(new CParamBase(name, token));
        }
        /// <summary>
        /// 2024.7.17 李焕彬
        /// 更新毛刺参数结构体
        /// </summary>
        public override void UpdataAlgorParamUse() { }
        /// <summary>
        /// 2024.10.28 鲍赞宝
        /// 执行算法
        /// </summary>
        /// <param name="cell">cell</param>
        /// <returns>检测结果</returns>
        public override void DetectImage(Cell cell)
        {

        }
    }

}
