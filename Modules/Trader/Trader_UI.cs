using Marketplace.ExternalLoads;
using Marketplace.Modules.Banker;
using Marketplace.Modules.Gambler;
using Marketplace.Modules.Global_Options;
using Marketplace.UI_OptimizationHandler;
using Object = UnityEngine.Object;

namespace Marketplace.Modules.Trader;

public static class Trader_UI
{
    private static GameObject UI;
    private static GameObject BuyElement;
    private static Transform ContentTransform;
    private static readonly Color Enabled = new(0.5361788f, 1f, 0.1176471f, 0.6862745f);
    private static readonly Color Disabled = new(1f, 0.1766787f, 0.1176471f, 0.6862745f);
    private static readonly List<GameObject> CurrentObjects = new();
    private static string CurrentProfile = "";
    private static readonly List<ContentSizeFitter> AllFilters = new();
    private static Scrollbar ScrollbarMain;
    private static Text NPCname;
    private static List<Trader_DataTypes.TraderData> SortedList;
    private static InputField SearchInput;
    private static Modifier CurrentModifier;
    private static readonly Transform[] ModifierButtons = new Transform[Enum.GetValues(typeof(Modifier)).Length];
    private static bool ToBank;
    private static bool FromBank;
    private static Text FromBankText;


    public static bool IsPanelVisible()
    {
        return UI && UI.activeSelf;
    }

    private enum Modifier
    {
        x1,
        x5,
        x10,
        x100
    }

    private static readonly Dictionary<Modifier, int> ModifierValues = new()
    {
        { Modifier.x1, 1 },
        { Modifier.x5, 5 },
        { Modifier.x10, 10 },
        { Modifier.x100, 100 }
    };
 

    public static void Init()
    {
        UI = Object.Instantiate(AssetStorage.asset.LoadAsset<GameObject>("MarketplaceTraderNewUI"));
        BuyElement = AssetStorage.asset.LoadAsset<GameObject>("TraderInstantiateV3");
        ContentTransform = UI.transform.Find("Canvas/Scroll View/Viewport/Content");
        Object.DontDestroyOnLoad(UI);
        UI.SetActive(false);
        AllFilters.AddRange(UI.GetComponentsInChildren<ContentSizeFitter>().ToList());
        ScrollbarMain = UI.GetComponentInChildren<Scrollbar>();
        NPCname = UI.transform.Find("Canvas/Header/Text").GetComponent<Text>();
        SearchInput = UI.GetComponentInChildren<InputField>();
        SearchInput.onValueChanged.AddListener(OnSearchInput);
        FromBankText = UI.transform.Find("Canvas/Background/ToFromText").GetComponent<Text>();

        ModifierButtons[0] = UI.transform.Find("Canvas/Background/x1");
        ModifierButtons[1] = UI.transform.Find("Canvas/Background/x5");
        ModifierButtons[2] = UI.transform.Find("Canvas/Background/x10");
        ModifierButtons[3] = UI.transform.Find("Canvas/Background/x100");

        for (int i = 0; i < ModifierButtons.Length; ++i)
        {
            int i1 = i;
            ModifierButtons[i].GetComponent<Button>().onClick.AddListener(() =>
            {
                AssetStorage.AUsrc.Play();
                SetModifier((Modifier)i1);
                CreateElementsNew();
            });
        }

        UI.transform.Find("Canvas/Background/ToBank").GetComponent<Button>().onClick.AddListener(() =>
        {
            AssetStorage.AUsrc.Play();
            ToBank = !ToBank;
            UI.transform.Find("Canvas/Background/ToBank/Image/img").GetComponent<Image>().color =
                ToBank ? Enabled : Disabled;
            ResetBankText();
            Reload();
        });
        UI.transform.Find("Canvas/Background/FromBank").GetComponent<Button>().onClick.AddListener(() =>
        {
            AssetStorage.AUsrc.Play();
            FromBank = !FromBank;
            UI.transform.Find("Canvas/Background/FromBank/Image/img").GetComponent<Image>().color =
                FromBank ? Enabled : Disabled;
            ResetBankText();
            Reload();
        });

        Localization.instance.Localize(UI.transform);
    }

