using AlgorithmDll;
using CameraModule;
using SDFilter;
using ZipperInfo;

namespace WH.DetectSystem.Models
{
    /// <summary>
    /// 拉链自动识别完成后的过滤参数回写（原 MainVM TestFinshTodo 方法群）。
    /// </summary>
    public partial class CMainModel
    {
        internal void SubscribeZipperAutoFinish()
        {
            CZipperAutomaticAlgorithm.Instance.TestFinshEven -= TestFinshTodo;
            CZipperAutomaticAlgorithm.Instance.TestFinshEven += TestFinshTodo;
        }

        internal void UnsubscribeZipperAutoFinish()
        {
            CZipperAutomaticAlgorithm.Instance.TestFinshEven -= TestFinshTodo;
        }

        private void TestFinshTodo(bool finsh)
        {

            if (CZipperAutomaticAlgorithm.Instance.TestFinsh)
            {
                foreach (var cell in MergeCells)
                {
                    SaveOtherOldImage(cell);
                }
                MergeCells.Clear();
                UpdatDetSet();
                ////UpdatWhiteZipperParam(); //白色拉链加严处理 20260424 鲍赞宝 弃用
                //if (this.Name == "正面")
                //{
                //    if (CZipperAutomaticAlgorithm.ZipperInfo.ZipperSliderType == PULLTYPE.正穿)
                //    {
                //        var camDic = CCameraManagement.CamParamDict.Values.FirstOrDefault(c => c.Name == "左相机");
                //        if (camDic != null)
                //        {
                //            UpdateCam(camDic.SerialNumber);
                //            UpdateLogo("左相机");
                //            Updatepull("左相机");
                //            UpdatepullSegArea("左相机");
                //        }


                //    }
                //    else
                //    {
                //        var camDic = CCameraManagement.CamParamDict.Values.FirstOrDefault(c => c.Name == "右相机");
                //        if (camDic != null)
                //        {
                //            UpdateCam(camDic.SerialNumber);
                //            UpdateLogo("右相机");
                //            Updatepull("右相机");
                //            UpdatepullSegArea("右相机");
                //        }

                //    }
                //}
                //if (this.Name == "反面")
                //{
                //    if (CZipperAutomaticAlgorithm.ZipperInfo.ZipperSliderType == PULLTYPE.正穿)
                //    {
                //        var camDic = CCameraManagement.CamParamDict.Values.FirstOrDefault(c => c.Name == "右相机");
                //        if (camDic != null)
                //        {
                //            UpdateCam(camDic.SerialNumber);
                //            UpdateLogo("右相机");
                //            Updatepull("右相机");
                //            UpdatepullSegArea("右相机");
                //        }

                //    }
                //    else
                //    {
                //        var camDic = CCameraManagement.CamParamDict.Values.FirstOrDefault(c => c.Name == "左相机");
                //        if (camDic != null)
                //        {
                //            UpdateCam(camDic.SerialNumber);
                //            UpdateLogo("左相机");
                //            Updatepull("左相机");
                //            UpdatepullSegArea("左相机");
                //        }

                //    }
                //}
                //if (this.Name == "正面内")
                //{
                //    if (CZipperAutomaticAlgorithm.ZipperInfo.ZipperSliderType == PULLTYPE.正穿)
                //    {
                //        var camDic = CCameraManagement.CamParamDict.Values.FirstOrDefault(c => c.Name == "上内相机");
                //        if (camDic != null)
                //        {
                //            UpdateCam(camDic.SerialNumber);
                //        }
                //    }
                //    else
                //    {
                //        var camDic = CCameraManagement.CamParamDict.Values.FirstOrDefault(c => c.Name == "下内相机");
                //        if (camDic != null)
                //        {
                //            UpdateCam(camDic.SerialNumber);
                //        }
                //    }
                //}
                //if (this.Name == "正面外")
                //{
                //    if (CZipperAutomaticAlgorithm.ZipperInfo.ZipperSliderType == PULLTYPE.正穿)
                //    {
                //        var camDic = CCameraManagement.CamParamDict.Values.FirstOrDefault(c => c.Name == "上外相机");
                //        if (camDic != null)
                //        {
                //            UpdateCam(camDic.SerialNumber);
                //        }
                //    }
                //    else
                //    {
                //        var camDic = CCameraManagement.CamParamDict.Values.FirstOrDefault(c => c.Name == "下外相机");
                //        if (camDic != null)
                //        {
                //            UpdateCam(camDic.SerialNumber);
                //        }
                //    }
                //}
                //if (this.Name == "反面内")
                //{
                //    if (CZipperAutomaticAlgorithm.ZipperInfo.ZipperSliderType == PULLTYPE.正穿)
                //    {
                //        var camDic = CCameraManagement.CamParamDict.Values.FirstOrDefault(c => c.Name == "下内相机");
                //        if (camDic != null)
                //        {
                //            UpdateCam(camDic.SerialNumber);
                //        }
                //    }
                //    else
                //    {
                //        var camDic = CCameraManagement.CamParamDict.Values.FirstOrDefault(c => c.Name == "上内相机");
                //        if (camDic != null)
                //        {
                //            UpdateCam(camDic.SerialNumber);
                //        }
                //    }
                //}
                //if (this.Name == "反面外")
                //{
                //    if (CZipperAutomaticAlgorithm.ZipperInfo.ZipperSliderType == PULLTYPE.正穿)
                //    {
                //        var camDic = CCameraManagement.CamParamDict.Values.FirstOrDefault(c => c.Name == "下外相机");
                //        if (camDic != null)
                //        {
                //            UpdateCam(camDic.SerialNumber);
                //        }
                //    }
                //    else
                //    {
                //        var camDic = CCameraManagement.CamParamDict.Values.FirstOrDefault(c => c.Name == "上外相机");
                //        if (camDic != null)
                //        {
                //            UpdateCam(camDic.SerialNumber);
                //        }
                //    }
                //}
            }
            if (!finsh)
            {
                IsAutomaticTest = false;
            }
        }

