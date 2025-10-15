using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reflection.Emit;
using System.Threading.Tasks;
using BepInEx.Configuration;
using Guilds;
using Marketplace.Modules.Global_Options;
using Marketplace.Modules.Teleporter;
using UnityEngine.SceneManagement;

namespace Marketplace.Modules.TerritorySystem;

[UsedImplicitly]
[Market_Autoload(Market_Autoload.Type.Client, Market_Autoload.Priority.Normal)]
public static class TerritorySystem_Main_Client
{
    public static TerritorySystem_DataTypes.Territory CurrentTerritory;
    private static Color[] originalMapColors = null!;
    private static Color[] originalHeightColors = null!;
    private static readonly int Pulse = Animator.StringToHash("pulse");

    public static readonly Dictionary<TerritorySystem_DataTypes.TerritoryFlags, List<TerritorySystem_DataTypes.Territory>> TerritoriesByFlags = new();
    public static readonly Dictionary<TerritorySystem_DataTypes.AdditionalTerritoryFlags, List<TerritorySystem_DataTypes.Territory>> TerritoriesByFlags_Additional = new();
    public static readonly List<TerritorySystem_DataTypes.Territory> ForSaleTerritories = new();

    private static float ZoneTick;
    private static DateTime LastNoAccessMesssage;

    private static ConfigEntry<bool> UseMapDraw;
    public static ConfigEntry<bool> AlwaysShowZoneVisualizer;

    [UsedImplicitly]
    private static void OnInit()
    {
        UseMapDraw = Marketplace._thistype.Config.Bind("Territories", "Use Map Draw", true);
        AlwaysShowZoneVisualizer = Marketplace._thistype.Config.Bind("Territories", "Always Show Zone Visualizer", false);
        foreach (TerritorySystem_DataTypes.TerritoryFlags flag in TerritorySystem_DataTypes.AllTerritoryFlagsArray)
        {
            TerritoriesByFlags[flag] = new List<TerritorySystem_DataTypes.Territory>();
        }

        foreach (TerritorySystem_DataTypes.AdditionalTerritoryFlags flag in TerritorySystem_DataTypes
                     .AllAdditionaTerritoryFlagsArray)
        {
            TerritoriesByFlags_Additional[flag] = new List<TerritorySystem_DataTypes.Territory>();
        }

        TerritorySystem_DataTypes.SyncedTerritoriesData.ValueChanged += OnTerritoryUpdate;
        Marketplace.Global_FixedUpdator += TerritoryFixedUpdate;

        Guilds.API.RegisterOnGuildJoined(guildsAPI_CacheClear);
        Guilds.API.RegisterOnGuildLeft(guildsAPI_CacheClear);
    }

    static void guildsAPI_CacheClear(Guilds.Guild guild, Guilds.PlayerReference playerReference)
    {
        TerritorySystem_DataTypes.SyncedTerritoriesData.Value.Territories.ForEach(t => t.ClearOwnerCache());
    }

    private static void TerritoryFixedUpdate(float dt)
    {
        ZoneTick -= Time.fixedDeltaTime;
        if (!Player.m_localPlayer) return;
        Player p = Player.m_localPlayer;
        Vector3 vec = p.transform.position;
        CurrentTerritory = TerritorySystem_DataTypes.Territory.GetCurrentTerritory(vec);
        if (CurrentTerritory == null) return;

        if (CurrentTerritory.Wind != 0)
        {
            EnvMan.instance.m_windDir2.w = CurrentTerritory.Wind;
        }

        if (ParticleMist.instance && ParticleMist.instance.m_ps)
        {
            if (CurrentTerritory.AdditionalFlags.HasFlagFast(TerritorySystem_DataTypes.AdditionalTerritoryFlags.NoMist))
            {
                if (ParticleMist.instance.m_ps.emission.enabled)
                {
                    ParticleSystem.EmissionModule emission = ParticleMist.instance.m_ps.emission;
                    emission.enabled = false;
                }
            }
            else
            {
                if (!ParticleMist.instance.m_ps.emission.enabled)
                {
                    ParticleSystem.EmissionModule emission = ParticleMist.instance.m_ps.emission;
                    emission.enabled = true;
                }
            }
        }


        if (CurrentTerritory.Flags.HasFlagFast(TerritorySystem_DataTypes.TerritoryFlags.PushAway) &&
            !CurrentTerritory.IsPermitted())
        {
            float pushValue = Time.fixedDeltaTime * 7f;

            Vector3 newVector3 = p.transform.position +
                                 (p.transform.position - CurrentTerritory.Pos3D()).normalized * pushValue;
            p.m_body.isKinematic = true;
            p.transform.position = new Vector3(newVector3.x, p.transform.position.y, newVector3.z);
            p.m_body.isKinematic = false;
        }

        if (CurrentTerritory.Flags.HasFlagFast(TerritorySystem_DataTypes.TerritoryFlags.PveOnly))
        {
            p.SetPVP(false);
            p.m_lastCombatTimer = 0;
            InventoryGui.instance.m_pvp.isOn = false;
        }

        if (CurrentTerritory.Flags.HasFlagFast(TerritorySystem_DataTypes.TerritoryFlags.PvpOnly))
        {
            p.SetPVP(true);
            p.m_lastCombatTimer = 0;
            InventoryGui.instance.m_pvp.isOn = true;
        }

        if (CurrentTerritory.Flags.HasFlagFast(TerritorySystem_DataTypes.TerritoryFlags.PeriodicDamage) &&
            !CurrentTerritory.IsPermitted() &&
            ZoneTick <= 0)
        {
            HitData hit = new HitData();
            hit.m_damage.m_fire = CurrentTerritory.PeriodicDamageValue;
            p.Damage(hit);
        }

        if (CurrentTerritory.Flags.HasFlagFast(TerritorySystem_DataTypes.TerritoryFlags.PeriodicHealALL) &&
            ZoneTick <= 0)
        {
            p.Heal(CurrentTerritory.PeriodicHealValue);
        }

        if (CurrentTerritory.Flags.HasFlagFast(TerritorySystem_DataTypes.TerritoryFlags.PeriodicHeal) &&
            CurrentTerritory.IsPermitted() &&
            ZoneTick <= 0)
        {
            p.Heal(CurrentTerritory.PeriodicHealValue);
        }

        if (CurrentTerritory.Flags.HasFlagFast(TerritorySystem_DataTypes.TerritoryFlags.CustomEnvironment))
        {
            EnvMan.instance.SetForceEnvironment(GetCurrentEnvironment(CurrentTerritory.CustomEnvironment, (int)EnvMan.instance.m_totalSeconds));
        }

        if (ZoneTick <= 0) ZoneTick = 1f;
    }
    
    private static string GetCurrentEnvironment(string[] envs, int m_seconds)
    {
        if (envs.Length == 0) return "";
        if (envs.Length == 1) return envs[0];
        int index = (m_seconds / 1200) % envs.Length;
        return envs[index];
    }

    public class RevealedTerritory
    {
        public TerritorySystem_DataTypes.TerritoryType Type;
        public Vector3 Pos;
        public int Radius;
    }

