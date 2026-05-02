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
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void EnterDream()
        {
            if (State != GameState.Vigilia) return;
            State = GameState.Transitioning;
            SceneLoader.Instance.LoadDream();
        }

        public void ExitDream(bool abrupt)
        {
            if (State != GameState.Dream && State != GameState.Transitioning) return;
            State = GameState.Transitioning;
            SceneLoader.Instance.LoadVigilia(abrupt);
        }

        public void OnDreamSceneReady() => State = GameState.Dream;

        public void OnVigiliaSceneReady() => State = GameState.Vigilia;
    }
}
