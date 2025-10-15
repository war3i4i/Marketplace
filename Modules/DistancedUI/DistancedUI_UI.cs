using Marketplace.ExternalLoads;
using Marketplace.Modules.Banker;
using Marketplace.Modules.Buffer;
using Marketplace.Modules.CMS;
using Marketplace.Modules.Dialogues;
using Marketplace.Modules.Gambler;
using Marketplace.Modules.MainMarketplace;
using Marketplace.Modules.Quests;
using Marketplace.Modules.ServerInfo;
using Marketplace.Modules.Teleporter;
using Marketplace.Modules.Trader;
using Marketplace.Modules.Transmogrification;
using UnityEngine.EventSystems;

namespace Marketplace.Modules.DistancedUI;

public static class DistancedUI_UI
{
      private enum NPCtype_Internal
        {
            Marketplace,
            Mail,
            Trader,
            Banker,
            Teleporter,
            Gambler,
            Buffer,
            Quests,
            Info,
            Transmogrification,
            Dialogues,
        }

        private static GameObject UI = null!;
        private static GameObject Content_Element = null!;
        private static Transform Viewport_Content = null!;
        private static readonly List<GameObject> Elements = new();

        private static GameObject View = null!;
        private static GameObject View_Profiles = null!;
        private static GameObject Left = null!;
        private static GameObject Right = null!;

        private static Image Profiles_Icon = null!;
        private static Text Profiles_Text = null!;
        private static readonly List<Image> Buttons_Images = new();
        
        private static readonly Dictionary<NPCtype_Internal, Sprite> Icons = new();

        private static readonly Dictionary<NPCtype_Internal, string> Texts = new()
        {
            { NPCtype_Internal.Marketplace, "$mpasn_Marketplace" },
            { NPCtype_Internal.Mail, "$mpasn_Mail" },
            { NPCtype_Internal.Trader, "$mpasn_Trader" },
            { NPCtype_Internal.Banker, "$mpasn_Banker" },
            { NPCtype_Internal.Teleporter, "$mpasn_Teleporter" },
            { NPCtype_Internal.Gambler, "$mpasn_Gambler" },
            { NPCtype_Internal.Buffer, "$mpasn_Buffer" },
            { NPCtype_Internal.Quests, "$mpasn_Quests" },
            { NPCtype_Internal.Info, "$mpasn_Info" },
            { NPCtype_Internal.Transmogrification, "$mpasn_Transmog" },
            { NPCtype_Internal.Dialogues, "$mpasn_Dialogues" }
        };

