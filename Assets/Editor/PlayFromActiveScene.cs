using UnityEditor;
using UnityEditor.SceneManagement;

/// <summary>
/// When the active scene is one of the listed debug/workshop scenes,
/// forces Play mode to start from that scene instead of the Bootstrap scene.
/// Restores default behaviour (start from Bootstrap) on exit.
/// </summary>
[InitializeOnLoad]
public static class PlayFromActiveScene
{
    static readonly string[] OverrideScenes = { "RoomWorkshop" };

    static PlayFromActiveScene()
    {
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingEditMode)
        {
            var active = EditorSceneManager.GetActiveScene();
            foreach (var name in OverrideScenes)
            {
                if (active.name != name) continue;
                var asset = AssetDatabase.LoadAssetAtPath<SceneAsset>(active.path);
                if (asset != null)
                {
                    EditorSceneManager.playModeStartScene = asset;
                    UnityEngine.Debug.Log($"[PlayFromActiveScene] Play override → {active.name}");
                }
                return;
            }
            // Active scene is not a workshop scene — restore default (Bootstrap)
            EditorSceneManager.playModeStartScene = null;
        }
        else if (state == PlayModeStateChange.EnteredEditMode)
        {
            EditorSceneManager.playModeStartScene = null;
        }
    }
}
