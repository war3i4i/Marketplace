using System.Data.Common;
using System.Security.Policy;
using System.Threading;
using BepInEx.Configuration;
using LiteDB;
using Marketplace.Modules.Global_Options;
using Marketplace.Modules.MainMarketplace;
using Marketplace.Paths;

namespace Marketplace.DB;

[Market_Autoload(Market_Autoload.Type.Server, Market_Autoload.Priority.First)]
public class DB
{
    private static ConfigEntry<string> LiteDBPath;

    public static Marketplace_Context_LiteDB _context;
    private static string DBPATH;

    [UsedImplicitly] 
    private static void OnInit()  
    {
        LiteDBPath = Marketplace._thistype.Config.Bind("Database", "Database File Path", "");
        DBPATH = string.IsNullOrEmpty(LiteDBPath.Value) ? Market_Paths.DBFile : LiteDBPath.Value;
        BsonMapper.Global.EmptyStringToNull = false;
        BsonMapper.Global.IncludeFields = true;
        BsonMapper.Global.TrimWhitespace = false;
        _context = new Marketplace_Context_LiteDB();
    }
 
    public class Marketplace_Context_LiteDB 
    {
        private LiteDatabase open => new LiteDatabase(new ConnectionString()
        {
            Filename = DBPATH,
            Connection = ConnectionType.Shared,
             
        });

        public void Marketplace_AddItem(MarketSlot item)
        {
            try 
            { 
                using LiteDatabase db = open;
                ILiteCollection<MarketSlot> col = db.GetCollection<MarketSlot>("Marketplace");
                col.Insert(item);
                col.EnsureIndex(x => x.ItemPrefab);
            }
            catch(Exception ex)
            {
                Utils.print("Error adding item to marketplace: " + ex);
            }
        }

        public void Marketplace_RemoveItem(int uid)
        {
            try
            {
                using LiteDatabase db = open;
                ILiteCollection<MarketSlot> col = db.GetCollection<MarketSlot>("Marketplace");
                col.Delete(uid);
            }
            catch (Exception ex)
            {
                Utils.print("Error removing item from marketplace: " + ex);
            }
        }

        public List<MarketSlot> Marketplace_GetItems()
        {
            try
            {
                using LiteDatabase db = open;
                ILiteCollection<MarketSlot> col = db.GetCollection<MarketSlot>("Marketplace");
                return col.FindAll().ToList();
            }
            catch (Exception ex)
            {
                Utils.print("Error getting items from marketplace: " + ex);
                return new List<MarketSlot>();
            }
        }

        public MarketSlot GetSlotByID(int id)
        {
            try
            {
                using LiteDatabase db = open;
                ILiteCollection<MarketSlot> col = db.GetCollection<MarketSlot>("Marketplace");
                return col.FindById(id);
            }
            catch (Exception ex)
            {
                Utils.print("Error getting slot by id: " + ex);
                return null;
            }
        }

        public void ModifyExisting(MarketSlot slot)
        {
            try
            {
                using LiteDatabase db = open;
                ILiteCollection<MarketSlot> col = db.GetCollection<MarketSlot>("Marketplace");
                col.Update(slot);
            }
            catch (Exception ex)
            {
                Utils.print("Error modifying slot: " + ex);
            }
        }

        public List<CMS_User> GetAllCMSUsers()
        {
            try
            {
                using LiteDatabase db = open;
                ILiteCollection<CMS_User> col = db.GetCollection<CMS_User>("Users");
                return col.FindAll().ToList();
            }
            catch (Exception ex)
            {
                Utils.print("Error getting all CMS users: " + ex);
                return new List<CMS_User>();
            }
        }

        public int GetTotalMailCountForUser(string userID)
        {
            try
            {
                using LiteDatabase db = open;
                ILiteCollection<CMS_Entry> col = db.GetCollection<CMS_Entry>("Mails");
                return col.Count(e => e.Owner == userID);
            }
            catch (Exception ex)
            {
                Utils.print("Error getting total mail count for user: " + ex);
                return 0;
            }
        }

        public int GetUnreadMailCountForUser(string userID)
        {
            try
            {
                using LiteDatabase db = open;
                ILiteCollection<CMS_Entry> col = db.GetCollection<CMS_Entry>("Mails");
                return col.Count(e => e.Owner == userID && !e.WasRead);
            }
            catch (Exception ex)
            {
                Utils.print("Error getting unread mail count for user: " + ex);
                return 0;
            }
        }

