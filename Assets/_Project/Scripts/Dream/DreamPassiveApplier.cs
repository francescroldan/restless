using UnityEngine;
using Restless.Core;
using Restless.Vigil;

namespace Restless.Dream
{
    /// <summary>
    /// Reads SaveData.selectedAllyIds at dream start and applies all passive modifiers.
    /// Attach to _Managers in the Dream scene alongside DreamSceneBootstrap.
    /// </summary>
    public class DreamPassiveApplier : MonoBehaviour
    {
        [SerializeField] private AllyRegistry _registry;

        /// <summary>
        /// Applies ally passives and returns the modified dream duration.
        /// Must be called before DreamTimer.StartTimer.
        /// </summary>
        public static DreamPassiveApplier FindApplier() =>
            Object.FindFirstObjectByType<DreamPassiveApplier>();

        public float MinigameSpeedMultiplier { get; private set; } = 1f;
        public float HealthCostMultiplier    { get; private set; } = 1f;
        public int   InventoryBonusCells     { get; private set; } = 0;

        public float ApplyPassives(float baseDuration)
        {
            MinigameSpeedMultiplier = 1f;
            HealthCostMultiplier    = 1f;
            InventoryBonusCells     = 0;

            if (_registry == null) { Debug.LogWarning("[DreamPassiveApplier] No AllyRegistry assigned."); return baseDuration; }

            var selectedIds = SaveManager.Instance?.Data?.selectedAllyIds;
            if (selectedIds == null || selectedIds.Count == 0) return baseDuration;

            float rateMultiplier  = 1f;
            float durationBonus   = 0f;

            foreach (var id in selectedIds)
            {
                var ally = _registry.GetById(id);
                if (ally == null) continue;
                rateMultiplier          += ally.restlessnessRateModifier;
                durationBonus           += ally.dreamDurationBonus;
                MinigameSpeedMultiplier *= ally.minigameSpeedMultiplier;
                HealthCostMultiplier    *= ally.healthCostMultiplier;
                InventoryBonusCells     += ally.inventoryBonusCells;
            }

            rateMultiplier = Mathf.Max(0.1f, rateMultiplier);
            RestlessnessManager.Instance?.SetPassiveMultiplier(rateMultiplier);

            float finalDuration = baseDuration + durationBonus;
            Debug.Log($"[DreamPassiveApplier] rate×{rateMultiplier:F2}  duration {baseDuration}+{durationBonus}={finalDuration}s  minigame×{MinigameSpeedMultiplier:F2}  health×{HealthCostMultiplier:F2}  invBonus={InventoryBonusCells}");
            return finalDuration;
        }
    }
}
