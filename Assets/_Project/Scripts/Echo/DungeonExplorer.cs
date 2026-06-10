using System;
using System.Collections.Generic;
using UnityEngine;
using Amane.Core;

namespace Amane.Echo
{
    public enum RoomType
    {
        Empty,
        Battle,
        Treasure,
        Trap,
        RestPoint,
        Event,
        MiniBoss,
        Boss,
        Stairs,
        Entrance
    }

    [Serializable]
    public class DungeonRoom
    {
        public int roomId;
        public RoomType type;
        public string description;
        public bool explored;
        public bool cleared;
        public List<int> connectedRooms = new();

        public int trapDamage;
        public string eventDialogueFile;
    }

    [Serializable]
    public class DungeonFloor
    {
        public int floorNumber;
        public string floorName;
        public List<DungeonRoom> rooms = new();
        public int entranceRoomId;
        public int stairsRoomId;
        public bool completed;
    }

    public class DungeonExplorer : MonoBehaviour
    {
        public static DungeonExplorer Instance { get; private set; }

        [Header("Settings")]
        [SerializeField] private int minRoomsPerFloor = 6;
        [SerializeField] private int maxRoomsPerFloor = 10;
        [SerializeField] private float battleEncounterRate = 0.35f;
        [SerializeField] private float treasureRate = 0.15f;
        [SerializeField] private float trapRate = 0.1f;
        [SerializeField] private float restPointRate = 0.08f;

        public DungeonFloor CurrentFloor { get; private set; }
        public DungeonRoom CurrentRoom { get; private set; }
        public int ExploredRoomCount { get; private set; }

        public event Action<DungeonFloor> OnFloorGenerated;
        public event Action<DungeonRoom> OnRoomEntered;
        public event Action<DungeonRoom> OnRoomCleared;
        public event Action<int> OnFloorCompleted;

        private MigenkaiData activeDungeon;
        private readonly List<DungeonFloor> generatedFloors = new();

        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
        }

        private void OnEnable()
        {
            if (MigenkaiManager.Instance != null)
            {
                MigenkaiManager.Instance.OnDungeonEntered += OnDungeonEntered;
                MigenkaiManager.Instance.OnFloorChanged += OnFloorChanged;
            }
        }

        private void OnDisable()
        {
            if (MigenkaiManager.Instance != null)
            {
                MigenkaiManager.Instance.OnDungeonEntered -= OnDungeonEntered;
                MigenkaiManager.Instance.OnFloorChanged -= OnFloorChanged;
            }
        }

        private void OnDungeonEntered(MigenkaiData dungeon)
        {
            activeDungeon = dungeon;
            generatedFloors.Clear();
            ExploredRoomCount = 0;
            GenerateAndEnterFloor(1);
        }

        private void OnFloorChanged(int floorNumber)
        {
            GenerateAndEnterFloor(floorNumber);
        }

        public void GenerateAndEnterFloor(int floorNumber)
        {
            var existing = generatedFloors.Find(f => f.floorNumber == floorNumber);
            if (existing != null)
            {
                CurrentFloor = existing;
            }
            else
            {
                CurrentFloor = GenerateFloor(floorNumber);
                generatedFloors.Add(CurrentFloor);
            }

            OnFloorGenerated?.Invoke(CurrentFloor);
            EnterRoom(CurrentFloor.entranceRoomId);
        }

        public void EnterRoom(int roomId)
        {
            var room = CurrentFloor?.rooms.Find(r => r.roomId == roomId);
            if (room == null) return;

            CurrentRoom = room;

            if (!room.explored)
            {
                room.explored = true;
                ExploredRoomCount++;
            }

            OnRoomEntered?.Invoke(room);
        }

        public void MoveToConnectedRoom(int index)
        {
            if (CurrentRoom == null || index < 0 || index >= CurrentRoom.connectedRooms.Count)
                return;
            EnterRoom(CurrentRoom.connectedRooms[index]);
        }

        public void UseStairs()
        {
            if (CurrentRoom?.type != RoomType.Stairs) return;

            CurrentFloor.completed = true;
            OnFloorCompleted?.Invoke(CurrentFloor.floorNumber);
            MigenkaiManager.Instance?.AdvanceFloor();
        }

