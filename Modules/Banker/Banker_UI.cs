using Marketplace.ExternalLoads;

namespace Marketplace.Modules.Banker;

public static class Banker_UI
{
    private static GameObject UI = null!;
    private static GameObject ItemElement = null!;
    private static Transform ContentTransform = null!;
    private static readonly List<GameObject> CurrentObjects = new();
    private static readonly List<InputField> CurrentValues = new();
    private static string CurrentProfile = "";
    private static Scrollbar MainBar = null!;
    private static readonly List<ContentSizeFitter> AllFilters = new();
    private static Text NPCName = null!;

    public static bool IsPanelVisible()
    {
        return UI && UI.activeSelf;
    }

    [HarmonyPatch(typeof(TextInput), nameof(TextInput.IsVisible))]
    [ClientOnlyPatch]
    private static class BankerUIFix
    {
        [UsedImplicitly]        
        private static void Postfix(ref bool __result)
        {
            if (IsPanelVisible()) __result = true;
        }
    }

    public static void Init()
    {
        UI = UnityEngine.Object.Instantiate(AssetStorage.asset.LoadAsset<GameObject>("MarketplaceBankerNewUI"));
        ItemElement = AssetStorage.asset.LoadAsset<GameObject>("BankerItem");
        ContentTransform = UI.transform.Find("Canvas/Trader/ItemList/ListPanel/Scroll View/Viewport/Content");
        UnityEngine.Object.DontDestroyOnLoad(UI);
        MainBar = UI.GetComponentInChildren<Scrollbar>();
        AllFilters.AddRange(UI.GetComponentsInChildren<ContentSizeFitter>().ToList());
        NPCName = UI.transform.Find("Canvas/Trader/Header/Text").GetComponent<Text>();
        UI.transform.Find("Canvas/Trader/Background/PutAll").GetComponent<Button>().onClick.AddListener(PutAll);
        UI.SetActive(false);
        Localization.instance.Localize(ItemElement.transform);
        Localization.instance.Localize(UI.transform);
    }


    private static void PutAll()
    {
        if (string.IsNullOrEmpty(CurrentProfile)) return;
        if (!Banker_DataTypes.SyncedBankerProfiles.Value.ContainsKey(CurrentProfile)) return;

        Dictionary<string, int> items = new();
        foreach (ItemDrop.ItemData data in Player.m_localPlayer.m_inventory.m_inventory)
        {
            if (data.m_stack <= 0 || !data.m_dropPrefab) continue;
            if (!Banker_DataTypes.SyncedBankerProfiles.Value[CurrentProfile].Contains(data.m_dropPrefab.name.GetStableHashCode())) continue;
            if (data.m_quality > 1) continue;
            if (items.ContainsKey(data.m_dropPrefab.name))
            {
                items[data.m_dropPrefab.name] += data.m_stack;
            }
            else
            {
                items.Add(data.m_dropPrefab.name, data.m_stack);
            }

            data.m_stack = 0;
        }

 
        foreach (KeyValuePair<string, int> item in items)
        {
            ZRoutedRpc.instance.InvokeRoutedRPC("KGmarket BankerDeposit", item.Key, item.Value);
        }

        Player.m_localPlayer.m_inventory.m_inventory.RemoveAll(x => x.m_stack <= 0);
        Player.m_localPlayer.m_inventory.Changed();
    }


    private static void CreateElements(string profile)
    {
        CurrentObjects.ForEach(UnityEngine.Object.Destroy);
        CurrentObjects.Clear();
        CurrentValues.Clear();
        if (!Banker_DataTypes.SyncedBankerProfiles.Value.ContainsKey(profile)) return;
        for (int i = 0; i < Banker_DataTypes.SyncedBankerProfiles.Value[profile].Count; ++i)
        {
            int data = Banker_DataTypes.SyncedBankerProfiles.Value[profile][i];
            ItemDrop item = ZNetScene.instance.GetPrefab(data)?.GetComponent<ItemDrop>()!;
            if (item == null) break;
            GameObject go = UnityEngine.Object.Instantiate(ItemElement, ContentTransform);
            string text = Localization.instance.Localize(item.m_itemData.m_shared.m_name);
            int quantityBank = Banker_DataTypes.BankerClientData.TryGetValue(data, out int value) ? value : 0;
            int id = i;
            go.transform.Find("Icon/IconItem").GetComponent<Image>().sprite = item.m_itemData.GetIcon();
            go.transform.Find("Inventory").GetComponent<Text>().text =
                Localization.instance.Localize("$mpasn_inventory: ") + Utils.CustomCountItems(item.name, 1);
            go.transform.Find("ItemName").GetComponent<Text>().text = text;
            go.transform.Find("BankCount").GetComponent<Text>().text = quantityBank.ToString();
            go.transform.Find("Control/InputField").GetComponent<InputField>().onValueChanged
                .AddListener(delegate(string arg0) { ValueChange(id, arg0); });
            go.transform.Find("Control/Add").GetComponent<Button>().onClick
                .AddListener(delegate { ClickBUTTON(id, data, true); });
            go.transform.Find("Control/Remove").GetComponent<Button>().onClick
                .AddListener(delegate { ClickBUTTON(id, data, false); });
            CurrentObjects.Add(go);
            CurrentValues.Add(go.transform.Find("Control/InputField").GetComponent<InputField>());
        }

        Canvas.ForceUpdateCanvases();
        AllFilters.ForEach(filter => filter.enabled = false);
        AllFilters.ForEach(filter => filter.enabled = true);
        MainBar.value = 1f;
    }

    private static void ValueChange(int index, string data)
    {
        AssetStorage.AUsrc.PlayOneShot(AssetStorage.TypeClip, 0.7f);
        if (int.TryParse(data, out int value) && value < 0)
        {
            CurrentValues[index].text = "0";
        }
    }

    private static void ClickBUTTON(int index, int hash, bool DEPOSIT)
    {
        AssetStorage.AUsrc.Play();
        if (!int.TryParse(CurrentValues[index].text, out int value)) return;
        CurrentValues[index].text = "";
        if (value <= 0) return;
        GameObject item = ZNetScene.instance.GetPrefab(hash);
        string prefab = item.name;
        if (DEPOSIT)
        {
            int inventoryCount = Utils.CustomCountItems(prefab, 1);
            if (value > inventoryCount) value = inventoryCount;
            if (value <= 0) return;
            Utils.CustomRemoveItems(prefab, value, 1);
            ZRoutedRpc.instance.InvokeRoutedRPC("KGmarket BankerDeposit", prefab, value);
        }
        else
        {
            ZRoutedRpc.instance.InvokeRoutedRPC("KGmarket BankerWithdraw", prefab, value);
        }

        CurrentValues[index].text = "";
    }


    public static void Hide()
    {
        UI.SetActive(false);
        CurrentObjects.ForEach(UnityEngine.Object.Destroy);
        CurrentObjects.Clear();
    }

    public static void Reload()
    {
        if (IsPanelVisible())
        {
            CreateElements(CurrentProfile);
        }
    }

    public static void Show(string profile, string _npcName)
    {
        if (!Banker_DataTypes.SyncedBankerProfiles.Value.ContainsKey(profile)) return;
        CurrentProfile = profile;
        UI.SetActive(true);
        CreateElements(CurrentProfile);
        _npcName = Utils.RichTextFormatting(_npcName);
        NPCName.text = string.IsNullOrEmpty(_npcName) ? Localization.instance.Localize("$mpasn_Banker") : _npcName;
    }
}