using Marketplace.Modules.Global_Options;
using Random = UnityEngine.Random;

namespace Marketplace.Modules.TerritorySystem;

public static class TerritorySystem_DataTypes
{
    public static readonly CustomSyncedValue<TerritoryContainer> SyncedTerritoriesData = new(Marketplace.configSync, "territoryData", new TerritoryContainer());
    public static readonly TerritoryFlags[] AllTerritoryFlagsArray = (TerritoryFlags[])Enum.GetValues(typeof(TerritoryFlags));
    public static readonly AdditionalTerritoryFlags[] AllAdditionaTerritoryFlagsArray = (AdditionalTerritoryFlags[])Enum.GetValues(typeof(AdditionalTerritoryFlags));

    //serialization part

    public class TerritoryContainer : ISerializableParameter
    {
        public List<Territory> Territories = new();
        public bool ShouldRedraw;
        
        public void Serialize(ref ZPackage pkg)
        {
            pkg.Write(Territories.Count);
            foreach (Territory territory in Territories)
            {
                territory.Serialize(ref pkg);
            }
            pkg.Write(ShouldRedraw);
        }
        public void Deserialize(ref ZPackage pkg)
        {
            int count = pkg.ReadInt();
            Territories = new List<Territory>();
            for (int i = 0; i < count; ++i)
            {
                Territory territory = new();
                territory.Deserialize(ref pkg);
                Territories.Add(territory);
            }
            ShouldRedraw = pkg.ReadBool();
        }
    }
    
    public partial class Territory : ISerializableParameter
    {
        public void Serialize(ref ZPackage pkg)
        {
            pkg.Write((int)Shape);
            pkg.Write((int)Flags);
            pkg.Write((int)AdditionalFlags);

            pkg.Write(X.Length);
            foreach (int x in X)
            {
                pkg.Write(x);
            }
            pkg.Write(Y.Length);
            foreach (int y in Y)
            {
                pkg.Write(y);
            }
            
            pkg.Write(Name ?? "");
            pkg.Write(Colors.Count);
            foreach (Color32 color in Colors)
            {
                pkg.Write(color.r);
                pkg.Write(color.g);
                pkg.Write(color.b);
            }

            pkg.Write(Radius);
            pkg.Write(ShowExternalWater);
            pkg.Write(Priority);
            pkg.Write(PeriodicHealValue);
            pkg.Write(PeriodicDamageValue);
            pkg.Write(IncreasedPlayerDamageValue);
            pkg.Write(IncreasedMonsterDamageValue);
            pkg.Write(CustomEnvironment?.Length ?? 0);
            if (CustomEnvironment != null)
            {
                foreach (string env in CustomEnvironment)
                {
                    pkg.Write(env ?? "");
                }
            }
            pkg.Write(MoveSpeedMultiplier);
            pkg.Write(Permitted ?? "");
            pkg.Write(OverridenHeight);
            pkg.Write(OverridenBiome);
            pkg.Write(AddMonsterLevel);
            pkg.Write((int)PaintGround);
            pkg.Write((int)GradientType);
            pkg.Write(ExponentialValue);
            pkg.Write(HeightBounds.Item1);
            pkg.Write(HeightBounds.Item2);
            pkg.Write(Wind);
            pkg.Write(DropMultiplier);
            pkg.Write(OnlyForGuild ?? "");
            pkg.Write(CustomSpawnerNames.Count);
            foreach (string customSpawnerName in CustomSpawnerNames)
            {
                pkg.Write(customSpawnerName);
            }
            pkg.Write((int)Type);
            pkg.Write(Owner ?? "");
            pkg.Write(SellPrice);
            pkg.Write(UID);
            pkg.Write(JoinOtherServerString ?? "");
        }

