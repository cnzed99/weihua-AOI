using System.Text.RegularExpressions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HandyControl.Controls;
using Newtonsoft.Json;
using WH.Controls;
using WH.Entity.LogRecord;
using WH.RecipeCellRootBase;

namespace AlgorithmDll
{
    /// <summary>
    /// 2024.6.25 李焕彬
    /// 算法参数VM
    /// </summary>
    public partial class CMaociAlgorParamCtrlVm : ObservableObject
    {
        /// <summary>
        /// 2024.9.6 李焕彬
        /// 权限信息，启动暂停、账户登录时切换
        /// </summary>
        [ObservableProperty]
        CLoginPerson loginPerson = new CLoginPerson() { IsNoPermission = true };

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 默认分组名
        /// </summary>
        private const string c_ParamName = "分组";

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 算法参数配置
        /// </summary>
        [ObservableProperty]
        private CAlgorithmParamBase config;

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
                var match = Regex.Match(Config.PcParams[i].Name, c_ParamName + "[0-9]+");
                if (match.Success)
                {
                    index = int.Parse(match.Value.Substring(2)) + 1;
                    break;
                }
            }
            Config.AddPcParam(c_ParamName + index);
            Config.PcSelect = c_ParamName + index;
        }

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 移除参数组
        /// </summary>
        /// <param name="maociAlgorParam">删除的参数</param>
        [RelayCommand]
        public void RemovePcParam(CPcParamBase maociAlgorParam)
        {
            Growl.AskGlobal(
                Config.Name + "-" + Properties.Resources.DelecteAsk,
                b =>
                {
                    if (b)
                    {
                        int index = Math.Max(Config.PcParams.IndexOf(maociAlgorParam) - 1, 0);
                        if (Config.PcParams.Count > 1)
                            Config.PcParams.Remove(maociAlgorParam);
                        Config.PcSelect = Config.PcParams[index].Name;
                    }
                    return true;
                }
            );
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
            Config.AddFpgaParam(c_ParamName + index);
            Config.FpgaSelect = c_ParamName + index;
        }

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 移除FPGA参数组
        /// </summary>
        /// <param name="maociAlgorParamFpga">删除的参数</param>
        [RelayCommand]
        public void RemoveFpgaParam(CFpgaParamBase maociAlgorParamFpga)
        {
            Growl.AskGlobal(
                Config.Name + "-" + Properties.Resources.DelecteAsk,
                b =>
                {
                    if (b)
                    {
                        int index = Math.Max(Config.FpgaParams.IndexOf(maociAlgorParamFpga) - 1, 0);
                        if (Config.FpgaParams.Count > 1)
                            Config.FpgaParams.Remove(maociAlgorParamFpga);
                        Config.FpgaSelect = Config.FpgaParams[index].Name;
                    }
                    return true;
                }
            );
        }

        /// <summary>
        /// 2024.7.4 李焕彬
        /// 写入FPGA
        /// </summary>
        /// <param name="maociAlgorParamFpga">写入的参数</param>
        [RelayCommand]
        public void WriteFpga(CFpgaParamBase maociAlgorParamFpga) { }
    }
}
