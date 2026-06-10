using UnityEngine;
using UnityEngine.UI;
using Amane.Core.Tween;

namespace Amane.Field
{
    /// <summary>
    /// 2Dフィールド移動キャラクター。
    /// WASD/矢印キーで移動、Spaceでインタラクト。
    /// ペルソナ5のフィールド探索を2Dトップダウンで再現。
    /// </summary>
    public sealed class FieldPlayer2D : MonoBehaviour
    {
        [SerializeField] private RectTransform _playerRect;
        [SerializeField] private Image _playerImage;
        [SerializeField] private Text _playerLabel;
        [SerializeField] private float _moveSpeed = 200f;
        [SerializeField] private RectTransform _fieldBounds;

        private Vector2 _velocity;
        private bool _canMove = true;
        private System.Action<Vector2> _onInteract;
        private float _animTimer;

        public Vector2 Position => _playerRect != null ? _playerRect.anchoredPosition : Vector2.zero;
        public bool CanMove { get => _canMove; set => _canMove = value; }

        public event System.Action<Vector2> OnInteract;

        private void Update()
        {
            if (!_canMove || _playerRect == null) return;

            // 入力取得
            float h = 0, v = 0;
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) v = 1;
            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) v = -1;
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) h = -1;
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) h = 1;

            _velocity = new Vector2(h, v).normalized * _moveSpeed;

            // 移動
            if (_velocity.sqrMagnitude > 0)
            {
                var pos = _playerRect.anchoredPosition + _velocity * UnityEngine.Time.deltaTime;

                // フィールド境界制限
                if (_fieldBounds != null)
                {
                    var bounds = _fieldBounds.rect;
                    float halfW = 15f; // プレイヤー半径
                    pos.x = Mathf.Clamp(pos.x, bounds.xMin + halfW, bounds.xMax - halfW);
                    pos.y = Mathf.Clamp(pos.y, bounds.yMin + halfW, bounds.yMax - halfW);
                }

                _playerRect.anchoredPosition = pos;

                // 簡易歩行アニメ（左右に揺れる）
                _animTimer += UnityEngine.Time.deltaTime * 8f;
                float sway = Mathf.Sin(_animTimer) * 3f;
                _playerRect.localRotation = Quaternion.Euler(0, 0, sway);
            }
            else
            {
                _playerRect.localRotation = Quaternion.identity;
                _animTimer = 0;
            }

            // インタラクト
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
            {
                OnInteract?.Invoke(Position);
            }
        }

        public void SetPosition(Vector2 pos)
        {
            if (_playerRect != null) _playerRect.anchoredPosition = pos;
        }
    }
}
