# RPG開発 引き継ぎ

## 最終更新: 2026-06-07（第4夜）

## ゲームタイトル / エンジン
**残響のアルカディア（Echoes of Arcadia）** / Unity 2022.3 LTS

## コアテーマ
「聞こえないふりをした声が、世界を静寂に沈める」

---

## 現在のフェーズ
**UIアニメーション・音声基盤完成 / 全UIにDOTween統合、AudioManager構築済み**

---

## 品質スコア（最新・ペルソナ比較）
- キャラクター深度:      5/10（パーティ全員のRank1台詞が揃った。暁の「秒針の音」、みことの「温かい鍵盤」は記憶に残る描写）
- 戦闘の爽快感:          4/10（弱点ヒット時の画面シェイク、ダメージポップアップ、SE連携。DOTweenでコマンドメニュースライドイン。まだBGM/SE素材はない）
- 時間システムの機能性:  4/10（日付変更アニメーション実装。時間帯変化の背景色DOTweenトランジション追加）
- 社会リンクの感情密度:  5/10（パーティ4人全員Rank1完備 + 詩織Rank2 + 凱Rank2。ランクアップ演出にPopIn+PunchScale追加）
- UI・演出の完成度:      4/10（全UIにDOTweenフェード/スライド/ポップ統合。UIAnimator静的ユーティリティ完備。ダメージポップアップ。BGM/SE基盤完成。素材が入れば一気に化ける）
- ストーリーの引力:      5/10（11本の台詞データ。凱の「溶接の跡に刻まれた声」、暁の「時計の秒針」、みことの「温かい鍵盤」。テーマとの接続が一貫して強い）
- 総合:                  4/10
- 前回からの変化: 3→4。UIが「動く」ようになり、音声基盤が完成。面白さの公式の「演出密度」が大幅に改善。

---

## 今夜の作業サマリー（第4夜）

### やったこと
- **DOTweenアニメーション基盤構築（C# 2ファイル新規作成）**
  - UIAnimator: FadeIn/Out, SlideIn/Out, PopIn/Out, PunchScale, ShakePosition, ScaleIn, FlashColorの静的ユーティリティ。全UIコントローラーから共通利用。
  - DamagePopupUI: 弱点/クリティカル/回復/ミスの状況別アニメーション付きダメージ数字。属性色対応。

- **音声システム構築（C# 2ファイル新規作成）**
  - AudioManager: シングルトン。BGMクロスフェード対応（1.5秒遷移）。SFX分類（UI系/戦闘系/会話系）。ボリューム管理。
  - AudioLibrary: ScriptableObject。BGM14トラック + SFX23種類のenum定義。AudioClipマッピング。

- **既存UIコントローラー全面DOTween統合（C# 8ファイル修正）**
  - BattleUIController: コマンドメニューSlideIn、メッセージSlideInFromBottom、共鳴PopIn/PopOut、全共鳴PopIn+PunchScale、勝敗PopIn、弱点/クリティカルのShakePosition、ダメージポップアップ生成、全操作にSE連携
  - DialogueUIController: 会話ウィンドウSlideInFromBottom、選択肢SlideInFromRight、終了SlideOutToLeft、SE連携
  - TitleScreenController: DOTween Sequenceでロゴ→メニューの開幕演出、メニュー操作にFadeOut+DelayedCall
  - SceneLoader: FadeOut/FadeInをDOTween化（Ease.InQuad/OutQuad）
  - BondUIController: 絆マップFadeIn/Out、キャラ詳細SlideInFromRight、ランクアップPopIn+PunchScale+SE
  - StatRankUpUI: PopIn+PunchScale+SE → DelayedCallでPopOut
  - PauseMenuController: メニューSlideInFromRight/FadeOut、サブパネルFadeIn、全操作にSE
  - FieldHUDController: 日付変更オーバーレイ（ScaleIn+FadeIn→FadeOut）、時間帯変化DOTween色遷移

