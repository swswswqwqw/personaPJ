using UnityEngine;
using Amane.Core;
using Amane.Time;
using Amane.Stat;
using Amane.Dialogue;
using Amane.UI.Effects;
using Amane.Field;
using Amane.Echo;
using Amane.Data;
using Amane.Battle;
using System;
using System.Collections.Generic;

namespace Amane.UI
{
    public sealed class FieldController : MonoBehaviour
    {
        [SerializeField] private CalendarUI _calendar;
        [SerializeField] private ActionSelectUI _actionSelect;
        [SerializeField] private DialogueUI _dialogueUI;
        [SerializeField] private BondRankUpEffect _bondRankUp;
        [SerializeField] private StatusUI _statusUI;
        [SerializeField] private FieldMap2D _fieldMap;
        [SerializeField] private FieldManager3D _fieldManager3D;
        [SerializeField] private FusionSelectUI _fusionSelectUI;

        private DialogueRunner _dialogueRunner;
        private IDisposable _bondRankUpSub;
        private IDisposable _dayChangedSub;
        private IDisposable _timeAdvancedSub;
        private bool _isCalendarEventDialogue;

        private void OnEnable()
        {
            // DialogueRunner は1度だけ生成
            if (_dialogueRunner == null)
            {
                _dialogueRunner = new DialogueRunner();

                if (_dialogueUI != null)
                    _dialogueUI.Bind(_dialogueRunner);

                _dialogueRunner.OnDialogueEnd += OnDialogueFinished;
            }

            // 3Dフィールドマネージャー（再有効化時にも購読する）
            if (_fieldManager3D != null)
                _fieldManager3D.OnInteracted += OnLocation3DInteracted;

            // 2Dマップのインタラクト（フォールバック）
            if (_fieldMap != null)
            {
                _fieldMap.OnLocationInteracted += OnLocationInteracted;
                if (_fieldMap.Player != null)
                    _fieldMap.Player.OnInteract += pos => _fieldMap.ProcessInteract(pos);
            }

            // レガシーアクション選択（フォールバック）
            if (_actionSelect != null)
                _actionSelect.OnActionSelected += OnAction;

            // 語り手融合UIのコールバック設定
            if (_fusionSelectUI != null)
            {
                _fusionSelectUI.OnFusionComplete += OnFusionComplete;
                _fusionSelectUI.OnCancelled      += OnFusionCancelled;
            }

            // 絆ランクアップ + 日付変更 + 時間帯変更イベント
            var gm = GameManager.Instance;
            if (gm != null)
            {
                _bondRankUpSub = gm.Events.Subscribe<BondRankUpEvent>(OnBondRankUp);
                _dayChangedSub = gm.Events.Subscribe<DayChangedEvent>(OnDayChanged);
                _timeAdvancedSub = gm.Events.Subscribe<TimeAdvancedEvent>(OnTimeAdvanced);
            }

            _calendar?.Refresh();
            CheckCalendarEvents(gm);
        }

        private void OnDisable()
        {
            if (_fusionSelectUI != null)
            {
                _fusionSelectUI.OnFusionComplete -= OnFusionComplete;
                _fusionSelectUI.OnCancelled      -= OnFusionCancelled;
            }

            if (_actionSelect != null)
                _actionSelect.OnActionSelected -= OnAction;

            if (_dialogueRunner != null)
                _dialogueRunner.OnDialogueEnd -= OnDialogueFinished;

            if (_fieldMap != null)
                _fieldMap.OnLocationInteracted -= OnLocationInteracted;

            if (_fieldManager3D != null)
                _fieldManager3D.OnInteracted -= OnLocation3DInteracted;

            _bondRankUpSub?.Dispose();
            _bondRankUpSub = null;
            _dayChangedSub?.Dispose();
            _dayChangedSub = null;
            _timeAdvancedSub?.Dispose();
            _timeAdvancedSub = null;
        }

