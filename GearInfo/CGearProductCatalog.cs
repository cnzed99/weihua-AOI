using System;
using System.Collections.Generic;
using System.IO;
using WH.Entity;
using WH.Entity.LogRecord;

namespace GearInfo
{
    /// <summary>
    /// 【盘齿方案8-注释】盘齿型号目录。启动只 Load，缺文件用内存三型号且不写盘；切型号不得 Save。
    /// </summary>
    public class CGearProductCatalog
    {
        public const string ParameterPath = "..\\SystemConfig\\GearInfoData.Json";

        public string CurrentModelId { get; set; }

        public List<CGearProductModel> Models { get; set; }

        public static string GetResolvedPath()
        {
            try
            {
                return Path.GetFullPath(ParameterPath);
            }
            catch
            {
                return ParameterPath;
            }
        }

        /// <summary>
        /// 【盘齿方案8-注释】仅 Load。文件缺失/解析失败时 Warn + 内存默认，不 Save。
        /// </summary>
        public static CGearProductCatalog Load()
        {
            CGearProductCatalog builtIn = CreateBuiltIn();
            try
            {
                string resolved = GetResolvedPath();
                if (!File.Exists(ParameterPath) && !File.Exists(resolved))
                {
                    TryLogWarn("盘齿型号文件不存在: " + resolved + "，使用内存默认三型号，不写盘。");
                    return builtIn;
                }

                CGearProductCatalog loaded = ConfigAPI.LoadDeserialize<CGearProductCatalog>(ParameterPath);
                if (loaded == null || loaded.Models == null || loaded.Models.Count == 0)
                {
                    TryLogWarn("盘齿型号文件解析为空: " + resolved + "，使用内存默认三型号，不写盘。");
                    return builtIn;
                }

                loaded.Models.RemoveAll(m => m == null);
                if (loaded.Models.Count == 0)
                {
                    TryLogWarn("盘齿型号列表为空: " + resolved + "，使用内存默认三型号，不写盘。");
                    return builtIn;
                }

                return loaded;
            }
            catch (Exception ex)
            {
                TryLogWarn("盘齿型号加载失败: " + ex.Message + "，使用内存默认三型号，不写盘。");
                return builtIn;
            }
        }

        public CGearProductModel FindCurrentModel()
        {
            if (Models == null || Models.Count == 0)
            {
                return null;
            }

            if (!string.IsNullOrEmpty(CurrentModelId))
            {
                for (int i = 0; i < Models.Count; i++)
                {
                    CGearProductModel model = Models[i];
                    if (model != null && string.Equals(model.Id, CurrentModelId, StringComparison.Ordinal))
                    {
                        return model;
                    }
                }
            }

            return Models[0];
        }

        public static CGearProductCatalog CreateBuiltIn()
        {
            return new CGearProductCatalog
            {
                CurrentModelId = "C100-17",
                Models = new List<CGearProductModel>
                {
                    new CGearProductModel
                    {
                        Id = "SQ150",
                        Name = "SQ150 初级主动齿",
                        LengthMm = 55.5,
                        WidthMm = 55.5,
                        HeightMm = 27.5
                    },
                    new CGearProductModel
                    {
                        Id = "C100-17",
                        Name = "C100-17 初级主动齿",
                        LengthMm = 75,
                        WidthMm = 75,
                        HeightMm = 83
                    },
                    new CGearProductModel
                    {
                        Id = "4699303086",
                        Name = "4699303086 行星齿轮",
                        LengthMm = 41.5,
                        WidthMm = 41.5,
                        HeightMm = 17.5
                    }
                }
            };
        }

        static void TryLogWarn(string message)
        {
            try
            {
                CLogRec.Default.Warn(message);
            }
            catch
            {
            }
        }
    }
}
