using ItemDataManager;
using Marketplace.ExternalLoads;
using Marketplace.Modules.Gambler;

namespace Marketplace.Modules.Transmogrification;

public static class Transmogrification_UI
{
    private static GameObject UI;
    private static GameObject Inventory_Element;
    private static Transform Inventory_Content;
    private static GameObject Category_Element;
    private static Transform Category_Content;
    private static GameObject Transmog_Element;
    private static ItemDrop.ItemData CurrentChoosenItem;
    private static Transform ChoosenItem_Transform;
    private static GameObject ClickEffect;
    private static GameObject ClickEffect2;
    private static GameObject ClickEffectReverse;
    private static string CurrentProfile;

    public static Shader SPEC_SHADER;
    
    private static readonly List<ContentSizeFitter> AllFillers = new();
    private static Scrollbar MainBar;
    private static Scrollbar SubBar;


    private static readonly Dictionary<ItemDrop.ItemData.ItemType, Color> TypeColors = new()
    {
        { (ItemDrop.ItemData.ItemType)999, Color.cyan },
        { ItemDrop.ItemData.ItemType.OneHandedWeapon, Color.yellow },
        { ItemDrop.ItemData.ItemType.TwoHandedWeapon, new Color(1f, 0.56f, 0.03f) },
        { ItemDrop.ItemData.ItemType.TwoHandedWeaponLeft, new Color(1f, 0.46f, 0.23f) },
        { ItemDrop.ItemData.ItemType.Bow, Color.green },
        { ItemDrop.ItemData.ItemType.Tool, new Color(1f, 0.33f, 0.42f) },
        { ItemDrop.ItemData.ItemType.Shield, Color.magenta },
        { ItemDrop.ItemData.ItemType.Chest, Color.blue },
        { ItemDrop.ItemData.ItemType.Helmet, new Color(0.53f, 0.66f, 1f) },
        { ItemDrop.ItemData.ItemType.Legs, new Color(0.36f, 0.05f, 1f) },
        { ItemDrop.ItemData.ItemType.Shoulder, new Color(0.74f, 0.53f, 1f) },
        { ItemDrop.ItemData.ItemType.Utility, Color.white }
    };

    private static readonly HashSet<ItemDrop.ItemData.ItemType> AvaliableTypes = new()
    {
        ItemDrop.ItemData.ItemType.OneHandedWeapon,
        ItemDrop.ItemData.ItemType.TwoHandedWeapon,
        ItemDrop.ItemData.ItemType.TwoHandedWeaponLeft,
        ItemDrop.ItemData.ItemType.Bow,
        ItemDrop.ItemData.ItemType.Tool,
        ItemDrop.ItemData.ItemType.Shield,
        ItemDrop.ItemData.ItemType.Chest, 
        ItemDrop.ItemData.ItemType.Helmet,
        ItemDrop.ItemData.ItemType.Legs, 
        ItemDrop.ItemData.ItemType.Shoulder,
        ItemDrop.ItemData.ItemType.Utility
    };

    public static bool IsVisble() => UI && UI.activeSelf;
 
    public static void Init()
    { 
        UI = UnityEngine.Object.Instantiate(
            AssetStorage.asset.LoadAsset<GameObject>("MarketplaceTransmogUI"));
        UnityEngine.Object.DontDestroyOnLoad(UI);
        UI.SetActive(false);
        SPEC_SHADER = UI.transform.Find("Canvas/ShaderHolder").GetComponent<Image>().material.shader;
        MainBar = UI.transform.Find("Canvas/SelectItemTab/ItemList/ListPanel/Scroll View/Scrollbar")
            .GetComponent<Scrollbar>();
        SubBar = UI.transform.Find("Canvas/SelectTransmogTab/ItemList/ListPanel/Scroll View/Scrollbar")
            .GetComponent<Scrollbar>();
        AllFillers.AddRange(UI.GetComponentsInChildren<ContentSizeFitter>(true).ToList());
        Inventory_Element = AssetStorage.asset.LoadAsset<GameObject>("TransmogInventoryItem");
        Inventory_Content =
            UI.transform.Find("Canvas/SelectItemTab/ItemList/ListPanel/Scroll View/Viewport/Content");
        Category_Element = AssetStorage.asset.LoadAsset<GameObject>("TransmogCategory");
        Category_Content =
            UI.transform.Find("Canvas/SelectTransmogTab/ItemList/ListPanel/Scroll View/Viewport/Content");
        Transmog_Element = AssetStorage.asset.LoadAsset<GameObject>("TransmogItem");
        ChoosenItem_Transform = UI.transform.Find("Canvas/SelectItemTab/ItemList/ChoosenItem");
        ClickEffect = AssetStorage.asset.LoadAsset<GameObject>("TransmogClick");
        ClickEffect2 = AssetStorage.asset.LoadAsset<GameObject>("TransmogClick2");
        ClickEffectReverse = AssetStorage.asset.LoadAsset<GameObject>("TransmogReverse");
        UI.transform.Find("Canvas/SelectItemTab/ItemList/DestroyTransmog").GetComponent<Button>().onClick
            .AddListener(TransformReverse);

        Localization.instance.Localize(UI.transform);
    } 

