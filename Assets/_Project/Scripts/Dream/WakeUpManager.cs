using UnityEngine;
using UnityEngine.InputSystem;
using Restless.Core;

namespace Restless.Dream
{
    public class WakeUpManager : MonoBehaviour
    {
        private PlayerInput _playerInput;
        private bool _waking;

        private void Start()
        {
            var protagonistGO = GameObject.FindWithTag("Player");
            if (protagonistGO != null)
                _playerInput = protagonistGO.GetComponent<PlayerInput>();

            if (DreamTimer.Instance != null)
                DreamTimer.Instance.OnExpired += TriggerAbruptWakeUp;
        }

        private void OnDestroy()
        {
            if (DreamTimer.Instance != null)
                DreamTimer.Instance.OnExpired -= TriggerAbruptWakeUp;
        }

        private void Update()
        {
            if (_waking) return;
            if (_playerInput == null) return;
            if (InventoryPlacementUI.Instance != null && InventoryPlacementUI.Instance.IsOpen) return;
            if (_playerInput.actions["Player/WakeUp"].WasPressedThisFrame())
                BeginWakeUp(abrupt: false);
        }

        private void BeginWakeUp(bool abrupt)
        {
            if (_waking) return;
            _waking = true;
            Debug.Log($"[WakeUpManager] Wake-up — abrupt: {abrupt}");
            if (abrupt) DreamSFXPlayer.Instance?.PlayWakeupAbrupt();
            else        DreamSFXPlayer.Instance?.PlayWakeupVoluntary();

            if (TransitionFX.Instance != null)
            {
                if (abrupt)
                    TransitionFX.Instance.BeginAbrupt(() => GameManager.Instance?.ExitDream(abrupt: true));
                else
                    TransitionFX.Instance.BeginVoluntary(() => GameManager.Instance?.ExitDream(abrupt: false));
            }
            else
            {
                GameManager.Instance?.ExitDream(abrupt: abrupt);
            }
        }

        private void TriggerAbruptWakeUp() => BeginWakeUp(abrupt: true);
    }
}
