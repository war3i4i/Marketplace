using System.Net;
using System.Threading.Tasks;
using API;
using BepInEx.Bootstrap;
using Guilds;
using Marketplace.ExternalLoads;
using Marketplace.Modules.Global_Options;
using Marketplace.Modules.Leaderboard;
using Marketplace.Modules.NPC;
using Marketplace.Modules.Quests;
using Marketplace.OtherModsAPIs;
using Splatform;
using YamlDotNet.Serialization;
using Random = UnityEngine.Random;

namespace Marketplace.Modules.Dialogues;

public static class Dialogues_DataTypes
{
    internal static readonly CustomSyncedValue<List<RawDialogue>> SyncedDialoguesData = new(Marketplace.configSync, "dialoguesData", new List<RawDialogue>());

    internal static readonly Dictionary<string, Dialogue> ClientReadyDialogues = new();

    public delegate bool Dialogue_Condition(Market_NPC.NPCcomponent npc, out string reason, out OptionCondition typeCondition);

    [Serializable]
    public class CustomSpawnZDO : ISerializableParameter
    {
        public Dictionary<string, int> Ints;
        public Dictionary<string, float> Floats;
        public Dictionary<string, long> Longs;
        public Dictionary<string, string> Strings;
        public Dictionary<string, bool> Bools;

        public void Serialize(ref ZPackage pkg)
        {
            pkg.Write(Ints != null);
            if (Ints != null)
            {
                pkg.Write(Ints.Count);
                foreach (var pair in Ints)
                {
                    pkg.Write(pair.Key);
                    pkg.Write(pair.Value);
                }
            }

            pkg.Write(Floats != null);
            if (Floats != null)
            {
                pkg.Write(Floats.Count);
                foreach (var pair in Floats)
                {
                    pkg.Write(pair.Key);
                    pkg.Write(pair.Value);
                }
            }

            pkg.Write(Strings != null);
            if (Strings != null)
            {
                pkg.Write(Strings.Count);
                foreach (var pair in Strings)
                {
                    pkg.Write(pair.Key);
                    pkg.Write(pair.Value);
                }
            }

            pkg.Write(Bools != null);
            if (Bools != null)
            {
                pkg.Write(Bools.Count);
                foreach (var pair in Bools)
                {
                    pkg.Write(pair.Key);
                    pkg.Write(pair.Value);
                }
            }

            pkg.Write(Longs != null);
            if (Longs != null)
            {
                pkg.Write(Longs.Count);
                foreach (var pair in Longs)
                {
                    pkg.Write(pair.Key);
                    pkg.Write(pair.Value);
                }
            }
        }

        public void Deserialize(ref ZPackage pkg)
        {
            if (pkg.ReadBool())
            {
                int count = pkg.ReadInt();
                Ints = new Dictionary<string, int>(count);
                for (int i = 0; i < count; ++i)
                {
                    string key = pkg.ReadString();
                    int value = pkg.ReadInt();
                    Ints[key] = value;
                }
            }

            if (pkg.ReadBool())
            {
                int count = pkg.ReadInt();
                Floats = new Dictionary<string, float>(count);
                for (int i = 0; i < count; ++i)
                {
                    string key = pkg.ReadString();
                    float value = pkg.ReadSingle();
                    Floats[key] = value;
                }
            }

            if (pkg.ReadBool())
            {
                int count = pkg.ReadInt();
                Strings = new Dictionary<string, string>(count);
                for (int i = 0; i < count; ++i)
                {
                    string key = pkg.ReadString();
                    string value = pkg.ReadString();
                    Strings[key] = value;
                }
            }

            if (pkg.ReadBool())
            {
                int count = pkg.ReadInt();
                Bools = new Dictionary<string, bool>(count);
                for (int i = 0; i < count; ++i)
                {
                    string key = pkg.ReadString();
                    bool value = pkg.ReadBool();
                    Bools[key] = value;
                }
            }

            if (pkg.ReadBool())
            {
                int count = pkg.ReadInt();
                Longs = new Dictionary<string, long>(count);
                for (int i = 0; i < count; ++i)
                {
                    string key = pkg.ReadString();
                    long value = pkg.ReadLong();
                    Longs[key] = value;
                }
            }
        }
    }

    public static CustomSyncedValue<Dictionary<string, CustomSpawnZDO>> CustomSpawnZDOData_Synced =
        new(Marketplace.configSync, "customSpawnZDOData", new Dictionary<string, CustomSpawnZDO>());


    private enum OptionCommand
    {
        OpenUI,
        PlaySound,
        GiveQuest,
        RemoveQuest,
        FinishQuest,
        GiveItem,
        GiveItemWithData,
        SetPlayerData,
        RemoveItem,
        Spawn,
        SpawnXYZ,
        SpawnWithData,
        SpawnXYZWithData,
        Teleport,
        Damage,
        Heal,
        GiveBuff,
        AddPin,
        PingMap,
        AddEpicMMOExp,
        AddCozyheimExp,
        PlayAnimation,
        EnterPassword,
        GuildAddLevel,
        Battlepass_EXP,
        ConsoleCommand,
        AddCustomValue,
        SetCustomValue,
        PlayVideo,
        AddRustyClassesEXP,
        SendWebhook,
        AddPlayerKey,
        RemovePlayerKey,
        SetPlayerGuild,
        SetPlayerGuildRank,
        SetNPCModelLocal,
        SetNPCModelGlobal,
        SetNPCNameLocal,
        SetNPCNameGlobal,
        SetNPCPatrol,
        SpawnRaw,
    }

    private const byte reverseFlag = 1 << 7;

    private static OptionCondition Reverse(this OptionCondition condition) =>
        (OptionCondition)((byte)condition ^ reverseFlag);

    public enum OptionCondition : byte
    {
        None = 0,
        HasItem = 1,
        NotHasItem = 1 | reverseFlag,
        HasBuff = 2,
        NotHasBuff = 2 | reverseFlag,
        SkillMore = 3,
        SkillLess = 3 | reverseFlag,
        GlobalKey = 4,
        NotGlobalKey = 4 | reverseFlag,
        HasQuest = 5,
        NotHasQuest = 5 | reverseFlag,
        QuestProgressDone = 6,
        QuestProgressNotDone = 6 | reverseFlag,
        QuestFinished = 7,
        QuestNotFinished = 7 | reverseFlag,
        EpicMMOLevelMore = 8,
        EpicMMOLevelLess = 8 | reverseFlag,
        CozyheimLevelMore = 9,
        CozyheimLevelLess = 9 | reverseFlag,
        HasAchievement = 10,
        NotHasAchievement = 10 | reverseFlag,
        HasAchievementScore = 11,
        NotHasAchievementScore = 11 | reverseFlag,
        CustomValueMore = 12,
        CustomValueLess = 12 | reverseFlag,
        ModInstalled = 13,
        NotModInstalled = 13 | reverseFlag,
        IronGateStatMore = 14,
        IronGateStatLess = 14 | reverseFlag,
        HasGuild = 15,
        NotHasGuild = 15 | reverseFlag,
        HasGuildWithName = 16,
        NotHasGuildWithName = 16 | reverseFlag,
        GuildLevelMore = 17,
        GuildLevelLess = 17 | reverseFlag,
        GuildHasAchievement = 18,
        GuildNotHasAchievement = 18 | reverseFlag,
        MHLevelMore = 19,
        MHLevelLess = 19 | reverseFlag,
        IsVIP = 20,
        NotIsVIP = 20 | reverseFlag,
        RustyClassesLevelMore = 21,
        RustyClassesLevelLess = 21 | reverseFlag,
        HasPlayerKey = 22,
        NotHasPlayerKey = 22 | reverseFlag,
        NPCModelEquals = 23,
        NotNPCModelEquals = 23 | reverseFlag,
        HealthMore = 24,
        HealthLess = 24 | reverseFlag,
        MaxHealthMore = 25,
        MaxHealthLess = 25 | reverseFlag,
        StaminaMore = 26,
        StaminaLess = 26 | reverseFlag,
        MaxStaminaMore = 27,
        MaxStaminaLess = 27 | reverseFlag,
        EitrMore = 28,
        EitrLess = 28 | reverseFlag,
        MaxEitrMore = 29,
        MaxEitrLess = 29 | reverseFlag,
        PlayerHasAllCustomDataKeys = 30,
        PlayerHasOneOfCustomDataKeys = 31,
        NPCNameEquals = 32,
        NotNPCNameEquals = 32 | reverseFlag,

        /* Old quest aliases (backwards compat) */
        OtherQuest = 100,
        NotFinished = 101,  

        Skill = 102
        /* ____________________________________ */
    }

    public class RawDialogue : ISerializableParameter
    {
        [YamlMember(Alias = "Name")] public string UID;
        [YamlMember(Alias = "Text")] public string Text;
        [YamlMember(Alias = "Background Image Link")] public string BG_Image;
        [YamlMember(Alias = "Interactions")] public RawPlayerOption[] Interactions = Array.Empty<RawPlayerOption>();

        public enum InteractionType
        {
            Option,
            Interaction,
            InRange,
            OutRange
        }

        public class RawPlayerOption
        {
            public InteractionType Type;
            public string Text;
            public string Icon;
            [YamlMember(Alias = "TransitionTo")] public string NextUID;
            public string[] RandomTransition = Array.Empty<string>();
            public string[] Commands = Array.Empty<string>();
            public List<KeyValuePair<int, string>> RandomCommands = [];
            public string[] Conditions = Array.Empty<string>();
            public bool AlwaysVisible = true;
            public Color32 Color = new Color32(255, 255, 255, 255);
            public string OverrideError;
        }

