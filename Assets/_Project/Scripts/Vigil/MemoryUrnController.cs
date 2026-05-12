using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using Restless.Core;

namespace Restless.Vigil
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class MemoryUrnController : MonoBehaviour
    {
        private SpriteRenderer _sr;
        private Light2D        _light;
        private Texture2D      _tex;
        private Sprite         _sprite;
        private int            _lastCount = -1;
        private bool           _filling;

        // Urn pixel dimensions — matches bottle sprite (32×30)
        const int W = 32, H = 30;

        // Profile left/right X per row (y=0 at bottom). -1 = empty row.
        // PL mirrored from PR so the bottle is symmetric (center ≈ x=15.5).
        static readonly int[] PL = {
            11,10, 9, 8, 7, 6, 5, 5,   // y=0-7:  base + lower body
             5, 5, 6, 6, 6, 7, 7, 8,   // y=8-15: upper body
             9,10,11,12,13,13,13,       // y=16-22: shoulder + neck
            12,11,12,13,                // y=23-26: cork
            -1,-1,-1                    // y=27-29: empty
        };
        static readonly int[] PR = {
            20,21,22,23,24,25,26,26,   // y=0-7
            26,26,25,25,25,24,24,23,   // y=8-15
            22,21,20,19,18,18,18,       // y=16-22
            19,20,19,18,                // y=23-26
            -1,-1,-1                    // y=27-29
        };

        const int FillMinRow = 1;
        const int FillMaxRow = 18;

        private void Awake()
        {
            _sr    = GetComponent<SpriteRenderer>();
            _light = GetComponentInChildren<Light2D>(includeInactive: true);

            _tex = new Texture2D(W, H, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
                wrapMode   = TextureWrapMode.Clamp
            };
        }

        // ── Lifecycle ────────────────────────────────────────────────────

        private void Start()
        {
            int gained     = SaveManager.Instance?.RecentFragmentsGained ?? 0;
            int totalCount = SaveManager.Instance?.CollectedFragmentCount ?? 0;

            if (gained > 0 && GameManager.Instance != null)
            {
                int   prevCount = totalCount - gained;
                float prevFill  = Mathf.Clamp01((float)prevCount / GameManager.Instance.DemoFragmentTarget);
                float newFill   = Mathf.Clamp01((float)totalCount / GameManager.Instance.DemoFragmentTarget);

                DrawUrn(prevFill, false);
                _lastCount = totalCount;

                SaveManager.Instance.ConsumeRecentGain();
                float duration = Mathf.Clamp(gained * 0.5f, 0.4f, 2.5f);
                VigiliaAudioPlayer.Instance?.PlayUrnFill(gained);
                StartCoroutine(AnimateFill(prevFill, newFill, duration));
            }
            else
            {
                Refresh(force: true);
            }
        }

        private void OnEnable() => Refresh(force: true);

        private void Update()
        {
            if (_filling || SaveManager.Instance == null) return;
            Refresh(force: false);
        }

        private void Refresh(bool force)
        {
            int count = SaveManager.Instance?.CollectedFragmentCount ?? 0;
            if (!force && count == _lastCount) return;
            _lastCount = count;

            if (GameManager.Instance == null) return;
            float fill    = Mathf.Clamp01((float)count / GameManager.Instance.DemoFragmentTarget);
            bool complete = count >= GameManager.Instance.DemoFragmentTarget;
            DrawUrn(fill, complete);

            if (_light != null)
            {
                _light.gameObject.SetActive(complete);
                if (complete) _light.color = new Color(0.78f, 0.22f, 0.92f);
            }
        }

        // ── Animations ───────────────────────────────────────────────────

        private IEnumerator AnimateFill(float fromFill, float toFill, float duration)
        {
            _filling = true;
            bool complete = _lastCount >= GameManager.Instance.DemoFragmentTarget;

            float t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                float fill = Mathf.Lerp(fromFill, toFill, Mathf.SmoothStep(0f, 1f, t / duration));
                DrawUrn(fill, complete && fill >= toFill - 0.01f);
                yield return null;
            }

            DrawUrn(toFill, complete);
            _filling = false;

            yield return StartCoroutine(PulseUrn());
        }

        private IEnumerator PulseUrn()
        {
            Vector3 baseScale = transform.localScale;
            Vector3 peakScale = baseScale * 1.18f;
            float   t         = 0f;

            while (t < 1f)
            {
                t += Time.deltaTime * 4f;
                transform.localScale = Vector3.Lerp(baseScale, peakScale, Mathf.Sin(t * Mathf.PI));
                _sr.color = Color.Lerp(Color.white, new Color(0.88f, 0.35f, 0.98f), Mathf.Sin(t * Mathf.PI));
                yield return null;
            }

            transform.localScale = baseScale;
            _sr.color = Color.white;
        }

        // ── Draw ─────────────────────────────────────────────────────────

        private void DrawUrn(float fill, bool complete)
        {
            Color clear    = new Color(0, 0, 0, 0);
            Color outline  = complete
                ? new Color(0.82f, 0.45f, 0.95f)       // fuchsia when full
                : new Color(0.22f, 0.19f, 0.16f);       // dark brown
            Color interior = new Color(0.10f, 0.06f, 0.14f, 0.45f); // dark purple, semi-transparent

            int fillTopRow = FillMinRow + Mathf.RoundToInt(fill * (FillMaxRow - FillMinRow));

            for (int y = 0; y < H; y++)
                for (int x = 0; x < W; x++)
                    _tex.SetPixel(x, y, clear);

            for (int y = 0; y < H; y++)
            {
                int l = PL[y], r = PR[y];
                if (l < 0) continue;

                bool solidRow = y <= 1 || (y >= 23 && y <= 26);

                for (int x = l; x <= r; x++)
                {
                    bool isEdge = (x == l || x == r);

                    if (solidRow || isEdge)
                    {
                        _tex.SetPixel(x, y, outline);
                    }
                    else if (y >= FillMinRow && y <= fillTopRow)
                    {
                        float rowNorm = (float)(y - FillMinRow) / (FillMaxRow - FillMinRow);
                        Color fillCol = Color.Lerp(
                            new Color(0.42f, 0.08f, 0.52f),   // deep violet base
                            new Color(0.82f, 0.28f, 0.92f),   // bright fuchsia top
                            rowNorm);

                        // Center highlight stripe
                        if (x == (l + r) / 2)
                            fillCol = Color.Lerp(fillCol, new Color(0.96f, 0.72f, 1.00f), 0.55f);

                        _tex.SetPixel(x, y, fillCol);
                    }
                    else
                    {
                        _tex.SetPixel(x, y, interior);
                    }
                }
            }

            _tex.Apply();

            if (_sprite != null) Destroy(_sprite);
            _sprite = Sprite.Create(_tex, new Rect(0, 0, W, H), new Vector2(0.5f, 0f), 16f);
            _sr.sprite = _sprite;
        }
    }
}
