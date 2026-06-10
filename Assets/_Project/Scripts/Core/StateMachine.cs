using System;
using System.Collections.Generic;

namespace EchoesOfArcadia.Core
{
    public interface IState
    {
        void Enter();
        void Update(float deltaTime);
        void Exit();
    }

    public class StateMachine<TStateId> where TStateId : Enum
    {
        private readonly Dictionary<TStateId, IState> _states = new();
        private IState _currentState;

        public TStateId CurrentStateId { get; private set; }
        public event Action<TStateId, TStateId> OnStateChanged;

        public void AddState(TStateId id, IState state)
        {
            _states[id] = state;
        }

        public void SetState(TStateId id)
        {
            if (!_states.ContainsKey(id))
                throw new InvalidOperationException($"State {id} not registered.");

            var previousId = CurrentStateId;
            _currentState?.Exit();
            CurrentStateId = id;
            _currentState = _states[id];
            _currentState.Enter();
            OnStateChanged?.Invoke(previousId, id);
        }

        public void Update(float deltaTime)
        {
            _currentState?.Update(deltaTime);
        }
    }
}
