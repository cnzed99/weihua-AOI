using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using WH.Entity.LogRecord;

namespace XinGearInfo
{
    /// <summary>现场新兴盘齿型号及侧面拍照配方。</summary>
    public class CXinGearProductModel
    {
        public string Id { get; set; }

        public int ToothCount { get; set; }

        // 旧目录没有此字段时，以齿数作为侧面张数。
        public int? SidePhotoCount { get; set; }

        [JsonIgnore]
        public int EffectiveSidePhotoCount => SidePhotoCount ?? ToothCount;
    }

    /// <summary>型号目录与最近选择分开存储，不依赖 .burrproj 或富川 GearInfo。</summary>
    public class CXinGearProductCatalog
    {
        public const string CatalogPath = "..\\SystemConfig\\XinGearProductCatalog.Json";
        public const string SelectionPath = "..\\SystemConfig\\XinGearProductSelection.Json";

        public List<CXinGearProductModel> Models { get; set; }

        public static CXinGearProductCatalog Load()
        {
            string path = Path.GetFullPath(CatalogPath);
            try
            {
                if (!File.Exists(path))
                {
                    throw new FileNotFoundException("型号目录文件不存在", path);
                }

                var catalog = JsonConvert.DeserializeObject<CXinGearProductCatalog>(File.ReadAllText(path));
                if (!IsValid(catalog))
                {
                    throw new InvalidDataException("型号目录必须包含非空且不重复的型号 ID，以及大于 0 的齿数和侧面拍照张数");
                }

                return catalog;
            }
            catch (Exception ex)
            {
                string message = "现场新兴盘齿型号目录不可用，请检查 " + path + "：" + ex.Message;
                Warn(message);
                throw new InvalidDataException(message, ex);
            }
        }

        public CXinGearProductModel FindSelectedModel()
        {
            string selectedId = null;
            try
            {
                if (File.Exists(SelectionPath))
                {
                    var selection = JsonConvert.DeserializeObject<CXinGearProductSelection>(File.ReadAllText(SelectionPath));
                    selectedId = selection?.CurrentModelId;
                    if (string.IsNullOrWhiteSpace(selectedId) || !Models.Any(m => m.Id == selectedId))
                    {
                        Warn("现场新兴盘齿上次选择无效，回退默认型号: " + Path.GetFullPath(SelectionPath));
                    }
                }
            }
            catch (Exception ex)
            {
                Warn("现场新兴盘齿上次选择读取失败，回退默认型号: " + ex.Message);
            }

            return Models.FirstOrDefault(m => m.Id == selectedId)
                ?? Models.FirstOrDefault(m => m.Id == "A")
                ?? Models[0];
        }

        public static bool TrySaveSelection(string modelId, out string error)
        {
            error = null;
            string tempPath = null;
            try
            {
                string resolved = Path.GetFullPath(SelectionPath);
                Directory.CreateDirectory(Path.GetDirectoryName(resolved));
                tempPath = resolved + ".tmp";
                string json = JsonConvert.SerializeObject(
                    new CXinGearProductSelection { CurrentModelId = modelId }, Formatting.Indented);
                File.WriteAllText(tempPath, json, new UTF8Encoding(false));
                File.Move(tempPath, resolved, true);
                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                Warn("现场新兴盘齿型号选择保存失败: " + error);
                try
                {
                    if (tempPath != null && File.Exists(tempPath)) File.Delete(tempPath);
                }
                catch { }
                return false;
            }
        }


        private static bool IsValid(CXinGearProductCatalog catalog)
        {
            return catalog?.Models != null
                && catalog.Models.Count > 0
                && catalog.Models.All(m => m != null && !string.IsNullOrWhiteSpace(m.Id) && m.ToothCount > 0 && m.EffectiveSidePhotoCount > 0)
                && catalog.Models.Select(m => m.Id).Distinct(StringComparer.Ordinal).Count() == catalog.Models.Count;
        }

        private static void Warn(string message)
        {
            try { CLogRec.Default.Warn(message); }
            catch { }
        }

        private class CXinGearProductSelection
        {
            public string CurrentModelId { get; set; }
        }
    }
}
