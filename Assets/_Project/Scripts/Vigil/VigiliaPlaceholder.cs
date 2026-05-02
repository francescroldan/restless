using UnityEngine;
using Restless.Core;

namespace Restless.Vigil
{
    /// <summary>
    /// Minimal Vigilia screen: shows protagonist stats, collected fragments, and a Sleep button.
    /// Replace with full art in M3+.
    /// </summary>
    public class VigiliaPlaceholder : MonoBehaviour
    {
        [SerializeField] private ProtagonistState _protagonistState;

        private GUIStyle _title;
        private GUIStyle _body;
        private GUIStyle _button;
        private GUIStyle _stat;
        private Texture2D _white;

        private bool _abruptWakeUp;

        private void Start()
        {
            var loader = FindFirstObjectByType<SceneLoader>();
            _abruptWakeUp = loader != null && loader.LastWakeUpWasAbrupt;

            _white = new Texture2D(1, 1);
            _white.SetPixel(0, 0, Color.white);
            _white.Apply();
        }

        private void OnGUI()
        {
            EnsureStyles();

            float sw = Screen.width, sh = Screen.height;

            // Full black background
            GUI.color = Color.black;
            GUI.DrawTexture(new Rect(0, 0, sw, sh), _white);

            float panelW = Mathf.Min(sw * 0.55f, 520f);
            float panelX = (sw - panelW) * 0.5f;
            float y = sh * 0.12f;

            // Abrupt wake-up notice
            if (_abruptWakeUp)
            {
                GUI.color = new Color(1f, 0.3f, 0.3f);
                GUI.Label(new Rect(panelX, y, panelW, 30f),
                    "— DESPERTAR ABRUPTO —", _title);
                y += 36f;
                GUI.color = new Color(0.85f, 0.85f, 0.85f);
                GUI.Label(new Rect(panelX, y, panelW, 22f),
                    "Los fragmentos no colocados se han perdido.", _body);
                y += 32f;
            }
            else
            {
                GUI.color = new Color(0.7f, 0.7f, 0.75f);
                GUI.Label(new Rect(panelX, y, panelW, 30f),
                    "— VIGILIA —", _title);
                y += 42f;
            }

            // Stats
            if (_protagonistState != null)
            {
                DrawStat(panelX, ref y, panelW, "Salud mental",
                    _protagonistState.mentalHealth, 100f,
                    new Color(0.5f, 0.8f, 1f));

                DrawStat(panelX, ref y, panelW, "Salud física",
                    _protagonistState.physicalHealth, 100f,
                    new Color(0.5f, 1f, 0.6f));

                y += 10f;
                GUI.color = new Color(0.6f, 0.6f, 0.65f);
                GUI.Label(new Rect(panelX, y, panelW, 20f),
                    $"Tamaño de inventario: {_protagonistState.inventoryGridWidth}×{_protagonistState.inventoryGridHeight}  |  " +
                    $"Despertares abruptos: {_protagonistState.totalAbruptWakeUps}", _body);
                y += 28f;
            }

            // Collected fragments
            y += 8f;
            GUI.color = new Color(0.7f, 0.7f, 0.75f);
            GUI.Label(new Rect(panelX, y, panelW, 22f), "Fragmentos de memoria:", _stat);
            y += 26f;

            var saveData = SaveManager.Instance?.Data;
            if (saveData != null && saveData.collectedFragmentIds.Count > 0)
            {
                foreach (var id in saveData.collectedFragmentIds)
                {
                    GUI.color = new Color(0.3f, 0.9f, 0.75f);
                    GUI.Label(new Rect(panelX + 16f, y, panelW - 16f, 20f), $"• {id}", _body);
                    y += 22f;
                }
            }
            else
            {
                GUI.color = new Color(0.45f, 0.45f, 0.5f);
                GUI.Label(new Rect(panelX + 16f, y, panelW - 16f, 20f),
                    "Ninguno todavía.", _body);
                y += 22f;
            }

            // Sleep button
            y = sh * 0.78f;
            GUI.color = new Color(0.18f, 0.18f, 0.22f, 0.95f);
            GUI.DrawTexture(new Rect(panelX, y, panelW, 44f), _white);

            GUI.color = new Color(0.75f, 0.75f, 0.8f);
            if (GUI.Button(new Rect(panelX, y, panelW, 44f), "DORMIR", _button))
                EnterDream();

            GUI.color = Color.white;
        }

        private void DrawStat(float x, ref float y, float w, string label, float value, float max, Color barColor)
        {
            GUI.color = new Color(0.6f, 0.6f, 0.65f);
            GUI.Label(new Rect(x, y, w, 20f), label, _body);
            y += 20f;

            float barH = 10f, barW = w;
            GUI.color = new Color(0.2f, 0.2f, 0.22f);
            GUI.DrawTexture(new Rect(x, y, barW, barH), _white);

            float fill = Mathf.Clamp01(value / max);
            GUI.color = barColor;
            GUI.DrawTexture(new Rect(x, y, barW * fill, barH), _white);
            y += 18f;
        }

        private void EnterDream()
        {
            GameManager.Instance?.EnterDream();
        }

        private void EnsureStyles()
        {
            if (_title != null) return;

            _title = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize  = 18,
                fontStyle = FontStyle.Bold
            };
            _title.normal.textColor = Color.white;

            _body = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleLeft,
                fontSize  = 13
            };
            _body.normal.textColor = Color.white;

            _stat = new GUIStyle(_body) { fontStyle = FontStyle.Bold };
            _stat.normal.textColor = Color.white;

            _button = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize  = 15,
                fontStyle = FontStyle.Bold
            };
            _button.normal.textColor = new Color(0.85f, 0.85f, 0.9f);
            _button.hover.textColor  = Color.white;
        }
    }
}
