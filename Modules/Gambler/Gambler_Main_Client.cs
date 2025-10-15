using Marketplace.ExternalLoads;

namespace Marketplace.Modules.Gambler;

[UsedImplicitly]
[Market_Autoload(Market_Autoload.Type.Client, Market_Autoload.Priority.Normal)]
public static class Gambler_Main_Client
{
    [UsedImplicitly]
    private static void OnInit()
    {
        Gambler_UI.Init();
        Gambler_DataTypes.SyncedGamblerData.ValueChanged += OnGamblerUpdate;
        Marketplace.Global_Updator += Update;
    }

    private static void OnGamblerUpdate()
    {
        GamblerInit();
        if (Gambler_UI.IsPanelVisible() && Gambler_UI.CurrentStatus == Gambler_UI.Status.Idle) 
        {
            Gambler_UI.Reload();
        }
    }
    
    private static void Update(float dt)
    {
        if (!Input.GetKeyDown(KeyCode.Escape) || !Gambler_UI.IsPanelVisible()) return;
        Gambler_UI.Hide();
        Menu.instance.OnClose();
    }
    
    [HarmonyPatch(typeof(ZNetScene),nameof(ZNetScene.Awake))]
    [ClientOnlyPatch, HarmonyPriority(-10000)]
    private static class ZNetScene_Awake_Patch
    {
        [UsedImplicitly]
private static void Postfix() => GamblerInit();
    }
    
    public static void GamblerInit()
    {
        if(!ZNetScene.instance) return;
        foreach (KeyValuePair<string, Gambler_DataTypes.BigData> item in Gambler_DataTypes.SyncedGamblerData.Value)
        {
            foreach (Gambler_DataTypes.Item data in item.Value.Data)
            {
                GameObject prefab = ZNetScene.instance.GetPrefab(data.Prefab);
                if (!prefab)
                {
                    data.SetData(data.Prefab!, AssetStorage.PlaceholderGamblerIcon);
                }
                else
                {
                    Sprite hook = prefab.GetComponent<ItemDrop>()?.m_itemData.GetIcon()!;
                    string name = prefab.GetComponent<ItemDrop>().m_itemData.m_shared.m_name;
                    data.SetData(name, hook);
                }
            }
            
            GameObject reqPrefab = ZNetScene.instance.GetPrefab(item.Value.RequiredItem.Prefab);
            if (!reqPrefab)
            {
                item.Value.RequiredItem.SetData(item.Value.RequiredItem.Prefab!, AssetStorage.PlaceholderGamblerIcon);
            }
            else
            {
                Sprite hook = reqPrefab.GetComponent<ItemDrop>()?.m_itemData.GetIcon()!;
                string name = reqPrefab.GetComponent<ItemDrop>().m_itemData.m_shared.m_name;
                item.Value.RequiredItem.SetData(name, hook);
            }
        }
    }
}