using System;
using System.IO;
using HalconDotNet;
using WH.RunCell;

namespace GearTestAlgorihm.Halcon
{
    /// <summary>
    /// 倒角偏
    /// </summary>
    public static class ChamferOffsetAlgorithm
    {
        struct Hole
        {
            public double Row;
            public double Col;
            public double Rad;
        }

        public static ChamferOffsetResult Run(Cell cell, ChamferOffsetParams p)
        {
            if (p == null)
            {
                p = new ChamferOffsetParams();
            }
            ChamferOffsetResult result = new ChamferOffsetResult();
            HObject image = null;
            HObject gray = null;
            try
            {
                if (!TryLoad(cell, out image, out string warn))
                {
                    result.Warn = warn;
                    return result;
                }
                ToGray(image, out gray);
                if (!FindInner(gray, p, out double r0, out double row0, out double col0, out warn))
                {
                    result.Warn = warn;
                    return result;
                }
                result.Radius0 = r0;
                if (!FindHoles(gray, p, r0, row0, col0, out Hole[] holes, out warn))
                {
                    result.Warn = warn;
                    result.HoleNum = holes == null ? 0 : holes.Length;
                    return result;
                }
                result.HoleNum = 3;
                SortRightmost(holes, row0, col0);
                result.Dist1 = ExportDist(FitHole(gray, p, holes[0]), p, cell);
                result.Dist2 = ExportDist(FitHole(gray, p, holes[1]), p, cell);
                result.Dist3 = ExportDist(FitHole(gray, p, holes[2]), p, cell);
                if (result.Dist1 < 0 && result.Dist2 < 0 && result.Dist3 < 0)
                {
                    result.Warn = "fit fail";
                }
            }
            catch (Exception ex)
            {
                result.Warn = ex.Message;
            }
            finally
            {
                image?.Dispose();
                gray?.Dispose();
            }
            return result;
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
                // 有文件走 ReadImage（离线 BMP）；仅相机 ImageFile 空时才 GenImage 指针。
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
            if (channels.I == 3)
            {
                HObject r = null, g = null, b = null, maxRg = null;
                try
                {
                    HOperatorSet.Decompose3(image, out r, out g, out b);
                    HOperatorSet.MaxImage(r, g, out maxRg);
                    HOperatorSet.MaxImage(maxRg, b, out gray);
                }
                finally
                {
                    r?.Dispose();
                    g?.Dispose();
                    b?.Dispose();
                    maxRg?.Dispose();
                }
            }
            else
            {
                HOperatorSet.CopyImage(image, out gray);
            }
        }

        static bool FindInner(
            HObject gray,
            ChamferOffsetParams p,
            out double r0,
            out double row0,
            out double col0,
            out string warn)
        {
            r0 = 0;
            row0 = 0;
            col0 = 0;
            warn = "inner fail";
            HObject search = null, reduced = null, dark = null, conn = null, cands = null, one = null, filled = null;
            try
            {
                HOperatorSet.GetImageSize(gray, out HTuple width, out HTuple height);
                double w = width.D;
                double h = height.D;
                double areaImg = w * h;
                double minSide = Math.Min(w, h);
                HOperatorSet.GenCircle(out search, h / 2.0, w / 2.0, minSide * 0.72);
                HOperatorSet.ReduceDomain(gray, search, out reduced);
                HOperatorSet.Threshold(reduced, out dark, 0, p.InnerDarkMax);
                HOperatorSet.Connection(dark, out conn);
                HOperatorSet.SelectShape(
                    conn,
                    out cands,
                    ((new HTuple("area")).TupleConcat("outer_radius")).TupleConcat("circularity"),
                    "and",
                    (new HTuple(areaImg * p.InnerAreaMinFrac)).TupleConcat(0.0).TupleConcat(0.4),
                    (new HTuple(areaImg * p.InnerAreaMaxFrac)).TupleConcat(minSide * p.InnerRMaxFrac).TupleConcat(1.0));
                HOperatorSet.CountObj(cands, out HTuple n);
                if (n.I < 1)
                {
                    return false;
                }
                HOperatorSet.AreaCenter(cands, out HTuple areas, out HTuple rows, out HTuple cols);
                int best = 0;
                double bestD = double.MaxValue;
                for (int i = 0; i < n.I; i++)
                {
                    double dr = rows[i].D - h / 2.0;
                    double dc = cols[i].D - w / 2.0;
                    double d = dr * dr + dc * dc;
                    if (d < bestD)
                    {
                        bestD = d;
                        best = i;
                    }
                }
                HOperatorSet.SelectObj(cands, out one, best + 1);
                HOperatorSet.FillUp(one, out filled);
                HOperatorSet.SmallestCircle(filled, out HTuple sr, out HTuple sc, out HTuple rad);
                r0 = rad.D;
                row0 = sr.D;
                col0 = sc.D;
                warn = null;
                return r0 > 1;
            }
            finally
            {
                search?.Dispose();
                reduced?.Dispose();
                dark?.Dispose();
                conn?.Dispose();
                cands?.Dispose();
                one?.Dispose();
                filled?.Dispose();
            }
        }

