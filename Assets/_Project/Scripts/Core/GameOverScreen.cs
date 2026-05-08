using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Restless.Core
{
    public class GameOverScreen : MonoBehaviour
    {
        [SerializeField] private Texture2D _imagePhysical;
        [SerializeField] private Texture2D _imageMental;
        [SerializeField] private AudioClip _music;
        [SerializeField] private float _fadeInDuration   = 3.0f;
        [SerializeField] private float _textDelay        = 4.0f;
        [SerializeField] private float _textFadeDuration = 3.0f;
        [SerializeField] private string _vigilSceneName  = "Vigil";

        private Texture2D   _image;
        private float       _elapsed;
        private float       _alpha;
        private bool        _done;
        private GUIStyle    _titleStyle;
        private GUIStyle    _bodyStyle;
        private GUIStyle    _promptStyle;
        private Texture2D   _white;
        private AudioSource _audio;

        private const string PROMPT_TEXT = "Pulsa cualquier tecla para continuar";

        private static readonly string[] TITLE_PHYSICAL = { "Ha muerto." };
        private static readonly string[] BODY_PHYSICAL  = { "Su cuerpo no resistió más.\nLa llama se apagó en silencio." };

        private static readonly string[] TITLE_MENTAL   = { "Se ha perdido." };
        private static readonly string[] BODY_MENTAL    = { "Su mente se quebró en el abismo.\nEl Rey Amarillo lo reclamó." };

        private string _titleText;
        private string _bodyText;

        private void Awake()
        {
            _white = new Texture2D(1, 1);
            _white.SetPixel(0, 0, Color.white);
            _white.Apply();

            _audio = gameObject.AddComponent<AudioSource>();
            _audio.loop         = true;
            _audio.spatialBlend = 0f;
            _audio.volume       = 0f;
            _audio.playOnAwake  = false;
        }

        private IEnumerator Start()
        {
            yield return null;

            bool isMental = SceneLoader.Instance != null &&
                            SceneLoader.Instance.LastGameOverType == GameManager.GameOverType.Mental;

            _image     = isMental ? _imageMental   : _imagePhysical;
            _titleText = isMental ? TITLE_MENTAL[0] : TITLE_PHYSICAL[0];
            _bodyText  = isMental ? BODY_MENTAL[0]  : BODY_PHYSICAL[0];

            if (_music != null)
            {
                _audio.clip = _music;
                _audio.Play();
            }

            StartCoroutine(FadeIn());
        }

        private IEnumerator FadeIn()
        {
            while (_elapsed < _fadeInDuration)
            {
                _elapsed += Time.deltaTime;
                _alpha = Mathf.Clamp01(_elapsed / _fadeInDuration);
                if (_audio != null) _audio.volume = _alpha * 0.5f;
                yield return null;
            }
            while (true) { _elapsed += Time.deltaTime; yield return null; }
        }

        private void Update()
        {
            if (_done) return;
            if (_elapsed < _textDelay + _textFadeDuration * 0.5f) return;

            bool anyKey  = Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame;
            bool click   = Mouse.current    != null && Mouse.current.leftButton.wasPressedThisFrame;
            bool gamepad = Gamepad.current  != null && Gamepad.current.buttonSouth.wasPressedThisFrame;
            if (anyKey || click || gamepad)
                StartCoroutine(Proceed());
        }

        private IEnumerator Proceed()
        {
            _done = true;
            float t = 0f, startAlpha = _alpha, startVol = _audio != null ? _audio.volume : 0f;
            while (t < 1.0f)
            {
                t += Time.deltaTime;
                _alpha = Mathf.Lerp(startAlpha, 0f, t);
                if (_audio != null) _audio.volume = Mathf.Lerp(startVol, 0f, t);
                yield return null;
            }

            if (GameManager.Instance != null)
                GameManager.Instance.StartNewGame();
            else
                SceneManager.LoadScene(_vigilSceneName);
        }

        private void OnGUI()
        {
            if (_image == null || _alpha <= 0f) return;
            EnsureStyles();

            float sw = Screen.width, sh = Screen.height;

            GUI.color = new Color(1f, 1f, 1f, _alpha);
            GUI.DrawTexture(new Rect(0, 0, sw, sh), _image, ScaleMode.ScaleToFit);

            float textT = Mathf.Clamp01((_elapsed - _textDelay) / _textFadeDuration);
            if (textT <= 0f) { GUI.color = Color.white; return; }

            // Gradient
            float gradH = sh * 0.45f;
            GUI.color = new Color(0f, 0f, 0f, _alpha * 0.85f * textT);
            GUI.DrawTexture(new Rect(0, sh - gradH, sw, gradH), _white);

            // Title
            GUI.color = new Color(0.9f, 0.85f, 0.70f, _alpha * textT);
            GUI.Label(new Rect(0, sh - gradH + 20f, sw, 40f), _titleText, _titleStyle);

            // Body
            GUI.color = new Color(0.75f, 0.68f, 0.52f, _alpha * textT);
            float tw = sw * 0.55f, tx = (sw - tw) * 0.5f;
            GUI.Label(new Rect(tx, sh - gradH + 68f, tw, gradH * 0.5f), _bodyText, _bodyStyle);

            // Prompt
            float blink = 0.5f + 0.5f * Mathf.Sin(Time.time * 2.8f);
            GUI.color = new Color(0.55f, 0.50f, 0.40f, _alpha * textT * blink);
            GUI.Label(new Rect(0, sh - 44f, sw, 30f), PROMPT_TEXT, _promptStyle);

            GUI.color = Color.white;
        }

        private void EnsureStyles()
        {
            if (_titleStyle != null) return;

            _titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize  = 28,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };

            _bodyStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize  = 20,
                fontStyle = FontStyle.Normal,
                alignment = TextAnchor.UpperCenter,
                wordWrap  = true
            };

            _promptStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize  = 15,
                fontStyle = FontStyle.Italic,
                alignment = TextAnchor.MiddleCenter
            };
        }
    }
}
