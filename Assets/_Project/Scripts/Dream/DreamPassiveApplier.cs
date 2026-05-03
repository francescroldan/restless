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

        public void ApplyPassives()
        {
            if (_registry == null) { Debug.LogWarning("[DreamPassiveApplier] No AllyRegistry assigned."); return; }

            var selectedIds = SaveManager.Instance?.Data?.selectedAllyIds;
            if (selectedIds == null || selectedIds.Count == 0) return;

            float combinedRateMultiplier = 1f;
            foreach (var id in selectedIds)
            {
                var ally = _registry.GetById(id);
                if (ally == null) continue;
                // restlessnessRateModifier is additive: -0.3 means -30%
                combinedRateMultiplier += ally.restlessnessRateModifier;
            }
            combinedRateMultiplier = Mathf.Max(0.1f, combinedRateMultiplier);

            RestlessnessManager.Instance?.SetPassiveMultiplier(combinedRateMultiplier);
            Debug.Log($"[DreamPassiveApplier] Passive rate multiplier: {combinedRateMultiplier:F2}");
        }
    }
}
