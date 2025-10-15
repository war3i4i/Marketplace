using Marketplace.Modules.MainMarketplace;
using static Marketplace.Modules.CMS.CentralizedMailSystem_DataTypes;

namespace Marketplace.Modules.CMS;

[Market_Autoload(Market_Autoload.Type.Client)]
public static class CentralizedMailSystem_Client
{
    [UsedImplicitly]
    private static void OnInit()
    {
        CentralizedMailSystem_UI.Init();
        SyncedCMSUsers.Value = new();
        SyncedServerTime.Value = DateTime.Now.Ticks;
    }

    public static void SendMail_Admin(string sender, string subject, string message, string userID, Marketplace_DataTypes.ClientMarketSendData[] attachments = null, int minutesDuration = 10080)
    {
        if (userID == "0") return;
        ZPackage pkg = new();
        pkg.Write(sender ?? "");
        pkg.Write(userID ?? "");
        pkg.Write(subject ?? "");
        pkg.Write(message ?? "");
        pkg.Write(minutesDuration);
        pkg.Write(attachments?.Length ?? 0);
        if (attachments != null)
        {
            foreach (Marketplace_DataTypes.ClientMarketSendData attachment in attachments)
            {
                pkg.Write(attachment.ItemPrefab);
                pkg.Write(attachment.Count);
                pkg.Write(attachment.Quality);
            }
        }
        pkg.Compress();
        ZRoutedRpc.instance.InvokeRoutedRPC("KGmarket CMS SendMail_Admin", [pkg]);
    }
 
    public static void SendMail_User(string subject, string message, string userID)
    {
        if (userID == "0") return;
        ZPackage pkg = new();
        pkg.Write(userID);
        pkg.Write(subject ?? "");
        pkg.Write(message ?? "");
        pkg.Compress();
        ZRoutedRpc.instance.InvokeRoutedRPC("KGmarket CMS SendMail_User", [pkg]);
    }
    
    
    
    
}