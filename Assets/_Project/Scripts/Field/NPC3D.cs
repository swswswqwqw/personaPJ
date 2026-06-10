using UnityEngine;

namespace Amane.Field
{
    /// <summary>
    /// 3DフィールドのNPC。プレイヤーが近づくとプレイヤーの方を向く。
    /// </summary>
    public sealed class NPC3D : MonoBehaviour
    {
        public string Id;
        public string DisplayName;
        public LocationType Type = LocationType.NPC;
        public float InteractRadius = 3f;

        private Transform _playerTransform;
        private bool _playerNearby;

        public bool IsPlayerNearby => _playerNearby;

        private void Update()
        {
            if (_playerTransform == null)
            {
                var player = Object.FindAnyObjectByType<PlayerController3D>();
                if (player != null) _playerTransform = player.transform;
                return;
            }

            float dist = Vector3.Distance(transform.position, _playerTransform.position);
            _playerNearby = dist < InteractRadius;

            // プレイヤーが近いとそちらを向く
            if (_playerNearby)
            {
                var lookDir = _playerTransform.position - transform.position;
                lookDir.y = 0;
                if (lookDir.sqrMagnitude > 0.01f)
                {
                    var targetRot = Quaternion.LookRotation(lookDir);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRot,
                        4f * UnityEngine.Time.deltaTime);
                }
            }
        }
    }

    /// <summary>
    /// 3Dフィールドのロケーションマーカー（建物等）。
    /// </summary>
    public sealed class LocationMarker3D : MonoBehaviour
    {
        public string Id;
        public string DisplayName;
        public LocationType Type;
        public float InteractRadius = 5f;
    }

    /// <summary>
    /// ゆっくり浮遊するアニメーション。NPCや光る球に付ける。
    /// </summary>
    public sealed class FloatAnimation : MonoBehaviour
    {
        public float Amplitude = 0.15f;
        public float Speed = 1.5f;
        private Vector3 _startPos;

        private void Start() => _startPos = transform.localPosition;

        private void Update()
        {
            var pos = _startPos;
            pos.y += Mathf.Sin(UnityEngine.Time.time * Speed) * Amplitude;
            transform.localPosition = pos;
        }
    }
}
