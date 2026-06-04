# このリポジトリについて

ここはペルソナ3〜5レベルのJRPGをC# + Unity / Unreal Engineで作り続けるプロジェクトです。
Claude Code Routinesによって毎晩自動的に開発が進みます。

## エージェントへの絶対ルール

1. 作業前に必ず読むファイル（順番厳守）
   - docs/DESIGN.md — ゲームの聖典。コアテーマに常に立ち返る
   - docs/CLAUDE.md — 前回の引き継ぎ。ここから今夜の優先タスクを把握する

2. 毎晩の作業手順
   - docs/routine_prompt.md の STEP 1〜7 を全て実行する
   - ステップのスキップは禁止
   - 必ず最後に docs/CLAUDE.md を更新して終了する

3. 方針に迷ったとき
   - docs/research_prompt.md の手順で調査・壁打ちを行う
   - Web検索でペルソナシリーズの設計を必ず調べる
   - 3案を出してから決断する

4. 品質の判断基準
   - ペルソナ3〜5と比較して、この実装は遜色ないか
   - プレイヤーの感情が動くかを技術的完成度より優先する
   - 面白さの公式: 感情移入 × システム融合 × 時間の希少性 × 演出密度

5. 技術スタック
   - 言語: C#
   - エンジン: Unity または Unreal Engine（DESIGN.mdに記載）
   - Unity使用時: UniTask / DOTween / ScriptableObject / Addressables
   - Unreal使用時: GAS / CommonUI / GameInstance / DataTable

6. コミット・ブランチ運用
   - 作業ブランチ: claude/nightly-YYYYMMDD
   - コミットメッセージ: [nightly] 今夜の作業サマリー1行
   - PRはdraftで作成し、概要に品質スコアを記載する

## ディレクトリ構成

personaPJ/
├── CLAUDE.md                    エージェント向け最上位指示
├── .claude/
│   ├── settings.json            Claude Code設定・権限・hooks
│   └── hooks/
│       └── on_stop.sh           セッション終了時チェック
├── docs/
│   ├── DESIGN.md                ゲーム設計書（聖典・初回生成後は参照専用）
│   ├── CLAUDE.md                開発引き継ぎ（毎晩更新）
│   ├── design_init_prompt.md    初回のみ：設計書生成プロンプト
│   ├── routine_prompt.md        毎晩：夜間深化ルーティン本体
│   └── research_prompt.md       迷ったとき：調査・壁打ち
└── Unity or Unreal プロジェクト（design_init_prompt.md 実行後に構築）
