using UnityEngine;
using Restless.Core;

namespace Restless.Dream
{
    /// <summary>
    /// Bootstraps the Dream scene: initializes the inventory grid and starts the dream timer.
    /// Attach to _Managers. Configure dimensions and duration in the Inspector.
    /// </summary>
    public class DreamSceneBootstrap : MonoBehaviour
    {
        [SerializeField] private int   _inventoryWidth  = 4;
        [SerializeField] private int   _inventoryHeight = 5;
        [SerializeField] private ProtagonistState _protagonistState;

        private void Start()
        {
            GameManager.Instance?.OnDreamSceneReady();

            if (SaveManager.Instance != null)
            {
                SaveManager.Instance.Data.totalRuns++;
                SaveManager.Instance.Save();
            }

            float baseDuration = _protagonistState != null
                ? _protagonistState.BaseDreamDuration
                : 200f;
            var applier = GetComponent<DreamPassiveApplier>();
            float duration = applier != null ? applier.ApplyPassives(baseDuration) : baseDuration;

            int bonusCells = applier?.InventoryBonusCells ?? 0;
            int totalCells = _inventoryWidth * _inventoryHeight + bonusCells;
            // Keep width fixed, grow height to accommodate bonus cells
            int finalHeight = Mathf.CeilToInt((float)totalCells / _inventoryWidth);
            DreamInventory.Instance?.Initialize(_inventoryWidth, finalHeight);

            DreamTimer.Instance?.StartTimer(duration);

            Debug.Log($"[DreamSceneBootstrap] Inventory {_inventoryWidth}x{finalHeight} (bonus={bonusCells}), timer {duration}s");
        }
    }
}