- **実台詞データ作成（JSON 3ファイル新規作成）**
  - akira_bond_rank1.json: 暁Rank1 17行 — 空を見上げる暁、「先のことを考えない」宣言、秒針の残響
  - mikoto_bond_rank1.json: みことRank1 20行 — 鍵盤に触れる指、「聴きたくなかったから」の告白、温かい鍵盤
  - gai_bond_rank2.json: 凱Rank2 21行 — 弟の車椅子用荷台の修理、母の失踪、「溶接の跡に刻まれた声」

### なぜそれを選んだか
面白さの公式「演出密度」が最大のボトルネック（2/10）だった。ペルソナ5のUIが心を掴むのは
「動く」からであり、静的UIでは設計がどれだけ良くても体験が伝わらない。
DOTween統合 + AudioManager構築で「演出密度」を一気に引き上げ、同時にパーティ全員のRank1を
揃えて「感情移入」の均等化も達成した。

### 実装してみての気づき・反省
- UIAnimatorを静的ユーティリティにしたのは正解。全コントローラーから1行で呼べるため、アニメーション追加のコストがゼロに近い。将来的にDOTweenの設定を一箇所で変更できる。
- 暁の「時計の秒針の音」は想像以上に良い伏線。死神アルカナの運命を暗示しつつ、蓮の残響聴取能力で「聴こえてしまう」設計が核心テーマと一致する。
- みことのRank1で「何も書かず、ただそこにいる」が最高ポイント（3pt）になる設計は、このゲーム全体の哲学を体現している。「聴く」ことの本質は「返事をする」ことではなく「そこにいる」こと。
- 凱のRank2「溶接の跡に刻まれた愛情」は言語化されない声の究極の表現。殴ることしかできないと嘆く凱が、実は荷台という形で「声」を出していたことに気づく構造が美しい。
- BattleUIControllerのSetOverlayVisible→UIAnimator移行で、async void + Task.Delayパターンが全廃された。DOVirtual.DelayedCallに統一されたことで、コルーチンの管理が簡潔になった。

---

