using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Log = LabApi.Features.Console.Logger;

using GameCore;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.CustomHandlers;
using LabApi.Features.Wrappers;
using MEC;
using PlayerRoles;
using PlayerRoles.RoleAssign;
using System.Data;
using Utils.NonAllocLINQ;

namespace StartingRoleSelection
{
    public class EventHandler : CustomEventsHandler
    {
        public override void OnPlayerChangedRole(PlayerChangedRoleEventArgs ev)
        {
            if (ev.NewRole.RoleTypeId == RoleTypeId.Overwatch && roleSelectPlayers.Remove(ev.Player))
            {
                ev.Player.SendBroadcast(Translation.OverwatchRole, 5, Broadcast.BroadcastFlags.Normal, true);
            }
        }

        public override void OnPlayerLeft(PlayerLeftEventArgs ev)
        {
            if (Round.IsRoundStarted || Round.IsRoundEnded || roleSelectPlayers.IsEmpty())
            {
                return;
            }
            string spawnQueue = ConfigFile.ServerConfig.GetString("team_respawn_queue", RoleAssigner.DefaultQueue);
            foreach (Team team in Enum.GetValues(typeof(Team)))
            {
                int takenSlots = roleSelectPlayers.Count(x => x.Value.GetTeam() == team);
                char teamEnum = char.Parse(((byte)team).ToString());
                int availableRole = spawnQueue.Remove(Player.Count).Count(t => t == teamEnum);
                if (takenSlots <= availableRole)
                {
                    continue;
                }
                Player player = roleSelectPlayers.FirstOrDefault(x => x.Value == RoleTypeId.Scp079).Key;
                if (player == null || team != Team.SCPs || availableRole > 1)
                {
                    player = roleSelectPlayers.Last(x => x.Value.GetTeam() == team).Key;
                } 
                player.SendBroadcast(Translation.TooMuchLeft.Replace("%rolename%", roleSelectPlayers[player].ToString()), 5);
                roleSelectPlayers.Remove(player);
                Log.Debug($"Player {player.Nickname} had his staring role removed, because too many players left.", Config.Debug);
            }
        }

        public override void OnServerRoundStarted()
        {
            if (roleSelectPlayers.IsEmpty())
            {
                return;
            }
            Timing.CallDelayed(0.1f, delegate ()
            {
                Dictionary<Player, RoleTypeId> initialRoles = Player.ReadyList.Zip(Player.ReadyList.Select(p => p.Role), (key, value) => new {key, value})
                                                              .ToDictionary(entry => entry.key, entry => entry.value);
                Dictionary<Player, RoleTypeId> finalRoles = initialRoles.ToDictionary(entry => entry.Key, entry => entry.Value);
                List<Player> normalPlayers = Player.ReadyList.ToList();
                Log.Debug($"Initial player roles:\n- {string.Join("\n- ", initialRoles.Select(entry => $"{entry.Key.Nickname}: {entry.Value}"))}", Config.Debug);
                roleSelectPlayers.ForEach(entry =>
                {
                    Player player = entry.Key;
                    RoleTypeId oldRole = player.Role;
                    RoleTypeId newRole = entry.Value;
                    if (oldRole == newRole || oldRole.GetTeam() == newRole.GetTeam() && !normalPlayers.Select(p => p.Role).Contains(newRole))
                    {
                        finalRoles[player] = newRole;
                        normalPlayers.Remove(player);
                        Log.Debug($"Player {player.Nickname} ({oldRole}) was granted role {newRole}.", Config.Debug);
                        return;
                    }
                    List<Player> exchangePlayers = normalPlayers.Where(p => p.Team == newRole.GetTeam()).ToList();
                    if (oldRole.GetTeam() == Team.SCPs && exchangePlayers.Count() - exchangePlayers.Count(p => ScpPlayerPicker.IsOptedOutOfScp(p.ReferenceHub)) > 0)
                    {
                        int scpOptOut = exchangePlayers.RemoveAll(p => ScpPlayerPicker.IsOptedOutOfScp(p.ReferenceHub));
                        Log.Debug($"Removed {scpOptOut} players with SCP opt-out.", Config.Debug);
                    }
                    if (exchangePlayers.IsEmpty())
                    {
                        player.SendBroadcast(Translation.NoReplacements.Replace("%rolename%", newRole.ToString()), 5);
                        Log.Debug($"Role {newRole} couldn't be granted to player {player.Nickname} due to no players to exhange role with.", Config.Debug);
                        return;
                    }
                    Player replacement = exchangePlayers.Count(p => p.Role == newRole) == 1 ? exchangePlayers.Single(p => p.Role == newRole) : exchangePlayers.ElementAt(random.Next(exchangePlayers.Count()));
                    RoleTypeId oldRoleReplacement = replacement.Role;
                    finalRoles[player] = newRole;
                    finalRoles[replacement] = oldRole;
                    normalPlayers.Remove(player);
                    Log.Debug($"Player {player.Nickname} ({oldRole}) was granted role {newRole} in exchange with player {replacement.Nickname} ({oldRoleReplacement}).", Config.Debug);
                });
                finalRoles.ForEach(entry =>
                {
                    if (entry.Value != initialRoles[entry.Key])
                    {
                        entry.Key.SetRole(entry.Value, RoleChangeReason.RoundStart);
                    }
                });
                roleSelectPlayers.Clear();
                Log.Debug($"Final player roles:\n- {string.Join("\n- ", finalRoles.Select(entry => $"{entry.Key.Nickname}: {entry.Value}"))}", Config.Debug);
            });
        }

        internal static Dictionary<Player, RoleTypeId> roleSelectPlayers = new();
        private readonly Random random = new();

        private Config Config => MainClass.Instance.pluginConfig;
        private Translation Translation => MainClass.Instance.pluginTranslation;
    }
}
