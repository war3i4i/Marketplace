using Marketplace.ExternalLoads;
using Marketplace.Modules.Global_Options;
using Marketplace.Modules.Leaderboard;
using Marketplace.Modules.NPC;
using Marketplace.Modules.Quests;
using Marketplace.Modules.TerritorySystem;
using Marketplace.Paths;
using Marketplace.Stats;

namespace Marketplace;

public static class ChatCommands
{
    [HarmonyPatch(typeof(Terminal), nameof(Terminal.InitTerminal))]
    [ClientOnlyPatch]
    private static class Chat_InputText_Patch_AdminCommands
    {
        private static void Postfix()
        {
            new Terminal.ConsoleCommand("mreloadsounds", "Reloads all sounds", (args) =>
            {
                AssetStorage.ReloadSounds();
                args.Context.AddString("Reloading sounds...");
            });
             
            new Terminal.ConsoleCommand("mreloadimages", "Reloads all sounds", (args) =>
            {
                AssetStorage.ReloadImages(); 
                args.Context.AddString("Reloading images...");
            });
            
            new Terminal.ConsoleCommand("mnpcremove", "Remove NPCs in range of 5 meters", (args) =>
            {
                if (!Utils.IsDebug_Marketplace) return; 
                IEnumerable<Market_NPC.NPCcomponent> FindNPCsInRange = Market_NPC.NPCcomponent.ALL.Where(x =>
                    global::Utils.DistanceXZ(Player.m_localPlayer.transform.position, x.transform.position) <= 5f);
                int c = 0;
                string total = "";
                foreach (Market_NPC.NPCcomponent npc in FindNPCsInRange)
                {
                    ++c;
                    string name = npc.GetClearNPCName();
                    if (string.IsNullOrWhiteSpace(name))
                    {
                        name = Localization.instance.Localize("$mpasn_" + npc._currentNpcType);
                    }

                    total += $"\n{c + 1}) {name} {npc.transform.position}";
                    ZDOMan.instance.m_destroySendList.Add(npc.znv.m_zdo.m_uid);
                }

                args.Context.AddString($"Removed total {c} NPCs in range:{total}");
            });


            new Terminal.ConsoleCommand("idm", "Get Current Left/Right ItemDataManager values", (args) =>
            {
                if (!Utils.IsDebug_Marketplace) return;
                ItemDrop.ItemData weapon = Player.m_localPlayer.GetRightItem() != null
                    ? Player.m_localPlayer.GetRightItem()
                    : Player.m_localPlayer.GetLeftItem();
                if (weapon == null)
                {
                    args.Context.AddString("No weapon in hand");
                    return;
                }

                string IDM = JSON.ToNiceJSON(weapon.m_customData);
                args.Context.AddString($"IDM for {weapon.m_dropPrefab.name}:\n{IDM}");
            });

            new Terminal.ConsoleCommand("mpos", "Get Current Position",
                (args) =>
                {
                    args.Context.AddString($"<color=green>Position: {Player.m_localPlayer.transform.position}</color>");
                });

            new Terminal.ConsoleCommand("mfpslimit", "Set Fixed Update FPS", (args) =>
            {
                if (!Utils.IsDebug_Marketplace || args.Args.Length < 2) return;
                int fps = int.Parse(args.Args[1]);
                fps = Mathf.Clamp(fps, 50, 144);
                float time = 1f / fps;
                Time.fixedDeltaTime = time;
                args.Context.AddString($"<color=green>Fixed Update FPS set to {fps}</color>");
            });

            new Terminal.ConsoleCommand("zonevisualizer", "Toggle zone visualizer", (_) =>
            {
                if (!Utils.IsDebug_Marketplace) return;
                if (ZoneVisualizer.Visualizers.Count == 0)
                    ZoneVisualizer.On();
                else
                    ZoneVisualizer.Off();
            });

            new Terminal.ConsoleCommand("zonevisualizeralpha", "Set Zone Visualizer Alpha", (args) =>
            {
                if (args.Args.Length <= 1)
                {
                    args.Context.AddString("Current Alpha: " + ZoneVisualizer.VisualizerAlpha);
                    return;
                }

                if (!int.TryParse(args.Args[1], out int alpha))
                {
                    args.Context.AddString("Invalid Alpha");
                    return;
                }

                alpha = Mathf.Clamp(alpha, 25, 255);
                ZoneVisualizer.VisualizerAlpha = Mathf.Clamp(alpha, 25, 255) / 255f;
                args.Context.AddString("Set Alpha to: " + alpha);
                if (ZoneVisualizer.Visualizers.Count > 0) ZoneVisualizer.On();
            });

            new Terminal.ConsoleCommand("mquestmarker", "Enable / Disable quest mark", (args) =>
            {
                Quests_Main_Client.ShowQuestMark.Value = !Quests_Main_Client.ShowQuestMark.Value;
                string state = Quests_Main_Client.ShowQuestMark.Value ? "<color=green>Enabled</color>" : "<color=red>Disabled</color>";
                args.Context.AddString($"Quest Mark is now {state}");
                Quests_Main_Client.ShowQuestMark.ConfigFile.Save();
            });
            
            new Terminal.ConsoleCommand("mresetcustomvalues", "Reset custom values", (args) =>
            {
                if (!Utils.IsDebug_Marketplace) return;
                Player.m_localPlayer.m_customData.Clear();
            });

            new Terminal.ConsoleCommand("mcustomvalues", "Show Custom Values", (args) =>
            {
                string values = "<color=yellow>Custom Values:</color>";
                const string start = "kgMarketplaceValue@";
                foreach (KeyValuePair<string, string> pair in Player.m_localPlayer.m_customData.Where(x =>
                             x.Key.Contains(start)))
                {
                    values +=
                        $"\n<color=green>{pair.Key.Replace(start, "").Replace("_", " ")}</color> = <color=yellow>{pair.Value}</color>";
                }
                args.Context.AddString(values);
                CustomValuesShowup.Show = true;
            });
            
            
             new Terminal.ConsoleCommand("mclearallquests", "Clears all quests for player", (args) =>
                {
                    if (!Utils.IsDebug_Marketplace) return;
                    string playerName = "";
                    for (int i = 1; i < args.Args.Length; ++i)
                    {
                        playerName += args.Args[i] + " ";
                    }

                    playerName = playerName.Trim();
                    ZNet.PlayerInfo find = ZNet.instance.m_players.Find(p => p.m_name == playerName);
                    if (find.m_name == null || find.m_userInfo.m_id.m_userID == null)
                    {
                        args.Context.AddString($"<color=red>Player {playerName} not found</color>");
                        return;
                    }

                    ZRoutedRpc.instance.InvokeRoutedRPC(find.m_characterID.UserID, "Marketplace_ResetQuests", new object[] { null });
                    args.Context.AddString($"<color=green>All Quests cleared for {playerName}</color>");
                },
                optionsFetcher: () =>
                {
                    List<string> players = new List<string>();
                    foreach (ZNet.PlayerInfo player in ZNet.instance.m_players)
                    {
                        players.Add(player.m_name);
                    }

                    return players;
                });

            new Terminal.ConsoleCommand("mclearquest", "Clear quest for player", 
                (args) =>
                {
                    if (!Utils.IsDebug_Marketplace) return;
                    string playerName = "";
                    for (int i = 1; i < args.Args.Length - 1; ++i)
                    {
                        playerName += args.Args[i] + " ";
                    }

                    playerName = playerName.Trim();
                    ZNet.PlayerInfo find = ZNet.instance.m_players.Find(p => p.m_name == playerName);
                    if (find.m_name == null || find.m_userInfo.m_id.m_userID == null)
                    {
                        args.Context.AddString($"<color=red>Player {playerName} not found</color>");
                        return;
                    }
 
                    string questUID = args.Args[args.Args.Length - 1];
                    ZRoutedRpc.instance.InvokeRoutedRPC(find.m_characterID.UserID, "Marketplace_ResetQuest", new object[] { questUID });
                    args.Context.AddString($"<color=green>Quest {questUID} cleared for {playerName}</color>");
                },
                optionsFetcher: () =>
                {
                    List<string> players = new List<string>();
                    foreach (ZNet.PlayerInfo player in ZNet.instance.m_players)
                    {
                        players.Add(player.m_name);
                    }

                    return players;
                });
            
            
            new Terminal.ConsoleCommand("maddterritory", "Add Territory", (args) =>
            {
                if (!Utils.IsDebug_Strict) return;
                TerritorySystem_DataTypes.TerritoryShape shape = (TerritorySystem_DataTypes.TerritoryShape)Enum.Parse(typeof(TerritorySystem_DataTypes.TerritoryShape), args.Args[1], true);
                int x = int.Parse(args.Args[2]);
                int z = int.Parse(args.Args[3]);
                int radius = int.Parse(args.Args[4]);
                byte r = byte.Parse(args.Args[5]);
                byte g = byte.Parse(args.Args[6]);
                byte b = byte.Parse(args.Args[7]);
                int price = int.Parse(args.Args[8]);
                ZPackage pkg = new();
                pkg.Write((int)shape); pkg.Write(x); pkg.Write(z); pkg.Write(radius); pkg.Write(r); pkg.Write(g); pkg.Write(b); pkg.Write(price);
                ZRoutedRpc.instance.InvokeRoutedRPC("KGmarket TerritorySystem AddTerritory", pkg);
            });
            
        }
    }
    
