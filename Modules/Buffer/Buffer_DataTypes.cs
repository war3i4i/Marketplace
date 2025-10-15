using Marketplace.ExternalLoads;

namespace Marketplace.Modules.Buffer;

public static class Buffer_DataTypes
{
    internal static readonly CustomSyncedValue<Dictionary<string, string>> SyncedBufferProfiles =
        new(Marketplace.configSync, "bufferProfiles", new Dictionary<string, string>());

    internal static readonly CustomSyncedValue<List<BufferBuffData>> SyncedBufferBuffs =
        new(Marketplace.configSync, "bufferBuffs", new List<BufferBuffData>());

    public static readonly Dictionary<string, List<BufferBuffData>> ClientSideBufferProfiles = new();

    public static readonly WhatToModify[] BufferModifyList = (WhatToModify[])Enum.GetValues(typeof(WhatToModify));

    [Flags]
    public enum WhatToModify
    {
        None = 0,
        ModifyAttack = 1 << 0,
        ModifyHealthRegen = 1 << 1,
        ModifyStaminaRegen = 1 << 2,
        ModifyRaiseSkills = 1 << 3,
        ModifySpeed = 1 << 4,
        ModifyNoise = 1 << 5,
        ModifyMaxCarryWeight = 1 << 6,
        ModifyStealth = 1 << 7,
        RunStaminaDrain = 1 << 8,
        DamageReduction = 1 << 9
    }

    public class BufferBuffData : ISerializableParameter
    {
        public string UniqueName;
        public string Name;
        public int Duration;

        public Dictionary<WhatToModify, float> ModifyList = new()
        {
            { WhatToModify.ModifyAttack, 1f },
            { WhatToModify.ModifyHealthRegen, 1f },
            { WhatToModify.ModifyStaminaRegen, 1f },
            { WhatToModify.ModifyRaiseSkills, 1f },
            { WhatToModify.ModifySpeed, 1f },
            { WhatToModify.ModifyNoise, 1f },
            { WhatToModify.ModifyMaxCarryWeight, 0f },
            { WhatToModify.ModifyStealth, 1f },
            { WhatToModify.RunStaminaDrain, 1f },
            { WhatToModify.DamageReduction, 0f }
        };

        public string NeededPrefab;
        public int NeededPrefabCount;
        public WhatToModify Flags;
        public string SpritePrefab;
        public string StartEffectPrefab;
        public string BuffGroup;

        public void Serialize(ref ZPackage pkg)
        {
            pkg.Write(UniqueName ?? "");
            pkg.Write(Name ?? "");
            pkg.Write(Duration);
            pkg.Write(ModifyList[WhatToModify.ModifyAttack]);
            pkg.Write(ModifyList[WhatToModify.ModifyHealthRegen]);
            pkg.Write(ModifyList[WhatToModify.ModifyStaminaRegen]);
            pkg.Write(ModifyList[WhatToModify.ModifyRaiseSkills]);
            pkg.Write(ModifyList[WhatToModify.ModifySpeed]);
            pkg.Write(ModifyList[WhatToModify.ModifyNoise]);
            pkg.Write(ModifyList[WhatToModify.ModifyMaxCarryWeight]);
            pkg.Write(ModifyList[WhatToModify.ModifyStealth]);
            pkg.Write(ModifyList[WhatToModify.RunStaminaDrain]);
            pkg.Write(ModifyList[WhatToModify.DamageReduction]);
            pkg.Write(NeededPrefab ?? "");
            pkg.Write(NeededPrefabCount);
            pkg.Write((int)Flags);
            pkg.Write(SpritePrefab ?? "");
            pkg.Write(StartEffectPrefab ?? "");
            pkg.Write(BuffGroup ?? "");
        }

        public void Deserialize(ref ZPackage pkg)
        {
            UniqueName = pkg.ReadString();
            Name = pkg.ReadString();
            Duration = pkg.ReadInt();
            ModifyList[WhatToModify.ModifyAttack] = pkg.ReadSingle();
            ModifyList[WhatToModify.ModifyHealthRegen] = pkg.ReadSingle();
            ModifyList[WhatToModify.ModifyStaminaRegen] = pkg.ReadSingle();
            ModifyList[WhatToModify.ModifyRaiseSkills] = pkg.ReadSingle();
            ModifyList[WhatToModify.ModifySpeed] = pkg.ReadSingle();
            ModifyList[WhatToModify.ModifyNoise] = pkg.ReadSingle();
            ModifyList[WhatToModify.ModifyMaxCarryWeight] = pkg.ReadSingle();
            ModifyList[WhatToModify.ModifyStealth] = pkg.ReadSingle();
            ModifyList[WhatToModify.RunStaminaDrain] = pkg.ReadSingle();
            ModifyList[WhatToModify.DamageReduction] = pkg.ReadSingle();
            NeededPrefab = pkg.ReadString();
            NeededPrefabCount = pkg.ReadInt();
            Flags = (WhatToModify)pkg.ReadInt();
            SpritePrefab = pkg.ReadString();
            StartEffectPrefab = pkg.ReadString();
            BuffGroup = pkg.ReadString();
        }

        private BufferMain Main = null!;

