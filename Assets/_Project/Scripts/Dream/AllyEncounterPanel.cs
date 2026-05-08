using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;
using Restless.Vigil;

namespace Restless.Dream
{
    public class AllyEncounterPanel : MonoBehaviour
    {
        public static AllyEncounterPanel Instance { get; private set; }

        [SerializeField] private GameObject _root;
        [SerializeField] private Image      _portrait;
        [SerializeField] private TMP_Text   _nameText;
        [SerializeField] private TMP_Text   _passiveText;
        [SerializeField] private Button     _acceptButton;
        [SerializeField] private Button     _ignoreButton;

        private TMP_Text _acceptLabel;
        private TMP_Text _ignoreLabel;
        private Action   _onAccept;
        private Action   _onIgnore;

        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;

            _acceptButton.onClick.AddListener(OnAccept);
            _ignoreButton.onClick.AddListener(OnIgnore);

            _acceptLabel = _acceptButton.GetComponentInChildren<TMP_Text>();
            _ignoreLabel = _ignoreButton.GetComponentInChildren<TMP_Text>();

            _root.SetActive(false);
        }

        private void Update()
        {
            if (!_root.activeSelf) return;
            var gp = Gamepad.current;
            if (gp == null) return;
            if (gp.leftShoulder.wasPressedThisFrame)  OnAccept();
            if (gp.rightShoulder.wasPressedThisFrame) OnIgnore();
        }

        public void Show(AllyData ally, Action onAccept, Action onIgnore)
        {
            _onAccept = onAccept;
            _onIgnore = onIgnore;

            if (_portrait  != null && ally.portraitSprite != null)
                _portrait.sprite = ally.portraitSprite;
            if (_nameText    != null) _nameText.text    = ally.displayName;
            if (_passiveText != null) _passiveText.text = ally.passiveDescription;

            if (_acceptLabel != null) _acceptLabel.text = "Aceptar  (LB)";
            if (_ignoreLabel != null) _ignoreLabel.text = "Ignorar  (RB)";

            _root.SetActive(true);
        }

        public void Hide() => _root.SetActive(false);

        private void OnAccept() { Hide(); _onAccept?.Invoke(); }
        private void OnIgnore() { Hide(); _onIgnore?.Invoke(); }
    }
}
