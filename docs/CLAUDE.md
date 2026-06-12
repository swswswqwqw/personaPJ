# RPG開発 引き継ぎ

## 最終更新: 2026-06-12（第10セッション・夜間自動実行）
## 作業ブランチ: claude/sweet-fermat-znRAO
## ゲームタイトル / エンジン: 残響都市アマネ / AMANE: City of Echoes — Unity 6 (6000.3.2f1) + URP

---

## 現在のフェーズ
**佳乃（七尾佳乃）コープ全実装完了（intro + rank1-10 + lunch + 保健室解放イベント + ランク進行対応）**

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
- キャラクター深度:      9/10（6キャラのコープ全rank10完成。佳乃rank8の「光の語り手で謝罪の場を作る」がシステム×感情融合の最高密度）
- 戦闘の爽快感:          8/10（デュアルナレーター実装済み。FusionSelectUIで語り手獲得経路が完成）
- 時間システムの機能性:  9/10（保健室LunchHealthRoomがランク進行対応済み。kano_opening DayIndex98(七夕)で解放）
- 社会リンクの感情密度: 10/10（灯里/律/蓮/渚/八雲/佳乃の6キャラ全rank10到達。第2幕中盤まで全コープ完成）
- UI・演出の完成度:      8/10（kano NPC 3D/2D両追加）
- ストーリーの引力:      9/10（佳乃「……明日（あした）って、呼ぼうと思ってた」→rank8「泣けた。10年ぶりに。」は最高の感情弧）
- 総合:                  9.0/10（前回8.8→9.0。佳乃コープ完成で時間システム・社会リンク・ストーリー3項目が底上げ）
前回からの変化: 佳乃（七尾佳乃）コープ全実装（intro+rank1-10+lunch+event_kano_opening）、DoLunchHealthRoomをランク進行対応に修正、3D/2DフィールドにkanoNPC追加、CalendarEventにkano_opening(DayIndex98)追加、AmaneLogicTest拡充。

---

## ペルソナ5 調査結果サマリー（実装に反映済み）
- **色先行デザイン**: 赤×黒→本作は群青×蛍光ピンク。UIの色がテーマを語る。
- **1ドット1フレーム**: 自作Tweenエンジンで全アニメーションを1フレーム単位で制御可能に。
- **モーションリズム**: 急停止→残像のメリハリ。OutBack/OutElastic/OutBounce easing適用。
- **テーマUI一貫性**: P5の切り抜き文字＝怪盗の呼び状→本作の「既読」「封筒」モチーフ。
- **バトンタッチ**: +50%初段/+25%後続。本作「言継ぎ」として実装済み。
- **ALL-OUT ATTACK**: コミック調フィニッシュ+決め台詞→本作「総告白」（4フェーズ演出実装）。

---

## 今夜の作業サマリー（2026-06-12 第10セッション）
- **🏥 佳乃（七尾佳乃）コープ全実装 — 「自罰→受容→生を肯定」感情弧フル完成**:
  - `kano_intro.json`: 「薬草茶と静寂」——「ここは、痛い場所でしょう。でも——安全な場所でもある。」〈未読〉がかすかに反応
  - `kano_rank1.json`: 「消毒液の香り」——〈未読〉が「……ごめん」という断片を拾う。佳乃は気づかない
  - `kano_rank2.json`: 「白いカーテン」——1年生を手当てした後「子どもを前にすると、どうしても——」と漏らす
  - `kano_rank3.json`: 「残響の漏れ」——残響同調が暴走し「……ごめんなさい。産んであげられなくて。」を聴いてしまう。「あなたには、聞こえても。」という受容
  - `kano_rank4.json`: 「あの子のこと」——「名前を考えてた。「明日（あした）」って、呼ぼうと思ってた。」
  - `kano_rank5.json`: 「自罰の名前」——転換点。「責め続けることが、あの子のためじゃなくて——私のため、なのかな。」
  - `kano_rank6.json`: 「空の揺籠」——「あの子が生きていたら、今年で10歳。」想像する柔らかい目。夏の光
  - `kano_rank7.json`: 「許してもらえるか」——「謝りたい。届かなくても。」主人公への深い信頼
  - `kano_rank8.json`: 「水底の声」——光（赦し）の語り手で謝罪の「場」を作る。ゲームシステムと感情の融合。「泣けた。10年ぶりに。」
  - `kano_rank9.json`: 「生を肯定する」——「あなたのおかげで、生きていいって思えるようになった。」
  - `kano_rank10.json`: 「最後の言伝 — 佳乃」——「——生きていてくれてありがとう。」コープ報酬: 全回復＋状態異常治療
  - `kano_lunch.json`: 「白い午後」——「今日は何か、痛いところある？」「……心とか」「まあ、おいで。」
  - `event_kano_opening.json`: DayIndex 98（7月7日・七夕）保健室解放イベント
