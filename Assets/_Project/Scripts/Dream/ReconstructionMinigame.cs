using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Restless.Dream
{
    /// <summary>
    /// Variant B: Navigate scrambled pieces to their correct slots within a time limit.
    /// Uses UI/Navigate and UI/Submit actions from the UI action map.
    /// </summary>
    public class ReconstructionMinigame : MonoBehaviour, IExtractionMinigame
    {
        [SerializeField] private int _pieceCount = 5;
        [SerializeField] private float _timeLimit = 20f;

        private Action _onSuccess;
        private Action _onFailure;
        private bool _isActive;
        private float _timeRemaining;
        private int[] _pieceSlots;   // current slot index each piece occupies
        private int[] _targetSlots;  // correct slot index for each piece
        private int _selectedPiece;
        private PlayerInput _playerInput;

        public bool IsActive => _isActive;
        public float TimeRemaining => _timeRemaining;
        public float NormalizedTime => _timeLimit > 0 ? _timeRemaining / _timeLimit : 0f;
        public int SelectedPiece => _selectedPiece;
        public int PieceCount => _pieceCount;
        public int[] PieceSlots => _pieceSlots;
        public int[] TargetSlots => _targetSlots;

        private void Awake()
        {
            var protagonistGO = GameObject.FindWithTag("Player");
            if (protagonistGO != null)
                _playerInput = protagonistGO.GetComponent<PlayerInput>();
        }

        public void Begin(Action onSuccess, Action onFailure)
        {
            _onSuccess = onSuccess;
            _onFailure = onFailure;
            _timeRemaining = _timeLimit;
            _selectedPiece = 0;

            _pieceSlots = new int[_pieceCount];
            _targetSlots = new int[_pieceCount];

            for (int i = 0; i < _pieceCount; i++)
                _targetSlots[i] = i;

            // Scramble: assign each piece a random slot (Fisher-Yates)
            for (int i = 0; i < _pieceCount; i++)
                _pieceSlots[i] = i;

            for (int i = _pieceCount - 1; i > 0; i--)
            {
                int j = UnityEngine.Random.Range(0, i + 1);
                (_pieceSlots[i], _pieceSlots[j]) = (_pieceSlots[j], _pieceSlots[i]);
            }

            _isActive = true;
        }

        public void Cancel()
        {
            _isActive = false;
        }

        private void Update()
        {
            if (!_isActive) return;

            _timeRemaining -= Time.deltaTime;
            if (_timeRemaining <= 0f)
            {
                _timeRemaining = 0f;
                _isActive = false;
                _onFailure?.Invoke();
                return;
            }

            if (_playerInput == null) return;

            var navigate = _playerInput.actions["UI/Navigate"];
            var submit = _playerInput.actions["UI/Submit"];

            if (navigate.WasPressedThisFrame())
            {
                Vector2 dir = navigate.ReadValue<Vector2>();
                if (dir.x > 0.5f) _selectedPiece = (_selectedPiece + 1) % _pieceCount;
                else if (dir.x < -0.5f) _selectedPiece = (_selectedPiece - 1 + _pieceCount) % _pieceCount;
            }

            if (submit.WasPressedThisFrame())
            {
                // Swap current piece toward its target slot
                int target = _targetSlots[_selectedPiece];
                if (_pieceSlots[_selectedPiece] != target)
                {
                    for (int i = 0; i < _pieceCount; i++)
                    {
                        if (_pieceSlots[i] == target)
                        {
                            _pieceSlots[i] = _pieceSlots[_selectedPiece];
                            _pieceSlots[_selectedPiece] = target;
                            DreamSFXPlayer.Instance?.PlayMinigameHit();
                            break;
                        }
                    }
                }
                else
                {
                    DreamSFXPlayer.Instance?.PlayMinigameMiss();
                }

                if (CheckSolved())
                {
                    _isActive = false;
                    _onSuccess?.Invoke();
                }
            }
        }

        private bool CheckSolved()
        {
            for (int i = 0; i < _pieceCount; i++)
                if (_pieceSlots[i] != _targetSlots[i]) return false;
            return true;
        }
    }
}