        public static void Init()
        {
            UI = UnityEngine.Object.Instantiate(AssetStorage.asset.LoadAsset<GameObject>("MarketplaceDistanedUI"));
            Viewport_Content = UI.transform.Find("Canvas/View_Profiles/CraftView/Scroll View/Viewport/Content");
            View = UI.transform.Find("Canvas/Open/View").gameObject;
            View_Profiles = UI.transform.Find("Canvas/View_Profiles").gameObject;
            Left = UI.transform.Find("Canvas/Open/Left").gameObject;
            Right = UI.transform.Find("Canvas/Open/Right").gameObject;
            Content_Element = AssetStorage.asset.LoadAsset<GameObject>("MarketplaceDistanedUI_Element");
            UI.transform.Find("Canvas/Open").GetComponent<Button>().onClick.AddListener(ClickView);
            UnityEngine.Object.DontDestroyOnLoad(UI);
            UI.SetActive(false);
            Profiles_Icon = UI.transform.Find("Canvas/View_Profiles/Image").GetComponent<Image>();
            Profiles_Text = UI.transform.Find("Canvas/View_Profiles/Text").GetComponent<Text>();
            Icons[NPCtype_Internal.Marketplace] = AssetStorage.asset.LoadAsset<Sprite>("marketicon_pm");
            Icons[NPCtype_Internal.Mail] = AssetStorage.asset.LoadAsset<Sprite>("mailicon_pm");
            Icons[NPCtype_Internal.Trader] = AssetStorage.asset.LoadAsset<Sprite>("trader_pm");
            Icons[NPCtype_Internal.Banker] = AssetStorage.asset.LoadAsset<Sprite>("banker_pm");
            Icons[NPCtype_Internal.Teleporter] = AssetStorage.asset.LoadAsset<Sprite>("teleporter_pm");
            Icons[NPCtype_Internal.Gambler] = AssetStorage.asset.LoadAsset<Sprite>("gamble_pm");
            Icons[NPCtype_Internal.Buffer] = AssetStorage.asset.LoadAsset<Sprite>("buffer_pm");
            Icons[NPCtype_Internal.Quests] = AssetStorage.asset.LoadAsset<Sprite>("quests_pm"); 
            Icons[NPCtype_Internal.Info] = AssetStorage.asset.LoadAsset<Sprite>("info_pm");
            Icons[NPCtype_Internal.Transmogrification] = AssetStorage.asset.LoadAsset<Sprite>("transmog_pm");
            Icons[NPCtype_Internal.Dialogues] = AssetStorage.asset.LoadAsset<Sprite>("dialogues_pm");
            UI.transform.Find("Canvas/Open/View/Marketplace").GetComponent<Button>().onClick.AddListener(() => ClickOpen(NPCtype_Internal.Marketplace));
            UI.transform.Find("Canvas/Open/View/Mail").GetComponent<Button>().onClick.AddListener(() => ClickOpen(NPCtype_Internal.Mail));
            UI.transform.Find("Canvas/Open/View/Trader").GetComponent<Button>().onClick.AddListener(() => ClickOpen(NPCtype_Internal.Trader));
            UI.transform.Find("Canvas/Open/View/Banker").GetComponent<Button>().onClick.AddListener(() => ClickOpen(NPCtype_Internal.Banker));
            UI.transform.Find("Canvas/Open/View/Teleporter").GetComponent<Button>().onClick.AddListener(() => ClickOpen(NPCtype_Internal.Teleporter));
            UI.transform.Find("Canvas/Open/View/Gambler").GetComponent<Button>().onClick.AddListener(() => ClickOpen(NPCtype_Internal.Gambler));
            UI.transform.Find("Canvas/Open/View/Buffer").GetComponent<Button>().onClick.AddListener(() => ClickOpen(NPCtype_Internal.Buffer));
            UI.transform.Find("Canvas/Open/View/Quests").GetComponent<Button>().onClick.AddListener(() => ClickOpen(NPCtype_Internal.Quests));
            UI.transform.Find("Canvas/Open/View/Info").GetComponent<Button>().onClick.AddListener(() => ClickOpen(NPCtype_Internal.Info));
            UI.transform.Find("Canvas/Open/View/Transmogrification").GetComponent<Button>().onClick.AddListener(() => ClickOpen(NPCtype_Internal.Transmogrification));
            UI.transform.Find("Canvas/Open/View/Dialogues").GetComponent<Button>().onClick.AddListener(() => ClickOpen(NPCtype_Internal.Dialogues));

            Buttons_Images.Add(UI.transform.Find("Canvas/Open/View/Marketplace").GetComponent<Image>());
            Buttons_Images.Add(UI.transform.Find("Canvas/Open/View/Mail").GetComponent<Image>());
            Buttons_Images.Add(UI.transform.Find("Canvas/Open/View/Trader").GetComponent<Image>());
            Buttons_Images.Add(UI.transform.Find("Canvas/Open/View/Banker").GetComponent<Image>());
            Buttons_Images.Add(UI.transform.Find("Canvas/Open/View/Teleporter").GetComponent<Image>());
            Buttons_Images.Add(UI.transform.Find("Canvas/Open/View/Gambler").GetComponent<Image>());
            Buttons_Images.Add(UI.transform.Find("Canvas/Open/View/Buffer").GetComponent<Image>());
            Buttons_Images.Add(UI.transform.Find("Canvas/Open/View/Quests").GetComponent<Image>());
            Buttons_Images.Add(UI.transform.Find("Canvas/Open/View/Info").GetComponent<Image>());
            Buttons_Images.Add(UI.transform.Find("Canvas/Open/View/Transmogrification").GetComponent<Image>());
            Buttons_Images.Add(UI.transform.Find("Canvas/Open/View/Dialogues").GetComponent<Image>());

            foreach (Image image in Buttons_Images)
            {
                image.gameObject.AddComponent<HoverOnButton>();
            }

            Default();
        }

