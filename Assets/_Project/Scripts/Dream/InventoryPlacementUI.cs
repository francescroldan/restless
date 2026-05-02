using UnityEngine;
using UnityEngine.InputSystem;

namespace Restless.Dream
{
    /// <summary>
    /// Shown after a successful extraction. Player places the fragment in the grid manually.
    /// Controls: arrow keys / WASD to move cursor, R to rotate, E/Enter to place, Escape to discard.
    /// Reads Keyboard directly so it works regardless of the active PlayerInput action map.
    /// </summary>
    public class InventoryPlacementUI : MonoBehaviour
    {
        public static InventoryPlacementUI Instance { get; private set; }

        [SerializeField] private float _cellSize = 48f;
        [SerializeField] private float _cellGap  = 4f;

        private bool _open;
        private MemoryFragment _pending;
        private int _rotation;
        private Vector2Int _cursor;
        private DreamInventory _inventory;

        private Texture2D _white;
        private GUIStyle _labelCenter;

        private readonly Color _colEmpty     = new Color(0.18f, 0.18f, 0.22f, 0.95f);
        private readonly Color _colOccupied  = new Color(0.35f, 0.35f, 0.40f, 0.95f);
        private readonly Color _colPreviewOk = new Color(0.25f, 0.85f, 0.55f, 0.75f);
        private readonly Color _colPreviewBad= new Color(0.9f,  0.25f, 0.25f, 0.75f);
        private readonly Color _colCursor    = new Color(1f,    1f,    1f,    0.25f);

        private float _inputCooldown;
        private const float INPUT_REPEAT = 0.18f;

        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;

            _white = new Texture2D(1, 1);
            _white.SetPixel(0, 0, Color.white);
            _white.Apply();
        }

        private void Start()
        {
            _inventory = DreamInventory.Instance;
        }

        /// <summary>Opens the placement screen for a newly extracted fragment.</summary>
        public void Open(MemoryFragment fragment)
        {
            _pending  = fragment;
            _rotation = 0;
            _cursor   = Vector2Int.zero;
            _open     = true;
            Time.timeScale = 0f;
        }

        private void Update()
        {
            if (!_open) return;

            var kb = Keyboard.current;
            if (kb == null) return;

            _inputCooldown -= Time.unscaledDeltaTime;

            // Navigate — arrows or WASD
            if (_inputCooldown <= 0f)
            {
                int dx = 0, dy = 0;
                if (kb.rightArrowKey.isPressed || kb.dKey.isPressed) dx =  1;
                if (kb.leftArrowKey.isPressed  || kb.aKey.isPressed) dx = -1;
                if (kb.upArrowKey.isPressed    || kb.wKey.isPressed) dy = -1;
                if (kb.downArrowKey.isPressed  || kb.sKey.isPressed) dy =  1;

                if (dx != 0 || dy != 0)
                {
                    _cursor.x = Mathf.Clamp(_cursor.x + dx, 0, _inventory.Width  - 1);
                    _cursor.y = Mathf.Clamp(_cursor.y + dy, 0, _inventory.Height - 1);
                    _inputCooldown = INPUT_REPEAT;
                }
            }

            // Rotate
            if (kb.rKey.wasPressedThisFrame)
                _rotation = (_rotation + 1) % 4;

            // Place
            if (kb.eKey.wasPressedThisFrame || kb.enterKey.wasPressedThisFrame ||
                kb.numpadEnterKey.wasPressedThisFrame)
                TryPlace();

            // Discard
            if (kb.escapeKey.wasPressedThisFrame)
                Close(placed: false);
        }

        private void TryPlace()
        {
            var cells = _pending.GetRotated(_rotation);
            if (CanPlace(cells, _cursor))
            {
                PlaceAt(cells, _cursor);
                Close(placed: true);
            }
        }

