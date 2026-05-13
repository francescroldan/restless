using UnityEngine;

namespace Restless.Dream.Procedural
{
    /// <summary>
    /// Lives on the root GO of every room prefab. Holds the semantic definition
    /// and provides runtime access to sockets and spawn bounds.
    ///
    /// Room prefab structure:
    ///   [RoomRoot]              ← this component
    ///   ├── Tilemaps/           ← floor, walls, props
    ///   ├── Sockets/            ← child GOs with DoorSocket components
    ///   ├── SpawnZone           ← empty GO whose bounds define where presences spawn
    ///   └── [other content]
    /// </summary>
    public class RoomController : MonoBehaviour
    {
        [SerializeField] private RoomDefinition _definition;
        [SerializeField] private Bounds         _spawnBounds;

        private DoorSocket[] _sockets;

        public RoomDefinition Definition  => _definition;
        public DoorSocket[]   Sockets     => _sockets ??= GetComponentsInChildren<DoorSocket>(true);
        public Bounds         SpawnBounds => new Bounds(transform.position + _spawnBounds.center, _spawnBounds.size);

        // Set by the assembler when this room is placed in the world
        [HideInInspector] public int GraphNodeIndex = -1;

        public DoorSocket GetFreeSocket(SocketDirection preferred)
        {
            foreach (var s in Sockets)
                if (!s.isOccupied && s.direction == preferred) return s;
            foreach (var s in Sockets)
                if (!s.isOccupied) return s;
            return null;
        }

        public DoorSocket GetFreeSocketFacing(SocketDirection incomingDirection)
        {
            // We want a socket that faces OPPOSITE to the incoming direction
            var needed = incomingDirection switch
            {
                SocketDirection.North => SocketDirection.South,
                SocketDirection.South => SocketDirection.North,
                SocketDirection.East  => SocketDirection.West,
                SocketDirection.West  => SocketDirection.East,
                _                     => SocketDirection.South
            };
            foreach (var s in Sockets)
                if (!s.isOccupied && s.direction == needed) return s;
            return null;
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0.4f, 0.8f, 1f, 0.2f);
            Gizmos.DrawCube(transform.position + _spawnBounds.center, _spawnBounds.size);
            Gizmos.color = new Color(0.4f, 0.8f, 1f, 0.6f);
            Gizmos.DrawWireCube(transform.position + _spawnBounds.center, _spawnBounds.size);
        }
#endif
    }
}
