using UnityEngine;
using Restless.Core;

namespace Restless.Vigil
{
    /// <summary>
    /// Manages audio for the Vigilia scene: ambient loop + transition SFX.
    /// Attach to _Managers in Vigil scene.
    /// </summary>
    public class VigiliaAudioPlayer : MonoBehaviour
    {
        public static VigiliaAudioPlayer Instance { get; private set; }

        [SerializeField] private AudioClip _ambientClip;
        [SerializeField] private AudioClip _sfxReturnTranquil;
        [SerializeField] private AudioClip _sfxReturnAbrupt;
        [SerializeField] private AudioClip _sfxSleep;
        [SerializeField] private AudioClip _sfxAllyHover;
        [SerializeField] private AudioClip _sfxAllyClick;
        [SerializeField] private AudioClip _sfxIncompatible;

        [SerializeField] private float _ambientVolume      = 0.4f;
        [SerializeField] private float _sfxVolume          = 0.7f;
        [SerializeField] private float _sfxAllyHoverVol    = 0.4f;
        [SerializeField] private float _sfxAllyClickVol    = 0.6f;
        [SerializeField] private float _sfxIncompatibleVol = 0.7f;

        private AudioSource _ambientSrc;
        private AudioSource _sfxSrc;

        private void Awake()
        {
            if (Instance != null) { Destroy(this); return; }
            Instance = this;

            _ambientSrc = gameObject.AddComponent<AudioSource>();
            _ambientSrc.loop         = true;
            _ambientSrc.playOnAwake  = false;
            _ambientSrc.spatialBlend = 0f;
            _ambientSrc.volume       = _ambientVolume;

            _sfxSrc = gameObject.AddComponent<AudioSource>();
            _sfxSrc.loop         = false;
            _sfxSrc.playOnAwake  = false;
            _sfxSrc.spatialBlend = 0f;
        }

        private void Start()
        {
            if (_ambientClip != null)
            {
                _ambientSrc.clip = _ambientClip;
                _ambientSrc.Play();
            }

            if (SceneLoader.Instance != null)
            {
                if (SceneLoader.Instance.LastWakeUpWasAbrupt) PlayReturnAbrupt();
                else                                          PlayReturnTranquil();
            }
        }

        public void PlayReturnTranquil() => PlaySFX(_sfxReturnTranquil, _sfxVolume);
        public void PlayReturnAbrupt()   => PlaySFX(_sfxReturnAbrupt,   _sfxVolume);
        public void PlaySleep()          => PlaySFX(_sfxSleep,          _sfxVolume);
        public void PlayAllyHover()        => PlaySFX(_sfxAllyHover,      _sfxAllyHoverVol);
        public void PlayAllyClick()        => PlaySFX(_sfxAllyClick,      _sfxAllyClickVol);
        public void PlayIncompatibleError() => PlaySFX(_sfxIncompatible,   _sfxIncompatibleVol);

        private void PlaySFX(AudioClip clip, float volume)
        {
            if (clip == null) return;
            _sfxSrc.PlayOneShot(clip, volume);
        }
    }
}
