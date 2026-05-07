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

        // Urn pixel dimensions
        const int W = 16, H = 32;

        // Profile left/right X per row (y=0 at bottom)
        static readonly int[] PL = {3,3,4,4,5,5,4,3,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,3,4,5,6,6,6,6,5,5};
        static readonly int[] PR = {12,12,11,11,10,10,11,12,13,13,13,13,13,13,13,13,13,13,13,13,13,13,13,12,11,10,9,9,9,9,10,10};

        // Interior fill spans rows 4-22 (body + lower)
        const int FillMinRow = 4;
        const int FillMaxRow = 22;

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

        private void Start()  => Refresh(force: true);
        private void OnEnable() => Refresh(force: true);

        private void Update()
        {
            if (SaveManager.Instance == null) return;
            Refresh(force: false);
        }

        private void Refresh(bool force)
        {
            int count = SaveManager.Instance?.CollectedFragmentCount ?? 0;
            if (!force && count == _lastCount) return;
            _lastCount = count;

            if (GameManager.Instance == null) return;
            float fill     = Mathf.Clamp01((float)count / GameManager.Instance.DemoFragmentTarget);
            bool  complete = count >= GameManager.Instance.DemoFragmentTarget;
            DrawUrn(fill, complete);

            if (_light != null)
            {
                _light.gameObject.SetActive(complete);
                if (complete) _light.color = new Color(1f, 0.82f, 0.28f);
            }
        }

        private void DrawUrn(float fill, bool complete)
        {
            Color clear   = new Color(0, 0, 0, 0);
            Color outline = complete
                ? new Color(0.86f, 0.70f, 0.24f)   // gold outline when full
                : new Color(0.22f, 0.19f, 0.16f);   // dark brown
            Color interior = new Color(0.08f, 0.06f, 0.05f, 0.88f);

            // Fill range in pixels
            int fillTopRow = FillMinRow + Mathf.RoundToInt(fill * (FillMaxRow - FillMinRow));

            for (int y = 0; y < H; y++)
                for (int x = 0; x < W; x++)
                    _tex.SetPixel(x, y, clear);

            for (int y = 0; y < H; y++)
            {
                int l = PL[y], r = PR[y];
                bool solidRow = y <= 3 || y >= 26;   // base and neck/rim = fully solid

                for (int x = l; x <= r; x++)
                {
                    bool isEdge = (x == l || x == r);

                    if (solidRow || isEdge)
                    {
                        _tex.SetPixel(x, y, outline);
                    }
                    else
                    {
                        // Interior: filled or empty
                        if (y >= FillMinRow && y <= fillTopRow)
                        {
                            float rowNorm = (float)(y - FillMinRow) / (FillMaxRow - FillMinRow);
                            Color fillCol = Color.Lerp(
                                new Color(0.55f, 0.28f, 0.06f),   // dark amber
                                new Color(0.92f, 0.75f, 0.24f),   // bright gold
                                rowNorm);

                            // Center highlight stripe
                            int cx = (l + r) / 2;
                            if (x == cx)
                                fillCol = Color.Lerp(fillCol, new Color(0.98f, 0.94f, 0.62f), 0.5f);

                            _tex.SetPixel(x, y, fillCol);
                        }
                        else
                        {
                            _tex.SetPixel(x, y, interior);
                        }
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
