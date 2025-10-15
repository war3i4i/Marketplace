using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Marketplace.Modules.Global_Options;

namespace Marketplace.Modules.Leaderboard;

[UsedImplicitly]
[Market_Autoload(Market_Autoload.Type.Client)]
public static class LeaderBoard_Main_Client
{
    [UsedImplicitly]
    private static void OnInit()
    {
        Leaderboard_DataTypes.SyncedClientLeaderboard.Value.Count();
        Leaderboard_DataTypes.SyncedClientAchievements.Value.Count();
        GameEvents.OnPlayerDeath += () => SendToServer(Leaderboard_DataTypes.TriggerType.Died);
        GameEvents.OnCreatureKilled += (prefab, _) => SendToServer(Leaderboard_DataTypes.TriggerType.MonstersKilled, prefab);
        GameEvents.OnStructureBuilt += prefab => SendToServer(Leaderboard_DataTypes.TriggerType.StructuresBuilt, prefab);
        GameEvents.OnItemCrafted += (prefab, _) => SendToServer(Leaderboard_DataTypes.TriggerType.ItemsCrafted, prefab);
        GameEvents.KilledBy += prefab => SendToServer(Leaderboard_DataTypes.TriggerType.KilledBy, prefab);
        GameEvents.OnHarvest += prefab => SendToServer(Leaderboard_DataTypes.TriggerType.Harvested, prefab);
        Leaderboard_UI.Init();
        Marketplace.Global_Updator += Update;
    }

    private static void Update(float dt)
    {
        if (Input.GetKeyDown(KeyCode.Escape) && Leaderboard_UI.IsVisible())
        {
            Leaderboard_UI.Hide();
            Menu.instance.OnClose();
        }

        if (Input.GetKey(KeyCode.LeftAlt) && Input.GetKeyDown(KeyCode.L))
        {
            if (Leaderboard_UI.IsVisible())
            {
                Leaderboard_UI.Hide();
            }
            else
            {
                Leaderboard_UI.Show();
            }
        }
    }

    public static bool HasAchievement(string achievementID)
    {
        int toId = achievementID.Replace(" ", "").ToLower().GetStableHashCode();
        return Leaderboard_DataTypes.SyncedClientLeaderboard.Value.TryGetValue(Global_Configs._localUserID + "_" + Game.instance.m_playerProfile.m_playerName,
            out Leaderboard_DataTypes.Client_Leaderboard LB) && LB.Achievements.Contains(toId);
    }

    public static string GetAchievementName(string achievementID)
    {
        int toId = achievementID.Replace(" ", "").ToLower().GetStableHashCode();
        return Leaderboard_DataTypes.SyncedClientAchievements.Value.Find(x => x.ID == toId)?.Name ?? achievementID;
    }

    private static void SendToServer(Leaderboard_DataTypes.TriggerType type, params object[] args)
    {
        if (!Global_Configs.SyncedGlobalOptions.Value._useLeaderboard) return;
        switch (type)
        {
            case Leaderboard_DataTypes.TriggerType.MonstersKilled:
            case Leaderboard_DataTypes.TriggerType.ItemsCrafted:
            case Leaderboard_DataTypes.TriggerType.StructuresBuilt:
            case Leaderboard_DataTypes.TriggerType.KilledBy:
            case Leaderboard_DataTypes.TriggerType.Harvested:
                ZPackage pkg = new();
                pkg.Write((string)args[0]);
                ZRoutedRpc.instance.InvokeRoutedRPC(ZRoutedRpc.instance.GetServerPeerID(),
                    "Marketplace_Leaderboard_Receive", (int)type, pkg);
                return;
            case Leaderboard_DataTypes.TriggerType.Died:
                ZRoutedRpc.instance.InvokeRoutedRPC(ZRoutedRpc.instance.GetServerPeerID(),
                    "Marketplace_Leaderboard_Receive", (int)type, new ZPackage());
                return;
            case Leaderboard_DataTypes.TriggerType.Explored:
                pkg = new();
                pkg.Write((float)args[0]);
                ZRoutedRpc.instance.InvokeRoutedRPC(ZRoutedRpc.instance.GetServerPeerID(),
                    "Marketplace_Leaderboard_Receive", (int)type, pkg);
                return;
            default:
                return;
        }
    }

    [HarmonyPatch(typeof(Player), nameof(Player.Update))]
    [ClientOnlyPatch]
    private static class Player_Update_Patch
    {
        private static float _time;

        [UsedImplicitly]
        private static void Postfix(Player __instance)
        {
            if (!Minimap.instance || __instance != Player.m_localPlayer ||
                !Global_Configs.SyncedGlobalOptions.Value._useLeaderboard) return;
            _time += Time.deltaTime;
            if (_time >= 3 * 60)
            {
                _time = 0;
                int length = Minimap.instance.m_explored.Length;
                int trueAmount = 0;
                Parallel.ForEach(Partitioner.Create(0, length),
                    new() { MaxDegreeOfParallelism = Environment.ProcessorCount / 2 }, range =>
                    {
                        int localCount = 0;
                        for (int i = range.Item1; i < range.Item2; ++i)
                            if (Minimap.instance.m_explored[i])
                                ++localCount;
                        Interlocked.Add(ref trueAmount, localCount);
                    });
                float exploration = (float)trueAmount / Minimap.instance.m_explored.Length;
                exploration = Mathf.Min(1f, exploration * (4 / Mathf.PI));
                exploration = (float)Math.Round(exploration * 100, 2);
                SendToServer(Leaderboard_DataTypes.TriggerType.Explored, exploration);
            }
        }
    }
}