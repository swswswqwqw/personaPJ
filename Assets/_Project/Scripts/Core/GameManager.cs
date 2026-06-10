using UnityEngine;
using Amane.Time;
using Amane.Social;
using Amane.Stat;
using Amane.Battle;

namespace Amane.Core
{
    /// <summary>
    /// ゲーム全体の統括シングルトン（唯一の MonoBehaviour エントリ）。
    /// ロジックは Plain C#（TimeManager / BondManager / InnerStatSet）に委譲し、
    /// ここはライフサイクルと配線だけを担う。
    /// </summary>
    [DefaultExecutionOrder(-1000)]
    public sealed class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        // ---- 共有サービス ----
        public EventChannel Events { get; private set; }
        public StateMachine Machine { get; private set; }
        public TimeManager Time { get; private set; }
        public BondManager Bonds { get; private set; }
        public InnerStatSet Stats { get; private set; }
        public ISaveSystem Save { get; private set; }
        public Amane.Time.CalendarEventScheduler Calendar { get; private set; }
        public ExperienceSystem Experience { get; private set; }

        public GamePhase CurrentPhase =>
            Machine?.Current is PhaseStateBase p ? p.Phase : GamePhase.Boot;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Initialize();
        }

        private void Initialize()
        {
            Events = new EventChannel();
            Stats = new InnerStatSet();
            Time = new TimeManager(Events);
            Bonds = new BondManager(Events);
            Bonds.SeedDesignBonds();
            Save = new JsonSaveSystem(SerializeState, DeserializeState);
            Calendar = new Amane.Time.CalendarEventScheduler();
            Calendar.SeedStoryEvents();
            Experience = new ExperienceSystem();

            // パーティメンバーをEXPシステムに登録
            Experience.Register("yomi", 1);
            Experience.Register("akari", 1);
            Experience.Register("ritsu", 1);
            Experience.Register("ren", 1);

            SeedDeadlines();

            Machine = new StateMachine();
            Machine.Register(new BootState(this));
            Machine.Register(new TitleState(this));
            Machine.Register(new FieldState(this));
            Machine.Register(new BattleState(this));
            Machine.Register(new DialogueState(this));
            Machine.ChangeTo<BootState>();
        }

        // DESIGN.md 月次デッドラインの初期登録（第1幕分の代表例）。
        private void SeedDeadlines()
        {
            // 4/1 = DayIndex 1。5月末＝ 30(4月) + 31(5月) = 61。
            Time.RegisterDeadline(new Deadline("case_mizuki", "美月の心象", new GameDate(61)));
            // 6月末＝ 61 + 30 = 91。
            Time.RegisterDeadline(new Deadline("case_ritsu", "律の父の言伝", new GameDate(91)));
            // 7月末＝ 91 + 31 = 122。
            Time.RegisterDeadline(new Deadline("case_ren", "蓮の祖母", new GameDate(122)));
        }

        private void Update()
        {
            Machine?.Tick(UnityEngine.Time.deltaTime);
        }

        // ---- フィールド/UI から呼ばれる薄いファサード ----

        public void StartNewGame()
        {
            // TODO: 新規データ初期化（章ロード）。まずはフィールドへ。
            Machine.ChangeTo<FieldState>();
        }

        public void GoToTitle() => Machine.ChangeTo<TitleState>();

        public void EnterBattle() => Machine.ChangeTo<BattleState>();
        public void ReturnToField() => Machine.ChangeTo<FieldState>();

        private SaveData SerializeState()
        {
            var data = new SaveData
            {
                dayIndex = Time.Today.DayIndex,
                currentSlot = (int)Time.CurrentSlot,
                actionPoints = Time.ActionPoints
            };
            foreach (Amane.Stat.InnerStat s in System.Enum.GetValues(typeof(Amane.Stat.InnerStat)))
                data.innerStats.Add(new StatEntry { stat = s.ToString(), points = Stats.GetPoints(s) });
            foreach (var bond in Bonds.All)
                data.bonds.Add(new BondEntry { id = bond.Id, rank = bond.Rank, points = bond.PointsInRank });
            foreach (var d in Time.Deadlines)
                if (d.Cleared) data.clearedDeadlines.Add(d.CaseId);
            return data;
        }

        private void DeserializeState(SaveData data)
        {
            // Restore time/stats/bonds from save data.
            // Full restore requires resettable setters on TimeManager etc. —
            // placeholder until those are added.
            Debug.Log($"[Save] Restoring day {data.dayIndex}, slot {data.currentSlot}");
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
    }
}
