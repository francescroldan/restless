using UnityEngine;

namespace Restless.Core
{
    /// <summary>
    /// Smooth camera follow. Attach to the Main Camera.
    /// Bounds clamping is intentionally removed — procedural rooms have dynamic
    /// layouts, and wall colliders are the authority on player containment.
    /// </summary>
    public class CameraFollow : MonoBehaviour
    {
        [SerializeField] private float _smoothSpeed = 5f;
        [SerializeField] private float _orthoSize   = 6f;

        private Transform _target;
        private Camera    _cam;

        private void Awake()
        {
            _cam = GetComponent<Camera>();
            _cam.orthographicSize = _orthoSize;
        }

        private void Start()
        {
            var player = GameObject.FindWithTag("Player");
            if (player != null)
            {
                _target = player.transform;
                SnapToTarget();
            }
            else
            {
                Debug.LogWarning("[CameraFollow] No GameObject tagged 'Player' found.");
            }
        }

        public void SetTarget(Transform t)
        {
            _target = t;
            SnapToTarget();
        }

        private void LateUpdate()
        {
            if (_target == null) return;
            Vector3 desired = new Vector3(_target.position.x, _target.position.y, transform.position.z);
            transform.position = Vector3.Lerp(transform.position, desired, _smoothSpeed * Time.deltaTime);
        }

        private void SnapToTarget()
        {
            if (_target == null) return;
            transform.position = new Vector3(_target.position.x, _target.position.y, transform.position.z);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            var cam = GetComponent<Camera>();
            if (cam != null) cam.orthographicSize = _orthoSize;
        }
#endif
    }
}
