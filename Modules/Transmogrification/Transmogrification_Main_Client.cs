using ItemDataManager;
using Marketplace.ExternalLoads;

namespace Marketplace.Modules.Transmogrification;

[UsedImplicitly]
[Market_Autoload(Market_Autoload.Type.Client, Market_Autoload.Priority.Normal)]
public static class Transmogrification_Main_Client
{
    private static GameObject Transmog_UI_Icon_Part;

    public static readonly
        Dictionary<string, Dictionary<ItemDrop.ItemData.ItemType, List<Transmogrification_DataTypes.TransmogItem_Data>>>
        FilteredTransmogData = new();

    private static void OnInit()
    {
        Transmogrification_UI.Init();
        Transmog_UI_Icon_Part = AssetStorage.asset.LoadAsset<GameObject>("TransmogUI_Part");
        Transmogrification_DataTypes.SyncedTransmogData.ValueChanged += InitTransmogData;
        Marketplace.Global_Updator += Update;
    }

    private static void Update(float dt)
    {
        if (!Input.GetKeyDown(KeyCode.Escape) || !Transmogrification_UI.IsVisble()) return;
        Transmogrification_UI.Hide();
        Menu.instance.OnClose();
    }

    [HarmonyPatch(typeof(ZNetScene), nameof(ZNetScene.Awake))]
    [ClientOnlyPatch]
    private static class ZNetScene_Awake_Patch
    {
        private static void Postfix() => InitTransmogData();
    }

    public static void InitTransmogData()
    {
        if (!ZNetScene.instance) return;
        FilteredTransmogData.Clear();
        foreach (KeyValuePair<string, List<Transmogrification_DataTypes.TransmogItem_Data>> profile in
                 Transmogrification_DataTypes.SyncedTransmogData.Value)
        {
            if (!FilteredTransmogData.ContainsKey(profile.Key))
                FilteredTransmogData.Add(profile.Key,
                    new Dictionary<ItemDrop.ItemData.ItemType, List<Transmogrification_DataTypes.TransmogItem_Data>>());
            foreach (Transmogrification_DataTypes.TransmogItem_Data data in profile.Value)
            {
                GameObject prefab = ZNetScene.instance.GetPrefab(data.Prefab);
                GameObject priceItem = ZNetScene.instance.GetPrefab(data.Price_Prefab);
                if (!prefab || !priceItem) continue;
                ItemDrop.ItemData item = prefab.GetComponent<ItemDrop>().m_itemData;
                data.SetIcon(item.GetIcon());
                string localizedName = Localization.instance.Localize(item.m_shared.m_name);
                data.SetLocalizedName(localizedName);
                data.SetLocalizedPrice(Localization.instance.Localize("$mpasn_transmog_price: <color=#00ff00>") +
                                       Localization.instance.Localize(priceItem.GetComponent<ItemDrop>().m_itemData
                                           .m_shared.m_name) + "</color>");
                data.SetPriceIcon(priceItem.GetComponent<ItemDrop>().m_itemData.m_shared.m_icons[0]);
                if (data.IgnoreCategory)
                {
                    if (FilteredTransmogData[profile.Key].ContainsKey((ItemDrop.ItemData.ItemType)999))
                    {
                        FilteredTransmogData[profile.Key][(ItemDrop.ItemData.ItemType)999].Add(data);
                    }
                    else
                    {
                        FilteredTransmogData[profile.Key]
                            .Add((ItemDrop.ItemData.ItemType)999,
                                new List<Transmogrification_DataTypes.TransmogItem_Data>() { data });
                    }

                    continue;
                }

                if (FilteredTransmogData[profile.Key].ContainsKey(item.m_shared.m_itemType))
                {
                    FilteredTransmogData[profile.Key][item.m_shared.m_itemType].Add(data);
                }
                else
                {
                    FilteredTransmogData[profile.Key].Add(item.m_shared.m_itemType,
                        new List<Transmogrification_DataTypes.TransmogItem_Data>() { data });
                }
            }
        }

        Transmogrification_UI.Reload();
    }


