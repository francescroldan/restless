using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;
using Restless.Dream.Procedural;

/// <summary>
/// Rebuilds all room prefabs in Assets/_Project/Prefabs/Rooms/ using the
/// DarkDungeon tileset. Run via Tools > Restless > Rebuild Rooms (DarkDungeon).
///
/// Door openings are driven by DoorSocket components on the prefab — NOT by
/// scanning for gaps in the existing tilemap. Add/remove DoorSocket children,
/// then run Rebuild to repaint.
/// </summary>
public static class RebuildRoomsDarkDungeon
{
    const string TileDir   = "Assets/_Project/Art/Tiles/DarkDungeon";
    const string PrefabDir = "Assets/_Project/Prefabs/Rooms";

    // ── Tile indices ──────────────────────────────────────────────────────────
    const int TL = 25, TR = 27, BL = 44, BR = 43;
    const int T = 26, B = 26, L = 30, R = 35;
    const int I_TL = 28, I_TR = 29, I_BL = 32, I_BR = 33;
    static readonly int[] FloorVariants = { 1, 1, 1, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
    const int DoorClosed = 23;

    static readonly int[] WallTiles = { 25, 26, 27, 28, 29, 30, 32, 33, 35, 36, 37, 38, 39, 40, 43, 44 };
    static readonly int[] DoorTiles = { 23 };

    // ── Fix Tile Colliders ────────────────────────────────────────────────────

    [MenuItem("Tools/Restless/Fix Tile Colliders (DarkDungeon)")]
    public static void FixColliders()
    {
        int changed = 0;
        foreach (int n in WallTiles)
            changed += SetCollider(n, UnityEngine.Tilemaps.Tile.ColliderType.Grid);
        foreach (int n in DoorTiles)
            changed += SetCollider(n, UnityEngine.Tilemaps.Tile.ColliderType.Grid);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"[RebuildRooms] Colliders fixed — {changed} tile(s) updated.");
    }

    static int SetCollider(int n, UnityEngine.Tilemaps.Tile.ColliderType colliderType)
    {
        string path = $"{TileDir}/dd_{n:D2}.asset";
        var tile = AssetDatabase.LoadAssetAtPath<UnityEngine.Tilemaps.Tile>(path);
        if (tile == null) { Debug.LogWarning($"[RebuildRooms] Tile not found: {path}"); return 0; }
        if (tile.colliderType == colliderType) return 0;
        tile.colliderType = colliderType;
        EditorUtility.SetDirty(tile);
        return 1;
    }

    // ── Add All-4-Direction Sockets ───────────────────────────────────────────


    static bool AddMissingDirectionSockets(GameObject root)
    {
        var ctrl = root.GetComponent<RoomController>();
        if (ctrl == null) return false;

        Vector2 extents = SizeToHalfExtents(ctrl.Definition?.size ?? RoomSize.Medium);

        // Determine which directions this room supports.
        // If socketDirections is empty/null → default to all four.
        var dirConfig = ctrl.Definition?.socketDirections;
        var allowedDirs = (dirConfig != null && dirConfig.Length > 0)
            ? new HashSet<SocketDirection>(dirConfig)
            : null; // null = all four

        Transform socketsParent = root.transform.Find("Sockets");
        if (socketsParent == null)
        {
            socketsParent = new GameObject("Sockets").transform;
            socketsParent.SetParent(root.transform, false);
        }

        bool changed = false;
        foreach (SocketDirection dir in System.Enum.GetValues(typeof(SocketDirection)))
        {
            bool allowed = allowedDirs == null || allowedDirs.Contains(dir);
            Vector3 targetPos = SocketLocalPos(dir, extents);

            DoorSocket existing = null;
            foreach (var s in root.GetComponentsInChildren<DoorSocket>(true))
                if (s.direction == dir) { existing = s; break; }

            if (!allowed)
            {
                // Remove socket if this direction is not in the definition
                if (existing != null)
                {
                    Object.DestroyImmediate(existing.gameObject);
                    changed = true;
                }
                continue;
            }

            if (existing != null)
            {
                // Reset to formula position in case it was manually mis-placed
                if (Vector3.Distance(existing.transform.localPosition, targetPos) > 0.01f)
                {
                    existing.transform.localPosition = targetPos;
                    changed = true;
                }
            }
            else
            {
                var go = new GameObject($"Socket_{dir}");
                go.transform.SetParent(socketsParent, false);
                go.transform.localPosition = targetPos;

                var s = go.AddComponent<DoorSocket>();
                s.direction = dir;
                s.width     = 2f;
                changed = true;
            }
        }
        return changed;
    }