    private static void UpdateFillers()
    {
        Canvas.ForceUpdateCanvases();  
        AllFillers.ForEach(filter => filter.enabled = false);
        AllFillers.ForEach(filter => filter.enabled = true);
    }

    private static void LoadInventory()
    {
        ClearStuff();
        MainBar.value = 1;
        SubBar.value = 1;
        IOrderedEnumerable<ItemDrop.ItemData> inventory = Player.m_localPlayer.m_inventory.GetAllItems()
            .Where(item => AvaliableTypes.Contains(item.m_shared.m_itemType))
            .OrderBy(item => !item.HasTransmog())
            .ThenBy(item => Localization.instance.Localize(item.m_shared.m_name));
        int count = 0;
        foreach (ItemDrop.ItemData item in inventory)
        {
            GameObject element = UnityEngine.Object.Instantiate(Inventory_Element, Inventory_Content);
            element.transform.Find("Text").GetComponent<Text>().text =
                Localization.instance.Localize(item.m_shared.m_name);
            element.transform.Find("Text").GetComponent<Text>().color =
                item.HasTransmog() ? new Color(1f, 0.31f, 0.31f) : Color.white;
            element.transform.Find("Icon").GetComponent<Image>().sprite = item.GetIcon();
            int index = count;
            element.GetComponent<Button>().onClick.AddListener(() => ChooseItem(item, index));
            ++count;
        }

        LoadCategories(null);
        UpdateFillers();
    }


    private static void LoadCategories(ItemDrop.ItemData item)
    {
        foreach (Transform child in Category_Content)
            UnityEngine.Object.Destroy(child.gameObject);
        if (item == null)
        {
            LoadOneCategory((ItemDrop.ItemData.ItemType)999, false);
               foreach (ItemDrop.ItemData.ItemType c in AvaliableTypes)
                    LoadOneCategory(c, false);
            return;
        }
        ItemDrop.ItemData.ItemType category = item.m_shared.m_itemType;
        LoadOneCategory((ItemDrop.ItemData.ItemType)999, true);
        LoadOneCategory(category, true);
        UpdateFillers();
    }
    
