using System.Net.Configuration;
using System.Reflection.Emit;
using Marketplace.Paths;

namespace Marketplace.Modules.TerritorySystem;

[UsedImplicitly]
[Market_Autoload(Market_Autoload.Type.Server, Market_Autoload.Priority.Normal, "OnInit",
    new[] { "TD" },
    new[] { "OnTerritoryConfigChange" })]
public static class TerritorySystem_Main_Server
{
    private static string TerritoriesForSale;
    private static string TerritoryOverrides;

    private struct TerritoryOverride
    {
        public string TerritoryFlags;
        public string TerritoryColor;
    }
    
    private static List<TerritorySystem_DataTypes.Territory> TerritoriesForSaleData = new();
    private static Dictionary<string, TerritoryOverride> TerritoriesOverridesData = new();
    
    private static void OnInit()
    { 
        TerritoriesForSale = Path.Combine(Market_Paths.TerritoriesFolder, Marketplace._InternalMarketplace_ + "TerritoriesForSale.yml");
        TerritoryOverrides = Path.Combine(Market_Paths.TerritoriesFolder, Marketplace._InternalMarketplace_ + "TerritoriesFlagOverrides.yml");
        if (File.Exists(TerritoriesForSale))
        {
            string ymlData = File.ReadAllText(TerritoriesForSale);
            if (!string.IsNullOrWhiteSpace(ymlData))
            {
                TerritoriesForSaleData =  new YamlDotNet.Serialization.Deserializer().Deserialize<List<TerritorySystem_DataTypes.Territory>>(ymlData);
            }
        }
        if (File.Exists(TerritoryOverrides))
        {
            string ymlData = File.ReadAllText(TerritoryOverrides);
            if (!string.IsNullOrWhiteSpace(ymlData))
            {
                TerritoriesOverridesData = new YamlDotNet.Serialization.Deserializer().Deserialize<Dictionary<string, TerritoryOverride>>(ymlData);
            }
        }
        
        ReadServerTerritoryDatabase();
    }

    private static void OnTerritoryConfigChange()
    {
        ReadServerTerritoryDatabase();
        Utils.print("Territory Changed. Sending new info to all clients");
    }

