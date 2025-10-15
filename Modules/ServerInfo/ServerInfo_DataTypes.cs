namespace Marketplace.Modules.ServerInfo;

public static class ServerInfo_DataTypes
{
    internal static readonly CustomSyncedValue<Dictionary<string, ServerInfoQueue>> SyncedServerInfoData =
        new(Marketplace.configSync, "infoData", new Dictionary<string, ServerInfoQueue>());


    public class ServerInfoQueue : ISerializableParameter
    {
        public List<Info> infoQueue = new();

        public class Info
        {
            public string Text;
            public InfoType Type;
            private Sprite tex;
            public enum InfoType
            {
                Text, Image
            }
            public void SetSprite(Sprite _sprite) => tex = _sprite;
            public Sprite GetSprite() => tex;
        }
        
        
        public void Serialize(ref ZPackage pkg)
        {
            pkg.Write(infoQueue.Count);
            foreach (Info info in infoQueue)
            {
                pkg.Write((int)info.Type);
                pkg.Write(info.Text ?? "");
            }
        }

        public void Deserialize(ref ZPackage pkg)
        {
            int count = pkg.ReadInt();
            for (int i = 0; i < count; ++i)
            {
                Info info = new()
                {
                    Type = (Info.InfoType)pkg.ReadInt(),
                    Text = pkg.ReadString()
                };
                infoQueue.Add(info);
            }
        }
    }
    
}