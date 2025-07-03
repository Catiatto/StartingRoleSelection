using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CommandSystem;
using Log = LabApi.Features.Console.Logger;
using RemoteAdmin;
using StartingRoleSelection.Commands.RemoteAdmin;

namespace StartingRoleSelection.Commands
{
    [CommandHandler(typeof(ClientCommandHandler))]
    public class SelectSelf : ICommand, IUsageProvider
    {
        public SelectSelf()
        {
            translation = Translation.AccessTranslation();
            Command = translation.SelectSelfCommand ?? _command;
            Description = translation.SelectSelfDescription;
            Aliases = translation.SelectSelfAliases;
            Usage = new[] { "%role% or ID" };
            Log.Debug($"Registered {this.Command} command.", translation.Debug);
        }

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            PlayerCommandSender commandsender = sender as PlayerCommandSender;
            string[] newArguments = new string[] { commandsender.PlayerId.ToString() }.Concat(arguments).ToArray();
            ParentCommand parentCommand = CommandProcessor.RemoteAdminCommandHandler.AllCommands.First(c => c is StartingRoleParent) as ParentCommand;
            ICommand command = parentCommand.AllCommands.First(c => c is Select);
            return command.Execute(new(newArguments), commandsender, out response);
        }

        internal const string _command = "selectself";
        internal const string _description = "Select your starting role. Use \"None\" if you want to remove your previously chosen role.";
        internal static readonly string[] _aliases = new[] { "sels" };
        private readonly Translation translation;

        public string Command { get; }
        public string Description { get; }
        public string[] Aliases { get; }
        public string[] Usage { get; }
    }
}
