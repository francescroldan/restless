using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Restless.Core;

namespace Restless.Dream.Procedural
{
    /// <summary>
    /// Room-based camera for the dream scene. Only the active room is visible;
    /// entering a new room triggers a black fade, camera snap, and fade back in.
    /// Added automatically to Camera.main by DreamSceneBootstrap.
    /// Disables CameraFollow if present on the same GO.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class RoomCamera : MonoBehaviour
    {
        [SerializeField] private float _fadeDuration = 0.15f;

        public static RoomController ActiveRoom { get; private set; }

        private IReadOnlyList<RoomController> _rooms;
        private RoomController _activeRoom;
        private bool   _transitioning;
        private float  _fadeAlpha;
        private Vector2 _pendingSpawnPos;
        private Texture2D _black;

        private void Awake()
        {
            var follow = GetComponent<CameraFollow>();
            if (follow != null) follow.enabled = false;

            _black = new Texture2D(1, 1);
            _black.SetPixel(0, 0, Color.black);
            _black.Apply();
        }

        private void OnEnable()  => RoomEnterTrigger.PlayerEnteredRoom += OnRoomEntered;
        private void OnDisable() => RoomEnterTrigger.PlayerEnteredRoom -= OnRoomEntered;

        public void Init(IReadOnlyList<RoomController> rooms, RoomController startRoom)
        {
            _rooms = rooms;
            ActivateRoom(startRoom);
        }

        private void OnRoomEntered(RoomController room, Vector2 spawnPos)
        {
            if (_transitioning || room == _activeRoom) return;
            _pendingSpawnPos = spawnPos;
            StartCoroutine(TransitionTo(room));
        }

        private IEnumerator TransitionTo(RoomController room)
        {
            _transitioning = true;
            yield return FadeTo(1f);
            ActivateRoom(room);
            TeleportPlayer(_pendingSpawnPos);   // teleport while screen is black
            yield return FadeTo(0f);
            _transitioning = false;
        }

        private IEnumerator FadeTo(float target)
        {
            float start   = _fadeAlpha;
            float elapsed = 0f;
            while (elapsed < _fadeDuration)
            {
                elapsed += Time.deltaTime;
                _fadeAlpha = Mathf.Lerp(start, target, elapsed / _fadeDuration);
                yield return null;
            }
            _fadeAlpha = target;
        }

        private void ActivateRoom(RoomController room)
        {
            _activeRoom = room;
            ActiveRoom  = room;
            var p = room.transform.position;
            transform.position = new Vector3(p.x, p.y, transform.position.z);
            if (_rooms != null)
                foreach (var r in _rooms)
                    SetRoomVisible(r, r == room);
        }

        private static void TeleportPlayer(Vector2 pos)
        {
            if (pos == Vector2.zero) return;
            var player = GameObject.FindWithTag("Player");
            if (player == null) return;
            var rb = player.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.position = pos;
            }
            else
            {
                player.transform.position = pos;
            }
        }

        private static void SetRoomVisible(RoomController room, bool visible)
        {
            foreach (var r in room.GetComponentsInChildren<Renderer>(true))
                r.enabled = visible;
        }

        private void OnGUI()
        {
            if (_fadeAlpha <= 0f || _black == null) return;
            GUI.color = new Color(0f, 0f, 0f, _fadeAlpha);
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), _black);
            GUI.color = Color.white;
        }
    }
}
