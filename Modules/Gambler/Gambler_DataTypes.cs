namespace Marketplace.Modules.Gambler;

public static class Gambler_DataTypes
{
    internal static readonly CustomSyncedValue<Dictionary<string, BigData>>
        SyncedGamblerData = new(Marketplace.configSync, "gamblerData", new Dictionary<string, BigData>());
    
    public class BigData : ISerializableParameter
    {
        public Item RequiredItem = new();
        public List<Item> Data = new();
        public int MAXROLLS;

        public void Serialize(ref ZPackage pkg)
        {
            pkg.Write(MAXROLLS);
            pkg.Write(Data.Count);
            foreach (Item item in Data)
            {
                pkg.Write(item.Prefab ?? "");
                pkg.Write(item.Min);
                pkg.Write(item.Max);
            }
            pkg.Write(RequiredItem.Prefab ?? "");
            pkg.Write(RequiredItem.Min);
            pkg.Write(RequiredItem.Max);
        }

        public void Deserialize(ref ZPackage pkg)
        {
            MAXROLLS = pkg.ReadInt();
            int count = pkg.ReadInt();
            for (int i = 0; i < count; ++i)
            {
                Data.Add(new Item
                {
                    Prefab = pkg.ReadString(),
                    Min = pkg.ReadInt(),
                    Max = pkg.ReadInt()
                });
            }
            RequiredItem = new Item
            {
                Prefab = pkg.ReadString(),
                Min = pkg.ReadInt(),
                Max = pkg.ReadInt()
            };
        }
    }
    
    public class Item
    {
        public string Prefab;
        public int Min, Max;
        
        private Sprite icon = null!;
        private string LocalizedName;
        private string rawName;
        public Sprite Sprite => icon;
        public string Name => LocalizedName!;
        public string RawName => rawName!;

        public void SetData(string RAWNAME, Sprite s)
        {
            rawName = RAWNAME;
            LocalizedName = Localization.instance.Localize(rawName);
            icon = s;
        }
    }
}