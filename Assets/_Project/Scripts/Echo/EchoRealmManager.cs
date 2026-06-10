using System;
using System.Collections.Generic;
using UnityEngine;
using EchoesOfArcadia.Core;
using EchoesOfArcadia.Time;

namespace EchoesOfArcadia.Echo
{
    public enum DungeonState
    {
        Exploring,
        InBattle,
        InEvent,
        BossRoom,
        Cleared
    }

    [Serializable]
    public class DungeonFloor
    {
        public int floorNumber;
        public string floorName;
        public bool isExplored;
        public bool hasBoss;
    }

    public class EchoRealmManager : MonoBehaviour
    {
        public static EchoRealmManager Instance { get; private set; }

        [SerializeField] private int maxExplorationTurns = 20;

        private DungeonState _currentState;
        private List<DungeonFloor> _floors = new();
        private int _currentFloorIndex;
        private int _remainingTurns;

        public DungeonState CurrentState => _currentState;
        public int CurrentFloor => _currentFloorIndex + 1;
        public int RemainingTurns => _remainingTurns;

        public event Action<DungeonState> OnDungeonStateChanged;
        public event Action<int> OnFloorChanged;
        public event Action OnExplorationComplete;

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public void EnterDungeon(List<DungeonFloor> floors)
        {
            _floors = floors;
            _currentFloorIndex = 0;
            _remainingTurns = maxExplorationTurns;
            _currentState = DungeonState.Exploring;

            GameManager.Instance?.ChangePhase(GamePhase.Dungeon);
            OnDungeonStateChanged?.Invoke(_currentState);
            GameEventBus.Publish(new DungeonEnteredEvent(_floors[0].floorName));
        }

        public void AdvanceFloor()
        {
            if (_currentFloorIndex >= _floors.Count - 1) return;

            _currentFloorIndex++;
            var floor = _floors[_currentFloorIndex];
            floor.isExplored = true;

            if (floor.hasBoss)
                _currentState = DungeonState.BossRoom;

            OnFloorChanged?.Invoke(_currentFloorIndex + 1);
            GameEventBus.Publish(new FloorChangedEvent(_currentFloorIndex + 1, floor.floorName));
        }

        public void ConsumeExplorationTurn()
        {
            _remainingTurns--;

            if (_remainingTurns <= 0)
            {
                ForceRetreat();
            }
        }

        public void CompleteDungeon()
        {
            _currentState = DungeonState.Cleared;
            OnExplorationComplete?.Invoke();
            GameEventBus.Publish(new DungeonClearedEvent());

            TimeManager.Instance?.AdvancePeriod();
        }

        public void ForceRetreat()
        {
            _currentState = DungeonState.Exploring;
            GameEventBus.Publish(new DungeonRetreatEvent());
            GameManager.Instance?.ChangePhase(GamePhase.Field);

            TimeManager.Instance?.AdvancePeriod();
        }

        public void ReturnToEntrance()
        {
            _currentFloorIndex = 0;
            _currentState = DungeonState.Exploring;
        }
    }

    public readonly struct DungeonEnteredEvent
    {
        public readonly string DungeonName;
        public DungeonEnteredEvent(string name) => DungeonName = name;
    }

    public readonly struct FloorChangedEvent
    {
        public readonly int FloorNumber;
        public readonly string FloorName;
        public FloorChangedEvent(int number, string name)
        {
            FloorNumber = number;
            FloorName = name;
        }
    }

    public readonly struct DungeonClearedEvent { }
    public readonly struct DungeonRetreatEvent { }
}
