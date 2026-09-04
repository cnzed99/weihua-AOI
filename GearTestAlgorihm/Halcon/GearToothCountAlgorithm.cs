using System;
using System.IO;
using HalconDotNet;
using WH.RunCell;

namespace GearTestAlgorihm.Halcon
{
    /// <summary>
    /// 【盘齿方案3.9-注释】齿轮数拉链式包装：Cell 入图、HObject 成对 Dispose、C# 交齿数。
    /// 算子顺序对照 test.cs 的 action() / 步骤 123；禁止 HDevelopExport 原样。
    /// </summary>
    public static class GearToothCountAlgorithm
    {
        public static GearToothCountResult Run(Cell cell, GearToothCountParams p)
        {
            if (p == null)
            {
                p = new GearToothCountParams();
            }

            HObject image = null;
            HObject gray = null;
            HTuple prevClip = null;
            HTuple prevEmpty = null;
            try
            {
                HOperatorSet.GetSystem("clip_region", out prevClip);
                HOperatorSet.GetSystem("store_empty_region", out prevEmpty);
                HOperatorSet.SetSystem("clip_region", "true");
                HOperatorSet.SetSystem("store_empty_region", "true");

                if (!TryLoad(cell, out image, out string warn))
                {
                    return GearToothCountResult.Fail(warn);
                }

                ToGray(image, out gray);

                if (!FindHub(gray, p, out double centerRow, out double centerCol, out double hubRadius, out warn))
                {
                    return GearToothCountResult.Fail(warn);
                }

                GearToothCountResult counted = CountFromHub(gray, p, centerRow, centerCol, hubRadius, false);
                if (counted.Count >= 0 || !IsRootFail(counted.Warn))
                {
                    return counted;
                }

                // 【盘齿方案3.9-注释】root fail：第一轮可能误收圆心（HDev Pic_17 要 BestScore<0 才进模糊轮）。丢掉第一轮，强制模糊再填孔。
                if (TryReplaceHubWithBlur(gray, p, ref centerRow, ref centerCol, ref hubRadius))
                {
                    counted = CountFromHub(gray, p, centerRow, centerCol, hubRadius, false);
                    if (counted.Count >= 0 || !IsRootFail(counted.Warn))
                    {
                        return counted;
                    }
                }

                if (TryReplaceHubWithFill(gray, p, ref centerRow, ref centerCol, ref hubRadius))
                {
                    counted = CountFromHub(gray, p, centerRow, centerCol, hubRadius, false);
                    if (counted.Count >= 0 || !IsRootFail(counted.Warn))
                    {
                        return counted;
                    }
                }

                return CountFromHub(gray, p, centerRow, centerCol, hubRadius, true);
            }
            catch (Exception ex)
            {
                return GearToothCountResult.Fail(ex.Message);
            }
            finally
            {
                image?.Dispose();
                gray?.Dispose();
                RestoreSystem("clip_region", prevClip);
                RestoreSystem("store_empty_region", prevEmpty);
            }
        }

        static bool IsRootFail(string warn)
        {
            return warn != null && warn.StartsWith("root fail", StringComparison.Ordinal);
        }

        static GearToothCountResult CountFromHub(
            HObject gray,
            GearToothCountParams p,
            double centerRow,
            double centerCol,
            double hubRadius,
            bool softRoot)
        {
            if (!BuildPolar(
                gray,
                p,
                centerRow,
                centerCol,
                out HObject polarImage,
                out double[] occupancy,
                out int polarWidth,
                out int polarHeight,
                out double polarRadiusStart,
                out double polarRadiusSpan,
                out double minSize,
                out string warn))
            {
                polarImage?.Dispose();
                return GearToothCountResult.Fail(warn);
            }

            try
            {
                int bestRootRow;
                bool foundRoot = FindRoot(
                    occupancy,
                    p,
                    hubRadius,
                    polarRadiusStart,
                    polarRadiusSpan,
                    polarHeight,
                    minSize,
                    false,
                    0.70,
                    out bestRootRow,
                    out warn)
                    || FindRoot(
                        occupancy,
                        p,
                        hubRadius,
                        polarRadiusStart,
                        polarRadiusSpan,
                        polarHeight,
                        minSize,
                        true,
                        0.70,
                        out bestRootRow,
                        out warn);
                if (!foundRoot && softRoot)
                {
                    foundRoot = FindRoot(
                        occupancy,
                        p,
                        hubRadius,
                        polarRadiusStart,
                        polarRadiusSpan,
                        polarHeight,
                        minSize,
                        true,
                        0.55,
                        out bestRootRow,
                        out warn);
                }

                if (!foundRoot)
                {
                    return GearToothCountResult.Fail(warn);
                }

                if (!FindAnalysisBand(occupancy, p, bestRootRow, polarHeight,
                    out int analysisRowStart, out int analysisRowEnd, out warn))
                {
                    return GearToothCountResult.Fail(warn);
                }

                if (!VoteToothCount(polarImage, p, analysisRowStart, analysisRowEnd, polarWidth,
                    out int toothCount, out warn))
                {
                    return GearToothCountResult.Fail(warn);
                }

                return new GearToothCountResult { Count = toothCount };
            }
            finally
            {
                polarImage?.Dispose();
            }
        }

