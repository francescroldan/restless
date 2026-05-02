using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Restless.Dream
{
    /// <summary>
    /// Variant C: Hold Interact to fill a concentration bar. Restlessness and nearby entities drain it.
    /// Success when bar reaches 1; failure if it drops to 0 while the bar was already started.
    /// </summary>
    public class RetentionMinigame : MonoBehaviour, IExtractionMinigame
    {
        [SerializeField] private float _fillRate = 0.4f;
        [SerializeField] private float _decayRate = 0.2f;
        [SerializeField] private float _restlessnessDecayBonus = 0.3f; // extra drain when High/Critical

        private Action _onSuccess;
        private Action _onFailure;
        private bool _isActive;
        private float _concentration; // 0..1
        private bool _started;        // true once player has pressed at least once
        private PlayerInput _playerInput;

        public bool IsActive => _isActive;
        public float Concentration => _concentration;

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
            _concentration = 0f;
            _started = false;
            _isActive = true;
        }

        public void Cancel()
        {
            _isActive = false;
        }

        public void ApplyInterruption(float magnitude)
        {
            if (!_isActive) return;
            _concentration = Mathf.Max(0f, _concentration - magnitude);
        }

        private void Update()
        {
            if (!_isActive) return;

            bool holding = _playerInput != null &&
                           _playerInput.actions["Player/Interact"].IsPressed();

            if (holding)
            {
                _started = true;
                _concentration = Mathf.Min(1f, _concentration + _fillRate * Time.deltaTime);
            }
            else if (_started)
            {
                float drain = _decayRate;

                if (RestlessnessManager.Instance != null)
                {
                    var threshold = RestlessnessManager.Instance.CurrentThreshold;
                    if (threshold >= RestlessnessManager.Threshold.High)
                        drain += _restlessnessDecayBonus;
                }

                _concentration = Mathf.Max(0f, _concentration - drain * Time.deltaTime);
            }

            if (_concentration >= 1f)
            {
                _isActive = false;
                _onSuccess?.Invoke();
                return;
            }

            if (_started && _concentration <= 0f)
            {
                _isActive = false;
                _onFailure?.Invoke();
            }
        }
    }
}