## 実装済み機能リスト
- [x] DESIGN.md（ゲーム設計聖典）
- [x] Unityプロジェクト ディレクトリ構成
- [x] GameManager（ゲームフェーズ管理）
- [x] GameEventBus（イベントバスシステム）
- [x] StateMachine（汎用ステートマシン）
- [x] TimeManager（カレンダー・時間帯・天候・行動ポイント）
- [x] BattleManager（戦闘ループ・共鳴・全共鳴攻撃・絆ボーナス連携済み）
- [x] BattleUnit / DamageCalculator（ダメージ計算・属性相性）
- [x] BattleRewardProcessor（勝利報酬・絆ボーナス反映）
- [x] BattleFlowController（戦闘操作フロー完成：コマンド→ターゲット→スキル→実行）
- [x] EnemyAI（敵行動パターン：弱点優先・回復判断・最弱者ターゲット）
- [x] SocialLinkManager（絆の調べ・ランク管理）
- [x] DialogueSystem + DialogueDataLoader（会話・選択肢・JSON読み込み）
- [x] PlayerStats（5種内面ステータス・ランクアップ）
- [x] SaveManager / SaveData（セーブ/ロード基盤）
- [x] EchoRealmManager / EchoRealmData（残響界ダンジョン管理）
- [x] ScriptableObject定義（Character, Ability, Enemy, ResonanceBody, FusionRecipe, Location）
- [x] SceneLoader（DOTweenフェード付きシーン遷移）
- [x] UIAnimator（DOTween静的ユーティリティ：Fade/Slide/Pop/Shake/Scale/Flash）
- [x] UIColors（テーマカラー定数・属性色マッピング）
- [x] TitleScreenController（DOTween Sequence開幕演出・BGM連携）
- [x] BattleUIController（DOTweenアニメーション全面統合・ダメージポップアップ・SE連携）
- [x] DamagePopupUI（属性色・弱点/クリティカル対応のアニメーション数字）
- [x] PartyMemberStatusUI / EnemyStatusUI（HPSPバー表示）
- [x] DialogueUIController（DOTweenスライド/フェード・SE連携）
- [x] FieldHUDController（DOTween日付変更演出・時間帯色遷移・デッドライン警告）
- [x] CalendarUIController（カレンダー・デッドラインマーク）
- [x] ActionSelectUIController（行動選択UI）
- [x] BondUIController（DOTween絆マップフェード・詳細スライド・ランクアップPopIn+PunchScale）
- [x] PauseMenuController（DOTweenスライド/フェード・SE連携）
- [x] StatRankUpUI（DOTween PopIn+PunchScale・SE連携）
- [x] AudioManager（BGMクロスフェード・SFX再生・ボリューム管理）
- [x] AudioLibrary（ScriptableObject BGM14トラック+SFX23種類定義）
- [x] FieldManager（場所移動・NPC配置・時間/天候制限・アクティビティ）
- [x] FieldUIController（ナビゲーション・NPC・アクティビティUI）
- [x] GameFlowController（ゲームループ統合：パーティ→戦闘→社会リンク→プロローグ）
- [x] LocationData（17エリア定義・NPC配置・アクティビティ定義）
- [x] プロローグ台詞データ（prologue_train.json）
- [x] 詩織初対面台詞データ（shiori_first_meeting.json）
- [x] 暁初対面台詞データ（akira_first_meeting.json）
- [x] 凱初対面台詞データ（gai_first_meeting.json）
- [x] みこと初対面台詞データ（mikoto_first_meeting.json）
- [x] 詩織Rank1台詞データ（shiori_bond_rank1.json）
- [x] 詩織Rank2台詞データ（shiori_bond_rank2.json）
- [x] 凱Rank1台詞データ（gai_bond_rank1.json）
- [x] 凱Rank2台詞データ（gai_bond_rank2.json）
- [x] 暁Rank1台詞データ（akira_bond_rank1.json）
- [x] みことRank1台詞データ（mikoto_bond_rank1.json）
- [ ] BGM / SE素材（仮素材でも）
- [ ] 残響界ダンジョン生成・探索
- [ ] 共鳴体フュージョンUI・ロジック
- [ ] アイテムシステム
- [ ] ショップシステム
- [ ] 残りキャラ（祐介・葵・花・竜一・紗夜・陽菜・源蔵）の台詞
- [ ] パーティ全員Rank3以降の台詞
- [ ] 戦闘カットイン演出
- [ ] 共鳴覚醒イベント

---

## 未実装・課題リスト
- **BGM/SE素材**: 優先度最高。AudioManagerは完成、素材が入れば即座に鳴る。仮素材でもいいので用意する
- **残響界ダンジョン**: 優先度高。ランダム or 固定マップの探索。フロア遷移・エンカウント・ボス
- **アイテムシステム**: 優先度高。回復アイテム・素材アイテム・ショップ。戦闘で使えるように
- **共鳴体フュージョン**: 優先度中。ペルソナ合体相当。UIと計算ロジック
- **台詞量の拡充**: 優先度中。パーティ全員Rank3〜5、非パーティキャラの初対面
- **戦闘カットイン**: 優先度中。共鳴覚醒・全共鳴攻撃時のキャラカットイン

---

## 次回セッションの最優先タスク

1. **最重要: 残響界ダンジョンシステム**
   ダンジョン生成（ランダム or 固定）、フロア探索UI、エンカウント発生、
   ボス部屋、被害者救出イベント。ゲームの「もう一つの柱」を作る。
   フィールド→残響界→戦闘→帰還のループを完成させる。

2. **アイテムシステム + ショップ**
   ItemData ScriptableObject、インベントリ管理、戦闘中アイテム使用、
   ショップUI。戦闘の奥行きと日常パートの経済圏を作る。

