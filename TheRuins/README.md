Allows using .vbuild and .blueprint files as ruined generated structures.

## Installation
Copy the contents of `plugins` to your `Valheim/BepInEx/plugins` directory or use your favourite mod manager.


## Github
To get issues addressed, you must submit them directly to github.

- **[Bug Report](https://github.com/heinermann/Valheim_mods/issues/new?assignees=&labels=bug%2C+TheRuins&template=-theruins--bug-report.md&title=)**
- **[Feature Request](https://github.com/heinermann/Valheim_mods/issues/new?assignees=&labels=enhancement%2C+TheRuins&template=-theruins--feature-request.md&title=)**
- **[Build Submissions](https://github.com/heinermann/Valheim_mods/issues/new?assignees=heinermann&labels=build+permission%2C+TheRuins&template=-theruins--build-submission.md&title=)**
- **[Source Code](https://github.com/heinermann/Valheim_mods/tree/main/TheRuins)**

*Important: While the main code repository is licensed under GPL, the included vbuild and blueprint files are **not** licensed under the GPL and permission is **not** granted to distribute them outside of The Ruins mod, including in project forks.*


## Usage
You can include your custom structures by pasting the `.vbuild` or `.blueprint` file in `Valheim/BepInEx/config/TheRuins/BIOME_NAME/`. You can have any number of directories before or after `BIOME_NAME`, as long as `BIOME_NAME` is one of the following:

- meadows
- swamp
- mountain
- blackforest
- plains
- ashlands
- deepnorth
- ocean
- mistlands

Note: It uses the lowercased name of the biome in the `Heightmap.Biome` enum, so it should also work for mods that add new biomes.

You can create `.vbuild` files with **BuildShare** ([Nexus](https://www.nexusmods.com/valheim/mods/5)), and `.blueprint` files with **PlanBuild** ([Nexus](https://www.nexusmods.com/valheim/mods/1125), [Thunderstore](https://valheim.thunderstore.io/package/MathiasDecrock/PlanBuild/)).


## Submit your Builds to be Distributed with The Ruins
You can submit your builds to be distributed with this mod by default, if you so choose. There will be a relatively low (subjective) quality bar for submissions (for example it must be at least as decent as the vanilla structures).

You can give explicit permission and link/upload your .vbuild, .blueprint, or world files using [this issue template](https://github.com/heinermann/Valheim_mods/issues/new?assignees=heinermann&labels=build+permission%2C+TheRuins&template=-theruins--build-submission.md&title=). If submitting a world file, include the coordinates of the build using the `pos` [console command](https://valheim.fandom.com/wiki/Console_Commands), or including a screenshot of an annotated map.


## How Ruining Works
1. Some pieces are replaced with alternatives for balancing, or with treasure.
    - Torches -> Their unlimited counterparts (regular and green, i.e. as seen in vanilla swamp crypts).
    - Sign -> Uneditable sign without text.
    - Bed -> Unusable bed (i.e. as seen in vanilla meadow ruined buildings).
    - Horizontal Item Stands -> Random pickable treasure (lower value in Meadow and Black Forest), or random pickable food (if near a cooking station).
    - Chests -> Random biome treasure chest.
2. Some pieces are removed based on a few rules.
    - If the piece can drop materials that substantially alter the vanilla game's progression in any way (i.e. can't get metal ingots).
    - If the piece is blacklisted. Blacklisted pieces are any of the holiday pieces and wood torches.
    - If the piece has a function. Pieces with any of the following functions are removed:
        - Beds
        - Crafting Stations
        - Portals
        - Wards
        - Beehives
        - Smelters
        - Ships
        - Carts
3. TODO Foliage
4. Random natural beehives are added to some roof ridge pieces in meadows locations.
5. TODO mobs
6. The build gets "ruined" in the following ways:
    - All fuel is removed from fireplace pieces.
    - Beehive spawn chances are tweaked.
    - Pieces have a random chance of removal based on the materials they are made with (i.e. pieces made with fine wood or nails are less likely to appear than pieces made with only wood).
    - Pieces that are higher off the ground have a greater chance of removal.
7. Treasure probabilities get distributed based on the size of the build. Larger builds have a higher probability of more treasures. This applies to:
    - Chests
    - Pickables
    - Item Stands
    - Armour Stands
    - Wood/Log/Stone piles
8. Terrain flattening and smoothing starts at the y=0 level of the saved blueprint/vbuild. It will lower terrain locally based on nearby pieces' heights. It's still a work in progress.
9. Random damage is applied to every piece.
10. Additional things happen when an instance is first discovered in the world:
    - Doors are put into random states.
    - Item stands are assigned random painted wood shields or biome specific trophies.
    - Attempts to "settle" the structural integrity of larger builds are made. This is still a work in progress.


## Known Issues
- When some locations spawn, several pieces start exploding. This is because the settling algorithm is incomplete.
- Sometimes there is a visible hump of land surrounding a build. This is an edge case in the poorly implemented terrain leveling algorithm I wrote.

If your issue is not listed, [submit a new issue here](https://github.com/heinermann/Valheim_mods/issues/new?assignees=&labels=bug%2C+TheRuins&template=-theruins--bug-report.md&title=).