    private static void OnTerritoryUpdate()
    {
        API.ClientSide.FillingTerritoryData = true;
        
        foreach (KeyValuePair<TerritorySystem_DataTypes.TerritoryFlags, List<TerritorySystem_DataTypes.Territory>>
                     territoriesByFlag in TerritoriesByFlags)
        {
            territoriesByFlag.Value.Clear();
        }

        foreach (KeyValuePair<TerritorySystem_DataTypes.AdditionalTerritoryFlags,
                     List<TerritorySystem_DataTypes.Territory>> territoriesByFlag in
                 TerritoriesByFlags_Additional)
        {
            territoriesByFlag.Value.Clear();
        }

        ForSaleTerritories.Clear();

        if (TerritorySystem_DataTypes.SyncedTerritoriesData.Value.Territories.Count == 0)
        {
            API.ClientSide.FillingTerritoryData = false;
            DoMapMagic();
            return;
        }

        foreach (TerritorySystem_DataTypes.Territory territory in TerritorySystem_DataTypes.SyncedTerritoriesData.Value.Territories)
        {
            foreach (TerritorySystem_DataTypes.TerritoryFlags flag in TerritorySystem_DataTypes.AllTerritoryFlagsArray)
            {
                if (territory.Flags.HasFlagFast(flag))
                {
                    TerritoriesByFlags[flag].Add(territory);
                }
            }

            foreach (TerritorySystem_DataTypes.AdditionalTerritoryFlags flag in TerritorySystem_DataTypes.AllAdditionaTerritoryFlagsArray)
            {
                if (territory.AdditionalFlags.HasFlagFast(flag))
                {
                    TerritoriesByFlags_Additional[flag].Add(territory);
                }
            }

            if (territory.Type is TerritorySystem_DataTypes.TerritoryType.ForSale) ForSaleTerritories.Add(territory);
        }

        TerritoriesByFlags[TerritorySystem_DataTypes.TerritoryFlags.ForceGroundHeight]
            .Sort((a, b) => a.Priority.CompareTo(b.Priority));
        TerritoriesByFlags[TerritorySystem_DataTypes.TerritoryFlags.AddGroundHeight]
            .Sort((a, b) => a.Priority.CompareTo(b.Priority));
        TerritoriesByFlags[TerritorySystem_DataTypes.TerritoryFlags.LimitZoneHeight]
            .Sort((a, b) => a.Priority.CompareTo(b.Priority));
        API.ClientSide.FillingTerritoryData = false;
        if (TerritorySystem_DataTypes.SyncedTerritoriesData.Value.ShouldRedraw)
        {
            DoMapMagic();
        }

        ZoneVisualizer.OnMapChange();
    }


    [HarmonyPatch(typeof(SpawnSystem), nameof(SpawnSystem.FindBaseSpawnPoint))]
    public class IsSpawnPointGood
    {
        static bool Prefix(SpawnSystem.SpawnData spawn)
        {
            if (CurrentTerritory != null && CurrentTerritory.AdditionalFlags.HasFlagFast(TerritorySystem_DataTypes.AdditionalTerritoryFlags.CustomJereSpawner))
            {
                bool result = CurrentTerritory.CustomSpawnerNames.Contains(spawn.m_name);
                return result;
            }

            bool endResult = !spawn.m_name.Contains("MARKETPLACESPAWNER");
            return endResult;
        }
    }

    [HarmonyPatch(typeof(Minimap), nameof(Minimap.UpdateBiome))]
    [ClientOnlyPatch]
    private static class Minimap_UpdateBiome_Patch
    {
        private static bool Prefix()
        {
            if (CurrentTerritory != null && Minimap.instance.m_mode != Minimap.MapMode.Large) return false;
            return true;
        }

        private const string search = "\n<i><b></b></i>";

        private static void Postfix(Minimap __instance)
        {
            if (!Player.m_localPlayer) return;
            if (__instance.m_mode != Minimap.MapMode.Large)
            {
                if (CurrentTerritory != null && TerritorySystem_DataTypes.Territory.LastTerritory != CurrentTerritory)
                {
                    string newText = $"<color=green>{CurrentTerritory.GetName()}</color>";
                    __instance.m_biomeNameSmall.text = newText;
                    __instance.m_biomeNameSmall.GetComponent<Animator>().SetTrigger(Pulse);
                    ShowCustomTerritoryMessage(CurrentTerritory);
                    TerritorySystem_DataTypes.Territory.LastTerritory = CurrentTerritory;
                    EnvMan.instance.SetForceEnvironment("");
                }

                if (CurrentTerritory == null && TerritorySystem_DataTypes.Territory.LastTerritory != null)
                {
                    TerritorySystem_DataTypes.Territory.LastTerritory = null;
                    Minimap.instance.m_biome = Heightmap.Biome.None;
                    EnvMan.instance.SetForceEnvironment("");
                    __instance.m_biomeNameSmall.text = "";
                }
            } 
            else 
            { 
                Vector3 vector = __instance.ScreenToWorldPoint(ZInput.IsMouseActive() ? Input.mousePosition : new Vector3(Screen.width / 2, Screen.height / 2));
                bool found = false;
                foreach (TerritorySystem_DataTypes.Territory territory in TerritorySystem_DataTypes.SyncedTerritoriesData.Value.Territories)
                {
                    if ((territory.AdditionalFlags.HasFlagFast(TerritorySystem_DataTypes.AdditionalTerritoryFlags.RevealOnMap) || __instance.IsExplored(vector)) && territory.IsInside2D(new Vector2(vector.x, vector.z)))
                    { 
                        string newText = search + "\n" + territory.GetName() + territory.GetTerritoryFlags() + search;
                        TryReplaceTerritoryString(__instance.m_biomeNameLarge, newText);
                        found = true;
                        break;
                    }
                }
  
                if (!found)
                {
                    TryReplaceTerritoryString(__instance.m_biomeNameLarge, "");
                }
            }
        }

        private static void TryReplaceTerritoryString(TMP_Text textElement, string to)
        {
            string text = textElement.text;
            int startIndex = text.IndexOf(search, StringComparison.Ordinal);
            if (startIndex > 0)
            {
                int endIndex = text.IndexOf(search, startIndex + search.Length, StringComparison.Ordinal);
                string match = text.Substring(startIndex, endIndex - startIndex + search.Length);
                text = text.Replace(match, to);
                textElement.text = text;
            }
            else
            {
                textElement.text += to;
            }
        }

        private static DateTime LastTimeTerritoryMessage;