        private void Update()
        {
            // 会話中の入力
            if (_dialogueRunner != null && _dialogueRunner.IsRunning && !_dialogueRunner.IsWaitingForChoice)
            {
                if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
                    _dialogueUI?.OnAdvanceInput();
            }

            // 会話中はプレイヤー移動を止める
            bool inDialogue = _dialogueRunner != null && _dialogueRunner.IsRunning;
            if (_fieldMap != null && _fieldMap.Player != null)
                _fieldMap.Player.CanMove = !inDialogue;
            if (_fieldManager3D != null && _fieldManager3D.Player != null)
                _fieldManager3D.Player.CanMove = !inDialogue;

            // Tabキーでステータス（WASDと競合しないように）
            if (Input.GetKeyDown(KeyCode.Tab) && (_dialogueRunner == null || !_dialogueRunner.IsRunning))
                _statusUI?.Toggle();
        }

        // ===== 3Dフィールドからのインタラクト =====
        private void OnLocation3DInteracted(string id, LocationType type)
        {
            var gm = GameManager.Instance;
            if (gm == null) return;

            bool isLunch = gm.Time.CurrentSlot == TimeSlot.Lunch;

            switch (type)
            {
                case LocationType.NPC:
                    if (isLunch)
                    {
                        // 昼休みは UseLunch() で行動消費。すでに使用済みなら無視。
                        if (!gm.Time.UseLunch()) return;
                        StartLunchBondDialogue(id, gm);
                    }
                    else
                    {
                        gm.Time.SpendActionPoint();
                        StartBondDialogue(id);
                    }
                    break;
                case LocationType.Dungeon:
                    if (isLunch) return; // 昼休みは潜行不可（学校内）
                    gm.Time.Dive();
                    StartDiveTransition();
                    break;
                case LocationType.Study:
                    if (isLunch)
                    {
                        if (!gm.Time.UseLunch()) return;
                        gm.Stats.Add(InnerStat.Intellect, 3);
                        Debug.Log("[Lunch] 図書室で読書した。知性+3");
                    }
                    else
                    {
                        gm.Time.SpendActionPoint();
                        gm.Stats.Add(InnerStat.Intellect, 5);
                    }
                    AdvanceTimeWithTransition(gm);
                    break;
                case LocationType.Shop:
                    if (isLunch) return; // 昼休みはバイト不可
                    // 古書堂「八雲」= 語り手融合UI
                    if (_fusionSelectUI != null && gm.Narrators != null)
                    {
                        _fusionSelectUI.Show(gm.Narrators);
                        return; // 融合完了コールバック(OnFusionComplete)でAdvanceTimeを呼ぶ
                    }
                    gm.Time.SpendActionPoint();
                    gm.Stats.Add(InnerStat.Intellect, 3);
                    gm.Stats.Add(InnerStat.Empathy, 2);
                    AdvanceTimeWithTransition(gm);
                    break;
                case LocationType.Job:
                    if (isLunch) return; // 昼休みはバイト不可
                    gm.Time.SpendActionPoint();
                    gm.Stats.Add(InnerStat.Expression, 3);
                    gm.Stats.Add(InnerStat.Composure, 2);
                    AdvanceTimeWithTransition(gm);
                    break;
                case LocationType.Meditate:
                    if (isLunch)
                    {
                        if (!gm.Time.UseLunch()) return;
                        gm.Stats.Add(InnerStat.Composure, 3);
                        Debug.Log("[Lunch] 屋上で静かに過ごした。静けさ+3");
                    }
                    else
                    {
                        gm.Time.SpendActionPoint();
                        gm.Stats.Add(InnerStat.Composure, 5);
                    }
                    AdvanceTimeWithTransition(gm);
                    break;
                case LocationType.Home:
                    AdvanceTimeWithTransition(gm);
                    break;
            }
        }

