using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;
using DG.Tweening;
using Restless.Core;

namespace Restless.Vigil
{
    public class PreDreamSelectionPanel : MonoBehaviour
    {
        public static PreDreamSelectionPanel Instance { get; private set; }

        [SerializeField] private GameObject    _root;
        [SerializeField] private Button        _confirmButton;
        [SerializeField] private Button        _cancelButton;

        [Header("Slot A")]
        [SerializeField] private Button        _slotAButton;
        [SerializeField] private Image         _slotAPortrait;
        [SerializeField] private TMP_Text      _slotAName;
        [SerializeField] private TMP_Text      _slotAPassive;
        [SerializeField] private TMP_Text      _slotAWarning;

        [Header("Slot B")]
        [SerializeField] private Button        _slotBButton;
        [SerializeField] private Image         _slotBPortrait;
        [SerializeField] private TMP_Text      _slotBName;
        [SerializeField] private TMP_Text      _slotBPassive;
        [SerializeField] private TMP_Text      _slotBWarning;

        [SerializeField] private AllyRegistry  _registry;

        private List<AllyData> _unlockedAllies = new();
        private int _slotAIndex = -1;  // -1 = empty
        private int _slotBIndex = -1;

        private System.Action _onConfirm;
        private TMP_Text _confirmLabel;
        private TMP_Text _cancelLabel;

        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;

            _slotAButton.onClick.AddListener(CycleSlotA);
            _slotBButton.onClick.AddListener(CycleSlotB);
            _confirmButton.onClick.AddListener(OnConfirm);
            if (_cancelButton != null) _cancelButton.onClick.AddListener(Hide);

            _confirmLabel = _confirmButton?.GetComponentInChildren<TMP_Text>();
            _cancelLabel  = _cancelButton?.GetComponentInChildren<TMP_Text>();
            if (_confirmLabel != null) _confirmLabel.text = "Dormir  (RB)";
            if (_cancelLabel  != null) _cancelLabel.text  = "Cancelar  (LB)";

            var btnImg = _confirmButton?.GetComponent<Image>();
            if (btnImg != null) _confirmButtonOriginalColor = btnImg.color;

            _root.SetActive(false);
        }

        private void Update()
        {
            if (!_root.activeSelf) return;
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
                Hide();

            var gp = Gamepad.current;
            if (gp == null) return;
            if (gp.leftShoulder.wasPressedThisFrame)  Hide();
            if (gp.rightShoulder.wasPressedThisFrame) OnConfirm();
            if (gp.leftTrigger.wasPressedThisFrame)   CycleSlotA();
            if (gp.rightTrigger.wasPressedThisFrame)  CycleSlotB();
        }

        public void Show(System.Action onConfirm)
        {
            _onConfirm = onConfirm;
            RefreshUnlockedAllies();
            _slotAIndex = -1;
            _slotBIndex = -1;

            // Pre-fill from last selection if still valid
            var saved = SaveManager.Instance?.Data?.selectedAllyIds;
            if (saved != null && saved.Count >= 1)
            {
                int idx = _unlockedAllies.FindIndex(a => a.id == saved[0]);
                if (idx >= 0) _slotAIndex = idx;
            }
            if (saved != null && saved.Count >= 2)
            {
                int idx = _unlockedAllies.FindIndex(a => a.id == saved[1]);
                if (idx >= 0 && idx != _slotAIndex) _slotBIndex = idx;
            }

            // Auto-populate slots when no saved selection exists
            if (_slotAIndex < 0 && _unlockedAllies.Count > 0)
                _slotAIndex = 0;
            if (_slotBIndex < 0 && _unlockedAllies.Count > 1 && _slotAIndex != 1)
                _slotBIndex = 1;

            UpdateSlotUI();
            _root.SetActive(true);
        }

        public void Hide() => _root.SetActive(false);

        private void RefreshUnlockedAllies()
        {
            _unlockedAllies.Clear();
            if (_registry == null || SaveManager.Instance == null) return;

            foreach (var ally in _registry.All)
            {
                if (ally != null && SaveManager.Instance.IsAllyUnlocked(ally.id))
                    _unlockedAllies.Add(ally);
            }
        }

        private void CycleSlotA()
        {
            if (_unlockedAllies.Count == 0) return;
            _slotAIndex = NextIndex(_slotAIndex, _unlockedAllies.Count, _slotBIndex);
            UpdateSlotUI();
        }

        private void CycleSlotB()
        {
            if (_unlockedAllies.Count == 0) return;
            _slotBIndex = NextIndex(_slotBIndex, _unlockedAllies.Count, _slotAIndex);
            UpdateSlotUI();
        }

