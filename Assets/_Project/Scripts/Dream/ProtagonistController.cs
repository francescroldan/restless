using UnityEngine;
using UnityEngine.InputSystem;

namespace Restless.Dream
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class ProtagonistController : MonoBehaviour
    {
        [SerializeField] private float _walkSpeed = 3f;
        [SerializeField] private float _runSpeed = 5.5f;
        [SerializeField] private float _lookLerpSpeed = 8f;

        private Rigidbody2D _rb;
        private PlayerInput _playerInput;
        private InputAction _moveAction;
        private InputAction _lookAction;
        private InputAction _runAction;

        private Vector2 _moveInput;
        private Vector2 _lookDirection = Vector2.up;
        private bool _isRunning;

        public Vector2 LookDirection => _lookDirection;
        public bool IsMoving => _moveInput.sqrMagnitude > 0.01f;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _rb.gravityScale = 0f;
            _rb.freezeRotation = true;

            _playerInput = GetComponent<PlayerInput>();
            _moveAction = _playerInput.actions["Player/Move"];
            _lookAction = _playerInput.actions["Player/Look"];
            _runAction = _playerInput.actions["Player/Run"];
        }

        private void Update()
        {
            _moveInput = _moveAction.ReadValue<Vector2>();
            _isRunning = _runAction.IsPressed();
            UpdateLookDirection();
        }

        private void FixedUpdate()
        {
            float speed = _isRunning ? _runSpeed : _walkSpeed;
            _rb.linearVelocity = _moveInput.normalized * speed;
        }

        private void UpdateLookDirection()
        {
            // Gamepad: right stick overrides look direction
            Vector2 stickLook = _lookAction.ReadValue<Vector2>();
            if (stickLook.sqrMagnitude > 0.25f)
            {
                _lookDirection = Vector2.Lerp(_lookDirection, stickLook.normalized, Time.deltaTime * _lookLerpSpeed);
                return;
            }

            // Keyboard+Mouse: look towards mouse position
            if (Mouse.current != null)
            {
                Vector2 mouseScreen = Mouse.current.position.ReadValue();
                Vector2 mouseWorld = Camera.main.ScreenToWorldPoint(mouseScreen);
                Vector2 toMouse = mouseWorld - (Vector2)transform.position;
                if (toMouse.sqrMagnitude > 0.1f)
                    _lookDirection = Vector2.Lerp(_lookDirection, toMouse.normalized, Time.deltaTime * _lookLerpSpeed);
                return;
            }

            // Fallback: look in movement direction
            if (_moveInput.sqrMagnitude > 0.01f)
                _lookDirection = Vector2.Lerp(_lookDirection, _moveInput.normalized, Time.deltaTime * _lookLerpSpeed);
        }
    }
}