        // 昼休みの絆会話: ランク・ポイントは半分（短い会話のため）
        private void StartLunchBondDialogue(string bondId, GameManager gm)
        {
            var data = DialogueRunner.LoadFromStreamingAssets($"{bondId}_lunch.json");
            if (data == null)
            {
                // 専用JSONがなければ絆ポイント少量付与して終了
                gm.Bonds.GivePoints(bondId, 10);
                Debug.Log($"[Lunch] {bondId}と短く話した。絆ポイント+10");
                AdvanceTimeWithTransition(gm);
                return;
            }
            _dialogueUI?.Show();
            _dialogueRunner.Start(data);
        }

        // ===== 2Dマップからのインタラクト =====
        private void OnLocationInteracted(FieldLocation loc)
        {
            var gm = GameManager.Instance;
            if (gm == null) return;

            switch (loc.Type)
            {
                case LocationType.NPC:
                    gm.Time.SpendActionPoint();
                    StartBondDialogue(loc.Id);
                    break;

                case LocationType.Dungeon:
                    gm.Time.Dive();
                    StartDiveTransition();
                    break;

                case LocationType.Study:
                    gm.Time.SpendActionPoint();
                    if (gm.Stats.Add(InnerStat.Intellect, 5))
                        Debug.Log("[Stat] 知性がランクアップ！");
                    AdvanceTimeWithTransition(gm);
                    break;

                case LocationType.Shop:
                    // 古書堂「八雲」= 語り手融合UI（2Dフォールバック）
                    if (_fusionSelectUI != null && gm.Narrators != null)
                    {
                        _fusionSelectUI.Show(gm.Narrators);
                        return;
                    }
                    gm.Time.SpendActionPoint();
                    gm.Stats.Add(InnerStat.Intellect, 3);
                    gm.Stats.Add(InnerStat.Empathy, 2);
                    Debug.Log("[Field] 古書堂で読書した。知性+3 慈しみ+2");
                    AdvanceTimeWithTransition(gm);
                    break;

                case LocationType.Job:
                    gm.Time.SpendActionPoint();
                    gm.Stats.Add(InnerStat.Expression, 3);
                    gm.Stats.Add(InnerStat.Composure, 2);
                    Debug.Log("[Field] バイトした。ことのは+3 静けさ+2");
                    AdvanceTimeWithTransition(gm);
                    break;

                case LocationType.Meditate:
                    gm.Time.SpendActionPoint();
                    if (gm.Stats.Add(InnerStat.Composure, 5))
                        Debug.Log("[Stat] 静けさがランクアップ！");
                    AdvanceTimeWithTransition(gm);
                    break;

                case LocationType.Home:
                    AdvanceTimeWithTransition(gm);
                    break;
            }
        }

