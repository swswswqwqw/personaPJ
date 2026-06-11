# RPG開発 引き継ぎ

## 最終更新: 2026-06-11（第6セッション・夜間自動実行）
## 作業ブランチ: claude/sweet-fermat-znRAO
## ゲームタイトル / エンジン: 残響都市アマネ / AMANE: City of Echoes — Unity 6 (6000.3.2f1) + URP

---

## 現在のフェーズ
**デュアルナレーター（DESIGN.md 9-1）システム実装完了 + 渚（nagisa）初期会話JSON3本追加**

2026-06-10: 8本の設計ブランチを統合。DESIGN.mdにセクション9「統合拡張仕様」を追加。
focused-shannonブランチからGameEventBus・InventoryManager・未言界ダンジョン探索（DungeonExplorer/MigenkaiManager/MigenkaiData）・ItemDataを移植。
namespace を全て Amane.* に統一。

3Dフィールド探索は前回から引き続き実装済み

全コアシステム＋演出統合に加え、3Dフィールド探索を実装。
Field3DBuilder: コードのみで雨音市を3D構築（地面・水路・道路・建物5棟・未言界入口・NPC3名・プレイヤー）。
PlayerController3D: WASD移動＋カメラ相対方向＋Rigidbody物理。ThirdPersonCamera: P5風斜め見下ろし追従。
FieldManager3D: NPC/ロケーション近接判定＋プロンプト表示＋インタラクト処理。
PrototypeBootstrap で3Dフィールドを自動生成し、FieldControllerに接続済み。

---

## 品質スコア（最新・ペルソナ比較）
- キャラクター深度:      9/10（全3キャラのランク1-10 JSON完成。渚intro/rank1追加で第2幕の骨格も始動）
- 戦闘の爽快感:          8/10（デュアルナレーター実装で「語り手2体同時保持×属性相性戦略」が加わった）
- 時間システムの機能性:  8/10（深夜強行潜行追加済み）
- 社会リンクの感情密度:  8/10（3キャラ完全。渚の「演じることの疲れ」描写でアーク開始）
- UI・演出の完成度:      7/10（PlayDualNarratorActivated: シナジー=ピンク/対立=紫の色分け演出追加）
- ストーリーの引力:      8/10（event_case_nagisa_start.jsonで第2幕「失踪×デッドライン」テンション開幕）
- 総合:                  8.2/10（前回8.1→8.2。戦闘戦略深度向上・第2幕開始）
前回からの変化: デュアルナレーターシステム全実装（Narrator/NarratorAffinityMatrix/BattleManager統合）、渚JSON3本（intro/rank1/失踪イベント）。

---

## ペルソナ5 調査結果サマリー（実装に反映済み）
- **色先行デザイン**: 赤×黒→本作は群青×蛍光ピンク。UIの色がテーマを語る。
- **1ドット1フレーム**: 自作Tweenエンジンで全アニメーションを1フレーム単位で制御可能に。
- **モーションリズム**: 急停止→残像のメリハリ。OutBack/OutElastic/OutBounce easing適用。
- **テーマUI一貫性**: P5の切り抜き文字＝怪盗の呼び状→本作の「既読」「封筒」モチーフ。
- **バトンタッチ**: +50%初段/+25%後続。本作「言継ぎ」として実装済み。
- **ALL-OUT ATTACK**: コミック調フィニッシュ+決め台詞→本作「総告白」（4フェーズ演出実装）。

---

