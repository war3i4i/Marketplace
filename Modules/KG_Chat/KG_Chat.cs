using System.Diagnostics;
using System.Reflection.Emit;
using System.Text.RegularExpressions;
using BepInEx.Configuration;
using GUIFramework;
using Marketplace.ExternalLoads;
using Marketplace.Modules.Global_Options;
using Splatform;
using UnityEngine.EventSystems;
using Object = UnityEngine.Object;

namespace Marketplace.Modules.KG_Chat;

[UsedImplicitly]
[Market_Autoload(Market_Autoload.Type.Client)]
public static class KG_Chat 
{
    private static GameObject original_KG_Chat = null!;
    private static readonly GameObject[] origStuff = new GameObject[3];
    private static TMP_FontAsset origFont = null!;
    private static TMP_FontAsset origFont2 = null!;
    private static ConfigEntry<int> kgchat_Fontsize = null!;
    private static ConfigEntry<bool> useTypeSound = null!;
    private static ConfigEntry<ChatController.Transparency> kgchat_Transparency = null!;
    private static Chat kgChat = null!;
    private static Scrollbar kgChat_Scrollbar = null!;
    private static readonly List<ContentSizeFitter> fitters = new();

    [Flags]
    public enum ChatFilter
    {
        None = 0,
        Normal = 1,
        Shout = 2,
        Whisper = 4,
        All = Normal | Shout | Whisper
    }

    public static bool HasFlagFast(this ChatFilter flags, ChatFilter flag) => (flags & flag) != 0;

    private static ConfigEntry<bool> HideFloatingText = null!;
    private static ConfigEntry<ChatFilter> ChatFilterMode = null!;

    [UsedImplicitly]
    private static void OnInit() 
    {
        kgchat_Fontsize = Marketplace._thistype.Config.Bind("KG Chat", "Font Size", 18, "KG Chat Font Size");
        useTypeSound = Marketplace._thistype.Config.Bind("KG Chat", "Use Type Sound", false, "Use KG Chat Type Sound");
        HideFloatingText =
            Marketplace._thistype.Config.Bind("KG Chat", "Hide Floating Text", true, "Hide Floating Text");
        ChatFilterMode = Marketplace._thistype.Config.Bind("KG Chat", "Chat Filter", ChatFilter.All, "Chat Filter");
        kgchat_Transparency = Marketplace._thistype.Config.Bind("KG Chat", "Transparency",
            ChatController.Transparency.Two, "KG Chat Transparency");
        original_KG_Chat = AssetStorage.asset.LoadAsset<GameObject>("Marketplace_KGChat_SelfCanvas");
        original_KG_Chat.GetComponent<Canvas>().sortingOrder = 599;

        Global_Configs.SyncedGlobalOptions.ValueChanged += ApplyKGChat;

        string spritesheetPath_Original =
            Path.Combine(BepInEx.Paths.ConfigPath, "Marketplace_KGChat_Emojis", "spritesheet_original.png");
        string spritesheetPath_New =
            Path.Combine(BepInEx.Paths.ConfigPath, "Marketplace_KGChat_Emojis", "spritesheet.png");
        if (!Directory.Exists(Path.GetDirectoryName(spritesheetPath_Original)))
            Directory.CreateDirectory(Path.GetDirectoryName(spritesheetPath_Original)!);

        if (!File.Exists(spritesheetPath_Original))
        {
            Texture2D tex = (Texture2D)original_KG_Chat.GetComponentInChildren<TextMeshProUGUI>(true).spriteAsset
                .spriteSheet;
            byte[] bytes = tex.EncodeToPNG();
            File.WriteAllBytes(spritesheetPath_Original, bytes);
        }

        if (File.Exists(spritesheetPath_New))
        {
            Texture2D tex = new Texture2D(2, 2);
            tex.LoadImage(File.ReadAllBytes(spritesheetPath_New));
            foreach (TextMeshProUGUI ugui in original_KG_Chat.GetComponentsInChildren<TextMeshProUGUI>(true))
            {
                ugui.spriteAsset.spriteSheet = tex;
                ugui.spriteAsset.material.SetTexture(ShaderUtilities.ID_MainTex, tex);
            }
        }

        Marketplace.Global_FixedUpdator += KGChat_Update;
    }


    private static Coroutine _corout;

