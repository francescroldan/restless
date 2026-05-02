using UnityEngine;
using UnityEngine.InputSystem;
using Restless.Core;

namespace Restless.Dream
{
    public class WakeUpManager : MonoBehaviour
    {
        [SerializeField] private float _fadeDuration = 1.2f;

        private PlayerInput _playerInput;
        private bool _waking;
        private bool _abrupt;
        private float _fadeAlpha;
        private Texture2D _white;
        private GUIStyle _labelStyle;

        private void Awake()
        {
            _white = new Texture2D(1, 1);
            _white.SetPixel(0, 0, Color.white);
            _white.Apply();
        }

        private void Start()
        {
            var protagonistGO = GameObject.FindWithTag("Player");
            if (protagonistGO != null)
                _playerInput = protagonistGO.GetComponent<PlayerInput>();

            if (RestlessnessManager.Instance != null)
                RestlessnessManager.Instance.OnMaxReached += TriggerAbruptWakeUp;

            if (DreamTimer.Instance != null)
                DreamTimer.Instance.OnExpired += TriggerAbruptWakeUp;
        }

        private void OnDestroy()
        {
            if (RestlessnessManager.Instance != null)
                RestlessnessManager.Instance.OnMaxReached -= TriggerAbruptWakeUp;

            if (DreamTimer.Instance != null)
                DreamTimer.Instance.OnExpired -= TriggerAbruptWakeUp;
        }

        private void Update()
        {
            if (_waking)
            {
                _fadeAlpha += Time.deltaTime / _fadeDuration;
                if (_fadeAlpha >= 1f)
                {
                    _fadeAlpha = 1f;
                    GameManager.Instance?.ExitDream(abrupt: _abrupt);
                }
                return;
            }

            if (_playerInput == null) return;
            if (_playerInput.actions["Player/WakeUp"].WasPressedThisFrame())
                BeginWakeUp(abrupt: false);
        }

        private void BeginWakeUp(bool abrupt)
        {
            if (_waking) return;
            _waking  = true;
            _abrupt  = abrupt;
            _fadeAlpha = 0f;
            Debug.Log($"[WakeUpManager] Wake-up — abrupt: {abrupt}");
        }

        private void TriggerAbruptWakeUp() => BeginWakeUp(abrupt: true);

        private void OnGUI()
        {
            if (!_waking || _fadeAlpha <= 0f) return;

            EnsureStyle();

            float sw = Screen.width, sh = Screen.height;
            Color overlayColor = _abrupt
                ? new Color(0.6f, 0.05f, 0.05f, _fadeAlpha)
                : new Color(0f, 0f, 0f, _fadeAlpha);

            GUI.color = overlayColor;
            GUI.DrawTexture(new Rect(0, 0, sw, sh), _white);

            if (_fadeAlpha > 0.3f)
            {
                float textAlpha = Mathf.InverseLerp(0.3f, 0.7f, _fadeAlpha);
                GUI.color = new Color(1f, 1f, 1f, textAlpha);
                string msg = _abrupt ? "DESPERTAR ABRUPTO" : "despertando...";
                GUI.Label(new Rect(0, sh * 0.5f - 20f, sw, 40f), msg, _labelStyle);
            }

            GUI.color = Color.white;
        }

        private void EnsureStyle()
        {
            if (_labelStyle != null) return;
            _labelStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize  = 20,
                fontStyle = FontStyle.Bold
            };
            _labelStyle.normal.textColor = Color.white;
        }
    }
}
