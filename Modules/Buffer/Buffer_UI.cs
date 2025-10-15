using Marketplace.ExternalLoads;

namespace Marketplace.Modules.Buffer;

public static class Buffer_UI
{
    private static GameObject UI = null!;
    private static GameObject BufferGroup = null!;
    private static GameObject BufferItem = null!;
    private static Transform MainTransform = null!;
    private static Text NPCName = null!;
    private static Scrollbar Scrollbar = null!;
    private static string CurrentProfile = "";
    private static GameObject Explosion = null!;
    private static readonly List<GameObject> AllElements = new();
    private static readonly Dictionary<GameObject, Buffer_DataTypes.BufferBuffData> tempBuffData = new();


    public static bool IsVisible()
    {
        return UI && UI.activeSelf;
    }


    public static void Init()
    {
        UI = UnityEngine.Object.Instantiate(AssetStorage.asset.LoadAsset<GameObject>("BufferHud"));
        Explosion = AssetStorage.asset.LoadAsset<GameObject>("MarketplaceBuffExplosion");
        BufferGroup = AssetStorage.asset.LoadAsset<GameObject>("BufferGroup");
        BufferItem = AssetStorage.asset.LoadAsset<GameObject>("BufferListItem");
        MainTransform = UI.transform.Find("Canvas/Buffer/BufferList/ListPanel/Scroll View/Viewport/Content");
        Scrollbar = UI.GetComponentInChildren<Scrollbar>();
        UnityEngine.Object.DontDestroyOnLoad(UI);
        UI.SetActive(false);
        NPCName = UI.transform.Find("Canvas/Buffer/Header/Text").GetComponent<Text>();
        Localization.instance.Localize(UI.transform);
    }

    public static string GetFlagsText(Buffer_DataTypes.BufferBuffData data)
    {
        string result = "";
        foreach (Buffer_DataTypes.WhatToModify mod in Buffer_DataTypes.BufferModifyList)
        {
            if (data.Flags.HasFlagFast(mod))
            {
                switch (mod)
                {
                    case Buffer_DataTypes.WhatToModify.None:
                        break;
                    case Buffer_DataTypes.WhatToModify.ModifyAttack:
                        result +=
                            $"{Localization.instance.Localize("$mpasn_modifyattack")}: <color=#00FFFF>x{data.ModifyList[mod]}</color>\n";
                        break;
                    case Buffer_DataTypes.WhatToModify.ModifyHealthRegen:
                        result +=
                            $"{Localization.instance.Localize("$mpasn_healthregenmodifier")}: <color=#00FFFF>x{data.ModifyList[mod]}</color>\n";
                        break;
                    case Buffer_DataTypes.WhatToModify.ModifyStaminaRegen:
                        result +=
                            $"{Localization.instance.Localize("$mpasn_staminaregenmodifier")}: <color=#00FFFF>x{data.ModifyList[mod]}</color>\n";
                        break;
                    case Buffer_DataTypes.WhatToModify.ModifyRaiseSkills:
                        result +=
                            $"{Localization.instance.Localize("$mpasn_modifyraisingskills")}: <color=#00FFFF>x{data.ModifyList[mod]}</color>\n";
                        break;
                    case Buffer_DataTypes.WhatToModify.ModifySpeed:
                        result +=
                            $"{Localization.instance.Localize("$mpasn_movementspeedmodifier")}: <color=#00FFFF>x{data.ModifyList[mod]}</color>\n";
                        break;
                    case Buffer_DataTypes.WhatToModify.ModifyNoise:
                        result +=
                            $"{Localization.instance.Localize("$mpasn_noisemodifier")}: <color=#00FFFF>x{data.ModifyList[mod]}</color>\n";
                        break;
                    case Buffer_DataTypes.WhatToModify.ModifyMaxCarryWeight:
                        result +=
                            $"{Localization.instance.Localize("$mpasn_maxcarryweight")}: <color=#00FFFF>+{data.ModifyList[mod]}</color>\n";
                        break;
                    case Buffer_DataTypes.WhatToModify.ModifyStealth:
                        result +=
                            $"{Localization.instance.Localize("$mpasn_stealthbonus")}: <color=#00FFFF>x{data.ModifyList[mod]}</color>\n";
                        break;
                    case Buffer_DataTypes.WhatToModify.RunStaminaDrain:
                        result +=
                            $"{Localization.instance.Localize("$mpasn_runstaminadrain")}: <color=#00FFFF>x{data.ModifyList[mod]}</color>\n";
                        break;
                    case Buffer_DataTypes.WhatToModify.DamageReduction:
                        result +=
                            $"{Localization.instance.Localize("$mpasn_damagereduction")}: <color=#00FFFF>-{(int)(data.ModifyList[mod] * 100)}%</color>\n";
                        break;
                }
            }
        }

        return result;
    }

    private static void SetColors()
    {
        foreach (KeyValuePair<GameObject, Buffer_DataTypes.BufferBuffData> kvp in tempBuffData)
        {
            Image buttonImage = kvp.Key.transform.Find("Button").GetComponent<Image>();
            buttonImage.color = kvp.Value.CanTake() ? Color.green : Color.red;
        }
    }