## 今夜の作業サマリー（2026-06-11 第6セッション）
- **⚔️ デュアルナレーター（DESIGN.md 9-1）フル実装**:
  - `Narrator.cs` 新規: 語り手データクラス（Id/DisplayName/PrimaryElement/Affinities/Skills）
  - `NarratorAffinityMatrix.cs` 新規: 語り手間属性相性（同属性x1.2/対立属性x0.7/中立x1.0）
    - 対立ペア: 光×闇 / 焔×氷 / 雷×風
  - `Combatant.cs` 拡張: `ActiveNarrator`/`SecondaryNarrator`/`IsDualNarratorActive`/`SetNarrators()`/`GetDualNarratorSkills()`
    - 語り手セット時にAffinities/Skillsを語り手のものに切り替え
  - `BattleAction.cs` 拡張: `DualNarrator` enum追加、`SecondarySkill`フィールド、`DualNarratorAttack()`ファクトリ
  - `BattleManager.cs` 拡張: `IsDualNarratorUnlocked`フラグ（第3幕解放）、`OnDualNarratorActivated`イベント、`ExecuteDualNarratorSkill()`
    - SP消費1.5倍（Math.Ceiling）、SP不足時は通常攻撃フォールバック
    - 語り手間属性相性補正を`dualMultiplier`としてDamageCalculatorに注入
    - 主スキル→副スキルの2段階Hit処理（1ターン内で完結）
  - `DamageCalculator.cs` 拡張: `dualMultiplier`パラメータ追加（デフォルト1.0f、後方互換）
  - `BattleEffects.cs` 拡張: `PlayDualNarratorActivated()`追加
    - シナジー: 蛍光ピンクフラッシュ + 「2つの声が、共鳴する。」
    - 対立: 紫フラッシュ + 「相反する声が、ぶつかり合う。」
  - `BattleController.cs` 拡張: `OnDualNarratorActivated`購読＋演出分岐
- **💬 渚（鵠沼渚）初期会話JSON3本追加**:
  - `nagisa_intro.json`: 9月初回遭遇。「本物って、何？」スマホを伏せる渚との出会い。〈未読〉が震える演出
  - `nagisa_rank1.json`: 「三時間、ずっと笑ってた。喉、カラカラ。」配信の消耗感を初めて吐露
  - `event_case_nagisa_start.json`: 渚失踪イベント。霧の月曜日、3日間音信不通。デッドライン14日告知
- **🧪 AmaneLogicTest 大幅拡充**:
  - NarratorAffinityMatrix 5テスト（シナジー/対立/中立）
  - Narrator生成・PrimaryElement確認
  - Combatant語り手セット→IsDualNarratorActive確認
  - SetNarrators後のAffinities切り替え確認
  - GetDualNarratorSkills重複除去確認
  - BattleManagerでのDualNarrator実行→SP消費1.5倍確認
  - OnDualNarratorActivatedイベント発火確認
  - SP不足時フォールバック確認（SP不変）

## 今夜の作業サマリー（2026-06-11 第4セッション）
- **🌙 深夜強行潜行オプション実装（DESIGN.md 9-2）**:
  - TimeManager: `MidnightDiveComposureRank = 4`（解放条件）+ `MidnightDiveDebt`プロパティ + `ForceDive()` メソッド
  - `ForceDive()`: AP消費なし・`MidnightDiveDebt = true` → 翌日 `ActionPoints = 0`（疲労デバフ）
  - `AdvanceDay()`: `MidnightDiveDebt` フラグを確認し翌日APを0に設定してリセット
  - ActionSelectUI: `MidnightDive` を `FieldAction` enum に追加・`_midnightDiveButton` フィールド追加
  - `Show()` に `midnightDiveUnlocked` パラメータ追加（LateNight×静けさ4以上で表示）
  - FieldController: `MidnightDive` アクションハンドリング + `StartMidnightDiveTransition()` メソッド
  - FieldController: `TimeAdvancedEvent` 購読追加 → LateNight到達時に深夜メニュー自動表示
  - CalendarUI: `MidnightDiveDebt` 中は「疲労（翌日AP=0）」と表示
  - PrototypeBootstrap: 深夜専用パネル + 「眠る」「強行潜行（翌日AP消滅）」ボタン追加
- **🎭 潜行開始演出の強化（DESIGN.md必須演出7番）**:
  - `PlayDiveTransition()` を3フェーズ演出に刷新:
    - Phase1 (0.35s): 「——〈未読〉が、届いている。」（蛍光ピンク）
    - Phase2 (0.45s): 「水紋が、広がる……」（白）
    - Phase3 (0.3s): 「世界が、裏返る。」（完全暗転＋蛍光ピンク）
  - `PlayMidnightDiveTransition()` 新規追加: 重く遅い暗紫演出（0.9s）「疲れているのに潜らなければならない」焦燥
  - `SetText()` ヘルパーメソッド追加（テキスト+色の一括設定）
- **🧪 AmaneLogicTest 拡充**:
  - ForceDive成功テスト
  - MidnightDiveDebt フラグON確認
  - 2回目ForceDive失敗確認
  - AdvanceDay後: 疲労フラグリセット・AP=0確認
  - 疲労なし翌々日: AP=2回復確認（12ステップ）

