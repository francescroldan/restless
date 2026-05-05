using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.EventSystems;
using DG.Tweening;
using TMPro;

namespace Restless.Vigil
{
    [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(Collider2D))]
    public class AllySlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        [SerializeField] private AllyData _data;
        [SerializeField] private Light2D  _allyLight;
        [SerializeField] private TMP_Text _nameLabel;

        private SpriteRenderer _sr;
        private bool  _isPresent;
        private Vector3 _baseScale;
        private float   _baseLightIntensity;
        private Tween   _scaleTween;
        private Tween   _lightTween;
        private Tween   _colorTween;
        private Tween   _labelTween;

        private static readonly Color HoverTint = new Color(1.4f, 1.3f, 1.1f);

        public AllyData Data      => _data;
        public bool     IsPresent => _isPresent;

        private void Awake()
        {
            _sr        = GetComponent<SpriteRenderer>();
            _baseScale = transform.localScale;
        }

        public void SetPresence(bool present)
        {
            _isPresent = present;
            gameObject.SetActive(present);
            _sr.enabled = present;

            if (_data?.roomSprite != null)
                _sr.sprite = _data.roomSprite;

            if (_allyLight != null)
            {
                _allyLight.enabled = present;
                if (present && _data != null)
                {
                    _allyLight.color              = _data.lightColor;
                    _allyLight.intensity          = _data.lightIntensity;
                    _allyLight.pointLightOuterRadius = _data.lightRadius;
                    _baseLightIntensity           = _data.lightIntensity;
                }
            }

            if (_nameLabel != null)
            {
                _nameLabel.text  = _data != null ? _data.displayName : "";
                var c = _nameLabel.color; c.a = 0f; _nameLabel.color = c;
                _nameLabel.gameObject.SetActive(present);
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!_isPresent) return;
            VigiliaAudioPlayer.Instance?.PlayAllyHover();

            _scaleTween?.Kill();
            _scaleTween = transform.DOScale(_baseScale * 1.1f, 0.18f).SetEase(Ease.OutBack);

            if (_allyLight != null)
            {
                _lightTween?.Kill();
                _lightTween = DOTween.To(
                    () => _allyLight.intensity,
                    v  => _allyLight.intensity = v,
                    _baseLightIntensity * 2.5f, 0.2f);
            }

            _colorTween?.Kill();
            _colorTween = _sr.DOColor(HoverTint, 0.2f);

            ShowLabel();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (!_isPresent) return;

            _scaleTween?.Kill();
            _scaleTween = transform.DOScale(_baseScale, 0.2f).SetEase(Ease.OutSine);

            if (_allyLight != null)
            {
                _lightTween?.Kill();
                _lightTween = DOTween.To(
                    () => _allyLight.intensity,
                    v  => _allyLight.intensity = v,
                    _baseLightIntensity, 0.3f);
            }

            _colorTween?.Kill();
            _colorTween = _sr.DOColor(Color.white, 0.2f);

            HideLabel();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!_isPresent || _data == null) return;
            VigiliaAudioPlayer.Instance?.PlayAllyClick();
            ClickPulse();
            VigiliaRoomController.Instance?.OnAllyClicked(this);
        }

        private void ShowLabel()
        {
            if (_nameLabel == null) return;
            PositionLabelInCanvas(_nameLabel, Vector3.up * 1.2f);
            _labelTween?.Kill();
            _labelTween = DOTween.To(
                () => _nameLabel.color,
                v  => _nameLabel.color = v,
                new Color(1f, 1f, 1f, 1f), 0.2f);
        }

        private void HideLabel()
        {
            if (_nameLabel == null) return;
            _labelTween?.Kill();
            _labelTween = DOTween.To(
                () => _nameLabel.color,
                v  => _nameLabel.color = v,
                new Color(1f, 1f, 1f, 0f), 0.15f);
        }

        private void PositionLabelInCanvas(TMPro.TMP_Text label, Vector3 worldOffset)
        {
            var cam = Camera.main;
            if (cam == null) return;
            var canvas = label.GetComponentInParent<Canvas>();
            if (canvas == null || canvas.renderMode == UnityEngine.RenderMode.WorldSpace) return;
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

        private void ClickPulse()
        {
            if (_allyLight == null) return;
            _lightTween?.Kill();
            _lightTween = DOTween.Sequence()
                .Append(DOTween.To(() => _allyLight.intensity, v => _allyLight.intensity = v, _baseLightIntensity * 4f, 0.08f))
                .Append(DOTween.To(() => _allyLight.intensity, v => _allyLight.intensity = v, _baseLightIntensity, 0.4f).SetEase(Ease.OutSine));
        }

        private void OnDestroy()
        {
            _scaleTween?.Kill();
            _lightTween?.Kill();
            _colorTween?.Kill();
            _labelTween?.Kill();
        }
    }
}
