using Marketplace.ExternalLoads;
using Marketplace.Modules.Global_Options;
using UnityEngine.EventSystems;
using Random = UnityEngine.Random;

namespace Marketplace.Modules.Gambler;

public static class Gambler_UI
{
    private static GameObject UI = null!;
    private static Transform MainTransform = null!;
    private static string CurrentProfile = "";
    private static readonly List<Transform> Elements = new();
    private static readonly HashSet<int> Exclude = new();
    private static readonly Dictionary<Transform, Gambler_DataTypes.Item> tempDictionary = new();
    private static Transform Button = null!;
    private static List<int> currentShuffle = new();
    private static readonly bool[] Shuffling = new bool[2];
    private static readonly List<Image> ElementsAlpha = new();
    private static readonly List<Transform> ALLELEMENTS = new();
    private static readonly List<Image> ALLIMAGES = new();
    public static Status CurrentStatus;
    private static readonly List<GameObject> CurrentCircleEffects = new();
    private static Text Description = null!;
    private static Text NeededItemText = null!;
    private static Transform RequiredTab = null!;
    private static int CurrentRollMAX = 1;
    private static readonly HashSet<int> AlreadyClicked = new();
    private static bool DisableSound;

    private static readonly List<Vector3> InitialPositions = new();
    private static GameObject ClickEffect = null!;
    private static GameObject SmokeEffect = null!;
    private static Sprite QuestionMarkIcon = null!;
    private static GameObject CircleEffect = null!;
    private static AudioClip SOUNDEFFECT = null!;
    private static AudioClip SOUNDEFFECT2 = null!;
    public static AudioClip SOUNDEFFECT3 = null!;

    public enum Status
    {
        Idle,
        Shuffling,
        Done,
        Revealed
    }

    public static void Reload()
    {
        Hide();
        Show(CurrentProfile);
    }


    public static bool IsPanelVisible()
    {
        return UI && UI.activeSelf;
    }

    private static void PlaySound(AudioClip clip, float volume)
    {
        if (!DisableSound)
        {
            AssetStorage.AUsrc.PlayOneShot(clip, volume);
        }
    }

    private class OnHoverGambler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private Transform t = null!;
        private Image img = null!;

        private void Start()
        {
            t = transform.Find("Choose");
            img = t.GetComponent<Image>();
            t.gameObject.SetActive(true);
            img.color = new Color(1, 1, 1, 0);
        }

        // Update is called once per frame

        public void OnPointerEnter(PointerEventData eventData)
        {
            StopAllCoroutines();
            if (CurrentStatus != Status.Done) return;
            StartCoroutine(AddRemoveAlpha(true));
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            StopAllCoroutines();
            StartCoroutine(AddRemoveAlpha(false));
        }


