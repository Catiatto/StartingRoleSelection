using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Log = LabApi.Features.Console.Logger;

using CommandSystem;
using LabApi.Features.Permissions;

namespace StartingRoleSelection.Commands.RemoteAdmin
{
    public class List : ICommand
    {
        public List(string command, string description, string[] aliases)
        {
            Command = command ?? _command;
            Description = description;
            Aliases = aliases;
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
            if (!sender.HasPermissions("srs.list"))
            {
                response = Translation.NoPermission;
                Log.Debug($"Player {sender.LogName} doesn't have permission to use this command.", Config.Debug);
                return false;
            }
            response = $"{Translation.ListSuccess.Replace("%count%", EventHandler.roleSelectPlayers.Count().ToString())}:\n- {string.Join("\n- ", EventHandler.roleSelectPlayers.Select(entry => $"{entry.Key.Nickname}: {entry.Value}"))}";
            return true;
        }

        internal const string _command = "list";
        internal const string _description = "Print a list of all players with selected staring role.";
        internal static readonly string[] _aliases = new[] { "l" };

        public string Command { get; }
        public string Description { get; }
        public string[] Aliases { get; }
        private Config Config => MainClass.Instance.pluginConfig;
        private Translation Translation => MainClass.Instance.pluginTranslation;
    }
}
