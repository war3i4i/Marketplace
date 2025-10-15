using Marketplace.ExternalLoads;
using Object = UnityEngine.Object;

namespace Marketplace.Modules.TerritorySystem;

[UsedImplicitly]
[Market_Autoload(Market_Autoload.Type.Client, Market_Autoload.Priority.First)]
public static class ZoneVisualizer
{
    private static GameObject ZoneVisualizer_Square, ZoneVisualizer_Circle;
    public static readonly List<GameObject> Visualizers = new();
    public static float VisualizerAlpha = 0.2f;

    private static void OnInit()
    {
        ZoneVisualizer_Square = AssetStorage.asset.LoadAsset<GameObject>("SquareZoneVisualizer");
        ZoneVisualizer_Circle = AssetStorage.asset.LoadAsset<GameObject>("CircleZoneVisualizer"); 
    }

    public static void OnMapChange()
    {
        if (Visualizers.Count > 0 || TerritorySystem_Main_Client.AlwaysShowZoneVisualizer.Value && ZoneVisualizer_Square)
        {
            On();
        }
    }
 
    public static void On()
    {
        Visualizers.ForEach(x =>
        {
            if (x) Object.Destroy(x);
        });
        Visualizers.Clear();

        foreach (TerritorySystem_DataTypes.Territory territory in TerritorySystem_DataTypes.SyncedTerritoriesData.Value.Territories)
        {
            if(territory.Colors.Count == 0) continue;
            
            GameObject visualizer = Object.Instantiate(territory.Shape is TerritorySystem_DataTypes.TerritoryShape.Circle
                ? ZoneVisualizer_Circle
                : ZoneVisualizer_Square);

            visualizer.transform.position = territory.Shape switch
            {
                TerritorySystem_DataTypes.TerritoryShape.Circle => territory.Pos3D(),
                TerritorySystem_DataTypes.TerritoryShape.Square => territory.Pos3D(),
                TerritorySystem_DataTypes.TerritoryShape.Rectangle => territory.Pos3D() +
                                                                  new Vector3(territory.X[1] / 2f, 0,
                                                                      territory.Y[1] / 2f),
                _ => territory.Pos3D()
            };

            float heightScale = territory.HeightBounds.Item2 - territory.HeightBounds.Item1;
            visualizer.transform.localScale = territory.Shape switch
            {
                TerritorySystem_DataTypes.TerritoryShape.Circle => new Vector3(territory.Radius * 2f, heightScale,
                    territory.Radius * 2f),
                TerritorySystem_DataTypes.TerritoryShape.Square => new Vector3(territory.Radius * 2f, heightScale,
                    territory.Radius * 2f),
                TerritorySystem_DataTypes.TerritoryShape.Rectangle => new Vector3(territory.X[1], heightScale,
                    territory.Y[1]),
                _ => new Vector3(territory.Radius, heightScale, territory.Radius),
            };

            Color color = territory.GetColor();
            color.a = VisualizerAlpha;
            visualizer.GetComponentInChildren<MeshRenderer>().material.color = color;
            Visualizers.Add(visualizer);
        }

        EnvMan.instance?.transform.Find("Clouds").gameObject.SetActive(false);
    }

    public static void Off()
    {
        Visualizers.ForEach(x =>
        {
            if (x) Object.Destroy(x);
        });
        Visualizers.Clear();
        EnvMan.instance?.transform.Find("Clouds").gameObject.SetActive(false);
    }

    [HarmonyPatch(typeof(FejdStartup), nameof(FejdStartup.Awake))]
    [ClientOnlyPatch]
    private static class FejdStartup_Awake_Patch
    {
        private static void Postfix() => Off();
    }
    
    
}