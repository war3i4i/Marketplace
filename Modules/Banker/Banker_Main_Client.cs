namespace Marketplace.Modules.Banker;

[UsedImplicitly]
[Market_Autoload(Market_Autoload.Type.Client, Market_Autoload.Priority.Normal)]
public static class Banker_Main_Client
{
    [UsedImplicitly]
    private static void OnInit()
    {
        Banker_UI.Init();
        Banker_DataTypes.SyncedBankerProfiles.ValueChanged += OnBankerUpdate;
        Marketplace.Global_Updator += Update;
    }

    private static void OnBankerUpdate()
    {
        Banker_UI.Reload();
    }
    
    private static void Update(float dt)
    {
        if (!Input.GetKeyDown(KeyCode.Escape) || !Banker_UI.IsPanelVisible()) return;
        Banker_UI.Hide();
        Menu.instance.OnClose();
    }
    
    
    [MarketplaceRPC("KGmarket GetBankerClientData", Market_Autoload.Type.Client)]
    private static void GetBankerClientData(long sender, ZPackage pkg)
    {
        pkg.Decompress();
        Banker_DataTypes.BankerClientData = JSON.ToObject<Dictionary<int, int>>(pkg.ReadString());
        Banker_UI.Reload();
    }
    
    
}