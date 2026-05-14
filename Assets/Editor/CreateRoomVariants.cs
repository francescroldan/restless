using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;
using Restless.Dream.Procedural;

/// <summary>
/// Creates new room prefab variants with different sizes and socket configurations.
/// Run via Tools > Restless > Create Room Variants.
/// After running, execute Rebuild Rooms (DarkDungeon) to paint the tilemaps.
/// </summary>
public static class CreateRoomVariants
{
    const string PrefabDir = "Assets/_Project/Prefabs/Rooms";
    const string DataDir   = "Assets/_Project/Data/Rooms";

    // ── Variant descriptors ───────────────────────────────────────────────────

    struct RoomVariant
    {
        public string           id;
        public RoomSize         size;
        public RoomType[]       types;
        public SocketDirection[] sockets;   // null = all four
        public float            danger;
        public float            surrealism;
        public bool             supportsThreats;
        public bool             supportsFragments;
    }

    static readonly RoomVariant[] Variants =
    {
        // ── Safe variants ─────────────────────────────────────────────────────
        new RoomVariant {
            id = "safe_room_medium",
            size = RoomSize.Medium,
            types = new[]{ RoomType.Safe },
            sockets = null,
            danger = 0.15f, surrealism = 0.1f, supportsThreats = false
        },
        new RoomVariant {
            id = "safe_corridor",
            size = RoomSize.Medium,
            types = new[]{ RoomType.Safe },
            sockets = new[]{ SocketDirection.North, SocketDirection.South },
            danger = 0.15f, surrealism = 0.1f, supportsThreats = false
        },
        new RoomVariant {
            id = "safe_corner",
            size = RoomSize.Small,
            types = new[]{ RoomType.Safe },
            sockets = new[]{ SocketDirection.North, SocketDirection.East },
            danger = 0.15f, surrealism = 0.15f, supportsThreats = false
        },
        new RoomVariant {
            id = "safe_room_large",
            size = RoomSize.Large,
            types = new[]{ RoomType.Safe, RoomType.Encounter },
            sockets = null,
            danger = 0.25f, surrealism = 0.2f, supportsThreats = true
        },

        // ── Encounter variants ────────────────────────────────────────────────
        new RoomVariant {
            id = "encounter_room_medium",
            size = RoomSize.Medium,
            types = new[]{ RoomType.Encounter },
            sockets = null,
            danger = 0.5f, surrealism = 0.3f, supportsThreats = true
        },
        new RoomVariant {
            id = "encounter_corridor",
            size = RoomSize.Medium,
            types = new[]{ RoomType.Encounter },
            sockets = new[]{ SocketDirection.North, SocketDirection.South },
            danger = 0.5f, surrealism = 0.3f, supportsThreats = true
        },
        new RoomVariant {
            id = "encounter_junction",
            size = RoomSize.Medium,
            types = new[]{ RoomType.Encounter, RoomType.Safe },
            sockets = new[]{ SocketDirection.North, SocketDirection.South, SocketDirection.East },
            danger = 0.45f, surrealism = 0.25f, supportsThreats = true
        },
        new RoomVariant {
            id = "encounter_hall",
            size = RoomSize.Large,
            types = new[]{ RoomType.Encounter },
            sockets = null,
            danger = 0.6f, surrealism = 0.4f, supportsThreats = true
        },

        // ── Memory variants ───────────────────────────────────────────────────
        new RoomVariant {
            id = "memory_vault",
            size = RoomSize.Large,
            types = new[]{ RoomType.Memory },
            sockets = null,
            danger = 0.2f, surrealism = 0.7f, supportsThreats = false, supportsFragments = true
        },

        // ── Ritual variants ───────────────────────────────────────────────────
        new RoomVariant {
            id = "ritual_room_medium",
            size = RoomSize.Medium,
            types = new[]{ RoomType.Ritual },
            sockets = null,
            danger = 0.7f, surrealism = 0.6f, supportsThreats = true
        },
        new RoomVariant {
            id = "ritual_corridor",
            size = RoomSize.Medium,
            types = new[]{ RoomType.Ritual },
            sockets = new[]{ SocketDirection.North, SocketDirection.South },
            danger = 0.7f, surrealism = 0.6f, supportsThreats = true
        },
        new RoomVariant {
            id = "ritual_hall",
            size = RoomSize.Large,
            types = new[]{ RoomType.Ritual, RoomType.Encounter },
            sockets = null,
            danger = 0.75f, surrealism = 0.65f, supportsThreats = true
        },

        // ── Dead-end variants ─────────────────────────────────────────────────
        new RoomVariant {
            id = "dead_end_small",
            size = RoomSize.Small,
            types = new[]{ RoomType.DeadEnd },
            sockets = null,
            danger = 0.4f, surrealism = 0.35f, supportsThreats = true
        },
        new RoomVariant {
            id = "dead_end_alcove",
            size = RoomSize.Small,
            types = new[]{ RoomType.DeadEnd, RoomType.Memory },
            sockets = new[]{ SocketDirection.North, SocketDirection.South },
            danger = 0.3f, surrealism = 0.5f, supportsThreats = false, supportsFragments = true
        },

        // ── Dungeon biome — Sprint 4 additions ───────────────────────────────
        new RoomVariant {
            id = "dungeon_corridor_b",
            size = RoomSize.Small,
            types = new[]{ RoomType.Safe },
            sockets = new[]{ SocketDirection.East, SocketDirection.West },
            danger = 0.2f, surrealism = 0.15f, supportsThreats = false
        },
        new RoomVariant {
            id = "operating_theatre",
            size = RoomSize.Medium,
            types = new[]{ RoomType.Ritual, RoomType.Memory },
            sockets = null,
            danger = 0.75f, surrealism = 0.65f, supportsThreats = true, supportsFragments = true
        },
        new RoomVariant {
            id = "flooded_basement",
            size = RoomSize.Medium,
            types = new[]{ RoomType.Encounter },
            sockets = null,
            danger = 0.6f, surrealism = 0.5f, supportsThreats = true
        },
        new RoomVariant {
            id = "nurses_station",
            size = RoomSize.Small,
            types = new[]{ RoomType.DeadEnd, RoomType.Safe },
            sockets = new[]{ SocketDirection.North },
            danger = 0.2f, surrealism = 0.25f, supportsThreats = false
        },
    };