        public void Deserialize(ref ZPackage pkg)
        {
            Shape = (TerritoryShape)pkg.ReadInt();
            Flags = (TerritoryFlags)pkg.ReadInt();
            AdditionalFlags = (AdditionalTerritoryFlags)pkg.ReadInt();
            
            int xCount = pkg.ReadInt();
            X = new int[xCount];
            for (int i = 0; i < xCount; ++i)
            {
                X[i] = pkg.ReadInt();
            }
            int yCount = pkg.ReadInt();
            Y = new int[yCount];
            for (int i = 0; i < yCount; ++i)
            {
                Y[i] = pkg.ReadInt();
            }
            
            Name = pkg.ReadString();
            int colorCount = pkg.ReadInt();
            Colors = new List<Color32>();
            for (int i = 0; i < colorCount; ++i)
            {
                Colors.Add(new Color32(pkg.ReadByte(), pkg.ReadByte(), pkg.ReadByte(), 255));
            }

            Radius = pkg.ReadInt();
            ShowExternalWater = pkg.ReadBool();
            Priority = pkg.ReadInt();
            PeriodicHealValue = pkg.ReadSingle();
            PeriodicDamageValue = pkg.ReadSingle();
            IncreasedPlayerDamageValue = pkg.ReadSingle();
            IncreasedMonsterDamageValue = pkg.ReadSingle();
            
            int customEnvironmentCount = pkg.ReadInt();
            CustomEnvironment = new string[customEnvironmentCount];
            for (int i = 0; i < customEnvironmentCount; ++i) CustomEnvironment[i] = pkg.ReadString();
            MoveSpeedMultiplier = pkg.ReadSingle();
            Permitted = pkg.ReadString();
            OverridenHeight = pkg.ReadSingle();
            OverridenBiome = pkg.ReadInt();
            AddMonsterLevel = pkg.ReadInt();
            PaintGround = (PaintType)pkg.ReadInt();
            GradientType = (GradientType)pkg.ReadInt();
            ExponentialValue = pkg.ReadSingle();
            HeightBounds = new ValueTuple<int, int>(pkg.ReadInt(), pkg.ReadInt());
            Wind = pkg.ReadSingle();
            DropMultiplier = pkg.ReadSingle();
            OnlyForGuild = pkg.ReadString();
            int customSpawnerCount = pkg.ReadInt();
            CustomSpawnerNames = new HashSet<string>();
            for (int i = 0; i < customSpawnerCount; ++i)
            {
                CustomSpawnerNames.Add(pkg.ReadString());
            }
            Type = (TerritoryType)pkg.ReadInt();
            Owner = pkg.ReadString();
            SellPrice = pkg.ReadInt();
            UID = pkg.ReadInt();
            JoinOtherServerString = pkg.ReadString();
        }
    }

    //main part
    public partial class Territory
    {
        [NonSerialized] public static Territory LastTerritory;
        public TerritoryShape Shape;
        public TerritoryFlags Flags;
        public AdditionalTerritoryFlags AdditionalFlags;
        public int[] X;
        public int[] Y;
        public string Name = "";
        public List<Color32> Colors = new();
        public string Permitted = "";
        public int Radius;
        public bool ShowExternalWater = true;
        public int Priority;
        public GradientType GradientType = GradientType.LeftRight;
        public float ExponentialValue = 1f;
        public string OnlyForGuild = null;
        
        public int UID = Random.Range(int.MinValue, int.MaxValue);

        public float PeriodicHealValue;
        public float PeriodicDamageValue;
        public float IncreasedPlayerDamageValue;
        public float IncreasedMonsterDamageValue;
        public string[] CustomEnvironment;
        public float MoveSpeedMultiplier;
        public float OverridenHeight;
        public int OverridenBiome;
        public int AddMonsterLevel;
        public float Wind;
        public float DropMultiplier;
        public PaintType PaintGround;
        public HashSet<string> CustomSpawnerNames = new HashSet<string>();
        public string JoinOtherServerString = "";

        public TerritoryType Type;
        public string Owner = "";
        public int SellPrice;

        public ValueTuple<int, int> HeightBounds = new ValueTuple<int, int>(-100000, 100000);

        public bool DrawOnMap() => Colors.Count > 0 && Vector2.Distance(new Vector2(0, 0), new Vector2(X[0], Y[0])) <= 12500;
        public bool UsingGradient() => Shape is not TerritoryShape.Rectangle && Colors.Count > 1;

        public static Territory GetCurrentTerritory(Vector3 pos)
        {
            foreach (Territory territory in SyncedTerritoriesData.Value.Territories)
            {
                if (territory.IsInside(pos))
                {
                    return territory;
                }
            }

            return null;
        }
        
        public Vector3 Pos3D()
        {
            return new Vector3(X[0], (HeightBounds.Item1 + HeightBounds.Item2) / 2f, Y[0]);
        }