## 今夜の作業サマリー（2026-06-11 第3セッション）
- **⏰ 昼休み行動枠（Lunch AP）実装**:
  - TimeSlot enum に `Lunch` 追加（Class→Lunch→AfterSchool の流れ）
  - TimeManager: `LunchUsed` プロパティ + `UseLunch()` + `AdvanceDay` でリセット
  - CalendarUI: Lunch時は「昼休み AP 1/1」または「使用済み」を表示
  - ActionSelectUI: LunchChat/LunchLibrary/LunchSkip をFieldActionに追加
  - FieldController: OnLocation3DInteracted に昼休みブランチを追加
    - NPC → `StartLunchBondDialogue(id, gm)` で昼休み専用会話
    - Study（学校） → 知性+3（放課後より少ない）
    - Meditate → 静けさ+3
    - Dungeon/Shop/Job → ブロック（昼休みは校内限定）
  - JSON: akari/ritsu/ren_lunch.json（各30秒以内の短い会話）
    - 灯里: 窓際の席でお弁当。港の霧を眺める日常感
    - 律: 図書室で黙って隣に座る。「いてもいい」の許容
    - 蓮: 廊下で壁にもたれてパン。「邪魔じゃねえけど」
  - AmaneLogicTest: Lunch スロット進行テスト追加（9ステップ）
- **📦 前セッション未コミット分のコミット**:
  - CharacterData/EnemyData/AbilityData ScriptableObject
  - event_awakening/fullmoon_apr/jun/jul/case_mizuki/ritsu/ren_start/end_act1 JSON（第1幕全イベント）
  - akari/ren/ritsu_rank1.json（絆ランク1会話）
  - FieldController: DayChangedEvent→CalendarEvent自動会話起動
  - DialogueUI: SelectOption の bondBonus を BondManager に即時適用
  - DialogueRunner: CurrentData プロパティ公開

## 今夜の作業サマリー（2026-06-11 第2セッション）
- **💬 律（ritsu）ランク6-10 JSON完成**:
  - rank6「送信」: 父への返信を書こうとするが書けない。バッファが溢れる
  - rank7「下書きの全文」: 父の下書きを主人公に読み上げる。「俺はお前の父親だ」
  - rank8「サーバールームへ」: 父の心象世界への潜行決意。「怖い」と初めて言う
  - rank9「エラーなし」: 潜行後。無数のモニターに「律へ」が点滅。「受信した」
  - rank10「最後の言伝 — 律」: KOTOBAシステム完成。「届けられなかった言葉を記録する」
- **💬 蓮（ren）ランク6-10 JSON完成**:
  - rank6「病室の名前」: 祖母が「蓮」と呼んだ。「毎日来てよかった」
  - rank7「残り少ない時間」: 医師から容態の話。「いなくなるのは怖い」と初めて言う
  - rank8「最後の面会」: 「好きだよ、ばあちゃん」と初めて言う
  - rank9「形見」: 祖母の遺品に手書き手紙。「私も好きだよ、蓮」
  - rank10「最後の言伝 — 蓮」: バンデージを渡す。「お前と組んで、よかった」
- **💬 灯里（akari）ランク6-10**（前セッションで既に完成済み、今回コミットに含めた）
- **🔗 ダンジョン探索ループ接続確認**:
  - FieldController.StartDiveTransition: MigenkaiManager→DungeonState遷移
  - DungeonController: 部屋入室→戦闘トリガー→BattleState遷移
  - BattleController.OnResultContinue: DungeonBattleContext.IsInDungeon判定→ReturnToDungeon
  - 未言界→エンカウント→戦闘→帰還ループが完全に成立

## 今夜の作業サマリー（2026-06-11）
- **⚔️ パーフェクト言継ぎ（DESIGN.md 9-1）**:
  - BattleManager: HashSet<string>で参加者追跡、全員参加でOnPerfectKotsugi発火
  - 即時効果: SP全員15%回復（MaxSp * 0.15f）
  - 次ラウンド: GetTotalBonus()に+10%バフ組み込み（_perfectKotsugiBuffThisRound）
  - BattleEffects.PlayPerfectKotsugi(): 波形エフェクト+「想いは、ひとりじゃ届かない」タイプライター演出
  - AmaneLogicTestに自動テスト追加（2人パーティでの発火確認+SP回復確認）
