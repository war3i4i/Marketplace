using System.Configuration;
using BepInEx.Configuration;
using Marketplace.ExternalLoads;
using Marketplace.Modules.MainMarketplace;
using UnityEngine.EventSystems;
using YamlDotNet.Core;

namespace Marketplace.Modules.CMS;

public static class CentralizedMailSystem_UI
{
    public class DragUI_CMS : MonoBehaviour, IDragHandler, IBeginDragHandler
    {
        private static List<DragUI_CMS> Instances = [];
        private readonly Transform[] Markers = new Transform[4];
        private RectTransform dragRect;
        private Vector2 startPos;

        public static void Ensure()
        {
            foreach (DragUI_CMS instance in Instances)
            {
                if (CheckMarkersOutsideScreen(new Vector2(Screen.width, Screen.height), instance.Markers)) instance.dragRect.anchoredPosition = instance.startPos;
            }
        }

        public void Init(RectTransform target)
        {
            Instances.Add(this);
            dragRect = target;
            startPos = dragRect.anchoredPosition;
            Markers[0] = transform.Find("LeftTop"); 
            Markers[1] = transform.Find("RightTop");
            Markers[2] = transform.Find("LeftBottom");
            Markers[3] = transform.Find("RightBottom");
        }

        public void OnDrag(PointerEventData eventData)
        {
            Vector2 anchoredPosition = dragRect.anchoredPosition;
            Vector2 vec = anchoredPosition + eventData.delta * dragRect.lossyScale * dragRect.localScale;
            Vector2 lastPos = anchoredPosition;
            anchoredPosition = vec;
            dragRect.anchoredPosition = anchoredPosition;
            if (CheckMarkersOutsideScreen(new Vector2(Screen.width, Screen.height), Markers))
                dragRect.anchoredPosition = lastPos;
        }
        private static bool CheckMarkersOutsideScreen(Vector2 screen, Transform[] Markers)
        {
            foreach (Transform marker in Markers) 
            {
                Vector3 position = marker.position;
                float markerX = position.x;
                float markerY = position.y;
                if (markerX < 0 || markerX > screen.x || markerY < 0 || markerY > screen.y)
                    return true;
            }
            return false;
        }

        public void OnBeginDrag(PointerEventData eventData)  => dragRect.SetAsLastSibling();
    }
    
    public static string FormatTime(TimeSpan timeSpan)
    {
        if (timeSpan.Days > 0) 
        {
            return $"{timeSpan.Days} $mpasn_CMS_Days\n{timeSpan.Hours} $mpasn_CMS_Hours";
        }
        else if (timeSpan.Hours > 0)
        {
            return $"{timeSpan.Hours} $mpasn_CMS_Hours";
        }
        else if (timeSpan.Minutes > 0)
        { 
            return $"{timeSpan.Minutes} $mpasn_CMS_Minutes";
        }
        else if (timeSpan.Seconds > 0)
        { 
            return $"{timeSpan.Seconds} $mpasn_CMS_Seconds";
        }
        else
        {
            return "$mpasn_CMS_Expired";
        }
    }
    
    private static bool CanTakeAttachments = false;
    private static int MaxPages = 1;
    private static int UnreadMail = 0;
    private static int TotalMail = 0;
    private static int CurrentPage = 1; 
    
    public static List<DB.DB.CMS_Entry> Mails = new();
    
    private class CMS_UI_Entry
    {
        private Transform Data;
        private Image Icon;
        private Text Sender;
        private Text Topic;
        private Text TimeLeft;
        private Transform IsRead;
        private Button OpenMail;
        private GameObject Selected;
        private GameObject Taken;

        public CMS_UI_Entry(Transform t)
        {
            Data = t.Find("Data");
            Icon = Data.Find("Icon/Img").GetComponent<Image>();
            Sender = Data.Find("Sender").GetComponent<Text>();
            Topic = Data.Find("Topic").GetComponent<Text>();
            TimeLeft = Data.Find("TimeLeft").GetComponent<Text>();
            IsRead = Data.Find("NewMail");
            int index = t.GetSiblingIndex();
            OpenMail = t.Find("bg").GetComponent<Button>();
            OpenMail.onClick.AddListener(() => ClickedOnEntry(index));
            Selected = t.Find("Selected").gameObject;
            Taken = t.Find("Data/Icon/Taken").gameObject;
            Data.gameObject.SetActive(false);
        } 
        