        public BufferMain GetMain() => Main;
        public string GetItemName() => NeededItemName;
        private string NeededItemName = null!;
        private Sprite Icon = null!;
        private Sprite NeededItemIcon = null!;
        private GameObject StartEffect;

        public bool? IsValid;

        public void Init()
        {
            if (IsValid.HasValue) return;
            IsValid = false;
            GameObject obj = ZNetScene.instance.GetPrefab(SpritePrefab);
            GameObject neededItemPrefab = ZNetScene.instance.GetPrefab(NeededPrefab);
            if (!neededItemPrefab)
                return;

            NeededItemName = neededItemPrefab.GetComponent<ItemDrop>().m_itemData.m_shared.m_name;
            Icon = AssetStorage.PlaceholderGamblerIcon;
            if (obj)
            {
                if (obj.GetComponent<Character>())
                {
                    PhotoManager.__instance.MakeSprite(obj);
                    Icon = PhotoManager.__instance.GetSprite(obj.name, AssetStorage.PlaceholderMonsterIcon,
                        1);
                }
                else if (obj.GetComponent<ItemDrop>())
                {
                    Icon = obj.GetComponent<ItemDrop>().m_itemData.GetIcon();
                }
            }
            else
            {
                if (AssetStorage.GlobalCachedSprites.TryGetValue(SpritePrefab!, out Sprite sprite))
                {
                    Icon = sprite;
                }
            }


            NeededItemIcon = neededItemPrefab.GetComponent<ItemDrop>().m_itemData.GetIcon();

            StartEffect = ZNetScene.instance.GetPrefab(StartEffectPrefab);

            ObjectDB odb = ObjectDB.instance;
            if (!odb.m_StatusEffects.Find(se => se.name == UniqueName))
            {
                Main = ScriptableObject.CreateInstance<BufferMain>();
                Main.name = UniqueName;
                Main.Data = this;
                Main.m_icon = Icon;
                Main.m_name = Name;
                Main.m_ttl = Duration;
                Main.m_tooltip = Buffer_UI.GetFlagsText(this);
                if (StartEffect)
                    Main.m_startEffects = new EffectList
                    {
                        m_effectPrefabs = new[]
                        {
                            new EffectList.EffectData()
                            {
                                m_attach = true, m_enabled = true, m_inheritParentRotation = true,
                                m_inheritParentScale = true,
                                m_prefab = StartEffect, m_randomRotation = false, m_scale = true
                            }
                        }
                    };
                odb.m_StatusEffects.Add(Main);
                IsValid = true;
            }
        }

        public Sprite GetIcon() => Icon;
        public Sprite GetIconNeeded() => NeededItemIcon;

        public bool CanTake()
        {
            return Player.m_localPlayer && !string.IsNullOrEmpty(BuffGroup) && !Player.m_localPlayer.GetSEMan()
                .m_statusEffects
                .Any(s => s is BufferMain main && main.Data.BuffGroup == BuffGroup) && EnoughMaterials();
        }

        private bool EnoughMaterials()
        {
            if (!Player.m_localPlayer) return false;
            return Player.m_localPlayer.m_inventory.CountItems(NeededItemName) >= NeededPrefabCount;
        }
    }

    public class BufferMain : StatusEffect
    {
        public BufferBuffData Data = null!;

        public override void ModifyAttack(Skills.SkillType skill, ref HitData hitData)
        {
            hitData.ApplyModifier(Data.ModifyList[WhatToModify.ModifyAttack]);
        }

        public override void ModifyHealthRegen(ref float regenMultiplier)
        {
            regenMultiplier *= Data.ModifyList[WhatToModify.ModifyHealthRegen];
        }

        public override void ModifyStaminaRegen(ref float staminaRegen)
        {
            staminaRegen *= Data.ModifyList[WhatToModify.ModifyStaminaRegen];
        }

        public override void ModifyRaiseSkill(Skills.SkillType skill, ref float value)
        {
            value *= Data.ModifyList[WhatToModify.ModifyRaiseSkills];
        }
        
        public override void ModifySpeed(float baseSpeed, ref float speed, Character character, Vector3 dir)
        {
            speed *= Data.ModifyList[WhatToModify.ModifySpeed];
        }

        public override void ModifyNoise(float baseNoise, ref float noise)
        {
            noise *= Data.ModifyList[WhatToModify.ModifyNoise];
        }

        public override void ModifyStealth(float baseStealth, ref float stealth)
        {
            stealth *= Data.ModifyList[WhatToModify.ModifyStealth];
        }

        public override void ModifyMaxCarryWeight(float baseLimit, ref float limit)  
        {
            limit += Data.ModifyList[WhatToModify.ModifyMaxCarryWeight];
        } 

        public override void ModifyRunStaminaDrain(float baseDrain, ref float drain, Vector3 dir)
        {
            drain *= Data.ModifyList[WhatToModify.RunStaminaDrain];
        }

        public override void ModifyJumpStaminaUsage(float baseStaminaUse, ref float staminaUse)
        {
            staminaUse *= Data.ModifyList[WhatToModify.RunStaminaDrain];
        }

        public override void OnDamaged(HitData hit, Character attacker)
        {
            float mod = Mathf.Clamp01(1f - Data.ModifyList[WhatToModify.DamageReduction]);
            hit.ApplyModifier(mod);
        }
    }
}