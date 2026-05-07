using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Restless.Dream
{
    public class TimingMinigame : MonoBehaviour, IExtractionMinigame
    {
        [SerializeField] private float _markerSpeed = 0.55f;
        [SerializeField] private float _markerSpeedMax = 1.6f;
        [SerializeField] private float _greenZoneCenter = 0.5f;
        [SerializeField] private float _greenZoneHalfWidth = 0.1f;
        [SerializeField] private float _greenZoneHalfWidthMin = 0.04f;
        [SerializeField] private int _successesRequired = 3;
        [SerializeField] private int _failuresAllowed = 2;

        private Action _onSuccess;
        private Action _onFailure;
        private bool _isActive;
        private float _markerPosition; // 0..1
        private int _markerDirection = 1;
        private int _successes;
        private int _failures;
        private PlayerInput _playerInput;
        private int _startFrame = -1;

        public bool IsActive => _isActive;

        // Normalized marker position for HUD display (0..1)
        public float MarkerPosition => _markerPosition;
        public float GreenZoneCenter => _greenZoneCenter;
        public float GreenZoneHalfWidth { get; private set; }
        public int Successes => _successes;
        public int Failures => _failures;

        private float _speedMultiplier = 1f;

        private void Start()
        {
            var protagonistGO = GameObject.FindWithTag("Player");
            if (protagonistGO != null)
                _playerInput = protagonistGO.GetComponent<PlayerInput>();

            // Apply ally passive if available
            var applier = FindFirstObjectByType<DreamPassiveApplier>();
            if (applier != null) _speedMultiplier = applier.MinigameSpeedMultiplier;
        }

        public void Begin(Action onSuccess, Action onFailure)
        {
            _onSuccess = onSuccess;
            _onFailure = onFailure;
            _markerPosition = 0f;
            _markerDirection = 1;
            _successes = 0;
            _failures = 0;
            GreenZoneHalfWidth = _greenZoneHalfWidth;
            _isActive = true;
            _startFrame = Time.frameCount;
        }

        public void Cancel()
        {
            _isActive = false;
        }

        private void Update()
        {
            if (!_isActive) return;

            if (_playerInput == null)
            {
                var p = GameObject.FindWithTag("Player");
                if (p != null) _playerInput = p.GetComponent<PlayerInput>();
            }

            // Scale difficulty with restlessness
            float restT = RestlessnessManager.Instance != null
                ? RestlessnessManager.Instance.NormalizedValue : 0f;
            float speed = Mathf.Lerp(_markerSpeed, _markerSpeedMax, restT) * _speedMultiplier;
            GreenZoneHalfWidth = Mathf.Lerp(_greenZoneHalfWidth, _greenZoneHalfWidthMin, restT);

            _markerPosition += _markerDirection * speed * Time.deltaTime;

            if (_markerPosition >= 1f)
            {
                _markerPosition = 1f;
                _markerDirection = -1;
            }
            else if (_markerPosition <= 0f)
            {
                _markerPosition = 0f;
                _markerDirection = 1;
            }

            if (_playerInput == null) return;
            if (Time.frameCount == _startFrame) return;

            bool pressed = _playerInput.actions["Player/Interact"].WasPressedThisFrame();
            if (!pressed) return;

            float delta = Mathf.Abs(_markerPosition - _greenZoneCenter);
            if (delta <= _greenZoneHalfWidth)
            {
                _successes++;
                DreamSFXPlayer.Instance?.PlayMinigameHit();
                if (_successes >= _successesRequired)
                {
                    _isActive = false;
                    _onSuccess?.Invoke();
                }
            }
            else
            {
                _failures++;
                DreamSFXPlayer.Instance?.PlayMinigameMiss();
                if (_failures > _failuresAllowed)
                {
                    _isActive = false;
                    _onFailure?.Invoke();
                }
            }
        }
    }
}
