using UnityEngine;

namespace Restless.Dream
{
    /// <summary>
    /// In-game HUD overlay for all three minigame variants.
    /// Uses OnGUI — no Canvas setup required for the prototype.
    /// </summary>
    public class MinigameHUD : MonoBehaviour
    {
        [SerializeField] private Color _barBg        = new Color(0.1f, 0.1f, 0.12f, 0.92f);
        [SerializeField] private Color _greenZone    = new Color(0.2f, 0.9f, 0.3f,  0.75f);
        [SerializeField] private Color _markerColor  = Color.white;
        [SerializeField] private Color _fillColor    = new Color(0.3f, 0.85f, 1f,   0.85f);
        [SerializeField] private Color _failColor    = new Color(1f,   0.25f, 0.25f,0.85f);

        private GUIStyle _labelCenter;
        private GUIStyle _labelSmall;
        private Texture2D _white;

        private void Awake()
        {
            _white = new Texture2D(1, 1);
            _white.SetPixel(0, 0, Color.white);
            _white.Apply();
        }

        private void OnGUI()
        {
            // Find any active extraction
            var memoryPoints = FindObjectsByType<MemoryPoint>(FindObjectsSortMode.None);
            foreach (var mp in memoryPoints)
            {
                if (mp.CurrentState != MemoryPoint.State.Extracting) continue;

                var timing    = mp.GetComponent<TimingMinigame>();
                var retention = mp.GetComponent<RetentionMinigame>();
                var recon     = mp.GetComponent<ReconstructionMinigame>();

                if (timing    != null && timing.IsActive)    { DrawTimingHUD(timing);    return; }
                if (retention != null && retention.IsActive) { DrawRetentionHUD(retention); return; }
                if (recon     != null && recon.IsActive)     { DrawReconHUD(recon);       return; }
            }
        }

        // ── Timing ──────────────────────────────────────────────────────

        private void DrawTimingHUD(TimingMinigame timing)
        {
            float sw = Screen.width, sh = Screen.height;

            float panelW = sw * 0.5f;
            float panelH = 110f;
            float panelX = (sw - panelW) * 0.5f;
            float panelY = sh - panelH - 40f;

            // Panel background
            DrawRect(new Rect(panelX, panelY, panelW, panelH), _barBg);

            float padding = 16f;
            float barX = panelX + padding;
            float barW = panelW - padding * 2f;
            float barH = 24f;
            float barY = panelY + 38f;

            // Bar track
            DrawRect(new Rect(barX, barY, barW, barH), new Color(0.25f, 0.25f, 0.28f));

            // Green zone
            float gzCenter = timing.GreenZoneCenter;
            float gzHalf   = timing.GreenZoneHalfWidth;
            float gzX = barX + (gzCenter - gzHalf) * barW;
            float gzW = gzHalf * 2f * barW;
            DrawRect(new Rect(gzX, barY, gzW, barH), _greenZone);

            // Moving marker
            float markerX = barX + timing.MarkerPosition * barW - 2f;
            DrawRect(new Rect(markerX, barY - 4f, 4f, barH + 8f), _markerColor);

            // Labels
            EnsureStyles();
            GUI.color = Color.white;
            GUI.Label(new Rect(panelX, panelY + 8f, panelW, 22f),
                "EXTRAER MEMORIA — presiona E en la zona verde", _labelCenter);

            // Success / failure dots
            float dotR = 9f, dotY = barY + barH + 10f;
            float dotsStartX = panelX + panelW * 0.5f - (timing.Successes + 1) * (dotR * 2.5f) * 0.5f;
            for (int i = 0; i < 3; i++)
            {
                Color c = i < timing.Successes ? _greenZone : new Color(0.4f, 0.4f, 0.4f);
                DrawRect(new Rect(dotsStartX + i * dotR * 2.5f, dotY, dotR * 2f, dotR * 2f), c);
            }
            float failStartX = panelX + panelW * 0.5f + dotR * 2f;
            for (int i = 0; i < 3; i++)
            {
                Color c = i < timing.Failures ? _failColor : new Color(0.4f, 0.4f, 0.4f);
                DrawRect(new Rect(failStartX + i * dotR * 2.5f, dotY, dotR * 2f, dotR * 2f), c);
            }

            GUI.color = Color.white;
        }

