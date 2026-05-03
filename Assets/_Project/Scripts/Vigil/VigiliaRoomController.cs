using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using Restless.Core;

namespace Restless.Vigil
{
    public class VigiliaRoomController : MonoBehaviour
    {
        public static VigiliaRoomController Instance { get; private set; }

        [Header("Scene references")]
        [SerializeField] private ProtagonistState _protagonistState;
        [SerializeField] private AllySlot[]       _allySlots;
        [SerializeField] private ProtagonistBed   _protagonistBed;
        [SerializeField] private AllyInfoPanel    _allyInfoPanel;

        [Header("Lighting")]
        [SerializeField] private Light2D _globalLight;
        [SerializeField] private Light2D _bedLight;
        [SerializeField] private float   _globalLightHealthyIntensity  = 0.18f;
        [SerializeField] private float   _globalLightDepletedIntensity = 0.05f;

        private bool _transitioning;

        private void Awake()
        {
            // On return from Dream, replace the stale DontDestroyOnLoad instance with the fresh one.
            if (Instance != null && Instance != this) Destroy(Instance);
            Instance = this;
        }

        private void Start()
        {
            InitializeRoom();

            bool wasAbrupt = SceneLoader.Instance != null && SceneLoader.Instance.LastWakeUpWasAbrupt;
            VigiliaTransitionFX.Instance?.PlayEntry(wasAbrupt);
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        // ── Room setup ─────────────────────────────────────────────────────

        private void InitializeRoom()
        {
            var unlockedIds = SaveManager.Instance?.Data?.unlockedAllyIds ?? new List<string>();

            foreach (var slot in _allySlots)
            {
                bool present = slot.Data != null && unlockedIds.Contains(slot.Data.id);
                slot.SetPresence(present);
            }

            AdjustAmbientLighting();
        }

        private void AdjustAmbientLighting()
        {
            if (_globalLight == null || _protagonistState == null) return;
            float t = _protagonistState.mentalHealth / 100f;
            _globalLight.intensity = Mathf.Lerp(_globalLightDepletedIntensity, _globalLightHealthyIntensity, t);
        }

        // ── Interaction ────────────────────────────────────────────────────

        public void OnAllyClicked(AllySlot slot)
        {
            if (_transitioning) return;
            _allyInfoPanel?.Show(slot);
        }

        public void RequestEnterDream()
        {
            if (_transitioning) return;

            _allyInfoPanel?.Hide();

            var saveData = SaveManager.Instance?.Data;
            bool hasAllies = saveData != null && saveData.unlockedAllyIds.Count > 0;

            if (PreDreamSelectionPanel.Instance != null && hasAllies)
            {
                PreDreamSelectionPanel.Instance.Show(EnterDream);
            }
            else
            {
                EnterDream();
            }
        }

        private void EnterDream()
        {
            Debug.Log($"[VRC] EnterDream — _transitioning={_transitioning}");
            if (_transitioning) return;
            _transitioning = true;

            _protagonistBed?.SetInteractable(false);

            if (VigiliaTransitionFX.Instance != null)
            {
                Debug.Log("[VRC] PlaySleep starting...");
                VigiliaTransitionFX.Instance.PlaySleep(() =>
                {
                    Debug.Log("[VRC] PlaySleep done → GameManager.EnterDream");
                    GameManager.Instance?.EnterDream();
                });
            }
            else
            {
                Debug.LogWarning("[VRC] VigiliaTransitionFX.Instance is NULL — calling EnterDream directly");
                GameManager.Instance?.EnterDream();
            }
        }
    }
}
