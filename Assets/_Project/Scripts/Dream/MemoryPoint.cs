using UnityEngine;
using UnityEngine.InputSystem;

namespace Restless.Dream
{
    public class MemoryPoint : MonoBehaviour
    {
        public enum State { Available, Extracting, Collected, Failed }

        [SerializeField] private MemoryFragment _fragment;
        [SerializeField] private float _interactRange = 1.5f;

        private State _state = State.Available;
        private Transform _protagonist;
        private IExtractionMinigame _minigame;
        private VisionCone _visionCone;
        private PlayerInput _playerInput;

        public MemoryFragment Fragment => _fragment;
        public State CurrentState => _state;

        public void AssignFragment(MemoryFragment fragment) => _fragment = fragment;

        private void Awake()
        {
            _minigame = GetComponent<IExtractionMinigame>();
        }

        private void Start()
        {
            var protagonistGO = GameObject.FindWithTag("Player");
            if (protagonistGO != null)
            {
                _protagonist = protagonistGO.transform;
                _visionCone = protagonistGO.GetComponentInChildren<VisionCone>();
                _playerInput = protagonistGO.GetComponent<PlayerInput>();
            }

            Debug.Log($"[MemoryPoint] {gameObject.name} Start — protagonist={_protagonist != null} visionCone={_visionCone != null} playerInput={_playerInput != null}");
            Invoke(nameof(LogDebugState), 3f);
        }

        private void Update()
        {
            if (_state != State.Available || _protagonist == null) return;
            if (InventoryPlacementUI.Instance != null && InventoryPlacementUI.Instance.IsOpen) return;

            float dist = Vector2.Distance(transform.position, _protagonist.position);
            if (dist > _interactRange) return;

            bool interactPressed = _playerInput != null &&
                                   _playerInput.actions["Player/Interact"].WasPressedThisFrame();
            if (interactPressed)
            {
                Debug.Log($"[MemoryPoint] Interact on {gameObject.name}");
                StartExtraction();
            }
        }

        // Called once per second via InvokeRepeating to verify runtime setup
        private void LogDebugState()
        {
            if (_state != State.Available) { CancelInvoke(nameof(LogDebugState)); return; }
            float dist = _protagonist != null ? Vector2.Distance(transform.position, _protagonist.position) : -1f;
            bool inCone = _visionCone == null || (_protagonist != null && _visionCone.ContainsPoint(transform.position));
            Debug.Log($"[MemoryPoint] {gameObject.name} | state={_state} protagonist={_protagonist != null} dist={dist:F1}/{_interactRange} inCone={inCone} hasInput={_playerInput != null}");
            CancelInvoke(nameof(LogDebugState));
        }

        private void StartExtraction()
        {
            if (_minigame == null)
            {
                Debug.LogWarning($"[MemoryPoint] No IExtractionMinigame found on {gameObject.name}");
                return;
            }

            _state = State.Extracting;
            RestlessnessManager.Instance?.SetMinigameActive(true);
            _visionCone?.Freeze();
            DreamSFXPlayer.Instance?.PlayMemoryActivate();

            _minigame.Begin(
                onSuccess: OnExtractionSuccess,
                onFailure: OnExtractionFailure
            );
        }

        private void OnExtractionSuccess()
        {
            _state = State.Collected;
            RestlessnessManager.Instance?.SetMinigameActive(false);
            _visionCone?.Unfreeze();
            DreamSFXPlayer.Instance?.PlayMinigameSuccess();
            DreamSFXPlayer.Instance?.PlayFragmentCollect();

            // Open manual placement UI if available; fall back to auto-place
            if (InventoryPlacementUI.Instance != null)
                InventoryPlacementUI.Instance.Open(_fragment);
            else
                DreamInventory.Instance?.TryAddFragment(_fragment);
        }

        private void OnExtractionFailure()
        {
            _state = State.Failed;
            RestlessnessManager.Instance?.SetMinigameActive(false);
            _visionCone?.Unfreeze();
            DreamSFXPlayer.Instance?.PlayMinigameFail();
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, _interactRange);
        }
    }
}
