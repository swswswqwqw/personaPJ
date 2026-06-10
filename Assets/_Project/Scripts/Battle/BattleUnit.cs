using System;
using UnityEngine;
using EchoesOfArcadia.Data;

namespace EchoesOfArcadia.Battle
{
    [Serializable]
    public class BattleUnit
    {
        public CharacterData CharacterData { get; private set; }

        public string Name => CharacterData?.characterName ?? "Unknown";
        public int MaxHP { get; private set; }
        public int CurrentHP { get; private set; }
        public int MaxSP { get; private set; }
        public int CurrentSP { get; private set; }
        public int Strength { get; private set; }
        public int Magic { get; private set; }
        public int Endurance { get; private set; }
        public int Agility { get; private set; }
        public int Luck { get; private set; }
        public int Level { get; private set; }

        public bool IsAlive => CurrentHP > 0;
        public bool IsDown { get; set; }
        public bool IsPlayerControlled { get; private set; }

        public EchoFormData CurrentEchoForm { get; private set; }

        public event Action<int, int> OnHPChanged;
        public event Action<int, int> OnSPChanged;
        public event Action OnDowned;
        public event Action OnDefeated;

        public BattleUnit(CharacterData data, int level, bool isPlayer)
        {
            CharacterData = data;
            Level = level;
            IsPlayerControlled = isPlayer;

            MaxHP = data.baseHP + level * 8;
            CurrentHP = MaxHP;
            MaxSP = data.baseSP + level * 3;
            CurrentSP = MaxSP;
            Strength = data.baseStrength + level * 2;
            Magic = data.baseMagic + level * 2;
            Endurance = data.baseEndurance + level * 2;
            Agility = data.baseAgility + level * 2;
            Luck = data.baseLuck + level;
        }

        public void TakeDamage(int damage)
        {
            int prev = CurrentHP;
            CurrentHP = Mathf.Max(0, CurrentHP - damage);
            OnHPChanged?.Invoke(prev, CurrentHP);

            if (!IsAlive)
                OnDefeated?.Invoke();
        }

        public void Heal(int amount)
        {
            int prev = CurrentHP;
            CurrentHP = Mathf.Min(MaxHP, CurrentHP + amount);
            OnHPChanged?.Invoke(prev, CurrentHP);
        }

        public bool ConsumeSP(int amount)
        {
            if (CurrentSP < amount) return false;
            int prev = CurrentSP;
            CurrentSP -= amount;
            OnSPChanged?.Invoke(prev, CurrentSP);
            return true;
        }

        public void RestoreSP(int amount)
        {
            int prev = CurrentSP;
            CurrentSP = Mathf.Min(MaxSP, CurrentSP + amount);
            OnSPChanged?.Invoke(prev, CurrentSP);
        }

        public void SetEchoForm(EchoFormData form)
        {
            CurrentEchoForm = form;
            if (form == null) return;

            Strength = CharacterData.baseStrength + form.strength + Level * 2;
            Magic = CharacterData.baseMagic + form.magic + Level * 2;
            Endurance = CharacterData.baseEndurance + form.endurance + Level * 2;
            Agility = CharacterData.baseAgility + form.agility + Level * 2;
            Luck = CharacterData.baseLuck + form.luck + Level;
        }

        public void StandUp()
        {
            IsDown = false;
        }
    }
}