        private static void ResetColors()
        {
            foreach (Image image in Buttons_Images)
            {
                image.color = Color.white;
                image.GetComponent<HoverOnButton>()._locked = false;
            }
        }

        private static bool HasAny(NPCtype_Internal type)
        {
            List<string> distancedSource = type switch
            {
                NPCtype_Internal.Trader => DistancedUI_DataType.SyncedDistancedUIData.Value.TraderProfiles,
                NPCtype_Internal.Banker => DistancedUI_DataType.SyncedDistancedUIData.Value.BankerProfiles,
                NPCtype_Internal.Teleporter => DistancedUI_DataType.SyncedDistancedUIData.Value.TeleporterProfiles,
                NPCtype_Internal.Gambler => DistancedUI_DataType.SyncedDistancedUIData.Value.GamblerProfiles,
                NPCtype_Internal.Buffer => DistancedUI_DataType.SyncedDistancedUIData.Value.BufferProfiles,
                NPCtype_Internal.Quests => DistancedUI_DataType.SyncedDistancedUIData.Value.QuestProfiles,
                NPCtype_Internal.Info => DistancedUI_DataType.SyncedDistancedUIData.Value.InfoProfiles,
                NPCtype_Internal.Transmogrification => DistancedUI_DataType.SyncedDistancedUIData.Value.TransmogrificationProfiles,
                NPCtype_Internal.Dialogues => DistancedUI_DataType.SyncedDistancedUIData.Value.DialogueProfiles,
                _ => new()
            };
            List<string> syncedSource = type switch
            {
                NPCtype_Internal.Trader => Trader_DataTypes.ClientSideItemList.Keys.ToList(),
                NPCtype_Internal.Banker => Banker_DataTypes.SyncedBankerProfiles.Value.Keys.ToList(),
                NPCtype_Internal.Teleporter => Teleporter_DataTypes.SyncedTeleporterData.Value.Keys.ToList(),
                NPCtype_Internal.Gambler => Gambler_DataTypes.SyncedGamblerData.Value.Keys.ToList(), 
                NPCtype_Internal.Buffer => Buffer_DataTypes.ClientSideBufferProfiles.Keys.ToList(),
                NPCtype_Internal.Quests => Quests_DataTypes.SyncedQuestProfiles.Value.Keys.ToList(),
                NPCtype_Internal.Info => ServerInfo_DataTypes.SyncedServerInfoData.Value.Keys.ToList(),
                NPCtype_Internal.Transmogrification => Transmogrification_DataTypes.SyncedTransmogData.Value.Keys.ToList(),
                NPCtype_Internal.Dialogues => Dialogues_DataTypes.SyncedDialoguesData.Value.Select(x => x.UID).ToList(),
                _ => new()
            };
            
            return distancedSource.Any(syncedSource.Contains);
        }