    private static void ProcessTerritoryConfig(string fPath, IReadOnlyList<string> profiles)
    {
        string splitProfile = "default";
        int Priority = 1;
        for (int i = 0; i < profiles.Count; ++i)
        {
            if (string.IsNullOrWhiteSpace(profiles[i]) || profiles[i].StartsWith("#")) continue;
            if (profiles[i].StartsWith("["))
            {
                string[] split = profiles[i].Replace("[", "").Replace("]", "").Split('@');
                splitProfile = split[0];
                Priority = 1;
                if (split.Length == 2)
                {
                    Priority = int.Parse(split[1]);
                }
            }
            else
            {
                try
                {
                    if (i + 4 > profiles.Count) break;
                    TerritorySystem_DataTypes.Territory newTerritory = new()
                    {
                        Name = splitProfile,
                        Priority = Priority
                    };
                    if (!(Enum.TryParse(profiles[i], true,  out TerritorySystem_DataTypes.TerritoryShape type) &&
                          Enum.IsDefined(typeof(TerritorySystem_DataTypes.TerritoryShape), type))) continue;
                    newTerritory.Shape = type;

                    string[] xyr = profiles[i + 1].Replace(" ", "").Split(',');

                    switch (type)
                    {
                        case TerritorySystem_DataTypes.TerritoryShape.Circle:
                        case TerritorySystem_DataTypes.TerritoryShape.Square:
                            newTerritory.X = new[] { Convert.ToInt32(xyr[0]) };
                            newTerritory.Y = new[] { Convert.ToInt32(xyr[1]) };
                            newTerritory.Radius = Convert.ToInt32(xyr[2]);
                            break;
                        case TerritorySystem_DataTypes.TerritoryShape.Rectangle:
                            newTerritory.X = new[] { Convert.ToInt32(xyr[0]), Convert.ToInt32(xyr[2]) };
                            newTerritory.Y = new[] { Convert.ToInt32(xyr[1]), Convert.ToInt32(xyr[3]) };
                            break;
                    }

                    string colors = TerritoriesOverridesData.ContainsKey(newTerritory.Name) && !string.IsNullOrWhiteSpace(TerritoriesOverridesData[newTerritory.Name].TerritoryColor)
                        ? TerritoriesOverridesData[newTerritory.Name].TerritoryColor
                        : profiles[i + 2];
                    List<string> rgb = colors.Replace(" ", "").Split(',').ToList();
 
                    // draw water
                    for (int tf = 0; tf < rgb.Count; ++tf) 
                    {
                        if (rgb[tf].ToLower() is "true" or "false")
                        {
                            newTerritory.ShowExternalWater = Convert.ToBoolean(rgb[tf]);
                            rgb.RemoveAt(tf);
                            break;
                        }
                    }

                    // find gradient type
                    for (int tf = 0; tf < rgb.Count; ++tf)
                    {
                        if (!int.TryParse(rgb[tf], out _) && Enum.TryParse(rgb[tf], true,
                                out TerritorySystem_DataTypes.GradientType gradient))
                        {
                            newTerritory.GradientType = gradient;
                            rgb.RemoveAt(tf);
                            break;
                        }
                    }

                    // find exp value
                    for (int tf = 0; tf < rgb.Count; ++tf)
                    {
                        if (rgb[tf].ToLower().Contains("exp:"))
                        {
                            string val = rgb[tf].Split(':')[1];
                            if (!float.TryParse(val, NumberStyles.AllowDecimalPoint,
                                    CultureInfo.InvariantCulture, out float exp)) continue;
                            newTerritory.ExponentialValue = exp;
                            rgb.RemoveAt(tf);
                            break;
                        }
                    }

                    // find specify height override
                    for (int tf = 0; tf < rgb.Count; ++tf)
                    {
                        if (rgb[tf].ToLower().Contains("heightbounds:"))
                        {
                            string val = rgb[tf].Split(':')[1];
                            string[] split = val.Split('-');
                            newTerritory.HeightBounds = new(int.Parse(split[0]), int.Parse(split[1]));
                            rgb.RemoveAt(tf);
                            break;
                        }
                    }

                    newTerritory.Colors = new();
                    for (int tf = 0; tf < rgb.Count; tf += 3)
                    {
                        try
                        {
                            newTerritory.Colors.Add(new Color32
                            {
                                r = Convert.ToByte(rgb[tf]),
                                g = Convert.ToByte(rgb[tf + 1]),
                                b = Convert.ToByte(rgb[tf + 2])
                            });
                        }
                        catch
                        {
                            break;
                        }
                    }
                    
                    TerritorySystem_DataTypes.TerritoryFlags flags = TerritorySystem_DataTypes.TerritoryFlags.None;
                    TerritorySystem_DataTypes.AdditionalTerritoryFlags additionalflags =
                        TerritorySystem_DataTypes.AdditionalTerritoryFlags.None;
                    string territoryFlags = TerritoriesOverridesData.ContainsKey(newTerritory.Name) && !string.IsNullOrWhiteSpace(TerritoriesOverridesData[newTerritory.Name].TerritoryFlags)
                        ? TerritoriesOverridesData[newTerritory.Name].TerritoryFlags
                        : profiles[i + 3];
                    string[] splitFlags = territoryFlags.ReplaceSpacesOutsideQuotes().Split(',');
                    foreach (string flag in splitFlags)
                    {
                        string customData = "";
                        string[] split = flag.Split('=');
                        string workingFlag = split[0];
                        if (split.Length == 2)
                        {
                            customData = split[1];
                        }
                        
                        if (Enum.TryParse(workingFlag, true, out TerritorySystem_DataTypes.AdditionalTerritoryFlags testAdditionalFlag) &&
                            Enum.IsDefined(typeof(TerritorySystem_DataTypes.AdditionalTerritoryFlags),
                                testAdditionalFlag))
                        {
                            additionalflags |= testAdditionalFlag;
                            switch (testAdditionalFlag) 
                            {
                                case TerritorySystem_DataTypes.AdditionalTerritoryFlags.ForceWind:
                                    newTerritory.Wind = Convert.ToSingle(customData, new CultureInfo("en-US"));
                                    break;
                                case TerritorySystem_DataTypes.AdditionalTerritoryFlags.DropMultiplier:
                                    newTerritory.DropMultiplier = Convert.ToSingle(customData, new CultureInfo("en-US"));
                                    break;
                                case TerritorySystem_DataTypes.AdditionalTerritoryFlags.OnlyForGuild:
                                    newTerritory.OnlyForGuild = customData;
                                    break;
                                case TerritorySystem_DataTypes.AdditionalTerritoryFlags.CustomJereSpawner:
                                    newTerritory.CustomSpawnerNames.Add(customData);
                                    break;
                                case TerritorySystem_DataTypes.AdditionalTerritoryFlags.JoinOtherServer:
                                    newTerritory.JoinOtherServerString = customData;
                                    break;
                            }
                            continue;
                        }

                        if (!(Enum.TryParse(workingFlag, true, out TerritorySystem_DataTypes.TerritoryFlags testFlag) &&
                              Enum.IsDefined(typeof(TerritorySystem_DataTypes.TerritoryFlags), testFlag))) continue;
                        flags |= testFlag;
                        switch (testFlag) 
                        {
                            case TerritorySystem_DataTypes.TerritoryFlags.CustomEnvironment:
                                newTerritory.CustomEnvironment = customData.Split(',');
                                break;
                            case TerritorySystem_DataTypes.TerritoryFlags.PeriodicDamage:
                                newTerritory.PeriodicDamageValue = Convert.ToSingle(customData, new CultureInfo("en-US"));
                                break;
                            case TerritorySystem_DataTypes.TerritoryFlags.PeriodicHealALL:
                            case TerritorySystem_DataTypes.TerritoryFlags.PeriodicHeal:
                                newTerritory.PeriodicHealValue = Convert.ToSingle(customData, new CultureInfo("en-US"));
                                break;
                            case TerritorySystem_DataTypes.TerritoryFlags.IncreasedMonsterDamage:
                                newTerritory.IncreasedMonsterDamageValue = Convert.ToSingle(customData, new CultureInfo("en-US"));
                                break;
                            case TerritorySystem_DataTypes.TerritoryFlags.IncreasedPlayerDamage:
                                newTerritory.IncreasedPlayerDamageValue = Convert.ToSingle(customData, new CultureInfo("en-US"));
                                break;
                            case TerritorySystem_DataTypes.TerritoryFlags.MoveSpeedMultiplier:
                                newTerritory.MoveSpeedMultiplier = Convert.ToSingle(customData, new CultureInfo("en-US"));
                                break;
                            case TerritorySystem_DataTypes.TerritoryFlags.ForceGroundHeight
                                or TerritorySystem_DataTypes.TerritoryFlags.AddGroundHeight
                                or TerritorySystem_DataTypes.TerritoryFlags.LimitZoneHeight:
                                newTerritory.OverridenHeight = Convert.ToSingle(customData, new CultureInfo("en-US"));
                                break;
                            case TerritorySystem_DataTypes.TerritoryFlags.ForceBiome:
                                newTerritory.OverridenBiome = Convert.ToInt32(customData, new CultureInfo("en-US"));
                                break;
                            case TerritorySystem_DataTypes.TerritoryFlags.MonstersAddStars:
                                newTerritory.AddMonsterLevel = Convert.ToInt32(customData, new CultureInfo("en-US"));
                                break;
                            case TerritorySystem_DataTypes.TerritoryFlags.CustomPaint:
                                newTerritory.PaintGround =
                                    (TerritorySystem_DataTypes.PaintType)Convert.ToInt32(customData,
                                        new CultureInfo("en-US"));
                                break;
                        }
                    }
                    newTerritory.Permitted = string.IsNullOrEmpty(profiles[i + 4]) ? "None" : profiles[i + 4].Replace(" ", "");
                    newTerritory.Flags = flags;
                    newTerritory.AdditionalFlags = additionalflags;
                    TerritorySystem_DataTypes.SyncedTerritoriesData.Value.Territories.Add(newTerritory);
                }
                catch (Exception ex)
                {
                    Utils.print($"Error loading zone profile {splitProfile} in {fPath}:\n{ex}", ConsoleColor.Red);
                }
            }
        }
    }