        // ── Retention ───────────────────────────────────────────────────

        private void DrawRetentionHUD(RetentionMinigame retention)
        {
            float sw = Screen.width, sh = Screen.height;

            float panelW = sw * 0.5f;
            float panelH = 90f;
            float panelX = (sw - panelW) * 0.5f;
            float panelY = sh - panelH - 40f;

            DrawRect(new Rect(panelX, panelY, panelW, panelH), _barBg);

            float padding = 16f;
            float barX = panelX + padding;
            float barW = panelW - padding * 2f;
            float barH = 24f;
            float barY = panelY + 40f;

            // Track
            DrawRect(new Rect(barX, barY, barW, barH), new Color(0.25f, 0.25f, 0.28f));

            // Fill
            float fillW = retention.Concentration * barW;
            Color fillC = Color.Lerp(_fillColor, _failColor, 1f - retention.Concentration);
            DrawRect(new Rect(barX, barY, fillW, barH), fillC);

            EnsureStyles();
            GUI.color = Color.white;
            GUI.Label(new Rect(panelX, panelY + 8f, panelW, 22f),
                "MANTÉN E PULSADO — no pierdas la concentración", _labelCenter);
            GUI.color = Color.white;
        }

        // ── Reconstruction ──────────────────────────────────────────────

        private void DrawReconHUD(ReconstructionMinigame recon)
        {
            float sw = Screen.width, sh = Screen.height;

            float panelW = sw * 0.5f;
            float panelH = 110f;
            float panelX = (sw - panelW) * 0.5f;
            float panelY = sh - panelH - 40f;

            DrawRect(new Rect(panelX, panelY, panelW, panelH), _barBg);

            EnsureStyles();
            GUI.color = Color.white;
            GUI.Label(new Rect(panelX, panelY + 8f, panelW, 22f),
                "RECONSTRUYE EL RECUERDO — ← → para seleccionar, Enter para colocar", _labelCenter);

            // Piece slots
            float slotSize = 28f, slotGap = 6f;
            int count = recon.PieceCount;
            float totalW = count * (slotSize + slotGap) - slotGap;
            float startX = panelX + (panelW - totalW) * 0.5f;
            float slotY  = panelY + 38f;

            for (int i = 0; i < count; i++)
            {
                float sx = startX + i * (slotSize + slotGap);
                bool inPlace = recon.PieceSlots[i] == recon.TargetSlots[i];
                bool selected = i == recon.SelectedPiece;

                Color slotColor = inPlace
                    ? _greenZone
                    : selected ? _fillColor : new Color(0.4f, 0.4f, 0.45f);

                DrawRect(new Rect(sx, slotY, slotSize, slotSize), slotColor);

                if (selected)
                    DrawRect(new Rect(sx - 2f, slotY - 2f, slotSize + 4f, slotSize + 4f),
                        new Color(1f, 1f, 1f, 0.3f));

                GUI.color = Color.white;
                GUI.Label(new Rect(sx, slotY + 5f, slotSize, slotSize),
                    (i + 1).ToString(), _labelCenter);
            }

            // Countdown
            float t = recon.NormalizedTime;
            Color tColor = t < 0.33f ? _failColor : Color.white;
            GUI.color = tColor;
            GUI.Label(new Rect(panelX, panelY + 78f, panelW, 22f),
                $"{recon.TimeRemaining:F1}s", _labelCenter);
            GUI.color = Color.white;
        }

        // ── Helpers ─────────────────────────────────────────────────────

        private void DrawRect(Rect rect, Color color)
        {
            GUI.color = color;
            GUI.DrawTexture(rect, _white);
        }

        private void EnsureStyles()
        {
            if (_labelCenter != null) return;

            _labelCenter = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize   = 13,
                fontStyle  = FontStyle.Bold
            };
            _labelCenter.normal.textColor = Color.white;

            _labelSmall = new GUIStyle(_labelCenter)
            {
                fontSize = 11,
                fontStyle = FontStyle.Normal
            };
        }
    }
}
