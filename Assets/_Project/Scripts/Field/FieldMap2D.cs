using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Amane.Core.Tween;

namespace Amane.Field
{
    /// <summary>
    /// 2Dフィールドマップ。PrototypeBootstrapから呼ばれ、
    /// 街の各ロケーションとプレイヤーキャラを管理する。
    /// プレイヤーが各場所に近づくとプロンプトが表示され、Spaceでインタラクト。
    /// </summary>
    public sealed class FieldMap2D : MonoBehaviour
    {
        [SerializeField] private RectTransform _mapArea;
        [SerializeField] private FieldPlayer2D _player;
        [SerializeField] private Text _locationNameText;

        private readonly List<FieldLocation> _locations = new();
        private FieldLocation _currentNearby;

        public FieldPlayer2D Player => _player;
        public event System.Action<FieldLocation> OnLocationInteracted;

        public void RegisterLocation(FieldLocation loc)
        {
            _locations.Add(loc);
            loc.OnInteracted += HandleInteract;
        }

        private void Update()
        {
            if (_player == null) return;
            var playerPos = _player.Position;

            // 最寄りのロケーションを検出
            FieldLocation nearest = null;
            float nearestDist = float.MaxValue;

            foreach (var loc in _locations)
            {
                float dist = Vector2.Distance(playerPos, loc.Position);
                if (dist < loc.InteractRadius && dist < nearestDist)
                {
                    nearest = loc;
                    nearestDist = dist;
                }
            }

            // 近接状態の更新
            if (_currentNearby != nearest)
            {
                _currentNearby?.SetPlayerNearby(false);
                nearest?.SetPlayerNearby(true);
                _currentNearby = nearest;

                // ロケーション名表示
                if (_locationNameText != null)
                    _locationNameText.text = nearest != null ? nearest.DisplayName : "";
            }
        }

        private void HandleInteract(FieldLocation loc)
        {
            OnLocationInteracted?.Invoke(loc);
        }

        /// <summary>プレイヤーのインタラクト入力を処理。</summary>
        public void ProcessInteract(Vector2 playerPos)
        {
            if (_currentNearby != null)
                _currentNearby.TriggerInteract();
        }
    }
}
