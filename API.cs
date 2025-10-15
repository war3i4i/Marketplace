using Marketplace.Modules.Gambler;
using Marketplace.Modules.Quests;
using Marketplace.Modules.TerritorySystem;
using Marketplace.Modules.Trader;
using Marketplace.Modules.Transmogrification;

namespace API;

//if you want to use marketplace api just copy-paste this whole class into your code and use its methods
[PublicAPI]
public static class Marketplace_API
{
    private static readonly bool _IsInstalled;
    private static readonly MethodInfo MI_IsPlayerInsideTerritory;
    private static readonly MethodInfo MI_IsObjectInsideTerritoryWithFlag;
    private static readonly MethodInfo MI_IsObjectInsideTerritoryWithFlag_Additional;
    private static readonly MethodInfo MI_ResetTraderItems;
    private static readonly MethodInfo MI_OpenQuestJournal;

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
    }

    //uncomment that if you want to use HasFlagFast
    /*
    public static bool HasFlagFast(this TerritoryFlags flags, TerritoryFlags flag)
    {
        return (flags & flag) != 0;
    }
    public static bool HasFlagFast(this AdditionalTerritoryFlags flags, AdditionalTerritoryFlags flag)
    {
        return (flags & flag) != 0;
    }*/

    public static bool IsInstalled() => _IsInstalled;

    public static bool IsPlayerInsideTerritory(out string name, out TerritoryFlags flags,
        out AdditionalTerritoryFlags additionalFlags)
    {
        flags = 0;
        additionalFlags = 0;
        name = "";
        if (!_IsInstalled || MI_IsPlayerInsideTerritory == null)
            return false;

        object[] args = { "", 0, 0 };
        bool result = (bool)MI_IsPlayerInsideTerritory.Invoke(null, args);
        name = (string)args[0];
        flags = (TerritoryFlags)args[1];
        additionalFlags = (AdditionalTerritoryFlags)args[2];
        return result;
    }

    public static bool IsObjectInsideTerritoryWithFlag(GameObject go, TerritoryFlags flag, out string name,
        out TerritoryFlags flags, out AdditionalTerritoryFlags additionalFlags) =>
        IsPointInsideTerritoryWithFlag(go.transform.position, flag, out name, out flags, out additionalFlags);

    public static bool IsObjectInsideTerritoryWithFlag(GameObject go, AdditionalTerritoryFlags flag, out string name,
        out TerritoryFlags flags, out AdditionalTerritoryFlags additionalFlags) =>
        IsPointInsideTerritoryWithFlag(go.transform.position, flag, out name, out flags, out additionalFlags);

    public static bool IsPointInsideTerritoryWithFlag(Vector3 pos, TerritoryFlags flag, out string name,
        out TerritoryFlags flags, out AdditionalTerritoryFlags additionalFlags)
    {
        name = "";
        flags = 0;
        additionalFlags = 0;
        if (!_IsInstalled || MI_IsObjectInsideTerritoryWithFlag == null)
            return false;

        object[] args = { pos, (int)flag, "", 0, 0 };
        bool result = (bool)MI_IsObjectInsideTerritoryWithFlag.Invoke(null, args);
        name = (string)args[2];
        flags = (TerritoryFlags)args[3];
        additionalFlags = (AdditionalTerritoryFlags)args[4];
        return result;
    }

    public static bool IsPointInsideTerritoryWithFlag(Vector3 pos, AdditionalTerritoryFlags flag, out string name,
        out TerritoryFlags flags, out AdditionalTerritoryFlags additionalFlags)
    {
        name = "";
        flags = 0;
        additionalFlags = 0;
        if (!_IsInstalled || MI_IsObjectInsideTerritoryWithFlag_Additional == null)
            return false;

        object[] args = { pos, (int)flag, "", 0, 0 };
        bool result = (bool)MI_IsObjectInsideTerritoryWithFlag_Additional.Invoke(null, args);
        name = (string)args[2];
        flags = (TerritoryFlags)args[3];
        additionalFlags = (AdditionalTerritoryFlags)args[4];
        return result;
    }
    
    public static void ResetTraderItems()
    {
        if (!_IsInstalled || MI_ResetTraderItems == null)
            return;
        MI_ResetTraderItems.Invoke(null, null);
    }   
    
    public static void OpenQuestJournal()
    {
        if (!_IsInstalled || MI_OpenQuestJournal == null)
            return;
        MI_OpenQuestJournal.Invoke(null, null);
    }
    
    public enum API_NPCType { None, Trader, Info, Teleporter, Feedback, Banker, Gambler, Quests, Buffer, Transmog, Marketplace }
    public static void SET_NPC_Type(this ZDO zdo, API_NPCType type) => zdo.Set("KGmarketNPC", (int)type);
    public static void SET_NPC_Profile(this ZDO zdo, string profile) => zdo.Set("KGnpcProfile", profile);
    public static void SET_NPC_Model(this ZDO zdo, string prefab) => zdo.Set("KGnpcModelOverride", prefab);
    public static void SET_NPC_Name(this ZDO zdo, string name) => zdo.Set("KGnpcNameOverride", name);
    public static void SET_NPC_Dialogue(this ZDO zdo, string dialogue) => zdo.Set("KGnpcDialogue", dialogue);
    public static void SET_NPC_PatrolData(this ZDO zdo, string patrolData) => zdo.Set("KGmarket PatrolData", patrolData);
    
    public static void SET_NPC_LeftItem(this ZDO zdo, string prefab) => zdo.Set("KGleftItem", prefab);
    public static void SET_NPC_RightItem(this ZDO zdo, string prefab) => zdo.Set("KGrightItem", prefab);
    public static void SET_NPC_HelmetItem(this ZDO zdo, string prefab) => zdo.Set("KGhelmetItem", prefab);
    public static void SET_NPC_ChestItem(this ZDO zdo, string prefab) => zdo.Set("KGchestItem", prefab);
    public static void SET_NPC_LegsItem(this ZDO zdo, string prefab) => zdo.Set("KGlegsItem", prefab);
    public static void SET_NPC_CapeItem(this ZDO zdo, string prefab) => zdo.Set("KGcapeItem", prefab);
    public static void SET_NPC_HairItem(this ZDO zdo, string prefab) => zdo.Set("KGhairItem", prefab);
    public static void SET_NPC_HairColor(this ZDO zdo, string color) => zdo.Set("KGhairItemColor", color);
    public static void SET_NPC_LeftItemBack(this ZDO zdo, string prefab) => zdo.Set("KGLeftItemBack", prefab);
    public static void SET_NPC_RightItemBack(this ZDO zdo, string prefab) => zdo.Set("KGRightItemBack", prefab);
    public static void SET_NPC_InteractAnimation(this ZDO zdo, string animation) => zdo.Set("KGinteractAnimation", animation);
    public static void SET_NPC_GreetingAnimation(this ZDO zdo, string animation) => zdo.Set("KGgreetingAnimation", animation);
    public static void SET_NPC_ByeAnimation(this ZDO zdo, string animation) => zdo.Set("KGbyeAnimation", animation);
    public static void SET_NPC_GreetingText(this ZDO zdo, string text) => zdo.Set("KGgreetingText", text);
    public static void SET_NPC_ByeText(this ZDO zdo, string text) => zdo.Set("KGbyeText", text);
    public static void SET_NPC_SkinColor(this ZDO zdo, string color) => zdo.Set("KGskinColor", color);
    public static void SET_NPC_CraftingAnimation(this ZDO zdo, string animation) => zdo.Set("KGcraftingAnimation", animation);
    public static void SET_NPC_BeardItem(this ZDO zdo, string prefab) => zdo.Set("KGbeardItem", prefab);
    public static void SET_NPC_BeardColor(this ZDO zdo, string color) => zdo.Set("KGbeardColor", color);
    public static void SET_NPC_InteractSound(this ZDO zdo, string sound) => zdo.Set("KGinteractSound", sound);
    public static void SET_NPC_TextSize(this ZDO zdo, string size) => zdo.Set("KGtextSize", float.TryParse(size, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out float scaleFloat) ? scaleFloat : 3f);
    public static void SET_NPC_TextHeight(this ZDO zdo, string height) => zdo.Set("KGtextHeight", float.TryParse(height, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out float heightFloat) ? heightFloat : 0f);
    public static void SET_NPC_PeriodicAnimation(this ZDO zdo, string animation) => zdo.Set("KGperiodicAnimation", animation);
    public static void SET_NPC_PeriodicAnimationTime(this ZDO zdo, string time) => zdo.Set("KGperiodicAnimationTime", float.TryParse(time, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out float timeFloat) ? timeFloat : 0f);
    public static void SET_NPC_PeriodicSound(this ZDO zdo, string sound) => zdo.Set("KGperiodicSound", sound);
    public static void SET_NPC_PeriodicSoundTime(this ZDO zdo, string time) => zdo.Set("KGperiodicSoundTime", float.TryParse(time, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out float timeFloat) ? timeFloat : 0f);
    public static void SET_NPC_NPCScale(this ZDO zdo, string scale) => zdo.Set("KGnpcScale", float.TryParse(scale, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out float scaleFloat) ? scaleFloat : 1f);
    public static void SET_NPC_IsPinned(this ZDO zdo, bool pinned) => zdo.Set("KGmarketPinned", pinned);
    public static void SET_NPC_PinIcon(this ZDO zdo, string pinIcon) => zdo.Set("KGmarketPinIcon", string.IsNullOrWhiteSpace(pinIcon) ? "" : pinIcon);
    
    public static API_NPCType GET_NPC_Type(this ZDO zdo) => (API_NPCType)zdo.GetInt("KGmarketNPC"); 
    public static string GET_NPC_Profile(this ZDO zdo) => zdo.GetString("KGnpcProfile", "default");
    public static string GET_NPC_Model(this ZDO zdo) => zdo.GetString("KGnpcModelOverride");
    public static string GET_NPC_Name(this ZDO zdo) => zdo.GetString("KGnpcNameOverride");
    public static string GET_NPC_Dialogue(this ZDO zdo) => zdo.GetString("KGnpcDialogue");
    public static string GET_NPC_PatrolData(this ZDO zdo) => zdo.GetString("KGmarket PatrolData");
    public static bool GET_NPC_IsPinned(this ZDO zdo) => zdo.GetBool("KGmarketPinned");
    public static string GET_NPC_PinIcon(this ZDO zdo) => zdo.GetString("KGmarketPinIcon");
    
    public static string GET_NPC_LeftItem(this ZDO zdo) => zdo.GetString("KGleftItem");
    public static string GET_NPC_RightItem(this ZDO zdo) => zdo.GetString("KGrightItem");
    public static string GET_NPC_HelmetItem(this ZDO zdo) => zdo.GetString("KGhelmetItem");
    public static string GET_NPC_ChestItem(this ZDO zdo) => zdo.GetString("KGchestItem");
    public static string GET_NPC_LegsItem(this ZDO zdo) => zdo.GetString("KGlegsItem");
    public static string GET_NPC_CapeItem(this ZDO zdo) => zdo.GetString("KGcapeItem");
    public static string GET_NPC_HairItem(this ZDO zdo) => zdo.GetString("KGhairItem");
    public static string GET_NPC_HairColor(this ZDO zdo) => zdo.GetString("KGhairItemColor"); 
    public static string GET_NPC_LeftItemBack(this ZDO zdo) => zdo.GetString("KGLeftItemBack");
    public static string GET_NPC_RightItemBack(this ZDO zdo) => zdo.GetString("KGRightItemBack");
    public static string GET_NPC_InteractAnimation(this ZDO zdo) => zdo.GetString("KGinteractAnimation");
    public static string GET_NPC_GreetingAnimation(this ZDO zdo) => zdo.GetString("KGgreetingAnimation");
    public static string GET_NPC_ByeAnimation(this ZDO zdo) => zdo.GetString("KGbyeAnimation");
    public static string GET_NPC_GreetingText(this ZDO zdo) => zdo.GetString("KGgreetingText");
    public static string GET_NPC_ByeText(this ZDO zdo) => zdo.GetString("KGbyeText");
    public static string GET_NPC_SkinColor(this ZDO zdo) => zdo.GetString("KGskinColor");
    public static string GET_NPC_CraftingAnimation(this ZDO zdo) => zdo.GetString("KGcraftingAnimation");
    public static string GET_NPC_BeardItem(this ZDO zdo) => zdo.GetString("KGbeardItem");
    public static string GET_NPC_BeardColor(this ZDO zdo) => zdo.GetString("KGbeardColor");
    public static string GET_NPC_InteractSound(this ZDO zdo) => zdo.GetString("KGinteractSound");
    public static float GET_NPC_TextSize(this ZDO zdo) => zdo.GetFloat("KGtextSize", 3f);
    public static float GET_NPC_TextHeight(this ZDO zdo) => zdo.GetFloat("KGtextHeight");
    public static string GET_NPC_PeriodicAnimation(this ZDO zdo) => zdo.GetString("KGperiodicAnimation");
    public static float GET_NPC_PeriodicAnimationTime(this ZDO zdo) => zdo.GetFloat("KGperiodicAnimationTime");
    public static string GET_NPC_PeriodicSound(this ZDO zdo) => zdo.GetString("KGperiodicSound");
    public static float GET_NPC_PeriodicSoundTime(this ZDO zdo) => zdo.GetFloat("KGperiodicSoundTime");
    public static float GET_NPC_NPCScale(this ZDO zdo) => zdo.GetFloat("KGnpcScale", 1f);
    
    
        
    static Marketplace_API()
    {
        if (Type.GetType("API.ClientSide, kg.Marketplace") is not { } marketplaceAPI)
        {
            _IsInstalled = false;
            return;
        }

        _IsInstalled = true;
        MI_IsPlayerInsideTerritory = marketplaceAPI.GetMethod("IsPlayerInsideTerritory",
            BindingFlags.Public | BindingFlags.Static);
        MI_IsObjectInsideTerritoryWithFlag = marketplaceAPI.GetMethod("IsObjectInsideTerritoryWithFlag",
            BindingFlags.Public | BindingFlags.Static);
        MI_IsObjectInsideTerritoryWithFlag_Additional = marketplaceAPI.GetMethod(
            "IsObjectInsideTerritoryWithFlag_Additional",
            BindingFlags.Public | BindingFlags.Static);
        MI_ResetTraderItems = marketplaceAPI.GetMethod("ResetTraderItems",
            BindingFlags.Public | BindingFlags.Static);
        MI_OpenQuestJournal = marketplaceAPI.GetMethod("OpenQuestJournal",
            BindingFlags.Public | BindingFlags.Static);
    }
}

