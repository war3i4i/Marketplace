using Marketplace.ExternalLoads;

namespace Marketplace.Modules.Feedback;

public static class Feedback_UI
{
    private static GameObject UI = null!;
    private static InputField Subject = null!;
    private static InputField MSG = null!; 

    public static bool IsPanelVisible()
    {
        return UI && UI.activeSelf;
    }

    public static void Init() 
    {
        UI = UnityEngine.Object.Instantiate(AssetStorage.asset.LoadAsset<GameObject>("MarketFeedback"));
        UnityEngine.Object.DontDestroyOnLoad(UI);
        Subject = UI.transform.Find("UI/panel/Subject").GetComponent<InputField>();
        MSG = UI.transform.Find("UI/panel/Text").GetComponent<InputField>();
        MSG.onValueChanged.AddListener(PlayTypeSound);
        Subject.onValueChanged.AddListener(PlayTypeSound);
        UI.transform.Find("UI/panel/Back").GetComponent<Button>().onClick.AddListener(CancelClick);
        UI.transform.Find("UI/panel/Send").GetComponent<Button>().onClick.AddListener(SendClick);
        UI.SetActive(false);
    }
    
    [HarmonyPatch(typeof(TextInput), nameof(TextInput.IsVisible))]
    [ClientOnlyPatch]
    private static class INPUTPATCHforFeedback
    {
        [UsedImplicitly]
        private static void Postfix(ref bool __result) 
        {
            if (IsPanelVisible()) __result = true;
        }
    } 

    private static void PlayTypeSound(string arg0)
    {
        AssetStorage.AUsrc.PlayOneShot(AssetStorage.TypeClip, 0.7f);
    }


    private static void SendClick()
    {
        AssetStorage.AUsrc.Play();
        if (string.IsNullOrWhiteSpace(MSG.text)) return;
        string playername = Player.m_localPlayer?.GetPlayerName()!;
        ZPackage pkg = new ZPackage();
        pkg.Write(playername);
        pkg.Write(Subject.text);
        pkg.Write(MSG.text);
        ZRoutedRpc.instance.InvokeRoutedRPC("KGmarket ReceiveFeedback", pkg);
        Hide();
    }

    private static void CancelClick()
    {
        AssetStorage.AUsrc.Play();
        Hide();
    }


    public static void Hide()
    {
        UI.SetActive(false);
    }

    public static void Show()
    {
        Subject.text = "";
        MSG.text = "";
        UI.SetActive(true);
    }
}