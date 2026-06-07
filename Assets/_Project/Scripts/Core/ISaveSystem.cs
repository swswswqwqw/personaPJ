namespace Amane.Core
{
    /// <summary>
    /// セーブ/ロードの抽象。実体（JSON/バイナリ）は後日実装する。
    /// セーブ演出は「日記を書く」（DESIGN.md 必須演出11）。
    /// </summary>
    public interface ISaveSystem
    {
        bool HasSave(int slot);
        void Save(int slot);
        bool Load(int slot);
    }

    /// <summary>まだ永続化を実装しないための仮実装（NullObject）。</summary>
    public sealed class NullSaveSystem : ISaveSystem
    {
        public bool HasSave(int slot) => false;
        public void Save(int slot) { /* TODO: 日記演出つき永続化を後日実装 */ }
        public bool Load(int slot) => false;
    }
}