- **💀 逆総告白・リバース・コーリング（DESIGN.md 9-1）**:
  - CheckReverseAllOutCalling: 味方全員DOWN後に2~3回の追撃を実行
  - OnReverseAllOutCalling イベントでUIへ通知
  - BattleEffects.PlayReverseAllOutCalling(): 赤フラッシュ（3回点滅）+震えるテキスト演出
  - BattleController: イベントサブスクリプションとログ出力追加
- **💬 言伝ランク2-5 JSON拡充**:
  - ren_rank2: 老人の荷物を拾う蓮。「ばあちゃんに似てた」
  - ren_rank3: 病室の前で立ち尽くす蓮。「足が動かねえ」
  - ren_rank4: 潜行翌日。病室で「ありがとう」を言えた
  - ren_rank5: 守るための拳へ。「言葉のほうがずっと怖え」
  - ritsu_rank2: 図書室。「なんで来るんだ」という問い
  - ritsu_rank3: 父のスマホの下書き。「どっちも送れなかった」
  - ritsu_rank4: 初めて泣く律。「オーバーフロー」と呼ぶ涙
  - ritsu_rank5: 届けられなかった言葉を救う側へ

## 今夜の作業サマリー（2026-06-10）
- **🎵 プロシージャル音響システム構築**:
  - ProceduralAudio: コード生成のBGM/SE（外部ファイル不要）
  - AudioManager: BGM切替（Title/Field/Battle）、SE再生、雨環境音
  - 各シーンでBGM自動切替（OnStateChanged連動）
  - UI操作音・弱点ヒット音・クリティカル音・レベルアップ音・インタラクト音
- **⬆️ 経験値・レベルアップシステム**:
  - ExperienceSystem: EXP計算・レベル判定・ペルソナ風成長カーブ（99レベル対応）
  - 戦闘勝利→EXP自動付与→レベルアップ判定→パーティ全員に適用
  - GameManagerに統合、パーティメンバー4人を自動登録
- **✨ レベルアップ演出（LevelUpEffect）**:
  - EXPバーアニメーション → 満タン → "LEVEL UP!" フラッシュ
  - アルペジオSE → ステータス上昇表示
  - BattleController→勝利時に自動再生→リザルトUI表示
- **🌧️ 3Dフィールド環境演出（FieldEnvironment）**:
  - 雨パーティクルシステム（2000粒子の小雨）
  - 時間帯ライティングプリセット5段階（朝/昼/夕/夜/深夜）
  - NPC頭上ネームプレート（BillboardText — カメラ追従）
  - プレイヤー頭上にも蛍光ピンクのネーム表示
- **3Dフィールド統合修正**:
  - URP互換シェーダー: 5段階フォールバック（URP/Lit→Simple Lit→Unlit→Sprites→UI）
  - カメラ切替: タイトル/バトル時はUICamera、フィールド時はFollowCamera
  - ステータスキーをS→Tabに変更（WASD競合回避）

---