    private static void ResetScroll()
    {
        if (_corout != null) Marketplace._thistype.StopCoroutine(_corout);
        _corout = Marketplace._thistype.StartCoroutine(corout_ResetScroll());
    }

    private static IEnumerator corout_ResetScroll()
    {
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        if (kgChat_Scrollbar)
            kgChat_Scrollbar.value = 0f;
    }

    public class ResizeUI : MonoBehaviour, IDragHandler, IEndDragHandler
    {
        private static RectTransform dragRect = null!;
        private static TextMeshProUGUI text = null!;
        private static ConfigEntry<float> UI_X = null!;
        private static ConfigEntry<float> UI_Y = null!;
        public Vector3 Scale => new(dragRect.localScale.x, dragRect.localScale.y, 1f);

        public static void Default()
        {
            if (!dragRect) return;
            UI_X.Value = (float)UI_X.DefaultValue;
            UI_Y.Value = (float)UI_Y.DefaultValue;
            Marketplace._thistype.Config.Save();
            dragRect.localScale = new Vector3(UI_X.Value, UI_Y.Value, 1f);
            text.fontSize = kgchat_Fontsize.Value;
        }

        public void Setup()
        {
            Transform parent = transform.parent.parent;
            text = parent.Find("Tabs Content/MainTab/Scroll Rect/Viewport/Content/Text")
                .GetComponent<TextMeshProUGUI>();
            dragRect = parent.parent.GetComponent<RectTransform>();
            UI_X = Marketplace._thistype.Config.Bind("KG Chat", "UI_sizeX", 1f, "UI X size");
            UI_Y = Marketplace._thistype.Config.Bind("KG Chat", "UI_sizeY", 1f, "UI Y size");
            dragRect.localScale = new Vector3(UI_X.Value, UI_Y.Value, 1f);
            text.fontSize = (int)(kgchat_Fontsize.Value + kgchat_Fontsize.Value * (1f - UI_X.Value));
        }

        public void OnDrag(PointerEventData eventData)
        {
            Vector2 vec = -eventData.delta;
            Vector3 localScale = dragRect.localScale;
            Vector2 sizeDelta = dragRect.sizeDelta + new Vector2(34f * localScale.x, 0f);
            vec.x /= sizeDelta.x;
            Vector3 resized = localScale + new Vector3(vec.x, vec.x, 0);
            resized.x = Mathf.Clamp(resized.x, 0.5f, 1.5f);
            resized.y = Mathf.Clamp(resized.y, 0.5f, 1.5f);
            resized.z = 1f;
            localScale = resized; 
            dragRect.localScale = localScale;
            text.fontSize = (int)(kgchat_Fontsize.Value + 16 * Mathf.Abs(1f - resized.x));
        }

        public void OnEndDrag(PointerEventData data)
        {
            Vector3 localScale = dragRect.localScale;
            UI_X.Value = localScale.x;
            UI_Y.Value = localScale.y;
            Marketplace._thistype.Config.Save();
            Chat.instance.m_input.MoveTextEnd(false);
        }
    }

    public class DragUI : MonoBehaviour, IDragHandler, IEndDragHandler
    {
        private static RectTransform dragRect = null!;
        private static ConfigEntry<float> UI_X = null!;
        private static ConfigEntry<float> UI_Y = null!;
        private readonly Transform[] Markers = new Transform[4];

        public static void Default()
        {
            if (!dragRect) return;
            UI_X.Value = (float)UI_X.DefaultValue;
            UI_Y.Value = (float)UI_Y.DefaultValue;
            Marketplace._thistype.Config.Save();
            dragRect.anchoredPosition = new Vector2(UI_X.Value, UI_Y.Value);
        }

        public void Setup()
        {
            UI_X = Marketplace._thistype.Config.Bind("KG Chat", "UI_posX", -15f, "UI X position");
            UI_Y = Marketplace._thistype.Config.Bind("KG Chat", "UI_posY", 60f, "UI Y position");
            dragRect = transform.parent.parent.parent.GetComponent<RectTransform>();
            Vector2 configPos = new Vector2(UI_X.Value, UI_Y.Value);
            dragRect.anchoredPosition = configPos;
            Markers[0] = dragRect.transform.Find("LeftTop");
            Markers[1] = dragRect.transform.Find("RightTop");
            Markers[2] = dragRect.transform.Find("LeftBottom");
            Markers[3] = dragRect.transform.Find("RightBottom");

            if (CheckMarkersOutsideScreen(new Vector2(Screen.width, Screen.height)))
                Default();
        }

