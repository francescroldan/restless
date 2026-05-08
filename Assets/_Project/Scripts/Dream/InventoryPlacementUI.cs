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
        public bool IsOpen => _open;
        private MemoryFragment _pending;
        private int _rotation;
        private Vector2Int _cursor;
        private DreamInventory _inventory;

        private Texture2D _white;
        private GUIStyle _labelCenter;

        private readonly Color _colEmpty      = new Color(0.18f, 0.18f, 0.22f, 0.95f);
        private readonly Color _colOccupied   = new Color(0.45f, 0.38f, 0.20f, 0.95f); // amber-grey, distinct from empty
        private readonly Color _colPreviewOk  = new Color(0.25f, 0.85f, 0.55f, 0.80f);
        private readonly Color _colPreviewBad = new Color(0.9f,  0.25f, 0.25f, 0.80f);
        private readonly Color _colCursorBorder = new Color(1f, 1f, 1f, 0.55f);

        private float _inputCooldown;
        private int   _openFrame = -1;
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
            _pending   = fragment;
            _rotation  = 0;
            _cursor    = Vector2Int.zero;
            _openFrame = Time.frameCount;
            _open      = true;
            if (_inventory == null) _inventory = DreamInventory.Instance;
            Time.timeScale = 0f;
            ClampCursor(_pending.GetRotated(_rotation));
        }

        private void Update()
        {
            if (!_open) return;
            if (Time.frameCount == _openFrame) return; // skip frame that triggered the minigame success

            if (_inventory == null) _inventory = DreamInventory.Instance;
            if (_inventory == null) return;

            var kb = Keyboard.current;
            if (kb == null) return;

            _inputCooldown -= Time.unscaledDeltaTime;

            var cells = _pending.GetRotated(_rotation);

            var gp = Gamepad.current;

            // Rotate
            if (kb.rKey.wasPressedThisFrame ||
                (gp != null && gp.rightShoulder.wasPressedThisFrame))
            {
                _rotation = (_rotation + 1) % 4;
                cells = _pending.GetRotated(_rotation);
                ClampCursor(cells);
            }

            // Navigate
            if (_inputCooldown <= 0f)
            {
                int dx = 0, dy = 0;
                if (kb.rightArrowKey.isPressed || kb.dKey.isPressed) dx =  1;
                if (kb.leftArrowKey.isPressed  || kb.aKey.isPressed) dx = -1;
                if (kb.upArrowKey.isPressed    || kb.wKey.isPressed) dy = -1;
                if (kb.downArrowKey.isPressed  || kb.sKey.isPressed) dy =  1;

                if (gp != null)
                {
                    Vector2 stick = gp.leftStick.ReadValue();
                    if (gp.dpad.right.isPressed || stick.x >  0.5f) dx =  1;
                    if (gp.dpad.left.isPressed  || stick.x < -0.5f) dx = -1;
                    if (gp.dpad.up.isPressed    || stick.y >  0.5f) dy = -1;
                    if (gp.dpad.down.isPressed  || stick.y < -0.5f) dy =  1;
                }

                if (dx != 0 || dy != 0)
                {
                    int maxX, maxY;
                    GetPieceExtent(cells, out maxX, out maxY);
                    _cursor.x = Mathf.Clamp(_cursor.x + dx, 0, _inventory.Width  - 1 - maxX);
                    _cursor.y = Mathf.Clamp(_cursor.y + dy, 0, _inventory.Height - 1 - maxY);
                    _inputCooldown = INPUT_REPEAT;
                }
            }

            // Place
            if (kb.eKey.wasPressedThisFrame || kb.enterKey.wasPressedThisFrame ||
                kb.numpadEnterKey.wasPressedThisFrame ||
                (gp != null && gp.buttonSouth.wasPressedThisFrame))
                TryPlace();

            // Discard
            if (kb.escapeKey.wasPressedThisFrame ||
                (gp != null && gp.buttonEast.wasPressedThisFrame))
                Close(placed: false);
        }

        // ── Helpers ───────────────────────────────────────────────────────

        private void ClampCursor(Vector2Int[] cells)
        {
            int maxX, maxY;
            GetPieceExtent(cells, out maxX, out maxY);
            _cursor.x = Mathf.Clamp(_cursor.x, 0, Mathf.Max(0, _inventory.Width  - 1 - maxX));
            _cursor.y = Mathf.Clamp(_cursor.y, 0, Mathf.Max(0, _inventory.Height - 1 - maxY));
        }

        private static void GetPieceExtent(Vector2Int[] cells, out int maxX, out int maxY)
        {
            maxX = 0; maxY = 0;
            foreach (var c in cells)
            {
                if (c.x > maxX) maxX = c.x;
                if (c.y > maxY) maxY = c.y;
            }
        }

        private void TryPlace()
        {
            var cells = _pending.GetRotated(_rotation);
            if (CanPlace(cells, _cursor))
            {
                _inventory.PlaceFragment(_pending, cells, _cursor, _rotation);
                Close(placed: true);
            }
        }

        private bool CanPlace(Vector2Int[] cells, Vector2Int origin)
        {
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

        private void Close(bool placed)
        {
            _open = false;
            _pending = null;
            Time.timeScale = 1f;
        }

        // ── GUI ───────────────────────────────────────────────────────────

        private void OnGUI()
        {
            if (!_open || _pending == null || _inventory == null) return;

            EnsureStyles();

            float sw = Screen.width, sh = Screen.height;
            var cells = _pending.GetRotated(_rotation);
            bool previewOk = CanPlace(cells, _cursor);

            var previewSet = new System.Collections.Generic.HashSet<Vector2Int>();
            foreach (var c in cells)
                previewSet.Add(_cursor + c);

            // ── Panel background ──────────────────────────────────────────
            float step   = _cellSize + _cellGap;
            float gridW  = _inventory.Width  * step - _cellGap;
            float gridH  = _inventory.Height * step - _cellGap;
            float pad    = 24f;
            float panelW = gridW + pad * 2f;
            float panelH = gridH + pad * 2f + 80f;
            float panelX = (sw - panelW) * 0.5f;
            float panelY = (sh - panelH) * 0.5f;

            DrawRect(new Rect(panelX, panelY, panelW, panelH), new Color(0.08f, 0.08f, 0.1f, 0.97f));

            GUI.color = Color.white;
            GUI.Label(new Rect(panelX, panelY + 10f, panelW, 24f),
                "COLOCA EL FRAGMENTO — R girar  |  Escape descartar", _labelCenter);

            float gridX = panelX + pad;
            float gridY = panelY + pad + 36f;

            // ── Grid cells ────────────────────────────────────────────────
            for (int row = 0; row < _inventory.Height; row++)
            {
                for (int col = 0; col < _inventory.Width; col++)
                {
                    float cx = gridX + col * step;
                    float cy = gridY + row * step;
                    var cellRect  = new Rect(cx, cy, _cellSize, _cellSize);
                    var cellCoord = new Vector2Int(col, row);

                    Color cellColor;
                    if (previewSet.Contains(cellCoord))
                        cellColor = previewOk ? _colPreviewOk : _colPreviewBad;
                    else if (_inventory.IsCellOccupied(col, row))
                        cellColor = _colOccupied;
                    else
                        cellColor = _colEmpty;

                    DrawRect(cellRect, cellColor);
                }
            }

            // ── Cursor border (drawn after cells, no overlap bleed) ───────
            float cx0 = gridX + _cursor.x * step;
            float cy0 = gridY + _cursor.y * step;
            DrawBorder(new Rect(cx0, cy0, _cellSize, _cellSize), 2f, _colCursorBorder);

            // ── Fragment shape preview (top-right of panel) ───────────────
            float previewX = gridX + gridW + 16f;
            float previewY = gridY;
            GUI.color = Color.white;
            GUI.Label(new Rect(previewX - 40f, previewY - 24f, 100f, 20f), "Forma:", _labelCenter);
            float mini = 18f, miniGap = 2f;
            foreach (var c in cells)
            {
                float px = previewX + c.x * (mini + miniGap);
                float py = previewY + c.y * (mini + miniGap);
                DrawRect(new Rect(px, py, mini, mini), _colPreviewOk);
            }

            GUI.color = Color.white;
        }

        private void DrawRect(Rect rect, Color color)
        {
            GUI.color = color;
            GUI.DrawTexture(rect, _white);
        }

        private void DrawBorder(Rect rect, float thickness, Color color)
        {
            GUI.color = color;
            GUI.DrawTexture(new Rect(rect.x,                         rect.y,                          rect.width, thickness), _white);
            GUI.DrawTexture(new Rect(rect.x,                         rect.y + rect.height - thickness, rect.width, thickness), _white);
            GUI.DrawTexture(new Rect(rect.x,                         rect.y,                          thickness, rect.height), _white);
            GUI.DrawTexture(new Rect(rect.x + rect.width - thickness, rect.y,                          thickness, rect.height), _white);
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
