using Marketplace.Paths;

namespace Marketplace.Modules.Trader;

[UsedImplicitly]
[Market_Autoload(Market_Autoload.Type.Server, Market_Autoload.Priority.Normal, "OnInit", new[] { "TR" },
    new[] { "OnTraderProfilesFileChange" })]
public static class Trader_Main_Server
{
    private static void OnInit()
    {
        ReadServerTraderProfiles();
    }
    
    private static void OnTraderProfilesFileChange()
    {
        ReadServerTraderProfiles();
        Utils.print("Trader Changed. Sending new info to all clients");
    }

    private static void ProcessTraderProfiles(string fPath, IReadOnlyList<string> profiles)
    {
        string splitProfile = "default";
        bool _NeedToKnow = false;
        for (int i = 0; i < profiles.Count; ++i)
        {
            if (string.IsNullOrWhiteSpace(profiles[i]) || profiles[i].StartsWith("#")) continue;
            if (profiles[i].StartsWith("["))
            {
                splitProfile = profiles[i].Replace("[", "").Replace("]", "").Replace(" ","").ToLower();
                string[] split = splitProfile.Split('=');
                if (split.Length == 2)
                {
                    splitProfile = split[0];
                    _NeedToKnow = Convert.ToBoolean(split[1]);
                }
                else
                {
                    _NeedToKnow = false;
                }
            }
            else
            {
                try
                {
                    if (!profiles[i].Contains('='))
                    {
                        //old format
                        string[] test = profiles[i].Replace(" ", "").Split(',');
                        if (test.Length == 5)
                        {
                            Trader_DataTypes.TraderData traderData = new Trader_DataTypes.TraderData()
                            {
                                NeedToKnow = _NeedToKnow,
                                NeededItems = new Trader_DataTypes.TraderItem
                                {
                                    ItemPrefab = test[0],
                                    Count = int.Parse(test[1]),
                                }.ToList(),
                                ResultItems = new Trader_DataTypes.TraderItem
                                {
                                    ItemPrefab = test[2],
                                    Count = int.Parse(test[3]),
                                    Level = int.Parse(test[4])
                                }.ToList()
                            };
                            if (Trader_DataTypes.SyncedTraderItemList.Value.TryGetValue(splitProfile, out List<Trader_DataTypes.TraderData> value))
                            {
                                value.Add(traderData);
                            }
                            else
                            {
                                Trader_DataTypes.SyncedTraderItemList.Value[splitProfile] = new List<Trader_DataTypes.TraderData> { traderData };
                            }
                        }

                        if (test.Length == 4)
                        {
                            Trader_DataTypes.TraderData traderData = new Trader_DataTypes.TraderData()
                            {
                                NeedToKnow = _NeedToKnow,
                                NeededItems = new List<Trader_DataTypes.TraderItem>
                                {
                                    new Trader_DataTypes.TraderItem
                                    {
                                        ItemPrefab = test[0],
                                        Count = int.Parse(test[1]),
                                    }
                                },
                                ResultItems = new List<Trader_DataTypes.TraderItem>
                                {
                                    new Trader_DataTypes.TraderItem
                                    {
                                        ItemPrefab = test[2],
                                        Count = int.Parse(test[3]),
                                    }
                                }
                            };
                            if (Trader_DataTypes.SyncedTraderItemList.Value.TryGetValue(splitProfile, out List<Trader_DataTypes.TraderData> value))
                            {
                                value.Add(traderData);
                            }
                            else
                            {
                                Trader_DataTypes.SyncedTraderItemList.Value[splitProfile] = new List<Trader_DataTypes.TraderData> { traderData };
                            }
                        }
                    }
                    else
                    {
                        // new format
                        string[] test = profiles[i].Replace(" ", "").Split('=');

                        void FillTrader(ICollection<Trader_DataTypes.TraderItem> list, IReadOnlyList<string> data)
                        {
                            for (int n = 0; n < data.Count;)
                            {
                                Trader_DataTypes.TraderItem traderItem = new Trader_DataTypes.TraderItem()
                                {
                                    ItemPrefab = data[n],
                                    Count = int.Parse(data[n + 1])
                                };
                                if (n + 2 < data.Count && int.TryParse(data[n + 2], out int lvl))
                                {
                                    traderItem.Level = lvl;
                                    n += 3;
                                }
                                else
                                {
                                    n += 2;
                                }

                                if (list.Count < 5)
                                    list.Add(traderItem);
                            }
                        }
                        if (test.Length != 2) continue;

                        Trader_DataTypes.TraderData traderData = new Trader_DataTypes.TraderData()
                        {
                            NeedToKnow = _NeedToKnow,
                            NeededItems = new List<Trader_DataTypes.TraderItem>(),
                            ResultItems = new List<Trader_DataTypes.TraderItem>()
                        };

                        string[] needed = test[0].Split(',');
                        string[] result = test[1].Split(',');
                        FillTrader(traderData.NeededItems, needed);
                        FillTrader(traderData.ResultItems, result);
                        if (Trader_DataTypes.SyncedTraderItemList.Value.TryGetValue(splitProfile, out List<Trader_DataTypes.TraderData> value)) 
                        {
                            value.Add(traderData);
                        }
                        else
                        {
                            Trader_DataTypes.SyncedTraderItemList.Value[splitProfile] = new List<Trader_DataTypes.TraderData> { traderData };
                        }
                    }
                }
                catch (Exception ex)
                {
                    Utils.print($"{ex}\nERROR: Can't add items in {fPath}, line: {i + 1}", ConsoleColor.Red);
                    return;
                }
            }
        }
    }
    
     private static void ReadServerTraderProfiles()
    {
        Trader_DataTypes.SyncedTraderItemList.Value.Clear();
        string folder = Market_Paths.TraderProfilesFolder;
        string[] files = Directory.GetFiles(folder, "*.cfg", SearchOption.AllDirectories);
        foreach (string file in files)
        {
            IReadOnlyList<string> profiles = File.ReadAllLines(file).ToList();
            ProcessTraderProfiles(file, profiles);
        }
        Trader_DataTypes.SyncedTraderItemList.Update();
    }
}