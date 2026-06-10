using System;
using UnityEngine;

namespace AriaOfEchoes.Core
{
    public enum GamePhase
    {
        Title,
        Field,
        Battle,
        Dungeon,
        Dialogue,
        Menu,
        Event,
        Loading
    }

    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [SerializeField] GameConfig config;

        GamePhase currentPhase;
        public GamePhase CurrentPhase => currentPhase;

        public event Action<GamePhase, GamePhase> OnPhaseChanged;

        void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        void Start()
        {
            ChangePhase(GamePhase.Title);
        }

        public void ChangePhase(GamePhase newPhase)
        {
            var oldPhase = currentPhase;
            currentPhase = newPhase;
            OnPhaseChanged?.Invoke(oldPhase, newPhase);
            EventBus.Publish(new PhaseChangedEvent(oldPhase, newPhase));
        }

        public void StartNewGame()
        {
            EventBus.Publish(new NewGameEvent());
            ChangePhase(GamePhase.Field);
        }

        public void ReturnToTitle()
        {
            ChangePhase(GamePhase.Title);
        }
    }

    public struct PhaseChangedEvent
    {
        public GamePhase OldPhase { get; }
        public GamePhase NewPhase { get; }

        public PhaseChangedEvent(GamePhase oldPhase, GamePhase newPhase)
        {
            OldPhase = oldPhase;
            NewPhase = newPhase;
        }
    }

    public struct NewGameEvent { }
}
