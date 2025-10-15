using System.Text.RegularExpressions;
using API;
using Marketplace.ExternalLoads;
using Marketplace.Hammer;
using Marketplace.Modules.Banker;
using Marketplace.Modules.Buffer;
using Marketplace.Modules.CMS;
using Marketplace.Modules.Feedback;
using Marketplace.Modules.Gambler;
using Marketplace.Modules.Global_Options;
using Marketplace.Modules.MainMarketplace;
using Marketplace.Modules.Dialogues;
using Marketplace.Modules.Quests;
using Marketplace.Modules.ServerInfo;
using Marketplace.Modules.Teleporter;
using Marketplace.Modules.Trader;
using Marketplace.Modules.Transmogrification;
using UnityEngine.EventSystems;
using GameObject = UnityEngine.GameObject;
using Image = UnityEngine.UI.Image;
using Object = UnityEngine.Object;

namespace Marketplace.Modules.NPC;

[UsedImplicitly]
[Market_Autoload(Market_Autoload.Type.Both, Market_Autoload.Priority.Normal)]
public static class Market_NPC
{
    public static GameObject NPC = null!;
    private static readonly string[] TypeNames = Enum.GetNames(typeof(NPCType));
    private static readonly int SkinColor = Shader.PropertyToID("_SkinColor");
    private static readonly int MainTex = Shader.PropertyToID("_MainTex");
    private static readonly int SkinBumpMap = Shader.PropertyToID("_SkinBumpMap");
    private static readonly int ChestTex = Shader.PropertyToID("_ChestTex");
    private static readonly int ChestBumpMap = Shader.PropertyToID("_ChestBumpMap");
    private static readonly int ChestMetal = Shader.PropertyToID("_ChestMetal");
    private static readonly int LegsTex = Shader.PropertyToID("_LegsTex");
    private static readonly int LegsBumpMap = Shader.PropertyToID("_LegsBumpMap");
    private static readonly int LegsMetal = Shader.PropertyToID("_LegsMetal");
    private static readonly int Wakeup = Animator.StringToHash("wakeup");
    private static readonly int Crafting = Animator.StringToHash("crafting");

    [HarmonyPatch(typeof(FejdStartup), nameof(FejdStartup.Awake))]
    [ClientOnlyPatch]
    private static class FejdStartup_Awake_Patch
    {
        private static bool done;

        [UsedImplicitly]
        private static void Postfix(FejdStartup __instance)
        {
            if (done) return;
            done = true;
            if (__instance.transform.Find("StartGame/Panel/JoinPanel/serverCount")
                    ?.GetComponent<TextMeshProUGUI>() is not { } tmp) return;
            NPC.transform.Find("TMP").GetComponent<TMP_Text>().font = tmp.font;
            NPC.transform.Find("TMP").GetComponent<TMP_Text>().outlineWidth = 0.075f;
            Utils.print("Replaced TMP for NPC");
        }
    }
    
    public enum NPCType
    {
        None,
        Trader,
        Info,
        Teleporter,
        Feedback,
        Banker,
        Gambler,
        Quests,
        Buffer,
        Transmog,
        Marketplace,
        Mail
    }

    [UsedImplicitly]
    private static void OnInit()
    {
        NPC = AssetStorage.asset.LoadAsset<GameObject>("MarketPlaceNPC");
        NPC.AddComponent<NPCcomponent>();
        if (Marketplace.WorkingAsType is Marketplace.WorkingAs.Server) return;
        NPC.transform.Find("TMP").gameObject.AddComponent<TextComponent>();
        NPCUI.Init();
        Marketplace.Global_Updator += UpdateNPCGUI;
    }
 
    [HarmonyPatch(typeof(FejdStartup), nameof(FejdStartup.Start))]

    private static class FejdStartup_Start_Patch
    {
        [UsedImplicitly]
        private static void Postfix(FejdStartup __instance)
        {
            Animator animator = NPC.GetComponentInChildren<Animator>(true);
            animator.gameObject.AddComponent<CustomLookAt>(); 
            Shader creatureShader = Resources.FindObjectsOfTypeAll<Shader>().FirstOrDefault(s => s.name == "Custom/Creature");
            if (!creatureShader) return;
            SkinnedMeshRenderer mesh = animator.gameObject.GetComponentInChildren<SkinnedMeshRenderer>(true);
            foreach (Material material in mesh.materials)
            {
                material.shader = creatureShader;
            }
        }
    }

    private static void UpdateNPCGUI(float dt)
    {
        if (Input.GetKeyDown(KeyCode.Escape) && NPCUI.IsVisible())
        {
            NPCUI.Hide();
            Menu.instance.OnClose();
        }
    }

    [HarmonyPatch(typeof(ZNetScene), nameof(ZNetScene.Awake))]
    private static class NPCZnet
    {
        [UsedImplicitly]
        private static void Postfix(ZNetScene __instance)
        {
            __instance.m_namedPrefabs.Add(NPC.name.GetStableHashCode(), NPC);
        }
    }

    [HarmonyPatch(typeof(ZNetScene), nameof(ZNetScene.Awake))]
    [ClientOnlyPatch]
    private static class OdinFix
    {
        [UsedImplicitly]
        private static void Postfix(ZNetScene __instance)
        {
            GameObject odin = __instance.GetPrefab("odin");
            if (odin?.GetComponent<Animator>() is not { } anim) return;
            Object.Destroy(anim);
        }
    }

    [HarmonyPatch(typeof(TextInput), nameof(TextInput.IsVisible))]
    [ClientOnlyPatch]
    private static class IsVisiblePatch
    {
        [UsedImplicitly]
        private static void Postfix(ref bool __result)
        {
            if (NPCUI.IsVisible()) __result = true;
        }
    }


    private class TextComponent : MonoBehaviour
    {
        private void Update()
        {
            Vector3 rot = Quaternion.LookRotation(GameCamera.instance.transform.forward).eulerAngles;
            rot.z = 0;
            transform.rotation = Quaternion.Euler(rot);
        }
    }

    public class HoverOnButton_WithImage : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private static readonly Dictionary<string, Sprite> Cache = new();
        private bool HasImage;
        private Image _image = null!;
        private Sprite _sprite = null!;
        private Transform _tt = null!;

        private void Awake()
        {
            _image = GetComponent<Image>();
        }

