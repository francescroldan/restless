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

                    // Find a free socket on the current room
                    var fromSocket = FindFreeSocket(currentRoom);
                    if (fromSocket == null)
                    {
                        Debug.LogWarning($"[RoomAssembler] No free socket on {currentRoom.name} for node {neighbour.Index}");
                        continue;
                    }

                    var newRoom = PlaceNode(neighbour, Vector2.zero, currentRoom, fromSocket, rng);
                    if (newRoom == null)
                        Debug.LogWarning($"[RoomAssembler] Could not place node {neighbour.Index} ({neighbour.Type})");
                }
            }

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

                    if (HasOverlap(prefab, pos))
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
                        }
                    }

                    return instance;
                }
            }

            return null;
        }

        private Vector2 CalculatePosition(DoorSocket fromSocket, DoorSocket toSocket, RoomController prefab)
        {
            // We want toSocket's world pos after placement == fromSocket's current world pos.
            // toSocket's offset from prefab root = toSocket.position - prefab.transform.position
            // (handles any nesting depth, not just one level below root)
            Vector2 fromPos         = fromSocket.transform.position;
            Vector2 toOffsetFromRoot = (Vector2)(toSocket.transform.position - prefab.transform.position);
            Vector2 result           = fromPos - toOffsetFromRoot;
            Debug.Log($"[RoomAssembler] CalcPos: from={fromSocket.direction}@{fromPos}  toOffset={toOffsetFromRoot}  → newRoomPos={result}");
            return result;
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private List<RoomController> GetCandidates(GraphNode node)
        {
            var result = new List<RoomController>();
            foreach (var p in _roomPrefabs)
            {
                if (p.Definition == null) continue;
                if (!p.Definition.HasType(node.Type)) continue;
                if (node.PreferredSize != RoomSize.Medium && p.Definition.size != node.PreferredSize) continue;
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

        private DoorSocket FindFreeSocket(RoomController room)
        {
            foreach (var s in room.Sockets)
                if (!s.isOccupied) return s;
            return null;
        }

        private DoorSocket FindCompatibleSocket(RoomController prefab, DoorSocket fromSocket)
        {
            foreach (var s in prefab.Sockets)
                if (!s.isOccupied && s.CanConnectTo(fromSocket)) return s;
            return null;
        }

        private DoorSocket GetMatchingSocket(RoomController instance, DoorSocket prefabSocket)
        {
            // After instantiation, find the socket that matches prefabSocket by direction + local position
            foreach (var s in instance.Sockets)
                if (s.direction == prefabSocket.direction &&
                    Vector2.Distance(s.transform.localPosition, prefabSocket.transform.localPosition) < 0.05f)
                    return s;
            return null;
        }

        private bool HasOverlap(RoomController prefab, Vector2 pos)
        {
            if (_placed.Count == 0) return false;

            // Derive 2D bounds from the prefab's socket extents.
            // TilemapRenderer.bounds is unreliable on prefab assets (not in scene),
            // so we use the socket positions as a proxy for room extents.
            Vector2 size = GetSizeFromSockets(prefab);
            var bounds = new Bounds((Vector3)pos, new Vector3(size.x - _overlapCheckPadding,
                                                               size.y - _overlapCheckPadding, 1f));

            foreach (var placed in _placed)
            {
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
            var sockets     = room.Sockets;
            var defaultSize = SizeEnumToVector(room.Definition?.size ?? RoomSize.Medium);

            if (sockets == null || sockets.Length < 2) return defaultSize;

            float yMin = float.MaxValue, yMax = float.MinValue;
            foreach (var s in sockets)
            {
                float y = s.transform.localPosition.y;
                if (y < yMin) yMin = y;
                if (y > yMax) yMax = y;
            }

            float h = yMax - yMin;
            if (h < 1f) h = defaultSize.y;   // all sockets on same row — use enum fallback
            return new Vector2(defaultSize.x, h);
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
    }
}
