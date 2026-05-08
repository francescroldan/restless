using UnityEngine;

namespace Restless.Dream
{
    /// <summary>
    /// Attached to the protagonist. Each frame checks whether any DreamEntity is inside
    /// the vision cone.
    ///
    /// Activation uses a narrower focused zone (activationRange + activationHalfAngle)
    /// so entities only wake up when the player looks at them directly and up close —
    /// not when they appear at the edge of the peripheral cone.
    ///
    /// Restlessness drains continuously while an already-triggered entity is anywhere
    /// inside the full cone.
    /// </summary>
    public class EntityDetection : MonoBehaviour
    {
        [SerializeField] private float _spikePerSecond     = 8f;
        [SerializeField] private float _interruptRadius    = 1.2f;
        [SerializeField] private float _interruptMagnitude = 0.35f;

        [Header("Focused activation zone (narrower than full cone)")]
        [SerializeField] private float _activationRange     = 3.5f;
        [SerializeField] private float _activationHalfAngle = 30f;   // degrees from cone centre

        private VisionCone    _visionCone;
        private DreamEntity[] _entities;
        private float         _interruptCooldown;
        private bool          _wasDetecting;
        private float         _detectionBuzzCooldown;

        private void Start()
        {
            _visionCone = GetComponentInChildren<VisionCone>();
        }

        private void Update()
        {
            if (_entities == null || _entities.Length == 0)
                _entities = FindObjectsByType<DreamEntity>(FindObjectsSortMode.None);

            if (_interruptCooldown     > 0f) _interruptCooldown     -= Time.deltaTime;
            if (_detectionBuzzCooldown > 0f) _detectionBuzzCooldown -= Time.deltaTime;

            bool anyInCone = false;

            foreach (var entity in _entities)
            {
                if (entity == null || !entity.gameObject.activeInHierarchy) continue;

                bool inCone = _visionCone != null && _visionCone.ContainsPoint(entity.transform.position);

                if (inCone)
                {
                    anyInCone = true;

                    // Only wake up dormant entities when looked at directly and up close
                    if (entity.IsDormant && IsFocused(entity.transform.position))
                        entity.Trigger();

                    // Restlessness drains while active entity is anywhere in cone
                    if (!entity.IsDormant)
                        RestlessnessManager.Instance?.AddSpike(_spikePerSecond * Time.deltaTime);
                }

                if (_interruptCooldown <= 0f)
                {
                    float dist = Vector2.Distance(transform.position, entity.transform.position);
                    if (dist <= _interruptRadius)
                    {
                        var retention = FindAnyRetentionMinigame();
                        if (retention != null && retention.IsActive)
                        {
                            retention.ApplyInterruption(_interruptMagnitude);
                            _interruptCooldown = 0.5f;
                        }
                    }
                }
            }

            if (anyInCone && !_wasDetecting && _detectionBuzzCooldown <= 0f)
            {
                DreamSFXPlayer.Instance?.PlayEntityDetected();
                RestlessnessVisualFX.Instance?.TriggerDetectionBuzz();
                _detectionBuzzCooldown = 1.5f;
            }
            _wasDetecting = anyInCone;
        }

        /// <summary>
        /// Returns true if worldPos is within the focused activation zone:
        /// close enough AND near the centre of the vision cone.
        /// </summary>
        private bool IsFocused(Vector3 worldPos)
        {
            if (_visionCone == null) return false;

            Vector2 toTarget = (Vector2)(worldPos - _visionCone.transform.position);
            if (toTarget.magnitude > _activationRange) return false;

            float angle = Vector2.Angle(_visionCone.transform.up, toTarget);
            return angle <= _activationHalfAngle;
        }

        private RetentionMinigame FindAnyRetentionMinigame()
        {
            var memoryPoints = FindObjectsByType<MemoryPoint>(FindObjectsSortMode.None);
            foreach (var mp in memoryPoints)
            {
                var retention = mp.GetComponent<RetentionMinigame>();
                if (retention != null) return retention;
            }
            return null;
        }
    }
}
