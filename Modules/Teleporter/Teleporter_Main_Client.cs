using Marketplace.ExternalLoads;
using Marketplace.Modules.Global_Options;

namespace Marketplace.Modules.Teleporter;
[Market_Autoload(Market_Autoload.Type.Client)]
public static class Teleporter_Main_Client
{
    private static readonly List<Minimap.PinData> CurrentTeleporterObjects = new();
    private static readonly Dictionary<Minimap.PinData, int> SpeedValues = new();
    private const Minimap.PinType PINTYPE = (Minimap.PinType)177;
    private static bool TeleporterJump;
    public static bool DEBUG_TELEPORTTO_TERRITORY;

    [UsedImplicitly]
    private static void OnInit()
    {
        Teleporter_DataTypes.SyncedTeleporterData.Value.Count();
    }
    
    internal static void ShowTeleporterUI(string profile)
    {
        InventoryGui.instance.Hide();
        foreach (Minimap.PinData obj in CurrentTeleporterObjects) Minimap.instance.RemovePin(obj);
        CurrentTeleporterObjects.Clear();
        SpeedValues.Clear();
        if (!Teleporter_DataTypes.SyncedTeleporterData.Value.ContainsKey(profile)) return;
        List<Teleporter_DataTypes.TeleporterData> current = Teleporter_DataTypes.SyncedTeleporterData.Value[profile];
        Minimap.instance.SetMapMode(Minimap.MapMode.Large);
        foreach (Teleporter_DataTypes.TeleporterData data in current)
        {
            Minimap.PinData pinData = new Minimap.PinData
            {
                m_type = PINTYPE,
                m_name = data.name,
                m_pos = new Vector3(data.x, data.y, data.z)
            };
            if (!string.IsNullOrEmpty(pinData.m_name))
            {
                pinData.m_NamePinData = new Minimap.PinNameData(pinData);
            }
            Sprite icon = Utils.TryFindIcon(data.sprite, AssetStorage.PortalIconDefault);
            pinData.m_icon = icon;
            pinData.m_save = false;
            pinData.m_checked = false;
            pinData.m_ownerID = 0L;
            Minimap.instance.m_pins.Add(pinData);
            CurrentTeleporterObjects.Add(pinData);
            SpeedValues.Add(pinData, data.speed);
            Minimap.instance.m_largeZoom = 1f;
            Minimap.instance.CenterMap(Vector3.zero);
        }
    }
    
    [HarmonyPatch(typeof(Minimap), nameof(Minimap.OnMapLeftClick))]
    [ClientOnlyPatch]
    private static class PatchClickIconMinimap
    {
        private static bool Prefix(Minimap __instance)
        {
            Vector3 pos = __instance.ScreenToWorldPoint(Input.mousePosition);
            Minimap.PinData closestPin = Utils.GetCustomPin(PINTYPE, pos, __instance.m_removeRadius * (__instance.m_largeZoom * 2f));
            if (closestPin != null)
            {
                if (!Global_Configs.SyncedGlobalOptions.Value._canTeleportWithOre && !Player.m_localPlayer.IsTeleportable())
                {
                    MessageHud.instance.ShowMessage(MessageHud.MessageType.Center,
                        $"<color=red>{Localization.instance.Localize("$mpasn_teleportwithore")}</color>");
                    __instance.SetMapMode(Minimap.MapMode.Small);
                    return false;
                }

                __instance.SetMapMode(Minimap.MapMode.Small);
                if (SpeedValues.TryGetValue(closestPin, out int SPEED) && SPEED > 0)
                {
                    Player.m_localPlayer.StartCoroutine(MarketplaceTeleporterMovement(SPEED, Player.m_localPlayer.transform.position, closestPin.m_pos));
                }
                else
                {
                    DEBUG_TELEPORTTO_TERRITORY = true;
                    Player.m_localPlayer.TeleportTo(closestPin.m_pos, Player.m_localPlayer.transform.rotation, true);
                    DEBUG_TELEPORTTO_TERRITORY = false;
                }

                return false;
            }
            return true;
        }
    }
    
    
    [HarmonyPatch(typeof(Minimap), nameof(Minimap.SetMapMode))]
    [ClientOnlyPatch]
    private static class PatchRemovePinsMinimap
    {
        private static void Prefix(Minimap.MapMode mode)
        {
            if (mode != Minimap.MapMode.Large)
            {
                foreach (Minimap.PinData obj in CurrentTeleporterObjects) Minimap.instance.RemovePin(obj);
                CurrentTeleporterObjects.Clear();
            }
        }
    }

    private static readonly int Wakeup = Animator.StringToHash("wakeup");
    private static IEnumerator DelayInvokeSMR(Player p)
    {
        yield return new WaitForEndOfFrame();
        if (!p) yield break;
        p.m_visual.SetActive(false);
        p.m_animator.SetBool(Wakeup, false);
    }

