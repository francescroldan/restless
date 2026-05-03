using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Restless.Vigil;

namespace Restless.Dream
{
    /// <summary>
    /// Screen Space Overlay panel shown when the player encounters an ally in the Dream.
    /// Wire up via Inspector. Attach to the AllyEncounterPanel Canvas child.
    /// </summary>
    public class AllyEncounterPanel : MonoBehaviour
    {
        public static AllyEncounterPanel Instance { get; private set; }

        [SerializeField] private GameObject    _root;
        [SerializeField] private Image         _portrait;
        [SerializeField] private TMP_Text      _nameText;
        [SerializeField] private TMP_Text      _passiveText;
        [SerializeField] private Button        _acceptButton;
        [SerializeField] private Button        _ignoreButton;

        private Action _onAccept;
        private Action _onIgnore;

        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;

            _acceptButton.onClick.AddListener(OnAccept);
            _ignoreButton.onClick.AddListener(OnIgnore);
            _root.SetActive(false);
        }

        public void Show(AllyData ally, Action onAccept, Action onIgnore)
        {
            _onAccept = onAccept;
            _onIgnore = onIgnore;

            if (_portrait != null && ally.portraitSprite != null)
                _portrait.sprite = ally.portraitSprite;

            if (_nameText != null)
                _nameText.text = ally.displayName;

            if (_passiveText != null)
                _passiveText.text = ally.passiveDescription;

            _root.SetActive(true);
        }

        public void Hide() => _root.SetActive(false);

        private void OnAccept()
        {
            Hide();
            _onAccept?.Invoke();
        }

        private void OnIgnore()
        {
            Hide();
            _onIgnore?.Invoke();
        }
    }
}