    // ── Entry point ───────────────────────────────────────────────────────────

    [MenuItem("Tools/Restless/Create Room Variants")]
    public static void CreateAll()
    {
        int created = 0, skipped = 0;

        foreach (var v in Variants)
        {
            string prefabPath = $"{PrefabDir}/{v.id}.prefab";
            if (AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath) != null)
            {
                // Keep the existing prefab but refresh tileExtents on the definition
                // so RebuildRoomsDarkDungeon can repaint with the correct room shape.
                string defPath = $"{DataDir}/RoomDef_{v.id}.asset";
                var existing = AssetDatabase.LoadAssetAtPath<RoomDefinition>(defPath);
                if (existing != null)
                {
                    existing.tileExtents = ShapeHalfExtents(v);
                    EditorUtility.SetDirty(existing);
                }
                skipped++;
                continue;
            }

            var def = CreateDefinition(v);
            CreatePrefab(v, def, prefabPath);
            created++;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"[CreateRoomVariants] Done — {created} created, {skipped} skipped. Run Rebuild Rooms (DarkDungeon) to paint tilemaps.");
    }

    // ── RoomDefinition asset ──────────────────────────────────────────────────

    static RoomDefinition CreateDefinition(RoomVariant v)
    {
        string assetPath = $"{DataDir}/RoomDef_{v.id}.asset";

        var existing = AssetDatabase.LoadAssetAtPath<RoomDefinition>(assetPath);
        if (existing != null) return existing;

        var def = ScriptableObject.CreateInstance<RoomDefinition>();
        def.id               = v.id;
        def.size             = v.size;
        def.types            = v.types;
        def.socketDirections = v.sockets;
        def.dangerLevel      = v.danger;
        def.surrealism       = v.surrealism;
        def.supportsThreats   = v.supportsThreats;
        def.supportsFragments = v.supportsFragments;
        def.tileExtents       = ShapeHalfExtents(v);

        AssetDatabase.CreateAsset(def, assetPath);
        return def;
    }

    // ── Prefab ────────────────────────────────────────────────────────────────

    static void CreatePrefab(RoomVariant v, RoomDefinition def, string prefabPath)
    {
        var root = new GameObject(v.id);

        // RoomController on root
        var ctrl = root.AddComponent<RoomController>();
        // Assign definition + default spawn bounds via SerializedObject so they survive prefab save
        var ext = ShapeHalfExtents(v);
        var so = new SerializedObject(ctrl);
        so.FindProperty("_definition").objectReferenceValue = def;
        var spawnProp = so.FindProperty("_spawnBounds");
        spawnProp.FindPropertyRelative("m_Center").vector3Value = Vector3.zero;
        spawnProp.FindPropertyRelative("m_Extent").vector3Value =
            new Vector3(Mathf.Max(0.5f, ext.x - 1.5f), Mathf.Max(0.5f, ext.y - 1.5f), 1f);
        so.ApplyModifiedPropertiesWithoutUndo();

        // ── Tilemaps hierarchy ────────────────────────────────────────────────
        var tilemapsGO = new GameObject("Tilemaps");
        tilemapsGO.transform.SetParent(root.transform, false);

        var gridGO = new GameObject("Grid");
        gridGO.transform.SetParent(tilemapsGO.transform, false);
        gridGO.AddComponent<Grid>();

        var floorGO = new GameObject("Tilemap_Floor");
        floorGO.transform.SetParent(gridGO.transform, false);
        floorGO.AddComponent<Tilemap>();
        var floorRenderer = floorGO.AddComponent<TilemapRenderer>();
        floorRenderer.sortingOrder = -5;

        var cliffGO = new GameObject("Tilemap_Cliff");
        cliffGO.transform.SetParent(gridGO.transform, false);
        var cliffTilemap = cliffGO.AddComponent<Tilemap>();
        var cliffRenderer = cliffGO.AddComponent<TilemapRenderer>();
        cliffRenderer.sortingOrder = 0;

        // Composite collider for walls
        var rb = cliffGO.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Static;
        var composite = cliffGO.AddComponent<CompositeCollider2D>();
        composite.geometryType = CompositeCollider2D.GeometryType.Outlines;

        // Seed the cliff tilemap with a single placeholder tile so the bounds
        // are non-zero — RebuildRoomsDarkDungeon reads cellBounds to paint walls.
        // The Rebuild tool will replace this with the correct tiles.
        SeedCliffBounds(cliffTilemap, ShapeHalfExtents(v));

        // ── Sockets ───────────────────────────────────────────────────────────
        var socketsGO = new GameObject("Sockets");
        socketsGO.transform.SetParent(root.transform, false);

        var dirs = (v.sockets != null && v.sockets.Length > 0)
            ? v.sockets
            : new[]{ SocketDirection.North, SocketDirection.South, SocketDirection.East, SocketDirection.West };

        Vector2 extents = ShapeHalfExtents(v);

        foreach (var dir in dirs)
        {
            var sGO = new GameObject($"Socket_{dir}");
            sGO.transform.SetParent(socketsGO.transform, false);
            sGO.transform.localPosition = SocketLocalPos(dir, extents);

            var socket = sGO.AddComponent<DoorSocket>();
            socket.direction = dir;
            socket.width     = 2f;

            var col = sGO.AddComponent<BoxCollider2D>();
            bool horizontal = dir == SocketDirection.North || dir == SocketDirection.South;
            col.size = horizontal ? new Vector2(2f, 0.5f) : new Vector2(0.5f, 2f);
        }

        // ── Save prefab ───────────────────────────────────────────────────────
        PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
        Object.DestroyImmediate(root);

        Debug.Log($"[CreateRoomVariants] Created {v.id}");
    }

    // Seeds the cliff tilemap bounds so RebuildRoomsDarkDungeon can read cellBounds.
    // Places corner tiles at the expected wall positions for the given size.
    static void SeedCliffBounds(Tilemap cliff, Vector2 ext)
    {
        int xMin = -(int)ext.x;
        int xMax =  (int)ext.x;
        int yMin = -(int)ext.y;
        int yMax =  (int)ext.y;

        // Place a dummy white tile at the four corners so bounds are set correctly.
        // Rebuild Rooms will overwrite everything with real tiles.
        var tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        var sprite     = Sprite.Create(tex, new Rect(0, 0, 1, 1), Vector2.one * 0.5f, 1f);
        var dummyTile  = ScriptableObject.CreateInstance<Tile>();
        dummyTile.sprite = sprite;

        cliff.SetTile(new Vector3Int(xMin, yMin, 0), dummyTile);
        cliff.SetTile(new Vector3Int(xMax, yMax, 0), dummyTile);
        cliff.CompressBounds();
    }

    // ── Helpers (mirrors RebuildRoomsDarkDungeon formulas) ────────────────────

    static Vector3 SocketLocalPos(SocketDirection dir, Vector2 extents) => dir switch
    {
        SocketDirection.North => new Vector3(-0.5f,  extents.y,       0),
        SocketDirection.South => new Vector3(-0.5f, -extents.y,       0),
        SocketDirection.East  => new Vector3( extents.x,       -0.5f,  0),
        SocketDirection.West  => new Vector3(-extents.x,       -0.5f, 0),
        _                     => Vector3.zero
    };

    static Vector2 SizeToHalfExtents(RoomSize size) => size switch
    {
        RoomSize.Small    => new Vector2(4f, 3f),
        RoomSize.Large    => new Vector2(8f, 5f),
        RoomSize.Landmark => new Vector2(8f, 5f),
        _                 => new Vector2(6f, 4f),
    };

    // Returns the actual tile half-extents for a variant, considering its shape.
    // Pure N-S corridors are narrow in X; pure E-W corridors narrow in Y.
    static Vector2 ShapeHalfExtents(RoomVariant v)
    {
        var base_ = SizeToHalfExtents(v.size);
        var dirs   = v.sockets;
        if (dirs == null || dirs.Length == 0 || dirs.Length >= 4) return base_;

        bool hasN = System.Array.IndexOf(dirs, SocketDirection.North) >= 0;
        bool hasS = System.Array.IndexOf(dirs, SocketDirection.South) >= 0;
        bool hasE = System.Array.IndexOf(dirs, SocketDirection.East)  >= 0;
        bool hasW = System.Array.IndexOf(dirs, SocketDirection.West)  >= 0;

        if (hasN && hasS && !hasE && !hasW) return new Vector2(3f, base_.y);  // N-S corridor
        if (hasE && hasW && !hasN && !hasS) return new Vector2(base_.x, 3f);  // E-W corridor
        return base_;
    }
}
