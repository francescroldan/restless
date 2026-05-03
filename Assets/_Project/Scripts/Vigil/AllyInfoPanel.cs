using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;
using DG.Tweening;

namespace Restless.Vigil
{
    public class AllyInfoPanel : MonoBehaviour
    {
        [SerializeField] private RectTransform _panel;
        [SerializeField] private Image         _icon;
        [SerializeField] private TMP_Text      _nameText;
        [SerializeField] private TMP_Text      _passiveText;
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

            if (_icon != null)
            {
                Sprite icon = null;
                if (slot.Data.portraitSprite != null)      icon = slot.Data.portraitSprite;
                else if (slot.Data.roomSprite != null)     icon = slot.Data.roomSprite;
                else if (slot.Data.iconSprite != null)     icon = slot.Data.iconSprite;
                _icon.enabled = icon != null;
                if (icon != null) _icon.sprite = icon;
            }

            if (_nameText != null)
                _nameText.text = slot.Data.displayName;

            if (_passiveText != null)
                _passiveText.text = slot.Data.passiveDescription;

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
            if (Mouse.current == null || !Mouse.current.leftButton.wasPressedThisFrame) return;

            if (!RectTransformUtility.RectangleContainsScreenPoint(_panel, Mouse.current.position.ReadValue()))
                Hide();
        }

        private void OnDestroy() => _tween?.Kill();
    }
}
