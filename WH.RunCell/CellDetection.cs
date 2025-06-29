using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WH.RecipeCellRootBase;

namespace WH.RunCell
{
    public class CellDetection : CellDetectionBase<CellDetection>
    {
        public CellDetection()
            : base() { }

        /// <summary>
        /// 缺陷顺序
        /// </summary>
        public int Index { get; set; }
        public int ID { get; set; }

        ///// <summary>
        ///// 缺陷名称 DefectFilter里有
        ///// </summary>
        //public string Name { get; set; }
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
        /// 在那个窗口显示区域
        /// </summary>
        public int ShowInView { get; set; } = 0;

        ///// <summary>
        ///// 缺陷颜色 DefectFilter里有
        ///// </summary>
        //public CKnownColor ShowColor { get; set; }
        ///// <summary>
        ///// 优先级 DefectFilter里有
        ///// </summary>
        //public int Priority { get; set; } = 0;
        /// <summary>
        /// 20240705 TCG
        /// 缺陷过滤器，包含缺陷类型，质量等级所有信息
        /// </summary>
        public dynamic DefectFilter { get; set; }

        public override void Dispose()
        {
            base.Dispose();
        }

        public override CellDetection Clone()
        {
            CellDetection detection = base.Clone();
            detection.ID = this.ID;
            detection.Index = this.Index;
            detection.Type = this.Type;
            detection.Category = this.Category;
            detection.RecipeDefectName = this.RecipeDefectName;
            detection.DefectFilter = this.DefectFilter;
            detection.ShowInView = this.ShowInView;
            return detection;
        }
    }
}
