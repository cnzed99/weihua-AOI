using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using System.Xml.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HandyControl.Controls;
using HistoryPlayback.Model;
using QualityGrade;
using SDFilter;
using WH.Controls;

namespace HistoryPlayback
{
    public partial class CHistoryVM : ObservableObject
    {
        public CHistoryModel HistoryModel { get; set; }

        /// <summary>
        /// 2024.9.6 李焕彬
        /// 权限信息，启动暂停、账户登录时切换
        /// </summary>
        [ObservableProperty]
        CLoginPerson loginPerson;

        /// <summary>
        /// 2024.7.6鲍赞宝
        /// 单张图片信息
        /// </summary>
        [ObservableProperty]
        CellInfo selectedCellInfo = new CellInfo();

        /// <summary>
        /// 20240711 TCG
        /// 重置报警选项
        /// </summary>
        public void Reset()
        {
            SelectedCellInfo = new CellInfo();
            SelectClassify = 0;
            OnPropertyChanged(nameof(FileNames));
        }

        /// <summary>
        /// 2024.7.6鲍赞宝
        /// 选择排版方式
        /// </summary>
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(FileNames))]
        int selectClassify = 0;

        /// <summary>
        /// 20240717 TCG
        /// 图片名称
        /// </summary>
        public ObservableCollection<string> FileNames
        {
            get
            {
                switch (SelectClassify)
                {
                    case 0:
                        return HistoryModel.NgImagePaths;

                    case 1:
                        if (SelectedDefect is null)
                            break;
                        var defectFilePath = HistoryModel
                            .NgImagePaths.Where(p => p.Contains(SelectedDefect?.Name))
                            .ToList();

                        if (defectFilePath != null)
                        {
                            return new ObservableCollection<string>(defectFilePath);
                        }
                        break;
                }
                return HistoryModel.NgImagePaths;
            }
        }

        /// <summary>
        /// 2024.7.6鲍赞宝
        /// 选择的图片序号
        /// </summary>
        [ObservableProperty]
        int fileIndex;

        /// <summary>
        /// 2024.7.6鲍赞宝
        /// 选择的图片路径
        /// </summary>
        [ObservableProperty]
        string selectedItem;

        /// <summary>
        /// 2024.7.6鲍赞宝
        /// 选择的缺陷名称
        /// </summary>
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(FileNames))]
        DefectFilter selectedDefect;

        /// <summary>
        /// 2024.7.6鲍赞宝
        /// 读取的图片
        /// </summary>
        [ObservableProperty]
        BitmapImage readImage;
        /// <summary>
        /// 2025.9.15鲍赞宝
        /// 读取的拉头图片
        /// </summary>
        [ObservableProperty]
        BitmapImage readPullImage;

        /// <summary>
        /// 2025.9.15鲍赞宝
        /// 标题
        /// </summary>
        [ObservableProperty]
        string hisTital;
        /// <summary>
        /// 空图像
        /// </summary>
        [ObservableProperty]
        BitmapSource clearImage;

        /// <summary>
        /// 2024.7.6鲍赞宝
        /// 下一张
        /// </summary>
        [RelayCommand]
        void NextImage()
        {
            if (FileNames != null && FileNames.Count > 0)
            {
                FileIndex++;
                if (FileIndex >= FileNames.Count)
                    FileIndex = FileNames.Count - 1;
                if (FileIndex <= 0)
                    FileIndex = 0;
                SelectedItem = FileNames[FileIndex];
            }
        }

        /// <summary>
        /// 2024.7.6鲍赞宝
        /// 上一张
        /// </summary>
        [RelayCommand]
        void FrontImage()
        {
            if (FileNames != null && FileNames.Count > 0)
            {
                FileIndex--;
                if (FileIndex >= FileNames.Count)
                    FileIndex = FileNames.Count - 1;
                if (FileIndex <= 0)
                    FileIndex = 0;
                SelectedItem = FileNames[FileIndex];
            }
        }

        /// <summary>
        /// 2024.7.6鲍赞宝
        /// 选择后触发
        /// </summary>
        [RelayCommand]
        void ItemSelected(object selecteditem)
        {
            try
            {
                if (selecteditem is string selectedobj)
                {
                    if (File.Exists(selectedobj))
                    {
                        ReadImage = new BitmapImage(new Uri(selectedobj));
                    }
                    string[] pullname = selectedobj.Split('.');
                    string pullpath= pullname[0]+"_Pull"+"."+ pullname[1];
                    if (File.Exists(pullpath))
                    {
                        ReadPullImage = new BitmapImage(new Uri(pullpath));
                    }

                    // 图片命名:时间-流水ID-质量信号-质量等级-缺陷名-处理时间
                    string name = Path.GetFileNameWithoutExtension(selectedobj);
                    DateTime fileCreateTime = File.GetCreationTime(selectedobj);

                    string[] spiltName = name.Split('_');
                    SelectedCellInfo.ID = spiltName[0];
                    SelectedCellInfo.CreateTime = fileCreateTime.ToString("F");

                    SelectedCellInfo.Level = spiltName[3];
                    SelectedCellInfo.DefectName = spiltName[4];
                    // SelectedCellInfo.TakeTime = spiltName[5];
                }
            }
            catch (Exception ex)
            {
                Growl.Error("解析图片信息异常："+ex.Message);
            }
           
        }

        private void Receive(CFilterConfig filter)
        {
            List<string> strings = new List<string>();
            foreach (var sp in filter.SpeciesFilters)
            {
                foreach (var rp in sp.RecipeDefects)
                {
                    foreach (var de in rp.DefectFilters)
                    {
                        if (!HistoryModel.DefectList.Contains(de))
                        {
                            HistoryModel.DefectList.Add(de);
                        }
                        strings.Add(de.Name);
                    }
                }
            }
            for (int i = HistoryModel.DefectList.Count - 1; i >= 0; i--)
            {
                if (!strings.Contains(HistoryModel.DefectList[i].Name))
                {
                    HistoryModel.DefectList.RemoveAt(i);
                }
            }
        }
    }

    public class FileNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string filePath = value as string;
            if (filePath != null)
            {
                return Path.GetFileNameWithoutExtension(filePath);
            }
            return null;
        }

        public object ConvertBack(
            object value,
            Type targetType,
            object parameter,
            CultureInfo culture
        )
        {
            throw new NotImplementedException();
        }
    }
}
