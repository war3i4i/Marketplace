using UnityEngine.Networking;

namespace Marketplace.Modules.ServerInfo;

[UsedImplicitly]
[Market_Autoload(Market_Autoload.Type.Client, Market_Autoload.Priority.Normal)]
public static class ServerInfo_Main_Client
{
    private static void OnInit()
    {
        ServerInfo_UI.Init();
        ServerInfo_DataTypes.SyncedServerInfoData.ValueChanged += OnInfoUpdate;
        Marketplace.Global_Updator += Update;
        GameEvents.OnPlayerFirstSpawn += OnPlayerFirstSpawn;
    }
 
    private static void OnPlayerFirstSpawn()
    {
        if (ServerInfo_DataTypes.SyncedServerInfoData.Value.ContainsKey("onplayerfirstspawn"))
            ServerInfo_UI.Show("onplayerfirstspawn", "");
    } 

    private static void Update(float dt)
    { 
        if (!Input.GetKeyDown(KeyCode.Escape) || !ServerInfo_UI.IsPanelVisible()) return;
        ServerInfo_UI.Hide();
        Menu.instance.OnClose();
    }

    private static void OnInfoUpdate()
    {
        foreach (ServerInfo_DataTypes.ServerInfoQueue.Info url in  ServerInfo_DataTypes.SyncedServerInfoData.Value.Values.SelectMany(x => x.infoQueue).Where(x => x.Type == ServerInfo_DataTypes.ServerInfoQueue.Info.InfoType.Image))
        {
            Utils.LoadImageFromWEB(url.Text, (sprite) => url.SetSprite(sprite));
        }
        if (ServerInfo_UI.IsPanelVisible()) ServerInfo_UI.Reload();
        
      
    }
}