using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using GearInfo;

namespace 断面毛刺检测软件.Views.GearProduct
{
    /// <summary>
    /// 【盘齿方案8-注释】一期薄 VM：切 Combo 只刷新展示数字，不写 JSON、不写 PLC。
    /// </summary>
    public partial class GearProductVM : ObservableObject
    {
        public CGearProductCatalog Catalog { get; }

        public ObservableCollection<CGearProductModel> Models { get; }

        [ObservableProperty]
        private CGearProductModel selectedModel;

        public GearProductVM()
        {
            Catalog = CGearProductCatalog.Load();
            Models = new ObservableCollection<CGearProductModel>(
                Catalog.Models ?? new System.Collections.Generic.List<CGearProductModel>());
            SelectedModel = Catalog.FindCurrentModel();
        }
    }
}
