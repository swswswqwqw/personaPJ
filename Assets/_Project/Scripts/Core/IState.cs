namespace Amane.Core
{
    /// <summary>
    /// 状態マシンの1状態。戦闘・会話・フィールドなどの状態を表す。
    /// Plain C# で実装し、MonoBehaviour に依存しない。
    /// </summary>
    public interface IState
    {
        /// <summary>状態に入った瞬間。</summary>
        void Enter();

        /// <summary>毎フレーム呼ばれる更新。deltaTime はゲーム側から渡す。</summary>
        void Tick(float deltaTime);

        /// <summary>状態を抜ける瞬間。後始末を行う。</summary>
        void Exit();
    }
}
