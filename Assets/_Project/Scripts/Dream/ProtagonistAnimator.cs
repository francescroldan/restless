using UnityEngine;

namespace Restless.Dream
{
    public class ProtagonistAnimator : MonoBehaviour
    {
        // 36 sprites: 9 rows x 4 frames, row-major (assigned in Inspector or via editor script)
        // Rows: S=0, SW=1, W=2, NW=3, N=4, NE=5, E=6, SE=7, Death=8
        [SerializeField] private Sprite[] _sprites = new Sprite[36];

        private SpriteRenderer        _sr;
        private Rigidbody2D           _rb;
        private ProtagonistController _controller;

        private enum State { Idle, Walk, Scared }
        private State _state      = State.Idle;
        private int   _dirRow     = 0;
        private int   _frame      = 0;
        private float _frameTimer = 0f;
        private float _scaredTimer;

        const int   DeathRow     = 8;
        const int   ScaredFrames = 3;
        const float WalkFps      = 10f;
        const float IdleFps      = 4f;
        const float ScaredFps    = 5f;

        // Clockwise sectors from North (0°) mapped to spritesheet rows.
        // Spritesheet is horizontally mirrored vs. convention, so E/W pairs are swapped.
        // Sectors: N=0, NE=1, E=2, SE=3, S=4, SW=5, W=6, NW=7
        // Rows:    N=4, NW=3, W=2, SW=1, S=0, SE=7, E=6, NE=5
        static readonly int[] SectorToRow = { 4, 3, 2, 1, 0, 7, 6, 5 };

        private void Awake()
        {
            _sr         = GetComponent<SpriteRenderer>();
            _rb         = GetComponent<Rigidbody2D>();
            _controller = GetComponent<ProtagonistController>();
        }

        public void TriggerScared()
        {
            _state       = State.Scared;
            _frame       = 0;
            _frameTimer  = 0f;
            _scaredTimer = ScaredFrames / ScaredFps;
        }

        private void Update()
        {
            if (_sprites == null || _sprites.Length < 36) return;

            UpdateDirection();
            UpdateState();
            AdvanceFrame();
            ApplySprite();
        }

        private void UpdateDirection()
        {
            if (_controller == null) return;
            Vector2 dir = _controller.LookDirection;
            if (dir.sqrMagnitude < 0.01f) return;

            // Atan2(x, y): 0° = North, increases clockwise
            float angle = Mathf.Atan2(dir.x, dir.y) * Mathf.Rad2Deg;
            if (angle < 0f) angle += 360f;
            int sector = Mathf.RoundToInt(angle / 45f) % 8;
            _dirRow = SectorToRow[sector];
        }

        private void UpdateState()
        {
            if (_state == State.Scared)
            {
                _scaredTimer -= Time.deltaTime;
                if (_scaredTimer <= 0f)
                    TransitionTo(State.Idle);
                return;
            }

            bool moving = _rb != null && _rb.linearVelocity.sqrMagnitude > 0.1f;
            State next = moving ? State.Walk : State.Idle;
            if (next != _state) TransitionTo(next);
        }

        private void TransitionTo(State next)
        {
            _state      = next;
            _frame      = 0;
            _frameTimer = 0f;
        }

        private void AdvanceFrame()
        {
            (int total, float fps) = _state switch
            {
                State.Walk   => (4,            WalkFps),
                State.Scared => (ScaredFrames, ScaredFps),
                _            => (2,            IdleFps),
            };

            _frameTimer += Time.deltaTime;
            if (_frameTimer >= 1f / fps)
            {
                _frameTimer -= 1f / fps;
                _frame = (_frame + 1) % total;
            }
        }

        private void ApplySprite()
        {
            int row   = _state == State.Scared ? DeathRow : _dirRow;
            int index = row * 4 + _frame;
            if (index < _sprites.Length && _sprites[index] != null)
                _sr.sprite = _sprites[index];
        }
    }
}
