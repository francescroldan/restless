using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Restless.Vigil
{
    /// <summary>
    /// Handles the screen-space fade/flash effects for the Vigilia scene:
    ///   Entry (tranquil)  — slow fade-in from black.
    ///   Entry (abrupt)    — red flash, then fade-in from black.
    ///   Sleep transition  — fade to black, then calls back to trigger scene load.
    /// </summary>
    public class VigiliaTransitionFX : MonoBehaviour
    {
        public static VigiliaTransitionFX Instance { get; private set; }

        [Header("Entry")]
        [SerializeField] private float _fadeInDuration     = 1.6f;
        [SerializeField] private float _abruptFlashDuration = 0.4f;
        [SerializeField] private Color _abruptFlashColor    = new Color(0.75f, 0.05f, 0.05f, 0.8f);

        [Header("Sleep")]
        [SerializeField] private float _sleepFadeDuration = 1.2f;

        private float  _alpha;
        private Color  _overlayColor = Color.black;
        private bool   _active;
        private Texture2D _white;

        private void Awake()
        {
            if (Instance != null && Instance != this) Destroy(Instance);
            Instance = this;

            _white = new Texture2D(1, 1);
            _white.SetPixel(0, 0, Color.white);
            _white.Apply();

            _alpha = 1f;
            _active = true;
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            // After the sleep fade (Vigil→Dream), fade out to reveal the dream scene.
            // On return to Vigil, PlayEntry() handles the fade-in instead.
            if (scene.name == "Dream" && _active && _alpha >= 0.99f)
                StartCoroutine(ClearFade());
        }

        private IEnumerator ClearFade()
        {
            yield return FadeAlpha(1f, 0f, 1.0f);
            _active = false;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        // ── Public API ─────────────────────────────────────────────────────

        public void PlayEntry(bool wasAbrupt) =>
            StartCoroutine(wasAbrupt ? EntryAbrupt() : EntryTranquil());

        public void PlaySleep(System.Action onFadeComplete) =>
            StartCoroutine(SleepFade(onFadeComplete));

        // ── Routines ───────────────────────────────────────────────────────

        private IEnumerator EntryTranquil()
        {
            _overlayColor = Color.black;
            _alpha = 1f;
            _active = true;

            yield return FadeAlpha(1f, 0f, _fadeInDuration);
            _active = false;
        }

        private IEnumerator EntryAbrupt()
        {
            _overlayColor = _abruptFlashColor;
            _alpha = 1f;
            _active = true;

            // Red flash fades out quickly
            yield return FadeAlpha(1f, 0f, _abruptFlashDuration);

            // Brief black hold
            _overlayColor = Color.black;
            _alpha = 0.6f;
            yield return new WaitForSeconds(0.1f);

            // Then slow fade from black
            yield return FadeAlpha(0.6f, 0f, _fadeInDuration);
            _active = false;
        }

        private IEnumerator SleepFade(System.Action onComplete)
        {
            _overlayColor = Color.black;
            _alpha = 0f;
            _active = true;

            yield return FadeAlpha(0f, 1f, _sleepFadeDuration);

            onComplete?.Invoke();
        }

        private IEnumerator FadeAlpha(float from, float to, float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                _alpha = Mathf.Lerp(from, to, elapsed / duration);
                yield return null;
            }
            _alpha = to;
        }

        // ── Rendering ──────────────────────────────────────────────────────

        private void OnGUI()
        {
            if (!_active || _alpha <= 0.01f) return;
            GUI.color = new Color(_overlayColor.r, _overlayColor.g, _overlayColor.b, _alpha);
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), _white);
            GUI.color = Color.white;
        }
    }
}
