using System.Collections.Generic;
using UnityEngine;

namespace Restless.Dream
{
    /// <summary>
    /// Spawns all dream fog entities at run start using zone-based random positioning.
    /// Replaces both EntitySpawner and MemoryPointSpawner.
    ///
    /// Distribution per run:
    ///   Threat fog       — DreamEntity (haunted) wrapped in DreamFog
    ///   Wanderer fog     — DreamEntity (inert)   wrapped in DreamFog
    ///   Visible wanderer — DreamEntity (inert),  no fog, pure atmosphere
    ///   Fragment fog     — MemoryPoint prefab    wrapped in DreamFog (minigame still required)
    /// </summary>
    public class DreamFogSpawner : MonoBehaviour
    {
        [Header("Prefabs")]
        [SerializeField] private GameObject _entityPrefab;
        [SerializeField] private GameObject _memoryPointPrefab;
        [SerializeField] private Core.GameConfig _config;

        [Header("Fragment pool")]
        [SerializeField] private System.Collections.Generic.List<MemoryFragment> _fragmentPool = new();
        [SerializeField, Range(1, 8)] private int _minFragments = 2;
        [SerializeField, Range(1, 8)] private int _maxFragments = 4;

        [Header("Spawn zones")]
        [SerializeField] private FogSpawnZone[] _zones;
        [SerializeField] private LayerMask _obstacleLayer;
        [SerializeField] private float _clearanceRadius = 0.35f;
        [SerializeField] private int   _maxAttempts     = 20;

        private void Start()
        {
            if (_entityPrefab == null)
            {
                Debug.LogWarning("[DreamFogSpawner] Entity prefab not assigned.");
                return;
            }

            var run   = Core.RunConfig.Current;
            int total = run?.entitySpawnCount ?? (_config != null ? _config.entitySpawnCount : 12);

            float tFrac = _config != null ? _config.fogThreatFraction          : 0.35f;
            float wFrac = _config != null ? _config.fogWandererFraction         : 0.30f;
            float vFrac = _config != null ? _config.fogWandererVisibleFraction  : 0.15f;

            int threatCount   = Mathf.RoundToInt(total * tFrac);
            int wandererFog   = Mathf.RoundToInt(total * wFrac);
            int wandererVis   = Mathf.RoundToInt(total * vFrac);
            int fragmentCount = _config != null ? _config.fogFragmentCount : 2;
            fragmentCount     = Mathf.Clamp(Random.Range(_minFragments, _maxFragments + 1),
                                            _minFragments, fragmentCount > 0 ? fragmentCount : _maxFragments);

            SpawnEntities(threatCount, haunted: true,  fogType: DreamFog.FogType.Threat,   withFog: true);
            SpawnEntities(wandererFog, haunted: false, fogType: DreamFog.FogType.Wanderer,  withFog: true);
            SpawnEntities(wandererVis, haunted: false, fogType: DreamFog.FogType.Wanderer,  withFog: false);
            SpawnFragments(fragmentCount);

            Debug.Log($"[DreamFogSpawner] threat×{threatCount} wanderer(fog)×{wandererFog} " +
                      $"wanderer(vis)×{wandererVis} fragment×{fragmentCount}");
        }

        private void SpawnEntities(int count, bool haunted, DreamFog.FogType fogType, bool withFog)
        {
            for (int i = 0; i < count; i++)
            {
                var pos = FindFreePosition();
                if (pos == null) { Debug.LogWarning("[DreamFogSpawner] No free position — skipping."); continue; }

                var go     = Instantiate(_entityPrefab, pos.Value, Quaternion.identity);
                var entity = go.GetComponent<DreamEntity>();
                if (entity != null) entity.SetHaunted(haunted);

                if (withFog)
                {
                    var fog = go.AddComponent<DreamFog>();
                    fog.SetType(fogType);
                }
            }
        }

        private void SpawnFragments(int count)
        {
            if (_memoryPointPrefab == null || _fragmentPool.Count == 0)
            {
                Debug.LogWarning("[DreamFogSpawner] MemoryPoint prefab or fragment pool not assigned.");
                return;
            }

            for (int i = 0; i < count; i++)
            {
                var pos = FindFreePosition();
                if (pos == null) { Debug.LogWarning("[DreamFogSpawner] No free position for fragment — skipping."); continue; }

                var go = Instantiate(_memoryPointPrefab, pos.Value, Quaternion.identity);
                var mp = go.GetComponent<MemoryPoint>();
                if (mp != null)
                    mp.AssignFragment(_fragmentPool[Random.Range(0, _fragmentPool.Count)]);

                var fog = go.AddComponent<DreamFog>();
                fog.SetType(DreamFog.FogType.Fragment);
            }
        }

        private Vector2? FindFreePosition()
        {
            if (_zones == null || _zones.Length == 0) return null;

            for (int attempt = 0; attempt < _maxAttempts; attempt++)
            {
                var zone = _zones[Random.Range(0, _zones.Length)];
                var pos  = new Vector2(
                    zone.center.x + Random.Range(-zone.halfExtents.x, zone.halfExtents.x),
                    zone.center.y + Random.Range(-zone.halfExtents.y, zone.halfExtents.y));

                if (!IsBlocked(pos))
                    return pos;
            }

            return null;
        }

        private bool IsBlocked(Vector2 pos)
        {
            var hits = Physics2D.OverlapCircleAll(pos, _clearanceRadius, _obstacleLayer);
            foreach (var hit in hits)
                if (!hit.isTrigger) return true;
            return false;
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (_zones == null) return;
            Gizmos.color = new Color(0.4f, 0.7f, 1f, 0.15f);
            foreach (var z in _zones)
                Gizmos.DrawCube(z.center, new Vector3(z.halfExtents.x * 2f, z.halfExtents.y * 2f, 0.1f));
        }
#endif
    }

    [System.Serializable]
    public struct FogSpawnZone
    {
        public Vector2 center;
        public Vector2 halfExtents;
    }
}