        // ===== レガシー行動メニュー（フォールバック）=====
        private void OnAction(FieldAction action)
        {
            var gm = GameManager.Instance;
            if (gm == null) return;

            switch (action)
            {
                case FieldAction.Study:
                    gm.Time.SpendActionPoint();
                    gm.Stats.Add(InnerStat.Intellect, 5);
                    AdvanceTimeWithTransition(gm);
                    break;
                case FieldAction.Socialize:
                    gm.Time.SpendActionPoint();
                    StartBondDialogue("akari");
                    break;
                case FieldAction.PartTimeJob:
                    gm.Time.SpendActionPoint();
                    gm.Stats.Add(InnerStat.Expression, 3);
                    AdvanceTimeWithTransition(gm);
                    break;
                case FieldAction.Meditate:
                    gm.Time.SpendActionPoint();
                    gm.Stats.Add(InnerStat.Composure, 5);
                    AdvanceTimeWithTransition(gm);
                    break;
                case FieldAction.Dive:
                    gm.Time.Dive();
                    StartDiveTransition();
                    break;
                case FieldAction.GoHome:
                    AdvanceTimeWithTransition(gm);
                    break;

                case FieldAction.MidnightDive:
                    // 深夜強行潜行（DESIGN.md 9-2）: AP消費なし・翌日疲労デバフ
                    if (!gm.Stats.Meets(InnerStat.Composure, TimeManager.MidnightDiveComposureRank))
                    {
                        Debug.Log("[MidnightDive] 静けさが足りない（4以上必要）");
                        break;
                    }
                    if (gm.Time.ForceDive())
                        StartMidnightDiveTransition();
                    break;

                case FieldAction.LunchChat:
                    // 昼休み: 近くにいるキャラとの短会話（ランダムに選択）
                    if (gm.Time.UseLunch())
                        StartLunchChat(gm);
                    break;

                case FieldAction.LunchLibrary:
                    // 昼休み: 図書室で読書（知性+3）
                    if (gm.Time.UseLunch())
                    {
                        gm.Stats.Add(InnerStat.Intellect, 3);
                        Debug.Log("[Lunch] 図書室で読書した。知性+3");
                        AdvanceTimeWithTransition(gm);
                    }
                    break;

                case FieldAction.LunchSkip:
                    // 昼休み: 何もしない（放課後へスキップ）
                    AdvanceTimeWithTransition(gm);
                    break;

                case FieldAction.LunchCanteen:
                    // 昼休み: 購買（消耗品入手 + 度胸+1）
                    if (gm.Time.UseLunch())
                        DoLunchCanteen(gm);
                    break;

                case FieldAction.LunchRooftop:
                    // 昼休み: 屋上（静けさ+5 + 演出テキスト）
                    if (gm.Time.UseLunch())
                        DoLunchRooftop(gm);
                    break;

                case FieldAction.LunchHealthRoom:
                    // 昼休み: 保健室（慈しみ+3・佳乃への導線）
                    if (gm.Time.UseLunch())
                        DoLunchHealthRoom(gm);
                    break;
            }
        }

        // 昼休み会話: ランダムに灯里/律/蓮/渚のうちひとりを選んで短い会話を始める
        private void StartLunchChat(GameManager gm)
        {
            // 日数に応じてキャラを選択（循環）。渚は9月解放想定のため絆rank1以上で追加。
            bool nagisaMet = gm.Bonds.Get("nagisa")?.Rank >= 1;
            string[] lunchChars = nagisaMet
                ? new[] { "akari", "ritsu", "ren", "nagisa" }
                : new[] { "akari", "ritsu", "ren" };
            string bondId = lunchChars[gm.Time.Today.DayIndex % lunchChars.Length];

            var data = DialogueRunner.LoadFromStreamingAssets($"{bondId}_lunch.json");
            if (data == null)
            {
                // JSONなければ絆ポイント少量付与して終了
                gm.Bonds.GivePoints(bondId, 10);
                AdvanceTimeWithTransition(gm);
                return;
            }
            _dialogueUI?.Show();
            _dialogueRunner.Start(data);
        }

        private void DoLunchCanteen(GameManager gm)
        {
            // 購買アイテム: 薬草茶（HP30回復）を1個付与
            var tea = ScriptableObject.CreateInstance<ItemData>();
            tea.itemName = "薬草茶";
            tea.description = "購買で売っている、体に良さそうなお茶。HP30回復。";
            tea.category = ItemCategory.Consumable;
            tea.effect = ItemEffect.HealHP;
            tea.effectValue = 30;
            tea.buyPrice = 150;
            InventoryManager.Instance?.AddItem(tea, 1);

            gm.Stats.Add(InnerStat.Courage, 1);
            Debug.Log("[Lunch] 購買で薬草茶を買った。HP+30アイテム入手 / 度胸+1");
            AdvanceTimeWithTransition(gm);
        }

