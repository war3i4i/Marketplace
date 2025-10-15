using Marketplace.ExternalLoads;
using UnityEngine.EventSystems;
using LightType = UnityEngine.LightType;

namespace Marketplace.Modules.Transmogrification;

public static class PlayerModelPreview
{
    private static readonly Camera renderCamera;
    private static readonly Light Light;
    private static readonly Vector3 SpawnPoint = new(25000f, 25000f, 25000f);
    private static readonly GameObject UI;
    private static readonly RectTransform CHECK;
    private static float OriginalYPos;
    private static float OriginalCameraZPos;
    private static GameObject CurrentPreviewGO;
    private static GameObject BehindWallRender;
    private static string CurrentWall = "woodwall@grey"; 
    private static readonly int ChestTex = Shader.PropertyToID("_ChestTex");
    private static readonly int ChestBumpMap = Shader.PropertyToID("_ChestBumpMap");
    private static readonly int ChestMetal = Shader.PropertyToID("_ChestMetal");
    private static readonly int LegsTex = Shader.PropertyToID("_LegsTex");
    private static readonly int LegsBumpMap = Shader.PropertyToID("_LegsBumpMap");
    private static readonly int LegsMetal = Shader.PropertyToID("_LegsMetal");
    private static readonly int Wakeup = Animator.StringToHash("wakeup");

    static PlayerModelPreview()
    {
        UI = UnityEngine.Object.Instantiate(
            AssetStorage.asset.LoadAsset<GameObject>("MarketplacePreviewUI"));
        UI.transform.Find("Canvas/Preview/Background/Close").GetComponent<Button>().onClick
            .AddListener(() =>
            {
                AssetStorage.AUsrc.Play();
                StopPreview();
            });
        CHECK = UI.transform.Find("Canvas/Preview/Background").GetComponent<RectTransform>();
        UI.transform.Find("Canvas/Preview/Background/Light").GetComponent<Button>().onClick.AddListener(ChangeLight);
        UI.transform.Find("Canvas/Preview/Background/Wood").GetComponent<Button>().onClick
            .AddListener(() => ResetWall("woodwall"));
        UI.transform.Find("Canvas/Preview/Background/Black").GetComponent<Button>().onClick
            .AddListener(() => ResetWall("woodwall@black"));
        UI.transform.Find("Canvas/Preview/Background/Stone").GetComponent<Button>().onClick
            .AddListener(() => ResetWall("woodwall@grey"));
        UnityEngine.Object.DontDestroyOnLoad(UI);
        UI.AddComponent<PreviewModelAngleController>();
        UI.SetActive(false);
        renderCamera = new GameObject("Render Camera", typeof(Camera)).GetComponent<Camera>();
        renderCamera.backgroundColor = new Color(0, 0, 0, 0);
        renderCamera.clearFlags = CameraClearFlags.SolidColor;
        renderCamera.transform.position = SpawnPoint;
        renderCamera.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
        renderCamera.fieldOfView = 0.5f;
        renderCamera.farClipPlane = 100000;
        renderCamera.targetTexture = new RenderTexture(2048, 2048, 32);
        UnityEngine.Object.DontDestroyOnLoad(renderCamera);
        Light = new GameObject("Render Light", typeof(Light)).GetComponent<Light>();
        Light.transform.position = SpawnPoint;
        Light.transform.rotation = Quaternion.Euler(5f, 180f, 5f);
        Light.type = LightType.Point;
        Light.intensity = 2f;
        Light.range = 30f;
        UnityEngine.Object.DontDestroyOnLoad(Light);
        renderCamera.gameObject.SetActive(false);
        Light.gameObject.SetActive(false);
        UI.GetComponentInChildren<RawImage>().texture = renderCamera.targetTexture;
        Marketplace.Global_Updator += Update;
    }

