using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using HandyControl.Controls;
using XinGearInfo;

namespace 断面毛刺检测软件.Views.XinGearProduct
{
    /// <summary>现场新兴物料展示与选择记忆；不修改工程张数或 PLC。</summary>
    public class XinGearProductVM : ObservableObject
    {
        private CXinGearProductModel selectedModel;
        private bool isLoading;

        public ObservableCollection<CXinGearProductModel> Models { get; }

        public CXinGearProductModel SelectedModel
        {
            get => selectedModel;
            set
            {
                CXinGearProductModel previous = selectedModel;
                if (!SetProperty(ref selectedModel, value)) return;
                OnPropertyChanged(nameof(ToothCount));
                if (isLoading || value == null) return;
                if (CXinGearProductCatalog.TrySaveSelection(value.Id, out string error)) return;

                isLoading = true;
                SelectedModel = previous;
                isLoading = false;
                Growl.Error("新兴盘齿型号保存失败：" + error);
            }
        }

        public int? ToothCount => SelectedModel?.ToothCount;

        public bool IsCatalogAvailable => Models.Count > 0;

        public string CatalogError { get; private set; }

        public string CatalogErrorDetail { get; private set; }

        public XinGearProductVM()
        {
            Models = new ObservableCollection<CXinGearProductModel>();
            ReloadFromCatalog();
        }

        public void ReloadFromCatalog()
        {
            isLoading = true;
            try
            {
                CXinGearProductCatalog catalog = CXinGearProductCatalog.Load();
                Models.Clear();
                foreach (CXinGearProductModel model in catalog.Models)
                {
                    Models.Add(model);
                }

                SelectedModel = catalog.FindSelectedModel();
                CatalogError = null;
                CatalogErrorDetail = null;
            }
            catch (System.Exception ex)
            {
                Models.Clear();
                SelectedModel = null;
                CatalogError = "型号配置不可用，请检查配置文件和日志。";
                CatalogErrorDetail = ex.Message;
            }
            finally
            {
                isLoading = false;
                OnPropertyChanged(nameof(IsCatalogAvailable));
                OnPropertyChanged(nameof(CatalogError));
                OnPropertyChanged(nameof(CatalogErrorDetail));
            }
        }
    }
}
