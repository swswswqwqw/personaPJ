using UnityEngine;

namespace ArcanaOfHollows.Core
{
    public enum GamePhase
    {
        Title,
        Field,
        Battle,
        Social,
        Dungeon,
        Cutscene,
        Menu,
        Loading
    }

    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [SerializeField] private GamePhase initialPhase = GamePhase.Title;

        public GamePhase CurrentPhase { get; private set; }
        public bool IsPaused { get; private set; }

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

        public void TransitionTo(GamePhase newPhase)
        {
            var previousPhase = CurrentPhase;
            CurrentPhase = newPhase;
            EventBus.Publish(new GamePhaseChangedEvent(previousPhase, newPhase));
        }

        public void Pause()
        {
            if (IsPaused) return;
            IsPaused = true;
            Time.timeScale = 0f;
            EventBus.Publish(new GamePausedEvent(true));
        }

        public void Resume()
        {
            if (!IsPaused) return;
            IsPaused = false;
            Time.timeScale = 1f;
            EventBus.Publish(new GamePausedEvent(false));
        }
    }

    public readonly struct GamePhaseChangedEvent
    {
        public readonly GamePhase PreviousPhase;
        public readonly GamePhase NewPhase;

        public GamePhaseChangedEvent(GamePhase previous, GamePhase next)
        {
            PreviousPhase = previous;
            NewPhase = next;
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
