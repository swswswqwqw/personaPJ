using System;
using AriaOfEchoes.Data;

namespace AriaOfEchoes.Battle
{
    [Serializable]
    public class BattleStats
    {
        public int MaxHP;
        public int MaxSP;
        public int Strength;
        public int Magic;
        public int Endurance;
        public int Agility;
        public int Luck;

        public BattleStats(CharacterData data)
        {
            MaxHP = data.baseHP;
            MaxSP = data.baseSP;
            Strength = data.baseStrength;
            Magic = data.baseMagic;
            Endurance = data.baseEndurance;
            Agility = data.baseAgility;
            Luck = data.baseLuck;
        }

        public BattleStats(int hp, int sp, int str, int mag, int end, int agi, int lck)
        {
            MaxHP = hp; MaxSP = sp; Strength = str; Magic = mag;
            Endurance = end; Agility = agi; Luck = lck;
        }
    }

    public class BattleUnit
    {
        public string Name { get; }
        public BattleStats Stats { get; }
        public bool IsPlayerControlled { get; }

        public int CurrentHP { get; private set; }
        public int CurrentSP { get; private set; }
        public bool IsDown { get; private set; }
        public bool IsDead => CurrentHP <= 0;

        CharacterData characterData;
        ResonanceFormData currentForm;

        public event Action<int, int> OnHPChanged;
        public event Action<int, int> OnSPChanged;
        public event Action<bool> OnDownChanged;

        public BattleUnit(CharacterData data, bool isPlayer)
        {
            Name = data.characterName;
            characterData = data;
            Stats = new BattleStats(data);
            IsPlayerControlled = isPlayer;
            CurrentHP = Stats.MaxHP;
            CurrentSP = Stats.MaxSP;
            currentForm = data.initialResonanceForm;

            if (currentForm != null)
                ApplyFormBonuses(currentForm);
        }

        public BattleUnit(string name, BattleStats stats, bool isPlayer)
        {
            Name = name;
            Stats = stats;
            IsPlayerControlled = isPlayer;
            CurrentHP = stats.MaxHP;
            CurrentSP = stats.MaxSP;
        }

        public void ApplyDamage(int amount)
        {
            int oldHP = CurrentHP;
            CurrentHP = Math.Max(0, CurrentHP - amount);
            OnHPChanged?.Invoke(oldHP, CurrentHP);
        }

        public void Heal(int amount)
        {
            int oldHP = CurrentHP;
            CurrentHP = Math.Min(Stats.MaxHP, CurrentHP + amount);
            OnHPChanged?.Invoke(oldHP, CurrentHP);
        }

        public bool ConsumeSP(int amount)
        {
            if (CurrentSP < amount) return false;
            int oldSP = CurrentSP;
            CurrentSP -= amount;
            OnSPChanged?.Invoke(oldSP, CurrentSP);
            return true;
        }

        public void SetDown(bool down)
        {
            IsDown = down;
            OnDownChanged?.Invoke(down);
        }

        public AffinityType GetAffinity(ElementType element)
        {
            if (currentForm != null)
            {
                var formAffinity = currentForm.GetAffinity(element);
                if (formAffinity != AffinityType.Normal)
                    return formAffinity;
            }

            return characterData?.GetAffinity(element) ?? AffinityType.Normal;
        }

        public void SwitchResonanceForm(ResonanceFormData newForm)
        {
            if (currentForm != null)
                RemoveFormBonuses(currentForm);

            currentForm = newForm;

            if (currentForm != null)
                ApplyFormBonuses(currentForm);
        }

        void ApplyFormBonuses(ResonanceFormData form)
        {
            Stats.MaxHP += form.hpBonus;
            Stats.MaxSP += form.spBonus;
            Stats.Strength += form.strengthBonus;
            Stats.Magic += form.magicBonus;
            Stats.Endurance += form.enduranceBonus;
            Stats.Agility += form.agilityBonus;
            Stats.Luck += form.luckBonus;
        }

        void RemoveFormBonuses(ResonanceFormData form)
        {
            Stats.MaxHP -= form.hpBonus;
            Stats.MaxSP -= form.spBonus;
            Stats.Strength -= form.strengthBonus;
            Stats.Magic -= form.magicBonus;
            Stats.Endurance -= form.enduranceBonus;
            Stats.Agility -= form.agilityBonus;
            Stats.Luck -= form.luckBonus;
        }
    }
}
