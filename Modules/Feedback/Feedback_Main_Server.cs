using System.Net;
using System.Threading.Tasks;
using Marketplace.Modules.Global_Options;

namespace Marketplace.Modules.Feedback;

public static class Feedback_Main_Server
{
    [MarketplaceRPC("KGmarket ReceiveFeedback", Market_Autoload.Type.Server)]
    private static void ReceiveFeedback(long sender, ZPackage pkg)
    {
        ZNetPeer peer = ZNet.instance.GetPeer(sender);
        if (peer == null) return;
        string PlayerInfo = pkg.ReadString() + " (" + peer.m_socket.GetHostName() + ")";
        Utils.print($"Got feedback from {PlayerInfo}");
        string Subject = pkg.ReadString();
        string Message = pkg.ReadString();
        Task.Run(() =>
        {
            string json =
                @"{""username"":""FeedbackNPC""," +
                @"""embeds"":[{""author"":{""name"":""" + "Player: " + PlayerInfo +
                @"""},""title"":""Subject"",""description"":""" + Subject +
                @""",""color"":15258703,""fields"":[{""name"":""Message"",""value"":""" + Message +
                @""",""inline"":true}]}]}";
            SendMSG(Global_Configs.WebHookLink, json);
        });
    }


    private static void SendMSG(string link, string message)
    {
        if (Uri.TryCreate(link, UriKind.Absolute, out Uri outUri)
            && (outUri.Scheme == Uri.UriSchemeHttp || outUri.Scheme == Uri.UriSchemeHttps))
        {
            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(link);
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";
            using (StreamWriter streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                streamWriter.Write(message);
            }

            httpWebRequest.GetResponseAsync();
        }
    }
}