        static void GetHubScale(
            HObject gray,
            GearToothCountParams p,
            out double minSize,
            out double imageCenterRow,
            out double imageCenterCol,
            out double twoPi,
            out double hubRadiusMin,
            out double hubRadiusMax,
            out double hubRadiusSpan)
        {
            HOperatorSet.GetImageSize(gray, out HTuple width, out HTuple height);
            minSize = Math.Min(width.D, height.D);
            imageCenterRow = 0.5 * (height.D - 1);
            imageCenterCol = 0.5 * (width.D - 1);
            twoPi = 2.0 * (new HTuple(180)).TupleRad().D;
            hubRadiusMin = p.HubRadiusMinFrac * minSize;
            hubRadiusMax = p.HubRadiusMaxFrac * minSize;
            hubRadiusSpan = hubRadiusMax - hubRadiusMin;
            if (hubRadiusSpan < 1.0)
            {
                hubRadiusSpan = 1.0;
            }
        }

        static bool TryReplaceHubWithBlur(
            HObject gray,
            GearToothCountParams p,
            ref double centerRow,
            ref double centerCol,
            ref double hubRadius)
        {
            GetHubScale(
                gray, p,
                out double minSize, out double imageCenterRow, out double imageCenterCol,
                out double twoPi, out double hubRadiusMin, out double hubRadiusMax, out double hubRadiusSpan);
            double bestScore = -1.0;
            double bestRow = centerRow;
            double bestCol = centerCol;
            double bestRadius = hubRadius;
            if (!TryHubBlur(
                gray, p, minSize, imageCenterRow, imageCenterCol, twoPi,
                hubRadiusMin, hubRadiusMax, hubRadiusSpan,
                ref bestScore, ref bestRow, ref bestCol, ref bestRadius)
                || bestScore < 0.0)
            {
                return false;
            }

            centerRow = bestRow;
            centerCol = bestCol;
            hubRadius = bestRadius;
            return true;
        }

        static bool TryReplaceHubWithFill(
            HObject gray,
            GearToothCountParams p,
            ref double centerRow,
            ref double centerCol,
            ref double hubRadius)
        {
            GetHubScale(
                gray, p,
                out double minSize, out double imageCenterRow, out double imageCenterCol,
                out double twoPi, out double hubRadiusMin, out double hubRadiusMax, out double hubRadiusSpan);
            double bestScore = -1.0;
            double bestRow = centerRow;
            double bestCol = centerCol;
            double bestRadius = hubRadius;
            TryHubFill(
                gray, p, minSize, imageCenterRow, imageCenterCol, p.FlangeGuardFrac * minSize,
                hubRadiusMin, hubRadiusMax,
                ref bestScore, ref bestRow, ref bestCol, ref bestRadius);
            if (bestScore < 0.0)
            {
                return false;
            }

            centerRow = bestRow;
            centerCol = bestCol;
            hubRadius = bestRadius;
            return true;
        }

        static void RestoreSystem(string key, HTuple prev)
        {
            if (prev == null)
            {
                return;
            }
            try
            {
                HOperatorSet.SetSystem(key, prev);
            }
            catch
            {
            }
        }

        static int ToInt(HTuple t)
        {
            if (t == null || t.TupleLength() == 0)
            {
                return 0;
            }
            return Convert.ToInt32(t.D);
        }

        static bool IsZero(HTuple t)
        {
            return ToInt(t) == 0;
        }

        static bool TryLoad(Cell cell, out HObject image, out string warn)
        {
            image = null;
            warn = null;
            bool hasFile = cell != null && !string.IsNullOrEmpty(cell.ImageFile) && File.Exists(cell.ImageFile);
            bool hasMem = cell?.Image != null && cell.Image.ImageData != IntPtr.Zero
                && cell.Image.ImageWidth > 0 && cell.Image.ImageHeight > 0;
            if (!hasFile && !hasMem)
            {
                warn = "无图";
                return false;
            }

            HObject raw = null;
            try
            {
                if (hasFile)
                {
                    HOperatorSet.ReadImage(out raw, cell.ImageFile);
                }
                else
                {
                    int w = cell.Image.ImageWidth;
                    int h = cell.Image.ImageHeight;
                    int bpp = cell.Image.PixelFormat.BitsPerPixel;
                    if (bpp <= 8)
                    {
                        HOperatorSet.GenImage1(out raw, "byte", w, h, cell.Image.ImageData);
                    }
                    else
                    {
                        HOperatorSet.GenImageInterleaved(
                            out raw,
                            cell.Image.ImageData,
                            "rgb",
                            w,
                            h,
                            -1,
                            "byte",
                            0,
                            0,
                            0,
                            0,
                            -1,
                            0);
                    }
                }

                HOperatorSet.CopyImage(raw, out image);
                return true;
            }
            finally
            {
                raw?.Dispose();
            }
        }