        public void OnDrag(PointerEventData eventData)
        {
            Vector2 anchoredPosition = dragRect.anchoredPosition;
            Vector2 vec = anchoredPosition + eventData.delta / dragRect.lossyScale * dragRect.localScale;
            Vector2 lastPos = anchoredPosition;
            anchoredPosition = vec;
            dragRect.anchoredPosition = anchoredPosition;
            if (CheckMarkersOutsideScreen(new Vector2(Screen.width, Screen.height)))
                dragRect.anchoredPosition = lastPos;
        }

        private bool CheckMarkersOutsideScreen(Vector2 screen)
        {
            foreach (Transform marker in Markers)
            {
                Vector3 position = marker.position;
                float markerX = position.x;
                float markerY = position.y;
                if (markerX < 0 || markerX > screen.x || markerY < 0 || markerY > screen.y)
                    return true;
            }

            return false;
        }

        public void OnEndDrag(PointerEventData data)
        {
            Vector2 anchoredPosition = dragRect.anchoredPosition;
            UI_X.Value = anchoredPosition.x;
            UI_Y.Value = anchoredPosition.y;
            Marketplace._thistype.Config.Save();
            Chat.instance.m_input.MoveTextEnd(false);
        }
    }

    [HarmonyPatch(typeof(Player), nameof(Player.OnSpawned))]
    [ClientOnlyPatch]
    private static class ZNetScene_Awake_Patch
    {
        [UsedImplicitly]
        private static void Postfix() => ApplyKGChat();
    }

    private static void ApplyKGChat()
    {
        if (!Global_Configs.SyncedGlobalOptions.Value._enableKGChat || kgChat || !Chat.instance) return;
        Utils.print($"Switching to KG Chat", ConsoleColor.Cyan);
        ZRoutedRpc.instance.m_functions.Remove("ChatMessage".GetStableHashCode());
        ZRoutedRpc.instance.m_functions.Remove("RPC_TeleportPlayer".GetStableHashCode());
        Transform parent = Chat.instance.transform.parent;
        Chat.instance.m_chatWindow.gameObject.SetActive(false); 
        Chat.instance.m_input.gameObject.SetActive(false);
        Chat.instance.m_output.gameObject.SetActive(false); 
        Object.DestroyImmediate(Chat.instance);
        kgChat = Object.Instantiate(original_KG_Chat, parent).transform.Find("KGChat").GetComponent<Chat>();
        fitters.Clear();
        fitters.AddRange(kgChat.GetComponentsInChildren<ContentSizeFitter>(true));
        kgChat.gameObject.AddComponent<ChatController>().Setup(); 
        kgChat.transform.Find("CHATWINDOW/Input Field/Resize").gameObject.AddComponent<ResizeUI>().Setup();
        kgChat.transform.Find("CHATWINDOW/Input Field/Move").gameObject.AddComponent<DragUI>().Setup();
        kgChat.transform.Find("CHATWINDOW/Input Field/Reset").gameObject.GetComponent<Button>().onClick
            .AddListener(
                () =>
                { 
                    ResizeUI.Default();
                    DragUI.Default();
                    AssetStorage.AUsrc.Play();
                });
        kgChat.GetComponentInChildren<GuiInputField>(true).onValueChanged.AddListener(IF_OnValueChanged);
        kgChat_Scrollbar = kgChat.GetComponentInChildren<Scrollbar>(true);
        Chat.instance.AddString("<color=green>KG Chat Loaded</color>");
        Chat.instance.AddString("<color=green>/say | /shout | /whisper to switch chat mode</color>");
        if (Groups.API.IsLoaded())
            Chat.instance.AddString("<color=green>/group | /party to switch to groups chat mode</color>");

        Chat.instance.m_input.characterLimit = 128;
    }

    private static void KGChat_Update(float dt)
    {
        if (!kgChat || !Chat.instance.m_input.IsActive()) return;
        Chat.instance.m_input.ActivateInputField();
    }

