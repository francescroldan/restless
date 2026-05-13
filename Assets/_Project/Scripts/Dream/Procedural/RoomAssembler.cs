using System.Collections.Generic;
using UnityEngine;

namespace Restless.Dream.Procedural
{
    /// <summary>
    /// Takes a RunGraph and places room prefabs in the world.
    ///
    /// Algorithm:
    ///   1. Pick a prefab for each node (by type compatibility + size + dangerLevel)
    ///   2. Place entrance at origin
    ///   3. BFS from entrance — for each edge, align a free socket on the new room
    ///      with the corresponding socket on the already-placed room
    ///   4. Detect overlaps — retry with next compatible prefab if needed
    /// </summary>
    public class RoomAssembler : MonoBehaviour
    {
        [Header("Room catalogue")]
        [SerializeField] private RoomController[] _roomPrefabs;

        [Header("Settings")]
        [SerializeField] private int   _maxPlacementRetries = 8;
        [SerializeField] private float _overlapCheckPadding = 0.3f;

        private readonly List<RoomController> _placed = new();

        public IReadOnlyList<RoomController> PlacedRooms => _placed;

        public bool Assemble(RunGraph graph, int seed)
        {
            _placed.Clear();
            var rng = new System.Random(seed ^ 0xDEAD);

            // BFS
            var queue   = new Queue<GraphNode>();
            var visited = new HashSet<int>();

            queue.Enqueue(graph.Entrance);
            visited.Add(graph.Entrance.Index);

            // Place entrance at world origin
            var entranceRoom = PlaceNode(graph.Entrance, Vector2.zero, null, null, rng);
            if (entranceRoom == null)
            {
                Debug.LogError("[RoomAssembler] Could not place entrance room.");
                return false;
            }

            while (queue.Count > 0)
            {
                var current     = queue.Dequeue();
                var currentRoom = current.PlacedRoom;
                if (currentRoom == null) continue;

                foreach (var neighbour in graph.GetNeighbours(current))
                {
                    if (visited.Contains(neighbour.Index)) continue;
                    visited.Add(neighbour.Index);
                    queue.Enqueue(neighbour);

                    // Try free sockets in random order; fall back to next if placement fails
                    RoomController newRoom = null;
                    var triedSockets = new HashSet<DoorSocket>();
                    while (newRoom == null)
                    {
                        var fromSocket = FindFreeSocket(currentRoom, rng, triedSockets);
                        if (fromSocket == null) break;
                        triedSockets.Add(fromSocket);
                        newRoom = PlaceNode(neighbour, Vector2.zero, currentRoom, fromSocket, rng);
                    }
                    if (newRoom == null)
                        Debug.LogWarning($"[RoomAssembler] Could not place node {neighbour.Index} ({neighbour.Type}) after trying all free sockets on {currentRoom.name}.");
                }
            }

            // Close unused sockets: replace inner-corner frames with straight wall tiles
            // so unconnected doors show a solid wall with no visual door hint.
            foreach (var room in _placed)
                foreach (var s in room.Sockets)
                    if (!s.isOccupied)
                        room.CloseSocket(s);

            var sb = new System.Text.StringBuilder("[RoomAssembler] Layout:\n");
            foreach (var r in _placed)
                sb.AppendLine($"  {r.name}  pos={r.transform.position}  sockets={string.Join(",", System.Array.ConvertAll(r.Sockets, s => s.direction + (s.isOccupied ? "✓" : "○")))}");
            Debug.Log(sb.ToString());

            return true;
        }

        // ── Placement ─────────────────────────────────────────────────────────

