using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Log = LabApi.Features.Console.Logger;

using CommandSystem;
using GameCore;
using LabApi.Features.Permissions;
using LabApi.Features.Wrappers;
using PlayerRoles;
using PlayerRoles.RoleAssign;
using Utils.NonAllocLINQ;

namespace StartingRoleSelection.Commands.RemoteAdmin
{
    public class Select : ICommand, IUsageProvider
    {
        public Select(string command, string description, string[] aliases)
        {
            Command = command ?? _command;
            Description = description;
            Aliases = aliases;
            Usage = new[] { "PlayerID", "%role% or ID" };
            Log.Debug($"Registered {this.Command} subcommand.", Translation.AccessTranslation().Debug);
        }

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (MainClass.Instance == null)
            {
                response = Translation.PluginNotEnabled;
                Log.Debug($"Plugin {MainClass.Instance.Name} is not enabled.", Translation.Debug);
                return false;
            }
            if (sender == null)
            {
                response = Translation.SenderNull;
                Log.Debug("Command sender doesn't exist.", Config.Debug);
                return false;
            }
            Player commandsender = Player.Get(sender);
            if (!(sender.HasPermissions("srs.select.all")
            || sender.HasPermissions("srs.select.self") && arguments.Count > 0 && arguments.At(0) == commandsender.PlayerId.ToString()))
            {
                response = Translation.NoPermission;
                Log.Debug($"Player {sender.LogName} doesn't have permission to use this command.", Config.Debug);
                return false;
            }
            if (Round.IsRoundStarted)
            {
                response = Translation.RoundStarted;
                Log.Debug($"Player {sender.LogName} tried to use this command before round start.", Config.Debug);
                return false;
            }
            if (!(Toggle.Toggled || sender.HasPermissions("srs.toggle")))
            {
                response = Translation.ToggledOff;
                Log.Debug($"Player {sender.LogName} doesn't have the permission to use this command with toggle off.", Config.Debug);
                return false;
            }
            if (arguments.Count > 0 && arguments.Contains("help"))
            {
                response = $"{Translation.SelectHelp}:\n- " + string.Join("\n- ", spawnableRoles.Select(r => $"{((byte)r.Value)}, {r.Key} or {r.Value}"));
                return true;
            }
            if (arguments.Count < 2)
            {
                response = $"{Description} {Translation.Usage}: {this.DisplayCommandUsage()}";
                Log.Debug($"Player {sender.LogName} didn't provide enough arguments.", Config.Debug);
                return false;
            }
            if (!int.TryParse(arguments.At(0), out int id))
            {
                response = Translation.MustBeNumber;
                Log.Debug($"Player {sender.LogName} didn't provide any player ID.", Config.Debug);
                return false;
            }
            Player player = Player.Get(id);
            if (player == null)
            {
                response = Translation.NoPlayers;
                Log.Debug("Provided player(s) doesn't exist.", Config.Debug);
                return false;
            }
            if (player.IsHost)
            {
                response = Translation.DedicatedServer;
                Log.Debug($"Player {commandsender.Nickname} tried to use this command on dedicated server.", Config.Debug);
                return false;
            }
            if (player.IsOverwatchEnabled)
            {
                response = Translation.OverwatchEnabled.Replace("%playernick%", player.Nickname);
                Log.Debug($"Player {commandsender.Nickname} can't choose a role for player {player.Nickname} because they have overwatch enabled.", Config.Debug);
                return false;
            }
            if (!(spawnableRoles.TryGetValue(arguments.At(1), out RoleTypeId role) || Enum.TryParse(arguments.At(1), true, out role)))
            {
                response = Translation.InvalidRole.Replace("%rolename%", arguments.At(1));
                Log.Debug($"Player {commandsender.Nickname} provided invalid role name or ID.", Config.Debug);
                return false;
            }
            if (role == RoleTypeId.None)
            {
                if (EventHandler.roleSelectPlayers.Remove(player))
                {
                    response = Translation.SelectRemoveSuccess.Replace("%rolename%", role.ToString()).Replace("%playernick%", player.Nickname);
                    return true;
                }
                response = Translation.SelectRemoveFail.Replace("%playernick%", player.Nickname);
                return false;
            }
            if (!spawnableRoles.Values.Contains(role))
            {
                response = Translation.MustBeStarting.Replace("%rolename%", role.ToString());
                Log.Debug($"Player {commandsender.Nickname} can't select non-starting role {role}.", Config.Debug);
                return false;
            }
            if (Config.BlacklistedRoles.Contains(role) && !commandsender.HasPermissions("srs.blacklisted"))
            {
                response = Translation.BlacklistedRole;
                Log.Debug($"Player {commandsender.Nickname} can't select blacklisted role {role}.", Config.Debug);
                return false;
            }
            if (EventHandler.roleSelectPlayers.TryGetValue(player, out RoleTypeId chosenRole) && chosenRole == role)
            {
                response = Translation.AlreadySelected;
                Log.Debug($"Player {commandsender.Nickname} already selected role {role} for player {player.Nickname}.", Config.Debug);
                return false;
            }
            if (EventHandler.roleSelectPlayers.Count(p => p.Key != player) > Player.Count / 2 && !commandsender.HasPermissions("srs.override"))
            {
                response = Translation.TooManyChose.Replace("%rolename%", role.ToString());
                Log.Debug($"Player {commandsender.Nickname} can't choose a role as too many people have already chosen a role.", Config.Debug);
                return false;
            }
            Team roleTeam = role.GetTeam();
            if (roleTeam == Team.SCPs && ScpPlayerPicker.IsOptedOutOfScp(player.ReferenceHub))
            {
                response = Translation.ScpOptOut.Replace("%playernick%", player.Nickname);
                Log.Debug($"Player {commandsender.Nickname} can't choose a role {role} for player {player.Nickname} as they opted out of SCP.", Config.Debug);
                return false;
            }
            int takenSlots = EventHandler.roleSelectPlayers.Count(p => p.Key != player && p.Value.GetTeam() == roleTeam);
            if (Config.SlotLimit.TryGetValue(roleTeam, out int slotLimit) && takenSlots >= slotLimit)
            {
                response = Translation.AllSlotsTaken.Replace("%teamname%", roleTeam.ToString());
                Log.Debug($"Player {commandsender.Nickname} can't choose role {role} for player {player.Nickname} as all slots are already taken.", Config.Debug);
                return false;
            }
            string spawnQueue = ConfigFile.ServerConfig.GetString("team_respawn_queue", RoleAssigner.DefaultQueue);
            char teamEnum = char.Parse(((byte)roleTeam).ToString());
            int teamLimit = spawnQueue.Remove(Player.Count).Count(t => t == teamEnum);
            if (takenSlots >= teamLimit)
            {
                response = Translation.TeamLimitReached.Replace("%teamname%", roleTeam.ToString()).Replace("%rolename%", role.ToString());
                Log.Debug($"Player {commandsender.Nickname} can't choose a role {role} for player {player.Nickname} as the team limit has been already reached.", Config.Debug);
                return false;
            }
            if (role == RoleTypeId.Scp079 && teamLimit == 1)
            {
                response = Translation.NoAlone079;
                Log.Debug($"Player {commandsender.Nickname} can't choose SCP-079 for player {player.Nickname} as there is either not enough SCP slots.", Config.Debug);
                return false;
            }
            EventHandler.roleSelectPlayers[player] = role;
            response = Translation.SelectAddSuccess.Replace("%rolename%", role.ToString()).Replace("%playernick%", player.Nickname);
            Log.Debug($"Player {commandsender.Nickname} has chosen role {role} for player {player.Nickname}.", Config.Debug);
            return true;
        }

        public readonly Dictionary<string, RoleTypeId> spawnableRoles = new()
        {
            { "dboy", RoleTypeId.ClassD },
            { "guard", RoleTypeId.FacilityGuard },
            { "nerd", RoleTypeId.Scientist },
            { "049", RoleTypeId.Scp049 },
            { "079", RoleTypeId.Scp079 },
            { "096", RoleTypeId.Scp096 },
            { "106", RoleTypeId.Scp106 },
            { "173", RoleTypeId.Scp173 },
            { "3114", RoleTypeId.Scp3114 },
            { "939", RoleTypeId.Scp939 }
        };

        internal const string _command = "select";
        internal const string _description = "Select player's starting role. Use \"None\" if you want to remove a previously chosen role. Use \"help\" to display role names.";
        internal static readonly string[] _aliases = new[] { "s" };

        public string Command { get; }
        public string Description { get; }
        public string[] Aliases { get; }
        public string[] Usage { get; }
        private static Config Config => MainClass.Instance.pluginConfig;
        private Translation Translation => MainClass.Instance.pluginTranslation;
    }
}