        static bool FindHoles(
            HObject gray,
            ChamferOffsetParams p,
            double r0,
            double row0,
            double col0,
            out Hole[] holes,
            out string warn)
        {
            holes = null;
            warn = "HoleNum!=3";
            HObject circleOut = null, circleIn = null, ring = null, imgRing = null;
            HObject region = null, conn = null, sel = null, one = null, filled = null;
            try
            {
                HOperatorSet.GenCircle(out circleOut, row0, col0, p.RingOuterMul * r0);
                HOperatorSet.GenCircle(out circleIn, row0, col0, p.RingInnerMul * r0);
                HOperatorSet.Difference(circleOut, circleIn, out ring);
                HOperatorSet.ReduceDomain(gray, ring, out imgRing);
                double openR = Math.Max(1.5, 0.008 * r0);
                double pi = Math.PI;
                double areaMin = 0.025 * pi * r0 * r0;
                double areaMax = 0.10 * pi * r0 * r0;
                double rMin = p.HoleRMinMul * r0;
                double rMax = p.HoleRMaxMul * r0;
                double distMin = p.HoleDistMinMul * r0;
                double distMax = p.HoleDistMaxMul * r0;
                double bestCirc = -1;
                Hole[] best = null;
                for (int th = 100; th <= 130; th += 10)
                {
                    region?.Dispose();
                    HOperatorSet.Threshold(imgRing, out region, 0, th);
                    HObject opened = null;
                    HOperatorSet.OpeningCircle(region, out opened, openR);
                    region.Dispose();
                    region = opened;
                    conn?.Dispose();
                    HOperatorSet.Connection(region, out conn);
                    sel?.Dispose();
                    HOperatorSet.SelectShape(
                        conn,
                        out sel,
                        ((new HTuple("area")).TupleConcat("circularity")).TupleConcat("outer_radius"),
                        "and",
                        (new HTuple(areaMin)).TupleConcat(0.5).TupleConcat(rMin),
                        (new HTuple(areaMax)).TupleConcat(1.0).TupleConcat(rMax));
                    HOperatorSet.CountObj(sel, out HTuple nSel);
                    var tmp = new System.Collections.Generic.List<Hole>();
                    double minC = 1.0;
                    for (int i = 1; i <= nSel.I; i++)
                    {
                        one?.Dispose();
                        HOperatorSet.SelectObj(sel, out one, i);
                        filled?.Dispose();
                        HOperatorSet.FillUp(one, out filled);
                        HOperatorSet.SmallestCircle(filled, out HTuple sr, out HTuple sc, out HTuple srad);
                        double distC = Dist(sr.D, sc.D, row0, col0);
                        if (distC < distMin || distC > distMax)
                        {
                            continue;
                        }
                        HOperatorSet.Circularity(one, out HTuple circ);
                        if (circ.D < minC)
                        {
                            minC = circ.D;
                        }
                        tmp.Add(new Hole { Row = sr.D, Col = sc.D, Rad = srad.D });
                    }
                    if (tmp.Count == 3 && minC > bestCirc)
                    {
                        bestCirc = minC;
                        best = tmp.ToArray();
                    }
                }
                if (best == null)
                {
                    return false;
                }
                holes = best;
                warn = null;
                return true;
            }
            finally
            {
                circleOut?.Dispose();
                circleIn?.Dispose();
                ring?.Dispose();
                imgRing?.Dispose();
                region?.Dispose();
                conn?.Dispose();
                sel?.Dispose();
                one?.Dispose();
                filled?.Dispose();
            }
        }

