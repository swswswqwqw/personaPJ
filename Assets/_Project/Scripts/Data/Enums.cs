namespace AstralEchoes.Data
{
    public enum GameState
    {
        Title,
        Field,
        Battle,
        Dialogue,
        Menu,
        Astral,
        Loading,
        Cutscene
    }

    public enum TimeOfDay
    {
        Morning,
        Class,
        AfterSchool,
        Evening,
        LateNight
    }

    public enum Attribute
    {
        Slash,
        Strike,
        Pierce,
        Fire,
        Ice,
        Thunder,
        Wind,
        Light,
        Dark,
        Resonance,
        Almighty
    }

    public enum ResistanceLevel
    {
        Weak,
        Normal,
        Resist,
        Null,
        Absorb,
        Reflect
    }

    public enum Arcana
    {
        Fool,
        Magician,
        Priestess,
        Empress,
        Emperor,
        Hierophant,
        Lovers,
        Chariot,
        Justice,
        Hermit,
        Fortune,
        Strength,
        HangedMan,
        Death,
        Temperance,
        Devil,
        Tower,
        Star,
        Moon,
        Sun,
        Judgement,
        World
    }

    public enum InnerFrequency
    {
        Empathy,
        Resolve,
        Insight,
        Allure,
        Harmony
    }

    public enum StatusAilment
    {
        None,
        Burn,
        Freeze,
        Shock,
        Down,
        Dizzy,
        Fear,
        Despair,
        Rage
    }

    public enum AttunementRank
    {
        Rank0 = 0,
        Rank1 = 1,
        Rank2 = 2,
        Rank3 = 3,
        Rank4 = 4,
        Rank5 = 5,
        Rank6 = 6,
        Rank7 = 7,
        Rank8 = 8,
        Rank9 = 9,
        Rank10 = 10
    }

    public enum BattleAction
    {
        Attack,
        Skill,
        Guard,
        Item,
        Escape,
        EchoShift,
        FullResonance
    }

    public enum Season
    {
        Spring,
        Summer,
        Autumn,
        Winter
    }
}
