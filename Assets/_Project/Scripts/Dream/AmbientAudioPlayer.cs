using UnityEngine;

namespace Restless.Dream
{
    /// <summary>
    /// Crossfades between 4 ambient clips based on Restlessness threshold.
    /// Attach to _Managers alongside RestlessnessAudioController.
    /// Uses two AudioSources for smooth crossfade.
    /// </summary>
    public class AmbientAudioPlayer : MonoBehaviour
    {
        [SerializeField] private AudioClip _clipCalm;
        [SerializeField] private AudioClip _clipTense;
        [SerializeField] private AudioClip _clipCritical;
        [SerializeField] private AudioClip _clipOverload;

        [SerializeField] private float _crossfadeTime = 2.5f;
        [SerializeField] private float _maxVolume     = 0.55f;

        private AudioSource _srcA;
        private AudioSource _srcB;
        private bool _aIsActive = true;

        private RestlessnessManager.Threshold _currentThreshold = (RestlessnessManager.Threshold)(-1);
        private float _fadeTimer;
        private bool  _crossfading;

        private void Awake()
        {
            _srcA = gameObject.AddComponent<AudioSource>();
            _srcB = gameObject.AddComponent<AudioSource>();
            foreach (var src in new[] { _srcA, _srcB })
            {
                src.loop         = true;
                src.playOnAwake  = false;
                src.spatialBlend = 0f;
                src.volume       = 0f;
            }
        }

        private void Start()
        {
            if (RestlessnessManager.Instance == null) return;
            var threshold = RestlessnessManager.Instance.CurrentThreshold;
            _srcA.clip = ClipFor(threshold);
            _srcA.volume = _maxVolume;
            _srcA.Play();
            _currentThreshold = threshold;
        }

        private void Update()
        {
            if (RestlessnessManager.Instance == null) return;

            var threshold = RestlessnessManager.Instance.CurrentThreshold;
            if (threshold != _currentThreshold)
            {
                BeginCrossfade(ClipFor(threshold));
                _currentThreshold = threshold;
            }

            if (_crossfading) TickCrossfade();
        }

        private void BeginCrossfade(AudioClip next)
        {
            if (next == null) return;

            var incoming = _aIsActive ? _srcB : _srcA;
            var outgoing = _aIsActive ? _srcA : _srcB;

            incoming.clip   = next;
            incoming.volume = 0f;
            incoming.time   = 0f;
            incoming.Play();

            _fadeTimer   = 0f;
            _crossfading = true;
            _aIsActive   = !_aIsActive;
        }

        private void TickCrossfade()
        {
            _fadeTimer += Time.deltaTime;
            float t = Mathf.Clamp01(_fadeTimer / _crossfadeTime);

            var incoming = _aIsActive ? _srcA : _srcB;
            var outgoing = _aIsActive ? _srcB : _srcA;

            incoming.volume = Mathf.Lerp(0f, _maxVolume, t);
            outgoing.volume = Mathf.Lerp(_maxVolume, 0f, t);

            if (t >= 1f)
            {
                outgoing.Stop();
                _crossfading = false;
            }
        }

        private AudioClip ClipFor(RestlessnessManager.Threshold t) => t switch
        {
            RestlessnessManager.Threshold.Low      => _clipCalm,
            RestlessnessManager.Threshold.Medium   => _clipTense,
            RestlessnessManager.Threshold.High     => _clipCritical,
            RestlessnessManager.Threshold.Critical => _clipOverload,
            RestlessnessManager.Threshold.Max      => _clipOverload,
            _ => _clipCalm
        };
    }
}
