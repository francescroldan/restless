using UnityEngine;

namespace Restless.Dream.Procedural
{
    /// <summary>
    /// Entry point for procedural dream generation. Replaces the static Dream scene layout.
    /// Attach to _Managers in the Dream scene alongside DreamPresenceSpawner.
    ///
    /// Order of operations:
    ///   1. Generate abstract graph (RunGraphGenerator)
    ///   2. Place room prefabs (RoomAssembler)
    ///   3. Spawn presences into placed rooms (DreamPresenceSpawner — runs next frame)
    /// </summary>
    public class DreamSceneBootstrap : MonoBehaviour
    {
        [Header("Generation")]
        [SerializeField] private int  _targetRooms    = 10;
        [SerializeField] private bool _useRandomSeed  = true;
        [SerializeField] private int  _fixedSeed      = 42;

        [Header("References")]
        [SerializeField] private RoomAssembler _assembler;

        [Header("Scene management")]
        [Tooltip("Root GO that holds the hand-crafted static environment. Disabled when procedural generation runs.")]
        [SerializeField] private GameObject _staticEnvironment;

        private void Awake()
        {
            // Disable static environment so it doesn't clash with procedural rooms
            if (_staticEnvironment != null)
                _staticEnvironment.SetActive(false);

            // Suppress the "2 audio listeners" warning: keep only the first active one.
            var listeners = FindObjectsByType<AudioListener>(FindObjectsSortMode.None);
            bool keptOne = false;
            foreach (var l in listeners)
            {
                if (!keptOne && l.enabled) { keptOne = true; continue; }
                l.enabled = false;
            }

            int seed = _useRandomSeed ? Random.Range(0, int.MaxValue) : _fixedSeed;

            var graph = RunGraphGenerator.Generate(seed, _targetRooms);
            bool ok   = _assembler.Assemble(graph, seed);

            if (!ok)
            {
                Debug.LogError("[DreamSceneBootstrap] Room assembly failed.");
                return;
            }

            // Pass placed rooms to the presence spawner so it can use room spawn zones
            var spawner = GetComponent<DreamPresenceSpawner>();
            if (spawner != null)
                spawner.SetRooms(_assembler.PlacedRooms);

            // Initialize room camera: show entrance, hide all other rooms.
            // RoomCamera is added automatically to Camera.main if not already present.
            var cam = Camera.main;
            if (cam != null)
            {
                var roomCam = cam.GetComponent<RoomCamera>() ?? cam.gameObject.AddComponent<RoomCamera>();
                roomCam.Init(_assembler.PlacedRooms, graph.Entrance.PlacedRoom);
            }
            else
            {
                Debug.LogWarning("[DreamSceneBootstrap] Camera.main not found — RoomCamera not initialized.");
            }

            Debug.Log($"[DreamSceneBootstrap] Generated run — seed={seed} rooms={_assembler.PlacedRooms.Count}");
        }
    }
}
