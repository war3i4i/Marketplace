using Marketplace.Modules.Global_Options;

namespace Marketplace.Modules.PlayersTag;

[UsedImplicitly]
[Market_Autoload(Market_Autoload.Type.Client)]
public static class PlayerTag_Main_Client
{
    [UsedImplicitly]
    private static void OnInit()
    {
        PlayersTag_DataTypes.SyncedPlayersTagData.ValueChanged += OnTagsUpdate;
    }

    private static void OnTagsUpdate()
    {
        if (!Game.instance || !Player.m_localPlayer) return;
        Player.m_localPlayer.m_nview.m_zdo.Set("Marketplace_PlayerTag",
            PlayersTag_DataTypes.SyncedPlayersTagData.Value.TryGetValue(Global_Configs._localUserID, out string tag)
                ? tag
                : "");
    }
    
    [HarmonyPatch(typeof(Player),nameof(Player.GetHoverName))]
    [ClientOnlyPatch]
    private static class Player_GetHoverName_Patch
    {
        [UsedImplicitly]
        private static void Postfix(Player __instance, ref string __result)
        {
            if(!__instance.m_nview.IsValid()) return;
            string tag = __instance.m_nview.m_zdo.GetString("Marketplace_PlayerTag");
            if (tag == "") return;
            __result = $"{tag} {__result}";
        }
    }

    [HarmonyPatch(typeof(Player), nameof(Player.SetPlayerID))]
    [ClientOnlyPatch]
    static class Player_SetPlayerID_Patch
    {
        [UsedImplicitly]
        static void Postfix()
        {
            Player.m_localPlayer?.m_nview.m_zdo.Set("Marketplace_PlayerTag", 
                PlayersTag_DataTypes.SyncedPlayersTagData.Value.TryGetValue(Global_Configs._localUserID, out string tag)
                    ? tag
                    : "");
        }
    }

    [HarmonyPatch(typeof(UserInfo), nameof(UserInfo.GetLocalUser))]
    [ClientOnlyPatch]
    private static class UserInfo_GetLocalUser_Patch
    {
        [UsedImplicitly]
        private static void Postfix(ref UserInfo __result)
        {
            if (PlayersTag_DataTypes.SyncedPlayersTagData.Value.TryGetValue(Global_Configs._localUserID,
                    out string SpecialTag))
            {
                __result.Name = $"{SpecialTag} {__result.Name}";
            }
        }
    }
}