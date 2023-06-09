﻿using Marketplace.Modules.Banker;
using Marketplace.Modules.Buffer;
using Marketplace.Modules.Gambler;
using Marketplace.Modules.Marketplace_NPC;
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
            Trader,
            Banker,
            Teleporter,
            Gambler,
            Buffer,
            Quests,
            Info,
            Transmogrification
        }

        private static GameObject UI;
        private static GameObject Content_Element;
        private static Transform Viewport_Content;
        private static readonly List<GameObject> Elements = new();

        private static GameObject View;
        private static GameObject View_Profiles;
        private static GameObject Left;
        private static GameObject Right;

        private static Image Profiles_Icon;
        private static Text Profiles_Text;
        private static readonly List<Image> Buttons_Images = new();


        private static readonly Dictionary<NPCtype_Internal, Sprite> Icons = new();

        private static readonly Dictionary<NPCtype_Internal, string> Texts = new()
        {
            { NPCtype_Internal.Marketplace, "$mpasn_Marketplace" },
            { NPCtype_Internal.Trader, "$mpasn_Trader" },
            { NPCtype_Internal.Banker, "$mpasn_Banker" },
            { NPCtype_Internal.Teleporter, "$mpasn_Teleporter" },
            { NPCtype_Internal.Gambler, "$mpasn_Gambler" },
            { NPCtype_Internal.Buffer, "$mpasn_Buffer" },
            { NPCtype_Internal.Quests, "$mpasn_Quests" },
            { NPCtype_Internal.Info, "$mpasn_Info" },
            { NPCtype_Internal.Transmogrification, "$mpasn_Transmog" }
        };

        public static void Init()
        {
            UI = UnityEngine.Object.Instantiate(AssetStorage.AssetStorage.asset.LoadAsset<GameObject>("PremiumMemebershipUI"));
            Viewport_Content = UI.transform.Find("Canvas/View_Profiles/CraftView/Scroll View/Viewport/Content");
            View = UI.transform.Find("Canvas/Open/View").gameObject;
            View_Profiles = UI.transform.Find("Canvas/View_Profiles").gameObject;
            Left = UI.transform.Find("Canvas/Open/Left").gameObject;
            Right = UI.transform.Find("Canvas/Open/Right").gameObject;
            Content_Element = AssetStorage.AssetStorage.asset.LoadAsset<GameObject>("PremiumUI_Element");
            UI.transform.Find("Canvas/Open").GetComponent<Button>().onClick.AddListener(ClickView);
            UnityEngine.Object.DontDestroyOnLoad(UI);
            UI.SetActive(false);
            Profiles_Icon = UI.transform.Find("Canvas/View_Profiles/Image").GetComponent<Image>();
            Profiles_Text = UI.transform.Find("Canvas/View_Profiles/Text").GetComponent<Text>();
            Icons[NPCtype_Internal.Marketplace] = AssetStorage.AssetStorage.asset.LoadAsset<Sprite>("marketicon_pm");
            Icons[NPCtype_Internal.Trader] = AssetStorage.AssetStorage.asset.LoadAsset<Sprite>("trader_pm");
            Icons[NPCtype_Internal.Banker] = AssetStorage.AssetStorage.asset.LoadAsset<Sprite>("banker_pm");
            Icons[NPCtype_Internal.Teleporter] = AssetStorage.AssetStorage.asset.LoadAsset<Sprite>("teleporter_pm");
            Icons[NPCtype_Internal.Gambler] = AssetStorage.AssetStorage.asset.LoadAsset<Sprite>("gamble_pm");
            Icons[NPCtype_Internal.Buffer] = AssetStorage.AssetStorage.asset.LoadAsset<Sprite>("buffer_pm");
            Icons[NPCtype_Internal.Quests] = AssetStorage.AssetStorage.asset.LoadAsset<Sprite>("quests_pm");
            Icons[NPCtype_Internal.Info] = AssetStorage.AssetStorage.asset.LoadAsset<Sprite>("info_pm");
            Icons[NPCtype_Internal.Transmogrification] = AssetStorage.AssetStorage.asset.LoadAsset<Sprite>("transmog_pm");
            UI.transform.Find("Canvas/Open/View/Marketplace").GetComponent<Button>().onClick
                .AddListener(() => ClickOpen(NPCtype_Internal.Marketplace));
            UI.transform.Find("Canvas/Open/View/Trader").GetComponent<Button>().onClick
                .AddListener(() => ClickOpen(NPCtype_Internal.Trader));
            UI.transform.Find("Canvas/Open/View/Banker").GetComponent<Button>().onClick
                .AddListener(() => ClickOpen(NPCtype_Internal.Banker));
            UI.transform.Find("Canvas/Open/View/Teleporter").GetComponent<Button>().onClick
                .AddListener(() => ClickOpen(NPCtype_Internal.Teleporter));
            UI.transform.Find("Canvas/Open/View/Gambler").GetComponent<Button>().onClick
                .AddListener(() => ClickOpen(NPCtype_Internal.Gambler));
            UI.transform.Find("Canvas/Open/View/Buffer").GetComponent<Button>().onClick
                .AddListener(() => ClickOpen(NPCtype_Internal.Buffer));
            UI.transform.Find("Canvas/Open/View/Quests").GetComponent<Button>().onClick
                .AddListener(() => ClickOpen(NPCtype_Internal.Quests));
            UI.transform.Find("Canvas/Open/View/Info").GetComponent<Button>().onClick
                .AddListener(() => ClickOpen(NPCtype_Internal.Info));
            UI.transform.Find("Canvas/Open/View/Transmogrification").GetComponent<Button>().onClick
                .AddListener(() => ClickOpen(NPCtype_Internal.Transmogrification));

            Buttons_Images.Add(UI.transform.Find("Canvas/Open/View/Marketplace").GetComponent<Image>());
            Buttons_Images.Add(UI.transform.Find("Canvas/Open/View/Trader").GetComponent<Image>());
            Buttons_Images.Add(UI.transform.Find("Canvas/Open/View/Banker").GetComponent<Image>());
            Buttons_Images.Add(UI.transform.Find("Canvas/Open/View/Teleporter").GetComponent<Image>());
            Buttons_Images.Add(UI.transform.Find("Canvas/Open/View/Gambler").GetComponent<Image>());
            Buttons_Images.Add(UI.transform.Find("Canvas/Open/View/Buffer").GetComponent<Image>());
            Buttons_Images.Add(UI.transform.Find("Canvas/Open/View/Quests").GetComponent<Image>());
            Buttons_Images.Add(UI.transform.Find("Canvas/Open/View/Info").GetComponent<Image>());
            Buttons_Images.Add(UI.transform.Find("Canvas/Open/View/Transmogrification").GetComponent<Image>());

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

        private static void ClickOpen(NPCtype_Internal type)
        {
            if (!DistancedUI_DataType.CurrentPremiumSystemData.Value.isAllowed || !Player.m_localPlayer) return;
            HideViewProfiles();
            if (type is not NPCtype_Internal.Marketplace)
            {
                Buttons_Images[(int)type].color = Color.green;
                Buttons_Images[(int)type].GetComponent<HoverOnButton>()._locked = true;
            }

            Menu.instance.OnClose();
            AssetStorage.AssetStorage.AUsrc.Play();
            Profiles_Icon.sprite = Icons[type];
            Profiles_Text.text = Localization.instance.Localize(Texts[type]);
            if (type is NPCtype_Internal.Marketplace)
            {
                if (DistancedUI_DataType.CurrentPremiumSystemData.Value.MarketplaceEnabled)
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

            View_Profiles.SetActive(true);

            List<string> premiumSource = type switch
            {
                NPCtype_Internal.Trader => DistancedUI_DataType.CurrentPremiumSystemData.Value.TraderProfiles,
                NPCtype_Internal.Banker => DistancedUI_DataType.CurrentPremiumSystemData.Value.BankerProfiles,
                NPCtype_Internal.Teleporter => DistancedUI_DataType.CurrentPremiumSystemData.Value.TeleporterProfiles,
                NPCtype_Internal.Gambler => DistancedUI_DataType.CurrentPremiumSystemData.Value.GamblerProfiles,
                NPCtype_Internal.Buffer => DistancedUI_DataType.CurrentPremiumSystemData.Value.BufferProfiles,
                NPCtype_Internal.Quests => DistancedUI_DataType.CurrentPremiumSystemData.Value.QuestProfiles,
                NPCtype_Internal.Info => DistancedUI_DataType.CurrentPremiumSystemData.Value.InfoProfiles,
                NPCtype_Internal.Transmogrification => DistancedUI_DataType.CurrentPremiumSystemData.Value.TransmogrificationProfiles,
                _ => new()
            };
            List<string> source = type switch
            {
                NPCtype_Internal.Trader => Trader_DataTypes.ClientSideItemList.Keys.ToList(),
                NPCtype_Internal.Banker => Banker_DataTypes.SyncedBankerProfiles.Value.Keys.ToList(),
                NPCtype_Internal.Teleporter => Teleporter_DataTypes.TeleporterDataServer.Value.Keys.ToList(),
                NPCtype_Internal.Gambler => Gambler_DataTypes.SyncedGamblerData.Value.Keys.ToList(), 
                NPCtype_Internal.Buffer => Buffer_DataTypes.ClientSideBufferProfiles.Keys.ToList(),
                NPCtype_Internal.Quests => Quests_DataTypes.SyncedQuestProfiles.Value.Keys.ToList(),
                NPCtype_Internal.Info => ServerInfo_DataTypes.ServerInfoData.Value.Keys.ToList(),
                NPCtype_Internal.Transmogrification => Transmogrification_DataTypes.TransmogData.Value.Keys.ToList(),
                _ => new()
            };

            foreach (string item in premiumSource)
            {
                if (!source.Contains(item)) continue;
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
            if (!DistancedUI_DataType.CurrentPremiumSystemData.Value.isAllowed || !Player.m_localPlayer) return;
            Menu.instance.OnClose();
            AssetStorage.AssetStorage.AUsrc.Play();
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
            if (!DistancedUI_DataType.CurrentPremiumSystemData.Value.isAllowed || !Player.m_localPlayer) return;
            Menu.instance.OnClose();
            AssetStorage.AssetStorage.AUsrc.Play();
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
            }
        }

        public static void Show(bool allowMarket)
        {
            Buttons_Images[0].gameObject.SetActive(allowMarket);
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

        [HarmonyPatch(typeof(Menu), nameof(Menu.IsVisible))]
        [ClientOnlyPatch]
        private static class Menu_IsVisible_Patch
        {
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
            private static void Postfix()
            {
                Hide();
            }
        }
    }

    
    public class HoverOnButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private Image _image;
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
