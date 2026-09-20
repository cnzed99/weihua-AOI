using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using WH.Entity.LogRecord;
using WH.RunCell;

namespace WH.DetectSystem.Models
{
    /// <summary>【方案7.1-注释】离线按张评测。只分 OK / NG 两项，再打合计。除了 OK 都是 NG。</summary>
    public sealed class OfflineSampleEval
    {
        readonly object _lock = new object();
        readonly DefectStat _ok = new DefectStat();
        readonly DefectStat _ng = new DefectStat();
        readonly HashSet<string> _fnFiles = new HashSet<string>();
        readonly HashSet<string> _fpFiles = new HashSet<string>();
        int _total;
        int _skipped;

        public bool IsRunning { get; private set; }

        public void Begin()
        {
            lock (_lock)
            {
                IsRunning = true;
                _total = 0;
                _skipped = 0;
                _ok.Clear();
                _ng.Clear();
                _fnFiles.Clear();
                _fpFiles.Clear();
            }
        }

        public bool TryFinish(string processFullName, CLogRec log, out string message)
        {
            lock (_lock)
            {
                if (!IsRunning)
                {
                    message = null;
                    return false;
                }

                IsRunning = false;
                message = BuildLog(processFullName);
            }

            log?.Info(message);
            return true;
        }

        public void Record(Cell sourceCell, Cell filteredCell)
        {
            if (sourceCell == null || string.IsNullOrEmpty(sourceCell.ImageFile))
            {
                return;
            }

            string name = Path.GetFileNameWithoutExtension(sourceCell.ImageFile);
            if (!TryParseEvalLabel(name, out bool gtNg))
            {
                lock (_lock)
                {
                    if (!IsRunning)
                    {
                        return;
                    }

                    _total++;
                    _skipped++;
                }

                return;
            }

            bool hit = HasAnyNg(filteredCell);
            string fileName = Path.GetFileName(sourceCell.ImageFile);
            lock (_lock)
            {
                if (!IsRunning)
                {
                    return;
                }

                _total++;
                DefectStat stat = gtNg ? _ng : _ok;
                if (gtNg)
                {
                    if (hit)
                    {
                        stat.Tp++;
                    }
                    else
                    {
                        stat.Fn++;
                        if (!string.IsNullOrEmpty(fileName)) { _fnFiles.Add(fileName); }
                    }
                }
                else
                {
                    if (hit)
                    {
                        stat.Fp++;
                        if (!string.IsNullOrEmpty(fileName)) { _fpFiles.Add(fileName); }
                    }
                    else
                    {
                        stat.Tn++;
                    }
                }
            }
        }

        /// <summary>
        /// 真值只分 OK / NG：找到第一段 OK 或 NG。不是 OK 的都算 NG。
        /// 可在最前面，也可在方案 0.4 的 {ID}_{张号}_ 之后。
        /// </summary>
        public static bool TryParseEvalLabel(string fileNameNoExt, out bool isNg)
        {
            isNg = false;
            if (string.IsNullOrWhiteSpace(fileNameNoExt))
            {
                return false;
            }

            string[] parts = fileNameNoExt.Split('_');
            int start = 0;
            if (LooksLikeImport04Head(parts))
            {
                start = 2;
            }

            for (int i = start; i < parts.Length; i++)
            {
                if (parts[i] == "OK")
                {
                    isNg = false;
                    return true;
                }

                if (parts[i] == "NG")
                {
                    isNg = true;
                    return true;
                }
            }

            return false;
        }

        /// <summary>兼容旧名。缺陷名不再用于分行，isNg 有效。</summary>
        public static bool TryParseEvalPrefix(string fileNameNoExt, out string defectName, out bool isNg)
        {
            defectName = null;
            if (!TryParseEvalLabel(fileNameNoExt, out isNg))
            {
                return false;
            }

            defectName = isNg ? "NG" : "OK";
            return true;
        }

