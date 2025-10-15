using Marketplace.Paths;

namespace Marketplace.Modules.ServerInfo;

[UsedImplicitly]
[Market_Autoload(Market_Autoload.Type.Server, Market_Autoload.Priority.Normal, "OnInit",
    new[] { "SI" }, new[] { "OnServerInfoProfileChange" })]
public static class ServerInfo_Main_Server
{
    private static void OnInit()
    {
        ReadServerInfoProfiles();
    }

    private static void OnServerInfoProfileChange()
    {
        ReadServerInfoProfiles();
        Utils.print("Info Profiles Changed. Sending new info to all clients");
    }
 
    private static void ProcessServerInfoProfiles(IReadOnlyList<string> profiles)
    {
        string splitProfile = "default";
        List<ServerInfo_DataTypes.ServerInfoQueue.Info> _infoQueue = new();
        string data = "";
        for (int i = 0; i < profiles.Count; ++i)
        {
            if (profiles[i].StartsWith("#")) continue;
            if (profiles[i].StartsWith("["))
            {
                if (!string.IsNullOrEmpty(data))
                {
                    _infoQueue.Add(new()
                    {
                        Type = ServerInfo_DataTypes.ServerInfoQueue.Info.InfoType.Text,
                        Text = data.TrimEnd('\n')
                    });
                    ServerInfo_DataTypes.SyncedServerInfoData.Value[splitProfile] = new() { infoQueue = new(_infoQueue) };
                }

                splitProfile = profiles[i].Replace("[", "").Replace("]", "").Replace(" ", "").ToLower();
                _infoQueue = new();
                data = "";
            }
            else
            {
                if (profiles[i].Contains("<image="))
                {
                    string[] splitImage = profiles[i].Split(new[] { "<image=" }, StringSplitOptions.None);
                    string[] splitImage2 = splitImage[1].Split(new[] { ">" }, StringSplitOptions.None);
                    string link = splitImage2[0];
                    
                    string textBefore = splitImage[0];
                    string textAfter = splitImage2[1];
                    
                    data += textBefore;
                    _infoQueue.Add(new()
                    {
                        Type = ServerInfo_DataTypes.ServerInfoQueue.Info.InfoType.Text,
                        Text = data
                    });
                    _infoQueue.Add(new()
                    {
                        Type = ServerInfo_DataTypes.ServerInfoQueue.Info.InfoType.Image,
                        Text = link
                    });
                    data = textAfter + "\n";
                }
                else
                {
                    data += profiles[i] + "\n";
                }
            }
        }

        if (!string.IsNullOrEmpty(data))
        {
            _infoQueue.Add(new()
            {
                Type = ServerInfo_DataTypes.ServerInfoQueue.Info.InfoType.Text,
                Text = data.TrimEnd('\n')
            });
            ServerInfo_DataTypes.SyncedServerInfoData.Value[splitProfile] = new() { infoQueue = new(_infoQueue) };
            _infoQueue.Clear();
        }

    }
    
    private static void ReadServerInfoProfiles()
    {
        ServerInfo_DataTypes.SyncedServerInfoData.Value.Clear();
        string folder = Market_Paths.ServerInfoProfilesFolder;
        string[] files = Directory.GetFiles(folder, "*.cfg", SearchOption.AllDirectories);
        foreach (string file in files)
        {
            IReadOnlyList<string> profiles = File.ReadAllLines(file).ToList();
            ProcessServerInfoProfiles(profiles);
        }
        ServerInfo_DataTypes.SyncedServerInfoData.Update();
    }
}