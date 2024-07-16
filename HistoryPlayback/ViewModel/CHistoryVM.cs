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
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace HistoryPlayback
{
    public partial class CHistoryVM : ObservableObject
    {
        /// <summary>
        /// 2024.7.6鲍赞宝
        /// 单张图片信息
        /// </summary>
        [ObservableProperty]
        CHistoryParam imageInfo = new CHistoryParam();

        /// <summary>
        /// 2024.7.6鲍赞宝
        /// 图片路径集合
        /// </summary>

        private ObservableCollection<string> imagePaths;

        /// <summary>
        /// 2024.7.6鲍赞宝
        /// 图片地址
        /// </summary>

        public ObservableCollection<string> ImagePaths
        {
            get { return imagePaths; }
            set
            {
                SetProperty(ref imagePaths, value);
                FileIndex = 0;
                switch (SelectClassify)
                {
                    case 0:
                        FileNames = imagePaths;
                        break;
                    case 1:
                        var defectFilePath = ImagePaths
                            .Where(p => p.Contains(SelectedDefectName))
                            .ToList();
                        if (defectFilePath != null)
                        {
                            FileNames = new ObservableCollection<string>(defectFilePath);
                        }
                        break;
                    default:
                        FileNames = imagePaths;
                        break;
                }
            }
        }

        /// <summary>
        /// 2024.7.6鲍赞宝
        /// 选择排版方式
        /// </summary>
        [ObservableProperty]
        int selectClassify = 0;

        /// <summary>
        /// 2024.7.6鲍赞宝
        /// 图片名称
        /// </summary>
        [ObservableProperty]
        ObservableCollection<string> fileNames;

        /// <summary>
        /// 2024.7.6鲍赞宝
        /// 选择的图片序号
        /// </summary>
        [ObservableProperty]
        int fileIndex;

        /// <summary>
        /// 2024.7.6鲍赞宝
        /// 缺陷名列表
        /// </summary>
        [ObservableProperty]
        ObservableCollection<string> filters = new ObservableCollection<string>();

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
        string selectedDefectName = "毛刺";

        /// <summary>
        /// 2024.7.6鲍赞宝
        /// 读取的图片
        /// </summary>
        [ObservableProperty]
        BitmapImage readImage;

        /// <summary>
        /// 2024.7.6鲍赞宝
        /// 下一张
        /// </summary>
        [RelayCommand]
        void NextImage()
        {
            if (FileNames != null)
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
            if (FileNames != null)
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
            if (selecteditem is string selectedobj)
            {
                if (File.Exists(selectedobj))
                {
                    ReadImage = new BitmapImage(new Uri(selectedobj));
                }

                // 图片命名:时间-流水ID-质量信号-质量等级-缺陷名-处理时间
                string name = Path.GetFileNameWithoutExtension(selectedobj);
                DateTime fileCreateTime = File.GetCreationTime(selectedobj);

                string[] spiltName = name.Split("-");
                ImageInfo.ID = spiltName[1];
                ImageInfo.CreateTime = fileCreateTime.ToString("F");

                ImageInfo.Level = spiltName[3];
                ImageInfo.DefectName = spiltName[4];
                ImageInfo.TakeTime = spiltName[5];
            }
        }

        /// <summary>
        /// 2024.7.6鲍赞宝
        /// 查看方式切换
        /// </summary>
        [RelayCommand]
        void RbCheck()
        {
            switch (SelectClassify)
            {
                case 0:
                    FileNames = imagePaths;
                    break;
                case 1:
                    if (SelectedDefectName != null && ImagePaths != null)
                    {
                        var defectFilePath = ImagePaths
                            .Where(p => p.Contains(SelectedDefectName))
                            .ToList();
                        if (defectFilePath != null)
                        {
                            FileNames = new ObservableCollection<string>(defectFilePath);
                        }
                    }

                    break;
                default:
                    FileNames = imagePaths;
                    break;
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
