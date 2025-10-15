using System.Text.RegularExpressions;
using System.Threading;
using Marketplace.Modules.Global_Options;
using Marketplace.Paths;

namespace Marketplace.Modules.Leaderboard;
 
[UsedImplicitly]
[Market_Autoload(Market_Autoload.Type.Server, Market_Autoload.Priority.Last, "OnInit",
    new[] { "LA" },
    new[] { "OnAchievementsFileChange" })]
public static class Leaderboard_Server_Main
{
    [UsedImplicitly] 
    private static void OnInit()
    {
        ReadAchievementsProfiles();
        SendToPlayers();
        Marketplace._thistype.StartCoroutine(UpdateLB());
    }
    
    [MarketplaceRPC("Marketplace_Leaderboard_Receive", Market_Autoload.Type.Server)]
    private static void RPC_ReceiveLeaderboardEvent(long sender, int type, ZPackage pkg)
    {
        if (!Global_Configs.SyncedGlobalOptions.Value._useLeaderboard) return;
        Leaderboard_DataTypes.TriggerType triggerType = (Leaderboard_DataTypes.TriggerType)type;
        ZNetPeer peer = ZNet.instance.GetPeer(sender);
        if (peer == null) return;
        string id = peer.m_socket.GetHostName();
        if (id == "0") return;
        id = id + "_" + peer.m_playerName;
        DB.DB.Player_Leaderboard leaderboard = DB.DB._context.GetOrCreateLeaderboard(id.Trim(), peer.m_playerName);

        if (leaderboard.PlayerName == null)
            leaderboard.PlayerName = peer.m_playerName;
        switch (triggerType)
        {
            case Leaderboard_DataTypes.TriggerType.MonstersKilled:
                string prefab = pkg.ReadString();
                if (string.IsNullOrWhiteSpace(prefab)) return;
                if (!leaderboard.KilledCreatures.ContainsKey(prefab))
                    leaderboard.KilledCreatures.Add(prefab, 1);
                else leaderboard.KilledCreatures[prefab]++;
                break;
            case Leaderboard_DataTypes.TriggerType.ItemsCrafted:
                prefab = pkg.ReadString();
                if (string.IsNullOrWhiteSpace(prefab)) return;
                if (!leaderboard.ItemsCrafted.ContainsKey(prefab))
                    leaderboard.ItemsCrafted.Add(prefab, 1);
                else leaderboard.ItemsCrafted[prefab]++;
                break;
            case Leaderboard_DataTypes.TriggerType.StructuresBuilt:
                prefab = pkg.ReadString();
                if (string.IsNullOrWhiteSpace(prefab)) return;
                if (!leaderboard.BuiltStructures.ContainsKey(prefab))
                    leaderboard.BuiltStructures.Add(prefab, 1);
                else leaderboard.BuiltStructures[prefab]++;
                break;
            case Leaderboard_DataTypes.TriggerType.KilledBy:
                prefab = pkg.ReadString();
                if (string.IsNullOrWhiteSpace(prefab)) return;
                if (!leaderboard.KilledBy.ContainsKey(prefab))
                    leaderboard.KilledBy.Add(prefab, 1);
                else leaderboard.KilledBy[prefab]++;
                break;
            case Leaderboard_DataTypes.TriggerType.Died:
                leaderboard.DeathAmount++;
                break;
            case Leaderboard_DataTypes.TriggerType.Explored:
                leaderboard.MapExplored = pkg.ReadSingle();
                break;
            case Leaderboard_DataTypes.TriggerType.Harvested:
                prefab = pkg.ReadString();
                if (string.IsNullOrWhiteSpace(prefab)) return;
                if (!leaderboard.Harvested.ContainsKey(prefab))
                    leaderboard.Harvested.Add(prefab, 1);
                else leaderboard.Harvested[prefab]++;
                break;
            default:
                break;
        }
        DB.DB._context.UpdateLeaderboard(leaderboard);
    } 

    private static IEnumerator UpdateLB()
    {
        while (true)
        {
            yield return new WaitForSecondsRealtime(5 * 60);
            if (!ZNet.instance || !ZNet.instance.IsServer() || !Global_Configs.SyncedGlobalOptions.Value._useLeaderboard) continue;
            SendToPlayers();
        }
    } 