        static void ToGray(HObject image, out HObject gray)
        {
            HOperatorSet.CountChannels(image, out HTuple channels);
            if (ToInt(channels) == 3)
            {
                HOperatorSet.Rgb1ToGray(image, out gray);
            }
            else
            {
                HOperatorSet.CopyObj(image, out gray, 1, 1);
            }
        }

        static bool FindHub(
            HObject gray,
            GearToothCountParams p,
            out double centerRow,
            out double centerCol,
            out double hubRadius,
            out string warn)
        {
            centerRow = 0;
            centerCol = 0;
            hubRadius = 0;
            warn = "hub fail";

            HOperatorSet.GetImageSize(gray, out HTuple width, out HTuple height);
            double minSize = Math.Min(width.D, height.D);
            double imageCenterRow = 0.5 * (height.D - 1);
            double imageCenterCol = 0.5 * (width.D - 1);
            double twoPi = 2.0 * (new HTuple(180)).TupleRad().D;
            double hubRadiusMin = p.HubRadiusMinFrac * minSize;
            double hubRadiusMax = p.HubRadiusMaxFrac * minSize;
            double flangeGuardRadius = p.FlangeGuardFrac * minSize;
            double hubRadiusSpan = hubRadiusMax - hubRadiusMin;
            if (hubRadiusSpan < 1.0)
            {
                hubRadiusSpan = 1.0;
            }

            double bestScore = -1.0;
            double bestRow = imageCenterRow;
            double bestCol = imageCenterCol;
            double bestRadius = hubRadiusMin;

            HObject graySmooth = null;
            HObject flangeGuard = null;
            HObject searchImage = null;
            HObject rawEdges = null;
            HObject linkedEdges = null;
            HObject longEdges = null;
            HObject circleCands = null;
            HObject longEdgesLoose = null;
            try
            {
                HOperatorSet.GaussFilter(gray, out graySmooth, 5);
                HOperatorSet.GenCircle(out flangeGuard, imageCenterRow, imageCenterCol, flangeGuardRadius);
                HOperatorSet.ReduceDomain(graySmooth, flangeGuard, out searchImage);
                HOperatorSet.EdgesSubPix(searchImage, out rawEdges, "canny", 1.2, 10, 26);
                HOperatorSet.UnionAdjacentContoursXld(rawEdges, out linkedEdges, 12, 1, "attr_forget");
                HOperatorSet.SelectContoursXld(
                    linkedEdges, out longEdges, "contour_length", p.MinContourLen, 9999999, -0.5, 0.5);
                SelectCircleCands(longEdges, p.MinCircularity, hubRadiusMin, hubRadiusMax, out circleCands);
                HOperatorSet.CountObj(circleCands, out HTuple numberCandidates);
                if (IsZero(numberCandidates))
                {
                    longEdgesLoose?.Dispose();
                    HOperatorSet.SelectContoursXld(
                        linkedEdges, out longEdgesLoose, "contour_length",
                        p.MinContourLenLoose, 9999999, -0.5, 0.5);
                    circleCands?.Dispose();
                    SelectCircleCands(
                        longEdgesLoose, p.MinCircularityLoose, hubRadiusMin, hubRadiusMax, out circleCands);
                    HOperatorSet.CountObj(circleCands, out numberCandidates);
                }

                // 【盘齿方案3.9-注释】与导出 test.cs 一致：第一轮 0 候选不 abort，交给模糊/填孔（Pic_17）。
                // 步骤123 的 stop() 在导出里已被注释；HDevelop 对 Pic_17 走的是 BestScore<0 后续轮。
                int candCount = ToInt(numberCandidates);
                if (candCount > 0)
                {
                    ScoreHubCands(
                        circleCands,
                        candCount,
                        twoPi,
                        imageCenterRow,
                        imageCenterCol,
                        minSize,
                        hubRadiusMin,
                        hubRadiusMax,
                        hubRadiusSpan,
                        p.MinArcCoverage,
                        ref bestScore,
                        ref bestRow,
                        ref bestCol,
                        ref bestRadius);
                }
            }
            catch
            {
            }
            finally
            {
                graySmooth?.Dispose();
                flangeGuard?.Dispose();
                searchImage?.Dispose();
                rawEdges?.Dispose();
                linkedEdges?.Dispose();
                longEdges?.Dispose();
                circleCands?.Dispose();
                longEdgesLoose?.Dispose();
            }

            if (bestScore < 0.0)
            {
                if (!TryHubBlur(
                    gray, p, minSize, imageCenterRow, imageCenterCol, twoPi,
                    hubRadiusMin, hubRadiusMax, hubRadiusSpan,
                    ref bestScore, ref bestRow, ref bestCol, ref bestRadius))
                {
                    TryHubFill(
                        gray, p, minSize, imageCenterRow, imageCenterCol, flangeGuardRadius,
                        hubRadiusMin, hubRadiusMax,
                        ref bestScore, ref bestRow, ref bestCol, ref bestRadius);
                }
            }

            if (bestScore < 0.0)
            {
                warn = "hub fail";
                return false;
            }

            centerRow = bestRow;
            centerCol = bestCol;
            hubRadius = bestRadius;
            warn = null;
            return true;
        }

