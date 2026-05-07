using UnityEngine;

namespace Restless.Dream
{
    /// <summary>
    /// Attached to the protagonist. Checks each frame whether any DreamEntity is inside
    /// the vision cone and applies restlessness spikes accordingly. Also interrupts any
    /// active RetentionMinigame when an entity enters the interrupt radius.
    /// </summary>
    public class EntityDetection : MonoBehaviour
    {
        [SerializeField] private float _spikePerSecond = 8f;
        [SerializeField] private float _interruptRadius = 1.2f;
        [SerializeField] private float _interruptMagnitude = 0.35f;

        private VisionCone    _visionCone;
        private DreamEntity[] _entities;
        private float         _interruptCooldown;
        private bool          _wasDetecting;
        private float         _detectionBuzzCooldown;

        private void Start()
        {
            _visionCone = GetComponentInChildren<VisionCone>();
            _entities = FindObjectsByType<DreamEntity>(FindObjectsSortMode.None);
        }

        private void Update()
        {
            if (_interruptCooldown     > 0f) _interruptCooldown     -= Time.deltaTime;
            if (_detectionBuzzCooldown > 0f) _detectionBuzzCooldown -= Time.deltaTime;

            bool anyInCone = false;

            foreach (var entity in _entities)
            {
                if (entity == null) continue;

                bool inCone = _visionCone != null && _visionCone.ContainsPoint(entity.transform.position);

                if (inCone)
                {
                    anyInCone = true;
                    RestlessnessManager.Instance?.AddSpike(_spikePerSecond * Time.deltaTime);
                }

                // Interrupt active RetentionMinigame when entity is very close
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

            // Rising edge: entity just entered cone — buzz + sound
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
            // MemoryPoints in the scene own the minigame components
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
