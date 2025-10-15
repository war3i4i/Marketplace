using Marketplace.ExternalLoads;
using Marketplace.Modules.Global_Options;

namespace Marketplace.Modules.Leaderboard;

using Object = UnityEngine.Object;

public static class Leaderboard_UI
{
    private static GameObject UI = null!;
    private static GameObject Main = null!;
    private static SortBy CurrentSort;
    private static GameObject Element = null!;
    private static Transform Content = null!;
    private static GameObject Achievements = null!;
    private static GameObject AchievementsElement = null!;
    private static Transform AchievementContent = null!;
    private static Text PageText = null!;

    public static bool IsVisible() => Main && Main.activeSelf;

    private enum SortBy
    {
        CreaturesKilled,
        BuiltStructures,
        ItemsCrafted,
        PlayersKilled,
        Died,
        MapExplored,
        TotalAchievements,
        Harvested
    }

    private static readonly Dictionary<SortBy, Button> SortButtons = new();

    private static int CurrentPage = 1;
    private const int MaxPerPage = 10;
    private static GameObject CurrentSelectedElement;
    private static List<Leaderboard_DataTypes.Client_Leaderboard> SortedList = new();

    public static void Init()
    {
        UI = Object.Instantiate(AssetStorage.asset.LoadAsset<GameObject>("MarketplaceLeaderboardUI"));
        Main = UI.transform.Find("GO").gameObject;
        Element = AssetStorage.asset.LoadAsset<GameObject>("MarketplaceLB_PlayerElement");
        Object.DontDestroyOnLoad(UI);
        Transform buttonsTransform = UI.transform.Find("GO/Header_Buttons");
        SortButtons.Add(SortBy.CreaturesKilled, buttonsTransform.Find("CreaturesKilled").GetComponent<Button>());
        SortButtons.Add(SortBy.BuiltStructures, buttonsTransform.Find("BuiltStructures").GetComponent<Button>());
        SortButtons.Add(SortBy.ItemsCrafted, buttonsTransform.Find("ItemsCrafted").GetComponent<Button>());
        SortButtons.Add(SortBy.Died, buttonsTransform.Find("Died").GetComponent<Button>());
        SortButtons.Add(SortBy.MapExplored, buttonsTransform.Find("MapExplored").GetComponent<Button>());
        SortButtons.Add(SortBy.TotalAchievements, buttonsTransform.Find("TotalAchievements").GetComponent<Button>());
        SortButtons.Add(SortBy.PlayersKilled, buttonsTransform.Find("PlayersKilled").GetComponent<Button>());
        SortButtons.Add(SortBy.Harvested, buttonsTransform.Find("Harvested").GetComponent<Button>());
        foreach (KeyValuePair<SortBy, Button> button in SortButtons)
            button.Value.onClick.AddListener(() =>
            {
                AssetStorage.AUsrc.Play();
                SetSortBy(button.Key);
                CreateElements();
            });
        Content = UI.transform.Find("GO/LB");
        PageText = UI.transform.Find("GO/Page/Text").GetComponent<Text>();
        UI.transform.Find("GO/PageRight").GetComponent<Button>().onClick.AddListener(NextPage);
        UI.transform.Find("GO/PageLeft").GetComponent<Button>().onClick.AddListener(PrevPage);
        Achievements = UI.transform.Find("Achievements").gameObject;
        AchievementsElement = AssetStorage.asset.LoadAsset<GameObject>("Marketplace_Achievement_Element");
        AchievementContent = Achievements.transform.Find("Scroll Rect/Viewport/Content");
        UI.transform.Find("Open").GetComponent<Button>().onClick.AddListener(() =>
        {
            AssetStorage.AUsrc.Play();
            if (IsVisible()) Hide();
            else Show();
        });
        UI.SetActive(false);
        Main.SetActive(false);
        Achievements.SetActive(false);
        SetSortBy(SortBy.TotalAchievements);
        Global_Configs.SyncedGlobalOptions.ValueChanged += OnChange;
        Localization.instance.Localize(UI.transform);
    }

    private static void OnChange()
    {
        if (Global_Configs.SyncedGlobalOptions.Value._useLeaderboard)
        {
            ShowTF();
        }
        else
        {
            HideTF();
        }
    }

