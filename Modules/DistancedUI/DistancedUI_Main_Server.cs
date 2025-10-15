using BepInEx.Configuration;
using Marketplace.Paths;

namespace Marketplace.Modules.DistancedUI;

[UsedImplicitly]
[Market_Autoload(Market_Autoload.Type.Server, Market_Autoload.Priority.Normal, "OnInit", new[] { "DistancedUI.cfg" },
    new[] { "OnConfigChange" })]
public static class DistanceUI_Main_Server
{
    private static ConfigFile DistancedUI_Config = null!;
    private static ConfigEntry<bool> CanUseMarketplace_DistancedUI = null!;
    private static ConfigEntry<bool> CanUseMail_DistancedUI = null!;
    private static ConfigEntry<bool> CanTakeMailAttachments_DistancedUI = null!;
    private static ConfigEntry<bool> Enabled = null!;
    private static ConfigEntry<string> TraderProfiles_DistancedUI = null!;
    private static ConfigEntry<string> BankerProfiles_DistancedUI = null!;
    private static ConfigEntry<string> TeleporterProfiles_DistancedUI = null!;
    private static ConfigEntry<string> GamblerProfiles_DistancedUI = null!;
    private static ConfigEntry<string> BufferProfiles_DistancedUI = null!;
    private static ConfigEntry<string> QuestProfiles_DistancedUI = null!;
    private static ConfigEntry<string> InfoProfiles_DistancedUI = null!;
    private static ConfigEntry<string> TransmogrificationProfiles_DistancedUI = null!;
    private static ConfigEntry<string> Dialogues_DistancedUI = null!;

    [UsedImplicitly]
    private static void OnInit()
    {
        DistancedUI_Config = new ConfigFile(Market_Paths.DistancedUIConfig, true);
        Enabled = DistancedUI_Config.Bind("DistancedUI", "Enabled", false, "Enable or disable dystem");
        CanUseMarketplace_DistancedUI = DistancedUI_Config.Bind("DistancedUI", "CanUseMarketplace", true, "Can use the marketplace?");
        CanUseMail_DistancedUI = DistancedUI_Config.Bind("DistancedUI", "CanUseMail", true, "Can use the mail system?");
        CanTakeMailAttachments_DistancedUI = DistancedUI_Config.Bind("DistancedUI", "CanTakeMailAttachments", false, "Can take mail attachments?");
        TraderProfiles_DistancedUI = DistancedUI_Config.Bind("DistancedUI", "TraderProfiles", "Profiles here with ,", "Trader profiles");
        TeleporterProfiles_DistancedUI = DistancedUI_Config.Bind("DistancedUI", "TeleporterProfiles", "Profiles here with ,", "Teleporter profiles");
        GamblerProfiles_DistancedUI = DistancedUI_Config.Bind("DistancedUI", "GamblerProfiles", "Profiles here with ,", "Gambler profiles");
        BufferProfiles_DistancedUI = DistancedUI_Config.Bind("DistancedUI", "BufferProfiles", "Profiles here with ,", "Buffer profiles");
        BankerProfiles_DistancedUI = DistancedUI_Config.Bind("DistancedUI", "BankerProfiles", "Profiles here with ,", "Banker profiles");
        QuestProfiles_DistancedUI = DistancedUI_Config.Bind("DistancedUI", "QuestProfiles", "Profiles here with ,", "Quest profiles");
        InfoProfiles_DistancedUI = DistancedUI_Config.Bind("DistancedUI", "InfoProfiles", "Profiles here with ,", "Info profiles");
        TransmogrificationProfiles_DistancedUI = DistancedUI_Config.Bind("DistancedUI", "TransmogrificationProfiles", "Profiles here with ,", "Transmogrification profiles");
        Dialogues_DistancedUI = DistancedUI_Config.Bind("DistancedUI", "Dialogues", "Profiles here with ,", "Dialogues profiles");

        ReadDistancedUIData();
    }

    private static void ReadDistancedUIData()
    {
        DistancedUI_DataType.SyncedDistancedUIData.Value = new DistancedUI_DataType.DistancedUIData()
        {
            Enabled = Enabled.Value,
            MarketplaceEnabled = CanUseMarketplace_DistancedUI.Value,
            MailEnabled = CanUseMail_DistancedUI.Value,
            CanTakeMailAttachments = CanTakeMailAttachments_DistancedUI.Value,
            TraderProfiles = TraderProfiles_DistancedUI.Value.Split(',').Select(d => d.ToLower().Trim()).ToList(),
            BankerProfiles = BankerProfiles_DistancedUI.Value.Split(',').Select(d => d.ToLower().Trim()).ToList(),
            TeleporterProfiles = TeleporterProfiles_DistancedUI.Value.Split(',').Select(d => d.ToLower().Trim()).ToList(),
            GamblerProfiles = GamblerProfiles_DistancedUI.Value.Split(',').Select(d => d.ToLower().Trim()).ToList(),
            BufferProfiles = BufferProfiles_DistancedUI.Value.Split(',').Select(d => d.ToLower().Trim()).ToList(),
            QuestProfiles = QuestProfiles_DistancedUI.Value.Split(',').Select(d => d.ToLower().Trim()).ToList(),
            InfoProfiles = InfoProfiles_DistancedUI.Value.Split(',').Select(d => d.ToLower().Trim()).ToList(),
            TransmogrificationProfiles = TransmogrificationProfiles_DistancedUI.Value.Split(',').Select(d => d.ToLower().Trim()).ToList(),
            DialogueProfiles = Dialogues_DistancedUI.Value.Split(',').Select(d => d.ToLower().Trim()).ToList()
        };
    }

    [UsedImplicitly]
    private static void OnConfigChange()
    {
        Utils.DelayReloadConfig(DistancedUI_Config, ReadDistancedUIData);
    }
}