    private static void IF_OnValueChanged(string value)
    {
        if (useTypeSound.Value && !string.IsNullOrEmpty(value))
            AssetStorage.AUsrc.PlayOneShot(AssetStorage.TypeClip,
                UnityEngine.Random.Range(0.65f, 0.75f));
        switch (value)
        {
            case "/shout":
                kgChat.m_input.text = "";
                ChatController.Instance.ButtonClick(ChatController.SendMode.Shout);
                break; 
            case "/say":
                kgChat.m_input.text = "";
                ChatController.Instance.ButtonClick(ChatController.SendMode.Say);
                break;
            case "/group" or "/party" when Groups.API.IsLoaded():
                kgChat.m_input.text = "";
                ChatController.Instance.ButtonClick(ChatController.SendMode.Group); 
                break; 
            case "/whisper": 
                kgChat.m_input.text = "";
                ChatController.Instance.ButtonClick(ChatController.SendMode.Whisper);
                break;
        }
    }

    [HarmonyPatch(typeof(Chat), nameof(Chat.Awake))]
    [ClientOnlyPatch]
    private static class Chat_Awake_Patch
    {
        private static bool isKGChat(GameObject go) => go.name == "KGChat";

        [UsedImplicitly]
        private static void Prefix(Chat __instance)
        {
            if (!isKGChat(__instance.gameObject))
            {
                origStuff[0] = __instance.m_worldTextBase;
                origStuff[1] = __instance.m_npcTextBase;
                origStuff[2] = __instance.m_npcTextBaseLarge;
                origFont = __instance.m_output.font;
                origFont2 = __instance.m_input.textComponent.font;
            }
            else
            {
                __instance.m_worldTextBase = origStuff[0];
                __instance.m_npcTextBase = origStuff[1];
                __instance.m_npcTextBaseLarge = origStuff[2];
                __instance.m_output.font = origFont;
                __instance.m_input.placeholder.GetComponent<TMP_Text>().font = origFont2;
                __instance.m_input.textComponent.font = origFont2;
                __instance.m_input.fontAsset = origFont2; 
                __instance.m_input.gameObject.SetActive(false);
                __instance.m_input.OnInputSubmit.AddListener((_) => __instance.SendInput());
            }
        }
    }

    [HarmonyPatch(typeof(TextInput), nameof(TextInput.IsVisible))]
    [ClientOnlyPatch]
    private static class Menu_Patch
    {
        [UsedImplicitly]
        private static void Postfix(ref bool __result)
        {
            if (!kgChat) return;
            __result |= Chat.instance.m_input.gameObject.activeInHierarchy;
        }
    }

    [HarmonyPatch(typeof(Chat), nameof(Chat.Update))]
    [ClientOnlyPatch] 
    private static class Chat_Patches3
    {
        [HarmonyTranspiler]
        [UsedImplicitly]
        private static IEnumerable<CodeInstruction> Code(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> list = new List<CodeInstruction>(instructions);
            for (int i = 0; i < list.Count; ++i)
            {
                if (list[i].opcode == OpCodes.Ldc_I4 && ((int)list[i].operand == 323 || (int)list[i].operand == 324))
                {
                    list[i].opcode = OpCodes.Nop;
                    list[i + 1].opcode = OpCodes.Nop;
                    list[i + 2].opcode = OpCodes.Nop;
                    list[i + 3].opcode = OpCodes.Nop;
                }
            }

            return list;
        }
    }

    [HarmonyPatch(typeof(Terminal), nameof(Terminal.AddString), typeof(PlatformUserID), typeof(string), typeof(Talker.Type), typeof(bool))]
    [ClientOnlyPatch]
    private static class Chat_Patches2
    {
        [HarmonyTranspiler]
        [UsedImplicitly]
        private static IEnumerable<CodeInstruction> Code(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> list = new List<CodeInstruction>(instructions); 
            MethodInfo methodInfo = AccessTools.DeclaredMethod(typeof(string), "ToUpper", Type.EmptyTypes);
            MethodInfo methodInfo2 = AccessTools.DeclaredMethod(typeof(string), "ToLowerInvariant", Type.EmptyTypes);
            for (int i = 0; i < list.Count; ++i)
            {
                if (list[i].opcode == OpCodes.Callvirt &&
                    (ReferenceEquals(list[i].operand, methodInfo) || ReferenceEquals(list[i].operand, methodInfo2)))
                {
                    list[i - 1].opcode = OpCodes.Nop; 
                    list[i].opcode = OpCodes.Nop;
                    list[i + 1].opcode = OpCodes.Nop;
                }
            }
            return list;
        }
    }

