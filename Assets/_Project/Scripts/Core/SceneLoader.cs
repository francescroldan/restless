using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

namespace Restless.Core
{
    public class SceneLoader : MonoBehaviour
    {
        public static SceneLoader Instance { get; private set; }

        [SerializeField] private AssetReference _dreamSceneRef;
        [SerializeField] private AssetReference _vigiliaSceneRef;

        public bool LastWakeUpWasAbrupt { get; private set; }

        private AsyncOperationHandle<SceneInstance> _loadedSceneHandle;
        private bool _hasDreamLoaded;

        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void LoadDream()
        {
            StartCoroutine(LoadSceneRoutine(_dreamSceneRef, onLoaded: () =>
                GameManager.Instance.OnDreamSceneReady()));
        }

        public void LoadVigilia(bool abrupt)
        {
            LastWakeUpWasAbrupt = abrupt;
            StartCoroutine(UnloadDreamAndLoadVigilia());
        }

        private IEnumerator UnloadDreamAndLoadVigilia()
        {
            if (_hasDreamLoaded)
            {
                yield return Addressables.UnloadSceneAsync(_loadedSceneHandle);
                _hasDreamLoaded = false;
            }

            yield return LoadSceneRoutine(_vigiliaSceneRef, onLoaded: () =>
                GameManager.Instance.OnVigiliaSceneReady());
        }

        private IEnumerator LoadSceneRoutine(AssetReference sceneRef, System.Action onLoaded)
        {
            var handle = Addressables.LoadSceneAsync(sceneRef, LoadSceneMode.Additive);
            yield return handle;

            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                _loadedSceneHandle = handle;
                _hasDreamLoaded = sceneRef == _dreamSceneRef;
                onLoaded?.Invoke();
            }
            else
            {
                Debug.LogError($"[SceneLoader] Failed to load scene: {sceneRef}");
            }
        }
    }
}
