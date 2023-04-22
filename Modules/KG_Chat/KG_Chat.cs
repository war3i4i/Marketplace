﻿using System.Reflection.Emit;
using System.Text.RegularExpressions;
using BepInEx.Configuration;
using UnityEngine.EventSystems;
using Object = UnityEngine.Object;

namespace Marketplace.Modules.KG_Chat;

[Market_Autoload(Market_Autoload.Type.Client, Market_Autoload.Priority.Last, "OnInit")]
public static class KG_Chat
{
    private static GameObject original_KG_Chat;
    private static readonly GameObject[] origStuff = new GameObject[3];
    private static ConfigEntry<int> kgchat_Fontsize;
    private static Chat kgChat;
    private static Scrollbar kgChat_Scrollbar;

    private static void OnInit()
    {
        kgchat_Fontsize = Marketplace._thistype.Config.Bind("KG Chat", "Font Size", 18, "KG Chat Font Size");
        original_KG_Chat = AssetStorage.AssetStorage.asset.LoadAsset<GameObject>("Marketplace_KGChat");
        original_KG_Chat.transform.Find("CHATWINDOW/Tabs Content/MainTab/Scroll Rect/Viewport/Content/Text")
            .GetComponent<TextMeshProUGUI>().fontSize = kgchat_Fontsize.Value;
        Global_Values._container.ValueChanged += ApplyKGChat;
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
        private static RectTransform dragRect;
        private static TextMeshProUGUI text;
        private static ConfigEntry<float> UI_X;
        private static ConfigEntry<float> UI_Y;
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
            text = transform.parent.parent.Find("Tabs Content/MainTab/Scroll Rect/Viewport/Content/Text")
                .GetComponent<TextMeshProUGUI>();
            dragRect = transform.parent.parent.parent.GetComponent<RectTransform>();
            UI_X = Marketplace._thistype.Config.Bind("KG Chat", "UI_sizeX", 1f, "UI X size");
            UI_Y = Marketplace._thistype.Config.Bind("KG Chat", "UI_sizeY", 1f, "UI Y size");
            dragRect.localScale = new Vector3(UI_X.Value, UI_Y.Value, 1f);
            text.fontSize = (int)(kgchat_Fontsize.Value + kgchat_Fontsize.Value * (1f - UI_X.Value));
        }

        public void OnDrag(PointerEventData eventData)
        {
            var vec = -eventData.delta;
            var sizeDelta = dragRect.sizeDelta + new Vector2(34f * dragRect.localScale.x, 0f);
            vec.x /= sizeDelta.x;
            var resized = dragRect.localScale + new Vector3(vec.x, vec.x, 0);
            resized.x = Mathf.Clamp(resized.x, 0.75f, 1.25f);
            resized.y = Mathf.Clamp(resized.y, 0.75f, 1.25f); 
            resized.z = 1f;
            dragRect.localScale = resized;
            text.fontSize = (int)(kgchat_Fontsize.Value + 16 * Mathf.Abs(1f - resized.x));
        }