    [HarmonyPatch(typeof(Terminal), nameof(Terminal.UpdateChat))]
    [ClientOnlyPatch]
    private static class Terminal_UpdateChat_Patch
    {
        [UsedImplicitly]
        private static void Prefix(Terminal __instance)
        {
            if (__instance != kgChat) return;
            __instance.m_scrollHeight = 0;
            __instance.m_maxVisibleBufferLength = 45;
        }

        [UsedImplicitly]
        private static void Postfix(Terminal __instance)
        {
            if (__instance == kgChat && !__instance.m_input.isFocused)
            {
                ResetScroll();
            }
        }
    }

    private static readonly Dictionary<string, string> Emoji_Map = new()
    {
        { ":moji0:", "<sprite=0>" },
        { ":moji1:", "<sprite=1>" },
        { ":moji2:", "<sprite=2>" },
        { ":moji3:", "<sprite=3>" },
        { ":moji4:", "<sprite=4>" },
        { ":moji5:", "<sprite=5>" },
        { ":moji6:", "<sprite=6>" },
        { ":moji7:", "<sprite=7>" },
        { ":moji8:", "<sprite=8>" },
        { ":moji9:", "<sprite=9>" },
        { ":moji10:", "<sprite=10>" },
        { ":moji11:", "<sprite=11>" },
        { ":moji12:", "<sprite=12>" },
        { ":moji13:", "<sprite=13>" },
        { ":moji14:", "<sprite=14>" },
        { ":moji15:", "<sprite=15>" },
        { ":moji16:", "<sprite=16>" },
        { ":moji17:", "<sprite=17>" },
        { ":moji18:", "<sprite=18>" },
        { ":moji19:", "<sprite=19>" },
        { ":moji20:", "<sprite=20>" },
        { ":moji21:", "<sprite=21>" },
        { ":moji22:", "<sprite=22>" },
        { ":moji23:", "<sprite=23>" },
        { ":moji24:", "<sprite=24>" },
    };

    public class ChatController : MonoBehaviour
    {
        public static ChatController Instance = null!;
        public SendMode mode;
        private Transform Emojis_Tab = null!;

        public void Setup()
        {
            Instance = this;
            mode = SendMode.Say;
            Button _sayButton = transform.Find("CHATWINDOW/Input Field/Say").GetComponent<Button>();
            Button _shoutButton = transform.Find("CHATWINDOW/Input Field/Shout").GetComponent<Button>();
            Button _whisperButton = transform.Find("CHATWINDOW/Input Field/Whisper").GetComponent<Button>();
            Button _groupButton = transform.Find("CHATWINDOW/Input Field/Groups").GetComponent<Button>();
            Button _muteSoundsButton = transform.Find("CHATWINDOW/Input Field/MuteSounds").GetComponent<Button>();
            _sayButton.onClick.AddListener(delegate { ButtonClick(SendMode.Say); });
            _shoutButton.onClick.AddListener(delegate { ButtonClick(SendMode.Shout); });
            _whisperButton.onClick.AddListener(delegate { ButtonClick(SendMode.Whisper); });
            _groupButton.onClick.AddListener(delegate { ButtonClick(SendMode.Group); });
            SayImage = _sayButton.gameObject.transform.GetChild(0).GetComponent<Image>();
            ShoutImage = _shoutButton.gameObject.transform.GetChild(0).GetComponent<Image>();
            WhisperImage = _whisperButton.gameObject.transform.GetChild(0).GetComponent<Image>();
            GroupImage = _groupButton.gameObject.transform.GetChild(0).GetComponent<Image>();
            SayImage.color = Color.green;
            if (!Groups.API.IsLoaded())
            {
                _groupButton.gameObject.SetActive(false);
            }
 
            Emojis_Tab = transform.Find("CHATWINDOW/Input Field/Emoji_Tab");
              
            transform.Find("CHATWINDOW/Input Field").GetComponent<TMP_InputField>().textViewport = transform.Find("CHATWINDOW/Input Field") as RectTransform;
            
            FillEmojis();
            transform.Find("CHATWINDOW/Input Field/Emojis").GetComponent<Button>().onClick.AddListener(() =>
            {
                Emojis_Tab.gameObject.SetActive(!Emojis_Tab.gameObject.activeSelf);
                AssetStorage.AUsrc.Play();
            });
            _muteSoundsButton.transform.Find("TF").gameObject.SetActive(!useTypeSound.Value);
            _muteSoundsButton.onClick.AddListener(() =>
            {
                AssetStorage.AUsrc.Play();
                useTypeSound.Value = !useTypeSound.Value;
                Marketplace._thistype.Config.Save();
                _muteSoundsButton.transform.Find("TF").gameObject.SetActive(!useTypeSound.Value);
            });

            Transform bgone = transform.Find("CHATWINDOW/Background/Upper Background");
            Transform bgtwo = transform.Find("CHATWINDOW/Input Field/Lower Background");
            bgone.GetComponent<Image>().color = new Color(0, 0, 0, Transparency_Map[kgchat_Transparency.Value]);
            bgtwo.GetComponent<Image>().color = new Color(0, 0, 0, Transparency_Map[kgchat_Transparency.Value]);

            Button _transparencyButton = transform.Find("CHATWINDOW/Input Field/Transparency").GetComponent<Button>();
            _transparencyButton.onClick.AddListener(() =>
            {
                AssetStorage.AUsrc.Play();
                kgchat_Transparency.Value = (Transparency)(((int)kgchat_Transparency.Value + 1) % 6);
                bgone.GetComponent<Image>().color = new Color(0, 0, 0, Transparency_Map[kgchat_Transparency.Value]);
                bgtwo.GetComponent<Image>().color = new Color(0, 0, 0, Transparency_Map[kgchat_Transparency.Value]);
                MessageHud.instance.ShowMessage(MessageHud.MessageType.Center,
                    $"<color=green>{Transparency_Map[kgchat_Transparency.Value] * 100f}%</color>");
                Marketplace._thistype.Config.Save();
            });
        }

