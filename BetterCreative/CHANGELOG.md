## 1.5.0
- Update for Ashlands PTB

## 1.4.1
- Work around an issue that caused categories to disappear.
- Fix issue where the last piece on a category page could be cut off.

## 1.4.0
- Update for latest Valheim version.
- Removed ice pillar murderer (I think this has been long fixed)
- Fixed an issue that would reset devcommands in a new game

## 1.3.0
- Changed some rules for the Mistlands update.
- Added the ability to ignore max carry weight.
- Added the ability to suppress unlock messages.
- Fix MountainGraveStone01 and Rock_7 from floating.
- Fixed Fish under NPCs.
- Fixed beehives.
- tolroko_flyer is now persistent.

## 1.2.3
- Fix issues with cultivator and hoe.
- No longer modifies existing pieces.

## 1.2.2
- Remove the extended tab and the mod's attempt to appropriate other mods' gameobjects which caused BuildShare, PlanBuild, Gizmo, and others to malfunction.
- When constructing pieces, aggressively remove ZNetViews if there are multiple attached to an object.
- More aggressively delete the entire object if one is found to have multiple ZNetViews.
- Objects that would have had multiple ZNetViews by default (and would still break vanilla players) are removed from the hammer menu.
- Added option to aggressively murder all objects which ever had multiple ZNetViews.
- Added multiple pages when there are too many build pieces.

## 1.2.1
- Fixed major compatibility issues with other mods such as PlanBuild
- Fixed issues with some pieces like signs not working correctly

## 1.2.0
- Fixed some prefabs for the caves update.
- Now requires server to have the mod.
- No longer using MMHooks as a dependency.
- Can now remove pieces from inside no build zones.
- Removes ZNetViews when the "Double ZNetview" error is hit (note: this is a workaround).
- No longer creates a bunch of duplicated prefabs for the placement ghosts
- Build categories have been reorganized
- Software license updated
- Rulesets are now more permissive, so some additional prefabs are now available (birds, fish, and others)

## 1.1.0
- Piece descriptions will now include the names of the object in the user's language.
- Added undo/redo (Ctrl+Z, Ctrl+Shift+Z or Ctrl+Y by default, configurable)
- Added area delete (Del, configurable)

## 1.0.2
- Fixed issues where some vanilla objects clipped through floors.
- Fixed issues where many objects couldn't be placed on structures.
- Fixed an issue where workstation extensions couldn't be placed even when unrestricted placement was enabled.
- Added a note next to config items that may need a restart.
- Put the actual name of the objects in the piece descriptions (should show up in your native language).

## 1.0.1
- Fixed an issue where BeeHive placement ghosts could poison things in the world.
- Fixed an issue where many objects were unplaceable when Unrestricted Placement was disabled.
- Now enables all config options by default to avoid any confusion.