        public List<CMS_Entry> GetUserMails(string userID)
        {
            try
            {
                using LiteDatabase db = open;
                ILiteCollection<CMS_Entry> col = db.GetCollection<CMS_Entry>("Mails");
                return col.Find(e => e.Owner == userID).ToList();
            }
            catch (Exception ex)
            {
                Utils.print("Error getting user mails: " + ex);
                return new List<CMS_Entry>();
            }
        }

        public Dictionary<string, List<CMS_Entry>> GetAllMailsByUserId()
        {
            try
            {
                using LiteDatabase db = open;
                ILiteCollection<CMS_Entry> col = db.GetCollection<CMS_Entry>("Mails");
                Dictionary<string, List<CMS_Entry>> mails = new();
                foreach (CMS_Entry entry in col.FindAll())
                {
                    if (!mails.ContainsKey(entry.Owner)) mails.Add(entry.Owner, new List<CMS_Entry>());
                    mails[entry.Owner].Add(entry);
                }

                return mails;
            }
            catch (Exception ex)
            {
                Utils.print("Error getting all mails by user id: " + ex);
                return new Dictionary<string, List<CMS_Entry>>();
            }
        }

        public void SetMailRead(string userID, int mailUID)
        {
            try
            {
                using LiteDatabase db = open;
                ILiteCollection<CMS_Entry> col = db.GetCollection<CMS_Entry>("Mails");
                CMS_Entry findEntry = col.FindOne(e => e.Owner == userID && e._id == mailUID);
                if (findEntry == null) return;
                findEntry.WasRead = true;
                col.Update(findEntry);
            }
            catch (Exception ex)
            {
                Utils.print("Error setting mail read: " + ex);
            }
        }

        public void AddMail(CMS_Entry mail)
        {
            try
            {
                using LiteDatabase db = open;
                ILiteCollection<CMS_Entry> col = db.GetCollection<CMS_Entry>("Mails");
                col.EnsureIndex(x => x.Owner);
                col.Insert(mail);
            }
            catch (Exception ex)
            {
                Utils.print("Error adding mail: " + ex);
            }
        }

        public CMS_Entry GetMailByUID(string userID, int mailUID)
        {
            try
            {
                using LiteDatabase db = open;
                ILiteCollection<CMS_Entry> col = db.GetCollection<CMS_Entry>("Mails");
                return col.FindOne(e => e.Owner == userID && e._id == mailUID);
            } 
            catch (Exception ex)
            {
                Utils.print("Error getting mail by uid: " + ex);
                return null;
            }
        } 
        
        public void Mail_SetAttachmentTaken(string userID, int mailUID)
        {
            try
            {
                using LiteDatabase db = open;
                ILiteCollection<CMS_Entry> col = db.GetCollection<CMS_Entry>("Mails");
                CMS_Entry findEntry = col.FindOne(e => e.Owner == userID && e._id == mailUID);
                if (findEntry == null) return;
                findEntry.AttachmentsTaken = true;
                col.Update(findEntry);
            }
            catch (Exception ex)
            {
                Utils.print("Error setting attachment taken: " + ex);
            }
        }

        public void RemoveMailFromUser(string userID, int mailUID)
        {
            try
            {
                using LiteDatabase db = open;
                ILiteCollection<CMS_Entry> col = db.GetCollection<CMS_Entry>("Mails");
                CMS_Entry findEntry = col.FindOne(e => e.Owner == userID && e._id == mailUID);
                if (findEntry == null) return;
                col.Delete(findEntry._id);
            }
            catch (Exception ex)
            {
                Utils.print("Error removing mail from user: " + ex);
            }
        }

        public void AddCMSUser(CMS_User user)
        {
            try
            {
                using LiteDatabase db = open;
                ILiteCollection<CMS_User> col = db.GetCollection<CMS_User>("Users");
                col.EnsureIndex(x => x.UserID);
                
                var existing = col.FindOne(e => e.UserID == user.UserID);
                if (existing != null)
                {
                    user._id = existing._id;
                    col.Update(user);
                    return;
                }
                col.Insert(user);
            }
            catch (Exception ex)
            {
                Utils.print("Error adding CMS user: " + ex);
            }
        }

