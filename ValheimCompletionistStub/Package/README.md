# ValheimCompletionist

**ValheimCompletionist** is a Valheim mod that adds an in-game completion checklist for tracking discovered items by biome. It is intended for players who want a clearer way to measure progression toward fully completing each biome.

Upon building for release, this `README.md` is copied into the `Package` folder for Thunderstore packaging. Remember to also edit `manifest.json` and supply your own mod icon.

## Installation

### Thunderstore / r2modman

1. Install the mod through Thunderstore or r2modman.
2. Make sure the required dependencies are installed:

   * `denikson-BepInExPack_Valheim`
   * `ValheimModding-Jotunn`
3. Launch Valheim through your mod manager.

### Manual installation

1. Install BepInEx for Valheim.
2. Install Jotunn.
3. Download the latest release of `ValheimCompletionist`.
4. Place `ValheimCompletionist.dll` into:

```text
Valheim/BepInEx/plugins/
```

5. Launch Valheim.

## Features

* Adds an in-game completion checklist.
* Tracks completion progress by biome.
* Tracks discovered checklist entries such as items and resources.
* Stores checklist progress per character.
* New characters start with their own separate completion progress.
* Displays biome completion percentages.
* Biomes turn green when they reach 100% completion.
* When all biomes are complete, completed biome sections are shown in gold.
* Uses a CSV-based checklist database for easier editing and expansion.

## Changelog

### 0.1.0

* Initial release.
* Added biome-based completion tracking.
* Added per-character progress storage.
* Added checklist entry database.
* Added completion percentage display.
* Added visual color changes for completed biomes.
* Added gold completion state when all biomes reach 100%.

## Known issues

* Some checklist entries may be missing, incomplete, or assigned to the wrong biome.
* Some items may not be tracked if they are not included in the checklist database.
* Completion data depends on the current character save being loaded correctly.
* Multiplayer behavior has not been fully tested.
* Checklist balancing and entry categorization are still subject to change.

## GitHub

You can find the GitHub repository at:

```text
PASTE_GITHUB_LINK_HERE
```