        /// <summary>仅当最前面就是评测前缀时才剥，给灌图用。0.4 头后面的 OK_OK / 有钢珠_NG 不剥。</summary>
        public static bool TryStripEvalPrefix(string fileName, out string remainder)
        {
            remainder = null;
            if (string.IsNullOrWhiteSpace(fileName))
            {
                return false;
            }

            string justName = Path.GetFileName(fileName);
            string noExt = Path.GetFileNameWithoutExtension(justName);
            string ext = Path.GetExtension(justName);
            string[] parts = noExt.Split('_');
            if (parts.Length < 2 || (parts[1] != "NG" && parts[1] != "OK"))
            {
                return false;
            }

            if (parts.Length == 2)
            {
                remainder = string.Empty;
                return true;
            }

            remainder = string.Join("_", parts, 2, parts.Length - 2) + ext;
            return true;
        }

        static bool LooksLikeImport04Head(string[] parts)
        {
            if (parts == null || parts.Length < 3 || string.IsNullOrEmpty(parts[0]))
            {
                return false;
            }

            if (parts[1] == "OK" || parts[1] == "NG")
            {
                return false;
            }

            return int.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out _);
        }

        static bool HasAnyNg(Cell cell)
        {
            if (cell == null)
            {
                return false;
            }

            if (!cell.IsOK)
            {
                return true;
            }

            if (cell.Detections == null)
            {
                return false;
            }

            foreach (var item in cell.Detections)
            {
                if (item != null && item.Result == false)
                {
                    return true;
                }
            }

            return false;
        }

        string BuildLog(string processFullName)
        {
            DefectStat sum = new DefectStat();
            sum.Add(_ok);
            sum.Add(_ng);

            int valid = _total - _skipped;
            var sb = new StringBuilder();
            sb.Append("【方案7.1-注释】暂停 ").Append(processFullName).AppendLine();
            sb.Append("  张数=").Append(_total).Append(" 有效=").Append(valid).Append(" 跳过=").Append(_skipped);
            sb.AppendLine();
            sb.Append("  OK: ").Append(FormatStat(_ok));
            sb.AppendLine();
            sb.Append("  NG: ").Append(FormatStat(_ng));
            sb.AppendLine();
            sb.Append("  合计: ").Append(FormatStat(sum));
            if (sum.Tn + sum.Fp == 0)
            {
                sb.Append(" 无OK前缀，误判率未测");
            }

            AppendFileList(sb, "漏检", _fnFiles);
            AppendFileList(sb, "误判", _fpFiles);

            return sb.ToString();
        }

        static void AppendFileList(StringBuilder sb, string title, HashSet<string> files)
        {
            sb.AppendLine();
            sb.Append("  ").Append(title).Append(" ").Append(files.Count).Append("张:");
            if (files.Count == 0)
            {
                sb.Append(" 无");
                return;
            }

            int i = 0;
            foreach (string name in files)
            {
                i++;
                sb.AppendLine();
                sb.Append("    ").Append(i).Append(". ").Append(name);
            }
        }
        static string FormatStat(DefectStat s)
        {
            return "真NG=" + (s.Tp + s.Fn)
                + " 真OK=" + (s.Tn + s.Fp)
                + " TP=" + s.Tp
                + " FN=" + s.Fn
                + " FP=" + s.Fp
                + " TN=" + s.Tn
                + " 检出率=" + Rate(s.Tp, s.Tp + s.Fn)
                + " 漏检率=" + Rate(s.Fn, s.Tp + s.Fn)
                + " 误判率=" + Rate(s.Fp, s.Tn + s.Fp);
        }

        static string Rate(int num, int den)
        {
            if (den <= 0)
            {
                return "NA";
            }

            return (num * 100.0 / den).ToString("F2", CultureInfo.InvariantCulture) + "%";
        }

        sealed class DefectStat
        {
            public int Tp;
            public int Fn;
            public int Fp;
            public int Tn;

            public void Add(DefectStat o)
            {
                Tp += o.Tp;
                Fn += o.Fn;
                Fp += o.Fp;
                Tn += o.Tn;
            }

            public void Clear()
            {
                Tp = 0;
                Fn = 0;
                Fp = 0;
                Tn = 0;
            }
        }
    }
}
