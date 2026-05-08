using System.Collections.Generic;
using UnityEngine;

namespace Restless.Dream
{
    /// <summary>
    /// At dream start, instantiates a randomised set of presences from the pool of
    /// spawn points. A configurable fraction are haunted; the rest are inert decor.
    /// </summary>
    public class EntitySpawner : MonoBehaviour
    {
        [SerializeField] private GameObject  _entityPrefab;
        [SerializeField] private Transform[] _spawnPoints;

        [Header("Fallback (used when RunConfig is absent)")]
        [SerializeField] private int   _spawnCount        = 12;
        [SerializeField] [Range(0f,1f)] private float _hauntedFraction = 0.4f;

        private void Start()
        {
            if (_entityPrefab == null || _spawnPoints == null || _spawnPoints.Length == 0)
            {
                Debug.LogWarning("[EntitySpawner] Prefab or spawn points not assigned.");
                return;
            }

            var run    = Core.RunConfig.Current;
            int count  = run != null ? run.entitySpawnCount    : _spawnCount;
            float frac = run != null ? run.entityHauntedFraction : _hauntedFraction;

            count = Mathf.Min(count, _spawnPoints.Length);
            int hauntedCount = Mathf.RoundToInt(count * frac);

            // Shuffle spawn points
            var indices = new List<int>(_spawnPoints.Length);
            for (int i = 0; i < _spawnPoints.Length; i++) indices.Add(i);
            for (int i = indices.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (indices[i], indices[j]) = (indices[j], indices[i]);
            }

            for (int i = 0; i < count; i++)
            {
                var point  = _spawnPoints[indices[i]];
                var go     = Instantiate(_entityPrefab, point.position, Quaternion.identity);
                var entity = go.GetComponent<DreamEntity>();
                if (entity != null)
                    entity.SetHaunted(i < hauntedCount);
            }

            Debug.Log("[EntitySpawner] Spawned " + count + " presences (" + hauntedCount + " haunted).");
        }
    }
}
