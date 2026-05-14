using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using Restless.Core;
using Restless.Dream;

/// <summary>
/// One-shot setup for the RoomWorkshop test scene.
/// Run via Tools > Restless > Setup RoomWorkshop Scene.
///
/// What it does:
///   1. Opens (or creates) Assets/_Project/Scenes/RoomWorkshop.unity
///   2. Extracts the Protagonist from Dream.unity as a reusable prefab
///   3. Adds a Main Camera with CameraFollow
///   4. Adds a RoomWorkshopBootstrap wired to the protagonist prefab
///      and to the first available room prefab
///   5. Saves the scene
/// </summary>
public static class SetupRoomWorkshop
{
    const string WorkshopScenePath  = "Assets/_Project/Scenes/RoomWorkshop.unity";
    const string DreamScenePath     = "Assets/_Project/Scenes/Dream.unity";
    const string ProtoPrefabPath    = "Assets/_Project/Prefabs/Characters/Protagonist.prefab";
    const string RoomPrefabDir      = "Assets/_Project/Prefabs/Rooms";

    [MenuItem("Tools/Restless/Setup RoomWorkshop Scene")]
    public static void Setup()
    {
        // ── 1. Extract Protagonist prefab from Dream scene ──────────────────
        var protoPrefab = ExtractProtagonistPrefab();

        // ── 2. Open / create the workshop scene ─────────────────────────────
        var workshopScene = OpenOrCreateScene();

        // ── 3. Clear leftover objects except lights ──────────────────────────
        foreach (var root in workshopScene.GetRootGameObjects())
        {
            if (root.name == "Directional Light") continue;
            Object.DestroyImmediate(root);
        }

        // ── 4. Camera ────────────────────────────────────────────────────────
        var camGO = new GameObject("Main Camera");
        camGO.tag = "MainCamera";
        var cam = camGO.AddComponent<Camera>();
        cam.orthographic     = true;
        cam.orthographicSize = 6f;
        cam.transform.position = new Vector3(0f, 0f, -10f);
        camGO.AddComponent<AudioListener>();
        camGO.AddComponent<CameraFollow>();

        // ── 5. Bootstrap ─────────────────────────────────────────────────────
        var bootstrapGO = new GameObject("RoomWorkshopBootstrap");
        var bootstrap   = bootstrapGO.AddComponent<RoomWorkshopBootstrap>();

        var so = new SerializedObject(bootstrap);

        if (protoPrefab != null)
            so.FindProperty("_protagonistPrefab").objectReferenceValue = protoPrefab;

        // Wire the first room prefab found
        var roomGuids = AssetDatabase.FindAssets("t:Prefab", new[] { RoomPrefabDir });
        if (roomGuids.Length > 0)
        {
            string firstPath = AssetDatabase.GUIDToAssetPath(roomGuids[0]);
            var roomPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(firstPath);
            so.FindProperty("_roomPrefab").objectReferenceValue = roomPrefab;
        }

        so.ApplyModifiedPropertiesWithoutUndo();

        // ── 6. Save ──────────────────────────────────────────────────────────
        EditorSceneManager.SaveScene(workshopScene, WorkshopScenePath);
        AssetDatabase.Refresh();

        Debug.Log($"[SetupRoomWorkshop] Done. Open {WorkshopScenePath} and press Play.");

        // Make sure it's added to Build Settings (won't override existing entries)
        EnsureInBuildSettings(WorkshopScenePath);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    static GameObject ExtractProtagonistPrefab()
    {
        var existing = AssetDatabase.LoadAssetAtPath<GameObject>(ProtoPrefabPath);
        if (existing != null) return existing;

        // Open Dream additively to find the Player
        var dreamScene = EditorSceneManager.OpenScene(DreamScenePath, OpenSceneMode.Additive);
        GameObject protagonistGO = null;

        foreach (var root in dreamScene.GetRootGameObjects())
        {
            var tagged = FindTaggedInChildren(root, "Player");
            if (tagged != null) { protagonistGO = tagged; break; }
        }

        GameObject prefab = null;
        if (protagonistGO != null)
        {
            prefab = PrefabUtility.SaveAsPrefabAsset(protagonistGO, ProtoPrefabPath);
            Debug.Log($"[SetupRoomWorkshop] Protagonist prefab saved → {ProtoPrefabPath}");
        }
        else
        {
            Debug.LogWarning("[SetupRoomWorkshop] Could not find a 'Player'-tagged object in Dream.unity. " +
                             "Assign _protagonistPrefab manually in RoomWorkshopBootstrap.");
        }

        EditorSceneManager.CloseScene(dreamScene, true);
        return prefab;
    }

    static GameObject FindTaggedInChildren(GameObject root, string tag)
    {
        if (root.CompareTag(tag)) return root;
        foreach (Transform child in root.transform)
        {
            var found = FindTaggedInChildren(child.gameObject, tag);
            if (found != null) return found;
        }
        return null;
    }

    static Scene OpenOrCreateScene()
    {
        var existing = AssetDatabase.LoadAssetAtPath<SceneAsset>(WorkshopScenePath);
        if (existing != null)
            return EditorSceneManager.OpenScene(WorkshopScenePath, OpenSceneMode.Single);

        var newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        EditorSceneManager.SaveScene(newScene, WorkshopScenePath);
        return newScene;
    }

    static void EnsureInBuildSettings(string scenePath)
    {
        var scenes = EditorBuildSettings.scenes;
        foreach (var s in scenes)
            if (s.path == scenePath) return;

        var newScenes = new EditorBuildSettingsScene[scenes.Length + 1];
        System.Array.Copy(scenes, newScenes, scenes.Length);
        newScenes[scenes.Length] = new EditorBuildSettingsScene(scenePath, false);
        EditorBuildSettings.scenes = newScenes;
    }
}
