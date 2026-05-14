using UnityEngine;
using UnityEngine.SceneManagement;

namespace Restless.Core
{
    /// <summary>
    /// Ensures Bootstrap loads first regardless of which scene is entered in the Editor.
    /// In a build, Bootstrap is always the first scene (index 0) so this is a no-op.
    /// </summary>
    public static class BootstrapLoader
    {
#if UNITY_EDITOR
        static readonly string[] _standaloneScenes = { "RoomWorkshop" };

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void EnsureBootstrap()
        {
            var active = SceneManager.GetActiveScene();
            foreach (var name in _standaloneScenes)
                if (active.name == name) return;

            if (SceneManager.GetSceneByName("Bootstrap").isLoaded) return;
            SceneManager.LoadScene("Bootstrap", LoadSceneMode.Additive);
        }
#endif
    }
}
