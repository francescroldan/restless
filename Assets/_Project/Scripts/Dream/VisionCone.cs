using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Restless.Dream
{
    /// <summary>
    /// Controls a Point Light2D configured as a directional cone.
    /// Setup: attach a Light2D (Point type) to this GameObject.
    /// In the Inspector set Inner Angle ~80, Outer Angle ~110, Outer Radius ~8.
    /// Add a Global Light2D at intensity 0.05 to the scene for ambient darkness.
    /// Add Shadow Caster 2D to all wall tilemaps/colliders.
    /// </summary>
    [RequireComponent(typeof(Light2D))]
    public class VisionCone : MonoBehaviour
    {
        [SerializeField] private float _range = 8f;
        [SerializeField] private float _outerAngle = 110f;
        [SerializeField] private float _minVisibleRadius = 1.2f;

        private Light2D _light;
        private ProtagonistController _protagonist;

        public bool IsVisible => _light.enabled;

        private void Awake()
        {
            _light = GetComponent<Light2D>();
            _protagonist = GetComponentInParent<ProtagonistController>();
        }

        private void LateUpdate()
        {
            if (_protagonist == null) return;
            SetDirection(_protagonist.LookDirection);
        }

        public void SetDirection(Vector2 direction)
        {
            if (direction.sqrMagnitude < 0.01f) return;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }

        /// <summary>
        /// Freezes the cone in place (during minigame extraction).
        /// </summary>
        public void Freeze() => enabled = false;

        /// <summary>
        /// Resumes cone tracking.
        /// </summary>
        public void Unfreeze() => enabled = true;

        /// <summary>
        /// Returns true if worldPosition is inside the cone's angle and range.
        /// Used by EntityDetection without needing a physics query.
        /// </summary>
        public bool ContainsPoint(Vector3 worldPosition)
        {
            Vector2 toTarget = (Vector2)(worldPosition - transform.position);
            float distance = toTarget.magnitude;

            if (distance <= _minVisibleRadius) return true;
            if (distance > _range) return false;

            // transform.up is the cone's forward direction after rotation
            float angle = Vector2.Angle(transform.up, toTarget);
            return angle <= _outerAngle * 0.5f;
        }
    }
}
