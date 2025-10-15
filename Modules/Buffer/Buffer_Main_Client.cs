namespace Marketplace.Modules.Buffer;

[UsedImplicitly]
[Market_Autoload(Market_Autoload.Type.Client, Market_Autoload.Priority.Normal)]
public static class Buffer_Main_Client
{
    [UsedImplicitly]
    private static void OnInit()
    {
        Buffer_UI.Init();
        Marketplace.Global_Updator += Update;
        Buffer_DataTypes.SyncedBufferProfiles.ValueChanged += OnBufferUpdate;
        Buffer_DataTypes.SyncedBufferBuffs.ValueChanged += OnBufferUpdate;
    }
    
    private static void Update(float dt)
    {
        if (!Player.m_localPlayer || !Input.GetKeyDown(KeyCode.Escape)) return;
        if (Buffer_UI.IsVisible())
        {
            Buffer_UI.Hide();
            Menu.instance.OnClose();
        }
    }
    
    [HarmonyPatch(typeof(ObjectDB),nameof(ObjectDB.Awake))]
    [ClientOnlyPatch]
    private static class ObjectDB_Awake_Patch
    {
        [UsedImplicitly]
        private static void Postfix() => OnBufferUpdate();
    }
    
    private static void OnBufferUpdate()
    {
        if(!ZNetScene.instance) return;
        Buffer_DataTypes.ClientSideBufferProfiles.Clear();
        ObjectDB.instance.m_StatusEffects.RemoveAll(x => x is Buffer_DataTypes.BufferMain);
        
        try
        {
            foreach (Buffer_DataTypes.BufferBuffData buff in Buffer_DataTypes.SyncedBufferBuffs.Value)
                buff.Init();

            foreach (KeyValuePair<string, string> kvp in Buffer_DataTypes.SyncedBufferProfiles.Value)
            {
                Buffer_DataTypes.ClientSideBufferProfiles.Add(kvp.Key, new List<Buffer_DataTypes.BufferBuffData>());
                foreach (string split in kvp.Value.Split(','))
                {
                    if (string.IsNullOrEmpty(split)) continue;
                    if (Buffer_DataTypes.SyncedBufferBuffs.Value.Find(d => d.UniqueName == split) is { IsValid: true } find)
                        Buffer_DataTypes.ClientSideBufferProfiles[kvp.Key].Add(find);
                }
            }

            Buffer_UI.Reload();
        }
        catch (Exception ex)
        {
            Utils.print($"Error on trying to Init Buff:\n{ex}", ConsoleColor.Red);
        }
    }
}