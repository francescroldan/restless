using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Restless.Dream
{
    public class TimingMinigame : MonoBehaviour, IExtractionMinigame
    {
        [SerializeField] private float _markerSpeed = 1.5f;
        [SerializeField] private float _greenZoneCenter = 0.5f;
        [SerializeField] private float _greenZoneHalfWidth = 0.1f;
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

        public bool IsActive => _isActive;

        // Normalized marker position for HUD display (0..1)
        public float MarkerPosition => _markerPosition;
        public float GreenZoneCenter => _greenZoneCenter;
        public float GreenZoneHalfWidth => _greenZoneHalfWidth;
        public int Successes => _successes;
        public int Failures => _failures;

        private void Awake()
        {
            var protagonistGO = GameObject.FindWithTag("Player");
            if (protagonistGO != null)
                _playerInput = protagonistGO.GetComponent<PlayerInput>();
        }

        public void Begin(Action onSuccess, Action onFailure)
        {
            _onSuccess = onSuccess;
            _onFailure = onFailure;
            _markerPosition = 0f;
            _markerDirection = 1;
            _successes = 0;
            _failures = 0;
            _isActive = true;
        }

        public void Cancel()
        {
            _isActive = false;
        }

        private void Update()
        {
            if (!_isActive) return;

            _markerPosition += _markerDirection * _markerSpeed * Time.deltaTime;

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

            bool pressed = _playerInput.actions["Player/Interact"].WasPressedThisFrame();
            if (!pressed) return;

            float delta = Mathf.Abs(_markerPosition - _greenZoneCenter);
            if (delta <= _greenZoneHalfWidth)
            {
                _successes++;
                if (_successes >= _successesRequired)
                {
                    _isActive = false;
                    _onSuccess?.Invoke();
                }
            }
            else
            {
                _failures++;
                if (_failures > _failuresAllowed)
                {
                    _isActive = false;
                    _onFailure?.Invoke();
                }
            }
        }
    }
}
