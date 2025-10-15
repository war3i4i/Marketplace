//using Marketplace.Database;
using Marketplace.Modules.Banker;
using Marketplace.Modules.CMS;
using Marketplace.Modules.Global_Options;
using Marketplace.Paths;

namespace Marketplace.Modules.MainMarketplace;


[UsedImplicitly]
[Market_Autoload(Market_Autoload.Type.Server, Market_Autoload.Priority.Normal)]
public static class Marketplace_Main_Server
{
    [UsedImplicitly]
    private static void OnInit()
    {
        
    }
    
    [MarketplaceRPC("KGmarket ReceiveItem", Market_Autoload.Type.Server)]
    private static void ReceiveItemFromClient(long sender, string data)
    {
        if (data.Length <= 0) return;
        Marketplace_DataTypes.ClientMarketSendData toConvert =
            JSON.ToObject<Marketplace_DataTypes.ClientMarketSendData>(data);
        if (toConvert.Count <= 0) return;
        ZNetPeer peer = ZNet.instance.GetPeer(sender);
        if (peer == null) return;
        string userID = peer.m_socket.GetHostName();
        DB.DB.MarketSlot newData = new DB.DB.MarketSlot(toConvert, userID);
        DB.DB._context.Marketplace_AddItem(newData);
        SaveMarketAndSendToClients();
        Market_Logger.Log(Market_Logger.LogType.Marketplace, $"Player User ID: {userID} added an item {newData.ItemPrefab} with quantity: {newData.Count} price: {newData.Price}");
        DiscordStuff.DiscordStuff.SendMarketplaceWebhook(newData);
    }

    private static void SaveMarketAndSendToClients()
    {
        List<DB.DB.MarketSlot> allSlots = DB.DB._context.Marketplace_GetItems();
        ZPackage pkg = new();
        pkg.Write(int.MaxValue);
        pkg.Write(allSlots.Count);
        for(int i = 0; i < allSlots.Count; ++i) allSlots[i].Serialize(ref pkg);
        pkg.Compress();
        foreach (ZNetPeer peer in ZNet.instance.m_peers)
        {
            if (!peer.IsReady()) continue;
            ZRoutedRpc.instance.InvokeRoutedRPC(peer.m_uid, "KGmarket ReceiveTradepostData", pkg);
        }
        if (ZNet.IsSinglePlayer) ZRoutedRpc.instance.InvokeRoutedRPC(ZRoutedRpc.instance.m_id, "KGmarket ReceiveTradepostData", pkg);
    }
    
    [MarketplaceRPC("KGmarket ReceiveTradepostData_s", Market_Autoload.Type.Server)]
    private static void ReceiveTradepostRequest(long sender, int revision)
    {
        List<DB.DB.MarketSlot> allSlots = DB.DB._context.Marketplace_GetItems();
        ZPackage pkg = new();
        pkg.Write(revision);
        pkg.Write(allSlots.Count);
        for(int i = 0; i < allSlots.Count; ++i) allSlots[i].Serialize(ref pkg);
        pkg.Compress();
        ZRoutedRpc.instance.InvokeRoutedRPC(sender, "KGmarket ReceiveTradepostData", pkg);
    }

    [MarketplaceRPC("KGmarket RemoveItemAdmin", Market_Autoload.Type.Server)]
    private static void RemoveItemAdminStatus(long sender, int id)
    {
        ZNetPeer peer = ZNet.instance.GetPeer(sender);
        if (peer == null) return;
        string userID = peer.m_socket.GetHostName();
        if (ZNet.instance.m_adminList == null || !ZNet.instance.ListContainsId(ZNet.instance.m_adminList, userID)) return;
        DB.DB._context.Marketplace_RemoveItem(id);
        SaveMarketAndSendToClients();
    }

