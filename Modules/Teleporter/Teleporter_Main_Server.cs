using Marketplace.Paths;

namespace Marketplace.Modules.Teleporter;

[UsedImplicitly]
[Market_Autoload(Market_Autoload.Type.Server, Market_Autoload.Priority.Normal, "OnInit",
    new[] { "TE" }, new[] { "OnTeleporterProfilesChange" })]
public static class Teleporter_Main_Server
{
    [UsedImplicitly]
    private static void OnInit()
    {
        ReadServerTeleporterProfiles();
    }

    private static void ProcessTeleporterProfiles(string fPath, IReadOnlyList<string> profiles)
    {
        string splitProfile = "default";
        for (int i = 0; i < profiles.Count; ++i)
        {
            if (string.IsNullOrWhiteSpace(profiles[i]) || profiles[i].StartsWith("#")) continue;
            if (profiles[i].StartsWith("["))
            {
                splitProfile = profiles[i].Replace("[", "").Replace("]", "").Replace(" ","").ToLower();
            }
            else
            {
                string[] test = profiles[i].Split(',');
                try
                {

                    int _speed = 0;
                    string name = test[0];
                    int index = name.IndexOf("<speed=", StringComparison.Ordinal);
                    if (index != -1)
                    {
                        string sub = name.Substring(index + "<speed=".Length);
                        int endIndex = sub.IndexOf('>');
                        if (endIndex > 0)
                        {
                            string findSpeed = sub.Replace(">", "");
                            if (!string.IsNullOrWhiteSpace(findSpeed))
                            { 
                                _speed = Convert.ToInt32(findSpeed);
                            }
                            name = Utils.RemoveRichTextDynamicTag(name, "speed");
                        }
                    }

                    Teleporter_DataTypes.TeleporterData pinnerData = new Teleporter_DataTypes.TeleporterData
                    {
                        name = name,
                        x = int.Parse(test[1].Replace(" ", "")),
                        y = int.Parse(test[2].Replace(" ", "")),
                        z = int.Parse(test[3].Replace(" ", "")),
                        sprite = test.Length == 5 ? test[4].Replace(" ", "") : "none",
                        speed =  _speed
                    };

                    if (Teleporter_DataTypes.SyncedTeleporterData.Value.TryGetValue(splitProfile, out List<Teleporter_DataTypes.TeleporterData> value))
                    {
                        value.Add(pinnerData);
                    }
                    else
                    {
                        Teleporter_DataTypes.SyncedTeleporterData.Value[splitProfile] = new List<Teleporter_DataTypes.TeleporterData> { pinnerData };
                    }
                }
                catch (Exception ex)
                {
                    Utils.print($"{ex}\nERROR: Can't add pins in {fPath}, line: {i}", ConsoleColor.Red);
                    return;
                }
            }
        }
    }
    
    private static void ReadServerTeleporterProfiles()
    {
        Teleporter_DataTypes.SyncedTeleporterData.Value.Clear();
        string folder = Market_Paths.TeleportHubProfilesFolder;
        string[] files = Directory.GetFiles(folder, "*.cfg", SearchOption.AllDirectories);
        foreach (string file in files)
        {
            IReadOnlyList<string> profiles = File.ReadAllLines(file).ToList();
            ProcessTeleporterProfiles(file, profiles);
        }
        Teleporter_DataTypes.SyncedTeleporterData.Update();
    }
    

    private static void OnTeleporterProfilesChange()
    {
        ReadServerTeleporterProfiles();
        Utils.print("TeleportHub changed, sending options to peers");
    }
    
}