using UnityEngine;
using Restless.Core;

namespace Restless.Vigil
{
    /// <summary>
    /// Attach to an ally's room GameObject in the Vigilia scene.
    /// Shows the ally sprite only if the ally is unlocked in SaveData.
    /// </summary>
    public class RoomAllyPresence : MonoBehaviour
    {
        [SerializeField] private AllyData        _allyData;
        [SerializeField] private SpriteRenderer  _spriteRenderer;
        [SerializeField] private Animator        _animator;

        private void Start()
        {
            Refresh();
        }

        public void Refresh()
        {
            if (_allyData == null) return;

            bool unlocked = SaveManager.Instance != null && SaveManager.Instance.IsAllyUnlocked(_allyData.id);
            gameObject.SetActive(unlocked);

            if (unlocked && _spriteRenderer != null && _allyData.roomSprite != null)
                _spriteRenderer.sprite = _allyData.roomSprite;
        }
    }
}
