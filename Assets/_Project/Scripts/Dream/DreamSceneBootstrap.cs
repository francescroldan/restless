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
        [SerializeField] private int   _inventoryWidth    = 4;
        [SerializeField] private int   _inventoryHeight   = 5;
        [SerializeField] private float _dreamDuration     = 300f;
        [SerializeField] private float _firstRunBonusTime = 60f;

        private void Start()
        {
            GameManager.Instance?.OnDreamSceneReady();
            DreamInventory.Instance?.Initialize(_inventoryWidth, _inventoryHeight);

            bool isFirstRun = SaveManager.Instance != null && SaveManager.Instance.Data.totalRuns == 0;
            if (SaveManager.Instance != null)
            {
                SaveManager.Instance.Data.totalRuns++;
                SaveManager.Instance.Save();
            }

            float baseDuration = isFirstRun ? _dreamDuration + _firstRunBonusTime : _dreamDuration;
            var applier = GetComponent<DreamPassiveApplier>();
            float duration = applier != null ? applier.ApplyPassives(baseDuration) : baseDuration;
            DreamTimer.Instance?.StartTimer(duration);

            Debug.Log($"[DreamSceneBootstrap] Inventory {_inventoryWidth}x{_inventoryHeight}, timer {duration}s (firstRun={isFirstRun})");
        }
    }
}
