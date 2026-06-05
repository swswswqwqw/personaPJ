using System;
using UnityEngine;

namespace ArcadiaOfEchoes.Core
{
    public enum GamePhase
    {
        Title,
        Field,
        Battle,
        Social,
        EchoRealm,
        Calendar,
        Menu,
        Cutscene
    }

    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [SerializeField] private GamePhase initialPhase = GamePhase.Title;

        public GamePhase CurrentPhase { get; private set; }
        public bool IsTransitioning { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            CurrentPhase = initialPhase;
        }

        private void Start()
        {
            EventBus.Publish(new GamePhaseChangedEvent(GamePhase.Title, CurrentPhase));
        }

        public void TransitionTo(GamePhase newPhase)
        {
            if (IsTransitioning) return;
            if (newPhase == CurrentPhase) return;

            IsTransitioning = true;
            var previousPhase = CurrentPhase;
            CurrentPhase = newPhase;

            EventBus.Publish(new GamePhaseChangedEvent(previousPhase, newPhase));
            IsTransitioning = false;
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
}
