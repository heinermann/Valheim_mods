An improved creative mode experience.

## Installation
1. Install dependencies - Jotunn.
2. Copy the contents of `plugins` to your `Valheim/BepInEx/plugins` directory or use your favourite mod manager.

## Github
To get issues addressed, you must submit them directly to github.

- **[Changelog](https://github.com/heinermann/Valheim_mods/blob/main/BetterCreative/CHANGELOG.md)**
- **[Bug Report](https://github.com/heinermann/Valheim_mods/issues/new?assignees=&labels=BetterCreative%2C+bug&template=-bettercreative--bug-report.md&title=)**
- **[Feature Request](https://github.com/heinermann/Valheim_mods/issues/new?assignees=&labels=BetterCreative%2C+enhancement&template=-bettercreative--feature-request.md&title=)**
- **[Source Code](https://github.com/heinermann/Valheim_mods/tree/main/BetterCreative)**

## Features

- Console access without needing the command line argument.
- Automatically set commonly used creative console commands (devcommands, god, ghost, etc.).
- Unlimited stamina.
- Allow placing most prefabs.
- No deconstruction drops to get in your way.
- No durability drain.
- No placement delay.
- No over encumberance.
- No unlock messages.
- Hide Hugin tutorials.
- Undo/Redo (Ctrl+Z and Ctrl+Shift+Z or Ctrl+Y by default, note you will need to change it to avoid conflicts).
- Area deletion (Delete key, select the object type you want to delete and move the placement ghost near the objects before pressing Delete)
- Configurable.

## Config Options

You can change the configuration options through a separate configuration mod. Note that a game restart may be required for some changes to take effect.

### Command States
See [Console Commands](https://valheim.fandom.com/wiki/Console_Commands) for context.

- **debugmode** - Enables fly mode and debug hotkeys.
- **decommands** - Enable devcommands automatically. Required for other commands to function.
- **ghost** - Prevents mobs from seeing you.
- **god** - Makes it so you don't take damage from monsters.
- **nocost** - No build cost, unlocks everything.

### Improvements

- **All Prefabs** - Allow placement of all functional prefabs.
- **Delete Range** - Range to delete objects with the delete key. This is the radius from the placement ghost's center. *Default: 5*
- **No Durability Drain** - Tools don't lose durability.
- **No Encumbered** - No effect when surpassing maximum carry weight.
- **No Piece Drops** - Don't drop materials when pieces are destroyed.
- **No Placement Delay** - No cooldowns for the hammer, cultivator, or hoe.
- **No Unlock Messages** - No popup message when unlocking a recipe.
- **Unlimited Stamina** - Can always perform stamina actions regardless of stamina amount.
- **Unrestricted Placement** - Allow unrestricted placements (no collision, campfire on wood, etc).

### Hotkeys
- **Delete** - Destroys all prefabs that match the currently selected piece in the area. *Default: Delete*
- **Delete (alt)**
- **Redo** - *Default: Ctrl+Shift+Z*
- **Redo (alt)** - *Default: Ctrl+Y*
- **Undo** - *Default: Ctrl+Z*
- **Undo (alt)**

## Known Issues
- Sometimes the icon colours are messed up, usually restarting the game fixes it.

If your issue is not listed, [submit a new issue here](https://github.com/heinermann/Valheim_mods/issues/new?assignees=&labels=BetterCreative%2C+bug&template=-bettercreative--bug-report.md&title=).
