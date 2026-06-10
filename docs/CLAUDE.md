# RPG開発 引き継ぎ

## 最終更新: 2026-06-10

## ゲームタイトル / エンジン
**空蝉のアルカナ (ARCANA OF HOLLOWS)** / Unity 2022.3 LTS (URP)

---

## 現在のフェーズ
初期設計完了 / コアシステム骨格実装済み / UIおよびシーン未実装

---

## 品質スコア（最新・ペルソナ比較）
- キャラクター深度:      3/10（設計書で深く定義。プロローグ・心弦Rank1の台詞を執筆済み）
- 戦闘の爽快感:          1/10（BattleManagerの骨格のみ。演出・UIなし）
- 時間システムの機能性:  2/10（TimeManager実装済み。UIなし）
- 社会リンクの感情密度:  2/10（HeartStringManager実装済み。会話データ2本）
- UI・演出の完成度:      0/10（完全未実装）
- ストーリーの引力:      3/10（3幕構成・どんでん返し設計済み。体験不可能）
- 総合:                  2/10
- 前回からの変化: 全項目0→現在値（初回セッション）

---

## 今夜の作業サマリー

### やったこと
- **docs/DESIGN.md を完全執筆**（約800行の設計聖典）
  - タイトル「空蝉のアルカナ」決定、コアテーマ・世界観・キャラクター5人＋心弦キャラ8人を詳細設計
  - 戦闘システム（8属性・1MORE・ハーモニーブレイク）、時間システム（5時間帯）、心弦システム（10ランク）を設計
  - 3幕構成のストーリー、3つのどんでん返し、3種のエンディング分岐を設計
  - UI/UXカラーパレット・12個の必須演出リストを定義
- **ディレクトリ構成作成**（Unityプロジェクト構造、12サブフォルダ）
- **コアC#スクリプト実装**（15ファイル）:
  - Core: GameManager, EventBus, SceneTransitionManager, SaveManager, StoryFlagManager
  - Time: TimeManager, CalendarData
  - Battle: BattleManager（BattleUnit, DamageResult含む）
  - Data: ElementSystem, CharacterData, SkillData, EnemyData, EchoData
  - Dialogue: DialogueManager, DialogueLoader
  - Social: HeartStringManager, HeartStringData
  - Echo: EchoFusionManager, FusionTable
  - Player: PlayerStats（5種ソーシャルステータス）
  - Audio: AudioManager（BGMフェード・SE・イベント連動）
  - Field: ActionSelectionManager（放課後/夜の行動選択）
- **会話データ（JSON）2本**:
  - prologue_001.json: プロローグ（蓮司の転校初日・凱との出会い）
  - hina_heartstring_rank1.json: 陽菜の心弦ランク1イベント

### なぜそれを選んだか
プロジェクトが完全な空の状態だったため、design_init_prompt.mdの指示に従い設計書を生成し、初日タスクリストの1〜3を実行した。全てのゲーム体験はコードの上に立つため、まずシステムの骨格を最優先とした。

### 実装してみての気づき・反省
- 設計書を書く過程で「共鳴が双方向だった」というどんでん返し3が生まれた。これはペルソナにない独自の感動ポイントになりうる
- EventBusによる疎結合設計は正しい判断。各システムが独立してテスト可能
- 会話データを書いて気づいたが、陽菜の「いい子仮面」の描写は繊細さが必要。やりすぎると鬱陶しく、足りないと伝わらない
- 台詞の選択肢にステータス要求を入れる仕組みは、システムとナラティブの融合として機能する

---

