using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using HandyControl.Controls;
using WH.DetectSystem.ViewModels;
using XinGearInfo;

namespace 断面毛刺检测软件.Views.XinGearProduct
{
    /// <summary>新兴型号选择及侧面拍照张数。</summary>
    public class XinGearProductVM : ObservableObject
    {
        private readonly CMainModelsModelVM mainModelVM;
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
                OnPropertyChanged(nameof(SidePhotoCount));
                if (isLoading || value == null) return;

                if (mainModelVM != null && !mainModelVM.TryChangeXinGearModel(value, out string switchError))
                {
                    RestoreSelection(previous);
                    Growl.Error("新兴盘齿型号切换失败：" + switchError);
                    return;
                }
                if (CXinGearProductCatalog.TrySaveSelection(value.Id, out string error)) return;

                if (previous != null && mainModelVM != null
                    && !mainModelVM.TryChangeXinGearModel(previous, out string rollbackError))
                {
                    mainModelVM.IsStart = false;
                    mainModelVM.StartStop = false;
                    Growl.Error("型号保存失败且 PLC 回退失败，检测已停止：" + rollbackError);
                }
                RestoreSelection(previous);
                Growl.Error("新兴盘齿型号保存失败：" + error);
            }
        }

        public int? ToothCount => SelectedModel?.ToothCount;
        public int? SidePhotoCount => SelectedModel?.EffectiveSidePhotoCount;
        public bool IsCatalogAvailable => Models.Count > 0;
        public string CatalogError { get; private set; }
        public string CatalogErrorDetail { get; private set; }

        public XinGearProductVM(CMainModelsModelVM mainModelVM = null)
        {
            this.mainModelVM = mainModelVM;
            Models = new ObservableCollection<CXinGearProductModel>();
            ReloadFromCatalog();
        }

        private void RestoreSelection(CXinGearProductModel previous)
        {
            isLoading = true;
            SelectedModel = previous;
            isLoading = false;
        }

        public void ReloadFromCatalog()
        {
            isLoading = true;
            try
            {
                CXinGearProductCatalog catalog = CXinGearProductCatalog.Load();
                Models.Clear();
                foreach (CXinGearProductModel model in catalog.Models) Models.Add(model);
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
