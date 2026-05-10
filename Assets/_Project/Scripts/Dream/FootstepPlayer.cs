using UnityEngine;

namespace Restless.Dream
{
    /// <summary>
    /// Plays footstep SFX in sync with protagonist movement speed.
    /// Attach to the Protagonist GameObject in the Dream scene.
    /// </summary>
    public class FootstepPlayer : MonoBehaviour
    {
        [SerializeField] private AudioClip[] _clips;
        [SerializeField] private float _walkInterval = 0.42f;
        [SerializeField] private float _runInterval  = 0.26f;
        [SerializeField] private float _volume       = 0.5f;

        private AudioSource       _src;
        private ProtagonistController _ctrl;
        private float _timer;
        private int   _clipIndex;

        private void Awake()
        {
            _src  = gameObject.AddComponent<AudioSource>();
            _src.spatialBlend = 0f;
            _src.playOnAwake  = false;
            _ctrl = GetComponent<ProtagonistController>();
        }

        private void Update()
        {
            if (_ctrl == null || !_ctrl.IsMoving || _clips == null || _clips.Length == 0) return;

            _timer -= Time.deltaTime;
            if (_timer > 0f) return;

            _timer = _ctrl.IsRunning ? _runInterval : _walkInterval;
            PlayNext();
        }

        private void PlayNext()
        {
            var clip = _clips[_clipIndex % _clips.Length];
            if (clip != null)
            {
                _src.pitch = Random.Range(0.9f, 1.1f);
                _src.PlayOneShot(clip, _volume);
                _src.pitch = 1f;
            }
            _clipIndex++;
        }
    }
}
