using Marketplace.Paths;

namespace Marketplace.Modules.Transmogrification;

[UsedImplicitly]
[Market_Autoload(Market_Autoload.Type.Server, Market_Autoload.Priority.Normal, "OnInit",
    new[] { "TM" },
    new[] { "OnTransmogrificationProfileChanged" })]
public static class Transmogrification_Main_Server
{
    private static void OnInit() => ReadTransmogrificationProfiles();

    private static void OnTransmogrificationProfileChanged()
    {
        ReadTransmogrificationProfiles();
        Utils.print($"Transmogrification profiles changed, sending to clients");
    }

    private static void ProcessTransmogrificationProfiles(IReadOnlyList<string> profiles)
    {
        string splitProfile = "default";

        for (int i = 0; i < profiles.Count; ++i)
        {
            if (string.IsNullOrWhiteSpace(profiles[i]) || profiles[i].StartsWith("#")) continue;
            if (profiles[i].StartsWith("["))
            {
                splitProfile = profiles[i].Replace("[", "").Replace("]", "").Replace(" ", "").ToLower();
                if (!Transmogrification_DataTypes.SyncedTransmogData.Value.ContainsKey(splitProfile))
                    Transmogrification_DataTypes.SyncedTransmogData.Value.Add(splitProfile,
                        new List<Transmogrification_DataTypes.TransmogItem_Data>());
            }
            else
            {
                string[] split = profiles[i].Replace(" ", "").Split(',');
                if (split.Length < 4) continue;
                try
                {
                    string prefab = split[0];
                    string pricePrefab = split[1];
                    int price = int.Parse(split[2]);
                    bool isSkip = bool.Parse(split[3]);
                    Transmogrification_DataTypes.SyncedTransmogData.Value[splitProfile].Add(
                        new Transmogrification_DataTypes.TransmogItem_Data()
                        {
                            Prefab = prefab,
                            Price_Prefab = pricePrefab,
                            Price_Amount = price,
                            IgnoreCategory = isSkip,
                        });
                }
                catch (Exception ex)
                {
                    Utils.print($"Error while parsing line {i + 1} in TransmogConfig: {ex}",
                        ConsoleColor.Red);
                }
            }
        }
    }

    private static void ReadTransmogrificationProfiles()
    {
        Transmogrification_DataTypes.SyncedTransmogData.Value.Clear();
        string folder = Market_Paths.TransmogrificationFolder;
        string[] files = Directory.GetFiles(folder, "*.cfg", SearchOption.AllDirectories);
        foreach (string file in files)
        {
            IReadOnlyList<string> profiles = File.ReadAllLines(file).ToList();
            ProcessTransmogrificationProfiles(profiles);
        }
        Transmogrification_DataTypes.SyncedTransmogData.Update();
    }
}