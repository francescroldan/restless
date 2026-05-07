using System;
using UnityEngine;
using Restless.Core;

namespace Restless.Dream
{
    public class DreamTimer : MonoBehaviour
    {
        public event Action OnExpired;
        public static DreamTimer Instance { get; private set; }

        [Header("Events")]
        [SerializeField] private GameEvent _onTimerExpired;

        [Header("Settings")]
        [SerializeField] private float _highRestlessnessAcceleration = 2f;
        [SerializeField] private float _maxRestlessnessAcceleration  = 4f;

        private float _remaining;
        private bool _running;
        private bool _expired;

        public float Remaining => _remaining;
        public float Duration { get; private set; }
        public float NormalizedRemaining => Duration > 0 ? _remaining / Duration : 0f;

        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
        }

        public void StartTimer(float duration)
        {
            Duration = duration;
            _remaining = duration;
            _running = true;
            _expired = false;
        }

        public void Pause() => _running = false;
        public void Resume() => _running = true;

        private void Update()
        {
            if (!_running || _expired) return;

            float drain = Time.deltaTime;

            // Accelerate when restlessness is elevated; Max drains much faster
            if (RestlessnessManager.Instance != null)
            {
                var threshold = RestlessnessManager.Instance.CurrentThreshold;
                if (threshold == RestlessnessManager.Threshold.Max)
                    drain *= _maxRestlessnessAcceleration;
                else if (threshold >= RestlessnessManager.Threshold.High)
                    drain *= _highRestlessnessAcceleration;
            }

            _remaining -= drain;

            if (_remaining <= 0f)
            {
                _remaining = 0f;
                _running = false;
                _expired = true;
                _onTimerExpired?.Raise();
                OnExpired?.Invoke();
            }
        }
    }
}
