/*using UnityEngine.Rendering;

namespace Marketplace.Modules.PieceSaver;

[UsedImplicitly]
[Market_Autoload(Market_Autoload.Type.Both, Market_Autoload.Priority.Last, "OnInit")]
public static class PieceSaver
{
    private const bool ONLY_FOR_CREATOR = true; 
    private static AssetBundle asset;
    private static GameObject PieceSaverPrefab;
    private static GameObject PieceSaverCrystalPrefab;
    private static Material pieceSaverMaterial;
    private static readonly LayerMask layer = LayerMask.NameToLayer("character");

    private static void OnInit()
    {
        asset = GetAssetBundle("kg_piecesaver");
        PieceSaverPrefab = asset.LoadAsset<GameObject>("kg_PieceSaver");
        PieceSaverCrystalPrefab = asset.LoadAsset<GameObject>("kg_PieceSaverCrystal");
        if (Marketplace.WorkingAsType is Marketplace.WorkingAs.Server) return;
        PieceSaverPrefab.AddComponent<PieceSaver_Component>();
        PieceSaverCrystalPrefab.AddComponent<PieceSaverCrystal>();
        pieceSaverMaterial = asset.LoadAsset<Material>("kg_PieceSaverMaterial");
    }

    private static AssetBundle GetAssetBundle(string filename)
    {
        Assembly execAssembly = Assembly.GetExecutingAssembly();
        string resourceName = execAssembly.GetManifestResourceNames().Single(str => str.EndsWith(filename));
        using Stream stream = execAssembly.GetManifestResourceStream(resourceName);
        return AssetBundle.LoadFromStream(stream);
    }
    

    [HarmonyPatch(typeof(ZNetScene), nameof(ZNetScene.Awake))]
    private static class ZNetScene_Awake_Patch
    {
        private static void Postfix(ZNetScene __instance)
        {
            __instance.m_namedPrefabs[PieceSaverPrefab.name.GetStableHashCode()] = PieceSaverPrefab;
            __instance.m_namedPrefabs[PieceSaverCrystalPrefab.name.GetStableHashCode()] = PieceSaverCrystalPrefab;
            if(Marketplace.WorkingAsType is Marketplace.WorkingAs.Server) return;
            PieceTable hammer = __instance.GetPrefab("Hammer").GetComponent<ItemDrop>().m_itemData.m_shared
                .m_buildPieces;
            if (!hammer.m_pieces.Contains(PieceSaverCrystalPrefab))
                hammer.m_pieces.Add(PieceSaverCrystalPrefab);
        }
    }


    public class PieceSaver_Component : MonoBehaviour, Hoverable, Interactable
    {
        private static readonly List<PieceSaver_Component> _allPieceSavers = new();
        private ZNetView _znet;
        private Transform PieceObject;

        public string _Prefab
        {
            get => _znet.m_zdo.GetString("Prefab");
            set => _znet.m_zdo.Set("Prefab", value);
        }

        public long _CreatorID
        {
            get => _znet.m_zdo.GetLong("CreatorID");
            set => _znet.m_zdo.Set("CreatorID", value);
        }

        private void Awake()
        {
            _znet = GetComponent<ZNetView>();
            if (!_znet.IsValid()) return;
            PieceObject = transform.Find("PieceObject");
            _znet.Register<string, long>("Setup", RPC_Setup);
            _znet.Register("Build", RPC_Build);
            _znet.Register("Destroy", RPC_Destroy);
            CreatePieceObject(_Prefab, _CreatorID);
            _allPieceSavers.Add(this);
        }

        private void OnDestroy()
        {
            _allPieceSavers.Remove(this);
        }

        public static void DestroyAll() => _allPieceSavers.ForEach(x =>
        {
            x._znet.ClaimOwnership();
            ZNetScene.instance.Destroy(x.gameObject);
        });

        private void RPC_Destroy(long obj)
        {
            _znet.ClaimOwnership();
            ZNetScene.instance.Destroy(this.gameObject);
        }
 
        public void Setup(string prefab, long creatorID) =>
            _znet.InvokeRPC(ZNetView.Everybody, "Setup", prefab, creatorID);

        private void RPC_Setup(long sender, string prefab, long creatorID)
        {
            if (_znet.IsOwner())
            {
                _Prefab = prefab;
                _CreatorID = creatorID;
            }

           
            CreatePieceObject(prefab, creatorID);
        }

        private void RPC_Build(long sender)
        {
            string prefab = _Prefab;
            long creator = _CreatorID;
            Vector3 pos = transform.position;
            Quaternion rot = transform.rotation;
            _znet.ClaimOwnership();
            ZNetScene.instance.Destroy(this.gameObject);
            GameObject go = ZNetScene.instance.GetPrefab(prefab);
            if (!go) return;
            GameObject newPiece = Instantiate(go, pos, rot);
            newPiece.GetComponent<Piece>().m_placeEffect.Create(pos, rot, newPiece.transform);
            newPiece.GetComponent<Piece>().SetCreator(creator);
            if (newPiece.GetComponent<WearNTear>() is { } wnt) wnt.OnPlaced();
        }

        private void CreatePieceObject(string prefab, long creatorID)
        {
            // ReSharper disable once HeuristicUnreachableCode
            if (ONLY_FOR_CREATOR && Game.instance.m_playerProfile.GetPlayerID() != creatorID) return;

            if (string.IsNullOrEmpty(prefab)) return;
            foreach (Transform child in PieceObject)
                Destroy(child.gameObject);
            
            Collider[] colliders = PieceObject.GetComponentsInChildren<Collider>();
            foreach (Collider collider in colliders) Destroy(collider);

            PieceObject.gameObject.SetActive(false);
            GameObject piece = ZNetScene.instance.GetPrefab(prefab);

            if (piece.GetComponent<BoxCollider>())
                Utils.CopyComponent(piece.GetComponent<BoxCollider>(), PieceObject.gameObject);

            PieceObject.gameObject.layer = layer;
            foreach (Transform o in piece.transform)
            {
                if (!o.gameObject.activeSelf) continue;
                GameObject newObject = Instantiate(o.gameObject, PieceObject);
                newObject.transform.localPosition = o.localPosition;
                newObject.transform.localRotation = o.localRotation;
                newObject.transform.localScale = o.localScale;
                newObject.layer = layer;
            }

            bool checkComponent(Component component) =>
                component is Renderer or MeshFilter or Transform or Animator or Collider;

            foreach (Component comp in PieceObject.GetComponentsInChildren<Component>(true).Reverse())
                if (!checkComponent(comp))
                    Destroy(comp);

            foreach (Collider collider in PieceObject.GetComponentsInChildren<Collider>(false))
                collider.isTrigger = true;

            var rendereres = PieceObject.GetComponentsInChildren<Renderer>(false);
            for (int i = 0; i < rendereres.Length; i++)
            {
                if (rendereres[i] is MeshRenderer mesh)
                    mesh.shadowCastingMode = ShadowCastingMode.Off;

                if (rendereres[i] is SkinnedMeshRenderer skinnedmesh)
                    skinnedmesh.shadowCastingMode = ShadowCastingMode.Off;

                Material[] newMaterials = new Material[rendereres[i].materials.Length];
                for (int j = 0; j < rendereres[i].materials.Length; j++)
                {
                    newMaterials[j] = pieceSaverMaterial;
                }

                rendereres[i].materials = newMaterials;
            }

            PieceObject.gameObject.SetActive(true);
        }

        private string PrepareResourcesString()
        {
            GameObject go = ZNetScene.instance.GetPrefab(_Prefab);
            if (!go) return "";
            Piece piece = go.GetComponent<Piece>();
            if (!piece) return "";
            string result = $"Use to build <color=yellow>{piece.m_name}</color>:\n".Localize();
            foreach (Piece.Requirement requirement in piece.m_resources)
            {
                string itemName = requirement.m_resItem.m_itemData.m_shared.m_name.Localize();
                int amount = requirement.m_amount;
                string color =
                    Player.m_localPlayer.m_inventory.CountItems(requirement.m_resItem.m_itemData.m_shared.m_name) >=
                    amount
                        ? "green"
                        : "red";
                result += $"<color=yellow>{itemName}</color> <color={color}>x{amount}</color>\n";
            }

            return result;
        }

        private bool HaveAllResources(bool removeOnTrue = true)
        {
            GameObject go = ZNetScene.instance.GetPrefab(_Prefab);
            if (!go) return false;
            Piece piece = go.GetComponent<Piece>();
            if (!piece) return false;
            if (Player.m_debugMode) return true;
            foreach (Piece.Requirement requirement in piece.m_resources)
                if (Player.m_localPlayer.m_inventory.CountItems(requirement.m_resItem.m_itemData.m_shared.m_name) <
                    requirement.m_amount)
                    return false;

            if (removeOnTrue)
                foreach (Piece.Requirement requirement in piece.m_resources)
                    Player.m_localPlayer.m_inventory.RemoveItem(requirement.m_resItem.m_itemData.m_shared.m_name,
                        requirement.m_amount);

            return true;
        }

        public string GetHoverText()
        {
            if (!_znet.IsValid() || string.IsNullOrEmpty(_Prefab)) return "";
            return $"[<color=yellow><b>$KEY_Use</b></color>] ".Localize() + PrepareResourcesString() +
                   "\n[<color=yellow><b><color=red>L. Alt</color> + $KEY_Use</b></color>] Delete".Localize();
        }

        public string GetHoverName()
        {
            return "";
        }

        public bool Interact(Humanoid user, bool hold, bool alt)
        {
            if (Input.GetKey(KeyCode.LeftAlt))
            {
                _znet.InvokeRPC("Destroy");
                return true;
            }

            if (HaveAllResources())
            {
                _znet.InvokeRPC("Build");
                return true;
            }

            return false;
        }

        public bool UseItem(Humanoid user, ItemDrop.ItemData item)
        {
            return false;
        }
    }

    [HarmonyPatch(typeof(WearNTear), nameof(WearNTear.Destroy))]
    [ClientOnlyPatch]
    private static class WearNTear_Destroy_Patch
    {
        private static PieceTable _hammerTable;
        public static bool SKIP;

        private static void Prefix(WearNTear __instance)
        {
            if (SKIP) return;
            if (!PieceSaverCrystal.IsCrystalNear(__instance.transform.position)) return;
            if (!__instance.m_piece || __instance.m_piece.GetCreator() == 0 ||
                !__instance.m_piece.m_canBeRemoved) return;
            string prefab = global::Utils.GetPrefabName(__instance.m_piece.gameObject);
            GameObject saver = UnityEngine.Object.Instantiate(PieceSaverPrefab, __instance.transform.position,
                __instance.transform.rotation);
            saver.GetComponent<PieceSaver_Component>().Setup(prefab, __instance.m_piece.GetCreator());
        }
    }

    [HarmonyPatch(typeof(WearNTear), nameof(WearNTear.RPC_Remove))]
    [ClientOnlyPatch]
    private static class WearNTear_RPC_Remove_Patch
    {
        private static void Prefix() => WearNTear_Destroy_Patch.SKIP = true;
        private static void Postfix() => WearNTear_Destroy_Patch.SKIP = false;
    }


    public class PieceSaverCrystal : MonoBehaviour
    {
        private static readonly List<PieceSaverCrystal> _crystals = new List<PieceSaverCrystal>();
        private const int DISTANCE = 100;
        private ZNetView _znv;

        private void Awake()
        {
            _znv = GetComponent<ZNetView>();
            if (!_znv.IsValid()) return;
            _crystals.Add(this);
            transform.Find("Crystal").GetComponent<Animator>().enabled = true;
        }

        private void OnDestroy()
        {
            _crystals.Remove(this);
        }

        public static bool IsCrystalNear(Vector3 pos) =>
            _crystals.Count > 0;
        //_crystals.Any(crystal => global::Utils.DistanceXZ(crystal.transform.position, pos) < DISTANCE);
    }
}*/