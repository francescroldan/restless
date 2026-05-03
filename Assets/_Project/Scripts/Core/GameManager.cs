using UnityEngine;

namespace Restless.Core
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public enum GameState { Vigilia, Transitioning, Dream }

        public GameState State { get; private set; } = GameState.Vigilia;

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(this);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void EnterDream()
        {
            Debug.Log($"[GameManager] EnterDream — State={State}");
            if (State != GameState.Vigilia) { Debug.LogWarning("[GameManager] EnterDream blocked: State is not Vigilia"); return; }
            State = GameState.Transitioning;
            SceneLoader.Instance.LoadDream();
        }

        public void ExitDream(bool abrupt)
        {
            Debug.Log($"[GameManager] ExitDream — State={State} abrupt={abrupt}");
            if (State != GameState.Dream && State != GameState.Transitioning) { Debug.LogWarning("[GameManager] ExitDream blocked: State is not Dream/Transitioning"); return; }
            State = GameState.Transitioning;
            SceneLoader.Instance.LoadVigilia(abrupt);
        }

        public void OnDreamSceneReady() => State = GameState.Dream;

        public void OnVigiliaSceneReady() => State = GameState.Vigilia;
    }
}
