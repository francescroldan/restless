using UnityEngine;

namespace Restless.Dream
{
    /// <summary>
    /// Attached to the protagonist. Manages two independent zones:
    ///
    ///   Vision cone  — the rendered light cone. While a triggered entity is inside it,
    ///                  restlessness drains continuously.
    ///
    ///   Perception radius — a small circle around the player. A dormant entity only
    ///                       wakes up when it is BOTH inside the vision cone AND within
    ///                       this radius. The player has to deliberately aim at something
    ///                       nearby to activate it, not just glance across the room.
    /// </summary>
    public class EntityDetection : MonoBehaviour
    {
        [SerializeField] private float _spikePerSecond     = 8f;
        [SerializeField] private float _interruptRadius    = 1.2f;
        [SerializeField] private float _interruptMagnitude = 0.35f;

        [Header("Perception radius (activation only)")]
        [SerializeField] private float _perceptionRadius = 3f;

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

                    // Wake up only when close enough — requires deliberate aim, not a distant glance
                    if (entity.IsDormant)
                    {
                        float dist = Vector2.Distance(transform.position, entity.transform.position);
                        if (dist <= _perceptionRadius)
                            entity.Trigger();
                    }

                    // Drain restlessness while any triggered entity stays in cone
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

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Gizmos.color = new Color(0.8f, 0.4f, 1f, 0.3f);
            Gizmos.DrawWireSphere(transform.position, _perceptionRadius);
        }
#endif
    }
}
