using System;
using UnityEngine;

namespace EchoesOfArcadia.Core
{
    public enum GamePhase
    {
        Title,
        Field,
        Battle,
        Dungeon,
        Social,
        Cutscene,
        Menu
    }

    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [SerializeField] private GamePhase initialPhase = GamePhase.Title;

        public GamePhase CurrentPhase { get; private set; }
        public bool IsPaused { get; private set; }

        public event Action<GamePhase, GamePhase> OnPhaseChanged;

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            CurrentPhase = initialPhase;
        }

        public void ChangePhase(GamePhase newPhase)
        {
            if (CurrentPhase == newPhase) return;
            var previous = CurrentPhase;
            CurrentPhase = newPhase;
            OnPhaseChanged?.Invoke(previous, newPhase);
            GameEventBus.Publish(new PhaseChangedEvent(previous, newPhase));
        }

        public void Pause()
        {
            if (IsPaused) return;
            IsPaused = true;
            Time.timeScale = 0f;
            GameEventBus.Publish(new GamePausedEvent(true));
        }

        public void Resume()
        {
            if (!IsPaused) return;
            IsPaused = false;
            Time.timeScale = 1f;
            GameEventBus.Publish(new GamePausedEvent(false));
        }
    }

    public readonly struct PhaseChangedEvent
    {
        public readonly GamePhase Previous;
        public readonly GamePhase Current;

        public PhaseChangedEvent(GamePhase previous, GamePhase current)
        {
            Previous = previous;
            Current = current;
        }
    }

    public readonly struct GamePausedEvent
    {
        public readonly bool IsPaused;

        public GamePausedEvent(bool isPaused)
        {
            IsPaused = isPaused;
        }
    }
}
