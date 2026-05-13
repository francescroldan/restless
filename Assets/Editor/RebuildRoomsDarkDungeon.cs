using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;
using Restless.Dream.Procedural;
using System.IO;

/// <summary>
/// Rebuilds all room prefabs in Assets/_Project/Prefabs/Rooms/ using the
/// DarkDungeon tileset. Run via Tools > Restless > Rebuild Rooms (DarkDungeon).
/// </summary>
public static class RebuildRoomsDarkDungeon
{
    const string TileDir   = "Assets/_Project/Art/Tiles/DarkDungeon";
    const string PrefabDir = "Assets/_Project/Prefabs/Rooms";

    // ── Tile mapping (confirmed by designer) ──────────────────────────────────
    // Outer corners
    const int TL = 25, TR = 27, BL = 44, BR = 43;
    // Outer edges  (top and bottom share the same tile)
    const int T = 26, B = 26, L = 30, R = 35;
    // Inner corners (concave, used when room opens towards another room)
    const int I_TL = 28, I_TR = 29, I_BL = 32, I_BR = 33;
    // Floor base + variants (scattered decoration)
    static readonly int[] FloorBase     = { 1 };
    static readonly int[] FloorVariants = { 1, 1, 1, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
    // Door tiles (bottom wall opening)
    const int DoorClosed = 23, DoorOpen = 24;

    // Wall tiles that need collision (outer edges, corners, inner corners)
    static readonly int[] WallTiles  = { 25, 26, 27, 28, 29, 30, 32, 33, 35, 36, 37, 38, 39, 40, 43, 44 };
    // Door-closed tile also blocks passage
    static readonly int[] DoorTiles  = { 23 };

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
                RebuildTilemaps(scope.prefabContentsRoot, ctrl);
                rebuilt++;
                Debug.Log($"[RebuildRooms] Rebuilt {prefab.name}");
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"[RebuildRooms] Done — {rebuilt} prefabs rebuilt with DarkDungeon tileset.");
    }

    // ─────────────────────────────────────────────────────────────────────────

    static void RebuildTilemaps(GameObject root, RoomController ctrl)
    {
        // Find floor + cliff tilemaps under the prefab
        var floor = FindTilemap(root, "Tilemap_Floor");
        var cliff = FindTilemap(root, "Tilemap_Cliff");
        if (floor == null || cliff == null)
        {
            Debug.LogWarning($"[RebuildRooms] Missing Tilemap_Floor or Tilemap_Cliff on {root.name}");
            return;
        }

        // Derive grid bounds from the current cliff tilemap content
        cliff.CompressBounds();
        var bounds = cliff.cellBounds;
        if (bounds.size == Vector3Int.zero)
        {
            Debug.LogWarning($"[RebuildRooms] Empty cliff tilemap on {root.name}, skipping.");
            return;
        }

        int xMin = bounds.xMin, xMax = bounds.xMax - 1;
        int yMin = bounds.yMin, yMax = bounds.yMax - 1;
        int xIn0 = xMin + 1, xIn1 = xMax - 1;
        int yIn0 = yMin + 1, yIn1 = yMax - 1;

        // Detect which sides have door openings (gap in the original wall)
        bool hasNorth = HasGap(cliff, xIn0, xIn1, yMax);
        bool hasSouth = HasGap(cliff, xIn0, xIn1, yMin);
        bool hasEast  = HasGap(cliff, yIn0, yIn1, xMax, vertical: true);
        bool hasWest  = HasGap(cliff, yIn0, yIn1, xMin, vertical: true);

        // Clear both tilemaps
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
        for (int x = xIn0; x <= xIn1; x++)
        {
            bool isDoor = hasNorth && (x == -1 || x == 0);
            cliff.SetTile(new Vector3Int(x, yMax, 0), isDoor ? null : Tile(T));
            if (isDoor) floor.SetTile(new Vector3Int(x, yMax, 0), Tile(1));
        }

        // ── Bottom edge ───────────────────────────────────────────────────────
        for (int x = xIn0; x <= xIn1; x++)
        {
            bool isDoor = hasSouth && (x == -1 || x == 0);
            cliff.SetTile(new Vector3Int(x, yMin, 0), isDoor ? Tile(DoorClosed) : Tile(B));
            if (isDoor) floor.SetTile(new Vector3Int(x, yMin, 0), Tile(1));
        }

        // ── Left edge ─────────────────────────────────────────────────────────
        for (int y = yIn0; y <= yIn1; y++)
        {
            bool isDoor = hasWest && (y == -1 || y == 0);
            cliff.SetTile(new Vector3Int(xMin, y, 0), isDoor ? null : Tile(L));
            if (isDoor) floor.SetTile(new Vector3Int(xMin, y, 0), Tile(1));
        }

        // ── Right edge ────────────────────────────────────────────────────────
        for (int y = yIn0; y <= yIn1; y++)
        {
            bool isDoor = hasEast && (y == -1 || y == 0);
            cliff.SetTile(new Vector3Int(xMax, y, 0), isDoor ? null : Tile(R));
            if (isDoor) floor.SetTile(new Vector3Int(xMax, y, 0), Tile(1));
        }

        // ── Inner corners at door edges ───────────────────────────────────────
        // When a door is cut into a wall the adjacent floor needs an inner corner
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
            cliff.SetTile(new Vector3Int(xMin,  1, 0), Tile(I_TL));  // above door
            cliff.SetTile(new Vector3Int(xMin, -2, 0), Tile(I_BL));  // below door
        }
        if (hasEast)
        {
            cliff.SetTile(new Vector3Int(xMax,  1, 0), Tile(I_TR));  // above door
            cliff.SetTile(new Vector3Int(xMax, -2, 0), Tile(I_BR));  // below door
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

    // Returns true if there is a gap (missing tile) at x==-1 or x==0 in a horizontal row
    static bool HasGap(Tilemap tm, int xFrom, int xTo, int y)
    {
        for (int x = xFrom; x <= xTo; x++)
            if ((x == -1 || x == 0) && tm.GetTile(new Vector3Int(x, y, 0)) == null)
                return true;
        return false;
    }

    // Vertical variant: checks y==-1 or y==0 in a vertical column
    static bool HasGap(Tilemap tm, int yFrom, int yTo, int x, bool vertical)
    {
        for (int y = yFrom; y <= yTo; y++)
            if ((y == -1 || y == 0) && tm.GetTile(new Vector3Int(x, y, 0)) == null)
                return true;
        return false;
    }
}
