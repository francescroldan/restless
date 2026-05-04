using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Restless.Dream
{
    /// <summary>
    /// Controls GlobalLight2D intensity at runtime to produce the dream's oppressive darkness.
    /// In the editor the light stays at inspector value (bright, easy to work with).
    /// At runtime this script overrides the intensity based on zone and Restlessness.
    /// </summary>
    public class DreamAtmosphereController : MonoBehaviour
    {
        [SerializeField] private Light2D _globalLight;

        [Header("Base atmosphere")]
        [SerializeField] private float _baseIntensity    = 0.05f;
        [SerializeField] private float _maxIntensity     = 0.18f;  // at full restlessness
        [SerializeField] private float _transitionSpeed  = 1f;

        [Header("Ally modifier (set by DreamPassiveApplier)")]
        public float BrightnessBonus = 0f;  // allies can add to this

        private float _targetIntensity;

        private void Start()
        {
            if (_globalLight == null)
                _globalLight = FindFirstObjectByType<Light2D>();

            _targetIntensity = _baseIntensity + BrightnessBonus;

            if (_globalLight != null)
                _globalLight.intensity = _targetIntensity;
        }

        private void Update()
        {
            if (_globalLight == null || RestlessnessManager.Instance == null) return;

            float t = RestlessnessManager.Instance.NormalizedValue;
            // As restlessness rises, slightly increase ambient (more "awake" = less darkness)
            _targetIntensity = Mathf.Lerp(_baseIntensity, _maxIntensity, t) + BrightnessBonus;
            _globalLight.intensity = Mathf.MoveTowards(
                _globalLight.intensity, _targetIntensity, _transitionSpeed * Time.deltaTime);
        }
    }
}
