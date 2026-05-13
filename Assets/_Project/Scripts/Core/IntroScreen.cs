using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Restless.Core
{
    public class IntroScreen : MonoBehaviour
    {
        [SerializeField] private Texture2D _image;
        [SerializeField] private AudioClip _music;
        [SerializeField] private float _fadeInDuration  = 2.5f;
        [SerializeField] private float _textDelay       = 4.0f;
        [SerializeField] private float _textFadeDuration = 2.5f;
[SerializeField] private string _vigilSceneName = "Vigil";

        private float       _elapsed;
        private float       _imageAlpha;
        private bool        _done;
        private GUIStyle    _bodyStyle;
        private GUIStyle    _promptStyle;
        private Texture2D   _white;
        private AudioSource _audio;

        private const string BODY_TEXT =
            "Tu hijo ha sido arrastrado a las profundidades del sueño\n" +
            "por una presencia que no comprende la luz.\n\n" +
            "Cada noche entras en ese abismo en busca de sus recuerdos.\n" +
            "Reúnelos. Encuéntralo.";

        private const string PROMPT_TEXT = "Pulsa cualquier tecla para continuar";

        private void Awake()
        {
            _white = new Texture2D(1, 1);
            _white.SetPixel(0, 0, Color.white);
            _white.Apply();

            _audio = gameObject.AddComponent<AudioSource>();
            _audio.loop        = true;
            _audio.spatialBlend = 0f;
            _audio.volume      = 0f;
            _audio.playOnAwake = false;
            if (_music != null)
            {
                _audio.clip = _music;
                _audio.Play();
            }
        }

        private IEnumerator Start()
        {
            yield return null;
            StartCoroutine(FadeIn());
        }

        private IEnumerator FadeIn()
        {
            while (_elapsed < _fadeInDuration)
            {
                _elapsed += Time.deltaTime;
                _imageAlpha = Mathf.Clamp01(_elapsed / _fadeInDuration);
                if (_audio != null) _audio.volume = _imageAlpha * 0.6f;
                yield return null;
            }
            while (true)
            {
                _elapsed += Time.deltaTime;
                yield return null;
            }
        }

        private void Update()
        {
            if (_done) return;
            if (_elapsed < _textDelay + _textFadeDuration * 0.5f) return;

            bool anyKey    = Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame;
            bool click     = Mouse.current    != null && Mouse.current.leftButton.wasPressedThisFrame;
            bool gamepad   = Gamepad.current  != null && Gamepad.current.buttonSouth.wasPressedThisFrame;
            if (anyKey || click || gamepad)
                StartCoroutine(Proceed());
        }

        private IEnumerator Proceed()
        {
            _done = true;
            float t = 0f;
            float startAlpha = _imageAlpha;
            float startVol   = _audio != null ? _audio.volume : 0f;
            while (t < 1.0f)
            {
                t += Time.deltaTime;
                float progress = t / 1.0f;
                _imageAlpha = Mathf.Lerp(startAlpha, 0f, progress);
                if (_audio != null) _audio.volume = Mathf.Lerp(startVol, 0f, progress);
                yield return null;
            }
            SceneManager.LoadScene(_vigilSceneName);
        }

        private void OnGUI()
        {
            if (_image == null || _imageAlpha <= 0f) return;
            EnsureStyles();

            float sw = Screen.width, sh = Screen.height;

            // Full-screen image
            GUI.color = new Color(1f, 1f, 1f, _imageAlpha);
            GUI.DrawTexture(new Rect(0, 0, sw, sh), _image, ScaleMode.ScaleToFit);

            // Body text + gradient — only after text delay
            float textT = Mathf.Clamp01((_elapsed - _textDelay) / _textFadeDuration);
            if (textT > 0f)
            {
                // Dark gradient at the bottom for text readability
                float gradH = sh * 0.42f;
                GUI.color = new Color(0f, 0f, 0f, _imageAlpha * 0.78f * textT);
                GUI.DrawTexture(new Rect(0, sh - gradH, sw, gradH), _white);

                GUI.color = new Color(0.82f, 0.75f, 0.58f, _imageAlpha * textT);
                float tw = sw * 0.6f;
                float tx = (sw - tw) * 0.5f;
                float ty = sh - gradH + 28f;
                GUI.Label(new Rect(tx, ty, tw, gradH * 0.6f), BODY_TEXT, _bodyStyle);

                // Prompt appears together with text
                float blink = 0.55f + 0.45f * Mathf.Sin(Time.time * 2.8f);
                GUI.color = new Color(0.55f, 0.52f, 0.44f, _imageAlpha * textT * blink);
                GUI.Label(new Rect(0, sh - 44f, sw, 30f), PROMPT_TEXT, _promptStyle);
            }

            GUI.color = Color.white;
        }

        private void EnsureStyles()
        {
            if (_bodyStyle != null) return;

            _bodyStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize  = 22,
                fontStyle = FontStyle.Normal,
                alignment = TextAnchor.UpperCenter,
                wordWrap  = true,
                richText  = false
            };

            _promptStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize  = 16,
                fontStyle = FontStyle.Italic,
                alignment = TextAnchor.MiddleCenter
            };
        }
    }
}
