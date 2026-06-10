using UnityEngine;

namespace Astra.Core
{
    public enum GamePhase
    {
        Title,
        Field,
        Battle,
        Dungeon,
        Dialogue,
        Menu,
        Calendar,
        Transition
    }

    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [SerializeField] private GamePhase initialPhase = GamePhase.Title;

        public GamePhase CurrentPhase => _stateMachine.CurrentState;

        private StateMachine<GamePhase> _stateMachine;

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            _stateMachine = new StateMachine<GamePhase>(initialPhase);
            _stateMachine.OnStateChanged += OnPhaseChanged;
        }

        public void ChangePhase(GamePhase newPhase)
        {
            _stateMachine.TransitionTo(newPhase);
        }

        private void OnPhaseChanged(GamePhase from, GamePhase to)
        {
            EventBus.Publish(new GamePhaseChangedEvent(from, to));
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                _stateMachine.OnStateChanged -= OnPhaseChanged;
                Instance = null;
            }
        }
    }

    public readonly struct GamePhaseChangedEvent
    {
        public readonly GamePhase From;
        public readonly GamePhase To;

        public GamePhaseChangedEvent(GamePhase from, GamePhase to)
        {
            From = from;
            To = to;
        }
    }
}
