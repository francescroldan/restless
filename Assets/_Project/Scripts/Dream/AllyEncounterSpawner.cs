using System.Collections.Generic;
using UnityEngine;
using Restless.Core;
using Restless.Vigil;

namespace Restless.Dream
{
    /// <summary>
    /// Each time the Dream scene loads, picks ONE ally not yet unlocked and places
    /// its encounter at a random position from the predefined spawn points.
    /// All other encounter GameObjects are deactivated.
    /// </summary>
    public class AllyEncounterSpawner : MonoBehaviour
    {
        [SerializeField] private AllyEncounter[] _encounters;
        [SerializeField] private Transform[]     _spawnPoints;

        private void Start()
        {
            if (_encounters == null || _encounters.Length == 0) return;

            // Deactivate all encounters first
            foreach (var enc in _encounters)
                if (enc != null) enc.gameObject.SetActive(false);

            // Filter to allies not yet unlocked
            var candidates = new List<AllyEncounter>();
            foreach (var enc in _encounters)
            {
                if (enc == null) continue;
                var ally = GetAllyData(enc);
                if (ally == null) continue;
                if (SaveManager.Instance != null && SaveManager.Instance.IsAllyUnlocked(ally.id)) continue;
                candidates.Add(enc);
            }

            if (candidates.Count == 0) return;  // all allies already unlocked

            // Pick one random encounter and one random spawn point
            var chosen = candidates[Random.Range(0, candidates.Count)];

            if (_spawnPoints != null && _spawnPoints.Length > 0)
            {
                var spawnPt = _spawnPoints[Random.Range(0, _spawnPoints.Length)];
                chosen.transform.position = spawnPt.position;
            }

            chosen.gameObject.SetActive(true);
        }

        private static AllyData GetAllyData(AllyEncounter enc)
        {
            // AllyEncounter stores _allyData as a private serialized field — access via property if available,
            // otherwise use SerializedObject in editor or reflection at runtime.
            // We expose it via a public accessor added to AllyEncounter.
            return enc.AllyDataRef;
        }
    }
}
