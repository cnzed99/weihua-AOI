using System;
using System.IO;
using System.Windows.Media;
using HalconDotNet;
using WH.RunCell;

namespace GearTestAlgorihm
{
    /// <summary>
    /// 【盘齿方案3.7-注释】上端面「孔钻错」包装 gear_angle_fast 主干。
    /// MaxDev 用拟合外圆圆心 BigRows/BigCols 相对内圆圆心的极角夹角对 120° 的最大偏差（度），不用 SmallRows。
    /// </summary>
    public class GearAngleAlgorithm
    {
        private const double Pi = 3.141592653589793;

        //【盘齿方案3.7-注释】数值旋钮只挂本类，默认抄 fast；禁止写入共用 CParam（七制程会脏面板）
        public int UseMetrology = 1;
        public int InnerDarkMax = 120;
        public double InnerAreaMinFrac = 0.015;
        public double InnerAreaMaxFrac = 0.15;
        public double InnerRMaxFrac = 0.28;
        public double InnerSearchFrac = 0.72;
        public double InnerCircMin = 0.4;
        public double RingOutFrac = 2.70;
        public double RingInFrac = 1.43;
        public double OpenRFrac = 0.008;
        public double OpenRMin = 1.5;
        public double HoleAreaMinFrac = 0.025;
        public double HoleAreaMaxFrac = 0.10;
        public double HoleRMinFrac = 0.17;
        public double HoleRMaxFrac = 0.30;
        public double DistCMinFrac = 2.05;
        public double DistCMaxFrac = 2.75;
        public int HoleThStart = 100;
        public int HoleThEnd = 130;
        public int HoleThStep = 10;
        public double MetroInitRFrac = 1.15;
        public double MetroLen1Frac = 0.26;
        public double MetroLen1AltFrac = 0.45;
        public double MetroLen2 = 5.0;
        public double MetroSigma = 1.5;
        public int MetroThreshold = 20;
        public double MetroMinScore = 0.35;
        public int MetroNumMeasures = 40;
        public double MetroRadMinFrac = 0.85;
        public double MetroRadMaxFrac = 1.08;
        public double MetroDistLocFrac = 0.15;
        public double MorphNbhdFrac = 1.20;
        public double MorphAnnOutFrac = 1.14;
        public double MorphAnnInFrac = 0.70;
        public double MorphInnerMinFrac = 0.22;
        public int MorphThStart = 50;
        public int MorphThEnd = 80;
        public int MorphThStep = 10;
        public double TargetAngleDeg = 120.0;

        /// <summary>
        /// 【盘齿方案3.7-注释】灌图/WPF 常为 Bgr32、Pbgra32。Halcon 的 "rgb" 按 3 字节读会拆坏像素，内圆 SelectShape 会 NInner=0。
        /// </summary>
        private static string HalconColorFormat(PixelFormat pf)
        {
            if (pf == PixelFormats.Bgr24)
            {
                return "bgr";
            }
            if (pf == PixelFormats.Rgb24)
            {
                return "rgb";
            }
            if (pf == PixelFormats.Bgr32 || pf == PixelFormats.Bgra32 || pf == PixelFormats.Pbgra32)
            {
                return "bgrx";
            }
            if (pf.BitsPerPixel == 32)
            {
                return "bgrx";
            }
            return "rgb";
        }

        //【盘齿方案3.7-注释】禁止 params IDisposable[]/HTuple[]。单参数 HTuple 会走隐式转 HObject[]，整数 tuple 抛 Cannot convert to handle array。
        private static void DisposeH(params HObject[] items)
        {
            if (items == null)
            {
                return;
            }
            for (int i = 0; i < items.Length; i++)
            {
                HObject d = items[i];
                if (d == null)
                {
                    continue;
                }
                try
                {
                    d.Dispose();
                }
                catch
                {
                }
            }
        }

        private static void DisposeT(HTuple t)
        {
            if (t == null)
            {
                return;
            }
            try
            {
                t.Dispose();
            }
            catch
            {
            }
        }

        private static void DisposeT(HTuple a, HTuple b)
        {
            DisposeT(a);
            DisposeT(b);
        }

        private static void DisposeT(HTuple a, HTuple b, HTuple c)
        {
            DisposeT(a);
            DisposeT(b);
            DisposeT(c);
        }

        private static void DisposeT(HTuple a, HTuple b, HTuple c, HTuple d, HTuple e, HTuple f)
        {
            DisposeT(a);
            DisposeT(b);
            DisposeT(c);
            DisposeT(d);
            DisposeT(e);
            DisposeT(f);
        }

        private static double[] ToD(HTuple t)
        {
            if (t == null || t.Length < 1)
            {
                return new double[0];
            }
            return t.ToDArr();
        }

