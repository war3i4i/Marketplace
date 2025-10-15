using Marketplace.Paths;

namespace Marketplace.Modules.Quests;

[UsedImplicitly]
[Market_Autoload(Market_Autoload.Type.Server, Market_Autoload.Priority.Normal, "OnInit",
    new[] { "QP", "QD", "QE" },
    new[] { "OnQuestsProfilesFileChange", "OnQuestDatabaseFileChange", "OnQuestsEventFileChange" })]
public static class Quests_Main_Server
{
    private static void OnInit()
    {
        ReadQuestProfiles();
        ReadQuestDatabase();
        ReadEventDatabase();
    }

    private static void OnQuestsProfilesFileChange()
    {
        ReadQuestProfiles();
        Utils.print("Quests Profiles Changed. Sending new info to all clients");
    }

    private static IEnumerator DelayMore()
    {
        yield return new WaitForSeconds(3);
        ReadQuestDatabase();
        Utils.print("Quests Database Changed. Sending new info to all clients");
    }

    private static void OnQuestDatabaseFileChange()
    {
        Marketplace._thistype.StartCoroutine(DelayMore());
    }

    private static void OnQuestsEventFileChange()
    {
        ReadEventDatabase();
        Utils.print("Quests Events Changed. Sending new info to all clients");
    }

    private static void ProcessQuestProfiles(IReadOnlyList<string> profiles)
    {
        string splitProfile = "default";
        for (int i = 0; i < profiles.Count; ++i)
        {
            if (string.IsNullOrWhiteSpace(profiles[i]) || profiles[i].StartsWith("#")) continue;
            if (profiles[i].StartsWith("["))
            {
                splitProfile = profiles[i].Replace("[", "").Replace("]", "").Replace(" ", "").ToLower();
            }
            else
            {
                string[] split = profiles[i].Replace(" ", "").Split(',').Distinct().ToArray();
                if (!Quests_DataTypes.SyncedQuestProfiles.Value.ContainsKey(splitProfile))
                {
                    Quests_DataTypes.SyncedQuestProfiles.Value.Add(splitProfile, new List<int>());
                }

                foreach (string quest in split)
                {
                    int questToHashcode = quest.ToLower().GetStableHashCode();
                    Quests_DataTypes.SyncedQuestProfiles.Value[splitProfile].Add(questToHashcode);
                }
            }
        }
    }

    private static void ReadQuestProfiles()
    {
        Quests_DataTypes.SyncedQuestProfiles.Value.Clear();
        string folder = Market_Paths.QuestsProfilesFolder;
        string[] files = Directory.GetFiles(folder, "*.cfg", SearchOption.AllDirectories);
        foreach (string file in files)
        {
            IReadOnlyList<string> profiles = File.ReadAllLines(file).ToList();
            ProcessQuestProfiles(profiles);
        }
        Quests_DataTypes.SyncedQuestProfiles.Update();
    }


