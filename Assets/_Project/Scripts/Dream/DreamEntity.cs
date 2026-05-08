using UnityEngine;

namespace Restless.Dream
{
    /// <summary>
    /// Ghostly presence. Stays dormant until illuminated by the player's vision cone.
    /// If active (haunted), it moves in a random direction and fades out.
    /// Inert entities (isActive=false) never react — visually identical to active ones.
    /// Restlessness drains continuously while the entity is inside the cone (handled by EntityDetection).
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(SpriteRenderer))]
    public class DreamEntity : MonoBehaviour
    {
        [Header("Behaviour")]
        [SerializeField] private bool  _isHaunted    = true;
        [SerializeField] private float _moveSpeed    = 1.5f;
        [SerializeField] private float _moveDuration = 1.2f;
        [SerializeField] private float _fadeDuration = 0.8f;

        private enum State { Dormant, Moving, Vanishing }

        private Rigidbody2D    _rb;
        private SpriteRenderer _sr;
        private State          _state = State.Dormant;
        private Vector2        _moveDir;
        private float          _moveTimer;
        private float          _fadeTimer;

        public bool IsDormant  => _state == State.Dormant;
        public bool IsHaunted  => _isHaunted;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _rb.gravityScale  = 0f;
            _rb.freezeRotation = true;
            _sr = GetComponent<SpriteRenderer>();
        }

        /// <summary>Called by EntityDetection on first cone contact.</summary>
        public void Trigger()
        {
            if (_state != State.Dormant || !_isHaunted) return;

            _state     = State.Moving;
            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            _moveDir   = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            _moveTimer = _moveDuration;
        }

        private void FixedUpdate()
        {
            if (_state != State.Moving) return;

            float speed = Core.RunConfig.Current?.entitySpeed ?? _moveSpeed;
            _rb.MovePosition(_rb.position + _moveDir * speed * Time.fixedDeltaTime);
        }

        private void Update()
        {
            switch (_state)
            {
                case State.Moving:
                    _moveTimer -= Time.deltaTime;
                    if (_moveTimer <= 0f)
                    {
                        _rb.linearVelocity = Vector2.zero;
                        _state     = State.Vanishing;
                        _fadeTimer = _fadeDuration;
                    }
                    break;

                case State.Vanishing:
                    _fadeTimer -= Time.deltaTime;
                    float alpha = Mathf.Clamp01(_fadeTimer / _fadeDuration);
                    var c = _sr.color;
                    c.a = alpha;
                    _sr.color = c;
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
