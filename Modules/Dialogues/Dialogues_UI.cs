using Marketplace.ExternalLoads;
using Marketplace.Modules.NPC;
using Random = System.Random;

namespace Marketplace.Modules.Dialogues;

public static class Dialogues_UI
{
    private static GameObject UI = null!;
    private static GameObject Dialogue_Element = null!;
    private static readonly List<GameObject> Elements = new();
    private static Text NPC_Name = null!;
    private static Text Dialogue_Text = null!;
    private static Image BG_Image = null!;
    private static Transform Content = null!;


    private static CanvasGroup CanvasAlpha = null!;
    private static readonly Dictionary<int, Action> HotbarActions = new();

    public static bool IsVisible()
    {
        return UI && UI.activeSelf;
    }

    private static void ResetFitters()
    {
        Canvas.ForceUpdateCanvases();
        foreach (ContentSizeFitter fitter in UI.GetComponentsInChildren<ContentSizeFitter>())
        {
            fitter.enabled = false;
            fitter.enabled = true;
        }
    }
    
    public static void Init()
    {
        UI = UnityEngine.Object.Instantiate(AssetStorage.asset.LoadAsset<GameObject>("NPCDialogueUI"));
        CanvasAlpha = UI.GetComponent<CanvasGroup>();
        Dialogue_Element = AssetStorage.asset.LoadAsset<GameObject>("Dialogue_Option");
        UnityEngine.Object.DontDestroyOnLoad(UI);
        UI.SetActive(false);

        NPC_Name = UI.transform.Find("GO/NPC_Name").GetComponent<Text>();
        Dialogue_Text = UI.transform.Find("GO/Dialogue_Text").GetComponent<Text>();
        Content = UI.transform.Find("GO");
        BG_Image = UI.transform.Find("GO/BACKGROUND").GetComponent<Image>();
        Default();
        Marketplace.Global_Updator += PressHotbarUpdate;
    }

    private static readonly KeyCode[] hotbarKeys =
    {
        KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3, KeyCode.Alpha4, KeyCode.Alpha5, KeyCode.Alpha6,
        KeyCode.Alpha7, KeyCode.Alpha8, KeyCode.Alpha9, KeyCode.Alpha0
    };

    private static void PressHotbarUpdate(float dt)
    {
        if (!IsVisible() || !Player.m_localPlayer) return;
        for (int i = 0; i < hotbarKeys.Length; ++i)
        {
            if (!Input.GetKeyDown(hotbarKeys[i])) continue;
            if (HotbarActions.TryGetValue(i + 1, out Action action))
            {
                action();
            }
        }
    }

    private static void Default()
    {
        foreach (GameObject element in Elements)
        {
            UnityEngine.Object.Destroy(element);
        }

        Elements.Clear();
        NPC_Name.text = "";
        Dialogue_Text.text = "";
        HotbarActions.Clear();
    }


    private static Coroutine FadeCoroutine;

    private enum Fade
    {
        Show,
        Hide
    }

    private static void SmoothAlpha(Fade type, float time)
    {
        if (FadeCoroutine != null)
            Marketplace._thistype.StopCoroutine(FadeCoroutine);
        FadeCoroutine = Marketplace._thistype.StartCoroutine(SmoothAlphaCoroutine(type, time));
    }

    private static IEnumerator SmoothAlphaCoroutine(Fade type, float time)
    {
        float start = type == Fade.Show ? 0 : 1;
        CanvasAlpha.alpha = start;
        float target = type == Fade.Show ? 1 : 0;
        float counter = 0;
        const float exponent = 2f;
        while (counter <= 1f)
        {
            if (!UI) yield break;
            counter += Time.unscaledDeltaTime / time;
            float easedCounter = counter < 1 ? 1 - Mathf.Pow(1 - counter, exponent) : 1;
            CanvasAlpha.alpha = Mathf.Clamp01(Mathf.Lerp(start, target, easedCounter));
            yield return null;
        }
    }

    public static Dictionary<string, Func<string>> _replacements = new()
    {
        {"%playername%", () => Player.m_localPlayer?.GetPlayerName() ?? "Player"}
    };
     
