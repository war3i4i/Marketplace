using System.Diagnostics.CodeAnalysis;
using API;
using Marketplace.ExternalLoads;
using Marketplace.Modules.Quests;

namespace Marketplace.Modules.NPC;

[UsedImplicitly]
[Market_Autoload(Market_Autoload.Type.Both)]
[SuppressMessage("ReSharper", "IteratorNeverReturns")]
public static class NPC_MapPins
{
    private const string npcToSearchPrefabName = "MarketPlaceNPC";
    private static readonly List<ZDO> TempNPCList = new();
    private static readonly List<Minimap.PinData> TempNPCpins = new();
    public const Minimap.PinType PINTYPENPC = (Minimap.PinType)175;
 
    [UsedImplicitly]
    private static void OnInit() 
    {
        Marketplace._thistype.StartCoroutine(SendNPCsToClients());
        Marketplace._thistype.StartCoroutine(UpdateNPCpins());
    }

    private static IEnumerator UpdateNPCpins()
    {
        while (true)
        {
            if (Player.m_localPlayer && ZDOMan.instance != null && Minimap.instance)
            {
                foreach (Minimap.PinData obj in TempNPCpins) Minimap.instance.RemovePin(obj);
                TempNPCpins.Clear();
                if (Minimap.instance.m_mode == Minimap.MapMode.Large && Utils.IsDebug_Strict && NPC_MapController.EnableMapControl.Value)
                {
                    yield return new WaitForSecondsRealtime(1f);
                    continue;
                }
                List<ZDO> AllNPCs = new();
                int index = 0;
                while (!ZDOMan.instance.GetAllZDOsWithPrefabIterative(npcToSearchPrefabName, AllNPCs, ref index))
                {
                    if (!Player.m_localPlayer || !Minimap.instance) break;
                    yield return null;
                }
                AllNPCs.RemoveAll(zdo => !zdo.GET_NPC_IsPinned());
                foreach (ZDO zdo in AllNPCs)
                {
                    if (!zdo.IsValid()) continue;
                    string name = zdo.GET_NPC_Name();
                    int type = (int)zdo.GET_NPC_Type();
                    bool questTarget = Quests_DataTypes.Quest.IsQuestTarget(name);
                    if (string.IsNullOrWhiteSpace(name))
                    {
                        name = Localization.instance.Localize("$" + (Market_NPC.NPCType)type);
                    }

                    Minimap.PinData pinData = new Minimap.PinData
                    {
                        m_type = PINTYPENPC,
                        m_name = Utils.RichTextFormatting(name),
                        m_pos = zdo.GetPosition(),
                    };
                    if (!string.IsNullOrEmpty(pinData.m_name))
                    {
                        pinData.m_NamePinData = new Minimap.PinNameData(pinData);
                    }

                    Sprite icon = Utils.TryFindIcon(zdo.GET_NPC_PinIcon(), AssetStorage.PlaceholderGamblerIcon);

                    if (questTarget)
                    {
                        icon = AssetStorage.QuestCompleteIcon;
                        pinData.m_animate = true;
                        pinData.m_doubleSize = true;
                    }

                    pinData.m_icon = icon;
                    pinData.m_save = false;
                    pinData.m_checked = false;
                    pinData.m_ownerID = 0L;
                    TempNPCpins.Add(pinData);
                }
            
                foreach (Minimap.PinData p in TempNPCpins.OrderBy(pin => pin.m_animate))
                {
                    Minimap.instance.m_pins.Add(p);
                }
            }

            yield return new WaitForSecondsRealtime(10f);
        }
    }
    
    [HarmonyPatch(typeof(Minimap), nameof(Minimap.GetSprite))]
    [ClientOnlyPatch]
    private static class Minimap_GetSprite_Patch
    {
        [UsedImplicitly]
        private static void Postfix(Minimap.PinType type, ref Sprite __result)
        {
            if (type is PINTYPENPC) __result = AssetStorage.PlaceholderGamblerIcon;
        }
    }

    [HarmonyPatch(typeof(Minimap), nameof(Minimap.SetMapMode))]
    [ClientOnlyPatch]
    private static class NPC_MapControllerPatch
    {
        [UsedImplicitly]
        private static void Prefix(Minimap __instance, Minimap.MapMode mode)
        {
            if (mode != Minimap.MapMode.Large) return;
            if (Utils.IsDebug_Strict && NPC_MapController.EnableMapControl.Value)
            {
                foreach (Minimap.PinData obj in TempNPCpins) __instance.RemovePin(obj);
                TempNPCpins.Clear();
            }
        }
    }

    private static IEnumerator SendNPCsToClients()
    {
        for (;;)
        {
            if (Game.instance && ZDOMan.instance != null && ZNet.instance && ZNet.instance.IsServer())
            {
                TempNPCList.Clear();
                int index = 0;
                while (!ZDOMan.instance.GetAllZDOsWithPrefabIterative(npcToSearchPrefabName, TempNPCList, ref index))
                {
                    yield return null;
                }

                foreach (ZDO zdo in TempNPCList)
                {
                    ZDOMan.instance.ForceSendZDO(zdo.m_uid);
                }
            }

            yield return new WaitForSeconds(10f);
        }
    }
}