    private static void NextPage()
    {
        AssetStorage.AUsrc.Play();
        int maxPage = Mathf.CeilToInt(SortedList.Count / (float)MaxPerPage);
        if (CurrentPage + 1 > maxPage) return;
        CurrentPage++;
        if (maxPage == 0) maxPage = 1;
        PageText.text = $"{CurrentPage}/{maxPage}";
        CreateElements();
    }

    private static void PrevPage()
    {
        AssetStorage.AUsrc.Play();
        if (CurrentPage - 1 < 1) return;
        CurrentPage--;
        int maxPage = Mathf.CeilToInt(SortedList.Count / (float)MaxPerPage);
        if (maxPage == 0) maxPage = 1;
        PageText.text = $"{CurrentPage}/{maxPage}";
        CreateElements();
    }

    public static int GetAchievementScore(List<int> achievements)
    {
        int result = 0;
        foreach (int achievement in achievements)
            result += Leaderboard_DataTypes.SyncedClientAchievements.Value.Find(x => x.ID == achievement) is { } t
                ? t.Score
                : 0;
        return result;
    }

    private static void SetSortBy(SortBy sort)
    {
        CurrentSort = sort;
        foreach (KeyValuePair<SortBy, Button> button in SortButtons)
            button.Value.GetComponent<Text>().color = new Color(0.9137256f, 0.8627452f, 0.007843138f);
        SortButtons[sort].GetComponent<Text>().color = Color.green;

        List<Leaderboard_DataTypes.Client_Leaderboard> valuesOnly =
            Leaderboard_DataTypes.SyncedClientLeaderboard.Value.Select(x => x.Value).ToList();
        SortedList = sort switch
        {
            SortBy.CreaturesKilled => valuesOnly.OrderByDescending(x => x.KilledCreatures).ToList(),
            SortBy.BuiltStructures => valuesOnly.OrderByDescending(x => x.BuiltStructures).ToList(),
            SortBy.ItemsCrafted => valuesOnly.OrderByDescending(x => x.ItemsCrafted).ToList(),
            SortBy.Died => valuesOnly.OrderByDescending(x => x.Died).ToList(),
            SortBy.MapExplored => valuesOnly.OrderByDescending(x => x.MapExplored).ToList(),
            SortBy.TotalAchievements => valuesOnly.OrderByDescending(x => GetAchievementScore(x.Achievements)).ToList(),
            SortBy.PlayersKilled => valuesOnly.OrderByDescending(x => x.KilledPlayers).ToList(),
            SortBy.Harvested => valuesOnly.OrderByDescending(x => x.Harvested).ToList(),
            _ => new()
        };
        int maxPage = Mathf.CeilToInt(SortedList.Count / (float)MaxPerPage);
        if (maxPage == 0) maxPage = 1;
        PageText.text = $"{CurrentPage}/{maxPage}";
    }

    private static void Default()
    {
        Main.SetActive(false);
        Achievements.SetActive(false);
        CurrentPage = 1;
        CurrentSelectedElement = null;
        SetSortBy(SortBy.TotalAchievements);
    }

    private static void CreateElements()
    {
        foreach (Transform child in Content)
            Object.Destroy(child.gameObject);
        Achievements.SetActive(false);
        CurrentSelectedElement = null;
        int start = (CurrentPage - 1) * MaxPerPage;
        int end = start + MaxPerPage;
        for (int i = start; i < end; ++i)
        {
            if (i >= SortedList.Count) break;
            GameObject element = Object.Instantiate(Element, Content);
            Leaderboard_DataTypes.Client_Leaderboard data = SortedList[i];

            Transform rankTransform = element.transform.Find("Rank");
            string cstring = (i + 1).ToString();
            if (i + 1 <= 3)
                rankTransform.Find(cstring).gameObject.SetActive(true);
            else
                rankTransform.Find("etc").gameObject.SetActive(true);
            rankTransform.Find("Text").GetComponent<Text>().text = cstring;

            element.transform.Find("PlayerName").GetComponent<Text>().text = data.PlayerName;
            element.transform.Find("CreaturesKilled").GetComponent<Text>().text = data.KilledCreatures.ToString();
            element.transform.Find("BuiltStructures").GetComponent<Text>().text = data.BuiltStructures.ToString();
            element.transform.Find("ItemsCrafted").GetComponent<Text>().text = data.ItemsCrafted.ToString();
            element.transform.Find("Died").GetComponent<Text>().text = data.Died.ToString();
            element.transform.Find("PlayersKilled").GetComponent<Text>().text = data.KilledPlayers.ToString();
            element.transform.Find("Harvested").GetComponent<Text>().text = data.Harvested.ToString();
            element.transform.Find("MapExplored").GetComponent<Text>().text = data.MapExplored + "%";
            
            element.transform.Find("TotalAchievements").GetComponent<Text>().text = GetAchievementScore(data.Achievements).ToString();

            element.transform.Find(CurrentSort.ToString()).GetComponent<Text>().color = Color.green;

            element.GetComponent<UIInputHandler>().m_onPointerEnter += (_) => OnHoverStart(element);
            element.GetComponent<UIInputHandler>().m_onPointerExit += (_) => OnHoverEnd();
            element.GetComponent<UIInputHandler>().m_onLeftClick += (_) => OnElementClick(element, data);
        }
    }