    private static void ApplyBuff(Buffer_DataTypes.BufferBuffData buff)
    {
        AssetStorage.AUsrc.Play();
        if (!Player.m_localPlayer) return;
        if (buff.CanTake())
        {
            Player.m_localPlayer.m_seman.AddStatusEffect(buff.GetMain(), true);
            Player.m_localPlayer.m_inventory.RemoveItem(buff.GetItemName(), buff.NeededPrefabCount);
            UnityEngine.Object.Instantiate(Explosion, Player.m_localPlayer.transform.position + Vector3.up, Quaternion.identity);
            SetColors();
        }
    }


    private static void CreateElements()
    {
        tempBuffData.Clear();
        AllElements.ForEach(UnityEngine.Object.Destroy);
        AllElements.Clear();
        if (!IsVisible() || !Buffer_DataTypes.ClientSideBufferProfiles.ContainsKey(CurrentProfile)) return;
        Dictionary<string, List<Buffer_DataTypes.BufferBuffData>> tempDictionary = new();
        foreach (Buffer_DataTypes.BufferBuffData buff in Buffer_DataTypes.ClientSideBufferProfiles[CurrentProfile])
        {
            string group = buff.BuffGroup!;
            if (string.IsNullOrEmpty(group)) group = "No Group";
            if (!tempDictionary.ContainsKey(group)) tempDictionary[group] = new List<Buffer_DataTypes.BufferBuffData>();
            tempDictionary[group].Add(buff);
        }

        foreach (KeyValuePair<string, List<Buffer_DataTypes.BufferBuffData>> group in tempDictionary)
        {
            GameObject newGroup = UnityEngine.Object.Instantiate(BufferGroup, MainTransform);
            newGroup.transform.Find("HeaderSeparator/Text").GetComponent<Text>().text =
                $"<color=yellow>{group.Key}</color>";
            Transform gTransform = newGroup.transform.Find("Content");

            foreach (Buffer_DataTypes.BufferBuffData buff in group.Value)
            {
                GameObject newItem = UnityEngine.Object.Instantiate(BufferItem, gTransform);
                newItem.transform.Find("Name").GetComponent<Text>().text = buff.Name;
                newItem.transform.Find("Time").GetComponent<Text>().text =
                    $"{Localization.instance.Localize("$mpasn_duration")}: {buff.Duration}";
                newItem.transform.Find("Description").GetComponent<Text>().text = GetFlagsText(buff);
                newItem.transform.Find("Image").GetComponent<Image>().sprite = buff.GetIcon();
                newItem.transform.Find("PriceImage").GetComponent<Image>().sprite = buff.GetIconNeeded();
                newItem.transform.Find("PriceText").GetComponent<Text>().text = $"x {buff.NeededPrefabCount}";
                newItem.transform.Find("Button/Text").GetComponent<Text>().text =
                    Localization.instance.Localize("$mpasn_bufferbuy");
                Button button = newItem.transform.Find("Button").GetComponent<Button>();
                button.onClick.AddListener(() => { ApplyBuff(buff); });
                tempBuffData.Add(newItem, buff);
                SetColors();
            }

            AllElements.Add(newGroup);
        }

        Canvas.ForceUpdateCanvases();
        List<ContentSizeFitter> AllFilters = MainTransform.GetComponentsInChildren<ContentSizeFitter>().ToList();
        AllFilters.ForEach(filter => filter.enabled = false);
        AllFilters.ForEach(filter => filter.enabled = true);
    }


    public static void Reload()
    {
        if (!IsVisible()) return;
        if (!Buffer_DataTypes.ClientSideBufferProfiles.ContainsKey(CurrentProfile))
            Hide();
        else
            CreateElements();
    }

    public static void Show(string profile, string _npcName)
    {
        if (!Buffer_DataTypes.ClientSideBufferProfiles.ContainsKey(profile)) return;
        UI.SetActive(true);
        NPCName.text = string.IsNullOrEmpty(_npcName) ? Localization.instance.Localize("$mpasn_Buffer") : _npcName;
        CurrentProfile = profile;
        CreateElements();
    }

    public static void Hide()
    {
        tempBuffData.Clear();
        AllElements.ForEach(UnityEngine.Object.Destroy);
        AllElements.Clear();
        Scrollbar.value = 1;
        UI.SetActive(false);
    }
    
    [HarmonyPatch(typeof(ZNetScene), nameof(ZNetScene.Awake))]
    [ClientOnlyPatch]
    private static class ZNetScene_Awake_Patch
    {
        [UsedImplicitly]
private static void Postfix(ZNetScene __instance)
        {
            __instance.m_prefabs.Add(Explosion);
            __instance.m_namedPrefabs.Add(Explosion.name.GetStableHashCode(), Explosion);
        }
    }
    
    [HarmonyPatch(typeof(TextInput), nameof(TextInput.IsVisible))]
    [ClientOnlyPatch]
    private static class BufferUIFix
    {
        [UsedImplicitly]
        private static void Postfix(ref bool __result)
        {
            if (IsVisible()) __result = true;
        }
    }
    
}



