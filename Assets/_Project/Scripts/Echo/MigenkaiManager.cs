using System;
using UnityEngine;
using Amane.Core;

namespace Amane.Echo
{
    public class MigenkaiManager : MonoBehaviour
    {
        public static MigenkaiManager Instance { get; private set; }

        private MigenkaiData currentDungeon;
        private int currentFloor;
        private bool isExploring;

        public MigenkaiData CurrentDungeon => currentDungeon;
        public int CurrentFloor => currentFloor;
        public bool IsExploring => isExploring;

        public event Action<MigenkaiData> OnDungeonEntered;
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

        public void EnterDungeon(MigenkaiData dungeon)
        {
            currentDungeon = dungeon;
            currentFloor = 1;
            isExploring = true;

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
        }

        private void CompleteDungeon(bool rescued)
        {
            isExploring = false;
            OnDungeonCompleted?.Invoke(rescued);
            GameEventBus.Publish(new DungeonCompletedEvent(currentDungeon.dungeonName, rescued));
            currentDungeon = null;
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
