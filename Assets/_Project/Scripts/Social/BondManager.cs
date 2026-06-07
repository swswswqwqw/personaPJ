using System;
using System.Collections.Generic;
using Amane.Core;

namespace Amane.Social
{
    /// <summary>
    /// 全ての絆「言伝」を統括する Plain C# クラス。
    /// 同アルカナの語り手所持で x1.5 補正を適用し、ランクアップを EventChannel へ通知する。
    /// </summary>
    public sealed class BondManager
    {
        private readonly EventChannel _events;
        private readonly Dictionary<string, Bond> _bonds = new();
        private readonly HashSet<Arcana> _ownedNarratorArcana = new();

        public BondManager(EventChannel events)
        {
            _events = events ?? throw new ArgumentNullException(nameof(events));
        }

        public IReadOnlyCollection<Bond> All => _bonds.Values;

        public void Register(Bond bond)
        {
            if (bond == null) throw new ArgumentNullException(nameof(bond));
            _bonds[bond.Id] = bond;
        }

        public Bond Get(string id) => _bonds.TryGetValue(id, out var b) ? b : null;

        /// <summary>所持している「語り手」のアルカナを記録する（融合で増減）。</summary>
        public void SetNarratorArcanaOwned(Arcana arcana, bool owned)
        {
            if (owned) _ownedNarratorArcana.Add(arcana);
            else _ownedNarratorArcana.Remove(arcana);
        }

        /// <summary>
        /// 絆ポイントを与える。同アルカナの語り手を所持していれば x1.5。
        /// ランクが上がれば BondRankUpEvent を発行する。
        /// </summary>
        public void GivePoints(string bondId, int basePoints)
        {
            var bond = Get(bondId);
            if (bond == null) return;
            int amount = _ownedNarratorArcana.Contains(bond.Arcana)
                ? (int)Math.Round(basePoints * 1.5)
                : basePoints;
            if (bond.AddPoints(amount))
                _events.Publish(new BondRankUpEvent(bond.Id, bond.Rank));
        }

        /// <summary>DESIGN.md の8キャラを初期登録する。</summary>
        public void SeedDesignBonds()
        {
            Register(new Bond("akari",  "望月 灯里", Arcana.Sun));
            Register(new Bond("ritsu",  "久遠 律",   Arcana.Hermit));
            Register(new Bond("nagisa", "鵠沼 渚",   Arcana.Moon));
            Register(new Bond("yakumo", "八雲 老人", Arcana.Hierophant));
            Register(new Bond("kano",   "七尾 佳乃", Arcana.Lovers));
            Register(new Bond("ren",    "朝霧 蓮",   Arcana.Chariot));
            Register(new Bond("manabe", "真鍋 刑事", Arcana.Justice));
            Register(new Bond("suzu",   "雛森 すず", Arcana.Star));
        }
    }
}