        public List<DungeonRoom> GetConnectedRooms()
        {
            var result = new List<DungeonRoom>();
            if (CurrentRoom == null || CurrentFloor == null) return result;

            foreach (int id in CurrentRoom.connectedRooms)
            {
                var room = CurrentFloor.rooms.Find(r => r.roomId == id);
                if (room != null) result.Add(room);
            }
            return result;
        }

        private DungeonFloor GenerateFloor(int floorNumber)
        {
            var floor = new DungeonFloor
            {
                floorNumber = floorNumber,
                floorName = $"B{floorNumber}F"
            };

            int roomCount = UnityEngine.Random.Range(minRoomsPerFloor, maxRoomsPerFloor + 1);
            bool isBossFloor = activeDungeon != null && floorNumber >= activeDungeon.totalFloors;
            MigenkaiFloorData floorData = GetFloorData(floorNumber);

            for (int i = 0; i < roomCount; i++)
            {
                var room = new DungeonRoom { roomId = i };

                if (i == 0)
                {
                    room.type = RoomType.Entrance;
                    room.description = "未言界への入口。微かな残響が漂っている。";
                    floor.entranceRoomId = i;
                }
                else if (i == roomCount - 1)
                {
                    if (isBossFloor)
                    {
                        room.type = RoomType.Boss;
                        room.description = "強大な澱の気配。言伝の間だ。";
                    }
                    else
                    {
                        room.type = RoomType.Stairs;
                        room.description = "次の層へ続く道が見える。";
                        floor.stairsRoomId = i;
                    }
                }
                else
                {
                    room.type = RollRoomType(i, roomCount, floorData);
                    room.description = GetRoomDescription(room.type);
                    if (room.type == RoomType.Trap)
                        room.trapDamage = 10 + floorNumber * 5;
                }

                floor.rooms.Add(room);
            }

            for (int i = 0; i < roomCount; i++)
            {
                var room = floor.rooms[i];
                if (i > 0)
                    room.connectedRooms.Add(i - 1);
                if (i < roomCount - 1)
                    room.connectedRooms.Add(i + 1);

                if (i > 1 && UnityEngine.Random.value < 0.25f)
                {
                    int shortcut = UnityEngine.Random.Range(0, i - 1);
                    if (!room.connectedRooms.Contains(shortcut))
                    {
                        room.connectedRooms.Add(shortcut);
                        floor.rooms[shortcut].connectedRooms.Add(i);
                    }
                }
            }

            return floor;
        }

        private RoomType RollRoomType(int index, int totalRooms, MigenkaiFloorData floorData)
        {
            if (floorData != null && floorData.hasMiniBoss && index == totalRooms / 2)
                return RoomType.MiniBoss;

            float roll = UnityEngine.Random.value;
            float cumulative = 0f;

            cumulative += battleEncounterRate;
            if (roll < cumulative) return RoomType.Battle;

            cumulative += treasureRate;
            if (roll < cumulative) return RoomType.Treasure;

            cumulative += trapRate;
            if (roll < cumulative) return RoomType.Trap;

            cumulative += restPointRate;
            if (roll < cumulative) return RoomType.RestPoint;

            return RoomType.Empty;
        }

        private MigenkaiFloorData GetFloorData(int floorNumber)
        {
            if (activeDungeon?.floors == null) return null;
            foreach (var f in activeDungeon.floors)
            {
                if (f.floorNumber == floorNumber) return f;
            }
            return activeDungeon.floors.Length > 0
                ? activeDungeon.floors[Mathf.Min(floorNumber - 1, activeDungeon.floors.Length - 1)]
                : null;
        }

        private static string GetRoomDescription(RoomType type) => type switch
        {
            RoomType.Empty => "静かな部屋。言えなかった言葉の残響が微かに漂う。",
            RoomType.Battle => "澱の怪物の気配を感じる……！",
            RoomType.Treasure => "部屋の奥に、光る封筒が見える。",
            RoomType.Trap => "何か嫌な予感がする……。",
            RoomType.RestPoint => "穏やかな空気が漂う安息の間。",
            RoomType.Event => "何かが聴こえる……声の残響だ。",
            RoomType.MiniBoss => "強い澱の塊が行く手を阻んでいる。",
            _ => ""
        };
    }
}
