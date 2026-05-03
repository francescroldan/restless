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
        [SerializeField] private int _inventoryWidth = 4;
        [SerializeField] private int _inventoryHeight = 5;
        [SerializeField] private float _dreamDuration = 300f;

        private void Start()
        {
            GameManager.Instance?.OnDreamSceneReady();
            DreamInventory.Instance?.Initialize(_inventoryWidth, _inventoryHeight);
            DreamTimer.Instance?.StartTimer(_dreamDuration);
            GetComponent<DreamPassiveApplier>()?.ApplyPassives();
            Debug.Log($"[DreamSceneBootstrap] Inventory {_inventoryWidth}x{_inventoryHeight}, timer {_dreamDuration}s");
        }
    }
}