        static void SelectCircleCands(
            HObject contours,
            double minCircularity,
            double hubRadiusMin,
            double hubRadiusMax,
            out HObject circleCands)
        {
            HOperatorSet.SelectShapeXld(
                contours,
                out circleCands,
                new HTuple("circularity").TupleConcat("outer_radius"),
                "and",
                new HTuple(minCircularity).TupleConcat(hubRadiusMin),
                new HTuple(1.0).TupleConcat(hubRadiusMax));
        }

        static void ScoreHubCands(
            HObject circleCands,
            int numberCandidates,
            double twoPi,
            double imageCenterRow,
            double imageCenterCol,
            double minSize,
            double hubRadiusMin,
            double hubRadiusMax,
            double hubRadiusSpan,
            double minArcCoverage,
            ref double bestScore,
            ref double bestRow,
            ref double bestCol,
            ref double bestRadius)
        {
            for (int candIndex = 1; candIndex <= numberCandidates; candIndex++)
            {
                HObject oneCand = null;
                try
                {
                    HOperatorSet.SelectObj(circleCands, out oneCand, candIndex);
                    HOperatorSet.FitCircleContourXld(
                        oneCand, "geometric", -1, 0, 0, 3, 2,
                        out HTuple fitRow, out HTuple fitColumn, out HTuple fitRadius,
                        out HTuple startPhi, out HTuple endPhi, out HTuple pointOrder);
                    HOperatorSet.LengthXld(oneCand, out HTuple candLength);
                    double circumference = twoPi * fitRadius.D;
                    if (circumference < 1.0)
                    {
                        circumference = 1.0;
                    }

                    double coverage = candLength.D / circumference;
                    double shiftRow = fitRow.D - imageCenterRow;
                    double shiftCol = fitColumn.D - imageCenterCol;
                    double centerShift = Math.Sqrt((shiftRow * shiftRow) + (shiftCol * shiftCol));
                    double radiusNorm = (fitRadius.D - hubRadiusMin) / hubRadiusSpan;
                    double score = coverage + (0.15 * radiusNorm) - (centerShift / minSize);
                    if (coverage >= minArcCoverage
                        && fitRadius.D >= hubRadiusMin
                        && fitRadius.D <= hubRadiusMax
                        && score > bestScore)
                    {
                        bestScore = score;
                        bestRow = fitRow.D;
                        bestCol = fitColumn.D;
                        bestRadius = fitRadius.D;
                    }
                }
                catch
                {
                }
                finally
                {
                    oneCand?.Dispose();
                }
            }
        }

        static bool TryHubBlur(
            HObject gray,
            GearToothCountParams p,
            double minSize,
            double imageCenterRow,
            double imageCenterCol,
            double twoPi,
            double hubRadiusMin,
            double hubRadiusMax,
            double hubRadiusSpan,
            ref double bestScore,
            ref double bestRow,
            ref double bestCol,
            ref double bestRadius)
        {
            HObject graySmoothBlur = null;
            HObject flangeGuardBlur = null;
            HObject searchImageBlur = null;
            HObject rawEdgesBlur = null;
            HObject linkedEdgesBlur = null;
            HObject longEdgesBlur = null;
            HObject circleCands = null;
            HObject longEdgesBlurLoose = null;
            try
            {
                HOperatorSet.GaussFilter(gray, out graySmoothBlur, 11);
                double flangeGuardRadiusBlur = p.FlangeGuardBlurFrac * minSize;
                HOperatorSet.GenCircle(out flangeGuardBlur, imageCenterRow, imageCenterCol, flangeGuardRadiusBlur);
                HOperatorSet.ReduceDomain(graySmoothBlur, flangeGuardBlur, out searchImageBlur);
                HOperatorSet.EdgesSubPix(searchImageBlur, out rawEdgesBlur, "canny", 1.5, 15, 30);
                HOperatorSet.UnionAdjacentContoursXld(rawEdgesBlur, out linkedEdgesBlur, 12, 1, "attr_forget");
                HOperatorSet.SelectContoursXld(
                    linkedEdgesBlur, out longEdgesBlur, "contour_length",
                    p.MinContourLen, 9999999, -0.5, 0.5);
                SelectCircleCands(longEdgesBlur, p.MinCircularity, hubRadiusMin, hubRadiusMax, out circleCands);
                HOperatorSet.CountObj(circleCands, out HTuple numberCandidates);
                if (IsZero(numberCandidates))
                {
                    HOperatorSet.SelectContoursXld(
                        linkedEdgesBlur, out longEdgesBlurLoose, "contour_length",
                        p.MinContourLenLoose, 9999999, -0.5, 0.5);
                    circleCands?.Dispose();
                    SelectCircleCands(
                        longEdgesBlurLoose, p.MinCircularityLoose, hubRadiusMin, hubRadiusMax, out circleCands);
                    HOperatorSet.CountObj(circleCands, out numberCandidates);
                }

                int candCount = ToInt(numberCandidates);
                if (candCount == 0)
                {
                    return false;
                }

                ScoreHubCands(
                    circleCands,
                    candCount,
                    twoPi,
                    imageCenterRow,
                    imageCenterCol,
                    minSize,
                    hubRadiusMin,
                    hubRadiusMax,
                    hubRadiusSpan,
                    p.MinArcCoverage,
                    ref bestScore,
                    ref bestRow,
                    ref bestCol,
                    ref bestRadius);
                return bestScore >= 0.0;
            }
            catch
            {
                return false;
            }
            finally
            {
                graySmoothBlur?.Dispose();
                flangeGuardBlur?.Dispose();
                searchImageBlur?.Dispose();
                rawEdgesBlur?.Dispose();
                linkedEdgesBlur?.Dispose();
                longEdgesBlur?.Dispose();
                circleCands?.Dispose();
                longEdgesBlurLoose?.Dispose();
            }
        }

