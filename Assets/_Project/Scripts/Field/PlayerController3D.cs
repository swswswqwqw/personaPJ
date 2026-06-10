using UnityEngine;

namespace Amane.Field
{
    /// <summary>
    /// 3Dフィールドのプレイヤー移動コントローラー。
    /// WASD/矢印で移動、Spaceでインタラクト。
    /// カメラの向きに合わせて移動方向を補正。
    /// </summary>
    public sealed class PlayerController3D : MonoBehaviour
    {
        [SerializeField] private float _moveSpeed = 8f;
        [SerializeField] private float _rotationSpeed = 12f;

        private Rigidbody _rb;
        private bool _canMove = true;
        private Vector3 _moveDir;

        public bool CanMove { get => _canMove; set => _canMove = value; }
        public event System.Action OnInteract;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
        }

        private void Update()
        {
            if (!_canMove) return;

            // 入力
            float h = 0, v = 0;
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) v = 1;
            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) v = -1;
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) h = -1;
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) h = 1;

            // カメラ基準の移動方向
            var cam = Camera.main;
            if (cam != null)
            {
                var camForward = cam.transform.forward;
                var camRight = cam.transform.right;
                camForward.y = 0;
                camRight.y = 0;
                camForward.Normalize();
                camRight.Normalize();
                _moveDir = (camForward * v + camRight * h).normalized;
            }
            else
            {
                _moveDir = new Vector3(h, 0, v).normalized;
            }

            // キャラクターの回転（移動方向を向く）
            if (_moveDir.sqrMagnitude > 0.01f)
            {
                var targetRotation = Quaternion.LookRotation(_moveDir);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation,
                    _rotationSpeed * UnityEngine.Time.deltaTime);
            }

            // インタラクト
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
                OnInteract?.Invoke();
        }

        private void FixedUpdate()
        {
            if (!_canMove || _rb == null) return;

            var velocity = _moveDir * _moveSpeed;
            velocity.y = _rb.linearVelocity.y; // 重力を維持
            _rb.linearVelocity = velocity;
        }
    }
}
