using System;
using System.Collections.Generic;

namespace ArcadiaOfEchoes.Core
{
    public interface IState
    {
        void Enter();
        void Update();
        void Exit();
    }

    public class StateMachine<TStateId> where TStateId : Enum
    {
        private readonly Dictionary<TStateId, IState> _states = new();
        private IState _currentState;

        public TStateId CurrentStateId { get; private set; }
        public event Action<TStateId, TStateId> OnStateChanged;

        public void RegisterState(TStateId id, IState state)
        {
            _states[id] = state;
        }

        public void ChangeState(TStateId newStateId)
        {
            if (!_states.TryGetValue(newStateId, out var newState))
                throw new InvalidOperationException($"State {newStateId} not registered.");

            var oldStateId = CurrentStateId;
            _currentState?.Exit();
            CurrentStateId = newStateId;
            _currentState = newState;
            _currentState.Enter();
            OnStateChanged?.Invoke(oldStateId, newStateId);
        }

        public void Update()
        {
            _currentState?.Update();
        }
    }
}
