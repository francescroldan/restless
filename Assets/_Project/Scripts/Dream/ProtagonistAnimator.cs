using UnityEngine;

namespace Restless.Dream
{
    /// <summary>
    /// Feeds movement speed to the Animator so it can transition idle ↔ walk.
    /// </summary>
    [RequireComponent(typeof(Animator))]
    public class ProtagonistAnimator : MonoBehaviour
    {
        private Animator     _animator;
        private Rigidbody2D  _rb;

        private static readonly int SpeedHash = Animator.StringToHash("speed");

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            _rb       = GetComponent<Rigidbody2D>();
        }

        private void Update()
        {
            if (_rb == null) return;
            _animator.SetFloat(SpeedHash, _rb.linearVelocity.magnitude);
        }
    }
}
