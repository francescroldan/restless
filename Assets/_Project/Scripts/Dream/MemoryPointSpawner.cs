using System.Collections.Generic;
using UnityEngine;

namespace Restless.Dream
{
    /// <summary>
    /// At dream start, activates a random subset of MemoryPoints and assigns
    /// each a random fragment from the pool. All inactive points are disabled.
    /// Attach to _Managers in the Dream scene.
    /// </summary>
    public class MemoryPointSpawner : MonoBehaviour
    {
        [SerializeField] private List<MemoryPoint>    _points        = new();
        [SerializeField] private List<MemoryFragment> _fragmentPool  = new();
        [SerializeField, Range(1, 10)] private int    _minActive     = 2;
        [SerializeField, Range(1, 10)] private int    _maxActive     = 4;

        [Header("Fog wrapping")]
        [Tooltip("How many of the active memory points start hidden under fog (Fragment type).")]
        [SerializeField, Range(0, 6)] private int     _fogFragmentCount = 1;

        private void Start()
        {
            if (_points.Count == 0 || _fragmentPool.Count == 0)
            {
                Debug.LogWarning("[MemoryPointSpawner] Points or fragment pool is empty — skipping randomization.");
                return;
            }

            int count = Random.Range(_minActive, Mathf.Min(_maxActive, _points.Count) + 1);

            // Shuffle point list
            var shuffled = new List<MemoryPoint>(_points);
            for (int i = shuffled.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (shuffled[i], shuffled[j]) = (shuffled[j], shuffled[i]);
            }

            int fogged = 0;
            for (int i = 0; i < shuffled.Count; i++)
            {
                if (shuffled[i] == null) continue;

                if (i < count)
                {
                    var frag = _fragmentPool[Random.Range(0, _fragmentPool.Count)];
                    shuffled[i].AssignFragment(frag);
                    shuffled[i].gameObject.SetActive(true);

                    if (fogged < _fogFragmentCount)
                    {
                        var presence = shuffled[i].gameObject.AddComponent<DreamPresence>();
                        presence.SetType(DreamPresence.PresenceType.Fragment);
                        fogged++;
                    }
                }
                else
                {
                    shuffled[i].gameObject.SetActive(false);
                }
            }

            Debug.Log($"[MemoryPointSpawner] Activated {count}/{_points.Count} memory points ({fogged} under fog).");
        }
    }
}