        private static void ShowCustomTerritoryMessage(TerritorySystem_DataTypes.Territory territory)
        {
            string rawName = territory.RawName();
            if ((DateTime.Now - LastTimeTerritoryMessage).TotalSeconds <= 5) return;
            LastTimeTerritoryMessage = DateTime.Now;
            GameObject Prefab = UnityEngine.Object.Instantiate(MessageHud.instance.m_biomeFoundPrefab,
                MessageHud.instance.transform);
            RectTransform Rect = Prefab.GetComponent<RectTransform>();
            Rect.anchorMin = new Vector2(0.5f, 1f);
            Rect.anchorMax = new Vector2(0.5f, 1f);
            Rect.anchoredPosition = new Vector2(0, -200f);
            TimedDestruction timed = Prefab.AddComponent<TimedDestruction>();
            Prefab.transform.GetChild(0).GetComponent<Animator>().speed = 2f;
            global::Utils.FindChild(Prefab.transform, "Title").GetComponent<TMP_Text>().text = rawName;
            timed.m_timeout = 2f;
            timed.Trigger();

            if (!string.IsNullOrWhiteSpace(territory.JoinOtherServerString))
            {
                string[] split = territory.JoinOtherServerString.Split('@');
                string serverName = split[0];
                string ip = split[1];
                string port = split[2];
                string password = split[3];
                UnifiedPopup.Push(
                    new YesNoPopup("Travel", $"Do you wanna travel to <color=yellow>{serverName}</color>?",
                        () =>
                        {
                            UnifiedPopup.Pop();
                            ZNet.SetServerHost(ip, Convert.ToInt32(port), OnlineBackendType.Steamworks);
                            SceneManager.LoadScene("main");
                        },
                        () => { UnifiedPopup.Pop(); }));
            }
        }
    }

    /*[HarmonyPatch(typeof(FejdStartup), nameof(FejdStartup.Start))]
    private static class FejdStartup_Start_Patch
    {
        public static string IP, Port, Password;

        [UsedImplicitly]
        private static void Postfix(FejdStartup __instance)
        {
            if (!string.IsNullOrWhiteSpace(IP))
            {
                string ip = IP;
                string port = Port;
                IP = null;
                Port = null;
                ZNet.SetServerHost(ip, int.Parse(port), OnlineBackendType.Steamworks);
                FejdStartup.ResetPendingInvite?.Invoke();
                FejdStartup.instance.AddToServerList(FejdStartup.instance, new FejdStartup.StartGameEventArgs(false));
                FejdStartup.instance.TransitionToMainScene();
            }
        }
    }*/

    /*[HarmonyPatch(typeof(ZNet), nameof(ZNet.RPC_ClientHandshake))]
    private static class ZNet_RPC_ClientHandshake_Patch
    {
        [UsedImplicitly]
        private static void Postfix(ZNet __instance, ZRpc rpc)
        {
            if (!string.IsNullOrWhiteSpace(FejdStartup_Start_Patch.Password))
            {
                string pwd = FejdStartup_Start_Patch.Password;
                FejdStartup_Start_Patch.Password = null;
                __instance.OnPasswordEntered(pwd);
            }
        }
    }*/

    [HarmonyPatch(typeof(Minimap), nameof(Minimap.Awake))]
    [ClientOnlyPatch]
    private static class Minimap_Awake_Patch
    {
        private static void Postfix(Minimap __instance)
        {
            __instance.m_biomeNameLarge.alignment = TextAlignmentOptions.TopRight;
            __instance.m_biomeNameSmall.alignment = TextAlignmentOptions.TopRight;
        }
    }

    [HarmonyPatch(typeof(Minimap), nameof(Minimap.GenerateWorldMap))]
    private static class Minimap_GenerateWorldMap_Patch
    {
        public static bool GENERATING;

        [UsedImplicitly]
        private static void Prefix(Minimap __instance) 
        {
            GENERATING = true;
        }

        [UsedImplicitly]
        private static void Postfix(Minimap __instance)
        {
            GENERATING = false;
        }
    }

    [HarmonyPatch(typeof(Minimap), nameof(Minimap.LoadMapData))]
    [ClientOnlyPatch]
    private class PatchMinimapCircles
    {
        [HarmonyPriority(-3000)]
        private static void Postfix(Minimap __instance)
        {
            originalMapColors = __instance.m_mapTexture.GetPixels();
            originalHeightColors = __instance.m_heightTexture.GetPixels();
            DoMapMagic();
        }
    }
    
    
    [HarmonyPatch(typeof(Minimap), nameof(Minimap.GenerateWorldMap))]
    [ClientOnlyPatch]
    private class PatchMinimapCircles2
    {
        [HarmonyPriority(3000)]
        private static void Postfix(Minimap __instance)
        {
            originalMapColors = __instance.m_mapTexture.GetPixels();
            originalHeightColors = __instance.m_heightTexture.GetPixels();
            DoMapMagic();
        }
    }

    private static int MapMagicCounter;

