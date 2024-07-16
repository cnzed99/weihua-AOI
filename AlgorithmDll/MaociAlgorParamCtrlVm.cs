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
    public partial class CMaociAlgorParamCtrlVm : ObservableObject
    {
        /// <summary>
        /// 2024.7.4 李焕彬
        /// 默认分组名
        /// </summary>
        private const string c_ParamName = "分组";

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 日志
        /// </summary>
        [JsonIgnore]
        public CLogRec OperateLog { get; set; } = CLogRec.Create("Operate", "D:/Data");

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 算法参数配置
        /// </summary>
        [ObservableProperty]
        private CMaociAlgorParamConfig config = new CMaociAlgorParamConfig();

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 增加参数组
        /// </summary>
        [RelayCommand]
        public void AddPcParam()
        {
            int index = 1;
            for (int i = Config.PcParams.Count - 1; i >= 0; i--)
            {
                var match = Regex.Match(Config.PcParams[i].Name, c_ParamName+"[0-9]+");
                if (match.Success)
                {
                    index = int.Parse(match.Value.Substring(2)) + 1;
                    break;
                }
            }
            Config.PcParams.Add(new MaociAlgorParam(c_ParamName + index, Config.token));
            Config.PcSelect = c_ParamName + index;
        }

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 移除参数组
        /// </summary>
        /// <param name="maociAlgorParam">删除的参数</param>
        [RelayCommand]
        public void RemovePcParam(MaociAlgorParam maociAlgorParam)
        {
            int index = Math.Max(Config.PcParams.IndexOf(maociAlgorParam)-1, 0);
            if(Config.PcParams.Count > 1) Config.PcParams.Remove(maociAlgorParam);
            Config.PcSelect = Config.PcParams[index].Name;
        }

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 增加FPGA参数组
        /// </summary>
        [RelayCommand]
        public void AddFpgaParam()
        {
            int index = 1;
            for (int i = Config.FpgaParams.Count - 1; i >= 0; i--)
            {
                var match = Regex.Match(Config.FpgaParams[i].Name, c_ParamName + "[0-9]+");
                if (match.Success)
                {
                    index = int.Parse(match.Value.Substring(2)) + 1;
                    break;
                }
            }
            Config.FpgaParams.Add(new MaociAlgorParamFpga(c_ParamName + index, Config.token));
            Config.FpgaSelect = c_ParamName + index;
        }

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 移除FPGA参数组
        /// </summary>
        /// <param name="maociAlgorParamFpga">删除的参数</param>
        [RelayCommand]
        public void RemoveFpgaParam(MaociAlgorParamFpga maociAlgorParamFpga)
        {
            int index = Math.Max(Config.FpgaParams.IndexOf(maociAlgorParamFpga) - 1, 0);
            if (Config.FpgaParams.Count > 1) Config.FpgaParams.Remove(maociAlgorParamFpga);
            Config.FpgaSelect = Config.FpgaParams[index].Name;
        }

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 写入FPGA
        /// </summary>
        /// <param name="maociAlgorParamFpga">写入的参数</param>
        [RelayCommand]
        public void WriteFpga(MaociAlgorParamFpga maociAlgorParamFpga)
        {

        }
    }
}