## 実装済み機能リスト
- ✅ ゲーム設計書（DESIGN.md）
- ✅ ディレクトリ構成（Unity準拠）
- ✅ GameManager（ゲームフェーズ管理・ポーズ）
- ✅ EventBus（型安全なイベントシステム）
- ✅ SceneTransitionManager（フェード付きシーン遷移）
- ✅ TimeManager（日付・時間帯・カレンダー管理）
- ✅ CalendarData（ScriptableObjectカレンダー）
- ✅ BattleManager（ターン制戦闘ループ・弱点システム・1MORE・ハーモニーブレイク）
- ✅ ElementSystem（8属性・6種親和性）
- ✅ CharacterData / EnemyData / SkillData / EchoData（ScriptableObject定義）
- ✅ DialogueManager + DialogueLoader（会話システム・タイプライター・選択肢）
- ✅ HeartStringManager（心弦ランクアップ・報酬・イベント）
- ✅ HeartStringData（心弦ScriptableObject定義）
- ✅ EchoFusionManager + FusionTable（反響体合体システム）
- ✅ PlayerStats（レベル・経験値・ゴールド・5種ソーシャルステータス）
- ✅ SaveManager（16スロット・JSON永続化）
- ✅ AudioManager（BGMフェード・SE・イベント自動連動）
- ✅ StoryFlagManager（ストーリーフラグ管理）
- ✅ ActionSelectionManager（放課後/夜の行動選択・ステータス連動）
- ✅ 会話データ: プロローグ、陽菜心弦Rank1

---

## 未実装・課題リスト
- 🔴 Unity Editor プロジェクト未作成（.unityproj / ProjectSettings）: 優先度・最高
- 🔴 タイトル画面: 優先度・高
- 🔴 戦闘UI（コマンド選択・HP/SP表示・エフェクト）: 優先度・高
- 🔴 フィールド画面（移動・NPC・インタラクション）: 優先度・高
- 🔴 カレンダーUI（日付表示・時間帯遷移演出）: 優先度・高
- 🔴 心弦UI（一覧表示・ランクアップ演出）: 優先度・中
- 🔴 メニューUI（ステータス・装備・反響体・アイテム）: 優先度・中
- 🔴 反響体合体UI: 優先度・中
- 🟡 敵AI拡張（ボス用特殊行動パターン）: 優先度・中
- 🟡 装備システム: 優先度・低
- 🟡 アイテムシステム: 優先度・低
- 🟡 BGM/SE仮素材の用意: 優先度・中
- 🟡 追加会話データ（プロローグ全体・凱/透/詩織の心弦）: 優先度・中

---

## 次回セッションの最優先タスク

1. **最重要: 戦闘UIの実装**
   なぜ: 戦闘の爽快感がゲームの生死を分ける。コマンド選択→弱点ヒット→1MORE→ハーモニーブレイクの「気持ちいい流れ」を最速で体験可能にする必要がある。BattleManagerは動くので、UIを被せれば戦闘が遊べるようになる

2. **カレンダー/時間帯UIの実装**
   なぜ: 時間の有限性はペルソナの面白さの根幹。「今日は何をしよう」の緊張感を視覚化する

3. **プロローグの会話データ拡充**
   なぜ: キャラクターへの感情移入は最序盤で決まる。凱・陽菜との最初の会話が心を掴めなければ全てが失敗する

---

## 設計メモ（意思決定の記録）

### 2026-06-10 初期設計・初回実装
- **エンジン選定:** Unity 2022.3 LTS。ペルソナ的な2D UI演出密度を最優先し、C#+UniTask+DOTweenの生産性を重視。Unrealは3Dリッチ表現向きだが、本作のUI中心の設計思想にはUnityが適合
- **タイトル決定:** 「空蝉のアルカナ」。空蝉（蝉の抜け殻）＝中身のない仮面、というメタファー。ペルソナの「仮面」と同根だが「中身が空かもしれない」不安を追加
- **戦闘システム:** Press Turn変種を採用。ペルソナの実績あるシステムを土台にしつつ、「ハーモニーブレイク」で心弦ランクとの連動（演出が変化する）を独自要素として追加
- **心弦（Heart Strings）:** 「弦のチューニング」メタファー。同じ周波数に合わせる＝共鳴する、というゲーム全体のテーマとの一貫性
- **捨てた案:** 異世界をSNS空間にする案（陳腐化リスク）、主人公をAI的存在にする案（感情移入困難）、リアルタイム戦闘案（ペルソナの哲学「ターン制＝内省」に反する）