## 実装済み機能リスト
- タイトル画面（フロー骨格・C#）
- 時間システム / カレンダー（ロジック＋CalendarUI接続）
- 内面ステータス（データ構造＋StatusUI＋Toggle表示）
- 絆 BONDS（データ構造＋ランク10＋ボンドポイント付与）
- Core 基盤（GameManager / StateMachine / EventChannel）
- **戦闘システム（ターン制・弱点・畳み掛け・言継ぎ・総告白のフルループ）**
- **DialogueRunner（JSON駆動会話・選択肢・ボンドポイント）**
- **フィールド行動選択（ActionSelectUI・FieldController）**
- **BattleHUD / BattleController（戦闘UI・入力制御・エフェクト統合）**
- **セーブ / ロード（JsonSaveSystem実体）**
- **DeadlineUI（デッドライン接近警告）**
- **StatusUI（内面能力＋絆一覧表示＋Toggle）**
- **DialogueUI（会話表示・選択肢UI・タイプライター統合）**
- **初回会話JSON: 灯里/律/蓮**
- **言伝ランク2-5 JSON: 灯里・蓮・律（全キャラ完成）**
- **言伝ランク6-10 JSON: 灯里・蓮・律（全キャラ完成）— 最後の言伝まで実装**
- **パーフェクト言継ぎ（PerfectKotsugi — DESIGN.md 9-1）**
- **逆総告白（ReverseAllOutCalling — DESIGN.md 9-1）**
- **SkillSelectUI（スキル一覧選択・SP表示）**
- **TargetSelectUI（敵/味方ターゲット選択）**
- **KotsugiSelectUI（言継ぎ先選択）**
- **BattleResultUI（勝利/敗北リザルト→フィールド復帰）**
- **CalendarEventScheduler（全ストーリーイベントのスケジュール管理）**
- **自作Tweenエンジン（DOTween非依存、7種Easing）**
- **BattleEffects（弱点/クリティカル/DOWN/ダメージポップ/総告白/言継ぎ演出）**
- **TransitionEffect（潜行/復帰/時間帯遷移演出）**
- **TypewriterText（タイプライター式テキスト表示）**
- **BondRankUpEffect（絆ランクアップ「封筒が既読」演出）**
- **3Dフィールド探索（Field3DBuilder/PlayerController3D/ThirdPersonCamera/FieldManager3D/NPC3D）**
- **ActionCommand（タイミングゲージ戦闘）**
- **2Dフィールド探索（FieldMap2D/FieldPlayer2D/FieldLocation — 3Dフォールバック）**
- **プロシージャルBGM/SE（AudioManager/ProceduralAudio — タイトル・フィールド・バトルBGM + 各種SE + 雨環境音）**
- **経験値・レベルアップシステム（ExperienceSystem — 99レベル対応・戦闘EXP自動付与）**
- **レベルアップ演出（LevelUpEffect — EXPバー＋LEVEL UP!フラッシュ＋SE）**
- **3D環境演出（FieldEnvironment — 雨パーティクル・時間帯ライティング・ネームプレート）**
- **GameEventBus（型安全Pub/Subイベントバス — EventChannelの上位互換）**
- **InventoryManager（アイテム所持・所持金管理・64スロット・スタック対応）**
- **ItemData（ScriptableObjectアイテム定義 — 消費/装備/素材/キーアイテム4カテゴリ）**
- **未言界ダンジョン探索（MigenkaiManager/DungeonExplorer/MigenkaiData — プロシージャル部屋生成・宝箱・罠・休憩・中ボス・ボス）**
- **ScriptableObject データ定義（CharacterData/EnemyData/AbilityData — ToCombatant()対応）**
- **CalendarEvent 自動会話起動（event_awakening/fullmoon×3/case×3/end_act1 — 第1幕全ストーリービート）**
- **絆ランク1 JSON（akari/ren/ritsu — イントロ翌日の再会シーン）**
- **昼休み行動枠（Lunch APシステム — 校内限定1回・NPC/Study/Meditate対応・Dungeon/Job/Shopブロック）**
- **昼休み会話JSON（akari/ren/ritsu_lunch — 各キャラ短会話・窓際/図書室/廊下）**
- **bondBonus即時適用（DialogueUI.SelectChoice → BondManager.GivePoints）**
- **深夜強行潜行（ForceDive/MidnightDiveDebt — 静けさ4以上解放・翌日AP=0疲労デバフ）**
- **潜行演出3フェーズ化（未読→水紋→世界裏返る — DESIGN.md必須演出7番実装）**
- **深夜潜行演出（PlayMidnightDiveTransition — 切迫した暗紫演出）**
- **デュアルナレーター（Narrator/NarratorAffinityMatrix/Combatant.SetNarrators/BattleManager.ExecuteDualNarratorSkill — DESIGN.md 9-1完全実装）**
- **渚（鵠沼渚）会話JSON（nagisa_intro/rank1/event_case_nagisa_start — 第2幕開幕素材）**

---

