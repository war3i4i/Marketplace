/*using System.Reflection.Emit;
using Marketplace.Modules.Teleporter;

namespace Marketplace.Stats;

[Market_Autoload(Market_Autoload.Type.Client)]
public static class StatsPanel
{
    private static bool Show;
    private static JSONParameters PARAMS;
    private static readonly Dictionary<string, Stats> StatsDict = new();

    private class Stats
    {
        public bool Show = false;
        public string Search = "";
        public List<Stat> _statsList = new();
    }

    private class Stat
    {
        private readonly FieldInfo field;
        public bool Show;
        public string Cache;
        private readonly string _Name;
        private string reflection;

        public Stat(FieldInfo field, string typeName, string reflection, bool useFormatter)
        {
            this.field = field;
            Show = false;
            this.reflection = reflection;
            _Name = $"<color=yellow>[{typeName}]{(useFormatter ? "" : " ")}</color>";

            foreach (char c in field.Name.Replace("Synced", ""))
            {
                if (char.IsUpper(c) && useFormatter)
                    _Name += " ";
                _Name += c;
            }
        }

        public string Name() => _Name;
        public string FName() => field.Name;

        public void Update()
        {
            try
            {
                Type toGeneric = typeof(CustomSyncedValue<>).MakeGenericType(field.FieldType.GetGenericArguments()[0]);
                object obj = field.GetValue(null);
                if (reflection != null)
                {
                    Cache = JSON.ToNiceJSON(AccessTools.PropertyGetter(toGeneric, reflection).Invoke(obj, null), PARAMS);
                }
                else
                {
                    Cache = JSON.ToNiceJSON(obj, PARAMS);
                }
               
            }
            catch
            {
               
            }
        }
    }

    private static void OnInit()
    {
        PARAMS = new JSONParameters
        {
            UseExtensions = false,
            SerializeNullValues = true,
            DateTimeMilliseconds = false,
            UseUTCDateTime = true,
            UseOptimizedDatasetSchema = true,
            UseValuesOfEnums = false,
            SerializerMaxDepth = 40,
        };
        Marketplace.Global_OnGUI_Updator += OnGUI;
        StatsDict.Add("Synced Data", new ());
        StatsDict.Add("Dictionary / List", new ());
        foreach (Type type in Assembly.GetExecutingAssembly().GetTypes())
        {
            foreach (FieldInfo field in AccessTools.GetDeclaredFields(type))
            {
                try
                {
                    string gentypedefstr = field.FieldType.GetGenericTypeDefinition().ToString();
                    bool cond = gentypedefstr.Contains("Marketplace.CustomSyncedValue");
                    if (field.IsStatic && field.FieldType.IsGenericType && cond)
                    {
                        StatsDict["Synced Data"]._statsList.Add(new(field, type.FullName, "Value", true));
                    }
                    
                    bool cond2 = gentypedefstr.Contains("System.Collections.Generic.Dictionary") || gentypedefstr.Contains("System.Collections.Generic.List");
                    
                    if (type.Namespace != null && type.Namespace.Contains("Marketplace") && field.IsStatic && field.FieldType.IsGenericType && cond2)
                    {
                        StatsDict["Dictionary / List"]._statsList.Add(new(field, type.FullName, null, false));
                    }
                }
                catch
                {
                }
            }
        }

        foreach (KeyValuePair<string, Stats> dict in StatsDict)
        {
            dict.Value._statsList = dict.Value._statsList.OrderBy(x => x.FName()).ToList();
        }
    }

    private static float _mult1, _mult2;
    private static Rect _mainMenuRect;
    private static bool firstTime = true;
    private static GUIStyle guibutton;

    private static void OnGUI()
    {
        if (Show)
        {
            if (firstTime)
            {
                _mult1 = (float)Screen.width / 1920;
                _mult2 = (float)Screen.height / 1080;
                _mainMenuRect = new Rect(1920 * _mult1 * 0.28f, 1080 * _mult2 * 0.05f, 1920 * _mult1 / 2f,
                    1080 * _mult2 * 0.90f);
                guibutton = new GUIStyle(GUI.skin.button)
                {
                    fixedWidth = 100 * _mult1
                };
                firstTime = false;
            }

            GUI.backgroundColor = Color.black;
            _mainMenuRect = GUI.Window(12205777, _mainMenuRect, GUI_FUNC, "Marketplace Debug Stats");
            GUI.Window(435013215, _mainMenuRect, Test, "");
            GUI.Window(431022538, _mainMenuRect, Test, "");
        }
    }

    [HarmonyPatch(typeof(TextInput), nameof(TextInput.IsVisible))]
    [ClientOnlyPatch]
    private static class Menu_IsVisible_Patch
    {
        private static void Postfix(ref bool __result) => __result |= Show;
    }

    private static Vector2 SV;

    private static void GUI_FUNC(int id)
    {
        SV = GUILayout.BeginScrollView(SV);
        GUILayout.BeginVertical();
        foreach (KeyValuePair<string, Stats> statList in StatsDict)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("<color=lime>" + statList.Key + "</color>",
                new GUIStyle(GUI.skin.button) { fontSize = 25 });
            if (GUILayout.Button(statList.Value.Show ? "<color=red>Hide</color>" : "<color=lime>Open</color>", guibutton))
            {
                statList.Value.Show = !statList.Value.Show;
            }
            GUILayout.EndHorizontal();
            if (statList.Value.Show)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("<color=cyan>Search</color>");
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                statList.Value.Search = GUILayout.TextField(statList.Value.Search);
                GUILayout.EndHorizontal();
                GUILayout.Space(5);
                foreach (Stat stat in statList.Value._statsList)
                {
                    string statName = stat.Name();
                    if (!statName.ToLower().Replace(" ","").Contains(statList.Value.Search.ToLower().Replace(" ",""))) continue;
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("<color=lime>" + statName + "</color>");
                    if (GUILayout.Button(stat.Show ? "<color=red>-</color>" : "<color=lime>+</color>", guibutton))
                    {
                        stat.Show = !stat.Show;
                        if (stat.Show) stat.Update();
                    }

                    GUILayout.EndHorizontal();
                    if (stat.Show)
                    {
                        GUI.contentColor = Color.green;
                        GUILayout.TextArea(stat.Cache);
                        GUI.contentColor = Color.white;
                    }
                }
                GUILayout.Space(10);
            }
        }

        GUILayout.EndVertical();
        GUILayout.EndScrollView();
        GUI.DragWindow();
    }

    private static void Test(int id)
    {
    }

    [HarmonyPatch(typeof(ConnectPanel), nameof(ConnectPanel.Update))]
    [ClientOnlyPatch]
    private static class PatchConnectPanel
    {
        private static void Switch()
        {
            if (!Utils.IsDebug_Strict) return;
            Show = ConnectPanel.IsVisible();
            if (Show)
            {
                foreach (KeyValuePair<string, Stats> stat in StatsDict)
                {
                    stat.Value.Show = false;
                    foreach (Stat stat2 in stat.Value._statsList)
                    {
                        stat2.Show = false;
                    }
                }
            }
        }

        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> Code(IEnumerable<CodeInstruction> code)
        {
            CodeMatcher matcher = new CodeMatcher(code);
            matcher.MatchForward(false,
                    new CodeMatch(OpCodes.Callvirt,
                        AccessTools.Method(typeof(GameObject), nameof(GameObject.SetActive))))
                .Advance(1).Insert(
                    new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(PatchConnectPanel), nameof(Switch))));
            return matcher.Instructions();
        }
    }

    [HarmonyPatch]
    [ClientOnlyPatch]
    private static class PatchJSONConverter
    {
        private static MethodInfo TargetMethod()
        {
            Type jsonSerializer = Type.GetType("fastJSON.JSONSerializer");
            MethodInfo method = AccessTools.Method(jsonSerializer, "WriteValue");
            return method;
        }

        private static bool Prefix(object __instance, object obj)
        {

            if (obj is Tuple<int, int> tuple)
            {
                StringBuilder output =
                    (StringBuilder)AccessTools.Field(__instance.GetType(), "_output").GetValue(__instance);
                output.Append($"\"{tuple.Item1}, {tuple.Item2}\"");
                return false;
            }

            if (obj is Color or Color32)
            {
                StringBuilder output =
                    (StringBuilder)AccessTools.Field(__instance.GetType(), "_output").GetValue(__instance);
                if (obj is Color color)
                {
                    output.Append($"\"{color.r}, {color.g}, {color.b}, {color.a}\"");
                }
                else
                {
                    Color32 color32 = (Color32)obj;
                    output.Append($"\"{color32.r}, {color32.g}, {color32.b}, {color32.a}\"");
                }

                return false;
            }

            if (obj is Vector2 vec2)
            {
                StringBuilder output =
                    (StringBuilder)AccessTools.Field(__instance.GetType(), "_output").GetValue(__instance);
                output.Append($"\"{vec2.x}, {vec2.y}\"");
                return false;
            }
            
            if (obj is Vector3 vec3)
            {
                StringBuilder output =
                    (StringBuilder)AccessTools.Field(__instance.GetType(), "_output").GetValue(__instance);
                output.Append($"\"{vec3.x}, {vec3.y}, {vec3.z}\"");
                return false;
            }
            
            if (obj is Vector4 vec4)
            {
                StringBuilder output =
                    (StringBuilder)AccessTools.Field(__instance.GetType(), "_output").GetValue(__instance);
                output.Append($"\"{vec4.x}, {vec4.y}, {vec4.z}, {vec4.w}\"");
                return false;
            }

            if (obj is Rect r)
            {
                StringBuilder output =
                    (StringBuilder)AccessTools.Field(__instance.GetType(), "_output").GetValue(__instance);
                output.Append($"\"{r.x}, {r.y}, {r.width}, {r.height}\"");
                return false;
            }

            return true;
        }
    }
}*/