        public enum Transparency
        {
            None,
            One,
            Two,
            Three,
            Four,
            Five
        }

        private static readonly Dictionary<Transparency, float> Transparency_Map = new()
        {
            { Transparency.None, 0f },
            { Transparency.One, 0.2f },
            { Transparency.Two, 0.4f },
            { Transparency.Three, 0.6f },
            { Transparency.Four, 0.8f },
            { Transparency.Five, 0.95f }
        };


        private void FillEmojis()
        {
            Transform button = Emojis_Tab.Find("Emoji");
            foreach (KeyValuePair<string, string> em in Emoji_Map)
            {
                Button newButton = Instantiate(button, Emojis_Tab).GetComponent<Button>();
                newButton.gameObject.SetActive(true);
                newButton.transform.Find("text").GetComponent<TextMeshProUGUI>().text = em.Value;
                newButton.onClick.AddListener(delegate { EmojiClick(em.Key); });
            }
        }

        private void EmojiClick(string key)
        { 
            if (!Chat.instance) return;
            AssetStorage.AUsrc.Play();
            Chat.instance.m_input.text += key + " ";
            Chat.instance.m_input.MoveTextEnd(false);
        }

        public void ButtonClick(SendMode newMode)
        {
            mode = newMode;
            SayImage.color = mode == SendMode.Say ? Color.green : Color.white;
            ShoutImage.color = mode == SendMode.Shout ? Color.green : Color.white;
            WhisperImage.color = mode == SendMode.Whisper ? Color.green : Color.white;
            GroupImage.color = mode == SendMode.Group ? Color.green : Color.white;
            AssetStorage.AUsrc.Play();
        }

        private Image SayImage = null!;
        private Image ShoutImage = null!;
        private Image WhisperImage = null!;
        private Image GroupImage = null!;

        public enum SendMode
        {
            Say,
            Shout,
            Whisper,
            Group
        }
    }

    [HarmonyPatch(typeof(Chat), nameof(Chat.InputText))]
    [ClientOnlyPatch]
    private static class Chat_InputText_Patch
    {
        private static void ModifyInput(ref string text)
        {
            if (!kgChat) return;
            if (text.Length == 0 || text[0] == '/') return;
            text = ChatController.Instance.mode switch
            {
                ChatController.SendMode.Say => "/say " + text,
                ChatController.SendMode.Shout => "/s " + text,
                ChatController.SendMode.Whisper => "/w " + text,
                ChatController.SendMode.Group => "/p " + text,
                _ => "/say " + text
            };
        }

        [UsedImplicitly]
        private static void Postfix() => ResetScroll();

        [UsedImplicitly]
        private static bool Prefix() =>
            !Global_Configs.SyncedGlobalOptions.Value._blockedChatUsers.Contains(Global_Configs._localUserID);