    [HarmonyPatch(typeof(ZNetScene), nameof(ZNetScene.Awake))]
    [ClientOnlyPatch]
    private static class ZNetScene_Awake_Patch
    {
        private static void Postfix()
        {
            ZRoutedRpc.instance.Register("Marketplace_ResetQuests", (_) =>
            {
                foreach (string quest in Player.m_localPlayer.m_customData.Where(d => d.Key.Contains("[MPASN]quest="))
                             .Select(d => d.Key).ToList())
                {
                    Player.m_localPlayer.m_customData.Remove(quest);
                    Utils.print($"Cleared quest data: {quest.Replace("[MPASN]quest=", "")}");
                }
                foreach (string quest in Player.m_localPlayer.m_customData.Where(d => d.Key.Contains("[MPASN]questCD="))
                             .Select(d => d.Key).ToList())
                {
                    Player.m_localPlayer.m_customData.Remove(quest);
                    Utils.print($"Cleared quest CD data: {quest.Replace("[MPASN]quest=", "")}");
                }
                Quests_DataTypes.AcceptedQuests.Clear();
                Quests_UIs.AcceptedQuestsUI.CheckQuests();
            });

            ZRoutedRpc.instance.Register<string>("Marketplace_ResetQuest", (_, questUID) =>
            {
                Utils.print($"Quest: {questUID} removed");
                int toID = questUID.ToLower().GetStableHashCode();
                Player.m_localPlayer.m_customData.Remove($"[MPASN]quest={toID}");
                Player.m_localPlayer.m_customData.Remove($"[MPASN]questCD={toID}");
                Quests_DataTypes.AcceptedQuests.Remove(toID);
                Quests_UIs.AcceptedQuestsUI.CheckQuests();
            });
        }
    }
}