        private static void ClickOpen(NPCtype_Internal type)
        {
            if (!DistancedUI_DataType.SyncedDistancedUIData.Value.Enabled || !Player.m_localPlayer) return;
            HideViewProfiles(); 
            if (type is not (NPCtype_Internal.Marketplace or NPCtype_Internal.Mail))
            {
                Buttons_Images[(int)type].color = Color.green;
                Buttons_Images[(int)type].GetComponent<HoverOnButton>()._locked = true;
            } 

            Menu.instance.OnClose();
            AssetStorage.AUsrc.Play();
            Profiles_Icon.sprite = Icons[type];
            Profiles_Text.text = Localization.instance.Localize(Texts[type]);
            if (type is NPCtype_Internal.Marketplace)
            {
                if (DistancedUI_DataType.SyncedDistancedUIData.Value.MarketplaceEnabled)
                {
                    if (Marketplace_UI.IsPanelVisible())
                    {
                        Marketplace_UI.Hide();
                    }
                    else
                    {
                        Marketplace_UI.Show();
                    }
                }
                return;
            }

            if (type is NPCtype_Internal.Mail)
            {
                if (DistancedUI_DataType.SyncedDistancedUIData.Value.MailEnabled)
                {
                    if (CentralizedMailSystem_UI.IsMainVisible)
                    {
                        CentralizedMailSystem_UI.Hide();
                    }
                    else
                    {
                        CentralizedMailSystem_UI.Show(DistancedUI_DataType.SyncedDistancedUIData.Value.CanTakeMailAttachments);
                    }
                }
                return;
            }

            View_Profiles.SetActive(true);

            List<string> distancedSource = type switch
            {
                NPCtype_Internal.Trader => DistancedUI_DataType.SyncedDistancedUIData.Value.TraderProfiles,
                NPCtype_Internal.Banker => DistancedUI_DataType.SyncedDistancedUIData.Value.BankerProfiles,
                NPCtype_Internal.Teleporter => DistancedUI_DataType.SyncedDistancedUIData.Value.TeleporterProfiles,
                NPCtype_Internal.Gambler => DistancedUI_DataType.SyncedDistancedUIData.Value.GamblerProfiles,
                NPCtype_Internal.Buffer => DistancedUI_DataType.SyncedDistancedUIData.Value.BufferProfiles,
                NPCtype_Internal.Quests => DistancedUI_DataType.SyncedDistancedUIData.Value.QuestProfiles,
                NPCtype_Internal.Info => DistancedUI_DataType.SyncedDistancedUIData.Value.InfoProfiles,
                NPCtype_Internal.Transmogrification => DistancedUI_DataType.SyncedDistancedUIData.Value.TransmogrificationProfiles,
                NPCtype_Internal.Dialogues => DistancedUI_DataType.SyncedDistancedUIData.Value.DialogueProfiles,
                _ => new()
            };
            List<string> syncedSource = type switch
            {
                NPCtype_Internal.Trader => Trader_DataTypes.ClientSideItemList.Keys.ToList(),
                NPCtype_Internal.Banker => Banker_DataTypes.SyncedBankerProfiles.Value.Keys.ToList(),
                NPCtype_Internal.Teleporter => Teleporter_DataTypes.SyncedTeleporterData.Value.Keys.ToList(),
                NPCtype_Internal.Gambler => Gambler_DataTypes.SyncedGamblerData.Value.Keys.ToList(), 
                NPCtype_Internal.Buffer => Buffer_DataTypes.ClientSideBufferProfiles.Keys.ToList(),
                NPCtype_Internal.Quests => Quests_DataTypes.SyncedQuestProfiles.Value.Keys.ToList(),
                NPCtype_Internal.Info => ServerInfo_DataTypes.SyncedServerInfoData.Value.Keys.ToList(),
                NPCtype_Internal.Transmogrification => Transmogrification_DataTypes.SyncedTransmogData.Value.Keys.ToList(),
                NPCtype_Internal.Dialogues => Dialogues_DataTypes.SyncedDialoguesData.Value.Select(x => x.UID).ToList(),
                _ => new()
            };

            foreach (string item in distancedSource)
            {
                if (!syncedSource.Contains(item)) continue;
                GameObject element = UnityEngine.Object.Instantiate(Content_Element, Viewport_Content);
                element.AddComponent<HoverOnButton>();
                string toUppper = "";
                for (int i = 0; i < item.Length; ++i)
                {
                    if (i == 0 || item[i - 1] == '_')
                    {
                        toUppper += char.ToUpper(item[i]);
                    }
                    else
                    {
                        toUppper += (item[i] == '_' ? ' ' : item[i]);
                    }
                }

                element.transform.Find("Text").GetComponent<Text>().text = toUppper;
                element.GetComponent<Button>().onClick
                    .AddListener(() => ClickElement(item, type, toUppper));
                Elements.Add(element);
            }
        }

