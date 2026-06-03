/*
* ryansberc21 - 2026-06-03
* This class holds the data for each item in the checklist.
*/

namespace ValheimCompletionist.Checklist
{
    public class ChecklistEntry
    {
        public string Id { get; }
        public string DisplayName { get; }
        public Biome Biome { get; }
        public ChecklistCategory Category { get; }
        public CompletionType CompletionType { get; }
        public string PrefabName { get; }
        public string GlobalKey { get; }

        public ChecklistEntry(
            string id,
            string displayName,
            Biome biome,
            ChecklistCategory category,
            CompletionType completionType,
            string prefabName = null,
            string globalKey = null)
        {
            Id = id;
            DisplayName = displayName;
            Biome = biome;
            Category = category;
            CompletionType = completionType;
            PrefabName = prefabName;
            GlobalKey = globalKey;
        }
    }
}