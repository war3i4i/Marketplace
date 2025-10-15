using System.Reflection.Emit;
using Groups;
using Marketplace.ExternalLoads;
using Marketplace.Modules.Global_Options;

namespace Marketplace.Modules.Quests;

public static class Quest_ProgressionHook
{
    [HarmonyPatch(typeof(Pickable), nameof(Pickable.RPC_Pick))]
    [ClientOnlyPatch]
    private static class Pickable_Interact_Patch
    {
        [UsedImplicitly]
        private static void Prefix(Pickable __instance, long sender)
        {
            if (__instance.m_picked) return;
            string prefab = global::Utils.GetPrefabName(__instance.gameObject);
            ZRoutedRpc.instance.InvokeRoutedRPC(sender, "KGmarket QuestPickup", prefab);
        }
    }


    [HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.DoCrafting))]
    [ClientOnlyPatch]
    private static class HOOKCRAFTING
    {
        public static void HookCrafting()
        {
            if (InventoryGui.instance.m_craftRecipe?.m_item is not { } item) return;
            string itemName = item.name;
            int level = InventoryGui.instance.m_craftUpgradeItem?.m_quality + 1 ?? 1;
            GameEvents.OnItemCrafted?.Invoke(itemName, level);
            Quests_DataTypes.Quest.TryAddRewardCraft(itemName, level);
        }

        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> code)
        {
            CodeMatcher matcher = new CodeMatcher(code);
            matcher.MatchForward(false, new CodeMatch((i) => i.opcode == OpCodes.Stloc_S && ((LocalBuilder)i.operand).LocalIndex == 11));
            if (matcher.IsInvalid)
            {
                Utils.print($"InventoryGui_DoCrafting_Patch: failed to match", ConsoleColor.Red);
                return matcher.Instructions();
            }
            matcher.Advance(1);
            matcher.Insert(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(HOOKCRAFTING), nameof(HookCrafting))));
            return matcher.Instructions();
        }
    }


    private static readonly Dictionary<Character, long> CharacterLastDamageList = new();

    [HarmonyPatch(typeof(Character), nameof(Character.RPC_Damage))]
    [ClientOnlyPatch]
    private static class QuestEnemyKill
    {
        [UsedImplicitly]
        private static void Prefix(Character __instance, long sender, HitData hit)
        {
            if (__instance.GetHealth() <= 0) return;
            Character attacker = hit.GetAttacker();
            if (attacker)
            {
                if (attacker.IsPlayer())
                {
                    CharacterLastDamageList[__instance] = sender;
                }
                else
                {
                    if (!attacker.IsTamed())
                    {
                        CharacterLastDamageList[__instance] = 100;
                    }
                }
            }
        }

        [UsedImplicitly]
        private static void Postfix(Character __instance)
        {
            if (__instance.GetHealth() <= 0f && CharacterLastDamageList.ContainsKey(__instance))
            {
                ZPackage pkg = new();
                pkg.Write(global::Utils.GetPrefabName(__instance.gameObject));
                pkg.Write(__instance.GetLevel());
                pkg.Write(__instance.transform.position);
                pkg.Write(true);
                ZRoutedRpc.instance.InvokeRoutedRPC(CharacterLastDamageList[__instance], "KGmarket QuestKill", [pkg]);
                CharacterLastDamageList.Remove(__instance);
            }
        }
    }

    [HarmonyPatch(typeof(Character), nameof(Character.ApplyDamage))]
    [ClientOnlyPatch]
    private static class Character_ApplyDamage_Patch
    {
        [UsedImplicitly]
        private static void Postfix(Character __instance)
        {
            if (__instance.GetHealth() <= 0f && CharacterLastDamageList.ContainsKey(__instance))
            {
                ZPackage pkg = new();
                pkg.Write(global::Utils.GetPrefabName(__instance.gameObject));
                pkg.Write(__instance.GetLevel());
                pkg.Write(__instance.transform.position);
                pkg.Write(true);
                ZRoutedRpc.instance.InvokeRoutedRPC(CharacterLastDamageList[__instance], "KGmarket QuestKill", [pkg]);
                CharacterLastDamageList.Remove(__instance);
            }
        }
    }

    [HarmonyPatch(typeof(Character), nameof(Character.OnDestroy))]
    [ClientOnlyPatch]
    private static class Character_OnDestroy_Patch
    {
        [UsedImplicitly]
        private static void Postfix(Character __instance)
        {
            if (CharacterLastDamageList.ContainsKey(__instance)) CharacterLastDamageList.Remove(__instance);
        }
    }


    [MarketplaceRPC("KGmarket QuestPickup", Market_Autoload.Type.Client)]
    private static void QuestPickup(long sender, string prefab)
    {
        Quests_DataTypes.Quest.TryAddRewardPickup(prefab);
        GameEvents.OnHarvest?.Invoke(prefab);
    }

    [MarketplaceRPC("KGmarket QuestKill", Market_Autoload.Type.Client)]
    private static void QuestKillEvent(long sender, ZPackage pkg)
    {
        if (!Player.m_localPlayer) return;
        
        string prefab = pkg.ReadString();
        int level = pkg.ReadInt();
        Vector3 pos = pkg.ReadVector3();
        bool ownerRPC = pkg.ReadBool();
        
        GameEvents.OnCreatureKilled?.Invoke(prefab, level);
        if (Global_Configs.SyncedGlobalOptions.Value._allowKillQuestsInParty && ownerRPC && Groups.API.IsLoaded() &&
            Groups.API.GroupPlayers() is { Count: > 1 } group)
                foreach (PlayerReference member in group)
                    if (member.peerId != ZDOMan.instance.m_sessionID)
                    {
                        ZPackage newPkg = new();
                        newPkg.Write(prefab);
                        newPkg.Write(level);
                        newPkg.Write(pos);
                        newPkg.Write(false);
                        ZRoutedRpc.instance.InvokeRoutedRPC(member.peerId, "KGmarket QuestKill", [newPkg]);
                     }
        if (!ownerRPC && Vector3.Distance(Player.m_localPlayer.transform.position, pos) >= 100f) return;
        Quests_DataTypes.Quest.TryAddRewardKill(prefab, level);
    }

    [HarmonyPatch(typeof(Player), nameof(Player.OnInventoryChanged))]
    [ClientOnlyPatch]
    private static class Player_OnInventoryChanged_Patch
    {
        [UsedImplicitly]
        private static void Postfix()
        {
            if (!Player.m_localPlayer || Player.m_localPlayer.m_isLoading) return;
            Quests_DataTypes.Quest.InventoryChanged();
        }
    }

    [HarmonyPatch(typeof(Player), nameof(Player.PlacePiece))]
    [ClientOnlyPatch]
    private static class HookBuildQuest
    {
        public static void PlacedPiece(GameObject obj)
        {
            if (obj?.GetComponent<Piece>() is not { } piece) return;
            string pieceName = global::Utils.GetPrefabName(piece.gameObject);
            GameEvents.OnStructureBuilt?.Invoke(pieceName);
            if (Quests_DataTypes.Quest.TryAddRewardBuild(pieceName) && piece.m_nview &&
                piece.m_nview.m_zdo != null)
            {
                piece.m_nview.m_zdo.Set("MPASNquestBuild", true);
            }  
        } 

        [UsedImplicitly]
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            MethodInfo method =
                AccessTools.Method(typeof(HookBuildQuest), nameof(PlacedPiece), new[] { typeof(GameObject) });
            foreach (CodeInstruction instruction in instructions)
            {
                yield return instruction;
                if (instruction.opcode == OpCodes.Stloc_0)
                {
                    yield return new CodeInstruction(OpCodes.Ldloc_S, 0);
                    yield return new CodeInstruction(OpCodes.Call, method);
                }
            }
        }
    }

    [HarmonyPatch(typeof(Humanoid), nameof(Humanoid.Start))]
    [ClientOnlyPatch]
    private static class Character_Awake_Quest
    {
        [UsedImplicitly]
        private static void Postfix(Humanoid __instance)
        {
            float radius = __instance.m_collider.radius;
            float height = __instance.m_collider.height - 1.7f;
            if (!Quests_Main_Client.ShowQuestMark.Value) return;
            GameObject go = UnityEngine.Object.Instantiate(AssetStorage.MarketplaceQuestTargetIcon, __instance.transform);
            go.name = "MPASNquest";
            go.transform.localPosition += Vector3.up * height;
            go.transform.localScale += new Vector3(radius, radius, radius);
            go.SetActive(Quests_DataTypes.Quest.IsQuestTarget(__instance));
        }
    }

    [HarmonyPatch(typeof(Pickable), nameof(Pickable.Awake))]
    [ClientOnlyPatch]
    private static class Pickable_Awake_Quest
    {
        [UsedImplicitly]
        private static void Postfix(Pickable __instance)
        {
            __instance.gameObject.AddComponent<Pickable_Hook>();
            if (!Quests_Main_Client.ShowQuestMark.Value) return;
            GameObject go = UnityEngine.Object.Instantiate(AssetStorage.MarketplaceQuestTargetIcon, __instance.transform);
            go.name = "MPASNquest";
            go.SetActive(Quests_DataTypes.Quest.IsQuestTarget(__instance));
        }
    }

    [HarmonyPatch(typeof(ItemDrop), nameof(ItemDrop.Awake))]
    [ClientOnlyPatch]
    private static class ItemDrop_Awake_Quest
    {
        [UsedImplicitly]
        private static void Postfix(ItemDrop __instance)
        {
            if (!Quests_Main_Client.ShowQuestMark.Value) return;
            GameObject go = UnityEngine.Object.Instantiate(AssetStorage.MarketplaceQuestTargetIcon,
                __instance.transform);
            go.name = "MPASNquest";
            go.SetActive(false);
        }
    }

    [HarmonyPatch(typeof(ItemDrop), nameof(ItemDrop.Start))]
    [ClientOnlyPatch]
    private static class ItemDrop_Awake_Quest2
    {
        [UsedImplicitly]
        private static void Postfix(ItemDrop __instance)
        {
            __instance.transform.Find("MPASNquest")?.gameObject
                .SetActive(Quests_DataTypes.Quest.IsQuestTarget(__instance));
        }
    }


    public class Pickable_Hook : MonoBehaviour
    {
        private static readonly List<Pickable> pickables = new();
        private Pickable pick = null!;

        private void Awake()
        {
            ZNetView znv = GetComponent<ZNetView>();
            if (!znv || !znv.IsValid()) return;
            pick = GetComponent<Pickable>();
            if (!pick) return;
            pickables.Add(pick);
        }

        private void OnDestroy()
        {
            if (pick) pickables.Remove(pick);
        }

        public static List<Pickable> GetPickables()
        {
            return pickables;
        }
    }


    [HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.AddRecipeToList))]
    [ClientOnlyPatch]
    private static class TranspileRecipeQuestTest
    {
        private static void ChangeRecipeName(Recipe r, ref string str, ItemDrop.ItemData item)
        {
            if (!Quests_DataTypes.Quest.IsQuestTarget(r, item?.m_quality + 1 ?? 1)) return;
            if (!str.Contains('★'))
                str = "<color=#00ff00>★</color> " + str;
        }


        [HarmonyTranspiler]
        [UsedImplicitly]
        private static IEnumerable<CodeInstruction> TanspileRecipeQuest(IEnumerable<CodeInstruction> code)
        {
            MethodInfo method = AccessTools.DeclaredMethod(typeof(TranspileRecipeQuestTest), nameof(ChangeRecipeName));
            bool isDone = false;
            foreach (CodeInstruction instruction in code)
            {
                yield return instruction;
                if (instruction.opcode == OpCodes.Stloc_2 && !isDone)
                {
                    isDone = true;
                    yield return new CodeInstruction(OpCodes.Ldarg_2);
                    yield return new CodeInstruction(OpCodes.Ldloca_S, 2);
                    yield return new CodeInstruction(OpCodes.Ldarg_3);
                    yield return new CodeInstruction(OpCodes.Call, method);
                }
            }
        }
    }

    [HarmonyPatch(typeof(Piece), nameof(Piece.DropResources))]
    [ClientOnlyPatch]
    private static class Piece_DropResources_Patch
    {
        [UsedImplicitly]
        private static bool Prefix(Piece __instance)
        {
            return !__instance.m_nview.m_zdo.GetBool("MPASNquestBuild");
        }
    }


    [HarmonyPatch(typeof(Hud), nameof(Hud.Awake))]
    [ClientOnlyPatch]
    private static class Hud_Awake_Patch
    {
        public static int ChildNumber;

        [UsedImplicitly]
        private static void Postfix(Hud __instance)
        {
            GameObject instant = AssetStorage.asset.LoadAsset<GameObject>("QuestBuild");
            GameObject go = UnityEngine.Object.Instantiate(instant);
            go.name = "QuestBuild";
            // ReSharper disable once Unity.InstantiateWithoutParent
            go.transform.SetParent(__instance.m_pieceIconPrefab.transform);
            go.transform.localPosition = new Vector3(48, -48, 0);
            go.SetActive(false);
            ChildNumber = __instance.m_pieceIconPrefab.transform.childCount - 1;
            go.transform.SetAsLastSibling();
        }
    }

    [HarmonyPatch(typeof(Hud), nameof(Hud.UpdatePieceList))]
    [ClientOnlyPatch]
    private static class Hud_UpdatePieceList_Patch
    {
        private static void SpriteEnabler(Piece p, Hud.PieceIconData data)
        {
            GameObject t = data.m_go.transform.GetChild(Hud_Awake_Patch.ChildNumber).gameObject;
            if (!p)
            {
                t.SetActive(false);
                return;
            }
            t.SetActive(Quests_DataTypes.Quest.IsQuestTarget(p));
        }

        
            private static void Prefix(Hud __instance)
            {
                if (__instance.m_pieceIcons == null) return;
                foreach (Hud.PieceIconData data in __instance.m_pieceIcons)
                {
                    data.m_go.transform.GetChild(Hud_Awake_Patch.ChildNumber).gameObject.SetActive(false);
                }
            }

        [HarmonyTranspiler]
        [UsedImplicitly]
        private static IEnumerable<CodeInstruction> Patch(IEnumerable<CodeInstruction> code)
        {
            MethodInfo method = AccessTools.DeclaredPropertySetter(typeof(Image), nameof(Image.sprite));
            foreach (CodeInstruction instruction in code)
            {
                yield return instruction;
                if (instruction.opcode == OpCodes.Callvirt && ReferenceEquals(instruction.operand, method))
                {
                    yield return new CodeInstruction(OpCodes.Ldloc_S, 12);
                    yield return new CodeInstruction(OpCodes.Ldloc_S, 11);
                    yield return new CodeInstruction(OpCodes.Call,
                        AccessTools.DeclaredMethod(typeof(Hud_UpdatePieceList_Patch), nameof(SpriteEnabler)));
                }
            }
        }
    }
}