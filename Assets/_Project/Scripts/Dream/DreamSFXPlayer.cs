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
        [SerializeField] private AudioClip _sfxEntityDetected;
        [SerializeField] private float     _entityNearbyRadius    = 6f;
        [SerializeField] private float     _entityNearbyCooldown  = 3f;

        [Header("Fragment proximity")]
        [SerializeField] private AudioClip _sfxFragmentNearby;
        [SerializeField] private float     _fragmentNearbyRadius   = 3f;
        [SerializeField] private float     _fragmentNearbyCooldown = 2.5f;

        [SerializeField] private float _sfxVolume = 0.7f;

        private AudioSource _src;
        private float       _entityNearbyCooldownTimer;
        private float       _fragmentNearbyCooldownTimer;

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
            if (_entityNearbyCooldownTimer  > 0f) _entityNearbyCooldownTimer  -= Time.deltaTime;
            if (_fragmentNearbyCooldownTimer > 0f) _fragmentNearbyCooldownTimer -= Time.deltaTime;

            var protagonist = GameObject.FindWithTag("Player");
            if (protagonist == null) return;

            // Entity nearby sound
            if (_entityNearbyCooldownTimer <= 0f)
            {
                var entities = FindObjectsByType<DreamEntity>(FindObjectsSortMode.None);
                foreach (var e in entities)
                {
                    if (e == null) continue;
                    if (Vector2.Distance(protagonist.transform.position, e.transform.position) <= _entityNearbyRadius)
                    {
                        Play(_sfxEntityNearby, 0.5f);
                        _entityNearbyCooldownTimer = _entityNearbyCooldown;
                        break;
                    }
                }
            }

            // Fragment nearby sound
            if (_fragmentNearbyCooldownTimer <= 0f)
            {
                var memPoints = FindObjectsByType<MemoryPoint>(FindObjectsSortMode.None);
                foreach (var mp in memPoints)
                {
                    if (mp == null || mp.CurrentState != MemoryPoint.State.Available) continue;
                    if (Vector2.Distance(protagonist.transform.position, mp.transform.position) <= _fragmentNearbyRadius)
                    {
                        Play(_sfxFragmentNearby, 0.4f);
                        _fragmentNearbyCooldownTimer = _fragmentNearbyCooldown;
                        break;
                    }
                }
            }
        }

        public void PlayEntityDetected()   => Play(_sfxEntityDetected, 0.8f);
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
