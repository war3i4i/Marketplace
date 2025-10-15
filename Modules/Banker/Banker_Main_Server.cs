using System.Threading.Tasks;
//using Marketplace.Database;
using Marketplace.Modules.Global_Options;
using Marketplace.Modules.MainMarketplace;
using Marketplace.Paths;
using Splatform;

namespace Marketplace.Modules.Banker;

[UsedImplicitly]
[Market_Autoload(Market_Autoload.Type.Server, Market_Autoload.Priority.Normal, "OnInit", new[] { "BA" }, new[] { "OnBankerProfilesFileChange" })]
public static class Banker_Main_Server
{
    public static readonly Dictionary<string, Dictionary<int, DateTime>> BankerTimeStamp = new();


    [UsedImplicitly]
    private static void OnInit()
    {
        if (Global_Configs.BankerIncomeTime > 0)
        { 
            Marketplace._thistype.StartCoroutine(BankerIncome());
        }
        ReadServerBankerProfiles();
    }
  
    [UsedImplicitly]
    private static void OnBankerProfilesFileChange()
    {
        ReadServerBankerProfiles();
        Utils.print("Banker Changed. Sending new info to all clients");
    }

    private static void ProcessBankerProfiles(IReadOnlyList<string> profiles)
    {
        string splitProfile = "default";
        foreach (string line in profiles)
        {
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#")) continue;
            if (line.StartsWith("["))
            {
                splitProfile = line.Replace("[", "").Replace("]", "").Replace(" ", "").ToLower();
            }
            else
            {
                int test = line.Replace(" ", "").GetStableHashCode();
                if (Banker_DataTypes.SyncedBankerProfiles.Value.TryGetValue(splitProfile, out List<int> value))
                {
                    value.Add(test);
                }
                else
                {
                    Banker_DataTypes.SyncedBankerProfiles.Value[splitProfile] = new List<int> { test };
                }
            }
        }
    }

    public static void ReadServerBankerProfiles()
    {
        Banker_DataTypes.SyncedBankerProfiles.Value.Clear();
        string folder = Market_Paths.BankerProfilesFolder;
        string[] files = Directory.GetFiles(folder, "*.cfg", SearchOption.AllDirectories);
        foreach (string file in files)
        {
            IReadOnlyList<string> profiles = File.ReadAllLines(file).ToList();
            ProcessBankerProfiles(profiles);
        }

        Banker_DataTypes.SyncedBankerProfiles.Update();
    }
 
    private static IEnumerator BankerIncome()
    {
        while (true)
        {
            yield return new WaitForSecondsRealtime(Global_Configs.BankerIncomeTime * 3600);
            if (!ZNet.instance || !ZNet.instance.IsServer()) continue;
            Utils.print("Adding Banker Income");
            Task task = Task.Run(() => { DB.DB._context.IncreaseIntereset(Global_Configs.SyncedGlobalOptions.Value._vipPlayerList.Split(',').ToHashSet(), BankerTimeStamp); });
            yield return new WaitUntil(() => task.IsCompleted);
            foreach (ZNetPeer peer in ZNet.instance.m_peers)
            {
                if (!peer.IsReady()) continue;
                
                SendBankerDataToClient(peer, false);
            }
            if (ZNet.IsSinglePlayer) SendBankerDataToClient(PlatformManager.DistributionPlatform.LocalUser.PlatformUserID.m_userID, ZRoutedRpc.instance.m_id, false);
        }
    }

    private static void SendBankerDataToClient(ZNetPeer peer, bool sendif0)
    {
        string userID = peer.m_socket.GetHostName();
        SendBankerDataToClient(userID, peer.m_uid, sendif0);
    }
    
    private static void SendBankerDataToClient(string hostname, long id, bool sendif0)
    {
        if (hostname == "0") return;

        Dictionary<int, int> bankData = DB.DB._context.GetUserBankItems(hostname);
        if (bankData.Count == 0 && !sendif0) return;
        string data = JSON.ToJSON(bankData);
        ZPackage pkg = new();
        pkg.Write(data);
        pkg.Compress();
        ZRoutedRpc.instance.InvokeRoutedRPC(id, "KGmarket GetBankerClientData", pkg);
    }
    
    [HarmonyPatch(typeof(ZNet), nameof(ZNet.RPC_CharacterID))]
    [ServerOnlyPatch]
    private static class ZnetSyncBankerProfiles
    {
        /*[UsedImplicitly]
        private static bool Condition() => !BankerDatabase.UseBankerDatabase;*/

