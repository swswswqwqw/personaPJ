using System;
using System.Collections.Generic;

namespace ArcadiaOfEchoes.Core
{
    public interface IState
    {
        void Enter();
        void Update(float deltaTime);
        void Exit();
    }

    public class StateMachine
    {
        private IState _currentState;
        private readonly Dictionary<Type, IState> _states = new();

        public IState CurrentState => _currentState;
        public Type CurrentStateType => _currentState?.GetType();

        public void AddState<T>(T state) where T : class, IState
        {
            _states[typeof(T)] = state;
        }

        public void TransitionTo<T>() where T : class, IState
        {
            var type = typeof(T);
            if (!_states.TryGetValue(type, out var nextState))
                throw new InvalidOperationException($"State {type.Name} not registered.");

            _currentState?.Exit();
            _currentState = nextState;
            _currentState.Enter();
        }

        public void Update(float deltaTime)
        {
            _currentState?.Update(deltaTime);
        }
    }
}
