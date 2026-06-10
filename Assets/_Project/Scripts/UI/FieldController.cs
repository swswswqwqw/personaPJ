using UnityEngine;
using Amane.Core;
using Amane.Time;
using Amane.Stat;
using Amane.Dialogue;
using Amane.UI.Effects;
using Amane.Field;
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

        private DialogueRunner _dialogueRunner;
        private IDisposable _bondRankUpSub;

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

            // 絆ランクアップイベント
            var gm = GameManager.Instance;
            if (gm != null)
                _bondRankUpSub = gm.Events.Subscribe<BondRankUpEvent>(OnBondRankUp);

            _calendar?.Refresh();
            CheckCalendarEvents(gm);
        }

        private void OnDisable()
        {
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

            switch (type)
            {
                case LocationType.NPC:
                    gm.Time.SpendActionPoint();
                    StartBondDialogue(id);
                    break;
                case LocationType.Dungeon:
                    gm.Time.Dive();
                    StartDiveTransition();
                    break;
                case LocationType.Study:
                    gm.Time.SpendActionPoint();
                    gm.Stats.Add(InnerStat.Intellect, 5);
                    AdvanceTimeWithTransition(gm);
                    break;
                case LocationType.Shop:
                    gm.Time.SpendActionPoint();
                    gm.Stats.Add(InnerStat.Intellect, 3);
                    gm.Stats.Add(InnerStat.Empathy, 2);
                    AdvanceTimeWithTransition(gm);
                    break;
                case LocationType.Job:
                    gm.Time.SpendActionPoint();
                    gm.Stats.Add(InnerStat.Expression, 3);
                    gm.Stats.Add(InnerStat.Composure, 2);
                    AdvanceTimeWithTransition(gm);
                    break;
                case LocationType.Meditate:
                    gm.Time.SpendActionPoint();
                    gm.Stats.Add(InnerStat.Composure, 5);
                    AdvanceTimeWithTransition(gm);
                    break;
                case LocationType.Home:
                    AdvanceTimeWithTransition(gm);
                    break;
            }
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
                    // 古書堂 — 知性＋慈しみ
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
            }
        }

        private void AdvanceTimeWithTransition(GameManager gm)
        {
            var transition = TransitionEffect.Instance;
            if (transition != null)
            {
                string nextSlotName = gm.Time.CurrentSlot switch
                {
                    TimeSlot.Morning => "授業",
                    TimeSlot.Class => "放課後",
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
            var transition = TransitionEffect.Instance;
            if (transition != null)
            {
                transition.PlayDiveTransition(() =>
                {
                    GameManager.Instance?.Machine.ChangeTo<BattleState>();
                });
            }
            else
            {
                GameManager.Instance?.Machine.ChangeTo<BattleState>();
            }
        }

        private void CheckCalendarEvents(GameManager gm)
        {
            if (gm == null) return;
            var pending = gm.Calendar.GetPendingEvents(gm.Time.Today);
            foreach (var evt in pending)
            {
                evt.MarkTriggered();
                gm.Events.Publish(new CalendarEventTriggered(evt.Id, evt.DisplayName, evt.Type));
                Debug.Log($"[Event] {evt.DisplayName} ({evt.Type})");
            }
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
    }
}
