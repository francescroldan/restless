using UnityEngine;
using Restless.Core;
using Restless.Dream.Procedural;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Restless.Dream
{
    /// <summary>
    /// Minimal runtime bootstrap for the RoomWorkshop test scene.
    /// Spawns the protagonist and a test room at play-time.
    /// Prefab paths are resolved automatically in the Editor; no wiring required.
    /// </summary>
    public class RoomWorkshopBootstrap : MonoBehaviour
    {
        [Header("Override (leave empty to auto-resolve in Editor)")]
        [SerializeField] private GameObject _protagonistPrefab;
        [SerializeField] private GameObject _roomPrefab;

        [Header("Placement")]
        [SerializeField] private Vector2 _protagonistSpawn = new Vector2(0f, 0f);
        [SerializeField] private Vector2 _roomPosition     = new Vector2(0f, 0f);

        private void Start()
        {
            AutoResolvePrefabs();
            SpawnRoom();
            var player = SpawnProtagonist();
            WireCamera(player);
        }

        private void AutoResolvePrefabs()
        {
#if UNITY_EDITOR
            if (_protagonistPrefab == null)
                _protagonistPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                    "Assets/_Project/Prefabs/Characters/Protagonist.prefab");

            if (_roomPrefab == null)
            {
                var guids = AssetDatabase.FindAssets("t:Prefab",
                    new[] { "Assets/_Project/Prefabs/Rooms" });
                foreach (var g in guids)
                {
                    var path = AssetDatabase.GUIDToAssetPath(g);
                    // Prefer entrance_hall as default test room
                    if (path.Contains("entrance_hall"))
                    {
                        _roomPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                        break;
                    }
                }
                if (_roomPrefab == null && guids.Length > 0)
                    _roomPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                        AssetDatabase.GUIDToAssetPath(guids[0]));
            }
#endif
        }

        private void SpawnRoom()
        {
            if (_roomPrefab == null) { Debug.LogWarning("[RoomWorkshopBootstrap] No room prefab found."); return; }
            var room = Instantiate(_roomPrefab, _roomPosition, Quaternion.identity);
            CloseAllSockets(room);
        }

        private static void CloseAllSockets(GameObject room)
        {
            var controller = room.GetComponent<RoomController>();
            if (controller == null) return;
            foreach (var socket in controller.Sockets)
                controller.CloseSocket(socket);
        }

        private GameObject SpawnProtagonist()
        {
            var existing = GameObject.FindWithTag("Player");
            if (existing != null) return existing;

            if (_protagonistPrefab == null) { Debug.LogWarning("[RoomWorkshopBootstrap] No protagonist prefab found."); return null; }
            return Instantiate(_protagonistPrefab, _protagonistSpawn, Quaternion.identity);
        }

        private static void WireCamera(GameObject player)
        {
            var cam = Camera.main;
            if (cam == null) return;
            var follow = cam.GetComponent<CameraFollow>() ?? cam.gameObject.AddComponent<CameraFollow>();
            if (player != null) follow.SetTarget(player.transform);
        }
    }
}
