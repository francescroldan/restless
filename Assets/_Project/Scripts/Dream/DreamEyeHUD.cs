using UnityEngine;
using UnityEngine.UI;
using Restless.Core;

namespace Restless.Dream
{
    [RequireComponent(typeof(RawImage))]
    public class DreamEyeHUD : MonoBehaviour
    {
        private RawImage  _rawImage;
        private Texture2D _tex;

        const int W  = 48;
        const int H  = 32;
        const int CX = 23;   // center x (texture space)
        const int CY = 15;   // center y (texture space, y=0 at bottom)

        // Pre-computed eye half-height per column
        static readonly float[] EyeH = BuildEyeShape();

        static readonly Color Sclera  = new Color(0.11f, 0.09f, 0.09f, 1f);
        static readonly Color IrisCol = new Color(0.96f, 0.77f, 0.26f, 1f);  // #F5C542
        static readonly Color PupilCol= new Color(0.03f, 0.02f, 0.02f, 1f);
        static readonly Color LidCol  = new Color(0.13f, 0.10f, 0.10f, 1f);
        static readonly Color Outline = new Color(0.05f, 0.04f, 0.04f, 1f);
        static readonly Color Hilite  = new Color(0.98f, 0.96f, 0.85f, 1f);
        static readonly Color Clear   = new Color(0, 0, 0, 0);

        public static DreamEyeHUD Instance { get; private set; }

        // Animated state — lid
        private float _lid           = 0f;   // 0=closed, 1=fully open
        private float _blinkTimer    = 1f;   // countdown to next blink
        private float _blinkOpenTime = 0f;   // how long to stay open
        private bool  _eyeOpen       = false;
        private float _scaredFlash   = 0f;   // forces full open when > 0

        // Animated state — pupil
        private float _pupilX       = 0f;
        private float _pupilY       = 0f;
        private float _pupilTargetX = 0f;
        private float _pupilTargetY = 0f;
        private float _nextMoveTime = 0f;

        // ── Init ─────────────────────────────────────────────────────────────

        static float[] BuildEyeShape()
        {
            var h = new float[W];
            for (int x = 0; x < W; x++)
            {
                float t = (x - 1f) / (W - 3f);
                h[x] = (t >= 0f && t <= 1f) ? Mathf.Sin(t * Mathf.PI) * 9f : 0f;
            }
            return h;
        }

        private void Awake()
        {
            Instance  = this;
            _rawImage = GetComponent<RawImage>();
            _tex = new Texture2D(W, H, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
                wrapMode   = TextureWrapMode.Clamp
            };
            _rawImage.texture = _tex;

            for (int y = 0; y < H; y++)
                for (int x = 0; x < W; x++)
                    _tex.SetPixel(x, y, Clear);
            _tex.Apply();
        }

        // ── Public API ───────────────────────────────────────────────────────

        public void TriggerScared()
        {
            _scaredFlash = 0.45f;
            _lid         = 1f;
            _eyeOpen     = true;
            _blinkTimer  = 0f;
        }

        // ── Update loop ───────────────────────────────────────────────────────

        private void Update()
        {
            float restless = RestlessnessManager.Instance?.NormalizedValue ?? 0f;
            float timeNorm = DreamTimer.Instance?.NormalizedRemaining      ?? 1f;

            UpdateLid(timeNorm);
            UpdatePupil(restless, timeNorm);
            DrawEye();
        }