        public void AddBankItem(string userID, int prefab, int amount)
        {
            try
            {
                using LiteDatabase db = open;
                ILiteCollection<BankSlot> col = db.GetCollection<BankSlot>("Bank");
                col.EnsureIndex(x => x.Owner);
                BankSlot slot = col.FindOne(e => e.Owner == userID && e.Prefab == prefab);
                if (slot == null)
                {
                    col.Insert(new BankSlot { Owner = userID, Prefab = prefab, Amount = amount });
                }
                else
                {
                    slot.Amount += amount;
                    col.Update(slot);
                }
            }
            catch (Exception ex)
            {
                Utils.print("Error adding bank item: " + ex);
            }
        }

        public int RemoveBankItem(string userID, int prefab, int amount)
        {
            try
            {
                using LiteDatabase db = open;
                ILiteCollection<BankSlot> col = db.GetCollection<BankSlot>("Bank");
                col.EnsureIndex(x => x.Owner);
                BankSlot slot = col.FindOne(e => e.Owner == userID && e.Prefab == prefab);
                if (slot == null) return 0;
                int toRemove = Math.Min(slot.Amount, amount);
                slot.Amount -= toRemove;
                if (slot.Amount <= 0) col.Delete(slot._id);
                else col.Update(slot);
                return toRemove;
            }
            catch (Exception ex)
            {
                Utils.print("Error removing bank item: " + ex);
                return 0;
            }
        }

        public Dictionary<int, int> GetUserBankItems(string userID)
        {
            try
            {
                using LiteDatabase db = open;
                ILiteCollection<BankSlot> col = db.GetCollection<BankSlot>("Bank");
                return col.Find(e => e.Owner == userID).ToDictionary(e => e.Prefab, e => e.Amount);
            }
            catch (Exception ex)
            {
                Utils.print("Error getting user bank items: " + ex);
                return new Dictionary<int, int>();
            }
        }

        public Player_Leaderboard GetOrCreateLeaderboard(string userID, string playerName)
        {
            try
            {
                using LiteDatabase db = open;
                ILiteCollection<Player_Leaderboard> col = db.GetCollection<Player_Leaderboard>("Leaderboard");
                col.EnsureIndex(x => x.Owner);
                Player_Leaderboard player = col.FindOne(e => e.Owner == userID);
                if (player == null)
                {
                    player = new Player_Leaderboard { Owner = userID, PlayerName = playerName};
                    col.Insert(player);
                }
                return player;
            }
            catch (Exception ex)
            {
                Utils.print("Error getting or creating leaderboard: " + ex);
                return null;
            }
        }
        
        public Dictionary<string, Player_Leaderboard> GetAllLeaderboards()
        {
            try
            {
                using LiteDatabase db = open;
                ILiteCollection<Player_Leaderboard> col = db.GetCollection<Player_Leaderboard>("Leaderboard");
                Dictionary<string, Player_Leaderboard> all = [];
                foreach (var playerLeaderboard in col.FindAll()) all[playerLeaderboard.Owner] = playerLeaderboard;
                return all;
            }
            catch (Exception ex)
            {
                Utils.print("Error getting all leaderboards: " + ex);
                return new Dictionary<string, Player_Leaderboard>();
            }
        }
        
        public void UpdateLeaderboard(Player_Leaderboard leaderboard)
        {
            try
            {
                using LiteDatabase db = open;
                ILiteCollection<Player_Leaderboard> col = db.GetCollection<Player_Leaderboard>("Leaderboard");
                col.Update(leaderboard);
            }
            catch (Exception ex)
            {
                Utils.print("Error updating leaderboard: " + ex);
            }
        }