    private static void ProcessQuestDatabaseProfiles(string fPath, IReadOnlyList<string> profiles)
    {
        if (profiles.Count == 0) return;
        string dbProfile = null;
        Quests_DataTypes.SpecialQuestTag specialQuestTag = Quests_DataTypes.SpecialQuestTag.None;
        for (int i = 0; i < profiles.Count; ++i)
        {
            if (string.IsNullOrWhiteSpace(profiles[i]) || profiles[i].StartsWith("#")) continue; 
            if (profiles[i].StartsWith("["))
            {
                dbProfile = profiles[i].Replace("[", "").Replace("]", "").Replace(" ", "").ToLower();
                string[] checkTags = dbProfile.Split('=');
                if (checkTags.Length == 2)
                {
                    if (!Enum.TryParse(checkTags[1], true, out Quests_DataTypes.SpecialQuestTag tag)) continue;
                    specialQuestTag |= tag;
                    dbProfile = checkTags[0];
                }
                else
                {
                    specialQuestTag = Quests_DataTypes.SpecialQuestTag.None;
                }
            }
            else
            {
                if (dbProfile == null) continue;
                try
                {
                    int UID = dbProfile.GetStableHashCode();
                    string typeString = profiles[i];
                    string name = profiles[i + 1];
                    string image = "";
                    int imageIndex = name.IndexOf("<image=", StringComparison.Ordinal);
                    if (imageIndex != -1)
                    {
                        int imageEndIndex = name.IndexOf(">", imageIndex, StringComparison.Ordinal);
                        if (imageEndIndex != -1)
                        {
                            image = name.Substring(imageIndex + 7, imageEndIndex - imageIndex - 7);
                            name = name.Remove(imageIndex, imageEndIndex - imageIndex + 1);
                        }
                    }

                    string description = profiles[i + 2];
                    string target = profiles[i + 3];
                    string reward = profiles[i + 4];
                    
                    string[] cooldownsString = profiles[i + 5].Replace(" ","").Split(',');
                    string cooldown = cooldownsString[0];
                    string timeLimit = cooldownsString.Length > 1 ? cooldownsString[1] : "0";
                    string restrictions = profiles[i + 6];
                    if (!(Enum.TryParse(typeString, true, out Quests_DataTypes.QuestType type) &&
                          Enum.IsDefined(typeof(Quests_DataTypes.QuestType), type)))
                    {
                        dbProfile = null;
                        continue;
                    }
                    string[] rewardsArray = reward.Replace(" ", "").Split('|');
                    int _RewardsAMOUNT = Mathf.Max(1, rewardsArray.Length);
                    Quests_DataTypes.QuestRewardType[] rewardTypes = new Quests_DataTypes.QuestRewardType[_RewardsAMOUNT];
                    string[] RewardPrefabs = new string[_RewardsAMOUNT];
                    int[] RewardLevels = new int[_RewardsAMOUNT];
                    int[] RewardCounts = new int[_RewardsAMOUNT];
                    for (int r = 0; r < _RewardsAMOUNT; ++r)
                    {
                        string[] rwdTypeCheck = rewardsArray[r].Split(':');
                        if (!(Enum.TryParse(rwdTypeCheck[0], true,
                                out rewardTypes[r])))
                        {
                            Utils.print($"Failed to parse reward type {rewardsArray[r]} in quest {name} (File: {fPath}). Skipping quest");
                            continue;
                        }

                        string[] RewardSplit = rwdTypeCheck[1].Split(',');

                        if (rewardTypes[r] 
                            is Quests_DataTypes.QuestRewardType.EpicMMO_EXP
                            or Quests_DataTypes.QuestRewardType.MH_EXP 
                            or Quests_DataTypes.QuestRewardType.Cozyheim_EXP
                            or Quests_DataTypes.QuestRewardType.GuildAddLevel
                            or Quests_DataTypes.QuestRewardType.Battlepass_EXP
                            or Quests_DataTypes.QuestRewardType.RustyClasses_EXP)
                        {
                            RewardPrefabs[r] = "NONE";
                            RewardCounts[r] = int.Parse(RewardSplit[0]);
                            RewardLevels[r] = 1;
                        }
                        else if (rewardTypes[r] is Quests_DataTypes.QuestRewardType.RandomItem)
                        {
                            RewardPrefabs[r] = rwdTypeCheck[1];
                            RewardCounts[r] = 0;
                            RewardLevels[r] = 0;
                        }
                        else
                        {
                            RewardPrefabs[r] = RewardSplit[0];
                            RewardCounts[r] = int.Parse(RewardSplit[1]);
                            RewardLevels[r] = 1;
                        }

                        if (RewardSplit.Length >= 3)
                        {
                            RewardLevels[r] = Convert.ToInt32(RewardSplit[2]);
                            if (rewardTypes[r] == Quests_DataTypes.QuestRewardType.Pet) ++RewardLevels[r];
                        }
                    }


                    if (type is not Quests_DataTypes.QuestType.Talk and not Quests_DataTypes.QuestType.Move)
                    {
                        target = target.Replace(" ", "");
                    }
                    string[] targetsArray = target.Split('|');
                    int _targetsCount = Mathf.Max(1, targetsArray.Length);
                    string[] TargetPrefabs = new string[_targetsCount];
                    int[] TargetLevels = new int[_targetsCount];
                    int[] TargetCounts = new int[_targetsCount];
                    for (int t = 0; t < _targetsCount; ++t)
                    {
                        string[] TargetSplit = targetsArray[t].Split(',');
                        TargetPrefabs[t] = TargetSplit[0];
                        TargetLevels[t] = 1; 
                        TargetCounts[t] = 1; 
                        switch (type)
                        {
                            case Quests_DataTypes.QuestType.Kill:
                                if (TargetSplit.Length >= 3) TargetLevels[t] = Mathf.Max(1, Convert.ToInt32(TargetSplit[2]) + 1);
                                TargetCounts[t] = Mathf.Max(1, int.Parse(TargetSplit[1]));
                                break;
                            case Quests_DataTypes.QuestType.Collect:
                                if (TargetSplit.Length >= 3) TargetLevels[t] = Mathf.Max(1, Convert.ToInt32(TargetSplit[2]));
                                TargetCounts[t] = Mathf.Max(1, int.Parse(TargetSplit[1]));
                                break; 
                            case Quests_DataTypes.QuestType.Harvest or Quests_DataTypes.QuestType.Build:
                                TargetCounts[t] = Mathf.Max(1, int.Parse(TargetSplit[1]));
                                break;
                            case Quests_DataTypes.QuestType.Craft:
                                if (TargetSplit.Length >= 3) TargetLevels[t] = Mathf.Max(1, Convert.ToInt32(TargetSplit[2]));
                                TargetCounts[t] = Mathf.Max(1, int.Parse(TargetSplit[1]));
                                break;
                            case Quests_DataTypes.QuestType.Move:
                                TargetPrefabs[t] = targetsArray[t].ReplaceSpacesOutsideQuotes();
                                if (TargetSplit.Length >= 5) TargetLevels[t] = bool.Parse(TargetSplit[4]) ? 1 : 0;
                                break;
                            case Quests_DataTypes.QuestType.Talk:
                                TargetPrefabs[t] = targetsArray[t].Trim();
                                break;
                        }
                    }
                    string[] Conditions = restrictions.ReplaceSpacesOutsideQuotes().Split('|');
                    int transferComplition = 0;
                    string transferComplitionText = "";
                    if (profiles.Count > i + 7)
                    { 
                        string extras = profiles[i + 7];
                        if (!string.IsNullOrWhiteSpace(extras) && extras[0] != '[')
                        {
                            string[] splitTransfer = extras.ReplaceSpacesOutsideQuotes().Split(',');
                            transferComplition = splitTransfer[0].GetStableHashCode();
                            if (splitTransfer.Length > 1) transferComplitionText = splitTransfer[1];
                        }
                    }

                    Quests_DataTypes.Quest quest = new()
                    {
                        RewardsAmount = _RewardsAMOUNT,
                        Type = type,
                        RewardType = rewardTypes, 
                        Name = name,
                        Description = description,
                        TargetAmount = _targetsCount,
                        TargetPrefab = TargetPrefabs,
                        TargetCount = TargetCounts,
                        TargetLevel = TargetLevels, 
                        RewardPrefab = RewardPrefabs,
                        RewardCount = RewardCounts,
                        Cooldown = int.Parse(cooldown),
                        RewardLevel = RewardLevels,
                        SpecialTag = specialQuestTag,
                        PreviewImage = image,
                        TimeLimit = int.Parse(timeLimit),
                        Conditions = Conditions,
                        TransferComplition = transferComplition,
                        TransferComplitionText = transferComplitionText
                    };
                    if (!Quests_DataTypes.SyncedQuestData.Value.ContainsKey(UID))
                        Quests_DataTypes.SyncedQuestData.Value.Add(UID, quest);
                }
                catch (Exception ex)
                {
                    Utils.print($"Error in Quests {fPath} {dbProfile}\n{ex}", ConsoleColor.Red);
                }
                dbProfile = null;
            }
        }
    }
    

