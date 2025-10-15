using System.Threading;
using Marketplace.Modules.MainMarketplace;
using Marketplace.Paths;
using UnityEngine.Rendering;
using static Marketplace.Modules.CMS.CentralizedMailSystem_DataTypes;
using Random = UnityEngine.Random;

namespace Marketplace.Modules.CMS;

[Market_Autoload(Market_Autoload.Type.Server)]
public static class CentralizedMailSystem_Server
{
    [UsedImplicitly]
    private static void OnInit()
    {
        SyncedServerTime.Value = DateTime.Now.Ticks;
        SyncedCMSUsers.Value = DB.DB._context.GetAllCMSUsers();
        Marketplace.Global_FixedUpdator += FixedUpdate;
    }

    private static float _timer = 0f;
    private static void FixedUpdate(float dt)
    {
        if (!ZNet.instance || !ZNet.instance.IsServer()) return;
        _timer -= dt;
        if (_timer > 0) return;
        _timer = 60f;
        SyncedServerTime.Value = DateTime.Now.Ticks;
        PeriodicCheck();
    }
    
    //private static int GetTotalMailCountForUser(string userID) => AllMails.TryGetValue(userID, out List<CMS_Entry> mails) ? mails.Count : 0;
    //private static int GetUnreadMailCountForUser(string userID) => AllMails.TryGetValue(userID, out List<CMS_Entry> mails) ? mails.Count(e => !e.WasRead) : 0;
    //private static List<CMS_Entry> GetUserMails(string userID) => AllMails.TryGetValue(userID, out List<CMS_Entry> mails) ? mails : new();
    //private static CMS_Entry GetMailByUID(string userID, int mailUID) => AllMails.TryGetValue(userID, out List<CMS_Entry> mails) ? mails.Find(e => e.UID == mailUID) : null;
    /*private static void _internal_RemoveMailFromUser(string userID, CMS_Entry mail)
    {
        if (!AllMails.TryGetValue(userID, out List<CMS_Entry> mails)) return;
        mails.Remove(mail);
        if (mails.Count == 0) AllMails.Remove(userID);
        SaveMails();
    }
    private static void _internal_AddMailToUser(string userID, CMS_Entry mail)
    {
        if (userID == "0") return;
        if (!AllMails.TryGetValue(userID, out List<CMS_Entry> mails)) AllMails[userID] = new List<CMS_Entry> { mail };
        else mails.Add(mail);
        SaveMails();
    }
    private static int GenerateNewMailUIDForUser(string userID)
    {
        if (AllMails.TryGetValue(userID, out List<CMS_Entry> mails))
        {
            int generateUID = Random.Range(int.MinValue, int.MaxValue);
            while (mails.Find(e => e.UID == generateUID) is not null) generateUID = Random.Range(int.MinValue, int.MaxValue);
            return generateUID;
        }
        return Random.Range(int.MinValue, int.MaxValue);
    }*/
    