    [HarmonyPatch(typeof(Player), nameof(Player.Awake))]
    [ClientOnlyPatch]
    private static class PLAYERHIDE
    {
        private static void Postfix(Player __instance)
        {
            __instance.m_nview.Register("HideMarketplaceTeleporter", (long _, bool tf) =>
            {
                __instance.m_visual.SetActive(tf);
                __instance.m_animator.SetBool(Wakeup, false);
            });

            if (__instance.m_nview?.m_zdo?.GetBool("MarketplaceTeleporterHide") == true)
                __instance.StartCoroutine(DelayInvokeSMR(__instance));
        }
    }
    
    [HarmonyPatch(typeof(Character), nameof(Character.CustomFixedUpdate))]
    [ClientOnlyPatch]
    private static class PriestCancelTP
    {
        private static void Postfix(Character __instance)
        {
            if (TeleporterJump && __instance == Player.m_localPlayer)
            {
                __instance.m_body.useGravity = false;
                __instance.m_body.velocity = Vector3.zero;
                __instance.m_currentVel = Vector3.zero;
                __instance.m_body.angularVelocity = Vector3.zero;
            }
        }
    }
    
    [HarmonyPatch(typeof(Character), nameof(Character.Damage))]
    [ClientOnlyPatch]
    private static class Character_Damage_Patch
    {
        private static bool Prefix(Character __instance)
        {
            return !TeleporterJump || __instance != Player.m_localPlayer;
        }
    }
    
     private static IEnumerator MarketplaceTeleporterMovement(int speed, Vector3 startPos, Vector3 targetPos)
    {
        GameObject mainEffect = UnityEngine.Object.Instantiate(AssetStorage.Teleporter_VFX1, startPos, Player.m_localPlayer.transform.rotation);
        TeleporterJump = true;
        Player p = Player.m_localPlayer;
        UnityEngine.Object.Instantiate(AssetStorage.Teleporter_VFX2, p.transform.position + Vector3.up, p.transform.rotation);
        p.m_nview.InvokeRPC(ZNetView.Everybody, "HideMarketplaceTeleporter", false);
        p.m_nview.m_zdo.Set("MarketplaceTeleporterHide", true);
        p.m_zanim.SetTrigger("emote_stop");
        p.m_collider.isTrigger = true;
        float distance = global::Utils.DistanceXZ(startPos, targetPos);
        float time = Mathf.Max(1f,distance / speed);
        float count = 0;
        Player.m_localPlayer.transform.rotation = Quaternion.LookRotation((targetPos - startPos).normalized);
        while (count < 1f)
        {
            if (!p || p.IsDead())
            {
                ZNetScene.instance.Destroy(mainEffect); 
                TeleporterJump = false;
                yield break;
            }

            p.m_body.velocity = Vector3.zero;
            p.m_body.angularVelocity = Vector3.zero;
            p.m_lastGroundTouch = 0;
            p.m_maxAirAltitude = 0f;
            count += Time.deltaTime / time;
            Vector3 point = startPos + (targetPos - startPos) / 2 + Vector3.up * 500f;
            Vector3 m1 = Vector3.Lerp(startPos, point, count);
            Vector3 m2 = Vector3.Lerp(point, targetPos, count);
            p.transform.position = Vector3.Lerp(m1, m2, count);
            mainEffect.transform.position = p.transform.position;
            yield return null;
        }

        float messageTimer = 1f;
        while (!ZNetScene.instance.IsAreaReady(Player.m_localPlayer.transform.position))
        {
            messageTimer -= Time.deltaTime;
            if (!p || p.IsDead())
            {
                ZNetScene.instance.Destroy(mainEffect); 
                TeleporterJump = false;
                yield break;
            }
            
            if (messageTimer <= 0f)
            {
                messageTimer = 1f;
                MessageHud.instance.ShowMessage(MessageHud.MessageType.Center, "$mpasn_zoneloading".Localize());
            }
            yield return null;
        }

        ZNetScene.instance.Destroy(mainEffect);
        TeleporterJump = false;
        p.m_collider.isTrigger = false;
        p.m_nview.m_zdo.Set("MarketplaceTeleporterHide", false);
        p.m_body.velocity = Vector3.zero;
        p.m_body.useGravity = true;
        p.m_lastGroundTouch = 0f;
        p.m_maxAirAltitude = 0f;
        p.m_nview.InvokeRPC(ZNetView.Everybody, "HideMarketplaceTeleporter", true);
        UnityEngine.Object.Instantiate(AssetStorage.Teleporter_VFX2, p.transform.position + Vector3.up, p.transform.rotation);
    }
    
}