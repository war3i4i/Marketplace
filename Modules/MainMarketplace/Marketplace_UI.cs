using System.Diagnostics;
using BepInEx.Configuration;
using Marketplace.ExternalLoads;
using Marketplace.Modules.Global_Options;
using Object = UnityEngine.Object;
 
namespace Marketplace.Modules.MainMarketplace;

public static class Marketplace_UI
{
    private const int MAXITEMSPERPAGE = 14;
    private static GameObject UI = null!;
    private static GameObject ElementBUY = null!;
    private static GameObject ElementSELL = null!;
    private static Action OnSort = null!;
    private static readonly List<Marketplace_DataTypes.ClientMarketSendData> InventorySellData = new();
    private static List<Marketplace_DataTypes.ClientMarketSendData> InventorySellDataSORTED = new();
    private static List<DB.DB.MarketSlot> ServerMarketSendDataSORTED = new();
    private static readonly List<GameObject> CurrentGameObjects = new();
    private static Marketplace_DataTypes.ItemData_ItemCategory currentCategory;
    public static Marketplace_DataTypes.MarketMode currentMarketMode;
    private static Marketplace_DataTypes.SortBy currentSortMode;
    private static Marketplace_DataTypes.SortType currentSortType;
    private static readonly Button[] CategoryButtons = new Button[6];
    private static Button BuyButtonCategory = null!;
    private static Button SellButtonCategory = null!;
    private static readonly Color DefaultCategoryColorNotActive = new(1, 0.6941177f, 0);
    private static readonly Color DefaultCategoryColorActive = new(0.08995318f, 1, 0);
    private static string SEARCHVALUE = "";
    private static int CurrentPage;
    private static int CurrentMaxPage;
    private static Marketplace_DataTypes.ClientMarketSendData CurrentSendData;
    private static DB.DB.MarketSlot CurrentBuyData;
    private static int CurrentPrice;
    private static int CurrentQuantity;
    private static int CurrentBUYQuantity;
    private static bool IsAnon;
    private static bool MySalesOnly;
    private static Text PageNumber = null!;
    private static Transform BUYTAB = null!;
    private static Transform SELLTAB = null!;
    private static Text GoldCount = null!;
    private static Text MySalesText = null!;
    private static List<ContentSizeFitter> ALLFILTERS = null!;
    private static List<Scrollbar> ALLSCROLLS = null!;
    private static List<GameObject> JC_Api = null!;
    
    private static TMP_Dropdown CurrencyDropdown = null!;
    
    private static GameObject LOADING = null!;

    private enum MarketSize
    {
        Large,
        Medium,
        Small
    }
    

    private static void ERRORBLOCKED()
    {
        Hide();
        MessageHud.instance.ShowMessage(MessageHud.MessageType.Center,
            $"<color=red>{Localization.instance.Localize($"$mpasn_youareblocked")}</color>");
    }

    private static void ERRORLIMIT()
    {
        Hide();
        MessageHud.instance.ShowMessage(MessageHud.MessageType.Center,
            $"<color=red>{Localization.instance.Localize("$mpasn_limitmarket")}</color>");
    }


    private static void ERROR()
    {
        Hide();
        MessageHud.instance.ShowMessage(MessageHud.MessageType.Center,
            $"<color=red>{Localization.instance.Localize("$mpasn_error")}</color>");
    }

    public static bool IsPanelVisible()
    {
        return UI && UI.activeSelf;
    }

    public static void Init()
    {
        UI = Object.Instantiate(AssetStorage.asset.LoadAsset<GameObject>("MarketplaceUI"));
        ALLFILTERS = UI.GetComponentsInChildren<ContentSizeFitter>(true).ToList();
        ALLSCROLLS = UI.GetComponentsInChildren<Scrollbar>(true).ToList();
        ElementBUY = AssetStorage.asset.LoadAsset<GameObject>("BuyTabGO");
        ElementSELL = AssetStorage.asset.LoadAsset<GameObject>("SellTabGO");
        OnSort += SortValueList;
        Object.DontDestroyOnLoad(UI);
        string[] EnumNames = Enum.GetNames(typeof(Marketplace_DataTypes.ItemData_ItemCategory));
        for (int i = 0; i < EnumNames.Length; ++i)
        {
            int test = i;
            CategoryButtons[i] = UI.transform
                .Find("Canvas/BACKGROUND/CATEGORIES/" + (Marketplace_DataTypes.ItemData_ItemCategory)i)
                .GetComponent<Button>();
            CategoryButtons[i].onClick.AddListener(delegate { SetCategory(test); });
        }

        BuyButtonCategory = UI.transform.Find("Canvas/BACKGROUND/BUYSELL/BUY").GetComponent<Button>();
        SellButtonCategory = UI.transform.Find("Canvas/BACKGROUND/BUYSELL/SELL").GetComponent<Button>();
        BuyButtonCategory.onClick.AddListener(ClickBUYcategoryButton);
        SellButtonCategory.onClick.AddListener(ClickSELLcategoryButton);
        BUYTAB = UI.transform.Find("Canvas/BACKGROUND/BUYTAB");
        SELLTAB = UI.transform.Find("Canvas/BACKGROUND/SELLTAB");
        PageNumber = UI.transform.Find("Canvas/BACKGROUND/MainButtonsTab/Page/Text").GetComponent<Text>();
        UI.transform.Find("Canvas/BACKGROUND/MainButtonsTab/PageRight").GetComponent<Button>().onClick
            .AddListener(delegate { PageIncrementor(1); });
        UI.transform.Find("Canvas/BACKGROUND/MainButtonsTab/PageLeft").GetComponent<Button>().onClick
            .AddListener(delegate { PageIncrementor(-1); });
        UI.transform.Find("Canvas/BACKGROUND/SEARCH/Input").GetComponent<InputField>().onValueChanged
            .AddListener(SearchTabValue);
        SELLTAB.Find("AFTERPRESS/SetQuantity").GetComponent<InputField>().onValueChanged
            .AddListener(SetQuantityValueChange);
        SELLTAB.Find("AFTERPRESS/SetPrice").GetComponent<InputField>().onValueChanged
            .AddListener(SetPriceValueChange);
        BUYTAB.Find("AFTERPRESS/SetQuantity").GetComponent<InputField>().onValueChanged
            .AddListener(SetBuyQuantityChange);
        SELLTAB.transform.Find("itemname").GetComponent<Button>().onClick
            .AddListener(delegate { ChangeSortBy(1); });
        SELLTAB.transform.Find("count").GetComponent<Button>().onClick
            .AddListener(delegate { ChangeSortBy(2); });
        BUYTAB.transform.Find("itemname").GetComponent<Button>().onClick
            .AddListener(delegate { ChangeSortBy(1); });
        BUYTAB.transform.Find("count").GetComponent<Button>().onClick
            .AddListener(delegate { ChangeSortBy(2); });
        BUYTAB.transform.Find("price").GetComponent<Button>().onClick
            .AddListener(delegate { ChangeSortBy(3); });
        BUYTAB.transform.Find("seller").GetComponent<Button>().onClick
            .AddListener(delegate { ChangeSortBy(4); });
        SELLTAB.transform.Find("AFTERPRESS/BUTTON").GetComponent<Button>().onClick.AddListener(SELLITEMCLICK);
        BUYTAB.transform.Find("AFTERPRESS/BUTTON").GetComponent<Button>().onClick.AddListener(BUYITEMCLICK);
        GoldCount = UI.transform.Find("Canvas/BACKGROUND/MainButtonsTab/Gold/Text").GetComponent<Text>();
        UI.transform.Find("Canvas/BACKGROUND/MainButtonsTab/MySales").GetComponent<Button>().onClick
            .AddListener(MySalesClick);
        MySalesText = UI.transform.Find("Canvas/BACKGROUND/MainButtonsTab/MySales/Text").GetComponent<Text>();
        CurrencyDropdown = SELLTAB.transform.Find("AFTERPRESS/Currency").GetComponent<TMP_Dropdown>();
        CurrencyDropdown.onValueChanged.AddListener((_) => OnCurrencyChange());
        UI.transform.Find("Canvas/BACKGROUND/MainButtonsTab/Close").GetComponent<Button>().onClick.AddListener(
            delegate
            { 
                Hide();
                AssetStorage.AUsrc.Play();
            });
        UI.transform.Find("Canvas/BACKGROUND/MainButtonsTab/Size").GetComponent<Button>().onClick
            .AddListener(() => ChangeSize(true));

        UI.transform.Find("Canvas/BACKGROUND/MainButtonsTab/Anon/click").GetComponent<Button>().onClick
            .AddListener(ClickAnonButton);
        UI.SetActive(false); 
   
        JC_Api = 
        [   
            global::Utils.FindChild(SELLTAB, "TrannyHoles").gameObject,
            global::Utils.FindChild(BUYTAB, "TrannyHoles").gameObject
        ];
        Localization.instance.Localize(UI.transform);

        _marketSize = Marketplace._thistype.Config.Bind("Marketplace", "Market Size", MarketSize.Large, "Market size");
        ChangeSize(false);
        
        LOADING = global::Utils.FindChild(UI.transform, "LOADING").gameObject;
        ReloadCurrency();
    } 