    private static async void DoMapMagic()
    { 
        if (originalMapColors == null || TerritorySystem_DataTypes.SyncedTerritoriesData.Value == null || TerritorySystem_DataTypes.SyncedTerritoriesData.Value.Territories.Count == 0) return;
        if (!UseMapDraw.Value) return;
        ResetAndExplore();
        MapMagicCounter++;
        int currentCounter = MapMagicCounter;
        try
        {
            Color[] mapColors = new Color[originalMapColors.Length];
            Color[] heightColors = new Color[originalHeightColors.Length];
            bool[] copyExplored = new bool[Minimap.instance.m_explored.Length];
            Color[] fogColors = new Color[Minimap.instance.m_fogTexture.width * Minimap.instance.m_fogTexture.height];
            int segments = Environment.ProcessorCount;
            await Task.Run(() =>
            {
                Array.Copy(Minimap.instance.m_explored, copyExplored, Minimap.instance.m_explored.Length);
                Array.Copy(originalMapColors, mapColors, originalMapColors.Length);
                Array.Copy(originalHeightColors, heightColors, originalHeightColors.Length);
                Array.Copy(Minimap.instance.m_fogTexture.GetPixels(), fogColors, fogColors.Length);
                Parallel.ForEach(Enumerable.Range(0, segments), segment =>
                {
                    float pixelSize = Minimap.instance.m_pixelSize;
                    int textureSize = Minimap.instance.m_textureSize;
                    int num = textureSize / 2;
                    float num2 = pixelSize / 2;
                    int segmentSize = Mathf.RoundToInt(textureSize / (float)segments);
                    int segmentStart = segment * segmentSize;
                    int segmentEnd = segmentStart + segmentSize;
                    foreach (TerritorySystem_DataTypes.Territory territory in TerritorySystem_DataTypes
                                 .SyncedTerritoriesData
                                 .Value.Territories.Where(t => t.DrawOnMap()).OrderBy(t => t.Priority))
                    {
                        bool hasReveal = territory.AdditionalFlags.HasFlagFast(TerritorySystem_DataTypes.AdditionalTerritoryFlags.RevealOnMap);
                        Color32 MainColor = territory.GetColor();
                        bool externalWater = territory.ShowExternalWater;
                        int y = Mathf.Clamp(Mathf.RoundToInt((territory.Shape switch
                        {
                            TerritorySystem_DataTypes.TerritoryShape.Rectangle => territory.Y[0],
                            _ => territory.Y[0] - territory.Radius,
                        } + num2) / pixelSize + num), segmentStart, segmentEnd);
                        int endY = Mathf.Clamp(Mathf.RoundToInt((territory.Shape switch
                        {
                            TerritorySystem_DataTypes.TerritoryShape.Rectangle => territory.Y[0] + territory.Y[1],
                            _ => territory.Y[0] + territory.Radius,
                        } + num2) / pixelSize + num), segmentStart, segmentEnd);
                        for (; y < endY; y++)
                        {
                            int x = 0, endX = 0;
                            switch (territory.Shape)
                            {
                                case TerritorySystem_DataTypes.TerritoryShape.Rectangle:
                                    x = Mathf.RoundToInt((territory.X[0] + num2) / pixelSize + num);
                                    endX = Mathf.RoundToInt((territory.X[0] + territory.X[1] + num2) / pixelSize +
                                                            num);
                                    break;
                                case TerritorySystem_DataTypes.TerritoryShape.Square:
                                    x = Mathf.RoundToInt((territory.X[0] - territory.Radius + num2) / pixelSize + num);
                                    endX = Mathf.RoundToInt((territory.X[0] + territory.Radius + num2) / pixelSize + num);
                                    break;
                                case TerritorySystem_DataTypes.TerritoryShape.Circle:
                                    float halfWidth =
                                        Mathf.Cos(Mathf.Asin(((y - num) * pixelSize - num2 - territory.Y[0]) / territory.Radius)) * territory.Radius;
                                    x = Mathf.RoundToInt((territory.X[0] - halfWidth + num2) / pixelSize + num);
                                    endX = Mathf.RoundToInt((territory.X[0] + halfWidth + num2) / pixelSize + num);
                                    break;
                            }

                            for (; x < endX; x++)
                            {
                                int idx = y * textureSize + x;
                                if (territory.UsingGradient())
                                {
                                    mapColors[idx] = territory.GradientType switch
                                    {
                                        TerritorySystem_DataTypes.GradientType.FromCenter => territory
                                            .GetGradientFromCenter(new Vector2((x - 1024) * pixelSize,
                                                (y - 1024) * pixelSize)),
                                        TerritorySystem_DataTypes.GradientType.ToCenter => territory
                                            .GetGradientFromCenter(
                                                new Vector2((x - 1024) * pixelSize, (y - 1024) * pixelSize), true),

                                        TerritorySystem_DataTypes.GradientType.LeftRight => territory.GetGradientX(
                                            (x - 1024) * pixelSize),
                                        TerritorySystem_DataTypes.GradientType.RightLeft => territory.GetGradientX(
                                            (x - 1024) * pixelSize, true),

                                        TerritorySystem_DataTypes.GradientType.BottomTop => territory.GetGradientY(
                                            (y - 1024) * pixelSize),
                                        TerritorySystem_DataTypes.GradientType.TopBottom => territory.GetGradientY(
                                            (y - 1024) * pixelSize, true),

                                        TerritorySystem_DataTypes.GradientType.BottomLeftTopRight =>
                                            territory.GetGradientXY(new Vector2((x - 1024) * pixelSize,
                                                (y - 1024) * pixelSize)),
                                        TerritorySystem_DataTypes.GradientType.TopRightBottomLeft =>
                                            territory.GetGradientXY(
                                                new Vector2((x - 1024) * pixelSize, (y - 1024) * pixelSize), true),

                                        TerritorySystem_DataTypes.GradientType.BottomRightTopLeft =>
                                            territory.GetGradientXY_2(new Vector2((x - 1024) * pixelSize,
                                                (y - 1024) * pixelSize)),
                                        TerritorySystem_DataTypes.GradientType.TopLeftBottomRight =>
                                            territory.GetGradientXY_2(
                                                new Vector2((x - 1024) * pixelSize, (y - 1024) * pixelSize), true),

                                        _ => MainColor
                                    };
                                } 
                                else mapColors[idx] = MainColor;
                                if (externalWater) heightColors[idx] = new Color(Mathf.Clamp(heightColors[idx].r, 29f, 89), 0, 0);
                                if (hasReveal) fogColors[idx].r = 0f;

                            }
                        }
                    }
                }); 
            });
            if (currentCounter != MapMagicCounter) return;
            Minimap.instance.m_mapTexture.SetPixels(mapColors);
            Minimap.instance.m_heightTexture.SetPixels(heightColors);
            Minimap.instance.m_fogTexture.SetPixels(fogColors);
            Minimap.instance.m_mapTexture.Apply(false); 
            Minimap.instance.m_heightTexture.Apply(false);
            Minimap.instance.m_fogTexture.Apply(false);
            Minimap.instance.m_explored = copyExplored;
        }
        catch (Exception ex)
        {
            Utils.print($"Error while drawing territories on map: {ex}");
        }
    }
    
    private static void ResetAndExplore_Old(bool[] explored, bool[] exploredOthers)
    {
        Minimap.instance.m_sharedMapHint.gameObject.SetActive(false);
        int length = explored.Length;
        Color[] pixels =  Minimap.instance.m_fogTexture.GetPixels();

        if (length != pixels.Length || length != exploredOthers.Length)
        {
            ZLog.LogError("Dimension mismatch for exploring minimap");
            return;
        }
        for (int i = 0; i < length; i++)
        {
            pixels[i] = Color.white;
            if (explored[i])
            {
                pixels[i].r = 0f;
                Minimap.instance.m_explored[i] = true;
            }
            else
            {
                Minimap.instance.m_explored[i] = false;
            }

            if (exploredOthers[i])
            {
                pixels[i].g = 0f;
                Minimap.instance.m_exploredOthers[i] = true;
            }
            else Minimap.instance.m_exploredOthers[i] = false;
        }

        Minimap.instance.m_fogTexture.SetPixels(pixels);
    }

    private static void ResetAndExplore()
    {
        int length = Minimap.instance.m_explored.Length;
        Color[] pixels = new Color[Minimap.instance.m_fogTexture.width * Minimap.instance.m_fogTexture.height];
        int partitions = Math.Max(1, Environment.ProcessorCount);
        int chunkSize = (length + partitions - 1) / partitions;
        var ranges = new List<(int start, int end)>(partitions);
        for (int p = 0; p < partitions; p++)
        {
            int start = p * chunkSize;
            int end = Math.Min(start + chunkSize, length);
            if (start < end)
                ranges.Add((start, end));
        }
        Parallel.ForEach(ranges, range =>
        {
            for (int i = range.start; i < range.end; i++)
            {
                Color c = Color.white;
                if (Minimap.instance.m_explored[i]) c.r = 0f;
                if (Minimap.instance.m_exploredOthers[i]) c.g = 0f;
                pixels[i] = c;
            }
        });
        Minimap.instance.m_fogTexture.SetPixels(pixels);
    }
    
    private static void FakeReveal(Vector3 worldPos, float radius, TerritorySystem_DataTypes.TerritoryShape shape)
    {
        if (shape == TerritorySystem_DataTypes.TerritoryShape.Rectangle) return;
        int pixelRadius = (int)Mathf.Ceil(radius / Minimap.instance.m_pixelSize);
        int pixelX;
        int pixelY;
        Minimap.instance.WorldToPixel(worldPos, out pixelX, out pixelY);
        Func<int,int, bool> insideMethod = shape switch
        {
            TerritorySystem_DataTypes.TerritoryShape.Circle => (x, y) => new Vector2(x - pixelX, y - pixelY).magnitude <= pixelRadius,
            TerritorySystem_DataTypes.TerritoryShape.Square => (x, y) => Mathf.Abs(x - pixelX) <= pixelRadius && Mathf.Abs(y - pixelY) <= pixelRadius,
        };
        for (int y = pixelY - pixelRadius; y <= pixelY + pixelRadius; y++)
        {
            for (int x = pixelX - pixelRadius; x <= pixelX + pixelRadius; x++)
            {
                if (x < 0 || y < 0 || x >= Minimap.instance.m_textureSize || y >= Minimap.instance.m_textureSize) continue;
                if (insideMethod(x, y))
                {
                    Color pixel = Minimap.instance.m_fogTexture.GetPixel(x, y);
                    pixel.r = 0f;
                    Minimap.instance.m_fogTexture.SetPixel(x, y, pixel);
                }
            }
        }
        Minimap.instance.m_fogTexture.Apply();
    }
    