    private struct TerritoryCompareData
    {
        public Color32[] Colors;
        public int[] X, Y;
        public int Radius;
        public bool ShowOnMap;
        public TerritorySystem_DataTypes.TerritoryShape Shape;
        private bool HasReveal;

        public TerritoryCompareData(TerritorySystem_DataTypes.TerritoryShape shape, Color32[] c, int[] x, int[] y, int radius, bool showOnMap, bool reveal)
        {
            Colors = c;
            X = x;
            Y = y;
            Radius = radius;
            ShowOnMap = showOnMap;
            Shape = shape;
            HasReveal = reveal;
        }
 
        public bool Equals(TerritoryCompareData other)
        {
            return false;
            
            if (Shape != other.Shape || Colors.Length != other.Colors.Length || X.Length != other.X.Length || Y.Length != other.Y.Length)
            {
                return false;
            }
            
            if (HasReveal != other.HasReveal)
            {
                return false;
            }
            
            for (int i = 0; i < Colors.Length; ++i)
            {
               if (!Colors[i].Equals(other.Colors[i]))
               {
                   return false;
               }
            }

            for (int i = 0; i < X.Length; ++i)
            {
                if (X[i] != other.X[i])
                {
                    return false;
                }
            }

            for (int i = 0; i < Y.Length; ++i)
            {
                if (Y[i] != other.Y[i])
                {
                    return false;
                }
            }
            
            if (Radius != other.Radius)
            {
                return false;
            }
            
            if (ShowOnMap != other.ShowOnMap)
            {
                return false;
            }

            return true;
        }
        
