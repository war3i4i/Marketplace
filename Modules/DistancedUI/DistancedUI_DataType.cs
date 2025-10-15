namespace Marketplace.Modules.DistancedUI;

public static class DistancedUI_DataType
{
    internal static readonly CustomSyncedValue<DistancedUIData> SyncedDistancedUIData =
        new(Marketplace.configSync, "distancedUIData", new DistancedUIData());

    public class DistancedUIData : ISerializableParameter
    {
        public bool Enabled;
        public bool MarketplaceEnabled;
        public bool MailEnabled;
        public bool CanTakeMailAttachments;
        public List<string> TraderProfiles = new();
        public List<string> TeleporterProfiles = new();
        public List<string> GamblerProfiles = new();
        public List<string> BufferProfiles = new();
        public List<string> QuestProfiles = new();
        public List<string> InfoProfiles = new();
        public List<string> BankerProfiles = new();
        public List<string> TransmogrificationProfiles = new();
        public List<string> DialogueProfiles = new();

        public void Serialize(ref ZPackage pkg)
        {
            pkg.Write(Enabled);
            pkg.Write(MarketplaceEnabled);
            pkg.Write(MailEnabled);
            pkg.Write(CanTakeMailAttachments);
            
            pkg.Write(TraderProfiles.Count);
            foreach (string profile in TraderProfiles)
            {
                pkg.Write(profile ?? "");
            }

            pkg.Write(TeleporterProfiles.Count);
            foreach (string profile in TeleporterProfiles)
            {
                pkg.Write(profile ?? "");
            }

            pkg.Write(GamblerProfiles.Count);
            foreach (string profile in GamblerProfiles)
            {
                pkg.Write(profile ?? "");
            }

            pkg.Write(BufferProfiles.Count);
            foreach (string profile in BufferProfiles)
            {
                pkg.Write(profile ?? "");
            }

            pkg.Write(QuestProfiles.Count);
            foreach (string profile in QuestProfiles)
            {
                pkg.Write(profile ?? "");
            }

            pkg.Write(InfoProfiles.Count);
            foreach (string profile in InfoProfiles)
            {
                pkg.Write(profile ?? "");
            }

            pkg.Write(BankerProfiles.Count);
            foreach (string profile in BankerProfiles)
            {
                pkg.Write(profile ?? "");
            }

            pkg.Write(TransmogrificationProfiles.Count);
            foreach (string profile in TransmogrificationProfiles)
            {
                pkg.Write(profile ?? "");
            }
            
            pkg.Write(DialogueProfiles.Count);
            foreach (string profile in DialogueProfiles)
            {
                pkg.Write(profile ?? "");
            }
        }

        public void Deserialize(ref ZPackage pkg)
        {
            Enabled = pkg.ReadBool();
            MarketplaceEnabled = pkg.ReadBool();
            MailEnabled = pkg.ReadBool();
            CanTakeMailAttachments = pkg.ReadBool();

            int count = pkg.ReadInt();
            for (int i = 0; i < count; ++i)
            {
                TraderProfiles.Add(pkg.ReadString());
            }

            count = pkg.ReadInt();
            for (int i = 0; i < count; ++i)
            {
                TeleporterProfiles.Add(pkg.ReadString());
            }

            count = pkg.ReadInt();
            for (int i = 0; i < count; ++i)
            {
                GamblerProfiles.Add(pkg.ReadString());
            }

            count = pkg.ReadInt();
            for (int i = 0; i < count; ++i)
            {
                BufferProfiles.Add(pkg.ReadString());
            }

            count = pkg.ReadInt();
            for (int i = 0; i < count; ++i)
            {
                QuestProfiles.Add(pkg.ReadString());
            }

            count = pkg.ReadInt();
            for (int i = 0; i < count; ++i)
            {
                InfoProfiles.Add(pkg.ReadString());
            }

            count = pkg.ReadInt();
            for (int i = 0; i < count; ++i)
            {
                BankerProfiles.Add(pkg.ReadString());
            }
             
            count = pkg.ReadInt();
            for (int i = 0; i < count; ++i)
            {
                TransmogrificationProfiles.Add(pkg.ReadString());
            }
            
            count = pkg.ReadInt();
            for (int i = 0; i < count; ++i)
            {
                DialogueProfiles.Add(pkg.ReadString());
            }
        }
    }
}