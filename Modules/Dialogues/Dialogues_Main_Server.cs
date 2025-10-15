using Marketplace.Paths;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace Marketplace.Modules.Dialogues;

[UsedImplicitly]
[Market_Autoload(Market_Autoload.Type.Server, Market_Autoload.Priority.Normal, "OnInit",
    new[] { "DI", "DSCD" },
    new[] { "OnDialoguesChange", "OnCustomSpawnDataChange" })]
public class Dialogues_Main_Server
{
    [UsedImplicitly]
    private static void OnInit()
    {
        ReadDialoguesData();
        ReadCustomSpawnData();
    }

    private enum InputType
    {
        Text,
        Transition,
        Command,
        RandomCommand,
        Icon,
        Condition,
        AlwaysVisible,
        Color,
        RandomTransition,
        OverrideError
    }

    private static void ProcessDialogueProfiles(string fPath, IReadOnlyList<string> profiles)
    {
        Dialogues_DataTypes.RawDialogue dialogue = null;
        List<Dialogues_DataTypes.RawDialogue.RawPlayerOption> interactions = null;
        for (int i = 0; i < profiles.Count; ++i)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(profiles[i]) || profiles[i].StartsWith("#")) continue;
                if (profiles[i].StartsWith("["))
                {
                    if (dialogue != null)
                    {
                        dialogue.Interactions = interactions!.ToArray();
                        dialogue = null;
                    }

                    string[] splitProfile = profiles[i].Replace("[", "").Replace("]", "").Replace(" ", "").Split('=');
                    dialogue = new Dialogues_DataTypes.RawDialogue
                    {
                        UID = splitProfile[0].ToLower(),
                        BG_Image = splitProfile.Length > 1 ? splitProfile[1] : null
                    };
                    interactions = new List<Dialogues_DataTypes.RawDialogue.RawPlayerOption>();
                }
                else
                {
                    if (dialogue == null) continue;
                    if (dialogue.Text == null)
                    {
                        dialogue.Text = profiles[i].Replace(@"\n", "\n");
                    }
                    else
                    {
                        Dialogues_DataTypes.RawDialogue.RawPlayerOption interaction = new Dialogues_DataTypes.RawDialogue.RawPlayerOption();
                        List<KeyValuePair<int, string>> randomCommands = [];
                        string line = profiles[i];
                        if (line.StartsWith("@interaction"))
                        {
                            interaction.Type = Dialogues_DataTypes.RawDialogue.InteractionType.Interaction;
                            line = line.Replace("@interaction", "").Trim();
                        }
                        else if (line.StartsWith("@inrange"))
                        {
                            interaction.Type = Dialogues_DataTypes.RawDialogue.InteractionType.InRange;
                            line = line.Replace("@inrange", "").Trim();
                        }
                        else if (line.StartsWith("@outrange"))
                        {
                            interaction.Type = Dialogues_DataTypes.RawDialogue.InteractionType.OutRange;
                            line = line.Replace("@outrange", "").Trim();
                        }
                        List<string> commands = new List<string>();
                        List<string> conditions = new List<string>();
                        string[] split = line.Split('|');
                        foreach (string s in split)
                        {
                            string[] enumCheck = s.Split(new[] { ':' }, 2);
                            if (enumCheck.Length != 2) continue;
                            if (!Enum.TryParse(enumCheck[0], true, out InputType type)) continue;
                            switch (type)
                            {
                                case InputType.Text:
                                    interaction.Text = enumCheck[1].Trim().Replace(@"\n", "\n");
                                    break;
                                case InputType.Transition:
                                    interaction.NextUID = enumCheck[1].Replace(" ", "").ToLower();
                                    break;
                                case InputType.RandomTransition:
                                    interaction.RandomTransition = enumCheck[1].Replace(" ", "").ToLower().Split(',');
                                    break;
                                case InputType.Command:
                                    commands.Add(enumCheck[1].ReplaceSpacesOutsideQuotes());
                                    break;
                                case InputType.RandomCommand:
                                    string[] randomCommandSplit = enumCheck[1].Split([','], 2);
                                    if (randomCommandSplit.Length != 2) continue;
                                    if (int.TryParse(randomCommandSplit[0].Trim(), out int weight)) randomCommands.Add(new(weight, randomCommandSplit[1].ReplaceSpacesOutsideQuotes()));
                                    break;
                                case InputType.Condition:
                                    conditions.Add(enumCheck[1].ReplaceSpacesOutsideQuotes());
                                    break;
                                case InputType.Icon:
                                    interaction.Icon = enumCheck[1].Replace(" ", "");
                                    break;
                                case InputType.AlwaysVisible:
                                    interaction.AlwaysVisible = bool.Parse(enumCheck[1].Replace(" ", ""));
                                    break;
                                case InputType.OverrideError:
                                    interaction.OverrideError = enumCheck[1].Trim();
                                    break;
                                case InputType.Color:
                                    string[] colorSplit = enumCheck[1].Split(',');
                                    if (colorSplit.Length != 3) continue;
                                    interaction.Color = new Color32(byte.Parse(colorSplit[0]), byte.Parse(colorSplit[1]), byte.Parse(colorSplit[2]), 255);
                                    break;
                            }
                        } 

                        interaction.Commands = commands.ToArray();
                        interaction.Conditions = conditions.ToArray();
                        interaction.RandomCommands = randomCommands;
                        interactions!.Add(interaction);
                    }
                }
            }
            catch (Exception ex)
            {
                Utils.print($"Error reading {fPath} (line {i + 1}):\n" + ex);
                break;
            }
        }

        if (dialogue != null)
        {
            dialogue.Interactions = interactions.ToArray()!;
            Dialogues_DataTypes.SyncedDialoguesData.Value.Add(dialogue);
        }
    }

    private static void ReadDialoguesData()
    {
        Dialogues_DataTypes.SyncedDialoguesData.Value.Clear();
        string folder = Market_Paths.DialoguesFolder;
        string[] files = Directory.GetFiles(folder, "*.cfg", SearchOption.AllDirectories);
        foreach (string file in files)
        {
            IReadOnlyList<string> profiles = File.ReadAllLines(file).ToList();
            ProcessDialogueProfiles(file, profiles);
        }

        Dialogues_DataTypes.SyncedDialoguesData.Update();
    }

    private static void ReadCustomSpawnData()
    {
        string folder = Market_Paths.DialoguesCustomSpawnDataFolder;
        string[] files = Directory.GetFiles(folder, "*.yml", SearchOption.AllDirectories);
        var deserializer = new DeserializerBuilder().Build();
        Dictionary<string, Dialogues_DataTypes.CustomSpawnZDO> result = [];
        foreach (string file in files)
        {
            try
            {
                string content = File.ReadAllText(file);
                Dialogues_DataTypes.CustomSpawnZDO data = deserializer.Deserialize<Dialogues_DataTypes.CustomSpawnZDO>(content);
                string fName = Path.GetFileNameWithoutExtension(file).ToLower();
                result[fName] = data;
            }
            catch (Exception ex)
            {
                Utils.print($"Error reading {file}:\n" + ex);
                return;
            }
        }

        Dialogues_DataTypes.CustomSpawnZDOData_Synced.Value = result;
    }

    [UsedImplicitly]
    private static void OnDialoguesChange()
    {
        ReadDialoguesData();
        Utils.print("NpcDialogues changed, sending options to peers");
    }

    [UsedImplicitly]
    private static void OnCustomSpawnDataChange()
    {
        ReadCustomSpawnData();
        Utils.print("NpcDialogues CustomSpawnData changed, sending options to peers");
    }
}