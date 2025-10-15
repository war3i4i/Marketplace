using Marketplace.Paths;
namespace Marketplace.Modules.Gambler;

[UsedImplicitly]
[Market_Autoload(Market_Autoload.Type.Server, Market_Autoload.Priority.Normal, "OnInit",
    new[] { "GP" }, new[] { "OnGamblerProfilesFileChange" })]
public static class Gambler_Main_Server
{
    [UsedImplicitly]
    private static void OnInit()
    {
        ReadGamblerProfiles();
    }

    [UsedImplicitly]
    private static void OnGamblerProfilesFileChange()
    {
        ReadGamblerProfiles();
        Utils.print("Gambler Changed. Sending new info to all clients");
    }

    private static void ReadGamblerProfiles()
    {
        Gambler_DataTypes.SyncedGamblerData.Value.Clear();
        string folder = Market_Paths.GamblerProfilesFolder;
        string[] files = Directory.GetFiles(folder, "*.cfg", SearchOption.AllDirectories);
        foreach (string file in files)
        {
            IReadOnlyList<string> profiles = File.ReadAllLines(file).ToList();
            ReadGamblerProfiles(file, profiles);
        }
        ClientClearData();
        Gambler_DataTypes.SyncedGamblerData.Update();
    }

    private static void ClientClearData()
    {
        foreach (KeyValuePair<string, Gambler_DataTypes.BigData> data in Gambler_DataTypes.SyncedGamblerData.Value)
        {
            if (data.Value.Data.Count() > 19)
            {
                data.Value.Data.RemoveRange(19, data.Value.Data.Count() - 19);
            }
        }
    }

    private static void ReadGamblerProfiles(string fPath, IReadOnlyList<string> profiles)
    {
        string splitProfile = "default";
        int MAXSROLL = 1;
        for (int i = 0; i < profiles.Count; ++i)
        {
            if (string.IsNullOrWhiteSpace(profiles[i]) || profiles[i].StartsWith("#")) continue;
            if (profiles[i].StartsWith("["))
            {
                splitProfile = profiles[i].Replace("[", "").Replace("]", "").Replace(" ", "").ToLower();

                string[] testSplit = splitProfile.Split('=');
                if (testSplit.Length > 1)
                {
                    splitProfile = testSplit[0];
                    MAXSROLL = int.Parse(testSplit[1]);
                }
                else
                {
                    MAXSROLL = 1;
                }
            }
            else
            {
                string[] split = profiles[i].Replace(" ", "").Split(',');
                if (split.Length % 2 != 0)
                {
                    Utils.print($"Gambler {fPath} line {i + 1} has error, number of data is wrong", ConsoleColor.Red);
                    continue;
                }

                List<Gambler_DataTypes.Item> data = new List<Gambler_DataTypes.Item>();
                for (int j = 0; j < split.Length; j += 2)
                {
                    Gambler_DataTypes.Item itm = new Gambler_DataTypes.Item
                    {
                        Prefab = split[j]
                    };

                    string[] minmax = split[j + 1].Split('-');

                    if (minmax.Length == 1)
                    {
                        itm.Min = int.Parse(minmax[0]);
                        itm.Max = itm.Min;
                    }
                    else
                    {
                        itm.Min = int.Parse(minmax[0]);
                        itm.Max = int.Parse(minmax[1]);
                    }
                    data.Add(itm);
                }
                Gambler_DataTypes.Item required = data[0];
                data.RemoveAt(0);
                Gambler_DataTypes.SyncedGamblerData.Value[splitProfile] = new Gambler_DataTypes.BigData()
                {
                    MAXROLLS = MAXSROLL, Data = data, RequiredItem = required
                };
            }
        }
    }
}