        static void TryHubFill(
            HObject gray,
            GearToothCountParams p,
            double minSize,
            double imageCenterRow,
            double imageCenterCol,
            double flangeGuardRadius,
            double hubRadiusMin,
            double hubRadiusMax,
            ref double bestScore,
            ref double bestRow,
            ref double bestCol,
            ref double bestRadius)
        {
            HObject graySmoothBlur = null;
            HObject hubFillRoi = null;
            HObject hubFillImage = null;
            HObject hubBrightRaw = null;
            HObject hubFilled = null;
            HObject hubOpened = null;
            HObject hubParts = null;
            HObject hubRound = null;
            HObject hubBest = null;
            try
            {
                HOperatorSet.GaussFilter(gray, out graySmoothBlur, 11);
                HOperatorSet.GenCircle(out hubFillRoi, imageCenterRow, imageCenterCol, flangeGuardRadius);
                HOperatorSet.ReduceDomain(graySmoothBlur, hubFillRoi, out hubFillImage);
                HOperatorSet.BinaryThreshold(
                    hubFillImage, out hubBrightRaw, "max_separability", "light", out HTuple hubBrightTh);
                HOperatorSet.FillUp(hubBrightRaw, out hubFilled);
                HOperatorSet.OpeningCircle(hubFilled, out hubOpened, 8.0);
                HOperatorSet.Connection(hubOpened, out hubParts);
                HOperatorSet.SelectShape(
                    hubParts,
                    out hubRound,
                    new HTuple("circularity").TupleConcat("area"),
                    "and",
                    new HTuple(0.65).TupleConcat(80000),
                    new HTuple(1.0).TupleConcat(2500000));
                HOperatorSet.CountObj(hubRound, out HTuple numberHubParts);
                if (ToInt(numberHubParts) <= 0)
                {
                    return;
                }

                HOperatorSet.SelectShapeStd(hubRound, out hubBest, "max_area", 1);
                HOperatorSet.SmallestCircle(hubBest, out HTuple fitRow, out HTuple fitColumn, out HTuple fitRadius);
                double shiftRow = fitRow.D - imageCenterRow;
                double shiftCol = fitColumn.D - imageCenterCol;
                double centerShift = Math.Sqrt((shiftRow * shiftRow) + (shiftCol * shiftCol));
                double maxFillShift = p.MaxFillShiftFrac * minSize;
                if (fitRadius.D >= hubRadiusMin && fitRadius.D <= hubRadiusMax && centerShift <= maxFillShift)
                {
                    bestScore = 0.50;
                    bestRow = fitRow.D;
                    bestCol = fitColumn.D;
                    bestRadius = fitRadius.D;
                }
            }
            catch
            {
            }
            finally
            {
                graySmoothBlur?.Dispose();
                hubFillRoi?.Dispose();
                hubFillImage?.Dispose();
                hubBrightRaw?.Dispose();
                hubFilled?.Dispose();
                hubOpened?.Dispose();
                hubParts?.Dispose();
                hubRound?.Dispose();
                hubBest?.Dispose();
            }
        }

