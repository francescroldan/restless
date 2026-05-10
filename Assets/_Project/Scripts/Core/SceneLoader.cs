using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Restless.Core
{
    public class SceneLoader : MonoBehaviour
    {
        public static SceneLoader Instance { get; private set; }

        [SerializeField] private string _dreamSceneName    = "Dream";
        [SerializeField] private string _vigiliaSceneName  = "Vigil";
        [SerializeField] private string _gameOverSceneName = "GameOver";

        public bool LastWakeUpWasAbrupt  { get; private set; }
        public bool LastVigiliaCameFromDream { get; private set; }
        public GameManager.GameOverType LastGameOverType { get; private set; }

        private void Awake()
        {
            if (Instance != null) { Destroy(this); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void LoadDream()
        {
            StartCoroutine(LoadScene(_dreamSceneName, onLoaded: () =>
                GameManager.Instance?.OnDreamSceneReady()));
        }

        public void LoadVigilia(bool abrupt, bool fromDream = false)
        {
            LastWakeUpWasAbrupt      = abrupt;
            LastVigiliaCameFromDream = fromDream;
            StartCoroutine(LoadScene(_vigiliaSceneName, onLoaded: () =>
                GameManager.Instance?.OnVigiliaSceneReady()));
        }

        public void LoadGameOver(GameManager.GameOverType type)
        {
            LastGameOverType = type;
            StartCoroutine(LoadScene(_gameOverSceneName, onLoaded: null));
        }

        private IEnumerator LoadScene(string sceneName, System.Action onLoaded)
        {
            Debug.Log($"[SceneLoader] LoadSceneAsync '{sceneName}'");
            var op = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
            if (op == null) { Debug.LogError($"[SceneLoader] LoadSceneAsync returned null — '{sceneName}' not in Build Settings?"); yield break; }
            yield return op;
            Debug.Log($"[SceneLoader] '{sceneName}' loaded");
            onLoaded?.Invoke();
        }
    }
}