        public Color32 GetColor()
        {
            return Colors.Count > 0 ? Colors[0] : Color.green;
        }

        private Color32 CalculateGradient(float t)
        {
            switch (t)
            {
                case <= 0f:
                    return Colors[0];
                case >= 1f:
                    return Colors[Colors.Count - 1];
            }

            float intervalSize = 1f / (Colors.Count - 1);
            int index = Mathf.FloorToInt(t / intervalSize);
            float tInInterval = (t - index * intervalSize) / intervalSize;
            float expedValue = 1 - Mathf.Pow(1 - tInInterval, ExponentialValue);
            return Color32.Lerp(Colors[index], Colors[index + 1], expedValue);
        }

        public Color32 GetGradientX(float x, bool reverse = false)
        {
            float startX = X[0] - Radius;
            float endX = X[0] + Radius;
            float t = Mathf.Clamp01((x - startX) / (endX - startX));
            if (reverse) t = 1 - t;
            return CalculateGradient(t);
        }

        public Color32 GetGradientY(float y, bool reverse = false)
        {
            float startY = Y[0] - Radius;
            float endY = Y[0] + Radius;
            float t = Mathf.Clamp01((y - startY) / (endY - startY));
            if (reverse) t = 1 - t;
            return CalculateGradient(t);
        }

        public Color32 GetGradientXY(Vector2 pos, bool reverse = false)
        {
            float startX = X[0] - Radius;
            float endX = X[0] + Radius;
            float startY = Y[0] - Radius;
            float endY = Y[0] + Radius;
            float tX = Mathf.Clamp01((pos.x - startX) / (endX - startX));
            float tY = Mathf.Clamp01((pos.y - startY) / (endY - startY));
            float t = (tX + tY) / 2;
            if (reverse) t = 1 - t;
            return CalculateGradient(t);
        }

        public Color32 GetGradientXY_2(Vector2 pos, bool reverse = false)
        {
            float startX = X[0] + Radius;
            float endX = X[0] - Radius;
            float startY = Y[0] - Radius;
            float endY = Y[0] + Radius;
            float tX = Mathf.Clamp01((pos.x - startX) / (endX - startX));
            float tY = Mathf.Clamp01((pos.y - startY) / (endY - startY));
            float t = (tX + tY) / 2;
            if (reverse) t = 1 - t;
            return CalculateGradient(t);
        }

        public Color32 GetGradientFromCenter(Vector2 pos, bool reverse = false)
        {
            float t = Mathf.Clamp01(Vector2.Distance(pos, new Vector2(X[0], Y[0])) / Radius);
            if (reverse) t = 1 - t;
            return CalculateGradient(t);
        }

        public string RawName()
        {
            return Name;
        }

        public string GetName()
        {
            return $"{Name}\n{(IsPermitted() ? "<color=#00ff00>Permitted</color>\n" : "")}";
        }

        private bool? IsPermittedCache;

        public bool IsPermitted()
        {
            if (Utils.IsDebug_Strict) return true;
            IsPermittedCache ??= Owner == Global_Configs._localUserID || Permitted.Contains(Global_Configs._localUserID) || Permitted.Contains("ALL") ||
                              (Guilds.API.GetOwnGuild() != null && Guilds.API.GetOwnGuild().Name == this.OnlyForGuild);
            return IsPermittedCache.Value;
        }
        
        public void ClearOwnerCache() => IsPermittedCache = null;

        public bool IsInside2D(Vector2 mouse) => IsInside2D(mouse.x, mouse.y);
        
        public bool IsInside2D(float x, float y)
        {
          
            switch (Shape)
            {
                case TerritoryShape.Square:
                    return x >= X[0] - Radius && x <= X[0] + Radius && y >= Y[0] - Radius &&
                           y <= Y[0] + Radius;
                case TerritoryShape.Circle:
                    return Vector2.Distance(new Vector2(X[0], Y[0]), new Vector2(x,y)) <= Radius;
                case TerritoryShape.Rectangle:
                    return x >= X[0] && x <= X[0] + X[1] && y >= Y[0] && y <= Y[0] + Y[1];
                default: return false;
            }
        }
        

        public bool IsInside(Vector3 init)
        {
            return IsInside2D(init.x, init.z) && init.y >= HeightBounds.Item1 && init.y <= HeightBounds.Item2;
        }

