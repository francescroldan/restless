using UnityEngine;
using Restless.Core;
using Restless.Vigil;

namespace Restless.Dream
{
    /// <summary>
    /// Reads SaveData.selectedAllyIds and writes all passive modifiers into RunConfig.Current.
    /// Must be called after RunConfig.Create() and before DreamTimer.StartTimer().
    /// Attach to _Managers alongside DreamSceneBootstrap.
    /// </summary>
    public class DreamPassiveApplier : MonoBehaviour
    {
        [SerializeField] private AllyRegistry _registry;

        public static DreamPassiveApplier FindApplier() =>
            Object.FindFirstObjectByType<DreamPassiveApplier>();

        public void ApplyPassives()
        {
            var run = RunConfig.Current;
            if (run == null)
            {
                Debug.LogWarning("[DreamPassiveApplier] RunConfig.Current is null — passives not applied.");
                return;
            }

            if (_registry == null)
            {
                Debug.LogWarning("[DreamPassiveApplier] No AllyRegistry assigned.");
                return;
            }

            var selectedIds = SaveManager.Instance?.Data?.selectedAllyIds;
            if (selectedIds == null || selectedIds.Count == 0) return;

            float rateMultiplier = 1f;
            float durationBonus  = 0f;
            float minigameMult   = 1f;
            float healthMult     = 1f;
            int   invBonus       = 0;

            foreach (var id in selectedIds)
            {
                var ally = _registry.GetById(id);
                if (ally == null) continue;
                rateMultiplier += ally.restlessnessRateModifier;
                durationBonus  += ally.dreamDurationBonus;
                minigameMult   *= ally.minigameSpeedMultiplier;
                healthMult     *= ally.healthCostMultiplier;
                invBonus       += ally.inventoryBonusCells;
            }

            run.restlessnessPassiveMultiplier = Mathf.Max(0.1f, rateMultiplier);
            run.dreamDuration                += durationBonus;
            run.minigameSpeedMultiplier       = minigameMult;
            run.healthCostMultiplier          = healthMult;
            run.inventoryBonusCells           = invBonus;

            Debug.Log($"[DreamPassiveApplier] rate×{run.restlessnessPassiveMultiplier:F2}  " +
                      $"duration {run.dreamDuration}s  minigame×{minigameMult:F2}  " +
                      $"health×{healthMult:F2}  invBonus={invBonus}");
        }
    }
}
