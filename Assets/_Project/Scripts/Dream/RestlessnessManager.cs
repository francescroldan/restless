using System;
using UnityEngine;
using Restless.Core;

namespace Restless.Dream
{
    public class RestlessnessManager : MonoBehaviour
    {
        public static RestlessnessManager Instance { get; private set; }

        public enum Threshold { Low, Medium, High, Critical, Max }

        public event Action OnMaxReached;

        [Header("Events")]
        [SerializeField] private GameEventFloat _onRestlessnessChanged;
        [SerializeField] private GameEvent _onRestlessnessMax;

        [Header("Rates (units/sec)")]
        [SerializeField] private float _baseRate = 0.5f;
        [SerializeField] private float _minigameMultiplier = 2.5f;

        private float _value;
        private float _rateMultiplier = 1f;
        private bool _minigameActive;
        private Threshold _currentThreshold = Threshold.Low;

        public float Value => _value;
        public float NormalizedValue => _value / 100f;
        public Threshold CurrentThreshold => _currentThreshold;

        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
        }

        private void Update()
        {
            float rate = _baseRate * _rateMultiplier;
            if (_minigameActive) rate *= _minigameMultiplier;

            _value = Mathf.Clamp(_value + rate * Time.deltaTime, 0f, 100f);

            UpdateThreshold();
            _onRestlessnessChanged?.Raise(_value);

            if (_value >= 100f)
            {
                _onRestlessnessMax?.Raise();
                OnMaxReached?.Invoke();
            }
        }

        private void UpdateThreshold()
        {
            Threshold newThreshold = _value switch
            {
                < 25f => Threshold.Low,
                < 50f => Threshold.Medium,
                < 75f => Threshold.High,
                < 100f => Threshold.Critical,
                _ => Threshold.Max
            };
            _currentThreshold = newThreshold;
        }

        /// <summary>Called by RestlessnessZone when player enters/exits.</summary>
        public void SetZoneMultiplier(float multiplier) => _rateMultiplier = multiplier;

        /// <summary>Called by MemoryPoint when minigame starts/ends.</summary>
        public void SetMinigameActive(bool active) => _minigameActive = active;

        /// <summary>Instant spike — used by entity detection.</summary>
        public void AddSpike(float amount) => _value = Mathf.Min(100f, _value + amount);

        public void Reduce(float amount) => _value = Mathf.Max(0f, _value - amount);
    }
}