        /// <summary>
        /// 【盘齿方案3.7-注释】atan2(-(row-r0), col-c0) → 度 → 负角+360。漏负号会对不过 HDevelop。
        /// </summary>
        private static double PolarDeg(double row, double col, double row0, double col0)
        {
            double deg = Math.Atan2(-(row - row0), col - col0) * (180.0 / Math.PI);
            if (deg < 0.0)
            {
                deg += 360.0;
            }
            return deg;
        }

        //【盘齿方案3.7-注释】section 4：孔1=最靠右，其后逆时针。SortIdx 为 SmallRows 的 0-based 下标。
        private static int[] SortIdxRightThenCcw(double[] smallRows, double[] smallCols, double row0, double col0)
        {
            double[] degWrap = new double[3];
            for (int i = 0; i < 3; i++)
            {
                degWrap[i] = PolarDeg(smallRows[i], smallCols[i], row0, col0);
            }
            int[] polarIdx = new int[] { 0, 1, 2 };
            Array.Sort(polarIdx, delegate (int a, int b)
            {
                int c = degWrap[a].CompareTo(degWrap[b]);
                return c != 0 ? c : a.CompareTo(b);
            });
            double bestD = 999.0;
            int rot = 0;
            for (int k = 0; k < 3; k++)
            {
                double dRight = degWrap[polarIdx[k]];
                if (dRight > 180.0)
                {
                    dRight = 360.0 - dRight;
                }
                if (dRight < bestD)
                {
                    bestD = dRight;
                    rot = k;
                }
            }
            int[] sortIdx = new int[3];
            for (int k = 0; k < 3; k++)
            {
                int j = k + rot;
                if (j > 2)
                {
                    j -= 3;
                }
                sortIdx[k] = polarIdx[j];
            }
            return sortIdx;
        }

        private static void ReplaceObj(ref HObject dest, HObject neu)
        {
            DisposeH(dest);
            dest = neu;
        }