    private static void DoAreaEffect(Vector3 pos)
    {
        if ((DateTime.Now - LastNoAccessMesssage).TotalSeconds <= 2) return;
        LastNoAccessMesssage = DateTime.Now;
        GameObject znet = ZNetScene.instance.GetPrefab("vfx_lootspawn");
        UnityEngine.Object.Instantiate(znet, pos, Quaternion.identity);
        DamageText.WorldTextInstance worldTextInstance = new DamageText.WorldTextInstance
        {
            m_worldPos = pos,
            m_gui = UnityEngine.Object.Instantiate(DamageText.instance.m_worldTextBase, DamageText.instance.transform)
        };
        worldTextInstance.m_textField = worldTextInstance.m_gui.GetComponent<TMP_Text>();
        DamageText.instance.m_worldTexts.Add(worldTextInstance);
        worldTextInstance.m_textField.color = Color.cyan;
        worldTextInstance.m_textField.fontSize = 24;
        worldTextInstance.m_textField.text = "NO ACCESS";
        worldTextInstance.m_timer = -2f;
    }


    [HarmonyPatch(typeof(Player), nameof(Player.TryPlacePiece))]
    [ClientOnlyPatch]
    private static class NoBuild_Patch
    {
        private static bool Prefix(Player __instance, ref bool __result)
        {
            if (CurrentTerritory == null) return true;
            if (CurrentTerritory.Flags.HasFlagFast(TerritorySystem_DataTypes.TerritoryFlags.NoBuild) &&
                !CurrentTerritory.IsPermitted())
            {
                __result = false;
                DoAreaEffect(__instance.m_placementGhost.transform.position);
                return false;
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(WearNTear), nameof(WearNTear.Remove))]
    [ClientOnlyPatch]
    private static class WearNTear_Damage_Patch_Remove
    {
        private static bool Prefix(WearNTear __instance)
        {
            if (CurrentTerritory == null) return true;
            if (CurrentTerritory.Flags.HasFlagFast(TerritorySystem_DataTypes.TerritoryFlags.NoBuild) &&
                !CurrentTerritory.IsPermitted())
            {
                DoAreaEffect(__instance.transform.position);
                return false;
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(WearNTear), nameof(WearNTear.Damage))]
    [ClientOnlyPatch]
    private static class WearNTear_Damage_Patch_Damage
    {
        private static bool Prefix(WearNTear __instance)
        {
            return !TerritoriesByFlags[TerritorySystem_DataTypes.TerritoryFlags.NoBuildDamage].Any(t => t.IsInside(__instance.transform.position));
        }
    }

    [HarmonyPatch(typeof(Character), nameof(Character.Awake))]
    [ClientOnlyPatch]
    private static class Character_Awake_Patch
    {
        private static void Postfix(Character __instance)
        {
            __instance.m_nview.Register("KGtsLevelUpdate",
                new Action<long, int>((_, level) => { __instance.m_onLevelSet?.Invoke(level); }));
        }
    }

    [HarmonyPatch(typeof(Character), nameof(Character.Start))]
    [ClientOnlyPatch]
    private static class Character_Start_Patch
    {
        private static void Postfix(Character __instance)
        {
            if (CurrentTerritory == null || !__instance.IsOwner() || __instance.IsPlayer()) return;
            if (CurrentTerritory.Flags.HasFlagFast(TerritorySystem_DataTypes.TerritoryFlags.MonstersAddStars) &&
                !__instance.m_nview.m_zdo.GetBool("KGtsLevel"))
            {
                int level = __instance.GetLevel() + CurrentTerritory.AddMonsterLevel;
                __instance.m_nview.m_zdo.Set("KGtsLevel", true);
                __instance.SetLevel(level);
                __instance.m_nview.GetZDO().Set("level", level);
                __instance.SetupMaxHealth();
                __instance.m_nview.InvokeRPC(ZNetView.Everybody, "KGtsLevelUpdate", __instance.GetLevel());
            }
        }
    }

    [HarmonyPatch(typeof(Attack), nameof(Attack.SpawnOnHitTerrain))]
    [ClientOnlyPatch]
    private static class NoPickaxe_Patch
    {
        private static bool Prefix()
        {
            if (CurrentTerritory == null) return true;
            if (CurrentTerritory.Flags.HasFlagFast(TerritorySystem_DataTypes.TerritoryFlags.NoPickaxe) &&
                !CurrentTerritory.IsPermitted())
            {
                DoAreaEffect(Player.m_localPlayer.transform.position);
                return false;
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(Player), nameof(Player.Interact))]
    [ClientOnlyPatch]
    private static class NoInteract_Patch
    {
        private static bool Prefix(GameObject go)
        {
            if (CurrentTerritory == null) return true;
            if (CurrentTerritory.Flags.HasFlagFast(TerritorySystem_DataTypes.TerritoryFlags.NoInteract) &&
                !CurrentTerritory.IsPermitted())
            {
                DoAreaEffect(go.transform.position);
                return false;
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(ItemDrop), nameof(ItemDrop.Interact))]
    [ClientOnlyPatch]
    private static class NoInteractItems_Patch
    {
        private static bool Prefix()
        {
            if (CurrentTerritory == null) return true;
            if (CurrentTerritory.Flags.HasFlagFast(TerritorySystem_DataTypes.TerritoryFlags.NoInteractItems) &&
                !CurrentTerritory.IsPermitted())
            {
                DoAreaEffect(Player.m_localPlayer.transform.position);
                return false;
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(TeleportWorld), nameof(TeleportWorld.Interact))]
    [ClientOnlyPatch]
    private static class NoInteractPortals_Patch
    {
        private static bool Prefix()
        {
            if (CurrentTerritory == null) return true;
            if (CurrentTerritory.Flags.HasFlagFast(TerritorySystem_DataTypes.TerritoryFlags.NoInteractPortals) &&
                !CurrentTerritory.IsPermitted())
            {
                DoAreaEffect(Player.m_localPlayer.transform.position);
                return false;
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(CraftingStation), nameof(CraftingStation.Interact))]
    [ClientOnlyPatch]
    private static class NoInteractCraftingStation_Patch
    {
        private static bool Prefix()
        {
            if (CurrentTerritory == null) return true;
            if (CurrentTerritory.Flags.HasFlagFast(TerritorySystem_DataTypes.TerritoryFlags
                    .NoInteractCraftingStation) &&
                !CurrentTerritory.IsPermitted())
            {
                DoAreaEffect(Player.m_localPlayer.transform.position);
                return false;
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(ItemStand), nameof(ItemStand.Interact))]
    [ClientOnlyPatch]
    private static class NoInteractItemStands_Patch
    {
        private static bool Prefix()
        {
            if (CurrentTerritory == null) return true;
            if (CurrentTerritory.Flags.HasFlagFast(TerritorySystem_DataTypes.TerritoryFlags.NoInteractItemStands) &&
                !CurrentTerritory.IsPermitted())
            {
                DoAreaEffect(Player.m_localPlayer.transform.position);
                return false;
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(ArmorStand), nameof(ArmorStand.UseItem))]
    [ClientOnlyPatch]
    private static class NoInteractItemStands2_Patch
    {
        private static bool Prefix()
        {
            if (CurrentTerritory == null) return true;
            if (CurrentTerritory.Flags.HasFlagFast(TerritorySystem_DataTypes.TerritoryFlags.NoInteractItemStands) &&
                !CurrentTerritory.IsPermitted())
            {
                DoAreaEffect(Player.m_localPlayer.transform.position);
                return false;
            }

            return true;
        }
    }