    public static void LoadDialogue(Market_NPC.NPCcomponent npc, string UID)
    {
        Default();
        if (!Dialogues_DataTypes.ClientReadyDialogues.TryGetValue(UID, out Dialogues_DataTypes.Dialogue dialogue))
        {
            Hide(true); 
            return;
        }

        foreach (var interaction in dialogue.Interactions)
        {
            if (interaction.Type is not Dialogues_DataTypes.RawDialogue.InteractionType.Interaction) continue;
            if (!interaction.CheckCondition(npc, out _)) continue;
            interaction.CallCommands(npc);
        }

        if (dialogue.Interactions.Count(i => i.Type is Dialogues_DataTypes.RawDialogue.InteractionType.Option) == 0) return;
        
 
        UI.SetActive(true);
        SmoothAlpha(Fade.Show, 0.5f);

        string name = "";
        if (npc)
        {
            name = npc.GetClearNPCName();
            if (string.IsNullOrWhiteSpace(name))
            {
                name = Localization.instance.Localize("$mpasn_" + npc._currentNpcType);
            }
        }

        NPC_Name.text = name;
        Dialogue_Text.text = "\"" + Localization.instance.Localize(dialogue.Text) + "\"";
        foreach (KeyValuePair<string, Func<string>> replacement in _replacements)
            Dialogue_Text.text = Dialogue_Text.text.Replace(replacement.Key, replacement.Value());
        if (dialogue.BG_Image != null)
        {
            BG_Image.sprite = dialogue.BG_Image;
            BG_Image.gameObject.SetActive(true);
        }
        else
        {
            BG_Image.gameObject.SetActive(false);
        }
        int c = 0;
        foreach (Dialogues_DataTypes.Dialogue.PlayerInteraction option in dialogue.Interactions)
        {
            if (option.Type is not Dialogues_DataTypes.RawDialogue.InteractionType.Option) continue;
            bool alwaysVisibleCheck = false;
            if (!option.CheckCondition(npc, out string reason))
            {
                if (option.AlwaysVisible)
                    alwaysVisibleCheck = true;
                else
                    continue;
            }
 
            GameObject element = UnityEngine.Object.Instantiate(Dialogue_Element, Content);
            Elements.Add(element);
            string optionText = option.Text;
            foreach (KeyValuePair<string, Func<string>> replacement in _replacements)
                optionText = optionText.Replace(replacement.Key, replacement.Value());
            element.transform.Find("Text").GetComponent<Text>().text = Localization.instance.Localize(optionText);
            element.transform.Find("Indexer/Text").GetComponent<Text>().text = (++c).ToString();
            if (option.Icon != null)
                element.transform.Find("Text/Icon").GetComponent<Image>().sprite = option.Icon;
            element.transform.Find("line").GetComponent<Image>().color = option.Color;
            element.transform.Find("Indexer/bg").GetComponent<Image>().color = option.Color;

            if (alwaysVisibleCheck) 
            {
                UnityEngine.Object.Destroy(element.GetComponent<Button>());
                element.transform.Find("line").GetComponent<Image>().color = Color.red;
                element.transform.Find("Indexer/bg").GetComponent<Image>().color = Color.red;
                if (!string.IsNullOrWhiteSpace(reason))
                    element.transform.Find("Text").GetComponent<Text>().text +=
                        $"\n<color=red>[</color>{reason}<color=red>]</color>".Localize();
                continue;
            }

            void OnClick()
            {  
                AssetStorage.AUsrc.Play();
                if (!Player.m_localPlayer) return;
                if (!option.CheckCondition(npc, out string reason))
                {
                    if (!string.IsNullOrWhiteSpace(reason))
                        MessageHud.instance.ShowMessage(MessageHud.MessageType.Center,
                            $"<color=red>[</color>{reason}<color=red>]</color>");
                    return; 
                } 
                option.CallCommands(npc);
                if (option.NextTransition is {} transition) LoadDialogue(npc, transition);
                else Hide(true);
            }
 
            HotbarActions[c] = OnClick;
            element.GetComponent<Button>().onClick.AddListener(OnClick);
        }
  
        ResetFitters();
    }

    private static IEnumerator HideCoroutine()
    {
        yield return null;
        Hide();
    }

    public static void Hide(bool nextFrame = false)
    {
        if (!IsVisible()) return;
        if (nextFrame)
        {
            Marketplace._thistype.StartCoroutine(HideCoroutine());
            return;
        }

        UI.SetActive(false);
    }

    [HarmonyPatch(typeof(TextInput), nameof(TextInput.IsVisible))]
    [ClientOnlyPatch]
    private static class BankerUIFix
    {
        [UsedImplicitly]
        private static void Postfix(ref bool __result)
        {
            if (IsVisible()) __result = true;
        }
    }

    [HarmonyPatch(typeof(Player), nameof(Player.OnDeath))]
    [ClientOnlyPatch]
    private static class Player_OnDeath_Patch
    {
        [UsedImplicitly]
        private static void Prefix()
        {
            Hide();
        }
    }
}