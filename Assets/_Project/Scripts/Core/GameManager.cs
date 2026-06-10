using UnityEngine;

namespace ArcadiaOfEchoes.Core
{
    public enum GamePhase
    {
        Title,
        Field,
        Battle,
        Dungeon,
        Social,
        Dialogue,
        Calendar,
        Menu,
        Loading
    }

    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [SerializeField] private GamePhase initialPhase = GamePhase.Title;

        public GamePhase CurrentPhase { get; private set; }
        public bool IsPaused { get; private set; }

        public event System.Action<GamePhase, GamePhase> OnPhaseChanged;
        public event System.Action<bool> OnPauseChanged;

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

            var oldPhase = CurrentPhase;
            CurrentPhase = newPhase;
            OnPhaseChanged?.Invoke(oldPhase, newPhase);
        }

        public void SetPause(bool paused)
        {
            if (IsPaused == paused) return;

            IsPaused = paused;
            Time.timeScale = paused ? 0f : 1f;
            OnPauseChanged?.Invoke(paused);
        }
    }
}
