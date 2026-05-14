using UnityEngine;
using UnityEngine.Tilemaps;

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

        // Removes the wall tiles at this socket's door slot so the player can pass through.
        public void OpenSocket(DoorSocket socket)
        {
            Tilemap cliff = null;
            foreach (var tm in GetComponentsInChildren<Tilemap>())
                if (tm.gameObject.name == "Tilemap_Cliff") { cliff = tm; break; }
            if (cliff == null)
            {
                Debug.LogWarning($"[RoomController] OpenSocket: Tilemap_Cliff not found on {gameObject.name}");
                return;
            }

            bool       horizontal = socket.direction == SocketDirection.North || socket.direction == SocketDirection.South;
            Vector3Int step       = horizontal ? Vector3Int.right : Vector3Int.up;
            Vector3Int baseCell   = cliff.WorldToCell(socket.transform.position);
            Vector3Int cell2      = baseCell + step;

            cliff.SetTile(baseCell, null);
            cliff.SetTile(cell2,    null);
            cliff.RefreshTile(baseCell);
            cliff.RefreshTile(cell2);
        }

        // Replaces the inner-corner frame tiles beside an unused socket with the straight
        // wall tile. If closedDoorTile is provided, also places a closed-door visual on
        // the door cells so the socket is recognisable as a locked door.
        public void CloseSocket(DoorSocket socket, TileBase closedDoorTile = null)
        {
            Tilemap cliff = null;
            foreach (var tm in GetComponentsInChildren<Tilemap>())
                if (tm.gameObject.name == "Tilemap_Cliff") { cliff = tm; break; }
            if (cliff == null) return;

            bool       horizontal = socket.direction == SocketDirection.North || socket.direction == SocketDirection.South;
            Vector3Int step       = horizontal ? Vector3Int.right : Vector3Int.up;
            Vector3Int baseCell   = cliff.WorldToCell(socket.transform.position);
            Vector3Int cell2      = baseCell + step;

            var wallTile = cliff.GetTile(baseCell);
            if (wallTile == null) wallTile = cliff.GetTile(cell2);

            if (closedDoorTile != null)
            {
                cliff.SetTile(baseCell, closedDoorTile);
                cliff.SetTile(cell2,    closedDoorTile);
                cliff.RefreshTile(baseCell);
                cliff.RefreshTile(cell2);
            }

            var cornerA = baseCell - step;
            var cornerB = cell2    + step;

            cliff.SetTile(cornerA, wallTile);
            cliff.SetTile(cornerB, wallTile);
            cliff.RefreshTile(cornerA);
            cliff.RefreshTile(cornerB);
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
