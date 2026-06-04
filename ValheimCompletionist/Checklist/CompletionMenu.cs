/*
* ryansberc21 - 2026-06-04
* In-game completion menu for ValheimCompletionist.
* Shows checklist progress grouped by Biome -> Items -> Category, Enemies, Boss.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using Jotunn.Managers;
using UnityEngine;

namespace ValheimCompletionist.Checklist
{
    public class CompletionMenu : MonoBehaviour
    {
        private bool isOpen = false;

        private Rect windowRect = new Rect(120f, 80f, 900f, 700f);
        private Vector2 scrollPosition = Vector2.zero;

        private readonly Dictionary<Biome, bool> biomeFoldouts = new Dictionary<Biome, bool>();
        private readonly Dictionary<string, bool> sectionFoldouts = new Dictionary<string, bool>();

        private GUIStyle titleStyle;
        private GUIStyle headerStyle;
        private GUIStyle normalStyle;
        private GUIStyle completedStyle;
        private GUIStyle incompleteStyle;
        private GUIStyle foldoutButtonStyle;
        private GUIStyle entryBoxStyle;
        private GUIStyle completedBiomeStyle;
        private GUIStyle goldBiomeStyle;

        private const int WindowId = 832710;

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F8))
            {
                ToggleMenu();
            }

            if (isOpen && Input.GetKeyDown(KeyCode.Escape))
            {
                CloseMenu();
            }
        }

        private void OnDestroy()
        {
            GUIManager.BlockInput(false);
        }

        private void OnGUI()
        {
            if (!isOpen)
            {
                return;
            }

            InitializeStyles();

            windowRect = GUI.Window(
                WindowId,
                windowRect,
                DrawWindow,
                "Valheim Completionist"
            );
        }

        private void ToggleMenu()
        {
            isOpen = !isOpen;

            GUIManager.BlockInput(isOpen);
        }

        private void CloseMenu()
        {
            isOpen = false;

            GUIManager.BlockInput(false);
        }

        private void DrawWindow(int windowId)
        {
            GUILayout.BeginVertical();

            DrawHeader();

            GUILayout.Space(8f);

            scrollPosition = GUILayout.BeginScrollView(scrollPosition, false, true);

            DrawBiomeSections();

            GUILayout.EndScrollView();

            GUILayout.Space(8f);

            DrawFooter();

            GUILayout.EndVertical();

            GUI.DragWindow(new Rect(0f, 0f, 10000f, 26f));
        }

        private void DrawHeader()
        {
            int total = CompletionDatabase.Entries.Count;
            int completed = CompletionProgress.CountCompleted(CompletionDatabase.Entries);

            GUILayout.Label("Valheim Completionist Mod - by realberch", titleStyle);
            GUILayout.Label("Version 0.1.0", normalStyle);
            GUILayout.Label($"Total Completion: {completed}/{total} ({GetPercent(completed, total):0.0}%)", headerStyle);
            GUILayout.Label("Checklist for Bosses, Enemies, and all Items.", normalStyle);
        }

        private void DrawFooter()
        {
            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Refresh", GUILayout.Height(32f)))
            {
                // The main plugin already scans periodically.
                // This button is here so the menu redraws immediately.
                RepaintMenu();
            }

            if (GUILayout.Button("Close", GUILayout.Height(32f)))
            {
                CloseMenu();
            }

            GUILayout.EndHorizontal();
        }

        private void RepaintMenu()
        {
            // Intentionally empty.
            // IMGUI redraws every frame while the menu is open.
            // This method exists for the Refresh button and later manual scan calls.
        }

        private void DrawBiomeSections()
        {
            List<Biome> biomeOrder = GetBiomeDisplayOrder();

            bool allBiomesComplete = AreAllBiomesComplete();

            foreach (Biome biome in biomeOrder)
            {
                List<ChecklistEntry> biomeEntries = CompletionDatabase.Entries
                    .Where(entry => entry.Biome == biome)
                    .OrderBy(entry => entry.DisplayName)
                    .ToList();

                if (biomeEntries.Count == 0)
                {
                    continue;
                }

                DrawBiomeSection(biome, biomeEntries, allBiomesComplete);
            }
        }

        private void DrawBiomeSection(Biome biome, List<ChecklistEntry> biomeEntries, bool allBiomesComplete)
        {
            int total = biomeEntries.Count;
            int completed = CompletionProgress.CountCompleted(biomeEntries);

            bool biomeComplete = total > 0 && completed == total;

            GUIStyle biomeStyle = foldoutButtonStyle;

            if (biomeComplete && allBiomesComplete)
            {
                biomeStyle = goldBiomeStyle;
            }
            else if (biomeComplete)
            {
                biomeStyle = completedBiomeStyle;
            }

            if (!biomeFoldouts.ContainsKey(biome))
            {
                biomeFoldouts[biome] = false;
            }

            GUILayout.BeginVertical(entryBoxStyle);

            biomeFoldouts[biome] = GUILayout.Toggle(
                biomeFoldouts[biome],
                $"{GetFoldoutSymbol(biomeFoldouts[biome])} {biome}  —  {completed}/{total} ({GetPercent(completed, total):0.0}%)",
                biomeStyle
            );

            if (biomeFoldouts[biome])
            {
                GUILayout.Space(4f);

                DrawItemsSection(biome, biomeEntries);
                DrawEnemiesSection(biome, biomeEntries);
                DrawBossSection(biome, biomeEntries);
            }

            GUILayout.EndVertical();
        }





        private List<Biome> GetBiomeDisplayOrder()
        {
            return Enum.GetValues(typeof(Biome))
                .Cast<Biome>()
                .ToList();
        }

        private void DrawItemsSection(Biome biome, List<ChecklistEntry> biomeEntries)
        {
            List<ChecklistEntry> itemEntries = biomeEntries
                .Where(IsItemEntry)
                .OrderBy(entry => entry.Category.ToString())
                .ThenBy(entry => entry.DisplayName)
                .ToList();

            if (itemEntries.Count == 0)
            {
                return;
            }

            string key = $"{biome}_items";

            DrawFoldoutSection(
                key,
                "Items",
                itemEntries,
                16f,
                () =>
                {
                    List<IGrouping<ChecklistCategory, ChecklistEntry>> categoryGroups = itemEntries
                        .GroupBy(entry => entry.Category)
                        .OrderBy(group => group.Key.ToString())
                        .ToList();

                    foreach (IGrouping<ChecklistCategory, ChecklistEntry> categoryGroup in categoryGroups)
                    {
                        DrawItemCategorySection(
                            biome,
                            categoryGroup.Key,
                            categoryGroup.OrderBy(entry => entry.DisplayName).ToList()
                        );
                    }
                }
            );
        }

        private void DrawItemCategorySection(Biome biome, ChecklistCategory category, List<ChecklistEntry> entries)
        {
            string key = $"{biome}_items_{category}";

            DrawFoldoutSection(
                key,
                category.ToString(),
                entries,
                34f,
                () =>
                {
                    foreach (ChecklistEntry entry in entries)
                    {
                        DrawEntryRow(entry, 54f);
                    }
                }
            );
        }

        private void DrawEnemiesSection(Biome biome, List<ChecklistEntry> biomeEntries)
        {
            List<ChecklistEntry> enemyEntries = biomeEntries
                .Where(IsEnemyEntry)
                .OrderBy(entry => entry.DisplayName)
                .ToList();

            if (enemyEntries.Count == 0)
            {
                return;
            }

            string key = $"{biome}_enemies";

            DrawFoldoutSection(
                key,
                "Enemies",
                enemyEntries,
                16f,
                () =>
                {
                    foreach (ChecklistEntry entry in enemyEntries)
                    {
                        DrawEntryRow(entry, 36f);
                    }
                }
            );
        }

        private void DrawBossSection(Biome biome, List<ChecklistEntry> biomeEntries)
        {
            List<ChecklistEntry> bossEntries = biomeEntries
                .Where(IsBossEntry)
                .OrderBy(entry => entry.DisplayName)
                .ToList();

            if (bossEntries.Count == 0)
            {
                return;
            }

            string key = $"{biome}_bosses";

            DrawFoldoutSection(
                key,
                "Boss",
                bossEntries,
                16f,
                () =>
                {
                    foreach (ChecklistEntry entry in bossEntries)
                    {
                        DrawEntryRow(entry, 36f);
                    }
                }
            );
        }

        private void DrawFoldoutSection(
            string key,
            string title,
            List<ChecklistEntry> entries,
            float indent,
            Action drawContents
        )
        {
            if (!sectionFoldouts.ContainsKey(key))
            {
                sectionFoldouts[key] = false;
            }

            int total = entries.Count;
            int completed = CompletionProgress.CountCompleted(entries);

            GUILayout.BeginHorizontal();

            GUILayout.Space(indent);

            sectionFoldouts[key] = GUILayout.Toggle(
                sectionFoldouts[key],
                $"{GetFoldoutSymbol(sectionFoldouts[key])} {title}  —  {completed}/{total} ({GetPercent(completed, total):0.0}%)",
                foldoutButtonStyle
            );

            GUILayout.EndHorizontal();

            if (sectionFoldouts[key])
            {
                drawContents.Invoke();
            }
        }

        private void DrawEntryRow(ChecklistEntry entry, float indent)
        {
            bool completed = CompletionProgress.IsCompleted(entry.Id);
            string status = completed ? "[X]" : "[ ]";

            GUILayout.BeginHorizontal();

            GUILayout.Space(indent);

            GUILayout.Label(status, completed ? completedStyle : incompleteStyle, GUILayout.Width(36f));
            GUILayout.Label(entry.DisplayName, completed ? completedStyle : normalStyle, GUILayout.Width(260f));
            GUILayout.Label(entry.Category.ToString(), normalStyle, GUILayout.Width(130f));
            GUILayout.Label(entry.CompletionType.ToString(), normalStyle, GUILayout.Width(150f));

            if (!string.IsNullOrWhiteSpace(entry.PrefabName))
            {
                GUILayout.Label($"Prefab: {entry.PrefabName}", normalStyle, GUILayout.Width(220f));
            }

            GUILayout.EndHorizontal();
        }

        private bool AreAllBiomesComplete()
        {
            foreach (Biome biome in GetBiomeDisplayOrder())
            {
                List<ChecklistEntry> biomeEntries = CompletionDatabase.Entries
                    .Where(entry => entry.Biome == biome)
                    .ToList();

                if (biomeEntries.Count == 0)
                {
                    continue;
                }

                int completed = CompletionProgress.CountCompleted(biomeEntries);

                if (completed < biomeEntries.Count)
                {
                    return false;
                }
            }

            return true;
        }
        private bool IsItemEntry(ChecklistEntry entry)
        {
            if (entry == null)
            {
                return false;
            }

            return entry.CompletionType == CompletionType.ItemCollected;
        }

        private bool IsEnemyEntry(ChecklistEntry entry)
        {
            if (entry == null)
            {
                return false;
            }

            return entry.CompletionType == CompletionType.EnemyKilled;
        }

        private bool IsBossEntry(ChecklistEntry entry)
        {
            if (entry == null)
            {
                return false;
            }

            return entry.CompletionType == CompletionType.BossDefeated;
        }

        private string GetFoldoutSymbol(bool expanded)
        {
            return expanded ? "[-]" : "[+]";
        }

        private float GetPercent(int completed, int total)
        {
            if (total <= 0)
            {
                return 0f;
            }

            return ((float)completed / total) * 100f;
        }

        private void InitializeStyles()
        {
            if (titleStyle != null)
            {
                return;
            }

            titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 24,
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.white }
            };

            headerStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 16,
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.white }
            };

            normalStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 14,
                normal = { textColor = Color.white }
            };

            completedStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 14,
                normal = { textColor = Color.green }
            };

            incompleteStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 14,
                normal = { textColor = Color.gray }
            };

            foldoutButtonStyle = new GUIStyle(GUI.skin.button)
            {
                alignment = TextAnchor.MiddleLeft,
                fontSize = 15,
                fontStyle = FontStyle.Bold
            };

            completedBiomeStyle = new GUIStyle(foldoutButtonStyle)
            {
                normal = { textColor = Color.green },
                hover = { textColor = Color.green },
                active = { textColor = Color.green },
                focused = { textColor = Color.green }
            };

            goldBiomeStyle = new GUIStyle(foldoutButtonStyle)
            {
                normal = { textColor = new Color(1f, 0.75f, 0.15f) },
                hover = { textColor = new Color(1f, 0.75f, 0.15f) },
                active = { textColor = new Color(1f, 0.75f, 0.15f) },
                focused = { textColor = new Color(1f, 0.75f, 0.15f) }
            };

            entryBoxStyle = new GUIStyle(GUI.skin.box)
            {
                padding = new RectOffset(8, 8, 8, 8)
            };
        }
    }
}