        public void Serialize(ref ZPackage pkg)
        {
            pkg.Write(UID ?? "default");
            pkg.Write(Text ?? "");
            pkg.Write(BG_Image ?? "");
            pkg.Write(Interactions.Length);
            foreach (RawPlayerOption option in Interactions)
            {
                pkg.Write((int)option.Type);
                pkg.Write(option.Text ?? "");
                pkg.Write(option.Icon ?? "");
                pkg.Write(option.NextUID ?? ""); 
                pkg.Write(option.RandomTransition.Length);
                foreach (string transition in option.RandomTransition) pkg.Write(transition);
                pkg.Write(option.Commands.Length);
                foreach (string command in option.Commands)
                {
                    pkg.Write(command);
                }

                pkg.Write(option.RandomCommands.Count);
                foreach (var pair in option.RandomCommands)
                {
                    pkg.Write(pair.Key);
                    pkg.Write(pair.Value);
                }

                pkg.Write(option.Conditions.Length);
                foreach (string condition in option.Conditions)
                {
                    pkg.Write(condition);
                }

                pkg.Write(option.AlwaysVisible);
                pkg.Write(global::Utils.ColorToVec3(option.Color));
                pkg.Write(option.OverrideError ?? "");
            }
        }

        public void Deserialize(ref ZPackage pkg)
        {
            UID = pkg.ReadString();
            Text = pkg.ReadString();
            BG_Image = pkg.ReadString();
            int optionsLength = pkg.ReadInt();
            Interactions = new RawPlayerOption[optionsLength];
            for (int i = 0; i < optionsLength; ++i)
            {
                Interactions[i] = new RawPlayerOption
                {
                    Type = (InteractionType)pkg.ReadInt(),
                    Text = pkg.ReadString(),
                    Icon = pkg.ReadString(),
                    NextUID = pkg.ReadString(),
                    RandomTransition = new string[pkg.ReadInt()],
                };
                for (int j = 0; j < Interactions[i].RandomTransition.Length; ++j)
                {
                    Interactions[i].RandomTransition[j] = pkg.ReadString();
                }

                int commandsLength = pkg.ReadInt();
                Interactions[i].Commands = new string[commandsLength];
                for (int j = 0; j < commandsLength; ++j)
                {
                    Interactions[i].Commands[j] = pkg.ReadString();
                }

                int randomCommandsLength = pkg.ReadInt();
                Interactions[i].RandomCommands = new List<KeyValuePair<int, string>>(randomCommandsLength);
                for (int j = 0; j < randomCommandsLength; ++j)
                {
                    int key = pkg.ReadInt();
                    string value = pkg.ReadString();
                    Interactions[i].RandomCommands.Add(new KeyValuePair<int, string>(key, value));
                }

                int conditionsLength = pkg.ReadInt();
                Interactions[i].Conditions = new string[conditionsLength];
                for (int j = 0; j < conditionsLength; ++j)
                {
                    Interactions[i].Conditions[j] = pkg.ReadString();
                }

                Interactions[i].AlwaysVisible = pkg.ReadBool();
                Interactions[i].Color = global::Utils.Vec3ToColor(pkg.ReadVector3());
                Interactions[i].OverrideError = pkg.ReadString();
            }
        }
    }

    public class Dialogue
    {
        public string Text;
        public Sprite BG_Image = null!;
        public PlayerInteraction[] Interactions = Array.Empty<PlayerInteraction>();

        public class PlayerInteraction
        {
            public RawDialogue.InteractionType Type;
            public string Text;
            public Sprite Icon = null!;
            public string NextUID;
            public string[] RandomTransition;
            public Action<Market_NPC.NPCcomponent> Command;
            public Dictionary<int, Action<Market_NPC.NPCcomponent>> RandomCommands;
            public Dialogue_Condition Condition;
            public bool AlwaysVisible;
            public Color Color = Color.white;
            public string OverrideError;

            public string NextTransition
            {
                get
                {
                    if (!string.IsNullOrWhiteSpace(NextUID)) return NextUID;
                    if (RandomTransition.Length == 0) return null;
                    string random = RandomTransition[Random.Range(0, RandomTransition.Length)];
                    return string.IsNullOrWhiteSpace(random) ? null : random;
                }
            }

            public void CallCommands(Market_NPC.NPCcomponent npc)
            {
                if (Command != null) Command(npc);
                if (RandomCommands != null && RandomCommands.Count > 0)
                {
                    foreach (KeyValuePair<int, Action<Market_NPC.NPCcomponent>> pair in RandomCommands)
                    {
                        int random = Random.Range(0, 100);
                        if (random < pair.Key && Player.m_localPlayer) pair.Value?.Invoke(npc);
                    }
                }
            }

            public bool CheckCondition(Market_NPC.NPCcomponent npc, out string reason)
            {
                reason = "";
                if (Condition == null) return true;
                foreach (Dialogue_Condition cast in Condition.GetInvocationList().Cast<Dialogue_Condition>())
                {
                    if (!cast(npc, out reason, out _))
                    {
                        if (!string.IsNullOrWhiteSpace(OverrideError)) reason = OverrideError;
                        return false;
                    }
                }

                return true;
            }
        }

