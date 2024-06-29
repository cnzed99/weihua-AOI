using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;

namespace AlgorithmDll
{
    /// <summary>
    /// 2024.6.25 李焕彬
    /// 算法参数VM
    /// </summary>
    public partial class MaociAlgorParamCtrlVm : ObservableObject
    {
        private const string paramName = "分组";

        private const string paramName1 = "分组1";

        [ObservableProperty]
        private ObservableCollection<MaociAlgorParam> pcParams = new ObservableCollection<MaociAlgorParam>() {new MaociAlgorParam(paramName1) };

        [ObservableProperty]
        private string pcSelect = paramName1;

        [ObservableProperty]
        private ObservableCollection<MaociAlgorParamFpga> fpgaParams = new ObservableCollection<MaociAlgorParamFpga>() { new MaociAlgorParamFpga(paramName1) };

        [ObservableProperty]
        private string fpgaSelect = paramName1;

        [RelayCommand]
        public void AddPcParam()
        {
            int index = 1;
            for (int i = PcParams.Count - 1; i >= 0; i--)
            {
                var match = Regex.Match(PcParams[i].Name, paramName+"[0-9]+");
                if (match.Success)
                {
                    index = int.Parse(match.Value.Substring(2)) + 1;
                    break;
                }
            }
            PcParams.Add(new MaociAlgorParam(paramName + index));
            PcSelect = paramName + index;
        }

        [RelayCommand]
        public void RemovePcParam(MaociAlgorParam maociAlgorParam)
        {
            int index = Math.Max(PcParams.IndexOf(maociAlgorParam)-1, 0);
            if(PcParams.Count > 1) PcParams.Remove(maociAlgorParam);
            PcSelect = PcParams[index].Name;
        }

        [RelayCommand]
        public void AddFpgaParam()
        {
            int index = 1;
            for (int i = FpgaParams.Count - 1; i >= 0; i--)
            {
                var match = Regex.Match(FpgaParams[i].Name, paramName + "[0-9]+");
                if (match.Success)
                {
                    index = int.Parse(match.Value.Substring(2)) + 1;
                    break;
                }
            }
            FpgaParams.Add(new MaociAlgorParamFpga(paramName + index));
            FpgaSelect = paramName + index;
        }

        [RelayCommand]
        public void RemoveFpgaParam(MaociAlgorParamFpga maociAlgorParamFpga)
        {
            int index = Math.Max(FpgaParams.IndexOf(maociAlgorParamFpga) - 1, 0);
            if (FpgaParams.Count > 1) FpgaParams.Remove(maociAlgorParamFpga);
            FpgaSelect = FpgaParams[index].Name;
        }
    }
}
