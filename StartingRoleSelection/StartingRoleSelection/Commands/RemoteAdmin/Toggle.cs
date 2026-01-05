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
    public class Toggle : ICommand, IUsageProvider
    {
        public Toggle(string command, string description, string[] aliases)
        {
            Command = command ?? _command;
            Description = description;
            Aliases = aliases;
            Usage = new[] { "on/off" };
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
            if (!sender.HasPermissions("srs.toggle"))
            {
                response = Translation.NoPermission;
                Log.Debug($"Player {sender.LogName} doesn't have permission to use this command.", Config.Debug);
                return false;
            }
            if (arguments.IsEmpty() || arguments.At(0).ToLower() != "on" && arguments.At(0).ToLower() != "off")
            {
                response = $"{Translation.Usage}: {Usage[0]}.";
                Log.Debug($"Player {sender.LogName} didn't provide any arguments.", Config.Debug);
                return false;
            }
            Toggled = arguments.At(0).ToLower() == "on";
            response = Toggled ? Translation.ToggleSuccessOn : Translation.ToggleSuccessOff;
            Log.Debug($"Selecting roles was toggled {arguments.At(0)} by {sender.LogName}.", Config.Debug);
            return true;
        }

        internal static bool Toggled = true;

        internal const string _command = "toggle";
        internal const string _description = "Enable or disable choosing roles for players without the permission.";
        internal static readonly string[] _aliases = new[] { "t" };

        public string Command { get; }
        public string Description { get; }
        public string[] Aliases { get; }
        public string[] Usage { get; }
        private Config Config => MainClass.Instance.pluginConfig;
        private Translation Translation => MainClass.Instance.pluginTranslation;
    }
}
