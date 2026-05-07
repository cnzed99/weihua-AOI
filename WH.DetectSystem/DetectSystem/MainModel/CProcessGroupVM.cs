using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using AlarmSetCtrl;
using AlarmSetCtrl.View;
using AlgorithmDll;
using Autofac;
using CameraModule;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Mapster;
using MySqlOperatesApi;
using Mysqlx.Crud;
using MySqlX.XDevAPI;
using ProjProduceData;
using SDFilter;
using WH.DetectSystem.Models;
using WH.DetectSystem.ViewModels;
using WH.Entity.CommonLib;
using WH.RunCell;
using ZipperInfo;

namespace WH.DetectSystem.Models
{
    /// <summary>
    /// 2024.9.3 李焕彬
    /// 制程组
    /// </summary>
    public partial class CProcessGroupModel
    {
        public CProcessGroupModel(string name)
        {
            GUID = Guid.NewGuid().ToString();
            this.token = new Token(GUID, this.GetType().Namespace);
            this.Name = name;
            MaociQualityConfig.token.ProGuid = GUID;
            MaociQualityConfig.PrcessName = name;
            ConfigModifyObservableBase.UpdateToken(MaociQualityConfig, MaociQualityConfig.token); //新建制程组时更新Token
            Init();
        }

        /// <summary>
        /// 2024.9.2 李焕彬
        /// 缺陷汇总
        /// </summary>
        [ObservableProperty]
        CDefectsDataVM cDefectsDataVM = new CDefectsDataVM();

        /// <summary>
        /// 2025.11.29 鲍赞宝
        /// 单个料号的缺陷数据
        /// </summary>
        [ObservableProperty]
        CDefectsDataVM cDefectsOneFlowDataVM = new CDefectsDataVM();

        /// <summary>
        /// 报警设置
        /// </summary>
        [AdaptIgnore]
        [ObservableProperty]
        CAlarmSetConfigVM alarmSetVM = new CAlarmSetConfigVM(); //报警

        /// <summary>
        /// 2024.9.6 李焕彬
        /// 数据库
        /// </summary>
        public CMySqlVM MySqlVM { get; set; } = CPublicServices.Container.Resolve<CMySqlVM>();

        /// <summary>
        /// 2024.9.2 李焕彬
        /// 数据库
        /// </summary>
        public CMysqlBLL MysqlBLL { get; set; } = new CMysqlBLL();

        /// <summary>
        /// ID生成
        /// </summary>
        CCreateIDBase IDCreate;

        /// <summary>
        /// 2024.9.5 李焕彬
        /// 初始化，新建和加载时执行
        /// </summary>
        public void Init()
        {
            MaociQualityConfig.token.ProGuid = GUID;
            foreach (var item in CMainModels)
            {
                item.Init(this);
            }
            //2024.9.5 李焕彬 制程组缺陷统计初始化需要放在制程初始化后
            CDefectsDataVM.DefectsProduce = MaociDefectsProduce;
            CDefectsDataVM.DefectsProduce.SetQuality(MaociQualityConfig);
            CDefectsDataVM.DefectsProduce.SetFilter(
                this.CMainModels.Select(o => o.MaociFilterConfig).ToList()
            );
            CDefectsOneFlowDataVM.DefectsProduce = MaociDefectsOneFlowProduce;
            CDefectsOneFlowDataVM.DefectsProduce.SetQuality(MaociQualityConfig);
            CDefectsOneFlowDataVM.DefectsProduce.SetFilter(
                this.CMainModels.Select(o => o.MaociFilterConfig).ToList()
            );
            AlarmSetVM.CAlarmSet = AlarmSetConfig;
            AlarmSetVM.Reset();
            AlarmSetVM.SetFilter(this.CMainModels.Select(o => o.MaociFilterConfig).ToList());
            AlarmSetVM.SetQuality(MaociQualityConfig);

            MySqlVM.MysqlExecute.Clone(MysqlBLL);
            NameUpdata();
            MySqlVM.ActionUpdateParam += () =>
            {
                MySqlVM.MysqlExecute.Clone(MysqlBLL);
                NameUpdata();
            };
            //20260506 鲍赞宝
            if (Name == "制程组1")
            {
                IDCreate = new CCreateIDBase();
            }
            else if (Name == "制程组2")
            {
                IDCreate = new CCreateIDStation2();
            }
            else
            {
                IDCreate = new CCreateIDStation3();
            }
            IDCreate.IntThread();
            IDCreate.IDSendEvent += IDSend;
        }
        /// <summary>
        /// 分配ID给各个制程 
        /// 20260506 鲍赞宝
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="zipperID"></param>
        private void IDSend(object sender,ZipperID zipperID)
        {
            foreach (var item in CMainModels)
            {
                if (item == null) continue;
                item.m_WaitIDChannel.Writer.TryWrite(zipperID);
            }
        }

