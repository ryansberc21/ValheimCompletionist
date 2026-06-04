using System;
using System.Collections.Generic;
using System.IO;
using BepInEx;

namespace ValheimCompletionist.Checklist
{
    public static class ChecklistCsvLoader
    {
        public static List<ChecklistEntry> LoadFromCsv()
        {
            string folderPath = Path.Combine(Paths.ConfigPath, "ValheimCompletionist");
            string filePath = Path.Combine(folderPath, "checklist_entries.csv");

            if (!File.Exists(filePath))
            {
                Jotunn.Logger.LogWarning($"Checklist CSV not found at: {filePath}");
                return new List<ChecklistEntry>();
            }

            List<ChecklistEntry> entries = new List<ChecklistEntry>();

            string[] lines = File.ReadAllLines(filePath);

            if (lines.Length <= 1)
            {
                Jotunn.Logger.LogWarning("Checklist CSV is empty or only has a header row.");
                return entries;
            }

            string[] headers = SplitCsvLine(lines[0]);

            int prefabIndex = Array.IndexOf(headers, "PrefabName");
            int nameTokenIndex = Array.IndexOf(headers, "NameToken");
            int itemTypeIndex = Array.IndexOf(headers, "ItemType");
            int biomeIndex = Array.IndexOf(headers, "Biome");
            int categoryIndex = Array.IndexOf(headers, "ChecklistCategory");
            int includeIndex = Array.IndexOf(headers, "IncludeInChecklist");

            if (prefabIndex < 0 || nameTokenIndex < 0 || itemTypeIndex < 0 ||
                biomeIndex < 0 || categoryIndex < 0 || includeIndex < 0)
            {
                Jotunn.Logger.LogError("Checklist CSV is missing one or more required columns.");
                Jotunn.Logger.LogError("Required columns: PrefabName, NameToken, ItemType, Biome, ChecklistCategory, IncludeInChecklist");
                return entries;
            }

            for (int i = 1; i < lines.Length; i++)
            {
                string line = lines[i];

                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                string[] values = SplitCsvLine(line);

                if (values.Length <= includeIndex)
                {
                    Jotunn.Logger.LogWarning($"Skipping malformed CSV line {i + 1}: {line}");
                    continue;
                }

                string includeText = values[includeIndex].Trim();

                bool include =
                    includeText.Equals("true", StringComparison.OrdinalIgnoreCase) ||
                    includeText.Equals("yes", StringComparison.OrdinalIgnoreCase) ||
                    includeText.Equals("1");

                if (!include)
                {
                    continue;
                }

                string prefabName = values[prefabIndex].Trim();
                string nameToken = values[nameTokenIndex].Trim();
                string itemType = values[itemTypeIndex].Trim();
                string biomeText = values[biomeIndex].Trim();
                string categoryText = values[categoryIndex].Trim();

                if (!Enum.TryParse(biomeText, ignoreCase: true, out Biome biome))
                {
                    Jotunn.Logger.LogWarning($"Invalid biome '{biomeText}' on CSV line {i + 1}. Using Global.");
                    biome = Biome.Global;
                }

                if (!Enum.TryParse(categoryText, ignoreCase: true, out ChecklistCategory category))
                {
                    Jotunn.Logger.LogWarning($"Invalid category '{categoryText}' on CSV line {i + 1}. Using Misc.");
                    category = ChecklistCategory.Misc;
                }

                CompletionType completionType = GetCompletionTypeFromItemType(itemType);

                string id = CreateIdFromPrefab(prefabName, completionType);

                string displayName = CreateDisplayName(prefabName, nameToken);

                ChecklistEntry entry = new ChecklistEntry(
                    id: id,
                    displayName: displayName,
                    biome: biome,
                    category: category,
                    completionType: completionType,
                    prefabName: prefabName
                );

                entries.Add(entry);
            }

            Jotunn.Logger.LogInfo($"Loaded {entries.Count} checklist entries from CSV.");

            return entries;
        }

        private static CompletionType GetCompletionTypeFromItemType(string itemType)
        {
            if (itemType.Equals("Trophy", StringComparison.OrdinalIgnoreCase))
            {
                return CompletionType.ItemCollected;
            }

            if (itemType.Equals("Consumable", StringComparison.OrdinalIgnoreCase))
            {
                return CompletionType.ItemCollected;
            }

            if (itemType.Equals("Fish", StringComparison.OrdinalIgnoreCase))
            {
                return CompletionType.FishCaught;
            }

            return CompletionType.ItemCollected;
        }

        private static string CreateIdFromPrefab(string prefabName, CompletionType completionType)
        {
            string prefix = "item";

            if (completionType == CompletionType.FishCaught)
            {
                prefix = "fish";
            }

            string cleanPrefab = prefabName
                .Trim()
                .ToLowerInvariant()
                .Replace(" ", "_");

            return $"{prefix}.{cleanPrefab}";
        }

        private static string CreateDisplayName(string prefabName, string nameToken)
        {
            if (!string.IsNullOrWhiteSpace(nameToken) && nameToken.StartsWith("$item_"))
            {
                string cleaned = nameToken
                    .Replace("$item_", "")
                    .Replace("_", " ");

                return ToTitleCase(cleaned);
            }

            return prefabName;
        }

        private static string ToTitleCase(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return input;
            }

            string[] words = input.Split(' ');

            for (int i = 0; i < words.Length; i++)
            {
                if (words[i].Length == 0)
                {
                    continue;
                }

                words[i] = char.ToUpper(words[i][0]) + words[i].Substring(1);
            }

            return string.Join(" ", words);
        }

        private static string[] SplitCsvLine(string line)
        {
            List<string> values = new List<string>();
            bool insideQuotes = false;
            string current = "";

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];

                if (c == '"')
                {
                    if (insideQuotes && i + 1 < line.Length && line[i + 1] == '"')
                    {
                        current += '"';
                        i++;
                    }
                    else
                    {
                        insideQuotes = !insideQuotes;
                    }
                }
                else if (c == ',' && !insideQuotes)
                {
                    values.Add(current);
                    current = "";
                }
                else
                {
                    current += c;
                }
            }

            values.Add(current);

            return values.ToArray();
        }
    }
}