        //private void InfoChangeFunc()
        //{
        //    TestFinshTodo(true);
        //}

        private void UpdatWhiteZipperParam()
        {
            #region 布带脏污
            RecipeDefect dirtyDetNames = this.MaociFilterConfig["拉链"]["布带脏污"];
            if (dirtyDetNames != null)
            {
                foreach (var df in dirtyDetNames.DefectFilters)
                {
                    foreach (var fl in df.FilterList)
                    {
                        foreach (var se in fl.SelectList)
                        {
                            foreach (var pa in se.SelectParams)
                            {
                                if (pa.Character.ZhName == "分数" || pa.Character.EnName == "Score")
                                {
                                    if (CZipperAutomaticAlgorithm.Instance.ZipperInfo.WhiteZippers)
                                    {
                                        pa.Min = 30;
                                    }
                                    else
                                    {
                                        pa.Min = 35;
                                    }

                                }
                                if (pa.Character.ZhName == "面积" || pa.Character.EnName == "Area")
                                {
                                    if (CZipperAutomaticAlgorithm.Instance.ZipperInfo.WhiteZippers)
                                    {
                                        pa.Min = 180;
                                    }
                                    else
                                    {
                                        pa.Min = 280;
                                    }

                                }
                            }
                        }
                    }

                }
            }

            #endregion
            #region 点脏污
            RecipeDefect pointDetNames = this.MaociFilterConfig["拉链"]["点脏污"];
            if (pointDetNames != null)
            {
                foreach (var df in pointDetNames.DefectFilters)
                {
                    foreach (var fl in df.FilterList)
                    {
                        foreach (var se in fl.SelectList)
                        {
                            foreach (var pa in se.SelectParams)
                            {
                                if (pa.Character.ZhName == "数量" || pa.Character.EnName == "Count")
                                {
                                    if (CZipperAutomaticAlgorithm.Instance.ZipperInfo.WhiteZippers)
                                    {
                                        pa.Min = 2;
                                    }
                                    else
                                    {
                                        pa.Min = 3;
                                    }
                                }
                                if (pa.Character.ZhName == "分数" || pa.Character.EnName == "Score")
                                {
                                    if (CZipperAutomaticAlgorithm.Instance.ZipperInfo.WhiteZippers)
                                    {
                                        pa.Min = 30;
                                    }
                                    else
                                    {
                                        pa.Min = 35;
                                    }

                                }
                                if (pa.Character.ZhName == "面积" || pa.Character.EnName == "Area")
                                {
                                    if (CZipperAutomaticAlgorithm.Instance.ZipperInfo.WhiteZippers)
                                    {
                                        pa.Min = 130;
                                    }
                                    else
                                    {
                                        pa.Min = 280;
                                    }

                                }
                            }
                        }
                    }

                }
            }

            #endregion
        }
        /// <summary>
        /// 更新缺陷配置
        /// </summary>
        /// <param name="leftorright"></param>
        private void UpdatDetSet()
        {
            if (this.Name == "正面" || this.Name == "反面")
            {
                RecipeDefect zipperDetNames = this.MaociFilterConfig["方块插销"]["SAB"];
                if (zipperDetNames != null)
                {
                    foreach (var df in zipperDetNames.DefectFilters)
                    {
                        foreach (var fl in df.FilterList)
                        {
                            if (CZipperAutomaticAlgorithm.Instance.ZipperInfo.TempData1.AutoData.LockHaveSAB == HAVESAB.有SAB)
                            {
                                fl.FilterSelectEnable = true;
                            }
                            else
                            {
                                fl.FilterSelectEnable = false;
                            }

                        }
                    }
                }
            }

            if (this.Name == "拉头")
            {
                RecipeDefect zipperDetNames = this.MaociFilterConfig["拉头拉片"]["SAB"];
                if (zipperDetNames != null)
                {
                    foreach (var df in zipperDetNames.DefectFilters)
                    {
                        foreach (var fl in df.FilterList)
                        {
                            if (CZipperAutomaticAlgorithm.Instance.ZipperInfo.TempData1.PullerHaveSAB == HAVESAB.有SAB)
                            {
                                // fl.FilterSelectEnable = true;
                                fl.IsReversal = true;
                            }
                            else
                            {
                                // fl.FilterSelectEnable = false;
                                fl.IsReversal = true;
                            }

                        }
                    }
                }
            }

            if (this.Name == "拉片")
            {
                SpeciesFilter pullsdetName = this.MaociFilterConfig["拉头拉片"];
                foreach (var detname in pullsdetName.RecipeDefects)
                {
                    if (detname.Name != "拉片外形")
                    {
                        foreach (var df in detname.DefectFilters)
                        {
                            foreach (var fl in df.FilterList)
                            {

                                if (CZipperAutomaticAlgorithm.Instance.ZipperInfo.TempData1.AutoData.PullsHaveFilm == PULLSHAVEFILM.有膜)
                                {
                                    fl.FilterSelectEnable = false;
                                }
                                else
                                {
                                    fl.FilterSelectEnable = true;
                                }

                                // fl.IsReversal = false; //当没有LOGO时，如果检测到LOGO 说明是混拉头了
                            }
                        }
                    }

                }

            }

            //foreach (var detname in zipperDetNames.RecipeDefects)
            //{
            //    if (detname.Name.Contains("正面上止") || detname.Name.Contains("反面上止"))
            //    {
            //        foreach (var df in detname.DefectFilters)
            //        {
            //            foreach (var fl in df.FilterList)
            //            {
            //                if ("无" == CZipperAutomaticAlgorithm.ZipperInfo.ZipperUpMassType.ToString())
            //                {
            //                    fl.FilterSelectEnable = false;
            //                }
            //                else
            //                {
            //                    fl.FilterSelectEnable = true;
            //                }

            //            }
            //        }
            //    }

            //    if (detname.Name.Contains("正面下止") || detname.Name.Contains("反面下止"))
            //    {
            //        foreach (var df in detname.DefectFilters)
            //        {
            //            foreach (var fl in df.FilterList)
            //            {
            //                if ("无" == CZipperAutomaticAlgorithm.ZipperInfo.ZipperDownMassType.ToString())
            //                {
            //                    fl.FilterSelectEnable = false;
            //                }
            //                else
            //                {
            //                    fl.FilterSelectEnable = true;
            //                }

            //            }
            //        }
            //    }

            //}
        }
        /// <summary>
        /// 更新拉头配置
        /// </summary>
        /// <param name="leftorright"></param>
        private void Updatepull(string leftorright)
        {
            if (leftorright == "左相机")
            {
                RecipeDefect pullnames = this.MaociFilterConfig["LOGO"]["拉头"];
                if (pullnames != null)
                {
                    foreach (var df in pullnames.DefectFilters)
                    {
                        foreach (var fl in df.FilterList)
                        {
                            fl.FilterSelectEnable = false;
                        }
                    }
                }
                RecipeDefect pullernames = this.MaociFilterConfig["LOGO"]["拉片"];
                if (pullernames != null)
                {
                    foreach (var df in pullernames.DefectFilters)
                    {
                        foreach (var fl in df.FilterList)
                        {
                            fl.FilterSelectEnable = true;
                        }
                    }
                }
            }
            else
            {
                RecipeDefect pullnames = this.MaociFilterConfig["LOGO"]["拉头"];
                if (pullnames != null)
                {
                    foreach (var df in pullnames.DefectFilters)
                    {
                        foreach (var fl in df.FilterList)
                        {
                            fl.FilterSelectEnable = true;
                        }
                    }
                }
                RecipeDefect pullernames = this.MaociFilterConfig["LOGO"]["拉片"];
                if (pullernames != null)
                {
                    foreach (var df in pullernames.DefectFilters)
                    {
                        foreach (var fl in df.FilterList)
                        {
                            fl.FilterSelectEnable = false;
                        }
                    }
                }
            }
        }
        private void UpdatepullSegArea(string leftorright)
        {
            if (leftorright == "左相机")
            {
                RecipeDefect pullsegnames = this.MaociFilterConfig["拉头拉片"]["拉片外形"];
                if (pullsegnames != null)
                {
                    foreach (var df in pullsegnames.DefectFilters)
                    {
                        foreach (var fl in df.FilterList)
                        {
                            fl.FilterSelectEnable = true; //
                        }
                    }
                }
                RecipeDefect pullHnames = this.MaociFilterConfig["拉头拉片"]["拉头色差1"];
                if (pullHnames != null)
                {
                    foreach (var df in pullHnames.DefectFilters)
                    {
                        foreach (var fl in df.FilterList)
                        {
                            foreach (var se in fl.SelectList)
                            {
                                foreach (var pa in se.SelectParams)
                                {
                                    if (pa.Character.ZhName == "值" || pa.Character.EnName == "Value")
                                    {
                                        if (CZipperAutomaticAlgorithm.Instance.ZipperInfo.PullMaterlsType == PULLMATERIALSTYPE.烤漆)
                                        {

                                            double diff = CZipperAutomaticAlgorithm.Instance.ZipperInfo.TempData1.PullsMeanH - 20; //烤漆拉片H
                                            if (diff <= 0)
                                            {
                                                diff = 0;
                                            }
                                            pa.Min = diff;
                                            pa.Max = CZipperAutomaticAlgorithm.Instance.ZipperInfo.TempData1.PullsMeanH + 22;
                                        }
                                        else
                                        {
                                            double diff = CZipperAutomaticAlgorithm.Instance.ZipperInfo.TempData1.PullsMeanH - 22; //包胶拉片H
                                            if (diff <= 0)
                                            {
                                                diff = 0;
                                            }
                                            pa.Min = diff;
                                            pa.Max = CZipperAutomaticAlgorithm.Instance.ZipperInfo.TempData1.PullsMeanH + 25;
                                        }

                                    }
                                }
                            }
                        }
                    }
                }
                RecipeDefect pullSnames = this.MaociFilterConfig["拉头拉片"]["拉头色差2"];
                if (pullSnames != null)
                {
                    foreach (var df in pullSnames.DefectFilters)
                    {
                        foreach (var fl in df.FilterList)
                        {
                            foreach (var se in fl.SelectList)
                            {
                                foreach (var pa in se.SelectParams)
                                {
                                    if (pa.Character.ZhName == "值" || pa.Character.EnName == "Value")
                                    {
                                        if (CZipperAutomaticAlgorithm.Instance.ZipperInfo.PullMaterlsType == PULLMATERIALSTYPE.烤漆)
                                        {
                                            double diff = CZipperAutomaticAlgorithm.Instance.ZipperInfo.TempData1.PullsMeanS - 15; //烤漆拉片S
                                            if (diff <= 0)
                                            {
                                                diff = 0;
                                            }
                                            pa.Min = diff;
                                            pa.Max = CZipperAutomaticAlgorithm.Instance.ZipperInfo.TempData1.PullsMeanS + 22;
                                        }
                                        else
                                        {
                                            double diff = CZipperAutomaticAlgorithm.Instance.ZipperInfo.TempData1.PullsMeanS - 23; //包胶拉片S
                                            if (diff <= 0)
                                            {
                                                diff = 0;
                                            }
                                            pa.Min = diff;
                                            pa.Max = CZipperAutomaticAlgorithm.Instance.ZipperInfo.TempData1.PullsMeanS + 28;
                                        }

                                    }
                                }
                            }
                        }

                    }
                }
            }
            else
            {
                RecipeDefect pullsegnames = this.MaociFilterConfig["拉头拉片"]["拉片外形"]; ;
                if (pullsegnames != null)
                {
                    foreach (var df in pullsegnames.DefectFilters)
                    {
                        foreach (var fl in df.FilterList)
                        {
                            fl.FilterSelectEnable = false;
                        }
                    }
                }
                RecipeDefect pullHnames = this.MaociFilterConfig["拉头拉片"]["拉头色差1"];
                foreach (var df in pullHnames.DefectFilters)
                {
                    foreach (var fl in df.FilterList)
                    {
                        foreach (var se in fl.SelectList)
                        {
                            foreach (var pa in se.SelectParams)
                            {
                                if (pa.Character.ZhName == "值" || pa.Character.EnName == "Value")
                                {
                                    if (CZipperAutomaticAlgorithm.Instance.ZipperInfo.PullMaterlsType == PULLMATERIALSTYPE.烤漆)
                                    {
                                        double diff = CZipperAutomaticAlgorithm.Instance.ZipperInfo.TempData1.PullerMeanH - 15; //烤漆拉头H
                                        if (diff <= 0)
                                        {
                                            diff = 0;
                                        }
                                        pa.Min = diff;
                                        pa.Max = CZipperAutomaticAlgorithm.Instance.ZipperInfo.TempData1.PullerMeanH + 18;
                                    }
                                    else
                                    {
                                        double diff = CZipperAutomaticAlgorithm.Instance.ZipperInfo.TempData1.PullerMeanH - 20; //金属拉头H
                                        if (diff <= 0)
                                        {
                                            diff = 0;
                                        }
                                        pa.Min = diff;
                                        pa.Max = CZipperAutomaticAlgorithm.Instance.ZipperInfo.TempData1.PullerMeanH + 25;
                                    }
                                }
                            }
                        }
                    }
                }
                RecipeDefect pullSnames = this.MaociFilterConfig["拉头拉片"]["拉头色差2"];
                foreach (var df in pullSnames.DefectFilters)
                {
                    foreach (var fl in df.FilterList)
                    {
                        foreach (var se in fl.SelectList)
                        {
                            foreach (var pa in se.SelectParams)
                            {
                                if (pa.Character.ZhName == "值" || pa.Character.EnName == "Value")
                                {
                                    if (CZipperAutomaticAlgorithm.Instance.ZipperInfo.PullMaterlsType == PULLMATERIALSTYPE.烤漆)
                                    {
                                        double diff = CZipperAutomaticAlgorithm.Instance.ZipperInfo.TempData1.PullerMeanS - 12; //烤漆拉头S
                                        if (diff <= 0)
                                        {
                                            diff = 0;
                                        }
                                        pa.Min = diff;
                                        pa.Max = CZipperAutomaticAlgorithm.Instance.ZipperInfo.TempData1.PullerMeanS + 15;
                                    }
                                    else
                                    {
                                        double diff = CZipperAutomaticAlgorithm.Instance.ZipperInfo.TempData1.PullerMeanS - 15; //金属拉头S
                                        if (diff <= 0)
                                        {
                                            diff = 0;
                                        }
                                        pa.Min = diff;
                                        pa.Max = CZipperAutomaticAlgorithm.Instance.ZipperInfo.TempData1.PullerMeanS + 25;
                                    }
                                }
                            }
                        }
                    }

                }

            }


        }

