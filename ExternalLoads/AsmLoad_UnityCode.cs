namespace Marketplace.ExternalLoads;

[UsedImplicitly]
[Market_Autoload(Market_Autoload.Type.Both, Market_Autoload.Priority.Init)]
public static class AsmLoad_UnityCode
{
    [UsedImplicitly]
    public static void OnInit()
    { 
        LoadAsm("Marketplace_UnityCode");
    }
    public static void LoadAsm(string name)
    {
        Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Marketplace.Resources." + name + ".dll")!;
        byte[] buffer = new byte[stream.Length];
        // ReSharper disable once MustUseReturnValue
        stream.Read(buffer, 0, buffer.Length); 
        try
        {
            Assembly.Load(buffer);
            stream.Dispose();
        } 
        catch(Exception ex)
        {
            Utils.print($"Error loading {name} assembly\n:{ex}", ConsoleColor.Red);
        }
    }
}