    private static string AddTooltipTransmog(ItemDrop.ItemData item)
    {
        TempItem transmog = item.T_Data();
        string go = "";
        if (!string.IsNullOrEmpty(transmog.ReplacedPrefab))
        {
            go = ZNetScene.instance.GetPrefab(transmog.ReplacedPrefab)?.GetComponent<ItemDrop>()?.m_itemData
                .m_shared.m_name ?? "";
        }

        string result =
            Localization.instance.Localize(
                $"\n<color=#FF00FF>$mpasn_transmog_transmogrifiedinfo: <color=#00FFFF>{go}</color></color>");

        if (!string.IsNullOrEmpty(transmog.ItemColor))
        {
            if (ColorUtility.TryParseHtmlString(transmog.ItemColor, out Color color))
                result +=
                    $"\n<color=#FF00FF>Color: <color=#{ColorUtility.ToHtmlStringRGB(color)}>{transmog.ItemColor}</color></color>";
        }

        return result;
    }

    public static bool HasTransmog(this ItemDrop.ItemData item)
    {
        return item?.Data().Get<Transmogrification_DataTypes.TransmogItem_ComponentV2>() != null;
    }

    private class TempItem
    {
        public string ReplacedPrefab;
        public string ItemColor;
        public int Variant;

        public static implicit operator bool(TempItem item) => item != null;
    }

    private static TempItem T_Data(this ItemDrop.ItemData item)
    {
        if (item == null) return null;
        if (item.Data().Get<Transmogrification_DataTypes.TransmogItem_ComponentV2>() is { } t)
            return new TempItem
            {
                ReplacedPrefab = string.IsNullOrEmpty(t.ReplacedPrefab) ? item.m_dropPrefab.name : t.ReplacedPrefab,
                ItemColor = t.ItemColor ?? "", Variant = item.m_variant
            };

        return null;
    }


    [HarmonyPatch(typeof(Humanoid), nameof(Humanoid.SetupVisEquipment))]
    [ClientOnlyPatch]
    private static class Humanoid_SetupVisEquipment_Patch_TRANSMOG
    {
        private static void OneArg_SetItem(Player p, ItemDrop.ItemData item, string colorID, Action<string> invoke)
        {
            TempItem transmog = item.T_Data();
            if (transmog) invoke(transmog.ReplacedPrefab);
        }
        private static void OneArg_SetItem_ZNS(Player p, ItemDrop.ItemData item, string colorID, Action<string> invoke)
        {
            TempItem transmog = item.T_Data();
            if (transmog)
            {
                invoke(transmog.ReplacedPrefab);
                p.m_nview.m_zdo.Set(colorID, transmog.ItemColor);
            }
            else
                p.m_nview.m_zdo.Set(colorID, "");
        }
        private static void TwoArg_SetItem(Player p, ItemDrop.ItemData item, string colorID, Action<string, int> invoke)
        {
            TempItem transmog = item.T_Data();
            if (transmog) invoke(transmog.ReplacedPrefab, transmog.Variant);
        }
        private static void TwoArg_SetItem_ZNS(Player p, ItemDrop.ItemData item, string colorID, Action<string, int> invoke)
        {
            TempItem transmog = item.T_Data();
            if (transmog)
            {
                invoke(transmog.ReplacedPrefab, transmog.Variant);
                p.m_nview.m_zdo.Set(colorID, transmog.ItemColor);
            }
            else
                p.m_nview.m_zdo.Set(colorID, "");
        }

