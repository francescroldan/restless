using UnityEngine;
using UnityEngine.Audio;

namespace Restless.Dream
{
    /// <summary>
    /// Transitions Audio Mixer snapshots based on Restlessness thresholds.
    /// Attach to _Managers in the Dream scene. Assign the 4 snapshots in the Inspector.
    /// </summary>
    public class RestlessnessAudioController : MonoBehaviour
    {
        [SerializeField] private AudioMixerSnapshot _snapCalm;
        [SerializeField] private AudioMixerSnapshot _snapTense;
        [SerializeField] private AudioMixerSnapshot _snapCritical;
        [SerializeField] private AudioMixerSnapshot _snapOverload;

        [SerializeField] private float _transitionTime = 2f;

        private int _currentThreshold = -1;

        private void Awake()
        {
            if (_snapCalm == null || _snapTense == null || _snapCritical == null || _snapOverload == null)
                enabled = false;
        }

        private void Update()
        {
            if (RestlessnessManager.Instance == null) return;
            float value = RestlessnessManager.Instance.NormalizedValue;
            int threshold = GetThreshold(value);
            if (threshold == _currentThreshold) return;

            _currentThreshold = threshold;
            TransitionTo(threshold);
        }

        private static int GetThreshold(float value)
        {
            if (value < 0.25f) return 0;
            if (value < 0.5f)  return 1;
            if (value < 0.75f) return 2;
            return 3;
        }

        private void TransitionTo(int threshold)
        {
            AudioMixerSnapshot snap = threshold switch
            {
                0 => _snapCalm,
                1 => _snapTense,
                2 => _snapCritical,
                _ => _snapOverload
            };
            snap?.TransitionTo(_transitionTime);
        }
    }
}
