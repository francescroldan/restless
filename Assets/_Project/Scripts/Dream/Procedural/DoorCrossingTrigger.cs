using UnityEngine;

namespace Restless.Dream.Procedural
{
    /// <summary>
    /// Trigger that fires a room transition when the player enters.
    ///
    /// Corridors: one instance at the midpoint of the gap between two rooms.
    /// Doors:     two instances, each placed ~0.5 u inside their respective room.
    ///            The wall physically blocks the player from reaching the trigger
    ///            on the other side, so direction is always correct.
    /// </summary>
    [RequireComponent(typeof(BoxCollider2D))]
    public class DoorCrossingTrigger : MonoBehaviour
    {
        private RoomController _roomA;
        private RoomController _roomB;
        private Vector2        _spawnPosA;   // where to spawn when entering roomA
        private Vector2        _spawnPosB;   // where to spawn when entering roomB

        public void Init(RoomController roomA, RoomController roomB,
                         Vector2 size, Vector2 spawnPosA, Vector2 spawnPosB)
        {
            _roomA = roomA; _roomB = roomB;
            _spawnPosA = spawnPosA; _spawnPosB = spawnPosB;

            var col = GetComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = size;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;

            var active = RoomCamera.ActiveRoom;
            bool goingToB = (active == _roomA);
            var target   = goingToB ? _roomB     : _roomA;
            var spawnPos = goingToB ? _spawnPosB : _spawnPosA;
            if (target != null)
                RoomEnterTrigger.Notify(target, spawnPos);
        }
    }
}
