using UnityEngine;

namespace Amane.Field
{
    /// <summary>
    /// 三人称カメラ。プレイヤーを斜め上から追従する。
    /// ペルソナ5のフィールドカメラのような、やや見下ろしの角度。
    /// </summary>
    public sealed class ThirdPersonCamera : MonoBehaviour
    {
        public Transform Target;
        public float Distance = 10f;
        public float Height = 7f;
        public Vector3 LookAtOffset = new(0, 1, 0);
        public float SmoothSpeed = 5f;

        private void LateUpdate()
        {
            if (Target == null) return;

            // 目標位置（プレイヤーの後方上空）
            var targetPos = Target.position
                + Vector3.up * Height
                + Vector3.back * Distance * 0.7f
                + Vector3.left * Distance * 0.3f;

            // スムーズ追従
            transform.position = Vector3.Lerp(transform.position, targetPos,
                SmoothSpeed * UnityEngine.Time.deltaTime);

            // プレイヤーを見る
            transform.LookAt(Target.position + LookAtOffset);
        }
    }
}