        public static bool IsEqual(TerritoryCompareData[] a, TerritoryCompareData[] b)
        {
            if (a == null || b == null) return false;
            if (a.Length != b.Length)
            {
                return false;
            }

            for (int i = 0; i < a.Length; ++i)
            {
                if (!a[i].Equals(b[i]))
                {
                    return false;
                }
            }

            return true;
        }
    }
    private static TerritoryCompareData[] GetTerritoryCompareData()
    {
        TerritoryCompareData[] compare = new TerritoryCompareData[TerritorySystem_DataTypes.SyncedTerritoriesData.Value.Territories.Count];
        for (int i = 0; i < TerritorySystem_DataTypes.SyncedTerritoriesData.Value.Territories.Count; ++i)
        {
            TerritorySystem_DataTypes.Territory territory = TerritorySystem_DataTypes.SyncedTerritoriesData.Value.Territories[i];
            compare[i] = new TerritoryCompareData(territory.Shape, territory.Colors.ToArray(), territory.X, territory.Y, territory.Radius, territory.ShowExternalWater, territory.AdditionalFlags.HasFlagFast(TerritorySystem_DataTypes.AdditionalTerritoryFlags.RevealOnMap));
        }
        return compare;
    }
    
    private static void ReadServerTerritoryDatabase()
    {
        TerritoryCompareData[] before = GetTerritoryCompareData();
        TerritorySystem_DataTypes.SyncedTerritoriesData.Value.Territories.Clear();
        string folder = Market_Paths.TerritoriesFolder;
        string[] files = Directory.GetFiles(folder, "*.cfg", SearchOption.AllDirectories);
        foreach (string file in files)
        {
            IReadOnlyList<string> profiles = File.ReadAllLines(file).ToList();
            ProcessTerritoryConfig(file, profiles);
        }
        TerritorySystem_DataTypes.SyncedTerritoriesData.Value.Territories.AddRange(TerritoriesForSaleData);
        TerritorySystem_DataTypes.SyncedTerritoriesData.Value.Territories.Sort((x, y) => y.Priority.CompareTo(x.Priority));
        TerritoryCompareData[] after = GetTerritoryCompareData();
        TerritorySystem_DataTypes.SyncedTerritoriesData.Value.ShouldRedraw = !TerritoryCompareData.IsEqual(before, after);
        TerritorySystem_DataTypes.SyncedTerritoriesData.Update();
    }
    
