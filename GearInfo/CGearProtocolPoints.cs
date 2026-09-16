using System.IO;
using System.Text;
using Newtonsoft.Json;
using WH.Entity.LogRecord;

namespace GearInfo
{
    /// <summary>
    /// 点位定义（地址来自 JSON，代码零硬编码）。
    /// </summary>
    public class GearPointDef
    {
        public int Address { get; set; }
        public string Type { get; set; }
        public string Access { get; set; }
        public bool Enabled { get; set; } = true;
        public int? Default { get; set; }
    }

    /// <summary>
    /// PLC 站号等。
    /// </summary>
    public class GearPlcDef
    {
        public int Station { get; set; } = 1;
    }

    /// <summary>
    /// 盘齿点位表模型 + JSON 加载/保存。路径对齐 CCommunicationManagement.s_CommPath。
    /// </summary>
    public class CGearProtocolPoints
    {
        // Same directory as CCommunicationManagement.s_CommPath (DirectoryName + this file name). cwd-relative, same as File.Exists(s_CommPath).
        public static string s_PointsPath = Path.Combine(
            Path.GetDirectoryName("..\\SystemConfig\\CommConfig.Json") ?? "..\\SystemConfig",
            "GearProtocolPoints.json");

        public static string GetResolvedPointsPath()
        {
            try
            {
                return Path.GetFullPath(s_PointsPath);
            }
            catch
            {
                return s_PointsPath;
            }
        }

        public GearPlcDef Plc { get; set; } = new GearPlcDef();
        public Dictionary<string, GearPointDef> Points { get; set; } = new Dictionary<string, GearPointDef>();
        public Dictionary<string, string> GroupResults { get; set; } = new Dictionary<string, string>();

        public static bool TryLoad(out CGearProtocolPoints points)
        {
            points = new CGearProtocolPoints();
            try
            {
                string resolved = GetResolvedPointsPath();
                if (!File.Exists(resolved))
                {
                    TryLogWarn("Gear 点位文件不存在: " + resolved);
                    return false;
                }

                string json = File.ReadAllText(resolved, Encoding.UTF8);
                CGearProtocolPoints loaded = JsonConvert.DeserializeObject<CGearProtocolPoints>(json);
                if (loaded == null)
                {
                    TryLogWarn("Gear 点位文件解析为空: " + resolved);
                    return false;
                }

                points = loaded;
                if (points.Plc == null)
                {
                    points.Plc = new GearPlcDef();
                }
                if (points.Points == null)
                {
                    points.Points = new Dictionary<string, GearPointDef>();
                }
                if (points.GroupResults == null)
                {
                    points.GroupResults = new Dictionary<string, string>();
                }
                return true;
            }
            catch (Exception ex)
            {
                TryLogError("Gear 点位加载失败: " + GetResolvedPointsPath() + " " + ex.Message);
                points = new CGearProtocolPoints();
                return false;
            }
        }

        public static bool TrySave(CGearProtocolPoints points)
        {
            try
            {
                if (points == null)
                {
                    return false;
                }

                string resolved = GetResolvedPointsPath();
                string dir = Path.GetDirectoryName(resolved);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                string json = JsonConvert.SerializeObject(points, Formatting.Indented);
                File.WriteAllText(resolved, json, Encoding.UTF8);
                return true;
            }
            catch (Exception ex)
            {
                TryLogError("Gear 点位保存失败: " + ex.Message);
                return false;
            }
        }

        public bool TryGetPoint(string name, out GearPointDef def)
        {
            def = null;
            if (Points == null || string.IsNullOrEmpty(name))
            {
                return false;
            }
            return Points.TryGetValue(name, out def) && def != null;
        }

        public bool TryResolveGroupResultAddress(string groupName, out int address)
        {
            address = 0;
            if (GroupResults == null || Points == null || string.IsNullOrEmpty(groupName))
            {
                return false;
            }
            if (!GroupResults.TryGetValue(groupName, out string pointName) || string.IsNullOrEmpty(pointName))
            {
                return false;
            }
            if (!TryGetPoint(pointName, out GearPointDef def) || def == null)
            {
                return false;
            }
            address = def.Address;
            return true;
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

        static void TryLogError(string message)
        {
            try
            {
                CLogRec.Default.Error(message);
            }
            catch
            {
            }
        }
    }
}