        public void Setup(string _name, Transform t, string base64)
        {
            _tt = t;
            try
            {
                if (Cache.TryGetValue(_name, out Sprite value))
                {
                    _sprite = value;
                    HasImage = true;
                    return;
                }

                byte[] bytes = Convert.FromBase64String(base64);
                Texture2D texture = new Texture2D(1, 1);
                texture.LoadImage(bytes);
                _sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
                Cache.Add(_name, _sprite);
                HasImage = true;
            }
            catch
            {
                HasImage = false;
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _image.color = Color.green;
            if (HasImage)
            {
                _tt.gameObject.SetActive(true);
                _tt.transform.GetChild(0).GetComponent<Image>().sprite = _sprite;
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _image.color = Color.white;
            _tt.gameObject.SetActive(false);
        }
    }

    private class CustomLookAt : MonoBehaviour
    {
        private Animator animator = null!;
        private Vector3 currentLookAt = Vector3.zero;
        private ZNetView _znet = null!;

        private void Awake()
        {
            _znet = GetComponentInParent<ZNetView>();
            animator = GetComponent<Animator>();
            Transform transform1 = transform;
            currentLookAt = transform1.position + transform1.forward * 4f + Vector3.up * 1.2f;
        }

        private void OnAnimatorIK(int layerIndex)
        {
            if (!_znet.IsValid()) return;
            if (animator && Player.m_localPlayer)
            {
                float speed = 2f;
                Vector3 target = Player.m_localPlayer.m_head.position;
                if (Vector3.Distance(target, transform.position) > 4f)
                {
                    Transform transform1 = transform;
                    target = transform1.position + transform1.forward * 3f + Vector3.up * 1.2f;
                    speed = 4f;
                }
                else
                {
                    Vector3 pVec = (transform.position - target).normalized;
                    float playerVec = Quaternion.LookRotation(pVec).eulerAngles.y;
                    float enemyVec = transform.rotation.eulerAngles.y;
                    float num = Mathf.Abs(Mathf.DeltaAngle(playerVec, enemyVec));
                    if (num < 100f)
                    {
                        Transform transform1 = transform;
                        target = transform1.position + transform1.forward * 3f + Vector3.up * 1.2f;
                    }
                }

                currentLookAt = Vector3.MoveTowards(currentLookAt, target, Time.deltaTime * speed);
                animator.SetLookAtPosition(currentLookAt);
                animator.SetLookAtWeight(1f, 0.1f, 1f, 0f);
            }
        }
    }


    public class NPCcomponent : MonoBehaviour, Hoverable, Interactable, IDestructible
    {
        private const string PatrolData = "KGmarket PatrolData";
        private const string LatestPatrolPoint = "KGmarket LatestPatrolPoint";
        private const string GoBackPatrol = "KGmarket GoBack";

        public static readonly List<NPCcomponent> ALL = new();
        public ZNetView znv = null!;
        public GameObject pastOverrideModel;
        public NPCType _currentNpcType;

        public ZSyncAnimation zanim = null!;
        private TMP_Text canvas = null!;
        public AudioSource NPC_SoundSource = null!;
        private bool WasClose;
        private Vector2[] PatrolArray;
        private float MaxSpeed;
        private float ForwardSpeed;
        private float PatrolTime;
        private float periodicAnimationTimer = 999f;
        private float periodicSoundTimer = 999f;


        [HarmonyPatch(typeof(MonoUpdaters), nameof(MonoUpdaters.Update))]
        [ClientOnlyPatch]
        private static class MonoUpdaters_Update_Patch
        {
            [UsedImplicitly]
            private static void Postfix()
            {
                foreach (NPCcomponent ccomponent in ALL)
                    ccomponent.CustomUpdate();
            }
        }  

        [HarmonyPatch(typeof(MonoUpdaters), nameof(MonoUpdaters.FixedUpdate))]
        private static class MonoUpdaters_FixedUpdate_Patch
        {
            [UsedImplicitly]
            private static void Postfix()
            {
                foreach (NPCcomponent ccomponent in ALL)
                    ccomponent.CustomFixedUpdate();
            }
        }


        private void OnDestroy()
        {
            ALL.Remove(this);
        }

        private void InitPatrolData(string ForceData)
        {
            ForwardSpeed = 0f;
            if (zanim.enabled)
                zanim.SetFloat(Character.s_forwardSpeed, 0);
            MaxSpeed = 1f;
            if (string.IsNullOrEmpty(ForceData))
            {
                PatrolArray = null;
                return;
            }
            try
            {
                string[] maxSpeed = ForceData.Split('@');
                string[] split = maxSpeed[0].Replace(" ", "").Split(',');
                if (maxSpeed.Length == 2)
                {
                    MaxSpeed = Convert.ToSingle(maxSpeed[1], new CultureInfo("en-US"));
                }

                PatrolArray = new Vector2[split.Length / 2];
                for (int i = 0; i < split.Length; i += 2)
                {
                    float x = Convert.ToSingle(split[i], new CultureInfo("en-US"));
                    float y = Convert.ToSingle(split[i + 1], new CultureInfo("en-US"));
                    PatrolArray[i / 2] = new Vector2(x, y);
                }
            }
            catch
            {
                PatrolArray = null;
            }
        }

        private void CustomUpdate()
        {
            PatrolTime += Time.deltaTime;
            if (znv.m_zdo == null || !znv.IsOwner() || PatrolArray is not { Length: > 1 } || PatrolTime < 2f) return;
            Player closePlayer = Player.GetClosestPlayer(transform.position, 3f);
            ForwardSpeed = closePlayer && _currentNpcType is not NPCType.None
                ? Mathf.Clamp(ForwardSpeed - Time.deltaTime, 0, MaxSpeed)
                : Mathf.Clamp(ForwardSpeed + Time.deltaTime, 0, MaxSpeed);
            
            if (zanim.enabled)
                zanim.SetFloat(Character.s_forwardSpeed, ForwardSpeed * 1.5f);
            int currentPatrolPoint = znv.m_zdo.GetInt(LatestPatrolPoint);
            Vector3 position = transform.position;
            Vector2 currentPos = new Vector2(position.x, position.z);
            Vector2 move = Vector2.MoveTowards(currentPos, PatrolArray[currentPatrolPoint],
                Time.deltaTime * ForwardSpeed);
            Vector3 targetPoint = new Vector3(move.x, position.y, move.y);
            Utils.CustomFindFloor(targetPoint, out float height);
            height = Mathf.Max(30, height, ZoneSystem.instance.GetGroundHeight(targetPoint));
            if (ForwardSpeed > 0)
            {
                Vector3 vecTest = (targetPoint - transform.position).normalized;
                if (vecTest != Vector3.zero)
                {
                    Quaternion newRotation = Quaternion.LookRotation(vecTest);
                    newRotation.x = 0;
                    transform.rotation = Quaternion.RotateTowards(transform.rotation, newRotation, Time.deltaTime * 200f);
                }
            }

            transform.position = new Vector3(targetPoint.x, height, targetPoint.z);
            if (Vector2.Distance(PatrolArray[currentPatrolPoint],
                    new Vector2(transform.position.x, transform.position.z)) <= 0.25f)
            {
                bool goBack = znv.m_zdo.GetBool(GoBackPatrol);
                if (goBack)
                {
                    currentPatrolPoint--;

                    if (currentPatrolPoint < 0)
                    {
                        currentPatrolPoint = 1;
                        znv.m_zdo.Set(GoBackPatrol, false);
                    }

                    znv.m_zdo.Set(LatestPatrolPoint, currentPatrolPoint);
                    ForwardSpeed = MaxSpeed / 2.5f;
                }
                else
                {
                    currentPatrolPoint++;
                    if (currentPatrolPoint >= PatrolArray.Length)
                    {
                        currentPatrolPoint -= 2;
                        znv.m_zdo.Set(GoBackPatrol, true);
                    }

                    znv.m_zdo.Set(LatestPatrolPoint, currentPatrolPoint);
                    ForwardSpeed = MaxSpeed / 2.5f;
                }
            }
        }

        private void CustomFixedUpdate()
        {
            if (!znv || znv.m_zdo == null) return;
            Vector3 target = Player.m_localPlayer?.m_head.position ?? Vector3.zero;
            Vector3 pVec = (transform.position - target).normalized;
            float playerVec = Quaternion.LookRotation(pVec).eulerAngles.y;
            float enemyVec = transform.rotation.eulerAngles.y;
            float num = Mathf.Abs(Mathf.DeltaAngle(playerVec, enemyVec));

            if (znv.IsOwner() && znv.m_zdo.GET_NPC_PeriodicAnimationTime() > 0)
            {
                periodicAnimationTimer += Time.fixedDeltaTime;
                if (periodicAnimationTimer >= znv.m_zdo.GET_NPC_PeriodicAnimationTime())
                {
                    periodicAnimationTimer = 0;
                    if (!string.IsNullOrWhiteSpace(znv.m_zdo.GET_NPC_PeriodicAnimation()))
                    {
                        zanim.SetTrigger(znv.m_zdo.GET_NPC_PeriodicAnimation());
                    }
                }
            }

            if (znv.m_zdo.GET_NPC_PeriodicSoundTime() > 0)
            {
                periodicSoundTimer += Time.fixedDeltaTime;
                if (periodicSoundTimer >= znv.m_zdo.GET_NPC_PeriodicSoundTime())
                {
                    periodicSoundTimer = 0;
                    if (!string.IsNullOrWhiteSpace(znv.m_zdo.GET_NPC_PeriodicSound()))
                    {
                        if (AssetStorage.NPC_AudioClips.TryGetValue(znv.m_zdo.GET_NPC_PeriodicSound(),
                                out AudioClip sound))
                        {
                            if (!NPC_SoundSource.isPlaying)
                            {
                                NPC_SoundSource.clip = sound;
                                NPC_SoundSource.Play();
                            }
                        }
                    }
                }
            }


            if (!WasClose)
            {
                if (Vector3.Distance(target, transform.position) < 10f)
                {
                    WasClose = true;
                    
                    string npcDialogue = znv.m_zdo.GET_NPC_Dialogue();
                    if (!string.IsNullOrEmpty(npcDialogue) && Dialogues_DataTypes.ClientReadyDialogues.TryGetValue(npcDialogue, out var inDialogue))
                    {
                        foreach (var interaction in inDialogue.Interactions)
                        {
                            if (interaction.Type is not Dialogues_DataTypes.RawDialogue.InteractionType.InRange) continue;
                            if (!interaction.CheckCondition(this, out _)) continue;
                            interaction.CallCommands(this);
                        }
                    }
                    
                    
                    if (num > 80f)
                    {
                        if (!string.IsNullOrWhiteSpace(znv.m_zdo.GET_NPC_GreetingText()))
                        {
                            Chat.instance.SetNpcText(gameObject, Vector3.up * 4f, 20f, 3f, "",
                                znv.m_zdo.GET_NPC_GreetingText(), false);
                        }

                        if (!string.IsNullOrWhiteSpace(znv.m_zdo.GET_NPC_GreetingAnimation()))
                        {
                            zanim.SetTrigger(znv.m_zdo.GET_NPC_GreetingAnimation());
                        }
                    }
                }
            }
            else
            {
                if (Vector3.Distance(target, transform.position) > 12f)
                {
                    WasClose = false;
                    
                    string npcDialogue = znv.m_zdo.GET_NPC_Dialogue();
                    if (!string.IsNullOrEmpty(npcDialogue) && Dialogues_DataTypes.ClientReadyDialogues.TryGetValue(npcDialogue, out var inDialogue))
                    {
                        foreach (var interaction in inDialogue.Interactions)
                        {
                            if (interaction.Type is not Dialogues_DataTypes.RawDialogue.InteractionType.OutRange) continue;
                            if (!interaction.CheckCondition(this, out _)) continue;
                            interaction.CallCommands(this);
                        }
                    }
                    
                    if (num > 80f)
                    {
                        if (!string.IsNullOrWhiteSpace(znv.m_zdo.GET_NPC_ByeText()))
                        {
                            Chat.instance.SetNpcText(gameObject, Vector3.up * 4f, 20f, 3f, "",
                                znv.m_zdo.GET_NPC_ByeText(), false);
                        }

                        if (!string.IsNullOrWhiteSpace(znv.m_zdo.GET_NPC_ByeAnimation()))
                        {
                            zanim.SetTrigger(znv.m_zdo.GET_NPC_ByeAnimation());
                        }
                    }
                }
            }
        }

        private void Awake() 
        {
            znv = GetComponent<ZNetView>();
            if (!znv || !znv.IsValid()) return;
            ALL.Add((this));
            canvas = transform.Find("TMP").GetComponent<TMP_Text>();
            zanim = GetComponent<ZSyncAnimation>();
            if (!znv.m_functions.ContainsKey("KGMarket changeNpcType".GetStableHashCode()))
            {
                znv.Register("KGMarket changeNpcType", new Action<long, int, bool, string>(ChangeNpcType));
                znv.Register("KGmarket changeprofile", new Action<long, string, string>(ChangeProfile));
                znv.Register("KGmarket overridename", new Action<long, string>(OverrideName));
                znv.Register("KGmarket overridemodel", new Action<long, string>(OverrideModel));
                znv.Register("KGmarket fashion", new Action<long, NPC_DataTypes.NPC_Fashion>(FashionApply));
                znv.Register("KGmarket GetPatrolData", new Action<long, string>(GetPatrolData));
            } 
        } 

        private void Start()
        {
            if (!znv || !znv.IsValid()) return;
            NPC_SoundSource = gameObject.AddComponent<AudioSource>();
            NPC_SoundSource.spatialBlend = 1;
            NPC_SoundSource.volume = 0.8f;
            NPC_SoundSource.outputAudioMixerGroup = AudioMan.instance.m_masterMixer.outputAudioMixerGroup;
            NPC_SoundSource.maxDistance = 10f;
            NPC_SoundSource.bypassReverbZones = true;
            transform.Find("NPC").GetComponentInChildren<Collider>().gameObject.layer = LayerMask.NameToLayer("character");
            int npcType = znv.m_zdo.GetInt("KGmarketNPC");
            if (npcType >= TypeNames.Length) npcType = 0;
            _currentNpcType = (NPCType)npcType;
            OverrideModel(0, znv.m_zdo.GetString("KGnpcModelOverride"));
            GameObject go = Instantiate(AssetStorage.MarketplaceQuestQuestionIcon, transform);
            go.transform.position += Vector3.up * 4.5f;
            go.name = "MPASNquest";
            bool isQuestTarget = Quests_DataTypes.Quest.IsQuestTarget(GetNPCName());
            go.SetActive(isQuestTarget);

            GameObject hasQuests = Instantiate(AssetStorage.MarketplaceQuestHasQuestIcon, transform);
            hasQuests.transform.position += Vector3.up * 4.5f;
            hasQuests.name = "MPASNHasQuests";
            hasQuests.SetActive(!isQuestTarget && Quests_DataTypes.Quest.HasQuests(this));
            
            InitPatrolData(znv.m_zdo.GetString(PatrolData));
        }


        public void GetPatrolData(long sender, string data)
        {
            if (znv.IsOwner())
            {
                znv.m_zdo.Set(PatrolData, data);
                znv.m_zdo.Set(LatestPatrolPoint, 0);
                znv.m_zdo.Set(GoBackPatrol, false);
            }

            InitPatrolData(data);
        }


        public string GetHoverText()
        {
            if (TextInput.IsVisible()) return "";

            string admintext = Utils.IsDebug_Marketplace
                ? "\n" + Localization.instance.Localize("[<color=yellow><b>$KEY_AltPlace + $KEY_Use</b></color>]") +
                  " " + Localization.instance.Localize("$mpasn_opennnpcui") +
                  "\n" + Localization.instance.Localize("[<color=yellow><b>ALT + $KEY_Use</b></color>]") +
                  " " + Localization.instance.Localize("$mpasn_fashionmenu") +
                  "\n" + Localization.instance.Localize("[<color=yellow><b>C + $KEY_Use</b></color>]") +
                  " " + Localization.instance.Localize("$mpasn_savenpc")
                : "";
            string text = Localization.instance.Localize("[<color=yellow><b>$KEY_Use</b></color>] ") +
                          Localization.instance.Localize("$mpasn_interact");

            return text + admintext;
        }

        public string GetHoverName()
        {
            return "";
        }

        public string GetNPCName()
        {
            return znv.m_zdo?.GetString("KGnpcNameOverride") ?? "";
        }

        public string GetClearNPCName()
        {
            return Regex.Replace(GetNPCName(), @"</?(?!color\b)\w+[^>]*>", ""); 
        }

        public void OpenUIForType()
        {
            OpenUIForType(_currentNpcType, znv.m_zdo.GET_NPC_Profile(), GetClearNPCName());
        } 

        public static void OpenUIForType(string type, string profile, string npcName) =>
            OpenUIForType((NPCType)Enum.Parse(typeof(NPCType), type, true), profile, npcName);

        public static void OpenUIForType(NPCType? type, string profile, string npcName)
        {
            if (type == null || profile == null) return;
            profile = profile!.ToLower();
            switch (type) 
            { 
                case NPCType.Marketplace: 
                    if (!string.IsNullOrWhiteSpace(Global_Configs._localUserID)) Marketplace_UI.Show();
                    break;
                case NPCType.Info:
                    ServerInfo_UI.Show(profile, npcName);
                    break;
                case NPCType.Trader:
                    Trader_UI.Show(profile, npcName);
                    break;
                case NPCType.Banker:
                    Banker_UI.Show(profile, npcName);
                    break;
                case NPCType.Teleporter:
                    Teleporter_Main_Client.ShowTeleporterUI(profile);
                    break;
                case NPCType.Feedback:
                    Feedback_UI.Show();
                    break;
                case NPCType.Gambler:
                    Gambler_UI.Show(profile);
                    break;
                case NPCType.Quests:
                    Quests_UIs.QuestUI.Show(profile, npcName);
                    break;
                case NPCType.Buffer:
                    Buffer_UI.Show(profile, npcName);
                    break;
                case NPCType.Transmog:
                    Transmogrification_UI.Show(profile, npcName);
                    break;
                case NPCType.Mail:
                    CentralizedMailSystem_UI.Show(true);
                    break;
            }
        } 

        public bool Interact(Humanoid user, bool hold, bool alt)
        {
            if (!string.IsNullOrWhiteSpace(znv.m_zdo.GetString("KGinteractAnimation")))
            {
                zanim.SetTrigger(znv.m_zdo.GetString("KGinteractAnimation"));
            }

            string interactAudio = znv.m_zdo.GetString("KGinteractSound");
            NPC_SoundSource.Stop();
            if (!string.IsNullOrEmpty(interactAudio) &&
                AssetStorage.NPC_AudioClips.TryGetValue(interactAudio, out AudioClip interactClip))
            {
                NPC_SoundSource.clip = interactClip;
                NPC_SoundSource.Play();
            }

            if (Input.GetKey(KeyCode.C) && Utils.IsDebug_Marketplace)
            {
                MarketplaceHammer.SaveNPC(this);
                return true;
            }

            if (Input.GetKey(KeyCode.LeftAlt) && Utils.IsDebug_Marketplace)
            {
                NPCUI.ShowFashion(znv.m_zdo);
                return true;
            }

            if (alt && Utils.IsDebug_Marketplace)
            {
                NPCUI.ShowMain(znv.m_zdo);
                return true;
            }

            if (Quests_DataTypes.Quest.TryAddRewardTalk(GetClearNPCName()))
            {
                Quests_UIs.AcceptedQuestsUI.CheckQuests();
                return true;
            }

            if (!string.IsNullOrWhiteSpace(znv.m_zdo.GetString("KGnpcDialogue")))
            {
                Dialogues_UI.LoadDialogue(this, znv.m_zdo.GetString("KGnpcDialogue"));
                return true;
            }

            OpenUIForType();
            return true;
        }

        public bool UseItem(Humanoid user, ItemDrop.ItemData item)
        {
            return false;
        }

        public void FashionApply(long sender, NPC_DataTypes.NPC_Fashion fashion)
        {
            if (znv.m_zdo.IsOwner())
            {
                znv.m_zdo.Set("KGleftItem", fashion.LeftItem);
                znv.m_zdo.Set("KGrightItem", fashion.RightItem);
                znv.m_zdo.Set("KGhelmetItem", fashion.HelmetItem);
                znv.m_zdo.Set("KGchestItem", fashion.ChestItem);
                znv.m_zdo.Set("KGlegsItem", fashion.LegsItem);
                znv.m_zdo.Set("KGcapeItem", fashion.CapeItem);
                znv.m_zdo.Set("KGhairItem", fashion.HairItem);
                znv.m_zdo.Set("KGhairItemColor", fashion.HairColor);
                znv.m_zdo.Set("KGLeftItemBack", fashion.LeftItemHidden);
                znv.m_zdo.Set("KGRightItemBack", fashion.RightItemHidden);
                znv.m_zdo.Set("KGinteractAnimation", fashion.InteractAnimation);
                znv.m_zdo.Set("KGgreetingAnimation", fashion.GreetAnimation);
                znv.m_zdo.Set("KGbyeAnimation", fashion.ByeAnimation);
                znv.m_zdo.Set("KGgreetingText", fashion.GreetText);
                znv.m_zdo.Set("KGbyeText", fashion.ByeText);
                znv.m_zdo.Set("KGskinColor", fashion.SkinColor);
                znv.m_zdo.Set("KGcraftingAnimation",
                    int.TryParse(fashion.CraftingAnimation, out int craftingZDO) ? craftingZDO : 0);
                znv.m_zdo.Set("KGbeardItem", fashion.BeardItem);
                znv.m_zdo.Set("KGbeardColor", fashion.BeardColor);
                znv.m_zdo.Set("KGinteractSound", fashion.InteractAudioClip);
                znv.m_zdo.Set("KGtextSize",
                    float.TryParse(fashion.TextSize, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture,
                        out float textSizeZDO)
                        ? textSizeZDO
                        : 3);
                znv.m_zdo.Set("KGtextHeight",
                    float.TryParse(fashion.TextHeight, NumberStyles.Any, CultureInfo.InvariantCulture,
                        out float textHeightZDO)
                        ? textHeightZDO
                        : 0f);
                znv.m_zdo.Set("KGperiodicAnimation", fashion.PeriodicAnimation);
                znv.m_zdo.Set("KGperiodicAnimationTime",
                    float.TryParse(fashion.PeriodicAnimationTime, NumberStyles.AllowDecimalPoint,
                        CultureInfo.InvariantCulture,
                        out float periodicAnimationTimeZDO)
                        ? periodicAnimationTimeZDO
                        : 0f);
                znv.m_zdo.Set("KGperiodicSound", fashion.PeriodicSound);
                znv.m_zdo.Set("KGperiodicSoundTime",
                    float.TryParse(fashion.PeriodicSoundTime, NumberStyles.AllowDecimalPoint,
                        CultureInfo.InvariantCulture,
                        out float periodicSoundTimeZDO)
                        ? periodicSoundTimeZDO
                        : 0f);
                if (!float.TryParse(fashion.ModelScale, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture,
                        out float scaleFloat)) scaleFloat = 1f;

                znv.m_zdo.Set("KGnpcScale", scaleFloat);
            }

            periodicAnimationTimer = 999f;
            periodicSoundTimer = 999f;
            string prefab = znv.m_zdo.GetString("KGnpcModelOverride");
            if (TryOverrideModel(ref prefab, out bool isFemale, false))
            {
                pastOverrideModel!.GetComponent<Animator>()?.SetBool(Wakeup, false);
                if (int.TryParse(fashion.CraftingAnimation, out int CRAFTING))
                {
                    pastOverrideModel.GetComponent<Animator>()?.SetInteger(Crafting, CRAFTING);
                }

                VisEquipment visuals = ZNetScene.instance.GetPrefab(prefab).GetComponent<VisEquipment>();
                if (visuals)
                {
                    pastOverrideModel.GetComponent<Animator>()?.SetBool(Wakeup, false);
                    SkinnedMeshRenderer skin = pastOverrideModel.GetComponentInChildren<SkinnedMeshRenderer>();
                    Transform leftArm = global::Utils.FindChild(pastOverrideModel.transform, visuals.m_leftHand?.name);
                    Transform rightgArm = global::Utils.FindChild(pastOverrideModel.transform, visuals.m_rightHand?.name);
                    Transform helmetJoint = global::Utils.FindChild(pastOverrideModel.transform, visuals.m_helmet?.name);
                    Transform leftBack = global::Utils.FindChild(pastOverrideModel.transform, visuals.m_backShield?.name);
                    Transform rightBack = global::Utils.FindChild(pastOverrideModel.transform, visuals.m_backMelee?.name);
                    string leftItem = fashion.LeftItem;
                    string rightItem = fashion.RightItem;
                    string helmetItem = fashion.HelmetItem;
                    string leftBackItem = fashion.LeftItemHidden;
                    string rightBackItem = fashion.RightItemHidden;
                    EquipItemsOnModel(leftArm, leftItem, skin);
                    EquipItemsOnModel(rightgArm, rightItem, skin);
                    if (ZNetScene.instance.GetPrefab(leftBackItem))
                    {
                        if (ZNetScene.instance.GetPrefab(leftBackItem).GetComponent<ItemDrop>().m_itemData
                            .IsWeapon())
                        {
                            EquipItemsOnModel(rightBack, leftBackItem, skin, true);
                        }
                        else
                        {
                            EquipItemsOnModel(leftBack, leftBackItem, skin);
                        }
                    }

                    EquipItemsOnModel(rightBack, rightBackItem, skin);
                    EquipItemsOnModel(helmetJoint, helmetItem, skin);
                    if (pastOverrideModel.GetComponent<CustomLookAt>() == null && CRAFTING == 0)
                    {
                        pastOverrideModel.AddComponent<CustomLookAt>();
                    }

                    CapsuleCollider[] capsule = pastOverrideModel.GetComponentsInChildren<CapsuleCollider>().Where(x => x != pastOverrideModel.GetComponent<CapsuleCollider>()).ToArray();
                    string chestItem = fashion.ChestItem;
                    string legsItem = fashion.LegsItem;
                    string capeItem = fashion.CapeItem;
                    EquipItemsOnModel(skin, capeItem, capsule);
                    EquipItemsOnModel(skin, chestItem, capsule);
                    EquipItemsOnModel(skin, legsItem, capsule);
                    string hairItem = "Hair" + fashion.HairItem;
                    GameObject hair = EquipItemsOnModel(helmetJoint, hairItem, skin);
                    ColorUtility.TryParseHtmlString(fashion.HairColor, out Color c);
                    if (hair)
                    {
                        Renderer[] componentsInChildren = hair!.GetComponentsInChildren<Renderer>();
                        foreach (Renderer render in componentsInChildren)
                            render.material.SetColor(SkinColor, c);
                    }

                    string beardItem = "Beard" + fashion.BeardItem;
                    GameObject beard = EquipItemsOnModel(helmetJoint, beardItem, skin);
                    ColorUtility.TryParseHtmlString(fashion.BeardColor, out Color beardColor);
                    if (beard)
                    {
                        Renderer[] componentsInChildren = beard!.GetComponentsInChildren<Renderer>();
                        foreach (Renderer render in componentsInChildren)
                            render.material.SetColor(SkinColor, beardColor);
                    }


                    if (isFemale)
                    {
                        skin.sharedMesh = visuals.m_models[1].m_mesh;
                        skin.materials[0].SetTexture(MainTex,
                            visuals.m_models[1].m_baseMaterial.GetTexture(MainTex));
                        skin.materials[0].SetTexture(SkinBumpMap,
                            visuals.m_models[1].m_baseMaterial.GetTexture(SkinBumpMap));
                    }

                    ColorUtility.TryParseHtmlString(fashion.SkinColor, out Color skinColor);
                    skin.materials[0].SetColor(SkinColor, skinColor);
                }
            }

            if (pastOverrideModel)
            {
                if (!float.TryParse(fashion.ModelScale, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture,
                        out float val)) val = 1f;
                if (val < 0.1f) val = 0.1f;
                pastOverrideModel!.transform.localScale *= val;
            }

            float.TryParse(fashion.TextSize, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture,
                out float textSize);
            float.TryParse(fashion.TextHeight, NumberStyles.Any, CultureInfo.InvariantCulture,
                out float textAdditionalHeight);
            canvas.gameObject.transform.localScale = new Vector3(textSize, textSize, textSize);
            Vector3 localPosition = new Vector3(0, 3.4f, 0);
            localPosition += new Vector3(0, textAdditionalHeight, 0);
            canvas.transform.localPosition = localPosition;
        }

        public void ApplyName(string npcName)
        {
            if (string.IsNullOrWhiteSpace(npcName))
            {
                canvas.text = Localization.instance.Localize("$mpasn_" + _currentNpcType);
            }
            else
            {
                canvas.text = npcName;
            }
        }


        public void OverrideModelLocal(string newModelName)
        {
            transform.Find("NPC").gameObject.SetActive(false);
            if (!TryOverrideModel(ref newModelName, out _))
            {
                Transform t = transform.Find("NPC");
                t.gameObject.SetActive(true);
                zanim.m_animator = t.GetComponentInChildren<Animator>(true);
            }
        }
        public string LocalModelOverride;
        public void OverrideModel(long sender, string newModelName)
        {
            LocalModelOverride = newModelName;
            if (znv.IsOwner()) znv.m_zdo.Set("KGnpcModelOverride", LocalModelOverride);
            OverrideModelLocal(LocalModelOverride);
        }

        public void OverrideName(long sender, string newName)
        {
            if (znv.IsOwner()) znv.m_zdo.Set("KGnpcNameOverride", newName);
            ApplyName(newName);
            transform.Find("MPASNquest")?.gameObject.SetActive(Quests_DataTypes.Quest.IsQuestTarget(newName));
        }

        public void ChangeProfile(long sender, string profile, string dialogue)
        {
            if (znv.IsOwner())
            {
                if (string.IsNullOrWhiteSpace(profile)) profile = "default";
                znv.m_zdo.Set("KGnpcProfile", profile.ToLower());

                if (string.IsNullOrWhiteSpace(dialogue)) dialogue = "";
                znv.m_zdo.Set("KGnpcDialogue", dialogue.ToLower());
            }
        }
        

        private void CleanupInstance(GameObject instance)
        {
            Collider[] componentsInChildren = instance.GetComponentsInChildren<Collider>();
            foreach (Collider collider in componentsInChildren)
                collider.enabled = false;
        }

        private void EnableEquipedEffects(GameObject instance)
        {
            Transform find = instance.transform.Find("equiped");
            if (find) find.gameObject.SetActive(true);
        }

        private void AttachArmor(int itemHash, SkinnedMeshRenderer m_bodyModel, CapsuleCollider[] capsule,
            int variant = -1)
        {
            GameObject itemPrefab = ObjectDB.instance.GetItemPrefab(itemHash);
            if (itemPrefab == null) return;
            int childCount = itemPrefab.transform.childCount;
            for (int i = 0; i < childCount; ++i)
            {
                Transform child = itemPrefab.transform.GetChild(i);
                if (child.gameObject.name.StartsWith("attach_"))
                {
                    string text = child.gameObject.name.Substring(7);
                    GameObject gameObjectLocal;
                    if (text == "skin")
                    {
                        Transform transform1 = m_bodyModel.transform;
                        Transform parent;
                        gameObjectLocal = Instantiate(child.gameObject, transform1.position,
                            (parent = transform1.parent).rotation, parent);
                        gameObjectLocal.SetActive(true);
                        foreach (SkinnedMeshRenderer skinnedMeshRenderer in
                                 gameObjectLocal.GetComponentsInChildren<SkinnedMeshRenderer>())
                        {
                            skinnedMeshRenderer.rootBone = m_bodyModel.rootBone;
                            skinnedMeshRenderer.bones = m_bodyModel.bones;
                        }

                        foreach (Cloth cloth in gameObjectLocal.GetComponentsInChildren<Cloth>())
                            if (cloth.capsuleColliders.Length != 0)
                            {
                                List<CapsuleCollider> list2 = new List<CapsuleCollider>(capsule);
                                list2.AddRange(cloth.capsuleColliders);
                                cloth.capsuleColliders = list2.ToArray();
                            }
                            else
                            {
                                cloth.capsuleColliders = capsule;
                            }
                    }
                    else
                    {
                        Transform transformLocal = global::Utils.FindChild(pastOverrideModel!.transform, text);
                        if (transformLocal == null) return;
                        gameObjectLocal = Instantiate(child.gameObject, transformLocal, true);
                        gameObjectLocal.SetActive(true);
                        gameObjectLocal.transform.localPosition = Vector3.zero;
                        gameObjectLocal.transform.localRotation = Quaternion.identity;
                    }

                    if (variant >= 0)
                    {
                        IEquipmentVisual componentInChildren =
                            gameObjectLocal.GetComponentInChildren<IEquipmentVisual>();
                        if (componentInChildren != null) componentInChildren.Setup(variant);
                    }

                    CleanupInstance(gameObjectLocal);
                    EnableEquipedEffects(gameObjectLocal);
                }
            }
        }

        private GameObject AttachItem(GameObject itemPrefab, int variant, Transform joint, SkinnedMeshRenderer m_bodyModel,
            bool INVERT)
        {
            if (itemPrefab == null) return null;
            GameObject childGameObject = null;
            int childCount = itemPrefab.transform.childCount;
            for (int i = 0; i < childCount; ++i)
            {
                Transform child = itemPrefab.transform.GetChild(i);
                if (child.gameObject.name is "attach" or "attach_skin")
                {
                    childGameObject = child.gameObject;
                    break;
                }
            }

            if (childGameObject == null) return null;
            GameObject gameObject2 = Instantiate(childGameObject);
            gameObject2.SetActive(true);
            CleanupInstance(gameObject2);
            EnableEquipedEffects(gameObject2);
            if (childGameObject.name == "attach_skin")
            {
                gameObject2.transform.SetParent(m_bodyModel.transform.parent);
                gameObject2.transform.localPosition = Vector3.zero;
                gameObject2.transform.localRotation = Quaternion.identity;
                if (INVERT && itemPrefab.GetComponent<ItemDrop>().m_itemData.IsWeapon())
                {
                    gameObject2.transform.localPosition = new Vector3(-0.003f, 0.00002f, 0.003f);
                    gameObject2.transform.localEulerAngles = new Vector3(0f, 80f, 0f);
                }

                foreach (SkinnedMeshRenderer skinnedMeshRenderer in gameObject2
                             .GetComponentsInChildren<SkinnedMeshRenderer>())
                {
                    skinnedMeshRenderer.rootBone = m_bodyModel.rootBone;
                    skinnedMeshRenderer.bones = m_bodyModel.bones;
                }
            }
            else
            {
                gameObject2.transform.SetParent(joint);
                gameObject2.transform.localPosition = Vector3.zero;
                gameObject2.transform.localRotation = Quaternion.identity;
                if (INVERT && itemPrefab.GetComponent<ItemDrop>().m_itemData.IsWeapon())
                {
                    gameObject2.transform.localPosition = new Vector3(-0.003f, 0.00002f, 0.003f);
                    gameObject2.transform.localEulerAngles = new Vector3(0f, 80f, 0f);
                }
            }

            IEquipmentVisual componentInChildren = gameObject2.GetComponentInChildren<IEquipmentVisual>();
            if (componentInChildren != null) componentInChildren.Setup(variant);
            return gameObject2;
        }


        private GameObject EquipItemsOnModel(Transform joint, string prefab, SkinnedMeshRenderer mesh,
            bool INVERT = false)
        {
            GameObject tryFindPrefab = ObjectDB.instance.GetItemPrefab(prefab.GetStableHashCode());
            /*if (tryFindPrefab == null && ZNetScene.instance.GetPrefab(znv.m_zdo.GetString("KGnpcModelOverride")) is { } overridePrefab && overridePrefab.GetComponent<Character>() is { } character)
                tryFindPrefab = ((Humanoid)character).m_defaultItems.FirstOrDefault(x => x.name == prefab);*/

            if (joint == null || tryFindPrefab == null) return null;
            return AttachItem(tryFindPrefab, 0, joint, mesh, INVERT);
        }

        private void EquipItemsOnModel(SkinnedMeshRenderer joint, string prefab, CapsuleCollider[] capsule)
        {
            GameObject tryFindPrefab = ObjectDB.instance.GetItemPrefab(prefab.GetStableHashCode());
            /*
            if (tryFindPrefab == null && ZNetScene.instance.GetPrefab(znv.m_zdo.GetString("KGnpcModelOverride")) is { } overridePrefab && overridePrefab.GetComponent<Character>() is { } character)
                tryFindPrefab = ((Humanoid)character).m_defaultItems.FirstOrDefault(x => x.name == prefab);
                */

            if (joint == null || tryFindPrefab == null) return;
            ItemDrop item = tryFindPrefab.GetComponent<ItemDrop>();
            if (item == null) return;
            if (item.m_itemData.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Chest)
                if (item.m_itemData.m_shared.m_armorMaterial)
                {
                    joint.material.SetTexture(ChestTex,
                        item.m_itemData.m_shared.m_armorMaterial.GetTexture(ChestTex));
                    joint.material.SetTexture(ChestBumpMap,
                        item.m_itemData.m_shared.m_armorMaterial.GetTexture(ChestBumpMap));
                    joint.material.SetTexture(ChestMetal,
                        item.m_itemData.m_shared.m_armorMaterial.GetTexture(ChestMetal));
                }

            if (item.m_itemData.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Legs)
                if (item.m_itemData.m_shared.m_armorMaterial)
                {
                    joint.material.SetTexture(LegsTex,
                        item.m_itemData.m_shared.m_armorMaterial.GetTexture(LegsTex));
                    joint.material.SetTexture(LegsBumpMap,
                        item.m_itemData.m_shared.m_armorMaterial.GetTexture(LegsBumpMap));
                    joint.material.SetTexture(LegsMetal,
                        item.m_itemData.m_shared.m_armorMaterial.GetTexture(LegsMetal));
                }

            AttachArmor(prefab.GetStableHashCode(), joint, capsule);
        }


        private bool TryOverrideModel(ref string prefab, out bool isFemale, bool EquipItems = true)
        {
            bool overrideHumanoid = false;

            if (pastOverrideModel) Destroy(pastOverrideModel);
            zanim.enabled = false;
            if (prefab.Contains("@humanoid"))
            {
                overrideHumanoid = true;
                prefab = prefab.Replace("@humanoid", "");
            }

            isFemale = false;
            if (prefab == "Player_Female")
            {
                prefab = "Player";
                isFemale = true;
            }

            GameObject original = ZNetScene.instance.GetPrefab(prefab);
            if (!original) return false;
            if (original.GetComponent<Character>() == null)
            {
                Collider col = original.GetComponent<Collider>() != null
                    ? original.GetComponent<Collider>()
                    : original.GetComponentInChildren<Collider>();
                if (col == null) return false;
                pastOverrideModel = new GameObject("OverrideModel")
                {
                    layer = LayerMask.NameToLayer("character"),
                    transform =
                    {
                        parent = transform,
                        localPosition = Vector3.zero,
                        localRotation = Quaternion.identity,
                        localScale = Vector3.one,
                    }
                };


                for (int i = 0; i < original.transform.childCount; ++i)
                {
                    GameObject currentChild = original.transform.GetChild(i).gameObject;
                    Instantiate(currentChild, pastOverrideModel.transform);
                }

                foreach (Collider inChild in pastOverrideModel.GetComponentsInChildren<Collider>())
                {
                    inChild.gameObject.layer = LayerMask.NameToLayer("character");
                    if (inChild.isTrigger) inChild.enabled = false;
                }

                if (!pastOverrideModel.GetComponentInChildren<Collider>())
                {
                    Utils.CopyComponent(col, pastOverrideModel);
                }

                if (EquipItems)
                {
                    float val = znv.m_zdo.GetFloat("KGnpcScale", 1f);
                    if (val < 0.1f) val = 0.1f;
                    pastOverrideModel.transform.localScale *= val;
                }
            }
            else
            {
                Collider col = original.GetComponent<Collider>() != null
                    ? original.GetComponent<Collider>()
                    : original.GetComponentInChildren<Collider>();
                if (col == null) return false;
                original = original.GetComponentInChildren<Animator>()?.gameObject;
                if (!original) return false;
                pastOverrideModel = Instantiate(original, transform);
                LODGroup lodGroup = pastOverrideModel.GetComponent<LODGroup>();
                if (lodGroup) Destroy(lodGroup);
                foreach (ParticleSystem particleSystem in pastOverrideModel!.GetComponentsInChildren<ParticleSystem>())
                {
                    ParticleSystem.EmissionModule em = particleSystem.emission;
                    em.enabled = false;
                }

                pastOverrideModel.layer = LayerMask.NameToLayer("character");
                Utils.CopyComponent(col, pastOverrideModel);

                pastOverrideModel.transform.localPosition = Vector3.zero;
                pastOverrideModel.transform.rotation = transform.rotation;
                if (overrideHumanoid)
                {
                    pastOverrideModel.GetComponent<Animator>().runtimeAnimatorController = ZNetScene.instance
                        .GetPrefab("Player").GetComponentInChildren<Animator>().runtimeAnimatorController;
                }

                zanim.m_animator = pastOverrideModel.GetComponent<Animator>();
                zanim.enabled = true;
                pastOverrideModel.GetComponent<Animator>().SetBool(Wakeup, false);
                pastOverrideModel.GetComponent<Animator>().SetInteger(Crafting, znv.m_zdo.GetInt("KGcraftingAnimation"));
                if (EquipItems)
                {
                    VisEquipment visuals = ZNetScene.instance.GetPrefab(prefab).GetComponent<VisEquipment>();
                    if (visuals) 
                    {
                        SkinnedMeshRenderer skin = pastOverrideModel.GetComponentInChildren<SkinnedMeshRenderer>();
                        Transform leftArm =
                            global::Utils.FindChild(pastOverrideModel.transform, visuals.m_leftHand?.name);
                        Transform rightgArm =
                            global::Utils.FindChild(pastOverrideModel.transform, visuals.m_rightHand?.name);
                        Transform helmetJoint =
                            global::Utils.FindChild(pastOverrideModel.transform, visuals.m_helmet?.name);
                        Transform leftBack =
                            global::Utils.FindChild(pastOverrideModel.transform, visuals.m_backShield?.name);
                        Transform rightBack =
                            global::Utils.FindChild(pastOverrideModel.transform, visuals.m_backMelee?.name);
                        string leftItem = znv.m_zdo.GetString("KGleftItem");
                        string rightItem = znv.m_zdo.GetString("KGrightItem");
                        string helmetItem = znv.m_zdo.GetString("KGhelmetItem");
                        EquipItemsOnModel(leftArm, leftItem, skin);
                        EquipItemsOnModel(rightgArm, rightItem, skin);
                        string leftBackItem = znv.m_zdo.GetString("KGLeftItemBack");
                        string rightBackItem = znv.m_zdo.GetString("KGRightItemBack");
                        EquipItemsOnModel(leftArm, leftItem, skin);
                        EquipItemsOnModel(rightgArm, rightItem, skin);
                        EquipItemsOnModel(rightBack, rightBackItem, skin);


                        if (ZNetScene.instance.GetPrefab(leftBackItem))
                        {
                            if (ZNetScene.instance.GetPrefab(leftBackItem).GetComponent<ItemDrop>().m_itemData
                                .IsWeapon())
                            {
                                EquipItemsOnModel(rightBack, leftBackItem, skin, true);
                            }
                            else
                            {
                                EquipItemsOnModel(leftBack, leftBackItem, skin);
                            }
                        }


                        _ = EquipItemsOnModel(helmetJoint, helmetItem, skin) != null;
                        if (pastOverrideModel.GetComponent<CustomLookAt>() == null &&
                            znv.m_zdo.GetInt("KGcraftingAnimation") == 0)
                        {
                            pastOverrideModel.AddComponent<CustomLookAt>();
                        }

                        CapsuleCollider[] capsule = pastOverrideModel.GetComponentsInChildren<CapsuleCollider>().Where(x => x != pastOverrideModel.GetComponent<CapsuleCollider>()).ToArray();
                        string chestItem = znv.m_zdo.GetString("KGchestItem");
                        string legsItem = znv.m_zdo.GetString("KGlegsItem");
                        string capeItem = znv.m_zdo.GetString("KGcapeItem");
                        string beardItem = "Beard" + znv.m_zdo.GetString("KGbeardItem");
                        EquipItemsOnModel(skin, capeItem, capsule); 
                        EquipItemsOnModel(skin, chestItem, capsule);
                        EquipItemsOnModel(skin, legsItem, capsule);
                        string hairItem = "Hair" + znv.m_zdo.GetString("KGhairItem");
                        GameObject hair = EquipItemsOnModel(helmetJoint, hairItem, skin);
                        ColorUtility.TryParseHtmlString(znv.m_zdo.GetString("KGhairItemColor"), out Color c);
                        if (hair)
                        {
                            Renderer[] componentsInChildren = hair!.GetComponentsInChildren<Renderer>();
                            foreach (Renderer render in componentsInChildren)
                                render.material.SetColor(SkinColor, c);
                        }

                        GameObject beard = EquipItemsOnModel(helmetJoint, beardItem, skin);
                        ColorUtility.TryParseHtmlString(znv.m_zdo.GetString("KGbeardColor"), out Color beardColor);
                        if (beard)
                        {
                            Renderer[] componentsInChildren = beard!.GetComponentsInChildren<Renderer>();
                            foreach (Renderer render in componentsInChildren)
                                render.material.SetColor(SkinColor, beardColor);
                        }

                        if (isFemale)
                        {
                            skin.sharedMesh = visuals.m_models[1].m_mesh;
                            skin.materials[0].SetTexture(MainTex,
                                visuals.m_models[1].m_baseMaterial.GetTexture(MainTex));
                            skin.materials[0].SetTexture(SkinBumpMap,
                                visuals.m_models[1].m_baseMaterial.GetTexture(SkinBumpMap));
                        }

                        ColorUtility.TryParseHtmlString(znv.m_zdo.GetString("KGskinColor"), out Color skinColor);
                        skin.materials[0].SetColor(SkinColor, skinColor);
                    }

                    float val = znv.m_zdo.GET_NPC_NPCScale();
                    if (val < 0.1f) val = 0.1f;
                    pastOverrideModel.transform.localScale *= val;
                }
            }

            return true;
        }

        public void ChangeNpcType(long sender, int index, bool isPinned, string pinIcon)
        {
            if (znv.IsOwner())
            {
                znv.m_zdo.SET_NPC_Type((Marketplace_API.API_NPCType)index);
                znv.m_zdo.SET_NPC_IsPinned(isPinned);
                znv.m_zdo.SET_NPC_PinIcon(pinIcon);
            }

            _currentNpcType = (NPCType)index;
        }

        public void Damage(HitData hit)
        {
            if (!znv.IsValid() || !zanim.m_animator) return;
            zanim.SetTrigger("stagger");
        }

        public DestructibleType GetDestructibleType()
        {
            return DestructibleType.None;
        }
    }


    public static class NPCUI
    {
        private static Action _callback;
        private static GameObject UI = null!;
        private static ZDO _currentNPC;
        private static GameObject MAIN = null!;
        private static GameObject FASHION = null!;
        private static Transform NPCTypeButtons = null!;
        private static NPCType _currentType;
        private static InputField _npcprofile = null!;
        private static InputField _npcname = null!;
        private static InputField _npcmodel = null!;
        private static InputField _patroldata = null!;
        private static InputField _npcdialogue = null!;
        private static Toggle _npcpinned = null!;
        private static InputField _npcmapicon = null!;

        private static InputField LeftItemFashion = null!,
            RightItemFashion = null!,
            HelmetItemFashion = null!,
            ChestItemFashion = null!,
            LegsItemFashion = null!,
            RightItemHiddenFashion = null!,
            LeftItemHiddenFashion = null!,
            HairItemFashion = null!,
            SkinColorFashion = null!,
            HairItemFashionColor = null!,
            ModelScaleFashion = null!,
            CapeItemFashion = null!,
            NPCinteractAnimation = null!,
            NPCgreetAnimation = null!,
            NPCgreetText = null!,
            NPCbyeAnimation = null!,
            NPCbyeText = null!,
            NPCcraftingAnimation = null!,
            BeardItemFashion = null!,
            BeardItemFashionColor = null!,
            InteractAudioClip = null!,
            TextSize = null!,
            TextHeight = null!,
            PeriodicAnimation = null!,
            PeriodicAnimationTime = null!,
            PeriodicSound = null!,
            PeriodicSoundTime = null!;

        private static bool playSound = true;

        public static void Init()
        {
            UI = Object.Instantiate(AssetStorage.asset.LoadAsset<GameObject>("MarketplaceNPCUI"));
            Object.DontDestroyOnLoad(UI);
            UI.SetActive(false);
            NPCTypeButtons = UI.transform.Find("Canvas/MAIN/Pergament/NPCTYPE");
            MAIN = UI.transform.Find("Canvas/MAIN").gameObject;
            FASHION = UI.transform.Find("Canvas/FASHION").gameObject;
            _npcprofile = UI.transform.Find("Canvas/MAIN/Pergament/NPCPROFILE").GetComponent<InputField>();
            _npcname = UI.transform.Find("Canvas/MAIN/Pergament/NPCNAME").GetComponent<InputField>();
            _npcmodel = UI.transform.Find("Canvas/MAIN/Pergament/NPCMODEL").GetComponent<InputField>();
            _patroldata = UI.transform.Find("Canvas/MAIN/Pergament/PATROLDATA").GetComponent<InputField>();
            _npcdialogue = UI.transform.Find("Canvas/MAIN/Pergament/NPCDIALOGUE").GetComponent<InputField>();
            _npcpinned = UI.transform.Find("Canvas/MAIN/Pergament/ISPINNED").GetComponent<Toggle>();
            _npcpinned.onValueChanged.AddListener((_) =>
            {
                if (playSound) AssetStorage.AUsrc.Play();
            });
            _npcmapicon = UI.transform.Find("Canvas/MAIN/Pergament/MAPPINICON").GetComponent<InputField>();
            MAIN.SetActive(false);
            FASHION.SetActive(false);
            Localization.instance.Localize(UI.transform);

            int childCount = NPCTypeButtons.childCount;
            for (int i = 0; i < childCount; ++i)
            {
                Transform child = NPCTypeButtons.GetChild(i);
                int i1 = i;
                child.GetComponent<Button>().onClick.AddListener(() =>
                {
                    AssetStorage.AUsrc.Play();
                    _currentType = (NPCType)i1;
                    CheckColors();
                }); 
            } 

            UI.transform.Find("Canvas/MAIN/Pergament/Apply").GetComponent<Button>().onClick.AddListener(ApplyMain);

            /* FASHION */
            LeftItemFashion = UI.transform.Find("Canvas/FASHION/Pergament/LeftItemFashion").GetComponent<InputField>();
            RightItemFashion = UI.transform.Find("Canvas/FASHION/Pergament/RightItemFashion")
                .GetComponent<InputField>();
            HelmetItemFashion = UI.transform.Find("Canvas/FASHION/Pergament/HelmetItemFashion")
                .GetComponent<InputField>();
            ChestItemFashion = UI.transform.Find("Canvas/FASHION/Pergament/ChestItemFashion")
                .GetComponent<InputField>();
            LegsItemFashion = UI.transform.Find("Canvas/FASHION/Pergament/LegsItemFashion").GetComponent<InputField>();
            RightItemHiddenFashion = UI.transform.Find("Canvas/FASHION/Pergament/RightItemHiddenFashion")
                .GetComponent<InputField>();
            LeftItemHiddenFashion = UI.transform.Find("Canvas/FASHION/Pergament/LeftItemHiddenFashion")
                .GetComponent<InputField>();
            HairItemFashion = UI.transform.Find("Canvas/FASHION/Pergament/HairItemFashion").GetComponent<InputField>();
            SkinColorFashion = UI.transform.Find("Canvas/FASHION/Pergament/SkinColorFashion")
                .GetComponent<InputField>();
            HairItemFashionColor = UI.transform.Find("Canvas/FASHION/Pergament/HairItemFashionColor")
                .GetComponent<InputField>();
            ModelScaleFashion = UI.transform.Find("Canvas/FASHION/Pergament/ModelScaleFashion")
                .GetComponent<InputField>();
            CapeItemFashion = UI.transform.Find("Canvas/FASHION/Pergament/CapeItemFashion").GetComponent<InputField>();
            NPCinteractAnimation = UI.transform.Find("Canvas/FASHION/Pergament/NPCinteractAnimation")
                .GetComponent<InputField>();
            NPCgreetAnimation = UI.transform.Find("Canvas/FASHION/Pergament/NPCgreetAnimation")
                .GetComponent<InputField>();
            NPCgreetText = UI.transform.Find("Canvas/FASHION/Pergament/NPCgreetText").GetComponent<InputField>();
            NPCbyeAnimation = UI.transform.Find("Canvas/FASHION/Pergament/NPCbyeAnimation").GetComponent<InputField>();
            NPCbyeText = UI.transform.Find("Canvas/FASHION/Pergament/NPCbyeText").GetComponent<InputField>();
            NPCcraftingAnimation = UI.transform.Find("Canvas/FASHION/Pergament/NPCcraftingAnimation")
                .GetComponent<InputField>();
            BeardItemFashion = UI.transform.Find("Canvas/FASHION/Pergament/BeardItemFashion")
                .GetComponent<InputField>();
            BeardItemFashionColor = UI.transform.Find("Canvas/FASHION/Pergament/BeardItemFashionColor")
                .GetComponent<InputField>();
            InteractAudioClip = UI.transform.Find("Canvas/FASHION/Pergament/InteractAudioClip")
                .GetComponent<InputField>();
            TextSize = UI.transform.Find("Canvas/FASHION/Pergament/TextSize").GetComponent<InputField>();
            TextHeight = UI.transform.Find("Canvas/FASHION/Pergament/TextHeight").GetComponent<InputField>();
            PeriodicAnimation = UI.transform.Find("Canvas/FASHION/Pergament/PeriodicAnimation")
                .GetComponent<InputField>();
            PeriodicAnimationTime = UI.transform.Find("Canvas/FASHION/Pergament/PeriodicAnimationTime")
                .GetComponent<InputField>();
            PeriodicSound = UI.transform.Find("Canvas/FASHION/Pergament/PeriodicSound").GetComponent<InputField>();
            PeriodicSoundTime = UI.transform.Find("Canvas/FASHION/Pergament/PeriodicSoundTime")
                .GetComponent<InputField>();
            UI.transform.Find("Canvas/FASHION/Pergament/Apply").GetComponent<Button>().onClick
                .AddListener(ApplyFashion);
        }

        private static void ApplyFashion()
        {
            AssetStorage.AUsrc.Play();
            if (_currentNPC == null || !_currentNPC.IsValid())
            {
                Hide();
                return;
            }

            NPC_DataTypes.NPC_Fashion fashion = new()
            {
                LeftItem = LeftItemFashion.text,
                RightItem = RightItemFashion.text,
                HelmetItem = HelmetItemFashion.text,
                ChestItem = ChestItemFashion.text,
                LegsItem = LegsItemFashion.text,
                CapeItem = CapeItemFashion.text,
                HairItem = HairItemFashion.text,
                HairColor = HairItemFashionColor.text,
                ModelScale = ModelScaleFashion.text,
                LeftItemHidden = LeftItemHiddenFashion.text,
                RightItemHidden = RightItemHiddenFashion.text,
                InteractAnimation = NPCinteractAnimation.text,
                GreetAnimation = NPCgreetAnimation.text,
                ByeAnimation = NPCbyeAnimation.text,
                GreetText = NPCgreetText.text,
                ByeText = NPCbyeText.text,
                SkinColor = SkinColorFashion.text,
                CraftingAnimation = NPCcraftingAnimation.text,
                BeardItem = BeardItemFashion.text,
                BeardColor = BeardItemFashionColor.text,
                InteractAudioClip = InteractAudioClip.text,
                TextSize = TextSize.text,
                TextHeight = TextHeight.text,
                PeriodicAnimation = PeriodicAnimation.text,
                PeriodicAnimationTime = PeriodicAnimationTime.text,
                PeriodicSound = PeriodicSound.text,
                PeriodicSoundTime = PeriodicSoundTime.text
            };
            if (_currentNPC.HasOwner())
            {
                ZRoutedRpc.instance.InvokeRoutedRPC(ZNetView.Everybody, _currentNPC.m_uid, "KGmarket fashion", fashion);
            }
            else
            {
                _currentNPC.SET_NPC_LeftItem(fashion.LeftItem);
                _currentNPC.SET_NPC_RightItem(fashion.RightItem);
                _currentNPC.SET_NPC_HelmetItem(fashion.HelmetItem);
                _currentNPC.SET_NPC_ChestItem(fashion.ChestItem);
                _currentNPC.SET_NPC_LegsItem(fashion.LegsItem);
                _currentNPC.SET_NPC_CapeItem(fashion.CapeItem);
                _currentNPC.SET_NPC_HairItem(fashion.HairItem);
                _currentNPC.SET_NPC_HairColor(fashion.HairColor);
                _currentNPC.SET_NPC_NPCScale(fashion.ModelScale);
                _currentNPC.SET_NPC_LeftItemBack(fashion.LeftItemHidden);
                _currentNPC.SET_NPC_RightItemBack(fashion.RightItemHidden);
                _currentNPC.SET_NPC_InteractAnimation(fashion.InteractAnimation);
                _currentNPC.SET_NPC_GreetingAnimation(fashion.GreetAnimation);
                _currentNPC.SET_NPC_ByeAnimation(fashion.ByeAnimation);
                _currentNPC.SET_NPC_GreetingText(fashion.GreetText);
                _currentNPC.SET_NPC_ByeText(fashion.ByeText);
                _currentNPC.SET_NPC_SkinColor(fashion.SkinColor);
                _currentNPC.SET_NPC_CraftingAnimation(fashion.CraftingAnimation);
                _currentNPC.SET_NPC_BeardItem(fashion.BeardItem);
                _currentNPC.SET_NPC_BeardColor(fashion.BeardColor);
                _currentNPC.SET_NPC_InteractSound(fashion.InteractAudioClip);
                _currentNPC.SET_NPC_TextSize(fashion.TextSize);
                _currentNPC.SET_NPC_TextHeight(fashion.TextHeight);
                _currentNPC.SET_NPC_PeriodicAnimation(fashion.PeriodicAnimation);
                _currentNPC.SET_NPC_PeriodicAnimationTime(fashion.PeriodicAnimationTime);
                _currentNPC.SET_NPC_PeriodicSound(fashion.PeriodicSound);
                _currentNPC.SET_NPC_PeriodicSoundTime(fashion.PeriodicSoundTime);
            }

            _callback?.Invoke();
            Hide();
        }

        private static void ApplyMain()
        {
            AssetStorage.AUsrc.Play();
            if (_currentNPC == null || !_currentNPC.IsValid())
            {
                Hide();
                return;
            }

            if (_currentNPC.HasOwner())
            {
                ZRoutedRpc.instance.InvokeRoutedRPC(ZRoutedRpc.Everybody, _currentNPC.m_uid, "KGMarket changeNpcType",
                    (int)_currentType, _npcpinned.isOn, _npcmapicon.text);
                ZRoutedRpc.instance.InvokeRoutedRPC(ZRoutedRpc.Everybody, _currentNPC.m_uid, "KGmarket overridename", _npcname.text);
                ZRoutedRpc.instance.InvokeRoutedRPC(_currentNPC.GetOwner(), _currentNPC.m_uid, "KGmarket changeprofile",
                    _npcprofile.text, _npcdialogue.text);
                ZRoutedRpc.instance.InvokeRoutedRPC(ZRoutedRpc.Everybody, _currentNPC.m_uid, "KGmarket overridemodel",
                    _npcmodel.text);
                ZRoutedRpc.instance.InvokeRoutedRPC(ZRoutedRpc.Everybody, _currentNPC.m_uid, "KGmarket GetPatrolData", _patroldata.text);
            }
            else
            {
                _currentNPC.SET_NPC_Type((Marketplace_API.API_NPCType)_currentType);
                _currentNPC.SET_NPC_IsPinned(_npcpinned.isOn);
                _currentNPC.SET_NPC_Name(_npcname.text);
                _currentNPC.SET_NPC_Profile(_npcprofile.text);
                _currentNPC.SET_NPC_Dialogue(_npcdialogue.text);
                _currentNPC.SET_NPC_Model(_npcmodel.text);
                _currentNPC.SET_NPC_PatrolData(_patroldata.text);
                _currentNPC.SET_NPC_PinIcon(_npcmapicon.text);
            }

            _callback?.Invoke();
            Hide();
        }

        public static bool IsVisible()
        {
            return UI != null && UI.activeSelf;
        }

        public static void Hide()
        {
            UI.SetActive(false);
        }

        private static void CheckColors()
        {
            int childs = NPCTypeButtons.childCount;
            for (int i = 0; i < childs; ++i)
            {
                Transform child = NPCTypeButtons.GetChild(i);
                child.GetComponent<Image>().color = i == (int)_currentType ? Color.green : Color.white;
            }
        }

        public static void ShowFashion(ZDO _npc, Action callback = null)
        {
            FASHION.SetActive(true);
            MAIN.SetActive(false);
            _currentNPC = _npc;
            LeftItemFashion.text = _npc.GetString("KGleftItem");
            LeftItemHiddenFashion.text = _npc.GetString("KGLeftItemBack");
            RightItemHiddenFashion.text = _npc.GetString("KGRightItemBack");
            RightItemFashion.text = _npc.GetString("KGrightItem");
            HelmetItemFashion.text = _npc.GetString("KGhelmetItem");
            ChestItemFashion.text = _npc.GetString("KGchestItem");
            LegsItemFashion.text = _npc.GetString("KGlegsItem");
            HairItemFashion.text = _npc.GetString("KGhairItem");
            HairItemFashionColor.text = _npc.GetString("KGhairItemColor");
            CapeItemFashion.text = _npc.GetString("KGcapeItem");
            ModelScaleFashion.text = _npc.GetFloat("KGnpcScale", 1f).ToString(CultureInfo.InvariantCulture);
            NPCinteractAnimation.text = _npc.GetString("KGinteractAnimation");
            NPCgreetAnimation.text = _npc.GetString("KGgreetingAnimation");
            NPCbyeAnimation.text = _npc.GetString("KGbyeAnimation");
            NPCgreetText.text = _npc.GetString("KGgreetingText");
            NPCbyeText.text = _npc.GetString("KGbyeText");
            SkinColorFashion.text = _npc.GetString("KGskinColor");
            NPCcraftingAnimation.text = _npc.GetInt("KGcraftingAnimation").ToString();
            BeardItemFashion.text = _npc.GetString("KGbeardItem");
            BeardItemFashionColor.text = _npc.GetString("KGbeardColor");
            InteractAudioClip.text = _npc.GetString("KGinteractSound");
            TextSize.text = _npc.GetFloat("KGtextSize", 3).ToString(CultureInfo.InvariantCulture);
            TextHeight.text = _npc.GetFloat("KGtextHeight").ToString(CultureInfo.InvariantCulture);
            PeriodicAnimation.text = _npc.GetString("KGperiodicAnimation");
            PeriodicAnimationTime.text = _npc.GetFloat("KGperiodicAnimationTime")
                .ToString(CultureInfo.InvariantCulture);
            PeriodicSound.text = _npc.GetString("KGperiodicSound");
            PeriodicSoundTime.text =
                _npc.GetFloat("KGperiodicSoundTime").ToString(CultureInfo.InvariantCulture);
            UI.SetActive(true);
            _callback = callback;
        }

        public static void ShowMain(ZDO _npc, Action callback = null)
        {
            FASHION.SetActive(false);
            MAIN.SetActive(true);
            int npcType = _npc.GetInt("KGmarketNPC");
            if (npcType >= TypeNames.Length) npcType = 0;
            _currentType = (NPCType)npcType;
            _npcprofile.text = _npc.GET_NPC_Profile();
            _npcname.text = _npc.GET_NPC_Name();
            _npcmodel.text = _npc.GET_NPC_Model();
            _patroldata.text = _npc.GET_NPC_PatrolData();
            _npcdialogue.text = _npc.GET_NPC_Dialogue();
            _npcmapicon.text = _npc.GET_NPC_PinIcon();
            playSound = false;
            _npcpinned.isOn = _npc.GET_NPC_IsPinned();
            playSound = true;
            _currentNPC = _npc;
            CheckColors();
            UI.SetActive(true);
            _callback = callback;
        }
    }

    [HarmonyPatch(typeof(Player), nameof(Player.PlacePiece))]
    [ClientOnlyPatch]
    private static class DebugModeBuildCheck
    {
        [UsedImplicitly]
        private static bool Prefix(Piece piece)
        {
            if (piece.GetComponent<NPCcomponent>() && !Utils.IsDebug_Marketplace)
            {
                MessageHud.instance.ShowMessage(MessageHud.MessageType.Center,
                    $"<color=#00ff00>{Localization.instance.Localize("$mpasn_enabledebugmode")}</color>");
                return false;
            }

            return true;
        }
    }
    
    [HarmonyPatch(typeof(Player),nameof(Player.OnDeath))]
    [ClientOnlyPatch]
    private static class Player_OnDeath_Patch
    {
        private static void Prefix(Player __instance)
        {
            var myPos = __instance.transform.position;
            foreach (var npc in NPCcomponent.ALL)
            {
                float distance = Vector3.Distance(myPos, npc.transform.position); 
                if (distance > 12f) continue;
                string dialogue = npc.znv.m_zdo.GET_NPC_Dialogue();
                if (string.IsNullOrEmpty(dialogue)) continue;
                if (!Dialogues_DataTypes.ClientReadyDialogues.TryGetValue(dialogue, out var dia)) continue;
                foreach (var interaction in dia.Interactions)
                {
                    if (interaction.Type is not Dialogues_DataTypes.RawDialogue.InteractionType.OutRange) continue;
                    if (!interaction.CheckCondition(npc, out _)) continue;
                    interaction.CallCommands(npc);
                }
            }
        }
    }
}