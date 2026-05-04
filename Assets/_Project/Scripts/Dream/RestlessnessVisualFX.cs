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
        [SerializeField] private float _vignetteIdle     = 0.25f;
        [SerializeField] private float _vignetteCritical = 0.58f;  // capped so scene stays readable

        [Header("Vignette pulse (High+, simulates heartbeat)")]
        [SerializeField] private float _pulseAmplitude  = 0.04f;
        [SerializeField] private float _pulseSpeedHigh  = 1.2f;
        [SerializeField] private float _pulseSpeedCrit  = 2.8f;

        [Header("Chromatic Aberration")]
        [SerializeField] private float _chromaticMedium   = 0.12f;
        [SerializeField] private float _chromaticHigh     = 0.35f;
        [SerializeField] private float _chromaticCritical = 0.65f;  // capped for readability

        [Header("Lens Distortion")]
        [SerializeField] private float _distortionHigh     = -0.12f;
        [SerializeField] private float _distortionCritical = -0.28f;

        [Header("Threshold flash")]
        [SerializeField] private float _flashDuration = 0.25f;
        [SerializeField] private float _flashAlphaMax = 0.38f;

        private Volume   _volume;
        private Vignette _vignette;
        private ChromaticAberration _chromatic;
        private LensDistortion      _distortion;

        private RestlessnessManager.Threshold _lastThreshold;
        private float  _flashAlpha;
        private Color  _flashColor;
        private float  _pulseTime;
        private float  _lastZoneMultiplier = 1f;
        private Texture2D _white;

        private void Awake()
        {
            _volume = GetComponent<Volume>();
            if (_volume.profile == null)
                _volume.profile = ScriptableObject.CreateInstance<VolumeProfile>();

            if (!_volume.profile.TryGet(out _vignette))
                _vignette = _volume.profile.Add<Vignette>(true);
            if (!_volume.profile.TryGet(out _chromatic))
                _chromatic = _volume.profile.Add<ChromaticAberration>(true);
            if (!_volume.profile.TryGet(out _distortion))
                _distortion = _volume.profile.Add<LensDistortion>(true);

            _vignette.active   = true;
            _chromatic.active  = true;
            _distortion.active = true;

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

            float t         = RestlessnessManager.Instance.NormalizedValue;
            var   threshold = RestlessnessManager.Instance.CurrentThreshold;

            // Flash on ascending threshold crossings
            if (threshold != _lastThreshold)
            {
                if (threshold > _lastThreshold)
                {
                    _flashAlpha = _flashAlphaMax;
                    _flashColor = FlashColor(threshold);
                }
                _lastThreshold = threshold;
            }

            // Flash on entering a higher-danger zone (zone multiplier increase)
            float zone = RestlessnessManager.Instance.ZoneMultiplier;
            if (zone > _lastZoneMultiplier + 0.01f)
            {
                _flashAlpha = _flashAlphaMax * 0.7f;
                _flashColor = zone >= 2f ? new Color(1f, 0.1f, 0.05f) : new Color(1f, 0.55f, 0.05f);
            }
            _lastZoneMultiplier = zone;

            _flashAlpha = Mathf.MoveTowards(_flashAlpha, 0f, Time.deltaTime / _flashDuration);

            // Pulse (heartbeat) kicks in above 50% restlessness, speeds up at Critical
            float pulseBlend = Mathf.Clamp01(Mathf.InverseLerp(0.5f, 1f, t));
            float pulseSpeed = Mathf.Lerp(_pulseSpeedHigh, _pulseSpeedCrit, pulseBlend);
            _pulseTime += Time.deltaTime * pulseSpeed;
            float pulse = Mathf.Sin(_pulseTime) * _pulseAmplitude * pulseBlend;

            // Vignette: base curve + pulse + dark-red tint at Critical
            float baseVig = Mathf.Lerp(_vignetteIdle, _vignetteCritical, Mathf.Pow(t, 1.5f));
            _vignette.intensity.Override(Mathf.Lerp(
                _vignette.intensity.value, baseVig + pulse,
                Time.deltaTime * _lerpSpeed));

            // Vignette color fades from black to dark red as restlessness passes 0.5
            var vigColor = Color.Lerp(Color.black, new Color(0.45f, 0f, 0f),
                                      Mathf.InverseLerp(0.5f, 1f, t));
            _vignette.color.Override(vigColor);

            // Chromatic aberration: 0 at Low, escalates through thresholds
            float targetChromatic = t < 0.25f ? 0f
                : t < 0.5f  ? Mathf.InverseLerp(0.25f, 0.5f,  t) * _chromaticMedium
                : t < 0.75f ? Mathf.Lerp(_chromaticMedium,  _chromaticHigh,     Mathf.InverseLerp(0.5f,  0.75f, t))
                             : Mathf.Lerp(_chromaticHigh,    _chromaticCritical, Mathf.InverseLerp(0.75f, 1f,    t));
            _chromatic.intensity.Override(Mathf.Lerp(
                _chromatic.intensity.value, targetChromatic,
                Time.deltaTime * _lerpSpeed));

            // Lens distortion: starts at High threshold
            float targetDistortion = t < 0.5f ? 0f
                : t < 0.75f ? Mathf.Lerp(0f, _distortionHigh,     Mathf.InverseLerp(0.5f,  0.75f, t))
                             : Mathf.Lerp(_distortionHigh, _distortionCritical, Mathf.InverseLerp(0.75f, 1f,    t));
            _distortion.intensity.Override(Mathf.Lerp(
                _distortion.intensity.value, targetDistortion,
                Time.deltaTime * _lerpSpeed));
        }

        private void OnGUI()
        {
            if (_flashAlpha <= 0f) return;
            GUI.color = new Color(_flashColor.r, _flashColor.g, _flashColor.b, _flashAlpha);
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), _white);
            GUI.color = Color.white;
        }

        private static Color FlashColor(RestlessnessManager.Threshold t) => t switch
        {
            RestlessnessManager.Threshold.Medium   => new Color(1f,    0.9f,  0.3f),  // yellow
            RestlessnessManager.Threshold.High     => new Color(1f,    0.45f, 0.05f), // orange
            RestlessnessManager.Threshold.Critical => new Color(1f,    0.1f,  0.05f), // red
            RestlessnessManager.Threshold.Max      => new Color(0.9f,  0f,    0.2f),  // deep red
            _ => new Color(1f, 0.9f, 0.3f)
        };
    }
}
