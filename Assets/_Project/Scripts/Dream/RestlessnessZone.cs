using UnityEngine;

namespace Restless.Dream
{
    [RequireComponent(typeof(Collider2D))]
    public class RestlessnessZone : MonoBehaviour
    {
        [Tooltip("Multiplier applied to the base restlessness rate while inside this zone.")]
        [SerializeField] private float _rateMultiplier = 1f;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;
            RestlessnessManager.Instance?.SetZoneMultiplier(_rateMultiplier);
            if (_rateMultiplier > 1f)
                DreamSFXPlayer.Instance?.PlayZoneEnter();
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;
            RestlessnessManager.Instance?.SetZoneMultiplier(1f);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = _rateMultiplier > 1f
                ? new Color(1f, 0.2f, 0.2f, 0.25f)
                : new Color(0.2f, 1f, 0.2f, 0.25f);
            Gizmos.DrawCube(transform.position, GetComponent<Collider2D>().bounds.size);
        }
    }
}