    private static void SaveForSaleTerritories()
    {
        List<TerritorySystem_DataTypes.Territory> toSave = [];
        foreach (TerritorySystem_DataTypes.Territory territory in TerritorySystem_DataTypes.SyncedTerritoriesData.Value.Territories)
        {
            if (territory.Type is (TerritorySystem_DataTypes.TerritoryType.ForSale)) toSave.Add(territory);
        }
        File.WriteAllText(TerritoriesForSale, new YamlDotNet.Serialization.Serializer().Serialize(toSave));
    }
    
    public static void AddOverrideFlagAndSave(string territoryName, string flag, string color)
    {
        TerritoriesOverridesData[territoryName] = new TerritoryOverride
        {
            TerritoryFlags = flag,
            TerritoryColor = color
        };
        File.WriteAllText(TerritoryOverrides, new YamlDotNet.Serialization.Serializer().Serialize(TerritoriesOverridesData));
        OnTerritoryConfigChange();
    }

    [MarketplaceRPC("KGmarket TerritorySystem AddTerritory", Market_Autoload.Type.Server)]
    private static void AddTerritory(long sender, ZPackage pkg)
    {
        ZNetPeer peer = ZNet.instance.GetPeer(sender);
        if (peer == null) return;
        TerritorySystem_DataTypes.Territory territory = new();
        territory.Shape = (TerritorySystem_DataTypes.TerritoryShape)pkg.ReadInt();
        territory.X = new[] { pkg.ReadInt() };
        territory.Y = new[] { pkg.ReadInt() };
        territory.Radius = pkg.ReadInt();
        territory.Colors = new List<Color32>
        {
            new Color32(pkg.ReadByte(), pkg.ReadByte(), pkg.ReadByte(), 255)
        };
        territory.Type = TerritorySystem_DataTypes.TerritoryType.ForSale;
        territory.SellPrice = pkg.ReadInt();
        territory.ShowExternalWater = false;
        territory.Name = "Free Territory";
        TerritorySystem_DataTypes.SyncedTerritoriesData.Value.Territories.Add(territory);
        SaveForSaleTerritories();
        TerritorySystem_DataTypes.SyncedTerritoriesData.Value.ShouldRedraw = true;
        TerritorySystem_DataTypes.SyncedTerritoriesData.Update();
    }
    
    [MarketplaceRPC("KGmarket TerritorySystem RemoveTerritory", Market_Autoload.Type.Server)]
    private static void RemoveTerritory(long sender, ZPackage pkg)
    {
        ZNetPeer peer = ZNet.instance.GetPeer(sender);
        if (peer == null) return;
        int uid = pkg.ReadInt();
        TerritorySystem_DataTypes.Territory tryFind = TerritorySystem_DataTypes.SyncedTerritoriesData.Value.Territories.Find(t => t.Type is TerritorySystem_DataTypes.TerritoryType.ForSale && t.UID == uid);
        if (tryFind != null)
        {
            TerritorySystem_DataTypes.SyncedTerritoriesData.Value.Territories.Remove(tryFind);
            SaveForSaleTerritories();
            TerritorySystem_DataTypes.SyncedTerritoriesData.Value.ShouldRedraw = true;
            TerritorySystem_DataTypes.SyncedTerritoriesData.Update();
        }
    }
    
