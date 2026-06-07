using System;
using UnityEngine;
using EchoesOfArcadia.Core;
using EchoesOfArcadia.TimeSystem;

namespace EchoesOfArcadia.Echo
{
    public class EchoRealmManager : MonoBehaviour
    {
        public static EchoRealmManager Instance { get; private set; }

        private EchoRealmData currentDungeon;
        private int currentFloor;
        private bool isExploring;

        public EchoRealmData CurrentDungeon => currentDungeon;
        public int CurrentFloor => currentFloor;
        public bool IsExploring => isExploring;

        public event Action<EchoRealmData> OnDungeonEntered;
        public event Action<int> OnFloorChanged;
        public event Action<bool> OnDungeonCompleted;

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void EnterDungeon(EchoRealmData dungeon)
        {
            currentDungeon = dungeon;
            currentFloor = 1;
            isExploring = true;

            GameManager.Instance?.ChangePhase(GamePhase.EchoRealm);
            OnDungeonEntered?.Invoke(dungeon);
            GameEventBus.Publish(new DungeonEnteredEvent(dungeon.dungeonName));
        }

        public void AdvanceFloor()
        {
            if (!isExploring || currentDungeon == null) return;

            currentFloor++;
            if (currentFloor > currentDungeon.totalFloors)
            {
                CompleteDungeon(true);
                return;
            }

            OnFloorChanged?.Invoke(currentFloor);
        }

        public void Retreat()
        {
            if (!isExploring) return;
            isExploring = false;
            currentDungeon = null;
            GameManager.Instance?.ChangePhase(GamePhase.Field);
        }

        public void RetreatFromDungeon() => Retreat();

        public int GetDaysRemaining()
        {
            if (currentDungeon == null) return -1;
            return TimeManager.Instance?.DaysUntilDeadline(currentDungeon.deadline) ?? -1;
        }

        public bool IsDeadlineApproaching()
        {
            if (currentDungeon == null) return false;
            int days = GetDaysRemaining();
            return days >= 0 && days <= currentDungeon.warningDaysBeforeDeadline;
        }

        private void CompleteDungeon(bool rescued)
        {
            isExploring = false;
            OnDungeonCompleted?.Invoke(rescued);
            GameEventBus.Publish(new DungeonCompletedEvent(currentDungeon.dungeonName, rescued));
            currentDungeon = null;
            GameManager.Instance?.ChangePhase(GamePhase.Field);
        }
    }

    public readonly struct DungeonEnteredEvent
    {
        public readonly string DungeonName;
        public DungeonEnteredEvent(string name) { DungeonName = name; }
    }

    public readonly struct DungeonCompletedEvent
    {
        public readonly string DungeonName;
        public readonly bool VictimRescued;
        public DungeonCompletedEvent(string name, bool rescued)
        {
            DungeonName = name;
            VictimRescued = rescued;
        }
    }
}