    [HarmonyPatch(typeof(Container), nameof(Container.Interact))]
    [ClientOnlyPatch]
    private static class NoInteractChests_Patch
    {
        private static bool Prefix()
        {
            if (CurrentTerritory == null) return true;
            if (CurrentTerritory.Flags.HasFlagFast(TerritorySystem_DataTypes.TerritoryFlags.NoInteractChests) &&
                !CurrentTerritory.IsPermitted())
            {
                DoAreaEffect(Player.m_localPlayer.transform.position);
                return false;
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(Door), nameof(Door.Interact))]
    [ClientOnlyPatch]
    private static class NoInteractDoors_Patch
    {
        private static bool Prefix()
        {
            if (CurrentTerritory == null) return true;
            if (CurrentTerritory.Flags.HasFlagFast(TerritorySystem_DataTypes.TerritoryFlags.NoInteractDoors) &&
                !CurrentTerritory.IsPermitted())
            {
                DoAreaEffect(Player.m_localPlayer.transform.position);
                return false;
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(Player), nameof(Player.UseStamina))]
    [ClientOnlyPatch]
    private static class InfiniteStamina_Patch
    {
        private static bool Prefix()
        {
            if (CurrentTerritory == null) return true;
            if (CurrentTerritory.AdditionalFlags.HasFlagFast(TerritorySystem_DataTypes.AdditionalTerritoryFlags
                    .InfiniteStamina))
            {
                return false;
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(WearNTear), nameof(WearNTear.HaveSupport))]
    [ClientOnlyPatch]
    private static class NoStructureSupport_Patch
    {
        private static void Postfix(WearNTear __instance, ref bool __result)
        {
            foreach (TerritorySystem_DataTypes.Territory territory in TerritoriesByFlags[TerritorySystem_DataTypes.TerritoryFlags.NoStructureSupport])
            {
                if (territory.IsInside(__instance.transform.position))
                {
                    __result = true;
                    return;
                }
            }
        }
    }


    [HarmonyPatch(typeof(Player), nameof(Player.HaveEitr))]
    [ClientOnlyPatch]
    private static class InfiniteEitr_Patch
    {
        private static void Postfix(ref bool __result)
        {
            if (CurrentTerritory == null) return;
            if (CurrentTerritory.AdditionalFlags.HasFlagFast(TerritorySystem_DataTypes.AdditionalTerritoryFlags.InfiniteEitr))
            {
                __result = true;
            }
        }
    }

    [HarmonyPatch(typeof(Character), nameof(Character.RPC_Damage))]
    [ClientOnlyPatch]
    private static class Character_RPC_Damage_Patch
    {
        [UsedImplicitly]
        private static bool Prefix(Character __instance)
        {
            if (__instance == Player.m_localPlayer && CurrentTerritory != null)
                if (CurrentTerritory.AdditionalFlags.HasFlagFast(TerritorySystem_DataTypes.AdditionalTerritoryFlags
                        .GodMode))
                    return false;

            return true;
        }
    }

    [HarmonyPatch(typeof(Character), nameof(Character.Damage))]
    [ClientOnlyPatch]
    private static class IncreaseTerritoryDamagePatch
    {
        private static void Prefix(Character __instance, ref HitData hit)
        {
            if (CurrentTerritory == null) return;
            if (__instance.IsPlayer())
            {
                if (CurrentTerritory.Flags.HasFlagFast(TerritorySystem_DataTypes.TerritoryFlags.IncreasedMonsterDamage))
                {
                    hit.ApplyModifier(CurrentTerritory.IncreasedMonsterDamageValue);
                }
            }
            else
            {
                if (CurrentTerritory.Flags.HasFlagFast(TerritorySystem_DataTypes.TerritoryFlags.IncreasedPlayerDamage))
                {
                    hit.ApplyModifier(CurrentTerritory.IncreasedPlayerDamageValue);
                }
            }
        }
    }

    [HarmonyPatch(typeof(Attack), nameof(Attack.Start))]
    [ClientOnlyPatch]
    private static class NoAttackPatch
    {
        private static bool Prefix(Humanoid character)
        {
            if (!Player.m_localPlayer || CurrentTerritory == null || character != Player.m_localPlayer)
            {
                return true;
            }

            if (CurrentTerritory.Flags.HasFlagFast(TerritorySystem_DataTypes.TerritoryFlags.NoAttack) &&
                !CurrentTerritory.IsPermitted())
            {
                DoAreaEffect(Player.m_localPlayer.transform.position);
                return false;
            }

            return true;
        }
    }


    [HarmonyPatch(typeof(CreatureSpawner), nameof(CreatureSpawner.Spawn))]
    [ClientOnlyPatch]
    private static class NoMonsters_Start_Patch
    {
        private static bool Prefix(CreatureSpawner __instance)
        {
            foreach (TerritorySystem_DataTypes.Territory territory in TerritoriesByFlags[
                         TerritorySystem_DataTypes.TerritoryFlags.NoMonsters])
            {
                if (territory.IsInside(__instance.transform.position))
                    return false;
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(SpawnSystem), nameof(SpawnSystem.UpdateSpawning))]
    [ClientOnlyPatch]
    private static class NoMonsters_Start_Patch2
    {
        private static bool Prefix()
        {
            if (CurrentTerritory == null) return true;
            if (CurrentTerritory.Flags.HasFlagFast(TerritorySystem_DataTypes.TerritoryFlags.NoMonsters))
            {
                return false;
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(SpawnSystem), nameof(SpawnSystem.Spawn))]
    [ClientOnlyPatch]
    private static class SpawnSystem_Spawn_Patch
    {
        private static bool Prefix(Vector3 spawnPoint)
        {
            foreach (TerritorySystem_DataTypes.Territory territory in TerritoriesByFlags[
                         TerritorySystem_DataTypes.TerritoryFlags.NoMonsters])
            {
                if (territory.IsInside(spawnPoint))
                    return false;
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(Player), nameof(Player.GetRunSpeedFactor))]
    [ClientOnlyPatch]
    private static class MoveSpeed_Patch_Territory
    {
        private static void Postfix(ref float __result)
        {
            if (CurrentTerritory == null) return;
            if (CurrentTerritory.Flags.HasFlagFast(TerritorySystem_DataTypes.TerritoryFlags.MoveSpeedMultiplier))
            {
                __result *= CurrentTerritory.MoveSpeedMultiplier;
            }
        }
    }