## 未実装・課題リスト
- ~~BGM/SE 仮素材導入~~: 完了（ProceduralAudio + AudioManager実装済み）
- ~~アイテム・所持金管理~~: 完了（InventoryManager + ItemData実装済み）
- ~~ダンジョン探索システム~~: 完了（MigenkaiManager + DungeonExplorer実装済み）
- ~~ScriptableObject によるキャラ/スキルデータ整備~~: 完了（CharacterData/EnemyData/AbilityData実装済み）
- ~~経験値・レベルアップシステム~~: 完了（ExperienceSystem + LevelUpEffect実装済み）
- ~~絆ランク2以降の会話JSON~~: 完了（全3キャラのランク1-10完成）
- ~~潜行開始演出の強化（アプリ「未読」→水紋→世界裏返りのリッチ演出）~~: 完了（3フェーズ演出実装済み）
- ~~深夜強行潜行オプション~~: 完了（ForceDive/MidnightDiveDebt/疲労デバフ実装済み）
- DeserializeState の実体実装（TimeManager等にリストア用セッター追加）: 優先度: 低
- ~~CalendarEvent発生時の会話/シーン自動遷移~~: 完了（9本イベントJSON + 自動起動実装済み）
- ~~フィールドマップ（2D背景＋キャラ配置）~~: 完了（3Dプリミティブ版で実装済み）
- ~~デュアルナレーター（DESIGN.md 9-1）~~: 完了（Narrator/NarratorAffinityMatrix/Combatant/BattleManager/BattleEffects統合）
- 渚: intro/rank1/失踪イベントJSON完了。rank2〜10は未作成: 優先度: 中
- 八雲/佳乃/真鍋/すずのJSONとイベント: 優先度: 中（第2幕解放に必要）

---

## 次回セッションの最優先タスク
1. ~~デュアルナレーター（DESIGN.md 9-1）~~: 完了
2. ~~渚（Nagisa）の初期会話JSON~~: 完了（intro/rank1/失踪イベント）
3. **渚 rank2〜5 JSON作成**
   理由: intro/rank1は完成。「演じることの疲れ→素顔の一瞬→また隠す」の感情弧を描く。第2幕中盤の潜行準備として必要。
4. **昼休みのバリエーション拡充**
   理由: 現在はキャラ会話3種のみ。「購買」（消耗品入手）・「屋上」（静けさ+静謐演出）・「職員室」（知識アイテム）等でP4の昼休みらしさを再現。
5. **DeserializeStateの実体実装（セーブ/ロード完全化）**
   理由: 現在はシリアライズのみ。TimeManager/BondManager/InnerStatSet へのリストア処理追加で完全なセーブ機能に。
6. **八雲（Yakumo）の初期会話JSON追加**
   理由: 語り手融合（フュージョン）の案内役。6月〜登場。古書店「言ノ葉」でのシーンから始まる。
7. **語り手融合UIの最小実装（FusionSelectUI）**
   理由: デュアルナレーターの解放までに語り手を複数獲得する経路が必要。八雲のコープ報酬「融合費用半減」とも連動。

---

