using AlgorithmDll;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WH.Entity.CommonLib;
using WH.RecipeCellRootBase;

namespace WH.RunCell
{
    public class CellDetection : CellDetectionBase<CellDetection>
    {
        public CellDetection() : base()
        {

        }
        /// <summary>
        /// 输出区域
        /// </summary>
        public List<SRegion> regionOut { get; set; }
        /// <summary>
        /// 缺陷顺序
        /// </summary>
        public int Index { get; set; }
        public int ID { get; set; }
        /// <summary>
        /// 缺陷名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 算法缺陷名称
        /// </summary>
        public string RecipeDefectName { get; set; }
        /// <summary>
        /// 检测类
        /// </summary>
        public string Type { get; set; }

        public Category Category { get; set; } = Category.区域;
        /// <summary>
        /// 缺陷颜色
        /// </summary>
        public KnownColor ShowColor { get; set; }
        /// <summary>
        /// 优先级
        /// </summary>
        public int Priority { get; set; } = 0;
        /// <summary>
        /// 20240705 TCG
        /// 质量等级
        /// </summary>
        public dynamic Quality { get; set; }
        public dynamic DefectFilter { get; set; }
        public override void Dispose()
        {
            base.Dispose();
        }

        public override CellDetection Clone()
        {
            CellDetection detection = base.Clone();
            detection.ID = this.ID;
            detection.Name = this.Name;
            detection.Type = this.Type;
            detection.Category = this.Category;
            detection.RecipeDefectName = this.RecipeDefectName;

            if (this.ShowColor != null)
            {
                detection.ShowColor = this.ShowColor;
            }
            detection.Priority = this.Priority;
            detection.Quality = this.Quality;
            return detection;
        }
    }
}
