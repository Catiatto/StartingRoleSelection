using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ComponentModel;
using PlayerRoles;

namespace StartingRoleSelection
{
    public class Config
    {
        [Description("Should debug be enabled?")]
        public bool Debug { get; set; } = false;

        [Description("Blacklisted starting roles that cannot be selected by players without permission.")]
        public List<RoleTypeId> BlacklistedRoles { get; set; } = new List<RoleTypeId>
        {
            RoleTypeId.Scp079
        };

        [Description("How many players at once can have selected role from a certain team, regardless of player count?")]
        public Dictionary<Team, int> SlotLimit { get; set; } = new()
        {
            { Team.ClassD, 4},
            { Team.Scientists, 3},
            { Team.FoundationForces, 2},
            { Team.SCPs, 2}
        };
    }
}
