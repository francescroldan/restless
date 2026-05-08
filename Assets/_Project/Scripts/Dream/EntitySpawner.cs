using System.Collections.Generic;
using UnityEngine;

namespace Restless.Dream
{
    /// <summary>
    /// At dream start, instantiates a randomised set of presences from the pool of
    /// spawn points. A configurable fraction are haunted; the rest are inert decor.
    /// Spawn points that overlap a wall or pillar collider are automatically skipped.
    /// </summary>
    public class EntitySpawner : MonoBehaviour
    {
        [SerializeField] private GameObject  _entityPrefab;
        [SerializeField] private Transform[] _spawnPoints;
        [SerializeField] private LayerMask   _obstacleLayer = ~0;   // all layers by default; restrict in Inspector
        [SerializeField] private float       _clearanceRadius = 0.3f;

        [Header("Fallback (used when RunConfig is absent)")]
        [SerializeField] private int         _spawnCount      = 12;
        [SerializeField] [Range(0f, 1f)]
        private float                        _hauntedFraction = 0.4f;

        private void Start()
        {
            if (_entityPrefab == null || _spawnPoints == null || _spawnPoints.Length == 0)
            {
                Debug.LogWarning("[EntitySpawner] Prefab or spawn points not assigned.");
                return;
            }

            var   run   = Core.RunConfig.Current;
            int   count = run != null ? run.entitySpawnCount     : _spawnCount;
            float frac  = run != null ? run.entityHauntedFraction : _hauntedFraction;

            // Build a shuffled list of valid (obstacle-free) spawn points
            var valid = new List<int>();
            for (int i = 0; i < _spawnPoints.Length; i++)
            {
                if (_spawnPoints[i] == null) continue;
                if (!IsBlocked(_spawnPoints[i].position))
                    valid.Add(i);
            }

            for (int i = valid.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (valid[i], valid[j]) = (valid[j], valid[i]);
            }

            count = Mathf.Min(count, valid.Count);
            int hauntedCount = Mathf.RoundToInt(count * frac);

            for (int i = 0; i < count; i++)
            {
                var point  = _spawnPoints[valid[i]];
                var go     = Instantiate(_entityPrefab, point.position, Quaternion.identity);
                var entity = go.GetComponent<DreamEntity>();
                if (entity != null)
                    entity.SetHaunted(i < hauntedCount);
            }

            Debug.Log("[EntitySpawner] " + valid.Count + " clear points → spawned " +
                      count + " presences (" + hauntedCount + " haunted).");
        }

        private bool IsBlocked(Vector2 pos)
        {
            var hit = Physics2D.OverlapCircle(pos, _clearanceRadius, _obstacleLayer);
            return hit != null;
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (_spawnPoints == null) return;
            foreach (var pt in _spawnPoints)
            {
                if (pt == null) continue;
                bool blocked = IsBlocked(pt.position);
                Gizmos.color = blocked
                    ? new Color(1f, 0.2f, 0.2f, 0.6f)
                    : new Color(0.2f, 1f, 0.4f, 0.6f);
                Gizmos.DrawWireSphere(pt.position, _clearanceRadius);
            }
        }
#endif
    }
}