        public void SetSelected(bool selected)
        {
            Selected.SetActive(selected);
        }

        public void SetRead()
        {
            IsRead.gameObject.SetActive(false);
        }

        public void SetTaken()
        {
            Icon.color = new Color(0f, 1f, 1f);
            Taken.SetActive(true);
        }
        
        private static void ClickedOnEntry(int index)
        {
            AssetStorage.AUsrc.Play();
            if (index >= Mails.Count) return;
            if (Mails[index] == null) return;
            DB.DB.CMS_Entry mail = Mails[index];
            if (!mail.WasRead)
            {
                mail.WasRead = true;
                UnreadMail--;
                CheckUnreadMail();
                ZRoutedRpc.instance.InvokeRoutedRPC("KGmarket CMS ReadMail", mail._id);
                Entries[index].SetRead();
            }
            
            if (CurrentSelectedMail == index)
            {
                HideOpenMail(); 
                return;
            }
            CurrentSelectedMail = index;
            
            foreach (CMS_UI_Entry uiEntry in Entries) uiEntry.SetSelected(false);
            Entries[index].SetSelected(true);
            OpenMail_From.text = Localization.instance.Localize("$mpasn_CMS_From: ") + Localization.instance.Localize(mail.Sender);
            OpenMail_Subject.text = Localization.instance.Localize("$mpasn_CMS_Subject: ") + Localization.instance.Localize(mail.Topic);
            OpenMail_Message.text = Localization.instance.Localize(mail.Message);
            CMSLinkController.SetItemLinks(mail.Links, mail.Attachments);
            foreach (Transform t in AttachmentsContent) t.gameObject.SetActive(false);
            
            if (mail.Attachments is { Count: > 0 } attachments)
            {
                for (int i = 0; i < attachments.Count; ++i)  
                { 
                    if (i >= AttachmentsContent.childCount) break;
                    Transform t = AttachmentsContent.GetChild(i);
                    GameObject prefab = ZNetScene.instance.GetPrefab(mail.Attachments[i].ItemPrefab);
                    if (!prefab) continue;
                  
                    t.gameObject.SetActive(true);
                    Marketplace_DataTypes.ClientMarketSendData attachment = mail.Attachments[i];
                    Utils.GetItemForTooltip(attachment.ItemPrefab, attachment.Count,
                        attachment.Quality, attachment.Variant, attachment.CrafterID, attachment.CrafterName,
                        attachment.CUSTOMdata, attachment.DurabilityPercent, item =>
                        {
                            InventoryGui.instance.m_playerGrid.CreateItemTooltip(item, t.GetComponent<UITooltip>());
                            t.GetComponent<Image>().sprite = item.GetIcon();
                        });
          
                
                  
                    t.GetComponent<Image>().color = mail.AttachmentsTaken ? new Color(0f, 1f, 1f) : Color.white;
                    t.Find("AttachmentsText").GetComponent<Text>().text = mail.Attachments[i].Count > 1 ? $"{mail.Attachments[i].Count}" : "";
                }
            }
             
            TakeAttachments.gameObject.SetActive(!mail.AttachmentsTaken && mail.Attachments is { Count: > 0 }); 
            OpenedMail.gameObject.SetActive(true);
        }
        
