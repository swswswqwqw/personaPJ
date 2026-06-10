using UnityEditor;
using UnityEngine;
using Amane.Core;
using Amane.Time;
using Amane.Battle;
using Amane.Social;
using Amane.Stat;
using Amane.Dialogue;
using System.Collections.Generic;
using System.Linq;

public static class AmaneLogicTest
{
    [MenuItem("Amane/全ロジックテスト実行", false, 100)]
    public static void RunAllTests()
    {
        int passed = 0;
        int failed = 0;

        void Assert(bool condition, string testName)
        {
            if (condition) { passed++; Debug.Log($"  ✓ {testName}"); }
            else { failed++; Debug.LogError($"  ✗ FAIL: {testName}"); }
        }

        Debug.Log("===== 残響都市アマネ ロジックテスト =====\n");

        // --- GameDate ---
        Debug.Log("[GameDate]");
        var d1 = new GameDate(1);
        Assert(d1.Month == 4 && d1.Day == 1, "4/1 = DayIndex 1");
        var d31 = new GameDate(31);
        Assert(d31.Month == 5 && d31.Day == 1, "5/1 = DayIndex 31");
        var last = new GameDate(GameDate.TotalDays);
        Assert(last.Month == 3 && last.Day == 31, "学年末 = 3/31");
        Assert(last.IsLastDay, "IsLastDay at 3/31");

        // --- TimeManager ---
        Debug.Log("\n[TimeManager]");
        var events = new EventChannel();
        var tm = new TimeManager(events);
        Assert(tm.Today.DayIndex == 1, "初期日 = 4/1");
        Assert(tm.CurrentSlot == TimeSlot.Morning, "初期スロット = 朝");
        Assert(tm.ActionPoints == 2, "初期AP = 2");
        tm.AdvanceSlot();
        Assert(tm.CurrentSlot == TimeSlot.Class, "朝→授業");
        tm.AdvanceSlot();
        Assert(tm.CurrentSlot == TimeSlot.AfterSchool, "授業→放課後");
        Assert(tm.SpendActionPoint(), "AP消費成功");
        Assert(tm.ActionPoints == 1, "AP残1");
        tm.AdvanceSlot();
        Assert(tm.CurrentSlot == TimeSlot.Evening, "放課後→夜");

        var tm2 = new TimeManager(events);
        Assert(tm2.Dive(), "潜行成功");
        Assert(tm2.ActionPoints == 0, "潜行後AP = 0");
        Assert(tm2.CurrentSlot == TimeSlot.LateNight, "潜行後 = 深夜");
        Assert(!tm2.Dive(), "AP0で潜行失敗");

        // --- Deadline ---
        Debug.Log("\n[Deadline]");
        var dl = new Deadline("test", "テスト", new GameDate(10));
        Assert(dl.DaysLeft(new GameDate(7)) == 3, "残り3日");
        Assert(!dl.IsFailed(new GameDate(7)), "期限前は未失敗");
        Assert(dl.IsFailed(new GameDate(11)), "期限後は失敗");
        dl.MarkCleared();
        Assert(!dl.IsFailed(new GameDate(11)), "クリア後は失敗しない");

        // --- InnerStatSet ---
        Debug.Log("\n[InnerStatSet]");
        var stats = new InnerStatSet();
        Assert(stats.GetRank(InnerStat.Courage) == 0, "初期ランク = 0");
        stats.Add(InnerStat.Courage, 10);
        Assert(stats.GetRank(InnerStat.Courage) == 1, "10pt → ランク1");
        stats.Add(InnerStat.Courage, 20);
        Assert(stats.GetRank(InnerStat.Courage) == 2, "30pt → ランク2");
        Assert(stats.Meets(InnerStat.Courage, 2), "ランク2以上 = true");
        Assert(!stats.Meets(InnerStat.Courage, 3), "ランク3以上 = false");

        // --- Bond & BondManager ---
        Debug.Log("\n[Bond & BondManager]");
        var bondMgr = new BondManager(events);
        bondMgr.SeedDesignBonds();
        var akari = bondMgr.Get("akari");
        Assert(akari != null, "灯里の絆取得");
        Assert(akari.Arcana == Arcana.Sun, "灯里 = 太陽");
        Assert(akari.Rank == 0, "初期ランク = 0");
        bondMgr.GivePoints("akari", 100);
        Assert(akari.Rank == 1, "100pt → ランク1");
        bondMgr.SetNarratorArcanaOwned(Arcana.Sun, true);
        bondMgr.GivePoints("akari", 100);
        Assert(akari.Rank >= 2, "太陽の語り手所持で x1.5 → ランク2+");

        // --- Affinity ---
        Debug.Log("\n[AffinityTable]");
        var aff = AffinityTable.Build(
            weak: new[] { Element.Fire },
            resist: new[] { Element.Ice },
            nullify: new[] { Element.Light }
        );
        Assert(aff.Get(Element.Fire) == Affinity.Weak, "火 = 弱点");
        Assert(aff.Get(Element.Ice) == Affinity.Resist, "氷 = 耐性");
        Assert(aff.Get(Element.Light) == Affinity.Null, "光 = 無効");
        Assert(aff.Get(Element.Thunder) == Affinity.Normal, "雷 = 通常");
        Assert(aff.Get(Element.Almighty) == Affinity.Normal, "万能 = 常に通常");

        // --- Combatant ---
        Debug.Log("\n[Combatant]");
        var hero = PartyFactory.CreateProtagonist();
        Assert(hero.IsAlive, "主人公生存");
        Assert(hero.Hp == hero.MaxHp, "HP満タン");
        hero.TakeDamage(50);
        Assert(hero.Hp == hero.MaxHp - 50, "50ダメージ後のHP");
        hero.Heal(20);
        Assert(hero.Hp == hero.MaxHp - 30, "20回復後のHP");
        Assert(hero.SpendSp(4), "SP4消費成功");
        Assert(!hero.SpendSp(hero.MaxSp), "SP不足で消費失敗");

        // --- DamageCalculator ---
        Debug.Log("\n[DamageCalculator]");
        var attacker = new Combatant("a", "A", true, 100, 50, 20, 10, 20, 10, 15,
            AffinityTable.AllNormal());
        var weakTarget = new Combatant("b", "B", false, 200, 30, 15, 10, 15, 10, 10,
            AffinityTable.WithWeak(Element.Fire));
        var fireSkill = new Skill("fire", "ファイア", Element.Fire, 40, 5, TargetType.SingleEnemy);
        var result = DamageCalculator.Calculate(attacker, weakTarget, fireSkill);
        Assert(result.Type == HitType.Weak, "弱点ヒット判定");
        Assert(result.Damage > 0, "ダメージ > 0");
        Assert(result.TriggersOneMore, "One More発生");
        Assert(result.CausedDown, "DOWN発生");

        var nullTarget = new Combatant("c", "C", false, 200, 30, 15, 10, 15, 10, 10,
            AffinityTable.Build(nullify: new[] { Element.Fire }));
        var nullResult = DamageCalculator.Calculate(attacker, nullTarget, fireSkill);
        Assert(nullResult.Type == HitType.Null, "無効判定");
        Assert(nullResult.Damage == 0, "無効ダメージ = 0");

        // --- TurnSystem ---
        Debug.Log("\n[TurnSystem]");
        var turnSys = new TurnSystem();
        var fast = new Combatant("f", "Fast", true, 100, 50, 10, 10, 10, 10, 30, AffinityTable.AllNormal());
        var slow = new Combatant("s", "Slow", false, 100, 50, 10, 10, 10, 10, 5, AffinityTable.AllNormal());
        turnSys.BuildOrder(new[] { slow, fast });
        Assert(turnSys.Current == fast, "素早さ順: Fast先行");
        turnSys.Advance();
        Assert(turnSys.Current == slow, "次: Slow");

        // --- BattleManager ---
        Debug.Log("\n[BattleManager]");
        var battleEvents = new EventChannel();
        var battle = new BattleManager(battleEvents);
        var party = PartyFactory.CreateDefaultParty();
        var enemies = EnemyFactory.CreateTutorialEncounter();
        battle.StartBattle(party, enemies);
        Assert(battle.Phase == BattlePhase.PlayerTurn || battle.Phase == BattlePhase.EnemyTurn, "戦闘開始");
        Assert(battle.Party.Count == 4, "パーティ4人");
        Assert(battle.Enemies.Count == 3, "敵3体");
        float bonus0 = battle.GetKotsugiBonus();
        Assert(bonus0 == 0f, "初期言継ぎボーナス = 0");

        // --- EnemyAI ---
        Debug.Log("\n[EnemyAI]");
        var aiEnemy = EnemyFactory.CreateSilentOri();
        var aiParty = new List<Combatant> { PartyFactory.CreateProtagonist() };
        var aiAction = EnemyAI.DecideAction(aiEnemy, aiParty);
        Assert(aiAction != null, "AIが行動を決定");
        Assert(aiAction.Type == ActionType.Skill, "AIがスキルを使用");

        // --- DialogueRunner ---
        Debug.Log("\n[DialogueRunner]");
        var runner = new DialogueRunner();
        var dialogueData = new DialogueData
        {
            id = "test",
            title = "テスト会話",
            bondId = "akari",
            bondPointsOnComplete = 10,
            lines = new List<DialogueLine>
            {
                new() { speakerId = "akari", text = "こんにちは", emotion = "smile", preSilence = 0 },
                new() { speakerId = "yomi", text = "ああ", emotion = "neutral", preSilence = 0 },
                new() { speakerId = "akari", text = "また明日ね", emotion = "smile", preSilence = 0 }
            }
        };
        runner.Start(dialogueData);
        Assert(runner.IsRunning, "会話開始");
        Assert(runner.CurrentLine.speakerId == "akari", "最初の話者 = 灯里");
        runner.Advance();
        Assert(runner.CurrentLine.speakerId == "yomi", "2行目 = 主人公");
        runner.Advance();
        Assert(runner.CurrentLine.text == "また明日ね", "3行目テキスト");
        bool ended = false;
        runner.OnDialogueEnd += _ => ended = true;
        runner.Advance();
        Assert(!runner.IsRunning && ended, "会話終了");

        // --- CalendarEventScheduler ---
        Debug.Log("\n[CalendarEventScheduler]");
        var scheduler = new CalendarEventScheduler();
        scheduler.SeedStoryEvents();
        Assert(scheduler.AllEvents.Count >= 15, "ストーリーイベント15以上登録");
        var day3events = scheduler.GetPendingEvents(new GameDate(3));
        Assert(day3events.Count > 0, "Day3にイベントあり（覚醒）");
        day3events[0].MarkTriggered();
        var day3again = scheduler.GetPendingEvents(new GameDate(3));
        Assert(day3again.Count(e => e.Id == "awakening") == 0, "トリガー済みは再発火しない");

        // --- Summary ---
        Debug.Log($"\n===== 結果: {passed} passed / {failed} failed =====");
        if (failed == 0)
            Debug.Log("全テスト合格！ プロトタイプの準備完了です。");
        else
            Debug.LogWarning($"{failed}件のテストが失敗しています。修正してください。");

        EditorUtility.DisplayDialog("テスト結果",
            $"合格: {passed}\n失敗: {failed}\n\n" +
            (failed == 0 ? "全テスト合格！プロトタイプの準備完了です。" : "失敗があります。Consoleを確認してください。"),
            "OK");
    }
}
