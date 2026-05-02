using UnityEngine;

namespace Restless.Dream
{
    /// <summary>
    /// Pulsing glow ring around a MemoryPoint for prototype visibility.
    /// Driven purely by sin wave — no DOTween dependency.
    /// </summary>
    [RequireComponent(typeof(MemoryPoint))]
    public class MemoryPointVisual : MonoBehaviour
    {
        [SerializeField] private float _pulseSpeed = 2f;
        [SerializeField] private float _minScale = 0.7f;
        [SerializeField] private float _maxScale = 1.1f;
        [SerializeField] private Color _availableColor  = new Color(0.2f, 1f, 0.85f, 1f);
        [SerializeField] private Color _extractingColor = new Color(1f, 0.9f, 0.2f, 1f);
        [SerializeField] private Color _collectedColor  = new Color(0.3f, 0.3f, 0.3f, 0.4f);
        [SerializeField] private Color _failedColor     = new Color(1f, 0.2f, 0.2f, 0.6f);

        private MemoryPoint _mp;
        private SpriteRenderer _sr;
        private float _time;

        private void Awake()
        {
            _mp = GetComponent<MemoryPoint>();
            _sr = GetComponent<SpriteRenderer>();
        }

        private void Update()
        {
            if (_mp.CurrentState == MemoryPoint.State.Collected ||
                _mp.CurrentState == MemoryPoint.State.Failed)
            {
                _sr.color = _mp.CurrentState == MemoryPoint.State.Collected
                    ? _collectedColor : _failedColor;
                transform.localScale = Vector3.one * 0.6f;
                return;
            }

            _time += Time.deltaTime * _pulseSpeed;
            float t = (Mathf.Sin(_time) + 1f) * 0.5f;
            float scale = Mathf.Lerp(_minScale, _maxScale, t);
            transform.localScale = Vector3.one * scale;

            _sr.color = _mp.CurrentState == MemoryPoint.State.Extracting
                ? _extractingColor : _availableColor;
        }
    }
}
