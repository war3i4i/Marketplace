using Marketplace.ExternalLoads;

namespace Marketplace.Stats;

[Market_Autoload(Market_Autoload.Type.Client)]
public class CustomValuesShowup
{
    public static bool Show;

    public static void OnInit()
    {
        Marketplace.Global_OnGUI_Updator += OnGUI;
        Marketplace.Global_Updator += Updator;
    }

    private static void Updator(float obj)
    {
        if (Show && Input.GetKeyDown(KeyCode.Escape))
        {
            Show = false;
            Menu.instance?.OnClose();
        }
    }

    [HarmonyPatch(typeof(TextInput), nameof(TextInput.IsVisible))]
    [ClientOnlyPatch]
    private static class TextInput_IsVisible_Patch
    {
        [UsedImplicitly]
        private static void Postfix(TextInput __instance, ref bool __result) => __result |= Show;
    }

    private static Rect _ui;
    private static GUIStyle style;

    private static void OnGUI()
    {
        if (Show)
        {
            style ??= new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleLeft };
            float scale = 4f;
            float X = Screen.width / scale / 2f;
            float Y = Screen.height / scale / 2f;
            _ui = new Rect(Screen.width / 2f - X, Screen.height / 2f - Y, Screen.width / scale, Screen.height / scale);
            GUI.backgroundColor = Color.black;
            _ui = GUI.Window(1920577722, _ui, Window, "Custom Values");
            GUI.Window(43501325, _ui, Test, "");
            GUI.Window(43102238, _ui, Test, "");
        }
    }

    private static void Test(int id)
    {
    }

    private static Vector2 _scroll;

    private static void Window(int id)
    {
        _scroll = GUILayout.BeginScrollView(_scroll);
        foreach (KeyValuePair<string, int> customValue in Player.m_localPlayer.GetAllCustomValues())
        {
            GUILayout.BeginHorizontal();
            Sprite icon = Utils.TryFindIcon(customValue.Key, AssetStorage.CustomValue_Icon);
            GUILayout.Label(icon.texture, GUILayout.Width(60), GUILayout.Height(60));
            GUILayout.Label($"{customValue.Key.Replace("_", " ")}: {customValue.Value}", style, GUILayout.Height(60));
            GUILayout.EndHorizontal();
        }

        GUILayout.EndScrollView();
    }
}