    [HarmonyPatch(typeof(Player), nameof(Player.GetJogSpeedFactor))]
    [ClientOnlyPatch]
    private static class MoveSpeed2_Patch_Territory
    {
        private static void Postfix(ref float __result)
        {
            if (CurrentTerritory == null) return;
            if (CurrentTerritory.Flags.HasFlagFast(TerritorySystem_DataTypes.TerritoryFlags.MoveSpeedMultiplier))
            {
                __result *= CurrentTerritory.MoveSpeedMultiplier;
            }
        }
    }

    [HarmonyPatch(typeof(Player), nameof(Player.HardDeath))]
    [ClientOnlyPatch]
    private static class Skills_OnDeath_Patch
    {
        private static void Postfix(ref bool __result)
        {
            if (CurrentTerritory == null) return;
            if (CurrentTerritory.Flags.HasFlagFast(TerritorySystem_DataTypes.TerritoryFlags.NoDeathPenalty))
            {
                __result = false;
            }
        }
    }

    [HarmonyPatch(typeof(Player), nameof(Player.AutoPickup))]
    [ClientOnlyPatch]
    private static class Player_AutoPickup_Patch
    {
        private static bool Prefix()
        {
            if (CurrentTerritory == null) return true;
            if ((CurrentTerritory.Flags.HasFlagFast(TerritorySystem_DataTypes.TerritoryFlags.NoInteractItems) ||
                 CurrentTerritory.Flags.HasFlagFast(TerritorySystem_DataTypes.TerritoryFlags.NoInteract)) &&
                !CurrentTerritory.IsPermitted())
            {
                return false;
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(Player), nameof(Player.TeleportTo))]
    [ClientOnlyPatch]
    private static class NoPortals_Patch
    {
        private static bool Prefix(Vector3 pos)
        {
            if (Teleporter_Main_Client.DEBUG_TELEPORTTO_TERRITORY)
            {
                Teleporter_Main_Client.DEBUG_TELEPORTTO_TERRITORY = false;
                return true;
            }

            foreach (TerritorySystem_DataTypes.Territory noportals in TerritoriesByFlags[
                         TerritorySystem_DataTypes.TerritoryFlags.NoPortals])
            {
                if (noportals.IsPermitted() || (!noportals.IsInside(Player.m_localPlayer.transform.position) &&
                                                !noportals.IsInside(pos))) continue;
                DoAreaEffect(Player.m_localPlayer.transform.position + Vector3.up);
                MessageHud.instance.ShowMessage(MessageHud.MessageType.TopLeft,
                    Localization.instance.Localize("$mpasn_cantteleport"));
                return false;
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(WorldGenerator), nameof(WorldGenerator.GetBiomeHeight))]
    [ClientOnlyPatch]
    private static class WorldGenerator_GetBiomeHeight_Patch
    {
        private static void Postfix(float wx, float wy, ref float __result)
        {
            if (Minimap_GenerateWorldMap_Patch.GENERATING) return;
            Vector2 vec = new Vector2(wx, wy);

            foreach (TerritorySystem_DataTypes.Territory territory in TerritoriesByFlags[
                         TerritorySystem_DataTypes.TerritoryFlags.ForceGroundHeight])
            {
                if (territory.IsInside2D(vec))
                {
                    __result = territory.OverridenHeight;
                    //break;
                }
            }

            foreach (TerritorySystem_DataTypes.Territory territory in TerritoriesByFlags[
                         TerritorySystem_DataTypes.TerritoryFlags.AddGroundHeight])
            {
                if (territory.IsInside2D(vec))
                {
                    __result += territory.OverridenHeight;
                    //break;
                }
            }

            foreach (TerritorySystem_DataTypes.Territory territory in TerritoriesByFlags[
                         TerritorySystem_DataTypes.TerritoryFlags.LimitZoneHeight])
            {
                if (territory.IsInside2D(vec))
                {
                    __result = Mathf.Max(territory.OverridenHeight, __result);
                    //break;
                }
            }
        }
    }

    [HarmonyPatch(typeof(WorldGenerator), nameof(WorldGenerator.GetBiome), typeof(float), typeof(float), typeof(float), typeof(bool))]
    [ClientOnlyPatch]
    private static class WorldGenerator_GetBiome_Patch
    {
        private static void Postfix(float wx, float wy, ref Heightmap.Biome __result)
        {
            if (Minimap_GenerateWorldMap_Patch.GENERATING) return;
            Vector2 vec = new Vector2(wx, wy);
            foreach (TerritorySystem_DataTypes.Territory ground in TerritoriesByFlags[
                         TerritorySystem_DataTypes.TerritoryFlags.ForceBiome])
            {
                if (ground.IsInside2D(vec))
                {
                    __result = (Heightmap.Biome)ground.OverridenBiome;
                    break;
                }
            }
        }
    }

    [HarmonyPatch(typeof(CharacterDrop), nameof(CharacterDrop.DropItems))]
    [ClientOnlyPatch]
    private static class CharacterDrop_DropItems_Patch
    {
        private static void Prefix(ref List<KeyValuePair<GameObject, int>> drops)
        {
            if (CurrentTerritory == null ||
                !CurrentTerritory.AdditionalFlags.HasFlagFast(TerritorySystem_DataTypes.AdditionalTerritoryFlags
                    .DropMultiplier)) return;

            for (int i = 0; i < drops.Count; ++i)
                drops[i] = new KeyValuePair<GameObject, int>(drops[i].Key,
                    Mathf.RoundToInt(drops[i].Value * CurrentTerritory.DropMultiplier));
        }
    }


    [HarmonyPatch(typeof(Fireplace), nameof(Fireplace.UpdateFireplace))]
    [ClientOnlyPatch]
    private static class UnlimitedFuel1
    {
        private static void Postfix(Fireplace __instance)
        {
            if (CurrentTerritory == null || !__instance.m_nview.IsValid() || !__instance.m_nview.IsOwner() ||
                !CurrentTerritory.Flags.HasFlagFast(TerritorySystem_DataTypes.TerritoryFlags.InfiniteFuel)) return;
            __instance.m_nview.m_zdo.Set("fuel", __instance.m_maxFuel);
        }
    }


    [HarmonyPatch(typeof(Smelter), nameof(Smelter.UpdateSmelter))]
    [ClientOnlyPatch]
    private static class UnlimitedFuel2
    {
        private static void Postfix(Smelter __instance)
        {
            if (CurrentTerritory == null || !__instance.m_nview.IsValid() || !__instance.m_nview.IsOwner() ||
                !CurrentTerritory.Flags.HasFlagFast(TerritorySystem_DataTypes.TerritoryFlags.InfiniteFuel)) return;
            __instance.m_nview.m_zdo.Set("fuel", __instance.m_maxFuel);
        }
    }

