using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Restless.Core;
using Restless.Vigil;

namespace Restless.Dream
{
    public class AllyEncounterSpawner : MonoBehaviour
    {
        public static AllyEncounterSpawner Instance { get; private set; }

        [SerializeField] private AllyEncounter[] _encounters;

        public AllyData   ActiveAlly             { get; private set; }
        public Vector2    ActiveEncounterPosition { get; private set; }
        public bool       AllyObtained           => ActiveAlly != null &&
                                                    SaveManager.Instance != null &&
                                                    SaveManager.Instance.IsAllyUnlocked(ActiveAlly.id);

        private void Awake()
        {
            if (Instance != null) { Destroy(this); return; }
            Instance = this;
        }

        private IEnumerator Start()
        {
            foreach (var e in _encounters)
                if (e != null) e.gameObject.SetActive(false);

            // Wait for SaveManager so unlock state is accurate before picking a candidate
            yield return new WaitUntil(() => SaveManager.Instance != null);

            var candidates = new List<AllyEncounter>();
            foreach (var e in _encounters)
            {
                if (e?.AllyDataRef == null) continue;
                if (SaveManager.Instance.IsAllyUnlocked(e.AllyDataRef.id)) continue;
                candidates.Add(e);
            }

            if (candidates.Count == 0)
            {
                Debug.Log("[AllyEncounterSpawner] Todos los aliados ya desbloqueados — sin encuentro esta run.");
                yield break;
            }

            var chosen = candidates[Random.Range(0, candidates.Count)];
            chosen.gameObject.SetActive(true);

            var presence = chosen.gameObject.AddComponent<DreamPresence>();
            presence.SetType(DreamPresence.PresenceType.AllyEcho);

            ActiveAlly = chosen.AllyDataRef;
            ActiveEncounterPosition = chosen.transform.position;
            Debug.Log($"[AllyEncounterSpawner] Encuentro esta run: {ActiveAlly.displayName} @ {ActiveEncounterPosition}");
        }
    }
}