    public static void PeriodicCheck()
    {
        bool AnythingChanged(bool[] a, bool[] b)
        {
            if (a.Length != b.Length) return true;
            for (int i = 0; i < a.Length; i++)
                if (a[i] != b[i]) return true;
            return false;
        }
        
        foreach (DB.DB.CMS_User cmsUser in SyncedCMSUsers.Value) cmsUser.IsOnline = false;
        if (!ZNet.instance) return;
        bool[] boolSequenceBefore = SyncedCMSUsers.Value.Select(u => u.IsOnline).ToArray();
        foreach (ZNetPeer peer in ZNet.instance.m_peers)
        { 
            string userID = peer.m_socket.GetHostName();
            if (!peer.IsReady() || userID == "0") continue; 
            if (SyncedCMSUsers.Value.Find(u => u.UserID == userID) is not {} user) 
            {
                user = new DB.DB.CMS_User { Name = peer.m_playerName, UserID = userID, LastOnline = DateTime.Now.Ticks, IsOnline = true };
                DB.DB._context.AddCMSUser(user);
                SyncedCMSUsers.Value.Add(user);
            }
            user.IsOnline = true;
            user.LastOnline = DateTime.Now.Ticks;
            int unreadMailCount = DB.DB._context.GetUnreadMailCountForUser(userID);
            if (unreadMailCount > 0) ZRoutedRpc.instance.InvokeRoutedRPC(peer.m_uid, "CMS_ReceiveUnreadMailAmount", unreadMailCount);
        }
        bool[] boolSequenceAfter = SyncedCMSUsers.Value.Select(u => u.IsOnline).ToArray();
        if (AnythingChanged(boolSequenceBefore, boolSequenceAfter)) SyncedCMSUsers.Update();
    }
    [MarketplaceRPC("KGmarket CMS WithdrawMailAttachments", Market_Autoload.Type.Server)]
    public static void WithdrawMail_Attachments(long sender, int mailUID)
    {
         ZNetPeer peer = ZNet.instance.GetPeer(sender);
         if (peer is null) return;
         string userID = peer.m_socket.GetHostName();
         if (DB.DB._context.GetMailByUID(userID, mailUID) is not {} mail) return;
         if (mail.Attachments.Count == 0 || mail.AttachmentsTaken) return;
         DB.DB._context.Mail_SetAttachmentTaken(userID, mailUID);
         foreach (Marketplace_DataTypes.ClientMarketSendData clientMarketSendData in mail.Attachments)
         {
             ZRoutedRpc.instance.InvokeRoutedRPC(sender, "KGmarket BuyItemAnswer", JSON.ToJSON(clientMarketSendData));
         }
    }
    [MarketplaceRPC("KGmarket CMS DeleteMail", Market_Autoload.Type.Server)]
    public static void DeleteMail(long sender, int mailUID)
    { 
        ZNetPeer peer = ZNet.instance.GetPeer(sender);
        if (peer is null) return;
        string userID = peer.m_socket.GetHostName();
        DB.DB._context.RemoveMailFromUser(userID, mailUID);
    }
    [MarketplaceRPC("KGmarket CMS ReadMail", Market_Autoload.Type.Server)]
    public static void ReadMail(long sender, int mailUID)
    {
        ZNetPeer peer = ZNet.instance.GetPeer(sender);
        if (peer is null) return;
        string userID = peer.m_socket.GetHostName();
        DB.DB._context.SetMailRead(userID, mailUID);
    }
    [MarketplaceRPC("KGmarket CMS RequestMailAmount", Market_Autoload.Type.Server)]
    public static void RequestMailAmount(long sender)
    {
        ZNetPeer peer = ZNet.instance.GetPeer(sender);
        if (peer is null) return;
        string userID = peer.m_socket.GetHostName();
        int mailCount = DB.DB._context.GetTotalMailCountForUser(userID);
        ZRoutedRpc.instance.InvokeRoutedRPC(sender, "CMS_ReceiveMailAmount", mailCount);
    }
    [MarketplaceRPC("KGmarket CMS SendMail_Admin", Market_Autoload.Type.Server)]
    public static void SendMail_Admin(long sender, ZPackage pkg)
    {
        ZNetPeer peer = ZNet.instance.GetPeer(sender);
        if (peer is null) return;
        pkg.Decompress();
        
        string senderName = pkg.ReadString();
        string targetID = pkg.ReadString();
        if (targetID == "0") return;
        string topic = pkg.ReadString();
        string message = pkg.ReadString();
        int minutesDuration = pkg.ReadInt();
        int attachmentCount = pkg.ReadInt();
        Marketplace_DataTypes.ClientMarketSendData[] attachments = attachmentCount > 0 ? new Marketplace_DataTypes.ClientMarketSendData[attachmentCount] : null;
        for (int i = 0; i < attachmentCount; i++)
        {
            string itemPrefab = pkg.ReadString();
            int amount = pkg.ReadInt();
            int level = pkg.ReadInt();
            attachments[i] = new Marketplace_DataTypes.ClientMarketSendData() { ItemPrefab = itemPrefab, Count = amount, Quality = level };
        }
        if (targetID.ToLower() != "all") AddMail(senderName, topic, message, targetID, attachments, null, minutesDuration);
        else SyncedCMSUsers.Value.ForEach(u => AddMail(senderName, topic, message, u.UserID, attachments, null, minutesDuration));
    }
    [MarketplaceRPC("KGmarket CMS SendMail_User", Market_Autoload.Type.Server)]
    public static void SendMail_User(long sender, ZPackage pkg)
    {
        ZNetPeer peer = ZNet.instance.GetPeer(sender);
        if (peer is null) return;
        pkg.Decompress();
        
        string senderName = peer.m_playerName;
        string targetID = pkg.ReadString();
        if (targetID == "0") return;
        string topic = pkg.ReadString();
        string message = pkg.ReadString();
        AddMail(senderName, topic, message, targetID);
    }
    [MarketplaceRPC("KGmarket CMS RequestMailData", Market_Autoload.Type.Server)]
    public static void RequestMail(long sender, int page, int revision)
    {
        ZNetPeer peer = ZNet.instance.GetPeer(sender);
        if (peer is null) return;
        string userID = peer.m_socket.GetHostName();
        List<DB.DB.CMS_Entry> userMails = DB.DB._context.GetUserMails(userID);
        int minIndex = Math.Max(0, userMails.Count - ((page + 1) * MaxPerPage));
        int count = (page == userMails.Count / MaxPerPage) ? userMails.Count % MaxPerPage : MaxPerPage;
        userMails = userMails.GetRange(minIndex, count);
        userMails.Reverse();
        ZPackage pkg = new();
        pkg.Write(revision);
        pkg.Write(userMails.Count);
        foreach (DB.DB.CMS_Entry mail in userMails) mail.Serialize(ref pkg);
        pkg.Compress();
        ZRoutedRpc.instance.InvokeRoutedRPC(sender, "CMS_ReceiveMailData", [pkg]);
    }
    public static void AddMail(string sender, string topic, string message, string userID, Marketplace_DataTypes.ClientMarketSendData[] attachments = null, Marketplace_DataTypes.ClientMarketSendData[] links = null, int minutesDuration = 10080)
    {
        if (userID == "0") return;
        DB.DB.CMS_Entry mail = new()
        {
            Created = DateTime.Now.Ticks,
            Sender = sender,
            Owner = userID, 
            Topic = topic,
            Message = message,
            Attachments = attachments is {Length:>0} ? attachments.ToList() : null,
            Links = links is {Length:>0} ? links.ToList() : null,
            ExpireAfterMinutes = minutesDuration,
        };
        DB.DB._context.AddMail(mail);
        
        if (!ZNet.instance) return;
        ZNetPeer peer = ZNet.instance.GetPeerByHostName(userID);
        if (peer is null) return;
        int unreadMailCount = DB.DB._context.GetUnreadMailCountForUser(userID);
        ZRoutedRpc.instance.InvokeRoutedRPC(peer.m_uid, "CMS_ReceiveUnreadMailAmount", unreadMailCount);
    }
    
}