        static bool BuildPolar(
            HObject gray,
            GearToothCountParams p,
            double centerRow,
            double centerCol,
            out HObject polarImage,
            out double[] occupancy,
            out int polarWidth,
            out int polarHeight,
            out double polarRadiusStart,
            out double polarRadiusSpan,
            out double minSize,
            out string warn)
        {
            polarImage = null;
            occupancy = null;
            polarWidth = p.PolarWidth;
            polarHeight = 0;
            polarRadiusStart = 0;
            polarRadiusSpan = 0;
            minSize = 0;
            warn = "root fail";

            HOperatorSet.GetImageSize(gray, out HTuple width, out HTuple height);
            minSize = Math.Min(width.D, height.D);
            polarRadiusStart = p.PolarRadiusStartFrac * minSize;
            double polarRadiusEnd = p.PolarRadiusEndFrac * minSize;
            polarRadiusSpan = polarRadiusEnd - polarRadiusStart;
            polarHeight = ToInt((new HTuple(polarRadiusSpan)).TupleRound()) + 1;
            int polarHeightMinus1 = polarHeight - 1;
            int polarWidthMinus1 = polarWidth - 1;
            double polarAngleStart = 0.0;
            HTuple polarAngleEnd = (((new HTuple(360)).TupleRad()) * polarWidthMinus1) / polarWidth;

            HObject polarBrightRaw = null;
            HObject polarBrightClosed = null;
            HObject polarBright = null;
            HObject onePolarRow = null;
            HObject brightPixelsInRow = null;
            try
            {
                HOperatorSet.PolarTransImageExt(
                    gray,
                    out polarImage,
                    centerRow,
                    centerCol,
                    polarAngleStart,
                    polarAngleEnd,
                    polarRadiusStart,
                    polarRadiusEnd,
                    polarWidth,
                    polarHeight,
                    "bilinear");
                HOperatorSet.GetImageSize(polarImage, out HTuple polarW, out HTuple polarH);
                polarWidth = ToInt(polarW);
                polarHeight = ToInt(polarH);
                if (polarWidth < 8 || polarHeight < 16)
                {
                    warn = "root fail polar size";
                    return false;
                }

                polarHeightMinus1 = polarHeight - 1;
                polarWidthMinus1 = polarWidth - 1;
                HOperatorSet.BinaryThreshold(
                    polarImage, out polarBrightRaw, "max_separability", "light", out HTuple polarBrightThreshold);
                HOperatorSet.ClosingRectangle1(polarBrightRaw, out polarBrightClosed, 9, 3);
                HOperatorSet.OpeningCircle(polarBrightClosed, out polarBright, 1.5);

                occupancy = new double[polarHeight];
                for (int polarRow = 0; polarRow <= polarHeightMinus1; polarRow++)
                {
                    onePolarRow?.Dispose();
                    HOperatorSet.GenRectangle1(out onePolarRow, polarRow, 0, polarRow, polarWidthMinus1);
                    brightPixelsInRow?.Dispose();
                    HOperatorSet.Intersection(polarBright, onePolarRow, out brightPixelsInRow);
                    occupancy[polarRow] = RegionArea(brightPixelsInRow) / polarWidth;
                }

                warn = null;
                return true;
            }
            catch (Exception ex)
            {
                polarImage?.Dispose();
                polarImage = null;
                occupancy = null;
                warn = ex.Message;
                return false;
            }
            finally
            {
                polarBrightRaw?.Dispose();
                polarBrightClosed?.Dispose();
                polarBright?.Dispose();
                onePolarRow?.Dispose();
                brightPixelsInRow?.Dispose();
            }
        }

        static double RegionArea(HObject region)
        {
            try
            {
                HOperatorSet.AreaCenter(region, out HTuple area, out HTuple row, out HTuple column);
                if (area == null || area.TupleLength() == 0)
                {
                    return 0;
                }

                HOperatorSet.TupleSum(area, out HTuple areaSum);
                HOperatorSet.TupleReal(areaSum, out HTuple areaReal);
                return areaReal.D;
            }
            catch
            {
                return 0;
            }
        }

        static bool FindRoot(
            double[] occupancy,
            GearToothCountParams p,
            double hubRadius,
            double polarRadiusStart,
            double polarRadiusSpan,
            int polarHeight,
            double minSize,
            bool wideSearch,
            double minBeforeOccupancy,
            out int bestRootRow,
            out string warn)
        {
            bestRootRow = 0;
            warn = "root fail";
            if (occupancy == null || occupancy.Length == 0 || polarRadiusSpan < 1.0)
            {
                return false;
            }

            int searchRowStart;
            int searchRowEnd;
            if (wideSearch)
            {
                // 窄窗跟 hub 半径绑死；圆心偏时齿根落在窗外。内圈 55% 仍避开最外法兰。
                searchRowStart = 3;
                searchRowEnd = Math.Min(polarHeight - 4, Math.Max(3, (int)(polarHeight * 0.55)));
            }
            else
            {
                double rootRadiusMin = hubRadius + p.RootInnerPad;
                double rootRadiusMax = hubRadius + (p.RootOuterPadFrac * minSize);
                if (rootRadiusMin < polarRadiusStart + 8)
                {
                    rootRadiusMin = polarRadiusStart + 8;
                }
                if (rootRadiusMax > polarRadiusStart + polarRadiusSpan - 8)
                {
                    rootRadiusMax = polarRadiusStart + polarRadiusSpan - 8;
                }

                int polarHeightMinus1 = polarHeight - 1;
                double rootOffsetMin = rootRadiusMin - polarRadiusStart;
                double rootOffsetMax = rootRadiusMax - polarRadiusStart;
                searchRowStart = ToInt((new HTuple((rootOffsetMin * polarHeightMinus1) / polarRadiusSpan)).TupleRound());
                searchRowEnd = ToInt((new HTuple((rootOffsetMax * polarHeightMinus1) / polarRadiusSpan)).TupleRound());
                searchRowStart = Math.Max(3, searchRowStart);
                searchRowEnd = Math.Min(polarHeight - 4, searchRowEnd);
            }

            int rootFound = 0;
            double bestDrop = -999.0;
            double maxBefore = 0.0;
            double bestDropAny = -999.0;
            bestRootRow = searchRowStart;
            if (searchRowStart <= searchRowEnd)
            {
                for (int candidateRow = searchRowStart; candidateRow <= searchRowEnd; candidateRow++)
                {
                    if (candidateRow + 3 >= occupancy.Length || candidateRow - 3 < 0)
                    {
                        continue;
                    }

                    double beforeOccupancy = (occupancy[candidateRow - 3] + occupancy[candidateRow - 2] + occupancy[candidateRow - 1]) / 3.0;
                    double afterOccupancy = (occupancy[candidateRow + 1] + occupancy[candidateRow + 2] + occupancy[candidateRow + 3]) / 3.0;
                    double occupancyDrop = beforeOccupancy - afterOccupancy;
                    if (beforeOccupancy > maxBefore)
                    {
                        maxBefore = beforeOccupancy;
                    }

                    if (occupancyDrop > bestDropAny)
                    {
                        bestDropAny = occupancyDrop;
                    }

                    if (beforeOccupancy > minBeforeOccupancy && afterOccupancy < 0.97 && occupancyDrop > bestDrop)
                    {
                        rootFound = 1;
                        bestDrop = occupancyDrop;
                        bestRootRow = candidateRow;
                    }
                }
            }

            string winTag = wideSearch ? (minBeforeOccupancy < 0.70 ? " soft" : " wide") : " narrow";
            if (rootFound == 0 || bestDrop < p.MinimumOccupancyDrop)
            {
                warn = "root fail drop=" + bestDrop.ToString("0.###")
                    + " anyD=" + bestDropAny.ToString("0.###")
                    + " maxB=" + maxBefore.ToString("0.###")
                    + " win=" + searchRowStart + "-" + searchRowEnd
                    + " R=" + hubRadius.ToString("0.#")
                    + winTag;
                return false;
            }

            warn = null;
            return true;
        }

