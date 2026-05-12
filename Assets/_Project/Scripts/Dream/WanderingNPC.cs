using System.Collections;
using UnityEngine;

namespace Restless.Dream
{
    /// <summary>
    /// Inoffensive dream inhabitant that wanders between random waypoints within a
    /// radius of its spawn position. Does not interact with the player or affect
    /// restlessness. Can start hidden inside a DreamPresence.
    /// </summary>
    public class WanderingNPC : MonoBehaviour
    {
        [SerializeField] private float _moveSpeed      = 0.8f;
        [SerializeField] private float _wanderRadius   = 3f;
        [SerializeField] private float _waitAtWaypoint = 1.5f;

        private SpriteRenderer _sr;
        private Vector3        _origin;
        private Vector3        _target;
        private bool           _waiting;

        private void Awake()
        {
            _sr = GetComponentInChildren<SpriteRenderer>();
        }

        private void OnEnable()
        {
            _origin  = transform.position;
            _target  = PickTarget();
            _waiting = false;
        }

        private void Update()
        {
            if (_waiting) return;

            float speed = Core.RunConfig.Current?.wandererSpeed ?? _moveSpeed;
            transform.position = Vector3.MoveTowards(transform.position, _target, speed * Time.deltaTime);

            if (_sr != null)
            {
                float dx = _target.x - transform.position.x;
                if (Mathf.Abs(dx) > 0.05f)
                    _sr.flipX = dx < 0f;
            }

            if (Vector3.Distance(transform.position, _target) < 0.08f)
                StartCoroutine(WaitThenMove());
        }

        private IEnumerator WaitThenMove()
        {
            _waiting = true;
            float wait = Core.RunConfig.Current?.wandererWaitTime ?? _waitAtWaypoint;
            yield return new WaitForSeconds(wait);
            _target  = PickTarget();
            _waiting = false;
        }

        private Vector3 PickTarget()
        {
            float r   = Core.RunConfig.Current?.wandererRadius ?? _wanderRadius;
            float ang = Random.Range(0f, Mathf.PI * 2f);
            float dst = Random.Range(r * 0.3f, r);
            return _origin + new Vector3(Mathf.Cos(ang) * dst, Mathf.Sin(ang) * dst, 0f);
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Gizmos.color = new Color(0.4f, 0.8f, 0.6f, 0.25f);
            Gizmos.DrawWireSphere(transform.position, 0.2f);
        }
#endif
    }
}
