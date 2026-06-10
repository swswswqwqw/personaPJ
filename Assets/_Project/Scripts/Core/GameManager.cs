using UnityEngine;

namespace AriaOfBacklight.Core
{
    public enum GamePhase
    {
        Title,
        Field,
        Battle,
        Rimen,
        Social,
        Menu,
        Cutscene
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

        public void ChangePhase(GamePhase newPhase)
        {
            var previousPhase = CurrentPhase;
            CurrentPhase = newPhase;
            EventBus.Publish(new GamePhaseChangedEvent(previousPhase, newPhase));
        }

        public void Pause()
        {
            IsPaused = true;
            Time.timeScale = 0f;
            EventBus.Publish(new GamePausedEvent(true));
        }

        public void Resume()
        {
            IsPaused = false;
            Time.timeScale = 1f;
            EventBus.Publish(new GamePausedEvent(false));
        }
    }

    public readonly struct GamePhaseChangedEvent
    {
        public readonly GamePhase Previous;
        public readonly GamePhase Current;

        public GamePhaseChangedEvent(GamePhase previous, GamePhase current)
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
