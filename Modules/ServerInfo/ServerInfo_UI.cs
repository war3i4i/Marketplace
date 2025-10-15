using Marketplace.ExternalLoads;

namespace Marketplace.Modules.ServerInfo;

public static class ServerInfo_UI
{
    private static GameObject UI;
    private static Text NPCname;
    private static Scrollbar Scrollbar;
    private static string CurrentProfile;
    private static Transform Content;

    public static bool IsPanelVisible()
    {
        return UI && UI.activeSelf;
    }

    public static void Init()
    {
        UI = UnityEngine.Object.Instantiate(
            AssetStorage.asset.LoadAsset<GameObject>("MarketplaceInfoNewUI"));
        UnityEngine.Object.DontDestroyOnLoad(UI);
        NPCname = UI.transform.Find("Canvas/Header/Text").GetComponent<Text>();
        Content = UI.transform.Find("Canvas/Scroll View/Viewport/Content/");
        Scrollbar = UI.GetComponentInChildren<Scrollbar>();
        UI.SetActive(false);
    }


    public static void Reload()
    {
        if (!IsPanelVisible()) return;
        if (!ServerInfo_DataTypes.SyncedServerInfoData.Value.ContainsKey(CurrentProfile))
        {
            Hide();
            return;
        }

        const int maxX = 700;
        Transform origText = Content.Find("Text");
        Transform origImage = Content.Find("Image");

        for (int i = 2; i < Content.childCount; ++i)
            UnityEngine.Object.Destroy(Content.GetChild(i).gameObject);

        foreach (ServerInfo_DataTypes.ServerInfoQueue.Info q in ServerInfo_DataTypes.SyncedServerInfoData.Value[CurrentProfile].infoQueue)
        {
            if (q.Type == ServerInfo_DataTypes.ServerInfoQueue.Info.InfoType.Text)
            {
                Transform text = UnityEngine.Object.Instantiate(origText, Content);
                text.gameObject.SetActive(true);
                text.GetComponent<Text>().text = q.Text;
            }
            else
            {
                Transform image = UnityEngine.Object.Instantiate(origImage, Content);
                image.gameObject.SetActive(true);
                Sprite sprite = q.GetSprite();
                image.GetComponent<Image>().sprite = sprite;
                if (sprite != null && sprite.texture != null)
                    image.GetComponent<RectTransform>().sizeDelta = new Vector2(Mathf.Min(maxX, sprite.texture.width),
                        sprite.texture.height);
            }
        }

        Canvas.ForceUpdateCanvases();
        List<ContentSizeFitter> AllFilters = UI.GetComponentsInChildren<ContentSizeFitter>().ToList();
        AllFilters.ForEach(filter => filter.enabled = false);
        AllFilters.ForEach(filter => filter.enabled = true);
    }

    public static void Hide()
    {
        for (int i = 2; i < Content.childCount; ++i)
            UnityEngine.Object.Destroy(Content.GetChild(i).gameObject);
        UI.SetActive(false);
    }


    public static void Show(string profile, string _npcName)
    {
        if (!ServerInfo_DataTypes.SyncedServerInfoData.Value.ContainsKey(profile)) return;
        CurrentProfile = profile;
        UI.SetActive(true);
        _npcName = Utils.RichTextFormatting(_npcName);
        NPCname.text = string.IsNullOrEmpty(_npcName) ? Localization.instance.Localize("$mpasn_Info") : _npcName;
        Scrollbar.value = 1;
        Reload();
    }

    [HarmonyPatch(typeof(TextInput), nameof(TextInput.IsVisible))]
    [ClientOnlyPatch]
    private static class InfoUIFix
    {
        [UsedImplicitly]
private static void Postfix(ref bool __result)
        {
            if (IsPanelVisible()) __result = true;
        }
    }
}