        /// <summary>
        /// 2024.9.5 李焕彬
        /// 制程组名字更新
        /// </summary>
        public void NameUpdata()
        {
            MysqlBLL.UpdateDatabaseName(Name);
            MaociQualityConfig.PrcessName = Name;
        }

        /// <summary>
        /// 2024.9.2 李焕彬
        /// 增加制程（制程已经存在）
        /// </summary>
        /// <param name="model">制程模型</param>
        public void AddProcess(CMainModel model)
        {
            model.ProcessGroup = this;
            CMainModels.Add(model);
            CDefectsDataVM.DefectsProduce.SetFilter(
                this.CMainModels.Select(o => o.MaociFilterConfig).ToList()
            );
            AlarmSetVM.SetFilter(this.CMainModels.Select(o => o.MaociFilterConfig).ToList());
        }

        /// <summary>
        /// 2024.9.2 李焕彬
        /// 移除制程
        /// </summary>
        /// <param name="mainVM">制程视图模型</param>
        public void RemoveProcess(CMainModel mainVM)
        {
            if (CMainModels.Contains(mainVM))
            {
                CMainModels.Remove(mainVM);
                CDefectsDataVM.DefectsProduce.SetFilter(
                    this.CMainModels.Select(o => o.MaociFilterConfig).ToList()
                );
                AlarmSetVM.SetFilter(this.CMainModels.Select(o => o.MaociFilterConfig).ToList());
            }
        }

        [RelayCommand]
        public void OpenAlarmSet()
        {
            AlarmSetWindow alarmSetWindow = new AlarmSetWindow();
            alarmSetWindow.Title = Name + "-报警设置";
            alarmSetWindow.DataContext = AlarmSetVM;
            alarmSetWindow.Show();
        }

        /// <summary>
        /// 2024.9.5 李焕彬
        /// 汇总结果用,存储制程组内制程Cell集合
        /// </summary>
        public List<CCellPro> Cells { get; set; } = new List<CCellPro>();

        /// <summary>
        /// 2024.9.5 李焕彬
        /// 汇总结果lock用
        /// </summary>
        object objLock = new object();

        /// <summary>
        /// 2024.9.6 李焕彬
        /// 汇总结果统计
        /// </summary>
        /// <param name="CellIn">需要统计的cell</param>
        /// <param name="CellOut">统计输出结果</param>
        /// <returns>所有制程全部到位返回true，否则false</returns>
        public bool AddCellAndJudge(Cell CellIn, out CCellPro CellOut)
        {
            lock (objLock)
            {
                CellOut = new CCellPro();
                Cells.Add(new CCellPro() { Cell = CellIn, });
                if (Cells.Count > 100)
                {
                    //Cells[0].Dispose();
                    Cells.RemoveAt(0);
                }
                foreach (var item in CMainModels)
                {
                    if (
                        Cells.FindIndex(c => c.Cell.ProjGuid == item.GUID && c.Cell.ID == CellIn.ID)
                        < 0
                    )
                        return false;
                }
                foreach (var item in CMainModels)
                {
                    CCellPro result = Cells.Find(c =>
                        c.Cell.ProjGuid == item.GUID && c.Cell.ID == CellIn.ID
                    );
                    while (Cells.Find(c => c.Cell.ProjGuid == item.GUID) != result)
                    {
                        CCellPro cell = Cells.Find(c => c.Cell.ProjGuid == item.GUID);
                        //cell.Dispose();
                        Cells.Remove(cell);
                    }
                    Cells.Remove(result);
                    if (CellOut.Cell == null)
                    {
                        CellOut = result;
                        continue;
                    }
                    if (result.Cell.Quality.Priority > CellOut.Cell.Quality.Priority)
                    {
                        //CellOut.Dispose();
                        CellOut = result;
                        continue;
                    }
                    else if (
                        result.Cell.Quality.Priority == CellOut.Cell.Quality.Priority
                        && CellOut.Cell.Detection?.DefectFilter.Priority
                            < result.Cell.Detection?.DefectFilter.Priority
                    )
                    {
                        //CellOut.Dispose();
                        CellOut = result;
                        continue;
                    }
                    //result.Dispose();
                }
                return true;
            }
        }
    }

    /// <summary>
    /// 2024.9.6 李焕彬
    /// 汇总统计专用Cell
    /// </summary>
    public class CCellPro : IDisposable
    {
        /// <summary>
        /// 2024.9.6 李焕彬
        /// Cell
        /// </summary>
        public Cell Cell { get; set; }

        public void Dispose() { }
    }
}
