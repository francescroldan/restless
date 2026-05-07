using System.Collections;
using UnityEngine;
using Restless.Core;

namespace Restless.Vigil
{
    public class DemoCompleteScreen : MonoBehaviour
    {
        [SerializeField] private float _fadeInDuration  = 2.5f;
        [SerializeField] private float _textDelay       = 1.2f;
        [SerializeField] private float _buttonDelay     = 3.5f;  // seconds after fade starts

        private float     _alpha     = 0f;
        private float     _elapsed   = 0f;
        private bool      _triggered = false;
        private Texture2D _white;
        private GUIStyle  _titleStyle;
        private GUIStyle  _subtitleStyle;
        private GUIStyle  _buttonStyle;

        private void Awake()
        {
            _white = new Texture2D(1, 1);
            _white.SetPixel(0, 0, Color.white);
            _white.Apply();
        }

        private void Update()
        {
            if (_triggered || SaveManager.Instance == null) return;
            if (SaveManager.Instance.CollectedFragmentCount >= GameManager.Instance.DemoFragmentTarget)
            {
                _triggered = true;
                StartCoroutine(FadeIn());
            }
        }

        private IEnumerator FadeIn()
        {
            while (_elapsed < _fadeInDuration)
            {
                _elapsed += Time.deltaTime;
                _alpha = Mathf.Clamp01(_elapsed / _fadeInDuration);
                yield return null;
            }
            // Keep incrementing so button delay works
            while (true)
            {
                _elapsed += Time.deltaTime;
                yield return null;
            }
        }

        private void OnGUI()
        {
            if (!_triggered || _alpha <= 0f) return;
            EnsureStyles();

            // Black overlay
            GUI.color = new Color(0f, 0f, 0f, _alpha);
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), _white);

            // Text appears after the overlay is mostly opaque
            float textT = Mathf.Clamp01((_alpha - (_textDelay / _fadeInDuration)) / (1f - (_textDelay / _fadeInDuration)));

            if (textT > 0f)
            {
                float cx = Screen.width  * 0.5f;
                float cy = Screen.height * 0.5f;

                GUI.color = new Color(0.88f, 0.78f, 0.55f, textT);
                GUI.Label(new Rect(cx - 300f, cy - 60f, 600f, 60f),
                    "Has recuperado los recuerdos de tu hijo.", _titleStyle);

                GUI.color = new Color(0.55f, 0.52f, 0.46f, textT);
                GUI.Label(new Rect(cx - 200f, cy + 10f, 400f, 40f),
                    "— Demo completada —", _subtitleStyle);

                // Button appears after _buttonDelay seconds
                float buttonT = Mathf.Clamp01((_elapsed - _buttonDelay) / 0.8f);
                if (buttonT > 0f)
                {
                    GUI.color = new Color(0.75f, 0.65f, 0.42f, buttonT);
                    float bw = 200f, bh = 36f;
                    var btnRect = new Rect(cx - bw * 0.5f, cy + 70f, bw, bh);
                    if (GUI.Button(btnRect, "Volver a jugar", _buttonStyle))
                        GameManager.Instance?.StartNewGame();
                }
            }

            GUI.color = Color.white;
        }

        private void EnsureStyles()
        {
            if (_titleStyle != null) return;

            _titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize  = 22,
                fontStyle = FontStyle.Normal,
                alignment = TextAnchor.MiddleCenter,
                wordWrap  = true
            };
            _subtitleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize  = 14,
                fontStyle = FontStyle.Italic,
                alignment = TextAnchor.MiddleCenter
            };
            _buttonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize  = 13,
                fontStyle = FontStyle.Normal,
                alignment = TextAnchor.MiddleCenter
            };
        }
    }
}