        private RoomController PlaceNode(GraphNode node, Vector2 fallbackPos,
                                         RoomController fromRoom, DoorSocket fromSocket,
                                         System.Random rng)
        {
            var candidates = GetCandidates(node);
            Shuffle(candidates, rng);

            foreach (var prefab in candidates)
            {
                for (int attempt = 0; attempt < _maxPlacementRetries; attempt++)
                {
                    Vector2 pos;
                    DoorSocket inSocket = null;

                    if (fromSocket != null)
                    {
                        // Find a compatible receiving socket on this prefab
                        inSocket = FindCompatibleSocket(prefab, fromSocket);
                        if (inSocket == null) break; // prefab has no compatible socket, try next prefab

                        // Calculate position so inSocket aligns with fromSocket
                        pos = CalculatePosition(fromSocket, inSocket, prefab);
                    }
                    else
                    {
                        pos = fallbackPos;
                    }

                    if (HasOverlap(prefab, pos, fromRoom))
                        continue;

                    // Place it
                    var instance = Instantiate(prefab, pos, Quaternion.identity, transform);
                    instance.GraphNodeIndex = node.Index;
                    node.PlacedRoom = instance;
                    _placed.Add(instance);
                    Debug.Log($"[RoomAssembler] Placed node {node.Index} ({node.Type}) → {prefab.name} at {pos}  fromSocket={fromSocket?.direction} inSocket={inSocket?.direction}");

                    if (fromSocket != null && inSocket != null)
                    {
                        // Mark sockets occupied and link them
                        var instSocket = GetMatchingSocket(instance, inSocket);
                        if (instSocket != null)
                        {
                            fromSocket.isOccupied       = true;
                            instSocket.isOccupied       = true;
                            fromSocket.connectedSocket   = instSocket;
                            instSocket.connectedSocket   = fromSocket;

                            // Open wall tiles and disable blocker colliders
                            fromRoom.OpenSocket(fromSocket);
                            instance.OpenSocket(instSocket);
                            var fc = fromSocket.GetComponent<BoxCollider2D>();
                            if (fc != null) fc.enabled = false;
                            var ic = instSocket.GetComponent<BoxCollider2D>();
                            if (ic != null) ic.enabled = false;
                        }
                    }

                    return instance;
                }
            }

            return null;
        }

