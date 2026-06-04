using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Jotunn;

namespace ValheimCompletionist.Checklist
{
    public static class ChecklistCsvLoader
    {
        public static List<ChecklistEntry> LoadFromCsv(string fileName)
        {
            var entries = new List<ChecklistEntry>();

            string pluginFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string csvPath = Path.Combine(pluginFolder, "data", fileName);

            Logger.LogInfo($"Trying to load checklist CSV: {csvPath}");

            if (!File.Exists(csvPath))
            {
                Logger.LogWarning($"Checklist CSV not found: {csvPath}");
                return entries;
            }

            string[] lines = File.ReadAllLines(csvPath);

            Logger.LogInfo($"{fileName} line count: {lines.Length}");

            if (lines.Length <= 1)
            {
                Logger.LogWarning($"{fileName} has no data rows.");
                return entries;
            }

            Logger.LogInfo($"{fileName} header: {lines[0]}");

            for (int i = 1; i < lines.Length; i++)
            {
                string line = lines[i];

                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                string[] columns = line.Split(',');

                if (columns.Length < 5)
                {
                    Logger.LogWarning(
                        $"Skipping row {i + 1} in {fileName}: expected at least 5 columns, found {columns.Length}. Row: {line}"
                    );
                    continue;
                }

                // CSV header:
                // id,displayName,biome,category,completionType,prefabName,globalKey
                string id = columns[0].Trim();
                string displayName = columns[1].Trim();
                string biomeText = columns[2].Trim();
                string categoryText = columns[3].Trim();
                string completionTypeText = columns[4].Trim();
                string prefabName = columns.Length > 5 ? columns[5].Trim() : null;
                string globalKey = columns.Length > 6 ? columns[6].Trim() : null;

                if (string.IsNullOrWhiteSpace(id))
                {
                    Logger.LogWarning($"Skipping row {i + 1} in {fileName}: missing id.");
                    continue;
                }

                if (string.IsNullOrWhiteSpace(displayName))
                {
                    Logger.LogWarning($"Skipping row {i + 1} in {fileName}: missing displayName.");
                    continue;
                }

                if (!Enum.TryParse(biomeText, true, out Biome biome))
                {
                    Logger.LogWarning($"Skipping row {i + 1} in {fileName}: invalid Biome '{biomeText}'.");
                    continue;
                }

                if (!Enum.TryParse(categoryText, true, out ChecklistCategory category))
                {
                    Logger.LogWarning($"Skipping row {i + 1} in {fileName}: invalid Category '{categoryText}'.");
                    continue;
                }

                if (!Enum.TryParse(completionTypeText, true, out CompletionType completionType))
                {
                    Logger.LogWarning($"Skipping row {i + 1} in {fileName}: invalid CompletionType '{completionTypeText}'.");
                    continue;
                }

                ChecklistEntry entry = new ChecklistEntry(
                    id,
                    displayName,
                    biome,
                    category,
                    completionType,
                    prefabName,
                    globalKey
                );

                entries.Add(entry);
            }

            Logger.LogInfo($"Loaded {entries.Count} entries from {fileName}.");

            return entries;
        }
    }
}