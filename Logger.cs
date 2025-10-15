using Marketplace.Modules.Global_Options;
using Marketplace.Paths;

namespace Marketplace;

internal static class Market_Logger
{
    public enum LogType
    {
        Banker,
        Marketplace,
        Territory,
        Trader,
        Transmog,
    }

    internal static void Log(LogType type, string message)
    {
        if(type is LogType.Trader && !Global_Configs.EnableTraderLog) return;
        if(type is LogType.Transmog && !Global_Configs.EnableTransmogLog) return;
        string LogStr = type switch
        {
            LogType.Banker => $"[{DateTime.Now}] [Banker] " + message + "\n",
            LogType.Marketplace => $"[{DateTime.Now}] [Marketplace] " + message + "\n",
            LogType.Territory => $"[{DateTime.Now}] [Territory] " + message + "\n",
            LogType.Trader => $"[{DateTime.Now}] [Trader] " + message + "\n",
            LogType.Transmog => $"[{DateTime.Now}] [Transmog] " + message + "\n",
            _ => ""
        };
        File.AppendAllText(Market_Paths.LoggerPath, LogStr);
    }

    [MarketplaceRPC("LogOnServer_mpasn", Market_Autoload.Type.Server)]
    private static void LogOnServer(long _, int type, string message)
    {
        Log((LogType)type, message);
    }
}