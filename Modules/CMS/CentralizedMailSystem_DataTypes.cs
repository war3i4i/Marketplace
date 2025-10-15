using Marketplace.Modules.MainMarketplace;
using Valheim.SettingsGui;

namespace Marketplace.Modules.CMS;

public static class CentralizedMailSystem_DataTypes
{
    public static CustomSyncedValue<List<DB.DB.CMS_User>> SyncedCMSUsers = new CustomSyncedValue<List<DB.DB.CMS_User>>(Marketplace.configSync, "SyncedCMSUSers", []);
    public static readonly CustomSyncedValue<long> SyncedServerTime = new(Marketplace.configSync, "SyncedServerTime", 0L);
    public const int MaxPerPage = 7;
}
    
        