- **⚙️ FieldController.DoLunchHealthRoom 修正**:
  - ハードコード `kano_intro.json` → ランク進行対応（rank0→intro, rank1以降→rank{n}.json）
  - フォールバック: rankNが存在しない場合introに戻る
- **🗺️ kano NPC アクセシビリティ追加**:
  - `Field3DBuilder`: kano NPC（位置-18,0,14・淡水白）追加（高校建物に隣接）
  - `PrototypeBootstrap`: 2Dマップに保健室（佳乃）NPC拠点追加（位置160,60）
- **📅 CalendarEvent kano_opening 追加**:
  - DayIndex 98 = 7月7日（七夕）。「言えなかった言葉が届く日」という文脈で設計
- **🧪 AmaneLogicTest 拡充**:
  - kano_intro/rank1/rank5/rank10/lunch JSON ロード確認
  - kano_rank1〜rank10 全10本揃い確認
  - CalendarEvent kano_opening (DayIndex=98) 登録確認

## 今夜の作業サマリー（2026-06-12 第9セッション）
- **📖 八雲（八雲老人）rank1-10 JSON完成 — 「言えなかった一言」感情弧フル実装**:
  - `yakumo_rank1.json`: 「言葉の店番」—「言葉は使えば使うほど重くなる」。目に「言えない言葉の色がある」と見抜く
  - `yakumo_rank2.json`: 「常連客」—幸代(亡き妻)の名前が初登場。お茶を出す老人の日常
  - `yakumo_rank3.json`: 「背向きの本」—「48年一緒にいて一度も開けていない」妻の日記
  - `yakumo_rank4.json`: 「閉店後の灯り」—「3年前に逝った。今も、並べとる。」深夜の本棚に話しかける老人を目撃
  - `yakumo_rank5.json`: 「言えなかった一言」—核心。「48年連れ添って、一度も言えなかった。「愛しとる」という、たった一言が。」
  - `yakumo_rank6.json`: 「融合の意味」—「届かなかった言葉に別の経路を作ること。お前さんには、まだ間に合う。」
  - `yakumo_rank7.json`: 「日記を開く前に」—「字が滲んで、読めない。3年、開けられんかった。」主人公に託す
  - `yakumo_rank8.json`: 「幸代の言葉」—日記開封。「あなたのことが、好きです。ずっと。」先に書いてくれていた
  - `yakumo_rank9.json`: 「声に出す」—「愛しとった」を初めて声に出す。「声に出すと、こんなに、ほどけるんじゃな。」
  - `yakumo_rank10.json`: 「最後の言伝 — 八雲」—語り手融合費用半減報酬。「生きとる者には、まだ届く。早う言え。手遅れになる前に。」
  - `yakumo_lunch.json`: 「言ノ葉の午後」—短い温かい交流。「今日は何も聞かなくていい。ただ、おれ。」