        private void DoLunchRooftop(GameManager gm)
        {
            gm.Stats.Add(InnerStat.Composure, 5);

            // 屋上の情景をダイアログ演出で表示
            var data = new DialogueData
            {
                id = "lunch_rooftop",
                title = "屋上",
                bondId = "",
                bondPointsOnComplete = 0,
                lines = new List<DialogueLine>
                {
                    new() { speakerId = "narrator", text = "屋上。授業中はひとの来ない場所。", emotion = "neutral", preSilence = 0.3f },
                    new() { speakerId = "narrator", text = "霧に滲む雨音市が眼下に広がる。水路が光の帯になって、街の中を流れている。", emotion = "neutral", preSilence = 0.6f },
                    new() { speakerId = "narrator", text = "風の音だけがある。言葉は要らない。", emotion = "neutral", preSilence = 0.8f },
                    new() { speakerId = "narrator", text = "——静けさが、少しだけ深くなった気がした。", emotion = "neutral", preSilence = 0.5f }
                }
            };
            Debug.Log("[Lunch] 屋上でひとり過ごした。静けさ+5");
            _dialogueUI?.Show();
            _dialogueRunner.Start(data);
        }

        private void DoLunchHealthRoom(GameManager gm)
        {
            gm.Stats.Add(InnerStat.Empathy, 3);

            // 保健室: 佳乃のランク進行に基づく会話（rank0→intro、rank1以降→rank{n}.json）
            int kanoRank = gm.Bonds.Get("kano")?.Rank ?? 0;
            string kanoFile = kanoRank == 0 ? "kano_intro.json" : $"kano_rank{kanoRank}.json";
            var kanoData = DialogueRunner.LoadFromStreamingAssets(kanoFile);
            if (kanoData == null && kanoRank > 0)
                kanoData = DialogueRunner.LoadFromStreamingAssets("kano_intro.json");
            if (kanoData != null)
            {
                _dialogueUI?.Show();
                _dialogueRunner.Start(kanoData);
                return;
            }

            var data = new DialogueData
            {
                id = "lunch_healthroom",
                title = "保健室",
                bondId = "",
                bondPointsOnComplete = 0,
                lines = new List<DialogueLine>
                {
                    new() { speakerId = "narrator", text = "保健室。カーテンが静かに揺れている。", emotion = "neutral", preSilence = 0.3f },
                    new() { speakerId = "narrator", text = "窓の外は雨。消毒液の匂いと、微かな薬草のお茶の香り。", emotion = "neutral", preSilence = 0.5f },
                    new() { speakerId = "narrator", text = "ベッドのカーテン越しに、小さな声が聞こえる気がした。\n——気のせいかもしれない。", emotion = "neutral", preSilence = 0.6f },
                    new() { speakerId = "narrator", text = "この場所には、言えない痛みが集まってくる。", emotion = "neutral", preSilence = 0.8f },
                    new() { speakerId = "narrator", text = "——慈しみが、少しだけ深くなった気がした。", emotion = "neutral", preSilence = 0.5f }
                }
            };
            Debug.Log("[Lunch] 保健室で過ごした。慈しみ+3");
            _dialogueUI?.Show();
            _dialogueRunner.Start(data);
        }

        private void AdvanceTimeWithTransition(GameManager gm)
        {
            var transition = TransitionEffect.Instance;
            if (transition != null)
            {
                string nextSlotName = gm.Time.CurrentSlot switch
                {
                    TimeSlot.Morning => "授業",
                    TimeSlot.Class => "昼休み",
                    TimeSlot.Lunch => "放課後",
                    TimeSlot.AfterSchool => "夜",
                    TimeSlot.Evening => "深夜",
                    TimeSlot.LateNight => "翌朝",
                    _ => ""
                };
                transition.PlayTimeAdvance(nextSlotName, () =>
                {
                    gm.Time.AdvanceSlot();
                    _calendar?.Refresh();
                });
            }
            else
            {
                gm.Time.AdvanceSlot();
                _calendar?.Refresh();
            }
        }

        private void StartDiveTransition()
        {
            // MigenkaiManagerが存在する場合はダンジョン探索ループへ
            if (MigenkaiManager.Instance != null)
            {
                var dungeon = CreateDefaultDungeon();
                MigenkaiManager.Instance.EnterDungeon(dungeon);

                var transition = TransitionEffect.Instance;
                if (transition != null)
                    transition.PlayDiveTransition(() => GameManager.Instance?.Machine.ChangeTo<DungeonState>());
                else
                    GameManager.Instance?.Machine.ChangeTo<DungeonState>();
            }
            else
            {
                // フォールバック: 直接バトルへ
                var transition = TransitionEffect.Instance;
                if (transition != null)
                    transition.PlayDiveTransition(() => GameManager.Instance?.Machine.ChangeTo<BattleState>());
                else
                    GameManager.Instance?.Machine.ChangeTo<BattleState>();
            }
        }