        [UsedImplicitly]
        private static void Postfix(ZRpc rpc)
        {
            if (!ZNet.instance.IsServer()) return;
            if (!BankerTimeStamp.ContainsKey(rpc.m_socket.GetHostName())) BankerTimeStamp[rpc.m_socket.GetHostName()] = new Dictionary<int, DateTime>();
            ZNetPeer peer = ZNet.instance.GetPeer(rpc);
            if (peer == null) return;
            SendBankerDataToClient(peer, true);
        }
    } 
 
    [HarmonyPatch(typeof(ZNet),nameof(ZNet.GetPeer), typeof(long))]
    [ServerOnlyPatch]
    private static class ZNet_GetPeer_Patch 
    {
        private static void Postfix(ZNet __instance, long uid, ref ZNetPeer __result)
        {
            if (ZNet.IsSinglePlayer && uid == ZRoutedRpc.instance.m_id) 
                __result = new ZNetPeer(new Utils.SelfSocket(), true) { m_uid = uid, m_playerName = Game.instance.m_playerProfile.m_playerName };
        }
    }
    [HarmonyPatch(typeof(ZNet),nameof(ZNet.GetPeerByHostName))]
    [ServerOnlyPatch]
    private static class ZNet_GetPeerByHostName_Patch 
    {
        private static void Postfix(ZNet __instance, string endpoint, ref ZNetPeer __result)
        { 
            if (ZNet.IsSinglePlayer && PlatformManager.DistributionPlatform.LocalUser.PlatformUserID.m_userID == endpoint) 
                __result = new ZNetPeer(new Utils.SelfSocket(), true) { m_uid = ZRoutedRpc.instance.m_id, m_playerName = Game.instance.m_playerProfile.m_playerName };
        }
    }
    
    
    [MarketplaceRPC("KGmarket BankerDeposit", Market_Autoload.Type.Server)]
    public static void MethodBankerDeposit(long sender, string item, int value)
    {
        ZNetPeer peer = ZNet.instance.GetPeer(sender);
        if (peer == null) return; 
        string userID = peer.m_socket.GetHostName();
        int hash = item.GetStableHashCode();
        if (!BankerTimeStamp.ContainsKey(userID)) BankerTimeStamp[userID] = new Dictionary<int, DateTime>();
        BankerTimeStamp[userID][hash] = DateTime.Now;
        DB.DB._context.AddBankItem(userID, hash, value);
        SendBankerDataToClient(peer, true);
        Market_Logger.Log(Market_Logger.LogType.Banker, $"Player User ID: {userID} Deposit an item {item} with quantity: {value}.");
    } 

    [MarketplaceRPC("KGmarket BankerWithdraw", Market_Autoload.Type.Server)]
    private static void MethodBankerWithdraw(long sender, string item, int value)
    {
        ZNetPeer peer = ZNet.instance.GetPeer(sender);
        if (peer == null) return;
        string userID = peer.m_socket.GetHostName();
        int hash = item.GetStableHashCode();
        if (!BankerTimeStamp.ContainsKey(userID)) BankerTimeStamp[userID] = new Dictionary<int, DateTime>();
        BankerTimeStamp[userID][hash] = DateTime.Now;
        int removed = DB.DB._context.RemoveBankItem(userID, hash, value);
        if (removed <= 0) return;
        DB.DB.MarketSlot mockData = new()
            { Count = removed, ItemPrefab = item, Quality = 1 };
        string json = JSON.ToJSON(mockData);
        ZRoutedRpc.instance.InvokeRoutedRPC(sender, "KGmarket BuyItemAnswer", json);
        SendBankerDataToClient(peer, true);
        Market_Logger.Log(Market_Logger.LogType.Banker,
            $"Player User ID: {userID} Withdraw an item {item} with quantity: {removed}");
    }  
    
    [MarketplaceRPC("KGmarket BankerRemove", Market_Autoload.Type.Server)]
    private static void MethodBankerRemove(long sender, string item, int value)
    {
        ZNetPeer peer = ZNet.instance.GetPeer(sender);
        if (peer == null) return;
        string userID = peer.m_socket.GetHostName();
        int hash = item.GetStableHashCode();
        DB.DB._context.RemoveBankItem(userID, hash, value);
        SendBankerDataToClient(peer, true);
        Market_Logger.Log(Market_Logger.LogType.Banker,
            $"Player User ID: {userID} Removed (Paid with) an item {item} with quantity: {value}");
    }
}