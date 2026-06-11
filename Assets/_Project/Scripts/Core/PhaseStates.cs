using System;
using UnityEngine;

namespace Amane.Core
{
    /// <summary>
    /// トップレベルのフェーズ状態。各フェーズの入退場をログし、
    /// 後日それぞれのシーン/コントローラ起動へ拡張する。
    /// 軽量な Plain C# 状態（GameManager から駆動）。
    /// </summary>
    public abstract class PhaseStateBase : IState
    {
        protected readonly GameManager Game;
        public abstract GamePhase Phase { get; }

        protected PhaseStateBase(GameManager game)
        {
            Game = game ?? throw new ArgumentNullException(nameof(game));
        }

        public virtual void Enter() => Debug.Log($"[Phase] Enter {Phase}");
        public virtual void Tick(float deltaTime) { }
        public virtual void Exit() => Debug.Log($"[Phase] Exit {Phase}");
    }

    public sealed class BootState : PhaseStateBase
    {
        public BootState(GameManager game) : base(game) { }
        public override GamePhase Phase => GamePhase.Boot;

        public override void Enter()
        {
            base.Enter();
            // 初期化が済んだら即タイトルへ。
            Game.Machine.ChangeTo<TitleState>();
        }
    }

    public sealed class TitleState : PhaseStateBase
    {
        public TitleState(GameManager game) : base(game) { }
        public override GamePhase Phase => GamePhase.Title;
    }

    public sealed class FieldState : PhaseStateBase
    {
        public FieldState(GameManager game) : base(game) { }
        public override GamePhase Phase => GamePhase.Field;
    }

    public sealed class BattleState : PhaseStateBase
    {
        public BattleState(GameManager game) : base(game) { }
        public override GamePhase Phase => GamePhase.Battle;
    }

    public sealed class DialogueState : PhaseStateBase
    {
        public DialogueState(GameManager game) : base(game) { }
        public override GamePhase Phase => GamePhase.Dialogue;
    }

    public sealed class DungeonState : PhaseStateBase
    {
        public DungeonState(GameManager game) : base(game) { }
        public override GamePhase Phase => GamePhase.Dungeon;
    }
}