        private void StartMidnightDiveTransition()
        {
            if (MigenkaiManager.Instance != null)
            {
                var dungeon = CreateDefaultDungeon();
                MigenkaiManager.Instance.EnterDungeon(dungeon);

                var transition = TransitionEffect.Instance;
                if (transition != null)
                    transition.PlayMidnightDiveTransition(() => GameManager.Instance?.Machine.ChangeTo<DungeonState>());
                else
                    GameManager.Instance?.Machine.ChangeTo<DungeonState>();
            }
            else
            {
                var transition = TransitionEffect.Instance;
                if (transition != null)
                    transition.PlayMidnightDiveTransition(() => GameManager.Instance?.Machine.ChangeTo<BattleState>());
                else
                    GameManager.Instance?.Machine.ChangeTo<BattleState>();
            }
        }

        private static MigenkaiData CreateDefaultDungeon()
        {
            var dungeon = UnityEngine.ScriptableObject.CreateInstance<MigenkaiData>();
            dungeon.dungeonName = "美月の心象";
            dungeon.description = "鐘が止まらない教室。黒板の言葉が、少しずつ消えていく。";
            dungeon.victimName = "長峰 美月";
            dungeon.victimBackground = "灯里の親友。届かなかったSOSが澱になった。";
            dungeon.suppressedEmotion = "助けを求める声";
            dungeon.totalFloors = 3;
            dungeon.deadlineMonth = 5;
            dungeon.deadlineDay = 31;
            dungeon.bossName = "沈黙の美月";
            dungeon.bossThematicMeaning = "「行かないで」と言えなかった後悔の具現";
            dungeon.floors = new[]
            {
                new MigenkaiFloorData { floorNumber = 1, floorName = "B1F — 静謐の廊下", minEnemyLevel = 1, maxEnemyLevel = 3 },
                new MigenkaiFloorData { floorNumber = 2, floorName = "B2F — 滲む教室", minEnemyLevel = 3, maxEnemyLevel = 5, hasMiniBoss = true, miniBossName = "言葉の壁" },
                new MigenkaiFloorData { floorNumber = 3, floorName = "言伝の間", minEnemyLevel = 5, maxEnemyLevel = 8 }
            };
            dungeon.dominantColor = new UnityEngine.Color(0.106f, 0.165f, 0.29f);
            dungeon.environmentTheme = "永遠に鳴り続けるまたねの教室";
            return dungeon;
        }

        private void OnDayChanged(DayChangedEvent evt)
        {
            _calendar?.Refresh();
            CheckCalendarEvents(GameManager.Instance);
        }

        private void OnTimeAdvanced(TimeAdvancedEvent evt)
        {
            _calendar?.Refresh();

            var gm = GameManager.Instance;
            if (gm == null) return;

            // 昼休みになったら昼休みメニューを表示（DESIGN.md 9-2: 昼休み行動枠）
            if (evt.Slot == TimeSlot.Lunch)
            {
                bool lunchUsed = gm.Time.LunchUsed;
                _actionSelect?.Show(TimeSlot.Lunch, 0, lunchUsed, false);
                Debug.Log($"[Lunch] 昼休みメニュー表示。使用済み: {lunchUsed}");
                return;
            }

            // 深夜になったら「眠る / 強行潜行」メニューを表示（DESIGN.md 9-2）
            if (evt.Slot != TimeSlot.LateNight) return;

            bool midnightUnlocked = gm.Stats.Meets(InnerStat.Composure, TimeManager.MidnightDiveComposureRank);

            _actionSelect?.Show(TimeSlot.LateNight, 0, true, midnightUnlocked);
            Debug.Log($"[LateNight] 深夜メニュー表示。強行潜行: {(midnightUnlocked ? "解放済み" : "未解放（静けさ4以上で解放）")}");
        }

