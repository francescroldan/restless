using UnityEngine;

namespace Restless.Core
{
    /// <summary>
    /// Smooth camera follow with configurable deadzone and room bounds clamping.
    /// Attach to the Main Camera.
    /// </summary>
    public class CameraFollow : MonoBehaviour
    {
        [SerializeField] private float _smoothSpeed  = 5f;
        [SerializeField] private float _orthoSize    = 6f;

        [Header("Room bounds (match wall positions)")]
        [SerializeField] private float _roomHalfW = 14f;
        [SerializeField] private float _roomHalfH = 9f;

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
                // Snap immediately so there's no initial slide
                SnapToTarget();
            }
            else
            {
                Debug.LogWarning("[CameraFollow] No GameObject tagged 'Player' found.");
            }
        }

        private void LateUpdate()
        {
            if (_target == null) return;

            Vector3 desired = new Vector3(_target.position.x, _target.position.y, transform.position.z);
            Vector3 smoothed = Vector3.Lerp(transform.position, desired, _smoothSpeed * Time.deltaTime);
            transform.position = Clamp(smoothed);
        }

        private void SnapToTarget()
        {
            if (_target == null) return;
            transform.position = Clamp(new Vector3(_target.position.x, _target.position.y, transform.position.z));
        }

        private Vector3 Clamp(Vector3 pos)
        {
            float halfH = _cam.orthographicSize;
            float halfW = halfH * _cam.aspect;

            float clampedX = Mathf.Clamp(pos.x, -_roomHalfW + halfW, _roomHalfW - halfW);
            float clampedY = Mathf.Clamp(pos.y, -_roomHalfH + halfH, _roomHalfH - halfH);

            return new Vector3(clampedX, clampedY, pos.z);
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
