using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Restless.Dream
{
    /// <summary>
    /// Animates post-process Volume parameters based on the current Restlessness level.
    /// Attach to a GameObject with a Volume component (Global, weight=1).
    /// </summary>
    [RequireComponent(typeof(Volume))]
    public class RestlessnessVisualFX : MonoBehaviour
    {
        [Header("Transition speed")]
        [SerializeField] private float _lerpSpeed = 2f;

        [Header("Vignette")]
        [SerializeField] private float _vignetteIdle       = 0.25f;
        [SerializeField] private float _vignetteMedium     = 0.35f;
        [SerializeField] private float _vignetteHigh       = 0.48f;
        [SerializeField] private float _vignetteCritical   = 0.62f;

        [Header("Chromatic Aberration")]
        [SerializeField] private float _chromaticMedium    = 0.15f;
        [SerializeField] private float _chromaticHigh      = 0.45f;
        [SerializeField] private float _chromaticCritical  = 0.85f;

        [Header("Lens Distortion")]
        [SerializeField] private float _distortionHigh     = -0.15f;
        [SerializeField] private float _distortionCritical = -0.35f;

        [Header("Threshold flash")]
        [SerializeField] private float _flashDuration = 0.18f;
        [SerializeField] private Color _flashColor    = new Color(1f, 0.9f, 0.3f, 0.35f);

        private Volume _volume;
        private Vignette _vignette;
        private ChromaticAberration _chromatic;
        private LensDistortion _distortion;

        private RestlessnessManager.Threshold _lastThreshold;
        private float _flashAlpha;
        private Texture2D _white;
        private GUIStyle _dummyStyle;

        private void Awake()
        {
            _volume = GetComponent<Volume>();

            if (_volume.profile == null)
                _volume.profile = ScriptableObject.CreateInstance<VolumeProfile>();

            // Get or add overrides
            if (!_volume.profile.TryGet(out _vignette))
                _vignette = _volume.profile.Add<Vignette>(true);

            if (!_volume.profile.TryGet(out _chromatic))
                _chromatic = _volume.profile.Add<ChromaticAberration>(true);

            if (!_volume.profile.TryGet(out _distortion))
                _distortion = _volume.profile.Add<LensDistortion>(true);

            _vignette.active    = true;
            _chromatic.active   = true;
            _distortion.active  = true;

            _white = new Texture2D(1, 1);
            _white.SetPixel(0, 0, Color.white);
            _white.Apply();
        }

        private void Start()
        {
            if (RestlessnessManager.Instance != null)
                _lastThreshold = RestlessnessManager.Instance.CurrentThreshold;
        }

        private void Update()
        {
            if (RestlessnessManager.Instance == null) return;

            float t = RestlessnessManager.Instance.NormalizedValue;
            var threshold = RestlessnessManager.Instance.CurrentThreshold;

            // Threshold crossing flash
            if (threshold != _lastThreshold)
            {
                _flashAlpha = _flashColor.a;
                _lastThreshold = threshold;
            }
            _flashAlpha = Mathf.MoveTowards(_flashAlpha, 0f, Time.deltaTime / _flashDuration);

            // Target values by restlessness 0..1
            float targetVignette  = Mathf.Lerp(_vignetteIdle, _vignetteCritical, Mathf.Pow(t, 1.5f));
            float targetChromatic = t < 0.25f ? 0f
                : t < 0.5f  ? Mathf.InverseLerp(0.25f, 0.5f,  t) * _chromaticMedium
                : t < 0.75f ? Mathf.Lerp(_chromaticMedium, _chromaticHigh,     Mathf.InverseLerp(0.5f,  0.75f, t))
                             : Mathf.Lerp(_chromaticHigh,   _chromaticCritical, Mathf.InverseLerp(0.75f, 1f,    t));

            float targetDistortion = t < 0.5f ? 0f
                : t < 0.75f ? Mathf.Lerp(0f, _distortionHigh,     Mathf.InverseLerp(0.5f,  0.75f, t))
                             : Mathf.Lerp(_distortionHigh, _distortionCritical, Mathf.InverseLerp(0.75f, 1f,    t));

            float dt = Time.deltaTime * _lerpSpeed;
            _vignette.intensity.Override(  Mathf.Lerp(_vignette.intensity.value,   targetVignette,   dt));
            _chromatic.intensity.Override( Mathf.Lerp(_chromatic.intensity.value,  targetChromatic,  dt));
            _distortion.intensity.Override(Mathf.Lerp(_distortion.intensity.value, targetDistortion, dt));
        }

        private void OnGUI()
        {
            if (_flashAlpha <= 0f) return;
            GUI.color = new Color(_flashColor.r, _flashColor.g, _flashColor.b, _flashAlpha);
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), _white);
            GUI.color = Color.white;
        }
    }
}
