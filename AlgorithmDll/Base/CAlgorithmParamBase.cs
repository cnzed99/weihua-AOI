using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Newtonsoft.Json;
using WH.Entity.Attribute;
using WH.Entity.CommonLib;
using WH.Entity.LogRecord;
using WH.RecipeCellRootBase;
using WH.RunCell;

namespace AlgorithmDll
{
    /// <summary>
    /// 2024.7.4 李焕彬
    /// 算法参数配置管理类
    /// </summary>
    public class CAlgorithmParamBase : ConfigModifyObservableBase, IRecipient<OperateMessage>
    {
        /// <summary>
        /// 2024.7.4 李焕彬
        /// 操作日志
        /// </summary>
        [property: JsonIgnore]
        [property: IgnoreModifyLog]
        public CLogRec OperateLog { get; set; } = CLogRec.Create("Operate", "D:/Data");

        /// <summary>
        /// 2024.9.6 李焕彬
        /// 所属制程名
        /// </summary>
        [property: IgnoreModifyLog]
        public string PrcessName { get; set; }

        public CAlgorithmParamBase()
        {
            this.token = new Token("", "AlgorithmDll");
            AlgorParams = new ObservableCollection<CParamBase>();
            AddParam(c_ParamName);
            UpdataAlgorParamUse();
        }

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 日志消息处理
        /// </summary>
        /// <param name="message">消息</param>
        public void Receive(OperateMessage message)
        {
            if (typeof(CAlgorithmParamBase).IsAssignableFrom(message.obj.GetType()))
            {
                OperateLog.Info($"{PrcessName}-算法参数-{message.message}");
                UpdataAlgorParamUse();
                return;
            }
            foreach (var qua in AlgorParams)
            {
                if (typeof(CParamBase).IsAssignableFrom(message.obj.GetType()))
                {
                    if (qua == message.obj)
                    {
                        OperateLog.Info($"{PrcessName}-PC参数-{qua.Name}-{message.message}");
                        UpdataAlgorParamUse();
                        return;
                    }
                    continue;
                }
            }
        }

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 默认分组名
        /// </summary>
        private const string c_ParamName = "分组1";

        private ObservableCollection<CParamBase> algorParams;

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 参数列表
        /// </summary>
        [property: DisplayName("参数列表")]
        public ObservableCollection<CParamBase> AlgorParams
        {
            get { return algorParams; }
            set { SetProperty(ref algorParams, value); }
        }

        private string paramSelect = c_ParamName;

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 当前算法参数组
        /// </summary>
        [property: DisplayName("当前算法参数组")]
        public string ParamSelect
        {
            get { return paramSelect; }
            set { SetProperty(ref paramSelect, value); }
        }

        ///// <summary>
        ///// 2024.9.29 李焕彬
        ///// 类型，插件dll名
        ///// </summary>
        //public string AlgorithmType { get; set; }

        /// <summary>
        /// 2024.7.17 李焕彬
        /// 缺陷检测类
        /// </summary>
        public List<CDefectSpecies> DefectSpecies { get; set; }

        /// <summary>
        /// 2024.10.21 李焕彬
        /// 缺陷特征集合
        /// 与过滤分选缺陷特征对应
        /// </summary>
        public List<CFeacture> DefectFeatures { get; set; }

        /// <summary>
        /// 2024.7.17 李焕彬
        /// 更新毛刺参数结构体
        /// </summary>
        public virtual void UpdataAlgorParamUse() { }

        /// <summary>
        /// 2024.9.4 李焕彬
        /// 增加PC参数
        /// </summary>
        public virtual void AddParam(string name) { }

        public virtual void DetectImage(Cell cell)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 2024.9.29 李焕彬
        /// 获取清晰度计算函数
        /// </summary>
        /// <returns>清晰度计算函数</returns>
        /// <exception cref="NotImplementedException"></exception>
        public virtual Func<CImage, float> GetDistinctFunc()
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// 2024.6.25 李焕彬
    /// PC算法参数
    /// </summary>
    public class CParamBase : ConfigModifyObservableBase
    {
        public CParamBase()
        {
            this.token = new Token("", "AlgorithmDll");
        }

        public CParamBase(string name, Token token)
        {
            this.token = token;
            Name = name;
        }

        private string name = "";

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 分组名
        /// </summary>
        [property: Category("1.GroupName")]
        [property: DisplayName("分组名")]
        [property: Description("自定义名称")]
        public new string Name
        {
            get { return name; }
            set { SetProperty(ref name, value); }
        }

        /// <summary>
        /// 2024.7.4 李焕彬
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Name;
        }
    };
}
