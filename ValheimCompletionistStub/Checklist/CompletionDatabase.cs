using System.Collections.Generic;
using System.Linq;

namespace ValheimCompletionist.Checklist
{
    public static class CompletionDatabase
    {
        public static List<ChecklistEntry> ItemEntries { get; private set; } = new List<ChecklistEntry>();
        public static List<ChecklistEntry> EnemyEntries { get; private set; } = new List<ChecklistEntry>();

        /// <summary>
        /// Combined list of all checklist entries.
        /// This includes items, enemies, bosses, etc.
        /// </summary>
        public static List<ChecklistEntry> Entries { get; private set; } = new List<ChecklistEntry>();

        public static void Load()
        {
            // Load item entries from the item CSV
            ItemEntries = ChecklistCsvLoader.LoadItemsFromCsv();

            // Load enemy entries from the enemy CSV
            EnemyEntries = ChecklistCsvLoader.LoadEnemiesFromCsv();

            // Load boss entries from the boss CSV
            BossEntries = ChecklistCsvLoader.LoadBossesFromCsv();

            // Combine everything into one master list
            Entries = new List<ChecklistEntry>();
            Entries.AddRange(ItemEntries);
            Entries.AddRange(EnemyEntries);
            Entries.AddRange(BossEntries);

            if (Entries.Count == 0)
            {
                Jotunn.Logger.LogWarning("No checklist entries loaded from CSV files. Please ensure that the \nItems.csv and Enemies.csv files are in the BepInEx\nconfig folder.");
            }

            Jotunn.Logger.LogInfo($"CompletionDatabase loaded {Entries.Count} total entries.");
            Jotunn.Logger.LogInfo($"Loaded {ItemEntries.Count} item entries.");
            Jotunn.Logger.LogInfo($"Loaded {EnemyEntries.Count} enemy entries.");
            Jotunn.Logger.LogInfo($"Loaded {BossEntries.Count} boss entries.");
        }

        public static IEnumerable<ChecklistEntry> GetAll()
        {
            return Entries;
        }

        public static IEnumerable<ChecklistEntry> GetItems()
        {
            return ItemEntries;
        }

        public static IEnumerable<ChecklistEntry> GetEnemies()
        {
            return EnemyEntries;
        }

        public static IEnumerable<ChecklistEntry> GetByBiome(Biome biome)
        {
            return Entries.Where(entry => entry.Biome == biome);
        }

        public static IEnumerable<ChecklistEntry> GetByCategory(ChecklistCategory category)
        {
            return Entries.Where(entry => entry.Category == category);
        }

        public static IEnumerable<ChecklistEntry> GetByBiomeAndCategory(Biome biome, ChecklistCategory category)
        {
            return Entries.Where(entry =>
                entry.Biome == biome &&
                entry.Category == category);
        }

        public static ChecklistEntry GetById(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return null;
            }

            return Entries.FirstOrDefault(entry => entry.Id == id);
        }

        public static ChecklistEntry GetByPrefabName(string prefabName)
        {
            if (string.IsNullOrWhiteSpace(prefabName))
            {
                return null;
            }

            return Entries.FirstOrDefault(entry => entry.PrefabName == prefabName);
        }

        public static ChecklistEntry GetByPrefabName(string prefabName, CompletionType completionType)
        {
            if (string.IsNullOrWhiteSpace(prefabName))
            {
                return null;
            }

            return Entries.FirstOrDefault(entry =>
                entry.PrefabName == prefabName &&
                entry.CompletionType == completionType);
        }

        public static ChecklistEntry GetByGlobalKey(string globalKey)
        {
            if (string.IsNullOrWhiteSpace(globalKey))
            {
                return null;
            }

            return Entries.FirstOrDefault(entry => entry.GlobalKey == globalKey);
        }

        public static bool ContainsId(string id)
        {
            return GetById(id) != null;
        }

        public static int CountByBiome(Biome biome)
        {
            return Entries.Count(entry => entry.Biome == biome);
        }

        public static int CountByCategory(ChecklistCategory category)
        {
            return Entries.Count(entry => entry.Category == category);
        }

        public static int CountItems()
        {
            return ItemEntries.Count;
        }

        public static int CountEnemies()
        {
            return EnemyEntries.Count;
        }
    }
}