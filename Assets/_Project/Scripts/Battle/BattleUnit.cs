using AriaOfBacklight.Data;

namespace AriaOfBacklight.Battle
{
    public class BattleUnit
    {
        public string Name { get; }
        public int MaxHP { get; }
        public int CurrentHP { get; private set; }
        public int MaxSP { get; }
        public int CurrentSP { get; private set; }
        public int Attack { get; }
        public int Defense { get; }
        public int Speed { get; }
        public ElementType PrimaryElement { get; }
        public ElementType WeaknessElement { get; }
        public bool IsDown { get; set; }
        public bool IsAlive => CurrentHP > 0;
        public bool IsPlayer { get; }

        public BattleUnit(CharacterData data, bool isPlayer, int level = 1)
        {
            Name = data.characterName;
            MaxHP = data.baseHP + level * 8;
            CurrentHP = MaxHP;
            MaxSP = data.baseSP + level * 3;
            CurrentSP = MaxSP;
            Attack = data.baseAttack + level * 2;
            Defense = data.baseDefense + level * 2;
            Speed = data.baseSpeed + level;
            PrimaryElement = data.primaryElement;
            WeaknessElement = data.weakness;
            IsPlayer = isPlayer;
        }

        public BattleUnit(string name, int hp, int sp, int atk, int def, int spd,
            ElementType primary, ElementType weakness, bool isPlayer)
        {
            Name = name;
            MaxHP = hp;
            CurrentHP = hp;
            MaxSP = sp;
            CurrentSP = sp;
            Attack = atk;
            Defense = def;
            Speed = spd;
            PrimaryElement = primary;
            WeaknessElement = weakness;
            IsPlayer = isPlayer;
        }

        public void TakeDamage(int amount)
        {
            CurrentHP = System.Math.Max(0, CurrentHP - amount);
        }

        public void Heal(int amount)
        {
            CurrentHP = System.Math.Min(MaxHP, CurrentHP + amount);
        }

        public bool ConsumeSP(int amount)
        {
            if (CurrentSP < amount) return false;
            CurrentSP -= amount;
            return true;
        }

        public void RecoverSP(int amount)
        {
            CurrentSP = System.Math.Min(MaxSP, CurrentSP + amount);
        }
    }
}