    public static GameObject CreatePlayerModel(string item, ItemDrop.ItemData.ItemType itemType, Color c)
    {
        if (ObjectDB.instance.GetItemPrefab(item.GetStableHashCode()) is not { } itemObj) return null;
        ItemDrop itemDrop = itemObj.GetComponent<ItemDrop>();
        if (!itemDrop) return null;

        GameObject toDeactivateTemp = itemType switch
        {
            ItemDrop.ItemData.ItemType.OneHandedWeapon => Player.m_localPlayer.m_visEquipment.m_rightItemInstance,
            ItemDrop.ItemData.ItemType.Bow => Player.m_localPlayer.m_visEquipment.m_leftItemInstance,
            ItemDrop.ItemData.ItemType.Shield => Player.m_localPlayer.m_visEquipment.m_leftItemInstance,
            ItemDrop.ItemData.ItemType.Helmet => Player.m_localPlayer.m_visEquipment.m_helmetItemInstance,
            ItemDrop.ItemData.ItemType.Chest => Player.m_localPlayer.m_visEquipment.m_chestItemInstances?[0],
            ItemDrop.ItemData.ItemType.Legs => Player.m_localPlayer.m_visEquipment.m_legItemInstances?[0],
            ItemDrop.ItemData.ItemType.TwoHandedWeapon => Player.m_localPlayer.m_visEquipment.m_rightItemInstance,
            ItemDrop.ItemData.ItemType.Torch => Player.m_localPlayer.m_visEquipment.m_leftItemInstance,
            ItemDrop.ItemData.ItemType.Shoulder => Player.m_localPlayer.m_visEquipment.m_shoulderItemInstances?[0],
            ItemDrop.ItemData.ItemType.Utility => Player.m_localPlayer.m_visEquipment.m_utilityItemInstances?[0],
            ItemDrop.ItemData.ItemType.Tool => Player.m_localPlayer.m_visEquipment.m_rightItemInstance,
            ItemDrop.ItemData.ItemType.TwoHandedWeaponLeft => Player.m_localPlayer.m_visEquipment.m_leftItemInstance,
            _ => null
        };
        if (toDeactivateTemp) toDeactivateTemp.SetActive(false);
        GameObject go = UnityEngine.Object.Instantiate(
            Player.m_localPlayer.GetComponentInChildren<Animator>().gameObject, SpawnPoint, Quaternion.identity);
        SkinnedMeshRenderer body = go.GetComponentInChildren<SkinnedMeshRenderer>();
        if (toDeactivateTemp) toDeactivateTemp.SetActive(true);

        if (itemType is ItemDrop.ItemData.ItemType.Chest)
        {
            body.material.SetTexture(ChestTex, Player.m_localPlayer.m_visEquipment.m_emptyBodyTexture);
            body.material.SetTexture(ChestBumpMap, null);
            body.material.SetTexture(ChestMetal, null);
        }

        if (itemType is ItemDrop.ItemData.ItemType.Legs)
        {
            body.material.SetTexture(LegsTex, Player.m_localPlayer.m_visEquipment.m_emptyLegsTexture);
            body.material.SetTexture(LegsBumpMap, null);
            body.material.SetTexture(LegsMetal, null);
        }

        Animator animator = go.GetComponent<Animator>();
        Dictionary<ItemDrop.ItemData.ItemType, Transform> attachments = new()
        {
            { ItemDrop.ItemData.ItemType.OneHandedWeapon, global::Utils.FindChild(go.transform, "RightHand_Attach") },
            { ItemDrop.ItemData.ItemType.Bow, global::Utils.FindChild(go.transform, "LeftHand_Attach") },
            { ItemDrop.ItemData.ItemType.Shield, global::Utils.FindChild(go.transform, "LeftHand_Attach") },
            { ItemDrop.ItemData.ItemType.Helmet, global::Utils.FindChild(go.transform, "Helmet_attach") },
            { ItemDrop.ItemData.ItemType.TwoHandedWeapon, global::Utils.FindChild(go.transform, "RightHand_Attach") },
            { ItemDrop.ItemData.ItemType.Torch, global::Utils.FindChild(go.transform, "RightHand_Attach") },
            {
                ItemDrop.ItemData.ItemType.TwoHandedWeaponLeft, global::Utils.FindChild(go.transform, "LeftHand_Attach")
            },
            { ItemDrop.ItemData.ItemType.Tool, global::Utils.FindChild(go.transform, "RightHand_Attach") },
        };

        if (itemObj.transform.Find("attach") is { } attach)
        {
            Transform attachPoint = attachments.TryGetValue(itemType, out Transform attachment)
                ? attachment
                : attachments[ItemDrop.ItemData.ItemType.OneHandedWeapon];
            GameObject weapon = UnityEngine.Object.Instantiate(attach.gameObject, attachPoint);
            weapon.transform.localPosition = Vector3.zero;
            weapon.transform.localRotation = Quaternion.identity;
            if (c != Color.white)
            {
                Utils.SetGOColors(weapon, c);
            }
        }
        else if (itemObj.transform.Find("attach_skin") is { } attach_skin)
        {
            Transform attachPoint = go.transform;
            GameObject armor = UnityEngine.Object.Instantiate(attach_skin.gameObject, attachPoint.position,
                attachPoint.rotation);
            armor.SetActive(true);
            armor.GetComponentInChildren<SkinnedMeshRenderer>().rootBone = body.rootBone;
            armor.GetComponentInChildren<SkinnedMeshRenderer>().bones = body.bones;
            armor.transform.SetParent(go.transform);
            CapsuleCollider[] list = go.GetComponentsInChildren<CapsuleCollider>(false).ToArray();
            foreach (Cloth cloth in armor.GetComponentsInChildren<Cloth>())
            {
                if (list.Length != 0)
                {
                    if (cloth.capsuleColliders.Length != 0)
                    {
                        List<CapsuleCollider> list2 = new List<CapsuleCollider>(list);
                        list2.AddRange(cloth.capsuleColliders);
                        cloth.capsuleColliders = list2.ToArray();
                    }
                    else
                    {
                        cloth.capsuleColliders = list;
                    }
                }
            }

            if (c != Color.white)
            {
                Utils.SetGOColors(armor, c);
            }

            if (itemDrop.m_itemData.m_shared.m_armorMaterial && itemType is ItemDrop.ItemData.ItemType.Chest)
            {
                body.material.SetTexture(ChestTex,
                    itemDrop.m_itemData.m_shared.m_armorMaterial.GetTexture(ChestTex));
                body.material.SetTexture(ChestBumpMap,
                    itemDrop.m_itemData.m_shared.m_armorMaterial.GetTexture(ChestBumpMap));
                body.material.SetTexture(ChestMetal,
                    itemDrop.m_itemData.m_shared.m_armorMaterial.GetTexture(ChestMetal));
            }

            if (itemDrop.m_itemData.m_shared.m_armorMaterial && itemType is ItemDrop.ItemData.ItemType.Legs)
            {
                body.material.SetTexture(LegsTex, itemDrop.m_itemData.m_shared.m_armorMaterial.GetTexture(LegsTex));
                body.material.SetTexture(LegsBumpMap,
                    itemDrop.m_itemData.m_shared.m_armorMaterial.GetTexture(LegsBumpMap));
                body.material.SetTexture(LegsMetal, itemDrop.m_itemData.m_shared.m_armorMaterial.GetTexture(LegsMetal));
            }
        }

        animator.SetBool(Wakeup, false);
        animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
        if (itemDrop.m_itemData.IsWeapon())
        {
            animator.SetFloat(Humanoid.s_statef, (float)itemDrop.m_itemData.m_shared.m_animationState);
            animator.SetInteger(Humanoid.s_statei, (int)itemDrop.m_itemData.m_shared.m_animationState);
        }
        else
        {
            animator.SetFloat(Humanoid.s_statef, Player.m_localPlayer.m_animator.GetFloat(Humanoid.s_statef));
            animator.SetInteger(Humanoid.s_statei, Player.m_localPlayer.m_animator.GetInteger(Humanoid.s_statei));
        }

        animator.Update(0);
        animator.speed = 0f;
        return go;
    }

