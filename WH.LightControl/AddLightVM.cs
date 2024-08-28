using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HandyControl.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WH.LightControl
{
    public partial class AddLightVM:ObservableObject
    {
        /// <summary>
        /// 光源名称
        /// </summary>
        [ObservableProperty]
       private string lightName;

        private ObservableCollection<string> brandNames;

        public ObservableCollection<string> BrandNames
        {
            get 
            {
                ObservableCollection<string> tempname = new ObservableCollection<string>();
                try
                {
                    foreach (var item in Directory.GetDirectories("LightPlug"))
                    {
                        tempname.Add(Path.GetFileName(item));
                    }
                }
                catch (Exception)
                {
                }
                brandNames = tempname;
               return brandNames; 
            }
            set {brandNames = value;}
        }
        /// <summary>
        /// 用户选择的品牌
        /// </summary>

        private string selectBrand;

        public string SelectBrand
        {
            get { return selectBrand; }
            set 
            { 
                selectBrand = value;
                try
                {
                    LightName = "";
                    string[] dirFiles = Directory.GetDirectories("LightPlug");
                    if (!string.IsNullOrEmpty(SelectBrand))
                    {
                        var seledir = dirFiles.Where(s => s.Contains(SelectBrand));
                        if (seledir != null)
                        {
                            string[] dirNames = Directory.GetFiles(seledir.FirstOrDefault());
                            List<string> fileNames = new List<string>();
                            for (int i = 0; i < dirNames.Length; i++)
                            {
                                fileNames.Add(Path.GetFileNameWithoutExtension(dirNames[i]));
                            }

                            if (fileNames.Count > 0)
                            {
                                int num = 1;
                                string lightNameTemp = $"{selectBrand}-光源{num}";

                                while (fileNames.Contains(lightNameTemp))
                                {
                                    num++;
                                    lightNameTemp = $"{selectBrand}-光源{num}";
                                }

                                LightName = lightNameTemp;
                            }

                        }
                    }
                }
                catch (Exception ex)         
                {
                    Growl.Error(Properties.Resources.自动获取+" \n\r" + ex.Message);
                }
             
            }
        }

        /// <summary>
        /// 增加一个光源
        /// </summary>
        [RelayCommand]
        public void Add()
        {
            try
            {
                string[] dirFiles = Directory.GetDirectories("LightPlug");
                if (!string.IsNullOrEmpty(SelectBrand))
                {
                    var seledir = dirFiles.Where(s => s.Contains(SelectBrand));
                    if (seledir != null)
                    {
                        string[] fileNames = Directory.GetFiles(seledir.FirstOrDefault());
                        if (fileNames.Length > 0)
                        {
                            string CopyfileNmae = LightName;
                            string filename = Path.GetFileNameWithoutExtension(fileNames[0]);
                            string copypath = fileNames[0].Replace(filename, CopyfileNmae);
                            File.Copy(fileNames[0], copypath);
                            CLinghtManagement.LightHelpers.Clear();
                            CLinghtManagement.LightControlDict.Clear();
                            CLinghtManagement.LoadLightParams();
                            Growl.Info(Properties.Resources.成功添加光源 + copypath);
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                Growl.Error(Properties.Resources.添加光源出错+"\n\r"+ex.Message);
            }
         
           
        }
    }

}