        private Vector2 CalculatePosition(DoorSocket fromSocket, DoorSocket toSocket, RoomController prefab)
        {
            // We want toSocket's world pos after placement == fromSocket's current world pos,
            // plus 1 unit of gap in the connection direction so adjacent wall tiles don't
            // occupy the same world-space cell (which causes Z-fighting on tilemaps).
            Vector2 fromPos          = fromSocket.transform.position;
            Vector2 toOffsetFromRoot = (Vector2)(toSocket.transform.position - prefab.transform.position);
            // All sockets sit at the wall edge — uniform 1-unit gap prevents Z-fighting.
            Vector2 gap = fromSocket.direction switch
            {
                SocketDirection.North => Vector2.up,
                SocketDirection.South => Vector2.down,
                SocketDirection.East  => Vector2.right,
                SocketDirection.West  => Vector2.left,
                _                     => Vector2.zero
            };
            return fromPos - toOffsetFromRoot + gap;
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private List<RoomController> GetCandidates(GraphNode node)
        {
            var result = new List<RoomController>();
            foreach (var p in _roomPrefabs)
            {
                if (p.Definition == null) continue;
                if (!p.Definition.HasType(node.Type)) continue;
                if (p.Definition.size != node.PreferredSize) continue;
                result.Add(p);
            }
            // Size-agnostic fallback: any prefab with matching type
            if (result.Count == 0)
                foreach (var p in _roomPrefabs)
                    if (p.Definition != null && p.Definition.HasType(node.Type))
                        result.Add(p);

            if (result.Count == 0)
                Debug.LogWarning($"[RoomAssembler] No candidate prefab for node {node.Index} type={node.Type} — skipping.");

            return result;
        }

        private DoorSocket FindFreeSocket(RoomController room, System.Random rng,
            HashSet<DoorSocket> exclude = null)
        {
            var free = new List<DoorSocket>();
            foreach (var s in room.Sockets)
                if (!s.isOccupied && (exclude == null || !exclude.Contains(s))) free.Add(s);
            if (free.Count == 0) return null;
            Shuffle(free, rng);
            return free[0];
        }

        private DoorSocket FindCompatibleSocket(RoomController prefab, DoorSocket fromSocket)
        {
            foreach (var s in prefab.Sockets)
                if (!s.isOccupied && s.CanConnectTo(fromSocket)) return s;
            return null;
        }

        private DoorSocket GetMatchingSocket(RoomController instance, DoorSocket prefabSocket)
        {
            foreach (var s in instance.Sockets)
                if (s.direction == prefabSocket.direction) return s;
            return null;
        }

        private bool HasOverlap(RoomController prefab, Vector2 pos, RoomController excludeRoom = null)
        {
            if (_placed.Count == 0) return false;

            Vector2 size = GetSizeFromSockets(prefab);
            var bounds = new Bounds((Vector3)pos, new Vector3(size.x - _overlapCheckPadding,
                                                               size.y - _overlapCheckPadding, 1f));

            foreach (var placed in _placed)
            {
                if (placed == excludeRoom) continue;

                Vector2 ps = GetSizeFromSockets(placed);
                var placedBounds = new Bounds((Vector3)(Vector2)placed.transform.position,
                                              new Vector3(ps.x - _overlapCheckPadding,
                                                          ps.y - _overlapCheckPadding, 1f));
                if (bounds.Intersects(placedBounds)) return true;
            }

            return false;
        }

        // Returns the room's world size for overlap checking.
        // Width  = from RoomSize enum (sockets are typically at the same X, span useless).
        // Height = socket Y span (socket-to-socket = exact room extent on the connect axis).
        // Falls back to size enum when fewer than 2 sockets exist.
        private Vector2 GetSizeFromSockets(RoomController room)
        {
            var sockets = room.Sockets;

            // Use stored tile extents from definition when available (set by CreateRoomVariants)
            var def = room.Definition;
            var defaultSize = (def != null && def.tileExtents.x > 0f)
                ? def.tileExtents * 2f
                : SizeEnumToVector(def?.size ?? RoomSize.Medium);

            if (sockets == null || sockets.Length < 2) return defaultSize;

            float xMin = float.MaxValue, xMax = float.MinValue;
            float yMin = float.MaxValue, yMax = float.MinValue;
            foreach (var s in sockets)
            {
                float x = s.transform.localPosition.x;
                float y = s.transform.localPosition.y;
                if (x < xMin) xMin = x;
                if (x > xMax) xMax = x;
                if (y < yMin) yMin = y;
                if (y > yMax) yMax = y;
            }

            float w = xMax - xMin;
            float h = yMax - yMin;
            if (w < 1f) w = defaultSize.x;   // all sockets on same column — use enum fallback
            if (h < 1f) h = defaultSize.y;   // all sockets on same row — use enum fallback
            return new Vector2(w, h);
        }

        private static Vector2 SizeEnumToVector(RoomSize size) => size switch
        {
            RoomSize.Small    => new Vector2(8,  6),
            RoomSize.Large    => new Vector2(16, 10),
            RoomSize.Landmark => new Vector2(16, 10),
            _                 => new Vector2(12, 8),
        };

        private static void Shuffle<T>(List<T> list, System.Random rng)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (_placed == null) return;
            var colors = new[] {
                new Color(0f,1f,0f,0.4f), new Color(0f,0.5f,1f,0.4f),
                new Color(1f,0.5f,0f,0.4f), new Color(1f,1f,0f,0.4f),
                new Color(1f,0f,1f,0.4f), new Color(0f,1f,1f,0.4f),
            };
            for (int i = 0; i < _placed.Count; i++)
            {
                var r = _placed[i];
                if (r == null) continue;
                Vector2 sz = GetSizeFromSockets(r);
                Gizmos.color = colors[i % colors.Length];
                Gizmos.DrawWireCube(r.transform.position, new Vector3(sz.x, sz.y, 0.1f));
            }
        }
#endif
    }
}