    public static void SetAsCurrent(GameObject go)
    {
        StopPreview();
        if (!go) return;
        CurrentPreviewGO = go;
        CurrentPreviewGO.transform.position = Vector3.zero;
        CurrentPreviewGO.transform.rotation = Quaternion.Euler(2f, 0, 0);

        Vector3 min = new Vector3(1000f, 1000f, 1000f);
        Vector3 max = new Vector3(-1000f, -1000f, -1000f);
        foreach (Renderer meshRenderer in CurrentPreviewGO.GetComponentsInChildren<Renderer>())
        {
            if (meshRenderer is ParticleSystemRenderer) continue;
            min = Vector3.Min(min, meshRenderer.bounds.min);
            max = Vector3.Max(max, meshRenderer.bounds.max);
        }

        CurrentPreviewGO.transform.position = SpawnPoint - (min + max) / 2f;
        OriginalYPos = CurrentPreviewGO.transform.position.y;
        ResetWall(CurrentWall);
        Light.transform.position = SpawnPoint + new Vector3(0, 2f, 2f);

        Vector3 size = new Vector3(Mathf.Abs(min.x) + Mathf.Abs(max.x), Mathf.Abs(min.y) + Mathf.Abs(max.y),
            Mathf.Abs(min.z) + Mathf.Abs(max.z));
        float maxMeshSize = Mathf.Max(size.x, size.y) + 0.1f;
        float distance = maxMeshSize / Mathf.Tan(renderCamera.fieldOfView * Mathf.Deg2Rad) * 0.9f;
        renderCamera.transform.position = SpawnPoint + new Vector3(0, 0.05f, distance);
        OriginalCameraZPos = renderCamera.transform.position.z;
        UI.SetActive(true);
        renderCamera.gameObject.SetActive(true);
        Light.gameObject.SetActive(true);
    }

