using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace Restless.Vigil
{
    /// <summary>
    /// Minimal ally info panel — shows only the ally icon, no text.
    /// Slides in from the edge when an ally is clicked; closes on click-outside.
    /// Attach to a Canvas > Panel RectTransform.
    /// Requires: Image _icon (child Image), configured _hiddenOffset in Inspector.
    /// </summary>
    public class AllyInfoPanel : MonoBehaviour
    {
        [SerializeField] private RectTransform _panel;
        [SerializeField] private Image         _icon;
        [SerializeField] private Vector2       _hiddenOffset = new Vector2(320f, 0f);
        [SerializeField] private float         _slideDuration = 0.22f;

        private Vector2 _shownAnchoredPos;
        private bool    _visible;
        private Tween   _tween;

        private void Awake()
        {
            if (_panel == null) _panel = GetComponent<RectTransform>();
            _shownAnchoredPos = _panel.anchoredPosition;
            _panel.anchoredPosition = _shownAnchoredPos + _hiddenOffset;
        }

        public void Show(AllySlot slot)
        {
            if (slot?.Data == null) return;

            if (_icon != null && slot.Data.iconSprite != null)
                _icon.sprite = slot.Data.iconSprite;

            _tween?.Kill();
            _tween = _panel.DOAnchorPos(_shownAnchoredPos, _slideDuration)
                .SetEase(Ease.OutCubic);
            _visible = true;
        }

        public void Hide()
        {
            if (!_visible) return;
            _tween?.Kill();
            _tween = _panel.DOAnchorPos(_shownAnchoredPos + _hiddenOffset, _slideDuration * 0.8f)
                .SetEase(Ease.InCubic)
                .OnComplete(() => _visible = false);
        }

        private void Update()
        {
            if (!_visible) return;
            if (!Input.GetMouseButtonDown(0)) return;

            if (!RectTransformUtility.RectangleContainsScreenPoint(_panel, Input.mousePosition))
                Hide();
        }

        private void OnDestroy() => _tween?.Kill();
    }
}
