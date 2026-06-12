using System.Collections.Generic;
using System.Linq;

namespace Amane.Battle
{
    // 主人公が保持する語り手（ナレーター）のコレクション管理。
    // 融合（フュージョン）の受付・実行も担う。
    // テーマ根拠: 「言葉は組み合わせると新しい意味になる」（八雲老人）
    public sealed class NarratorInventory
    {
        private readonly List<Narrator> _owned = new();
        private bool _yakumoBonus; // 八雲Rank10コープ報酬: 融合費用半減

        public const int BaseFusionCost = 500;

        public IReadOnlyList<Narrator> Owned => _owned;
        public int FusionCost => _yakumoBonus ? BaseFusionCost / 2 : BaseFusionCost;

        public NarratorInventory()
        {
            SeedInitialNarrators();
        }

        private void SeedInitialNarrators()
        {
            _owned.Add(new Narrator("resonance",     "残響",       Element.Almighty));
            _owned.Add(new Narrator("light_question","白衣の問い", Element.Light));
            _owned.Add(new Narrator("silent_word",   "閉じた言葉", Element.Dark));
            _owned.Add(new Narrator("burning_fist",  "焦熱の拳",   Element.Fire));
        }

        public void Add(Narrator n)
        {
            if (!_owned.Any(x => x.Id == n.Id))
                _owned.Add(n);
        }

        public void ApplyYakumoBonus() => _yakumoBonus = true;

        // 融合: 2体の語り手を合体させ、新たな語り手を生成する。
        // 元の2体はインベントリから消費される。
        public Narrator Fuse(Narrator a, Narrator b)
        {
            var fusedId   = $"fused_{a.Id}_{b.Id}";
            var fusedName = $"{a.DisplayName}×{b.DisplayName}";
            var mergedElement = MergeElement(a.PrimaryElement, b.PrimaryElement);
            var fusedSkills   = a.Skills.Take(2).Concat(b.Skills.Take(2)).ToList();

            _owned.Remove(a);
            _owned.Remove(b);

            var fused = new Narrator(fusedId, fusedName, mergedElement,
                                     AffinityTable.AllNormal(), fusedSkills);
            _owned.Add(fused);
            return fused;
        }

        // 融合後の属性プレビュー（実際の消費なし）
        public Element PreviewElement(Narrator a, Narrator b) => MergeElement(a.PrimaryElement, b.PrimaryElement);

        private static Element MergeElement(Element a, Element b)
        {
            if (a == Element.Almighty) return b;
            if (b == Element.Almighty) return a;
            // 光×闇は特別: 無属性に昇格（統合のテーマ）
            if ((a == Element.Light && b == Element.Dark) ||
                (a == Element.Dark  && b == Element.Light))
                return Element.Almighty;
            return a;
        }
    }
}
