using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CommandSystem;
using Log = LabApi.Features.Console.Logger;
using NorthwoodLib.Pools;
using Utils.NonAllocLINQ;

namespace StartingRoleSelection.Commands.RemoteAdmin
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class StartingRoleParent : ParentCommand
    {
        public StartingRoleParent()
        {
            translation = Translation.AccessTranslation();
            Command = translation.StartingRoleParentCommand ?? _command;
            Description = translation.StartingRoleParentDescription;
            Aliases = translation.StartingRoleParentAliases;
            Log.Debug($"Registered {this.Command} parent command.", translation.Debug);
            this.LoadGeneratedCommands();
        }

        public sealed override void LoadGeneratedCommands()
        {
            this.RegisterCommand(new List(translation.ListCommand, translation.ListDescription, translation.ListAliases));
            this.RegisterCommand(new Select(translation.SelectCommand, translation.SelectDescription, translation.SelectAliases));
            this.RegisterCommand(new Toggle(translation.ToggleCommand, translation.ToggleDescription, translation.ToggleAliases));
            Log.Debug($"Loaded {this.AllCommands.Count()} command(s) for {this.Command} parent command.", translation.Debug);
        }

        protected override bool ExecuteParent(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (MainClass.Instance == null)
            {
                response = translation.PluginNotEnabled;
                Log.Debug("Plugin StartingRoleSelectiom is not enabled.", translation.Debug);
                return false;
            }
            StringBuilder stringBuilder = StringBuilderPool.Shared.Rent();
            stringBuilder.AppendLine($"{Description} {translation.Subcommands}:");
            foreach (ICommand command in this.AllCommands)
            {
                stringBuilder.AppendLine($"- {command.Command} | {translation.Aliases}: {(command.Aliases == null || command.Aliases.IsEmpty() ? "" : string.Join(", ", command.Aliases))} | {translation.Description}: {command.Description}");
            }
            response = StringBuilderPool.Shared.ToStringReturn(stringBuilder).TrimEnd(Array.Empty<char>());
            return true;
        }

        internal const string _command = "startingroleselection";
        internal const string _description = "Parent command for selecting starting role.";
        internal static readonly string[] _aliases = new[] { "srs" };
        private readonly Translation translation;

        public override string Command { get; }
        public override string Description { get; }
        public override string[] Aliases { get; }
    }
}
