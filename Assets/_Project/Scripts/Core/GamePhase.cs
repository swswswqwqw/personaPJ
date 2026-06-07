namespace Amane.Core
{
    /// <summary>ゲーム全体の大状態。トップレベルのステートマシンが遷移する。</summary>
    public enum GamePhase
    {
        Boot,    // 起動・初期化
        Title,   // タイトル画面
        Field,   // フィールド（日常）
        Battle,  // 戦闘（潜行）
        Dialogue // 会話・絆イベント
    }
}
