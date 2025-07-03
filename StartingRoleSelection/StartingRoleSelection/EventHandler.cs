using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GameCore;
using LabApi.Events.Arguments.PlayerEvents;
using Log = LabApi.Features.Console.Logger;
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
                ev.Player.SendBroadcast(translation.OverwatchRole, 5, Broadcast.BroadcastFlags.Normal, true);
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
                player.SendBroadcast(translation.TooMuchLeft.Replace("%rolename%", roleSelectPlayers[player].ToString()), 5);
                roleSelectPlayers.Remove(player);
                Log.Debug($"Player {player.Nickname} had his staring role removed, because too many players left.", config.Debug);
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
                Dictionary<Player, RoleTypeId> finalRoles = initialRoles;
                List<Player> normalPlayers = Player.ReadyList.ToList();
                Log.Debug($"Initial player roles:\n- {string.Join("\n- ", initialRoles.Select(entry => $"{entry.Key.Nickname}: {entry.Value}"))}", config.Debug);
                roleSelectPlayers.ForEach(entry =>
                {
                    Player player = entry.Key;
                    RoleTypeId oldRole = player.Role;
                    RoleTypeId newRole = entry.Value;
                    if (oldRole == newRole || oldRole.GetTeam() == newRole.GetTeam() && !normalPlayers.Select(p => p.Role).Contains(newRole))
                    {
                        finalRoles[player] = newRole;
                        normalPlayers.Remove(player);
                        Log.Debug($"Player {player.Nickname} ({oldRole}) was granted role {newRole}.", config.Debug);
                        return;
                    }
                    IEnumerable<Player> exchangePlayers = normalPlayers.Where(p => p.Team == newRole.GetTeam());
                    if (exchangePlayers.IsEmpty())
                    {
                        player.SendBroadcast(translation.NoReplacements.Replace("%rolename%", newRole.ToString()), 5);
                        Log.Debug($"Role {newRole} couldn't be granted to player {player.Nickname} due to no players to exhange role with.", config.Debug);
                        return;
                    }
                    Player replacement = exchangePlayers.Count(p => p.Role == newRole) == 1 ? exchangePlayers.Single(p => p.Role == newRole) : exchangePlayers.ElementAt(random.Next(exchangePlayers.Count()));
                    RoleTypeId oldRoleReplacement = replacement.Role;
                    finalRoles[player] = newRole;
                    finalRoles[replacement] = oldRole;
                    normalPlayers.Remove(player);
                    Log.Debug($"Player {player.Nickname} ({oldRole}) was granted role {newRole} in exchange with player {replacement.Nickname} ({oldRoleReplacement}).", config.Debug);
                });
                finalRoles.ForEach(entry => entry.Key.SetRole(entry.Value, RoleChangeReason.RoundStart));
                roleSelectPlayers.Clear();
                Log.Debug($"Final player roles:\n- {string.Join("\n- ", finalRoles.Select(entry => $"{entry.Key.Nickname}: {entry.Value}"))}", config.Debug);
            });
        }

        internal static Dictionary<Player, RoleTypeId> roleSelectPlayers = new();
        private readonly Random random = new();

        private readonly Config config = MainClass.Instance.pluginConfig;
        private readonly Translation translation = MainClass.Instance.pluginTranslation;
    }
}
