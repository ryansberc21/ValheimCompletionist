# ValheimCompletionist

**ValheimCompletionist** is a Valheim mod that adds an in-game completion checklist for tracking progress toward 100% game completion. The mod tracks checklist entries by biome and displays completion percentages so players can see what areas still need work.

This project is built using **BepInEx**, **Jötunn**, and the Valheim modding framework.

## Features

* In-game completion checklist
* Biome-based completion tracking
* Per-character completion progress
* Completion percentage display for each biome
* Completed biomes turn green at 100%
* All-biome completion state displays completed biome sections in gold
* CSV-based checklist database for easier editing and expansion
* Support for item/resource checklist entries

## Current Version

```text
0.1.0
```

## Requirements

This mod requires:

* Valheim
* BepInExPack Valheim
* Jötunn

Thunderstore dependencies:

```json
"dependencies": [
  "denikson-BepInExPack_Valheim-5.4.2333",
  "ValheimModding-Jotunn-2.26.1"
]
```

## Installation

### Recommended: Thunderstore / r2modman

1. Install the mod through Thunderstore or r2modman.
2. Make sure the required dependencies are installed.
3. Launch Valheim through your mod manager.

### Manual installation

1. Install BepInEx for Valheim.
2. Install Jötunn.
3. Download the latest release of ValheimCompletionist.
4. Place the compiled DLL into:

```text
Valheim/BepInEx/plugins/
```

5. Launch Valheim.

## Building from Source

This project was created from the Jötunn mod template.

To build locally:

1. Clone the repository.
2. Open `ValheimCompletionist.sln` in Visual Studio.
3. Make sure your Valheim install path is configured correctly for the Jötunn build scripts.
4. Build the project in either `Debug` or `Release`.

The build scripts may copy the compiled plugin into your Valheim `BepInEx/plugins` folder for testing, depending on your local configuration.

## Project Structure

```text
ValheimCompletionist/
├─ ValheimCompletionist/
│  ├─ Plugin.cs
│  ├─ CompletionDatabase.cs
│  ├─ checklist_entries.csv
│  └─ other source files
├─ ValheimCompletionistUnity/
├─ scripts/
├─ Package/
│  ├─ manifest.json
│  ├─ README.md
│  └─ icon.png
├─ README.md
├─ LICENSE
└─ ValheimCompletionist.sln
```

## Checklist Database

The mod uses a CSV-based checklist database to define completion entries. This makes it easier to add, remove, or adjust entries without hardcoding every checklist item directly into the plugin logic.

Some checklist entries may still need verification for biome assignment, item category, or tracking behavior.

## Changelog

### 0.1.0

* Initial release
* Added biome-based completion tracking
* Added per-character progress storage
* Added checklist entry database
* Added completion percentage display
* Added visual state for fully completed biomes
* Added gold visual state when all biomes are complete

## Known Issues

* Some checklist entries may be missing or incorrectly categorized.
* Some items may not be tracked if they are not included in the checklist database.
* Multiplayer behavior has not been fully tested.
* Completion data depends on the correct character save being loaded.
* Biome/item balancing may change in future versions.

## Roadmap

Potential future improvements:

* More complete checklist database
* Better UI polish
* Improved filtering or sorting
* More item categories
* Multiplayer testing and fixes
* Config options for checklist behavior
* Optional tracking for bosses, trophies, crafting, food, building pieces, and other progression goals

## Credits

Created by [realberch](https://github.com/ryansberc21).

Built using:

* [BepInEx](https://github.com/BepInEx/BepInEx)
* [Jötunn, the Valheim Library](https://github.com/Valheim-Modding/Jotunn)

## License

This project is licensed under the MIT-0 License. See the `LICENSE` file for details.