    private static void SendToPlayers()
    {
        Leaderboard_DataTypes.SyncedClientLeaderboard.Value.Clear();

        foreach (KeyValuePair<string, DB.DB.Player_Leaderboard> leaderboard in DB.DB._context.GetAllLeaderboards())
        {
            Leaderboard_DataTypes.Client_Leaderboard newLeaderboard = new()
            {
                PlayerName = leaderboard.Value.PlayerName,
                ItemsCrafted = leaderboard.Value.ItemsCrafted.Sum(x => x.Value),
                KilledCreatures = leaderboard.Value.KilledCreatures.Sum(x => x.Value),
                BuiltStructures = leaderboard.Value.BuiltStructures.Sum(x => x.Value),
                Harvested = leaderboard.Value.Harvested.Sum(x => x.Value),
                KilledPlayers = leaderboard.Value.KilledPlayers,
                Died = leaderboard.Value.DeathAmount,
                MapExplored = leaderboard.Value.MapExplored,
                Achievements = new()
            };
            foreach (Leaderboard_DataTypes.Achievement achievement in Leaderboard_DataTypes.AllAchievements)
            {
                if (achievement.Check(leaderboard.Value)) newLeaderboard.Achievements.Add(achievement.ID);
            }

            Leaderboard_DataTypes.SyncedClientLeaderboard.Value.Add(leaderboard.Key, newLeaderboard);
        }

        Leaderboard_DataTypes.SyncedClientLeaderboard.Update();
    }

    private static void ProcessAchievementsProfile(string fPath, IReadOnlyList<string> profiles)
    {
        Leaderboard_DataTypes.Achievement currentAchievement = null;
        for (int i = 0; i < profiles.Count; ++i)
        {
            if (string.IsNullOrWhiteSpace(profiles[i]) || profiles[i].StartsWith("#")) continue;
            if (profiles[i].StartsWith("["))
            {
                currentAchievement = new Leaderboard_DataTypes.Achievement
                {
                    ID = profiles[i].Replace("[", "").Replace("]", "").ToLower().GetStableHashCode()
                };
            }
            else
            {
                if (currentAchievement == null) continue;
                try
                {
                    string type = profiles[i];
                    string name = profiles[i + 1];
                    string description = profiles[i + 2];
                    string prefabParse = profiles[i + 3];
                    string color = profiles[i + 4];
                    string tierParse = profiles[i + 5];
                    if (!Enum.TryParse(type, true, out Leaderboard_DataTypes.TriggerType triggerType))
                    {
                        i += 5;
                        continue;
                    }

                    currentAchievement.Type = triggerType;
                    currentAchievement.Name = name;
                    currentAchievement.Description = description;

                    if (triggerType is not (Leaderboard_DataTypes.TriggerType.Explored 
                        or Leaderboard_DataTypes.TriggerType.Died
                        or Leaderboard_DataTypes.TriggerType.PlayersKilled)) 
                    {
                        string[] prefabSplit = prefabParse.Replace(" ", "").Split(',');
                        currentAchievement.Prefab = prefabSplit[0];
                        currentAchievement.MinAmount = int.Parse(prefabSplit[1]);
                    }
                    else
                    {
                        currentAchievement.MinAmount = int.Parse(prefabParse);
                    }

                    string[] colorSplit = color.Replace(" ", "").Split(',');
                    currentAchievement.Color = new Color32(byte.Parse(colorSplit[0]), byte.Parse(colorSplit[1]), byte.Parse(colorSplit[2]), 255);
                    currentAchievement.Score = int.Parse(tierParse);
                    Leaderboard_DataTypes.AllAchievements.Add(currentAchievement);
                    currentAchievement = null;
                }
                catch (Exception ex)
                {
                    Utils.print($"Failed to parse achievement {fPath} {currentAchievement?.Name}: {ex.Message}",
                        ConsoleColor.Red);
                    i += 5;
                }
            }
        }

        Leaderboard_DataTypes.AllAchievements =
            Leaderboard_DataTypes.AllAchievements.OrderByDescending(x => x.Score).ToList();
        Leaderboard_DataTypes.SyncedClientAchievements.Value.Clear();
        foreach (Leaderboard_DataTypes.Achievement achievement in Leaderboard_DataTypes.AllAchievements)
        {
            Leaderboard_DataTypes.SyncedClientAchievements.Value.Add(new()
            {
                ID = achievement.ID,
                Name = achievement.Name,
                Description = achievement.Description,
                Color = achievement.Color,
                Score = achievement.Score
            });
        }
    }
    
    private static void ReadAchievementsProfiles()
    {
        Leaderboard_DataTypes.AllAchievements.Clear();
        string folder = Market_Paths.LeaderboardAchievementsFolder;
        string[] files = Directory.GetFiles(folder, "*.cfg", SearchOption.AllDirectories);
        foreach (string file in files)
        {
            IReadOnlyList<string> profiles = File.ReadAllLines(file).ToList();
            ProcessAchievementsProfile(file, profiles);
        }
        Leaderboard_DataTypes.SyncedClientAchievements.Update();
    }

    [UsedImplicitly]
    private static void OnAchievementsFileChange()
    {
        ReadAchievementsProfiles();
        Utils.print("Achievements Changed. Sending new info to all clients");
    }
}