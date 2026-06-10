using System.Collections.Generic;
using System.Linq;

namespace Amane.Battle
{
    public sealed class TurnSystem
    {
        private readonly List<Combatant> _order = new();
        private int _index;

        public Combatant Current => _index < _order.Count ? _order[_index] : null;
        public int TurnNumber { get; private set; }

        public void BuildOrder(IEnumerable<Combatant> combatants)
        {
            _order.Clear();
            _order.AddRange(combatants.Where(c => c.IsAlive).OrderByDescending(c => c.Agility));
            _index = 0;
            TurnNumber++;
        }

        public Combatant Advance()
        {
            _index++;
            SkipDead();
            return Current;
        }

        public void InsertOneMore(Combatant actor)
        {
            if (_index + 1 <= _order.Count)
                _order.Insert(_index + 1, actor);
        }

        public bool IsRoundOver => _index >= _order.Count;

        private void SkipDead()
        {
            while (_index < _order.Count && !_order[_index].IsAlive)
                _index++;
        }

        public IReadOnlyList<Combatant> Order => _order;
    }
}