3. **非パーティキャラクターの初対面台詞**
   祐介（カフェマスター）、葵（担任教師）、花（図書室の少女）の初対面を書く。
   これにより社会リンクの選択肢が広がり、時間の希少性が実感できる。

---

## 設計メモ（意思決定の記録）

### 2026-06-07（第1夜）
- エンジン: Unity 2022.3 LTS に決定
- テーマ: 「声」「聴く」「共鳴」に統一
- 属性システム: 感情ベースの6属性を採用
- どんでん返し設計: 2つの大きな反転を確定

### 2026-06-07（第2夜）
- **UI設計方針: CanvasGroup統一パターンを採用**
  全UIパネルをCanvasGroupで管理し、alpha/interactable/blocksRaycastsの3点セットで
  表示/非表示を制御。DOTweenを入れたとき、alphaアニメーションだけで自然な遷移になる。

- **台詞のタイプライター速度: 0.03秒/文字 + 句読点0.15秒**
  ペルソナ5のテキスト速度を参考に設定。速すぎず遅すぎず、感情が乗るテンポ。

- **プロローグは地の文中心で台詞を極力削る方針**
  蓮の「声がない」状態を表現するため、プロローグでは蓮の台詞をゼロにした。
  ペルソナ3のオープニングの静謐さを参考にしている。

- **戦闘報酬に絆ボーナスを直結させた**
  葵（力）のランク→EXP倍率、祐介（戦車）のランク→ドロップ率。
  「日常パートの選択が戦闘に影響する」ペルソナの核心設計を実装。

### 2026-06-07（第3夜）
- **敵AIの行動優先度: 弱点 > 回復 > ランダム攻撃**
  ペルソナシリーズの敵は弱点を的確に突いてくる。これがpress turnシステム（本作では共鳴）の
  緊張感を生む。回復は30%以下でのみ発動し、プレイヤーに「今倒さないと回復される」という
  判断を強いる。

- **フィールドは「ロケーションベース」に決定**
  3Dフィールドウォークではなく、ロケーション選択→NPC/アクティビティ選択のメニュー式。
  ペルソナ3のタルタロス外パートに近い。開発効率と台詞密度を両立できる。

- **社会リンク台詞の「最高ポイント選択肢」設計思想**
  最もポイントが高い選択肢は「相手の核心を突く」もの。
  ペルソナ5のコープ会話で最適解が「空気を読む」のではなく「本質を見抜く」のと同じ設計。

### 2026-06-07（第4夜）
- **UIAnimatorを静的ユーティリティクラスとして設計**
  MonoBehaviourではなく静的クラスにした。UIアニメーションはシーン非依存で使える必要があり、
  シングルトンのライフサイクル管理を避けたかった。DOTweenは内部的にシーン非依存で動作するため問題ない。

- **AudioManagerのBGMクロスフェード方式: デュアルAudioSource**
  bgmSourceとbgmSubSourceの2つを用意し、クロスフェード完了時にswapする。
  ペルソナ5のBGM遷移（戦闘突入→フィールド復帰）がシームレスなのはこの方式。

- **社会リンクRank1の「最高ポイント」設計パターンの深化**
  暁Rank1: 「何も言わず隣に座る」(3pt) — 心配を口にせず寄り添う
  みことRank1: 「ここが好きだから」(3pt) — 相手のためではなく自分の意志で来たと伝える
  みことRank1第2選択: 「何も書かず、ただそこにいる」(3pt) — 言葉すら不要な理解
  凱Rank2: 「一人で抱えるな」(3pt) — 直接的に孤立を否定する
  パターン: 「相手を変えようとしない。ただ、相手の隣にいることを選ぶ」——これが本作の核心。

- **async void + Task.Delay パターンの全廃**
  DOTween統合に伴い、UI表示の待機をDOVirtual.DelayedCallに統一した。
  async voidは例外が捕捉されにくく、オブジェクト破棄後のコールバック問題もある。
  DOTweenのSequence/DelayedCallはKill可能で、MonoBehaviourの破棄と連動できる。
