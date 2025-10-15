using BepInEx.Configuration;
using Marketplace.ExternalLoads;
using Marketplace.Modules.Dialogues;
using UnityEngine.Networking;

namespace Marketplace.Modules.Quests;

[UsedImplicitly]
[Market_Autoload(Market_Autoload.Type.Client, Market_Autoload.Priority.Normal)]
public static class Quests_Main_Client
{
    private static ConfigEntry<KeyCode> QuestJournalOpenKey;
    private static int LatestRevision;
    public static ConfigEntry<bool> ShowQuestMark;
    private static Sprite MoveQuestPin;

    private static void OnInit()
    {
        MoveQuestPin = AssetStorage.asset.LoadAsset<Sprite>("CircleQuestMove");
        QuestJournalOpenKey = Marketplace._thistype.Config.Bind("General", "Quest Journal Keycode", KeyCode.J);
        ShowQuestMark = Marketplace._thistype.Config.Bind("General", "Show Quest Mark", true);
        Quests_UIs.QuestUI.Init();
        Quests_UIs.AcceptedQuestsUI.Init();
        Quests_DataTypes.SyncedQuestData.ValueChanged += OnQuestDataUpdate;
        Quests_DataTypes.SyncedQuestProfiles.ValueChanged += OnQuestProfilesUpdate;
        Marketplace.Global_Updator += Update;
        GameEvents.OnPlayerFirstSpawn += OnPlayerFirstSpawn;
        GameEvents.OnPlayerDeath += OnPlayerDeath;
    }

    private static void OnPlayerDeath()
    {
        foreach (var acceptedQuest in Quests_DataTypes.AcceptedQuests.ToList())
            Quests_DataTypes.HandleQuestEvent(acceptedQuest.Key, Quests_DataTypes.QuestEventCondition.OnDeath);
    }

    private static void OnPlayerFirstSpawn()
    {
        int startQuestID = "onplayerfirstspawn".GetStableHashCode();
        if (Quests_DataTypes.AllQuests.TryGetValue(startQuestID, out Quests_DataTypes.Quest quest))
        {
            if (!Quests_DataTypes.Quest.IsOnCooldown(startQuestID, out int _) && !Quests_DataTypes.Quest.IsAccepted(startQuestID))
            {
                Quests_DataTypes.Quest.AcceptQuest(startQuestID, handleEvent: true);
                Quests_UIs.AcceptedQuestsUI.CheckQuests();
            }
        }
    }

    private static void OnQuestDataUpdate()
    {
        Quests_DataTypes.AllQuests.Clear();
        Quests_DataTypes.TransferComplitionQuests.Clear();
        if (Player.m_localPlayer)
        {
            Quest_Main_LoadPatch.Postfix();
            Quests_UIs.QuestUI.Reload(); 
            CreatePins(true);
        }
    }

    private static void OnQuestProfilesUpdate()
    {
        Quests_UIs.QuestUI.Reload();
    }

    private static float _moveQuestcounter = 3f;
    private static void Update(float dt)
    {

        if (Player.m_localPlayer)
        {
            _moveQuestcounter -= dt;
            if (Minimap.instance && _moveQuestcounter <= 0)
            {
                _moveQuestcounter = 3f;
                Quests_DataTypes.Quest.TryAddRewardMove(Player.m_localPlayer.transform.position);
                CreatePins(false);
            }
        }
        
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (Quests_UIs.QuestUI.IsVisible())
            {
                Quests_UIs.QuestUI.Hide();
                Menu.instance.OnClose();
            }
        }