    [HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.Awake))]
    [ClientOnlyPatch]
    private static class InventoryGui_Awake_Patch
    {
        [UsedImplicitly]
        private static void Postfix(InventoryGui __instance)
        {
            foreach (UITooltip uiTooltip in BuyElement.GetComponentsInChildren<UITooltip>(true))
            {
                uiTooltip.m_tooltipPrefab = __instance.m_playerGrid.m_elementPrefab.GetComponent<UITooltip>().m_tooltipPrefab;
            }
        }
    }

    [HarmonyPatch(typeof(TextInput), nameof(TextInput.IsVisible))]
    [ClientOnlyPatch]
    private static class TraderUIFix
    {
        [UsedImplicitly]
        private static void Postfix(ref bool __result)
        {
            if (IsPanelVisible()) __result = true;
        }
    }

    private static void SetModifier(Modifier modifier)
    {
        CurrentModifier = modifier;
        for (int i = 0; i < ModifierButtons.Length; ++i)
        {
            ModifierButtons[i].transform.Find("Image").GetComponent<Image>().color =
                i == (int)modifier ? Color.green : Color.white;
            ModifierButtons[i].transform.Find("Text").GetComponent<Text>().color =
                i == (int)modifier ? Color.green : Color.white;
        }
    }

    private static void OnSearchInput(string arg0)
    {
        AssetStorage.AUsrc.PlayOneShot(AssetStorage.TypeClip, 0.7f);
        SortList();
        CreateElementsNew();
    }

    private static void SortList()
    {
        if (!Trader_DataTypes.ClientSideItemList.ContainsKey(CurrentProfile)) return;
        string search = SearchInput.text.ToLower();
        if (string.IsNullOrWhiteSpace(search))
        {
            SortedList = Trader_DataTypes.ClientSideItemList[CurrentProfile];
            return;
        }

        SortedList = Trader_DataTypes.ClientSideItemList[CurrentProfile].Where(data =>
            data.NeededItems.Any(i => i.ItemName.ToLower().Contains(search)) ||
            data.ResultItems.Any(i => i.ItemName.ToLower().Contains(search))).ToList();
    }

    private static float CalculateOffset(int amount) => Mathf.Max(0, (amount - 1) * 34.5f);

    private static void FillUITooltip(Transform transform, Trader_DataTypes.TraderItem data)
    {
        transform.GetComponent<UITooltip>().m_topic = data.ItemName;
        if (data.Type is Trader_DataTypes.TraderItem.TraderItemType.Monster)
            transform.GetComponent<UITooltip>().m_text = Localization.instance.Localize("$mpasn_tooltip_pet");
        else if (data.Type is Trader_DataTypes.TraderItem.TraderItemType.Skill)
            transform.GetComponent<UITooltip>().m_text = Localization.instance.Localize("$mpasn_Skill_EXP");
        else
            transform.GetComponent<UITooltip>().m_text = ItemDrop.ItemData.GetTooltip(
                ZNetScene.instance.GetPrefab(data.ItemPrefab).GetComponent<ItemDrop>()
                    .m_itemData, data.Level, false, Game.m_worldLevel);
    }

    private static void CreateElementsNew()
    {
        CurrentObjects.ForEach(Object.Destroy);
        CurrentObjects.Clear();
        float y = 0;
        foreach (Trader_DataTypes.TraderData data in SortedList)
        {
            if (data.NeedToKnow)
            {
                bool breakLoop =
                    data.NeededItems.Any(item => item.Type == Trader_DataTypes.TraderItem.TraderItemType.Item &&
                                                 !Player.m_localPlayer.m_knownMaterial.Contains(item.OriginalItemName))
                    ||
                    data.ResultItems.Any(item => item.Type == Trader_DataTypes.TraderItem.TraderItemType.Item &&
                                                 !Player.m_localPlayer.m_knownMaterial.Contains(item.OriginalItemName));

                if (breakLoop) continue;
            }

            if (FromBank && data.NeededItems.Any(x => !CanAddToBanker(x))) continue;
            if (ToBank && data.ResultItems.Any(x => !CanAddToBanker(x))) continue;
            
            GameObject go = Object.Instantiate(BuyElement, ContentTransform);

            int biggest = data.NeededItems.Count > data.ResultItems.Count
                ? data.NeededItems.Count
                : data.ResultItems.Count;
            int internalOffset = 0;
            for (int i = 0; i < data.NeededItems.Count; ++i)
            {
                Transform transform = i == 0
                    ? go.transform.Find("Background/NeededItems/NeededItem")
                    : Object.Instantiate(go.transform.Find("Background/NeededItems/NeededItem"),
                        go.transform.Find("Background/NeededItems"));
                transform.GetComponent<RectTransform>().anchoredPosition -= new Vector2(0, internalOffset);
                internalOffset += 69;
                int starCount = data.NeededItems[i].Level;
                string stars = data.NeededItems[i].DisplayStars ? $" <color=#00ff00>({starCount}★)</color>" : "";
                transform.transform.Find("Text").GetComponent<Text>().text =
                    $"{(FromBank ? "<color=cyan>[BANK]</color> " : "")}" +
                    $"{data.NeededItems[i].ItemName}{stars}\n<color=yellow>x{data.NeededItems[i].Count * ModifierValues[CurrentModifier]}</color>";
                transform.transform.Find("Icon").GetComponent<Image>().sprite = data.NeededItems[i].GetIcon();

                FillUITooltip(transform, data.NeededItems[i]);
                transform.GetComponent<Outline>().effectColor =
                    HasEnough(data.NeededItems[i]) ? Color.green : Color.red;
            }

            go.transform.Find("Background/NeededItems").GetComponent<RectTransform>().anchoredPosition +=
                new Vector2(0, CalculateOffset(data.NeededItems.Count));

            internalOffset = 0;
            for (int i = 0; i < data.ResultItems.Count; ++i)
            {
                Transform transform = i == 0
                    ? go.transform.Find("Background/ResultItems/ResultItem")
                    : Object.Instantiate(go.transform.Find("Background/ResultItems/ResultItem"),
                        go.transform.Find("Background/ResultItems"));
                transform.GetComponent<RectTransform>().anchoredPosition -= new Vector2(0, internalOffset);
                internalOffset += 69;
                int starCount = data.ResultItems[i].Type is Trader_DataTypes.TraderItem.TraderItemType.Monster
                    ? data.ResultItems[i].Level - 1
                    : data.ResultItems[i].Level;
                string stars = data.ResultItems[i].DisplayStars ? $" <color=#00ff00>({starCount}★)</color>" : "";
                transform.transform.Find("Text").GetComponent<Text>().text =
                    $"{(ToBank ? "<color=cyan>[BANK]</color> " : "")}" +
                    $"{data.ResultItems[i].ItemName}{stars}\n<color=yellow>x{data.ResultItems[i].Count * ModifierValues[CurrentModifier]}</color>";

                transform.transform.Find("Icon").GetComponent<Image>().sprite =
                    data.ResultItems[i].Type is Trader_DataTypes.TraderItem.TraderItemType.Skill
                        ? Utils.GetSkillIcon(data.ResultItems[i].ItemPrefab)
                        : data.ResultItems[i].GetIcon();
                FillUITooltip(transform, data.ResultItems[i]);
            }

            go.transform.Find("Background/ResultItems").GetComponent<RectTransform>().anchoredPosition +=
                new Vector2(0, CalculateOffset(data.ResultItems.Count));
            go.transform.Find("Background/Conversion").GetComponent<Button>().onClick
                .AddListener(delegate { TradeItemByGo(data); });
            go.GetComponent<RectTransform>().anchoredPosition -= new Vector2(0, y);
            CurrentObjects.Add(go);
            go.transform.Find("Background/Conversion/Conversion").GetComponent<Image>().color =
                CanBuy(data) ? Color.green : Color.red;

            go.GetComponent<RectTransform>().sizeDelta += new Vector2(0, 72.5f * (biggest - 1));
            y += 72.5f * (biggest - 1) + 79f;
        }

        ContentTransform.GetComponent<RectTransform>()
            .SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, y + 20f);
        Canvas.ForceUpdateCanvases();
        AllFilters.ForEach(filter => filter.enabled = false);
        AllFilters.ForEach(filter => filter.enabled = true);
    }

    private static bool CanBuy(Trader_DataTypes.TraderData data)
    {
        foreach (Trader_DataTypes.TraderItem r in data.NeededItems)
            if (!HasEnough(r))
                return false;
        return true;
    }

    private static bool HasEnough(Trader_DataTypes.TraderItem r)
    {
        if (r.Type is Trader_DataTypes.TraderItem.TraderItemType.Item)
        {
            if (!FromBank)
            {
                return Utils.CustomCountItems(r.ItemPrefab, r.Level) >= r.Count * ModifierValues[CurrentModifier];
            }
            else
            {
                int hash = r.ItemPrefab.GetStableHashCode();
                int amount = Banker_DataTypes.BankerClientData.TryGetValue(hash, out int value) ? value : 0;
                return amount >= r.Count * ModifierValues[CurrentModifier];
            }
        }

        return false;
    }

    private static bool CanAddToBanker(Trader_DataTypes.TraderItem item)
    {
        if (item.Type is not Trader_DataTypes.TraderItem.TraderItemType.Item) return false;
        if (item.Level > 1) return false;
        int hash = item.ItemPrefab.GetStableHashCode();
        return Banker_DataTypes.SyncedBankerProfiles.Value.Values.Any(x => x.Contains(hash));
    }

    private static int LastPaidBankHashcode = 0;

    private static void TradeItemByGo(Trader_DataTypes.TraderData data)
    {
        Player p = Player.m_localPlayer;
        string logMessage =
            $"{Player.m_localPlayer.GetPlayerName()} ({Global_Configs._localUserID}) TraderNPC: ";
        AssetStorage.AUsrc.Play();
        if (!CanBuy(data)) return;

        if (FromBank)
        {
            int bankHash = Banker_DataTypes.BankerClientData.GetHashCode();
            if (LastPaidBankHashcode == bankHash) return;
            LastPaidBankHashcode = bankHash;
        }

        foreach (Trader_DataTypes.TraderItem neededItem in data.NeededItems)
        {
            if (neededItem.Type is Trader_DataTypes.TraderItem.TraderItemType.Item)
            {
                int amount = neededItem.Count * ModifierValues[CurrentModifier];
                if (!FromBank)
                    Utils.CustomRemoveItems(neededItem.ItemPrefab, amount, neededItem.Level);
                else
                {
                    int hash = neededItem.ItemPrefab.GetStableHashCode();
                    if (Banker_DataTypes.BankerClientData.ContainsKey(hash))
                        Banker_DataTypes.BankerClientData[neededItem.ItemPrefab.GetStableHashCode()] -= amount;
                    ZRoutedRpc.instance.InvokeRoutedRPC("KGmarket BankerRemove", neededItem.ItemPrefab, amount);
                }

                logMessage += $"x{amount} {neededItem.ItemName}, ";
            }
        }

        logMessage += "for => ";
        foreach (Trader_DataTypes.TraderItem resultItem in data.ResultItems)
        {
            GameObject prefab = ZNetScene.instance.GetPrefab(resultItem.ItemPrefab);
            switch (resultItem.Type)
            {
                case Trader_DataTypes.TraderItem.TraderItemType.Skill:
                    Utils.IncreaseSkillEXP(resultItem.ItemPrefab, resultItem.Count * ModifierValues[CurrentModifier]);
                    logMessage += $"x{resultItem.Count * ModifierValues[CurrentModifier]} {resultItem.ItemName}, ";
                    break;
                case Trader_DataTypes.TraderItem.TraderItemType.Item:
                    if (ToBank && CanAddToBanker(resultItem))
                    {
                        ZRoutedRpc.instance.InvokeRoutedRPC("KGmarket BankerDeposit", resultItem.ItemPrefab, resultItem.Count * ModifierValues[CurrentModifier]);
                        MessageHud.instance.ShowMessage(MessageHud.MessageType.Center, Localization.instance.Localize(
                            "$mpasn_addedtobank",
                            (resultItem.Count * ModifierValues[CurrentModifier]).ToString(),
                            Localization.instance.Localize(resultItem.ItemName)));
                    }
                    else
                    {
                        MessageHud.instance.ShowMessage(MessageHud.MessageType.Center, Localization.instance.Localize(
                            "$mpasn_added",
                            (resultItem.Count * ModifierValues[CurrentModifier]).ToString(),
                            Localization.instance.Localize(resultItem.ItemName)));
                        Utils.AddItemToPlayer(prefab, resultItem.Count * ModifierValues[CurrentModifier],
                            resultItem.Level);
                    }

                    logMessage += $"x{resultItem.Count * ModifierValues[CurrentModifier]} {resultItem.ItemName}, ";
                    break;
                case Trader_DataTypes.TraderItem.TraderItemType.Monster:
                    MessageHud.instance.ShowMessage(MessageHud.MessageType.Center, Localization.instance.Localize(
                        "$mpasn_spawned",
                        (resultItem.Count * ModifierValues[CurrentModifier]).ToString(),
                        Localization.instance.Localize(resultItem.ItemName)));
                    Utils.AddItemToPlayer(prefab, resultItem.Count * ModifierValues[CurrentModifier], resultItem.Level);
                    logMessage += $"x{resultItem.Count * ModifierValues[CurrentModifier]} {resultItem.ItemName}, ";
                    break;
            }
        }

        logMessage = logMessage.Substring(0, logMessage.Length - 2);
        ZRoutedRpc.instance.InvokeRoutedRPC("LogOnServer_mpasn", 3, logMessage);
        CreateElementsNew();
    }

    public static void Hide()
    {
        ScrollbarMain.value = 1;
        UI.SetActive(false);
        CurrentObjects.ForEach(Object.Destroy);
        CurrentObjects.Clear();
    }

    public static void Reload()
    {
        if (IsPanelVisible())
        {
            SortList();
            CreateElementsNew();
        }
    }

    private static void ResetBankText() 
    {
        FromBankText.text =
            $"Pay from Bank: {(FromBank ? "<color=green>ON</color>" : "<color=red>OFF</color>")}\nSend to Bank: {(ToBank ? "<color=green>ON</color>" : "<color=red>OFF</color>")}";
    }

    public static void Show(string profile, string _npcName)
    {
        if (!Trader_DataTypes.ClientSideItemList.ContainsKey(profile)) return;
        CurrentProfile = profile;
        ToBank = false;
        FromBank = false;
        ResetBankText();
        SearchInput.text = "";
        SortList();
        SetModifier(Modifier.x1);
        CreateElementsNew();
        UI.transform.Find("Canvas/Background/ToBank/Image/img").GetComponent<Image>().color = Disabled;
        UI.transform.Find("Canvas/Background/FromBank/Image/img").GetComponent<Image>().color = Disabled;
        UI.transform.Find("Canvas/Background/ToBank").gameObject.SetActive(true);
        UI.transform.Find("Canvas/Background/FromBank").gameObject.SetActive(true);
        FromBankText.gameObject.SetActive(true);
        InventoryGui.instance.Show(null);
        UI.SetActive(true);
        _npcName = Utils.RichTextFormatting(_npcName);
        NPCname.text = string.IsNullOrEmpty(_npcName) ? Localization.instance.Localize("$mpasn_Trader") : _npcName;
        ScrollView_Optimization.StartOptimization(UI, IsPanelVisible, CurrentObjects,
            UI_Optimizations.OptimizationType.Vertical);
    }
}