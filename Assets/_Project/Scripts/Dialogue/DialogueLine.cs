using System;

namespace Amane.Dialogue
{
    /// <summary>
    /// 会話1行のデータ。StreamingAssets/Dialogue/*.json から読み込む（後日 DialogueRunner で実装）。
    /// 説明台詞を避け、emotion で「間」と表情を制御できるよう拡張余地を残す。
    /// </summary>
    [Serializable]
    public sealed class DialogueLine
    {
        public string speakerId;   // 例: "akari"
        public string text;        // 表示テキスト
        public string emotion;     // 例: "neutral" / "broken" / "smile"
        public float preSilence;   // 重要台詞前の無音（秒）。DESIGN.md 演出9。
    }
}
