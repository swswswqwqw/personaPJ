using System;
using System.Collections.Generic;

namespace AriaOfEchoes.Core
{
    public interface IState
    {
        void Enter();
        void Update();
        void Exit();
    }

    public class StateMachine<T> where T : Enum
    {
        Dictionary<T, IState> states = new();
        IState currentState;
        T currentKey;

        public T CurrentStateKey => currentKey;

        public void AddState(T key, IState state)
        {
            states[key] = state;
        }

        public void ChangeState(T key)
        {
            if (!states.ContainsKey(key)) return;

            currentState?.Exit();
            currentKey = key;
            currentState = states[key];
            currentState.Enter();
        }

        public void Update()
        {
            currentState?.Update();
        }
    }
}