        /// <summary>
        /// 【盘齿方案3.7-注释】入图优先 cell.Image；Image 为空才读 ImageFile。异常/无许可不弹窗，由调用方写哨兵 1000。
        /// </summary>
        public GearAngleRunResult Run(Cell cell)
        {
            GearAngleRunResult result = new GearAngleRunResult();
            result.MaxDev = 1000f;
            result.HoleNum = 0;
            result.FitN = 0;
            result.Warn = "";
            result.Angle12 = 0;
            result.Angle23 = 0;
            result.Angle31 = 0;

            HObject ho_Image = null, ho_ImgR = null, ho_ImgG = null, ho_ImgB = null, ho_MaxRG = null, ho_MaxGray = null;
            HObject ho_InnerSearch = null, ho_ImgInner = null, ho_RegionDark = null, ho_ConnectedDark = null, ho_InnerCands = null;
            HObject ho_InnerRegion = null, ho_InnerFilled = null, ho_CircleOut = null, ho_CircleIn = null, ho_RingROI = null, ho_Holes = null;
            HObject ho_ImgRing = null, ho_RegionHoles = null, ho_HoleCandidates = null, ho_HolesSel = null, ho_HolesTmp = null;
            HObject ho_One = null, ho_OneF = null, ho_Hole = null, ho_HoleFilled = null;
            HObject ho_HoleNbhd = null, ho_ImgHole = null, ho_AnnOut = null, ho_AnnIn = null, ho_Annulus = null, ho_ImgAnn = null;
            HObject ho_Chamfer = null, ho_ChamferU = null, ho_ChamferFilled = null, ho_InnerDisk = null;
            HObject ho_OuterBound = null, ho_InnerBound = null, ho_OuterXLD = null, ho_InnerXLD = null;
            HObject ho_TmpX = null, ho_OuterBest = null, ho_InnerBest = null;
            HTuple hv_MetrologyHandle = null, hv_MetroNames = null, hv_MetroVals = null;
            HTuple featHole = null, minHole = null, maxHole = null;
            bool metroCreated = false;
            int useMetrology = UseMetrology;

            try
            {
                //【盘齿方案3.7-注释】8bit 单通道 GenImage1；打包 RGB 与拉链相同 GenImageInterleaved；再按 fast 转 MaxGray
                bool gotImage = false;
                if (cell != null && cell.Image != null && cell.Image.ImageData != IntPtr.Zero)
                {
                    int bits = cell.Image.PixelFormat.BitsPerPixel;
                    int w = cell.Image.ImageWidth;
                    int h = cell.Image.ImageHeight;
                    DisposeH(ho_Image);
                    if (bits <= 8)
                    {
                        result.ColorFmt = "byte1";
                        HOperatorSet.GenImage1(out ho_Image, "byte", w, h, cell.Image.ImageData);
                    }
                    else
                    {
                        result.ColorFmt = HalconColorFormat(cell.Image.PixelFormat);
                        HOperatorSet.GenImageInterleaved(out ho_Image, cell.Image.ImageData,
                            result.ColorFmt, w, h, -1, "byte", 0, 0, 0, 0, -1, 0);
                    }
                    gotImage = true;
                    result.Bits = bits;
                }
                else if (cell != null && !string.IsNullOrEmpty(cell.ImageFile) && File.Exists(cell.ImageFile))
                {
                    DisposeH(ho_Image);
                    HOperatorSet.ReadImage(out ho_Image, cell.ImageFile);
                    gotImage = true;
                }

                if (!gotImage)
                {
                    result.Warn = "no image";
                    return result;
                }

                //【盘齿方案3.7-注释】以下为 gear_angle_fast action() 读图之后到 MaxDev（已删 SetSystem/窗口/显示/硬编码路径）
                HTuple hv_Width = null, hv_Height = null, hv_Channels = null;
                HOperatorSet.GetImageSize(ho_Image, out hv_Width, out hv_Height);
                HOperatorSet.CountChannels(ho_Image, out hv_Channels);
                int width = hv_Width.I;
                int height = hv_Height.I;
                int channels = hv_Channels.I;
                DisposeT(hv_Width, hv_Height, hv_Channels);
                result.Width = width;
                result.Height = height;
                result.Channels = channels;
                if (channels == 3)
                {
                    DisposeH(ho_ImgR, ho_ImgG, ho_ImgB);
                    HOperatorSet.Decompose3(ho_Image, out ho_ImgR, out ho_ImgG, out ho_ImgB);
                    DisposeH(ho_MaxRG);
                    HOperatorSet.MaxImage(ho_ImgR, ho_ImgG, out ho_MaxRG);
                    DisposeH(ho_MaxGray);
                    HOperatorSet.MaxImage(ho_MaxRG, ho_ImgB, out ho_MaxGray);
                }
                else
                {
                    DisposeH(ho_MaxGray);
                    HOperatorSet.CopyImage(ho_Image, out ho_MaxGray);
                }

                int err = 0;
                string errMsg = "OK";
                int holeNum = 0;
                int fitN = 0;
                double row0 = 0.0, col0 = 0.0, radius0 = 0.0;
                double angle12 = 0.0, angle23 = 0.0, angle31 = 0.0, maxDev = 0.0;
                double[] bigRows = new double[3];
                double[] bigCols = new double[3];
                double[] bigRads = new double[3];

                //========== 2. 内圆: 图心附近 reduce_domain 后 threshold 1次 ==========
                //面积 1.5%~15% 画面, outer_radius<=0.28*min(H,W), 圆度>=0.4, 质心离图心最近
                double areaImg = (double)width * height;
                double innerAreaMin = areaImg * InnerAreaMinFrac;
                double innerAreaMax = areaImg * InnerAreaMaxFrac;
                int minSide = width < height ? width : height;
                double innerRMax = minSide * InnerRMaxFrac;
                DisposeH(ho_InnerSearch);
                HOperatorSet.GenCircle(out ho_InnerSearch, height / 2.0, width / 2.0, minSide * InnerSearchFrac);
                DisposeH(ho_ImgInner);
                HOperatorSet.ReduceDomain(ho_MaxGray, ho_InnerSearch, out ho_ImgInner);
                DisposeH(ho_RegionDark);
                HOperatorSet.Threshold(ho_ImgInner, out ho_RegionDark, 0, InnerDarkMax);
                DisposeH(ho_ConnectedDark);
                HOperatorSet.Connection(ho_RegionDark, out ho_ConnectedDark);
                HTuple hv_NDark = null;
                HOperatorSet.CountObj(ho_ConnectedDark, out hv_NDark);
                result.NDark = hv_NDark.I;
                DisposeT(hv_NDark);
                HTuple featInner = ((new HTuple("area")).TupleConcat("outer_radius")).TupleConcat("circularity");
                HTuple minInner = new HTuple(innerAreaMin).TupleConcat((new HTuple(0)).TupleConcat(InnerCircMin));
                HTuple maxInner = (new HTuple(innerAreaMax).TupleConcat(innerRMax)).TupleConcat(1.0);
                DisposeH(ho_InnerCands);
                HOperatorSet.SelectShape(ho_ConnectedDark, out ho_InnerCands, featInner, "and", minInner, maxInner);
                DisposeT(featInner, minInner, maxInner);
                HTuple hv_NInner = null;
                HOperatorSet.CountObj(ho_InnerCands, out hv_NInner);
                int nInner = hv_NInner.I;
                DisposeT(hv_NInner);
                if (nInner < 1)
                {
                    err = 1;
                    errMsg = "inner fail NInner=0 NDark=" + result.NDark;
                }
                else
                {
                    HTuple candsArea = null, candsRow = null, candsCol = null;
                    HOperatorSet.AreaCenter(ho_InnerCands, out candsArea, out candsRow, out candsCol);
                    double imgCy = height / 2.0;
                    double imgCx = width / 2.0;
                    double[] candRows = ToD(candsRow);
                    double[] candCols = ToD(candsCol);
                    int distMinIdx = 0;
                    double bestDist2 = double.MaxValue;
                    for (int i = 0; i < candRows.Length; i++)
                    {
                        double dR = candRows[i] - imgCy;
                        double dC = candCols[i] - imgCx;
                        double dist2 = dR * dR + dC * dC;
                        if (dist2 < bestDist2)
                        {
                            bestDist2 = dist2;
                            distMinIdx = i;
                        }
                    }
                    DisposeT(candsArea, candsRow, candsCol);
                    DisposeH(ho_InnerRegion);
                    HOperatorSet.SelectObj(ho_InnerCands, out ho_InnerRegion, distMinIdx + 1);
                    DisposeH(ho_InnerFilled);
                    HOperatorSet.FillUp(ho_InnerRegion, out ho_InnerFilled);
                    HTuple hv_Row0 = null, hv_Col0 = null, hv_Radius0 = null;
                    HOperatorSet.SmallestCircle(ho_InnerFilled, out hv_Row0, out hv_Col0, out hv_Radius0);
                    row0 = hv_Row0.D;
                    col0 = hv_Col0.D;
                    radius0 = hv_Radius0.D;
                    DisposeT(hv_Row0, hv_Col0, hv_Radius0);
                }

                double[] smallRows = new double[3];
                double[] smallCols = new double[3];
                double[] smallRads = new double[3];
                int[] sortIdx = null;

                //========== 3. 环带 ROI=[1.43,2.70]*R0, reduce_domain 后再扫 4 档阈 ==========
                //定位三孔: Th 100..130 全扫, HoleNum=3 且 min circularity 最大. 禁止早停.
                if (err == 0)
                {
                    DisposeH(ho_CircleOut);
                    HOperatorSet.GenCircle(out ho_CircleOut, row0, col0, RingOutFrac * radius0);
                    DisposeH(ho_CircleIn);
                    HOperatorSet.GenCircle(out ho_CircleIn, row0, col0, RingInFrac * radius0);
                    DisposeH(ho_RingROI);
                    HOperatorSet.Difference(ho_CircleOut, ho_CircleIn, out ho_RingROI);
                    double openR = OpenRFrac * radius0;
                    if (openR < 1.5)
                    {
                        openR = OpenRMin;
                    }
                    double areaMin = ((HoleAreaMinFrac * Pi) * radius0) * radius0;
                    double areaMax = ((HoleAreaMaxFrac * Pi) * radius0) * radius0;
                    double holeRMin = HoleRMinFrac * radius0;
                    double holeRMax = HoleRMaxFrac * radius0;
                    double distCMin = DistCMinFrac * radius0;
                    double distCMax = DistCMaxFrac * radius0;
                    DisposeH(ho_Holes);
                    HOperatorSet.GenEmptyObj(out ho_Holes);
                    double bestMinCirc = -1.0;
                    DisposeH(ho_ImgRing);
                    HOperatorSet.ReduceDomain(ho_MaxGray, ho_RingROI, out ho_ImgRing);
                    DisposeT(featHole, minHole, maxHole);
                    featHole = ((new HTuple("area")).TupleConcat("circularity")).TupleConcat("outer_radius");
                    minHole = ((new HTuple(areaMin).TupleConcat(0.5))).TupleConcat(holeRMin);
                    maxHole = ((new HTuple(areaMax).TupleConcat(1.0))).TupleConcat(holeRMax);
                    for (int th = HoleThStart; th <= HoleThEnd; th += HoleThStep)
                    {
                        DisposeH(ho_RegionHoles);
                        HOperatorSet.Threshold(ho_ImgRing, out ho_RegionHoles, 0, th);
                        HObject opened = null;
                        HOperatorSet.OpeningCircle(ho_RegionHoles, out opened, openR);
                        ReplaceObj(ref ho_RegionHoles, opened);
                        DisposeH(ho_HoleCandidates);
                        HOperatorSet.Connection(ho_RegionHoles, out ho_HoleCandidates);
                        DisposeH(ho_HolesSel);
                        HOperatorSet.SelectShape(ho_HoleCandidates, out ho_HolesSel, featHole, "and", minHole, maxHole);
                        HTuple hv_NSel = null;
                        HOperatorSet.CountObj(ho_HolesSel, out hv_NSel);
                        int nSel = hv_NSel.I;
                        DisposeT(hv_NSel);
                        DisposeH(ho_HolesTmp);
                        HOperatorSet.GenEmptyObj(out ho_HolesTmp);
                        int nTmp = 0;
                        double minC = 1.0;
                        for (int i = 1; i <= nSel; i++)
                        {
                            DisposeH(ho_One);
                            HOperatorSet.SelectObj(ho_HolesSel, out ho_One, i);
                            DisposeH(ho_OneF);
                            HOperatorSet.FillUp(ho_One, out ho_OneF);
                            HTuple hv_SR = null, hv_SC = null, hv_SRad = null;
                            HOperatorSet.SmallestCircle(ho_OneF, out hv_SR, out hv_SC, out hv_SRad);
                            double distC = Math.Sqrt(((hv_SR.D - row0) * (hv_SR.D - row0)) + ((hv_SC.D - col0) * (hv_SC.D - col0)));
                            DisposeT(hv_SR, hv_SC, hv_SRad);
                            if (distC >= distCMin && distC <= distCMax)
                            {
                                HObject concat = null;
                                HOperatorSet.ConcatObj(ho_HolesTmp, ho_One, out concat);
                                ReplaceObj(ref ho_HolesTmp, concat);
                                nTmp = nTmp + 1;
                                HTuple hv_CircOne = null;
                                HOperatorSet.Circularity(ho_One, out hv_CircOne);
                                if (hv_CircOne.D < minC)
                                {
                                    minC = hv_CircOne.D;
                                }
                                DisposeT(hv_CircOne);
                            }
                        }
                        if (nTmp == 3 && minC > bestMinCirc)
                        {
                            DisposeH(ho_Holes);
                            HOperatorSet.CopyObj(ho_HolesTmp, out ho_Holes, 1, -1);
                            bestMinCirc = minC;
                        }
                    }
                    HTuple hv_HoleNum = null;
                    HOperatorSet.CountObj(ho_Holes, out hv_HoleNum);
                    holeNum = hv_HoleNum.I;
                    DisposeT(hv_HoleNum);
                    if (holeNum != 3)
                    {
                        err = 1;
                        errMsg = "HoleNum!=3";
                    }
                }

                //========== 4. 孔序: 孔1=最靠右, 其后逆时针 ==========
                if (err == 0)
                {
                    for (int i = 1; i <= 3; i++)
                    {
                        DisposeH(ho_Hole);
                        HOperatorSet.SelectObj(ho_Holes, out ho_Hole, i);
                        DisposeH(ho_HoleFilled);
                        HOperatorSet.FillUp(ho_Hole, out ho_HoleFilled);
                        HTuple hv_SR = null, hv_SC = null, hv_SRad = null;
                        HOperatorSet.SmallestCircle(ho_HoleFilled, out hv_SR, out hv_SC, out hv_SRad);
                        smallRows[i - 1] = hv_SR.D;
                        smallCols[i - 1] = hv_SC.D;
                        smallRads[i - 1] = hv_SRad.D;
                        DisposeT(hv_SR, hv_SC, hv_SRad);
                    }
                    sortIdx = SortIdxRightThenCcw(smallRows, smallCols, row0, col0);
                }

                //========== 5. 每孔外沿: Metrology 圆测量, 失败孔 fallback 形态学 ==========
                //扫描方向由内向外. positive=暗→亮=倒角外沿. 18.11 result= [Row,Column,Radius].
                //InitR=1.15*定位R 略大于外沿; Length1=0.26*定位R 盖到真正外沿约 0.93*定位R.
                if (err == 0)
                {
                    fitN = 0;
                    int[] metroIdx = new int[3];
                    if (useMetrology == 1)
                    {
                        DisposeT(hv_MetrologyHandle);
                        HOperatorSet.CreateMetrologyModel(out hv_MetrologyHandle);
                        metroCreated = true;
                        HOperatorSet.SetMetrologyModelImageSize(hv_MetrologyHandle, width, height);
                        hv_MetroNames = ((((new HTuple("measure_transition")).TupleConcat(
                            "measure_select")).TupleConcat("num_instances")).TupleConcat("min_score")).TupleConcat(
                            "num_measures");
                        hv_MetroVals = ((((new HTuple("positive")).TupleConcat("first")).TupleConcat(
                            1)).TupleConcat(MetroMinScore)).TupleConcat(MetroNumMeasures);
                        for (int k = 0; k < 3; k++)
                        {
                            int srcIdx = sortIdx[k];
                            double locR = smallRows[srcIdx];
                            double locC = smallCols[srcIdx];
                            double locRad = smallRads[srcIdx];
                            double initR = MetroInitRFrac * locRad;
                            double len1 = MetroLen1Frac * locRad;
                            if (len1 >= initR - 0.5)
                            {
                                len1 = MetroLen1AltFrac * initR;
                            }
                            HTuple hv_Idx = null;
                            HOperatorSet.AddMetrologyObjectCircleMeasure(hv_MetrologyHandle, locR,
                                locC, initR, len1, MetroLen2, MetroSigma, MetroThreshold, hv_MetroNames, hv_MetroVals, out hv_Idx);
                            metroIdx[k] = hv_Idx.I;
                            DisposeT(hv_Idx);
                        }
                        HOperatorSet.ApplyMetrologyModel(ho_MaxGray, hv_MetrologyHandle);
                    }
                    for (int k = 0; k < 3; k++)
                    {
                        int srcIdx = sortIdx[k];
                        double locR = smallRows[srcIdx];
                        double locC = smallCols[srcIdx];
                        double locRad = smallRads[srcIdx];
                        int okK = 0;
                        double bigRow = locR;
                        double bigCol = locC;
                        double bigRad = 0.0;
                        if (useMetrology == 1)
                        {
                            int idx = metroIdx[k];
                            HTuple hv_NInst = null;
                            HOperatorSet.GetMetrologyObjectNumInstances(hv_MetrologyHandle, idx, out hv_NInst);
                            int nInst = hv_NInst.I;
                            DisposeT(hv_NInst);
                            if (nInst >= 1)
                            {
                                HTuple hv_CircP = null;
                                HOperatorSet.GetMetrologyObjectResult(hv_MetrologyHandle, idx, 0,
                                    "result_type", "all_param", out hv_CircP);
                                double[] circP = ToD(hv_CircP);
                                if (circP.Length >= 3)
                                {
                                    double bigRowT = circP[0];
                                    double bigColT = circP[1];
                                    double bigRadT = circP[2];
                                    double distLoc = Math.Sqrt(((bigRowT - locR) * (bigRowT - locR)) + ((bigColT - locC) * (bigColT - locC)));
                                    if ((bigRadT >= MetroRadMinFrac * locRad) && (bigRadT <= MetroRadMaxFrac * locRad) &&
                                        (distLoc <= MetroDistLocFrac * locRad))
                                    {
                                        bigRow = bigRowT;
                                        bigCol = bigColT;
                                        bigRad = bigRadT;
                                        okK = 1;
                                    }
                                }
                                DisposeT(hv_CircP);
                            }
                        }
                        if (okK == 0)
                        {
                            DisposeH(ho_HoleNbhd);
                            HOperatorSet.GenCircle(out ho_HoleNbhd, locR, locC, MorphNbhdFrac * locRad);
                            DisposeH(ho_ImgHole);
                            HOperatorSet.ReduceDomain(ho_MaxGray, ho_HoleNbhd, out ho_ImgHole);
                            DisposeH(ho_AnnOut);
                            HOperatorSet.GenCircle(out ho_AnnOut, locR, locC, MorphAnnOutFrac * locRad);
                            DisposeH(ho_AnnIn);
                            HOperatorSet.GenCircle(out ho_AnnIn, locR, locC, MorphAnnInFrac * locRad);
                            DisposeH(ho_Annulus);
                            HOperatorSet.Difference(ho_AnnOut, ho_AnnIn, out ho_Annulus);
                            DisposeH(ho_ImgAnn);
                            HOperatorSet.ReduceDomain(ho_ImgHole, ho_Annulus, out ho_ImgAnn);
                            double innerMin = ((MorphInnerMinFrac * Pi) * locRad) * locRad;
                            double bestNi = -1.0;
                            for (int chTh = MorphThStart; chTh <= MorphThEnd; chTh += MorphThStep)
                            {
                                DisposeH(ho_Chamfer);
                                HOperatorSet.Threshold(ho_ImgAnn, out ho_Chamfer, 0, chTh);
                                HObject morphTmp = null;
                                HOperatorSet.OpeningCircle(ho_Chamfer, out morphTmp, 1.0);
                                ReplaceObj(ref ho_Chamfer, morphTmp);
                                morphTmp = null;
                                HOperatorSet.ClosingCircle(ho_Chamfer, out morphTmp, 1.5);
                                ReplaceObj(ref ho_Chamfer, morphTmp);
                                DisposeH(ho_ChamferU);
                                HOperatorSet.Union1(ho_Chamfer, out ho_ChamferU);
                                DisposeH(ho_ChamferFilled);
                                HOperatorSet.FillUp(ho_ChamferU, out ho_ChamferFilled);
                                DisposeH(ho_InnerDisk);
                                HOperatorSet.Difference(ho_ChamferFilled, ho_ChamferU, out ho_InnerDisk);
                                HTuple hv_Ni = null, hv_NiR = null, hv_NiC = null;
                                HOperatorSet.AreaCenter(ho_InnerDisk, out hv_Ni, out hv_NiR, out hv_NiC);
                                double ni = hv_Ni.D;
                                DisposeT(hv_Ni, hv_NiR, hv_NiC);
                                if (ni >= innerMin)
                                {
                                    DisposeH(ho_OuterBound);
                                    HOperatorSet.Boundary(ho_ChamferFilled, out ho_OuterBound, "outer");
                                    DisposeH(ho_InnerBound);
                                    HOperatorSet.Boundary(ho_InnerDisk, out ho_InnerBound, "outer");
                                    DisposeH(ho_OuterXLD);
                                    HOperatorSet.GenContourRegionXld(ho_OuterBound, out ho_OuterXLD, "border");
                                    DisposeH(ho_InnerXLD);
                                    HOperatorSet.GenContourRegionXld(ho_InnerBound, out ho_InnerXLD, "border");
                                    HTuple hv_NOut = null, hv_NInX = null;
                                    HOperatorSet.CountObj(ho_OuterXLD, out hv_NOut);
                                    HOperatorSet.CountObj(ho_InnerXLD, out hv_NInX);
                                    int nOut = hv_NOut.I;
                                    int nInX = hv_NInX.I;
                                    DisposeT(hv_NOut, hv_NInX);
                                    if (nOut >= 1 && nInX >= 1)
                                    {
                                        double bestLenO = -1.0;
                                        for (int q = 1; q <= nOut; q++)
                                        {
                                            DisposeH(ho_TmpX);
                                            HOperatorSet.SelectObj(ho_OuterXLD, out ho_TmpX, q);
                                            HTuple hv_LenQ = null;
                                            HOperatorSet.LengthXld(ho_TmpX, out hv_LenQ);
                                            if (hv_LenQ.D > bestLenO)
                                            {
                                                bestLenO = hv_LenQ.D;
                                                DisposeH(ho_OuterBest);
                                                HOperatorSet.CopyObj(ho_TmpX, out ho_OuterBest, 1, -1);
                                            }
                                            DisposeT(hv_LenQ);
                                        }
                                        double bestLenI = -1.0;
                                        for (int q = 1; q <= nInX; q++)
                                        {
                                            DisposeH(ho_TmpX);
                                            HOperatorSet.SelectObj(ho_InnerXLD, out ho_TmpX, q);
                                            HTuple hv_LenQ = null;
                                            HOperatorSet.LengthXld(ho_TmpX, out hv_LenQ);
                                            if (hv_LenQ.D > bestLenI)
                                            {
                                                bestLenI = hv_LenQ.D;
                                                DisposeH(ho_InnerBest);
                                                HOperatorSet.CopyObj(ho_TmpX, out ho_InnerBest, 1, -1);
                                            }
                                            DisposeT(hv_LenQ);
                                        }
                                        HTuple hv_BigRowT = null, hv_BigColT = null, hv_BigRadT = null;
                                        HTuple hv_PhiA = null, hv_PhiB = null, hv_OrdA = null;
                                        HOperatorSet.FitCircleContourXld(ho_OuterBest, "geohuber", -1, 0,
                                            0, 3, 2, out hv_BigRowT, out hv_BigColT, out hv_BigRadT, out hv_PhiA,
                                            out hv_PhiB, out hv_OrdA);
                                        HTuple hv_InRowT = null, hv_InColT = null, hv_InRadT = null;
                                        HTuple hv_PsiA = null, hv_PsiB = null, hv_OrdB = null;
                                        HOperatorSet.FitCircleContourXld(ho_InnerBest, "geohuber", -1, 0,
                                            0, 3, 2, out hv_InRowT, out hv_InColT, out hv_InRadT, out hv_PsiA,
                                            out hv_PsiB, out hv_OrdB);
                                        double[] bigRadArr = ToD(hv_BigRadT);
                                        double[] inRadArr = ToD(hv_InRadT);
                                        double[] bigRowArr = ToD(hv_BigRowT);
                                        double[] bigColArr = ToD(hv_BigColT);
                                        double[] inRowArr = ToD(hv_InRowT);
                                        double[] inColArr = ToD(hv_InColT);
                                        DisposeT(hv_BigRowT, hv_BigColT, hv_BigRadT, hv_PhiA, hv_PhiB, hv_OrdA);
                                        DisposeT(hv_InRowT, hv_InColT, hv_InRadT, hv_PsiA, hv_PsiB, hv_OrdB);
                                        if (bigRadArr.Length < 1 || inRadArr.Length < 1 || bigRowArr.Length < 1 ||
                                            bigColArr.Length < 1 || inRowArr.Length < 1 || inColArr.Length < 1)
                                        {
                                            continue;
                                        }
                                        double bigRad0 = bigRadArr[0];
                                        double inRad0 = inRadArr[0];
                                        double bigRow0 = bigRowArr[0];
                                        double bigCol0 = bigColArr[0];
                                        double inRow0 = inRowArr[0];
                                        double inCol0 = inColArr[0];
                                        if ((inRad0 >= 8) && (bigRad0 > inRad0 + 2))
                                        {
                                            double ratio = bigRad0 / inRad0;
                                            if ((ratio >= 1.08) && (ratio <= 1.32))
                                            {
                                                double distT = Math.Sqrt(((bigRow0 - inRow0) * (bigRow0 - inRow0)) + ((bigCol0 - inCol0) * (bigCol0 - inCol0)));
                                                double distMax = 0.25 * inRad0;
                                                if ((distT < distMax) && (ni > bestNi))
                                                {
                                                    bestNi = ni;
                                                    bigRow = bigRow0;
                                                    bigCol = bigCol0;
                                                    bigRad = bigRad0;
                                                    okK = 1;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        if (okK == 1)
                        {
                            fitN = fitN + 1;
                        }
                        bigRows[k] = bigRow;
                        bigCols[k] = bigCol;
                        bigRads[k] = bigRad;
                    }
                    if (useMetrology == 1)
                    {
                        HOperatorSet.ClearMetrologyModel(hv_MetrologyHandle);
                        metroCreated = false;
                    }
                    if (fitN != 3)
                    {
                        err = 1;
                        errMsg = "FitN!=3";
                    }
                }

                //========== 6. 夹角: 外沿圆心相对内圆心, atan2 必须带负号 ==========
                if (err == 0)
                {
                    double[] degWrap = new double[3];
                    for (int i = 0; i < 3; i++)
                    {
                        degWrap[i] = PolarDeg(bigRows[i], bigCols[i], row0, col0);
                    }
                    Array.Sort(degWrap);
                    double d0 = degWrap[0];
                    double d1 = degWrap[1];
                    double d2 = degWrap[2];
                    angle12 = d1 - d0;
                    angle23 = d2 - d1;
                    angle31 = (360.0 - d2) + d0;
                    double dev12 = Math.Abs(angle12 - TargetAngleDeg);
                    double dev23 = Math.Abs(angle23 - TargetAngleDeg);
                    double dev31 = Math.Abs(angle31 - TargetAngleDeg);
                    maxDev = Math.Max(dev12, Math.Max(dev23, dev31));
                }

                result.HoleNum = holeNum;
                result.FitN = fitN;
                result.Angle12 = angle12;
                result.Angle23 = angle23;
                result.Angle31 = angle31;
                if (err != 0 || result.HoleNum != 3 || result.FitN != 3)
                {
                    result.MaxDev = 1000f;
                    result.Warn = errMsg;
                    if (string.IsNullOrEmpty(result.Warn) || result.Warn == "OK")
                    {
                        result.Warn = "HoleNum=" + result.HoleNum + " FitN=" + result.FitN;
                    }
                }
                else
                {
                    result.MaxDev = (float)maxDev;
                    result.Warn = "";
                }
            }
            catch (Exception ex)
            {
                result.MaxDev = 1000f;
                result.Warn = ex.Message;
            }
            finally
            {
                if (metroCreated)
                {
                    try
                    {
                        HOperatorSet.ClearMetrologyModel(hv_MetrologyHandle);
                    }
                    catch
                    {
                    }
                }
                DisposeH(
                    ho_Image, ho_ImgR, ho_ImgG, ho_ImgB, ho_MaxRG, ho_MaxGray,
                    ho_InnerSearch, ho_ImgInner, ho_RegionDark, ho_ConnectedDark, ho_InnerCands,
                    ho_InnerRegion, ho_InnerFilled, ho_CircleOut, ho_CircleIn, ho_RingROI, ho_Holes,
                    ho_ImgRing, ho_RegionHoles, ho_HoleCandidates, ho_HolesSel, ho_HolesTmp,
                    ho_One, ho_OneF, ho_Hole, ho_HoleFilled,
                    ho_HoleNbhd, ho_ImgHole, ho_AnnOut, ho_AnnIn, ho_Annulus, ho_ImgAnn,
                    ho_Chamfer, ho_ChamferU, ho_ChamferFilled, ho_InnerDisk,
                    ho_OuterBound, ho_InnerBound, ho_OuterXLD, ho_InnerXLD,
                    ho_TmpX, ho_OuterBest, ho_InnerBest);
                DisposeT(hv_MetrologyHandle);
                DisposeT(hv_MetroNames, hv_MetroVals);
                DisposeT(featHole, minHole, maxHole);
            }

            return result;
        }
    }

    /// <summary>
    /// 【盘齿方案3.7-注释】孔钻错一次运行结果。失败 MaxDev=1000。
    /// </summary>
    public sealed class GearAngleRunResult
    {
        public float MaxDev;
        public int HoleNum;
        public int FitN;
        public string Warn;
        public double Angle12;
        public double Angle23;
        public double Angle31;
        public int Bits;
        public int Width;
        public int Height;
        public int Channels;
        public int NDark;
        public string ColorFmt;
    }
}