        /// <summary>
        /// 更新Logo配置
        /// </summary>
        private void UpdateLogo(string leftorright)
        {
            if (leftorright == "左相机") //拍拉片
            {
                SpeciesFilter logonames = this.MaociFilterConfig["LOGO"];
                if ("无LOGO" == CZipperAutomaticAlgorithm.Instance.ZipperInfo.ZipperLogoType)
                {
                    foreach (var logoname in logonames.RecipeDefects)
                    {
                        if (!logoname.Name.Contains("拉"))
                        {
                            foreach (var df in logoname.DefectFilters)
                            {
                                foreach (var fl in df.FilterList)
                                {
                                    fl.FilterSelectEnable = true;
                                    fl.IsReversal = false; //当没有LOGO时，如果检测到LOGO 说明是混拉头了
                                }
                            }
                        }
                    }
                }
                else
                {
                    foreach (var logoname in logonames.RecipeDefects)
                    {
                        if (!logoname.Name.Contains("拉"))
                        {
                            if (logoname.Name == CZipperAutomaticAlgorithm.Instance.ZipperInfo.ZipperLogoType)
                            {
                                foreach (var df in logoname.DefectFilters)
                                {

                                    foreach (var fl in df.FilterList)
                                    {
                                        fl.FilterSelectEnable = true;
                                        if (CZipperAutomaticAlgorithm.Instance.ZipperInfo.TempData1.FindLogoSider == 3)
                                        {
                                            fl.IsReversal = false;//当有LOGO时，如果检测到LOGO 和正确的LOGO一致时，需要取反为OK
                                        }
                                        else
                                        {
                                            fl.IsReversal = true;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                foreach (var df in logoname.DefectFilters)
                                {

                                    foreach (var fl in df.FilterList)
                                    {
                                        fl.FilterSelectEnable = true;
                                        fl.IsReversal = false; //
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else  //拍拉头
            {
                SpeciesFilter logonames = this.MaociFilterConfig["LOGO"];
                if ("无LOGO" == CZipperAutomaticAlgorithm.Instance.ZipperInfo.ZipperLogoType)
                {
                    foreach (var logoname in logonames.RecipeDefects)
                    {
                        if (!logoname.Name.Contains("拉"))
                        {
                            foreach (var df in logoname.DefectFilters)
                            {
                                foreach (var fl in df.FilterList)
                                {
                                    fl.FilterSelectEnable = true;
                                    fl.IsReversal = false; //当没有LOGO时，如果检测到LOGO 说明是混拉头了
                                }
                            }
                        }
                    }
                }
                else
                {
                    foreach (var logoname in logonames.RecipeDefects)
                    {
                        if (!logoname.Name.Contains("拉"))
                        {
                            if (logoname.Name == CZipperAutomaticAlgorithm.Instance.ZipperInfo.ZipperLogoType)
                            {
                                foreach (var df in logoname.DefectFilters)
                                {
                                    foreach (var fl in df.FilterList)
                                    {

                                        if (CZipperAutomaticAlgorithm.Instance.ZipperInfo.TempData1.FindLogoSider == 3)
                                        {
                                            fl.FilterSelectEnable = true;
                                            fl.IsReversal = true;//
                                        }
                                        else
                                        {
                                            fl.FilterSelectEnable = false;
                                            fl.IsReversal = false;//
                                        }

                                    }
                                }
                            }
                            else
                            {
                                foreach (var df in logoname.DefectFilters)
                                {
                                    foreach (var fl in df.FilterList)
                                    {
                                        fl.FilterSelectEnable = true;
                                        fl.IsReversal = false; //当有LOGO时，如果检测到别的LOGO ，不能取反，需要检出
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

    }
}