    private static void OnCurrencyChange()
    {
        SELLTAB.Find("AFTERPRESS/BUTTON/Text").GetComponent<Text>().text =
            $"<size=42>{Localization.instance.Localize("$mpasn_sellmarketplace")}</size>\n<color=#FF00FF>{Localization.instance.Localize("$mpasn_quantitymarketplace")}: {CurrentQuantity}</color> x <color=yellow>{CurrentPrice}</color>\n<color=#00FFFF>{Localization.instance.Localize("$mpasn_youget")}:</color> <color=yellow>{CurrentQuantity * CurrentPrice} {GetCurrentCurrencyName_Sell}</color>";
    } 
  
    private static string GetCurrentCurrencyName_Sell => ZNetScene.instance.GetPrefab(CurrencyMapper[CurrencyDropdown.value])?.GetComponent<ItemDrop>()?.m_itemData.m_shared.m_name.Localize() ?? "<color=red>Error</color>";
    private static string GetCurrentCurrencyName_Buy => CurrentBuyData != null ? ZNetScene.instance.GetPrefab(CurrentBuyData.Currency)?.GetComponent<ItemDrop>()?.m_itemData.m_shared.m_name.Localize() : "<color=red>Error</color>";
    private static List<string> CurrencyMapper = []; 
    [HarmonyPatch(typeof(ZNetScene),nameof(ZNetScene.Awake))]
    [ClientOnlyPatch]
    private static class ZNetScene_Awake_Patch
    {
        private static void Postfix() => ReloadCurrency();
    }  
    public static void ReloadCurrency()
    { 
        if (!ZNetScene.instance) return;
        CurrencyMapper.Clear();
        CurrencyDropdown.ClearOptions(); 
        if (Global_Configs.SyncedGlobalOptions.Value._possibleCurrencies == null) return;
        string[] currencies = Global_Configs.SyncedGlobalOptions.Value._possibleCurrencies;
        for (int i = 0; i < currencies.Length; i++) 
        { 
            ItemDrop item = ZNetScene.instance.GetPrefab(currencies[i])?.GetComponent<ItemDrop>();
            if (item == null) continue;
            CurrencyDropdown.options.Add(new TMP_Dropdown.OptionData
            {
                text = item.m_itemData.m_shared.m_name.Localize(),
                image = item.m_itemData.GetIcon()
            });
            CurrencyMapper.Add(currencies[i]);  
        }

        CurrencyDropdown.value = 0;
        CurrencyDropdown.interactable = CurrencyMapper.Count > 1;
        CurrencyDropdown.RefreshShownValue();
    }
    
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
            foreach (var text in UI.GetComponentsInChildren<TMP_Text>(true))
            { 
                text.font = tmp.font;
            } 
        }
    }

    private static Marketplace_DataTypes.ItemData_ItemCategory ChooseBestCategory(ItemDrop.ItemData item)
    {
        if (item.m_shared.m_itemType is ItemDrop.ItemData.ItemType.OneHandedWeapon
            or ItemDrop.ItemData.ItemType.TwoHandedWeapon or ItemDrop.ItemData.ItemType.Bow
            or ItemDrop.ItemData.ItemType.Torch or ItemDrop.ItemData.ItemType.Ammo
            or ItemDrop.ItemData.ItemType.TwoHandedWeaponLeft)
            return Marketplace_DataTypes.ItemData_ItemCategory.WEAPONS;

        if (item.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Consumable)
            return Marketplace_DataTypes.ItemData_ItemCategory.CONSUMABLE;

        if (item.m_shared.m_itemType is ItemDrop.ItemData.ItemType.Chest or ItemDrop.ItemData.ItemType.Helmet
            or ItemDrop.ItemData.ItemType.Legs or ItemDrop.ItemData.ItemType.Shield
            or ItemDrop.ItemData.ItemType.Shoulder
            or ItemDrop.ItemData.ItemType.Utility) return Marketplace_DataTypes.ItemData_ItemCategory.ARMOR;

        if (item.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Tool)
            return Marketplace_DataTypes.ItemData_ItemCategory.TOOLS;

        if (item.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Material)
            return Marketplace_DataTypes.ItemData_ItemCategory.RESOURCES;

        return Marketplace_DataTypes.ItemData_ItemCategory.ALL;
    }

    private static List<Marketplace_DataTypes.ClientMarketSendData> CollectInventoryPlayerData()
    {
        List<Marketplace_DataTypes.ClientMarketSendData> data = new List<Marketplace_DataTypes.ClientMarketSendData>();
        Player player = Player.m_localPlayer;
        List<ItemDrop.ItemData> list = player?.m_inventory?.GetAllItems()!;
        if (list == null || list.Count == 0) return data;
        foreach (ItemDrop.ItemData item in list)
            if (!Global_Configs.SyncedGlobalOptions.Value._blockedPrefabsServer.Replace(" ", "").Split(',').Contains(item.m_dropPrefab.name))
            {
                Marketplace_DataTypes.ItemData_ItemCategory best = ChooseBestCategory(item);
                string displayName = item.m_shared.m_name; 
                byte durabilityPercent = (byte)(item.GetDurabilityPercentage() * 100);
                string customDataJSON = JSON.ToJSON(item.m_customData);
                Marketplace_DataTypes.ClientMarketSendData alreadyExist = data.Find(x =>
                    x.ItemPrefab == item.m_dropPrefab?.name && x.Quality == item.m_quality &&
                    x.Variant == item.m_variant && x.CUSTOMdata == customDataJSON
                    && x.CrafterID == item.m_crafterID && x.CrafterName == item.m_crafterName
                    && x.ItemName == displayName && x.DurabilityPercent == durabilityPercent);

                if (alreadyExist != null && item.m_shared.m_maxQuality <= 1)
                {
                    alreadyExist.Count += item.m_stack;
                    continue;
                }

                Marketplace_DataTypes.ClientMarketSendData newData = new Marketplace_DataTypes.ClientMarketSendData
                {
                    ItemPrefab = item.m_dropPrefab.name,
                    Count = item.m_stack,
                    ItemCategory = best,
                    SellerName = "",
                    Quality = item.m_quality,
                    Variant = item.m_variant,
                    CUSTOMdata = customDataJSON,
                    CrafterID = item.m_crafterID,
                    CrafterName = item.m_crafterName,
                    DurabilityPercent = durabilityPercent 
                };
                data.Add(newData);
            }

        return data;
    }


    private static ConfigEntry<MarketSize> _marketSize = null!;
 
    private static void ChangeSize(bool increase)
    {
        if (increase)
        {
            AssetStorage.AUsrc.Play();
            _marketSize.Value = _marketSize.Value switch
            {
                MarketSize.Small => MarketSize.Medium,
                MarketSize.Medium => MarketSize.Large,
                MarketSize.Large => MarketSize.Small,
                _ => MarketSize.Large
            };
            Marketplace._thistype.Config.Save();
        }

        UI.transform.Find("Canvas/BACKGROUND").localScale = _marketSize.Value switch
        {
            MarketSize.Small => new Vector3(0.65f, 0.65f, 1f),
            MarketSize.Medium => new Vector3(0.85f, 0.85f, 1f),
            MarketSize.Large => new Vector3(1f, 1f, 1f),
            _ => new Vector3(1f, 1f, 1f)
        };

        UI.transform.Find("Canvas/BACKGROUND/MainButtonsTab/Size/Text").GetComponent<Text>().text =
            _marketSize.Value switch
            {
                MarketSize.Small => "S",
                MarketSize.Medium => "M",
                MarketSize.Large => "L",
                _ => "L"
            };
    }
    
    private static void ClickAnonButton()
    {
        AssetStorage.AUsrc.Play();
        IsAnon = !IsAnon; 
        UpdateAnnonIcon();
    } 
 
    private static void UpdateAnnonIcon()
    {
        UI.transform.Find("Canvas/BACKGROUND/MainButtonsTab/Anon/click/Image").gameObject.SetActive(IsAnon);
        UI.transform.Find("Canvas/BACKGROUND/MainButtonsTab/Anon/click/Image2").gameObject.SetActive(!IsAnon);
    }
    

    private static void MySalesClick()
    {  
        AssetStorage.AUsrc.Play(); 
        bool oldMySales = MySalesOnly;
        DefaultBUY();
        MySalesOnly = !oldMySales;  
        MySalesText.color = MySalesOnly ? Color.green : Color.red;
        OnSort();
    }

    private static void BUYITEMCLICK()
    {
        if (Global_Configs.SyncedGlobalOptions.Value._blockedPlayerList.Contains(Global_Configs._localUserID))
        {
            ERRORBLOCKED();
            return;
        }

        if (CurrentBuyData == null)
        {
            ERROR();
            return;
        }

        AssetStorage.AUsrc.Play();
        int totalPrice = 0;

        if (CurrentBUYQuantity > CurrentBuyData.Count)
            CurrentBUYQuantity = CurrentBuyData.Count;

        if (Global_Configs._localUserID != CurrentBuyData.SellerUserID)
        {
            totalPrice = CurrentBUYQuantity * CurrentBuyData.Price; 
            int goldCount = Utils.CustomCountItemsNoLevel(CurrentBuyData.Currency);
            if (goldCount >= totalPrice)
                Utils.CustomRemoveItems(CurrentBuyData.Currency, totalPrice, 1);
            else
                return;
        } 
        
        ZPackage pkg = new();
        pkg.Write(CurrentBuyData.ID);
        pkg.Write(totalPrice);
        pkg.Write(CurrentBUYQuantity);
        pkg.Write(CurrentBuyData.Currency);
        LOADING.SetActive(true);
        BUYTAB.transform.Find("AFTERPRESS").gameObject.SetActive(false);
        CurrentBuyData = null;
        ZRoutedRpc.instance.InvokeRoutedRPC("KGmarket RequestBuyItem", pkg);
    }

    private static void SELLITEMCLICK()
    {
        if (Global_Configs.SyncedGlobalOptions.Value._blockedPlayerList.Contains(Global_Configs._localUserID))
        {
            ERRORBLOCKED();
            return;
        }

        if (Marketplace_Main_Client.MarketData.Count(data =>
                data.SellerUserID == Global_Configs._localUserID) >=
            Global_Configs.SyncedGlobalOptions.Value._itemMarketLimit)
        {
            ERRORLIMIT();
            return;
        }

        if (CurrentSendData == null)
        {
            ERROR();
            return;
        }

        AssetStorage.AUsrc.Play();

        ItemDrop originalItem = ZNetScene.instance.GetPrefab(CurrentSendData.ItemPrefab)?.GetComponent<ItemDrop>()!;
        if (originalItem == null)
        {
            ERROR();
            return;
        }

        if (CurrentQuantity > CurrentSendData.Count) CurrentQuantity = CurrentSendData.Count;
        if (!CountItemByData(CurrentSendData, CurrentQuantity))
        {
            MessageHud.instance.ShowMessage(MessageHud.MessageType.Center,
                $"<color=red>{Localization.instance.Localize("$mpasn_noitems")}</color>");
            Hide();
            return;
        } 

        string currency = CurrencyMapper[CurrencyDropdown.value];
        if (string.IsNullOrWhiteSpace(currency) || !ZNetScene.instance.GetPrefab(currency) || Global_Configs.SyncedGlobalOptions.Value._possibleCurrencies == null ||
            !Global_Configs.SyncedGlobalOptions.Value._possibleCurrencies.Contains(currency))
        {
            Hide();
            return;
        }

        string sellerName = IsAnon ? "**************" : Player.m_localPlayer.GetPlayerName();
        Marketplace_DataTypes.ClientMarketSendData newData = new Marketplace_DataTypes.ClientMarketSendData
        {
            Count = CurrentQuantity,
            ItemCategory = CurrentSendData.ItemCategory,
            ItemPrefab = CurrentSendData.ItemPrefab,
            Price = CurrentPrice,
            Quality = CurrentSendData.Quality,
            SellerName = sellerName,
            Variant = CurrentSendData.Variant,
            CUSTOMdata = CurrentSendData.CUSTOMdata,
            CrafterName = CurrentSendData.CrafterName,
            CrafterID = CurrentSendData.CrafterID,
            DurabilityPercent = CurrentSendData.DurabilityPercent,
            Currency = currency,
        };
        string json = JSON.ToJSON(newData);
        ZRoutedRpc.instance.InvokeRoutedRPC("KGmarket ReceiveItem", json);
        SELLTAB.transform.Find("AFTERPRESS").gameObject.SetActive(false);
        ResetPage();
    }

    private static bool CountItemByData(Marketplace_DataTypes.ClientMarketSendData send, int cQuantity)
    {
        List<ItemDrop.ItemData> listOfItems = new List<ItemDrop.ItemData>();
        Player.m_localPlayer.m_inventory.GetAllItems(send.ItemName, listOfItems);
        IEnumerable<ItemDrop.ItemData> selectMany = listOfItems.Where(p =>
                p.m_variant == send.Variant && p.m_quality == send.Quality &&
                JSON.ToJSON(p.m_customData) == send.CUSTOMdata &&
                p.m_crafterID == send.CrafterID && p.m_crafterName == send.CrafterName)
            .Select(p => p);

        ItemDrop.ItemData[] itemDatas = selectMany as ItemDrop.ItemData[] ?? selectMany.ToArray();
        int count = itemDatas.Sum(p => p.m_stack);
        if (count < cQuantity) return false;

        while (cQuantity > 0)
        {
            foreach (ItemDrop.ItemData itemData in itemDatas)
            {
                if (cQuantity <= 0) break;
                if (itemData.m_stack <= cQuantity)
                {
                    cQuantity -= itemData.m_stack;
                    Player.m_localPlayer.m_inventory.RemoveItem(itemData);
                }
                else
                {
                    itemData.m_stack -= cQuantity;
                    cQuantity = 0;
                }
            }
        }

        return true;
    }


    private static void ChangeSortBy(int whichOne)
    {
        AssetStorage.AUsrc.Play();
        SELLTAB.transform.Find("itemname/Text").GetComponent<Text>().text =
            Localization.instance.Localize("$mpasn_itemnamemarket");
        SELLTAB.transform.Find("count/Text").GetComponent<Text>().text =
            Localization.instance.Localize("$mpasn_quantitymarketplace");
        BUYTAB.transform.Find("itemname/Text").GetComponent<Text>().text =
            Localization.instance.Localize("$mpasn_itemnamemarket");
        BUYTAB.transform.Find("count/Text").GetComponent<Text>().text =
            Localization.instance.Localize("$mpasn_quantitymarketplace");
        BUYTAB.transform.Find("price/Text").GetComponent<Text>().text =
            $"{Localization.instance.Localize("$mpasn_pricemarketplace")} <color=grey>x1</color>";
        BUYTAB.transform.Find("seller/Text").GetComponent<Text>().text =
            Localization.instance.Localize("$mpasn_sellernamemarket");
        if (currentSortMode == (Marketplace_DataTypes.SortBy)whichOne)
        {
            currentSortType = currentSortType == Marketplace_DataTypes.SortType.DOWN
                ? Marketplace_DataTypes.SortType.UP
                : Marketplace_DataTypes.SortType.DOWN;
        }
        else
        {
            currentSortMode = (Marketplace_DataTypes.SortBy)whichOne;
            currentSortType = Marketplace_DataTypes.SortType.UP;
        }

        string add = currentSortType == Marketplace_DataTypes.SortType.DOWN ? "⇩" : "⇧";
        if (whichOne == 1)
        {
            BUYTAB.transform.Find("itemname/Text").GetComponent<Text>().text =
                Localization.instance.Localize("$mpasn_itemnamemarket") + add;
            SELLTAB.transform.Find("itemname/Text").GetComponent<Text>().text =
                Localization.instance.Localize("$mpasn_itemnamemarket") + add;
        }

        if (whichOne == 2)
        {
            BUYTAB.transform.Find("count/Text").GetComponent<Text>().text =
                Localization.instance.Localize("$mpasn_quantitymarketplace") + add;
            SELLTAB.transform.Find("count/Text").GetComponent<Text>().text =
                Localization.instance.Localize("$mpasn_quantitymarketplace") + add;
        }

        if (whichOne == 3)
            BUYTAB.transform.Find("price/Text").GetComponent<Text>().text =
                $"{Localization.instance.Localize("$mpasn_pricemarketplace")} <color=grey>x1</color>" +
                add;

        if (whichOne == 4)
            BUYTAB.transform.Find("seller/Text").GetComponent<Text>().text =
                Localization.instance.Localize("$mpasn_sellernamemarket") + add;

        OnSort();
    }

    private const int maxprice = 9999999;

    private static void SetPriceValueChange(string value)
    {
        if (value != "0" && !string.IsNullOrWhiteSpace(value))
            AssetStorage.AUsrc.PlayOneShot(AssetStorage.TypeClip, 0.7f);
        if (string.IsNullOrWhiteSpace(value))
        {
            CurrentPrice = 0;
        }
        else
        {
            if (int.TryParse(value, out int result))
                CurrentPrice = result;
            else
                SELLTAB.Find("AFTERPRESS/SetPrice").GetComponent<InputField>().text = CurrentPrice.ToString();

            if (CurrentPrice > maxprice)
            {
                CurrentPrice = maxprice;
                SELLTAB.Find("AFTERPRESS/SetPrice").GetComponent<InputField>().text = CurrentPrice.ToString();
            }
        }
        
        SELLTAB.Find("AFTERPRESS/BUTTON/Text").GetComponent<Text>().text =
            $"<size=42>{Localization.instance.Localize("$mpasn_sellmarketplace")}</size>\n<color=#FF00FF>{Localization.instance.Localize("$mpasn_quantitymarketplace")}: {CurrentQuantity}</color> x <color=yellow>{CurrentPrice}</color>\n<color=#00FFFF>{Localization.instance.Localize("$mpasn_youget")}:</color> <color=yellow>{(long)CurrentQuantity * CurrentPrice} {GetCurrentCurrencyName_Sell}</color>";
    }


    private static bool SkipQuantityCheck;
    private static bool SkipNextSound;

    private static void SetBuyQuantityChange(string value)
    {
        if (CurrentBuyData == null || SkipQuantityCheck) return;
        if (!SkipNextSound)
            AssetStorage.AUsrc.PlayOneShot(AssetStorage.TypeClip, 0.7f);
        SkipNextSound = false;
        if (string.IsNullOrWhiteSpace(value))
        {
            CurrentBUYQuantity = 1; 
        }
        else
        {
            if (int.TryParse(value, out int result))
            {
                CurrentBUYQuantity = result > CurrentBuyData.Count ? CurrentBuyData.Count : result;
                if (CurrentBUYQuantity < 1)
                    CurrentBUYQuantity = 1;
            }
        }

        SkipQuantityCheck = true;
        BUYTAB.Find("AFTERPRESS/SetQuantity").GetComponent<InputField>().text = CurrentBUYQuantity.ToString();
        SkipQuantityCheck = false;

        int currencyCount = Utils.CustomCountItemsNoLevel(CurrentBuyData.Currency);
        string SELLBUYColor = "white";
        BUYTAB.transform.Find("AFTERPRESS/BUTTON").GetComponent<Image>().color = Color.red;
        if (currencyCount >= CurrentBuyData.Price * CurrentBUYQuantity)
        {
            BUYTAB.transform.Find("AFTERPRESS/BUTTON").GetComponent<Image>().color = Color.white;
            SELLBUYColor = "lime";
        } 
   
        string text =    
            $"<size=42><color={SELLBUYColor}>{Localization.instance.Localize("$mpasn_buymarketplace")}</color></size>\n<color=#FF00FF>{Localization.instance.Localize("$mpasn_quantitymarketplace")}: {CurrentBUYQuantity}</color> x <color=yellow>{CurrentBuyData.Price}</color>\n<color=#00FFFF>{Localization.instance.Localize("$mpasn_youneed")}:</color> <color=yellow>{CurrentBUYQuantity * CurrentBuyData.Price} {GetCurrentCurrencyName_Buy}</color>";
        if (CurrentBuyData.SellerUserID == Global_Configs._localUserID)
        {
            BUYTAB.transform.Find("AFTERPRESS/BUTTON").GetComponent<Image>().color = Color.red;
            text = $"<size=100>{Localization.instance.Localize("$mpasn_cancel")}</size>";
        }

        BUYTAB.transform.Find("AFTERPRESS/BUTTON/Text").GetComponent<Text>().text = text;
    }

    private static void SetQuantityValueChange(string value)
    {
        if (CurrentSendData == null) return;
        if (value != "1" && !string.IsNullOrWhiteSpace(value)) AssetStorage.AUsrc.PlayOneShot(AssetStorage.TypeClip, 0.7f);
        if (string.IsNullOrWhiteSpace(value))
        {
            CurrentQuantity = 1;
        }
        else
        {
            if (int.TryParse(value, out int result))
            {
                if (result > CurrentSendData.Count)
                {
                    CurrentQuantity = CurrentSendData.Count;
                    SELLTAB.Find("AFTERPRESS/SetQuantity").GetComponent<InputField>().text =
                        CurrentQuantity.ToString();
                }
                else
                {
                    CurrentQuantity = result;
                }

                if (CurrentQuantity <= 0)
                {
                    CurrentQuantity = 1;
                    SELLTAB.Find("AFTERPRESS/SetQuantity").GetComponent<InputField>().text =
                        CurrentQuantity.ToString();
                }

                if (CurrentQuantity > 999)
                {
                    CurrentQuantity = 999;
                    SELLTAB.Find("AFTERPRESS/SetQuantity").GetComponent<InputField>().text =
                        CurrentQuantity.ToString();
                }
            }
            else
            {
                SELLTAB.Find("AFTERPRESS/SetQuantity").GetComponent<InputField>().text =
                    CurrentQuantity.ToString();
            }
        }

        SELLTAB.Find("AFTERPRESS/BUTTON/Text").GetComponent<Text>().text =
            $"<size=42>{Localization.instance.Localize("$mpasn_sellmarketplace")}</size>\n<color=#FF00FF>{Localization.instance.Localize("$mpasn_quantitymarketplace")}: {CurrentQuantity}</color> x <color=yellow>{CurrentPrice}</color>\n<color=#00FFFF>{Localization.instance.Localize("$mpasn_youget")}:</color> <color=yellow>{CurrentQuantity * CurrentPrice} {GetCurrentCurrencyName_Sell}</color>";
    }

    private static void SearchTabValue(string data)
    {
        if (!string.IsNullOrWhiteSpace(data)) AssetStorage.AUsrc.PlayOneShot(AssetStorage.TypeClip, 0.7f);
        SEARCHVALUE = data;
        OnSort();
    }
    
    public static void OnMarketplaceDataUpdate()
    {
        int page = CurrentPage;
        int? prev = CurrentBuyData?.ID;
        OnSort();
        CurrentPage = page;
        CreateBuyGameObjects(prev);
        LOADING.SetActive(false); 
    }

    private static void ClearMarketplaceData()
    {
        Marketplace_Main_Client.MarketData.Clear();
    }

  

    private static void ResetPage()
    {
        if (!IsPanelVisible()) return;
        if (currentMarketMode == Marketplace_DataTypes.MarketMode.BUY)
        {
            int page = CurrentPage;
            OnSort();
            CurrentPage = page;
            CreateBuyGameObjects();
        }
        else
        {
            int page = CurrentPage;
            InventorySellData.Clear(); 
            InventorySellData.AddRange(CollectInventoryPlayerData());
            OnSort();
            CurrentPage = page;
            CreateSellGameObjects();
        }
 
        PageNumber.text = $"{CurrentPage + 1} / {CurrentMaxPage}";
    } 
    
    private static string GetTempItemTooltip(string prefab, int level, int stack, int variant, long crafterID, string crafterName, string CustomData)
    {  
        return Utils.GetItemForTooltip(prefab, stack, level, variant, crafterID, crafterName, CustomData, 1, item =>
        {
            JC_API_Class.JC_Api_Tooltip(item, currentMarketMode == Marketplace_DataTypes.MarketMode.BUY ? JC_Api[1] : JC_Api[0]);
        });
    }

    private static void PageIncrementor(int value)
    { 
        AssetStorage.AUsrc.Play();
        SELLTAB.Find("AFTERPRESS").gameObject.SetActive(false);
        BUYTAB.Find("AFTERPRESS").gameObject.SetActive(false);
        int test = CurrentPage;
        test += value;
        if (test >= 0 && test < CurrentMaxPage) CurrentPage = test;
        PageNumber.text = $"{CurrentPage + 1} / {CurrentMaxPage}";
        UI.transform.Find("Canvas/BACKGROUND/SEARCH/Input").GetComponent<InputField>().text = SEARCHVALUE;
        ResetPage();
    }

    private static void OnClickBUYItem(int value)
    {
        AssetStorage.AUsrc.Play();
        CurrentPrice = 0;
        CurrentQuantity = 1;
        BUYTAB.transform.Find("AFTERPRESS").gameObject.SetActive(true);
        CurrentBuyData = ServerMarketSendDataSORTED[value];
        ItemDrop.ItemData.SharedData icons = ZNetScene.instance.GetPrefab(CurrentBuyData.ItemPrefab)
            .GetComponent<ItemDrop>()
            .m_itemData.m_shared;
        BUYTAB.transform.Find("AFTERPRESS/icon").GetComponent<Image>().sprite =
            icons.m_icons.Length > CurrentBuyData.Variant
                ? icons.m_icons[CurrentBuyData.Variant]
                : icons.m_icons[0];
        string extention = GetTempItemTooltip(CurrentBuyData.ItemPrefab!, CurrentBuyData.Quality,
            CurrentBuyData.Count, CurrentBuyData.Variant, CurrentBuyData.CrafterID, CurrentBuyData.CrafterName,
            CurrentBuyData.CUSTOMdata);
        string itemName = Localization.instance.Localize(CurrentBuyData.ItemName);
        string text =
            $"{itemName}\n<color=#00ff00>{Localization.instance.Localize("$mpasn_quality")}: {CurrentBuyData.Quality}</color>\n<color=#FF00FF>{Localization.instance.Localize("$mpasn_quantitymarketplace")}: {CurrentBuyData.Count}</color>\n<color=yellow>{Localization.instance.Localize("$mpasn_pricemarketplace")}: {CurrentBuyData.Price}</color>\n<color=#00FFFF>{Localization.instance.Localize("$mpasn_sellernamemarket")}: {CurrentBuyData.SellerName}</color>";
        BUYTAB.transform.Find("AFTERPRESS/Text").GetComponent<Text>().text = text;
        BUYTAB.transform.Find("AFTERPRESS/TooltipView/Viewport/Content/Tooltip").GetComponent<Text>()
            .text = extention;
        SkipQuantityCheck = true;
        BUYTAB.transform.Find("AFTERPRESS/SetQuantity").GetComponent<InputField>().text = "-999";
        SkipQuantityCheck = false;
        SkipNextSound = true;
        BUYTAB.transform.Find("AFTERPRESS/SetQuantity").GetComponent<InputField>().text =
            CurrentBuyData.Count.ToString();
        foreach (GameObject obj in CurrentGameObjects)
            obj.GetComponent<Image>().color = new Color(0.3f, 0.3f, 0.3f, 0.7803922f);

        CurrentGameObjects[value - MAXITEMSPERPAGE * CurrentPage].GetComponent<Image>().color = Color.red;

        Canvas.ForceUpdateCanvases();
        ALLFILTERS.ForEach(filter => filter.enabled = false);
        ALLFILTERS.ForEach(filter => filter.enabled = true);
        ALLSCROLLS.ForEach(scroll => scroll.value = 1); 
    }


    private static void OnClickSELLItem(int value)
    {
        AssetStorage.AUsrc.Play();
        CurrentPrice = 0;
        CurrentQuantity = 1;
        CurrencyDropdown.value = 0;
        SELLTAB.transform.Find("AFTERPRESS").gameObject.SetActive(true);
        CurrentSendData = InventorySellDataSORTED[value];
        ItemDrop.ItemData.SharedData icons = ZNetScene.instance.GetPrefab(CurrentSendData.ItemPrefab)
            .GetComponent<ItemDrop>()
            .m_itemData.m_shared;
        SELLTAB.transform.Find("AFTERPRESS/icon").GetComponent<Image>().sprite =
            icons.m_icons.Length > CurrentSendData.Variant
                ? icons.m_icons[CurrentSendData.Variant]
                : icons.m_icons[0]; 
        string extention = GetTempItemTooltip(CurrentSendData.ItemPrefab!, CurrentSendData.Quality,
            CurrentSendData.Count, CurrentSendData.Variant, CurrentSendData.CrafterID, CurrentSendData.CrafterName,
            CurrentSendData.CUSTOMdata);
        string itemName = Localization.instance.Localize(CurrentSendData.ItemName);
        string text =
            $"{itemName}\n<color=#00ff00>{Localization.instance.Localize("$mpasn_quality")}: {CurrentSendData.Quality}</color>\n<color=#FF00FF>{Localization.instance.Localize("$mpasn_quantitymarketplace")}: {CurrentSendData.Count}</color>";

        SELLTAB.transform.Find("AFTERPRESS/Text").GetComponent<Text>().text = text;
        SELLTAB.transform.Find("AFTERPRESS/TooltipView/Viewport/Content/Tooltip").GetComponent<Text>().text = extention;
        SELLTAB.transform.Find("AFTERPRESS/SetQuantity").GetComponent<InputField>().text = CurrentQuantity.ToString();
        SELLTAB.transform.Find("AFTERPRESS/SetPrice").GetComponent<InputField>().text = CurrentPrice.ToString();
        string text2 = $"<size=42>{Localization.instance.Localize("$mpasn_sellmarketplace")}</size>\n<color=#FF00FF>{Localization.instance.Localize("$mpasn_quantitymarketplace")}: 1</color> x <color=yellow>0</color>\n<color=#00FFFF>{Localization.instance.Localize("$mpasn_youget")}:</color> <color=yellow>0 {GetCurrentCurrencyName_Sell}</color>";
        SELLTAB.transform.Find("AFTERPRESS/BUTTON/Text").GetComponent<Text>().text = text2;

        foreach (GameObject obj in CurrentGameObjects)
            obj.GetComponent<Image>().color = new Color(0.3f, 0.3f, 0.3f, 0.7803922f);

        CurrentGameObjects[value - MAXITEMSPERPAGE * CurrentPage].GetComponent<Image>().color = Color.red;

        Canvas.ForceUpdateCanvases();
        ALLFILTERS.ForEach(filter => filter.enabled = false);
        ALLFILTERS.ForEach(filter => filter.enabled = true);
        ALLSCROLLS.ForEach(scroll => scroll.value = 1);
    }
    

    private static void CreateBuyGameObjects(int? prevData = null)
    {
        CurrentGameObjects.ForEach(Object.Destroy);
        CurrentGameObjects.Clear();
        if (Marketplace_Main_Client.MarketData.Count == 0) return;
        int start = CurrentPage * MAXITEMSPERPAGE;
        Transform parent = BUYTAB.transform.Find("Content");
        for (int i = start; i < Mathf.Min(ServerMarketSendDataSORTED.Count, MAXITEMSPERPAGE + start); ++i)
        {
            ItemDrop item = ZNetScene.instance.GetPrefab(ServerMarketSendDataSORTED[i].ItemPrefab)?.GetComponent<ItemDrop>()!;
            GameObject go = Object.Instantiate(ElementBUY, parent);
            CurrentGameObjects.Add(go);

            if (Utils.IsDebug_Marketplace)
            {
                go.transform.Find("AdminRemove").gameObject.SetActive(true);
                int id = ServerMarketSendDataSORTED[i].ID;
                go.transform.Find("AdminRemove").GetComponent<Button>().onClick.AddListener(() =>
                {
                    AssetStorage.AUsrc.Play();
                    ZRoutedRpc.instance.InvokeRoutedRPC("KGmarket RemoveItemAdmin", id);
                });
            }

            if (item == null)
            {
                foreach (Text child in go.GetComponentsInChildren<Text>())
                {
                    child.text = "";
                }

                go.transform.Find("itemname").GetComponent<Text>().text =
                    $"<color=red>INVALID PREFAB ({ServerMarketSendDataSORTED[i].ItemPrefab})</color>";
                continue;
            }

            int data = i;
            go.GetComponent<Button>().onClick.AddListener(delegate { OnClickBUYItem(data); });
            string itemname = Localization.instance.Localize(ServerMarketSendDataSORTED[i].ItemName);
            go.transform.Find("itemname").GetComponent<Text>().text = itemname;
            go.transform.Find("count").GetComponent<Text>().text =
                ServerMarketSendDataSORTED[i].Count.ToString();
            go.transform.Find("price").GetComponent<Text>().text =
                ServerMarketSendDataSORTED[i].Price.ToString();
            go.transform.Find("seller").GetComponent<Text>().text = ServerMarketSendDataSORTED[i].SellerName;
            go.transform.Find("currency").GetComponent<Image>().sprite = Utils.TryFindIcon(ServerMarketSendDataSORTED[i].Currency);
            go.transform.Find("itemicon").GetComponent<Image>().sprite =
                item.m_itemData.m_shared.m_icons.Length > ServerMarketSendDataSORTED[i].Variant
                    ? item.m_itemData.m_shared.m_icons[ServerMarketSendDataSORTED[i].Variant]
                    : item.m_itemData.m_shared.m_icons[0];

            if (prevData != null && prevData == ServerMarketSendDataSORTED[i].ID)
            {
                go.GetComponent<Image>().color = Color.red;
                CurrentBuyData = ServerMarketSendDataSORTED[i];
            }
        }

        if (prevData != null && CurrentBuyData == null)
        { 
            BUYTAB.transform.Find("AFTERPRESS").gameObject.SetActive(false);
        }
    }


    private static void CreateSellGameObjects()
    {
        CurrentGameObjects.ForEach(Object.Destroy);
        CurrentGameObjects.Clear();
        if (InventorySellDataSORTED.Count == 0) return;

        int start = CurrentPage * MAXITEMSPERPAGE;
        Transform parent = SELLTAB.transform.Find("Content");
        for (int index = 0, i = start;
             i < Mathf.Min(InventorySellDataSORTED.Count, MAXITEMSPERPAGE + start);
             ++i, ++index)
        {
            ItemDrop item = ZNetScene.instance.GetPrefab(InventorySellDataSORTED[i].ItemPrefab)
                .GetComponent<ItemDrop>();
            if (item == null) continue;
            GameObject go = Object.Instantiate(ElementSELL, parent);
            int data = i;
            ///////////init go data
            go.GetComponent<Button>().onClick.AddListener(delegate { OnClickSELLItem(data); });
            go.transform.SetSiblingIndex(index);
            string itemname = Localization.instance.Localize(InventorySellDataSORTED[i].ItemName);
            go.transform.Find("itemname").GetComponent<Text>().text = itemname;
            go.transform.Find("count").GetComponent<Text>().text = InventorySellDataSORTED[i].Count.ToString();
            go.transform.Find("itemicon").GetComponent<Image>().sprite =
                item.m_itemData.m_shared.m_icons.Length > InventorySellDataSORTED[i].Variant
                    ? item.m_itemData.m_shared.m_icons[InventorySellDataSORTED[i].Variant]
                    : item.m_itemData.m_shared.m_icons[0];
            ///////////////////////
            CurrentGameObjects.Add(go);
        }
    }

    private static void SortValueList()
    {
        MySalesText.text =
            $"{Localization.instance.Localize("$mpasn_mysales")} (<color=#00ff00>{Marketplace_Main_Client.MarketData.Count(data => data.SellerUserID == Global_Configs._localUserID)}</color>)";
        CurrentSendData = null;
        CurrentBuyData = null;
        ServerMarketSendDataSORTED.Clear();
        InventorySellDataSORTED.Clear();
        if (currentMarketMode == Marketplace_DataTypes.MarketMode.BUY)
        {
            if (Marketplace_Main_Client.MarketData.Count == 0) return;
            ServerMarketSendDataSORTED = Marketplace_Main_Client.MarketData.OrderByDescending(data => data.TimeStamp).ToList();
            
            if (currentSortType == Marketplace_DataTypes.SortType.UP)
                ServerMarketSendDataSORTED = currentSortMode switch
                {
                    Marketplace_DataTypes.SortBy.None => ServerMarketSendDataSORTED.Where(
                        data => (currentCategory == Marketplace_DataTypes.ItemData_ItemCategory.ALL ||
                                 data.ItemCategory == currentCategory) &&
                                Localization.instance.Localize(data.ItemName).ToLower()
                                    .Contains(SEARCHVALUE.ToLower())).ToList(),
                    Marketplace_DataTypes.SortBy.ItemName => ServerMarketSendDataSORTED.Where(data =>
                            (currentCategory == Marketplace_DataTypes.ItemData_ItemCategory.ALL ||
                             data.ItemCategory == currentCategory) &&
                            Localization.instance.Localize(data.ItemName).ToLower()
                                .Contains(SEARCHVALUE.ToLower()))
                        .OrderBy(data => Localization.instance.Localize(data.ItemName)).ToList(),
                    Marketplace_DataTypes.SortBy.Count => ServerMarketSendDataSORTED.Where(data =>
                            (currentCategory == Marketplace_DataTypes.ItemData_ItemCategory.ALL ||
                             data.ItemCategory == currentCategory) &&
                            Localization.instance.Localize(data.ItemName).ToLower()
                                .Contains(SEARCHVALUE.ToLower())).OrderBy(data => data.Count).ToList(),
                    Marketplace_DataTypes.SortBy.Price => ServerMarketSendDataSORTED.Where(data =>
                            (currentCategory == Marketplace_DataTypes.ItemData_ItemCategory.ALL ||
                             data.ItemCategory == currentCategory) &&
                            Localization.instance.Localize(data.ItemName).ToLower()
                                .Contains(SEARCHVALUE.ToLower())).OrderBy(data => data.Price).ToList(),
                    Marketplace_DataTypes.SortBy.Seller => ServerMarketSendDataSORTED.Where(data =>
                            (currentCategory == Marketplace_DataTypes.ItemData_ItemCategory.ALL ||
                             data.ItemCategory == currentCategory) &&
                            Localization.instance.Localize(data.ItemName).ToLower()
                                .Contains(SEARCHVALUE.ToLower())).OrderBy(data => data.SellerName).ToList(),
                    _ => ServerMarketSendDataSORTED
                };

            if (currentSortType == Marketplace_DataTypes.SortType.DOWN)
                ServerMarketSendDataSORTED = currentSortMode switch
                {
                    Marketplace_DataTypes.SortBy.None => ServerMarketSendDataSORTED.Where(
                        data => (currentCategory == Marketplace_DataTypes.ItemData_ItemCategory.ALL ||
                                 data.ItemCategory == currentCategory) &&
                                Localization.instance.Localize(data.ItemName).ToLower()
                                    .Contains(SEARCHVALUE.ToLower())).ToList(),
                    Marketplace_DataTypes.SortBy.ItemName => ServerMarketSendDataSORTED.Where(data =>
                            (currentCategory == Marketplace_DataTypes.ItemData_ItemCategory.ALL ||
                             data.ItemCategory == currentCategory) &&
                            Localization.instance.Localize(data.ItemName).ToLower()
                                .Contains(SEARCHVALUE.ToLower()))
                        .OrderByDescending(data => Localization.instance.Localize(data.ItemName)).ToList(),
                    Marketplace_DataTypes.SortBy.Count => ServerMarketSendDataSORTED.Where(data =>
                            (currentCategory == Marketplace_DataTypes.ItemData_ItemCategory.ALL ||
                             data.ItemCategory == currentCategory) &&
                            Localization.instance.Localize(data.ItemName).ToLower()
                                .Contains(SEARCHVALUE.ToLower())).OrderByDescending(data => data.Count)
                        .ToList(),
                    Marketplace_DataTypes.SortBy.Price => ServerMarketSendDataSORTED.Where(data =>
                            (currentCategory == Marketplace_DataTypes.ItemData_ItemCategory.ALL ||
                             data.ItemCategory == currentCategory) &&
                            Localization.instance.Localize(data.ItemName).ToLower()
                                .Contains(SEARCHVALUE.ToLower())).OrderByDescending(data => data.Price)
                        .ToList(),
                    Marketplace_DataTypes.SortBy.Seller => ServerMarketSendDataSORTED.Where(data =>
                            (currentCategory == Marketplace_DataTypes.ItemData_ItemCategory.ALL ||
                             data.ItemCategory == currentCategory) &&
                            Localization.instance.Localize(data.ItemName).ToLower()
                                .Contains(SEARCHVALUE.ToLower())).OrderByDescending(data => data.SellerName)
                        .ToList(),
                    _ => ServerMarketSendDataSORTED
                };

            if (MySalesOnly)
            {
                ServerMarketSendDataSORTED = ServerMarketSendDataSORTED
                    .Where(data => data.SellerUserID == Global_Configs._localUserID).ToList();
            }

            CurrentMaxPage = (ServerMarketSendDataSORTED.Count - 1) / MAXITEMSPERPAGE + 1;
            PageNumber.text = $"1 / {CurrentMaxPage}";
            CurrentPage = 0;
            CreateBuyGameObjects();
        }


        if (currentMarketMode == Marketplace_DataTypes.MarketMode.SELL)
        {
            if (InventorySellData.Count == 0) return;
            if (currentSortType == Marketplace_DataTypes.SortType.UP)
                InventorySellDataSORTED = currentSortMode switch
                {
                    Marketplace_DataTypes.SortBy.None => InventorySellData.Where(data =>
                            (currentCategory == Marketplace_DataTypes.ItemData_ItemCategory.ALL ||
                             data.ItemCategory == currentCategory) &&
                            Localization.instance.Localize(data.ItemName).ToLower()
                                .Contains(SEARCHVALUE.ToLower()))
                        .ToList(),
                    Marketplace_DataTypes.SortBy.ItemName => InventorySellData
                        .Where(data =>
                            (currentCategory == Marketplace_DataTypes.ItemData_ItemCategory.ALL ||
                             data.ItemCategory == currentCategory) &&
                            Localization.instance.Localize(data.ItemName).ToLower()
                                .Contains(SEARCHVALUE.ToLower()))
                        .OrderBy(data => Localization.instance.Localize(data.ItemName)).ToList(),
                    Marketplace_DataTypes.SortBy.Count => InventorySellData.Where(data =>
                            (currentCategory == Marketplace_DataTypes.ItemData_ItemCategory.ALL ||
                             data.ItemCategory == currentCategory) &&
                            Localization.instance.Localize(data.ItemName).ToLower()
                                .Contains(SEARCHVALUE.ToLower()))
                        .OrderBy(data => data.Count).ToList(),
                    _ => InventorySellData
                };


            if (currentSortType == Marketplace_DataTypes.SortType.DOWN)
                InventorySellDataSORTED = currentSortMode switch
                {
                    Marketplace_DataTypes.SortBy.None => InventorySellData.Where(data =>
                            (currentCategory == Marketplace_DataTypes.ItemData_ItemCategory.ALL ||
                             data.ItemCategory == currentCategory) &&
                            Localization.instance.Localize(data.ItemName).ToLower()
                                .Contains(SEARCHVALUE.ToLower()))
                        .ToList(),
                    Marketplace_DataTypes.SortBy.ItemName => InventorySellData
                        .Where(data =>
                            (currentCategory == Marketplace_DataTypes.ItemData_ItemCategory.ALL ||
                             data.ItemCategory == currentCategory) &&
                            Localization.instance.Localize(data.ItemName).ToLower()
                                .Contains(SEARCHVALUE.ToLower()))
                        .OrderByDescending(data => Localization.instance.Localize(data.ItemName)).ToList(),
                    Marketplace_DataTypes.SortBy.Count => InventorySellData
                        .Where(data =>
                            (currentCategory == Marketplace_DataTypes.ItemData_ItemCategory.ALL ||
                             data.ItemCategory == currentCategory) &&
                            Localization.instance.Localize(data.ItemName).ToLower()
                                .Contains(SEARCHVALUE.ToLower())).OrderByDescending(data => data.Count)
                        .ToList(),
                    _ => InventorySellData
                };


            
            CurrentMaxPage = (InventorySellDataSORTED.Count - 1) / MAXITEMSPERPAGE + 1;
            PageNumber.text = $"1 / {CurrentMaxPage}";
            CurrentPage = 0;

            CreateSellGameObjects();
        }
    }


    ////////////////////BUY SELL BUTTONS/////////////
    private static void ClickBUYcategoryButton()
    {
        AssetStorage.AUsrc.Play();
        DefaultBUY(); 
        LOADING.SetActive(true);
        Marketplace_Main_Client.RequestData();
    }


    private static void ClickSELLcategoryButton()
    {
        AssetStorage.AUsrc.Play();
        DefaultSELL();
    }
    /////////////////////////////////////////////


    ////////////////////DEFAULT STATE/////////////
    private static void DefaultBUY()
    {
        MySalesOnly = false;
        MySalesText.color = Color.red;
        currentMarketMode = Marketplace_DataTypes.MarketMode.BUY;
        BUYTAB.transform.gameObject.SetActive(true);
        SELLTAB.transform.gameObject.SetActive(false);
        foreach (Button button in CategoryButtons)
            button.transform.Find("Text").GetComponent<Text>().color = DefaultCategoryColorNotActive;

        currentCategory = Marketplace_DataTypes.ItemData_ItemCategory.ALL;
        CategoryButtons[(int)currentCategory].transform.Find("Text").GetComponent<Text>().color =
            DefaultCategoryColorActive;
        BuyButtonCategory.transform.Find("Text").GetComponent<Text>().color = DefaultCategoryColorActive;
        SellButtonCategory.transform.Find("Text").GetComponent<Text>().color = DefaultCategoryColorNotActive; 
        Transform afterpress = BUYTAB.Find("AFTERPRESS");
        afterpress.gameObject.SetActive(false);
        afterpress.transform.Find("icon").GetComponent<Image>().sprite = AssetStorage.NullSprite; 
        afterpress.transform.Find("Text").GetComponent<Text>().text = "";
        afterpress.transform.Find("TooltipView/Viewport/Content/Tooltip").GetComponent<Text>().text = "";
        afterpress.transform.Find("BUTTON/Text").GetComponent<Text>().text = "";
        SEARCHVALUE = "";
        UI.transform.Find("Canvas/BACKGROUND/SEARCH/Input").GetComponent<InputField>().text = SEARCHVALUE;
        currentSortType = Marketplace_DataTypes.SortType.UP;
        currentSortMode = Marketplace_DataTypes.SortBy.None;
        CurrentPage = 0;
        CurrentMaxPage = 1;
        PageNumber.text = $"1 / {CurrentMaxPage}";
        BUYTAB.transform.Find("itemname/Text").GetComponent<Text>().text =
            Localization.instance.Localize("$mpasn_itemnamemarket");
        BUYTAB.transform.Find("count/Text").GetComponent<Text>().text =
            Localization.instance.Localize("$mpasn_quantitymarketplace");
        BUYTAB.transform.Find("price/Text").GetComponent<Text>().text =
            $"{Localization.instance.Localize("$mpasn_pricemarketplace")} <color=grey>x1</color>";
        BUYTAB.transform.Find("seller/Text").GetComponent<Text>().text =
            Localization.instance.Localize("$mpasn_sellernamemarket");
    }

    private static void DefaultSELL()
    {
        MySalesOnly = false;
        MySalesText.color = Color.red;
        InventorySellData.Clear();
        InventorySellData.AddRange(CollectInventoryPlayerData());
        currentMarketMode = Marketplace_DataTypes.MarketMode.SELL;
        BUYTAB.transform.gameObject.SetActive(false);
        SELLTAB.transform.gameObject.SetActive(true);
        foreach (Button button in CategoryButtons)
            button.transform.Find("Text").GetComponent<Text>().color = DefaultCategoryColorNotActive;
        currentCategory = Marketplace_DataTypes.ItemData_ItemCategory.ALL;
        CategoryButtons[(int)currentCategory].transform.Find("Text").GetComponent<Text>().color =
            DefaultCategoryColorActive;
        BuyButtonCategory.transform.Find("Text").GetComponent<Text>().color = DefaultCategoryColorNotActive;
        SellButtonCategory.transform.Find("Text").GetComponent<Text>().color = DefaultCategoryColorActive;
        Transform afterpress = SELLTAB.Find("AFTERPRESS");
        afterpress.gameObject.SetActive(false);
        afterpress.transform.Find("icon").GetComponent<Image>().sprite = AssetStorage.NullSprite;
        afterpress.transform.Find("Text").GetComponent<Text>().text = "";
        afterpress.transform.Find("TooltipView/Viewport/Content/Tooltip").GetComponent<Text>().text = "";
        afterpress.transform.Find("BUTTON/Text").GetComponent<Text>().text = "";
        afterpress.transform.Find("SetQuantity").GetComponent<InputField>().text = "";
        afterpress.transform.Find("SetPrice").GetComponent<InputField>().text = "";
        SEARCHVALUE = "";
        UI.transform.Find("Canvas/BACKGROUND/SEARCH/Input").GetComponent<InputField>().text = SEARCHVALUE;
        currentSortType = Marketplace_DataTypes.SortType.UP;
        currentSortMode = Marketplace_DataTypes.SortBy.None;
        CurrentPage = 0;
        CurrentSendData = null; 
        CurrentPrice = 0;
        CurrentQuantity = 1;
        CurrentMaxPage = 1;
        PageNumber.text = $"1 / {CurrentMaxPage}";
        SELLTAB.transform.Find("itemname/Text").GetComponent<Text>().text =
            Localization.instance.Localize("$mpasn_itemnamemarket");
        SELLTAB.transform.Find("count/Text").GetComponent<Text>().text =
            Localization.instance.Localize("$mpasn_quantitymarketplace");
        OnSort();
    }
    /////////////////////////////////////////////


    ////////////////////CATEGORY SET/////////////
    private static void SetCategory(int whichOne)
    {
        AssetStorage.AUsrc.Play();
        currentCategory = (Marketplace_DataTypes.ItemData_ItemCategory)whichOne;
        foreach (Button button in CategoryButtons)
            button.transform.Find("Text").GetComponent<Text>().color = DefaultCategoryColorNotActive;

        CategoryButtons[whichOne].transform.Find("Text").GetComponent<Text>().color = DefaultCategoryColorActive;
        OnSort();
        SELLTAB.Find("AFTERPRESS").gameObject.SetActive(false);
        BUYTAB.Find("AFTERPRESS").gameObject.SetActive(false);
    }
    /////////////////////////////////////////////


    public static void Hide()
    {
        UI.SetActive(false);
        CurrentGameObjects.ForEach(Object.Destroy);
        CurrentGameObjects.Clear();
        ClearMarketplaceData();
    }
 
    public static void Show() 
    {
        ClearMarketplaceData();
        UI.SetActive(true); 
        DefaultBUY();
        LOADING.SetActive(true); 
        Marketplace_Main_Client.RequestData();
        if (!Global_Configs.SyncedGlobalOptions.Value._blockedPlayerList.Contains(Global_Configs._localUserID))
        {
            if (Global_Configs.SyncedGlobalOptions.Value._vipPlayerList.Contains(Global_Configs._localUserID))
            {
                UI.transform.Find("Canvas/BACKGROUND/MainButtonsTab/Steam/Text").GetComponent<Text>().text =
                    Global_Configs._localUserID + "\n\t    <size=35>(VIP)</size>";
                UI.transform.Find("Canvas/BACKGROUND/MainButtonsTab/Steam/Text").GetComponent<Text>().color =
                    new Color(0, 1, 0);
            } 
            else 
            {
                UI.transform.Find("Canvas/BACKGROUND/MainButtonsTab/Steam/Text").GetComponent<Text>().text =
                    Global_Configs._localUserID +
                    $"\n\t    <size=35>({Localization.instance.Localize("$mpasn_allowed")})</size>";
                UI.transform.Find("Canvas/BACKGROUND/MainButtonsTab/Steam/Text").GetComponent<Text>().color =
                    new Color(0.8352942f, 0.8196079f, 0.7254902f);
            }
        }
        else
        {
            UI.transform.Find("Canvas/BACKGROUND/MainButtonsTab/MarketLimit/Text").GetComponent<Text>().text =
                Global_Configs.SyncedGlobalOptions.Value._vipPlayerList.Contains(Global_Configs._localUserID)
                    ? $"{Localization.instance.Localize("$mpasn_slotlimit")}\n{Global_Configs.SyncedGlobalOptions.Value._itemMarketLimit}\n<color=#00ff00>{Localization.instance.Localize("$mpasn_taxes")}:\n{Global_Configs.SyncedGlobalOptions.Value._vipmarketTaxes}%</color>"
                    : $"{Localization.instance.Localize("$mpasn_slotlimit")}\n{Global_Configs.SyncedGlobalOptions.Value._itemMarketLimit}\n{Localization.instance.Localize("$mpasn_taxes")}:\n{Global_Configs.SyncedGlobalOptions.Value._marketTaxes}%";
            UI.transform.Find("Canvas/BACKGROUND/MainButtonsTab/Steam/Text").GetComponent<Text>().color =
                Color.red;
        }

        UI.transform.Find("Canvas/BACKGROUND/MainButtonsTab/MarketLimit/Text").GetComponent<Text>().text =
            $"$mpasn_slotlimit\n{Global_Configs.SyncedGlobalOptions.Value._itemMarketLimit}\n$mpasn_taxes\n{(Global_Configs.SyncedGlobalOptions.Value._vipPlayerList.Contains(Global_Configs._localUserID) ? $"<color=#00ff00>{Global_Configs.SyncedGlobalOptions.Value._vipmarketTaxes}%</color>" : $"{Global_Configs.SyncedGlobalOptions.Value._marketTaxes}%")}".Localize();


        UpdateAnnonIcon();
    }
}