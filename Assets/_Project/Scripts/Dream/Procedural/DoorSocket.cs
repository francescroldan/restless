using UnityEngine;

namespace Restless.Dream.Procedural
{
    public enum SocketDirection { North, South, East, West }

    /// <summary>
    /// Marks a connection point on a room prefab. The assembler aligns matching
    /// sockets from adjacent rooms to stitch them together.
    ///
    /// Place one DoorSocket child GO per opening in the room, positioned at the
    /// centre of the doorway, facing outward (away from the room interior).
    /// </summary>
    public class DoorSocket : MonoBehaviour
    {
        [Tooltip("Cardinal direction this opening faces (outward from the room).")]
        public SocketDirection direction;

        [Tooltip("Width of this opening in world units. Only sockets with matching width connect.")]
        public float width = 2f;

        [Tooltip("Optional: restrict which room types can connect through this socket. Empty = any.")]
        public RoomType[] compatibleTypes;

        [HideInInspector]
        public bool isOccupied;

        [HideInInspector]
        public DoorSocket connectedSocket;

        public bool CanConnectTo(DoorSocket other)
        {
            if (Mathf.Abs(width - other.width) > 0.1f) return false;
            if (!IsFacing(other)) return false;
            if (compatibleTypes != null && compatibleTypes.Length > 0 &&
                other.GetComponentInParent<RoomController>() is RoomController rc)
            {
                bool anyMatch = false;
                foreach (var t in compatibleTypes)
                    if (rc.Definition.HasType(t)) { anyMatch = true; break; }
                if (!anyMatch) return false;
            }
            return true;
        }

        // Returns true if this socket faces the opposite direction to other
        private bool IsFacing(DoorSocket other)
        {
            return (direction == SocketDirection.North && other.direction == SocketDirection.South)
                || (direction == SocketDirection.South && other.direction == SocketDirection.North)
                || (direction == SocketDirection.East  && other.direction == SocketDirection.West)
                || (direction == SocketDirection.West  && other.direction == SocketDirection.East);
        }

        public Vector2 OutwardNormal()
        {
            return direction switch
            {
                SocketDirection.North => Vector2.up,
                SocketDirection.South => Vector2.down,
                SocketDirection.East  => Vector2.right,
                SocketDirection.West  => Vector2.left,
                _                     => Vector2.up
            };
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            var color = isOccupied ? new Color(0.3f, 1f, 0.3f, 0.8f) : new Color(1f, 0.8f, 0.2f, 0.8f);
            Gizmos.color = color;
            Gizmos.DrawWireCube(transform.position, new Vector3(width, 0.15f, 0.1f));
            Gizmos.DrawRay(transform.position, (Vector3)OutwardNormal() * 0.6f);
        }
#endif
    }
}
