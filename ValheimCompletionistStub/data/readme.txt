ValheimCompletionist/data

This folder contains the item and enemy data for the mod. Upon installation, this folder will be copied to the BepInEx config folder.

items.csv - Contains the item data for the mod.
enemies.csv - Contains the enemy data for the mod.

IMPORTANT:
- BOTH files must be present in the config folder for the mod to work.
- The files are comma-separated, with a header row.
- Items are identified by their prefab name, which is the name of the object in the game.
- Enemies are identified by their name token, which is the name of the object in the game.

In case of missing or incomplete data in the CSV files, the CSV files can be manually adjusted.

To add a new item or enemy to the checklist:
1. Locate the prefab name or name token in the game's files.
2. Add the item or enemy to the appropriate CSV file with all of the required fields.
3. Save the file.
4. Reload the mod ( /reload command or restart the game).
5. Verify the new item or enemy appears in the checklist.
