using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

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

        [Header("Door tiles (auto-loaded from DarkDungeon at Play)")]
        [SerializeField] private Tile _doorNS_Open;
        [SerializeField] private Tile _doorNS_Closed;
        [SerializeField] private Tile _doorE_Open;
        [SerializeField] private Tile _doorW_Open;

        [Header("Room types that get a door instead of an open corridor")]
        [SerializeField] private RoomType[] _doorRoomTypes = { RoomType.Ritual };

        [Header("Blood vein tiles (Ritual rooms — auto-loaded from DarkDungeon)")]
        [SerializeField] private Tile[] _allTiles; // index 0 = dd_01 ... index 43 = dd_44

        [Header("Transition")]
        [SerializeField] private float _spawnOffset = 2f;

        private readonly List<RoomController>              _placed              = new();
        private readonly List<(RoomController, DoorSocket)> _doorApproachSockets = new();

        public IReadOnlyList<RoomController> PlacedRooms => _placed;

        // Blood-vein remap: standard wall tiles → blood variants.
        // Only N/S straight walls and inner corners have blood variants.
        // Outer corner tiles (25/27/43/44) and E/W walls (30/35) have no blood variants.
        private static readonly (int clean, int blood)[] s_WallBloodRemap =
        {
            (26, 21),  // straight N/S wall → blood straight
            (28, 37),  // inner corner I_TL → blood I_TL
            (29, 38),  // inner corner I_TR → blood I_TR
            (32, 40),  // inner corner I_BL → blood I_BL
            (33, 39),  // inner corner I_BR → blood I_BR
        };

        public bool Assemble(RunGraph graph, int seed)
        {
            AutoLoadDoorAssets();

            // Serialized arrays lose C# default values on existing scene objects
            if (_doorRoomTypes == null || _doorRoomTypes.Length == 0)
                _doorRoomTypes = new[] { RoomType.Ritual };

            _placed.Clear();
            _doorApproachSockets.Clear();
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
                        newRoom = PlaceNode(neighbour, Vector2.zero, currentRoom, fromSocket, rng, current);
                    }
                    if (newRoom == null)
                        Debug.LogWarning($"[RoomAssembler] Could not place node {neighbour.Index} ({neighbour.Type}) after trying all free sockets on {currentRoom.name}.");
                }
            }

            // Close unused sockets: fix corner frames; show closed-door tile if assigned.
            foreach (var room in _placed)
                foreach (var s in room.Sockets)
                    if (!s.isOccupied)
                        room.CloseSocket(s);

            PaintRitualRooms(graph);

            // Door triggers are added per-connection in PlaceNode; no per-room triggers needed.

            var sb = new System.Text.StringBuilder("[RoomAssembler] Layout:\n");
            foreach (var r in _placed)
                sb.AppendLine($"  {r.name}  pos={r.transform.position}  sockets={string.Join(",", System.Array.ConvertAll(r.Sockets, s => s.direction + (s.isOccupied ? "✓" : "○")))}");
            Debug.Log(sb.ToString());

            return true;
        }

        // ── Placement ─────────────────────────────────────────────────────────

        private RoomController PlaceNode(GraphNode node, Vector2 fallbackPos,
                                         RoomController fromRoom, DoorSocket fromSocket,
                                         System.Random rng, GraphNode fromNode = null)
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

                            bool isDoor = IsDoorConnection(node, fromNode);

                            if (!isDoor)
                            {
                                // Corridor: remove wall tiles so player can walk through
                                fromRoom.OpenSocket(fromSocket);
                                instance.OpenSocket(instSocket);
                            }
                            else
                            {
                                // Door: one tile on each side (single cell each).
                                // CloseSocket(null) repairs flanking corner tiles.
                                // PlaceSingleDoorTile avoids CloseSocket's 2-cell fill.
                                fromRoom.CloseSocket(fromSocket, null);
                                PlaceSingleDoorTile(fromRoom, fromSocket,
                                                    GetDoorTile(fromSocket.direction));
                                instance.CloseSocket(instSocket, null);
                                PlaceSingleDoorTile(instance, instSocket,
                                                    GetDoorTile(instSocket.direction));
                                CleanDoorCorners(instance, instSocket);
                                _doorApproachSockets.Add((fromRoom, fromSocket));
                            }

                            var fc = fromSocket.GetComponent<BoxCollider2D>();
                            if (fc != null) fc.enabled = false;
                            var ic = instSocket.GetComponent<BoxCollider2D>();
                            if (ic != null) ic.enabled = false;

                            SetupConnectionTriggers(fromRoom, instance, fromSocket, instSocket, isDoor);
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

        // Returns true if either end of the connection is a Ritual (door) room type.
        private bool IsDoorConnection(GraphNode node, GraphNode fromNode = null)
        {
            foreach (var t in _doorRoomTypes)
            {
                if (node.Type == t) return true;
                if (fromNode != null && fromNode.Type == t) return true;
            }
            return false;
        }

        // Corridor: one trigger at the midpoint of the gap (player walks through freely).
        // Door:     two triggers, one just inside each room (player bumps the wall to cross).
        private void SetupConnectionTriggers(RoomController roomA, RoomController roomB,
                                             DoorSocket socketA, DoorSocket socketB,
                                             bool isDoor)
        {
            Vector2 spawnA = (Vector2)socketA.transform.position + Inward(socketA.direction, _spawnOffset);
            Vector2 spawnB = (Vector2)socketB.transform.position + Inward(socketB.direction, _spawnOffset);

            if (!isDoor)
            {
                // Single trigger at the midpoint of the 1-unit gap
                bool ns = socketA.direction == SocketDirection.North ||
                          socketA.direction == SocketDirection.South;
                Vector2 size = ns ? new Vector2(2f, 1.5f) : new Vector2(1.5f, 2f);
                Vector3 mid  = ((Vector3)socketA.transform.position +
                                (Vector3)socketB.transform.position) * 0.5f;
                SpawnTrigger(roomA, roomB, mid, size, spawnA, spawnB, parent: transform);
            }
            else
            {
                // Two triggers, one 0.5 u inside each room — wall blocks the far side
                bool ns = socketA.direction == SocketDirection.North ||
                          socketA.direction == SocketDirection.South;
                Vector2 size = ns ? new Vector2(2f, 0.8f) : new Vector2(0.8f, 2f);

                Vector3 posA = (Vector3)socketA.transform.position +
                               (Vector3)Inward(socketA.direction, 0.5f);
                Vector3 posB = (Vector3)socketB.transform.position +
                               (Vector3)Inward(socketB.direction, 0.5f);

                SpawnTrigger(roomA, roomB, posA, size, spawnA, spawnB, parent: roomA.transform);
                SpawnTrigger(roomA, roomB, posB, size, spawnA, spawnB, parent: roomB.transform);
            }
        }

        private void SpawnTrigger(RoomController roomA, RoomController roomB,
                                  Vector3 worldPos, Vector2 size,
                                  Vector2 spawnA, Vector2 spawnB,
                                  Transform parent)
        {
            var go = new GameObject("ConnectionTrigger");
            go.transform.position = worldPos;
            go.transform.SetParent(parent, true);
            go.AddComponent<BoxCollider2D>();
            go.AddComponent<DoorCrossingTrigger>().Init(roomA, roomB, size, spawnA, spawnB);
        }

        private Tile GetDoorTile(SocketDirection dir) => dir switch
        {
            SocketDirection.North or SocketDirection.South => _doorNS_Open,
            SocketDirection.East                           => _doorE_Open,
            SocketDirection.West                           => _doorW_Open,
            _                                              => null
        };

        private static Vector2 Inward(SocketDirection dir, float amount) => dir switch
        {
            SocketDirection.North => new Vector2( 0f,    -amount),
            SocketDirection.South => new Vector2( 0f,     amount),
            SocketDirection.East  => new Vector2(-amount,  0f),
            SocketDirection.West  => new Vector2( amount,  0f),
            _                     => Vector2.zero
        };

        private static void Shuffle<T>(List<T> list, System.Random rng)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }

        private void AutoLoadDoorAssets()
        {
#if UNITY_EDITOR
            const string D = "Assets/_Project/Art/Tiles/DarkDungeon";
            _doorNS_Open   ??= UnityEditor.AssetDatabase.LoadAssetAtPath<Tile>($"{D}/dd_24.asset");
            _doorNS_Closed ??= UnityEditor.AssetDatabase.LoadAssetAtPath<Tile>($"{D}/dd_23.asset");
            _doorE_Open    ??= UnityEditor.AssetDatabase.LoadAssetAtPath<Tile>($"{D}/dd_14.asset");
            _doorW_Open    ??= UnityEditor.AssetDatabase.LoadAssetAtPath<Tile>($"{D}/dd_15.asset");

            if (_allTiles == null || _allTiles.Length < 44)
            {
                _allTiles = new Tile[44];
                for (int n = 1; n <= 44; n++)
                    _allTiles[n - 1] = UnityEditor.AssetDatabase.LoadAssetAtPath<Tile>($"{D}/dd_{n:D2}.asset");
            }
            int loadedCount = 0;
            foreach (var t in _allTiles) if (t != null) loadedCount++;
            Debug.Log($"[RoomAssembler] Door tiles — NS_Open={_doorNS_Open?.name ?? "NULL"}  NS_Closed={_doorNS_Closed?.name ?? "NULL"}  E={_doorE_Open?.name ?? "NULL"}  W={_doorW_Open?.name ?? "NULL"}  allTiles_loaded={loadedCount}/44");
#endif
        }

        // ── Ritual room blood vein painter ───────────────────────────────────

        private void PaintRitualRooms(RunGraph graph)
        {
            if (_allTiles == null || _allTiles.Length < 44) return;

            // Blood remap: clean wall tiles → blood-vein variants
            var bloodRemap = new Dictionary<TileBase, TileBase>();
            foreach (var (c, b) in s_WallBloodRemap)
            {
                var clean = GetTileByNum(c);
                var blood = GetTileByNum(b);
                if (clean != null && blood != null) bloodRemap[clean] = blood;
            }
            if (bloodRemap.Count == 0)
            {
                Debug.LogWarning("[RoomAssembler] Blood remap is empty — are DarkDungeon tile assets loaded?");
                return;
            }

            // Use graph node types — prefab definitions can have multiple types
            // (e.g. [Ritual, Encounter]) and mislead Definition.HasType checks.
            var ritualRooms = new HashSet<RoomController>();
            foreach (var node in graph.Nodes)
                if (node.Type == RoomType.Ritual && node.PlacedRoom != null)
                    ritualRooms.Add(node.PlacedRoom);

            Debug.Log($"[RoomAssembler] PaintRitualRooms: {ritualRooms.Count} Ritual room(s).");

            // Build reverse lookup: RoomController → node type
            var roomToType = new Dictionary<RoomController, RoomType>(_placed.Count);
            foreach (var node in graph.Nodes)
                if (node.PlacedRoom != null)
                    roomToType[node.PlacedRoom] = node.Type;

            // Pass 0: clean blood floor tiles from all non-Ritual rooms (safety net)
            foreach (var room in _placed)
                if (!ritualRooms.Contains(room))
                    CleanFloor(room);

            // Pass 0b: paint blood floor seeping from Ritual room doorways
            foreach (var (room, socket) in _doorApproachSockets)
                PaintDoorBloodFloor(room, socket);

            // Pass 1: apply blood tiles to Ritual rooms at ~50 % density so the
            // N/S wall bands don't look like a uniform solid stripe.
            foreach (var room in ritualRooms)
                RepaintCliff(room, bloodRemap, faceDir: null, density: 0.5f);

            // After blood paint, restore regular wall tiles next to door openings.
            // The blood pass may have painted inner-corner tiles blood, but those
            // cells are adjacent to a door tile (not a corridor), so they must stay plain.
            foreach (var room in ritualRooms)
                foreach (var socket in room.Sockets)
                {
                    if (!socket.isOccupied || socket.connectedSocket == null) continue;
                    var connRoom = socket.connectedSocket.GetComponentInParent<RoomController>();
                    if (connRoom == null) continue;
                    if (!roomToType.TryGetValue(connRoom, out var connType)) continue;
                    if (connType != RoomType.Ritual)
                        CleanDoorCorners(room, socket);
                }

            // Pass 2: apply blood tiles on connecting face of rooms adjacent to Ritual,
            // limited to a small radius around the door to avoid repetition.
            foreach (var room in _placed)
            {
                if (ritualRooms.Contains(room)) continue;
                foreach (var socket in room.Sockets)
                {
                    if (!socket.isOccupied || socket.connectedSocket == null) continue;
                    var connRoom = socket.connectedSocket.GetComponentInParent<RoomController>();
                    if (connRoom == null) continue;
                    if (!roomToType.TryGetValue(connRoom, out var connType)) continue;
                    if (connType != RoomType.Ritual) continue;
                    RepaintCliff(room, bloodRemap, faceDir: socket.direction,
                                 socketWorldPos: socket.transform.position, faceRadius: 2);
                }
            }
        }

        private Tile GetTileByNum(int n) =>
            (_allTiles != null && n >= 1 && n <= _allTiles.Length) ? _allTiles[n - 1] : null;

        // Places a door tile in only the base cell of the 2-cell-wide opening.
        // CloseSocket(tile) fills both cells and produces a doubled visual; this
        // places it in exactly one cell so the sprite appears once.
        private void PlaceSingleDoorTile(RoomController room, DoorSocket socket, Tile tile)
        {
            if (tile == null) return;
            Tilemap cliff = null;
            foreach (var tm in room.GetComponentsInChildren<Tilemap>())
                if (tm.gameObject.name == "Tilemap_Cliff") { cliff = tm; break; }
            if (cliff == null) return;

            var baseCell = cliff.WorldToCell(socket.transform.position);
            cliff.SetTile(baseCell, tile);
            cliff.RefreshTile(baseCell);
        }

        // Replaces inner-corner tiles immediately flanking a door opening with the
        // plain wall tile for that face. Inner corners are valid next to corridor
        // openings, but cells adjacent to a door tile (23/24) must be plain wall.
        private void CleanDoorCorners(RoomController room, DoorSocket socket)
        {
            Tilemap cliff = null;
            foreach (var tm in room.GetComponentsInChildren<Tilemap>())
                if (tm.gameObject.name == "Tilemap_Cliff") { cliff = tm; break; }
            if (cliff == null) return;

            var socketCell = cliff.WorldToCell(socket.transform.position);

            // The 2-cell door opening sits at socketCell and socketCell+1 (along the face).
            // Inner corners placed by the builder are 1 cell outside that span.
            Vector3Int cornerA, cornerB;
            int replacementNum;
            switch (socket.direction)
            {
                case SocketDirection.North:
                case SocketDirection.South:
                    cornerA = socketCell + new Vector3Int(-1, 0, 0);
                    cornerB = socketCell + new Vector3Int( 2, 0, 0);
                    replacementNum = 26; // straight N/S wall
                    break;
                case SocketDirection.West:
                    cornerA = socketCell + new Vector3Int(0,  2, 0);
                    cornerB = socketCell + new Vector3Int(0, -1, 0);
                    replacementNum = 30; // West wall
                    break;
                case SocketDirection.East:
                    cornerA = socketCell + new Vector3Int(0,  2, 0);
                    cornerB = socketCell + new Vector3Int(0, -1, 0);
                    replacementNum = 35; // East wall
                    break;
                default: return;
            }

            var replacement = GetTileByNum(replacementNum);
            if (replacement == null) return;

            var innerCorners = new HashSet<TileBase>();
            foreach (int n in new[] { 28, 29, 32, 33, 37, 38, 39, 40 })
            {
                var t = GetTileByNum(n);
                if (t != null) innerCorners.Add(t);
            }

            if (innerCorners.Contains(cliff.GetTile(cornerA))) cliff.SetTile(cornerA, replacement);
            if (innerCorners.Contains(cliff.GetTile(cornerB))) cliff.SetTile(cornerB, replacement);
        }

        // Paints blood floor tiles spreading inward from the door socket —
        // denser near the door and fanning out in a triangle shape.
        private void PaintDoorBloodFloor(RoomController room, DoorSocket socket)
        {
            Tilemap floor = null;
            foreach (var tm in room.GetComponentsInChildren<Tilemap>())
                if (tm.gameObject.name == "Tilemap_Floor") { floor = tm; break; }
            if (floor == null) return;

            var socketCell = floor.WorldToCell(socket.transform.position);

            bool horizontal = socket.direction == SocketDirection.North ||
                              socket.direction == SocketDirection.South;
            Vector3Int inward = socket.direction switch
            {
                SocketDirection.North => new Vector3Int( 0, -1, 0),
                SocketDirection.South => new Vector3Int( 0,  1, 0),
                SocketDirection.East  => new Vector3Int(-1,  0, 0),
                SocketDirection.West  => new Vector3Int( 1,  0, 0),
                _                     => Vector3Int.zero
            };
            Vector3Int perp = horizontal ? new Vector3Int(1, 0, 0) : new Vector3Int(0, 1, 0);

            int[] bloodNums = { 4, 5, 6, 7, 8, 9, 10 };

            // 4 depth layers; density and spread both taper as we move inward.
            for (int depth = 1; depth <= 4; depth++)
            {
                float density  = 1.1f - depth * 0.25f; // 0.85 → 0.10
                int   halfSpan = depth;                 // fan widens each step

                for (int offset = -halfSpan; offset <= halfSpan + 1; offset++)
                {
                    var pos = socketCell + inward * depth + perp * offset;

                    // Only paint on existing floor cells
                    if (floor.GetTile(pos) == null) continue;

                    int hash = Mathf.Abs(pos.x * 7 + pos.y * 13);
                    if ((float)(hash % 100) / 100f > density) continue;

                    var tile = GetTileByNum(bloodNums[hash % bloodNums.Length]);
                    if (tile != null) floor.SetTile(pos, tile);
                }
            }
        }

        private void CleanFloor(RoomController room)
        {
            Tilemap floor = null;
            foreach (var tm in room.GetComponentsInChildren<Tilemap>())
                if (tm.gameObject.name == "Tilemap_Floor") { floor = tm; break; }
            if (floor == null) return;

            // Non-blood floor tiles: dd_01..dd_03 and dd_11..dd_13.
            // dd_04..dd_10 are blood variants — replace them in non-Ritual rooms.
            var cleanSet = new HashSet<TileBase>();
            foreach (int n in new[] { 1, 2, 3, 11, 12, 13 })
            {
                var t = GetTileByNum(n);
                if (t != null) cleanSet.Add(t);
            }
            var fallback = GetTileByNum(1);
            if (fallback == null) return;

            floor.CompressBounds();
            var positions = new List<Vector3Int>();
            foreach (var pos in floor.cellBounds.allPositionsWithin)
            {
                var tile = floor.GetTile(pos);
                if (tile != null && !cleanSet.Contains(tile))
                    positions.Add(pos);
            }
            foreach (var pos in positions)
                floor.SetTile(pos, fallback);
        }

        private static void RepaintCliff(RoomController room, Dictionary<TileBase, TileBase> remap,
                                         SocketDirection? faceDir,
                                         Vector3? socketWorldPos = null, int faceRadius = int.MaxValue,
                                         float density = 1f)
        {
            Tilemap cliff = null;
            foreach (var tm in room.GetComponentsInChildren<Tilemap>())
                if (tm.gameObject.name == "Tilemap_Cliff") { cliff = tm; break; }
            if (cliff == null) return;

            cliff.CompressBounds();
            var bounds = cliff.cellBounds;
            var positions = new List<Vector3Int>();

            Vector3Int socketCell = socketWorldPos.HasValue
                ? cliff.WorldToCell(socketWorldPos.Value)
                : Vector3Int.zero;

            foreach (var pos in bounds.allPositionsWithin)
            {
                if (faceDir.HasValue)
                {
                    bool onFace = faceDir.Value switch
                    {
                        SocketDirection.North => pos.y == bounds.yMax - 1,
                        SocketDirection.South => pos.y == bounds.yMin,
                        SocketDirection.East  => pos.x == bounds.xMax - 1,
                        SocketDirection.West  => pos.x == bounds.xMin,
                        _                     => false
                    };
                    if (!onFace) continue;

                    if (socketWorldPos.HasValue && faceRadius < int.MaxValue)
                    {
                        bool inRadius = faceDir.Value switch
                        {
                            SocketDirection.North or SocketDirection.South =>
                                Mathf.Abs(pos.x - socketCell.x) <= faceRadius,
                            _ => Mathf.Abs(pos.y - socketCell.y) <= faceRadius,
                        };
                        if (!inRadius) continue;
                    }
                }

                if (density < 1f)
                {
                    int hash = Mathf.Abs(pos.x * 7 + pos.y * 13);
                    if (hash % 10 >= Mathf.RoundToInt(density * 10)) continue;
                }

                var tile = cliff.GetTile(pos);
                if (tile != null && remap.ContainsKey(tile))
                    positions.Add(pos);
            }

            foreach (var pos in positions)
                cliff.SetTile(pos, remap[cliff.GetTile(pos)]);
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