    private static void ReadQuestDatabase()
    {
        Quests_DataTypes.SyncedQuestRevision.Value = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
        Quests_DataTypes.SyncedQuestData.Value.Clear();
        string folder = Market_Paths.QuestsDatabaseFolder;
        string[] files = Directory.GetFiles(folder, "*.cfg", SearchOption.AllDirectories);
        foreach (string file in files)
        {
            IReadOnlyList<string> profiles = File.ReadAllLines(file).ToList();
            ProcessQuestDatabaseProfiles(file, profiles);
        }

        Quests_DataTypes.SyncedQuestData.Update();
        
    }

    private static void ProcessEventDatabase(IReadOnlyList<string> profiles)
    {
        string splitProfile = "default";
        for (int i = 0; i < profiles.Count; ++i)
        {
            if (string.IsNullOrWhiteSpace(profiles[i]) || profiles[i].StartsWith("#")) continue;
            if (profiles[i].StartsWith("["))
            {
                splitProfile = profiles[i].Replace("[", "").Replace("]", "").ToLower();
            }
            else
            {
                string[] split = profiles[i].Split(':');
                if (split.Length < 2) continue;
                string key = split[0];
                string value = split[1].ReplaceSpacesOutsideQuotes();
                if (!Enum.TryParse(key, out Quests_DataTypes.QuestEventCondition cond) ||
                    !Enum.IsDefined(typeof(Quests_DataTypes.QuestEventCondition), cond)) continue;
                int keyHash = splitProfile.GetStableHashCode();
                if (!Quests_DataTypes.SyncedQuestsEvents.Value.ContainsKey(keyHash)) 
                    Quests_DataTypes.SyncedQuestsEvents.Value[keyHash] = new Quests_DataTypes.QuestEvent();
                switch (cond) 
                {
                    case Quests_DataTypes.QuestEventCondition.OnAcceptQuest:
                        Quests_DataTypes.SyncedQuestsEvents.Value[keyHash].AddOnAcceptQuest(value);
                        break;
                    case Quests_DataTypes.QuestEventCondition.OnCompleteQuest:
                        Quests_DataTypes.SyncedQuestsEvents.Value[keyHash].AddOnCompleteQuest(value);
                        break;
                    case Quests_DataTypes.QuestEventCondition.OnCancelQuest:
                        Quests_DataTypes.SyncedQuestsEvents.Value[keyHash].AddOnCancelQuest(value);
                        break;
                    case Quests_DataTypes.QuestEventCondition.OnQuestTimeout:
                        Quests_DataTypes.SyncedQuestsEvents.Value[keyHash].AddOnQuestTimeout(value);
                        break;
                    case Quests_DataTypes.QuestEventCondition.OnDeath:
                        Quests_DataTypes.SyncedQuestsEvents.Value[keyHash].AddOnDeath(value);
                        break;
                }
            }
        }
    }
    
    private static void ReadEventDatabase()
    {
        Quests_DataTypes.SyncedQuestsEvents.Value.Clear();
        string folder = Market_Paths.QuestsEventsFolder;
        string[] files = Directory.GetFiles(folder, "*.cfg", SearchOption.AllDirectories);
        foreach (string file in files)
        {
            IReadOnlyList<string> events = File.ReadAllLines(file).ToList();
            ProcessEventDatabase(events);
        }
        Quests_DataTypes.SyncedQuestsEvents.Update();
        foreach (KeyValuePair<int, Quests_DataTypes.QuestEvent> keyValuePair in Quests_DataTypes.SyncedQuestsEvents.Value)
        {
            keyValuePair.Value.ParseAll();
        }
    }
}