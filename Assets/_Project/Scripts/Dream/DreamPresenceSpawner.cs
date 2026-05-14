using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Restless.Dream.Procedural;

namespace Restless.Dream
{
    /// <summary>
    /// Replaces DreamFogSpawner. Spawns spectral presences at run start using
    /// zone-based random positioning. Every presence begins in the Spectral state
    /// and materialises only when the player observes it long enough.
    ///
    /// Distribution per run:
    ///   Threat     — DreamPresence(Threat)    → collapses into a haunted DreamEntity
    ///   Wanderer   — DreamPresence(Wanderer)  → collapses into an inert DreamEntity
    ///   Visible    — plain DreamEntity (no presence), atmosphere only
    ///   Fragment   — DreamPresence(Fragment)  → collapses into an interactable MemoryPoint
    ///   Undefined  — DreamPresence(Undefined) → type decided at collapse by restlessness
    /// </summary>
    public class DreamPresenceSpawner : MonoBehaviour
    {
        [Header("Prefabs")]
        [SerializeField] private GameObject      _entityPrefab;
        [SerializeField] private GameObject      _memoryPointPrefab;
        [SerializeField] private Core.GameConfig _config;

        [Header("Fragment pool")]
        [SerializeField] private List<MemoryFragment> _fragmentPool = new();
        [SerializeField, Range(1, 8)] private int _minFragments = 2;
        [SerializeField, Range(1, 8)] private int _maxFragments = 4;

        [Header("Spawn zones (static fallback)")]
        [SerializeField] private FogSpawnZone[] _zones;
        [SerializeField] private Tilemap        _floorTilemap;
        [SerializeField] private LayerMask      _obstacleLayer;
        [SerializeField] private float          _clearanceRadius = 0.5f;
        [SerializeField] private int            _maxAttempts     = 20;

        // Set by DreamSceneBootstrap when procedural generation is active
        private IReadOnlyList<RoomController> _rooms;
        private RunGraph                      _graph;

        public void SetRooms(IReadOnlyList<RoomController> rooms, RunGraph graph = null)
        {
            _rooms = rooms;
            _graph = graph;
        }

        private void Start()
        {
            if (_entityPrefab == null)
            {
                Debug.LogWarning("[DreamPresenceSpawner] Entity prefab not assigned.");
                return;
            }

            var run   = Core.RunConfig.Current;
            int total = run?.entitySpawnCount ?? (_config != null ? _config.entitySpawnCount : 12);

            float tFrac = _config != null ? _config.fogThreatFraction          : 0.35f;
            float wFrac = _config != null ? _config.fogWandererFraction         : 0.30f;
            float vFrac = _config != null ? _config.fogWandererVisibleFraction  : 0.15f;
            float uFrac = _config != null ? _config.presenceUndefinedFraction   : 0.20f;

            int threatCount    = Mathf.RoundToInt(total * tFrac);
            int wandererSpec   = Mathf.RoundToInt(total * wFrac);
            int wandererVis    = Mathf.RoundToInt(total * vFrac);
            int undefinedCount = Mathf.RoundToInt(total * uFrac);
            int fragmentCount  = _config != null ? _config.fogFragmentCount : 2;
            fragmentCount      = Mathf.Clamp(Random.Range(_minFragments, _maxFragments + 1),
                                             _minFragments, fragmentCount > 0 ? fragmentCount : _maxFragments);

            SpawnPresences(threatCount,    DreamPresence.PresenceType.Threat,    withPresence: true,  requiresThreat: true);
            SpawnPresences(wandererSpec,   DreamPresence.PresenceType.Wanderer,  withPresence: true);
            SpawnPresences(wandererVis,    DreamPresence.PresenceType.Wanderer,  withPresence: false);
            SpawnPresences(undefinedCount, DreamPresence.PresenceType.Undefined, withPresence: true,  requiresThreat: true);
            SpawnFragments(fragmentCount);

            Debug.Log($"[DreamPresenceSpawner] threat×{threatCount} wanderer(spec)×{wandererSpec} " +
                      $"wanderer(vis)×{wandererVis} undefined×{undefinedCount} fragment×{fragmentCount}");
        }

        private void SpawnPresences(int count, DreamPresence.PresenceType type, bool withPresence,
                                    bool requiresThreat = false)
        {
            for (int i = 0; i < count; i++)
            {
                var pos = FindFreePosition(requiresThreat: requiresThreat);
                if (pos == null) { Debug.LogWarning("[DreamPresenceSpawner] No free position — skipping."); continue; }

                var go = Instantiate(_entityPrefab, pos.Value, Quaternion.identity);

                if (withPresence)
                {
                    var presence = go.AddComponent<DreamPresence>();
                    presence.SetType(type);
                }
                else
                {
                    // Visible from start — just configure the entity directly
                    var entity = go.GetComponent<DreamEntity>();
                    if (entity != null) entity.SetHaunted(false);
                }
            }
        }

