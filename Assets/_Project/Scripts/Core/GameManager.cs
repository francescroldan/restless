using System.Collections.Generic;
using UnityEngine;
using Restless.Dream;
using Restless.Vigil;

namespace Restless.Core
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public enum GameState    { Vigilia, Transitioning, Dream }
        public enum GameOverType { Physical, Mental }

        public GameState State { get; private set; } = GameState.Vigilia;

        [SerializeField] private ProtagonistState _protagonistState;
        [SerializeField, Range(0f, 30f)] private float _normalRunMentalCost        = 3f;
        [SerializeField, Range(0f, 30f)] private float _normalRunPhysicalCost      = 2f;
        [SerializeField, Range(0f, 50f)] private float _abruptWakeUpMentalDamage   = 20f;
        [SerializeField, Range(0f, 50f)] private float _abruptWakeUpPhysicalDamage = 15f;
        [SerializeField, Range(0,  5)]   private int   _abruptFragmentLoss         = 1;
        [SerializeField, Range(1,  30)]  private int   _demoFragmentTarget         = 12;

        [Header("Aliados disponibles al iniciar (debug)")]
        [SerializeField] private List<AllyToggle> _startingAllies = new();

        [System.Serializable]
        public class AllyToggle
        {
            public Vigil.AllyData ally;
            public bool           active = true;
        }

        public int DemoFragmentTarget => _demoFragmentTarget;

#if UNITY_EDITOR
        public ProtagonistState ProtagonistStateDebug => _protagonistState;
#endif

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(this);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
#if UNITY_EDITOR
            _protagonistState?.ResetForNewGame();
#endif
        }

        private void Start() => UnlockStartingAllies();

        private void UnlockStartingAllies()
        {
            if (SaveManager.Instance == null || _startingAllies == null) return;
            foreach (var t in _startingAllies)
                if (t.active && t.ally != null) SaveManager.Instance.UnlockAlly(t.ally.id);

            // Refresh room if already loaded (Start() order issue)
            Vigil.VigiliaRoomController.Instance?.RefreshAllySlots();
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
            float healthMult = RunConfig.Current?.healthCostMultiplier ?? 1f;
            if (abrupt)
                _protagonistState?.ApplyAbruptWakeUp(_abruptWakeUpMentalDamage * healthMult, _abruptWakeUpPhysicalDamage * healthMult);
            else
                _protagonistState?.ApplyNormalRun(_normalRunMentalCost * healthMult, _normalRunPhysicalCost * healthMult);

            // Persist collected fragments — abrupt wake-up loses some
            if (DreamInventory.Instance != null && SaveManager.Instance != null)
            {
                var ids = new List<string>();
                foreach (var pf in DreamInventory.Instance.PlacedFragments)
                    if (pf.Fragment != null) ids.Add(pf.Fragment.fragmentId);
                SaveManager.Instance.CommitRunFragments(ids, abrupt, _abruptFragmentLoss);
            }

            // Check for game over before returning to Vigilia
            if (_protagonistState != null)
            {
                if (_protagonistState.physicalHealth <= 0f)
                {
                    SceneLoader.Instance.LoadGameOver(GameOverType.Physical);
                    return;
                }
                if (_protagonistState.mentalHealth <= 0f)
                {
                    SceneLoader.Instance.LoadGameOver(GameOverType.Mental);
                    return;
                }
            }

            SceneLoader.Instance.LoadVigilia(abrupt);
        }

        public void OnDreamSceneReady() => State = GameState.Dream;

        public void OnVigiliaSceneReady() => State = GameState.Vigilia;

        public void StartNewGame()
        {
            _protagonistState?.ResetForNewGame();
            SaveManager.Instance?.DeleteSave();
            UnlockStartingAllies();
            State = GameState.Transitioning;
            SceneLoader.Instance.LoadVigilia(abrupt: false);
        }
    }
}