    [MarketplaceRPC("KGmarket RequestBuyItem", Market_Autoload.Type.Server)]
    private static void RequestBuyItem(long sender, ZPackage data)
    {
        if (data == null) return;
        int id = data.ReadInt();
        int goldValue = data.ReadInt();
        int quantity = data.ReadInt();
        string currency = data.ReadString();
        DB.DB.MarketSlot findData = DB.DB._context.GetSlotByID(id);
        if (findData != null)
        {
            Marketplace_DataTypes.ClientMarketSendData sendToBuyer = null;
            if (quantity >= findData.Count) 
            {
                DB.DB._context.Marketplace_RemoveItem(id);
                sendToBuyer = findData.ToClient();
                int leftOver = quantity - findData.Count;
                goldValue = findData.Count * findData.Price;
                if (leftOver > 0)
                {
                    DB.DB.MarketSlot mockData = new DB.DB.MarketSlot
                    {
                        Count = leftOver * findData.Price,
                        ItemPrefab = currency,
                        Quality = 1
                    };
                    string jsonLeftOver = JSON.ToJSON(mockData);
                    ZRoutedRpc.instance.InvokeRoutedRPC(sender, "KGmarket BuyItemAnswer", jsonLeftOver);
                }
            }
            else
            {
                int needToSet = findData.Count - quantity;
                findData.Count = quantity;
                sendToBuyer = findData.ToClient(); 
                findData.Count = needToSet;
                DB.DB._context.ModifyExisting(findData);
            }

            SaveMarketAndSendToClients();
            ///////////income mechanics
            ZNetPeer peer = ZNet.instance.GetPeer(sender);
            if (peer == null) return;
            string buyerUserID = peer.m_socket.GetHostName();
            string sellerUserID = findData.SellerUserID!;
            
            string buyerName = peer.m_playerName;
            if (buyerUserID == sellerUserID)
            {
                CentralizedMailSystem_Server.AddMail(
                    "$mpasn_CMS_TradePost",
                    "$mpasn_CMS_TradePost_SlotCancelled",
                    $"$mpasn_CMS_TradePost_SlotCancelledMessage:\n<link=kg.CMSLINK:100><color=#ffbf00><u><b>[•{findData.LocalizeKey()} x{quantity}]</b></u></color></link>",
                    userID: buyerUserID, 
                    attachments: [sendToBuyer]);
                
                Market_Logger.Log(Market_Logger.LogType.Marketplace,
                    $"{buyerName} (ID: {buyerUserID}) cancelled his slot {findData.ItemPrefab} quantity: {findData.Count} price: {findData.Price}. He took: {quantity} items");
            }
            else
            {
                int applyTaxes = Global_Configs.SyncedGlobalOptions.Value._vipPlayerList.Contains(sellerUserID)
                    ? Global_Configs.SyncedGlobalOptions.Value._vipmarketTaxes
                    : Global_Configs.SyncedGlobalOptions.Value._marketTaxes;
                applyTaxes = Mathf.Max(0, applyTaxes);
                float endValue = goldValue - goldValue * (applyTaxes / 100f);
                
                CentralizedMailSystem_Server.AddMail(
                    "$mpasn_CMS_TradePost", 
                    "$mpasn_CMS_TradePost_SlotBought",
                    $"$mpasn_CMS_TradePost_YouBoughtItemFrom {findData.SellerName}:\n<link=kg.CMSLINK:100><color=#ffbf00><u><b>[•{findData.LocalizeKey()} x{quantity}]</b></u></color></link>\n$mpasn_CMS_Price: {findData.Price}",
                    userID: buyerUserID, 
                    attachments: [sendToBuyer]);
                
                Marketplace_DataTypes.ClientMarketSendData moneyToSeller = new Marketplace_DataTypes.ClientMarketSendData 
                {
                    Count = (int)Math.Min(int.MaxValue, (long)endValue),
                    ItemPrefab = currency,
                    Quality = 1
                }; 
                
                CentralizedMailSystem_Server.AddMail(
                    "$mpasn_CMS_TradePost",
                    "$mpasn_CMS_TradePost_Income",  
                    $"$mpasn_CMS_TradePost_YouSoldItem:\n<link=kg.CMSLINK:0><color=#ffbf00><u><b>[•{findData.LocalizeKey()} x{quantity}]</b></u></color></link>\n$mpasn_CMS_Price: {findData.Price}\n$mpasn_CMS_Taxes: {applyTaxes}%",
                    userID: sellerUserID, 
                    attachments: [moneyToSeller],
                    links: [sendToBuyer]);
                 
                Market_Logger.Log(Market_Logger.LogType.Marketplace,
                    $"{buyerName} (ID: {buyerUserID}) bought {findData.SellerName}'s (ID: {sellerUserID}) slot {findData.ItemPrefab} quantity: {findData.Count} (bought: x{quantity}) price: {findData.Price}");
            }
            ///////////////////////////
        }
        else
        {
            if (goldValue != null)
            {
                DB.DB.MarketSlot mockData = new DB.DB.MarketSlot
                {
                    Count = goldValue, ItemPrefab = currency, Quality = 1
                };
                string json = JSON.ToJSON(mockData);
                ZRoutedRpc.instance.InvokeRoutedRPC(sender, "KGmarket BuyItemAnswer", json);
            }
            List<DB.DB.MarketSlot> allSlots = DB.DB._context.Marketplace_GetItems();

            ZPackage pkg = new();
            pkg.Write(int.MaxValue);
            pkg.Write(allSlots.Count);
            for(int i = 0; i < allSlots.Count; ++i) allSlots[i].Serialize(ref pkg);
            pkg.Compress();
            ZRoutedRpc.instance.InvokeRoutedRPC(sender, "KGmarket ReceiveTradepostData", pkg);
        }
    }
}