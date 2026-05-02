using UnityEngine;
using UnityEngine.InputSystem;
using Restless.Core;

namespace Restless.Dream
{
    /// <summary>
    /// Listens for voluntary wake-up input and for the timer/restlessness-max events.
    /// Delegates actual scene transition to GameManager.
    /// </summary>
    public class WakeUpManager : MonoBehaviour
    {
        private PlayerInput _playerInput;
        private bool _waking;

        private void Start()
        {
            var protagonistGO = GameObject.FindWithTag("Player");
            if (protagonistGO != null)
                _playerInput = protagonistGO.GetComponent<PlayerInput>();

            // Subscribe to restlessness-max event via RestlessnessManager callback
            if (RestlessnessManager.Instance != null)
                RestlessnessManager.Instance.OnMaxReached += TriggerAbruptWakeUp;

            if (DreamTimer.Instance != null)
                DreamTimer.Instance.OnExpired += TriggerAbruptWakeUp;
        }

        private void OnDestroy()
        {
            if (RestlessnessManager.Instance != null)
                RestlessnessManager.Instance.OnMaxReached -= TriggerAbruptWakeUp;

            if (DreamTimer.Instance != null)
                DreamTimer.Instance.OnExpired -= TriggerAbruptWakeUp;
        }

        private void Update()
        {
            if (_waking) return;
            if (_playerInput == null) return;

            bool wakePressed = _playerInput.actions["Player/WakeUp"].WasPressedThisFrame();
            if (wakePressed)
                TriggerVoluntaryWakeUp();
        }

        private void TriggerVoluntaryWakeUp()
        {
            if (_waking) return;
            _waking = true;
            Debug.Log("[WakeUpManager] Voluntary wake-up.");
            GameManager.Instance?.ExitDream(abrupt: false);
        }

        private void TriggerAbruptWakeUp()
        {
            if (_waking) return;
            _waking = true;
            Debug.Log("[WakeUpManager] Abrupt wake-up triggered.");
            GameManager.Instance?.ExitDream(abrupt: true);
        }
    }
}
