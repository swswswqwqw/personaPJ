using System.Collections.Generic;

namespace Amane.Battle
{
    public sealed class AffinityTable
    {
        private readonly Dictionary<Element, Affinity> _map = new();

        public Affinity Get(Element element)
        {
            if (element == Element.Almighty) return Affinity.Normal;
            return _map.TryGetValue(element, out var a) ? a : Affinity.Normal;
        }

        public void Set(Element element, Affinity affinity) => _map[element] = affinity;

        public static AffinityTable AllNormal() => new();

        public static AffinityTable WithWeak(params Element[] weaknesses)
        {
            var table = new AffinityTable();
            foreach (var e in weaknesses) table.Set(e, Affinity.Weak);
            return table;
        }

        public static AffinityTable Build(
            Element[] weak = null, Element[] resist = null,
            Element[] nullify = null, Element[] drain = null, Element[] repel = null)
        {
            var table = new AffinityTable();
            if (weak != null) foreach (var e in weak) table.Set(e, Affinity.Weak);
            if (resist != null) foreach (var e in resist) table.Set(e, Affinity.Resist);
            if (nullify != null) foreach (var e in nullify) table.Set(e, Affinity.Null);
            if (drain != null) foreach (var e in drain) table.Set(e, Affinity.Drain);
            if (repel != null) foreach (var e in repel) table.Set(e, Affinity.Repel);
            return table;
        }
    }
}