- **🏥 保健室アクション（LunchHealthRoom）追加**:
  - `ActionSelectUI`: `LunchHealthRoom` enum値・ボタン追加
  - `FieldController`: `DoLunchHealthRoom()` 実装。慈しみ+3、kano_intro.json参照（未作成時は大気描写5行フォールバック）
  - `PrototypeBootstrap`: lunchHealthRoomBtn生成（y=-196、淡水色テキスト）
- **🗺️ NPCアクセシビリティ改善**:
  - `Field3DBuilder`: yakumo NPC（位置22,0,13・茶色）+ nagisa NPC（位置-5,0,-8・薄紫）追加
  - `PrototypeBootstrap`: 2Dマップに八雲・渚のNPC拠点追加
- **🧪 AmaneLogicTest 拡充**:
  - yakumo_rank1.json ロード + lines≥10 + choices確認
  - yakumo_rank1 が全ランク系列に含まれることを確認（rank1→rank10のチェーンが完全に揃った）

## 今夜の作業サマリー（2026-06-12 第8セッション）
- **💬 渚（鵠沼渚）rank6-10 JSON完成 — 感情弧フル実装（失踪前後）**:
  - `nagisa_rank6.json`: 「明日、やってみる」—rank5の突破口から外へ。素顔配信を決意した前夜、主人公だけに打ち明ける
  - `nagisa_rank7.json`: 「やった。」—配信翌日。「拍子抜けっていうか。怖いものって、もうちょっと怖いと思ってた」
  - `nagisa_rank8.json`: 「DM」—「ずっと待ってました。そっちの渚が見たかった」本物を受け取る怖さ
  - `nagisa_rank9.json`: 「少し消えたい」—本物でやろうとしたら迷子に。「翌朝、圏外」で失踪を静かに告げる
  - `nagisa_rank10.json`: 「最後の言伝 — 渚」—未言界帰還後。「お前がいてくれたから、言えた」封筒を渡す
- **🎭 語り手融合UI（FusionSelectUI）最小実装**:
  - `NarratorInventory.cs` 新規: 語り手コレクション管理（初期4体: 残響/白衣の問い/閉じた言葉/焦熱の拳）
    - `Fuse(a, b)`: 2体消費→新語り手生成。光×闇融合は無属性（統合のテーマ）に昇格
    - `ApplyYakumoBonus()`: 八雲rank10コープ報酬「融合費用半減」の適用先
    - `BaseFusionCost = 500`、`PreviewElement(a, b)`: 融合前プレビュー
  - `FusionSelectUI.cs` 新規: 3ステップ融合UI（一体目選択→二体目選択→確認）
    - `Show(inventory)` / `Hide()` / `OnFusionComplete` / `OnCancelled` イベント
    - `InventoryManager.SpendMoney(cost)` で融合費用消費。所持金不足時にメッセージ表示
    - 群青×蛍光ピンクのカラーパレット準拠
  - `GameManager.cs` 更新: `NarratorInventory Narrators` プロパティ追加・Initialize()で生成
  - `FieldController.cs` 更新:
    - `_fusionSelectUI` SerializeField追加
    - Shop(古書堂「八雲」)インタラクトでFusionSelectUI.Show()を呼ぶ（3D/2D両対応）
    - `OnFusionComplete()`: 融合完了後AP消費→時間帯進行
    - `using Amane.Battle` 追加
  - `PrototypeBootstrap.cs` 更新:
    - InventoryManager のインスタンス生成追加（Awake冒頭）
    - FusionSelectUIパネル生成・全フィールド接続（6ボタン・ステップラベル・コスト表示・確認ボタン）
    - FieldControllerに_fusionSelectUI接続

