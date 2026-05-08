using UnityEngine;
using UnityEngine.InputSystem;
using Restless.Core;
using Restless.Vigil;

namespace Restless.Dream
{
    public class DebugHUD : MonoBehaviour
    {
        private bool _visible = true;
        private PlayerInput _playerInput;
        private Texture2D _white;
        private GUIStyle _checkStyle;
        private GUIStyle _headerStyle;

        [SerializeField] private AllyRegistry _registry;

        // M6 playtest checklist — tick manually in the Inspector during testing
        [Header("Playtest Checklist M6 (marcar durante la prueba)")]
        [SerializeField] private bool _check_movimiento;
        [SerializeField] private bool _check_conoPorRaton;
        [SerializeField] private bool _check_corredoresBloquean;
        [SerializeField] private bool _check_inquietudSubePorZona;
        [SerializeField] private bool _check_entidadPatrulla;
        [SerializeField] private bool _check_interaccionMemoryPoint;
        [SerializeField] private bool _check_minijuego;
        [SerializeField] private bool _check_fragmentoEnInventario;
        [SerializeField] private bool _check_encuentroAliado;
        [SerializeField] private bool _check_pasivaAliado;
        [SerializeField] private bool _check_despertarVoluntario;
        [SerializeField] private bool _check_seleccionIncompatibles;
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
            DrawBars();
            DrawProximity();
            if (!_visible) return;
            DrawChecklist();
            DrawKeyLegend();
            DrawStats();
        }

        // ── Checklist (always visible) ───────────────────────────────────

        private void DrawChecklist()
        {
            float panelW = 272f;
            float lineH  = 19f;
            float pad    = 8f;

            (string label, bool done)[] items = {
                ("Mover protagonista (WASD)",            _check_movimiento),
                ("Cono sigue al ratón",                  _check_conoPorRaton),
                ("Corredores bloquean el paso",          _check_corredoresBloquean),
                ("Inquietud sube por zona (1→2→3)",      _check_inquietudSubePorZona),
                ("Entidad patrulla y detecta",           _check_entidadPatrulla),
                ("Interactuar con memory point (E)",     _check_interaccionMemoryPoint),
                ("Completar minijuego",                  _check_minijuego),
                ("Fragmento colocado en inventario",     _check_fragmentoEnInventario),
                ("Encuentro con aliado en sueño",        _check_encuentroAliado),
                ("Pasiva del aliado funciona",           _check_pasivaAliado),
                ("Despertar voluntario → Vigilia",       _check_despertarVoluntario),
                ("Selección aliados incompatibles",      _check_seleccionIncompatibles),
                ("Despertar abrupto (timer/máx.)",       _check_despertarAbrupto),
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
                $"M6 CHECKLIST  {doneCount}/{items.Length}", _headerStyle);
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
            (string kb, string gp, string action)[] keys = {
                ("WASD",    "L.Stick",  "Mover"),
                ("Ratón",   "R.Stick",  "Orientar cono"),
                ("Shift",   "LT",       "Correr"),
                ("E",       "A",        "Interactuar"),
                ("R",       "RB",       "Rotar fragmento"),
                ("Escape",  "Start",    "Despertar"),
                ("F1",      "—",        "Toggle HUD"),
            };

            float lineH  = 18f;
            float pad    = 8f;
            float panelW = 272f;
            float panelH = pad * 2 + lineH * (keys.Length + 1) + 2f;
            float x      = Screen.width - panelW - 8f;

            float checklistH = pad * 2 + lineH + 13 * lineH + 4f + 6f;
            float y = 8f + checklistH + 6f;

            GUI.color = new Color(0.06f, 0.06f, 0.08f, 0.88f);
            GUI.DrawTexture(new Rect(x, y, panelW, panelH), _white);

            y += pad;
            GUI.color = new Color(0.8f, 0.8f, 0.85f);
            GUI.Label(new Rect(x + pad, y, panelW - pad * 2, lineH), "CONTROLES", _headerStyle);
            y += lineH + 2f;

            float kbW    = 52f;
            float gpW    = 52f;
            float actX   = x + pad + kbW + gpW;
            float actW   = panelW - pad * 2 - kbW - gpW;

            foreach (var (kb, gp, action) in keys)
            {
                GUI.color = new Color(0.95f, 0.85f, 0.4f);
                GUI.Label(new Rect(x + pad,       y, kbW,  lineH), kb,     _checkStyle);
                GUI.color = new Color(0.4f, 0.85f, 0.95f);
                GUI.Label(new Rect(x + pad + kbW, y, gpW,  lineH), gp,     _checkStyle);
                GUI.color = new Color(0.7f, 0.7f, 0.75f);
                GUI.Label(new Rect(actX,           y, actW, lineH), action, _checkStyle);
                y += lineH;
            }

            GUI.color = Color.white;
        }

