using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AlgorithmDll;
using SDFilter;
using WH.Entity.Attribute;
using WH.RunCell;

namespace WH.DetectSystem
{
    /// <summary>
    /// 20240704 TCG
    /// 过滤阶段执行
    /// </summary>
    public static class CFilterStage
    {
        /// <summary>
        /// 20240704 TCG
        /// 对cell 中的缺陷进行过滤 得到最终的定级缺陷你，写入Cell中
        /// </summary>
        /// <param name="filter">过滤参数</param>
        /// <param name="cell">检测对象</param>
        public static void FilterExute(this CFilterConfig filterConfig, Cell cell)
        {
            if (cell._skipthis)
            {
                return;
            }
            var AlgorithmOut = cell.MaociTestOut.AlgorithmOut;
            //int deIndex = 0;
            foreach (var sp in AlgorithmOut.Specises)
            {
                filterConfig[sp.Name].Result = true;
                foreach (var rp in sp.Recipes)
                {
                    foreach (var de in filterConfig[sp.Name][rp.Name].DefectFilters) //缺陷
                    {
                        //deIndex++;
                        CellDetection detection = new CellDetection();
                        //detection.Name = de.Name;
                        detection.Type = sp.Name;
                        detection.RecipeDefectName = rp.Name;
                        //detection.Priority = de.Priority;
                        //detection.Quality = de.QualityLevel;//质量等级
                        detection.DefectFilter = de; //缺陷过滤器
                        //detection.Index = deIndex;
                        //detection.ShowColor = de.ShowColor;
                        detection.regionOut = AlgorithmOut[sp.Name][rp.Name].Region;
                        if (cell.CancelSource.IsCancellationRequested)
                            return; //任务取消时退出
                        foreach (var filter in de.FilterList) //过滤分选器
                        {
                            filter.Result = true;
                            List<SRegion> detectRegion = AlgorithmOut[sp.Name][rp.Name].Region;
                            switch (filter.UnionMethod)
                            {
                                case EMUNIONMETHOD.EMUNIONMETHOD_UNION:
                                    SRegionInfo regionInfo = new SRegionInfo();
                                    regionInfo.WidthBound = detectRegion
                                        .Select(o => o.RegionInfo.WidthBound)
                                        .Sum();
                                    regionInfo.HeightBound = detectRegion
                                        .Select(o => o.RegionInfo.HeightBound)
                                        .Sum();
                                    regionInfo.PeakHeight = detectRegion
                                        .Select(o => o.RegionInfo.PeakHeight)
                                        .Sum();
                                    regionInfo.LongLen = detectRegion
                                        .Select(o => o.RegionInfo.LongLen)
                                        .Sum();
                                    regionInfo.ShorLen = detectRegion
                                        .Select(o => o.RegionInfo.ShorLen)
                                        .Sum();
                                    regionInfo.Phi = detectRegion
                                        .Select(o => o.RegionInfo.Phi)
                                        .Max();
                                    regionInfo.ContLen = detectRegion
                                        .Select(o => o.RegionInfo.ContLen)
                                        .Sum();
                                    regionInfo.Area = detectRegion
                                        .Select(o => o.RegionInfo.Area)
                                        .Sum();
                                    for (int i = 0; i < detectRegion.Count; i++)
                                    {
                                        detectRegion[i] = new SRegion(
                                            regionInfo,
                                            detectRegion[i].points1
                                        );
                                    }
                                    break;
                            }

                            List<SRegion> filterOuts = new List<SRegion>(); //过滤后的区域
                            foreach (var select in filter.Filter) //过滤
                            {
                                List<SRegion> selRegion = detectRegion;
                                foreach (var selParam in select.SelectParams)
                                {
                                    selParam.Excute(selRegion, out selRegion); //&&
                                }
                                filterOuts.AddRange(selRegion); //||
                            }
                            foreach (var select in filter.SelectList) //分选
                            {
                                List<SRegion> selRegion = filterOuts;
                                bool bResult = true;
                                OneSelectParams oneSelectParams = null; //若有数量判断，则留到分选完后
                                foreach (var selParam in select.SelectParams)
                                {
                                    if (selParam.Character == EMFILTER.EMFILTER_NUM)
                                        oneSelectParams = selParam;
                                    else
                                        bResult = selParam.Excute(selRegion, out selRegion); //&&
                                }
                                if (!bResult)
                                    detection.regionOut = selRegion;
                                if (oneSelectParams != null)
                                    bResult = oneSelectParams.Excute(selRegion, out selRegion); //数量判断
                                if (!bResult)
                                {
                                    detection.regionOut = selRegion;
                                    detection.DetectLog.AppendLine(detection.DefectFilter.Name);
                                    detection.DetectLog.AppendLine(
                                        $"过滤器{de.FilterList.IndexOf(filter)}-分选{filter.SelectList.IndexOf(select)}"
                                    );
                                    detection.Result = false;
                                    break; //有一个分选不合格就跳出，不执行剩下的分选（||）
                                }
                            }
                            //有一个过滤分选器不合格就跳出，不执行剩下的过滤分选器（||）
                            if (!detection.Result)
                            {
                                filter.Result = false;
                                filterConfig[sp.Name].Result = false;
                                break;
                            }
                        }
                        SRegion maxRegion = new SRegion(); ;
                        if (detection.regionOut.Count > 0)
                        {
                            detection.regionOut.Sort(delegate (SRegion l, SRegion r) { return l.RegionInfo.PeakHeight.CompareTo(r.RegionInfo.PeakHeight); });
                            maxRegion = detection.regionOut.Last();
                        }
                        foreach (var item in de.ResultList)
                        {
                            switch (item.Feature)
                            {
                                case EMFILTER.EMFILTER_PEAKHEI:
                                    item.Value = maxRegion.RegionInfo.PeakHeight;
                                    break;
                                case EMFILTER.EMFILTER_AREA:
                                    item.Value = maxRegion.RegionInfo.Area;
                                    break;
                                case EMFILTER.EMFILTER_LONGLEN:
                                    item.Value = maxRegion.RegionInfo.LongLen;
                                    break;
                                case EMFILTER.EMFILTER_SHORTLEN:
                                    item.Value = maxRegion.RegionInfo.ShorLen;
                                    break;
                                case EMFILTER.EMFILTER_PHI:
                                    item.Value = maxRegion.RegionInfo.Phi;
                                    break;
                                case EMFILTER.EMFILTER_CONTLEN:
                                    item.Value = maxRegion.RegionInfo.ContLen;
                                    break;
                                case EMFILTER.EMFILTER_WIDTH:
                                    item.Value = maxRegion.RegionInfo.WidthBound;
                                    break;
                                case EMFILTER.EMFILTER_HEIGHT:
                                    item.Value = maxRegion.RegionInfo.HeightBound;
                                    break;
                                case EMFILTER.EMFILTER_NUM:
                                    item.Value = detection.regionOut.Count;
                                    break;
                                default:
                                    break;
                            }
                            detection.DetectLog.AppendLine(
                                $"{EnumStringAttribute.GetEnumName(item.Feature)}:"
                            );
                        }
                        if (!detection.Result) //NG
                        {
                            var qualityLevel = detection.DefectFilter.QualityLevel;
                            if (cell.Detection == null)
                            {
                                cell.Detection = detection;
                                cell.Quality = detection.DefectFilter.QualityLevel;
                            }
                            else
                            {
                                if (cell.Detection.DefectFilter.QualityLevel < qualityLevel) //质量等级 还需判断优先级
                                {
                                    cell.Detection = detection;
                                    cell.Quality = detection.DefectFilter.QualityLevel;
                                }
                                else if (
                                    cell.Detection.DefectFilter.QualityLevel == qualityLevel
                                    && cell.Detection?.DefectFilter.Priority
                                        < detection.DefectFilter.Priority
                                ) //质量等级相等时 判断优先级
                                {
                                    cell.Detection = detection;
                                    cell.Quality = detection.DefectFilter.QualityLevel;
                                }
                            }
                            cell.IsOK = false;
                        }
                        cell.Detections.Add(detection);
                    }
                }
            }
        }
    }
}
