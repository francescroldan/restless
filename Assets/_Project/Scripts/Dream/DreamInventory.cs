using System.Collections.Generic;
using UnityEngine;
using Restless.Core;

namespace Restless.Dream
{
    /// <summary>
    /// Tetris-style inventory grid. Fragments occupy shaped cells; placement uses the first
    /// fitting position found (top-left scan). Rotation is tracked per placed fragment.
    /// </summary>
    public class DreamInventory : MonoBehaviour
    {
        public static DreamInventory Instance { get; private set; }

        [SerializeField] private GameEvent _onInventoryChanged;

        private int _width;
        private int _height;
        private bool[,] _occupied;

        public int Width => _width;
        public int Height => _height;

        public readonly List<PlacedFragment> PlacedFragments = new();

        public struct PlacedFragment
        {
            public MemoryFragment Fragment;
            public Vector2Int Origin;
            public int Rotations;
            public Vector2Int[] OccupiedCells; // world-grid positions
        }

        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
        }

        public void Initialize(int width, int height)
        {
            _width = width;
            _height = height;
            _occupied = new bool[width, height];
            PlacedFragments.Clear();
        }

        /// <summary>
        /// Tries to place fragment in the first available position (0 rotations).
        /// Returns true if placed successfully.
        /// </summary>
        public bool TryAddFragment(MemoryFragment fragment)
        {
            if (fragment == null) return false;

            for (int rotations = 0; rotations < 4; rotations++)
            {
                var cells = fragment.GetRotated(rotations);
                if (TryPlaceWithRotation(fragment, cells, rotations))
                    return true;
            }

            Debug.Log($"[DreamInventory] No room for fragment {fragment.fragmentId}");
            return false;
        }

        private bool TryPlaceWithRotation(MemoryFragment fragment, Vector2Int[] cells, int rotations)
        {
            for (int row = 0; row < _height; row++)
            {
                for (int col = 0; col < _width; col++)
                {
                    var origin = new Vector2Int(col, row);
                    if (CanPlace(cells, origin))
                    {
                        Place(fragment, cells, origin, rotations);
                        return true;
                    }
                }
            }
            return false;
        }

        private bool CanPlace(Vector2Int[] cells, Vector2Int origin)
        {
            foreach (var c in cells)
            {
                int x = origin.x + c.x;
                int y = origin.y + c.y;
                if (x < 0 || x >= _width || y < 0 || y >= _height) return false;
                if (_occupied[x, y]) return false;
            }
            return true;
        }

        private void Place(MemoryFragment fragment, Vector2Int[] cells, Vector2Int origin, int rotations)
        {
            var worldCells = new Vector2Int[cells.Length];
            for (int i = 0; i < cells.Length; i++)
            {
                var pos = origin + cells[i];
                _occupied[pos.x, pos.y] = true;
                worldCells[i] = pos;
            }

            PlacedFragments.Add(new PlacedFragment
            {
                Fragment = fragment,
                Origin = origin,
                Rotations = rotations,
                OccupiedCells = worldCells
            });

            _onInventoryChanged?.Raise();
            Debug.Log($"[DreamInventory] Placed {fragment.fragmentId} at {origin} (rot {rotations * 90}°)");
        }

        public bool IsFull()
        {
            for (int y = 0; y < _height; y++)
                for (int x = 0; x < _width; x++)
                    if (!_occupied[x, y]) return false;
            return true;
        }

        public void Clear()
        {
            if (_occupied != null)
                System.Array.Clear(_occupied, 0, _occupied.Length);
            PlacedFragments.Clear();
            _onInventoryChanged?.Raise();
        }
    }
}
