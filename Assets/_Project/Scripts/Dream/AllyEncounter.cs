using UnityEngine;
using Restless.Core;
using Restless.Vigil;

namespace Restless.Dream
{
    /// <summary>
    /// Place on a trigger GameObject in the Dream scene. When the player enters the
    /// trigger zone, the DreamTimer pauses and AllyEncounterPanel is shown.
    /// On Accept: unlocks the ally in SaveData. One encounter per ally per session.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class AllyEncounter : MonoBehaviour
    {
        [SerializeField] private AllyData        _allyData;
        [SerializeField] private SpriteRenderer  _spriteRenderer;
        [SerializeField] private float           _triggerRadius = 1.2f;

        [Header("Idle Animation")]
        [SerializeField] private Sprite _idleFrameA;
        [SerializeField] private Sprite _idleFrameB;
        [SerializeField] private float  _idleFrameInterval = 0.55f;

        public AllyData AllyDataRef => _allyData;

        private bool _interacted;
        private Collider2D _col;
        private float _animTimer;
        private bool  _onFrameA = true;

        private void Start()
        {
            if (_allyData == null) { enabled = false; return; }

            if (SaveManager.Instance != null && SaveManager.Instance.IsAllyUnlocked(_allyData.id))
            {
                gameObject.SetActive(false);
                return;
            }

            _col = GetComponent<Collider2D>();
            _col.isTrigger = true;

            if (_spriteRenderer != null && _idleFrameA != null)
                _spriteRenderer.sprite = _idleFrameA;
        }

        private void Update()
        {
            if (_spriteRenderer == null || _idleFrameA == null || _idleFrameB == null) return;

            _animTimer += Time.deltaTime;
            if (_animTimer >= _idleFrameInterval)
            {
                _animTimer -= _idleFrameInterval;
                _onFrameA = !_onFrameA;
                _spriteRenderer.sprite = _onFrameA ? _idleFrameA : _idleFrameB;
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_interacted) return;
            if (!other.CompareTag("Player")) return;

            _interacted = true;
            DreamTimer.Instance?.Pause();
            DreamSFXPlayer.Instance?.PlayAllyEncounter();

            AllyEncounterPanel.Instance?.Show(
                _allyData,
                onAccept: HandleAccept,
                onIgnore: HandleIgnore
            );
        }

        private void HandleAccept()
        {
            SaveManager.Instance?.UnlockAlly(_allyData.id);
            DreamTimer.Instance?.Resume();
            gameObject.SetActive(false);
        }

        private void HandleIgnore()
        {
            DreamTimer.Instance?.Resume();
            _interacted = false;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0.9f, 0.85f, 0.3f, 0.4f);
            Gizmos.DrawWireSphere(transform.position, _triggerRadius);
        }
    }
}