    private static void OnHoverStart(GameObject go)
    {
        foreach (Transform child in Content)
            if (child.gameObject != CurrentSelectedElement)
                child.Find("fill").GetComponent<Image>().color = Color.white;
        if (go != CurrentSelectedElement)
            go.transform.Find("fill").GetComponent<Image>().color = new Color(0.44f, 1f, 0.51f);
    }

    private static void OnHoverEnd()
    {
        foreach (Transform child in Content)
            if (child.gameObject != CurrentSelectedElement)
                child.Find("fill").GetComponent<Image>().color = Color.white;
    }

    private static void OnElementClick(GameObject go, Leaderboard_DataTypes.Client_Leaderboard board)
    {
        AssetStorage.AUsrc.Play();
        if (CurrentSelectedElement == go)
        {
            CurrentSelectedElement.transform.Find("fill").GetComponent<Image>().color = Color.white;
            CurrentSelectedElement = null;
            Achievements.SetActive(false);
            return;
        }

        if (CurrentSelectedElement != null)
            CurrentSelectedElement.transform.Find("fill").GetComponent<Image>().color = Color.white;
        CurrentSelectedElement = go;
        go.transform.Find("fill").GetComponent<Image>().color = Color.green;
        InitAchievements(board);
    } 

    private static void InitAchievements(Leaderboard_DataTypes.Client_Leaderboard board)
    {
        Achievements.SetActive(true);
        foreach (Transform child in AchievementContent)
            Object.Destroy(child.gameObject);
        Achievements.transform.Find("Achievements").GetComponent<Text>().text = board.PlayerName + "$mpasn_LeaderboardAchievements".Localize();
        foreach (int achievement in board.Achievements)
        {
            if (Leaderboard_DataTypes.SyncedClientAchievements.Value.Find(x => x.ID == achievement) is not { } t) continue;
            GameObject element = Object.Instantiate(AchievementsElement, AchievementContent);
            element.transform.Find("color").GetComponent<Image>().color = t.Color;
            element.transform.Find("Name").GetComponent<Text>().text = t.Name;
            element.transform.Find("Name").GetComponent<Text>().color = t.Color;
            element.transform.Find("Description").GetComponent<Text>().text = "(" + t.Description + ")";
        }
    }

    public static void Show()
    {
        if (!Global_Configs.SyncedGlobalOptions.Value._useLeaderboard) return;
        Default();
        Main.SetActive(true);
        CreateElements();
    }

    public static void Hide()
    {
        Main.SetActive(false);
        Achievements.SetActive(false);
    }

    private static void ShowTF()
    {
        UI.SetActive(true);
        Default();
    }

    private static void HideTF()
    {
        UI.SetActive(false);
        Default();
    }

    [HarmonyPatch(typeof(TextInput), nameof(TextInput.IsVisible))]
    [ClientOnlyPatch]
    private static class Menu_IsVisible_Patch
    {
        [UsedImplicitly]
private static void Postfix(ref bool __result) => __result |= IsVisible();
    }

    [HarmonyPatch(typeof(FejdStartup), nameof(FejdStartup.Awake))]
    [ClientOnlyPatch]
    private static class Menu_OnLogoutYes_Patch
    {
        [UsedImplicitly]
private static void Postfix()
        {
            HideTF();
        }
    }
}