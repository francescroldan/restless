using UnityEngine;

namespace Restless.Dream
{
    /// <summary>
    /// Singleton for one-shot SFX in the Dream scene.
    /// </summary>
    public class DreamSFXPlayer : MonoBehaviour
    {
        public static DreamSFXPlayer Instance { get; private set; }

        [Header("Navigation")]
        [SerializeField] private AudioClip _sfxZoneEnter;
        [SerializeField] private AudioClip _sfxAllyEncounter;
        [SerializeField] private AudioClip _sfxWakeupVoluntary;
        [SerializeField] private AudioClip _sfxWakeupAbrupt;

        [Header("Minigame")]
        [SerializeField] private AudioClip _sfxMinigameHit;
        [SerializeField] private AudioClip _sfxMinigameMiss;
        [SerializeField] private AudioClip _sfxMinigameSuccess;
        [SerializeField] private AudioClip _sfxMinigameFail;
        [SerializeField] private AudioClip _sfxFragmentCollect;
        [SerializeField] private AudioClip _sfxMemoryActivate;

        [Header("Entity")]
        [SerializeField] private AudioClip _sfxEntityNearby;
        [SerializeField] private float     _entityNearbyRadius    = 6f;
        [SerializeField] private float     _entityNearbyCooldown  = 3f;

        [SerializeField] private float _sfxVolume = 0.7f;

        private AudioSource _src;
        private float       _entityNearbyCooldownTimer;

        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
            _src = gameObject.AddComponent<AudioSource>();
            _src.spatialBlend = 0f;
            _src.playOnAwake  = false;
        }

        private void Update()
        {
            if (_entityNearbyCooldownTimer > 0f)
                _entityNearbyCooldownTimer -= Time.deltaTime;

            if (_entityNearbyCooldownTimer > 0f) return;

            var protagonist = GameObject.FindWithTag("Player");
            if (protagonist == null) return;

            var entities = FindObjectsByType<DreamEntity>(FindObjectsSortMode.None);
            foreach (var e in entities)
            {
                if (e == null) continue;
                float dist = Vector2.Distance(protagonist.transform.position, e.transform.position);
                if (dist <= _entityNearbyRadius)
                {
                    Play(_sfxEntityNearby, 0.5f);
                    _entityNearbyCooldownTimer = _entityNearbyCooldown;
                    break;
                }
            }
        }

        public void PlayZoneEnter()        => Play(_sfxZoneEnter);
        public void PlayAllyEncounter()    => Play(_sfxAllyEncounter);
        public void PlayWakeupVoluntary()  => Play(_sfxWakeupVoluntary);
        public void PlayWakeupAbrupt()     => Play(_sfxWakeupAbrupt);
        public void PlayMinigameHit()      => Play(_sfxMinigameHit);
        public void PlayMinigameMiss()     => Play(_sfxMinigameMiss);
        public void PlayMinigameSuccess()  => Play(_sfxMinigameSuccess);
        public void PlayMinigameFail()     => Play(_sfxMinigameFail);
        public void PlayFragmentCollect()  => Play(_sfxFragmentCollect);
        public void PlayMemoryActivate()   => Play(_sfxMemoryActivate);

        private void Play(AudioClip clip, float volume = -1f)
        {
            if (clip == null) return;
            _src.PlayOneShot(clip, volume < 0f ? _sfxVolume : volume);
        }
    }
}
