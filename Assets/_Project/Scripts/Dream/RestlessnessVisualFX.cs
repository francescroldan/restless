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
        public static RestlessnessVisualFX Instance { get; private set; }
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

        [Header("Max restlessness red veil")]
        [SerializeField] private float _maxVeilBaseAlpha  = 0.10f;
        [SerializeField] private float _maxVeilPulseDepth = 0.10f;
        [SerializeField] private float _maxVeilPulseRate  = 2.2f;

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
        private bool   _maxReached;
        private float  _maxVeilTime;
        private float  _buzzChromatic;
        private float  _buzzVignette;

        [Header("Detection buzz")]
        [SerializeField] private float _buzzChromaticStrength = 0.6f;
        [SerializeField] private float _buzzVignetteStrength  = 0.15f;
        [SerializeField] private float _buzzDecaySpeed        = 3.5f;

        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;

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

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
            if (RestlessnessManager.Instance != null)
                RestlessnessManager.Instance.OnMaxReached -= OnMaxReached;
        }

        private void Start()
        {
            if (RestlessnessManager.Instance != null)
            {
                _lastThreshold = RestlessnessManager.Instance.CurrentThreshold;
                RestlessnessManager.Instance.OnMaxReached += OnMaxReached;
            }
        }

        private void OnMaxReached() => _maxReached = true;

        public void TriggerDetectionBuzz()
        {
            var run = Core.RunConfig.Current;
            _buzzChromatic = run?.buzzChromaticStrength ?? _buzzChromaticStrength;
            _buzzVignette  = run?.buzzVignetteStrength  ?? _buzzVignetteStrength;
        }

        private void Update()
        {
            if (RestlessnessManager.Instance == null) return;

            var   run = Core.RunConfig.Current;
            float vigIdle         = run?.vignetteIdle               ?? _vignetteIdle;
            float vigCritical     = run?.vignetteCritical            ?? _vignetteCritical;
            float pulseAmp        = run?.vignettePulseAmplitude      ?? _pulseAmplitude;
            float pulseHigh       = run?.vignettePulseSpeedHigh      ?? _pulseSpeedHigh;
            float pulseCrit       = run?.vignettePulseSpeedCritical  ?? _pulseSpeedCrit;
            float chromMedium     = run?.chromaticMedium             ?? _chromaticMedium;
            float chromHigh       = run?.chromaticHigh               ?? _chromaticHigh;
            float chromCritical   = run?.chromaticCritical           ?? _chromaticCritical;
            float distHigh        = run?.lensDistortionHigh          ?? _distortionHigh;
            float distCritical    = run?.lensDistortionCritical      ?? _distortionCritical;
            float flashDur        = run?.thresholdFlashDuration      ?? _flashDuration;
            float flashMax        = run?.thresholdFlashAlphaMax      ?? _flashAlphaMax;
            float veilBase        = run?.maxVeilBaseAlpha            ?? _maxVeilBaseAlpha;
            float veilDepth       = run?.maxVeilPulseDepth           ?? _maxVeilPulseDepth;
            float veilRate        = run?.maxVeilPulseRate            ?? _maxVeilPulseRate;
            float buzzDecay       = run?.buzzDecaySpeed              ?? _buzzDecaySpeed;
            float lerpSpd         = run?.fxLerpSpeed                 ?? _lerpSpeed;

            float t         = RestlessnessManager.Instance.NormalizedValue;
            var   threshold = RestlessnessManager.Instance.CurrentThreshold;

            if (threshold != _lastThreshold)
            {
                if (threshold > _lastThreshold)
                {
                    _flashAlpha = flashMax;
                    _flashColor = FlashColor(threshold);
                }
                _lastThreshold = threshold;
            }

            float zone = RestlessnessManager.Instance.ZoneMultiplier;
            if (zone > _lastZoneMultiplier + 0.01f)
            {
                _flashAlpha = flashMax * 0.7f;
                _flashColor = zone >= 2f ? new Color(1f, 0.1f, 0.05f) : new Color(1f, 0.55f, 0.05f);
            }
            _lastZoneMultiplier = zone;

            _flashAlpha    = Mathf.MoveTowards(_flashAlpha,    0f, Time.deltaTime / flashDur);
            _buzzChromatic = Mathf.MoveTowards(_buzzChromatic, 0f, Time.deltaTime * buzzDecay);
            _buzzVignette  = Mathf.MoveTowards(_buzzVignette,  0f, Time.deltaTime * buzzDecay);
            if (_maxReached)
                _maxVeilTime += Time.deltaTime * veilRate * Mathf.PI * 2f;

            float pulseBlend = Mathf.Clamp01(Mathf.InverseLerp(0.5f, 1f, t));
            float pulseSpeed = Mathf.Lerp(pulseHigh, pulseCrit, pulseBlend);
            _pulseTime += Time.deltaTime * pulseSpeed;
            float pulse = Mathf.Sin(_pulseTime) * pulseAmp * pulseBlend;

            float baseVig = Mathf.Lerp(vigIdle, vigCritical, Mathf.Pow(t, 1.5f));
            _vignette.intensity.Override(Mathf.Lerp(
                _vignette.intensity.value, baseVig + pulse + _buzzVignette,
                Time.deltaTime * lerpSpd));

            var vigColor = Color.Lerp(Color.black, new Color(0.45f, 0f, 0f),
                                      Mathf.InverseLerp(0.5f, 1f, t));
            _vignette.color.Override(vigColor);

            float targetChromatic = t < 0.25f ? 0f
                : t < 0.5f  ? Mathf.InverseLerp(0.25f, 0.5f,  t) * chromMedium
                : t < 0.75f ? Mathf.Lerp(chromMedium,  chromHigh,      Mathf.InverseLerp(0.5f,  0.75f, t))
                             : Mathf.Lerp(chromHigh,    chromCritical,  Mathf.InverseLerp(0.75f, 1f,    t));
            _chromatic.intensity.Override(Mathf.Lerp(
                _chromatic.intensity.value, targetChromatic + _buzzChromatic,
                Time.deltaTime * lerpSpd));

            float targetDistortion = t < 0.5f ? 0f
                : t < 0.75f ? Mathf.Lerp(0f,       distHigh,     Mathf.InverseLerp(0.5f,  0.75f, t))
                             : Mathf.Lerp(distHigh, distCritical, Mathf.InverseLerp(0.75f, 1f,    t));
            _distortion.intensity.Override(Mathf.Lerp(
                _distortion.intensity.value, targetDistortion,
                Time.deltaTime * lerpSpd));
        }

        private void OnGUI()
        {
            var rect = new Rect(0, 0, Screen.width, Screen.height);

            // Persistent red veil when restlessness is maxed
            if (_maxReached)
            {
                float veilAlpha = _maxVeilBaseAlpha + _maxVeilPulseDepth * (0.5f + 0.5f * Mathf.Sin(_maxVeilTime));
                GUI.color = new Color(0.85f, 0f, 0.05f, veilAlpha);
                GUI.DrawTexture(rect, _white);
                GUI.color = Color.white;
            }

            // Threshold / zone flash on top
            if (_flashAlpha > 0f)
            {
                GUI.color = new Color(_flashColor.r, _flashColor.g, _flashColor.b, _flashAlpha);
                GUI.DrawTexture(rect, _white);
                GUI.color = Color.white;
            }
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