        private void UpdateLid(float timeNorm)
        {
            // Scared flash: hold fully open, then snap closed and resume normal blink
            if (_scaredFlash > 0f)
            {
                _scaredFlash -= Time.deltaTime;
                _lid = Mathf.Lerp(_lid, 1f, Time.deltaTime * 30f);
                if (_scaredFlash <= 0f)
                {
                    _eyeOpen    = false;
                    _blinkTimer = 0.05f; // close immediately after flash
                }
                return;
            }

            // urgency: 0 = lots of time, 1 = no time
            float urgency = 1f - timeNorm;
            // sqrt makes the curve grow faster at low values — eye reacts earlier
            float t = Mathf.Sqrt(urgency);

            // Interval between blinks: 3s → 0.3s
            float interval = Mathf.Lerp(3f, 0.3f, t);
            // How wide the eye opens per blink: 0.3 → 1.0
            float maxOpen  = Mathf.Lerp(0.3f, 1.0f, t);
            // How long the eye stays open per blink: 0.2s → 0.5s
            float openDur  = Mathf.Lerp(0.2f, 0.5f, t);

            _blinkTimer -= Time.deltaTime;

            if (!_eyeOpen && _blinkTimer <= 0f)
            {
                _eyeOpen       = true;
                _blinkOpenTime = openDur;
                _blinkTimer    = openDur;
            }
            else if (_eyeOpen && _blinkTimer <= 0f)
            {
                _eyeOpen    = false;
                _blinkTimer = interval * Random.Range(0.7f, 1.3f);
            }

            float target     = _eyeOpen ? maxOpen : 0f;
            float closeSpeed = Mathf.Lerp(8f,  20f, t);
            float openSpeed  = Mathf.Lerp(12f, 28f, t);
            _lid = Mathf.Lerp(_lid, target, Time.deltaTime * (_eyeOpen ? openSpeed : closeSpeed));
        }

        private void UpdatePupil(float restless, float timeNorm)
        {
            // Spasm driven by both restlessness and urgency; sqrt makes it readable earlier
            float raw   = Mathf.Clamp01(Mathf.Max(restless, 1f - timeNorm));
            float spasm = Mathf.Sqrt(raw);

            if (Time.time >= _nextMoveTime)
            {
                float amp      = Mathf.Lerp(0.25f, 1f,    spasm);
                float interval = Mathf.Lerp(1.5f,  0.08f, spasm);
                _pupilTargetX  = Random.Range(-amp, amp);
                _pupilTargetY  = Random.Range(-amp * 0.3f, amp * 0.3f);
                _nextMoveTime  = Time.time + interval;
            }

            float speed = Mathf.Lerp(5f, 28f, spasm);
            _pupilX = Mathf.Lerp(_pupilX, _pupilTargetX, Time.deltaTime * speed);
            _pupilY = Mathf.Lerp(_pupilY, _pupilTargetY, Time.deltaTime * speed);
        }

        // ── Draw ─────────────────────────────────────────────────────────────

        private void DrawEye()
        {
            int pcx = CX + Mathf.RoundToInt(_pupilX * 7f);
            int pcy = CY + Mathf.RoundToInt(_pupilY * 2f);

            // Clear
            for (int y = 0; y < H; y++)
                for (int x = 0; x < W; x++)
                    _tex.SetPixel(x, y, Clear);

            for (int x = 0; x < W; x++)
            {
                float h = EyeH[x];
                if (h <= 0f) continue;

                // In texture space (y up), the top eyelid covers pixels with y > lidBottom.
                // lidBottom = CY + h*(2*_lid - 1):
                //   _lid=0 (closed) → lidBottom = CY - h → all eye pixels covered
                //   _lid=1 (open)   → lidBottom = CY + h → no pixels covered
                float lidBottom = CY + h * (2f * _lid - 1f);

                int yMin = Mathf.Max(0,    Mathf.FloorToInt(CY - h));
                int yMax = Mathf.Min(H-1,  Mathf.CeilToInt(CY  + h));

                for (int y = yMin; y <= yMax; y++)
                {
                    float dy = y - CY;
                    if (Mathf.Abs(dy) > h) continue;

                    // Top eyelid
                    if (y > lidBottom)
                    {
                        _tex.SetPixel(x, y, LidCol);
                        continue;
                    }

                    // Outline (edge pixels of the almond)
                    if (Mathf.Abs(dy) >= h - 1.3f)
                    {
                        _tex.SetPixel(x, y, Outline);
                        continue;
                    }

                    // Iris / pupil / sclera
                    float dist = Mathf.Sqrt((x - pcx) * (x - pcx) + (y - pcy) * (y - pcy));
                    Color c = dist <= 3f   ? PupilCol
                            : dist <= 6.5f ? IrisCol
                                           : Sclera;
                    _tex.SetPixel(x, y, c);
                }
            }

            // Specular highlight — only if not covered by lid
            int hx = Mathf.Clamp(pcx + 2, 0, W - 1);
            int hy = Mathf.Clamp(pcy + 2, 0, H - 1);
            float hh = EyeH[hx];
            if (hh > 0f && hy <= CY + hh * (2f * _lid - 1f))
                _tex.SetPixel(hx, hy, Hilite);

            _tex.Apply();
        }
    }
}
