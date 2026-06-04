/*
* ryansberc21 - 2026-06-03
* This class handles the completion of items in the checklist.
*/

using System.Collections.Generic;

namespace ValheimCompletionist.Checklist
{
    public static class ItemCompletionTracker
    {
        public static void ScanPlayerInventory()
        {
            if (Player.m_localPlayer == null)
            {
                return;
            }

            Inventory inventory = Player.m_localPlayer.GetInventory();

            if (inventory == null)
            {
                return;
            }

            List<ItemDrop.ItemData> items = inventory.GetAllItems();

            foreach (ItemDrop.ItemData item in items)
            {
                if (item == null || item.m_dropPrefab == null)
                {
                    continue;
                }

                string prefabName = Utils.GetPrefabName(item.m_dropPrefab);

                ChecklistEntry entry = CompletionDatabase.GetByPrefabName(
                    prefabName,
                    CompletionType.ItemCollected
                );

                if (entry == null)
                {
                    continue;
                }

                CompletionProgress.MarkCompleted(entry.Id);
            }
        }
    }
}