using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace ValheimCompletionist.Checklist
{
    public static class EnemyCompletionTracker
    {
        private static readonly Dictionary<int, float> RecentlyHitByLocalPlayer = new Dictionary<int, float>();

        private const float PlayerKillCreditWindowSeconds = 20f;

        public static void RegisterPlayerHit(Character character)
        {
            if (character == null)
            {
                return;
            }

            RecentlyHitByLocalPlayer[character.GetInstanceID()] = Time.time;
        }

        public static void TryMarkEnemyKilled(Character character)
        {
            if (character == null)
            {
                return;
            }

            int instanceId = character.GetInstanceID();

            if (!RecentlyHitByLocalPlayer.TryGetValue(instanceId, out float lastHitTime))
            {
                return;
            }

            RecentlyHitByLocalPlayer.Remove(instanceId);

            if (Time.time - lastHitTime > PlayerKillCreditWindowSeconds)
            {
                return;
            }

            string prefabName = GetPrefabName(character);

            if (string.IsNullOrWhiteSpace(prefabName))
            {
                return;
            }

            ChecklistEntry entry = CompletionDatabase.GetByPrefabName(
                prefabName,
                CompletionType.EnemyKilled
            );

            if (entry == null)
            {
                Jotunn.Logger.LogInfo($"Killed enemy prefab not in checklist: {prefabName}");
                return;
            }

            CompletionProgress.MarkCompleted(entry.Id);

            Jotunn.Logger.LogInfo($"Enemy checklist completed: {entry.DisplayName} ({prefabName})");
        }

        private static string GetPrefabName(Character character)
        {
            ZNetView zNetView = character.GetComponent<ZNetView>();

            if (zNetView != null)
            {
                string prefabName = zNetView.GetPrefabName();

                if (!string.IsNullOrWhiteSpace(prefabName))
                {
                    return prefabName;
                }
            }

            string objectName = character.gameObject.name;

            if (objectName.EndsWith("(Clone)"))
            {
                objectName = objectName.Replace("(Clone)", "").Trim();
            }

            return objectName;
        }
    }

    [HarmonyPatch(typeof(Character), nameof(Character.Damage))]
    public static class CharacterDamagePatch
    {
        private static void Prefix(Character __instance, HitData hit)
        {
            if (__instance == null || hit == null)
            {
                return;
            }

            if (Player.m_localPlayer == null)
            {
                return;
            }

            Character attacker = hit.GetAttacker();

            if (attacker == Player.m_localPlayer)
            {
                EnemyCompletionTracker.RegisterPlayerHit(__instance);
            }
        }
    }

    [HarmonyPatch(typeof(Character), nameof(Character.OnDeath))]
    public static class CharacterOnDeathPatch
    {
        private static void Postfix(Character __instance)
        {
            EnemyCompletionTracker.TryMarkEnemyKilled(__instance);
        }
    }
}