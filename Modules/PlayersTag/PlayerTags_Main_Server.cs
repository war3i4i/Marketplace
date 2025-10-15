using Marketplace.Paths;

namespace Marketplace.Modules.PlayersTag;

[UsedImplicitly]
[Market_Autoload(Market_Autoload.Type.Server, Market_Autoload.Priority.Last, "OnInit",
    new[] { "PlayerTags.cfg" },
    new[] { "OnPlayerTagsChange" })]
public static class PlayerTags_Main_Server
{
    [UsedImplicitly]
    private static void OnInit()
    {
        ReadPlayerTags();
    }

    private static void ReadPlayerTags()
    {
        PlayersTag_DataTypes.SyncedPlayersTagData.Value.Clear();
        IReadOnlyList<string> profiles = File.ReadAllLines(Market_Paths.PlayerTagsConfig);
        foreach (string line in profiles)
        {
            string[] split = line.Split(':');
            if (split.Length != 2) continue;
            PlayersTag_DataTypes.SyncedPlayersTagData.Value[split[0].Trim(' ')] = split[1].Trim(' ');
        }
        PlayersTag_DataTypes.SyncedPlayersTagData.Update();
    }

    [UsedImplicitly]
    private static void OnPlayerTagsChange()
    {
        ReadPlayerTags();
        Utils.print("Player Tags changed, sending to peers");
    }

    [HarmonyPatch(typeof(ZNet), nameof(ZNet.UpdatePlayerList))]
    [ServerOnlyPatch]
    private static class ZNet_UpdatePlayerList_Patch
    {
        [UsedImplicitly]
        private static void Postfix(ZNet __instance)
        {
            for (int i = 0; i < __instance.m_players.Count; ++i)
            {
                if (!PlayersTag_DataTypes.SyncedPlayersTagData.Value.TryGetValue(__instance.m_players[i].m_userInfo.m_id.m_userID, out string SpecialTag)) continue;
                ZNet.PlayerInfo newPI = __instance.m_players[i];
                newPI.m_name = $"{SpecialTag} {newPI.m_name}";
                __instance.m_players[i] = newPI;

            }
        }
    }
}