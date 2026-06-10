using System;

namespace Astra.Core
{
    public class StateMachine<T> where T : Enum
    {
        public T CurrentState { get; private set; }
        public T PreviousState { get; private set; }

        public event Action<T, T> OnStateChanged;

        public StateMachine(T initialState)
        {
            CurrentState = initialState;
            PreviousState = initialState;
        }

        public void TransitionTo(T newState)
        {
            if (CurrentState.Equals(newState)) return;

            PreviousState = CurrentState;
            CurrentState = newState;
            OnStateChanged?.Invoke(PreviousState, CurrentState);
        }

        public bool IsInState(T state) => CurrentState.Equals(state);
    }
}