    // Socket position at the wall edge, door centred at tiles (-1,0) / (0,-1).
    // North/South: x = -0.5 (2-tile door spanning x=-1 and x=0)
    // East/West:   y = -0.5 (2-tile door spanning y=-1 and y=0)
    static Vector3 SocketLocalPos(SocketDirection dir, Vector2 extents) => dir switch
    {
        SocketDirection.North => new Vector3(-0.5f,  extents.y,       0),
        SocketDirection.South => new Vector3(-0.5f, -extents.y,       0),
        SocketDirection.East  => new Vector3( extents.x,       -0.5f,  0),
        SocketDirection.West  => new Vector3(-extents.x,       -0.5f, 0),
        _                     => Vector3.zero
    };

    // Half-extents match the tile grid: width/2 and height/2 in world units.
    static Vector2 SizeToHalfExtents(RoomSize size) => size switch
    {
        RoomSize.Small    => new Vector2(4f, 3f),
        RoomSize.Large    => new Vector2(8f, 5f),
        RoomSize.Landmark => new Vector2(8f, 5f),
        _                 => new Vector2(6f, 4f),   // Medium
    };

    // ── Rebuild Rooms ─────────────────────────────────────────────────────────

    /// <summary>
    /// Ensures every room prefab has N/S/E/W DoorSocket children, then repaints
    /// all tilemaps with the DarkDungeon tileset. Door openings are driven by the
    /// sockets present on each prefab.
    /// </summary>
    [MenuItem("Tools/Restless/Rebuild Rooms (DarkDungeon)")]
    public static void RebuildAll()
    {
        var prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { PrefabDir });
        int rebuilt = 0;

        foreach (var guid in prefabGuids)
        {
            string path   = AssetDatabase.GUIDToAssetPath(guid);
            var    prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null) continue;

            var ctrl = prefab.GetComponent<RoomController>();
            if (ctrl == null) continue;

            using (var scope = new PrefabUtility.EditPrefabContentsScope(path))
            {
                AddMissingDirectionSockets(scope.prefabContentsRoot);
                EnsureCliffColliders(scope.prefabContentsRoot);
                EnsureSocketBlockers(scope.prefabContentsRoot);
                // Use ctrl from scope for fresh definition data (avoids stale cache)
                var scopeCtrl = scope.prefabContentsRoot.GetComponent<RoomController>();
                RebuildTilemaps(scope.prefabContentsRoot, scopeCtrl != null ? scopeCtrl : ctrl);
                rebuilt++;
                Debug.Log($"[RebuildRooms] Rebuilt {prefab.name}");
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"[RebuildRooms] Done — {rebuilt} prefabs rebuilt with DarkDungeon tileset.");

