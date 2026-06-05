using System;
using AstralEchoes.Data;

namespace AstralEchoes.Core
{
    public sealed class GameManager
    {
        static GameManager _instance;
        public static GameManager Instance => _instance ??= new GameManager();

        public GameState CurrentState { get; private set; } = GameState.Title;

        public event Action<GameState, GameState> OnStateChanged;

        GameManager() { }

        public void ChangeState(GameState newState)
        {
            if (CurrentState == newState) return;
            var previous = CurrentState;
            CurrentState = newState;
            OnStateChanged?.Invoke(previous, newState);
        }

        public bool IsInState(GameState state) => CurrentState == state;
    }
}
