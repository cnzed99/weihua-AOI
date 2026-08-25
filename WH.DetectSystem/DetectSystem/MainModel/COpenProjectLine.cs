using System;
using System.Collections.Generic;
using System.Linq;

namespace WH.DetectSystem.DetectSystem.MainModel
{
    /// <summary>
    /// 【盘齿方案11-注释】当前打开的 .burrproj 产线。仅 OpenProj / 关闭工程时赋值，不是 appConfig 锁。
    /// </summary>
    public enum OpenProjectLineKind
    {
        None = 0,
        Zipper = 1,
        Gear = 2,
    }

    /// <summary>
    /// 【盘齿方案11-注释】按制程算法插件名识别产线（方案11.2 §2）。停点 1 只识别，不挂协议。
    /// </summary>
    public static class COpenProjectLine
    {
        public static readonly string[] GearFixedProcessNames =
        {
            "下端面", "上齿面", "上端面", "内孔", "上轴侧面", "下轴侧面", "整轴侧面",
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

        public static OpenProjectLineKind Kind { get; internal set; } = OpenProjectLineKind.None;

        public static bool IsZipper => Kind == OpenProjectLineKind.Zipper;

        public static bool IsGear => Kind == OpenProjectLineKind.Gear;

        /// <summary>
        /// 冲突返回 false，不修改 Kind。成功时写出 kind，由 OpenProj 提交后再赋 Kind。
        /// </summary>
        public static bool TryRecognize(CMainModelsModel model, out OpenProjectLineKind kind, out string conflictMsg)
        {
            kind = OpenProjectLineKind.None;
            conflictMsg = null;
            bool hasGearPlugin = false;
            bool hasZipperPlugin = false;
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
                    }
                }
            }

            if (hasGearPlugin && hasZipperPlugin)
            {
                conflictMsg = "工程混用了两套算法插件（盘齿 GearTestAlgorihm 与拉链插件），已中止打开。请拆成两个 .burrproj。";
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
            if (names.Count == GearFixedProcessNames.Length
                && GearFixedProcessNames.All(p => names.Contains(p)))
            {
                kind = OpenProjectLineKind.Gear;
                return true;
            }

            kind = OpenProjectLineKind.None;
            return true;
        }
    }
}
