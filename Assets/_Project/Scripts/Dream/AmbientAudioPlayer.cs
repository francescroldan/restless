using UnityEngine;

namespace Restless.Dream
{
    /// <summary>
    /// Plays a single ambient loop and morphs it via low-pass filter, reverb,
    /// chorus and a tremolo LFO as Restlessness increases.
    /// Attach to _Managers in the Dream scene.
    /// </summary>
    public class AmbientAudioPlayer : MonoBehaviour
    {
        [SerializeField] private AudioClip _ambientClip;

        [Header("Volume")]
        [SerializeField] private float _volumeCalm     = 0.40f;
        [SerializeField] private float _volumeCritical = 0.85f;

        [Header("Low-Pass Filter (cutoff Hz)")]
        [SerializeField] private float _lpfCalm     = 22000f;
        [SerializeField] private float _lpfCritical =   300f;
        [SerializeField] private float _resonanceCalm     = 1f;
        [SerializeField] private float _resonanceCritical = 8f;

        [Header("Reverb (reverbLevel mB: -10000=off, 0=max)")]
        [SerializeField] private float _reverbLevelCalm     = -10000f;
        [SerializeField] private float _reverbLevelCritical =  -1000f;
        [SerializeField] private float _decayTimeCalm       =     0.5f;
        [SerializeField] private float _decayTimeCritical   =     6.0f;

        [Header("Chorus")]
        [SerializeField] private float _chorusDepthCalm     = 0.00f;
        [SerializeField] private float _chorusDepthCritical = 0.60f;
        [SerializeField] private float _chorusRateCalm      = 0.10f;
        [SerializeField] private float _chorusRateCritical  = 0.60f;

        [Header("Pitch")]
        [SerializeField] private float _pitchCalm     = 1.00f;
        [SerializeField] private float _pitchCritical = 0.87f;

        [Header("Tremolo LFO")]
        [SerializeField] private float _tremoloDepthCalm     = 0.00f;
        [SerializeField] private float _tremoloDepthCritical = 0.30f;
        [SerializeField] private float _tremoloRateCalm      = 0.15f;
        [SerializeField] private float _tremoloRateCritical  = 1.50f;

        [Header("Transition")]
        [SerializeField] private float _lerpSpeed = 2.0f;

        private AudioSource        _src;
        private AudioLowPassFilter _lpf;
        private AudioReverbFilter  _reverb;
        private AudioChorusFilter  _chorus;
        private float              _tremoloPhase;
        private float              _baseVolume;

        private void Awake()
        {
            // Isolated child so filters don't bleed into SFX sources on _Managers
            var child = new GameObject("AmbientAudioSource");
            child.transform.SetParent(transform, false);

            _src              = child.AddComponent<AudioSource>();
            _src.loop         = true;
            _src.playOnAwake  = false;
            _src.spatialBlend = 0f;
            _src.volume       = _volumeCalm;
            _baseVolume       = _volumeCalm;

            _lpf                   = child.AddComponent<AudioLowPassFilter>();
            _lpf.cutoffFrequency   = _lpfCalm;
            _lpf.lowpassResonanceQ = _resonanceCalm;

            _reverb             = child.AddComponent<AudioReverbFilter>();
            _reverb.reverbLevel = _reverbLevelCalm;
            _reverb.decayTime   = _decayTimeCalm;
            _reverb.dryLevel    = 0f;
            _reverb.room        = -10000f;

            _chorus          = child.AddComponent<AudioChorusFilter>();
            _chorus.dryMix   = 1f;
            _chorus.wetMix1  = 0f;
            _chorus.wetMix2  = 0f;
            _chorus.wetMix3  = 0f;
            _chorus.depth    = _chorusDepthCalm;
            _chorus.rate     = _chorusRateCalm;
        }

        private void Start()
        {
            if (_ambientClip == null) return;
            _src.clip = _ambientClip;
            _src.Play();
        }

        private void Update()
        {
            if (RestlessnessManager.Instance == null) return;

            float t  = RestlessnessManager.Instance.NormalizedValue;
            float dt = Time.deltaTime * _lerpSpeed;

            // ── Low-pass filter ──────────────────────────────────────────────
            _lpf.cutoffFrequency   = Mathf.Lerp(_lpf.cutoffFrequency,   Mathf.Lerp(_lpfCalm,          _lpfCritical,          t), dt);
            _lpf.lowpassResonanceQ = Mathf.Lerp(_lpf.lowpassResonanceQ, Mathf.Lerp(_resonanceCalm,    _resonanceCritical,    t), dt);

            // ── Reverb ───────────────────────────────────────────────────────
            float targetReverb = Mathf.Lerp(_reverbLevelCalm, _reverbLevelCritical, t);
            float targetDecay  = Mathf.Lerp(_decayTimeCalm,   _decayTimeCritical,   t);
            _reverb.reverbLevel = Mathf.Lerp(_reverb.reverbLevel, targetReverb, dt);
            _reverb.decayTime   = Mathf.Lerp(_reverb.decayTime,   targetDecay,  dt);
            // Open up room level in tandem with reverb
            _reverb.room = Mathf.Lerp(_reverb.room, Mathf.Lerp(-10000f, -2000f, t), dt);

            // ── Chorus ───────────────────────────────────────────────────────
            float targetDepth = Mathf.Lerp(_chorusDepthCalm, _chorusDepthCritical, t);
            float targetRate  = Mathf.Lerp(_chorusRateCalm,  _chorusRateCritical,  t);
            _chorus.depth    = Mathf.Lerp(_chorus.depth, targetDepth, dt);
            _chorus.rate     = Mathf.Lerp(_chorus.rate,  targetRate,  dt);
            // Bring in wet signal progressively
            float wetTarget = Mathf.Lerp(0f, 0.5f, t);
            _chorus.wetMix1  = Mathf.Lerp(_chorus.wetMix1, wetTarget, dt);

            // ── Pitch ────────────────────────────────────────────────────────
            _src.pitch = Mathf.Lerp(_src.pitch, Mathf.Lerp(_pitchCalm, _pitchCritical, t), dt);

            // ── Tremolo LFO ──────────────────────────────────────────────────
            float depth = Mathf.Lerp(_tremoloDepthCalm, _tremoloDepthCritical, t);
            float rate  = Mathf.Lerp(_tremoloRateCalm,  _tremoloRateCritical,  t);
            _tremoloPhase += rate * Time.deltaTime * Mathf.PI * 2f;

            _baseVolume = Mathf.Lerp(_baseVolume, Mathf.Lerp(_volumeCalm, _volumeCritical, t), dt);
            float tremolo = 1f - depth * (0.5f + 0.5f * Mathf.Sin(_tremoloPhase));
            _src.volume = _baseVolume * tremolo;
        }
    }
}
