using UnityEngine;
using UnityEditor;
using System.Reflection;

/// <summary>
/// Creates RestlessnessAudioMixer.mixer with 4 snapshots (Calm/Tense/Critical/Overload).
/// Run once via: Restless > Create Audio Mixer Setup
/// After running, assign snapshots to RestlessnessAudioController in the Dream scene Inspector.
/// </summary>
public static class CreateAudioMixerSetup
{
    const string MixerPath = "Assets/_Project/Audio/RestlessnessAudioMixer.mixer";

    [MenuItem("Restless/Create Audio Mixer Setup")]
    public static void Create()
    {
        if (AssetDatabase.LoadAssetAtPath<UnityEngine.Audio.AudioMixer>(MixerPath) != null)
        {
            Debug.Log("[AudioMixerSetup] Mixer already exists at " + MixerPath);
            return;
        }

        var editorAssembly = typeof(AudioImporter).Assembly;
        var controllerType = editorAssembly.GetType("UnityEditor.Audio.AudioMixerController");
        if (controllerType == null) { Debug.LogError("[AudioMixerSetup] AudioMixerController not found"); return; }

        // Create mixer via internal static factory if available, else direct instantiation
        object controller = null;
        var createMethod = controllerType.GetMethod("Create",
            BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
        if (createMethod != null)
            controller = createMethod.Invoke(null, null);
        else
            controller = ScriptableObject.CreateInstance(controllerType);

        if (controller == null) { Debug.LogError("[AudioMixerSetup] Could not create mixer"); return; }

        var mixerAsset = controller as ScriptableObject;
        AssetDatabase.CreateAsset(mixerAsset, MixerPath);

        // Add 4 snapshots as sub-assets
        var snapshotType = editorAssembly.GetType("UnityEditor.Audio.AudioMixerSnapshotController");
        if (snapshotType != null)
        {
            string[] snapNames = { "Calm", "Tense", "Critical", "Overload" };
            foreach (var name in snapNames)
            {
                var snap = ScriptableObject.CreateInstance(snapshotType);
                if (snap != null)
                {
                    snap.name = name;
                    AssetDatabase.AddObjectToAsset(snap, MixerPath);
                    Debug.Log($"[AudioMixerSetup] Snapshot '{name}' added");
                }
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"[AudioMixerSetup] RestlessnessAudioMixer created at {MixerPath}");
        Selection.activeObject = AssetDatabase.LoadAssetAtPath<Object>(MixerPath);
    }
}
