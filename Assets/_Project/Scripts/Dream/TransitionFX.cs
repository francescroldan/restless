using System.Collections;
using UnityEngine;

namespace Restless.Dream
{
    /// <summary>
    /// Handles visual transitions for entering/exiting the dream.
    /// Sleep: black fade-in with a brief white pulse at the moment of crossing.
    /// Voluntary wake: soft grey-black fade.
    /// Abrupt wake: red flash + camera shake, then fast black fade.
    /// Call Begin() from WakeUpManager; SceneLoader drives the actual scene switch.
    /// </summary>
    public class TransitionFX : MonoBehaviour
    {
        public static TransitionFX Instance { get; private set; }

        [Header("Fade")]
        [SerializeField] private float _sleepFadeDuration    = 1.2f;
        [SerializeField] private float _voluntaryFadeDuration = 0.8f;
        [SerializeField] private float _abruptFadeDuration   = 0.4f;

        [Header("Sleep pulse")]
        [SerializeField] private float _pulseDuration = 0.18f;
        [SerializeField] private Color _pulseColor    = new Color(1f, 1f, 1f, 0.6f);

        [Header("Abrupt shake")]
        [SerializeField] private float _shakeDuration  = 0.35f;
        [SerializeField] private float _shakeMagnitude = 0.18f;

        public bool IsPlaying { get; private set; }

        private float _fadeAlpha;
        private Color _fadeColor = Color.black;
        private float _pulseAlpha;
        private bool _showPulse;
        private string _overlayText;
        private float _textAlpha;
        private GUIStyle _textStyle;

        private Transform _cameraTransform;
        private Vector3 _cameraOrigin;
        private float _shakeTimer;

        private Texture2D _white;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;

            _white = new Texture2D(1, 1);
            _white.SetPixel(0, 0, Color.white);
            _white.Apply();
        }

        private void Start()
        {
            var cam = Camera.main;
            if (cam != null) _cameraTransform = cam.transform;
        }

        // ── Public API ────────────────────────────────────────────────────

        public void BeginSleep(System.Action onComplete)   => StartCoroutine(PlaySleep(onComplete));
        public void BeginVoluntary(System.Action onComplete) => StartCoroutine(PlayVoluntary(onComplete));
        public void BeginAbrupt(System.Action onComplete)  => StartCoroutine(PlayAbrupt(onComplete));

        // ── Coroutines ────────────────────────────────────────────────────

        private IEnumerator PlaySleep(System.Action onComplete)
        {
            IsPlaying = true;
            _fadeColor = Color.black;

            // Brief white pulse at sleep onset
            _showPulse = true;
            _pulseAlpha = _pulseColor.a;
            float t = 0f;
            while (t < _pulseDuration)
            {
                t += Time.unscaledDeltaTime;
                _pulseAlpha = Mathf.Lerp(_pulseColor.a, 0f, t / _pulseDuration);
                yield return null;
            }
            _showPulse = false;

            // Fade to black
            yield return Fade(0f, 1f, _sleepFadeDuration);

            IsPlaying = false;
            onComplete?.Invoke();
        }

        private IEnumerator PlayVoluntary(System.Action onComplete)
        {
            IsPlaying = true;
            _fadeColor = Color.black;
            _overlayText = "despertando...";
            yield return Fade(0f, 1f, _voluntaryFadeDuration);
            _overlayText = null;
            IsPlaying = false;
            onComplete?.Invoke();
        }

        private IEnumerator PlayAbrupt(System.Action onComplete)
        {
            IsPlaying = true;

            // Red flash + shake simultaneously
            _fadeColor = Color.red;
            _fadeAlpha = 0.75f;

            if (_cameraTransform != null)
                _cameraOrigin = _cameraTransform.localPosition;

            float elapsed = 0f;
            while (elapsed < _shakeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float progress = elapsed / _shakeDuration;
                _fadeAlpha = Mathf.Lerp(0.75f, 0f, progress);

                if (_cameraTransform != null)
                {
                    float strength = _shakeMagnitude * (1f - progress);
                    _cameraTransform.localPosition = _cameraOrigin
                        + new Vector3(Random.Range(-strength, strength), Random.Range(-strength, strength), 0f);
                }
                yield return null;
            }

            if (_cameraTransform != null)
                _cameraTransform.localPosition = _cameraOrigin;

            // Fast black fade with text
            _fadeColor = Color.black;
            _overlayText = "DESPERTAR ABRUPTO";
            yield return Fade(0f, 1f, _abruptFadeDuration);
            _overlayText = null;

            IsPlaying = false;
            onComplete?.Invoke();
        }

        private IEnumerator Fade(float from, float to, float duration)
        {
            float t = 0f;
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                _fadeAlpha = Mathf.Lerp(from, to, t / duration);
                yield return null;
            }
            _fadeAlpha = to;
        }

        // ── Rendering ─────────────────────────────────────────────────────

        private void OnGUI()
        {
            if (_fadeAlpha > 0f)
            {
                GUI.color = new Color(_fadeColor.r, _fadeColor.g, _fadeColor.b, _fadeAlpha);
                GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), _white);
            }

            if (_showPulse && _pulseAlpha > 0f)
            {
                GUI.color = new Color(_pulseColor.r, _pulseColor.g, _pulseColor.b, _pulseAlpha);
                GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), _white);
            }

            if (!string.IsNullOrEmpty(_overlayText) && _fadeAlpha > 0.3f)
            {
                EnsureTextStyle();
                float textAlpha = Mathf.InverseLerp(0.3f, 0.6f, _fadeAlpha);
                GUI.color = new Color(1f, 1f, 1f, textAlpha);
                GUI.Label(new Rect(0, Screen.height * 0.5f - 20f, Screen.width, 40f), _overlayText, _textStyle);
            }

            GUI.color = Color.white;
        }

        private void EnsureTextStyle()
        {
            if (_textStyle != null) return;
            _textStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize   = 20,
                fontStyle  = FontStyle.Bold
            };
            _textStyle.normal.textColor = Color.white;
        }
    }
}
