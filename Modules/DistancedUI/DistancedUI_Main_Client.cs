using Marketplace.Modules.Global_Options;

namespace Marketplace.Modules.DistancedUI;

[UsedImplicitly]
[Market_Autoload(Market_Autoload.Type.Client, Market_Autoload.Priority.Normal)]
public static class DistancedUI_Main_Client
{
    [UsedImplicitly]
    private static void OnInit()
    {
        DistancedUI_UI.Init();
        DistancedUI_DataType.SyncedDistancedUIData.ValueChanged += OnDistancedUIUpdate;
        Marketplace.Global_Updator += Update;
    }

    [HarmonyPatch(typeof(ZNetScene),nameof(ZNetScene.Awake))]
    [ClientOnlyPatch]
    private static class ZNetScene_Awake_Patch
    {
        [UsedImplicitly]
        private static void Postfix() => OnDistancedUIUpdate();
    }
    
    private static void OnDistancedUIUpdate()
    {
        if (DistancedUI_DataType.SyncedDistancedUIData.Value.Enabled)
        {
            DistancedUI_UI.Show(DistancedUI_DataType.SyncedDistancedUIData.Value.MarketplaceEnabled, DistancedUI_DataType.SyncedDistancedUIData.Value.MailEnabled);
        }
        else
        {
            DistancedUI_UI.Hide();
        }
    }
    
    private static void Update(float dt)
    {
        if (Input.GetKeyDown(KeyCode.Escape) && DistancedUI_UI.IsViewProfilesVisible())
        {
            DistancedUI_UI.HideViewProfiles();
            Menu.instance.OnClose();
        }

        if (!Player.m_localPlayer) return;
        if (Input.GetKey(KeyCode.LeftAlt) && Input.GetKeyDown(KeyCode.BackQuote))
        {
            DistancedUI_UI.ClickView();
        }
    }
}