    private static void LoadOneCategory(ItemDrop.ItemData.ItemType category, bool active)
    {
        if (Transmogrification_Main_Client.FilteredTransmogData[CurrentProfile]
                .TryGetValue(category, out List<Transmogrification_DataTypes.TransmogItem_Data> cat) &&
            cat.Count > 0)
        {
            GameObject element = UnityEngine.Object.Instantiate(Category_Element, Category_Content);
            element.GetComponent<Image>().color = TypeColors[category];
            element.transform.Find("Text").GetComponent<Text>().text =
                Localization.instance.Localize("$mpasn_transmog_" + category.ToString().ToLower());
            foreach (Transmogrification_DataTypes.TransmogItem_Data data in cat)
            {
                GameObject prefab = ZNetScene.instance.GetPrefab(data.Prefab);
                if (!prefab) continue;
                
                GameObject transmogElement = UnityEngine.Object.Instantiate(Transmog_Element, element.transform);
                transmogElement.transform.Find("ItemName").GetComponent<Text>().text = data.GetLocalizedName();
                transmogElement.transform.Find("Icon/IconItem").GetComponent<Image>().sprite = data.GetIcon();
                transmogElement.transform.Find("Price").GetComponent<Text>().text = data.GetLocalizedPrice();
                transmogElement.transform.Find("Price/X").GetComponent<Text>().text = $"X {data.Price_Amount}";
                transmogElement.transform.Find("Price/PriceIcon/IconItem").GetComponent<Image>().sprite =
                    data.GetPriceIcon();
                

                if (!active)
                {
                    transmogElement.transform.Find("Add").gameObject.SetActive(false);
                    transmogElement.transform.Find("HEX").gameObject.SetActive(false);
                    transmogElement.transform.Find("ColorText").gameObject.SetActive(false);
                    transmogElement.transform.Find("Preview").gameObject.SetActive(false);
                }
                else
                {
                    string name = ZNetScene.instance.GetPrefab(data.Price_Prefab).GetComponent<ItemDrop>().m_itemData
                        .m_shared.m_name;
                    int amountInInventory = Player.m_localPlayer.m_inventory.CountItems(name);
                    bool enough = amountInInventory >= data.Price_Amount;
                    transmogElement.transform.Find("Price/X").GetComponent<Text>().color =
                        enough ? Color.yellow : new Color(1f, 0.31f, 0.31f);
                    transmogElement.GetComponent<Image>().color = enough ? Color.white : new Color(0.78f, 0.49f, 0.51f);
                    transmogElement.transform.Find("Add").gameObject.SetActive(enough);
                    transmogElement.transform.Find("Add").GetComponent<Button>().onClick
                        .AddListener(() => ClickTransmog(data, transmogElement));
                    transmogElement.transform.Find("HEX").GetComponent<TMP_InputField>().onValueChanged.AddListener(
                        (str) =>
                        {
                            Image bg = transmogElement.transform.Find("HEX").GetComponent<Image>();
                            if (ColorUtility.TryParseHtmlString("#" + str, out Color c))
                            {
                                bg.color = c;
                            }
                            else
                            {
                                bg.color = Color.white;
                            }
                        });
                    
                    transmogElement.transform.Find("Preview").GetComponent<Button>().onClick.AddListener(() =>
                        StartPreview(transmogElement, data.Prefab, category));
                }
            }
        }
    }
    
    private static void StartPreview(GameObject go, string prefab, ItemDrop.ItemData.ItemType category)
    {
        Color c = Color.white;
        if (go.transform.Find("HEX").GetComponent<TMP_InputField>() is { } tmp &&
            ColorUtility.TryParseHtmlString("#" + tmp.text, out Color test)) c = test;
        PlayerModelPreview.SetAsCurrent(PlayerModelPreview.CreatePlayerModel(prefab, category, c));
    }
    


    private static void ClickTransmog(Transmogrification_DataTypes.TransmogItem_Data data, GameObject element)
    {
        AssetStorage.AUsrc.Play();
        if (CurrentChoosenItem == null) return;
        GameObject priceItem = ZNetScene.instance.GetPrefab(data.Price_Prefab);
        if (priceItem == null) return;
        int priceCount =
            Player.m_localPlayer.m_inventory.CountItems(priceItem.GetComponent<ItemDrop>().m_itemData.m_shared
                .m_name);
        if (priceCount < data.Price_Amount) return;
        Player.m_localPlayer.m_inventory.RemoveItem(priceItem.GetComponent<ItemDrop>().m_itemData.m_shared.m_name,
            data.Price_Amount);

        Transmogrification_DataTypes.TransmogItem_ComponentV2 newTransmog =
            CurrentChoosenItem.Data().GetOrCreate<Transmogrification_DataTypes.TransmogItem_ComponentV2>();
        newTransmog.ReplacedPrefab = data.Prefab;
        string hex = "#" + element.transform.Find("HEX").GetComponent<TMP_InputField>().text;
        newTransmog.ItemColor = ColorUtility.TryParseHtmlString(hex, out _) ? hex : "";
        newTransmog.Save();
        ChoosenItem_Transform.Find("EIDF").gameObject.SetActive(true);
        GameObject eff = UnityEngine.Object.Instantiate(ClickEffect, ChoosenItem_Transform);
        eff.transform.SetAsLastSibling();
        GameObject eff2 = UnityEngine.Object.Instantiate(ClickEffect2, ChoosenItem_Transform);
        eff2.GetComponent<RectTransform>().position = element.transform.position;
        AssetStorage.AUsrc.PlayOneShot(Gambler_UI.SOUNDEFFECT3, 0.6f);
        LoadCategories(CurrentChoosenItem);
        Inventory_Content.GetChild(IndexOfChoosenItem).Find("Text").GetComponent<Text>().color =
            new Color(1f, 0.31f, 0.31f);
        string logMessage =
            $"Player {Player.m_localPlayer.GetPlayerName()} has transmogged {CurrentChoosenItem.m_shared.m_name} " +
            $"to {data.GetLocalizedName()} for {data.Price_Amount} {priceItem.GetComponent<ItemDrop>().m_itemData.m_shared.m_name}";
        ZRoutedRpc.instance.InvokeRoutedRPC("LogOnServer_mpasn", 4, logMessage);
    }

