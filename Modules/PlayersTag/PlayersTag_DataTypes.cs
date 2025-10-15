namespace Marketplace.Modules.PlayersTag;

public static class PlayersTag_DataTypes
{
    internal static readonly CustomSyncedValue<Dictionary<string, string>> SyncedPlayersTagData =
        new CustomSyncedValue<Dictionary<string, string>>(Marketplace.configSync, "kgPlayersTagData",
            new Dictionary<string, string>());
}