        public void IncreaseIntereset(HashSet<string> vipList, Dictionary<string, Dictionary<int, DateTime>> BankerTimeStamp)
        {
            try
            {
                using LiteDatabase db = open;
                ILiteCollection<BankSlot> col = db.GetCollection<BankSlot>("Bank");
                List<BankSlot> all = col.FindAll().ToList();
                Dictionary<string, List<BankSlot>> grouped = new();
                foreach (BankSlot slot in all)
                {
                    if (!grouped.ContainsKey(slot.Owner)) grouped.Add(slot.Owner, new List<BankSlot>());
                    grouped[slot.Owner].Add(slot);
                }

                HashSet<int> interestItems = new(Global_Configs.BankerInterestItems.Split(',').Select(i => i.GetStableHashCode()));
                bool isAll = Global_Configs.BankerInterestItems == "All";

                foreach (KeyValuePair<string, List<BankSlot>> owner in grouped)
                {
                    float multiplier = Global_Configs.SyncedGlobalOptions.Value._vipPlayerList.Contains(owner.Key)
                        ? Global_Configs.BankerVIPIncomeMultiplier
                        : Global_Configs.BankerIncomeMultiplier;

                    foreach (BankSlot slot in owner.Value)
                    {
                        if (slot.Amount <= 0) continue;
                        if (!isAll && !interestItems.Contains(slot.Prefab)) continue;
                        
                        if (!BankerTimeStamp.ContainsKey(owner.Key) || !BankerTimeStamp[owner.Key].ContainsKey(slot.Prefab) ||
                            (DateTime.Now - BankerTimeStamp[owner.Key][slot.Prefab]).TotalHours >=
                            Global_Configs.BankerIncomeTime)
                        {
                            double toAdd = Math.Ceiling(slot.Amount * multiplier);
                            if (slot.Amount + toAdd > int.MaxValue)
                                slot.Amount = int.MaxValue;
                            else
                                slot.Amount = (int)(toAdd + slot.Amount);
                            col.Update(slot);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Utils.print("Error increasing interest: " + ex);
            }
        }
    }

    public class MarketSlot : ISerializableParameter
    {
        public int ID { get; set; }
        public string ItemPrefab { get; set; }
        public int Count { get; set; }
        public int Price { get; set; }
        public string SellerName { get; set; }
        public string SellerUserID { get; set; }
        public Marketplace_DataTypes.ItemData_ItemCategory ItemCategory { get; set; }
        public int Quality { get; set; }
        public int Variant { get; set; }
        public string CrafterName { get; set; } = "";
        public long CrafterID { get; set; }
        public string CUSTOMdata { get; set; } = "{}";
        public byte DurabilityPercent { get; set; }
        public uint TimeStamp { get; set; }
        public string Currency { get; set; } = "Coins";

        public void Serialize(ref ZPackage pkg)
        {
            pkg.Write(ID);
            pkg.Write(ItemPrefab ?? "");
            pkg.Write(Count);
            pkg.Write(Price);
            pkg.Write(SellerName ?? "");
            pkg.Write(SellerUserID ?? "");
            pkg.Write((int)ItemCategory);
            pkg.Write(Quality);
            pkg.Write(Variant);
            pkg.Write(CrafterName ?? "");
            pkg.Write(CrafterID);
            pkg.Write(CUSTOMdata ?? "{}");
            pkg.Write(DurabilityPercent);
            pkg.Write(TimeStamp);
            pkg.Write(string.IsNullOrWhiteSpace(Currency) ? "Coins" : Currency);
        }

        public void Deserialize(ref ZPackage pkg)
        {
            ID = pkg.ReadInt();
            ItemPrefab = pkg.ReadString();
            Count = pkg.ReadInt();
            Price = pkg.ReadInt();
            SellerName = pkg.ReadString();
            SellerUserID = pkg.ReadString();
            ItemCategory = (Marketplace_DataTypes.ItemData_ItemCategory)pkg.ReadInt();
            Quality = pkg.ReadInt();
            Variant = pkg.ReadInt();
            CrafterName = pkg.ReadString();
            CrafterID = pkg.ReadLong();
            CUSTOMdata = pkg.ReadString();
            DurabilityPercent = pkg.ReadByte();
            TimeStamp = pkg.ReadUInt();
            Currency = pkg.ReadString();
        } 

        public string LocalizeKey() => ZNetScene.instance.GetPrefab(ItemPrefab) is { } item ? item.GetComponent<ItemDrop>().m_itemData.m_shared.m_name : ItemPrefab;

        public MarketSlot()
        {
        }

        public Marketplace_DataTypes.ClientMarketSendData ToClient()
        {
            return new Marketplace_DataTypes.ClientMarketSendData
            {
                ItemPrefab = ItemPrefab,
                Count = Count,
                Price = Price,
                SellerName = SellerName,
                ItemCategory = (Marketplace_DataTypes.ItemData_ItemCategory)ItemCategory,
                Quality = Quality,
                Variant = Variant,
                CUSTOMdata = CUSTOMdata,
                CrafterName = CrafterName,
                CrafterID = CrafterID,
                DurabilityPercent = DurabilityPercent
            };
        }

        public MarketSlot(Marketplace_DataTypes.ClientMarketSendData other, string user)
        {
            ItemPrefab = other.ItemPrefab;
            Count = other.Count;
            Price = other.Price;
            SellerName = other.SellerName;
            SellerUserID = user;
            ItemCategory = other.ItemCategory;
            Quality = other.Quality;
            Variant = other.Variant;
            CUSTOMdata = other.CUSTOMdata;
            CrafterName = other.CrafterName;
            CrafterID = other.CrafterID;
            TimeStamp = (uint)ZNet.instance.m_netTime;
            DurabilityPercent = other.DurabilityPercent;
            Currency = other.Currency;
            if (string.IsNullOrWhiteSpace(Currency) || !ZNetScene.instance.GetPrefab(Currency) || !Global_Configs.SyncedGlobalOptions.Value._possibleCurrencies.Contains(Currency)) 
                Currency = Global_Configs.SyncedGlobalOptions.Value._possibleCurrencies[0];
        }
        
        

        public string ItemName => (ZNetScene.instance.GetPrefab(ItemPrefab) is { } item ? item.GetComponent<ItemDrop>().m_itemData.m_shared.m_name : ItemPrefab)!;
    }


    public class CMS_User : ISerializableParameter
    {
        public int _id { get; set; }
        public string Name { get; set; }
        public string UserID { get; set; }
        public long LastOnline { get; set; }
        public bool IsOnline { get; set; }

        public void Serialize(ref ZPackage pkg)
        {
            pkg.Write(Name ?? "");
            pkg.Write(UserID ?? "");
            pkg.Write(LastOnline);
            pkg.Write(IsOnline);
        }

        public void Deserialize(ref ZPackage pkg)
        {
            Name = pkg.ReadString();
            UserID = pkg.ReadString();
            LastOnline = pkg.ReadLong();
            IsOnline = pkg.ReadBool();
        }
    }

    public class CMS_Entry : ISerializableParameter
    {
        public int _id { get; set; }
        public string Owner { get; set; }
        public long Created { get; set; }
        public string Sender { get; set; }
        public string Topic { get; set; } 
        public string Message { get; set; }
        public List<Marketplace_DataTypes.ClientMarketSendData> Attachments { get; set; }
        public List<Marketplace_DataTypes.ClientMarketSendData> Links { get; set; }

        public int ExpireAfterMinutes { get; set; } = 10800;

        public bool WasRead { get; set; }
        public bool AttachmentsTaken { get; set; }

        public void Serialize(ref ZPackage pkg)
        {
            pkg.Write(_id);
            pkg.Write(Owner ?? "");
            pkg.Write(Created);
            pkg.Write(Sender ?? "");
            pkg.Write(Topic ?? "");
            pkg.Write(Message ?? "");
            if (Attachments != null)
            {
                pkg.Write(true);
                pkg.Write(JSON.ToJSON(Attachments));
            }
            else pkg.Write(false);

            if (Links != null)
            {
                pkg.Write(true);
                pkg.Write(JSON.ToJSON(Links));
            }
            else pkg.Write(false);

            pkg.Write(ExpireAfterMinutes);
            pkg.Write(WasRead);
            pkg.Write(AttachmentsTaken);
        }

        public void Deserialize(ref ZPackage pkg)
        {
            _id = pkg.ReadInt();
            Owner = pkg.ReadString();
            Created = pkg.ReadLong();
            Sender = pkg.ReadString();
            Topic = pkg.ReadString();
            Message = pkg.ReadString();
            if (pkg.ReadBool()) Attachments = JSON.ToObject<List<Marketplace_DataTypes.ClientMarketSendData>>(pkg.ReadString());
            if (pkg.ReadBool()) Links = JSON.ToObject<List<Marketplace_DataTypes.ClientMarketSendData>>(pkg.ReadString());
            ExpireAfterMinutes = pkg.ReadInt();
            WasRead = pkg.ReadBool();
            AttachmentsTaken = pkg.ReadBool();
        }
    }

    public class BankSlot
    {
        public int _id { get; set; }
        public string Owner { get; set; }
        public int Prefab { get; set; }
        public int Amount { get; set; }
    }
    
    public class Player_Leaderboard
    {
        public int _id { get; set; }
        public string Owner { get; set; }
        public Dictionary<string, int> KilledCreatures = new();
        public Dictionary<string, int> BuiltStructures = new();
        public Dictionary<string, int> ItemsCrafted = new();
        public Dictionary<string, int> KilledBy = new();
        public Dictionary<string, int> Harvested = new();
        public float MapExplored { get; set; }
        public int DeathAmount { get; set; }
        public string PlayerName { get; set; }

        [BsonIgnore]
        public int KilledPlayers => KilledCreatures.TryGetValue("Player", out int amount) ? amount : 0;
    }
}