        static void SortRightmost(Hole[] holes, double row0, double col0)
        {
            var keyed = new (Hole H, double Deg)[3];
            for (int i = 0; i < 3; i++)
            {
                double deg = Math.Atan2(-(holes[i].Row - row0), holes[i].Col - col0) * 180.0 / Math.PI;
                if (deg < 0)
                {
                    deg += 360;
                }
                keyed[i] = (holes[i], deg);
            }
            Array.Sort(keyed, (a, b) => a.Deg.CompareTo(b.Deg));
            int rot = 0;
            double best = 999;
            for (int k = 0; k < 3; k++)
            {
                double d0 = keyed[k].Deg;
                if (d0 > 180)
                {
                    d0 = 360 - d0;
                }
                if (d0 < best)
                {
                    best = d0;
                    rot = k;
                }
            }
            for (int k = 0; k < 3; k++)
            {
                holes[k] = keyed[(k + rot) % 3].H;
            }
        }

        static float FitHole(HObject gray, ChamferOffsetParams p, Hole hole)
        {
            HObject nbhd = null, imgHole = null, annOut = null, annIn = null, annulus = null, imgAnn = null;
            HObject chamfer = null, union = null, filled = null, innerDisk = null;
            HObject outerBound = null, innerBound = null, outerXld = null, innerXld = null;
            HObject tmp = null, outerBest = null, innerBest = null;
            try
            {
                HOperatorSet.GenCircle(out nbhd, hole.Row, hole.Col, 1.20 * hole.Rad);
                HOperatorSet.ReduceDomain(gray, nbhd, out imgHole);
                HOperatorSet.GenCircle(out annOut, hole.Row, hole.Col, 1.14 * hole.Rad);
                HOperatorSet.GenCircle(out annIn, hole.Row, hole.Col, 0.70 * hole.Rad);
                HOperatorSet.Difference(annOut, annIn, out annulus);
                HOperatorSet.ReduceDomain(imgHole, annulus, out imgAnn);
                double innerMin = 0.22 * Math.PI * hole.Rad * hole.Rad;
                double bestNi = -1;
                float bestDist = -1f;
                for (int chTh = 50; chTh <= 80; chTh += 10)
                {
                    chamfer?.Dispose();
                    HOperatorSet.Threshold(imgAnn, out chamfer, 0, chTh);
                    HObject opened = null;
                    HOperatorSet.OpeningCircle(chamfer, out opened, p.ChamferOpenR);
                    chamfer.Dispose();
                    chamfer = opened;
                    HObject closed = null;
                    HOperatorSet.ClosingCircle(chamfer, out closed, p.ChamferCloseR);
                    chamfer.Dispose();
                    chamfer = closed;
                    union?.Dispose();
                    HOperatorSet.Union1(chamfer, out union);
                    filled?.Dispose();
                    HOperatorSet.FillUp(union, out filled);
                    innerDisk?.Dispose();
                    HOperatorSet.Difference(filled, union, out innerDisk);
                    HOperatorSet.AreaCenter(innerDisk, out HTuple ni, out _, out _);
                    if (ni.D < innerMin)
                    {
                        continue;
                    }
                    outerBound?.Dispose();
                    HOperatorSet.Boundary(filled, out outerBound, "outer");
                    innerBound?.Dispose();
                    HOperatorSet.Boundary(innerDisk, out innerBound, "outer");
                    outerXld?.Dispose();
                    HOperatorSet.GenContourRegionXld(outerBound, out outerXld, "border");
                    innerXld?.Dispose();
                    HOperatorSet.GenContourRegionXld(innerBound, out innerXld, "border");
                    HOperatorSet.CountObj(outerXld, out HTuple nOut);
                    HOperatorSet.CountObj(innerXld, out HTuple nIn);
                    if (nOut.I < 1 || nIn.I < 1)
                    {
                        continue;
                    }
                    PickLongest(outerXld, nOut.I, ref tmp, ref outerBest);
                    PickLongest(innerXld, nIn.I, ref tmp, ref innerBest);
                    HOperatorSet.FitCircleContourXld(
                        outerBest, "geohuber", -1, 0, 0, 3, 2,
                        out HTuple bigRow, out HTuple bigCol, out HTuple bigRad, out _, out _, out _);
                    HOperatorSet.FitCircleContourXld(
                        innerBest, "geohuber", -1, 0, 0, 3, 2,
                        out HTuple inRow, out HTuple inCol, out HTuple inRad, out _, out _, out _);
                    double br = First(bigRad);
                    double ir = First(inRad);
                    if (ir < 8 || br <= ir + 2)
                    {
                        continue;
                    }
                    double ratio = br / ir;
                    if (ratio < 1.08 || ratio > 1.32)
                    {
                        continue;
                    }
                    double dist = Dist(First(bigRow), First(bigCol), First(inRow), First(inCol));
                    if (dist >= 0.25 * ir)
                    {
                        continue;
                    }
                    if (ni.D > bestNi)
                    {
                        bestNi = ni.D;
                        bestDist = (float)dist;
                    }
                }
                return bestDist;
            }
            finally
            {
                nbhd?.Dispose();
                imgHole?.Dispose();
                annOut?.Dispose();
                annIn?.Dispose();
                annulus?.Dispose();
                imgAnn?.Dispose();
                chamfer?.Dispose();
                union?.Dispose();
                filled?.Dispose();
                innerDisk?.Dispose();
                outerBound?.Dispose();
                innerBound?.Dispose();
                outerXld?.Dispose();
                innerXld?.Dispose();
                tmp?.Dispose();
                outerBest?.Dispose();
                innerBest?.Dispose();
            }
        }

        static void PickLongest(HObject xlds, int n, ref HObject tmp, ref HObject best)
        {
            double bestLen = -1;
            for (int q = 1; q <= n; q++)
            {
                tmp?.Dispose();
                HOperatorSet.SelectObj(xlds, out tmp, q);
                HOperatorSet.LengthXld(tmp, out HTuple len);
                if (len.D > bestLen)
                {
                    bestLen = len.D;
                    best?.Dispose();
                    HOperatorSet.CopyObj(tmp, out best, 1, -1);
                }
            }
        }

        static double First(HTuple t)
        {
            if (t == null || t.TupleLength() < 1)
            {
                return 0;
            }
            return t[0].D;
        }

        static double Dist(double r1, double c1, double r2, double c2)
        {
            double dr = r1 - r2;
            double dc = c1 - c2;
            return Math.Sqrt(dr * dr + dc * dc);
        }

        static float ExportDist(float dist, ChamferOffsetParams p, Cell cell)
        {
            if (dist < 0)
            {
                return -1f;
            }
            if (p != null && p.ExportDistAsMm && cell != null && cell.MmPerPixel > 0)
            {
                return (float)(dist * cell.MmPerPixel);
            }
            return dist;
        }
    }
}
