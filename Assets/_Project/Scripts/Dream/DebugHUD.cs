using UnityEngine;
using UnityEngine.InputSystem;

namespace Restless.Dream
{
    /// <summary>
    /// F1 overlay. Shows restlessness, dream timer, inventory fill, and active minigame state.
    /// Rendered with OnGUI — prototype-only, no art needed.
    /// </summary>
    public class DebugHUD : MonoBehaviour
    {
        private bool _visible;
        private PlayerInput _playerInput;

        private void Start()
        {
            var protagonistGO = GameObject.FindWithTag("Player");
            if (protagonistGO != null)
                _playerInput = protagonistGO.GetComponent<PlayerInput>();
        }

        private void Update()
        {
            if (Keyboard.current != null && Keyboard.current.f1Key.wasPressedThisFrame)
                _visible = !_visible;
        }

        private void OnGUI()
        {
            if (!_visible) return;

            const int x = 10, lineH = 20;
            int y = 10;

            GUI.color = Color.white;
            GUI.Label(new Rect(x, y, 400, lineH), "=== DEBUG HUD (F1) ===");
            y += lineH + 4;

            // Restlessness
            if (RestlessnessManager.Instance != null)
            {
                float val = RestlessnessManager.Instance.Value;
                var threshold = RestlessnessManager.Instance.CurrentThreshold;
                GUI.color = ThresholdColor(threshold);
                GUI.Label(new Rect(x, y, 400, lineH),
                    $"Restlessness: {val:F1} / 100  [{threshold}]");
                y += lineH;
            }

            // Timer
            if (DreamTimer.Instance != null)
            {
                float rem = DreamTimer.Instance.Remaining;
                float dur = DreamTimer.Instance.Duration;
                GUI.color = rem < 30f ? Color.red : Color.white;
                GUI.Label(new Rect(x, y, 400, lineH),
                    $"Dream Timer: {rem:F1}s / {dur:F0}s");
                y += lineH;
            }

            // Inventory
            if (DreamInventory.Instance != null)
            {
                int placed = DreamInventory.Instance.PlacedFragments.Count;
                bool full = DreamInventory.Instance.IsFull();
                GUI.color = full ? Color.yellow : Color.white;
                GUI.Label(new Rect(x, y, 400, lineH),
                    $"Inventory: {placed} fragment(s){(full ? "  [FULL]" : "")}");
                y += lineH;
            }

            // Active minigame
            GUI.color = Color.cyan;
            var memoryPoints = FindObjectsByType<MemoryPoint>(FindObjectsSortMode.None);
            foreach (var mp in memoryPoints)
            {
                if (mp.CurrentState == MemoryPoint.State.Extracting)
                {
                    var timing = mp.GetComponent<TimingMinigame>();
                    var recon = mp.GetComponent<ReconstructionMinigame>();
                    var retention = mp.GetComponent<RetentionMinigame>();

                    if (timing != null && timing.IsActive)
                    {
                        GUI.Label(new Rect(x, y, 400, lineH),
                            $"Minigame [TIMING]: marker={timing.MarkerPosition:F2}  " +
                            $"ok={timing.Successes}  fail={timing.Failures}");
                        y += lineH;
                    }
                    else if (recon != null && recon.IsActive)
                    {
                        GUI.Label(new Rect(x, y, 400, lineH),
                            $"Minigame [RECONSTRUCTION]: time={recon.TimeRemaining:F1}s  " +
                            $"pieces={recon.PieceCount}");
                        y += lineH;
                    }
                    else if (retention != null && retention.IsActive)
                    {
                        GUI.Label(new Rect(x, y, 400, lineH),
                            $"Minigame [RETENTION]: conc={retention.Concentration:F2}");
                        y += lineH;
                    }
                }
            }

            GUI.color = Color.white;
        }

        private static Color ThresholdColor(RestlessnessManager.Threshold t) => t switch
        {
            RestlessnessManager.Threshold.Low => Color.green,
            RestlessnessManager.Threshold.Medium => Color.yellow,
            RestlessnessManager.Threshold.High => new Color(1f, 0.5f, 0f),
            RestlessnessManager.Threshold.Critical => Color.red,
            RestlessnessManager.Threshold.Max => Color.magenta,
            _ => Color.white
        };
    }
}
