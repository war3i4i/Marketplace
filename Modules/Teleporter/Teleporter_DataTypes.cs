namespace Marketplace.Modules.Teleporter;

public static class Teleporter_DataTypes
{
    internal static readonly CustomSyncedValue<Dictionary<string, List<TeleporterData>>> SyncedTeleporterData = 
        new(Marketplace.configSync, "teleporterData", new Dictionary<string, List<TeleporterData>>());
    
    public class TeleporterData : ISerializableParameter
    {
        public string name;
        public string sprite;
        public int x;
        public int y;
        public int z;
        public int speed;
        public void Serialize(ref ZPackage pkg)
        {
            pkg.Write(name ?? "");
            pkg.Write(sprite ?? "");
            pkg.Write(x);
            pkg.Write(y);
            pkg.Write(z);
            pkg.Write(speed);
        }
        public void Deserialize(ref ZPackage pkg)
        {
            name = pkg.ReadString();
            sprite = pkg.ReadString();
            x = pkg.ReadInt();
            y = pkg.ReadInt();
            z = pkg.ReadInt();
            speed = pkg.ReadInt();
        }
    }
    
    
    
}