        public void SetData(DB.DB.CMS_Entry mail)
        {
            if (mail == null)
            {
                Data.gameObject.SetActive(false);
                Icon.sprite = null; 
                Sender.text = "";
                Topic.text = "";
                TimeLeft.text = ""; 
                IsRead.gameObject.SetActive(false);
                OpenMail.interactable = false;
                Selected.SetActive(false);
                Taken.SetActive(false);
                return; 
            }
            Data.gameObject.SetActive(true);
            OpenMail.interactable = true;
            if (mail.Attachments is { Count: > 0 })
            { 
                GameObject prefab = ZNetScene.instance.GetPrefab(mail.Attachments[0].ItemPrefab);
                Sprite icon = prefab ? prefab.GetComponent<ItemDrop>().m_itemData.GetIcon() : AssetStorage.PlaceholderGamblerIcon; 
                Icon.sprite = icon;
                Icon.color = mail.AttachmentsTaken ? new Color(0f, 1f, 1f) : Color.white;
            }
            else
            {
                Icon.sprite = AssetStorage.PlaceholderMailIcon; 
                Icon.color = Color.white;
            }
            Sender.text = Localization.instance.Localize(mail.Sender); 
            Topic.text = Localization.instance.Localize(mail.Topic); 
            
            long timePassed = CentralizedMailSystem_DataTypes.SyncedServerTime.Value - mail.Created;
            long timeLeft = mail.ExpireAfterMinutes.MinutesToTicks() - timePassed;
            TimeSpan timeSpan = TimeSpan.FromTicks(timeLeft);
            
            TimeLeft.text = mail.ExpireAfterMinutes > 0 ? Localization.instance.Localize(FormatTime(timeSpan)) : "";
            IsRead.gameObject.SetActive(!mail.WasRead);
            Taken.SetActive(mail.AttachmentsTaken && mail.Attachments is { Count: > 0 });
        }
    }
    
