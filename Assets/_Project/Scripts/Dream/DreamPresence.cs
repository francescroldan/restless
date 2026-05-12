using System.Collections.Generic;
using UnityEngine;

namespace Restless.Dream
{
    /// <summary>
    /// Replaces DreamFog. Every ambiguous element in the dream (threats, wanderers,
    /// fragments, ally echoes) starts as a spectral presence — an unstable, undefined
    /// form. The player's sustained gaze materialises it progressively. Looking away
    /// causes it to decay back toward the spectral state.
    ///
    /// manifestation: 0 = fully spectral (no gameplay, abstract visual)
    ///                1 = fully materialised (behaviours active, original sprite)
    ///
    /// State flow:  Spectral → Manifesting → Stable
    ///                          ↑──decay──┘  (unless _degradesWhenUnseen = false)
    /// </summary>
    public class DreamPresence : MonoBehaviour
    {
        public enum PresenceType  { Threat, Wanderer, Fragment, AllyEcho, Undefined }
        public enum PresenceState { Spectral, Manifesting, Stable }

        [Header("Identity")]
        [SerializeField] private PresenceType _type = PresenceType.Undefined;

        [Header("Manifestation")]
        [SerializeField] private float _manifestSpeed      = 0.35f;
        [SerializeField] private float _manifestDecay      = 0.12f;
        [SerializeField] private bool  _degradesWhenUnseen = true;

        [Header("Spectral visual size")]
        [SerializeField] private float _spectralScaleMin = 0.8f;
        [SerializeField] private float _spectralScaleMax = 2.2f;

        // ── Runtime ──────────────────────────────────────────────────────────

        private float         _manifestation;
        private PresenceState _state = PresenceState.Spectral;

        private SpriteRenderer   _spectralSR;
        private Material         _spectralMat;
        private VisionCone       _visionCone;

        private SpriteRenderer[] _contentSRs;
        private Color[]          _contentColors;
        private MonoBehaviour[]  _contentBehaviors;

        // ── Public API ────────────────────────────────────────────────────────

        public PresenceType  Type          => _type;
        public PresenceState State         => _state;
        public float         Manifestation => _manifestation;
        public bool          IsStable      => _state == PresenceState.Stable;

        public void SetType(PresenceType t) => _type = t;

        public void AssignFragment(MemoryFragment f)
        {
            var mp = GetComponent<MemoryPoint>();
            if (mp != null) mp.AssignFragment(f);
        }

        // ── Init ─────────────────────────────────────────────────────────────

        private void Awake()
        {
            // Collect content SRs before adding the spectral visual child
            _contentSRs    = GetComponentsInChildren<SpriteRenderer>(true);
            _contentColors = new Color[_contentSRs.Length];
            for (int i = 0; i < _contentSRs.Length; i++)
            {
                _contentColors[i] = _contentSRs[i].color;
                var c = _contentSRs[i].color;
                c.a = 0f;
                _contentSRs[i].color = c;
            }

            // Disable behaviours — they run Start() for the first time on re-enable at collapse
            var behaviors = new List<MonoBehaviour>();
            CollectIfPresent<DreamEntity>(behaviors);
            CollectIfPresent<MemoryPoint>(behaviors);
            CollectIfPresent<WanderingNPC>(behaviors);
            foreach (var b in behaviors) b.enabled = false;
            _contentBehaviors = behaviors.ToArray();

            BuildSpectralVisual();
        }

        private void CollectIfPresent<T>(List<MonoBehaviour> list) where T : MonoBehaviour
        {
            var c = GetComponent<T>();
            if (c != null) list.Add(c);
        }

        private void Start()
        {
            var player = GameObject.FindWithTag("Player");
            if (player != null)
                _visionCone = player.GetComponentInChildren<VisionCone>();
        }

        private void BuildSpectralVisual()
        {
            // Grab the first available sprite from content to use as the form hint
            Sprite formSprite = _contentSRs.Length > 0 ? _contentSRs[0].sprite : null;

            var go = new GameObject("SpectralVisual");
            go.transform.SetParent(transform);
            go.transform.localPosition = Vector3.zero;
            float s = Random.Range(_spectralScaleMin, _spectralScaleMax);
            go.transform.localScale = new Vector3(s, s, 1f);

            _spectralSR = go.AddComponent<SpriteRenderer>();

            var shader = Shader.Find("Restless/SpectralPresence");
            if (shader != null)
            {
                _spectralMat = new Material(shader);
                _spectralMat.SetFloat("_Manifestation", 0f);
                _spectralSR.material = _spectralMat;
            }
            else
            {
                Debug.LogWarning("[DreamPresence] Shader 'Restless/SpectralPresence' not found.");
            }

            if (formSprite != null)
            {
                _spectralSR.sprite = formSprite;
            }
            else
            {
                var tex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
                tex.SetPixel(0, 0, Color.white);
                tex.Apply();
                _spectralSR.sprite = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
            }

            if (_contentSRs.Length > 0)
            {
                _spectralSR.sortingLayerID = _contentSRs[0].sortingLayerID;
                _spectralSR.sortingOrder   = _contentSRs[0].sortingOrder;
            }
        }

        // ── Update ────────────────────────────────────────────────────────────

        private void Update()
        {
            if (_state == PresenceState.Stable) return;

            bool  observed = _visionCone != null && _visionCone.ContainsPoint(transform.position);
            float speed    = Core.RunConfig.Current?.presenceManifestSpeed ?? _manifestSpeed;
            float decay    = Core.RunConfig.Current?.presenceManifestDecay ?? _manifestDecay;

            if (observed)
            {
                _manifestation = Mathf.Clamp01(_manifestation + speed * Time.deltaTime);
                _state = PresenceState.Manifesting;
            }
            else if (_degradesWhenUnseen)
            {
                _manifestation = Mathf.Clamp01(_manifestation - decay * Time.deltaTime);
                if (_manifestation <= 0f)
                    _state = PresenceState.Spectral;
            }

            if (_spectralMat != null)
                _spectralMat.SetFloat("_Manifestation", _manifestation);

            if (_manifestation >= 1f)
                Materialise();
        }

        // ── Materialise ───────────────────────────────────────────────────────

        private void Materialise()
        {
            _state = PresenceState.Stable;

            ResolveUndefinedType();

            for (int i = 0; i < _contentSRs.Length; i++)
                _contentSRs[i].color = _contentColors[i];

            foreach (var b in _contentBehaviors)
                if (b != null) b.enabled = true;

            ConfigureByType();

            if (_spectralSR != null)
                Destroy(_spectralSR.gameObject);
        }

        private void ResolveUndefinedType()
        {
            if (_type != PresenceType.Undefined) return;

            // Higher restlessness → higher threat probability
            float restlessness = RestlessnessManager.Instance?.NormalizedValue ?? 0f;
            float threatChance = Mathf.Lerp(0.15f, 0.70f, restlessness);
            _type = Random.value < threatChance ? PresenceType.Threat : PresenceType.Wanderer;
        }

        private void ConfigureByType()
        {
            var entity = GetComponent<DreamEntity>();
            if (entity == null) return;

            switch (_type)
            {
                case PresenceType.Threat:
                    entity.SetHaunted(true);
                    break;
                case PresenceType.Wanderer:
                    entity.SetHaunted(false);
                    break;
            }
        }
    }
}
