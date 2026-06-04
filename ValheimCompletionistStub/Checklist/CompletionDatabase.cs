using System.Collections.Generic;
using System.Linq;

namespace ValheimCompletionist.Checklist
{
    public static class CompletionDatabase
    {
        public static List<ChecklistEntry> Entries { get; private set; } = new List<ChecklistEntry>();

        public static void Load()
        {
            Entries = ChecklistCsvLoader.LoadFromCsv();

            if (Entries.Count == 0)
            {
                Jotunn.Logger.LogWarning("No checklist entries loaded from CSV. Loading fallback entries instead.");
                LoadFallbackEntries();
            }

            Jotunn.Logger.LogInfo($"CompletionDatabase loaded {Entries.Count} entries.");
        }

        private static void LoadFallbackEntries()
        {
            Entries = new List<ChecklistEntry>
            {
                new ChecklistEntry(
                    id: "boss.eikthyr",
                    displayName: "Eikthyr",
                    biome: Biome.Meadows,
                    category: ChecklistCategory.Bosses,
                    completionType: CompletionType.BossDefeated,
                    prefabName: "Eikthyr",
                    globalKey: "defeated_eikthyr"
                ),

                new ChecklistEntry(
                    id: "boss.elder",
                    displayName: "The Elder",
                    biome: Biome.BlackForest,
                    category: ChecklistCategory.Bosses,
                    completionType: CompletionType.BossDefeated,
                    prefabName: "gd_king",
                    globalKey: "defeated_gdking"
                ),

                new ChecklistEntry(
                    id: "boss.bonemass",
                    displayName: "Bonemass",
                    biome: Biome.Swamp,
                    category: ChecklistCategory.Bosses,
                    completionType: CompletionType.BossDefeated,
                    prefabName: "Bonemass",
                    globalKey: "defeated_bonemass"
                ),

                new ChecklistEntry(
                    id: "boss.moder",
                    displayName: "Moder",
                    biome: Biome.Mountain,
                    category: ChecklistCategory.Bosses,
                    completionType: CompletionType.BossDefeated,
                    prefabName: "Dragon",
                    globalKey: "defeated_dragon"
                ),

                new ChecklistEntry(
                    id: "boss.yagluth",
                    displayName: "Yagluth",
                    biome: Biome.Plains,
                    category: ChecklistCategory.Bosses,
                    completionType: CompletionType.BossDefeated,
                    prefabName: "GoblinKing",
                    globalKey: "defeated_goblinking"
                ),

                new ChecklistEntry(
                    id: "boss.queen",
                    displayName: "The Queen",
                    biome: Biome.Mistlands,
                    category: ChecklistCategory.Bosses,
                    completionType: CompletionType.BossDefeated,
                    prefabName: "SeekerQueen",
                    globalKey: "defeated_queen"
                ),

                new ChecklistEntry(
                    id: "boss.fader",
                    displayName: "Fader",
                    biome: Biome.Ashlands,
                    category: ChecklistCategory.Bosses,
                    completionType: CompletionType.BossDefeated,
                    prefabName: "Fader",
                    globalKey: "defeated_fader"
                )
            };
        }

        public static IEnumerable<ChecklistEntry> GetAll()
        {
            return Entries;
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
    }
}