        // Cycles: ally0 → ally1 → ... → empty(-1) → ally0 → ...  skipping excluded index.
        private static int NextIndex(int current, int count, int exclude)
        {
            if (count == 0) return -1;

            // Build virtual list: [0,1,...,count-1,-1] minus exclude
            // Find current position and advance one step
            if (current == -1)
            {
                // After empty, wrap to first valid ally
                for (int i = 0; i < count; i++)
                    if (i != exclude) return i;
                return -1;
            }

            // Try indices after current, skipping exclude
            for (int i = current + 1; i < count; i++)
                if (i != exclude) return i;

            // Reached end of allies → go to empty
            return -1;
        }

        private void UpdateSlotUI()
        {
            UpdateSlot(_slotAIndex, _slotAPortrait, _slotAName, _slotAPassive);
            UpdateSlot(_slotBIndex, _slotBPortrait, _slotBName, _slotBPassive);
            UpdateCompatibilityWarnings();
        }

        private void UpdateSlot(int index, Image portrait, TMP_Text name, TMP_Text passive)
        {
            bool hasAlly = index >= 0 && index < _unlockedAllies.Count;
            AllyData ally = hasAlly ? _unlockedAllies[index] : null;

            if (portrait != null)
            {
                Sprite icon = null;
                if (hasAlly)
                {
                    if (ally.portraitSprite != null)      icon = ally.portraitSprite;
                    else if (ally.roomSprite != null)     icon = ally.roomSprite;
                    else if (ally.iconSprite != null)     icon = ally.iconSprite;
                }
                portrait.enabled = icon != null;
                if (portrait.enabled)
                {
                    portrait.sprite = icon;
                    portrait.color  = Color.white;
                }
            }

            if (name != null)
                name.text = hasAlly ? ally.displayName : "—";

            if (passive != null)
                passive.text = hasAlly ? ally.passiveDescription : "";
        }

        private void UpdateCompatibilityWarnings()
        {
            bool incompatible = false;
            if (_slotAIndex >= 0 && _slotBIndex >= 0)
            {
                var a = _unlockedAllies[_slotAIndex];
                var b = _unlockedAllies[_slotBIndex];
                incompatible = !IncompatibilityChecker.AreCompatible(a, b);
            }

            if (_slotAWarning != null) _slotAWarning.gameObject.SetActive(incompatible);
            if (_slotBWarning != null) _slotBWarning.gameObject.SetActive(incompatible);
        }

        private void OnConfirm()
        {
            if (_slotAIndex >= 0 && _slotBIndex >= 0)
            {
                var a = _unlockedAllies[_slotAIndex];
                var b = _unlockedAllies[_slotBIndex];
                if (!IncompatibilityChecker.AreCompatible(a, b))
                {
                    TriggerIncompatibleFlash();
                    return;
                }
            }

            var ids = new List<string>();
            if (_slotAIndex >= 0) ids.Add(_unlockedAllies[_slotAIndex].id);
            if (_slotBIndex >= 0) ids.Add(_unlockedAllies[_slotBIndex].id);
            SaveManager.Instance?.SetSelectedAllies(ids);

            Hide();
            _onConfirm?.Invoke();
        }

        private Color _confirmButtonOriginalColor;
        private Coroutine _flashCoroutine;
        private Tween _btnShakeTween;
        private Tween _btnColorTween;

        private void TriggerIncompatibleFlash()
        {
            VigiliaAudioPlayer.Instance?.PlayIncompatibleError();

            // Shake the confirm button horizontally
            var btnRT = _confirmButton?.GetComponent<RectTransform>();
            if (btnRT != null)
            {
                _btnShakeTween?.Kill(true);
                _btnShakeTween = btnRT.DOShakeAnchorPos(0.35f, new Vector2(10f, 0f), vibrato: 24, randomness: 0f);
            }

            // Flash confirm button image red → original color
            var btnImg = _confirmButton?.GetComponent<Image>();
            if (btnImg != null)
            {
                _btnColorTween?.Kill();
                btnImg.color = new Color(1f, 0.2f, 0.2f);
                _btnColorTween = btnImg.DOColor(_confirmButtonOriginalColor, 0.4f).SetEase(Ease.OutCubic);
            }

            // Also flash warning labels if wired
            if (_flashCoroutine != null) StopCoroutine(_flashCoroutine);
            _flashCoroutine = StartCoroutine(IncompatibleFlashRoutine());
        }

        private IEnumerator IncompatibleFlashRoutine()
        {
            var red   = new Color(1f, 0.1f, 0.1f);
            var white = Color.white;
            for (int i = 0; i < 3; i++)
            {
                if (_slotAWarning != null) _slotAWarning.color = red;
                if (_slotBWarning != null) _slotBWarning.color = red;
                yield return new WaitForSecondsRealtime(0.07f);
                if (_slotAWarning != null) _slotAWarning.color = white;
                if (_slotBWarning != null) _slotBWarning.color = white;
                yield return new WaitForSecondsRealtime(0.07f);
            }
            _flashCoroutine = null;
        }

        private void OnDestroy()
        {
            _btnShakeTween?.Kill();
            _btnColorTween?.Kill();
        }
    }
}
