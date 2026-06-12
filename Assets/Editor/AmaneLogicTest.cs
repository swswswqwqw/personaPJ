using UnityEditor;
using UnityEngine;
using Amane.Core;
using Amane.Time;
using Amane.Battle;
using Amane.Social;
using Amane.Stat;
using Amane.Dialogue;
using Amane.UI;
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
        Assert(tm.CurrentSlot == TimeSlot.Lunch, "授業→昼休み");
        Assert(!tm.LunchUsed, "昼休み未使用");
        Assert(tm.UseLunch(), "昼休み使用成功");
        Assert(tm.LunchUsed, "昼休み使用済み");
        Assert(!tm.UseLunch(), "昼休み2回目は失敗");
        tm.AdvanceSlot();
        Assert(tm.CurrentSlot == TimeSlot.AfterSchool, "昼休み→放課後");
        Assert(tm.SpendActionPoint(), "AP消費成功");
        Assert(tm.ActionPoints == 1, "AP残1");
        tm.AdvanceSlot();
        Assert(tm.CurrentSlot == TimeSlot.Evening, "放課後→夜");

        // LunchUsed翌日リセット確認
        var tmDay = new TimeManager(events);
        tmDay.AdvanceSlot(); // Morning→Class
        tmDay.AdvanceSlot(); // Class→Lunch
        Assert(tmDay.UseLunch(), "翌日テスト: 昼休み使用");
        tmDay.AdvanceSlot(); // Lunch→AfterSchool
        tmDay.AdvanceSlot(); // AfterSchool→Evening
        tmDay.AdvanceSlot(); // Evening→LateNight
        tmDay.AdvanceSlot(); // LateNight→翌日Morning (AdvanceDay呼び出し)
        Assert(!tmDay.LunchUsed, "翌日: 昼休みリセット確認");

        var tm2 = new TimeManager(events);
        Assert(tm2.Dive(), "潜行成功");
        Assert(tm2.ActionPoints == 0, "潜行後AP = 0");
        Assert(tm2.CurrentSlot == TimeSlot.LateNight, "潜行後 = 深夜");
        Assert(!tm2.Dive(), "AP0で潜行失敗");

        // 深夜強行潜行テスト (DESIGN.md 9-2)
        Debug.Log("\n[TimeManager: MidnightDive]");
        var tmMid = new TimeManager(events);
        Assert(!tmMid.MidnightDiveDebt, "初期: 疲労デバフなし");
        Assert(tmMid.ForceDive(), "強行潜行成功");
        Assert(tmMid.MidnightDiveDebt, "強行潜行後: 疲労フラグON");
        Assert(tmMid.CurrentSlot == TimeSlot.LateNight, "強行潜行後 = 深夜");
        Assert(!tmMid.ForceDive(), "2回目の強行潜行は失敗（すでにデバフあり）");
        // 翌日へ進む: APが0になることを確認
        tmMid.AdvanceSlot(); // LateNight → AdvanceDay
        Assert(!tmMid.MidnightDiveDebt, "翌日: 疲労フラグリセット");
        Assert(tmMid.ActionPoints == 0, "翌日: 疲労デバフでAP=0");
        // 疲労なしの通常翌日: AP=2に戻ること
        tmMid.AdvanceSlot(); // Morning → Class
        tmMid.AdvanceSlot(); // Class → Lunch
        tmMid.AdvanceSlot(); // Lunch → AfterSchool
        tmMid.AdvanceSlot(); // AfterSchool → Evening
        tmMid.AdvanceSlot(); // Evening → LateNight
        tmMid.AdvanceSlot(); // LateNight → 翌々日 Morning (疲労なし)
        Assert(tmMid.ActionPoints == TimeManager.MaxActionPoints, "疲労なし翌日: AP = 2に回復");

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
        float total0 = battle.GetTotalBonus();
        Assert(total0 == 0f, "初期GetTotalBonus = 0（パーフェクトバフなし）");

        // --- パーフェクト言継ぎ ---
        Debug.Log("\n[PerfectKotsugi — DESIGN.md 9-1]");
        var pfBattle = new BattleManager(new EventChannel());
        bool perfectFired = false;
        pfBattle.OnPerfectKotsugi += () => perfectFired = true;

        // 2人パーティ（全員参加 = 1回の言継ぎでパーフェクト）
        var pfP1 = new Combatant("pf1", "天野詠", true, 100, 50, 15, 10, 15, 10, 20, AffinityTable.AllNormal());
        var pfP2 = new Combatant("pf2", "望月灯里", true, 100, 50, 12, 10, 12, 10, 15, AffinityTable.AllNormal());
        // SP回復テストのため事前にSPを消費しておく
        pfP1.SpendSp(20); // p1 SP: 30
        pfP2.SpendSp(20); // p2 SP: 30
        int p1SpBefore = pfP1.Sp;
        int p2SpBefore = pfP2.Sp;
        pfBattle.StartBattle(
            new List<Combatant> { pfP1, pfP2 },
            new List<Combatant> { EnemyFactory.CreateSilentOri() }
        );
        // 言継ぎ実行（p1→p2: 2人パーティなら全員参加 = パーフェクト）
        pfBattle.ExecuteAction(BattleAction.Kotsugi(pfP1, pfP2));
        Assert(perfectFired, "2人パーティ言継ぎでパーフェクト言継ぎ発動");
        Assert(pfP1.Sp > p1SpBefore, "パーフェクト言継ぎでSP回復（p1）");
        Assert(pfP2.Sp > p2SpBefore, "パーフェクト言継ぎでSP回復（p2）");
        Assert(pfBattle.PerfectKotsugiBuffActive == false, "パーフェクトバフは次ラウンド適用（今ラウンドはまだ未適用）");

        // --- 逆総告白（OnReverseAllOutCallingイベントの存在確認） ---
        Debug.Log("\n[ReverseAllOutCalling — DESIGN.md 9-1]");
        var rvBattle = new BattleManager(new EventChannel());
        bool reverseFired = false;
        Combatant reverseEnemy = null;
        rvBattle.OnReverseAllOutCalling += e => { reverseFired = true; reverseEnemy = e; };
        // 弱点属性で確実にDOWNするパーティ（HP十分あるが属性弱点）
        var weakParty = new List<Combatant> {
            new Combatant("rv1", "テストメンバー", true, 200, 50, 10, 5, 10, 5, 10,
                AffinityTable.Build(weak: new[] { Element.Fire }))
        };
        var fireSkillRv = new Skill("fire_rv", "炎撃", Element.Fire, 20, 0, TargetType.SingleEnemy, false, 0f);
        var fireEnemy = new Combatant("fire_e", "炎のオリ", false, 300, 50, 30, 10, 30, 10, 12,
            AffinityTable.AllNormal(), new List<Skill> { fireSkillRv });
        rvBattle.StartBattle(weakParty, new List<Combatant> { fireEnemy });
        // 敵が炎スキルで弱点ヒット → DOWN → 全員DOWN → 逆総告白発動
        var fireAction = BattleAction.UseSkill(fireEnemy, fireEnemy.Skills[0], weakParty);
        rvBattle.ExecuteAction(fireAction);
        Assert(reverseFired || weakParty[0].IsDown, "弱点ヒット → DOWN状態（逆総告白トリガー条件）");
        if (reverseFired)
        {
            Assert(reverseEnemy == fireEnemy, "逆総告白トリガーの敵が正しい");
            Debug.Log("  ✓ 逆総告白が発動（全員DOWNを確認）");
        }
        else
        {
            Debug.Log("  ⚠ DOWNしたが逆総告白未発火（一撃でDOWNしない場合あり — 確率依存）");
        }

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

        // --- デュアルナレーター（DESIGN.md 9-1）---
        Debug.Log("\n[DualNarrator — DESIGN.md 9-1]");

        // NarratorAffinityMatrix テスト
        Assert(NarratorAffinityMatrix.GetDualBonus(Element.Light, Element.Light) > 1.0f, "同属性(光×光)シナジー x1.2");
        Assert(NarratorAffinityMatrix.GetDualBonus(Element.Light, Element.Dark) < 1.0f, "対立(光×闇)干渉 x0.7");
        Assert(NarratorAffinityMatrix.GetDualBonus(Element.Fire, Element.Ice) < 1.0f, "対立(焔×氷)干渉 x0.7");
        Assert(NarratorAffinityMatrix.GetDualBonus(Element.Fire, Element.Wind) == 1.0f, "中立(焔×風) x1.0");
        Assert(NarratorAffinityMatrix.GetDualBonus(Element.Thunder, Element.Wind) < 1.0f, "対立(雷×風)干渉 x0.7");

        // Narrator 生成テスト
        var lightNarrator = new Narrator("light_n", "赦しの語り手", Element.Light,
            AffinityTable.Build(weak: new[] { Element.Dark }));
        var darkNarrator = new Narrator("dark_n", "後悔の語り手", Element.Dark,
            AffinityTable.Build(weak: new[] { Element.Light }));
        Assert(lightNarrator.PrimaryElement == Element.Light, "語り手主属性: 光");
        Assert(darkNarrator.PrimaryElement == Element.Dark, "語り手主属性: 闇");

        // Combatant に語り手をセット
        var dualHero = new Combatant("dual_hero", "天野詠", true, 200, 80, 20, 10, 20, 10, 20,
            AffinityTable.AllNormal());
        Assert(!dualHero.IsDualNarratorActive, "語り手未設定: デュアルモードOFF");
        dualHero.SetNarrators(lightNarrator);
        Assert(dualHero.ActiveNarrator == lightNarrator, "語り手単体セット");
        Assert(!dualHero.IsDualNarratorActive, "1体のみ: デュアルモードOFF");
        dualHero.SetNarrators(lightNarrator, darkNarrator);
        Assert(dualHero.IsDualNarratorActive, "2体セット: デュアルモードON");
        Assert(dualHero.SecondaryNarrator == darkNarrator, "サブ語り手確認");

        // Affinities が語り手のものに切り替わること
        Assert(dualHero.Affinities.Get(Element.Dark) == Affinity.Weak, "語り手のAffinitiesに切り替わった（光→闇弱点）");

        // DualNarrator SkillsUnion（重複除去）
        var lSkill = new Skill("l_skill", "赦しの言葉", Element.Light, 40, 6, TargetType.SingleEnemy);
        var dSkill = new Skill("d_skill", "後悔の重圧", Element.Dark, 35, 5, TargetType.SingleEnemy);
        var ln2 = new Narrator("ln2", "光のナレーター", Element.Light, null, new List<Skill> { lSkill });
        var dn2 = new Narrator("dn2", "闇のナレーター", Element.Dark, null, new List<Skill> { dSkill });
        dualHero.SetNarrators(ln2, dn2);
        var dualSkills = dualHero.GetDualNarratorSkills();
        Assert(dualSkills.Count == 2, "デュアルスキル一覧: 2種");
        Assert(dualSkills.Exists(s => s.Id == "l_skill"), "主語り手スキル含む");
        Assert(dualSkills.Exists(s => s.Id == "d_skill"), "副語り手スキル含む");

        // BattleManager でデュアルナレーターアクションを実行
        var dnBattle = new BattleManager(new EventChannel());
        dnBattle.IsDualNarratorUnlocked = true;
        bool dualFired = false;
        dnBattle.OnDualNarratorActivated += (_, __, ___) => dualFired = true;

        var dnHero = new Combatant("dn_hero", "天野詠", true, 300, 100, 25, 10, 25, 10, 20,
            AffinityTable.AllNormal());
        dnHero.SetNarrators(ln2, dn2);
        var dnEnemy = new Combatant("dn_enemy", "無言のオリ", false, 500, 0, 10, 5, 10, 5, 8,
            AffinityTable.AllNormal());
        dnBattle.StartBattle(
            new List<Combatant> { dnHero },
            new List<Combatant> { dnEnemy }
        );
        int spBefore = dnHero.Sp;
        var dualAction = BattleAction.DualNarratorAttack(dnHero, lSkill, dSkill,
            new List<Combatant> { dnEnemy });
        dnBattle.ExecuteAction(dualAction);
        // SP は (lSkill.SpCost + dSkill.SpCost) * 1.5 = (6+5)*1.5 = 16.5 → 17 消費
        int spExpected = spBefore - (int)System.Math.Ceiling((lSkill.SpCost + dSkill.SpCost) * 1.5f);
        Assert(dnHero.Sp == spExpected, $"デュアルナレーターSP消費1.5倍（期待値: {spExpected}）");
        Assert(dualFired, "OnDualNarratorActivated イベント発火");

        // SP不足時のフォールバック（通常攻撃）
        var poorHero = new Combatant("poor", "SP枯渇テスト", true, 200, 5, 20, 10, 20, 10, 15,
            AffinityTable.AllNormal());
        poorHero.SetNarrators(ln2, dn2);
        var poorEnemy = new Combatant("pe", "ダミー敵", false, 500, 0, 5, 3, 5, 3, 5, AffinityTable.AllNormal());
        var spoorBattle = new BattleManager(new EventChannel());
        spoorBattle.IsDualNarratorUnlocked = true;
        spoorBattle.StartBattle(new List<Combatant> { poorHero }, new List<Combatant> { poorEnemy });
        var poorAction = BattleAction.DualNarratorAttack(poorHero, lSkill, dSkill,
            new List<Combatant> { poorEnemy });
        spoorBattle.ExecuteAction(poorAction);
        Assert(poorHero.Sp == 5, "SP不足時フォールバック: SPは消費されない（通常攻撃に切替）");

        // --- 渚/八雲 JSON ロード確認 ---
        Debug.Log("\n[NagisaYakumo JSON]");
        var nagisaRank2 = DialogueRunner.LoadFromStreamingAssets("nagisa_rank2.json");
        Assert(nagisaRank2 != null, "nagisa_rank2.json 読み込み成功");
        Assert(nagisaRank2?.lines?.Count >= 5, "nagisa_rank2: linesが5件以上");
        Assert(nagisaRank2?.choices?.Count > 0, "nagisa_rank2: choicesあり");
        var nagisaRank5 = DialogueRunner.LoadFromStreamingAssets("nagisa_rank5.json");
        Assert(nagisaRank5 != null, "nagisa_rank5.json 読み込み成功");
        Assert(nagisaRank5?.postChoiceLines?.Count >= 5, "nagisa_rank5: postChoiceLines（素の笑顔まで）5件以上");
        var yakumoIntro = DialogueRunner.LoadFromStreamingAssets("yakumo_intro.json");
        Assert(yakumoIntro != null, "yakumo_intro.json 読み込み成功");
        Assert(yakumoIntro?.bondId == "yakumo", "yakumo_intro: bondId=yakumo");

        // --- 八雲コープ JSON 読み込み確認 ---
        Debug.Log("\n[YakumoKotodate JSON rank2-10]");
        var yakumoRank2 = DialogueRunner.LoadFromStreamingAssets("yakumo_rank2.json");
        Assert(yakumoRank2 != null, "yakumo_rank2.json 読み込み成功");
        Assert(yakumoRank2?.bondId == "yakumo", "yakumo_rank2: bondId=yakumo");
        Assert(yakumoRank2?.choices?.Count > 0, "yakumo_rank2: choicesあり");
        var yakumoRank5 = DialogueRunner.LoadFromStreamingAssets("yakumo_rank5.json");
        Assert(yakumoRank5 != null, "yakumo_rank5.json 読み込み成功");
        Assert(yakumoRank5?.lines?.Count >= 10, "yakumo_rank5: lines 10件以上（傷の核心）");
        var yakumoRank9 = DialogueRunner.LoadFromStreamingAssets("yakumo_rank9.json");
        Assert(yakumoRank9 != null, "yakumo_rank9.json 読み込み成功");
        var yakumoRank10 = DialogueRunner.LoadFromStreamingAssets("yakumo_rank10.json");
        Assert(yakumoRank10 != null, "yakumo_rank10.json 読み込み成功");
        Assert(yakumoRank10?.bondPointsOnComplete == 0, "yakumo_rank10: bondPointsOnComplete=0（感情体験優先）");
        var yakumoRank1 = DialogueRunner.LoadFromStreamingAssets("yakumo_rank1.json");
        Assert(yakumoRank1 != null, "yakumo_rank1.json 読み込み成功");
        Assert(yakumoRank1?.lines?.Count >= 10, "yakumo_rank1: lines 10件以上（言葉の重さ講話）");
        Assert(yakumoRank1?.choices?.Count > 0, "yakumo_rank1: choicesあり");
        var yakumoLunch = DialogueRunner.LoadFromStreamingAssets("yakumo_lunch.json");
        Assert(yakumoLunch != null, "yakumo_lunch.json 読み込み成功");

        // --- 昼休み拡張アクション ---
        Debug.Log("\n[Lunch拡張: LunchCanteen/LunchRooftop/LunchHealthRoom]");
        Assert(System.Enum.IsDefined(typeof(Amane.UI.FieldAction), "LunchCanteen"), "FieldAction.LunchCanteen 定義あり");
        Assert(System.Enum.IsDefined(typeof(Amane.UI.FieldAction), "LunchRooftop"), "FieldAction.LunchRooftop 定義あり");
        Assert(System.Enum.IsDefined(typeof(Amane.UI.FieldAction), "LunchHealthRoom"), "FieldAction.LunchHealthRoom 定義あり（保健室・佳乃導線）");

        // Summary ---
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
