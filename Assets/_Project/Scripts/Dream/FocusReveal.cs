using UnityEngine;

namespace Restless.Dream
{
    /// <summary>
    /// Elements appear as faint silhouettes and materialise fully when the player
    /// gets within revealRadius. No cone direction required — proximity is enough.
    /// </summary>
    public class FocusReveal : MonoBehaviour
    {
        [Header("Ghost state (far from player)")]
        [SerializeField] private Color _ghostColor   = new Color(0.6f, 0.62f, 0.75f, 0.35f);

        [Header("Present state (within reveal radius)")]
        [SerializeField] private Color _presentColor = Color.white;

        [SerializeField] private float _revealRadius = 5f;
        [SerializeField] private float _lerpSpeed    = 3f;

        private SpriteRenderer _sr;
        private Transform      _player;

        private void Awake()
        {
            _sr = GetComponentInChildren<SpriteRenderer>();
            if (_sr != null)
                _sr.color = _ghostColor;
        }

        private void Start()
        {
            var playerGO = GameObject.FindWithTag("Player");
            if (playerGO != null) _player = playerGO.transform;
        }

        private void Update()
        {
            if (_sr == null || _player == null) return;

            float dist   = Vector2.Distance(_player.position, transform.position);
            Color target = dist <= _revealRadius ? _presentColor : _ghostColor;
            _sr.color    = Color.Lerp(_sr.color, target, Time.deltaTime * _lerpSpeed);
        }
    }
}
