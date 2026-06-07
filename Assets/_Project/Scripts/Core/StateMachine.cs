using System;
using System.Collections.Generic;

namespace EchoesOfArcadia.Core
{
    public interface IState
    {
        void Enter();
        void Update();
        void Exit();
    }

    public class StateMachine
    {
        private IState currentState;
        private readonly Dictionary<Type, IState> states = new();

        public IState CurrentState => currentState;

        public void AddState(IState state)
        {
            states[state.GetType()] = state;
        }

        public void ChangeState<T>() where T : IState
        {
            var type = typeof(T);
            if (!states.ContainsKey(type))
                throw new InvalidOperationException($"State {type.Name} not registered.");

            currentState?.Exit();
            currentState = states[type];
            currentState.Enter();
        }

        public void Update()
        {
            currentState?.Update();
        }

        public bool IsInState<T>() where T : IState
        {
            return currentState is T;
        }
    }
}
