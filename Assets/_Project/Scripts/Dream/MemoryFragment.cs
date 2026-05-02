using UnityEngine;

namespace Restless.Dream
{
    [CreateAssetMenu(menuName = "Restless/Memory Fragment")]
    public class MemoryFragment : ScriptableObject
    {
        public string fragmentId;
        [TextArea] public string description;

        // Grid shape: list of (col, row) offsets relative to pivot cell
        public Vector2Int[] cells = { new(0, 0) };

        public int Width
        {
            get
            {
                int max = 0;
                foreach (var c in cells) max = Mathf.Max(max, c.x);
                return max + 1;
            }
        }

        public int Height
        {
            get
            {
                int max = 0;
                foreach (var c in cells) max = Mathf.Max(max, c.y);
                return max + 1;
            }
        }

        /// <summary>Returns cells rotated clockwise, always normalized so min x=0, min y=0.</summary>
        public Vector2Int[] GetRotated(int rotations)
        {
            Vector2Int[] current = cells;
            for (int r = 0; r < rotations % 4; r++)
                current = Rotate90(current);
            return current;
        }

        private static Vector2Int[] Rotate90(Vector2Int[] input)
        {
            int maxY = 0;
            foreach (var c in input) maxY = Mathf.Max(maxY, c.y);

            var result = new Vector2Int[input.Length];
            for (int i = 0; i < input.Length; i++)
                result[i] = new Vector2Int(maxY - input[i].y, input[i].x);

            // Normalize so min x and min y are always 0
            int minX = int.MaxValue, minY = int.MaxValue;
            foreach (var c in result) { minX = Mathf.Min(minX, c.x); minY = Mathf.Min(minY, c.y); }
            for (int i = 0; i < result.Length; i++)
                result[i] = new Vector2Int(result[i].x - minX, result[i].y - minY);

            return result;
        }
    }
}
