using UnityEngine;

namespace Restless.Dream
{
    /// <summary>
    /// Waypoint-patrol entity. Moves between assigned waypoints at constant speed.
    /// EntityDetection handles restlessness spikes when spotted by the player's cone.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class DreamEntity : MonoBehaviour
    {
        [SerializeField] private Transform[] _waypoints;
        [SerializeField] private float _speed = 1.8f;
        [SerializeField] private float _waypointReachThreshold = 0.1f;

        private Rigidbody2D _rb;
        private int _currentWaypoint;
        private bool _patrolPaused;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _rb.gravityScale = 0f;
            _rb.freezeRotation = true;
        }

        private void FixedUpdate()
        {
            if (_patrolPaused || _waypoints == null || _waypoints.Length == 0) return;

            var   run       = Core.RunConfig.Current;
            float speed     = run?.entitySpeed             ?? _speed;
            float threshold = run?.entityWaypointThreshold ?? _waypointReachThreshold;

            Transform target    = _waypoints[_currentWaypoint];
            Vector2   direction = ((Vector2)target.position - _rb.position).normalized;
            float     distance  = Vector2.Distance(_rb.position, target.position);

            if (distance <= threshold)
                _currentWaypoint = (_currentWaypoint + 1) % _waypoints.Length;
            else
                _rb.MovePosition(_rb.position + direction * speed * Time.fixedDeltaTime);
        }

        public void PausePatrol() => _patrolPaused = true;
        public void ResumePatrol() => _patrolPaused = false;

        private void OnDrawGizmos()
        {
            if (_waypoints == null || _waypoints.Length < 2) return;
            Gizmos.color = new Color(1f, 0.3f, 0.3f, 0.6f);
            for (int i = 0; i < _waypoints.Length; i++)
            {
                if (_waypoints[i] == null) continue;
                Gizmos.DrawSphere(_waypoints[i].position, 0.15f);
                var next = _waypoints[(i + 1) % _waypoints.Length];
                if (next != null)
                    Gizmos.DrawLine(_waypoints[i].position, next.position);
            }
        }
    }
}