        [HarmonyPriority(-6000)]
        public static void Postfix(Humanoid __instance, VisEquipment visEq, bool isRagdoll)
        {
            Player p = ZNetScene.instance ? Player.m_localPlayer : __instance as Player;
            bool zns = ZNetScene.instance;
            if (__instance == p)
            {
                if (!isRagdoll)
                {
                    if (zns)
                    {
                        TwoArg_SetItem_ZNS(p, p.m_leftItem, "MPASN_TMGleftitemColor", visEq.SetLeftItem);
                        OneArg_SetItem_ZNS(p, p.m_rightItem, "MPASN_TMGrightitemColor", visEq.SetRightItem);
                        TwoArg_SetItem_ZNS(p, p.m_hiddenLeftItem, "MPASN_TMGleftbackitemColor", visEq.SetLeftBackItem);
                        OneArg_SetItem_ZNS(p, p.m_hiddenRightItem, "MPASN_TMGrightbackitemColor", visEq.SetRightBackItem);
                    }
                    else
                    {
                        TwoArg_SetItem(p, p.m_leftItem, "MPASN_TMGleftitemColor", visEq.SetLeftItem);
                        OneArg_SetItem(p, p.m_rightItem, "MPASN_TMGrightitemColor", visEq.SetRightItem);
                        TwoArg_SetItem(p, p.m_hiddenLeftItem, "MPASN_TMGleftbackitemColor", visEq.SetLeftBackItem);
                        OneArg_SetItem(p, p.m_hiddenRightItem, "MPASN_TMGrightbackitemColor", visEq.SetRightBackItem);
                    }
                }

                if (zns)
                {
                    OneArg_SetItem_ZNS(p, p.m_chestItem, "MPASN_TMGchestitemColor", visEq.SetChestItem);
                    OneArg_SetItem_ZNS(p, p.m_legItem, "MPASN_TMGlegitemColor", visEq.SetLegItem);
                    OneArg_SetItem_ZNS(p, p.m_helmetItem, "MPASN_TMGhelmetitemColor", visEq.SetHelmetItem);
                    TwoArg_SetItem_ZNS(p, p.m_shoulderItem, "MPASN_TMGshoulderitemColor", visEq.SetShoulderItem);
                }
                else
                {
                    OneArg_SetItem(p, p.m_chestItem, "MPASN_TMGchestitemColor", visEq.SetChestItem);
                    OneArg_SetItem(p, p.m_legItem, "MPASN_TMGlegitemColor", visEq.SetLegItem);
                    OneArg_SetItem(p, p.m_helmetItem, "MPASN_TMGhelmetitemColor", visEq.SetHelmetItem);
                    TwoArg_SetItem(p, p.m_shoulderItem, "MPASN_TMGshoulderitemColor", visEq.SetShoulderItem);
                }
               
            }
        }
    }

    [HarmonyPatch(typeof(InventoryGrid), nameof(InventoryGrid.Awake))]
    [ClientOnlyPatch]
    public static class InventoryGrid_Awake_Patch
    {
        private static readonly HashSet<GameObject> firsttime = new();
        