        static bool FindAnalysisBand(
            double[] occupancy,
            GearToothCountParams p,
            int bestRootRow,
            int polarHeight,
            out int analysisRowStart,
            out int analysisRowEnd,
            out string warn)
        {
            analysisRowStart = bestRootRow + 12;
            analysisRowEnd = polarHeight - 14;
            warn = "DFT fail";
            if (occupancy == null)
            {
                return false;
            }

            int scanStart = bestRootRow + 4;
            int scanEnd = polarHeight - 40;
            for (int scanRow = scanStart; scanRow <= scanEnd && scanRow < occupancy.Length; scanRow++)
            {
                if (occupancy[scanRow] > p.DarkGrooveOcc)
                {
                    analysisRowStart = scanRow;
                    break;
                }
            }

            int flangeScanStop = bestRootRow + 40;
            for (int scanRow = polarHeight - 2; scanRow >= flangeScanStop && scanRow >= 0; scanRow--)
            {
                if (occupancy[scanRow] > p.DarkGrooveOcc)
                {
                    analysisRowEnd = scanRow;
                    break;
                }
            }

            for (int scanRow = analysisRowEnd; scanRow >= flangeScanStop && scanRow >= 0; scanRow--)
            {
                if (occupancy[scanRow] < p.FlangeOccupancy)
                {
                    analysisRowEnd = scanRow - 8;
                    break;
                }
            }

            if (analysisRowEnd > polarHeight - 14)
            {
                analysisRowEnd = polarHeight - 14;
            }

            int minAnalysisSpan = 24;
            if (analysisRowEnd - analysisRowStart < minAnalysisSpan)
            {
                analysisRowEnd = analysisRowStart + minAnalysisSpan;
            }

            warn = null;
            return true;
        }