## 今夜の作業サマリー（2026-06-11 第7セッション）
- **💬 渚（鵠沼渚）rank2-5 JSON完成 — 感情弧フル実装**:
  - `nagisa_rank2.json`: 「数字の重力」—「どっちが本物なんだろう」笑う理由の変質を語る
  - `nagisa_rank3.json`: 「切れた電源」—配信中断に「よかった」と思った自分に気づく
  - `nagisa_rank4.json`: 「素顔の3分」—カメラ切れに気づかず話していた疲れた顔。「その顔のほうが好きだ」選択肢。アーカイブ消せなかった3分
  - `nagisa_rank5.json`: 「言いたいこと」—初めて「疲れた」と声にする。素の苦笑いで終わる
  - `nagisa_lunch.json`: 昼休みの短会話（スマホを伏せて「隣で食べれば」）
- **📖 八雲（八雲老人）初期会話JSON追加**:
  - `yakumo_intro.json`: 「言ノ葉の番人」—古書店での出会い。語り手融合への伏線
  - `event_yakumo_opening.json`: 6月3日（DayIndex 65）に古書店解放を告知するイベントJSON
  - `CalendarEvent.cs` に `yakumo_opening` イベント追加（DayIndex 65 = 6月3日）
- **🍱 昼休みバリエーション拡充**:
  - `FieldAction` enum に `LunchCanteen`（購買）・`LunchRooftop`（屋上）追加
  - `ActionSelectUI`: 2ボタン追加（binding/Show()制御）
  - `FieldController`: `DoLunchCanteen()`（薬草茶入手+度胸+1）・`DoLunchRooftop()`（静けさ+5+情景ダイアログ）
  - `FieldController.OnTimeAdvanced`: 昼休み到達時に actionPanel を表示するトリガー追加
  - `PrototypeBootstrap`: actionPanel を拡張（5ボタン対応・260×290）
- **🧪 AmaneLogicTest 拡充**:
  - nagisa_rank2/rank5/yakumo_intro JSON ロード確認
  - LunchCanteen/LunchRooftop FieldAction 定義確認

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
- **渚（鵠沼渚）会話JSON（nagisa_intro/rank1-5/lunch/event_case_nagisa_start — 第2幕感情弧完成）**
- **八雲（八雲老人）初期会話JSON（yakumo_intro/event_yakumo_opening — 語り手融合の伏線）**
- **昼休みバリエーション拡充（LunchCanteen/LunchRooftop — 購買・屋上アクション追加）**
- **渚（鵠沼渚）rank6-10 JSON（失踪前夜〜帰還〜最後の言伝 — 第2幕感情弧完全完成）**
- **NarratorInventory（語り手コレクション管理・融合ロジック — GameManager統合）**
- **FusionSelectUI（語り手融合3ステップUI — 古書堂「八雲」トリガー・InventoryManager費用連動）**
- **八雲（八雲老人）rank1-10 JSON（言葉の重さ→幸代の日記→声に出す — 第2幕感情弧完全完成）**
- **八雲 yakumo_lunch.json（書店の午後・温かい短会話）**
- **LunchHealthRoom アクション（保健室・慈しみ+3・佳乃kano_intro.json導線）**
- **Field3DBuilder: yakumo/nagisa NPC追加（2D/3D両対応）**
- **佳乃（七尾佳乃）コープ全実装（kano_intro + rank1-10 + lunch + event_kano_opening — 自罰→受容→生を肯定の感情弧完成）**
- **DoLunchHealthRoom ランク進行対応（rank0→intro / rank1以降→rank{n}.json）**
- **CalendarEvent kano_opening 追加（DayIndex 98 = 7月7日・七夕）**
- **Field3DBuilder: kano NPC追加（保健室・2D/3D両対応）**

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
- ~~渚 rank6〜10~~: 完了（失踪前夜→帰還→最後の言伝 全5本完成）
- ~~語り手融合UI（FusionSelectUI）~~: 完了（NarratorInventory + FusionSelectUI + GameManager統合）
- ~~八雲コープ rank2以降のJSON~~: 完了（rank1-10完成）
- ~~kano_intro.json未作成~~: 完了（kano intro+rank1-10+lunch+event全実装済み）
- 真鍋/すずのJSONとイベント: 優先度: 中（第2幕後半解放に必要）
- ~~昼休み: 保健室（佳乃への導線）~~: 完了（LunchHealthRoom実装済み・ランク進行対応済み）
- event_case_kano_start.json: 佳乃の心象世界への潜行イベント（第3幕で必要）: 優先度: 低（rank10到達後のイベント）