    [HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.Awake))]
    [ClientOnlyPatch]
    private static class InventoryGui_Awake_Patch
    {
        [UsedImplicitly]
        private static void Postfix(InventoryGui __instance)
        {
            foreach (Transform t in AttachmentsContent)
            {
                t.GetComponent<UITooltip>().m_tooltipPrefab = __instance.m_playerGrid.m_elementPrefab.GetComponent<UITooltip>().m_tooltipPrefab;
            }
        }
    }
    
    private static GameObject UI;
    public static bool IsMainVisible => MAIN && MAIN.activeSelf;
    private static bool IsOpenVisible => OpenedMail.activeSelf;

    private static GameObject MAIN;
    private static GameObject MyMailsPage;
    private static GameObject SendMailPage;
    private static GameObject OpenedMail;
    private static GameObject LOADING;
    
    private static Text PageNumber;
    
    private static bool IsLoading => IsMainVisible && LOADING.activeInHierarchy;
    
    private static List<CMS_UI_Entry> Entries = new();

    private static Text OpenMail_From;
    private static Text OpenMail_Subject;
    private static TMP_Text OpenMail_Message;
    private static CMS_Links.CMS_LinkController CMSLinkController;
    private static Transform AttachmentsContent;
    private static Button TakeAttachments;

    private static GameObject SendMail_Admin;
    private static GameObject SendMail_User;
    private static Dropdown SendMail_Target_Dropdown;
    
    private static InputField SendMail_Subject_User;
    private static InputField SendMail_Message_User;

    private static InputField SendMail_SenderName_Admin;
    private static InputField SendMail_Subject_Admin;
    private static InputField SendMail_Message_Admin;
    private static InputField SendMail_Attachments_Admin;
    private static InputField SendMail_ExpireTime_Admin;
    
    [HarmonyPatch(typeof(FejdStartup), nameof(FejdStartup.Awake))]
    [ClientOnlyPatch]
    private static class FejdStartup_Awake_Patch 
    { 
        private static bool done;

        [UsedImplicitly]
        private static void Postfix(FejdStartup __instance)
        {
            if (done) return;
            done = true;
            if (__instance.transform.Find("StartGame/Panel/JoinPanel/serverCount")?.GetComponent<TextMeshProUGUI>() is not { } tmp) return;
            OpenMail_Message.font = tmp.font;
            OpenMail_Message.outlineWidth = 0.075f;
        }
    }
    
    private static Button SendMail_Send;

    private static GameObject HaveUnreadMail;
    private static Text UnreadMailAmount_Text;
    
    private static int CurrentSelectedMail = -1;
    
    public static void Init()
    {
        UI = UnityEngine.Object.Instantiate(AssetStorage.asset.LoadAsset<GameObject>("MarketplaceCMS_UI"));
        UnityEngine.Object.DontDestroyOnLoad(UI);
        MAIN = UI.transform.Find("Canvas/Mails").gameObject;
        MAIN.SetActive(false);
        MyMailsPage = UI.transform.Find("Canvas/Mails/MyMail").gameObject;
        MyMailsPage.SetActive(false);
        OpenedMail = UI.transform.Find("Canvas/OpenMail").gameObject;
        OpenedMail.gameObject.SetActive(false); 
        SendMailPage = UI.transform.Find("Canvas/Mails/SendMail").gameObject;
        SendMailPage.SetActive(false); 
        LOADING = MAIN.transform.Find("LOADING").gameObject; 
        LOADING.SetActive(false); 
        PageNumber = MyMailsPage.transform.Find("PageText").GetComponent<Text>();
        PageNumber.gameObject.SetActive(false);
        MAIN.transform.Find("Close").GetComponent<Button>().onClick.AddListener(() =>
        { 
            AssetStorage.AUsrc.Play();
            Hide();
        });
        foreach (Transform c in UI.transform.Find("Canvas/Mails/MyMail/Content")) Entries.Add(new(c));
        MyMailsPage.transform.Find("Button_Left").GetComponent<Button>().onClick.AddListener(PrevPageClick);
        MyMailsPage.transform.Find("Button_Right").GetComponent<Button>().onClick.AddListener(NextPageClick);
        MAIN.transform.Find("Button_MyMail").GetComponent<Button>().onClick.AddListener(() =>
        {
            AssetStorage.AUsrc.Play();
            if (IsLoading) return;
            ShowMyMail();
        });
        MAIN.transform.Find("Button_SendMail").GetComponent<Button>().onClick.AddListener(()=>
        { 
            AssetStorage.AUsrc.Play(); 
            ShowSendMail();
        });
        OpenMail_From = OpenedMail.transform.Find("From").GetComponent<Text>();
        OpenMail_Subject = OpenedMail.transform.Find("Subject").GetComponent<Text>();
        OpenMail_Message = OpenedMail.transform.Find("Message").GetComponent<TMP_Text>(); 
        CMSLinkController = OpenMail_Message.gameObject.AddComponent<CMS_Links.CMS_LinkController>();
        AttachmentsContent = OpenedMail.transform.Find("AttachmentsContent");
        TakeAttachments = OpenedMail.transform.Find("TakeAll").GetComponent<Button>();
        TakeAttachments.onClick.AddListener(() => TakeAllAttachments());
        OpenedMail.transform.Find("Bottom_Close").GetComponent<Button>().onClick.AddListener(() =>
        { 
            AssetStorage.AUsrc.Play(); 
            HideOpenMail();
        }); 
        OpenedMail.transform.Find("Close").GetComponent<Button>().onClick.AddListener(() =>
        {
            AssetStorage.AUsrc.Play(); 
            HideOpenMail();   
        });
        OpenedMail.transform.Find("Bottom_Remove").GetComponent<Button>().onClick.AddListener(RemoveMail);
        
        SendMail_Target_Dropdown = SendMailPage.transform.Find("TargetDropdown").GetComponent<Dropdown>();
        SendMail_Admin = SendMailPage.transform.Find("ADMIN").gameObject;
        SendMail_User = SendMailPage.transform.Find("NON_ADMIN").gameObject;
        
        
        SendMail_Subject_User = SendMail_User.transform.Find("SubjectInput").GetComponent<InputField>();
        SendMail_Message_User = SendMail_User.transform.Find("MessageInput").GetComponent<InputField>();
        
        SendMail_SenderName_Admin = SendMail_Admin.transform.Find("SenderInput").GetComponent<InputField>();
        SendMail_Subject_Admin = SendMail_Admin.transform.Find("SubjectInput").GetComponent<InputField>();
        SendMail_Message_Admin = SendMail_Admin.transform.Find("MessageInput").GetComponent<InputField>();
        SendMail_ExpireTime_Admin = SendMail_Admin.transform.Find("TimeInput").GetComponent<InputField>();
        SendMail_Attachments_Admin = SendMail_Admin.transform.Find("AttachmentsInput").GetComponent<InputField>();
        
        
        SendMail_Send = SendMailPage.transform.Find("Confirm").GetComponent<Button>();
        SendMail_Send.onClick.AddListener(SendMail);
        
        MAIN.transform.Find("BG").gameObject.AddComponent<DragUI_CMS>().Init(MAIN.transform as RectTransform);
        OpenedMail.transform.Find("BG").gameObject.AddComponent<DragUI_CMS>().Init(OpenedMail.transform as RectTransform);
 
        HaveUnreadMail = UI.transform.Find("Canvas/UnreadMailNofitication").gameObject;
        HaveUnreadMail.SetActive(false);
        UnreadMailAmount_Text = HaveUnreadMail.transform.Find("Text").GetComponent<Text>();
        
        Marketplace.Global_Updator += Update;

        Localization.instance.Localize(UI.transform);
    } 
 
    private static void RemoveMail() 
    {
        if (!IsMainVisible || !IsOpenVisible || CurrentSelectedMail == -1) return;
        AssetStorage.AUsrc.Play();
        DB.DB.CMS_Entry mail = Mails[CurrentSelectedMail];
        HideOpenMail();
        ZRoutedRpc.instance.InvokeRoutedRPC("KGmarket CMS DeleteMail", mail._id);
        SetPage(CurrentPage);
    }
    
    private static void HideOpenMail()
    {
        foreach (CMS_UI_Entry uiEntry in Entries) uiEntry.SetSelected(false);
        OpenedMail.gameObject.SetActive(false); 
        CurrentSelectedMail = -1;
    }
    public static void Hide()
    {
        HideOpenMail();
        MAIN.SetActive(false);
        LOADING.SetActive(false);
    }

    private static void CheckUnreadMail()
    {
        if (UnreadMail > 0)
        {
            HaveUnreadMail.SetActive(true);
            UnreadMailAmount_Text.text = UnreadMail.ToString();
        } 
        else 
        {
            HaveUnreadMail.SetActive(false);
        }
    }
    
    private static void PrevPageClick()
    {
        AssetStorage.AUsrc.Play();
        if (IsLoading) return;
        if (CurrentPage - 1 < 1) return;
        SetPage(CurrentPage - 1);
    }
    private static void NextPageClick()
    {
        AssetStorage.AUsrc.Play();
        if (IsLoading) return;
        if (CurrentPage + 1 > MaxPages) return;
        SetPage(CurrentPage + 1);
    }
     
    private static void TakeAllAttachments()
    {
        if (!IsMainVisible || !IsOpenVisible || CurrentSelectedMail == -1 || !Player.m_localPlayer) return;
        AssetStorage.AUsrc.Play();
        if (!CanTakeAttachments) return;
        DB.DB.CMS_Entry mail = Mails[CurrentSelectedMail];
        if (mail.AttachmentsTaken) return;
        TakeAttachments.gameObject.SetActive(false);
        mail.AttachmentsTaken = true;
        
        Entries[CurrentSelectedMail].SetTaken();
        
        ZRoutedRpc.instance.InvokeRoutedRPC("KGmarket CMS WithdrawMailAttachments", mail._id);

        foreach (Transform t in AttachmentsContent)
        {
            t.GetComponent<Image>().color = new Color(0f, 1f, 1f);
        }
    }

    private static List<DB.DB.CMS_User> _tempUsers = new();
    
    private static void SendMail()
    {
        if (!IsMainVisible || !SendMailPage.activeSelf) return;
        AssetStorage.AUsrc.Play();   
        int getCurrentIndex = SendMail_Target_Dropdown.value - 1;
        if (getCurrentIndex < 0) return;
        if (_tempUsers.Count <= getCurrentIndex) return;
        DB.DB.CMS_User user = _tempUsers[getCurrentIndex];
        if (user == null) return;

        if (!Utils.IsDebug_Strict)
        {
            if (string.IsNullOrEmpty(SendMail_Subject_User.text))
            {
                MessageHud.instance.ShowMessage(MessageHud.MessageType.Center, "Subject cannot be empty");
                return;
            }
            string subject = SendMail_Subject_User.text;
            string message = SendMail_Message_User.text;
            ShowSendMail();
            CentralizedMailSystem_Client.SendMail_User(subject, message, user.UserID);
        }
        else
        {
            if (string.IsNullOrEmpty(SendMail_ExpireTime_Admin.text))
            {
                MessageHud.instance.ShowMessage(MessageHud.MessageType.Center, "Expire time cannot be empty");
                return;
            }
            if (!int.TryParse(SendMail_ExpireTime_Admin.text, out int expireTime))
            {
                MessageHud.instance.ShowMessage(MessageHud.MessageType.Center, "Expire time must be a number");
                return;
            }
            if (expireTime < 1)
            {
                MessageHud.instance.ShowMessage(MessageHud.MessageType.Center, "Expire time must be greater than 0");
                return;
            }
            expireTime *= 60;
            string senderName = string.IsNullOrWhiteSpace(SendMail_SenderName_Admin.text) ? "Admin" : SendMail_SenderName_Admin.text;
            string subject = SendMail_Subject_Admin.text ?? "";
            string message = SendMail_Message_Admin.text ?? "";

            Marketplace_DataTypes.ClientMarketSendData[] attachments = null;
            if (!string.IsNullOrWhiteSpace(SendMail_Attachments_Admin.text))
            {
                string[] split = SendMail_Attachments_Admin.text.Replace(" ","").Split(',');
                if (split.Length % 3 != 0)
                {
                    MessageHud.instance.ShowMessage(MessageHud.MessageType.Center, "Invalid attachment format");
                    return;
                }
                attachments = new Marketplace_DataTypes.ClientMarketSendData[split.Length / 3];
                for (int i = 0; i < split.Length; i += 3)
                {
                    string itemPrefab = split[i];
                    if (!ZNetScene.instance.GetPrefab(itemPrefab))
                    {
                        MessageHud.instance.ShowMessage(MessageHud.MessageType.Center, $"Invalid item prefab: {itemPrefab}");
                        return;
                    }
                    if (!int.TryParse(split[i + 1], out int count) || count < 1)
                    {
                        MessageHud.instance.ShowMessage(MessageHud.MessageType.Center, $"Invalid item count: {split[i + 1]}");
                        return;
                    }
                    if (!int.TryParse(split[i + 2], out int quality) || quality < 1)
                    {
                        MessageHud.instance.ShowMessage(MessageHud.MessageType.Center, $"Invalid item quality: {split[i + 2]}");
                        return;
                    }
                    attachments[i / 3] = new Marketplace_DataTypes.ClientMarketSendData
                    {
                        ItemPrefab = itemPrefab,
                        Count = count,
                        Quality = quality
                    };
                }
            }
            
            ShowSendMail();
            CentralizedMailSystem_Client.SendMail_Admin(senderName, subject, message, user.UserID, attachments, expireTime);
        }
    
    }

    
    private static void Update(float obj)
    {
        if (Input.GetKeyDown(KeyCode.Escape) && IsOpenVisible)
        {
            HideOpenMail();
            return;
        }
        if (Input.GetKeyDown(KeyCode.Escape) && IsMainVisible)
        {
            Hide();
            return;
        }
    }

    public static void Show(bool canTakeAttachments)
    {
        DragUI_CMS.Ensure();
        CanTakeAttachments = canTakeAttachments;
        MAIN.SetActive(true);
        ShowMyMail();
    }
    
    private static void ShowMyMail()
    {
        MyMailsPage.SetActive(true);
        SendMailPage.SetActive(false);
        SetPage(1);
    }

    private static void ShowSendMail_User() 
    {
        SendMail_Admin.SetActive(false);
        SendMail_User.SetActive(true);
        
        SendMail_Target_Dropdown.ClearOptions();
        List<string> options = new List<string> {"Select a player"};
        _tempUsers = CentralizedMailSystem_DataTypes.SyncedCMSUsers.Value.OrderBy(u => u.Name).ToList();
        foreach (DB.DB.CMS_User user in _tempUsers)
        {
            string userName = user.Name;
            string onlineString = user.IsOnline ? $"(<color=lime>Online</color>)" : $"(<color=red>Offline</color>)";
            options.Add($"{userName} {onlineString}");
        }
        SendMail_Target_Dropdown.AddOptions(options);
        SendMail_Target_Dropdown.value = 0;
        SendMail_Subject_User.text = "";
        SendMail_Message_User.text = "";
    }
    
    private static void ShowSendMail_Admin()
    {
        SendMail_Admin.SetActive(true);
        SendMail_User.SetActive(false);
        
        SendMail_Target_Dropdown.ClearOptions();
        List<string> options = new List<string> {"Select a player"};
        _tempUsers = CentralizedMailSystem_DataTypes.SyncedCMSUsers.Value.OrderBy(u => u.Name).ToList();
        _tempUsers.Insert(0, new DB.DB.CMS_User {Name = "All <color=lime>[Send to all users in database]</color>", UserID = "All"});
        foreach (DB.DB.CMS_User user in _tempUsers)
        {
            string userName = user.Name;
            string onlineString = user.IsOnline ? $"(<color=lime>Online</color>)" : $"(<color=red>Offline</color>)";
            if (user.UserID == "All") onlineString = "";
            options.Add($"{userName} {onlineString}");
        }
        SendMail_Target_Dropdown.AddOptions(options);
        SendMail_Target_Dropdown.value = 0;
        SendMail_SenderName_Admin.text = "";
        SendMail_Subject_Admin.text = "";
        SendMail_Message_Admin.text = "";
        SendMail_ExpireTime_Admin.text = "168";
        SendMail_Attachments_Admin.text = "";
    }
    
    private static void ShowSendMail()
    {
        HideOpenMail();
        MyMailsPage.SetActive(false);
        SendMailPage.SetActive(true);
        LOADING.SetActive(false);
        if (Utils.IsDebug_Strict) ShowSendMail_Admin();
        else ShowSendMail_User();
    }
    
    private static int _mailDataRevision = -1;
    private static void SetPage(int page)
    {
        HideOpenMail();
        CurrentPage = page;
        Mails.Clear();
        foreach (CMS_UI_Entry uiEntry in Entries) uiEntry.SetData(null);
        LOADING.SetActive(true);
        PageNumber.gameObject.SetActive(false);
        ++_mailDataRevision;
        ZRoutedRpc.instance.InvokeRoutedRPC("KGmarket CMS RequestMailAmount");
    }
    
    [MarketplaceRPC("CMS_ReceiveMailAmount", Market_Autoload.Type.Client)]
    public static void GetMailAmount(long sender, int totalMails)
    {
        if (!IsMainVisible || !IsLoading) return;
        TotalMail = totalMails;
        MaxPages = (TotalMail - 1) / CentralizedMailSystem_DataTypes.MaxPerPage + 1;
        if (CurrentPage > MaxPages) CurrentPage = MaxPages;
        PageNumber.text = $"{CurrentPage}/{MaxPages}";
        PageNumber.gameObject.SetActive(true);
        ZRoutedRpc.instance.InvokeRoutedRPC("KGmarket CMS RequestMailData", CurrentPage - 1, _mailDataRevision);
    }
    [MarketplaceRPC("CMS_ReceiveMailData", Market_Autoload.Type.Client)]
    public static void GetMailData(long sender, ZPackage pkg)
    {
        if (!IsMainVisible || !IsLoading) return;
        pkg.Decompress(); 
        int _revision = pkg.ReadInt(); 
        if (_revision != _mailDataRevision) return;
        int mailDataCount = pkg.ReadInt();
        Mails.Clear();
        for (int i = 0; i < mailDataCount; i++)
        {
            DB.DB.CMS_Entry mail = new();
            mail.Deserialize(ref pkg);
            Mails.Add(mail);
        }
        for(int i = 0; i < Mails.Count; i++)
        {
            if (i >= Entries.Count) break; 
            Entries[i].SetData(Mails[i]);
        }
        LOADING.SetActive(false);
    }
    [MarketplaceRPC("CMS_ReceiveUnreadMailAmount", Market_Autoload.Type.Client)]
    public static void GetUnreadMailAmount(long sender, int unreadMails)
    {
        UnreadMail = unreadMails;
        CheckUnreadMail();
    } 
    [HarmonyPatch(typeof(TextInput), nameof(TextInput.IsVisible))]
    [ClientOnlyPatch]
    private static class TextInput_IsVisible_Patch
    {
        [UsedImplicitly]
        private static void Postfix(TextInput __instance, ref bool __result) => __result |= IsMainVisible;
    }
    [HarmonyPatch(typeof(StoreGui), nameof(StoreGui.IsVisible))]
    [ClientOnlyPatch]
    private static class StoreGui_IsVisible_Patch
    {
        [UsedImplicitly]
        private static void Postfix(TextInput __instance, ref bool __result) => __result |= IsMainVisible;
    }
    [HarmonyPatch(typeof(FejdStartup), nameof(FejdStartup.Awake))]
    [ClientOnlyPatch]
    private static class Menu_OnLogoutYes_Patch
    {
        [UsedImplicitly]
        private static void Postfix()
        {
            HaveUnreadMail.SetActive(false);
            Hide();
        }
    }
}