        if (!Player.m_localPlayer) return;
        if (Input.GetKeyDown(QuestJournalOpenKey.Value) && Player.m_localPlayer.TakeInput())
            Quests_UIs.QuestUI.ClickJournal();
    }
    
    
    [HarmonyPatch(typeof(Game),nameof(Game.Awake))]
    private static class Game_Awake_Patch
    {
        private static void Postfix()
        {
            _moveQuestcounter = 3f;
            _questMovePins.Clear();
        }
    }
    
    private static readonly Dictionary<int, List<Minimap.PinData>> _questMovePins = new();
    private const Minimap.PinType _questMovePinType = (Minimap.PinType)191;
    private static void CreatePins(bool forceRestart)
    {
            if (forceRestart) 
            {
                foreach (var pin in _questMovePins)
                {
                    foreach (var pinData in pin.Value)
                    {
                        Minimap.instance.RemovePin(pinData);
                    }
                }
                _questMovePins.Clear();
            }
            else
            {
                int[] keys = _questMovePins.Keys.ToArray();
                foreach (int key in keys)
                {
                    if (!Quests_DataTypes.AcceptedQuests.ContainsKey(key))
                    {
                        for (int i = 0; i < _questMovePins[key].Count; i++)
                        {
                            Minimap.PinData pin = _questMovePins[key][i];
                            Minimap.instance.RemovePin(pin);
                        }
                        _questMovePins.Remove(key);
                    } 
                } 
            }
            foreach (KeyValuePair<int, Quests_DataTypes.Quest> quest in Quests_DataTypes.AcceptedQuests)
            {
                if (_questMovePins.ContainsKey(quest.Key)) continue;
                if (quest.Value.Type != Quests_DataTypes.QuestType.Move) continue;
                List<Minimap.PinData> pins = new(quest.Value.TargetPrefab.Length);
                for (int i = 0; i < quest.Value.TargetPrefab.Length; ++i)
                {
                    if (quest.Value.TargetLevel[i] == 0) continue;
                    string[] split = quest.Value.TargetPrefab[i].Split(',');
                    if (split.Length < 4) continue;
                    float x = float.Parse(split[0]);
                    float z = float.Parse(split[1]); 
                    float distance = float.Parse(split[2]);
                    string name = split[3]; 
                    Minimap.PinData pin = new Minimap.PinData
                    {
                        m_type = _questMovePinType,
                        m_pos = new Vector3(x, 0, z),
                        m_name = "<color=yellow>" + name + "</color>",
                        m_icon = MoveQuestPin,
                        m_save = false,
                        m_checked = false,
                        m_ownerID = 0L,
                        m_worldSize = distance * 2f
                    };
                    if (!string.IsNullOrEmpty(pin.m_name)) pin.m_NamePinData = new Minimap.PinNameData(pin);
                    pins.Add(pin);
                }
                _questMovePins[quest.Key] = pins;
                Minimap.instance.m_pins.AddRange(pins);
            }
            Minimap.instance.m_pinUpdateRequired = true; 
        }

    [HarmonyPatch(typeof(Player), nameof(Player.Load))]
    [ClientOnlyPatch]
    private static class Quest_Main_LoadPatch
    {
        private static void ClearEmptyQuestCooldowns()
        {
            if (!Player.m_localPlayer) return;
            HashSet<string> toRemove = new();
            const string str = "[MPASN]questCD=";
            const string str2 = "[MPASN]quest=";
            foreach (KeyValuePair<string, string> key in Player.m_localPlayer.m_customData)
            {
                if (key.Key.Contains(str))
                {
                    int UID = Convert.ToInt32(key.Key.Split('=')[1]);
                    if (!Quests_DataTypes.AllQuests.ContainsKey(UID) ||
                        !Quests_DataTypes.Quest.IsOnCooldown(UID, out _))
                    {
                        toRemove.Add(key.Key);
                    }
                }

                if (key.Key.Contains(str2))
                {
                    int UID = Convert.ToInt32(key.Key.Split('=')[1]);
                    if (!Quests_DataTypes.AllQuests.ContainsKey(UID))
                    {
                        toRemove.Add(key.Key);
                    }
                }
            }

            foreach (string remove in toRemove)
            {
                Player.m_localPlayer.m_customData.Remove(remove);
            }
        }

        private static void LoadQuests()
        {
            Quests_DataTypes.AcceptedQuests.Clear();
            if (!Player.m_localPlayer) return;
            const string str = "[MPASN]quest=";
            Dictionary<int, string> temp = new();
            foreach (KeyValuePair<string, string> key in Player.m_localPlayer.m_customData)
            {
                if (key.Key.Contains(str))
                {
                    int UID = Convert.ToInt32(key.Key.Split('=')[1]);
                    temp[UID] = key.Value;
                }
            }

            foreach (KeyValuePair<int, string> key in temp)
            {
                string[] split = key.Value.Split(';');
                string score = split[0];
                string time = split.Length > 1 ? split[1] : null;
                Quests_DataTypes.Quest.AcceptQuest(key.Key, score, time, false);
            }

            Quests_UIs.AcceptedQuestsUI.CheckQuests();
        }

        private static void InitRawQuests()
        {
            if (!Player.m_localPlayer || Quests_DataTypes.SyncedQuestData.Value.Count == 0) return;
            if (LatestRevision == Quests_DataTypes.SyncedQuestRevision.Value) return;
            LatestRevision = Quests_DataTypes.SyncedQuestRevision.Value;

            foreach (KeyValuePair<int, Quests_DataTypes.Quest> quest in Quests_DataTypes.SyncedQuestData.Value)
            {
                if (quest.Value.Init())
                {
                    Quests_DataTypes.AllQuests[quest.Key] = quest.Value;
                    int transfer = quest.Value.TransferComplition;
                    if (transfer != 0)
                    {
                        if (!Quests_DataTypes.TransferComplitionQuests.ContainsKey(transfer)) Quests_DataTypes.TransferComplitionQuests[transfer] = [];
                        Quests_DataTypes.TransferComplitionQuests[transfer].Add(quest.Key);
                    }
                }
                else
                {
                    Utils.print($"{quest.Value.Name} (id {quest.Key}) can't finish init");
                }
            }
             
            foreach (KeyValuePair<Quests_DataTypes.Quest, string> url in Quests_DataTypes.AllQuests.Select(x => new KeyValuePair<Quests_DataTypes.Quest, string>(x.Value, x.Value.PreviewImage)))
            {
                if (!string.IsNullOrWhiteSpace(url.Value))
                    Utils.LoadImageFromWEB(url.Value, (sprite) => url.Key.SetPreviewSprite(sprite));
            }
        }

        public static void Postfix()
        {
            InitRawQuests();
            ClearEmptyQuestCooldowns();
            LoadQuests();
        }
    }

    [HarmonyPatch(typeof(FejdStartup), nameof(FejdStartup.Awake))]
    [ClientOnlyPatch]
    private class CloseUIMenuLogout
    {
        private static void Postfix()
        {
            Quests_UIs.AcceptedQuestsUI.Hide();
            LatestRevision = int.MaxValue;
        }
    }
}