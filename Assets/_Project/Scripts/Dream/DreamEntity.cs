using UnityEngine;

namespace Restless.Dream
{
    /// <summary>
    /// Ghostly presence. Stays dormant until the player looks at it within perception range.
    /// If haunted, it drifts in a random direction and fades out.
    /// Inert entities never react — visually identical to haunted ones.
    /// Movement ignores physics so presences never get stuck on walls.
    /// </summary>
    public class DreamEntity : MonoBehaviour
    {
        [Header("Behaviour")]
        [SerializeField] private bool  _isHaunted    = true;
        [SerializeField] private float _moveSpeed    = 1.5f;
        [SerializeField] private float _moveDuration = 1.2f;
        [SerializeField] private float _fadeDuration = 0.8f;

        private enum State { Dormant, Moving, Vanishing }

        private SpriteRenderer _sr;
        private State          _state = State.Dormant;
        private Vector2        _moveDir;
        private float          _moveTimer;
        private float          _fadeTimer;

        public bool IsDormant => _state == State.Dormant;
        public bool IsHaunted => _isHaunted;

        public void SetHaunted(bool haunted) => _isHaunted = haunted;

        private void Awake()
        {
            _sr = GetComponentInChildren<SpriteRenderer>();

            // Disable physics collision — presences are visual only, must not get stuck on walls
            var rb = GetComponent<Rigidbody2D>();
            if (rb != null) Destroy(rb);
            var col = GetComponent<Collider2D>();
            if (col != null) col.enabled = false;
        }

        public void Trigger()
        {
            if (_state != State.Dormant || !_isHaunted) return;

            _state     = State.Moving;
            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            _moveDir   = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            _moveTimer = _moveDuration;
        }

        private void Update()
        {
            switch (_state)
            {
                case State.Moving:
                    float speed = Core.RunConfig.Current?.entitySpeed ?? _moveSpeed;
                    transform.position += (Vector3)(_moveDir * speed * Time.deltaTime);
                    _moveTimer -= Time.deltaTime;
                    if (_moveTimer <= 0f)
                    {
                        _state     = State.Vanishing;
                        _fadeTimer = _fadeDuration;
                    }
                    break;

                case State.Vanishing:
                    _fadeTimer -= Time.deltaTime;
                    if (_sr != null)
                    {
                        var c = _sr.color;
                        c.a = Mathf.Clamp01(_fadeTimer / _fadeDuration);
                        _sr.color = c;
                    }
                    if (_fadeTimer <= 0f)
                        gameObject.SetActive(false);
                    break;
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = _isHaunted
                ? new Color(1f, 0.2f, 0.2f, 0.5f)
                : new Color(0.4f, 0.4f, 0.8f, 0.3f);
            Gizmos.DrawWireSphere(transform.position, 0.25f);
        }
    }
}
