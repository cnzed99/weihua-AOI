using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Newtonsoft.Json;

namespace WH.DetectSystem.Models
{
    /// <summary>
    /// 制程副窗列表中的一行（是否含副窗 + 自定义显示名），随 .burrproj 序列化。
    /// </summary>
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public partial class CProcessSubWindowItem : ObservableObject
    {
        [ObservableProperty]
        [JsonProperty]
        bool enabled;

        [ObservableProperty]
        [JsonProperty]
        string displayName = ProcessSubWindowList.DefaultTitle;
    }

    /// <summary>
    /// 弹窗列表与工程存储之间的拷贝 / 缺省行。
    /// </summary>
    public static class ProcessSubWindowList
    {
        public const string DefaultTitle = "副图";

        public static CProcessSubWindowItem CreateDefaultRow()
        {
            return new CProcessSubWindowItem
            {
                Enabled = false,
                DisplayName = DefaultTitle,
            };
        }

        public static ObservableCollection<CProcessSubWindowItem> CreateEditorList(
            IEnumerable<CProcessSubWindowItem> stored
        )
        {
            var list = new ObservableCollection<CProcessSubWindowItem>();
            if (stored == null || !stored.Any())
            {
                list.Add(CreateDefaultRow());
                return list;
            }
            var item = stored.First();
            list.Add(
                new CProcessSubWindowItem
                {
                    Enabled = item.Enabled,
                    DisplayName = item.DisplayName,
                }
            );
            return list;
        }

        public static ObservableCollection<CProcessSubWindowItem> CopyForStore(
            IEnumerable<CProcessSubWindowItem> editorList
        )
        {
            var list = new ObservableCollection<CProcessSubWindowItem>();
            if (editorList == null)
            {
                return list;
            }
            var item = editorList.FirstOrDefault();
            if (item == null)
            {
                return list;
            }
            list.Add(
                new CProcessSubWindowItem
                {
                    Enabled = item.Enabled,
                    DisplayName = item.DisplayName,
                }
            );
            return list;
        }

        public static bool FillBlankEnabledDisplayNames(
            IEnumerable<CProcessSubWindowItem> items
        )
        {
            var filled = false;
            if (items == null)
            {
                return false;
            }
            foreach (var item in items)
            {
                if (item.Enabled && string.IsNullOrWhiteSpace(item.DisplayName))
                {
                    item.DisplayName = DefaultTitle;
                    filled = true;
                }
            }
            return filled;
        }
    }
}