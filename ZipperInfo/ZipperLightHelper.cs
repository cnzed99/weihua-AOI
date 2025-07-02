using HalconDotNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZipperLightHalconDet
{
    public class ZipperLightHelper
    {
        public static ZipperLightHelper Instance => _instance.Value;
        private static readonly Lazy<ZipperLightHelper> _instance = new Lazy<ZipperLightHelper>(() => new ZipperLightHelper());

        private ZipperLightHelper()
        {
        }

        /// <summary>
        /// 拉链图像亮度检测
        /// </summary>
        /// <param name="ho_Image">图像输入</param>
        /// <param name="hv_BrightnessDiff">亮度保留差值（计算值与预测值小于此值则无需调整直接返回），默认值：10，推荐值：8-15</param>
        /// <param name="hv_StrideRate">步幅调整灵敏度比率，默认值：0.5（根据实际光源控制器调整）</param>
        /// <param name="hv_VState">亮度评估状态，0：保持亮度、1：需增加光源亮度、2：需减少亮度</param>
        /// <param name="hv_VStride">推荐调整光源步幅值（供上层应用快速调整至合适的光源值使用）</param>
        public void ZipperLightDetection(HObject ho_Image, HTuple hv_BrightnessDiff, HTuple hv_StrideRate,
     out HTuple hv_VState, out HTuple hv_VStride)
        {




            // Local iconic variables 

            HObject ho_RegionClosing1, ho_RegionOpening1;
            HObject ho_SelectROI = null, ho_Edges, ho_Region, ho_RegionUnion;
            HObject ho_RegionTrans, ho_Rectangle, ho_SelectBgROI, ho_ImageReduced;
            HObject ho_GrayImageReduced, ho_R, ho_G, ho_B, ho_H, ho_S;
            HObject ho_V;

            // Local control variables 

            HTuple hv_MinBgMean = new HTuple(), hv_MaxBgMean = new HTuple();
            HTuple hv_MinMean = new HTuple(), hv_MaxMean = new HTuple();
            HTuple hv_Width = new HTuple(), hv_Height = new HTuple();
            HTuple hv_Area = new HTuple(), hv_Row = new HTuple(), hv_Column = new HTuple();
            HTuple hv_BgValue = new HTuple(), hv_HValue = new HTuple();
            HTuple hv_SValue = new HTuple(), hv_VValue = new HTuple();
            HTuple hv_MValue = new HTuple(), hv_NormalizedDiff = new HTuple();
            HTuple hv_BgRatio = new HTuple(), hv_ValueRatio = new HTuple();
            // Initialize local and output iconic variables 
            HOperatorSet.GenEmptyObj(out ho_RegionClosing1);
            HOperatorSet.GenEmptyObj(out ho_RegionOpening1);
            HOperatorSet.GenEmptyObj(out ho_SelectROI);
            HOperatorSet.GenEmptyObj(out ho_Edges);
            HOperatorSet.GenEmptyObj(out ho_Region);
            HOperatorSet.GenEmptyObj(out ho_RegionUnion);
            HOperatorSet.GenEmptyObj(out ho_RegionTrans);
            HOperatorSet.GenEmptyObj(out ho_Rectangle);
            HOperatorSet.GenEmptyObj(out ho_SelectBgROI);
            HOperatorSet.GenEmptyObj(out ho_ImageReduced);
            HOperatorSet.GenEmptyObj(out ho_GrayImageReduced);
            HOperatorSet.GenEmptyObj(out ho_R);
            HOperatorSet.GenEmptyObj(out ho_G);
            HOperatorSet.GenEmptyObj(out ho_B);
            HOperatorSet.GenEmptyObj(out ho_H);
            HOperatorSet.GenEmptyObj(out ho_S);
            HOperatorSet.GenEmptyObj(out ho_V);
            hv_VState = new HTuple();
            hv_VStride = new HTuple();
            try
            {
                hv_VState.Dispose();
                hv_VState = 0;
                //BrightnessDiff := 10
                //EmphaMaskValue := 7
                //EmphaFactorValue := 0.7
                hv_MinBgMean.Dispose();
                hv_MinBgMean = 130;
                hv_MaxBgMean.Dispose();
                hv_MaxBgMean = 230;
                hv_MinMean.Dispose();
                hv_MinMean = 100;
                hv_MaxMean.Dispose();
                hv_MaxMean = 180;
                hv_Width.Dispose(); hv_Height.Dispose();
                HOperatorSet.GetImageSize(ho_Image, out hv_Width, out hv_Height);

                //BGPolarity := 'light'
                //if (BGPolarity == 'dark')
                //MaxLinesGaussLineWidth := 35
                //MaxLinesGaussContrast := 25
                //else
                //MaxLinesGaussLineWidth := 30
                //MaxLinesGaussContrast := 40
                //EmphaMaskValue := 3
                //EmphaFactorValue := 0.3
                //endif

                //rgb1_to_gray (Image, GrayImage)
                //emphasize (GrayImage, ImageEmphasize, EmphaMaskValue, EmphaMaskValue, EmphaFactorValue)

                //mean_image (GrayImage, ImageMean, 15, 15)
                //dyn_threshold (ImageEmphasize, ImageMean, RegionDynThresh, 35, 'light')
                //过滤杂点
                //closing_circle (RegionDynThresh, RegionClosing1, 15.5)
                //opening_circle (RegionClosing1, RegionOpening1, 1.5)
                //强化ROI区域
                //closing_rectangle1 (RegionOpening1, RegionClosing, 25, 25)
                //fill_up (RegionClosing, RegionFillUp)
                //opening_rectangle1 (RegionFillUp, RegionOpening, 110, 110)

                //erosion_rectangle1 (RegionOpening, RegionErosion, 5, 5)
                //SelectROI := RegionErosion

                ho_Edges.Dispose();
                HOperatorSet.EdgesColorSubPix(ho_Image, out ho_Edges, "canny", 1.5, 20, 40);
                ho_Region.Dispose();
                HOperatorSet.GenRegionContourXld(ho_Edges, out ho_Region, "filled");
                ho_RegionUnion.Dispose();
                HOperatorSet.Union1(ho_Region, out ho_RegionUnion);
                ho_RegionClosing1.Dispose();
                HOperatorSet.ClosingCircle(ho_RegionUnion, out ho_RegionClosing1, 15.5);
                ho_RegionOpening1.Dispose();
                HOperatorSet.OpeningCircle(ho_RegionClosing1, out ho_RegionOpening1, 1.0);
                ho_RegionTrans.Dispose();
                HOperatorSet.ShapeTrans(ho_RegionOpening1, out ho_RegionTrans, "convex");
                hv_Area.Dispose(); hv_Row.Dispose(); hv_Column.Dispose();
                HOperatorSet.AreaCenter(ho_RegionTrans, out hv_Area, out hv_Row, out hv_Column);
                if ((int)((new HTuple(hv_Area.TupleEqual(new HTuple()))).TupleOr(new HTuple(hv_Area.TupleLess(
                    100)))) != 0)
                {
                    hv_VState.Dispose();
                    hv_VState = 1;
                    hv_VStride.Dispose();
                    hv_VStride = 30;
                    ho_RegionClosing1.Dispose();
                    ho_RegionOpening1.Dispose();
                    ho_SelectROI.Dispose();
                    ho_Edges.Dispose();
                    ho_Region.Dispose();
                    ho_RegionUnion.Dispose();
                    ho_RegionTrans.Dispose();
                    ho_Rectangle.Dispose();
                    ho_SelectBgROI.Dispose();
                    ho_ImageReduced.Dispose();
                    ho_GrayImageReduced.Dispose();
                    ho_R.Dispose();
                    ho_G.Dispose();
                    ho_B.Dispose();
                    ho_H.Dispose();
                    ho_S.Dispose();
                    ho_V.Dispose();

                    hv_MinBgMean.Dispose();
                    hv_MaxBgMean.Dispose();
                    hv_MinMean.Dispose();
                    hv_MaxMean.Dispose();
                    hv_Width.Dispose();
                    hv_Height.Dispose();
                    hv_Area.Dispose();
                    hv_Row.Dispose();
                    hv_Column.Dispose();
                    hv_BgValue.Dispose();
                    hv_HValue.Dispose();
                    hv_SValue.Dispose();
                    hv_VValue.Dispose();
                    hv_MValue.Dispose();
                    hv_NormalizedDiff.Dispose();
                    hv_BgRatio.Dispose();
                    hv_ValueRatio.Dispose();

                    return;
                }
                ho_SelectROI.Dispose();
                ho_SelectROI = new HObject(ho_RegionTrans);

                if (HDevWindowStack.IsOpen())
                {
                    HOperatorSet.SetColor(HDevWindowStack.GetActive(), "yellow");
                }
                if (HDevWindowStack.IsOpen())
                {
                    HOperatorSet.DispObj(ho_SelectROI, HDevWindowStack.GetActive());
                }

                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    ho_Rectangle.Dispose();
                    HOperatorSet.GenRectangle1(out ho_Rectangle, 0, hv_Width * 0.1, hv_Height, hv_Width * 0.9);
                }
                ho_SelectBgROI.Dispose();
                HOperatorSet.Difference(ho_Rectangle, ho_SelectROI, out ho_SelectBgROI);
                if (HDevWindowStack.IsOpen())
                {
                    HOperatorSet.SetColor(HDevWindowStack.GetActive(), "green");
                }
                if (HDevWindowStack.IsOpen())
                {
                    HOperatorSet.DispObj(ho_SelectBgROI, HDevWindowStack.GetActive());
                }
                hv_BgValue.Dispose();
                HOperatorSet.GrayFeatures(ho_SelectBgROI, ho_Image, "mean", out hv_BgValue);

                ho_ImageReduced.Dispose();
                HOperatorSet.ReduceDomain(ho_Image, ho_SelectROI, out ho_ImageReduced);
                ho_GrayImageReduced.Dispose();
                HOperatorSet.Rgb1ToGray(ho_ImageReduced, out ho_GrayImageReduced);

                //拆分RGB通道
                ho_R.Dispose(); ho_G.Dispose(); ho_B.Dispose();
                HOperatorSet.Decompose3(ho_ImageReduced, out ho_R, out ho_G, out ho_B);

                //转换为HSV颜色空间
                ho_H.Dispose(); ho_S.Dispose(); ho_V.Dispose();
                HOperatorSet.TransFromRgb(ho_R, ho_G, ho_B, out ho_H, out ho_S, out ho_V, "hsv");

                hv_HValue.Dispose();
                HOperatorSet.GrayFeatures(ho_SelectROI, ho_H, "mean", out hv_HValue);
                hv_SValue.Dispose();
                HOperatorSet.GrayFeatures(ho_SelectROI, ho_S, "mean", out hv_SValue);
                hv_VValue.Dispose();
                HOperatorSet.GrayFeatures(ho_SelectROI, ho_V, "mean", out hv_VValue);

                //提升深色系最佳均值亮度值
                hv_MValue.Dispose();
                HOperatorSet.GrayFeatures(ho_SelectROI, ho_GrayImageReduced, "mean", out hv_MValue);
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    {
                        HTuple
                          ExpTmpLocalVar_VValue = (hv_VValue + hv_MValue) / 2.0;
                        hv_VValue.Dispose();
                        hv_VValue = ExpTmpLocalVar_VValue;
                    }
                }

                //线性化参数计算
                //标准化差异值
                hv_NormalizedDiff.Dispose();
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_NormalizedDiff = (hv_BrightnessDiff.TupleReal()
                        ) / 100.0;
                }

                //状态1处理：低于下限的情况
                if ((int)((new HTuple((new HTuple(hv_VValue.TupleLessEqual(hv_MaxMean))).TupleAnd(
                    new HTuple(hv_BgValue.TupleLess(hv_MinBgMean - hv_BrightnessDiff))))).TupleOr(
                    new HTuple(hv_VValue.TupleLess(hv_MinMean - hv_BrightnessDiff)))) != 0)
                {
                    hv_VState.Dispose();
                    hv_VState = 1;
                    //线性插值计算步长
                    hv_BgRatio.Dispose();
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        hv_BgRatio = (new HTuple(0.0)).TupleMax2(
                            (hv_MinBgMean - hv_BgValue) / hv_MinBgMean);
                    }
                    hv_ValueRatio.Dispose();
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        hv_ValueRatio = (new HTuple(0.0)).TupleMax2(
                            (hv_MinMean - hv_VValue) / hv_MinMean);
                    }
                    //线性组合
                    hv_VStride.Dispose();
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        hv_VStride = ((((hv_BgRatio + hv_ValueRatio) * 10.0) * hv_StrideRate)).TupleInt()
                            ;
                    }

                    //状态2处理：高于上限的情况
                }
                else if ((int)((new HTuple(hv_BgValue.TupleGreater(hv_MaxBgMean + hv_BrightnessDiff))).TupleOr(
                    new HTuple(hv_VValue.TupleGreater(hv_MaxMean + hv_BrightnessDiff)))) != 0)
                {
                    hv_VState.Dispose();
                    hv_VState = 2;
                    //线性插值计算步长
                    hv_BgRatio.Dispose();
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        hv_BgRatio = (new HTuple(0.0)).TupleMax2(
                            (hv_BgValue - hv_MaxBgMean) / hv_MaxBgMean);
                    }
                    hv_ValueRatio.Dispose();
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        hv_ValueRatio = (new HTuple(0.0)).TupleMax2(
                            (hv_VValue - hv_MaxMean) / hv_MaxMean);
                    }
                    hv_VStride.Dispose();
                    using (HDevDisposeHelper dh = new HDevDisposeHelper())
                    {
                        hv_VStride = ((((hv_BgRatio + hv_ValueRatio) * 20.0) * hv_StrideRate)).TupleInt()
                            ;
                    }

                    //正常状态
                }
                else
                {
                    hv_VState.Dispose();
                    hv_VState = 0;
                    hv_VStride.Dispose();
                    hv_VStride = 0;
                }

                //predict_v_from_color (HValue, SValue, V_predicted)

                //CValue := abs(VValue - V_predicted)
                //VStride := int(CValue / 2.0 * StrideRate)

                //if (CValue <= BrightnessDiff or MaxBgMean - BgValue <= BrightnessDiff)
                //VState := 0
                //VStride := 0.0
                //elseif (VValue < V_predicted)
                //VState := 1
                //elseif (VValue >  V_predicted)
                //VState := 2
                //endif
                ho_RegionClosing1.Dispose();
                ho_RegionOpening1.Dispose();
                ho_SelectROI.Dispose();
                ho_Edges.Dispose();
                ho_Region.Dispose();
                ho_RegionUnion.Dispose();
                ho_RegionTrans.Dispose();
                ho_Rectangle.Dispose();
                ho_SelectBgROI.Dispose();
                ho_ImageReduced.Dispose();
                ho_GrayImageReduced.Dispose();
                ho_R.Dispose();
                ho_G.Dispose();
                ho_B.Dispose();
                ho_H.Dispose();
                ho_S.Dispose();
                ho_V.Dispose();

                hv_MinBgMean.Dispose();
                hv_MaxBgMean.Dispose();
                hv_MinMean.Dispose();
                hv_MaxMean.Dispose();
                hv_Width.Dispose();
                hv_Height.Dispose();
                hv_Area.Dispose();
                hv_Row.Dispose();
                hv_Column.Dispose();
                hv_BgValue.Dispose();
                hv_HValue.Dispose();
                hv_SValue.Dispose();
                hv_VValue.Dispose();
                hv_MValue.Dispose();
                hv_NormalizedDiff.Dispose();
                hv_BgRatio.Dispose();
                hv_ValueRatio.Dispose();

                return;
            }
            catch (HalconException HDevExpDefaultException)
            {
                ho_RegionClosing1.Dispose();
                ho_RegionOpening1.Dispose();
                ho_SelectROI.Dispose();
                ho_Edges.Dispose();
                ho_Region.Dispose();
                ho_RegionUnion.Dispose();
                ho_RegionTrans.Dispose();
                ho_Rectangle.Dispose();
                ho_SelectBgROI.Dispose();
                ho_ImageReduced.Dispose();
                ho_GrayImageReduced.Dispose();
                ho_R.Dispose();
                ho_G.Dispose();
                ho_B.Dispose();
                ho_H.Dispose();
                ho_S.Dispose();
                ho_V.Dispose();

                hv_MinBgMean.Dispose();
                hv_MaxBgMean.Dispose();
                hv_MinMean.Dispose();
                hv_MaxMean.Dispose();
                hv_Width.Dispose();
                hv_Height.Dispose();
                hv_Area.Dispose();
                hv_Row.Dispose();
                hv_Column.Dispose();
                hv_BgValue.Dispose();
                hv_HValue.Dispose();
                hv_SValue.Dispose();
                hv_VValue.Dispose();
                hv_MValue.Dispose();
                hv_NormalizedDiff.Dispose();
                hv_BgRatio.Dispose();
                hv_ValueRatio.Dispose();

                throw HDevExpDefaultException;
            }
        }

    }
}
