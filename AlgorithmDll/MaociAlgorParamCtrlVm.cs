using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using WH.Entity.LogRecord;

namespace AlgorithmDll
{
    /// <summary>
    /// 2024.6.25 李焕彬
    /// 算法参数VM
    /// </summary>
    public partial class MaociAlgorParamCtrlVm : ObservableObject
    {
        private const string paramName = "分组";

        [JsonIgnore]
        public CLogRec OperateLog { get; set; } = CLogRec.Create("Operate", "D:/Data");

        [ObservableProperty]
        private MaociAlgorParamConfig config = new MaociAlgorParamConfig();

        [RelayCommand]
        public void AddPcParam()
        {
            int index = 1;
            for (int i = Config.PcParams.Count - 1; i >= 0; i--)
            {
                var match = Regex.Match(Config.PcParams[i].Name, paramName+"[0-9]+");
                if (match.Success)
                {
                    index = int.Parse(match.Value.Substring(2)) + 1;
                    break;
                }
            }
            Config.PcParams.Add(new MaociAlgorParam(paramName + index));
            Config.PcSelect = paramName + index;
        }

        [RelayCommand]
        public void RemovePcParam(MaociAlgorParam maociAlgorParam)
        {
            int index = Math.Max(Config.PcParams.IndexOf(maociAlgorParam)-1, 0);
            if(Config.PcParams.Count > 1) Config.PcParams.Remove(maociAlgorParam);
            Config.PcSelect = Config.PcParams[index].Name;
        }

        [RelayCommand]
        public void AddFpgaParam()
        {
            int index = 1;
            for (int i = Config.FpgaParams.Count - 1; i >= 0; i--)
            {
                var match = Regex.Match(Config.FpgaParams[i].Name, paramName + "[0-9]+");
                if (match.Success)
                {
                    index = int.Parse(match.Value.Substring(2)) + 1;
                    break;
                }
            }
            Config.FpgaParams.Add(new MaociAlgorParamFpga(paramName + index));
            Config.FpgaSelect = paramName + index;
        }

        [RelayCommand]
        public void RemoveFpgaParam(MaociAlgorParamFpga maociAlgorParamFpga)
        {
            int index = Math.Max(Config.FpgaParams.IndexOf(maociAlgorParamFpga) - 1, 0);
            if (Config.FpgaParams.Count > 1) Config.FpgaParams.Remove(maociAlgorParamFpga);
            Config.FpgaSelect = Config.FpgaParams[index].Name;
        }

        [RelayCommand]
        public void WriteFpga(MaociAlgorParamFpga maociAlgorParamFpga)
        {

        }
    }
}
