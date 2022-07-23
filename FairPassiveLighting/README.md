# Fair Passive Lighting

## Description
A passive lighting mod to complement (NOT replace) fueled lighting solutions. It works by taking any glowing item and making it brighter, inheriting its colour and intensity while increasing its light radius.

The lanterns in this mod only buff the light given by items that glow (i.e. swamp keys or dragon tears). By default it increases the range by 2.5 times (configurable). So the disadvantage is you would need many swamp keys and dragon tears to fill them up.

The light ranges provided by *some* items are listed here (note that this mod works with all attachable glowing items including modded ones, except for weapons).

- Surtling Trophy, Meads, Freeze Gland: 1 -> 2.5
- Yellow Mushroom, Ancient Seed, Surtling Core: 1.5 -> 3.75
- Fuling Totem: 2 -> 5
- Swamp Key, Dragon Tear, Dragon Egg, Wishbone: 3 -> 7.5
- Yagluth Thing: 3.76 -> 9.4
- Golem Trophy: 4 -> 10

For comparison, a standing wooden torch has a light range of 10.

### Advantages
- No fuel needed.
- Many colour options.
- Enemies don't target it.

### Disadvantages
- Less light.
- Placement restrictions.
- Doesn't scare greydwarves.
- Isn't warm.
- Requires item hunting.

## Installation (manual)
1. Install dependencies - Jotunn.
2. Copy the contents of `plugins` to your `Valheim/BepInEx/plugins` directory or use your favourite mod manager.

## Github
To get issues addressed, you must submit them directly to github.

- **[Changelog](https://github.com/heinermann/Valheim_mods/blob/main/FairPassiveLighting/CHANGELOG.md)**
- **[Source Code](https://github.com/heinermann/Valheim_mods/tree/main/FairPassiveLighting)**

## Config Options

- **Lighting Multiplier** - The value to multiply the original light range by when placed inside of the passive lamp.