public static class ClientSide
{
    //Jere Expand World compatibility
    public static bool FillingTerritoryData = false;

    //trader
    public static void ResetTraderItems()
    {
        Trader_Main_Client.InitTraderItems();
        Gambler_Main_Client.GamblerInit();
        Transmogrification_Main_Client.InitTransmogData();
    }

    //quests
    public static void OpenQuestJournal() => Quests_UIs.QuestUI.ClickJournal();
    

    //territories
    public static bool IsPlayerInsideTerritory(out string name, out int flags,
        out int additionalFlags)
    {
        if (TerritorySystem_Main_Client.CurrentTerritory != null)
        {
            name = TerritorySystem_Main_Client.CurrentTerritory.Name;
            flags = (int)TerritorySystem_Main_Client.CurrentTerritory.Flags;
            additionalFlags = (int)TerritorySystem_Main_Client.CurrentTerritory.AdditionalFlags;
            return true;
        }

        name = "";
        flags = 0;
        additionalFlags = 0;
        return false;
    }

    public static bool IsObjectInsideTerritoryWithFlag(Vector3 pos, int flag,
        out string name,
        out int flags,
        out int additionalFlags)
    {
        foreach (TerritorySystem_DataTypes.Territory territory in TerritorySystem_Main_Client.TerritoriesByFlags[
                     (TerritorySystem_DataTypes.TerritoryFlags)flag])
        {
            if (!territory.IsInside(pos)) continue;
            name = territory.Name;
            flags = (int)territory.Flags;
            additionalFlags = (int)territory.AdditionalFlags;
            return true;
        }

        name = "";
        flags = 0;
        additionalFlags = 0;
        return false;
    }

    public static bool IsObjectInsideTerritoryWithFlag_Additional(Vector3 pos, int flag,
        out string name,
        out int flags,
        out int additionalFlags)
    {
        foreach (TerritorySystem_DataTypes.Territory territory in TerritorySystem_Main_Client.TerritoriesByFlags_Additional[
                     (TerritorySystem_DataTypes.AdditionalTerritoryFlags)flag])
        {
            if (!territory.IsInside(pos)) continue;
            name = territory.Name;
            flags = (int)territory.Flags;
            additionalFlags = (int)territory.AdditionalFlags;
            return true;
        }

        name = "";
        flags = 0;
        additionalFlags = 0;
        return false;
    }
}