        public override string ToString()
        {
            return
                $"Name: {Name}, Type: {Shape}, Flags: {Flags}, values:\nHeal: {PeriodicHealValue}, Damage: {PeriodicDamageValue}, MoveSpeed: {MoveSpeedMultiplier}, PlayerDamage: {IncreasedPlayerDamageValue}, MonsterDamage: {IncreasedMonsterDamageValue}, CustomEnvironment: {CustomEnvironment}\nOwners: {Permitted}";
        }

        public string GetTerritoryFlags()
        {
            string ret = $"";

            if (Type is TerritoryType.ForSale && string.IsNullOrWhiteSpace(Owner))
            {
                ret += $"\n<color=#00FFFF>Can be bought for {SellPrice} coins</color>\n";
            }
            
            if (HeightBounds.Item1 != int.MinValue && HeightBounds.Item2 != int.MaxValue)
                ret += $"\n(Height Bounds: {HeightBounds.Item1} - {HeightBounds.Item2})\n";
            foreach (TerritoryFlags flag in AllTerritoryFlagsArray)
            {
                if (!Flags.HasFlagFast(flag)) continue;
                if (LocalizedTerritoryFlags.TryGetValue(flag, out string territoryFlag))
                    ret += Localization.instance.Localize(territoryFlag);
            }

            foreach (AdditionalTerritoryFlags flag in AllAdditionaTerritoryFlagsArray)
            {
                if (!AdditionalFlags.HasFlagFast(flag)) continue;
                if (LocalizedAdditionalTerritoryFlags.TryGetValue(flag, out string territoryFlag))
                    ret += Localization.instance.Localize(territoryFlag);
            }

            return ret;
        }
    }
    
    public enum TerritoryShape
    {
        Circle,
        Square,
        Rectangle
    }

    public enum TerritoryType
    {
        AdminCreated,
        ForSale
    }

    public enum GradientType
    {
        FromCenter,
        ToCenter,
        LeftRight,
        RightLeft,
        TopBottom,
        BottomTop,
        BottomRightTopLeft,
        TopRightBottomLeft,
        BottomLeftTopRight,
        TopLeftBottomRight
    }

    public enum PaintType
    {
        Paved,
        Grass,
        Cultivated,
        Dirt
    }

    private static readonly Dictionary<TerritoryFlags, string> LocalizedTerritoryFlags = new()
    {
        { TerritoryFlags.NoBuildDamage, "\n<color=#00FFFF>$mpasn_nobuilddamage</color>" },
        { TerritoryFlags.PushAway, "\n<color=#00FFFF>$mpasn_pushaway</color>" },
        { TerritoryFlags.NoBuild, "\n<color=#00FFFF>$mpasn_cantbuild</color>" },
        { TerritoryFlags.NoPickaxe, "\n<color=#00FFFF>$mpasn_cantusepickaxe</color>" },
        { TerritoryFlags.NoInteract, "\n<color=#00FFFF>$mpasn_nointeractions</color>" },
        { TerritoryFlags.NoAttack, "\n<color=#00FFFF>$mpasn_cantattack</color>" },
        { TerritoryFlags.PvpOnly, "\n<color=#00FFFF>$mpasn_pvponly</color>" },
        { TerritoryFlags.PveOnly, "\n<color=#00FFFF>$mpasn_pveonly</color>" },
        { TerritoryFlags.PeriodicHealALL, "\n<color=#00FFFF>$mpasn_periodichealALL</color>" },
        { TerritoryFlags.PeriodicHeal, "\n<color=#00FFFF>$mpasn_periodicheal</color>" },
        { TerritoryFlags.PeriodicDamage, "\n<color=#00FFFF>$mpasn_periodicdamage</color>" },
        { TerritoryFlags.IncreasedPlayerDamage, "\n<color=#00FFFF>$mpasn_increasedamagePlayer</color>" },
        { TerritoryFlags.IncreasedMonsterDamage, "\n<color=#00FFFF>$mpasn_increasedamageMonsters</color>" },
        { TerritoryFlags.NoMonsters, "\n<color=#00FFFF>$mpasn_nomonsters</color>" },
        { TerritoryFlags.MoveSpeedMultiplier, "\n<color=#00FFFF>$mpasn_movementspeedmultiplier</color>" },
        { TerritoryFlags.NoDeathPenalty, "\n<color=#00FFFF>$mpasn_nodeathpenalty</color>" },
        { TerritoryFlags.NoPortals, "\n<color=#00FFFF>$mpasn_noportals</color>" },
        { TerritoryFlags.InfiniteFuel, "\n<color=#00FFFF>$mpasn_infinitefuel</color>" },
        { TerritoryFlags.NoInteractItems, "\n<color=#00FFFF>$mpasn_nointeractitems</color>" },
        { TerritoryFlags.NoInteractCraftingStation, "\n<color=#00FFFF>$mpasn_nointeractcraftingstations</color>" },
        { TerritoryFlags.NoInteractItemStands, "\n<color=#00FFFF>$mpasn_nointeractitemstands</color>" },
        { TerritoryFlags.NoInteractChests, "\n<color=#00FFFF>$mpasn_nointeractchests</color>" },
        { TerritoryFlags.NoInteractDoors, "\n<color=#00FFFF>$mpasn_nointeractdoors</color>" },
        { TerritoryFlags.NoStructureSupport, "\n<color=#00FFFF>$mpasn_nostructuresupport</color>" },
        { TerritoryFlags.NoInteractPortals, "\n<color=#00FFFF>$mpasn_nointeractportals</color>" },
    };

