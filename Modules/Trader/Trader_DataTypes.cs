namespace Marketplace.Modules.Trader;

public static class Trader_DataTypes
{
    internal static readonly CustomSyncedValue<Dictionary<string, List<TraderData>>>
        SyncedTraderItemList = new(Marketplace.configSync, "traderItemList", new Dictionary<string, List<TraderData>>());

    internal static readonly Dictionary<string, List<TraderData>> ClientSideItemList = new();


    public class TraderData : ISerializableParameter
    {
        public bool NeedToKnow;
        public List<TraderItem> NeededItems;
        public List<TraderItem> ResultItems;
        
        public void Serialize(ref ZPackage pkg)
        {
            pkg.Write(NeedToKnow);
            pkg.Write(NeededItems.Count);
            foreach (TraderItem item in NeededItems)
            {
                pkg.Write(item.ItemPrefab ?? "");
                pkg.Write(item.Count);
                pkg.Write(item.Level);
            }

            pkg.Write(ResultItems.Count);
            foreach (TraderItem item in ResultItems)
            {
                pkg.Write(item.ItemPrefab ?? "");
                pkg.Write(item.Count);
                pkg.Write(item.Level);
            }
        }

        public void Deserialize(ref ZPackage pkg)
        {
            NeedToKnow = pkg.ReadBool();
            int count = pkg.ReadInt();
            NeededItems = new List<TraderItem>();
            for (int i = 0; i < count; ++i)
            {
                TraderItem item = new TraderItem
                {
                    ItemPrefab = pkg.ReadString(),
                    Count = pkg.ReadInt(),
                    Level = pkg.ReadInt()
                };
                NeededItems.Add(item);
            }

            count = pkg.ReadInt();
            ResultItems = new List<TraderItem>();
            for (int i = 0; i < count; ++i)
            {
                TraderItem item = new TraderItem
                {
                    ItemPrefab = pkg.ReadString(),
                    Count = pkg.ReadInt(),
                    Level = pkg.ReadInt()
                };
                ResultItems.Add(item);
            }
        }
    }

    
    public class TraderItem
    {
        public int Count;
        public string ItemName;
        public string OriginalItemName;
        public string ItemPrefab;
        public int Level = 1;
        public bool DisplayStars;
        public TraderItemType Type = TraderItemType.Item;
        
        public enum TraderItemType
        {
            Item,
            Skill,
            Monster
        }

        private Sprite Icon;
        public Sprite GetIcon() => Icon;
        public void SetIcon(Sprite icon) => Icon = icon;
    }
}