    [HarmonyPatch(typeof(CookingStation), nameof(CookingStation.UpdateCooking))]
    [ClientOnlyPatch]
    private static class UnlimitedFuel3
    {
        private static void Postfix(CookingStation __instance)
        {
            if (CurrentTerritory == null || !__instance.m_nview.IsValid() || !__instance.m_nview.IsOwner() ||
                !CurrentTerritory.Flags.HasFlagFast(TerritorySystem_DataTypes.TerritoryFlags.InfiniteFuel)) return;
            __instance.m_nview.m_zdo.Set("fuel", __instance.m_maxFuel);
        }
    }

    [HarmonyPatch(typeof(Player), nameof(Player.CreateTombStone))]
    [ClientOnlyPatch]
    private static class Player_CreateTombStone_Patch
    {
        private static bool Prefix()
        {
            return CurrentTerritory == null ||
                   !CurrentTerritory.AdditionalFlags.HasFlagFast(TerritorySystem_DataTypes.AdditionalTerritoryFlags
                       .NoItemLoss);
        }
    }

    [HarmonyPatch(typeof(EnvMan), nameof(EnvMan.SetTargetWind))]
    [ClientOnlyPatch]
    private static class EnvMan_SetTargetWind_Patch
    {
        private static void ToCall()
        {
            if (CurrentTerritory == null) return;
            if (CurrentTerritory.AdditionalFlags.HasFlagFast(TerritorySystem_DataTypes.AdditionalTerritoryFlags
                    .ForceWind))
            {
                EnvMan.instance.m_windDir2.w = CurrentTerritory.Wind;
            }
        }

        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> code)
        {
            List<CodeInstruction> codeList = code.ToList();
            codeList.Insert(codeList.Count - 1,
                new CodeInstruction(OpCodes.Call,
                    AccessTools.Method(typeof(EnvMan_SetTargetWind_Patch), nameof(ToCall))));
            return codeList;
        }
    }

    [HarmonyPatch(typeof(Heightmap), nameof(Heightmap.ApplyModifiers))]
    [ClientOnlyPatch]
    private static class TerrainComp_ApplyToHeightmap_Patch
    {
        private static void Postfix(Heightmap __instance)
        {
            if (__instance.m_isDistantLod ||
                TerritoriesByFlags[TerritorySystem_DataTypes.TerritoryFlags.CustomPaint].Count == 0)
            {
                return;
            }

            Vector3 vector = __instance.transform.position -
                             new Vector3(__instance.m_width * __instance.m_scale * 0.5f, 0f,
                                 __instance.m_width * __instance.m_scale * 0.5f);
            bool invoke = false;
            for (int x = 0; x < __instance.m_width; ++x)
            {
                for (int z = 0; z < __instance.m_width; ++z)
                {
                    float FinalX = vector.x + x;
                    float FinalZ = vector.z + z;
                    foreach (TerritorySystem_DataTypes.Territory paint in TerritoriesByFlags[
                                 TerritorySystem_DataTypes.TerritoryFlags.CustomPaint])
                    {
                        if (!paint.IsInside2D(new Vector2(FinalX, FinalZ))) continue;
                        Color c = paint.PaintGround switch
                        {
                            TerritorySystem_DataTypes.PaintType.Paved => Color.blue,
                            TerritorySystem_DataTypes.PaintType.Grass => Color.black,
                            TerritorySystem_DataTypes.PaintType.Cultivated => Color.green,
                            TerritorySystem_DataTypes.PaintType.Dirt => Color.red,
                            _ => Color.black
                        };
                        invoke = true;
                        __instance.m_paintMask.SetPixel(x, z, c);
                        break;
                    }
                }
            }

            if (invoke)
            {
                __instance.m_paintMask.Apply();
            }
        }
    }


    [HarmonyPatch(typeof(Heightmap), nameof(Heightmap.RebuildRenderMesh))]
    [ClientOnlyPatch]
    private static class TerrainComp_ApplyToHeightmap_Patch_SnowMask
    {
        private static void Postfix(Heightmap __instance)
        {
            if (TerritoriesByFlags_Additional[TerritorySystem_DataTypes.AdditionalTerritoryFlags.SnowMask].Count == 0)
            {
                return;
            }

            Vector3 vector = __instance.transform.position - new Vector3(__instance.m_width * __instance.m_scale * 0.5f,
                0f, __instance.m_width * __instance.m_scale * 0.5f);
            int num = __instance.m_width + 1;
            bool invoke = false;
            Heightmap.s_tempColors.Clear();
            for (int x = 0; x < num; ++x)
            {
                for (int z = 0; z < num; ++z)
                {
                    float FinalX = vector.x + z;
                    float FinalZ = vector.z + x;
                    foreach (TerritorySystem_DataTypes.Territory paint in TerritoriesByFlags_Additional[
                                 TerritorySystem_DataTypes.AdditionalTerritoryFlags.SnowMask])
                    {
                        if (!paint.IsInside2D(new Vector2(FinalX, FinalZ)))
                        {
                            Heightmap.Biome biome = WorldGenerator.instance.GetBiome(FinalX, FinalZ);
                            Heightmap.s_tempColors.Add(Heightmap.GetBiomeColor(biome));
                        }
                        else
                        {
                            Heightmap.s_tempColors.Add(Heightmap.GetBiomeColor(Heightmap.Biome.Mountain));
                            invoke = true;
                        }

                        break;
                    }
                }
            }

            if (invoke)
            {
                __instance.m_renderMesh.SetColors(Heightmap.s_tempColors);
            }
        }
    }


    [HarmonyPatch(typeof(Minimap), nameof(Minimap.OnMapLeftClick))]
    [ClientOnlyPatch]
    private static class PatchClickIconMinimap
    {
        [UsedImplicitly]
        private static bool Prefix()
        {
            Vector3 pos = Minimap.instance.ScreenToWorldPoint(Input.mousePosition);
            TerritorySystem_DataTypes.Territory forSaleTerritory = ForSaleTerritories.FirstOrDefault(t => t.IsInside(pos));
            if (forSaleTerritory != null)
            {
                if (string.IsNullOrWhiteSpace(forSaleTerritory.Owner))
                {
                    UnifiedPopup.Push(new YesNoPopup($"Territory purchase", $"Do you wanna buy this territory for <color=yellow>{forSaleTerritory.SellPrice} Coins</color> ?",
                        () =>
                        {
                            UnifiedPopup.Pop();
                            ZRoutedRpc.instance.InvokeRoutedRPC("KGmarket TerritorySystem GetOwnership", forSaleTerritory.UID);
                        },
                        () => { UnifiedPopup.Pop(); }));
                }
               

                if (forSaleTerritory.Owner == Global_Configs._localUserID)
                {
                    UnifiedPopup.Push(new YesNoPopup($"Cancel territory ownership", $"Do you wanna cancel this territory ownership ?",
                        () =>
                        {
                            UnifiedPopup.Pop();
                            ZRoutedRpc.instance.InvokeRoutedRPC("KGmarket TerritorySystem CancelOwnership", forSaleTerritory.UID);
                        },
                        () => { UnifiedPopup.Pop(); }));
                }

                return false;
            }

            return true;
        }
    }
}