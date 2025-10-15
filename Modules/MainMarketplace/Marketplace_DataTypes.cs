using LiteDB;

namespace Marketplace.Modules.MainMarketplace;

public static class Marketplace_DataTypes
{
    public class ClientMarketSendData
    {
        public string ItemPrefab;
        public int Count;
        public int Price;
        public string SellerName;
        public ItemData_ItemCategory ItemCategory;
        public int Quality;
        public int Variant; 
        public string CUSTOMdata = "{}";
        public string CrafterName = "";
        public long CrafterID;
        public byte DurabilityPercent;
        public string Currency;

        [BsonIgnore]
        public string ItemName => (ZNetScene.instance.GetPrefab(ItemPrefab) is { } item ? item.GetComponent<ItemDrop>().m_itemData.m_shared.m_name : ItemPrefab)!;
    }

    public enum ItemData_ItemCategory
    {
        ALL,
        WEAPONS,
        ARMOR,
        CONSUMABLE,
        TOOLS,
        RESOURCES
    }

    public enum MarketMode
    {
        BUY,
        SELL
    }

    public enum SortBy
    {
        None,
        ItemName,
        Count,
        Price,
        Seller
    }

    public enum SortType
    {
        UP,
        DOWN
    }
}