        static bool VoteToothCount(
            HObject polarImage,
            GearToothCountParams p,
            int analysisRowStart,
            int analysisRowEnd,
            int polarWidth,
            out int toothCount,
            out string warn)
        {
            toothCount = -1;
            warn = "DFT fail";
            int candidateNumber = p.MaximumToothCount - p.MinimumToothCount + 1;
            if (candidateNumber < 2 || p.NumberRadialBands < 1)
            {
                return false;
            }

            int polarWidthMinus1 = polarWidth - 1;
            int signalLength = polarWidth;
            double twoPi = 2.0 * (new HTuple(180)).TupleRad().D;

            HObject polarAngularSmooth = null;
            HObject bandRoi = null;
            try
            {
                HOperatorSet.MeanImage(polarImage, out polarAngularSmooth, 9, 1);
                HOperatorSet.TupleGenSequence(p.MinimumToothCount, p.MaximumToothCount, 1, out HTuple candidateCounts);
                HOperatorSet.TupleGenConst(candidateNumber, 0.0, out HTuple frequencyScores);
                HOperatorSet.TupleGenConst(candidateNumber, 0, out HTuple frequencyVotes);
                HTuple cosBank = null;
                HTuple sinBank = null;
                HOperatorSet.TupleGenSequence(0, signalLength - 1, 1, out HTuple sampleIndices);

                for (int frequencyIndex = 0; frequencyIndex < candidateNumber; frequencyIndex++)
                {
                    int candidateCount = p.MinimumToothCount + frequencyIndex;
                    HTuple freqScale = (twoPi * candidateCount) / signalLength;
                    HTuple phaseSamples = freqScale * sampleIndices;
                    HOperatorSet.TupleCos(phaseSamples, out HTuple cosSamples);
                    HOperatorSet.TupleSin(phaseSamples, out HTuple sinSamples);
                    if (cosBank == null)
                    {
                        cosBank = cosSamples;
                        sinBank = sinSamples;
                    }
                    else
                    {
                        cosBank = cosBank.TupleConcat(cosSamples);
                        sinBank = sinBank.TupleConcat(sinSamples);
                    }
                }

                int validBandCount = 0;
                int bandDenom = p.NumberRadialBands - 1;
                if (bandDenom < 1)
                {
                    bandDenom = 1;
                }

                int analysisWidth = analysisRowEnd - analysisRowStart;
                for (int bandIndex = 0; bandIndex < p.NumberRadialBands; bandIndex++)
                {
                    try
                    {
                        int bandCenterRow = ToInt((new HTuple(
                            analysisRowStart + ((bandIndex * analysisWidth) / (double)bandDenom))).TupleRound());
                        int bandRow1 = Math.Max(analysisRowStart, bandCenterRow - p.BandHalfHeight);
                        int bandRow2 = Math.Min(analysisRowEnd, bandCenterRow + p.BandHalfHeight);
                        if (bandRow2 < bandRow1)
                        {
                            continue;
                        }
                        bandRoi?.Dispose();
                        HOperatorSet.GenRectangle1(out bandRoi, bandRow1, 0, bandRow2, polarWidthMinus1);
                        HOperatorSet.GrayProjections(
                            bandRoi, polarAngularSmooth, "simple",
                            out HTuple radialProjection, out HTuple angularSignal);
                        if (angularSignal == null || angularSignal.TupleLength() != signalLength)
                        {
                            continue;
                        }
                        HOperatorSet.TupleMean(angularSignal, out HTuple signalMean);
                        HTuple signalCentered = angularSignal - signalMean;
                        HOperatorSet.TupleGenConst(candidateNumber, 0.0, out HTuple bandEnergies);

                        for (int frequencyIndex = 0; frequencyIndex < candidateNumber; frequencyIndex++)
                        {
                            int rangeStart = frequencyIndex * signalLength;
                            int rangeEnd = rangeStart + signalLength - 1;
                            HOperatorSet.TupleSelectRange(cosBank, rangeStart, rangeEnd, out HTuple cosSamples);
                            HOperatorSet.TupleSelectRange(sinBank, rangeStart, rangeEnd, out HTuple sinSamples);
                            HOperatorSet.TupleSum(signalCentered * cosSamples, out HTuple cosComponent);
                            HOperatorSet.TupleSum(signalCentered * sinSamples, out HTuple sinComponent);
                            HTuple bandEnergy = ((cosComponent * cosComponent) + (sinComponent * sinComponent)).TupleSqrt();
                            bandEnergies[frequencyIndex] = bandEnergy;
                        }

                        HOperatorSet.TupleSortIndex(bandEnergies, out HTuple bandEnergyOrder);
                        HTuple bestBandIndex = bandEnergyOrder.TupleSelect(candidateNumber - 1);
                        HTuple secondBandIndex = bandEnergyOrder.TupleSelect(candidateNumber - 2);
                        HTuple bestBandEnergy = bandEnergies.TupleSelect(bestBandIndex);
                        HTuple normEnergies = bandEnergies / ((bestBandEnergy.TupleConcat(1.0)).TupleMax());
                        frequencyScores = frequencyScores + normEnergies;
                        frequencyVotes[bestBandIndex] = frequencyVotes.TupleSelect(bestBandIndex) + 1;
                        validBandCount++;
                    }
                    catch
                    {
                    }
                }

                if (validBandCount == 0)
                {
                    return false;
                }

                HOperatorSet.TupleSortIndex(frequencyScores, out HTuple frequencyScoreOrder);
                HTuple bestFrequencyIndex = frequencyScoreOrder.TupleSelect(candidateNumber - 1);
                HTuple secondFrequencyIndex = frequencyScoreOrder.TupleSelect(candidateNumber - 2);
                HTuple detected = candidateCounts.TupleSelect(bestFrequencyIndex);
                HTuple bestFrequencyScore = frequencyScores.TupleSelect(bestFrequencyIndex);
                HTuple secondFrequencyScore = frequencyScores.TupleSelect(secondFrequencyIndex);
                HTuple frequencyScoreRatio = bestFrequencyScore / ((secondFrequencyScore.TupleConcat(0.000001)).TupleMax());
                HTuple detectedCountVotes = frequencyVotes.TupleSelect(bestFrequencyIndex);

                toothCount = ToInt(detected);
                if (toothCount < p.MinimumToothCount
                    || frequencyScoreRatio.D < p.MinimumScoreRatio
                    || ToInt(detectedCountVotes) < p.MinimumVoteCount)
                {
                    toothCount = -1;
                    return false;
                }

                warn = null;
                return true;
            }
            catch (Exception ex)
            {
                warn = ex.Message;
                toothCount = -1;
                return false;
            }
            finally
            {
                polarAngularSmooth?.Dispose();
                bandRoi?.Dispose();
            }
        }
    }
}