        public static void Postfix(InventoryGrid __instance)
        {
            if (!__instance.m_elementPrefab) return;
            if (firsttime.Contains(__instance.m_elementPrefab)) return;
            firsttime.Add(__instance.m_elementPrefab);
            Transform transform = __instance.m_elementPrefab.transform;
            GameObject newIcon = UnityEngine.Object.Instantiate(Transmog_UI_Icon_Part);
            newIcon!.transform.SetParent(transform);
            newIcon.name = "MPASN_Transmog";
            newIcon.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -26);
            newIcon.gameObject.SetActive(false);
        }
    }
    
    [HarmonyPatch(typeof(Hud),nameof(Hud.Awake))]
    [ClientOnlyPatch]
    private static class Hud_Awake_Patch
    {
        public static HotkeyBar barRef;
        
        [UsedImplicitly]
        private static void Postfix(Hud __instance)
        {
            HotkeyBar bar = __instance.m_rootObject.transform.Find("HotKeyBar")?.GetComponent<HotkeyBar>();
            barRef = bar;
            if (!bar) return;
            Transform transform = bar.m_elementPrefab.transform;
            GameObject newIcon = UnityEngine.Object.Instantiate(Transmog_UI_Icon_Part);
            newIcon!.transform.SetParent(transform);
            newIcon.name = "MPASN_Transmog";
            newIcon.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -26);
            newIcon.gameObject.SetActive(false);
        }
    }
    

    [HarmonyPatch(typeof(HotkeyBar), nameof(HotkeyBar.UpdateIcons))]
    [ClientOnlyPatch]
    private static class HotkeyBar_UpdateIcons_Patch
    {
        private static bool FirstInit;

        public static void Postfix(HotkeyBar __instance)
        {
            if (__instance != Hud_Awake_Patch.barRef) return;
            if (!Player.m_localPlayer || Player.m_localPlayer.IsDead()) return;
            foreach (HotkeyBar.ElementData element in __instance.m_elements.Where(element => !element.m_used))
            {
                element.m_go.transform.Find("MPASN_Transmog").gameObject.SetActive(false);
            }

            for (int j = 0; j < __instance.m_items.Count; j++)
            {
                ItemDrop.ItemData itemData = __instance.m_items[j];
                HotkeyBar.ElementData element = __instance.m_elements[itemData.m_gridPos.x];
                element.m_go.transform.Find("MPASN_Transmog").gameObject
                    .SetActive(itemData.HasTransmog());
            }
        }
    }

    [HarmonyPatch(typeof(InventoryGrid), nameof(InventoryGrid.UpdateGui))]
    [ClientOnlyPatch]
    private static class InventoryGrid_UpdateGui_Patch
    {
        public static void Postfix(InventoryGrid __instance)
        {
            int width = __instance.m_inventory.GetWidth();
            foreach (InventoryGrid.Element element in __instance.m_elements)
            {
                if (!element.m_used) element.m_go.transform.Find("MPASN_Transmog")?.gameObject.SetActive(false);
            }

            foreach (ItemDrop.ItemData itemData in __instance.m_inventory.GetAllItems())
            {
                InventoryGrid.Element element =
                    __instance.GetElement(itemData.m_gridPos.x, itemData.m_gridPos.y, width);
                element.m_go.transform.Find("MPASN_Transmog")?.gameObject
                    .SetActive(itemData.HasTransmog());
            }
        }
    }

    [HarmonyPatch(typeof(ItemDrop.ItemData), nameof(ItemDrop.ItemData.GetTooltip), typeof(ItemDrop.ItemData),
        typeof(int), typeof(bool), typeof(float), typeof(int))]
    [ClientOnlyPatch]
    private static class ItemDrop_ItemData_Tooltip__Patch_TRANSMOG
    {
        public static void Postfix(ItemDrop.ItemData item, ref string __result)
        {
            if (item.HasTransmog())
            {
                __result += AddTooltipTransmog(item);
            }
        }
    }

    [HarmonyPatch(typeof(VisEquipment), nameof(VisEquipment.SetRightHandEquipped))]
    [ClientOnlyPatch]
    private static class MockRight
    {
        private static bool Transfer;

        private static void Prefix(VisEquipment __instance, int hash)
        {
            if (__instance.m_currentRightItemHash != hash)
            {
                Transfer = true;
            }
        }

        private static void Postfix(VisEquipment __instance)
        {
            if (!Transfer || !__instance.m_nview || __instance.m_nview.m_zdo == null) return;
            Transfer = false;
            if (__instance.m_rightItemInstance)
            {
                string colorString = __instance.m_nview.m_zdo.GetString("MPASN_TMGrightitemColor");
                if (!string.IsNullOrEmpty(colorString) && ColorUtility.TryParseHtmlString(colorString, out Color c) &&
                    c != Color.white)
                {
                    Utils.SetGOColors(__instance.m_rightItemInstance, c);
                }
            }
        }
    }

    [HarmonyPatch(typeof(VisEquipment), nameof(VisEquipment.SetLeftHandEquipped))]
    [ClientOnlyPatch]
    private static class MockLeft
    {
        private static bool Transfer;

        private static void Prefix(VisEquipment __instance, int hash)
        {
            if (__instance.m_currentLeftItemHash != hash)
            {
                Transfer = true;
            }
        }


        private static void Postfix(VisEquipment __instance)
        {
            if (!Transfer || !__instance.m_nview || __instance.m_nview.m_zdo == null) return;
            Transfer = false;
            if (__instance.m_leftItemInstance)
            {
                string colorString = __instance.m_nview.m_zdo.GetString("MPASN_TMGleftitemColor");
                if (!string.IsNullOrEmpty(colorString) && ColorUtility.TryParseHtmlString(colorString, out Color c) &&
                    c != Color.white)
                {
                    Utils.SetGOColors(__instance.m_leftItemInstance, c);
                }
            }
        }
    }

    [HarmonyPatch(typeof(VisEquipment), nameof(VisEquipment.SetHelmetEquipped))]
    [ClientOnlyPatch]
    private static class MockHelmet
    {
        private static bool Transfer;

        private static void Prefix(VisEquipment __instance, int hash)
        {
            if (__instance.m_currentHelmetItemHash != hash)
            {
                Transfer = true;
            }
        }


        private static void Postfix(VisEquipment __instance)
        {
            if (!Transfer || !__instance.m_nview || __instance.m_nview.m_zdo == null) return;
            Transfer = false;
            if (__instance.m_helmetItemInstance)
            {
                string colorString = __instance.m_nview.m_zdo.GetString("MPASN_TMGhelmetitemColor");
                if (!string.IsNullOrEmpty(colorString) && ColorUtility.TryParseHtmlString(colorString, out Color c) &&
                    c != Color.white)
                {
                    Utils.SetGOColors(__instance.m_helmetItemInstance, c);
                }
            }
        }
    }

    [HarmonyPatch(typeof(VisEquipment), nameof(VisEquipment.SetShoulderEquipped))]
    [ClientOnlyPatch]
    private static class MockCape
    {
        private static bool Transfer;

        private static void Prefix(VisEquipment __instance, int hash)
        {
            if (__instance.m_currentShoulderItemHash != hash)
            {
                Transfer = true;
            }
        }


        private static void Postfix(VisEquipment __instance)
        {
            if (!Transfer || !__instance.m_nview || __instance.m_nview.m_zdo == null) return;
            Transfer = false;
            if (__instance.m_shoulderItemInstances is { Count: > 0 })
            {
                string colorString = __instance.m_nview.m_zdo.GetString("MPASN_TMGshoulderitemColor");
                if (!string.IsNullOrEmpty(colorString) && ColorUtility.TryParseHtmlString(colorString, out Color c) &&
                    c != Color.white)
                {
                    foreach (GameObject VARIABLE in __instance.m_shoulderItemInstances)
                    {
                        Utils.SetGOColors(VARIABLE, c);
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(VisEquipment), nameof(VisEquipment.SetLegEquipped))]
    [ClientOnlyPatch]
    private static class MockLegs
    {
        private static bool Transfer;

        private static void Prefix(VisEquipment __instance, int hash)
        {
            if (__instance.m_currentLegItemHash != hash)
            {
                Transfer = true;
            }
        }


        private static void Postfix(VisEquipment __instance)
        {
            if (!Transfer || !__instance.m_nview || __instance.m_nview.m_zdo == null) return;
            Transfer = false;
            if (__instance.m_legItemInstances is { Count: > 0 })
            {
                string colorString = __instance.m_nview.m_zdo.GetString("MPASN_TMGlegitemColor");
                if (!string.IsNullOrEmpty(colorString) && ColorUtility.TryParseHtmlString(colorString, out Color c) &&
                    c != Color.white)
                {
                    foreach (GameObject VARIABLE in __instance.m_legItemInstances)
                    {
                        Utils.SetGOColors(VARIABLE, c);
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(VisEquipment), nameof(VisEquipment.SetBackEquipped))]
    [ClientOnlyPatch]
    private static class MockBackItems
    {
        private static bool Transfer;

        private static void Prefix(VisEquipment __instance, int leftItem, int rightItem, int leftVariant)
        {
            if (__instance.m_currentLeftBackItemHash != leftItem ||
                __instance.m_currentRightBackItemHash != rightItem ||
                __instance.m_currentLeftBackItemVariant != leftVariant)
            {
                Transfer = true;
            }
        }


        private static void Postfix(VisEquipment __instance)
        {
            if (!Transfer || !__instance.m_nview || __instance.m_nview.m_zdo == null) return;
            Transfer = false;
            if (__instance.m_leftBackItemInstance)
            {
                string colorString = __instance.m_nview.m_zdo.GetString("MPASN_TMGleftbackitemColor");
                if (!string.IsNullOrEmpty(colorString) && ColorUtility.TryParseHtmlString(colorString, out Color c) &&
                    c != Color.white)
                {
                    Utils.SetGOColors(__instance.m_leftBackItemInstance, c);
                }
            }

            if (__instance.m_rightBackItemInstance)
            {
                string colorString = __instance.m_nview.m_zdo.GetString("MPASN_TMGrightbackitemColor");
                if (!string.IsNullOrEmpty(colorString) && ColorUtility.TryParseHtmlString(colorString, out Color c) &&
                    c != Color.white)
                {
                    Utils.SetGOColors(__instance.m_rightBackItemInstance, c);
                }
            }
        }
    }

    [HarmonyPatch(typeof(VisEquipment), nameof(VisEquipment.SetChestEquipped))]
    [ClientOnlyPatch]
    private static class MockChest
    {
        private static bool Transfer;

        private static void Prefix(VisEquipment __instance, int hash)
        {
            if (__instance.m_currentChestItemHash != hash)
            {
                Transfer = true;
            }
        }


        private static void Postfix(VisEquipment __instance)
        {
            if (!Transfer || !__instance.m_nview || __instance.m_nview.m_zdo == null) return;
            Transfer = false;
            if (__instance.m_chestItemInstances is { Count: > 0 })
            {
                string colorString = __instance.m_nview.m_zdo.GetString("MPASN_TMGchestitemColor");
                if (!string.IsNullOrEmpty(colorString) && ColorUtility.TryParseHtmlString(colorString, out Color c) &&
                    c != Color.white)
                {
                    foreach (GameObject VARIABLE in __instance.m_chestItemInstances)
                    {
                        Utils.SetGOColors(VARIABLE, c);
                    }
                }
            }
        }
    }
}