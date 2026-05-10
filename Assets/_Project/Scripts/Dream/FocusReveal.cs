using UnityEngine;

namespace Restless.Dream
{
    /// <summary>
    /// Elements render as dark unlit silhouettes and permanently switch to their
    /// normal lit material the first time the player's vision cone illuminates them.
    /// Uses material swap so the ghost state is visible regardless of ambient light.
    /// </summary>
    public class FocusReveal : MonoBehaviour
    {
        [Header("Ghost material (Sprite-Unlit-Default)")]
        [SerializeField] private Material _ghostMaterial;

        [Header("Ghost colour applied to the unlit material")]
        [SerializeField] private Color _ghostColor = new Color(0.18f, 0.20f, 0.28f, 1f);

        [SerializeField] private float _lerpSpeed     = 0.25f;
        [SerializeField] private float _coneHalfAngle = 55f;
        [SerializeField] private float _maxDistance   = 5f;

        private SpriteRenderer _sr;
        private Material       _originalMaterial;
        private Transform      _coneTransform;
        private bool           _revealed;
        private float          _startDelay = 1.2f;

        public bool IsRevealed => _revealed;

        private void Awake()
        {
            _sr = GetComponentInChildren<SpriteRenderer>();
            if (_sr == null) return;

            _originalMaterial = _sr.material;

            if (_ghostMaterial != null)
            {
                _sr.material = _ghostMaterial;
                _sr.color    = _ghostColor;
            }
            else
            {
                _sr.color = _ghostColor;
            }
        }

        private void Start()
        {
            var cone = FindAnyObjectByType<VisionCone>();
            if (cone != null) _coneTransform = cone.transform;
        }

        private void Update()
        {
            if (_sr == null) return;

            if (_startDelay > 0f) { _startDelay -= Time.deltaTime; return; }

            if (!_revealed && IsInConeAngle())
            {
                _revealed = true;
                _sr.material = _originalMaterial;
                _sr.color    = _ghostColor; // start lerp from ghost colour using lit material
            }

            if (_revealed)
            {
                _sr.color = Color.Lerp(_sr.color, Color.white, Time.deltaTime * _lerpSpeed);

                if (Mathf.Abs(_sr.color.r - 1f) < 0.01f)
                {
                    _sr.color = Color.white;
                    enabled   = false;
                }
            }
        }

        private bool IsInConeAngle()
        {
            if (_coneTransform == null) return false;
            Vector2 toTarget = (Vector2)(transform.position - _coneTransform.position);
            if (toTarget.magnitude > _maxDistance) return false;
            return Vector2.Angle(_coneTransform.up, toTarget) <= _coneHalfAngle;
        }
    }
}