    [MarketplaceRPC("KGmarket TerritorySystem GetOwnership", Market_Autoload.Type.Server)]
    private static void BuyTerritory(long sender, int uid)
    {
        ZNetPeer peer = ZNet.instance.GetPeer(sender);
        if (peer == null) return;
        TerritorySystem_DataTypes.Territory tryFind = TerritorySystem_DataTypes.SyncedTerritoriesData.Value.Territories.Find(t => t.Type is TerritorySystem_DataTypes.TerritoryType.ForSale && t.UID == uid);
        if (tryFind != null && string.IsNullOrWhiteSpace(tryFind.Owner))
        {
            tryFind.Owner = peer.m_socket.GetHostName();
            SaveForSaleTerritories();
            TerritorySystem_DataTypes.SyncedTerritoriesData.Value.ShouldRedraw = false;
            TerritorySystem_DataTypes.SyncedTerritoriesData.Update();
        }
    }
    
    [MarketplaceRPC("KGmarket TerritorySystem CancelOwnership", Market_Autoload.Type.Server)]
    private static void SellTerritory(long sender, ZPackage pkg)
    {
        ZNetPeer peer = ZNet.instance.GetPeer(sender);
        if (peer == null) return;
        int uid = pkg.ReadInt();
        TerritorySystem_DataTypes.Territory tryFind = TerritorySystem_DataTypes.SyncedTerritoriesData.Value.Territories.Find(t => t.Type is TerritorySystem_DataTypes.TerritoryType.ForSale && t.UID == uid);
        if (tryFind != null && tryFind.Owner == peer.m_socket.GetHostName())
        {
            tryFind.Owner = "";
            SaveForSaleTerritories();
            TerritorySystem_DataTypes.SyncedTerritoriesData.Value.ShouldRedraw = false;
            TerritorySystem_DataTypes.SyncedTerritoriesData.Update();
        }
    }

    public static void CreateOrUpdateTerritory(string name, int x, int y, Color32 color, TerritorySystem_DataTypes.TerritoryShape shape, int radius, TerritorySystem_DataTypes.TerritoryFlags flags, TerritorySystem_DataTypes.AdditionalTerritoryFlags flags2, string owners)
    {
        TerritorySystem_DataTypes.Territory[] forSale = TerritorySystem_DataTypes.SyncedTerritoriesData.Value.Territories.Where(t => t.Type is TerritorySystem_DataTypes.TerritoryType.ForSale).ToArray();
        TerritorySystem_DataTypes.Territory tryFind = forSale.FirstOrDefault(t => t.Name == name);
        if (tryFind != null)
        {
            tryFind.X = new[] { x };
            tryFind.Y = new[] { y };
            tryFind.Shape = shape;
            tryFind.Radius = radius;
            tryFind.Colors = new List<Color32> { color };
            tryFind.Flags = flags;
            tryFind.AdditionalFlags = flags2;
            tryFind.Owner = owners;
            tryFind.Type = TerritorySystem_DataTypes.TerritoryType.ForSale;
            SaveForSaleTerritories();
            TerritorySystem_DataTypes.SyncedTerritoriesData.Value.ShouldRedraw = true;
            TerritorySystem_DataTypes.SyncedTerritoriesData.Update();
        }
        else
        {
            TerritorySystem_DataTypes.Territory territory = new()
            {
                Name = name,
                X = [x],
                Y = [y],
                Shape = shape,
                Radius = radius,
                Flags = flags,
                AdditionalFlags = flags2,
                Owner = owners,
                Colors = [color],
                ShowExternalWater = false,
                Type = TerritorySystem_DataTypes.TerritoryType.ForSale
            };
            TerritorySystem_DataTypes.SyncedTerritoriesData.Value.Territories.Add(territory);
        }
    }
}