        // ── Bars (always visible) ────────────────────────────────────────

        private void DrawBars()
        {
            const float barH        = 18f;
            const float labelW      = 110f;
            const float pad         = 4f;
            const float x           = 10f;
            const float rowGap      = 4f;
            const float rightReserve = 292f; // espacio reservado para checklist/controles
            float barW = Screen.width - x * 2f - labelW - rightReserve;
            float y    = 10f;

            if (DreamTimer.Instance != null)
            {
                float rem  = DreamTimer.Instance.Remaining;
                float dur  = DreamTimer.Instance.Duration;
                float fill = dur > 0f ? Mathf.Clamp01(rem / dur) : 0f;
                Color col  = Color.Lerp(Color.red, new Color(0.3f, 0.8f, 1f), fill);
                DrawBar(x, y, labelW, barW, barH, pad, $"SUEÑO  {rem:F0}s", fill, col);
                y += barH + rowGap;
            }

            if (RestlessnessManager.Instance != null)
            {
                float fill = RestlessnessManager.Instance.NormalizedValue;
                Color col  = Color.Lerp(new Color(0.2f, 0.85f, 0.35f), new Color(1f, 0.15f, 0.15f), fill);
                DrawBar(x, y, labelW, barW, barH, pad, $"INQUIETUD  {fill * 100f:F0}%", fill, col);
            }
        }

        private void DrawBar(float x, float y, float labelW, float barW, float barH, float pad,
                             string label, float fill, Color fillColor)
        {
            float totalW = labelW + barW;

            // Background
            GUI.color = new Color(0.06f, 0.06f, 0.08f, 0.88f);
            GUI.DrawTexture(new Rect(x, y, totalW, barH), _white);

            // Label
            GUI.color = new Color(0.78f, 0.78f, 0.85f);
            GUI.Label(new Rect(x + pad, y, labelW - pad, barH), label, _checkStyle);

            // Empty track
            float bx = x + labelW;
            GUI.color = new Color(0.18f, 0.18f, 0.22f);
            GUI.DrawTexture(new Rect(bx, y + pad, barW, barH - pad * 2f), _white);

            // Fill
            float fillPx = Mathf.Max(2f, (barW - 1f) * fill);
            GUI.color = fillColor;
            GUI.DrawTexture(new Rect(bx, y + pad, fillPx, barH - pad * 2f), _white);

            GUI.color = Color.white;
        }

        // ── Stats (F1 toggle) ────────────────────────────────────────────

