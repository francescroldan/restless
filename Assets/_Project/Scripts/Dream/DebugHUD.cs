using UnityEngine;
using UnityEngine.InputSystem;

namespace Restless.Dream
{
    public class DebugHUD : MonoBehaviour
    {
        private bool _visible = true;
        private PlayerInput _playerInput;
        private Texture2D _white;
        private GUIStyle _checkStyle;
        private GUIStyle _headerStyle;

        // M2 playtest checklist — tick manually in the Inspector during testing
        [Header("Playtest Checklist (marcar durante la prueba)")]
        [SerializeField] private bool _check_movimiento;
        [SerializeField] private bool _check_conoPorRaton;
        [SerializeField] private bool _check_inquietudSube;
        [SerializeField] private bool _check_interaccionMemoryPoint;
        [SerializeField] private bool _check_minijuegoTiming;
        [SerializeField] private bool _check_colocarFragmento;
        [SerializeField] private bool _check_entidadSubeInquietud;
        [SerializeField] private bool _check_timerBaja;
        [SerializeField] private bool _check_despertarVoluntario;
        [SerializeField] private bool _check_botonDormir;
        [SerializeField] private bool _check_despertarAbrupto;

        private void Awake()
        {
            _white = new Texture2D(1, 1);
            _white.SetPixel(0, 0, Color.white);
            _white.Apply();
        }

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
            EnsureStyles();
            DrawChecklist();
            DrawKeyLegend();
            DrawProximity();
            if (_visible) DrawStats();
        }

        // ── Checklist (always visible) ───────────────────────────────────

        private void DrawChecklist()
        {
            float panelW = 272f;
            float lineH  = 19f;
            float pad    = 8f;

            (string label, bool done)[] items = {
                ("Mover protagonista (WASD)",          _check_movimiento),
                ("Cono sigue al ratón",                _check_conoPorRaton),
                ("Inquietud sube con el tiempo",       _check_inquietudSube),
                ("Interactuar con memory point (E)",   _check_interaccionMemoryPoint),
                ("Completar minijuego Timing",         _check_minijuegoTiming),
                ("Colocar fragmento en inventario",    _check_colocarFragmento),
                ("Entidad sube inquietud (en cono)",   _check_entidadSubeInquietud),
                ("Timer del sueño baja",               _check_timerBaja),
                ("Despertar voluntario → Vigilia",     _check_despertarVoluntario),
                ("Botón Dormir → vuelve al sueño",     _check_botonDormir),
                ("Despertar abrupto (timer/máx.)",     _check_despertarAbrupto),
            };

            int doneCount = 0;
            foreach (var (_, done) in items) if (done) doneCount++;

            float panelH = pad * 2 + lineH + items.Length * lineH + 4f;
            float x = Screen.width - panelW - 8f;
            float y = 8f;

            // Background
            GUI.color = new Color(0.06f, 0.06f, 0.08f, 0.88f);
            GUI.DrawTexture(new Rect(x, y, panelW, panelH), _white);

            y += pad;

            // Header
            GUI.color = doneCount == items.Length ? Color.green : new Color(0.8f, 0.8f, 0.85f);
            GUI.Label(new Rect(x + pad, y, panelW - pad * 2, lineH),
                $"M2 CHECKLIST  {doneCount}/{items.Length}", _headerStyle);
            y += lineH + 2f;

            // Items
            foreach (var (label, done) in items)
            {
                GUI.color = done ? new Color(0.3f, 0.9f, 0.4f) : new Color(0.55f, 0.55f, 0.6f);
                GUI.Label(new Rect(x + pad, y, panelW - pad * 2, lineH),
                    (done ? "✓ " : "○ ") + label, _checkStyle);
                y += lineH;
            }

            GUI.color = Color.white;
        }

        // ── Key legend (always visible) ──────────────────────────────────

        private void DrawKeyLegend()
        {
            (string key, string action)[] keys = {
                ("WASD",    "Mover"),
                ("Ratón",   "Orientar cono de visión"),
                ("E",       "Interactuar / Confirmar"),
                ("R",       "Rotar fragmento (inventario)"),
                ("Escape",  "Despertar voluntario"),
                ("F1",      "Toggle stats HUD"),
            };

            float lineH  = 18f;
            float pad    = 8f;
            float panelW = 272f;
            float panelH = pad * 2 + lineH * (keys.Length + 1) + 2f;
            float x      = Screen.width - panelW - 8f;

            // Position below the checklist — estimate checklist height
            float checklistH = pad * 2 + lineH + 11 * lineH + 4f + 6f; // header + 11 items + gap
            float y = 8f + checklistH + 6f;

            GUI.color = new Color(0.06f, 0.06f, 0.08f, 0.88f);
            GUI.DrawTexture(new Rect(x, y, panelW, panelH), _white);

            y += pad;
            GUI.color = new Color(0.8f, 0.8f, 0.85f);
            GUI.Label(new Rect(x + pad, y, panelW - pad * 2, lineH), "CONTROLES", _headerStyle);
            y += lineH + 2f;

            foreach (var (key, action) in keys)
            {
                float keyW = 62f;
                GUI.color = new Color(0.95f, 0.85f, 0.4f);
                GUI.Label(new Rect(x + pad, y, keyW, lineH), key, _checkStyle);
                GUI.color = new Color(0.7f, 0.7f, 0.75f);
                GUI.Label(new Rect(x + pad + keyW, y, panelW - pad * 2 - keyW, lineH), action, _checkStyle);
                y += lineH;
            }

            GUI.color = Color.white;
        }

