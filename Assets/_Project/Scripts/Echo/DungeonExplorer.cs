using System;
using System.Collections.Generic;
using UnityEngine;
using EchoesOfArcadia.Core;
using EchoesOfArcadia.Data;
using EchoesOfArcadia.Battle;

namespace EchoesOfArcadia.Echo
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

        public EnemyData[] enemies;
        public ItemData treasureItem;
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

        private EchoRealmData activeDungeon;
        private readonly List<DungeonFloor> generatedFloors = new();

        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
        }

        private void OnEnable()
        {
            if (EchoRealmManager.Instance != null)
            {
                EchoRealmManager.Instance.OnDungeonEntered += OnDungeonEntered;
                EchoRealmManager.Instance.OnFloorChanged += OnFloorChanged;
            }
        }

        private void OnDisable()
        {
            if (EchoRealmManager.Instance != null)
            {
                EchoRealmManager.Instance.OnDungeonEntered -= OnDungeonEntered;
                EchoRealmManager.Instance.OnFloorChanged -= OnFloorChanged;
            }
        }

        private void OnDungeonEntered(EchoRealmData dungeon)
        {
            activeDungeon = dungeon;
            generatedFloors.Clear();
            ExploredRoomCount = 0;

            AudioManager.Instance?.PlayBGM(BGMTrack.EchoRealm);
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

            if (!room.cleared)
                ProcessRoomEvent(room);
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
            EchoRealmManager.Instance?.AdvanceFloor();
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

        private void ProcessRoomEvent(DungeonRoom room)
        {
            switch (room.type)
            {
                case RoomType.Battle:
                    StartRoomBattle(room);
                    break;
                case RoomType.Treasure:
                    CollectTreasure(room);
                    break;
                case RoomType.Trap:
                    TriggerTrap(room);
                    break;
                case RoomType.RestPoint:
                    RestoreParty(room);
                    break;
                case RoomType.MiniBoss:
                    StartMiniBossBattle(room);
                    break;
                case RoomType.Boss:
                    StartBossBattle(room);
                    break;
                case RoomType.Event:
                    TriggerRoomEvent(room);
                    break;
                default:
                    room.cleared = true;
                    break;
            }
        }

        private void StartRoomBattle(DungeonRoom room)
        {
            if (room.enemies == null || room.enemies.Length == 0)
            {
                room.cleared = true;
                return;
            }

            var enemyList = new List<EnemyData>();
            int count = UnityEngine.Random.Range(1, 4);
            for (int i = 0; i < count; i++)
                enemyList.Add(room.enemies[UnityEngine.Random.Range(0, room.enemies.Length)]);

            GameFlowController.Instance?.StartBattle(enemyList);

            GameEventBus.Subscribe<BattleEndedEvent>(OnBattleEndedInRoom);

            void OnBattleEndedInRoom(BattleEndedEvent e)
            {
                GameEventBus.Unsubscribe<BattleEndedEvent>(OnBattleEndedInRoom);
                room.cleared = true;
                OnRoomCleared?.Invoke(room);
                GameManager.Instance?.ChangePhase(GamePhase.EchoRealm);
            }
        }

        private void StartMiniBossBattle(DungeonRoom room)
        {
            EchoFloorData floorData = GetFloorData(CurrentFloor.floorNumber);
            if (floorData?.miniBoss == null) { room.cleared = true; return; }

            var enemies = new List<EnemyData> { floorData.miniBoss };
            GameFlowController.Instance?.StartBattle(enemies);

            GameEventBus.Subscribe<BattleEndedEvent>(OnMiniBossDefeated);

            void OnMiniBossDefeated(BattleEndedEvent e)
            {
                GameEventBus.Unsubscribe<BattleEndedEvent>(OnMiniBossDefeated);
                room.cleared = true;
                OnRoomCleared?.Invoke(room);
                GameManager.Instance?.ChangePhase(GamePhase.EchoRealm);
            }
        }

        private void StartBossBattle(DungeonRoom room)
        {
            if (activeDungeon?.bossEnemy == null) { room.cleared = true; return; }

            AudioManager.Instance?.PlayBGM(BGMTrack.Battle_Boss);
            var enemies = new List<EnemyData> { activeDungeon.bossEnemy };
            GameFlowController.Instance?.StartBattle(enemies);

            GameEventBus.Subscribe<BattleEndedEvent>(OnBossDefeated);

            void OnBossDefeated(BattleEndedEvent e)
            {
                GameEventBus.Unsubscribe<BattleEndedEvent>(OnBossDefeated);
                room.cleared = true;
                OnRoomCleared?.Invoke(room);
            }
        }

        private void CollectTreasure(DungeonRoom room)
        {
            if (room.treasureItem != null)
                InventoryManager.Instance?.AddItem(room.treasureItem, 1);

            AudioManager.Instance?.PlaySFX(SFXType.UI_Confirm);
            room.cleared = true;
            OnRoomCleared?.Invoke(room);
        }

        private void TriggerTrap(DungeonRoom room)
        {
            if (BattleManager.Instance != null)
            {
                foreach (var unit in BattleManager.Instance.PartyUnits)
                {
                    if (unit.IsAlive)
                        unit.TakeDamage(room.trapDamage);
                }
            }
            room.cleared = true;
            OnRoomCleared?.Invoke(room);
        }

        private void RestoreParty(DungeonRoom room)
        {
            if (BattleManager.Instance != null)
            {
                foreach (var unit in BattleManager.Instance.PartyUnits)
                {
                    if (!unit.IsAlive) continue;
                    unit.Heal((int)(unit.MaxHP * 0.3f));
                    unit.RestoreSP((int)(unit.MaxSP * 0.2f));
                }
            }
            AudioManager.Instance?.PlaySFX(SFXType.Battle_Heal);
            room.cleared = true;
            OnRoomCleared?.Invoke(room);
        }

        private void TriggerRoomEvent(DungeonRoom room)
        {
            if (!string.IsNullOrEmpty(room.eventDialogueFile))
            {
                var jsonData = Dialogue.DialogueDataLoader.LoadFromStreamingAssets(room.eventDialogueFile);
                if (jsonData != null)
                {
                    var data = Dialogue.DialogueDataLoader.ConvertToScriptableObject(jsonData);
                    Dialogue.DialogueSystem.Instance?.StartDialogue(data);
                }
            }
            room.cleared = true;
            OnRoomCleared?.Invoke(room);
        }

        private DungeonFloor GenerateFloor(int floorNumber)
        {
            var floor = new DungeonFloor
            {
                floorNumber = floorNumber,
                floorName = $"B{floorNumber}F"
            };

            EchoFloorData floorData = GetFloorData(floorNumber);
            int roomCount = UnityEngine.Random.Range(minRoomsPerFloor, maxRoomsPerFloor + 1);
            bool isBossFloor = activeDungeon != null && floorNumber >= activeDungeon.totalFloors;

            for (int i = 0; i < roomCount; i++)
            {
                var room = new DungeonRoom { roomId = i };

                if (i == 0)
                {
                    room.type = RoomType.Entrance;
                    room.description = "残響界への入口。微かな共鳴音が響いている。";
                    floor.entranceRoomId = i;
                }
                else if (i == roomCount - 1)
                {
                    if (isBossFloor)
                    {
                        room.type = RoomType.Boss;
                        room.description = "強大な残響の気配。ボスの部屋だ。";
                    }
                    else
                    {
                        room.type = RoomType.Stairs;
                        room.description = "次のフロアへ続く階段が見える。";
                        floor.stairsRoomId = i;
                    }
                }
                else
                {
                    room.type = RollRoomType(i, roomCount, floorData);
                    room.description = GetRoomDescription(room.type);
                    PopulateRoom(room, floorData, floorNumber);
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

        private RoomType RollRoomType(int index, int totalRooms, EchoFloorData floorData)
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

        private void PopulateRoom(DungeonRoom room, EchoFloorData floorData, int floorNumber)
        {
            switch (room.type)
            {
                case RoomType.Battle:
                    room.enemies = floorData?.possibleEnemies;
                    break;
                case RoomType.Treasure:
                    room.treasureItem = null;
                    break;
                case RoomType.Trap:
                    room.trapDamage = 10 + floorNumber * 5;
                    break;
            }
        }

        private EchoFloorData GetFloorData(int floorNumber)
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
            RoomType.Empty => "静かな部屋。残響が微かに漂っている。",
            RoomType.Battle => "敵の気配を感じる……！",
            RoomType.Treasure => "部屋の奥に、光るものが見える。",
            RoomType.Trap => "何か嫌な予感がする……。",
            RoomType.RestPoint => "穏やかな空気が漂う安息の間。",
            RoomType.Event => "何かが聴こえる……声の残響だ。",
            RoomType.MiniBoss => "強い残響の塊が行く手を阻んでいる。",
            _ => ""
        };
    }
}
