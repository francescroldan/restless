using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using TMPro;

namespace Restless.Vigil
{
    [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(Collider2D))]
    public class ProtagonistBed : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        [SerializeField] private SpriteRenderer _sleepIcon;
        [SerializeField] private TMP_Text       _actionLabel;
        [SerializeField] private float _breathingAmplitude = 0.025f;
        [SerializeField] private float _breathingDuration  = 2.8f;

        private SpriteRenderer _sr;
        private bool    _interactable = true;
        private Vector3 _baseScale;
        private Tween   _breathTween;
        private Tween   _colorTween;
        private Tween   _labelTween;

        private void Start()
        {
            _sr        = GetComponent<SpriteRenderer>();
            _baseScale = transform.localScale;

            if (_sleepIcon != null)
            {
                _sleepIcon.enabled = false;
                _sleepIcon.color   = new Color(1f, 1f, 1f, 0f);
            }

            if (_actionLabel != null)
            {
                var c = _actionLabel.color; c.a = 0f; _actionLabel.color = c;
            }

            StartBreathing();
        }

        private void StartBreathing()
        {
            _breathTween?.Kill();
            _breathTween = transform
                .DOScaleY(_baseScale.y * (1f + _breathingAmplitude), _breathingDuration)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutSine);
        }

        public void SetInteractable(bool interactable)
        {
            _interactable = interactable;
            if (!interactable) HideHoverFX();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!_interactable) return;
            ShowHoverFX();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            HideHoverFX();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!_interactable) return;
            VigiliaRoomController.Instance?.RequestEnterDream();
        }

        private void ShowHoverFX()
        {
            if (_sleepIcon != null)
            {
                _sleepIcon.enabled = true;
                _sleepIcon.DOFade(1f, 0.2f);
            }

            _colorTween?.Kill();
            _colorTween = _sr.DOColor(new Color(1.3f, 1.3f, 1.2f), 0.2f);

            if (_actionLabel != null)
            {
                PositionLabelInCanvas(_actionLabel, Vector3.down * 1.4f);
                _labelTween?.Kill();
                _labelTween = DOTween.To(
                    () => _actionLabel.color,
                    v  => _actionLabel.color = v,
                    new Color(0.85f, 0.9f, 1f, 1f), 0.2f);
            }
        }

        private void PositionLabelInCanvas(TMP_Text label, Vector3 worldOffset)
        {
            var cam = Camera.main;
            if (cam == null) return;
            var canvas = label.GetComponentInParent<Canvas>();
            if (canvas == null || canvas.renderMode == RenderMode.WorldSpace) return;
            var screenPos = cam.WorldToScreenPoint(transform.position + worldOffset);
            var canvasRT  = canvas.GetComponent<RectTransform>();
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    canvasRT, screenPos, null, out var localPos))
            {
                var rt = label.transform.parent?.GetComponent<RectTransform>()
                      ?? label.GetComponent<RectTransform>();
                if (rt != null) rt.anchoredPosition = localPos;
            }
        }

        private void HideHoverFX()
        {
            if (_sleepIcon != null)
                _sleepIcon.DOFade(0f, 0.15f).OnComplete(() => _sleepIcon.enabled = false);

            _colorTween?.Kill();
            _colorTween = _sr.DOColor(Color.white, 0.2f);

            if (_actionLabel != null)
            {
                _labelTween?.Kill();
                _labelTween = DOTween.To(
                    () => _actionLabel.color,
                    v  => _actionLabel.color = v,
                    new Color(0.85f, 0.9f, 1f, 0f), 0.15f);
            }
        }

        private void OnDestroy()
        {
            _breathTween?.Kill();
            _colorTween?.Kill();
            _labelTween?.Kill();
        }
    }
}
