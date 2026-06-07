using System;
using System.Collections.Generic;

namespace Amane.Core
{
    /// <summary>
    /// 汎用ステートマシン。戦闘・会話・フィールドの状態管理に使う。
    /// Plain C# クラス。MonoBehaviour からは Tick を駆動するだけにする。
    /// </summary>
    public sealed class StateMachine
    {
        private readonly Dictionary<Type, IState> _states = new();
        private IState _current;

        public IState Current => _current;
        public event Action<IState, IState> OnStateChanged; // (from, to)

        /// <summary>状態を登録する。同一型は1つだけ。</summary>
        public void Register<T>(T state) where T : IState
        {
            if (state == null) throw new ArgumentNullException(nameof(state));
            _states[typeof(T)] = state;
        }

        /// <summary>登録済みの型へ遷移する。</summary>
        public void ChangeTo<T>() where T : IState
        {
            if (!_states.TryGetValue(typeof(T), out var next))
                throw new InvalidOperationException($"State '{typeof(T).Name}' is not registered.");
            SwitchTo(next);
        }

        /// <summary>インスタンスを直接渡して遷移する（動的生成した状態用）。</summary>
        public void ChangeTo(IState next)
        {
            if (next == null) throw new ArgumentNullException(nameof(next));
            SwitchTo(next);
        }

        private void SwitchTo(IState next)
        {
            if (ReferenceEquals(_current, next)) return;
            var prev = _current;
            _current?.Exit();
            _current = next;
            _current.Enter();
            OnStateChanged?.Invoke(prev, _current);
        }

        /// <summary>毎フレーム呼ぶ。現在状態の Tick を回す。</summary>
        public void Tick(float deltaTime) => _current?.Tick(deltaTime);
    }
}