        private static void ClickElement(string profile, NPCtype_Internal type, string _NPCname)
        {
            if (!DistancedUI_DataType.SyncedDistancedUIData.Value.Enabled || !Player.m_localPlayer) return;
            Menu.instance.OnClose();
            AssetStorage.AUsrc.Play();
            HideViewProfiles();
            HideView();
            switch (type)
            {
                case NPCtype_Internal.Trader:
                    Trader_UI.Show(profile, _NPCname);
                    break;
                case NPCtype_Internal.Banker:
                    Banker_UI.Show(profile, _NPCname);
                    break;
                case NPCtype_Internal.Teleporter:
                    Teleporter_Main_Client.ShowTeleporterUI(profile);
                    break;
                case NPCtype_Internal.Gambler:
                    Gambler_UI.Show(profile);
                    break;
                case NPCtype_Internal.Buffer:
                    Buffer_UI.Show(profile, _NPCname);
                    break;
                case NPCtype_Internal.Quests:
                    Quests_UIs.QuestUI.Show(profile, _NPCname);
                    break;
                case NPCtype_Internal.Info:
                    ServerInfo_UI.Show(profile, _NPCname);
                    break;
                case NPCtype_Internal.Transmogrification:
                    Transmogrification_UI.Show(profile, _NPCname);
                    break;
                case NPCtype_Internal.Dialogues:
                    Dialogues_UI.LoadDialogue(null, profile);
                    break;
            }
        }

        private static bool IsViewVisible()
        {
            return UI && View.activeInHierarchy;
        }

        public static bool IsViewProfilesVisible()
        {
            return UI && View_Profiles.activeInHierarchy;
        }

        private static void Default()
        {
            View.SetActive(false);
            HideViewProfiles();
            Left.SetActive(true);
            Right.SetActive(false);
        }

        public static void ClickView()
        {
            ResetColors();
            if (!DistancedUI_DataType.SyncedDistancedUIData.Value.Enabled || !Player.m_localPlayer) return;
            Menu.instance.OnClose();
            AssetStorage.AUsrc.Play();
            View_Profiles.SetActive(false);
            bool active = View.activeSelf;
            View.SetActive(!active);
            if (active)
            {
                Left.SetActive(true);
                Right.SetActive(false);
            }
            else
            {
                Left.SetActive(false);
                Right.SetActive(true);
                Buttons_Images[2].gameObject.SetActive(HasAny(NPCtype_Internal.Trader));
                Buttons_Images[3].gameObject.SetActive(HasAny(NPCtype_Internal.Banker));
                Buttons_Images[4].gameObject.SetActive(HasAny(NPCtype_Internal.Teleporter));
                Buttons_Images[5].gameObject.SetActive(HasAny(NPCtype_Internal.Gambler));
                Buttons_Images[6].gameObject.SetActive(HasAny(NPCtype_Internal.Buffer));
                Buttons_Images[7].gameObject.SetActive(HasAny(NPCtype_Internal.Quests));
                Buttons_Images[8].gameObject.SetActive(HasAny(NPCtype_Internal.Info));
                Buttons_Images[9].gameObject.SetActive(HasAny(NPCtype_Internal.Transmogrification));
                Buttons_Images[10].gameObject.SetActive(HasAny(NPCtype_Internal.Dialogues));
            }
        }

        public static void Show(bool allowMarket, bool allowMail)
        {
            if (!ZNetScene.instance) return;
            Buttons_Images[0].gameObject.SetActive(allowMarket);
            Buttons_Images[1].gameObject.SetActive(allowMail);
            Default();
            UI.SetActive(true);
        }

        public static void Hide()
        {
            Default();
            UI.SetActive(false);
        }

        public static void HideViewProfiles()
        {
            Elements.ForEach(UnityEngine.Object.Destroy);
            Elements.Clear();
            View_Profiles.SetActive(false);
            ResetColors();
        }

        private static void HideView()
        {
            View.SetActive(false);
            Left.SetActive(true);
            Right.SetActive(false);
        }

        [HarmonyPatch(typeof(TextInput), nameof(TextInput.IsVisible))]
        [ClientOnlyPatch]
        private static class Menu_IsVisible_Patch
        {
            [UsedImplicitly]
private static void Postfix(ref bool __result)
            {
                if (IsViewProfilesVisible() || IsViewVisible())
                {
                    __result = true;
                }
            }
        }

        [HarmonyPatch(typeof(FejdStartup), nameof(FejdStartup.Awake))]
        [ClientOnlyPatch]
        private static class Menu_OnLogoutYes_Patch
        {
            [UsedImplicitly]
private static void Postfix()
            {
                Hide();
            }
        }
    }

    
    public class HoverOnButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private Image _image = null!;
        public bool _locked;
 
        private void Awake()
        { 
            _image = GetComponent<Image>();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (_locked) return;
            _image.color = Color.green;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (_locked) return;
            _image.color = Color.white;
        }
    }
