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
        private Vector2        _spawnPosA;
        private Vector2        _spawnPosB;

        // Lying connection — set by RoomAssembler after layout is finalised
        private RoomController _lyingRoomA;
        private RoomController _lyingRoomB;
        private Vector2        _lyingSpawnA;
        private Vector2        _lyingSpawnB;

        private int _crossingCount;

        public void Init(RoomController roomA, RoomController roomB,
                         Vector2 size, Vector2 spawnPosA, Vector2 spawnPosB)
        {
            _roomA = roomA; _roomB = roomB;
            _spawnPosA = spawnPosA; _spawnPosB = spawnPosB;

            var col = GetComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = size;
        }

        /// <summary>
        /// Marks this trigger as a lying connection.
        /// On the second+ crossing A→B the player lands in lyingRoomB, and vice-versa.
        /// </summary>
        public void SetLying(RoomController lyingRoomA, Vector2 lyingSpawnA,
                             RoomController lyingRoomB, Vector2 lyingSpawnB)
        {
            _lyingRoomA  = lyingRoomA;  _lyingSpawnA = lyingSpawnA;
            _lyingRoomB  = lyingRoomB;  _lyingSpawnB = lyingSpawnB;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;

            var active    = RoomCamera.ActiveRoom;
            bool goingToB = (active == _roomA);
            _crossingCount++;

            bool lie = _crossingCount > 1 && (_lyingRoomA != null || _lyingRoomB != null);

            RoomController target;
            Vector2        spawnPos;
            if (lie)
            {
                target   = goingToB ? _lyingRoomB  : _lyingRoomA;
                spawnPos = goingToB ? _lyingSpawnB  : _lyingSpawnA;
            }
            else
            {
                target   = goingToB ? _roomB     : _roomA;
                spawnPos = goingToB ? _spawnPosB : _spawnPosA;
            }

            if (target != null)
                RoomEnterTrigger.Notify(target, spawnPos);
        }
    }
}