        private void CheckCalendarEvents(GameManager gm)
        {
            if (gm == null) return;
            var pending = gm.Calendar.GetPendingEvents(gm.Time.Today);
            if (pending.Count == 0) return;

            // 複数イベントがある場合は最初の1つだけ処理（残りは翌フレームに持ち越し）
            var evt = pending[0];
            evt.MarkTriggered();
            gm.Events.Publish(new CalendarEventTriggered(evt.Id, evt.DisplayName, evt.Type));
            Debug.Log($"[Event] {evt.DisplayName} ({evt.Type})");

            // 対応するダイアログJSONがあれば自動起動
            TryStartCalendarEventDialogue(evt.Id, evt.DisplayName);
        }

        private void TryStartCalendarEventDialogue(string eventId, string displayName)
        {
            var data = DialogueRunner.LoadFromStreamingAssets($"event_{eventId}.json");
            if (data == null) return;

            _isCalendarEventDialogue = true;
            _dialogueUI?.Show();
            _dialogueRunner.Start(data);
        }

        private void StartBondDialogue(string bondId)
        {
            var gm = GameManager.Instance;
            int rank = gm?.Bonds.Get(bondId)?.Rank ?? 0;

            DialogueData data = null;

            if (rank == 0)
                data = DialogueRunner.LoadFromStreamingAssets($"{bondId}_intro.json");
            else
                data = DialogueRunner.LoadFromStreamingAssets($"{bondId}_rank{rank}.json");

            if (data == null && rank > 0)
                data = DialogueRunner.LoadFromStreamingAssets($"{bondId}_intro.json");

            if (data == null)
                data = CreateFallbackDialogue(bondId);

            _dialogueUI?.Show();
            _dialogueRunner.Start(data);
        }

        private void OnDialogueFinished(DialogueData data)
        {
            var gm = GameManager.Instance;
            if (gm != null && data.bondId != null && data.bondPointsOnComplete > 0)
                gm.Bonds.GivePoints(data.bondId, data.bondPointsOnComplete);

            // CalendarEventのダイアログは時間を進めない（日付変更後に発火するため）
            if (_isCalendarEventDialogue)
            {
                _isCalendarEventDialogue = false;
                _calendar?.Refresh();
                return;
            }

            AdvanceTimeWithTransition(gm);
        }

        private void OnBondRankUp(BondRankUpEvent evt)
        {
            if (_bondRankUp != null)
            {
                var gm = GameManager.Instance;
                string charName = gm?.Bonds.Get(evt.BondId)?.DisplayName ?? evt.BondId;
                _bondRankUp.Play(charName, evt.NewRank, () =>
                {
                    Debug.Log($"[Bond] {charName} → Rank {evt.NewRank}");
                });
            }
        }

        private static DialogueData CreateFallbackDialogue(string bondId)
        {
            return new DialogueData
            {
                id = $"{bondId}_fallback",
                title = "会話",
                bondId = bondId,
                bondPointsOnComplete = 10,
                lines = new List<DialogueLine>
                {
                    new() { speakerId = bondId, text = "……。", emotion = "neutral", preSilence = 0 },
                    new() { speakerId = "yomi", text = "（今日は少し話せた気がする）", emotion = "neutral", preSilence = 0 }
                }
            };
        }

        // ===== 語り手融合コールバック =====
        private void OnFusionComplete(Narrator result)
        {
            Debug.Log($"[Fusion] 語り手融合完了: {result.DisplayName} ({result.PrimaryElement})");
            var gm = GameManager.Instance;
            gm?.Time.SpendActionPoint();
            AdvanceTimeWithTransition(gm);
        }

        private void OnFusionCancelled()
        {
            Debug.Log("[Fusion] 語り手融合キャンセル");
        }
    }
}
