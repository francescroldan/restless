using System;
using UnityEngine;

namespace Restless.Dream.Procedural
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class RoomEnterTrigger : MonoBehaviour
    {
        public static event Action<RoomController, Vector2> PlayerEnteredRoom;

        private RoomController _room;

        public void Init(RoomController room, Vector2 triggerSize)
        {
            _room = room;
            var col = GetComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = triggerSize;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
                Notify(_room, Vector2.zero);
        }

        public static void Notify(RoomController room, Vector2 spawnPos) =>
            PlayerEnteredRoom?.Invoke(room, spawnPos);
    }
}
