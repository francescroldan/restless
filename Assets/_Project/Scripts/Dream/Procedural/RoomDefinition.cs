using UnityEngine;

namespace Restless.Dream.Procedural
{
    public enum RoomSize { Small, Medium, Large, Landmark }

    public enum RoomType
    {
        Corridor, Puzzle, Safe, Ritual, Encounter,
        Traversal, Collapse, Memory, DeadEnd, Landmark
    }

    /// <summary>
    /// ScriptableObject that describes a room's semantic identity.
    /// Attached to every room prefab via RoomController.
    /// The assembler uses this data — not geometry — to decide what connects where.
    /// </summary>
    [CreateAssetMenu(menuName = "Restless/Room Definition")]
    public class RoomDefinition : ScriptableObject
    {
        [Header("Identity")]
        public string id;
        public RoomSize size;
        public RoomType[] types;

        [Header("Semantic tags")]
        [Tooltip("Emotional / atmospheric tags used by the director for pacing.")]
        public string[] tags;

        [Header("Gameplay support")]
        [Tooltip("Presences of threat type can spawn here.")]
        public bool supportsThreats   = true;
        [Tooltip("Memory fragments can spawn here.")]
        public bool supportsFragments = false;
        [Tooltip("Ally encounters can spawn here.")]
        public bool supportsAllies    = false;

        [Header("Socket configuration")]
        [Tooltip("Which door directions this room supports. Leave empty for all four (default).")]
        public SocketDirection[] socketDirections;

        [Header("Intensity")]
        [Range(0f, 1f)]
        [Tooltip("How dangerous this room feels. Scales presence density.")]
        public float dangerLevel  = 0.3f;
        [Range(0f, 1f)]
        [Tooltip("How dreamlike / spatially unstable this room is.")]
        public float surrealism   = 0.2f;

        public bool HasType(RoomType type)
        {
            foreach (var t in types)
                if (t == type) return true;
            return false;
        }

        public bool HasTag(string tag)
        {
            foreach (var t in tags)
                if (t == tag) return true;
            return false;
        }
    }
}