---

## 次回セッションの最優先タスク
1. ~~語り手融合UIの最小実装（FusionSelectUI）~~: 完了
2. ~~渚 rank6〜10 JSON~~: 完了（失踪前夜→帰還→最後の言伝）
3. ~~八雲コープ rank1〜10 JSON~~: 完了（言葉の重さ→幸代の日記→声に出す）
4. ~~kano_intro.json / 佳乃コープ全実装~~: 完了（intro+rank1-10+lunch+event）
5. **渚 失踪イベントとFusionSelectUI連動テスト（次回最優先）**
   理由: rank9「消えたい」→失踪イベント（event_case_nagisa_start.json）→ダンジョン潜行→rank10という感情的フローが正しく機能するか確認。渚の心象世界（配信スタジオが水没した未言界）の設計も必要。ゲームの緊張感と時間プレッシャーの核心部分。
6. **DeserializeStateの実体実装（セーブ/ロード完全化）**
   理由: 現在はシリアライズのみ。TimeManager/BondManager/InnerStatSet へのリストア処理追加で完全なセーブ機能に。
7. **真鍋刑事（manabe）コープ初期JSON**
   理由: 第2幕後半で真鍋が事件捜査パートナーとして登場。manabe_intro.json + manabe_rank1-3程度あると第3幕への布石になる。テーマ「正義の暴走→懺悔→真の保護」。

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
- 2026-06-11 **渚rank4「素顔の3分」の設計**: カメラ切れに「気づかない」から「気づく」への転換で、プレイヤーが「見てしまった」共犯感を生む構造。「アーカイブ消せなかった」＝渚自身も変化を望んでいるという無言の伏線。選択肢「その顔のほうが好きだ」に度胸2が必要なのは「本音を言う勇気」の報酬構造と直結。
- 2026-06-11 **渚rank5「疲れた」の1行設計**: 長い文を書かず「……疲れた。」の1行にした。「少し考えて、小さく息を吸った」直後に来ることで、溜めに溜めた一言の重さを演出。postChoiceLines全7行の中で最も短い行が最も重い。
- 2026-06-11 **昼休みパネルの兼用設計**: actionPanelを深夜/昼休み兼用に設計。ActionSelectUI.Show()がスロット別にボタン表示を切替。「深夜」ラベルは常時表示されるがプロトタイプとして許容（FusionSelectUI実装時に専用パネル分離を検討）。
- 2026-06-11 **DoLunchCanteen のItemData動的生成**: ScriptableObject.CreateInstance<ItemData>()でランタイム生成。InventoryManagerがスロットに参照を保持するため GC対象にならない。プロダクション版ではAddressablesで事前定義済みItemDataをロードするべき。
- 2026-06-11 **八雲introの語り手融合伏線設計**: 「言葉は組み合わせると新しい意味になる→語り手も同じ」という台詞でFusionSelectUIへの導線を自然に敷いた。「焦らんでいい」=プレイヤーへのペース誘導でもある。
- 2026-06-12 **渚rank9「少し消えたい」の設計**: 失踪前夜フラグとして機能させた。「翌朝、圏外」という1行で失踪を宣告する。「大丈夫」の反復（rank5で剥がれた「大丈夫」が戻ってくる）によりランク5との感情的循環を作った。
- 2026-06-12 **渚rank10の封筒設計**: 言伝ランクアップ演出（封筒が既読になる）とシンクロさせた。rank10では封筒を渡すという行為がゲームシステムの演出と繋がる。「お前がいてくれたから、言えた」—bondBonus=0の選択肢3つはどれも正解（言葉にした勇気を体験させる）。
- 2026-06-12 **NarratorInventoryの光×闇特別ルール**: 光(赦し)×闇(後悔)の融合は無属性(Almighty)に昇格させた。DESIGN.mdのコアテーマ「後悔と向き合う勇気が明日を取り戻す」の融合=統合のメタファー。他属性は単純にaを優先。
- 2026-06-12 **FusionSelectUIのAwake遅延パターン**: fusionPanel.SetActive(false)した後にAddComponent→SetPrivateFieldで全フィールド設定。Awakeはパネルが最初にアクティブになった時（Show()内でSetActive(true)時）に呼ばれる。このBootstrapパターンを踏襲。
- 2026-06-12 **古書堂のFusion UI接続**: FieldControllerのLocationType.Shop判定をid依存から型依存に変更。Field3DBuilderでloc.Id=nameなので「古書堂「八雲」」という長い文字列チェックを避け、LocationType.Shopで判定。FusionUIがなければ知性/慈しみUP（既存フォールバック）。
- 2026-06-12 **八雲rank5「言えなかった一言」の設計**: コアテーマ「言えなかった後悔と向き合う勇気」を最も直接的に体現するシーン。「48年連れ添って一度も言えなかった」という事実が主人公の未解決の感情と共鳴する構造。rank8「幸代の言葉」で日記に「好きです」が書かれていたことで、伝わっていたのに言えなかったという逆説的な赦しを体験させた。
- 2026-06-12 **DialogueRunner afterLineIndexのバグ発見と対処**: postChoiceLines(未使用フィールド)と混同したafterLineIndex=lines.Count-1の設計は、End()がCheckForChoice()より先に呼ばれて選択肢がスキップされる。正しい実装はafterLineIndex < lines.Count-1で、lines配列に全ラインを含める。akari_rank9.jsonを正例として全yakumo JSONに適用。
- 2026-06-12 **保健室フォールバックの設計**: kano_intro.json未作成時に「消毒液の匂い」「カーテンが揺れる」「言えない痛みが集まる場所」という5行の大気描写でゲーム世界の感触を損なわずに処理。次回セッションでkano_intro.json作成後は自動的に佳乃の初回会話が読み込まれる。
- 2026-06-12 **佳乃rank3「残響の漏れ」の設計**: 主人公の〈残響同調〉が「制御できずに流れ込んできた」という形で佳乃の秘密を暴露するシーン。ペルソナシリーズではペルソナを通してキャラの内面に触れるが、本作では「聴こえてしまった」という主人公の後ろめたさがコープの信頼関係をより深くする設計。「あなたには、聞こえても。」という受容が主人公との特別な絆を確立。
- 2026-06-12 **佳乃rank8「水底の声」のゲームシステム融合**: 光（赦し）属性の語り手を使って謝罪の「場」を作るというシーン設計。DESIGN.mdの「光属性スキル＝赦しの言葉（味方の状態異常回復＋庇護）」との一貫性を保ちつつ、コープと戦闘システムを感情的に直結。捨てた案＝主人公が言葉を代弁する（主人公の個性が薄れるため）。
- 2026-06-12 **kano_opening を七夕（DayIndex 98）に設定した理由**: 「言えなかった言葉が届く日」という七夕の文化的文脈が、佳乃のテーマ（言えなかった後悔）と直結する。case_ren_start（DayIndex 92 = 7月1日）の直後で、第1幕の締めに向かう時期に保健室が開くことで、第2幕前半の多忙感を演出する。
- 2026-06-12 **DoLunchHealthRoom のランク進行修正**: 元実装は `kano_intro.json` ハードコードのため rank1以降の会話に進めなかった。`gm.Bonds.Get("kano")?.Rank ?? 0` でランク取得し、`$"kano_rank{rank}.json"` を動的生成するよう修正。フォールバック（rank存在しない場合はintroに戻る）も保持。
