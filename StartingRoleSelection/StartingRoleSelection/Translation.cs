using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ComponentModel;
using System.IO;

using LabApi.Loader.Features.Paths;
using Serialization;
using StartingRoleSelection.Commands;
using StartingRoleSelection.Commands.RemoteAdmin;

namespace StartingRoleSelection
{
    public class Translation
    {
        [Description("MISCELLANOUS TRANSLATION. Don't translate words put between two '%'.")]
        public string OverwatchRole { get; set; } = "Your starting role was removed due to changing to Overwatch role.";
        public string NoReplacements { get; set; } = "Your role %rolename% couldn't be granted, because there is nobody to exchange roles.";
        public string TooMuchLeft { get; set; } = "Your starting role %rolename% was removed, because too many people left.";

        [Description("COMMANDS\' TRANSLATION. Don't translate words put between two '%'." +
                     "\n# Should debug be enabled for command registering?")]
        public bool Debug { get; set; } = false;

        [Description("Translation of StartingRole parent command and its subcommands. Make sure not to duplicate commands or aliases." +
                     "\n# StartingRole parent command.")]
        public string StartingRoleParentCommand { get; set; } = StartingRoleParent._command;
        public string StartingRoleParentDescription { get; set; } = StartingRoleParent._description;
        public string[] StartingRoleParentAliases { get; set; } = StartingRoleParent._aliases;

        [Description("List command.")]
        public string ListCommand { get; set; } = List._command;
        public string ListDescription { get; set; } = List._description;
        public string[] ListAliases { get; set; } = List._aliases;
        public string ListSuccess { get; set; } = "List of players with preselected roles (%count%)";

        [Description("Select command.")]
        public string SelectCommand { get; set; } = Select._command;
        public string SelectDescription { get; set; } = Select._description;
        public string[] SelectAliases { get; set; } = Select._aliases;
        public string SelectAddSuccess { get; set; } = "You have selected starting role %rolename% for %playernick%.";
        public string SelectRemoveSuccess { get; set; } = "You have removed starting role %rolename% for %playernick%.";
        public string SelectRemoveFail { get; set; } = "Player %playernick% doesn't have any starting role.";
        public string SelectHelp { get; set; } = "Spawnable roles";

        [Description("Toggle command.")]
        public string ToggleCommand { get; set; } = Toggle._command;
        public string ToggleDescription { get; set; } = Toggle._description;
        public string[] ToggleAliases { get; set; } = Toggle._aliases;
        public string ToggleSuccessOn { get; set; } = "Selecting roles is now enabled.";
        public string ToggleSuccessOff { get; set; } = "Selecting roles is now disabled.";

        [Description("Translation of SelectSelf client command.")]
        public string SelectSelfCommand { get; set; } = SelectSelf._command;
        public string SelectSelfDescription { get; set; } = SelectSelf._description;
        public string[] SelectSelfAliases { get; set; } = SelectSelf._aliases;

        [Description("Translation of the command interface.")]
        public string Aliases { get; set; } = "Aliases";
        public string Description { get; set; } = "Description";
        public string Subcommands { get; set; } = "Available subcommands";
        public string Usage { get; set; } = "Usage";

        [Description("Translation of other command responses.")]
        public string AllSlotsTaken { get; set; } = "All available slots for the team %teamname% are already taken.";
        public string AlreadySelected { get; set; } = "You have already chosen this role.";
        public string BlacklistedRole { get; set; } = "You can't choose a blacklisted role.";
        public string DedicatedServer { get; set; } = "You can't use this command on Dedicated Server.";
        public string InvalidRole { get; set; } = "You provided invalid role %rolename%.";
        public string MustBeNumber { get; set; } = "First argument must be a number.";
        public string MustBeStarting { get; set; } = "Provided role %rolename% is not a starting role.";
        public string NoAlone079 { get; set; } = "You can't choose SCP-079 as there is not enough players.";
        public string NoPermission { get; set; } = "You don't have permission to use this feature.";
        public string NoPlayers { get; set; } = "Provided player(s) doesn't exist.";
        public string OverwatchEnabled { get; set; } = "Provided player %playernick% has overwatch enabled.";
        public string PluginNotEnabled { get; set; } = "StartingRoleSelection is not enabled.";
        public string RoundStarted { get; set; } = "You can't use this feature after round start.";
        public string ScpOptOut { get; set; } = "Provided player %playernick% has opted out of SCP.";
        public string SenderNull { get; set; } = "Command sender is null.";
        public string ToggledOff { get; set; } = "Choosing roles is currently disabled.";
        public string TeamLimitReached { get; set; } = "The team limit of this role (%rolename%) has been already reached.";
        public string TooManyChose { get; set; } = "Too many people have already chosen a starting role.";

        internal static Translation AccessTranslation()
        {
            return translation ??= File.Exists(filePath) ? YamlParser.Deserializer.Deserialize<Translation>(File.ReadAllText(filePath)) : new();
        }

        private static Translation translation;

        private static readonly string filePath = Path.Combine(PathManager.Configs.FullName, "StartingRoleSelection", "translation.yml");
    }
}
