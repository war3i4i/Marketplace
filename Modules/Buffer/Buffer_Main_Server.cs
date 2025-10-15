using Marketplace.Paths;

namespace Marketplace.Modules.Buffer;

[UsedImplicitly]
[Market_Autoload(Market_Autoload.Type.Server, Market_Autoload.Priority.Normal, "OnInit",
    new[] { "BP", "BD" },
    new[] { "OnBufferProfilesFileChange", "OnBufferProfilesFileChange" })]
public static class Buffer_Main_Server
{
    [UsedImplicitly]
    private static void OnInit()
    {
        ReadBufferConfigs();
    }

    private static void ReadBufferConfigs()
    {
        Buffer_DataTypes.SyncedBufferProfiles.Value.Clear();
        Buffer_DataTypes.SyncedBufferBuffs.Value.Clear();
        try
        {
            string folder = Market_Paths.BufferProfilesFolder;
            string[] files = Directory.GetFiles(folder, "*.cfg", SearchOption.AllDirectories);
            foreach (string file in files)
            {
                IReadOnlyList<string> profiles = File.ReadAllLines(file).ToList();
                ReadBufferProfiles(profiles);
            }
            folder = Market_Paths.BufferDatabaseFolder;
            files = Directory.GetFiles(folder, "*.cfg", SearchOption.AllDirectories);
            foreach (string file in files)
            {
                IReadOnlyList<string> profiles = File.ReadAllLines(file).ToList();
                ReadBufferDatabase(file, profiles);
            }
        }
        catch (Exception ex)
        {
            Utils.print($"Got exception on save buffer. Not sending data:{ex}", ConsoleColor.Red);
        }
        finally
        {
            Buffer_DataTypes.SyncedBufferProfiles.Update();
            Buffer_DataTypes.SyncedBufferBuffs.Update();
        }
    }

    [UsedImplicitly]
    private static void OnBufferProfilesFileChange()
    {
        ReadBufferConfigs();
        Utils.print("Buffer Profiles / Database Changed. Sending new info to all clients");
    }


    private static void ReadBufferProfiles(IReadOnlyList<string> profiles)
    {
        string splitProfile = "default";
        foreach (string line in profiles)
        {
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#")) continue;
            if (line.StartsWith("["))
            {
                splitProfile = line.Replace("[", "").Replace("]", "").Replace(" ", "").ToLower();
            }
            else
            {
                if (!Buffer_DataTypes.SyncedBufferProfiles.Value.ContainsKey(splitProfile))
                {
                    Buffer_DataTypes.SyncedBufferProfiles.Value.Add(splitProfile, line.Replace(" ", ""));
                }
            }
        }
    }

    private static void ReadBufferDatabase(string fPath, IReadOnlyList<string> profiles)
    {
        string dbProfile = null;
        for (int i = 0; i < profiles.Count; ++i)
        {
            if (string.IsNullOrWhiteSpace(profiles[i]) || profiles[i].StartsWith("#")) continue;
            if (profiles[i].StartsWith("["))
            {
                dbProfile = profiles[i].Replace("[", "").Replace("]", "").Replace(" ", "");
            }
            else
            {
                try
                {
                    if (dbProfile == null) continue;
                    if (i + 5 > profiles.Count) break;
                    Buffer_DataTypes.BufferBuffData data = new()
                    {
                        UniqueName = dbProfile,
                        Name = profiles[i],
                        Duration = int.Parse(profiles[i + 1].Replace(" ", "")),
                        SpritePrefab = profiles[i + 2].Replace(" ", "")
                    };
                    string[] price = profiles[i + 3].Replace(" ", "").Split(',');
                    data.NeededPrefab = price[0];
                    data.NeededPrefabCount = int.Parse(price[1]);
                    Buffer_DataTypes.WhatToModify _flags = Buffer_DataTypes.WhatToModify.None;
                    string[] splitFlags = profiles[i + 4].Replace(" ", "").Split(',');
                    foreach (string flag in splitFlags)
                    {
                        string[] split = flag.Split('=');
                        if (split.Length != 2) continue;
                        string workingFlag = split[0];
                        float customData = float.Parse(split[1], NumberStyles.AllowDecimalPoint,
                            CultureInfo.InvariantCulture);
                        if (!Enum.TryParse(workingFlag, true, out Buffer_DataTypes.WhatToModify testFlag)) continue;
                        _flags |= testFlag;
                        data.ModifyList[testFlag] = customData;
                    }

                    data.Flags = _flags;
                    data.StartEffectPrefab = profiles[i + 5].Replace(" ", "");
                    data.BuffGroup = profiles[i + 6].Trim(' ');
                    Buffer_DataTypes.SyncedBufferBuffs.Value.Add(data);
                    dbProfile = null;
                }
                catch (Exception ex)
                {
                    Utils.print($"Buffer error {fPath} in profile {dbProfile} : {ex}", ConsoleColor.Red);
                }
            }
        }
    }
}