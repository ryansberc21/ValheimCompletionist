/*
* ryansberc21 - 2026-06-03
* This class handles the completion progress of checklist items.
* It keeps track of completed checklist entries and saves them to a text file.
*/

using System.Collections.Generic;
using System.IO;
using BepInEx;

namespace ValheimCompletionist.Checklist
{
    public static class CompletionProgress
    {
        private static readonly HashSet<string> CompletedEntryIds = new HashSet<string>();

        private static string ProgressFilePath
        {
            get
            {
                string folderPath = Path.Combine(Paths.ConfigPath, "ValheimCompletionist");

                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }
                //Save to progress.txt
                return Path.Combine(folderPath, "progress.txt");
            }
        }

        public static void Load()
        {
            CompletedEntryIds.Clear();

            if (!File.Exists(ProgressFilePath))
            {
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

            Jotunn.Logger.LogInfo($"Loaded {CompletedEntryIds.Count} completed checklist entries.");
        }

        public static void Save()
        {
            File.WriteAllLines(ProgressFilePath, CompletedEntryIds);
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

            bool wasAdded = CompletedEntryIds.Add(id);

            if (wasAdded)
            {
                Jotunn.Logger.LogInfo($"Checklist entry completed: {id}");
                Save();
            }
        }
    }
}