        private bool CanPlace(Vector2Int[] cells, Vector2Int origin)
        {
            // Access DreamInventory grid state via a public helper
            foreach (var c in cells)
            {
                int x = origin.x + c.x;
                int y = origin.y + c.y;
                if (x < 0 || x >= _inventory.Width)  return false;
                if (y < 0 || y >= _inventory.Height) return false;
                if (_inventory.IsCellOccupied(x, y)) return false;
            }
            return true;
        }

        private void PlaceAt(Vector2Int[] cells, Vector2Int origin)
        {
            _inventory.PlaceFragment(_pending, cells, origin, _rotation);
        }

        private void Close(bool placed)
        {
            _open = false;
            Time.timeScale = 1f;
            if (!placed)
                Debug.Log("[InventoryPlacementUI] Fragment discarded.");
        }

        private void OnGUI()
        {
            if (!_open || _pending == null || _inventory == null) return;

            EnsureStyles();

            float sw = Screen.width, sh = Screen.height;
            var cells = _pending.GetRotated(_rotation);

            // ── Grid panel ───────────────────────────────────────────────
            float step  = _cellSize + _cellGap;
            float gridW = _inventory.Width  * step - _cellGap;
            float gridH = _inventory.Height * step - _cellGap;

            float panelPad = 24f;
            float panelW = gridW + panelPad * 2f;
            float panelH = gridH + panelPad * 2f + 80f;
            float panelX = (sw - panelW) * 0.5f;
            float panelY = (sh - panelH) * 0.5f;

            DrawRect(new Rect(panelX, panelY, panelW, panelH), new Color(0.08f, 0.08f, 0.1f, 0.97f));

            GUI.color = Color.white;
            GUI.Label(new Rect(panelX, panelY + 10f, panelW, 24f),
                $"COLOCA EL FRAGMENTO — R para rotar  |  Escape para descartar", _labelCenter);

            float gridX = panelX + panelPad;
            float gridY = panelY + panelPad + 36f;

            // Check if preview is valid at cursor
            bool previewOk = CanPlace(cells, _cursor);

            // Build set of preview cells
            var previewSet = new System.Collections.Generic.HashSet<Vector2Int>();
            foreach (var c in cells)
                previewSet.Add(_cursor + c);

            // Draw grid cells
            for (int row = 0; row < _inventory.Height; row++)
            {
                for (int col = 0; col < _inventory.Width; col++)
                {
                    float cx = gridX + col * step;
                    float cy = gridY + row * step;
                    var cellRect = new Rect(cx, cy, _cellSize, _cellSize);
                    var cellCoord = new Vector2Int(col, row);

                    Color cellColor;
                    if (previewSet.Contains(cellCoord))
                        cellColor = previewOk ? _colPreviewOk : _colPreviewBad;
                    else if (_inventory.IsCellOccupied(col, row))
                        cellColor = _colOccupied;
                    else
                        cellColor = _colEmpty;

                    DrawRect(cellRect, cellColor);

                    // Cursor highlight
                    if (cellCoord == _cursor)
                        DrawRect(new Rect(cx - 2f, cy - 2f, _cellSize + 4f, _cellSize + 4f), _colCursor);
                }
            }

            // Fragment shape preview (top-right corner)
            float previewX = gridX + gridW + 16f;
            float previewY = gridY;
            GUI.color = Color.white;
            GUI.Label(new Rect(previewX - 40f, previewY - 24f, 100f, 20f), "Forma:", _labelCenter);
            float miniCell = 18f;
            foreach (var c in cells)
            {
                float px = previewX + c.x * (miniCell + 2f);
                float py = previewY + c.y * (miniCell + 2f);
                DrawRect(new Rect(px, py, miniCell, miniCell), _colPreviewOk);
            }

            GUI.color = Color.white;
        }

        private void DrawRect(Rect rect, Color color)
        {
            GUI.color = color;
            GUI.DrawTexture(rect, _white);
        }

        private void EnsureStyles()
        {
            if (_labelCenter != null) return;
            _labelCenter = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize  = 12,
                fontStyle = FontStyle.Bold
            };
            _labelCenter.normal.textColor = Color.white;
        }
    }
}
