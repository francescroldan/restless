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
        }

        private void Update()
        {
            if (_state != State.Available || _protagonist == null) return;

            float dist = Vector2.Distance(transform.position, _protagonist.position);
            if (dist > _interactRange) return;

            bool inCone = _visionCone == null || _visionCone.ContainsPoint(transform.position);
            if (!inCone) return;

            bool interactPressed = _playerInput != null &&
                                   _playerInput.actions["Player/Interact"].WasPressedThisFrame();
            if (interactPressed)
                StartExtraction();
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
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, _interactRange);
        }
    }
}