    private static readonly Dictionary<AdditionalTerritoryFlags, string> LocalizedAdditionalTerritoryFlags = new()
    {
        { AdditionalTerritoryFlags.NoItemLoss, "\n<color=#00FFFF>$mpasn_noitemloss</color>" },
        { AdditionalTerritoryFlags.InfiniteEitr, "\n<color=#00FFFF>$mpasn_infiniteeitr</color>" },
        { AdditionalTerritoryFlags.InfiniteStamina, "\n<color=#00FFFF>$mpasn_infinitestamina</color>" },
        { AdditionalTerritoryFlags.NoMist, "\n<color=#00FFFF>$mpasn_nomistlandsmist</color>" },
        { AdditionalTerritoryFlags.DropMultiplier, "\n<color=#00FFFF>$mpasn_dropmultiplier</color>" },
        { AdditionalTerritoryFlags.OnlyForGuild , "\n<color=#00FFFF>$mpasn_onlyforguildterritory</color>" },
    };


    [Flags]
    public enum TerritoryFlags
    {
        None = 0,
        PushAway = 1 << 0,
        NoBuild = 1 << 1,
        NoPickaxe = 1 << 2,
        NoInteract = 1 << 3,
        NoAttack = 1 << 4,
        PvpOnly = 1 << 5,
        PveOnly = 1 << 6,
        PeriodicHeal = 1 << 7,
        PeriodicDamage = 1 << 8,
        IncreasedPlayerDamage = 1 << 9,
        IncreasedMonsterDamage = 1 << 10,
        NoMonsters = 1 << 11,
        CustomEnvironment = 1 << 12,
        MoveSpeedMultiplier = 1 << 13,
        NoDeathPenalty = 1 << 14,
        NoPortals = 1 << 15,
        PeriodicHealALL = 1 << 16,
        ForceGroundHeight = 1 << 17,
        ForceBiome = 1 << 18,
        AddGroundHeight = 1 << 19,
        NoBuildDamage = 1 << 20,
        MonstersAddStars = 1 << 21,
        InfiniteFuel = 1 << 22,
        NoInteractItems = 1 << 23,
        NoInteractCraftingStation = 1 << 24,
        NoInteractItemStands = 1 << 25,
        NoInteractChests = 1 << 26,
        NoInteractDoors = 1 << 27,
        NoStructureSupport = 1 << 28,
        NoInteractPortals = 1 << 29,
        CustomPaint = 1 << 30,
        LimitZoneHeight = 1 << 31,
    }

    [Flags]
    public enum AdditionalTerritoryFlags
    {
        None = 0,
        NoItemLoss = 1 << 0,
        SnowMask = 1 << 1,
        NoMist = 1 << 2,
        InfiniteEitr = 1 << 3,
        InfiniteStamina = 1 << 4,
        DropMultiplier = 1 << 5,
        ForceWind = 1 << 6,
        GodMode = 1 << 7,
        OnlyForGuild = 1 << 8,
        CustomJereSpawner = 1 << 9,
        JoinOtherServer = 1 << 10,
        RevealOnMap = 1 << 11,
    }
}