    public static string MakeScreenshot(GameObject go, int texSize)
    {
        StopPreview();
        if (!go) return null;
        Vector3 prevPos = go.transform.position;
        Quaternion prevRot = go.transform.rotation;
        go.transform.position = Vector3.zero;
        go.transform.rotation = Quaternion.Euler(2f, -24f, 0);

        Vector3 min = new Vector3(1000f, 1000f, 1000f);
        Vector3 max = new Vector3(-1000f, -1000f, -1000f);
        foreach (Renderer meshRenderer in go.GetComponentsInChildren<Renderer>())
        {
            if (meshRenderer is ParticleSystemRenderer) continue;
            min = Vector3.Min(min, meshRenderer.bounds.min);
            max = Vector3.Max(max, meshRenderer.bounds.max);
        }

        go.transform.position = SpawnPoint - (min + max) / 2f;
        Light.transform.position = SpawnPoint + new Vector3(0, 2f, 2f);

        float prevIntensity = Light.intensity;
        Light.intensity = 2f;

        Vector3 size = new Vector3(Mathf.Abs(min.x) + Mathf.Abs(max.x), Mathf.Abs(min.y) + Mathf.Abs(max.y), Mathf.Abs(min.z) + Mathf.Abs(max.z));
        float maxMeshSize = Mathf.Max(size.x, size.y);
        float distance = maxMeshSize / Mathf.Tan(renderCamera.fieldOfView * Mathf.Deg2Rad);
        renderCamera.transform.position = SpawnPoint + new Vector3(0, 0, distance);
        renderCamera.gameObject.SetActive(true);
        Light.gameObject.SetActive(true);

        Texture2D tex = new Texture2D(2048, 2048, TextureFormat.RGBA32, false);
        RenderTexture.active = renderCamera.targetTexture;
        renderCamera.Render();
        tex.ReadPixels(new Rect(0, 0, 2048, 2048), 0, 0);
        tex.Apply();
        RenderTexture.active = null;
        go.transform.position = prevPos;
        go.transform.rotation = prevRot;
        Light.intensity = prevIntensity;
        StopPreview();
        byte[] bytes = tex.CustomSize(texSize,texSize).EncodeToPNG();
        return Convert.ToBase64String(bytes);
    }

    private static void StopPreview()
    {
        if (CurrentPreviewGO)
        {
            UnityEngine.Object.Destroy(CurrentPreviewGO);
            CurrentPreviewGO = null;
        }

        if (BehindWallRender)
        {
            UnityEngine.Object.Destroy(BehindWallRender);
            BehindWallRender = null;
        }

        renderCamera.gameObject.SetActive(false);
        Light.gameObject.SetActive(false);
        UI.SetActive(false);
    }

    public class PreviewModelAngleController : MonoBehaviour, IDragHandler
    {
        public void OnDrag(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                CurrentPreviewGO.transform.Rotate(new Vector3(0, 1, 0), -eventData.delta.x / 2f, Space.World);
            }

