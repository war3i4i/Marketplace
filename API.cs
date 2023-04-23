﻿using Marketplace.Modules.TerritorySystem;

namespace API;

//if you want to use marketplace api just copy-paste this whole class into your code and use its methods
public static class Marketplace_API
{
    private static readonly bool _IsInstalled;
    private static readonly MethodInfo MI_IsPlayerInsideTerritory;
    private static readonly MethodInfo MI_IsObjectInsideTerritoryWithFlag;
    private static readonly MethodInfo MI_IsObjectInsideTerritoryWithFlag_Additional;

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
        NoCreatureDrops = 1 << 5,
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

    public static bool IsObjectInsideTerritoryWithFlag(GameObject obj, TerritoryFlags flag, out string name,
        out TerritoryFlags flags, out AdditionalTerritoryFlags additionalFlags)
    {
        name = "";
        flags = 0;
        additionalFlags = 0;
        if (!_IsInstalled || MI_IsObjectInsideTerritoryWithFlag == null)
            return false;

        Vector3 pos = obj.transform.position;
        object[] args = { pos, (int)flag, "", 0, 0 };
        bool result = (bool)MI_IsObjectInsideTerritoryWithFlag.Invoke(null, args);
        name = (string)args[2];
        flags = (TerritoryFlags)args[3];
        additionalFlags = (AdditionalTerritoryFlags)args[4];
        return result;
    }

    public static bool IsObjectInsideTerritoryWithFlag(GameObject obj, AdditionalTerritoryFlags flag, out string name,
        out TerritoryFlags flags, out AdditionalTerritoryFlags additionalFlags)
    {
        name = "";
        flags = 0;
        additionalFlags = 0;
        if (!_IsInstalled || MI_IsObjectInsideTerritoryWithFlag_Additional == null)
            return false;

        Vector3 pos = obj.transform.position;
        object[] args = { pos, (int)flag, "", 0, 0 };
        bool result = (bool)MI_IsObjectInsideTerritoryWithFlag_Additional.Invoke(null, args);
        name = (string)args[2];
        flags = (TerritoryFlags)args[3];
        additionalFlags = (AdditionalTerritoryFlags)args[4];
        return result;
    }

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
        MI_IsObjectInsideTerritoryWithFlag_Additional = marketplaceAPI.GetMethod("IsObjectInsideTerritoryWithFlag_Additional",
            BindingFlags.Public | BindingFlags.Static);
    }
}

public static class ClientSide
{
    //Jere Expand World compatibility
    public static bool FillingTerritoryData = false;


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
        foreach (var territory in TerritorySystem_Main_Client.TerritoriesByFlags[
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
        foreach (var territory in TerritorySystem_Main_Client.TerritoriesByFlags_Additional[
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