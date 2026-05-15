using DG.Tweening;
using Restless.Core;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Restless.Dream.Procedural
{
    /// <summary>
    /// Added at runtime by RoomAssembler to every placed room.
    /// Tracks player visits and applies a subtle visual mutation on the second entry.
    /// </summary>
    public class RoomMutator : MonoBehaviour
    {
        private RoomController _room;
        private Tilemap        _floor;
        private int            _visitCount;
        private Tween          _tween;

        public int VisitCount => _visitCount;

        public void Init(RoomController room)
        {
            _room  = room;
            _floor = FindTilemap("Tilemap_Floor");
        }

        private void OnEnable()  => RoomEnterTrigger.PlayerEnteredRoom += OnRoomEntered;
        private void OnDisable() => RoomEnterTrigger.PlayerEnteredRoom -= OnRoomEntered;
        private void OnDestroy() => _tween?.Kill();

        private void OnRoomEntered(RoomController room, Vector2 _)
        {
            if (room != _room) return;
            _visitCount++;
            if (_visitCount == 2) TryMutate();
        }

        private void TryMutate()
        {
            var cfg      = RunConfig.Current;
            float prob   = cfg?.roomMutationProbability ?? 0.7f;
            if (Random.value > prob) return;

            if (_floor == null) return;

            Color.RGBToHSV(_floor.color, out float h, out float s, out float v);
            float hueShift  = cfg?.mutationHueShift  ?? 0.10f;
            float valueMult = cfg?.mutationValueMult  ?? 0.70f;
            float duration  = cfg?.mutationFadeDuration ?? 2.5f;

            h = (h + hueShift) % 1f;
            Color target = Color.HSVToRGB(h, s, v * valueMult);

            _tween = DOTween.To(
                () => _floor.color,
                c  => _floor.color = c,
                target,
                duration
            ).SetEase(Ease.InOutSine);
        }

        private Tilemap FindTilemap(string tilemapName)
        {
            foreach (var tm in _room.GetComponentsInChildren<Tilemap>(true))
                if (tm.gameObject.name == tilemapName) return tm;
            return null;
        }
    }
}