        private void SpawnFragments(int count)
        {
            if (_memoryPointPrefab == null || _fragmentPool.Count == 0)
            {
                Debug.LogWarning("[DreamPresenceSpawner] MemoryPoint prefab or fragment pool not assigned.");
                return;
            }

            for (int i = 0; i < count; i++)
            {
                // First fragment is guaranteed to land in a Memory-type room.
                // Subsequent fragments use any room that supports fragments.
                bool forceMemory = (i == 0);
                var pos = forceMemory
                    ? (FindFreePosition(requiresFragment: true, requiresMemoryRoom: true)
                       ?? FindFreePosition(requiresFragment: true))
                    : FindFreePosition(requiresFragment: true);

                if (pos == null) { Debug.LogWarning("[DreamPresenceSpawner] No free position for fragment — skipping."); continue; }

                var go       = Instantiate(_memoryPointPrefab, pos.Value, Quaternion.identity);
                var presence = go.AddComponent<DreamPresence>();
                presence.SetType(DreamPresence.PresenceType.Fragment);

                var frag = _fragmentPool[Random.Range(0, _fragmentPool.Count)];
                presence.AssignFragment(frag);
            }
        }

        private Vector2? FindFreePosition(bool requiresFragment  = false,
                                          bool requiresThreat   = false,
                                          bool requiresMemoryRoom = false)
        {
            // Procedural mode — use room spawn bounds
            if (_rooms != null && _rooms.Count > 0)
            {
                // Build weighted candidate list; threats prefer high-danger rooms
                var candidates = new List<(RoomController room, float weight)>(_rooms.Count);
                foreach (var room in _rooms)
                {
                    if (room.Definition == null) continue;
                    if (requiresFragment   && !room.Definition.supportsFragments) continue;
                    if (requiresThreat     && !room.Definition.supportsThreats)   continue;
                    if (requiresMemoryRoom && !IsMemoryRoom(room))                continue;
                    float weight = requiresThreat ? Mathf.Max(0.05f, room.Definition.dangerLevel) : 1f;
                    candidates.Add((room, weight));
                }

                if (candidates.Count == 0) return null;

                for (int attempt = 0; attempt < _maxAttempts * 2; attempt++)
                {
                    var room      = WeightedPick(candidates);
                    var b         = room.SpawnBounds;
                    var roomFloor = GetRoomFloor(room);
                    var pos       = new Vector2(
                        Random.Range(b.min.x, b.max.x),
                        Random.Range(b.min.y, b.max.y));
                    if (!IsBlocked(pos, roomFloor)) return pos;
                }
                return null;
            }

            // Static fallback (pre-procedural scene)
            if (_zones == null || _zones.Length == 0) return null;

            for (int attempt = 0; attempt < _maxAttempts; attempt++)
            {
                var zone = _zones[Random.Range(0, _zones.Length)];
                var pos  = new Vector2(
                    zone.center.x + Random.Range(-zone.halfExtents.x, zone.halfExtents.x),
                    zone.center.y + Random.Range(-zone.halfExtents.y, zone.halfExtents.y));

                if (!IsBlocked(pos)) return pos;
            }

            return null;
        }

        private bool IsMemoryRoom(RoomController room)
        {
            if (_graph == null) return false;
            foreach (var node in _graph.Nodes)
                if (node.PlacedRoom == room && node.Type == RoomType.Memory) return true;
            return false;
        }

        private static RoomController WeightedPick(List<(RoomController room, float weight)> candidates)
        {
            float total = 0f;
            foreach (var (_, w) in candidates) total += w;
            float r = Random.Range(0f, total);
            float acc = 0f;
            foreach (var (room, w) in candidates)
            {
                acc += w;
                if (r <= acc) return room;
            }
            return candidates[candidates.Count - 1].room;
        }

        private bool IsBlocked(Vector2 pos, Tilemap floor = null)
        {
            // Procedural mode: use the room's own Tilemap_Floor.
            // Static fallback: use the scene-level _floorTilemap.
            var tileSource = floor ?? _floorTilemap;
            if (tileSource != null)
            {
                var cell = tileSource.WorldToCell(pos);
                if (!tileSource.HasTile(cell)) return true;
            }

            // Must not overlap a wall collider
            var hits = Physics2D.OverlapCircleAll(pos, _clearanceRadius, _obstacleLayer);
            foreach (var hit in hits)
                if (!hit.isTrigger) return true;

            return false;
        }

        private static Tilemap GetRoomFloor(RoomController room)
        {
            foreach (var tm in room.GetComponentsInChildren<Tilemap>())
                if (tm.gameObject.name == "Tilemap_Floor") return tm;
            return null;
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (_zones == null) return;
            Gizmos.color = new Color(0.6f, 0.4f, 1f, 0.15f);
            foreach (var z in _zones)
                Gizmos.DrawCube(z.center, new Vector3(z.halfExtents.x * 2f, z.halfExtents.y * 2f, 0.1f));
        }
#endif
    }
}
