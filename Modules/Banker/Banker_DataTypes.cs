namespace Marketplace.Modules.Banker;

public static class Banker_DataTypes
{
    internal static readonly CustomSyncedValue<Dictionary<string, List<int>>> SyncedBankerProfiles = new(Marketplace.configSync, "bankerData", new Dictionary<string, List<int>>());
    public static Dictionary<int, int> BankerClientData = new();
}