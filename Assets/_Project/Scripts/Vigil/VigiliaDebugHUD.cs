using UnityEngine;
using UnityEngine.InputSystem;
using Restless.Core;

namespace Restless.Vigil
{
    public class VigiliaDebugHUD : MonoBehaviour
    {
        [SerializeField] private ProtagonistState _protagonistState;
        [SerializeField] private AllyRegistry     _allyRegistry;

        private bool      _visible = true;
        private GUIStyle  _headerStyle;
        private GUIStyle  _rowStyle;
        private GUIStyle  _dimStyle;
        private Texture2D _white;

        private void Awake()
        {
            _white = new Texture2D(1, 1);
            _white.SetPixel(0, 0, Color.white);
            _white.Apply();
        }

        private void Update()
        {
            if (Keyboard.current != null && Keyboard.current.f1Key.wasPressedThisFrame)
                _visible = !_visible;
        }

        private void OnGUI()
        {
            if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "Vigil") return;
            EnsureStyles();
            if (!_visible) return;

            float panelW = 300f;
            float pad    = 8f;
            float lineH  = 18f;
            float x      = Screen.width - panelW - 8f;
            float y      = 8f;

            y = DrawProtagonistBlock(x, y, panelW, pad, lineH);
            y += 6f;
            y = DrawAlliesBlock(x, y, panelW, pad, lineH);
            y += 6f;
            DrawRunBlock(x, y, panelW, pad, lineH);

            GUI.color = Color.white;
        }

        private float DrawProtagonistBlock(float x, float y, float w, float pad, float lineH)
        {
            if (_protagonistState == null) return y;

            var ps = _protagonistState;
            int lines = 5;
            float h = pad * 2 + lineH + lines * lineH;

            GUI.color = new Color(0.06f, 0.06f, 0.08f, 0.88f);
            GUI.DrawTexture(new Rect(x, y, w, h), _white);
            y += pad;

            GUI.color = new Color(0.8f, 0.8f, 0.85f);
            GUI.Label(new Rect(x + pad, y, w - pad * 2, lineH), "PROTAGONISTA", _headerStyle);
            y += lineH;

            DrawRow(x + pad, y, w - pad * 2, lineH, "Salud mental",
                $"{ps.mentalHealth:F0} / 100",
                ps.mentalHealth > 60f ? Color.green : ps.mentalHealth > 30f ? Color.yellow : Color.red);
            y += lineH;

            DrawRow(x + pad, y, w - pad * 2, lineH, "Salud física",
                $"{ps.physicalHealth:F0} / 100",
                ps.physicalHealth > 60f ? Color.green : ps.physicalHealth > 30f ? Color.yellow : Color.red);
            y += lineH;

            DrawRow(x + pad, y, w - pad * 2, lineH, "Despertares abruptos",
                ps.totalAbruptWakeUps.ToString(), Color.white);
            y += lineH;

            DrawRow(x + pad, y, w - pad * 2, lineH, "Duración base sueño",
                $"{ps.BaseDreamDuration:F0}s",
                ps.BaseDreamDuration >= 120f ? Color.green : ps.BaseDreamDuration >= 80f ? Color.yellow : Color.red);
            y += lineH;

            DrawRow(x + pad, y, w - pad * 2, lineH, "Inventario",
                $"{ps.inventoryGridWidth}×{ps.inventoryGridHeight}", Color.white);
            y += lineH;

            y += pad;
            return y;
        }

        private float DrawAlliesBlock(float x, float y, float w, float pad, float lineH)
        {
            if (_allyRegistry == null || SaveManager.Instance == null) return y;

            var unlocked = SaveManager.Instance.Data.unlockedAllyIds;
            var selected = SaveManager.Instance.Data.selectedAllyIds;
            int count    = _allyRegistry.All.Count;
            float h = pad * 2 + lineH + count * lineH;

            GUI.color = new Color(0.06f, 0.06f, 0.08f, 0.88f);
            GUI.DrawTexture(new Rect(x, y, w, h), _white);
            y += pad;

            GUI.color = new Color(0.8f, 0.8f, 0.85f);
            GUI.Label(new Rect(x + pad, y, w - pad * 2, lineH), "ALIADOS", _headerStyle);
            y += lineH;

            foreach (var ally in _allyRegistry.All)
            {
                if (ally == null) continue;
                bool isUnlocked = unlocked.Contains(ally.id);
                bool isSelected = selected.Contains(ally.id);

                string status = isSelected ? "SEL" : isUnlocked ? "OK" : "—";
                Color col = isSelected ? new Color(0.3f, 0.9f, 0.4f)
                          : isUnlocked ? new Color(0.7f, 0.85f, 1f)
                          : new Color(0.35f, 0.35f, 0.4f);

                float keyW = 36f;
                GUI.color = col;
                GUI.Label(new Rect(x + pad, y, keyW, lineH), status, _rowStyle);
                GUI.color = isUnlocked ? new Color(0.85f, 0.85f, 0.9f) : new Color(0.4f, 0.4f, 0.45f);
                GUI.Label(new Rect(x + pad + keyW, y, w - pad * 2 - keyW, lineH),
                    ally.displayName + (isUnlocked && ally.passiveDescription != "" ? $"  [{ally.passiveDescription}]" : ""),
                    _rowStyle);
                y += lineH;
            }

            y += pad;
            return y;
        }

        private float DrawRunBlock(float x, float y, float w, float pad, float lineH)
        {
            if (SaveManager.Instance == null) return y;

            int lines = 2;
            float h = pad * 2 + lineH + lines * lineH;

            GUI.color = new Color(0.06f, 0.06f, 0.08f, 0.88f);
            GUI.DrawTexture(new Rect(x, y, w, h), _white);
            y += pad;

            GUI.color = new Color(0.8f, 0.8f, 0.85f);
            GUI.Label(new Rect(x + pad, y, w - pad * 2, lineH), "PARTIDA", _headerStyle);
            y += lineH;

            var data = SaveManager.Instance.Data;
            DrawRow(x + pad, y, w - pad * 2, lineH, "Runs completadas", data.totalRuns.ToString(), Color.white);
            y += lineH;
            DrawRow(x + pad, y, w - pad * 2, lineH, "F1", "Toggle este HUD", new Color(0.5f, 0.5f, 0.55f));
            y += lineH;
            y += pad;
            return y;
        }

        private void DrawRow(float x, float y, float w, float h, string label, string value, Color valueColor)
        {
            float labelW = w * 0.62f;
            GUI.color = new Color(0.55f, 0.55f, 0.6f);
            GUI.Label(new Rect(x, y, labelW, h), label, _rowStyle);
            GUI.color = valueColor;
            GUI.Label(new Rect(x + labelW, y, w - labelW, h), value, _rowStyle);
        }

        private void EnsureStyles()
        {
            if (_headerStyle != null) return;

            _headerStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize  = 12,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft
            };
            _rowStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize  = 11,
                alignment = TextAnchor.MiddleLeft,
                wordWrap  = false
            };
        }
    }
}
