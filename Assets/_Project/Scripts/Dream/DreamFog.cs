using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Restless.Dream
{
    /// <summary>
    /// Wraps any dream entity in a translucent fog that hides it until the player
    /// looks at it long enough (fogRevealDwellTime). On reveal the fog cross-fades
    /// out and the underlying content becomes active.
    ///
    /// Supported content (auto-detected on the same GameObject):
    ///   DreamEntity  — threat or passive, activates normally after reveal
    ///   MemoryPoint  — becomes interactable after reveal (minigame still required)
    ///   WanderingNPC — starts wandering after reveal
    /// </summary>
    public class DreamFog : MonoBehaviour
    {
        public enum FogType { Threat, Wanderer, Fragment, AllyEcho }

        [SerializeField] private FogType _type = FogType.Threat;

        private SpriteRenderer   _fogSR;
        private VisionCone       _visionCone;
        private float            _dwellTimer;
        private bool             _revealing;
        private bool             _revealed;

        private SpriteRenderer[] _contentSRs;
        private Color[]          _contentColors;
        private MonoBehaviour[]  _contentBehaviors;

        static readonly Color FogBaseColor = new Color(0.55f, 0.68f, 0.88f, 0.60f);

        public FogType Type       => _type;
        public bool    IsRevealed => _revealed;

        public void SetType(FogType type) => _type = type;

        // ── Init ─────────────────────────────────────────────────────────────

        private void Awake()
        {
            // Gather content SRs before adding the fog visual child
            _contentSRs    = GetComponentsInChildren<SpriteRenderer>(true);
            _contentColors = new Color[_contentSRs.Length];
            for (int i = 0; i < _contentSRs.Length; i++)
            {
                _contentColors[i]    = _contentSRs[i].color;
                var c                = _contentSRs[i].color;
                c.a                  = 0f;
                _contentSRs[i].color = c;
            }

            // Disable content behaviors until reveal
            var behaviors = new List<MonoBehaviour>();
            CollectIfPresent<DreamEntity>(behaviors);
            CollectIfPresent<MemoryPoint>(behaviors);
            CollectIfPresent<WanderingNPC>(behaviors);
            foreach (var b in behaviors) b.enabled = false;
            _contentBehaviors = behaviors.ToArray();

            BuildFogVisual();
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

        private void BuildFogVisual()
        {
            var go = new GameObject("FogVisual");
            go.transform.SetParent(transform);
            go.transform.localPosition = Vector3.zero;
            go.transform.localScale    = new Vector3(1.6f, 1.6f, 1f);

            _fogSR = go.AddComponent<SpriteRenderer>();

            // 1×1 white sprite — shader draws the actual shape
            var tex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            tex.SetPixel(0, 0, Color.white);
            tex.Apply();
            _fogSR.sprite = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
            _fogSR.color  = FogBaseColor;

            var shader = Shader.Find("Restless/DreamFog");
            if (shader != null)
                _fogSR.material = new Material(shader) { color = FogBaseColor };
            else
                Debug.LogWarning("[DreamFog] Shader 'Restless/DreamFog' not found — using default sprite material.");

            if (_contentSRs.Length > 0)
            {
                _fogSR.sortingLayerID = _contentSRs[0].sortingLayerID;
                _fogSR.sortingOrder   = _contentSRs[0].sortingOrder;
            }
        }

        // ── Update ────────────────────────────────────────────────────────────

        private void Update()
        {
            if (_revealed || _revealing) return;
            TrackDwell();
        }

        private void TrackDwell()
        {
            bool  inCone    = _visionCone != null && _visionCone.ContainsPoint(transform.position);
            float threshold = 1.5f;

            if (inCone)
            {
                _dwellTimer += Time.deltaTime;
                if (_dwellTimer >= threshold)
                    StartCoroutine(Reveal());
            }
            else
            {
                // Drain slowly — keeps tension without a hard reset
                _dwellTimer = Mathf.Max(0f, _dwellTimer - Time.deltaTime * 0.5f);
            }
        }

        // ── Reveal ────────────────────────────────────────────────────────────

        private IEnumerator Reveal()
        {
            _revealing = true;
            float duration = 0.6f;
            float t = 0f;

            while (t < duration)
            {
                t += Time.deltaTime;
                float p = Mathf.Clamp01(t / duration);

                if (_fogSR != null)
                {
                    var c = FogBaseColor;
                    c.a = Mathf.Lerp(FogBaseColor.a, 0f, p);
                    _fogSR.color = c;
                }

                for (int i = 0; i < _contentSRs.Length; i++)
                {
                    var c = _contentColors[i];
                    c.a = Mathf.Lerp(0f, _contentColors[i].a, p);
                    _contentSRs[i].color = c;
                }

                yield return null;
            }

            for (int i = 0; i < _contentSRs.Length; i++)
                _contentSRs[i].color = _contentColors[i];

            foreach (var b in _contentBehaviors)
                b.enabled = true;

            if (_fogSR != null)
                Destroy(_fogSR.gameObject);

            _revealed  = true;
            _revealing = false;
        }
    }
}
