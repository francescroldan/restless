using System.Collections;
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
        [SerializeField] private AudioClip _clockClip;
        [SerializeField] private AudioClip _sfxReturnTranquil;
        [SerializeField] private AudioClip _sfxReturnAbrupt;
        [SerializeField] private AudioClip _sfxSleep;
        [SerializeField] private AudioClip _sfxAllyHover;
        [SerializeField] private AudioClip _sfxAllyClick;
        [SerializeField] private AudioClip _sfxIncompatible;
        [SerializeField] private AudioClip _sfxUrnFill;

        [SerializeField] private float _ambientVolume      = 0.4f;
        [SerializeField] private float _clockVolume        = 0.25f;
        [SerializeField] private float _sfxVolume          = 0.7f;
        [SerializeField] private float _sfxAllyHoverVol    = 0.4f;
        [SerializeField] private float _sfxAllyClickVol    = 0.6f;
        [SerializeField] private float _sfxIncompatibleVol = 0.7f;
        [SerializeField] private float _sfxUrnFillVol      = 0.8f;

        private AudioSource _ambientSrc;
        private AudioSource _clockSrc;
        private AudioSource _sfxSrc;
        private AudioSource _urnSrc;

        private void Awake()
        {
            if (Instance != null) { Destroy(this); return; }
            Instance = this;

            _ambientSrc = gameObject.AddComponent<AudioSource>();
            _ambientSrc.loop         = true;
            _ambientSrc.playOnAwake  = false;
            _ambientSrc.spatialBlend = 0f;
            _ambientSrc.volume       = _ambientVolume;

            _clockSrc = gameObject.AddComponent<AudioSource>();
            _clockSrc.loop         = true;
            _clockSrc.playOnAwake  = false;
            _clockSrc.spatialBlend = 0f;
            _clockSrc.volume       = _clockVolume;

            _sfxSrc = gameObject.AddComponent<AudioSource>();
            _sfxSrc.loop         = false;
            _sfxSrc.playOnAwake  = false;
            _sfxSrc.spatialBlend = 0f;

            _urnSrc = gameObject.AddComponent<AudioSource>();
            _urnSrc.loop         = false;
            _urnSrc.playOnAwake  = false;
            _urnSrc.spatialBlend = 0f;
            _urnSrc.volume       = _sfxUrnFillVol;
        }

        private void Start()
        {
            if (_ambientClip != null)
            {
                _ambientSrc.clip = _ambientClip;
                _ambientSrc.Play();
            }

            if (_clockClip != null)
            {
                _clockSrc.clip = _clockClip;
                _clockSrc.Play();
            }

            if (SceneLoader.Instance != null && SceneLoader.Instance.LastVigiliaCameFromDream)
            {
                if (SceneLoader.Instance.LastWakeUpWasAbrupt) PlayReturnAbrupt();
                else                                          PlayReturnTranquil();
            }
        }

        public void PlayUrnFill(int gainedCount)
        {
            if (_sfxUrnFill == null || _urnSrc == null) return;
            // 0.5s per fragment, min 0.4s, max 2.5s
            float duration = Mathf.Clamp(gainedCount * 0.5f, 0.4f, 2.5f);
            StartCoroutine(PlayUrnClipped(duration));
        }
        public void PlayReturnTranquil() => PlaySFX(_sfxReturnTranquil, _sfxVolume);
        public void PlayReturnAbrupt()   => PlaySFX(_sfxReturnAbrupt,   _sfxVolume);
        public void PlaySleep()          => PlaySFX(_sfxSleep,          _sfxVolume);
        public void PlayAllyHover()        => PlaySFX(_sfxAllyHover,      _sfxAllyHoverVol);
        public void PlayAllyClick()        => PlaySFX(_sfxAllyClick,      _sfxAllyClickVol);
        public void PlayIncompatibleError() => PlaySFX(_sfxIncompatible,   _sfxIncompatibleVol);

        private IEnumerator PlayUrnClipped(float duration)
        {
            _urnSrc.clip = _sfxUrnFill;
            _urnSrc.Play();
            yield return new WaitForSeconds(duration);
            _urnSrc.Stop();
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
            _ambientSrc?.Stop();
            _clockSrc?.Stop();
            _urnSrc?.Stop();
        }

        private void PlaySFX(AudioClip clip, float volume)
        {
            if (clip == null) return;
            _sfxSrc.PlayOneShot(clip, volume);
        }
    }
}
