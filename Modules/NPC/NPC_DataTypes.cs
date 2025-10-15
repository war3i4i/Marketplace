using YamlDotNet.Core;
using YamlDotNet.Serialization;

namespace Marketplace.Modules.NPC;

public static class NPC_DataTypes
{
    public struct NPC_Main
    {
        public Market_NPC.NPCType Type;
        public string NameOverride;
        public string Profile;
        public string Prefab;
        public string Dialogue;
        public string PinIcon;
        public string IMAGE;

        public string Description()
        {
            string result = $"<color=yellow>Type:</color> {Type}. ";
            if (!string.IsNullOrEmpty(NameOverride))
                result += $"<color=yellow>Name:</color> {NameOverride}. ";
            if (!string.IsNullOrEmpty(Profile))
                result += $"<color=yellow>Profile:</color> {Profile}. ";
            if (!string.IsNullOrEmpty(Prefab))
                result += $"<color=yellow>MovelOverride:</color> {Prefab}. ";
            if (!string.IsNullOrEmpty(Dialogue))
                result += $"<color=yellow>Dialogue:</color> {Dialogue}. ";
            if (!string.IsNullOrEmpty(PinIcon))
                result += $"<color=yellow>PinIcon:</color> {PinIcon}. ";
            return result;
        }
    }

    public struct NPC_Fashion : ISerializableParameter
    {
        public string LeftItem;
        public string RightItem;
        public string HelmetItem;
        public string ChestItem;
        public string LegsItem;
        public string CapeItem;
        public string HairItem; 
        public string HairColor;
        public string ModelScale;
        public string LeftItemHidden;
        public string RightItemHidden;
        public string InteractAnimation;
        public string GreetAnimation;
        public string ByeAnimation;
        public string GreetText;
        public string ByeText;
        public string SkinColor;
        public string CraftingAnimation;
        public string BeardItem;
        public string BeardColor;
        public string InteractAudioClip;
        public string TextSize;
        public string TextHeight;
        public string PeriodicAnimation;
        public string PeriodicAnimationTime;
        public string PeriodicSound;
        public string PeriodicSoundTime;

        public void Serialize(ref ZPackage pkg)
        {
            pkg.Write(LeftItem ?? "");
            pkg.Write(RightItem ?? "");
            pkg.Write(HelmetItem ?? "");
            pkg.Write(ChestItem ?? "");
            pkg.Write(LegsItem ?? "");
            pkg.Write(CapeItem ?? "");
            pkg.Write(HairItem ?? "");
            pkg.Write(HairColor ?? "");
            pkg.Write(ModelScale ?? "");
            pkg.Write(LeftItemHidden ?? "");
            pkg.Write(RightItemHidden ?? "");
            pkg.Write(InteractAnimation ?? "");
            pkg.Write(GreetAnimation ?? "");
            pkg.Write(ByeAnimation ?? "");
            pkg.Write(GreetText ?? "");
            pkg.Write(ByeText ?? "");
            pkg.Write(SkinColor ?? "");
            pkg.Write(CraftingAnimation ?? "");
            pkg.Write(BeardItem ?? "");
            pkg.Write(BeardColor ?? "");
            pkg.Write(InteractAudioClip ?? "");
            pkg.Write(TextSize ?? "");
            pkg.Write(TextHeight ?? "");
            pkg.Write(PeriodicAnimation ?? "");
            pkg.Write(PeriodicAnimationTime ?? "");
            pkg.Write(PeriodicSound ?? "");
            pkg.Write(PeriodicSoundTime ?? "");
        }

        public void Deserialize(ref ZPackage pkg)
        {
            LeftItem = pkg.ReadString();
            RightItem = pkg.ReadString();
            HelmetItem = pkg.ReadString();
            ChestItem = pkg.ReadString();
            LegsItem = pkg.ReadString();
            CapeItem = pkg.ReadString();
            HairItem = pkg.ReadString();
            HairColor = pkg.ReadString();
            ModelScale = pkg.ReadString();
            LeftItemHidden = pkg.ReadString();
            RightItemHidden = pkg.ReadString();
            InteractAnimation = pkg.ReadString();
            GreetAnimation = pkg.ReadString();
            ByeAnimation = pkg.ReadString();
            GreetText = pkg.ReadString();
            ByeText = pkg.ReadString();
            SkinColor = pkg.ReadString();
            CraftingAnimation = pkg.ReadString();
            BeardItem = pkg.ReadString();
            BeardColor = pkg.ReadString();
            InteractAudioClip = pkg.ReadString();
            TextSize = pkg.ReadString();
            TextHeight = pkg.ReadString();
            PeriodicAnimation = pkg.ReadString();
            PeriodicAnimationTime = pkg.ReadString();
            PeriodicSound = pkg.ReadString();
            PeriodicSoundTime = pkg.ReadString();
        }

        public void ApplyRandom()
        {
            LeftItem = LeftItem.RandomSplitSpace();
            RightItem = RightItem.RandomSplitSpace();
            HelmetItem = HelmetItem.RandomSplitSpace();
            ChestItem = ChestItem.RandomSplitSpace();
            LegsItem = LegsItem.RandomSplitSpace();
            CapeItem = CapeItem.RandomSplitSpace();
            HairItem = HairItem.RandomSplitSpace();
            HairColor = HairColor.RandomSplitSpace();
            LeftItemHidden = LeftItemHidden.RandomSplitSpace();
            RightItemHidden = RightItemHidden.RandomSplitSpace();
            InteractAnimation = InteractAnimation.RandomSplitSpace();
            GreetAnimation = GreetAnimation.RandomSplitSpace();
            ByeAnimation = ByeAnimation.RandomSplitSpace();
            SkinColor = SkinColor.RandomSplitSpace();
            CraftingAnimation = CraftingAnimation.RandomSplitSpace();
            BeardItem = BeardItem.RandomSplitSpace();
            BeardColor = BeardColor.RandomSplitSpace();
            PeriodicAnimation = PeriodicAnimation.RandomSplitSpace();
        }
        
    }
}