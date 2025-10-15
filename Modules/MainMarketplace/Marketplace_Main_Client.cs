using Marketplace.Modules.Global_Options;
using Object = UnityEngine.Object;

namespace Marketplace.Modules.MainMarketplace;

[UsedImplicitly]
[Market_Autoload(Market_Autoload.Type.Client, Market_Autoload.Priority.Normal)]
public static class Marketplace_Main_Client
{

    [UsedImplicitly]
    private static void OnInit()
    { 
        Marketplace_UI.Init();
        Marketplace.Global_Updator += Update;
        Global_Configs.SyncedGlobalOptions.ValueChanged += Marketplace_UI.ReloadCurrency;
    }

    public static List<DB.DB.MarketSlot> MarketData = new(); 
    
    private static int _requestRevision = 0;
    public static void RequestData()
    {
        MarketData.Clear();
        _requestRevision++;
        ZRoutedRpc.instance.InvokeRoutedRPC("KGmarket ReceiveTradepostData_s", _requestRevision);
    }
    
    [MarketplaceRPC("KGmarket ReceiveTradepostData", Market_Autoload.Type.Client)]    
    private static void OnMarketplaceUpdate(long sender, ZPackage pkg)
    { 
        if (!Marketplace_UI.IsPanelVisible() || Marketplace_UI.currentMarketMode != Marketplace_DataTypes.MarketMode.BUY) return;
        pkg.Decompress();
        int revision = pkg.ReadInt();
        if (_requestRevision > revision) return;
        MarketData.Clear();
        int dataAmount = pkg.ReadInt();
        List<DB.DB.MarketSlot> data = new List<DB.DB.MarketSlot>(dataAmount);
        for (int i = 0; i < dataAmount; i++)
        {
            DB.DB.MarketSlot slot = new DB.DB.MarketSlot();
            slot.Deserialize(ref pkg);
            data.Add(slot);
        }
        
        MarketData = data;
        if (Marketplace_UI.IsPanelVisible()) Marketplace_UI.OnMarketplaceDataUpdate();
    }

    private static void Update(float dt)
    {
        if (Input.GetKeyDown(KeyCode.Escape) &&
            (Marketplace_UI.IsPanelVisible()))
        {
            Marketplace_UI.Hide();
            Menu.instance.OnClose();
        }
    }

    [HarmonyPatch(typeof(TextInput), nameof(TextInput.IsVisible))]
    [ClientOnlyPatch]
    private static class MarketplaceUIFix
    {
        [UsedImplicitly]
        private static void Postfix(ref bool __result)
        {
            if (Marketplace_UI.IsPanelVisible()) __result = true;
        }
    }

    [MarketplaceRPC("KGmarket BuyItemAnswer", Market_Autoload.Type.Client)]
    private static void InstantiateItemFromServer(long sender, string data)
    { 
        if (sender == ZRoutedRpc.instance.GetServerPeerID() && data.Length > 0)
        {
            Player p = Player.m_localPlayer;
            DB.DB.MarketSlot shopItem = JSON.ToObject<DB.DB.MarketSlot>(data);
            GameObject main = ZNetScene.instance.GetPrefab(shopItem.ItemPrefab);
            if (!main) return;
            string text = Localization.instance.Localize("$mpasn_added", shopItem.Count.ToString(), Localization.instance.Localize(main.GetComponent<ItemDrop>().m_itemData.m_shared.m_name));
            MessageHud.instance.ShowMessage(MessageHud.MessageType.Center, text);
            ItemDrop item = main.GetComponent<ItemDrop>();
            shopItem.Quality = Mathf.Max(1, shopItem.Quality);
            int stack = shopItem.Count;
            Dictionary<string, string> NewCustomData = JSON.ToObject<Dictionary<string, string>>(shopItem.CUSTOMdata);

            while (stack > 0)
            {
                if (p.m_inventory.FindEmptySlot(true) is { x: >= 0 } pos)
                {
                    int addStack = Math.Min(stack, item.m_itemData.m_shared.m_maxStackSize);
                    stack -= addStack;
                    /*ItemDrop.ItemData newData = item.m_itemData.Clone();
                    newData.m_customData = NewCustomData;
                    newData.m_quality = shopItem.Quality;
                    newData.m_stack = addStack;
                    newData.m_crafterName = shopItem.CrafterName;
                    newData.m_crafterID = shopItem.CrafterID;
                    newData.m_worldLevel = Game.m_worldLevel;
                    newData.m_variant = shopItem.Variant; 
                    newData.m_dropPrefab = item.gameObject;
                    newData.m_durability = item.m_itemData.GetMaxDurability(shopItem.Quality) * Mathf.Clamp01(shopItem.DurabilityPercent / 100f);
                    p.m_inventory.AddItem(newData);*/
                      
                    p.m_inventory.AddItem(shopItem.ItemPrefab, addStack, item.m_itemData.GetMaxDurability(shopItem.Quality) * Mathf.Clamp01(shopItem.DurabilityPercent / 100f), pos,
                        false, shopItem.Quality, shopItem.Variant, shopItem.CrafterID, shopItem.CrafterName,
                        NewCustomData, Game.m_worldLevel, true);
                }
                else
                {
                    break;
                }
            }

            if (stack <= 0) return;
            while (stack > 0)
            {
                int addStack = Math.Min(stack, item.m_itemData.m_shared.m_maxStackSize);
                stack -= addStack;
                Transform transform = p.transform;
                Vector3 position = transform.position;
                ItemDrop itemDrop = Object.Instantiate(main, position + Vector3.up, transform.rotation).GetComponent<ItemDrop>();
                itemDrop.m_itemData.m_customData = NewCustomData;
                itemDrop.m_itemData.m_quality = shopItem.Quality;
                itemDrop.m_itemData.m_stack = addStack;
                itemDrop.m_itemData.m_crafterName = shopItem.CrafterName;
                itemDrop.m_itemData.m_crafterID = shopItem.CrafterID;
                itemDrop.m_itemData.m_worldLevel = Game.m_worldLevel;
                itemDrop.m_itemData.m_variant = shopItem.Variant;
                itemDrop.m_itemData.m_durability = item.m_itemData.GetMaxDurability(shopItem.Quality) * Mathf.Clamp01(shopItem.DurabilityPercent / 100f);;
                
                itemDrop.Save();
                itemDrop.OnPlayerDrop();
                itemDrop.GetComponent<Rigidbody>().velocity = (transform.forward + Vector3.up);
                p.m_dropEffects.Create(position, Quaternion.identity);
            }
        }
    }
}