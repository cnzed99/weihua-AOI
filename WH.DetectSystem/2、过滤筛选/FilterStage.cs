using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SDFilter;
using WH.Entity.Attribute;
using WH.Entity.CommonLib;
using WH.RecipeCellRootBase;
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
            if (cell.Skipthis)
            {
                return;
            }
            foreach (var sp in filterConfig.SpeciesFilters)
            {
                sp.Result = true;//检测类重置为OK
            }
            foreach (var algorithmOut in cell.AlgorithmOut)
            {
                foreach (
                    var de in filterConfig[algorithmOut.Type][
                        algorithmOut.RecipeDefectName
                    ].DefectFilters
                ) //缺陷
                {
                    de.Result = true;
                    CellDetection detection = algorithmOut.Clone();
                    detection.DefectFilter = de;
                    if (cell.CancelSource.IsCancellationRequested)
                        return; //任务取消时退出
                    switch (detection.Category)
                    {
                        case Category.区域:
                            {
                                SRegion[] Originregs = new SRegion[detection.regionOut.Count];
                                detection.regionOut.CopyTo(Originregs); //复制而不是引用 原始区域
                                var OriginRegList = Originregs.ToList();
                                foreach (var filter in de.FilterList) //过滤分选器
                                {
                                    filter.Result = true;
                                    //如果过滤分选器未使能或前面的过滤分选已经判定为NG，则跳过，不用break,是要把上一次的结果置为true，filter.Result = true;
                                    if (!filter.FilterSelectEnable || !de.Result)
                                    {
                                        continue;
                                    }
                                    SRegion[] regs = new SRegion[OriginRegList.Count];
                                    OriginRegList.CopyTo(regs); //复制而不是引用
                                    List<SRegion> detectRegion = new List<SRegion>(regs);
                                    if (detectRegion.Count > 0)
                                    {
                                        switch (filter.UnionMethod)
                                        {
                                            case EMUNIONMETHOD.EMUNIONMETHOD_UNION:
                                            {
                                                SRegion regionUnion = detectRegion[0]
                                                    .regionInfo.Union(detectRegion);
                                                detectRegion.Clear();
                                                detectRegion.Add(regionUnion);
                                                break;
                                            }
                                        }
                                    }

                                    List<SRegion> filterOuts = new List<SRegion>(); //过滤后的区域
                                    foreach (var select in filter.Filter) //过滤
                                    {
                                        SRegion[] regions = new SRegion[detectRegion.Count];
                                        detectRegion.CopyTo(regions); //复制而不是引用 同一过滤分选中的不同过滤器 过滤同一原始对象，过滤器之间是或的关系
                                        List<SRegion> selRegion = new List<SRegion>(regions);
                                        foreach (var selParam in select.SelectParams)
                                        {
                                            selParam.Excute(selRegion, out selRegion); //&&
                                        }
                                        filterOuts.AddRange(selRegion); //||
                                    }
                                    bool once = false;
                                    List<SRegion> selRegionALL = new List<SRegion>(); //所有分选的缺陷区域 add by bzb 20240813
                                    foreach (var select in filter.SelectList) //分选
                                    {
                                        SRegion[] regions = new SRegion[filterOuts.Count];
                                        filterOuts.CopyTo(regions); //复制而不是引用 同一过滤分选中的不同分选器 分选同一组过滤对象，分选器之间是或的关系
                                        List<SRegion> selRegion = new List<SRegion>(regions);
                                        bool bResult = true;
                                        OneSelectParams oneSelectParams = null; //若有数量判断，则留到分选完后由数量决定最终结果
                                        foreach (var selParam in select.SelectParams)
                                        {
                                            if (selParam.Character == CFeacture.FeactureCount)
                                                oneSelectParams = selParam;
                                            else
                                                bResult = selParam.Excute(selRegion, out selRegion); //分选器中的分选条件顺序执行，看最后结果
                                        }
                                        //if (!bResult)
                                        //    detection.regionOut = selRegion;
                                        if (oneSelectParams != null)
                                            bResult = oneSelectParams.Excute(
                                                selRegion,
                                                out selRegion
                                            ); //数量判断
                                        if (!bResult)
                                        {
                                            // detection.regionOut = selRegion;
                                            selRegionALL.AddRange(selRegion);
                                            
                                            if (!once)
                                            {
                                                for (int i = 0; i<selRegionALL.Count; i++)
                                                {
                                                    detection.DetectLog.Add(new StringBuilder(
                                                    $"{detection.DefectFilter.Name}:过滤器{de.FilterList.IndexOf(filter)}-分选{filter.SelectList.IndexOf(select)}\r\n" 
                                                    ));   
                                                }
                                               
                                                once = true;
                                            }
                                            //for (int i = 0; i<selRegionALL.Count; i++)
                                            //{
                                            //    detection.DetectLog.Add(new StringBuilder (
                                            //     $"过滤器{de.FilterList.IndexOf(filter)}-分选{filter.SelectList.IndexOf(select)}"
                                            // ));
                                            //}
                                            //detection.DetectLog.AppendLine(
                                            //    $"过滤器{de.FilterList.IndexOf(filter)}-分选{filter.SelectList.IndexOf(select)}"
                                            //);
                                            detection.Result = false;
                                            // break; //有一个分选不合格就跳出，不执行剩下的分选（||）
                                        }
                                    }
                                    detection.regionOut = selRegionALL;

                                    //有一个过滤分选器不合格就跳出，不执行剩下的过滤分选器（||）
                                    if (!detection.Result)
                                    {
                                        filter.Result = false;
                                        de.Result = false;
                                        filterConfig[detection.Type].Result = false;
                                        //break;//不在这里break，还需要把上一次的排在后面的过滤分选器重置为true，否则NG状态一直未变
                                    }
                                }
                                if (detection.regionOut?.Count > 0 && de.ResultList.Count > 0)
                                {
                                    //detection.regionOut.Sort(
                                    //    delegate(SRegion l, SRegion r)
                                    //    {
                                    //        return l
                                    //            .regionInfo.GetValue(de.ResultList[0].Feature, l)
                                    //            .CompareTo(
                                    //                r.regionInfo.GetValue(
                                    //                    de.ResultList[0].Feature,
                                    //                    r
                                    //                )
                                    //            );
                                    //    }
                                    //);
                                    for (int i = 0; i < detection.regionOut.Count; i++)
                                    {
                                        var maxRegion = detection.regionOut[i];
                                        foreach (var item in de.ResultList)
                                        {
                                            if (item.Feature == CFeacture.FeactureCount)
                                            {
                                                item.Value = detection.regionOut.Count;
                                            }
                                            else
                                            {
                                                item.Value = maxRegion.regionInfo.GetValue(
                                                    item.Feature,
                                                    maxRegion
                                                );
                                            }
                                            detection.DetectLog[i].AppendLine(
                                                $"{item.Feature.GetName()}:{item.Value:F2}"
                                            );
                                        }
                                    }
                                    
                                    //SRegion maxRegion = detection.regionOut.Last();
                                    //foreach (var item in de.ResultList)
                                    //{
                                    //    if (item.Feature == CFeacture.FeactureCount)
                                    //    {
                                    //        item.Value = detection.regionOut.Count;
                                    //    }
                                    //    else
                                    //    {
                                    //        item.Value = maxRegion.regionInfo.GetValue(
                                    //            item.Feature,
                                    //            maxRegion
                                    //        );
                                    //    }
                                    //    detection.DetectLog.AppendLine(
                                    //        $"{item.Feature.GetName()}:{item.Value:F2}"
                                    //    );
                                    //}
                                }
                                else
                                {
                                    foreach (var item in de.ResultList)
                                    {
                                        item.Value = 0;
                                        detection.DetectLog.Add(new StringBuilder(
                                            $"{item.Feature.GetName()}:{item.Value:F2}"
                                        ));
                                    }
                                }
                            }
                            break;
                        case Category.值:
                            {
                                foreach (var filter in de.FilterList) //过滤分选器
                                {
                                    filter.Result = true;
                                    //如果过滤分选器未使能或前面的过滤分选已经判定为NG，则跳过，不用break,是要把上一次的结果置为true，filter.Result = true;
                                    if (!filter.FilterSelectEnable || !de.Result)
                                    {
                                        continue;
                                    }
                                    if (detection.Value == null)
                                        detection.Value = new();
                                    List<float> filterOuts = new(); //过滤后的数值
                                    foreach (var select in filter.Filter) //过滤
                                    {
                                        List<float> filtValues = new(detection.Value);
                                        foreach (var selParam in select.SelectParams)
                                        {
                                            filtValues = filtValues.FindAll(o =>
                                                selParam.Excute(o)
                                            );
                                        }
                                        filterOuts.AddRange(filtValues); //||
                                    }
                                    bool once = false;
                                    List<float> selValueALL = new(); //分选后的数值
                                    foreach (var select in filter.SelectList) //分选
                                    {
                                        List<float> filtValues = new(filterOuts);
                                        bool bResult = true;
                                        OneSelectParams oneSelectParams = null; //若有数量判断，则留到分选完后由数量决定最终结果
                                        foreach (var selParam in select.SelectParams)
                                        {
                                            if (selParam.Character == CFeacture.FeactureCount)
                                                oneSelectParams = selParam;
                                            else
                                            {
                                                filtValues = filtValues.FindAll(o =>
                                                    selParam.Excute(o)
                                                );
                                                bResult = filtValues.Count == 0;
                                            }
                                        }
                                        if (oneSelectParams != null)
                                            bResult = !oneSelectParams.Excute(filtValues.Count);
                                        if (!bResult)
                                        {
                                            selValueALL.AddRange(filtValues);
                                            if (!once)
                                            {
                                                for (int i = 0; i<selValueALL.Count; i++)
                                                {
                                                    detection.DetectLog.Add(new StringBuilder(
                                                    detection.DefectFilter.Name
                                                    ));
                                                }
                                                once = true;
                                            }
                                            for (int i = 0; i<selValueALL.Count; i++)
                                            {
                                                detection.DetectLog.Add(new StringBuilder(
                                                 $"过滤器{de.FilterList.IndexOf(filter)}-分选{filter.SelectList.IndexOf(select)}"
                                             ));
                                            }
                                            //detection.DetectLog.AppendLine(
                                            //    $"过滤器{de.FilterList.IndexOf(filter)}-分选{filter.SelectList.IndexOf(select)}"
                                            //);
                                            detection.Result = false;
                                            // break; //有一个分选不合格就跳出，不执行剩下的分选（||）
                                        }
                                    }
                                    detection.Value = selValueALL;

                                    //有一个过滤分选器不合格就跳出，不执行剩下的过滤分选器（||）
                                    if (!detection.Result)
                                    {
                                        filter.Result = false;
                                        de.Result = false;
                                        filterConfig[detection.Type].Result = false;
                                        //break;//不在这里break，还需要把上一次的排在后面的过滤分选器重置为true，否则NG状态一直未变
                                    }
                                }
                                foreach (var item in de.ResultList)
                                {
                                    if (item.Feature == CFeacture.FeactureCount)
                                    {
                                        item.Value = detection.Value.Count;
                                    }
                                    else
                                    {
                                        item.Value =
                                            detection.Value.Count > 0 ? detection.Value.Max() : 0;
                                    }
                                    detection.DetectLog.Add(new StringBuilder(
                                            $"{item.Feature.GetName()}:{item.Value:F2}"
                                        ));
                                    //detection.DetectLog.AppendLine(
                                    //    $"{item.Feature.GetName()}:{item.Value:F2}"
                                    //);
                                }
                            }
                            break;
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