    private static void TransformReverse()
    {
        AssetStorage.AUsrc.Play();
        if(CurrentChoosenItem?.Data().Get<Transmogrification_DataTypes.TransmogItem_ComponentV2>() == null) return;
        CurrentChoosenItem.Data().Remove<Transmogrification_DataTypes.TransmogItem_ComponentV2>();
        GameObject eff = UnityEngine.Object.Instantiate(ClickEffectReverse, ChoosenItem_Transform);
        eff.transform.SetAsLastSibling();
        AssetStorage.AUsrc.PlayOneShot(Gambler_UI.SOUNDEFFECT3, 0.6f);
        ChoosenItem_Transform.Find("EIDF").gameObject.SetActive(false);
        Inventory_Content.GetChild(IndexOfChoosenItem).Find("Text").GetComponent<Text>().color = Color.white;
    }


    private static int IndexOfChoosenItem;

    private static void ChooseItem(ItemDrop.ItemData item, int childIndex)
    {
        AssetStorage.AUsrc.Play();
        if (item == CurrentChoosenItem)
        {
            CurrentChoosenItem = null;
            LoadInventory();
            return;
        }

        CurrentChoosenItem = item;
        IndexOfChoosenItem = childIndex;
        foreach (Transform o in Inventory_Content)
        {
            o.GetComponent<Image>().color = Color.white;
        }

        Inventory_Content.GetChild(childIndex).GetComponent<Image>().color = Color.green;
        ChoosenItem_Transform.Find("Text").GetComponent<Text>().text =
            Localization.instance.Localize(item.m_shared.m_name);
        ChoosenItem_Transform.Find("Icon").GetComponent<Image>().sprite = item.GetIcon();
        ChoosenItem_Transform.Find("EIDF").gameObject
            .SetActive(item.HasTransmog());
        LoadCategories(item);
    } 

    private static void ClearStuff()
    {
        CurrentChoosenItem = null;
        ChoosenItem_Transform.Find("Text").GetComponent<Text>().text = "";
        ChoosenItem_Transform.Find("Icon").GetComponent<Image>().sprite =
            AssetStorage.NullSprite;
        ChoosenItem_Transform.Find("EIDF").gameObject.SetActive(false);
        foreach (Transform child in Inventory_Content)
            UnityEngine.Object.Destroy(child.gameObject);
        foreach (Transform child in Category_Content)
            UnityEngine.Object.Destroy(child.gameObject);
    }

    public static void Reload()
    {
        if (IsVisble())
        {
            LoadInventory();
        }
    }

    public static void Show(string profile, string _npcName)
    {
        if (!Transmogrification_Main_Client.FilteredTransmogData.ContainsKey(profile)) return;
        CurrentProfile = profile;
        UI.transform.Find("Canvas/Header/Text").GetComponent<Text>().text = _npcName;
        CurrentChoosenItem = null;
        UI.SetActive(true);
        LoadInventory();
    }

    public static void Hide()
    {
        UI.SetActive(false);
        ClearStuff();
    }


    [HarmonyPatch(typeof(TextInput), nameof(TextInput.IsVisible))]
    [ClientOnlyPatch]
    private static class visiblePatch
    {
        [UsedImplicitly]
private static void Postfix(ref bool __result)
        {
            if (IsVisble()) __result = true;
        }
    }
}