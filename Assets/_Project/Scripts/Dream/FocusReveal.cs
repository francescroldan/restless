using UnityEngine;

namespace Restless.Dream
{
    /// <summary>
    /// Attach to any interactive element (entities, fragments, allies).
    /// Outside the vision cone the sprite is ghostly (faint, desaturated).
    /// Inside the cone it materialises to full opacity and colour.
    /// </summary>
    public class FocusReveal : MonoBehaviour
    {
        [Header("Ghost state (outside cone)")]
        [SerializeField] private Color _ghostColor   = new Color(0.55f, 0.55f, 0.65f, 0.18f);

        [Header("Present state (inside cone)")]
        [SerializeField] private Color _presentColor = Color.white;

        [SerializeField] private float _lerpSpeed = 4f;

        private SpriteRenderer _sr;
        private VisionCone     _visionCone;

        private void Awake()
        {
            _sr = GetComponentInChildren<SpriteRenderer>();
            if (_sr != null)
                _sr.color = _ghostColor;
        }

        private void Start()
        {
            _visionCone = FindFirstObjectByType<VisionCone>();
        }

        private void Update()
        {
            if (_sr == null || _visionCone == null) return;

            bool inCone = _visionCone.ContainsPoint(transform.position);
            Color target = inCone ? _presentColor : _ghostColor;
            _sr.color = Color.Lerp(_sr.color, target, Time.deltaTime * _lerpSpeed);
        }
    }
}
