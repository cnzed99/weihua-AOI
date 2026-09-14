using System;
using System.Collections.Generic;
using System.Linq;

namespace WH.DetectSystem.DetectSystem.MainModel
{
    /// <summary>
    /// 当前打开的 .burrproj 产线
    /// </summary>
    public enum OpenProjectLineKind
    {
        None = 0,
        Zipper = 1,
        Gear = 2,
        Crank = 3, // 曲轴方案
        XinGear = 4, // 新兴盘齿
    }

    /// <summary>
    /// 按制程算法插件名识别产线
    /// </summary>
    public static class COpenProjectLine
    {
        public static readonly string[] GearFixedProcessNames =
        {
            "下端面", "上齿面", "上端面", "内孔", "上轴侧面", "下轴侧面", "整轴侧面",
        };

        // 【曲轴】
        public static readonly string[] CrankFixedProcessNames =
        {
            "端面", "底部光滑面", "杆面", "底盘侧面", "顶面", "底面",
        };

        // 【新兴盘齿】
        public static readonly string[] XinGearFixedProcessNames =
        {
            "齿底", "齿顶", "侧面",
        };

        public static readonly HashSet<string> ZipperAlgorithmNames = new HashSet<string>(StringComparer.Ordinal)
        {
            "ZipperTestAlgorihm",
            "ZipperTestAlgorihm2",
            "ZipperTestAlgorihm3",
            "MetalZipperAlgorihm",
            "PullZipperAlgorihm",
            "UpMassZipperAlgorihm",
        };

        public const string GearAlgorithmName = "GearTestAlgorihm";

        // 曲轴
        public const string CrankAlgorithmName = "CrankTestAlgorihm";

        // 新兴
        public const string PlaneGearAlgorithmName = "PlaneGearTestAlgorihm";

        public static OpenProjectLineKind Kind { get; internal set; } = OpenProjectLineKind.None;

        public static bool IsZipper => Kind == OpenProjectLineKind.Zipper;

        public static bool IsGear => Kind == OpenProjectLineKind.Gear;

        public static bool IsCrank => Kind == OpenProjectLineKind.Crank;

        public static bool IsXinGear => Kind == OpenProjectLineKind.XinGear;

        /// 离线评测：后做产线共用，拉链除外
        public static bool UsesOfflineSampleEval => IsGear || IsXinGear || IsCrank;

        /// <summary>
        /// 冲突返回 false，不修改 Kind。成功时写出 kind，由 OpenProj 提交后再赋 Kind。
        /// </summary>
        public static bool TryRecognize(CMainModelsModel model, out OpenProjectLineKind kind, out string conflictMsg)
        {
            kind = OpenProjectLineKind.None;
            conflictMsg = null;
            bool hasGearPlugin = false;
            bool hasZipperPlugin = false;
            bool hasCrankPlugin = false;
            bool hasXinGearPlugin = false;
            var names = new HashSet<string>(StringComparer.Ordinal);

            if (model?.CProcessGroups != null)
            {
                foreach (var group in model.CProcessGroups)
                {
                    if (group?.CMainModels == null)
                    {
                        continue;
                    }
                    foreach (var proc in group.CMainModels)
                    {
                        if (proc == null)
                        {
                            continue;
                        }
                        if (!string.IsNullOrEmpty(proc.Name))
                        {
                            names.Add(proc.Name);
                        }
                        string algo = proc.Algorithm;
                        if (string.IsNullOrEmpty(algo) || algo == "算法")
                        {
                            continue;
                        }
                        if (string.Equals(algo, GearAlgorithmName, StringComparison.Ordinal))
                        {
                            hasGearPlugin = true;
                        }
                        if (ZipperAlgorithmNames.Contains(algo))
                        {
                            hasZipperPlugin = true;
                        }
                        if (string.Equals(algo, CrankAlgorithmName, StringComparison.Ordinal))
                        {
                            hasCrankPlugin = true;
                        }
                        if (string.Equals(algo, PlaneGearAlgorithmName, StringComparison.Ordinal))
                        {
                            hasXinGearPlugin = true;
                        }
                    }
                }
            }

            // Gear / Zipper / Crank 插件任意两套拒绝打开
            int pluginKinds = (hasGearPlugin ? 1 : 0) + (hasZipperPlugin ? 1 : 0)
                + (hasCrankPlugin ? 1 : 0) + (hasXinGearPlugin ? 1 : 0);
            if (pluginKinds > 1)
            {
                var parts = new List<string>();
                if (hasGearPlugin) parts.Add("盘齿 GearTestAlgorihm");
                if (hasZipperPlugin) parts.Add("拉链插件");
                if (hasCrankPlugin) parts.Add("曲轴 CrankTestAlgorihm");
                if (hasXinGearPlugin) parts.Add("新兴 PlaneGearTestAlgorihm");
                conflictMsg = "工程混用了多套算法插件（" + string.Join(" 与 ", parts) + "），已中止打开。请拆成两个 .burrproj。";
                return false;
            }

            if (hasGearPlugin)
            {
                kind = OpenProjectLineKind.Gear;
                return true;
            }
            if (hasZipperPlugin)
            {
                kind = OpenProjectLineKind.Zipper;
                return true;
            }
            if (hasCrankPlugin)
            {
                kind = OpenProjectLineKind.Crank;
                return true;
            }
            if (hasXinGearPlugin)
            {
                kind = OpenProjectLineKind.XinGear;
                return true;
            }
            if (names.Count == GearFixedProcessNames.Length
                && GearFixedProcessNames.All(p => names.Contains(p)))
            {
                kind = OpenProjectLineKind.Gear;
                return true;
            }
            if (names.Count == CrankFixedProcessNames.Length
                && CrankFixedProcessNames.All(p => names.Contains(p)))
            {
                kind = OpenProjectLineKind.Crank;
                return true;
            }
            if (names.Count == XinGearFixedProcessNames.Length
                && XinGearFixedProcessNames.All(p => names.Contains(p)))
            {
                kind = OpenProjectLineKind.XinGear;
                return true;
            }

            kind = OpenProjectLineKind.None;
            return true;
        }
    }
}
