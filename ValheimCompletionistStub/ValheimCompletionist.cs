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
    //[NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.Minor)]
    internal class ValheimCompletionist : BaseUnityPlugin
    {
        public const string PluginGUID = "com.ryansberc21.ValheimCompletionist";
        public const string PluginName = "ValheimCompletionist";
        public const string PluginVersion = "0.0.1";
        
        public static CustomLocalization Localization = LocalizationManager.Instance.GetLocalization();

        // Object Instatiation
        private GameObject bossPanel;
        private ConfigEntry<KeyboardShortcut> toggleKey;

        private readonly List<BossInfo> bosses = new List<BossInfo>
        {
            new BossInfo("Eikthyr", "defeated_eikthyr"),
            new BossInfo("The Elder", "defeated_gdking"),
            new BossInfo("Bonemass", "defeated_bonemass"),
            new BossInfo("Moder", "defeated_dragon"),
            new BossInfo("Yagluth", "defeated_goblinking"),
            new BossInfo("The Queen", "defeated_queen"),
            new BossInfo("Fader", "defeated_fader")
        };


        private void Awake()
        {
            Jotunn.Logger.LogInfo($"{PluginName} loaded");

            CompletionDatabase.Load();

            toggleKey = Config.Bind(
                "General",
                "Toggle Boss Checklist Key",
                new KeyboardShortcut(KeyCode.F8),
                "Key used to open and close the boss checklist menu."
            );

            foreach (BossInfo boss in bosses)
            {
                boss.ManualComplete = Config.Bind(
                    "Boss Checklist",
                    boss.Name,
                    false,
                    $"Manual completion state for {boss.Name}."
                );
            }

            if (!GUIManager.IsHeadless())
            {
                GUIManager.OnCustomGUIAvailable += CreateBossChecklistGUI;
            }
        }

        private void OnDestroy()
        {
            GUIManager.OnCustomGUIAvailable -= CreateBossChecklistGUI;
            GUIManager.BlockInput(false);
        }

        private void Update()
        {
            if (toggleKey.Value.IsDown())
            {
                ToggleBossPanel();
            }
            
            if (Input.GetKeyDown(KeyCode.F10))
            {
                ExportObjectDBItemsToCsv();
            }
        }

            // DEBUG SECTIONS - REMOVE BEFORE RELEASE --------------------------
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

           // END DEBUG SECTIONS - REMOVE BEFORE RELEASE --------------------------


        private void CreateBossChecklistGUI()
        {
            bossPanel = null;

            if (GUIManager.Instance == null || GUIManager.CustomGUIFront == null)
            {
                Jotunn.Logger.LogWarning("GUIManager is not ready.");
                return;
            }

            bossPanel = GUIManager.Instance.CreateWoodpanel(
                parent: GUIManager.CustomGUIFront.transform,
                anchorMin: new Vector2(0.5f, 0.5f),
                anchorMax: new Vector2(0.5f, 0.5f),
                position: new Vector2(0f, 0f),
                width: 460f,
                height: 560f,
                draggable: true
            );

            bossPanel.name = "ValheimCompletionist_BossChecklistPanel";
            bossPanel.SetActive(false);

            GUIManager.Instance.CreateText(
                text: "Boss Checklist",
                parent: bossPanel.transform,
                anchorMin: new Vector2(0.5f, 1f),
                anchorMax: new Vector2(0.5f, 1f),
                position: new Vector2(0f, -45f),
                font: GUIManager.Instance.AveriaSerifBold,
                fontSize: 30,
                color: GUIManager.Instance.ValheimOrange,
                outline: true,
                outlineColor: Color.black,
                width: 400f,
                height: 45f,
                addContentSizeFitter: false
            );

            GUIManager.Instance.CreateText(
                text: "Auto-detected from world boss keys when available.",
                parent: bossPanel.transform,
                anchorMin: new Vector2(0.5f, 1f),
                anchorMax: new Vector2(0.5f, 1f),
                position: new Vector2(0f, -85f),
                font: GUIManager.Instance.AveriaSerif,
                fontSize: 16,
                color: Color.white,
                outline: true,
                outlineColor: Color.black,
                width: 390f,
                height: 30f,
                addContentSizeFitter: false
            );

            float startY = -135f;
            float rowSpacing = 48f;

            for (int i = 0; i < bosses.Count; i++)
            {
                BossInfo boss = bosses[i];
                float y = startY - i * rowSpacing;

                GameObject toggleObject = GUIManager.Instance.CreateToggle(
                    parent: bossPanel.transform,
                    width: 32f,
                    height: 32f
                );

                RectTransform toggleRect = toggleObject.GetComponent<RectTransform>();
                toggleRect.anchorMin = new Vector2(0.5f, 1f);
                toggleRect.anchorMax = new Vector2(0.5f, 1f);
                toggleRect.anchoredPosition = new Vector2(-165f, y);

                Toggle toggle = toggleObject.GetComponent<Toggle>();
                boss.Toggle = toggle;

                GUIManager.Instance.CreateText(
                    text: boss.Name,
                    parent: bossPanel.transform,
                    anchorMin: new Vector2(0.5f, 1f),
                    anchorMax: new Vector2(0.5f, 1f),
                    position: new Vector2(20f, y),
                    font: GUIManager.Instance.AveriaSerifBold,
                    fontSize: 22,
                    color: Color.white,
                    outline: true,
                    outlineColor: Color.black,
                    width: 300f,
                    height: 35f,
                    addContentSizeFitter: false
                );

                toggle.onValueChanged.AddListener(value =>
                {
                    boss.ManualComplete.Value = value;
                });
            }

            GameObject refreshButtonObject = GUIManager.Instance.CreateButton(
                text: "Refresh",
                parent: bossPanel.transform,
                anchorMin: new Vector2(0.5f, 0f),
                anchorMax: new Vector2(0.5f, 0f),
                position: new Vector2(-95f, 45f),
                width: 150f,
                height: 45f
            );

            refreshButtonObject.GetComponent<Button>().onClick.AddListener(RefreshChecklist);

            GameObject closeButtonObject = GUIManager.Instance.CreateButton(
                text: "Close",
                parent: bossPanel.transform,
                anchorMin: new Vector2(0.5f, 0f),
                anchorMax: new Vector2(0.5f, 0f),
                position: new Vector2(95f, 45f),
                width: 150f,
                height: 45f
            );

            closeButtonObject.GetComponent<Button>().onClick.AddListener(ToggleBossPanel);

            RefreshChecklist();
        }

        private class BossInfo
        {
            public string Name { get; }
            public string GlobalKey { get; }
            public Toggle Toggle { get; set; }
            public ConfigEntry<bool> ManualComplete { get; set; }

            public BossInfo(string name, string globalKey)
            {
                Name = name;
                GlobalKey = globalKey;
            }
        }

        private void ToggleBossPanel()
        {
            if (bossPanel == null)
            {
                return;
            }

            bool newState = !bossPanel.activeSelf;
            bossPanel.SetActive(newState);

            if (newState)
            {
                RefreshChecklist();
            }

            GUIManager.BlockInput(newState);
        }

        private void RefreshChecklist()
        {
            foreach (BossInfo boss in bosses)
            {
                bool defeatedInWorld = IsGlobalKeySet(boss.GlobalKey);
                bool complete = defeatedInWorld || boss.ManualComplete.Value;

                if (boss.Toggle != null)
                {
                    boss.Toggle.SetIsOnWithoutNotify(complete);
                }

                if (defeatedInWorld)
                {
                    boss.ManualComplete.Value = true;
                }
            }
        }

        private bool IsGlobalKeySet(string key)
        {
            if (ZoneSystem.instance == null)
            {
                return false;
            }

            return ZoneSystem.instance.GetGlobalKey(key);
        }
    }
}