## 設計メモ（意思決定の記録）
- 2026-06-07 エンジンを Unity 2022 LTS + URP に決定。理由＝UIそのものを演出にする本作では uGUI+DOTween+シェーダ制御の費用対効果が最良。Unreal の GAS は戦闘設計には魅力だが、UI演出密度と長編アセット管理で Unity を選択。
- 2026-06-07 コアテーマを「言えなかった後悔と向き合う勇気」に確定。全実装の判断基準とする。
- 2026-06-07 戦闘のバトンタッチを「言継ぎ」と命名し、テーマ（想いを継ぐ）と直結させた。捨てた案＝純粋な戦術バフのみの命名（テーマ希薄のため不採用）。
- 2026-06-07 ラスボス＝主人公自身の後悔の具現、という構造を採用。伏線は環境演出（消える封筒・改変される母の記憶）で静かに敷く方針。
- 2026-06-08 戦闘ダメージ計算: 言継ぎボーナスを初段+50%/以降+25%刻みに設定。DESIGN.mdのバトンタッチ仕様に準拠。
- 2026-06-08 敵AI: 弱点優先狙いを実装。プレイヤーにも同じく弱点を突く快感を与えるため、敵も弱点戦略を使う＝緊張感の担保。
- 2026-06-08 セーブデータ形式: Dictionary→List<StatEntry>/List<BondEntry>に変更。JsonUtilityがDictionaryを扱えないため。将来Newtonsoft.Json移行で戻す可能性あり。
- 2026-06-08 灯里の初回会話で3択を設計: 「黙って座る」「泣いてたのか？（度胸）」「無理に笑わなくていい（慈しみ）」。最もボンドが上がるのは共感選択肢＝テーマ「言葉にする勇気」の報酬構造。
- 2026-06-08 スキル選択→ターゲット選択→実行の3段階UIフローを採用。ペルソナのコマンド選択→対象選択の快感を再現。AllEnemies/AllAllies/Selfスキルは自動ターゲットでテンポ確保。
- 2026-06-08 CalendarEventScheduler: DESIGN.mdの月次イベントカレンダーを全てコード上にスケジュール。第1幕〜第3幕の全ストーリービート・満月・テスト・学園祭・どんでん返しを登録。
- 2026-06-08 PrototypeBootstrap方式を採用: シーンやプレハブを手作業で作る代わりに、C#コードで全UIをランタイム生成。理由＝CLIベース開発ではUnityエディタのGUI操作ができないため、コード駆動でプロトタイプを完結させた。DESIGN.mdのカラーパレット（群青#1B2A4A/蛍光ピンク#FF3E8A）を適用。
- 2026-06-08 Unity 6 (6000.3.2f1) を使用中。DESIGN.md記載のUnity 2022 LTSとは異なるが、URP/uGUI/Input Systemは互換。docs/DESIGN.mdは参照専用のため修正せず、実態はこのCLAUDE.mdに記録。
- 2026-06-08 ロジックテスト（AmaneLogicTest）を作成。GameDate/TimeManager/Deadline/InnerStat/Bond/Affinity/DamageCalculator/TurnSystem/BattleManager/EnemyAI/DialogueRunner/CalendarEventSchedulerの全コアシステムをカバー。
- 2026-06-08 **DOTween非依存を決定**: 外部依存を避け、自作Tweenerを構築。7種Easing関数・Shake/Punch/Sequenceを含む。ペルソナ5のCEDEC講演「1ドット1フレームの調整」を自前で可能にするため。
- 2026-06-08 **P5調査→演出設計**: 弱点ヒット＝画面フラッシュ+WEAKバナースライド+ひび割れ。総告白＝4フェーズ（暗転→タイトルスライド→引用タイプライター→フラッシュフェード）。言継ぎ＝左右からスライドイン+ボーナス表示。全てDESIGN.mdの演出仕様に準拠。
- 2026-06-08 **絆ランクアップ演出**: 封筒→開封→蛍光ピンクの光→「言伝 Rank X」。DESIGN.md演出3「封筒が"既読"になる — 想いが届いた瞬間の温度」をアニメーション化。
- 2026-06-08 **演出統合方式**: BattleManager.OnHit/OnAllOutConfession/OnKotsugiイベントをBattleControllerで購読し、BattleEffectsの対応メソッドを呼ぶ。FieldControllerではTransitionEffect.Instanceシングルトンを利用。BondRankUpEventはEventChannel経由でIDisposableパターンで購読。
- 2026-06-10 **8ブランチ設計統合**: 残響のアルカディア/アストラル/CURTAIN CALL/残響のコード/仮面都市カガミ/鏡夜のアルカナ/残響のアルカディア(別版)の7つの設計案から、アマネのコアテーマを強化する10要素を選定しDESIGN.mdセクション9に追加。採用: パーフェクト言継ぎ、逆総告白、デュアルナレーター、昼休みAP、深夜潜行デバフ、感情別色彩波形、心象世界固有ビジュアル、街スポットテーブル、波形シンクロUI、融合NPC統合。
- 2026-06-10 **focused-shannonからコード移植**: GameEventBus（型安全Pub/Sub）、InventoryManager（アイテム64スロット+所持金）、ItemData（SO）、MigenkaiManager+DungeonExplorer+MigenkaiData（プロシージャルダンジョン生成）。全namespaceをAmane.*に統一。EchoesOfArcadiaの名称・テーマをアマネ世界観に合わせてリネーム（残響界→未言界、ボス部屋→言伝の間等）。
- 2026-06-11 **パーフェクト言継ぎの参加者追跡**: HashSet<string>でCombatant.Idを管理。同一メンバーが複数回登場してもSetなので重複なし。aliveParty.All(p => 参加者集合.Contains(p.Id))で全員参加を判定。捨てた案=KotsugiChain >= aliveCount-1での判定（4人フルリレーとそれ未満の区別が曖昧なため）。
- 2026-06-11 **逆総告白の設計選択**: 追撃を自動実行（BattleManager内部で処理）。UIドリブンの選択式は採用しなかった。理由=「DOWNされた後に選択肢を与えるのは感情的に矛盾。プレイヤーは見ているしかない緊張を体験すべき」。
- 2026-06-11 **SP回復タイミング**: パーフェクト言継ぎ検出→即時SP回復→次ラウンドバフフラグ（_perfectKotsugiBuffNextRound）の順。AdvanceTurn()前に処理するため、回復が確実に行われる。
- 2026-06-11 **律rank10のKOTOBAシステム設計**: 届かなかった言葉を「記録するシステム」という形でゲームシステムと融合させた。単なる感情的解決ではなく「プログラマーとしての律の解答」という個性を持たせた。テーマ「届かなかった言葉を救う」をキャラクター属性（天才ハッカー）と直結。
- 2026-06-11 **蓮rank8の「好きだよ」設計**: ランク4で「ありがとう」、ランク8で「好きだよ」という段階的な感情表現の解放。前者は感謝（過去への清算）、後者は愛情（現在の肯定）。蓮の語彙が広がる=成長の証明として機能させた。
- 2026-06-11 **全3キャラのrank10選択肢設計**: bondBonus=0（ランク10は報酬より感情体験を優先）。選択肢3つは主人公の共感/勇気/慰労の3方向。どれを選んでも正解＝「言葉にした勇気」をプレイヤーに体験させる構造。
- 2026-06-11 **昼休みAPを別フラグ設計**: ActionPoints（放課後/夜のAP）とは分離し、LunchUsedフラグで管理。理由＝「3AP/日」にすると選択の重みが薄れるため。昼休みは校内限定・潜行不可の制約が「空間と時間の分離」という世界観設計に合う。
- 2026-06-11 **CalendarEventのイベントID命名規則**: CalendarEventScheduler.SeedStoryEvents()のId（"awakening", "fullmoon_apr"等）とJSONファイル名を"event_{id}.json"で統一。FieldControllerのTryStartCalendarEventDialogue()が自動マッチング。
- 2026-06-11 **深夜強行潜行の疲労デバフ設計**: MidnightDiveDebtフラグをTimeManagerが管理。AdvanceDay()が翌日AP=0に設定→フラグリセット。UIは「疲労（翌日AP=0）」表示で選択の重さを伝える。捨てた案=デバフをInnerStatの一時低下にする案（システムが複雑になりすぎるため）。
- 2026-06-11 **潜行演出3フェーズ設計**: Phase1「未読通知（蛍光ピンク）」→Phase2「水紋（白）」→Phase3「世界裏返る（暗転+ピンク）」。フェード値を0→0.7→1と段階的に上げることで「世界がゆっくり沈む」感覚を演出。深夜版は0.9sの単相フェード=「重く一気に暗くなる」表現の差別化。
- 2026-06-11 **LateNight専用メニューのトリガー方式**: TimeAdvancedEventをFieldControllerで購読し、LateNight到達時にActionSelectUI.Show()を呼ぶ。CalendarUIも同じ方式でRefresh()しているため一貫したアーキテクチャ。深夜メニューはSetActive制御のみで追加コンポーネント不要。
- 2026-06-11 **デュアルナレーターのSP消費設計**: (primary.SpCost + secondary.SpCost) * 1.5f を Math.Ceiling で切り上げ整数化。SP不足時は通常攻撃にフォールバック（SpendSp(0)で0消費）。「2つの力を同時に使う代償」がSP管理の緊張感を高める設計。捨てた案=各スキル個別に1.5倍（計算が2回に分かれてSP管理が複雑になるため）。
- 2026-06-11 **NarratorAffinityMatrixの対立ペア選択**: 光×闇（赦し vs 後悔）/ 焔×氷 / 雷×風の3ペアを対立とした。DESIGN.mdの属性設計（光=赦し・闇=後悔）とコアテーマが直結しているため、光×闇の対立が最も物語的意味を持つ。同属性シナジーは「同じ声が共鳴する」=確信を深めた状態の表現。
- 2026-06-11 **渚のintroでの〈未読〉演出**: speakerId="narrator"の「〈未読〉が静かに震えた」行を挿入。アプリUIの感覚的な反応をテキストで伝える手法。声に出して読んで「ここで空気が変わる」感覚を確認した。