            if (eventData.button == PointerEventData.InputButton.Right)
            {
                if (CurrentPreviewGO)
                {
                    float currentY = CurrentPreviewGO.transform.position.y;
                    float newY = currentY + (eventData.delta.y / 400f);
                    newY = Mathf.Clamp(newY, OriginalYPos - 1f, OriginalYPos + 1f);
                    Vector3 position = CurrentPreviewGO.transform.position;
                    position = new Vector3(position.x, newY, position.z);
                    CurrentPreviewGO.transform.position = position;
                }
            }
        }

        public void Update()
        {
            float ScrollWheel = Input.GetAxis("Mouse ScrollWheel");
            if (IsVisible && ScrollWheel != 0)
            {
                bool isMouseInside = RectTransformUtility.RectangleContainsScreenPoint(CHECK, Input.mousePosition);
                if (CurrentPreviewGO && isMouseInside)
                {
                    float currentZ = renderCamera.transform.position.z;
                    float newZ = currentZ - (ScrollWheel * 100f);
                    newZ = Mathf.Clamp(newZ, OriginalCameraZPos - 120f, OriginalCameraZPos);
                    Vector3 position = renderCamera.transform.position;
                    position = new Vector3(position.x, position.y, newZ);
                    renderCamera.transform.position = position;
                }
            }
        }
    }

    private static void ResetWall(string newWall)
    {
        AssetStorage.AUsrc.Play();
        if (BehindWallRender) UnityEngine.Object.Destroy(BehindWallRender);
        CurrentWall = newWall;

        bool grey = newWall.Contains("@grey");
        bool black = newWall.Contains("@black");
        string prefabName = newWall.Replace("@grey", "").Replace("@black", "");
        GameObject wallObj = ZNetScene.instance.GetPrefab(prefabName);
        if (wallObj == null) return;
        Vector3 scale = new Vector3(1.3f, 1.3f, 0.2f);
        Transform t = wallObj.transform.Find("New");
        if (!t) return;
        Vector3 pos = CurrentPreviewGO.transform.position;
        pos.y = OriginalYPos;
        BehindWallRender = UnityEngine.Object.Instantiate(t.gameObject, pos, Quaternion.identity);
        BehindWallRender.transform.localScale = scale;
        Vector3 position = BehindWallRender.transform.position + Vector3.up;
        position = new Vector3(position.x, position.y, position.z - 2f);
        BehindWallRender.transform.position = position;
        if (grey)
            BehindWallRender.GetComponentInChildren<Renderer>().material.color = Color.black;

        if (black)
        {
            BehindWallRender.GetComponentInChildren<Renderer>().material.shader = Transmogrification_UI.SPEC_SHADER;
            BehindWallRender.GetComponentInChildren<Renderer>().material.color = Color.black;
            BehindWallRender.GetComponentInChildren<Renderer>().material.SetColor("_SpecColor", Color.black);
        }
    }

    private enum Intensity
    {
        _025,
        _050,
        _1,
        _15,
        _2
    }

    private static Intensity LightIntensity = Intensity._2;

    private static void ChangeLight()
    {
        AssetStorage.AUsrc.Play();
        LightIntensity = (Intensity)(((int)LightIntensity + 1) % 5);
        switch (LightIntensity)
        {
            case Intensity._025:
                Light.intensity = 0.25f;
                break;
            case Intensity._050:
                Light.intensity = 0.5f;
                break;
            case Intensity._1:
                Light.intensity = 1f;
                break;
            case Intensity._15:
                Light.intensity = 1.5f;
                break;
            case Intensity._2:
                Light.intensity = 2f;
                break;
        }
    }

    private static bool IsVisible => CurrentPreviewGO && CurrentPreviewGO.activeSelf;

    [HarmonyPatch(typeof(TextInput), nameof(TextInput.IsVisible))]
    [ClientOnlyPatch]
    private static class Menu_IsVisible_Patch
    {
        [UsedImplicitly]
        private static void Postfix(ref bool __result) => __result |= IsVisible;
    }

    private static void Update(float _)
    {
        if (Input.GetKeyDown(KeyCode.Escape) && IsVisible)
        {
            StopPreview();
        }
    }
}