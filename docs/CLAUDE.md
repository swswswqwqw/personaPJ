# RPG開発 引き継ぎ

## 最終更新: 2026-06-10
## 作業ブランチ: claude/sweet-fermat-znRAO
## ゲームタイトル / エンジン: 残響都市アマネ / AMANE: City of Echoes — Unity 6 (6000.3.2f1) + URP

---

## 現在のフェーズ
**設計統合完了 + ダンジョン探索・インベントリシステム移植済み**

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
- キャラクター深度:      8/10（設計+初回会話3キャラ分実装）
- 戦闘の爽快感:          7/10（弱点WEAK!/クリティカル/DOWN/総告白/言継ぎ演出+SE+ActionCommand+EXP/LevelUp）
- 時間システムの機能性:  7/10（CalendarUI接続・AP消費・カレンダーイベント・時間帯遷移演出・ライティング連動）
- 社会リンクの感情密度:  5/10（灯里/律/蓮の初回会話JSON+絆ランクアップ「封筒が既読」演出）
- UI・演出の完成度:      7/10（自作Tween・BattleEffects・TransitionEffect・TypewriterText・BondRankUp・LevelUpEffect・ProceduralAudio）
- ストーリーの引力:      8/10（設計上＋初回会話で感情の種を蒔いた）
- 総合:                  7/10（前回6→7。音響・EXP・環境演出・3Dフィールド統合でRPGループ完成）
前回からの変化: ProceduralAudio+AudioManager（BGM3曲+SE5種+雨）→ExperienceSystem+LevelUpEffect→FieldEnvironment（雨・ライティング・ネームプレート）→3Dフィールド統合→URP互換修正

---

## ペルソナ5 調査結果サマリー（実装に反映済み）
- **色先行デザイン**: 赤×黒→本作は群青×蛍光ピンク。UIの色がテーマを語る。
- **1ドット1フレーム**: 自作Tweenエンジンで全アニメーションを1フレーム単位で制御可能に。
- **モーションリズム**: 急停止→残像のメリハリ。OutBack/OutElastic/OutBounce easing適用。
- **テーマUI一貫性**: P5の切り抜き文字＝怪盗の呼び状→本作の「既読」「封筒」モチーフ。
- **バトンタッチ**: +50%初段/+25%後続。本作「言継ぎ」として実装済み。
- **ALL-OUT ATTACK**: コミック調フィニッシュ+決め台詞→本作「総告白」（4フェーズ演出実装）。

---

## 今夜の作業サマリー
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

---

## 未実装・課題リスト
- ~~BGM/SE 仮素材導入~~: 完了（ProceduralAudio + AudioManager実装済み）
- ~~アイテム・所持金管理~~: 完了（InventoryManager + ItemData実装済み）
- ~~ダンジョン探索システム~~: 完了（MigenkaiManager + DungeonExplorer実装済み）
- ScriptableObject によるキャラ/スキルデータ整備（現在はハードコード）: 優先度: 高
- ~~経験値・レベルアップシステム~~: 完了（ExperienceSystem + LevelUpEffect実装済み）
- 絆ランク2以降の会話JSON: 優先度: 中
- 潜行開始演出の強化（アプリ「未読」→水紋→世界裏返りのリッチ演出）: 優先度: 中
- DeserializeState の実体実装（TimeManager等にリストア用セッター追加）: 優先度: 低
- CalendarEvent発生時の会話/シーン自動遷移: 優先度: 中
- ~~フィールドマップ（2D背景＋キャラ配置）~~: 完了（3Dプリミティブ版で実装済み）

---

## 次回セッションの最優先タスク
1. DungeonExplorerとMigenkaiManagerを既存のBattleManager/FieldControllerに接続
   理由: 移植したダンジョン探索システムを実際にゲームループに組み込む。
2. ScriptableObjectによるデータ駆動化（キャラ・スキル・敵）
   理由: ハードコードから脱し、ItemDataに続いてCharacterData/EnemyData/AbilityDataを整備。
3. 絆ランク2以降の会話JSON拡充
   理由: 社会リンクの感情密度が5/10と最低。テーマの核心なので急ぎたい。
4. DESIGN.md 9-1の戦闘拡張（パーフェクト言継ぎ・逆総告白）を実装
   理由: 統合設計で決定した新メカニクスを戦闘システムに反映。
5. 不要リモートブランチの整理（アーカイブタグ→削除）
   理由: 設計統合完了により、個別ブランチは役割を終えた。

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