        RegisterAllRoomPrefabs();
    }

    // ── Register prefabs in Dream scene ──────────────────────────────────────

    /// <summary>
    /// Scans Assets/_Project/Prefabs/Rooms/ and ensures every RoomController
    /// prefab is registered in the RoomAssembler component in Dream.unity.
    /// Called automatically at the end of RebuildAll().
    /// </summary>
    [MenuItem("Tools/Restless/Register All Room Prefabs")]
    public static void RegisterAllRoomPrefabs()
    {
        const string ScenePath = "Assets/_Project/Scenes/Dream.unity";

        var prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { PrefabDir });
        var prefabs = new List<RoomController>();
        foreach (var guid in prefabGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (go == null) continue;
            var rc = go.GetComponent<RoomController>();
            if (rc != null) prefabs.Add(rc);
        }

        // Open the Dream scene as a serialized object to update the array field
        var sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath);
        if (sceneAsset == null) { Debug.LogWarning("[RebuildRooms] Dream.unity not found."); return; }

        // Use the scene manager to find the RoomAssembler in the loaded scene.
        // The scene must not be dirty — we use SerializedObject on the prefab instances.
        var dreamScene = UnityEditor.SceneManagement.EditorSceneManager.OpenScene(ScenePath, UnityEditor.SceneManagement.OpenSceneMode.Additive);

        RoomAssembler assembler = null;
        foreach (var root in dreamScene.GetRootGameObjects())
        {
            var found = root.GetComponentsInChildren<RoomAssembler>(true);
            if (found.Length > 0) { assembler = found[0]; break; }
        }

        if (assembler == null)
        {
            Debug.LogWarning("[RebuildRooms] RoomAssembler not found in Dream.unity.");
            UnityEditor.SceneManagement.EditorSceneManager.CloseScene(dreamScene, true);
            return;
        }

        var so   = new SerializedObject(assembler);
        var prop = so.FindProperty("_roomPrefabs");
        prop.arraySize = prefabs.Count;
        for (int i = 0; i < prefabs.Count; i++)
            prop.GetArrayElementAtIndex(i).objectReferenceValue = prefabs[i];
        so.ApplyModifiedProperties();

        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(dreamScene);
        UnityEditor.SceneManagement.EditorSceneManager.CloseScene(dreamScene, true);

        Debug.Log($"[RebuildRooms] Registered {prefabs.Count} room prefabs in Dream.unity.");
    }

    // ─────────────────────────────────────────────────────────────────────────

    static void RebuildTilemaps(GameObject root, RoomController ctrl)
    {
        var floor = FindTilemap(root, "Tilemap_Floor");
        var cliff = FindTilemap(root, "Tilemap_Cliff");
        if (floor == null || cliff == null)
        {
            Debug.LogWarning($"[RebuildRooms] Missing Tilemap_Floor or Tilemap_Cliff on {root.name}");
            return;
        }

        cliff.CompressBounds();
        var bounds = cliff.cellBounds;
        if (bounds.size == Vector3Int.zero)
        {
            Debug.LogWarning($"[RebuildRooms] Empty cliff tilemap on {root.name}, skipping.");
            return;
        }

        // When the definition stores explicit tileExtents (set by CreateRoomVariants),
        // use those to drive the painted area so non-square rooms (corridors, corners)
        // get the correct shape regardless of what was seeded in the cliff tilemap.
        var def = ctrl.Definition;
        int xMin, xMax, yMin, yMax;
        if (def != null && def.tileExtents.x > 0f)
        {
            int ex = (int)def.tileExtents.x;
            int ey = (int)def.tileExtents.y;
            xMin = -ex; xMax = ex;
            yMin = -ey; yMax = ey;
        }
        else
        {
            // Wall rows/columns driven by socket local positions for pre-existing prefabs.
            xMin = SocketCol(root, SocketDirection.West,  bounds.xMin);
            xMax = SocketCol(root, SocketDirection.East,  bounds.xMax - 1);
            yMin = SocketRow(root, SocketDirection.South, bounds.yMin);
            yMax = SocketRow(root, SocketDirection.North, bounds.yMax - 1);
        }
        int xIn0 = xMin + 1, xIn1 = xMax - 1;
        int yIn0 = yMin + 1, yIn1 = yMax - 1;

        bool hasNorth = HasSocketInDirection(root, SocketDirection.North);
        bool hasSouth = HasSocketInDirection(root, SocketDirection.South);
        bool hasEast  = HasSocketInDirection(root, SocketDirection.East);
        bool hasWest  = HasSocketInDirection(root, SocketDirection.West);

        floor.ClearAllTiles();
        cliff.ClearAllTiles();

        var rng = new System.Random(root.name.GetHashCode());

        // ── Floor ─────────────────────────────────────────────────────────────
        for (int x = xIn0; x <= xIn1; x++)
        for (int y = yIn0; y <= yIn1; y++)
        {
            int idx = FloorVariants[rng.Next(FloorVariants.Length)];
            floor.SetTile(new Vector3Int(x, y, 0), Tile(idx));
        }

        // ── Outer corners ─────────────────────────────────────────────────────
        cliff.SetTile(new Vector3Int(xMin, yMax, 0), Tile(TL));
        cliff.SetTile(new Vector3Int(xMax, yMax, 0), Tile(TR));
        cliff.SetTile(new Vector3Int(xMin, yMin, 0), Tile(BL));
        cliff.SetTile(new Vector3Int(xMax, yMin, 0), Tile(BR));

        // ── Top edge ──────────────────────────────────────────────────────────
        // Door slots are always painted as wall — the assembler opens them at runtime.
        for (int x = xIn0; x <= xIn1; x++)
        {
            bool isPotentialDoor = hasNorth && (x == -1 || x == 0);
            cliff.SetTile(new Vector3Int(x, yMax, 0), Tile(T));
            if (isPotentialDoor) floor.SetTile(new Vector3Int(x, yMax, 0), Tile(1));
        }

        // ── Bottom edge ───────────────────────────────────────────────────────
        for (int x = xIn0; x <= xIn1; x++)
        {
            bool isPotentialDoor = hasSouth && (x == -1 || x == 0);
            cliff.SetTile(new Vector3Int(x, yMin, 0), Tile(B));
            if (isPotentialDoor) floor.SetTile(new Vector3Int(x, yMin, 0), Tile(1));
        }

        // ── Left edge ─────────────────────────────────────────────────────────
        for (int y = yIn0; y <= yIn1; y++)
        {
            bool isPotentialDoor = hasWest && (y == -1 || y == 0);
            cliff.SetTile(new Vector3Int(xMin, y, 0), Tile(L));
            if (isPotentialDoor) floor.SetTile(new Vector3Int(xMin, y, 0), Tile(1));
        }

        // ── Right edge ────────────────────────────────────────────────────────
        for (int y = yIn0; y <= yIn1; y++)
        {
            bool isPotentialDoor = hasEast && (y == -1 || y == 0);
            cliff.SetTile(new Vector3Int(xMax, y, 0), Tile(R));
            if (isPotentialDoor) floor.SetTile(new Vector3Int(xMax, y, 0), Tile(1));
        }

        // ── Inner corners at door edges ───────────────────────────────────────
        // Always baked into prefab. CloseSocket replaces them with straight wall
        // tiles at runtime when a socket remains unconnected.
        if (hasNorth)
        {
            cliff.SetTile(new Vector3Int(-2, yMax, 0), Tile(I_TL));
            cliff.SetTile(new Vector3Int( 1, yMax, 0), Tile(I_TR));
        }
        if (hasSouth)
        {
            cliff.SetTile(new Vector3Int(-2, yMin, 0), Tile(I_BL));
            cliff.SetTile(new Vector3Int( 1, yMin, 0), Tile(I_BR));
        }
        if (hasWest)
        {
            cliff.SetTile(new Vector3Int(xMin,  1, 0), Tile(I_TL));
            cliff.SetTile(new Vector3Int(xMin, -2, 0), Tile(I_BL));
        }
        if (hasEast)
        {
            cliff.SetTile(new Vector3Int(xMax,  1, 0), Tile(I_TR));
            cliff.SetTile(new Vector3Int(xMax, -2, 0), Tile(I_BR));
        }
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    static TileBase Tile(int n) =>
        AssetDatabase.LoadAssetAtPath<TileBase>($"{TileDir}/dd_{n:D2}.asset");

    static Tilemap FindTilemap(GameObject root, string name)
    {
        foreach (var t in root.GetComponentsInChildren<Tilemap>(true))
            if (t.gameObject.name == name) return t;
        return null;
    }

    // Ensures Tilemap_Cliff has the full collider stack: TilemapCollider2D (usedByComposite)
    // + Rigidbody2D (Static) + CompositeCollider2D. Adds any missing components without
    // touching ones that are already correctly configured.
    static void EnsureCliffColliders(GameObject root)
    {
        Tilemap cliff = FindTilemap(root, "Tilemap_Cliff");
        if (cliff == null) return;
        var go = cliff.gameObject;

        var rb = go.GetComponent<Rigidbody2D>();
        if (rb == null) rb = go.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Static;

        var tmCol = go.GetComponent<TilemapCollider2D>();
        if (tmCol == null) tmCol = go.AddComponent<TilemapCollider2D>();
        tmCol.usedByComposite = true;

        var composite = go.GetComponent<CompositeCollider2D>();
        if (composite == null) composite = go.AddComponent<CompositeCollider2D>();
        composite.geometryType  = CompositeCollider2D.GeometryType.Outlines;
        composite.generationType = CompositeCollider2D.GenerationType.Synchronous;
    }

    // Adds a BoxCollider2D to any DoorSocket that doesn't have one.
    // The collider acts as a door blocker — the assembler disables it at runtime
    // when the socket is connected to an adjacent room.
    static void EnsureSocketBlockers(GameObject root)
    {
        foreach (var s in root.GetComponentsInChildren<DoorSocket>(true))
        {
            if (s.GetComponent<BoxCollider2D>() != null) continue;
            var col = s.gameObject.AddComponent<BoxCollider2D>();
            bool horizontal = s.direction == SocketDirection.North || s.direction == SocketDirection.South;
            col.size = horizontal ? new Vector2(2f, 0.5f) : new Vector2(0.5f, 2f);
        }
    }

    static bool HasSocketInDirection(GameObject root, SocketDirection dir)
    {
        foreach (var s in root.GetComponentsInChildren<DoorSocket>(true))
            if (s.direction == dir) return true;
        return false;
    }

    // Returns the wall row (Y cell) for a N/S socket, or fallback if none exists.
    static int SocketRow(GameObject root, SocketDirection dir, int fallback)
    {
        foreach (var s in root.GetComponentsInChildren<DoorSocket>(true))
            if (s.direction == dir)
                return Mathf.RoundToInt(s.transform.localPosition.y);
        return fallback;
    }

    // Returns the wall column (X cell) for an E/W socket, or fallback if none exists.
    static int SocketCol(GameObject root, SocketDirection dir, int fallback)
    {
        foreach (var s in root.GetComponentsInChildren<DoorSocket>(true))
            if (s.direction == dir)
                return Mathf.RoundToInt(s.transform.localPosition.x);
        return fallback;
    }

    // ── Debug: Spawn Test Room ────────────────────────────────────────────────

    /// <summary>
    /// Instantiates the first room prefab found in the active scene at (0,0) with all
    /// sockets closed (no connections). Logs a full diagnostic of every collider
    /// component on Tilemap_Cliff so you can confirm the stack is correct at runtime.
    /// Run via Tools > Restless > Spawn Test Room.
    /// </summary>
    [MenuItem("Tools/Restless/Spawn Test Room (Collider Debug)")]
    public static void SpawnTestRoom()
    {
        var prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { PrefabDir });
        GameObject prefab = null;
        foreach (var guid in prefabGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (go != null && go.GetComponent<Restless.Dream.Procedural.RoomController>() != null)
            { prefab = go; break; }
        }
        if (prefab == null) { Debug.LogError("[SpawnTestRoom] No room prefab found."); return; }

        var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        instance.transform.position = Vector3.zero;
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());

        // ── Diagnostic log ────────────────────────────────────────────────────
        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"[SpawnTestRoom] Spawned '{prefab.name}' at (0,0)");

        foreach (var tm in instance.GetComponentsInChildren<UnityEngine.Tilemaps.Tilemap>(true))
        {
            if (tm.gameObject.name != "Tilemap_Cliff") continue;
            var go = tm.gameObject;

            var rb      = go.GetComponent<Rigidbody2D>();
            var tmCol   = go.GetComponent<TilemapCollider2D>();
            var comp    = go.GetComponent<CompositeCollider2D>();

            tm.CompressBounds();
            int tileCount = 0;
            foreach (var pos in tm.cellBounds.allPositionsWithin)
                if (tm.HasTile(pos)) tileCount++;

            sb.AppendLine($"  Tilemap_Cliff on '{go.name}':");
            sb.AppendLine($"    Tiles painted   : {tileCount}");
            sb.AppendLine($"    Rigidbody2D     : {(rb   != null ? $"found (bodyType={rb.bodyType})"         : "MISSING")}");
            sb.AppendLine($"    TilemapCollider : {(tmCol != null ? $"found (usedByComposite={tmCol.usedByComposite})" : "MISSING")}");
            sb.AppendLine($"    CompositeCollider: {(comp != null ? $"found (genType={comp.generationType}, pathCount={comp.pathCount})" : "MISSING")}");

            if (tmCol != null)
            {
                // Sample a few wall tile collider types
                int gridCount = 0, noneCount = 0;
                foreach (var pos in tm.cellBounds.allPositionsWithin)
                {
                    var tile = tm.GetTile<UnityEngine.Tilemaps.Tile>(pos);
                    if (tile == null) continue;
                    if (tile.colliderType == UnityEngine.Tilemaps.Tile.ColliderType.None) noneCount++;
                    else gridCount++;
                }
                sb.AppendLine($"    Tile colliderType Grid/Sprite: {gridCount}  None: {noneCount}");
            }
        }

        Debug.Log(sb.ToString());
        Selection.activeGameObject = instance;
    }
}
