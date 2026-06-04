using BepInEx;
using BepInEx.Configuration;
using Jotunn.Entities;
using Jotunn.Managers;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using ValheimCompletionist.Checklist;

namespace ValheimCompletionist
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [BepInDependency(Jotunn.Main.ModGuid)]
    internal class ValheimCompletionist : BaseUnityPlugin
    {
        public const string PluginGUID = "com.ryansberc21.ValheimCompletionist";
        public const string PluginName = "ValheimCompletionist";
        public const string PluginVersion = "0.1.1";

        public static CustomLocalization Localization = LocalizationManager.Instance.GetLocalization();

        private GameObject checklistPanel;

        private ConfigEntry<KeyboardShortcut> toggleKey;
        private ConfigEntry<bool> enableDebugExport;

        private readonly List<BossRow> bossRows = new List<BossRow>();

        private float itemScanTimer = 0f;
        private float bossScanTimer = 0f;

        private const float ItemScanInterval = 5f;
        private const float BossScanInterval = 5f;

        private void Awake()
        {
            Jotunn.Logger.LogInfo($"{PluginName} loading...");

            BindConfig();

            CompletionDatabase.Load();
            CompletionProgress.Load();

            if (!GUIManager.IsHeadless())
            {
                GUIManager.OnCustomGUIAvailable += CreateChecklistGUI;
            }

            Jotunn.Logger.LogInfo($"{PluginName} loaded.");
        }

        private void OnDestroy()
        {
            GUIManager.OnCustomGUIAvailable -= CreateChecklistGUI;
            GUIManager.BlockInput(false);
        }

        private void Update()
        {
            HandleToggleInput();
            HandleDebugInput();
            HandlePeriodicItemScan();
            HandlePeriodicBossScan();
        }

        private void BindConfig()
        {
            toggleKey = Config.Bind(
                "General",
                "Toggle Checklist Key",
                new KeyboardShortcut(KeyCode.F8),
                "Key used to open and close the completion checklist menu."
            );

            enableDebugExport = Config.Bind(
                "Debug",
                "Enable ObjectDB Export",
                false,
                "If true, pressing F10 exports ObjectDB item data to BepInEx/config/ValheimCompletionist/objectdb_items.csv."
            );
        }

        private void HandleToggleInput()
        {
            if (toggleKey.Value.IsDown())
            {
                ToggleChecklistPanel();
            }
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

            RefreshChecklist();
        }

        private void CreateChecklistGUI()
        {
            checklistPanel = null;
            bossRows.Clear();

            if (GUIManager.Instance == null || GUIManager.CustomGUIFront == null)
            {
                Jotunn.Logger.LogWarning("GUIManager is not ready.");
                return;
            }

            checklistPanel = GUIManager.Instance.CreateWoodpanel(
                parent: GUIManager.CustomGUIFront.transform,
                anchorMin: new Vector2(0.5f, 0.5f),
                anchorMax: new Vector2(0.5f, 0.5f),
                position: new Vector2(0f, 0f),
                width: 500f,
                height: 600f,
                draggable: true
            );

            checklistPanel.name = "ValheimCompletionist_ChecklistPanel";
            checklistPanel.SetActive(false);

            GUIManager.Instance.CreateText(
                text: "Valheim Completionist",
                parent: checklistPanel.transform,
                anchorMin: new Vector2(0.5f, 1f),
                anchorMax: new Vector2(0.5f, 1f),
                position: new Vector2(0f, -45f),
                font: GUIManager.Instance.AveriaSerifBold,
                fontSize: 30,
                color: GUIManager.Instance.ValheimOrange,
                outline: true,
                outlineColor: Color.black,
                width: 440f,
                height: 45f,
                addContentSizeFitter: false
            );

            GUIManager.Instance.CreateText(
                text: "Bosses are auto-detected from world global keys.",
                parent: checklistPanel.transform,
                anchorMin: new Vector2(0.5f, 1f),
                anchorMax: new Vector2(0.5f, 1f),
                position: new Vector2(0f, -85f),
                font: GUIManager.Instance.AveriaSerif,
                fontSize: 16,
                color: Color.white,
                outline: true,
                outlineColor: Color.black,
                width: 430f,
                height: 30f,
                addContentSizeFitter: false
            );

            CreateBossRows();

            GameObject refreshButtonObject = GUIManager.Instance.CreateButton(
                text: "Refresh",
                parent: checklistPanel.transform,
                anchorMin: new Vector2(0.5f, 0f),
                anchorMax: new Vector2(0.5f, 0f),
                position: new Vector2(-95f, 45f),
                width: 150f,
                height: 45f
            );

            refreshButtonObject.GetComponent<Button>().onClick.AddListener(RefreshChecklist);

            GameObject closeButtonObject = GUIManager.Instance.CreateButton(
                text: "Close",
                parent: checklistPanel.transform,
                anchorMin: new Vector2(0.5f, 0f),
                anchorMax: new Vector2(0.5f, 0f),
                position: new Vector2(95f, 45f),
                width: 150f,
                height: 45f
            );

            closeButtonObject.GetComponent<Button>().onClick.AddListener(ToggleChecklistPanel);

            RefreshChecklist();
        }

        private void CreateBossRows()
        {
            float startY = -135f;
            float rowSpacing = 48f;
            int index = 0;

            foreach (ChecklistEntry bossEntry in CompletionDatabase.GetByCategory(ChecklistCategory.Bosses))
            {
                float y = startY - index * rowSpacing;
                index++;

                GameObject toggleObject = GUIManager.Instance.CreateToggle(
                    parent: checklistPanel.transform,
                    width: 32f,
                    height: 32f
                );

                RectTransform toggleRect = toggleObject.GetComponent<RectTransform>();
                toggleRect.anchorMin = new Vector2(0.5f, 1f);
                toggleRect.anchorMax = new Vector2(0.5f, 1f);
                toggleRect.anchoredPosition = new Vector2(-175f, y);

                Toggle toggle = toggleObject.GetComponent<Toggle>();

                ChecklistEntry capturedEntry = bossEntry;

                toggle.onValueChanged.AddListener(value =>
                {
                    if (value)
                    {
                        CompletionProgress.MarkCompleted(capturedEntry.Id);
                    }
                });

                GUIManager.Instance.CreateText(
                    text: bossEntry.DisplayName,
                    parent: checklistPanel.transform,
                    anchorMin: new Vector2(0.5f, 1f),
                    anchorMax: new Vector2(0.5f, 1f),
                    position: new Vector2(25f, y),
                    font: GUIManager.Instance.AveriaSerifBold,
                    fontSize: 22,
                    color: Color.white,
                    outline: true,
                    outlineColor: Color.black,
                    width: 330f,
                    height: 35f,
                    addContentSizeFitter: false
                );

                bossRows.Add(new BossRow(bossEntry, toggle));
            }
        }

        private void ToggleChecklistPanel()
        {
            if (checklistPanel == null)
            {
                return;
            }

            bool newState = !checklistPanel.activeSelf;
            checklistPanel.SetActive(newState);

            if (newState)
            {
                ScanBossGlobalKeys();
                ItemCompletionTracker.ScanPlayerInventory();
                RefreshChecklist();
            }

            GUIManager.BlockInput(newState);
        }

        private void RefreshChecklist()
        {
            foreach (BossRow row in bossRows)
            {
                bool completed = CompletionProgress.IsCompleted(row.Entry.Id);

                if (row.Toggle != null)
                {
                    row.Toggle.SetIsOnWithoutNotify(completed);
                }
            }
        }

        private class BossRow
        {
            public ChecklistEntry Entry { get; }
            public Toggle Toggle { get; }

            public BossRow(ChecklistEntry entry, Toggle toggle)
            {
                Entry = entry;
                Toggle = toggle;
            }
        }

        // DEBUG SECTION - REMOVE OR DISABLE BEFORE RELEASE --------------------

        private void ExportObjectDBItemsToCsv()
        {
            if (ObjectDB.instance == null)
            {
                Jotunn.Logger.LogWarning("ObjectDB.instance is null. Enter a world first.");
                return;
            }

            string folderPath = Path.Combine(Paths.ConfigPath, "ValheimCompletionist");
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