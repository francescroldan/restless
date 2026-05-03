using UnityEngine;

namespace Restless.Dream
{
    /// <summary>
    /// Simple 2-frame idle bob for the Sage encounter sprite.
    /// No Animator needed — swaps sprites at a fixed interval.
    /// </summary>
    public class SageIdleAnimation : MonoBehaviour
    {
        [SerializeField] private Sprite _frameA;
        [SerializeField] private Sprite _frameB;
        [SerializeField] private float  _interval = 0.5f;

        private SpriteRenderer _sr;
        private bool _onA = true;
        private float _timer;

        private void Awake()
        {
            _sr = GetComponent<SpriteRenderer>();
        }

        private void Update()
        {
            if (_frameA == null || _frameB == null) return;
            _timer += Time.deltaTime;
            if (_timer >= _interval)
            {
                _timer -= _interval;
                _onA = !_onA;
                _sr.sprite = _onA ? _frameA : _frameB;
            }
        }
    }
}