        [HarmonyTranspiler]
        [UsedImplicitly]
        private static IEnumerable<CodeInstruction> Code(IEnumerable<CodeInstruction> instructions)
        {
            bool isdone = false;
            foreach (CodeInstruction instruction in instructions)
            {
                yield return instruction;
                if (instruction.opcode == OpCodes.Stloc_0 && !isdone)
                {
                    isdone = true;
                    yield return new CodeInstruction(OpCodes.Ldloca_S, 0);
                    yield return new CodeInstruction(OpCodes.Call,
                        AccessTools.Method(typeof(Chat_InputText_Patch), nameof(ModifyInput)));
                }
            }
        }
    }

    [HarmonyPatch]
    [ClientOnlyPatch]
    private static class EmojiPatch
    {
        private static FieldInfo textField = null!; 

        [UsedImplicitly]
        private static MethodInfo TargetMethod()
        {
            const string targetClass = "<>c__DisplayClass12_0";
            const string targetMethod = "<OnNewChatMessage>g__OnCheckPermissionAsyncCompleted|0";
            Type type = typeof(Chat).GetNestedTypes(BindingFlags.NonPublic).FirstOrDefault(t => t.Name == targetClass)!;
            textField = AccessTools.Field(type, "text");
            return AccessTools.Method(type, targetMethod);
        }

        private static void StringReplacer(object instance)
        {
            string str = (string)textField.GetValue(instance);
            str = Emoji_Map.Aggregate(str, (current, map) => current.Replace(map.Key, map.Value));
            textField.SetValue(instance, str);
        }

        [HarmonyTranspiler]
        [UsedImplicitly]
        private static IEnumerable<CodeInstruction> Code(IEnumerable<CodeInstruction> code)
        {
            FieldInfo targetField = AccessTools.Field(typeof(Chat), nameof(Chat.m_hideTimer));
            foreach (CodeInstruction instruction in code)
            {
                yield return instruction;
                if (instruction.opcode == OpCodes.Stfld && ReferenceEquals(instruction.operand, targetField))
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(EmojiPatch), nameof(StringReplacer)));
                }
            }
        }
    }

    [HarmonyPatch(typeof(Chat), nameof(Chat.AddInworldText))]
    [ClientOnlyPatch]
    private static class Chat_AddInworldText_Patch
    {
        [UsedImplicitly]
        private static bool Prefix(ref string text,  Talker.Type type)
        {
            if (!kgChat) return true;
            if (kgChat && HideFloatingText.Value && type is not Talker.Type.Ping) return false;
            text = Regex.Replace(text, @"<sprite=\d+>", "");
            return true;
        }
    }

    [HarmonyPatch(typeof(Chat), nameof(Chat.OnNewChatMessage))]
    [ClientOnlyPatch]
    private static class Chat_OnNewChatMessage_Patch
    {
        [UsedImplicitly]
        private static bool Prefix(Chat __instance, Talker.Type type)
        {
            if (!kgChat) return true;
            switch (type)
            {
                case Talker.Type.Normal when !ChatFilterMode.Value.HasFlagFast(ChatFilter.Normal):
                case Talker.Type.Shout when !ChatFilterMode.Value.HasFlagFast(ChatFilter.Shout):
                case Talker.Type.Whisper when !ChatFilterMode.Value.HasFlagFast(ChatFilter.Whisper):
                    return false;
            }
            return true;
        }
    } 

    [HarmonyPatch(typeof(Chat), nameof(Chat.isAllowedCommand))]
    [ClientOnlyPatch]
    private static class Chat_isAllowedCommand_Patch
    {
        [UsedImplicitly]
        private static void Postfix(ref bool __result) => __result |= (kgChat && Utils.IsDebug_Strict);
    }
    
    [HarmonyPatch(typeof(Terminal),nameof(Terminal.InitTerminal))]
    private static class Terminal_InitTerminal_Patch
    {
        [UsedImplicitly]
        private static void Postfix(Terminal __instance)
        {
            new Terminal.ConsoleCommand("chatfilter", "Add / Remove chat filter", (args) =>
            {
                string argument = args.Args[1];
                if (!Enum.TryParse(argument, true, out ChatFilter filter))
                    return;
                ChatFilterMode.Value ^= filter;
                Marketplace._thistype.Config.Save();
                Chat.instance.m_hideTimer = 0f;
                args.Context.AddString($"<color=green>Chat Filter: {ChatFilterMode.Value}</color>");
            });
        }
    }
}