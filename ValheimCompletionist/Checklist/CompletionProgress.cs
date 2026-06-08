/*
* ryansberc21 - 2026-06-03
* This class handles per-character completion progress.
* Each Valheim character gets its own progress file.
*/

using System.Collections.Generic;
using System.IO;
using System.Linq;
using BepInEx;

namespace ValheimCompletionist.Checklist
{
    public static class CompletionProgress
    {
        private static readonly HashSet<string> CompletedEntryIds = new HashSet<string>();

        private static string currentCharacterName = null;
        private static bool isLoadedForCharacter = false;

        private static string ProgressFolderPath
        {
            get
            {
                string folderPath = Path.Combine(Paths.ConfigPath, "ValheimCompletionist", "progress");

                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                    Jotunn.Logger.LogWarning("No progress folder found. Created new folder for completion progress: " + folderPath);
                }

                return folderPath;
            }
        }

        private static string ProgressFilePath
        {
            get
            {
                string safeCharacterName = GetSafeFileName(currentCharacterName);

                return Path.Combine(ProgressFolderPath, $"{safeCharacterName}.txt");
            }
        }

        public static string CurrentCharacterName
        {
            get
            {
                return currentCharacterName;
            }
        }

        public static bool IsLoadedForCharacter
        {
            get
            {
                return isLoadedForCharacter;
            }
        }

        public static void LoadForCharacter(string characterName)
        {
            if (string.IsNullOrWhiteSpace(characterName))
            {
                Jotunn.Logger.LogWarning("Cannot load completion progress: character name is blank.");
                return;
            }

            characterName = characterName.Trim();

            if (isLoadedForCharacter && currentCharacterName == characterName)
            {
                return;
            }

            CompletedEntryIds.Clear();

            currentCharacterName = characterName;
            isLoadedForCharacter = true;

            if (!File.Exists(ProgressFilePath))
            {
                Jotunn.Logger.LogInfo($"No progress file found for character '{currentCharacterName}'. Starting fresh.");
                Save();
                return;
            }

            string[] lines = File.ReadAllLines(ProgressFilePath);

            foreach (string line in lines)
            {
                string id = line.Trim();

                if (!string.IsNullOrWhiteSpace(id))
                {
                    CompletedEntryIds.Add(id);
                }
            }

            Jotunn.Logger.LogInfo(
                $"Loaded {CompletedEntryIds.Count} completed checklist entries for character '{currentCharacterName}'."
            );
        }

        public static void Unload()
        {
            Save();

            CompletedEntryIds.Clear();
            currentCharacterName = null;
            isLoadedForCharacter = false;
        }

        public static void Save()
        {
            if (!isLoadedForCharacter || string.IsNullOrWhiteSpace(currentCharacterName))
            {
                return;
            }

            File.WriteAllLines(
                ProgressFilePath,
                CompletedEntryIds.OrderBy(id => id)
            );
        }

        public static bool IsCompleted(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return false;
            }

            return CompletedEntryIds.Contains(id);
        }

        public static void MarkCompleted(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return;
            }

            if (!isLoadedForCharacter)
            {
                Jotunn.Logger.LogWarning($"Cannot mark checklist entry complete before character progress is loaded: {id}");
                return;
            }

            bool wasAdded = CompletedEntryIds.Add(id);

            if (wasAdded)
            {
                Jotunn.Logger.LogInfo($"Checklist entry completed for '{currentCharacterName}': {id}");
                Save();
            }
        }

        public static int CountCompleted(IEnumerable<ChecklistEntry> entries)
        {
            if (entries == null)
            {
                return 0;
            }

            int count = 0;

            foreach (ChecklistEntry entry in entries)
            {
                if (entry != null && IsCompleted(entry.Id))
                {
                    count++;
                }
            }

            return count;
        }

        public static int CountCompletedIds()
        {
            return CompletedEntryIds.Count;
        }

        private static string GetSafeFileName(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return "UnknownCharacter";
            }

            string safe = value.Trim();

            foreach (char invalidChar in Path.GetInvalidFileNameChars())
            {
                safe = safe.Replace(invalidChar, '_');
            }

            return safe;
        }
    }
}