        public void OnEndDrag(PointerEventData data)
        {
            UI_X.Value = dragRect.localScale.x;
            UI_Y.Value = dragRect.localScale.y;
            Marketplace._thistype.Config.Save();
        }
    }

    public class DragUI : MonoBehaviour, IDragHandler, IEndDragHandler
    {
        private static RectTransform dragRect;
        private static ConfigEntry<float> UI_X;
        private static ConfigEntry<float> UI_Y;

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
            UI_X = Marketplace._thistype.Config.Bind("KG Chat", "UI_posX", 1560f, "UI X position");
            UI_Y = Marketplace._thistype.Config.Bind("KG Chat", "UI_posY", 60f, "UI Y position");
            dragRect = transform.parent.parent.parent.GetComponent<RectTransform>();
            var configPos = new Vector2(UI_X.Value, UI_Y.Value);
            dragRect.anchoredPosition = configPos;
        }

        public void OnDrag(PointerEventData eventData)
        {
            var vec = dragRect.anchoredPosition + eventData.delta;
            Vector2 sizeDelta = dragRect.sizeDelta * dragRect.localScale + new Vector2(34f * dragRect.localScale.x, 0f);
            var ScreenSize = new Vector2(Screen.width, Screen.height);
            if (vec.x < sizeDelta.x / 2f)
            {
                vec.x = sizeDelta.x / 2f;
            }

            if (vec.y < 0)
            {
                vec.y = 0;
            }

            if (vec.x > ScreenSize.x - sizeDelta.x / 2f)
            {
                vec.x = ScreenSize.x - sizeDelta.x / 2f;
            }

            if (vec.y > ScreenSize.y - sizeDelta.y)
            {
                vec.y = ScreenSize.y - sizeDelta.y;
            }

            dragRect.anchoredPosition = vec;
        }

        public void OnEndDrag(PointerEventData data)
        {
            UI_X.Value = dragRect.anchoredPosition.x;
            UI_Y.Value = dragRect.anchoredPosition.y;
            Marketplace._thistype.Config.Save();
        }
    }

    private static void ApplyKGChat()
    {
        if (!Global_Values._container.Value._enableKGChat || kgChat) return;
        Utils.print($"Switching to KG Chat", ConsoleColor.Cyan);
        ZRoutedRpc.instance.m_functions.Remove("ChatMessage".GetStableHashCode());
        ZRoutedRpc.instance.m_functions.Remove("RPC_TeleportPlayer".GetStableHashCode());
        Transform parent = Chat.instance.transform.parent;
        Object.DestroyImmediate(Chat.instance);
        kgChat = Object.Instantiate(original_KG_Chat, parent).GetComponent<Chat>();
        kgChat.gameObject.AddComponent<ModeController>().Setup();
        kgChat.transform.Find("CHATWINDOW/Input Field/Resize").gameObject.AddComponent<ResizeUI>().Setup();
        kgChat.transform.Find("CHATWINDOW/Input Field/Move").gameObject.AddComponent<DragUI>().Setup();
        kgChat.transform.Find("CHATWINDOW/Input Field/Reset").gameObject.GetComponent<Button>().onClick
            .AddListener(
                () =>
                {
                    DragUI.Default();
                    ResizeUI.Default();
                    AssetStorage.AssetStorage.AUsrc.Play();
                });
        kgChat.GetComponentInChildren<InputField>(true).onValueChanged.AddListener(IF_OnValueChanged);
        kgChat_Scrollbar = kgChat.GetComponentInChildren<Scrollbar>(true);
    }

    private static void IF_OnValueChanged(string value)
    {
        switch (value)
        {
            case "/shout":
                kgChat.m_input.text = "";
                ModeController.Instance.ButtonClick(ModeController.SendMode.Shout);
                break;
            case "/say":
                kgChat.m_input.text = "";
                ModeController.Instance.ButtonClick(ModeController.SendMode.Say);
                break;
            case "/group" or "/party" when Groups.API.IsLoaded():
                kgChat.m_input.text = "";
                ModeController.Instance.ButtonClick(ModeController.SendMode.Group);
                break;
            case "/whisper":
                kgChat.m_input.text = "";
                ModeController.Instance.ButtonClick(ModeController.SendMode.Whisper);
                break;
        }
    }

    [HarmonyPatch(typeof(Chat), nameof(Chat.Awake))]
    [ClientOnlyPatch]
    private static class Chat_Awake_Patch
    {
        private static bool isKGChat(GameObject go) => go.name.Replace("(Clone)", "") == original_KG_Chat.name;

        private static void Prefix(Chat __instance)
        {
            if (!isKGChat(__instance.gameObject))
            {
                origStuff[0] = __instance.m_worldTextBase;
                origStuff[1] = __instance.m_npcTextBase;
                origStuff[2] = __instance.m_npcTextBaseLarge;
            }
            else
            {
                __instance.m_worldTextBase = origStuff[0];
                __instance.m_npcTextBase = origStuff[1];
                __instance.m_npcTextBaseLarge = origStuff[2];
            }
        }
    }

    [HarmonyPatch(typeof(Menu), nameof(Menu.IsVisible))]
    [ClientOnlyPatch]
    private static class Menu_Patch
    {
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
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> list = new List<CodeInstruction>(instructions);
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].opcode == OpCodes.Ldc_I4 && ((int)list[i].operand == 323 || (int)list[i].operand == 324))
                {
                    list[i].opcode = OpCodes.Nop;
                    list[i + 1].opcode = OpCodes.Nop;
                    list[i + 2].opcode = OpCodes.Nop;
                }
            }

            return list;
        }
    }

    [HarmonyPatch(typeof(Terminal), nameof(Terminal.AddString), typeof(string), typeof(string), typeof(Talker.Type), typeof(bool))]
    [ClientOnlyPatch]
    private static class Chat_Patches2
    {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> list = new List<CodeInstruction>(instructions);
            MethodInfo methodInfo = AccessTools.DeclaredMethod(typeof(string), "ToUpper", Type.EmptyTypes);
            MethodInfo methodInfo2 = AccessTools.DeclaredMethod(typeof(string), "ToLowerInvariant", Type.EmptyTypes);
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].opcode == OpCodes.Callvirt &&
                    (list[i].operand == methodInfo || list[i].operand == methodInfo2))
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
        private static void Prefix(Terminal __instance)
        {
            if (__instance == kgChat)
                __instance.m_scrollHeight = 0;
        }

        private static void Postfix(Terminal __instance)
        {
            if (__instance == kgChat && !__instance.m_input.isFocused)
                ResetScroll();
        }
    }

    private static readonly Dictionary<string, string> Emoji_Map = new()
    {
        { ":moji0:", "<sprite=0>" },
        { ":moji1:", "<sprite=1>"},
        { ":moji2:", "<sprite=2>"},
        { ":moji3:", "<sprite=3>"},
        { ":moji4:", "<sprite=4>"},
        { ":moji5:", "<sprite=5>"},
        { ":moji6:", "<sprite=6>"},
        { ":moji7:", "<sprite=7>"},
        { ":moji8:", "<sprite=8>"},
        { ":moji9:", "<sprite=9>"},
        { ":moji10:", "<sprite=10>"},
        { ":moji11:", "<sprite=11>"},
        { ":moji12:", "<sprite=12>"},
        { ":moji13:", "<sprite=13>"},
        { ":moji14:", "<sprite=14>"},
        { ":moji15:", "<sprite=15>"},
        { ":moji16:", "<sprite=16>"},
        { ":moji17:", "<sprite=17>"},
        { ":moji18:", "<sprite=18>"},
        { ":moji19:", "<sprite=19>"},
        { ":moji20:", "<sprite=20>"},
        { ":moji21:", "<sprite=21>"},
        { ":moji22:", "<sprite=22>"},
        { ":moji23:", "<sprite=23>"},
        { ":moji24:", "<sprite=24>"},
    };

    public class ModeController : MonoBehaviour
    {
        public static ModeController Instance;
        public SendMode mode;
        private Transform Emojis_Tab;

        public void Setup()
        {
            Instance = this;
            mode = SendMode.Say;
            Button _sayButton = transform.Find("CHATWINDOW/Input Field/Say").GetComponent<Button>();
            Button _shoutButton = transform.Find("CHATWINDOW/Input Field/Shout").GetComponent<Button>();
            Button _whisperButton = transform.Find("CHATWINDOW/Input Field/Whisper").GetComponent<Button>();
            Button _groupButton = transform.Find("CHATWINDOW/Input Field/Groups").GetComponent<Button>();
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
            FillEmojis();
            transform.Find("CHATWINDOW/Input Field/Emojis").GetComponent<Button>().onClick.AddListener(() =>
            {
                Emojis_Tab.gameObject.SetActive(!Emojis_Tab.gameObject.activeSelf);
                AssetStorage.AssetStorage.AUsrc.Play();
            });
        }

        private void FillEmojis()
        {
            var button = Emojis_Tab.Find("Emoji");
            foreach (var em in Emoji_Map)
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
            AssetStorage.AssetStorage.AUsrc.Play();
            Chat.instance.m_input.text += " " + key + " ";
            Chat.instance.m_input.MoveTextEnd(false);
        }

        public void ButtonClick(SendMode newMode)
        {
            mode = newMode;
            SayImage.color = mode == SendMode.Say ? Color.green : Color.white;
            ShoutImage.color = mode == SendMode.Shout ? Color.green : Color.white;
            WhisperImage.color = mode == SendMode.Whisper ? Color.green : Color.white;
            GroupImage.color = mode == SendMode.Group ? Color.green : Color.white;
            AssetStorage.AssetStorage.AUsrc.Play();
        }

        private Image SayImage;
        private Image ShoutImage;
        private Image WhisperImage;
        private Image GroupImage;

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
            text = ModeController.Instance.mode switch
            {
                ModeController.SendMode.Say => "/say " + text,
                ModeController.SendMode.Shout => "/s " + text,
                ModeController.SendMode.Whisper => "/w " + text,
                ModeController.SendMode.Group => "/p " + text
            };
        }

        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            bool isdone = false;
            foreach (var instruction in instructions)
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

        [HarmonyPatch]
        [ClientOnlyPatch]
        private static class EmojiPatch
        {
            private static FieldInfo textField;

            private static MethodInfo TargetMethod()
            {
                const string targetClass = "<>c__DisplayClass12_0";
                const string targetMethod = "<OnNewChatMessage>b__2";
                var type = typeof(Chat).GetNestedTypes(BindingFlags.NonPublic)
                    .FirstOrDefault(t => t.Name == targetClass);
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
            private static IEnumerable<CodeInstruction> Code(IEnumerable<CodeInstruction> code)
            {
                var targetField = AccessTools.Field(typeof(Chat), nameof(Chat.m_hideTimer));
                foreach (var instruction in code)
                {
                    yield return instruction;
                    if (instruction.opcode == OpCodes.Stfld && instruction.operand == targetField)
                    {
                        yield return new CodeInstruction(OpCodes.Ldarg_0);
                        yield return new CodeInstruction(OpCodes.Call,
                            AccessTools.Method(typeof(EmojiPatch), nameof(StringReplacer)));
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(Chat), nameof(Chat.AddInworldText))]
    [ClientOnlyPatch]
    private static class Chat_AddInworldText_Patch
    {
        private static void Prefix(ref string text)
        {
            if (!kgChat) return;
            text = Regex.Replace(text, @"<sprite=\d+>", "");
        }
    }
}