        private void DrawStats()
        {
            const int x = 10, lineH = 20;
            int y = 10 + (18 + 4) * 2 + 8; // below the two bars

            GUI.color = Color.white;
            GUI.Label(new Rect(x, y, 400, lineH), "=== DEBUG HUD (F1) ===");
            y += lineH + 4;

            var player = GameObject.FindWithTag("Player");
            if (player != null)
            {
                Vector2 pos = player.transform.position;
                GUI.color = new Color(0.7f, 0.7f, 0.75f);
                GUI.Label(new Rect(x, y, 400, lineH), $"Jugador: ({pos.x:F1}, {pos.y:F1})");
                y += lineH;
            }

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

            DrawModifiers(x, ref y, lineH);

            if (DreamInventory.Instance != null)
            {
                int placed = DreamInventory.Instance.PlacedFragments.Count;
                bool full = DreamInventory.Instance.IsFull();
                GUI.color = full ? Color.yellow : Color.white;
                GUI.Label(new Rect(x, y, 400, lineH),
                    $"Inventory: {placed} fragment(s){(full ? "  [FULL]" : "")}");
                y += lineH;
            }

            if (SaveManager.Instance != null && GameManager.Instance != null)
            {
                int collected = SaveManager.Instance.CollectedFragmentCount;
                int target    = GameManager.Instance.DemoFragmentTarget;
                int remaining = Mathf.Max(0, target - collected);
                GUI.color = remaining == 0 ? new Color(1f, 0.82f, 0.28f) : new Color(0.7f, 0.7f, 0.75f);
                GUI.Label(new Rect(x, y, 400, lineH),
                    $"Ecos: {collected}/{target}  (faltan {remaining})");
                y += lineH;
            }

            DrawActiveAllies(x, ref y, lineH);
            DrawEncounterAndMemories(x, ref y, lineH);

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

        // ── Modifiers block ─────────────────────────────────────────────────

        private void DrawModifiers(int x, ref int y, int lineH)
        {
            var rm = RestlessnessManager.Instance;
            var protagonist = GameObject.FindWithTag("Player");
            var ctrl = protagonist != null ? protagonist.GetComponent<ProtagonistController>() : null;

            GUI.color = new Color(1f, 0.85f, 0.4f);
            GUI.Label(new Rect(x, y, 500, lineH), "Modificadores activos:");
            y += lineH;

            if (rm != null)
            {
                float rate = rm.CurrentRate;
                GUI.color = rate > 1.5f ? Color.red : rate > 0.7f ? Color.yellow : Color.white;
                GUI.Label(new Rect(x + 12, y, 500, lineH),
                    $"Inquietud: base {rm.BaseRate:F2}/s  ×zona {rm.ZoneMultiplier:F2}  ×pasiva {rm.PassiveMultiplier:F2}  = {rate:F2}/s");
                y += lineH;
            }

            if (ctrl != null)
            {
                GUI.color = Color.white;
                GUI.Label(new Rect(x + 12, y, 500, lineH),
                    $"Velocidad: caminar {ctrl.WalkSpeed:F1} u/s  |  correr {ctrl.RunSpeed:F1} u/s");
                y += lineH;
            }

            GUI.color = Color.white;
        }

        // ── Active allies block ──────────────────────────────────────────────

        private void DrawActiveAllies(int x, ref int y, int lineH)
        {
            var selectedIds = SaveManager.Instance?.Data?.selectedAllyIds;

            GUI.color = new Color(0.6f, 0.7f, 1f);
            GUI.Label(new Rect(x, y, 400, lineH), "Aliados activos:");
            y += lineH;

            if (selectedIds == null || selectedIds.Count == 0)
            {
                GUI.color = new Color(0.5f, 0.5f, 0.55f);
                GUI.Label(new Rect(x + 12, y, 400, lineH), "— ninguno");
                y += lineH;
                return;
            }

            foreach (var id in selectedIds)
            {
                AllyData ally = _registry != null ? _registry.GetById(id) : null;
                string name   = ally != null ? ally.displayName : id;
                string detail = ally != null
                    ? $"  rate×{1f + ally.restlessnessRateModifier:F2}  dur+{ally.dreamDurationBonus:F0}s"
                    : "";
                GUI.color = new Color(0.4f, 0.9f, 0.5f);
                GUI.Label(new Rect(x + 12, y, 500, lineH), $"• {name}{detail}");
                y += lineH;
            }

            GUI.color = Color.white;
        }

        // ── Encounter + memories block ───────────────────────────────────────

        private void DrawEncounterAndMemories(int x, ref int y, int lineH)
        {
            // ── Encounter ally ──
            GUI.color = new Color(0.9f, 0.7f, 1f);
            GUI.Label(new Rect(x, y, 500, lineH), "Encuentro esta run:");
            y += lineH;

            var spawner = AllyEncounterSpawner.Instance;
            if (spawner == null || spawner.ActiveAlly == null)
            {
                GUI.color = new Color(0.5f, 0.5f, 0.55f);
                GUI.Label(new Rect(x + 12, y, 500, lineH), "— ninguno");
                y += lineH;
            }
            else
            {
                bool obtained = spawner.AllyObtained;
                GUI.color = obtained ? new Color(0.3f, 0.9f, 0.4f) : new Color(0.95f, 0.85f, 0.3f);
                GUI.Label(new Rect(x + 12, y, 500, lineH),
                    $"{(obtained ? "✓" : "○")} {spawner.ActiveAlly.displayName}  " +
                    $"{(obtained ? "obtenido" : "disponible")}");
                y += lineH;
                var pos = spawner.ActiveEncounterPosition;
                GUI.color = new Color(0.55f, 0.55f, 0.6f);
                GUI.Label(new Rect(x + 12, y, 500, lineH),
                    $"   spawn ({pos.x:F0}, {pos.y:F0})");
                y += lineH;
            }

            // ── Memory points ──
            GUI.color = new Color(0.9f, 0.7f, 1f);
            GUI.Label(new Rect(x, y, 500, lineH), "Recuerdos:");
            y += lineH;

            var memPoints = FindObjectsByType<MemoryPoint>(FindObjectsSortMode.None);
            if (memPoints.Length == 0)
            {
                GUI.color = new Color(0.5f, 0.5f, 0.55f);
                GUI.Label(new Rect(x + 12, y, 500, lineH), "— ninguno");
                y += lineH;
            }
            else
            {
                foreach (var mp in memPoints)
                {
                    (Color col, string icon) = mp.CurrentState switch
                    {
                        MemoryPoint.State.Collected  => (new Color(0.3f, 0.9f, 0.4f), "✓"),
                        MemoryPoint.State.Extracting => (Color.yellow,                 "⟳"),
                        MemoryPoint.State.Failed     => (Color.red,                    "✗"),
                        _                            => (new Color(0.55f, 0.55f, 0.6f), "○"),
                    };
                    GUI.color = col;
                    GUI.Label(new Rect(x + 12, y, 500, lineH),
                        $"{icon} {mp.gameObject.name}  [{mp.CurrentState}]");
                    y += lineH;
                }
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
