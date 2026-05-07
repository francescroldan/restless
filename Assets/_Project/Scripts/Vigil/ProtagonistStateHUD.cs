using UnityEngine;
using Restless.Core;

namespace Restless.Vigil
{
    public class ProtagonistStateHUD : MonoBehaviour
    {
        [SerializeField] private ProtagonistState _state;

        private Texture2D _white;
        private GUIStyle  _labelStyle;
        private GUIStyle  _valueStyle;

        private void Awake()
        {
            _white = new Texture2D(1, 1);
            _white.SetPixel(0, 0, Color.white);
            _white.Apply();
        }

        private void OnGUI()
        {
            if (_state == null) return;
            EnsureStyles();

            const float panelW = 220f;
            const float barH   = 10f;
            const float lineH  = 18f;
            const float pad    = 8f;
            const float gap    = 6f;

            // 4 rows: mental, physical, duration, wake-ups
            float panelH = pad * 2 + lineH + (lineH + barH + gap) * 2 + lineH + pad;
            float x = 10f;
            float y = Screen.height - panelH - 10f;

            // Background
            GUI.color = new Color(0.05f, 0.05f, 0.07f, 0.9f);
            GUI.DrawTexture(new Rect(x, y, panelW, panelH), _white);
            y += pad;

            // Title
            GUI.color = new Color(0.7f, 0.7f, 0.75f);
            GUI.Label(new Rect(x + pad, y, panelW - pad * 2, lineH), "ESTADO", _labelStyle);
            y += lineH;

            // Mental health bar
            DrawStatBar(x + pad, ref y, panelW - pad * 2, barH, lineH, gap,
                "Salud mental", _state.mentalHealth / 100f,
                HealthColor(_state.mentalHealth), $"{_state.mentalHealth:F0}");

            // Physical health bar
            DrawStatBar(x + pad, ref y, panelW - pad * 2, barH, lineH, gap,
                "Salud física", _state.physicalHealth / 100f,
                HealthColor(_state.physicalHealth), $"{_state.physicalHealth:F0}");

            // Dream duration + abrupt wake-ups on one row
            float dur = _state.BaseDreamDuration;
            Color durCol = dur >= 70f ? new Color(0.3f, 0.85f, 1f)
                         : dur >= 50f ? Color.yellow
                         : Color.red;

            float rowW = panelW - pad * 2;
            float halfW = rowW * 0.5f - 4f;

            GUI.color = new Color(0.45f, 0.45f, 0.5f);
            GUI.Label(new Rect(x + pad, y, halfW, lineH), "Sueño base", _labelStyle);
            GUI.color = durCol;
            GUI.Label(new Rect(x + pad + halfW * 0.6f, y, halfW * 0.4f, lineH), $"{dur:F0}s", _valueStyle);

            int wakeUps = _state.totalAbruptWakeUps;
            Color wakeCol = wakeUps == 0 ? new Color(0.4f, 0.4f, 0.45f)
                          : wakeUps <= 2 ? Color.yellow
                          : Color.red;
            GUI.color = new Color(0.45f, 0.45f, 0.5f);
            GUI.Label(new Rect(x + pad + halfW + 8f, y, halfW, lineH), "Despertares", _labelStyle);
            GUI.color = wakeCol;
            GUI.Label(new Rect(x + pad + halfW + 8f + halfW * 0.65f, y, halfW * 0.35f, lineH),
                wakeUps.ToString(), _valueStyle);

            GUI.color = Color.white;
        }

        private void DrawStatBar(float x, ref float y, float w, float barH, float lineH, float gap,
                                  string label, float fill, Color fillColor, string valueText)
        {
            float labelW = w * 0.58f;
            float valueW = w - labelW;

            // Label + value
            GUI.color = new Color(0.45f, 0.45f, 0.5f);
            GUI.Label(new Rect(x, y, labelW, lineH), label, _labelStyle);
            GUI.color = fillColor;
            GUI.Label(new Rect(x + labelW, y, valueW, lineH), valueText, _valueStyle);
            y += lineH;

            // Track
            GUI.color = new Color(0.15f, 0.15f, 0.18f);
            GUI.DrawTexture(new Rect(x, y, w, barH), _white);

            // Fill
            float fillPx = Mathf.Max(2f, (w - 1f) * fill);
            GUI.color = fillColor;
            GUI.DrawTexture(new Rect(x, y, fillPx, barH), _white);

            y += barH + gap;
        }

        private static Color HealthColor(float value) =>
            value > 60f ? new Color(0.3f, 0.85f, 0.45f) :
            value > 30f ? Color.yellow : Color.red;

        private void EnsureStyles()
        {
            if (_labelStyle != null) return;

            _labelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize  = 11,
                alignment = TextAnchor.MiddleLeft,
                wordWrap  = false
            };
            _valueStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize  = 11,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleRight,
                wordWrap  = false
            };
        }
    }
}