        private IEnumerator AddRemoveAlpha(bool add)
        {
            float dt = img.color.a;
            if (add)
            {
                while (dt < 0.3f)
                {
                    dt = dt + Time.deltaTime / 2;
                    dt = Mathf.Min(dt, 0.3f);
                    img.color = new Color(0.4464217f, 0.8f, 0.1179245f, dt);
                    yield return null;
                }
            }
            else
            {
                while (dt > 0)
                {
                    dt = dt - Time.deltaTime / 2;
                    dt = Mathf.Max(dt, 0);
                    img.color = new Color(0.4464217f, 0.8f, 0.1179245f, dt);
                    yield return null;
                }
            }
        }
    }

    public static void Init()
    {
        if (UI) UnityEngine.Object.Destroy(UI);
        DisableSound = Marketplace._thistype.Config.Bind("General", "Mute Gambler Sounds", false).Value;
        UI = UnityEngine.Object.Instantiate(
            AssetStorage.asset.LoadAsset<GameObject>("MarketplaceGamblerNewUI"));
        UnityEngine.Object.DontDestroyOnLoad(UI);
        ClickEffect = AssetStorage.asset.LoadAsset<GameObject>("RINGEFFECT");
        CircleEffect = AssetStorage.asset.LoadAsset<GameObject>("CircleEffect");
        SmokeEffect = AssetStorage.asset.LoadAsset<GameObject>("SMOKEEFFECT");
        SOUNDEFFECT = AssetStorage.asset.LoadAsset<AudioClip>("GAMBLERSOUND2");
        SOUNDEFFECT2 = AssetStorage.asset.LoadAsset<AudioClip>("BURSTGAMBLE");
        SOUNDEFFECT3 = AssetStorage.asset.LoadAsset<AudioClip>("GAMBLERDONE");
        QuestionMarkIcon = AssetStorage.asset.LoadAsset<Sprite>("qCS 2");
        CurrentStatus = Status.Idle; 
        Shuffling[0] = false;
        Shuffling[1] = false;
        ALLELEMENTS.Clear();
        InitialPositions.Clear();
        ALLIMAGES.Clear();
        MainTransform = UI.transform.Find("Canvas/GambleElements");
        Transform parent = MainTransform.parent;
        Button = parent.Find("Start");
        Button.GetComponent<Button>().onClick.AddListener(StartShuffle);
        Description = parent.Find("Description").GetComponent<Text>();
        NeededItemText = parent.Find("Required/Text").GetComponent<Text>();
        RequiredTab = parent.Find("Required");
        int count = 0;
        for (int i = 0; i < 18; ++i)
        {
            int deleg = count;
            Transform child = MainTransform.GetChild(i);
            child.GetComponent<Button>().onClick.AddListener(delegate
            {
                Marketplace._thistype.StartCoroutine(ClickOnElement(deleg));
            });
            count++;

            child.gameObject.AddComponent<OnHoverGambler>();
            ALLELEMENTS.Add(child);
            InitialPositions.Add(child.GetComponent<RectTransform>().anchoredPosition);
            foreach (Image componentsInChild in MainTransform.GetChild(i).GetComponentsInChildren<Image>())
            {
                ALLIMAGES.Add(componentsInChild);
            }
        }

        UI.SetActive(false);
        Localization.instance.Localize(UI.transform);
    }

    [HarmonyPatch(typeof(TextInput), nameof(TextInput.IsVisible))]
    [ClientOnlyPatch]
    private static class INPUTPATCHforGambler
    {
        [UsedImplicitly]
private static void Postfix(ref bool __result)
        {
            if (IsPanelVisible()) __result = true;
        }
    }

    private static void ResetDefault()
    {
        Shuffling[0] = false;
        Shuffling[1] = false;
        int posCounter = 0;
        foreach (Transform VARIABLE in ALLELEMENTS)
        {
            VARIABLE.gameObject.SetActive(false);
            VARIABLE.GetComponent<RectTransform>().anchoredPosition = InitialPositions[posCounter];
            posCounter++;
        }

        AlreadyClicked.Clear();
        CurrentRollMAX = 1;
        foreach (Image VARIABLE in ALLIMAGES)
        {
            VARIABLE.color *= new Color(1, 1, 1, 0);
        }

        CurrentCircleEffects.ForEach(UnityEngine.Object.Destroy);
        CurrentCircleEffects.Clear();
        CurrentStatus = Status.Idle;
        Description.gameObject.SetActive(true);
        Button.gameObject.SetActive(true);
        RequiredTab.gameObject.SetActive(true);
        Elements.Clear();
        ElementsAlpha.Clear();
        for (int i = 0; i < Gambler_DataTypes.SyncedGamblerData.Value[CurrentProfile].Data.Count(); ++i)
        {
            Transform child = MainTransform.GetChild(i);
            child.gameObject.SetActive(true);
            Elements.Add(child);
            ElementsAlpha.AddRange(child.GetComponentsInChildren<Image>()
                .Where(img => img.transform.name != "Choose"));
            child.GetChild(2).GetComponent<Image>().sprite = QuestionMarkIcon;
        }

        currentShuffle = Enumerable.Range(0, Gambler_DataTypes.SyncedGamblerData.Value[CurrentProfile].Data.Count()).ToList();
        Exclude.Clear();
        string description =
            $"<size=60>{Localization.instance.Localize("$mpasn_gambleryouhaveachance")}:</size><size=20>\n";
        foreach (Gambler_DataTypes.Item VARIABLE in Gambler_DataTypes.SyncedGamblerData.Value[CurrentProfile].Data)
        {
            if (VARIABLE.Min == VARIABLE.Max)
            {
                description += VARIABLE.Name + $" <color=#00ff00>x{VARIABLE.Min}</color>\n";
            }
            else
            {
                description += VARIABLE.Name + $" <color=#00ff00>x{VARIABLE.Min}-{VARIABLE.Max}</color>\n";
            }
        }

        Description.text = description + "</size>";
    }


    private static IEnumerator ClickOnElement(int whichOne)
    {
        if (CurrentStatus != Status.Done || CurrentRollMAX <= 0 || AlreadyClicked.Contains(whichOne) ||
            !Player.m_localPlayer) yield break;
        AlreadyClicked.Add(whichOne);
        CurrentRollMAX--;
        bool CONTINUE = CurrentRollMAX <= 0;
        UnityEngine.Object.Instantiate(ClickEffect, Elements[whichOne].transform);
        PlaySound(SOUNDEFFECT3, 1f);
        yield return new WaitForSecondsRealtime(1f);
        PlaySound(SOUNDEFFECT2, 1f);
        UnityEngine.Object.Instantiate(SmokeEffect, Elements[whichOne].transform);
        CurrentCircleEffects.Add(UnityEngine.Object.Instantiate(CircleEffect, Elements[whichOne].transform));
        yield return new WaitForSecondsRealtime(0.8f);
        Elements[whichOne].transform.GetChild(2).GetComponent<Image>().sprite =
            tempDictionary[Elements[whichOne]].Sprite;
        int wonItemCount = Random.Range(tempDictionary[Elements[whichOne]].Min,
            tempDictionary[Elements[whichOne]].Max + 1);
        InstantiateItem(ZNetScene.instance.GetPrefab(tempDictionary[Elements[whichOne]].Prefab),
            wonItemCount);
        if (!Description.text.Contains("You Won"))
        {
            Description.text =
                $"<size=30>You Won:</size>\n<size=20><color=#00ff00>{tempDictionary[Elements[whichOne]].Name} x{wonItemCount}</color></size>";
        }
        else
        {
            Description.text +=
                $"<size=20> , <color=#00ff00>{tempDictionary[Elements[whichOne]].Name} x{wonItemCount}</color></size>";
        }

        if (!CONTINUE) yield break;
        CurrentStatus = Status.Revealed;
        int c = 0;
        PlaySound(SOUNDEFFECT2, 1f);
        foreach (Transform element in Elements)
        {
            if (!AlreadyClicked.Contains(c)) UnityEngine.Object.Instantiate(SmokeEffect, element.transform);
            c++;
        }

        yield return new WaitForSecondsRealtime(0.8f);
        c = 0;
        foreach (Transform element in Elements)
        {
            if (!AlreadyClicked.Contains(c))
                element.transform.GetChild(2).GetComponent<Image>().sprite = tempDictionary[element].Sprite;
            c++;
        }

        yield return new WaitForSecondsRealtime(1f);


        CurrentStatus = Status.Idle;
        Button.gameObject.SetActive(true);
        RequiredTab.gameObject.SetActive(true);
        RollString();
    }


    private static void InstantiateItem(GameObject main, int count)
    {
        Player p = Player.m_localPlayer;
        if (!p || main == null) return;

        if (main.GetComponent<ItemDrop>())
        {
            Transform transform = p.transform;
            GameObject go = UnityEngine.Object.Instantiate(main,
                transform.position + transform.forward * 1.5f + Vector3.up * 1.5f,
                Quaternion.identity);
            ItemDrop itemDrop = go.GetComponent<ItemDrop>();
            itemDrop.m_itemData.m_stack = count;
            itemDrop.m_itemData.m_durability = itemDrop.m_itemData.GetMaxDurability(1);
            itemDrop.Save();
            if (p.m_inventory.CanAddItem(go))
            {
                p.m_inventory.AddItem(itemDrop.m_itemData);
                ZNetScene.instance.Destroy(go);
            }

            if (ZNet.instance.GetServerPeer() != null)
            {
                ZPackage pkg = new();
                pkg.Write((int)DiscordStuff.DiscordStuff.Webhooks.Gambler);
                pkg.Write(Player.m_localPlayer.GetPlayerName());
                pkg.Write(count);
                pkg.Write(itemDrop.m_itemData.m_shared.m_name);
                ZRoutedRpc.instance.InvokeRoutedRPC(ZRoutedRpc.instance.GetServerPeerID(), "KGmarket CustomWebhooks",
                    pkg);
            }

            if (Global_Configs.SyncedGlobalOptions.Value._gamblerEnableNotifications)
            {
                string sendText =
                    $"{Player.m_localPlayer.GetPlayerName()} just won {Localization.instance.Localize(itemDrop.m_itemData.m_shared.m_name)} x{count}";
                UserInfo info = UserInfo.GetLocalUser();
                info.Name = "<color=#FF00FF>[GAMBLER]</color>";
                ZRoutedRpc.instance.InvokeRoutedRPC(ZRoutedRpc.Everybody, "ChatMessage", new object[]
                {
                    Player.m_localPlayer.GetHeadPoint(),
                    2,
                    info,
                    sendText,
                    "Steam_0"
                });
            }

            return;
        }

        if (main.GetComponent<Character>())
        {
            for (int i = 0; i < count; ++i)
            {
                Transform transform = p.transform;
                GameObject monster = UnityEngine.Object.Instantiate(main,
                    transform.position + transform.forward * 1.5f + Vector3.up * 1.5f,
                    Quaternion.identity);
                if (monster.GetComponent<Tameable>())
                {
                    monster.GetComponent<Tameable>().Tame();
                }
            }
        }
    }

    private static void StartShuffle()
    {
        AssetStorage.AUsrc.Play();
        if (CurrentStatus != Status.Idle || !CanRoll() || !Player.m_localPlayer) return;
        ResetDefault();

        CurrentRollMAX = Mathf.Max(1, Gambler_DataTypes.SyncedGamblerData.Value[CurrentProfile].MAXROLLS);
        CurrentRollMAX = Mathf.Min(Elements.Count, CurrentRollMAX);

        Player.m_localPlayer.m_inventory.RemoveItem(
            Gambler_DataTypes.SyncedGamblerData.Value[CurrentProfile].RequiredItem.RawName,
            Gambler_DataTypes.SyncedGamblerData.Value[CurrentProfile].RequiredItem.Min);

        tempDictionary.Clear();
        List<int> range = Enumerable.Range(0, Gambler_DataTypes.SyncedGamblerData.Value[CurrentProfile].Data.Count()).ToList();
        HashSet<int> tempExclude = new HashSet<int>();
        foreach (Transform element in Elements)
        {
            int random = Random.Range(0, range.Count());
            int randomValue = range.ElementAt(random);
            tempDictionary[element] = Gambler_DataTypes.SyncedGamblerData.Value[CurrentProfile].Data[randomValue];
            tempExclude.Add(randomValue);
            range = range.Where(val => !tempExclude.Contains(val)).ToList();
        }

        Marketplace._thistype.StartCoroutine(ShuffleMaster(5f));
        Button.gameObject.SetActive(false);
        RequiredTab.gameObject.SetActive(false);
        Description.gameObject.SetActive(false);
        CurrentStatus = Status.Shuffling;
    }


    private static IEnumerator ShuffleMaster(float time)
    {
        float dt = 0;
        while (dt < 2f)
        {
            dt += Time.deltaTime;
            dt = Mathf.Min(2, dt);
            foreach (Image VARIABLE in ElementsAlpha)
            {
                VARIABLE.color = new Color(VARIABLE.color.r, VARIABLE.color.g, VARIABLE.color.b,
                    dt / 2);
            }

            yield return null;
        }


        dt = 0;
        while (dt <= time)
        {
            dt += Time.deltaTime;
            if (!IsShuffling())
            {
                yield return new WaitForFixedUpdate();
                yield return new WaitForFixedUpdate();
                yield return new WaitForFixedUpdate();
                Shuffle();
            }

            yield return null;
        }

        Description.gameObject.SetActive(true);
        CurrentStatus = Status.Done;
        Description.text = "\n<color=#00ff00>Choose Reward</color>";
    }

    private static bool IsShuffling()
    {
        return Shuffling[0] || Shuffling[1];
    }

    private static void Shuffle()
    {
        if (currentShuffle.Count() <= 1)
        {
            currentShuffle = Enumerable.Range(0, Gambler_DataTypes.SyncedGamblerData.Value[CurrentProfile].Data.Count()).ToList();
            Exclude.Clear();
        }

        int firstRandomValue = Random.Range(0, currentShuffle.Count());
        Transform firstElement = Elements[currentShuffle.ElementAt(firstRandomValue)];
        Exclude.Add(currentShuffle.ElementAt(firstRandomValue));
        currentShuffle = currentShuffle.Where(value => !Exclude.Contains(value)).ToList();
        int secondRandomValue = Random.Range(0, currentShuffle.Count());
        Transform secondElement = Elements[currentShuffle.ElementAt(secondRandomValue)];
        Exclude.Add(currentShuffle.ElementAt(secondRandomValue));
        currentShuffle = currentShuffle.Where(value => !Exclude.Contains(value)).ToList();

        if (Random.value > 0.5f)
        {
            Marketplace._thistype.StartCoroutine(CannonBall(firstElement, secondElement.position, 350f, 0));
            Marketplace._thistype.StartCoroutine(CannonBall(secondElement, firstElement.position, -350f, 1));
            PlaySound(SOUNDEFFECT, 0.4f);
        }
        else
        {
            Marketplace._thistype.StartCoroutine(CannonBall(firstElement, secondElement.position, -350f, 0));
            Marketplace._thistype.StartCoroutine(CannonBall(secondElement, firstElement.position, 350f, 1));
            PlaySound(SOUNDEFFECT, 0.4f);
        }
    }

    private static IEnumerator CannonBall(Transform t, Vector3 targetPos, float mod, int index)
    {
        Vector3 initialScale = t.localScale;
        Vector3 newScale = initialScale;
        Shuffling[index] = true;
        Vector3 startPos = t.position;
        float count = 0;
        yield return null;
        while (count <= 1)
        {
            float plus = Time.deltaTime * 5f;
            count += plus;
            if (count <= 0.5f)
            {
                newScale = newScale - new Vector3(plus, plus, plus);
            }
            else
            {
                newScale = newScale + new Vector3(plus, plus, plus);
            }

            newScale = Vector3.Min(initialScale, newScale);
            t.localScale = newScale;
            Vector3 point = startPos + (targetPos - startPos) / 2 + Vector3.up * mod;
            Vector3 m1 = Vector3.Lerp(startPos, point, count);
            Vector3 m2 = Vector3.Lerp(point, targetPos, count);
            t.position = Vector3.Lerp(m1, m2, count);
            yield return null;
        }

        t.localScale = initialScale;
        t.position = targetPos;
        Shuffling[index] = false;
    }

    private static bool CanRoll()
    {
        return Player.m_localPlayer.m_inventory.CountItems(Gambler_DataTypes.SyncedGamblerData.Value[CurrentProfile]
                   .RequiredItem.RawName) >=
               Gambler_DataTypes.SyncedGamblerData.Value[CurrentProfile].RequiredItem.Min;
    }

    private static void RollString()
    {
        int quantity =
            Player.m_localPlayer.m_inventory.CountItems(Gambler_DataTypes.SyncedGamblerData.Value[CurrentProfile]
                .RequiredItem.RawName);
        int needed = Gambler_DataTypes.SyncedGamblerData.Value[CurrentProfile].RequiredItem.Min;
        NeededItemText.text = quantity + " / " + needed;
        NeededItemText.transform.parent.Find("Image").GetComponent<Image>().sprite =
            Gambler_DataTypes.SyncedGamblerData.Value[CurrentProfile].RequiredItem.Sprite;
        NeededItemText.transform.parent.Find("Background").GetComponent<Image>().color =
            quantity >= needed ? Color.green : Color.red;
        NeededItemText.color = quantity >= needed ? Color.green : Color.red;
    }

    public static void Hide()
    {
        UI.SetActive(false);
    }

    public static void Show(string profile)
    {
        if (!Gambler_DataTypes.SyncedGamblerData.Value.ContainsKey(profile) ||
            Gambler_DataTypes.SyncedGamblerData.Value[profile].Data.Count < 2) return;
        CurrentProfile = profile;
        if (CurrentStatus == Status.Idle)
        {
            ResetDefault();
        }

        RollString();
        UI.transform.Find("Canvas/Header/Text").GetComponent<Text>().text =
            $"{Localization.instance.Localize("$mpasn_itemsperroll")}" + ":\n<color=#00ff00>" +
            Gambler_DataTypes.SyncedGamblerData.Value[profile].MAXROLLS + "</color>";
        UI.SetActive(true);
    }
}