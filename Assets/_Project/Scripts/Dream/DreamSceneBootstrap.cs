using UnityEngine;
using Restless.Core;

namespace Restless.Dream
{
    /// <summary>
    /// Bootstraps the Dream scene: creates RunConfig, applies ally passives, then starts
    /// the timer and initializes the inventory. Attach to _Managers alongside DreamPassiveApplier.
    /// </summary>
    public class DreamSceneBootstrap : MonoBehaviour
    {
        [SerializeField] private int              _inventoryWidth   = 4;
        [SerializeField] private int              _inventoryHeight  = 5;
        [SerializeField] private ProtagonistState _protagonistState;
        [SerializeField] private GameConfig       _gameConfig;

        private void Awake()
        {
            // Create RunConfig as early as possible so systems that read it in Start() get it.
            if (_gameConfig != null)
                RunConfig.Create(_gameConfig);
            else
                Debug.LogWarning("[DreamSceneBootstrap] GameConfig not assigned — RunConfig not created, systems will use SerializeField fallbacks.");
        }

        private void Start()
        {
            GameManager.Instance?.OnDreamSceneReady();

            if (SaveManager.Instance != null)
            {
                SaveManager.Instance.Data.totalRuns++;
                SaveManager.Instance.Save();
            }

            // Base duration from protagonist history (accounts for abrupt wake-up degradation).
            float baseDuration = _protagonistState != null
                ? _protagonistState.BaseDreamDuration
                : 200f;

            if (RunConfig.Current != null)
                RunConfig.Current.dreamDuration = baseDuration;

            // Apply ally passives — writes modifiers directly into RunConfig.Current.
            var applier = GetComponent<DreamPassiveApplier>();
            applier?.ApplyPassives();

            float duration      = RunConfig.Current?.dreamDuration ?? baseDuration;
            int   bonusCells    = RunConfig.Current?.inventoryBonusCells ?? 0;
            int   totalCells    = _inventoryWidth * _inventoryHeight + bonusCells;
            int   finalHeight   = Mathf.CeilToInt((float)totalCells / _inventoryWidth);
            DreamInventory.Instance?.Initialize(_inventoryWidth, finalHeight);

            DreamTimer.Instance?.StartTimer(duration);

            Debug.Log($"[DreamSceneBootstrap] Inventory {_inventoryWidth}x{finalHeight} (bonus={bonusCells}), timer {duration}s");
        }

        private void OnDestroy()
        {
            RunConfig.Clear();
        }
    }
}
