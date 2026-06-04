using BepInEx;
using BepInEx.Configuration;
using Jotunn.Entities;
using Jotunn.Managers;
using System.IO;
using System.Text;
using UnityEngine;
using ValheimCompletionist.Checklist;
using HarmonyLib;

namespace ValheimCompletionist
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [BepInDependency(Jotunn.Main.ModGuid)]
    internal class ValheimCompletionist : BaseUnityPlugin
    {
        private Harmony harmony;
        public const string PluginGUID = "com.ryansberc21.ValheimCompletionist";
        public const string PluginName = "ValheimCompletionist";
        public const string PluginVersion = "0.1.0";

        public static CustomLocalization Localization = LocalizationManager.Instance.GetLocalization();

        private ConfigEntry<bool> enableDebugExport;

        private float itemScanTimer = 0f;
        private float bossScanTimer = 0f;

        private const float ItemScanInterval = 5f;
        private const float BossScanInterval = 5f;

        private string loadedCharacterName = null;

        private void Awake()
        {
            Jotunn.Logger.LogInfo($"{PluginName} by realberch loading...");

            BindConfig();

            CompletionDatabase.Load();

            harmony = new Harmony(PluginGUID);
            harmony.PatchAll();
            Jotunn.Logger.LogInfo("Harmony patches applied.");

            if (!GUIManager.IsHeadless())
            {
                gameObject.AddComponent<CompletionMenu>();
                Jotunn.Logger.LogInfo("Completion menu registered.");
            }

            Jotunn.Logger.LogInfo($"{PluginName} loaded.");
        }

        private void OnDestroy()
        {
            harmony?.UnpatchSelf();

            CompletionProgress.Unload();
            loadedCharacterName = null;

            GUIManager.BlockInput(false);

        }

        private void Update()
        {
            HandleCharacterProgressState();

            HandleDebugInput();

            if (!CompletionProgress.IsLoadedForCharacter)
            {
                return;
            }

            HandlePeriodicItemScan();
            HandlePeriodicBossScan();
        }

        private void BindConfig()
        {
            enableDebugExport = Config.Bind(
                "Debug",
                "Enable ObjectDB Export",
                false,
                "If true, pressing F10 exports ObjectDB item data to BepInEx/config/ValheimCompletionist/objectdb_items.csv."
            );
        }

        private void HandleDebugInput()
        {
            if (!enableDebugExport.Value)
            {
                return;
            }

            if (Input.GetKeyDown(KeyCode.F10))
            {
                ExportObjectDBItemsToCsv();
            }
        }

        private void HandlePeriodicItemScan()
        {
            if (!CompletionProgress.IsLoadedForCharacter)
            {
                return;
            }

            itemScanTimer += Time.deltaTime;

            if (itemScanTimer < ItemScanInterval)
            {
                return;
            }

            itemScanTimer = 0f;

            ItemCompletionTracker.ScanPlayerInventory();
        }

        private void HandlePeriodicBossScan()
        {
            if (!CompletionProgress.IsLoadedForCharacter)
            {
                return;
            }

            bossScanTimer += Time.deltaTime;

            if (bossScanTimer < BossScanInterval)
            {
                return;
            }

            bossScanTimer = 0f;

            ScanBossGlobalKeys();
        }

        private void ScanBossGlobalKeys()
        {
            if (!CompletionProgress.IsLoadedForCharacter)
            {
                return;
            }

            if (ZoneSystem.instance == null)
            {
                return;
            }

            foreach (ChecklistEntry bossEntry in CompletionDatabase.GetByCategory(ChecklistCategory.Bosses))
            {
                if (string.IsNullOrWhiteSpace(bossEntry.GlobalKey))
                {
                    continue;
                }

                if (ZoneSystem.instance.GetGlobalKey(bossEntry.GlobalKey))
                {
                    CompletionProgress.MarkCompleted(bossEntry.Id);
                }
            }
        }

        private void HandleCharacterProgressLoading()
        {
            if (Player.m_localPlayer == null)
            {
                return;
            }

            string characterName = Player.m_localPlayer.GetPlayerName();

            if (string.IsNullOrWhiteSpace(characterName))
            {
                return;
            }

            if (loadedCharacterName == characterName)
            {
                return;
            }

            loadedCharacterName = characterName;

            CompletionProgress.LoadForCharacter(characterName);

            Jotunn.Logger.LogInfo($"ValheimCompletionist using progress for character: {characterName}");
        }

        private void HandleCharacterProgressState()
        {
            if (Player.m_localPlayer == null)
            {
                if (!string.IsNullOrWhiteSpace(loadedCharacterName))
                {
                    Jotunn.Logger.LogInfo($"Unloading completion progress for character: {loadedCharacterName}");

                    CompletionProgress.Unload();
                    loadedCharacterName = null;
                }

                return;
            }

            string characterName = Player.m_localPlayer.GetPlayerName();

            if (string.IsNullOrWhiteSpace(characterName))
            {
                return;
            }

            if (loadedCharacterName == characterName && CompletionProgress.IsLoadedForCharacter)
            {
                return;
            }

            if (!string.IsNullOrWhiteSpace(loadedCharacterName))
            {
                Jotunn.Logger.LogInfo($"Switching completion progress from '{loadedCharacterName}' to '{characterName}'.");

                CompletionProgress.Unload();
            }

            loadedCharacterName = characterName;

            CompletionProgress.LoadForCharacter(characterName);

            Jotunn.Logger.LogInfo($"Loaded completion progress for character: {characterName}");
        }

        // DEBUG SECTION - REMOVE OR DISABLE BEFORE RELEASE --------------------

        private void ExportObjectDBItemsToCsv()
        {
            if (ObjectDB.instance == null)
            {
                Jotunn.Logger.LogWarning("ObjectDB.instance is null. Enter a world first.");
                return;
            }

            string folderPath = Path.Combine(Paths.ConfigPath, PluginName);
            Directory.CreateDirectory(folderPath);

            string filePath = Path.Combine(folderPath, "objectdb_items.csv");

            StringBuilder csv = new StringBuilder();

            csv.AppendLine("PrefabName,NameToken,ItemType,MaxStack,Weight");

            foreach (GameObject itemPrefab in ObjectDB.instance.m_items)
            {
                if (itemPrefab == null)
                {
                    continue;
                }

                ItemDrop itemDrop = itemPrefab.GetComponent<ItemDrop>();

                if (itemDrop == null)
                {
                    continue;
                }

                ItemDrop.ItemData.SharedData shared = itemDrop.m_itemData.m_shared;

                bool likelyChecklistItem =
                    shared.m_name.StartsWith("$item_") ||
                    shared.m_itemType == ItemDrop.ItemData.ItemType.Fish;

                if (!likelyChecklistItem)
                {
                    continue;
                }

                csv.AppendLine(
                    $"{EscapeCsv(itemPrefab.name)}," +
                    $"{EscapeCsv(shared.m_name)}," +
                    $"{EscapeCsv(shared.m_itemType.ToString())}," +
                    $"{shared.m_maxStackSize}," +
                    $"{shared.m_weight}"
                );
            }

            File.WriteAllText(filePath, csv.ToString());

            Jotunn.Logger.LogInfo($"Exported ObjectDB items to: {filePath}");
        }

        private string EscapeCsv(string value)
        {
            if (value == null)
            {
                return "";
            }

            if (value.Contains(",") || value.Contains("\"") || value.Contains("\n"))
            {
                return $"\"{value.Replace("\"", "\"\"")}\"";
            }

            return value;
        }

        // END DEBUG SECTION --------------------------------------------------
    }
}