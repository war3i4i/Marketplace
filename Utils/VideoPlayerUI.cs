using System.Runtime.Remoting.Messaging;
using Marketplace.ExternalLoads;
using UnityEngine.Video;

namespace Marketplace;

[Market_Autoload(Market_Autoload.Type.Client, Market_Autoload.Priority.Last)]
public static class VideoPlayerUI
{
    private static GameObject UI;
    private static VideoPlayer Player;

    public static bool IsVisible => UI && UI.activeSelf;
    
    [UsedImplicitly]
    private static void OnInit()
    {
        UI = UnityEngine.Object.Instantiate(AssetStorage.asset.LoadAsset<GameObject>("MVideoPlayerUI"));
        UnityEngine.Object.DontDestroyOnLoad(UI);
        Player = UI.transform.Find("Canvas/MainTab/Video").GetComponent<VideoPlayer>();
        UI.SetActive(false);
        Player.isLooping = false;
        Marketplace.Global_Updator += Update;
    }

    private static void Update(float dt)
    {
        if (!IsVisible) return; 
        if (Input.GetKeyDown(KeyCode.Escape)) Hide();
    }

    public static void StartVideo(string name, float multiplier = 0.5f)
    {
        float sizeX = Screen.width * multiplier;
        float sizeY = Screen.height * multiplier;
        (UI.transform.Find("Canvas/MainTab") as RectTransform)!.sizeDelta = new Vector2(sizeX, sizeY);
        Hide();
        if (!AssetStorage.NPC_VideoClips.ContainsKey(name)) return;
        Player.gameObject.SetActive(true);
        UI.SetActive(true);
        Player.url = AssetStorage.NPC_VideoClips[name];
        Player.Play();
    }

    private static void Hide()
    {
        Player.Stop();
        Player.frame = 0;
        Player.url = null;
        Player.gameObject.SetActive(false);
        UI.SetActive(false);
    }
    
    [HarmonyPatch(typeof(TextInput), nameof(TextInput.IsVisible))]
    [ClientOnlyPatch]
    private static class fix1 
    {
        [UsedImplicitly]
        private static void Postfix(ref bool __result) => __result |= IsVisible;
    }
    
    [HarmonyPatch(typeof(StoreGui), nameof(StoreGui.IsVisible))]
    [ClientOnlyPatch]
    private static class fix2
    {
        [UsedImplicitly]
        private static void Postfix(ref bool __result) => __result |= IsVisible;
    }
}