        public static Action<Market_NPC.NPCcomponent> ParseCommands(IEnumerable<string> commands)
        {
            Action<Market_NPC.NPCcomponent> result = null;
            foreach (string command in commands)
            {
                try
                {
                    string[] split = command.Replace(":", ",").Split(',');
                    if (Enum.TryParse(split[0], true, out OptionCommand optionCommand))
                    {
                        switch (optionCommand)
                        {
                            case OptionCommand.AddPlayerKey:
                                result += (_) =>
                                {
                                    string key = split[1];
                                    Player.m_localPlayer.AddUniqueKey(key);
                                };
                                break;
                            case OptionCommand.RemovePlayerKey:
                                result += (_) =>
                                {
                                    string key = split[1];
                                    Player.m_localPlayer.RemoveUniqueKey(key);
                                };
                                break;
                            case OptionCommand.GuildAddLevel:
                                result += (_) =>
                                {
                                    if (Guilds.API.GetOwnGuild() is not { } g) return;
                                    g.General.level += int.Parse(split[1]);
                                    Guilds.API.SaveGuild(g);
                                };
                                break;
                            case OptionCommand.EnterPassword:
                                result += (npc) =>
                                {
                                    string title = split[1];
                                    string password = split[2];
                                    string onSuccess = split[3];
                                    string onFail = split[4];
                                    new DialoguePassword(npc, title, password, onSuccess, onFail);
                                };
                                break;
                            case OptionCommand.PlayAnimation:
                                result += (npc) => { npc?.zanim.SetTrigger(split[1]); };
                                break;
                            case OptionCommand.OpenUI:
                                result += (npc) =>
                                {
                                    string uitype = split.Length > 1 ? split[1] : null;
                                    string profile = split.Length > 2 ? split[2] : null;
                                    if (npc)
                                    {
                                        if (string.IsNullOrWhiteSpace(uitype))
                                            uitype = npc._currentNpcType.ToString();
                                        if (string.IsNullOrWhiteSpace(profile))
                                            profile = npc.znv.m_zdo.GET_NPC_Profile();
                                        string npcName = npc.znv.m_zdo.GetString("KGnpcNameOverride");
                                        Market_NPC.NPCcomponent.OpenUIForType(uitype, profile, npcName);
                                    }
                                    else
                                    {
                                        Market_NPC.NPCcomponent.OpenUIForType(uitype, profile, "");
                                    }
                                };
                                break;
                            case OptionCommand.SetPlayerGuild:
                                result += (_) =>
                                {
                                    PlayerReference playerRef = PlayerReference.forOwnPlayer();
                                    Guilds.API.RemovePlayerFromGuild(playerRef);
                                    string arg = split[1];
                                    Guild guild = null;
                                    if (int.TryParse(arg, out int id))
                                        guild = Guilds.API.GetGuild(id);
                                    else
                                        guild = Guilds.API.GetGuild(arg);
                                    if (guild != null)
                                    {
                                        Guilds.API.AddPlayerToGuild(playerRef, guild);
                                        Guilds.API.SaveGuild(guild);
                                    }
                                };
                                break;
                            case OptionCommand.SetNPCModelLocal:
                                result += (npc) =>
                                {
                                    if (!npc) return;
                                    string prefab = split[1];
                                    if (!ZNetScene.instance.GetPrefab(prefab)) return;
                                    npc.OverrideModelLocal(prefab);
                                };
                                break;
                            case OptionCommand.SetNPCNameLocal:
                                result += (npc) =>
                                {
                                    if (!npc) return;
                                    string name = split[1].Replace("_", " ");
                                    npc.ApplyName(name);
                                };
                                break;
                            case OptionCommand.SetNPCNameGlobal:
                                result += (npc) =>
                                {
                                    if (!npc) return;
                                    string name = split[1].Replace("_", " ");
                                    ZRoutedRpc.instance.InvokeRoutedRPC(ZRoutedRpc.Everybody, npc.znv.m_zdo.m_uid, "KGmarket overridename", name);
                                };
                                break;
                            case OptionCommand.SetNPCPatrol:
                                result += (npc) =>
                                {
                                    if (!npc) return;
                                    string patrol = split[1];
                                    ZRoutedRpc.instance.InvokeRoutedRPC(ZRoutedRpc.Everybody, npc.znv.m_zdo.m_uid, "KGmarket GetPatrolData", patrol);
                                };
                                break;
                            case OptionCommand.SetNPCModelGlobal:
                                result += (npc) =>
                                {
                                    if (!npc) return;
                                    string prefab = split[1];
                                    if (!ZNetScene.instance.GetPrefab(prefab)) return;
                                    ZRoutedRpc.instance.InvokeRoutedRPC(ZRoutedRpc.Everybody, npc.znv.m_zdo.m_uid, "KGmarket overridemodel", prefab);
                                };
                                break;
                            case OptionCommand.SetPlayerGuildRank:
                                result += (_) =>
                                {
                                    Guild ownGuild = Guilds.API.GetOwnGuild();
                                    if (ownGuild == null) return;
                                    PlayerReference playerRef = PlayerReference.forOwnPlayer();
                                    Ranks? rank = Enum.TryParse(split[1], true, out Ranks r) ? r : null;
                                    if (rank == null) return;
                                    Guilds.API.UpdatePlayerRank(playerRef, rank.Value);
                                };
                                break;
                            case OptionCommand.PlaySound:
                                result += (npc) =>
                                {
                                    if (!AssetStorage.NPC_AudioClips.TryGetValue(split[1], out AudioClip clip)) return;
                                    if (npc)
                                    {
                                        npc.NPC_SoundSource.Stop();
                                        npc.NPC_SoundSource.clip = clip;
                                        npc.NPC_SoundSource.Play();
                                    }
                                    else
                                    {
                                        AssetStorage.AUsrc.PlayOneShot(clip);
                                    }
                                };
                                break;
                            case OptionCommand.PlayVideo:
                                result += (_) =>
                                {
                                    float multiplier = split.Length > 2 ? int.Parse(split[2]) / 100f : 0.5f;
                                    VideoPlayerUI.StartVideo(split[1], multiplier);
                                };
                                break;
                            case OptionCommand.GiveQuest:
                                result += (_) =>
                                {
                                    string questName = split[1].ToLower();
                                    Quests_DataTypes.Quest.AcceptQuest(questName.GetStableHashCode(), handleEvent: false);
                                    Quests_UIs.AcceptedQuestsUI.CheckQuests();
                                };
                                break;
                            case OptionCommand.ConsoleCommand:
                                result += (_) =>
                                {
                                    split[1] = split[1].Replace("{playername}", Game.instance.m_playerProfile.m_playerName);
                                    if (Terminal.commands.TryGetValue(split[1].Split([' '], StringSplitOptions.None)[0].ToLower(), out Terminal.ConsoleCommand consoleCommand))
                                    {
                                        Player.m_debugMode = true;
                                        consoleCommand.RunAction(new Terminal.ConsoleEventArgs(split[1], Console.instance));
                                        Player.m_debugMode = false;
                                    }
                                };
                                break;
                            case OptionCommand.Battlepass_EXP:
                                result += (_) =>
                                {
                                    Type bp_t = System.Type.GetType("UL_B.Battlepass_Main_Client, UL_B");
                                    if (bp_t != null)
                                    {
                                        MethodInfo m = AccessTools.Method(bp_t, "AddExp");
                                        if (m != null)
                                        {
                                            m.Invoke(null, new object[] { Convert.ToInt32(split[1]) });
                                        }
                                    }
                                };
                                break;
                            case OptionCommand.SetCustomValue:
                                result += (_) =>
                                {
                                    string key = split[1];
                                    int value = int.Parse(split[2]);
                                    if (value != 0)
                                        Player.m_localPlayer.SetCustomValue(key, value);
                                    else
                                        Player.m_localPlayer.RemoveCustomValue(key);
                                };
                                break;
                            case OptionCommand.AddCustomValue:
                                result += (_) =>
                                {
                                    string key = split[1];
                                    int value = int.Parse(split[2]);
                                    Player.m_localPlayer.AddCustomValue(key, value);
                                };
                                break;
                            case OptionCommand.GiveItem:
                                result += (_) =>
                                {
                                    string itemPrefab = split[1];
                                    GameObject prefab = ZNetScene.instance.GetPrefab(itemPrefab);
                                    if (!prefab || !prefab.GetComponent<ItemDrop>()) return;
                                    int amount = int.Parse(split[2]);
                                    int level = int.Parse(split[3]);
                                    Utils.AddItemToPlayer(prefab, amount, level);
                                };
                                break;
                            case OptionCommand.GiveItemWithData:
                                result += (_) =>
                                {
                                    string itemPrefab = split[1];
                                    GameObject prefab = ZNetScene.instance.GetPrefab(itemPrefab);
                                    if (!prefab || !prefab.GetComponent<ItemDrop>()) return;
                                    int amount = int.Parse(split[2]);
                                    int level = int.Parse(split[3]);
                                    string dataKey = split[4].ToLower();
                                    if (!Dialogues_DataTypes.CustomSpawnZDOData_Synced.Value.TryGetValue(dataKey, out CustomSpawnZDO customData_Item))
                                    {
                                        Utils.print($"No custom spawn data found for key {dataKey}", ConsoleColor.DarkRed);
                                        return;
                                    }

                                    Utils.AddItemToPlayer(prefab, amount, level, customData_Item);
                                };
                                break;
                            case OptionCommand.SetPlayerData:
                                result += (_) =>
                                {
                                    string dataKey = split[1].ToLower();
                                    if (!Dialogues_DataTypes.CustomSpawnZDOData_Synced.Value.TryGetValue(dataKey, out CustomSpawnZDO customData_Player))
                                    {
                                        Utils.print($"No custom spawn data found for key {dataKey}", ConsoleColor.DarkRed);
                                        return;
                                    }

                                    Utils.print(JSON.ToNiceJSON(customData_Player));
                                    if (customData_Player.Ints != null)
                                        foreach (var zdoI in customData_Player.Ints)
                                            Player.m_localPlayer.m_nview.m_zdo.Set(zdoI.Key, zdoI.Value);
                                    if (customData_Player.Floats != null)
                                        foreach (var zdoF in customData_Player.Floats)
                                            Player.m_localPlayer.m_nview.m_zdo.Set(zdoF.Key, zdoF.Value);
                                    if (customData_Player.Strings != null)
                                        foreach (var zdoS in customData_Player.Strings)
                                            Player.m_localPlayer.m_nview.m_zdo.Set(zdoS.Key, zdoS.Value);
                                    if (customData_Player.Bools != null)
                                        foreach (var zdoB in customData_Player.Bools)
                                            Player.m_localPlayer.m_nview.m_zdo.Set(zdoB.Key, zdoB.Value);
                                    if (customData_Player.Longs != null)
                                        foreach (var zdoL in customData_Player.Longs)
                                            Player.m_localPlayer.m_nview.m_zdo.Set(zdoL.Key, zdoL.Value);
                                };
                                break;
                            case OptionCommand.RemoveItem:
                                result += (_) => { Utils.CustomRemoveItems(split[1], int.Parse(split[2]), 1); };
                                break;
                            case OptionCommand.Spawn or OptionCommand.SpawnRaw:
                                result += (_) =>
                                {
                                    string spawnPrefab = split[1];
                                    GameObject spawn = ZNetScene.instance.GetPrefab(spawnPrefab);
                                    if (!spawn) return;
                                    Vector3 spawnPos = Player.m_localPlayer.transform.position;
                                    int spawnAmount = int.Parse(split[2]);
                                    int spawnLevel = int.Parse(split[3]);
                                    for (int i = 0; i < spawnAmount; ++i)
                                    {
                                        float randomX = Random.Range(-15, 15);
                                        float randomZ = Random.Range(-15, 15);
                                        Vector3 randomPos = new Vector3(spawnPos.x + randomX, spawnPos.y, spawnPos.z + randomZ);
                                        Utils.CustomFindFloor(randomPos, out randomPos.y, 3f);
                                        GameObject newSpawn = UnityEngine.Object.Instantiate(spawn, randomPos, Quaternion.identity);

                                        if (newSpawn.GetComponent<Character>() is { } c) c.SetLevel(Mathf.Max(1, spawnLevel + 1));
                                        if (newSpawn.GetComponent<ItemDrop>() is { } item) item.SetQuality(spawnLevel);
                                    }
                                };
                                break;
                            case OptionCommand.SpawnWithData:
                                result += (_) =>
                                {
                                    string spawnPrefab = split[1];
                                    GameObject spawn = ZNetScene.instance.GetPrefab(spawnPrefab);
                                    if (!spawn) return;
                                    Vector3 spawnPos = Player.m_localPlayer.transform.position;
                                    int spawnAmount = int.Parse(split[2]);
                                    int spawnLevel = int.Parse(split[3]);
                                    string dataKey = split[4].ToLower();
                                    if (!Dialogues_DataTypes.CustomSpawnZDOData_Synced.Value.TryGetValue(dataKey, out CustomSpawnZDO customData))
                                    {
                                        Utils.print($"No custom spawn data found for key {dataKey}", ConsoleColor.DarkRed);
                                        return;
                                    }

                                    for (int i = 0; i < spawnAmount; ++i)
                                    {
                                        float randomX = Random.Range(-15, 15);
                                        float randomZ = Random.Range(-15, 15);
                                        Vector3 randomPos = new Vector3(spawnPos.x + randomX, spawnPos.y, spawnPos.z + randomZ);
                                        Utils.CustomFindFloor(randomPos, out randomPos.y, 3f);
                                        GameObject newSpawn = UnityEngine.Object.Instantiate(spawn, randomPos, Quaternion.identity);
                                        if (newSpawn.GetComponent<ZNetView>() is { } znvSpawn)
                                        {
                                            if (customData.Ints != null)
                                                foreach (var zdoI in customData.Ints)
                                                    znvSpawn.m_zdo.Set(zdoI.Key, zdoI.Value);
                                            if (customData.Floats != null)
                                                foreach (var zdoF in customData.Floats)
                                                    znvSpawn.m_zdo.Set(zdoF.Key, zdoF.Value);
                                            if (customData.Strings != null)
                                                foreach (var zdoS in customData.Strings)
                                                    znvSpawn.m_zdo.Set(zdoS.Key, zdoS.Value);
                                            if (customData.Bools != null)
                                                foreach (var zdoB in customData.Bools)
                                                    znvSpawn.m_zdo.Set(zdoB.Key, zdoB.Value);
                                            if (customData.Longs != null)
                                                foreach (var zdoL in customData.Longs)
                                                    znvSpawn.m_zdo.Set(zdoL.Key, zdoL.Value);
                                            if (newSpawn.GetComponent<Character>() is { } c) c.SetLevel(Mathf.Max(1, spawnLevel + 1));
                                            if (newSpawn.GetComponent<ItemDrop>() is { } item)
                                            {
                                                item.SetQuality(spawnLevel);
                                                item.LoadFromExternalZDO(znvSpawn.m_zdo);
                                            }
                                        }
                                    }
                                };
                                break;
                            case OptionCommand.SpawnXYZ:
                                result += (_) =>
                                {
                                    string spawnPrefab = split[1];
                                    GameObject spawn = ZNetScene.instance.GetPrefab(spawnPrefab);
                                    if (!spawn) return;
                                    int spawnAmount = int.Parse(split[2]);
                                    int spawnLevel = int.Parse(split[3]);
                                    Vector3 spawnPoint = new Vector3(int.Parse(split[4]), int.Parse(split[5]), int.Parse(split[6]));
                                    int maxDistance = int.Parse(split[7]);
                                    for (int i = 0; i < spawnAmount; ++i)
                                    {
                                        float randomX = Random.Range(-maxDistance, maxDistance);
                                        float randomZ = Random.Range(-maxDistance, maxDistance);
                                        Vector3 randomPos = new Vector3(spawnPoint.x + randomX, spawnPoint.y, spawnPoint.z + randomZ);
                                        GameObject newSpawn = UnityEngine.Object.Instantiate(spawn, randomPos, Quaternion.identity);
                                        if (newSpawn.GetComponent<Character>() is { } c) c.SetLevel(Mathf.Max(1, spawnLevel + 1));
                                        if (newSpawn.GetComponent<ItemDrop>() is { } item) item.SetQuality(spawnLevel);
                                    }
                                };
                                break;
                            case OptionCommand.SpawnXYZWithData:
                                result += (_) =>
                                {
                                    string spawnPrefab = split[1];
                                    GameObject spawn = ZNetScene.instance.GetPrefab(spawnPrefab);
                                    if (!spawn) return;
                                    int spawnAmount = int.Parse(split[2]);
                                    int spawnLevel = int.Parse(split[3]);
                                    Vector3 spawnPoint = new Vector3(int.Parse(split[4]), int.Parse(split[5]), int.Parse(split[6]));
                                    int maxDistance = int.Parse(split[7]);
                                    string dataKey = split[8].ToLower();
                                    if (!Dialogues_DataTypes.CustomSpawnZDOData_Synced.Value.TryGetValue(dataKey, out CustomSpawnZDO customData))
                                    {
                                        Utils.print($"No custom spawn data found for key {dataKey}", ConsoleColor.DarkRed);
                                        return;
                                    }

                                    for (int i = 0; i < spawnAmount; ++i)
                                    {
                                        float randomX = Random.Range(-maxDistance, maxDistance);
                                        float randomZ = Random.Range(-maxDistance, maxDistance);
                                        Vector3 randomPos = new Vector3(spawnPoint.x + randomX, spawnPoint.y, spawnPoint.z + randomZ);
                                        GameObject newSpawn = UnityEngine.Object.Instantiate(spawn, randomPos, Quaternion.identity);
                                        if (newSpawn.GetComponent<ZNetView>() is { } znvSpawn)
                                        {
                                            if (customData.Ints != null)
                                                foreach (var zdoI in customData.Ints)
                                                    znvSpawn.m_zdo.Set(zdoI.Key, zdoI.Value);
                                            if (customData.Floats != null)
                                                foreach (var zdoF in customData.Floats)
                                                    znvSpawn.m_zdo.Set(zdoF.Key, zdoF.Value);
                                            if (customData.Strings != null)
                                                foreach (var zdoS in customData.Strings)
                                                    znvSpawn.m_zdo.Set(zdoS.Key, zdoS.Value);
                                            if (customData.Bools != null)
                                                foreach (var zdoB in customData.Bools)
                                                    znvSpawn.m_zdo.Set(zdoB.Key, zdoB.Value);
                                            if (customData.Longs != null)
                                                foreach (var zdoL in customData.Longs)
                                                    znvSpawn.m_zdo.Set(zdoL.Key, zdoL.Value);
                                            if (newSpawn.GetComponent<Character>() is { } c) c.SetLevel(Mathf.Max(1, spawnLevel + 1));
                                            if (newSpawn.GetComponent<ItemDrop>() is { } item)
                                            {
                                                item.SetQuality(spawnLevel);
                                                item.LoadFromExternalZDO(znvSpawn.m_zdo);
                                            }
                                        }
                                    }
                                };
                                break;
                            case OptionCommand.Teleport:
                                result += (_) =>
                                {
                                    bool teleportWithOre = split.Length > 4 && bool.Parse(split[4]);
                                    if (!teleportWithOre && !Player.m_localPlayer.IsTeleportable())
                                    {
                                        MessageHud.instance.ShowMessage(MessageHud.MessageType.Center, "$msg_noteleport".Localize());
                                        return;
                                    }

                                    int x = int.Parse(split[1]);
                                    int y = int.Parse(split[2]);
                                    int z = int.Parse(split[3]);
                                    Player.m_localPlayer.TeleportTo(new Vector3(x, y, z),
                                        Player.m_localPlayer.transform.rotation,
                                        true);
                                };
                                break;
                            case OptionCommand.Damage:
                                result += (_) =>
                                {
                                    int damage = int.Parse(split[1]);
                                    HitData hitData = new HitData();
                                    hitData.m_damage.m_damage = damage;
                                    Player.m_localPlayer.Damage(hitData);
                                };
                                break;
                            case OptionCommand.Heal:
                                result += (_) =>
                                {
                                    int heal = int.Parse(split[1]);
                                    Player.m_localPlayer.Heal(heal);
                                };
                                break;
                            case OptionCommand.RemoveQuest:
                                result += (_) =>
                                {
                                    string removeQuestName = split[1].ToLower();
                                    bool triggerEvent = split.Length > 2 && bool.Parse(split[2]);
                                    Quests_DataTypes.Quest.RemoveQuestFailed(removeQuestName.GetStableHashCode(), triggerEvent ? Quests_DataTypes.QuestEventCondition.OnCancelQuest : null);
                                    Quests_UIs.AcceptedQuestsUI.CheckQuests();
                                };
                                break;
                            case OptionCommand.GiveBuff:
                                result += (_) =>
                                {
                                    Player.m_localPlayer.GetSEMan()
                                        .RemoveStatusEffect(split[1].GetStableHashCode(), true);
                                    StatusEffect se = Player.m_localPlayer.GetSEMan()
                                        .AddStatusEffect(split[1].GetStableHashCode(), true);
                                    if (se && split.Length > 2)
                                        se.m_ttl = Mathf.Min(1, float.Parse(split[2]));
                                };
                                break;
                            case OptionCommand.FinishQuest:
                                result += (_) =>
                                {
                                    int reqID = split[1].ToLower().GetStableHashCode();
                                    if (!Quests_DataTypes.AllQuests.ContainsKey(reqID)) return;
                                    Quests_DataTypes.Quest.RemoveQuestComplete(reqID);
                                    Quests_UIs.AcceptedQuestsUI.CheckQuests();
                                };
                                break;
                            case OptionCommand.PingMap:
                                result += (_) =>
                                {
                                    Vector3 pos = new Vector3(int.Parse(split[2]), int.Parse(split[3]),
                                        int.Parse(split[4]));
                                    long sender = Random.Range(-10000, 10000);
                                    Chat.instance.AddInworldText(null, sender, pos, Talker.Type.Ping, new() { Name = "", UserId = new PlatformUserID("") }, split[1]);
                                    Minimap.instance.ShowPointOnMap(pos);
                                    Minimap.instance.m_largeZoom = 0.1f;
                                };
                                break;
                            case OptionCommand.AddPin:
                                result += (_) =>
                                {
                                    Vector3 pos = new Vector3(int.Parse(split[2]), int.Parse(split[3]),
                                        int.Parse(split[4]));
                                    Minimap.instance.AddPin(pos, NPC_MapPins.PINTYPENPC, split[1], true, false);
                                    Minimap.instance.ShowPointOnMap(pos);
                                    Minimap.instance.m_largeZoom = 0.1f;
                                };
                                break;
                            case OptionCommand.AddEpicMMOExp:
                                result += (_) =>
                                {
                                    int exp = int.Parse(split[1]);
                                    EpicMMOSystem_API.AddExp(exp);
                                };
                                break;
                            case OptionCommand.AddRustyClassesEXP:
                                result += (_) =>
                                {
                                    int exp = int.Parse(split[1]);
                                    RustyClassesAPI.AddEXP(exp);
                                };
                                break;
                            case OptionCommand.SendWebhook:
                                result += (_) =>
                                {
                                    string link = "https://" + split[1];
                                    if (!Uri.TryCreate(link, UriKind.Absolute, out Uri _)) return;
                                    string text = split[2];
                                    foreach (var replacement in Dialogues_UI._replacements) text = text.Replace(replacement.Key, replacement.Value());
                                    Task.Run(async () =>
                                    {
                                        string json = $"{{\"username\":\"Dialogue Webhook\",\"content\":\"{text}\"}}";
                                        HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(link);
                                        httpWebRequest.ContentType = "application/json";
                                        httpWebRequest.Method = "POST";
                                        using (StreamWriter streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                                        {
                                            await streamWriter.WriteAsync(json);
                                        }

                                        await httpWebRequest.GetResponseAsync();
                                    });
                                };
                                break;
                            case OptionCommand.AddCozyheimExp:
                                result += (_) =>
                                {
                                    int exp = int.Parse(split[1]);
                                    Cozyheim_LevelingSystem.AddExp(exp);
                                };
                                break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Utils.print($"Error while parsing dialogue command ({command}):\n{ex}");
                }
            }

            return result;
        }


        public static Dialogue_Condition ParseConditions(IEnumerable<string> conditions, bool isQuest = false)
        {
            Dialogue_Condition result = null;
            if (conditions == null) return null;
            foreach (string condition in conditions)
            {
                if (string.IsNullOrWhiteSpace(condition)) continue;
                try
                {
                    string[] split = condition.Replace(":", ",").Split(',');
                    bool reverse = false;
                    if (split[0][0] == '!')
                    {
                        reverse = true;
                        split[0] = split[0].Substring(1);
                    }

                    if (Enum.TryParse(split[0], true, out OptionCondition optionCondition))
                    {
                        if (reverse) optionCondition = optionCondition.Reverse();
                        switch (optionCondition)
                        {
                            case OptionCondition.None:
                                result += (Market_NPC.NPCcomponent npc, out string reason, out OptionCondition type) =>
                                {
                                    reason = "";
                                    type = OptionCondition.None;
                                    return true;
                                };
                                break;
                            case OptionCondition.IsVIP:
                                result += (Market_NPC.NPCcomponent npc, out string reason, out OptionCondition type) =>
                                {
                                    type = OptionCondition.IsVIP;
                                    reason = $"{Localization.instance.Localize("$mpasn_onlyforvip")}";
                                    return Global_Configs.SyncedGlobalOptions.Value._vipPlayerList.Contains(
                                        Global_Configs
                                            ._localUserID);
                                };
                                break;
                            case OptionCondition.CustomValueMore:
                                result += (Market_NPC.NPCcomponent npc, out string reason, out OptionCondition type) =>
                                {
                                    type = OptionCondition.CustomValueMore;
                                    string key = split[1];
                                    reason =
                                        $"{Localization.instance.Localize("$mpasn_needcustomvalue")}: <color=#00ff00>{split[1]}. Current: {Player.m_localPlayer.GetCustomValue(key)}</color>";
                                    return Player.m_localPlayer.GetCustomValue(key) >= int.Parse(split[2]);
                                };
                                break;
                            case OptionCondition.CustomValueLess:
                                result += (Market_NPC.NPCcomponent npc, out string reason, out OptionCondition type) =>
                                {
                                    type = OptionCondition.CustomValueLess;
                                    string key = split[1];
                                    reason =
                                        $"{Localization.instance.Localize("$mpasn_dontneedcustomvalue")}: <color=#00ff00>{split[1]}. Current: {Player.m_localPlayer.GetCustomValue(key)}</color>";
                                    return Player.m_localPlayer.GetCustomValue(key) < int.Parse(split[2]);
                                };
                                break;
                            case OptionCondition.NotIsVIP:
                                result += (Market_NPC.NPCcomponent npc, out string reason, out OptionCondition type) =>
                                {
                                    type = OptionCondition.NotIsVIP;
                                    reason = $"{Localization.instance.Localize("$mpasn_notforvip")}";
                                    return !Global_Configs.SyncedGlobalOptions.Value._vipPlayerList.Contains(
                                        Global_Configs
                                            ._localUserID);
                                };
                                break;
                            case OptionCondition.MHLevelMore:
                                result += (Market_NPC.NPCcomponent npc, out string reason, out OptionCondition type) =>
                                {
                                    type = OptionCondition.MHLevelMore;
                                    int amount = int.Parse(split[1]);
                                    int current = MH_API.GetLevel();
                                    reason = $"{Localization.instance.Localize("$mpasn_needmhlevel")} <color=#00ff00>{amount}</color>. Current: <color=#00ff00>{current}</color>";
                                    return current >= amount;
                                };
                                break;
                            case OptionCondition.MHLevelLess:
                                result += (Market_NPC.NPCcomponent npc, out string reason, out OptionCondition type) =>
                                {
                                    type = OptionCondition.MHLevelLess;
                                    int amount = int.Parse(split[1]);
                                    int current = MH_API.GetLevel();
                                    reason = $"{Localization.instance.Localize("$mpasn_needmhlevel")} <color=#00ff00>{amount}</color>. Current: <color=#00ff00>{current}</color>";
                                    return current < amount;
                                };
                                break;
                            case OptionCondition.RustyClassesLevelMore:
                                result += (Market_NPC.NPCcomponent npc, out string reason, out OptionCondition type) =>
                                {
                                    type = OptionCondition.RustyClassesLevelMore;
                                    int amount = int.Parse(split[1]);
                                    int current = RustyClassesAPI.GetLevel();
                                    reason = $"{Localization.instance.Localize("$mpasn_needrustyclasseslevel")} <color=#00ff00>{amount}</color>. Current: <color=#00ff00>{current}</color>";
                                    return current >= amount;
                                };
                                break;
                            case OptionCondition.RustyClassesLevelLess:
                                result += (Market_NPC.NPCcomponent npc, out string reason, out OptionCondition type) =>
                                {
                                    type = OptionCondition.RustyClassesLevelLess;
                                    int amount = int.Parse(split[1]);
                                    int current = RustyClassesAPI.GetLevel();
                                    reason = $"{Localization.instance.Localize("$mpasn_needrustyclasseslevel")} <color=#00ff00>{amount}</color>. Current: <color=#00ff00>{current}</color>";
                                    return current < amount;
                                };
                                break;
                            case OptionCondition.HasGuild:
                                result += (Market_NPC.NPCcomponent npc, out string reason, out OptionCondition type) =>
                                {
                                    reason = $"$mpasn_noguild".Localize();
                                    type = OptionCondition.HasGuild;
                                    return Guilds.API.GetOwnGuild() != null;
                                };
                                break;
                            case OptionCondition.NotHasGuild:
                                result += (Market_NPC.NPCcomponent npc, out string reason, out OptionCondition type) =>
                                {
                                    reason = $"$mpasn_hasguild".Localize();
                                    type = OptionCondition.NotHasGuild;
                                    return Guilds.API.GetOwnGuild() == null;
                                };
                                break;
                            case OptionCondition.GuildLevelMore:
                                result += (Market_NPC.NPCcomponent npc, out string reason, out OptionCondition type) =>
                                {
                                    type = OptionCondition.GuildLevelMore;
                                    int amount = int.Parse(split[1]);
                                    int current = Guilds.API.GetOwnGuild() is { } g ? g.General.level : int.MinValue;
                                    reason =
                                        $"{Localization.instance.Localize("$mpasn_needguildlevelmore")} <color=#00ff00>{amount}</color>. Current: <color=#00ff00>{current}</color>";
                                    return current >= amount;
                                };
                                break;
                            case OptionCondition.GuildLevelLess:
                                result += (Market_NPC.NPCcomponent npc, out string reason, out OptionCondition type) =>
                                {
                                    type = OptionCondition.GuildLevelLess;
                                    int amount = int.Parse(split[1]);
                                    int current = Guilds.API.GetOwnGuild() is { } g ? g.General.level : int.MaxValue;
                                    reason =
                                        $"{Localization.instance.Localize("$mpasn_needguildlevelless")} <color=#00ff00>{amount}</color>. Current: <color=#00ff00>{current}</color>";
                                    return current < amount;
                                };
                                break;
                            case OptionCondition.HasGuildWithName:
                                result += (Market_NPC.NPCcomponent npc, out string reason, out OptionCondition type) =>
                                {
                                    type = OptionCondition.HasGuildWithName;
                                    reason =
                                        $"{Localization.instance.Localize("$mpasn_needguildname")}: <color=#00ff00>{split[1]}</color>";
                                    return Guilds.API.GetOwnGuild() is { } g && g.Name == split[1];
                                };
                                break;
                            case OptionCondition.NotHasGuildWithName:
                                result += (Market_NPC.NPCcomponent npc, out string reason, out OptionCondition type) =>
                                {
                                    type = OptionCondition.NotHasGuildWithName;
                                    reason =
                                        $"{Localization.instance.Localize("$mpasn_dontneedguildname")}: <color=#00ff00>{split[1]}</color>";
                                    return Guilds.API.GetOwnGuild() is not { } g || g.Name != split[1];
                                };
                                break;
                            case OptionCondition.GuildHasAchievement:
                                result += (Market_NPC.NPCcomponent npc, out string reason, out OptionCondition type) =>
                                {
                                    type = OptionCondition.GuildHasAchievement;
                                    reason =
                                        $"{Localization.instance.Localize("$mpasn_needguildachievement")}: <color=#00ff00>{split[1]}</color>";
                                    return Guilds.API.GetOwnGuild() is { } g && g.Achievements.ContainsKey(split[1]);
                                };
                                break;
                            case OptionCondition.GuildNotHasAchievement:
                                result += (Market_NPC.NPCcomponent npc, out string reason, out OptionCondition type) =>
                                {
                                    type = OptionCondition.GuildNotHasAchievement;
                                    reason =
                                        $"{Localization.instance.Localize("$mpasn_dontneedguildachievement")}: <color=#00ff00>{split[1]}</color>";
                                    return Guilds.API.GetOwnGuild() is not { } g ||
                                           !g.Achievements.ContainsKey(split[1]);
                                };
                                break;
                            case OptionCondition.IronGateStatMore:
                                result += (Market_NPC.NPCcomponent npc, out string reason, out OptionCondition type) =>
                                {
                                    reason = "";
                                    type = OptionCondition.IronGateStatMore;
                                    if (!PlayerStatType.TryParse(split[1], true, out PlayerStatType stat)) return false;
                                    int amount = int.Parse(split[2]);
                                    float current = Game.instance.m_playerProfile.m_playerStats[stat];
                                    reason =
                                        $"{Localization.instance.Localize("$mpasn_needIronGateStatMore")}: <color=#00ff00>{stat.ToString().BigLettersToSpaces()} x{amount}</color>. Current: <color=#00ff00>{(int)current}</color>";
                                    return current >= amount;
                                };
                                break;
                            case OptionCondition.IronGateStatLess:
                                result += (Market_NPC.NPCcomponent npc, out string reason, out OptionCondition type) =>
                                {
                                    reason = "";
                                    type = OptionCondition.IronGateStatLess;
                                    if (!PlayerStatType.TryParse(split[1], true, out PlayerStatType stat)) return false;
                                    int amount = int.Parse(split[2]);
                                    float current = Game.instance.m_playerProfile.m_playerStats[stat];
                                    reason =
                                        $"{Localization.instance.Localize("$mpasn_needIronGateStatLess")}: <color=#00ff00>{stat.ToString().BigLettersToSpaces()} x{amount}</color>. <color=#00ff00>Current: {(int)current}</color>";
                                    return current < amount;
                                };
                                break;
                            case OptionCondition.ModInstalled:
                                result += (Market_NPC.NPCcomponent npc, out string reason, out OptionCondition type) =>
                                {
                                    type = OptionCondition.ModInstalled;
                                    string modGUID = split[1];
                                    reason =
                                        $"{Localization.instance.Localize("$mpasn_needmodinstalled")}: <color=#00ff00>{split[1]}</color>";
                                    return Chainloader.PluginInfos.ContainsKey(modGUID);
                                };
                                break;
                            case OptionCondition.NotModInstalled:
                                result += (Market_NPC.NPCcomponent npc, out string reason, out OptionCondition type) =>
                                {
                                    type = OptionCondition.NotModInstalled;
                                    string modGUID = split[1];
                                    reason =
                                        $"{Localization.instance.Localize("$mpasn_dontneedmodinstalled")}: <color=#00ff00>{split[1]}</color>";
                                    return !Chainloader.PluginInfos.ContainsKey(modGUID);
                                };
                                break;
                            case OptionCondition.HasAchievement:
                                result += (Market_NPC.NPCcomponent npc, out string reason, out OptionCondition type) =>
                                {
                                    type = OptionCondition.HasAchievement;
                                    reason =
                                        $"{Localization.instance.Localize("$mpasn_needtitle")}: <color=#00ff00>{LeaderBoard_Main_Client.GetAchievementName(split[1])}</color>";
                                    return LeaderBoard_Main_Client.HasAchievement(split[1]);
                                };
                                break;
                            case OptionCondition.NotHasAchievement:
                                result += (Market_NPC.NPCcomponent npc, out string reason, out OptionCondition type) =>
                                {
                                    type = OptionCondition.NotHasAchievement;
                                    reason =
                                        $"{Localization.instance.Localize("$mpasn_dontneedtitle")}: <color=#00ff00>{LeaderBoard_Main_Client.GetAchievementName(split[1])}</color>";
                                    return !LeaderBoard_Main_Client.HasAchievement(split[1]);
                                };
                                break;
                            case OptionCondition.HasAchievementScore:
                                result += (Market_NPC.NPCcomponent npc, out string reason, out OptionCondition type) =>
                                {
                                    type = OptionCondition.HasAchievementScore;
                                    reason =
                                        $"{Localization.instance.Localize("$mpasn_needtitlescore")}: <color=#00ff00>{split[1]}</color>";
                                    return Leaderboard_DataTypes.SyncedClientLeaderboard.Value.TryGetValue(
                                               Global_Configs._localUserID + "_" +
                                               Game.instance.m_playerProfile.m_playerName,
                                               out Leaderboard_DataTypes.Client_Leaderboard LB) &&
                                           Leaderboard_UI.GetAchievementScore(LB.Achievements) >=
                                           int.Parse(split[1]);
                                };
                                break;
                            case OptionCondition.NotHasAchievementScore:
                                result += (Market_NPC.NPCcomponent npc, out string reason, out OptionCondition type) =>
                                {
                                    type = OptionCondition.NotHasAchievementScore;
                                    reason =
                                        $"{Localization.instance.Localize("$mpasn_dontneedtitlescore")}: <color=#00ff00>{split[1]}</color>";
                                    return Leaderboard_DataTypes.SyncedClientLeaderboard.Value.TryGetValue(
                                               Global_Configs._localUserID + "_" +
                                               Game.instance.m_playerProfile.m_playerName,
                                               out Leaderboard_DataTypes.Client_Leaderboard LB) &&
                                           Leaderboard_UI.GetAchievementScore(LB.Achievements) <
                                           int.Parse(split[1]);
                                };
                                break;
                            case OptionCondition.SkillMore or OptionCondition.Skill:
                                result += (Market_NPC.NPCcomponent npc, out string reason, out OptionCondition type) =>
                                {
                                    type = OptionCondition.SkillMore;
                                    reason =
                                        $"{Localization.instance.Localize("$mpasn_notenoughskilllevel")}: <color=#00ff00>{Utils.LocalizeSkill(split[1])} {split[2]}</color>";
                                    return Utils.GetPlayerSkillLevelCustom(split[1]) >= int.Parse(split[2]);
                                };
                                break;
                            case OptionCondition.SkillLess:
                                result += (Market_NPC.NPCcomponent npc, out string reason, out OptionCondition type) =>
                                {
                                    type = OptionCondition.SkillLess;
                                    reason =
                                        $"{Localization.instance.Localize("$mpasn_toomuchskilllevel")}: <color=#00ff00>{Utils.LocalizeSkill(split[1])} {split[2]}</color>";
                                    return Utils.GetPlayerSkillLevelCustom(split[1]) < int.Parse(split[2]);
                                };
                                break;
                            case OptionCondition.QuestFinished or OptionCondition.OtherQuest:
                                result += (Market_NPC.NPCcomponent npc, out string reason, out OptionCondition type) =>
                                {
                                    type = OptionCondition.QuestFinished;
                                    reason = "";
                                    int reqID = split[1].ToLower().GetStableHashCode();
                                    if (!Quests_DataTypes.AllQuests.ContainsKey(reqID)) return true;
                                    reason = $"{Localization.instance.Localize("$mpasn_needtofinishquest")}: <color=#00ff00>{Quests_DataTypes.AllQuests[reqID].Name}</color>";
                                    return Quests_DataTypes.Quest.IsOnCooldown(reqID, out _);
                                };
                                break;
                            case OptionCondition.QuestNotFinished or OptionCondition.NotFinished:
                                result += (Market_NPC.NPCcomponent npc, out string reason, out OptionCondition type) =>
                                {
                                    type = OptionCondition.QuestNotFinished;
                                    reason = "";
                                    int reqID = split[1].ToLower().GetStableHashCode();

                                    if (Quests_DataTypes.AcceptedQuests.TryGetValue(reqID, out Quests_DataTypes.Quest quest))
                                    {
                                        reason = $"$mpasn_questtaken: <color=#00ff00>{quest.Name}</color>".Localize();
                                        return false;
                                    }

                                    if (!Quests_DataTypes.AllQuests.TryGetValue(reqID, out Quests_DataTypes.Quest reqQuest)) return true;
                                    return !Quests_DataTypes.Quest.IsOnCooldown(reqID, out _);
                                };
                                break;
                            case OptionCondition.HasItem:
                                result += (Market_NPC.NPCcomponent npc, out string reason, out OptionCondition type) =>
                                {
                                    type = OptionCondition.HasItem;
                                    reason = "";
                                    GameObject prefab = ZNetScene.instance.GetPrefab(split[1]);
                                    if (!prefab || !prefab.GetComponent<ItemDrop>()) return true;
                                    reason = $"{Localization.instance.Localize("$mpasn_needhasitem")}: <color=#00ff00>{Localization.instance.Localize(prefab.GetComponent<ItemDrop>().m_itemData.m_shared.m_name)} x{split[2]}</color>";
                                    int itemLevel = split.Length > 3 ? int.Parse(split[3]) : 1;
                                    return Utils.CustomCountItems(split[1], itemLevel) >= int.Parse(split[2]);
                                };
                                break;
                            case OptionCondition.NotHasItem:
                                result += (Market_NPC.NPCcomponent npc, out string reason, out OptionCondition type) =>
                                {
                                    type = OptionCondition.NotHasItem;
                                    reason = "";
                                    GameObject prefab = ZNetScene.instance.GetPrefab(split[1]);
                                    if (!prefab || !prefab.GetComponent<ItemDrop>()) return true;
                                    reason = $"{Localization.instance.Localize("$mpasn_neednothasitem")}: <color=#00ff00>{Localization.instance.Localize(prefab.GetComponent<ItemDrop>().m_itemData.m_shared.m_name)} x{split[2]}</color>";
                                    int itemLevel = split.Length > 3 ? int.Parse(split[3]) : 1;
                                    return Utils.CustomCountItems(split[1], itemLevel) < int.Parse(split[2]);
                                };
                                break;
                            case OptionCondition.NPCModelEquals:
                                result += (Market_NPC.NPCcomponent npc, out string reason, out OptionCondition type) =>
                                {
                                    type = OptionCondition.NPCModelEquals;
                                    reason = "";
                                    if (!npc) return false;
                                    return npc.LocalModelOverride == split[1];
                                };
                                break;
                            case OptionCondition.NotNPCModelEquals:
                                result += (Market_NPC.NPCcomponent npc, out string reason, out OptionCondition type) =>
                                {
                                    type = OptionCondition.NotNPCModelEquals;
                                    reason = "";
                                    if (!npc) return false;
                                    return npc.LocalModelOverride != split[1];
                                };
                                break;
                            case OptionCondition.NPCNameEquals:
                                result += (Market_NPC.NPCcomponent npc, out string reason, out OptionCondition type) =>
                                {
                                    type = OptionCondition.NPCNameEquals;
                                    reason = "";
                                    if (!npc) return false;
                                    return npc.GetNPCName() == split[1];
                                };
                                break;
                            case OptionCondition.NotNPCNameEquals:
                                result += (Market_NPC.NPCcomponent npc, out string reason, out OptionCondition type) =>
                                {
                                    type = OptionCondition.NotNPCNameEquals;
                                    reason = "";
                                    if (!npc) return false;
                                    return npc.GetNPCName() != split[1];
                                };
                                break;
                            case OptionCondition.GlobalKey:
                                result += (Market_NPC.NPCcomponent npc, out string reason, out OptionCondition type) =>
                                {
                                    type = OptionCondition.GlobalKey;
                                    reason =
                                        $"{Localization.instance.Localize("$mpasn_needglobalkey")}: <color=#00ff00>{split[1]}</color>";
                                    return ZoneSystem.instance.GetGlobalKey(split[1]);
                                };
                                break;
                            case OptionCondition.NotGlobalKey:
                                result += (Market_NPC.NPCcomponent npc, out string reason, out OptionCondition type) =>
                                {
                                    type = OptionCondition.NotGlobalKey;
                                    reason =
                                        $"{Localization.instance.Localize("$mpasn_notneedglobalkey")}: <color=#00ff00>{split[1]}</color>";
                                    return !ZoneSystem.instance.GetGlobalKey(split[1]);
                                };
                                break;
                            case OptionCondition.HasPlayerKey:
                                result += (Market_NPC.NPCcomponent npc, out string reason, out OptionCondition type) =>
                                {
                                    type = OptionCondition.HasPlayerKey;
                                    reason =
                                        $"{Localization.instance.Localize("$mpasn_needplayerkey")}: <color=#00ff00>{split[1]}</color>";
                                    return Player.m_localPlayer.HaveUniqueKey(split[1]);
                                };
                                break;
                            case OptionCondition.NotHasPlayerKey:
                                result += (Market_NPC.NPCcomponent npc, out string reason, out OptionCondition type) =>
                                {
                                    type = OptionCondition.NotHasPlayerKey;
                                    reason =
                                        $"{Localization.instance.Localize("$mpasn_notneedplayerkey")}: <color=#00ff00>{split[1]}</color>";
                                    return !Player.m_localPlayer.HaveUniqueKey(split[1]);
                                };
                                break;
                            case OptionCondition.HasBuff:
                                result += (Market_NPC.NPCcomponent npc, out string reason, out OptionCondition type) =>
                                {
                                    type = OptionCondition.HasBuff;
                                    StatusEffect findSe =
                                        ObjectDB.instance.m_StatusEffects.FirstOrDefault(s => s.name == split[1])!;
                                    string seName = findSe == null
                                        ? split[1]
                                        : Localization.instance.Localize(findSe.m_name);
                                    reason =
                                        $"{Localization.instance.Localize("$mpasn_needhasbuff")}: <color=#00ff00>{seName}</color>";
                                    return Player.m_localPlayer.m_seman.HaveStatusEffect(split[1].GetStableHashCode());
                                };
                                break;
                            case OptionCondition.NotHasBuff:
                                result += (Market_NPC.NPCcomponent npc, out string reason, out OptionCondition type) =>
                                {
                                    type = OptionCondition.NotHasBuff;
                                    StatusEffect findSe =
                                        ObjectDB.instance.m_StatusEffects.FirstOrDefault(s => s.name == split[1])!;
                                    string seName = findSe == null
                                        ? split[1]
                                        : Localization.instance.Localize(findSe.m_name);
                                    reason =
                                        $"{Localization.instance.Localize("$mpasn_notneedhasbuff")}: <color=#00ff00>{seName}</color>";
                                    return !Player.m_localPlayer.m_seman.HaveStatusEffect(split[1].GetStableHashCode());
                                };
                                break;
                            case OptionCondition.QuestProgressDone:
                                result += (Market_NPC.NPCcomponent npc, out string reason, out OptionCondition type) =>
                                {
                                    type = OptionCondition.QuestProgressDone;
                                    reason = "";
                                    int reqID = split[1].ToLower().GetStableHashCode();
                                    if (!Quests_DataTypes.AllQuests.TryGetValue(reqID,
                                            out Quests_DataTypes.Quest reqQuest) ||
                                        !Quests_DataTypes.AcceptedQuests.ContainsKey(reqID))
                                        return false;
                                    reason =
                                        $"{Localization.instance.Localize("$mpasn_queststillnotfinished")}: <color=#00ff00>{reqQuest.Name}</color>";
                                    return Quests_DataTypes.AllQuests[reqID].IsComplete();
                                };
                                break;
                            case OptionCondition.QuestProgressNotDone:
                                result += (Market_NPC.NPCcomponent npc, out string reason, out OptionCondition type) =>
                                {
                                    type = OptionCondition.QuestProgressNotDone;
                                    reason = "";
                                    int reqID = split[1].ToLower().GetStableHashCode();
                                    if (!Quests_DataTypes.AllQuests.TryGetValue(reqID,
                                            out Quests_DataTypes.Quest reqQuest) ||
                                        !Quests_DataTypes.AcceptedQuests.ContainsKey(reqID))
                                        return false;
                                    reason =
                                        $"{Localization.instance.Localize("$mpasn_questalreadyfinished")}: <color=#00ff00>{reqQuest.Name}</color>";
                                    return !Quests_DataTypes.AllQuests[reqID].IsComplete();
                                };
                                break;
                            case OptionCondition.HasQuest:
                                result += (Market_NPC.NPCcomponent npc, out string reason, out OptionCondition type) =>
                                {
                                    type = OptionCondition.HasQuest;
                                    reason = "";
                                    int reqID = split[1].ToLower().GetStableHashCode();
                                    if (!Quests_DataTypes.AllQuests.TryGetValue(reqID,
                                            out Quests_DataTypes.Quest reqQuest))
                                        return false;
                                    reason =
                                        $"{Localization.instance.Localize("$mpasn_questnottaken")}: <color=#00ff00>{reqQuest.Name}</color>";
                                    return Quests_DataTypes.AcceptedQuests.ContainsKey(reqID);
                                };
                                break;
                            case OptionCondition.NotHasQuest:
                                result += (Market_NPC.NPCcomponent npc, out string reason, out OptionCondition type) =>
                                {
                                    type = OptionCondition.NotHasQuest;
                                    reason = "";
                                    int reqID = split[1].ToLower().GetStableHashCode();
                                    if (!Quests_DataTypes.AllQuests.TryGetValue(reqID,
                                            out Quests_DataTypes.Quest reqQuest))
                                        return false;
                                    reason =
                                        $"{Localization.instance.Localize("$mpasn_questtaken")}: <color=#00ff00>{reqQuest.Name}</color>";
                                    return !Quests_DataTypes.AcceptedQuests.ContainsKey(reqID);
                                };
                                break;
                            case OptionCondition.EpicMMOLevelMore:
                                result += (Market_NPC.NPCcomponent npc, out string reason, out OptionCondition type) =>
                                {
                                    type = OptionCondition.EpicMMOLevelMore;
                                    reason =
                                        $"{Localization.instance.Localize("$mpasn_needepicmmolevel")}: <color=#00ff00>{split[1]}</color>";
                                    return EpicMMOSystem_API.GetLevel() >= int.Parse(split[1]);
                                };
                                break;
                            case OptionCondition.EpicMMOLevelLess:
                                result += (Market_NPC.NPCcomponent npc, out string reason, out OptionCondition type) =>
                                {
                                    type = OptionCondition.EpicMMOLevelLess;
                                    reason =
                                        $"{Localization.instance.Localize("$mpasn_toomuchepicmmolevel")}: <color=#00ff00>{split[1]}</color>";
                                    return EpicMMOSystem_API.GetLevel() < int.Parse(split[1]);
                                };
                                break;
                            case OptionCondition.CozyheimLevelMore:
                                result += (Market_NPC.NPCcomponent npc, out string reason, out OptionCondition type) =>
                                {
                                    type = OptionCondition.CozyheimLevelMore;
                                    reason =
                                        $"{Localization.instance.Localize("$mpasn_needcozyheimlevel")}: <color=#00ff00>{split[1]}</color>";
                                    return Cozyheim_LevelingSystem.GetLevel() >= int.Parse(split[1]);
                                };
                                break;
                            case OptionCondition.CozyheimLevelLess:
                                result += (Market_NPC.NPCcomponent npc, out string reason, out OptionCondition type) =>
                                {
                                    type = OptionCondition.CozyheimLevelLess;
                                    reason =
                                        $"{Localization.instance.Localize("$mpasn_toomuchcozyheimlevel")}: <color=#00ff00>{split[1]}</color>";
                                    return Cozyheim_LevelingSystem.GetLevel() < int.Parse(split[1]);
                                };
                                break;
                            case OptionCondition.HealthMore:
                                result += (Market_NPC.NPCcomponent npc, out string reason, out OptionCondition type) =>
                                {
                                    type = OptionCondition.HealthMore;
                                    reason = $"{Localization.instance.Localize("$mpasn_needcurrenthealthmore")}: <color=#00ff00>{split[1]}</color>";
                                    return Player.m_localPlayer.GetHealth() >= int.Parse(split[1]);
                                };
                                break;
                            case OptionCondition.HealthLess:
                                result += (Market_NPC.NPCcomponent npc, out string reason, out OptionCondition type) =>
                                {
                                    type = OptionCondition.HealthLess;
                                    reason = $"{Localization.instance.Localize("$mpasn_needcurrenthealthless")}: <color=#00ff00>{split[1]}</color>";
                                    return Player.m_localPlayer.GetHealth() < int.Parse(split[1]);
                                };
                                break;
                            case OptionCondition.StaminaMore:
                                result += (Market_NPC.NPCcomponent npc, out string reason, out OptionCondition type) =>
                                {
                                    type = OptionCondition.StaminaMore;
                                    reason = $"{Localization.instance.Localize("$mpasn_needcurrentstaminamore")}: <color=#00ff00>{split[1]}</color>";
                                    return Player.m_localPlayer.GetStamina() >= int.Parse(split[1]);
                                };
                                break;
                            case OptionCondition.StaminaLess:
                                result += (Market_NPC.NPCcomponent npc, out string reason, out OptionCondition type) =>
                                {
                                    type = OptionCondition.StaminaLess;
                                    reason = $"{Localization.instance.Localize("$mpasn_needcurrentstaminaless")}: <color=#00ff00>{split[1]}</color>";
                                    return Player.m_localPlayer.GetStamina() < int.Parse(split[1]);
                                };
                                break;
                            case OptionCondition.EitrLess:
                                result += (Market_NPC.NPCcomponent npc, out string reason, out OptionCondition type) =>
                                {
                                    type = OptionCondition.EitrLess;
                                    reason = $"{Localization.instance.Localize("$mpasn_needcurrenteitrless")}: <color=#00ff00>{split[1]}</color>";
                                    return Player.m_localPlayer.GetEitr() < int.Parse(split[1]);
                                };
                                break;
                            case OptionCondition.EitrMore:
                                result += (Market_NPC.NPCcomponent npc, out string reason, out OptionCondition type) =>
                                {
                                    type = OptionCondition.EitrMore;
                                    reason = $"{Localization.instance.Localize("$mpasn_needcurrenteitrmore")}: <color=#00ff00>{split[1]}</color>";
                                    return Player.m_localPlayer.GetEitr() >= int.Parse(split[1]);
                                };
                                break;
                            case OptionCondition.MaxHealthMore:
                                result += (Market_NPC.NPCcomponent npc, out string reason, out OptionCondition type) =>
                                {
                                    type = OptionCondition.MaxHealthMore;
                                    reason = $"{Localization.instance.Localize("$mpasn_needmaxhealthmore")}: <color=#00ff00>{split[1]}</color>";
                                    return Player.m_localPlayer.GetMaxHealth() >= int.Parse(split[1]);
                                };
                                break;
                            case OptionCondition.MaxHealthLess:
                                result += (Market_NPC.NPCcomponent npc, out string reason, out OptionCondition type) =>
                                {
                                    type = OptionCondition.MaxHealthLess;
                                    reason = $"{Localization.instance.Localize("$mpasn_needmaxhealthless")}: <color=#00ff00>{split[1]}</color>";
                                    return Player.m_localPlayer.GetMaxHealth() < int.Parse(split[1]);
                                };
                                break;
                            case OptionCondition.MaxStaminaMore:
                                result += (Market_NPC.NPCcomponent npc, out string reason, out OptionCondition type) =>
                                {
                                    type = OptionCondition.MaxStaminaMore;
                                    reason = $"{Localization.instance.Localize("$mpasn_needmaxstaminamore")}: <color=#00ff00>{split[1]}</color>";
                                    return Player.m_localPlayer.GetMaxStamina() >= int.Parse(split[1]);
                                };
                                break;
                            case OptionCondition.MaxStaminaLess:
                                result += (Market_NPC.NPCcomponent npc, out string reason, out OptionCondition type) =>
                                {
                                    type = OptionCondition.MaxStaminaLess;
                                    reason = $"{Localization.instance.Localize("$mpasn_needmaxstaminaless")}: <color=#00ff00>{split[1]}</color>";
                                    return Player.m_localPlayer.GetMaxStamina() < int.Parse(split[1]);
                                };
                                break;
                            case OptionCondition.MaxEitrMore:
                                result += (Market_NPC.NPCcomponent npc, out string reason, out OptionCondition type) =>
                                {
                                    type = OptionCondition.MaxEitrMore;
                                    reason = $"{Localization.instance.Localize("$mpasn_needmaxeitrmore")}: <color=#00ff00>{split[1]}</color>";
                                    return Player.m_localPlayer.GetMaxEitr() >= int.Parse(split[1]);
                                };
                                break;
                            case OptionCondition.MaxEitrLess:
                                result += (Market_NPC.NPCcomponent npc, out string reason, out OptionCondition type) =>
                                {
                                    type = OptionCondition.MaxEitrLess;
                                    reason = $"{Localization.instance.Localize("$mpasn_needmaxeitrless")}: <color=#00ff00>{split[1]}</color>";
                                    return Player.m_localPlayer.GetMaxEitr() < int.Parse(split[1]);
                                };
                                break;
                            case OptionCondition.PlayerHasAllCustomDataKeys:
                            {
                                result += (Market_NPC.NPCcomponent npc, out string reason, out OptionCondition type) =>
                                {
                                    type = OptionCondition.PlayerHasAllCustomDataKeys;
                                    string dataKey = split[1];
                                    if (!Dialogues_DataTypes.CustomSpawnZDOData_Synced.Value.TryGetValue(dataKey, out var customData) )
                                    {
                                        reason = "No custom data found";
                                        return false;
                                    }

                                    reason = split[2];
                                    foreach (var key in customData.Bools) if (Player.m_localPlayer.m_nview.m_zdo.GetBool(key.Key) != key.Value) return false;
                                    foreach (var key in customData.Floats) if (Math.Abs(Player.m_localPlayer.m_nview.m_zdo.GetFloat(key.Key) - key.Value) > 0.01f) return false;
                                    foreach (var key in customData.Ints) if (Player.m_localPlayer.m_nview.m_zdo.GetInt(key.Key) != key.Value) return false;
                                    foreach (var key in customData.Strings) if (Player.m_localPlayer.m_nview.m_zdo.GetString(key.Key) != key.Value) return false;
                                    return true;
                                };
                                break;
                            }
                            case OptionCondition.PlayerHasOneOfCustomDataKeys:
                            {
                                result += (Market_NPC.NPCcomponent npc, out string reason, out OptionCondition type) =>
                                {
                                    type = OptionCondition.PlayerHasOneOfCustomDataKeys;
                                    string dataKey = split[1];
                                    if (!Dialogues_DataTypes.CustomSpawnZDOData_Synced.Value.TryGetValue(dataKey, out var customData) )
                                    {
                                        reason = "No custom data found";
                                        return false;
                                    }

                                    reason = split[2];
                                    foreach (var key in customData.Bools) if (Player.m_localPlayer.m_nview.m_zdo.GetBool(key.Key) != key.Value) return true;
                                    foreach (var key in customData.Floats) if (Math.Abs(Player.m_localPlayer.m_nview.m_zdo.GetFloat(key.Key) - key.Value) > 0.01f) return true;
                                    foreach (var key in customData.Ints) if (Player.m_localPlayer.m_nview.m_zdo.GetInt(key.Key) != key.Value) return true;
                                    foreach (var key in customData.Strings) if (Player.m_localPlayer.m_nview.m_zdo.GetString(key.Key) != key.Value) return true;
                                    return false;
                                };
                                break;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Utils.print($"Error while parsing {(isQuest ? "Quest" : "Dialogue")} condition ({condition}):\n{ex}");
                }
            }

            return result;
        }

        public static implicit operator Dialogue(RawDialogue raw)
        {
            Dialogue dialogue = new()
            {
                Text = raw.Text,
                Interactions = new PlayerInteraction[raw.Interactions.Length],
                BG_Image = Utils.TryFindIcon(raw.BG_Image)
            };
            for (int i = 0; i < raw.Interactions.Length; ++i)
            {
                dialogue.Interactions[i] = new PlayerInteraction
                {
                    Type = raw.Interactions[i].Type,
                    Text = raw.Interactions[i].Text,
                    Icon = Utils.TryFindIcon(raw.Interactions[i].Icon),
                    NextUID = raw.Interactions[i].NextUID,
                    RandomTransition = raw.Interactions[i].RandomTransition,
                    Condition = ParseConditions(raw.Interactions[i].Conditions),
                    Command = ParseCommands(raw.Interactions[i].Commands),
                    RandomCommands = raw.Interactions[i].RandomCommands.ToDictionary(entry => entry.Key, entry => ParseCommands([entry.Value])),
                    AlwaysVisible = raw.Interactions[i].AlwaysVisible,
                    Color = raw.Interactions[i].Color,
                    OverrideError = raw.Interactions[i].OverrideError,
                };
            }

            return dialogue;
        }
    }

    public class DialoguePassword : TextReceiver
    {
        private string _password;
        private string _onSuccess;
        private string _onFail;
        private Market_NPC.NPCcomponent _npc;

        public DialoguePassword(Market_NPC.NPCcomponent npc, string title, string password, string onSuccess,
            string onFail)
        {
            _password = password;
            _onSuccess = onSuccess;
            _onFail = onFail;
            _npc = npc;
            TextInput.instance.RequestText(this, title, 30);
        }

        public string GetText()
        {
            return "";
        }

        public void SetText(string text)
        {
            if (text == _password)
            {
                if (!string.IsNullOrEmpty(_onSuccess))
                    Dialogues_UI.LoadDialogue(_npc, _onSuccess.ToLower());
            }
            else
            {
                if (!string.IsNullOrEmpty(_onFail))
                    Dialogues_UI.LoadDialogue(_npc, _onFail.ToLower());
            }
        }
    }
}