        // ── Stats (F1 toggle) ────────────────────────────────────────────

        private void DrawStats()
        {
            const int x = 10, lineH = 20;
            int y = 10;

            GUI.color = Color.white;
            GUI.Label(new Rect(x, y, 400, lineH), "=== DEBUG HUD (F1) ===");
            y += lineH + 4;

            if (RestlessnessManager.Instance != null)
            {
                float val = RestlessnessManager.Instance.Value;
                var threshold = RestlessnessManager.Instance.CurrentThreshold;
                GUI.color = ThresholdColor(threshold);
                GUI.Label(new Rect(x, y, 400, lineH),
                    $"Restlessness: {val:F1} / 100  [{threshold}]");
                y += lineH;
            }

            if (DreamTimer.Instance != null)
            {
                float rem = DreamTimer.Instance.Remaining;
                float dur = DreamTimer.Instance.Duration;
                GUI.color = rem < 30f ? Color.red : Color.white;
                GUI.Label(new Rect(x, y, 400, lineH),
                    $"Dream Timer: {rem:F1}s / {dur:F0}s");
                y += lineH;
            }

            if (DreamInventory.Instance != null)
            {
                int placed = DreamInventory.Instance.PlacedFragments.Count;
                bool full = DreamInventory.Instance.IsFull();
                GUI.color = full ? Color.yellow : Color.white;
                GUI.Label(new Rect(x, y, 400, lineH),
                    $"Inventory: {placed} fragment(s){(full ? "  [FULL]" : "")}");
                y += lineH;
            }

            GUI.color = Color.cyan;
            var memoryPoints = FindObjectsByType<MemoryPoint>(FindObjectsSortMode.None);
            foreach (var mp in memoryPoints)
            {
                if (mp.CurrentState != MemoryPoint.State.Extracting) continue;

                var timing    = mp.GetComponent<TimingMinigame>();
                var recon     = mp.GetComponent<ReconstructionMinigame>();
                var retention = mp.GetComponent<RetentionMinigame>();

                if (timing != null && timing.IsActive)
                    GUI.Label(new Rect(x, y, 400, lineH),
                        $"Minigame [TIMING]: marker={timing.MarkerPosition:F2}  ok={timing.Successes}  fail={timing.Failures}");
                else if (recon != null && recon.IsActive)
                    GUI.Label(new Rect(x, y, 400, lineH),
                        $"Minigame [RECONSTRUCTION]: time={recon.TimeRemaining:F1}s  pieces={recon.PieceCount}");
                else if (retention != null && retention.IsActive)
                    GUI.Label(new Rect(x, y, 400, lineH),
                        $"Minigame [RETENTION]: conc={retention.Concentration:F2}");
                y += lineH;
            }

            GUI.color = Color.white;
        }

        // ── Proximity indicator (always visible, bottom-left) ───────────────

        private void DrawProximity()
        {
            var protagonist = GameObject.FindWithTag("Player");
            if (protagonist == null) return;

            var memoryPoints = FindObjectsByType<MemoryPoint>(FindObjectsSortMode.None);
            float closest = float.MaxValue;
            MemoryPoint closestMp = null;
            foreach (var mp in memoryPoints)
            {
                if (mp.CurrentState != MemoryPoint.State.Available) continue;
                float d = Vector2.Distance(protagonist.transform.position, mp.transform.position);
                if (d < closest) { closest = d; closestMp = mp; }
            }

            if (closestMp == null) return;

            bool inRange = closest <= 3f;
            string label = inRange
                ? $"[E] {closestMp.name}  dist={closest:F1}  EN RANGO"
                : $"{closestMp.name}  dist={closest:F1}  (rango=3.0)";

            float w = 320f, h = 22f;
            float x = 10f, y = Screen.height - h - 10f;

            GUI.color = new Color(0.05f, 0.05f, 0.07f, 0.85f);
            GUI.DrawTexture(new Rect(x - 4f, y - 2f, w + 8f, h + 4f), Texture2D.whiteTexture);
            GUI.color = inRange ? Color.green : new Color(0.7f, 0.7f, 0.7f);
            GUI.Label(new Rect(x, y, w, h), label, _checkStyle);
            GUI.color = Color.white;
        }

        private void EnsureStyles()
        {
            if (_checkStyle != null) return;

            _checkStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize  = 11,
                fontStyle = FontStyle.Normal,
                alignment = TextAnchor.MiddleLeft,
                wordWrap  = false
            };

            _headerStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize  = 12,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft
            };
        }

        private static Color ThresholdColor(RestlessnessManager.Threshold t) => t switch
        {
            RestlessnessManager.Threshold.Low      => Color.green,
            RestlessnessManager.Threshold.Medium   => Color.yellow,
            RestlessnessManager.Threshold.High     => new Color(1f, 0.5f, 0f),
            RestlessnessManager.Threshold.Critical => Color.red,
            RestlessnessManager.Threshold.Max      => Color.magenta,
            _ => Color.white
        };
    }
}
