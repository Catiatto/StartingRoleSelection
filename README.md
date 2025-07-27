# StartingRoleSelection (1.1.0)
Plugin for the "SCP: Secret Laboratory" game, that allows players to choose a starting role.

## Features
- Players can choose the starting role for themselves or other players (requires RA access), depending on permission.
- Player can choose a role either by typing role full name, short name or byte.
- Certain roles can be blacklisted from choosing.
- Slot limit per team can be set.
- Selecting roles can be toggled on and off. 
- No more than half people on the server in any moment can have the starting role selected.
- Players with SCP opt-out can't have chosen any SCP role.

## Installation
Place *StartingRoleSelection* dll in "...\AppData\Roaming\SCP Secret Laboratory\LabAPI-Beta\plugins\global OR port_number".

## Config
|Name|Type|Default value|Description|
|---|---|---|---|
|debug|bool|false|Should debug be enabled?|
|blacklisted_roles|List\<RoleTypeId>|- Scp079|Blacklisted starting roles that cannot be chosen by players without permission.|
|slot_limit|Dictionary\<Team, int>|ClassD: 4<br/>Scientists: 3<br/>FoundationForces: 2<br/>SCPs: 2|Slot limit per team.|

## Translation
The translation file is in the same folder as the config file and allows you to customize e.g:
- messages show to players
- command name, aliases, descripton and responses

*IMPORTANT:* If you translate command names and/or aliases (except subcommands), make sure not to duplicate them.

## Available roles
Any role can be selected by their byte. Short names are numbers for SCP roles (f.e. 096, 173) and nicknames for human classes (dboy, nerd, guard). 

## Remote Admin Commands
### startingroleselection
Parent command for selecting starting role. Subcommands:
- list - Print a list of all players with selected staring role.
- select - Select player's starting role. Use \"None\" if you want to remove a previously chosen role. Usage: [PlayerID] [Role Name or ID]
- toggle - Enable or disable choosing roles for players without the permission. Usage: on/off

## Client Console Commands
- selectself - Select your starting role. Use \"None\" if you want to remove your previously chosen role. Usage: Role Name or ID

## Permissions
- srs.blacklisted - allows a command sender to select a blacklisted role
- srs.list - allows a command sender to print a list of players with selected roles
- srs.select.all - allows a command sender to choose the starting role for anyone
- srs.select.self - allows a command sender to choose the starting role for themselves
- srs.toggle - allows a command sender to toggle selecting roles on/off without being affected themselves
