using System.Collections.Generic;

namespace Amane.Battle
{
    // 語り手（ナレーター）—— ペルソナ相当の仮面。主人公のみが複数所持・切り替え可能。
    // 各語り手は固有の主属性・スキルセット・親和性テーブルを持つ。
    // テーマ根拠: 「複数の物語を内包する」=過去と現在の自己を統合する力の表れ。
    public sealed class Narrator
    {
        public string Id { get; }
        public string DisplayName { get; }
        // 語り手の主属性（デュアルナレーター時の属性相性計算に使用）
        public Element PrimaryElement { get; }
        public AffinityTable Affinities { get; }
        public List<Skill> Skills { get; }

        public Narrator(string id, string displayName, Element primary,
                        AffinityTable affinities = null, List<Skill> skills = null)
        {
            Id = id;
            DisplayName = displayName;
            PrimaryElement = primary;
            Affinities = affinities ?? AffinityTable.AllNormal();
            Skills = skills ?? new List<Skill>();
        }

        // デフォルト語り手: 無属性の基本スペック
        public static Narrator CreateDefault(string id, string name)
            => new Narrator(id, name, Element.Almighty);
    }
}
