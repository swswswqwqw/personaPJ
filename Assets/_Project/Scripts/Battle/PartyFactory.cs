using System.Collections.Generic;

namespace Amane.Battle
{
    public static class PartyFactory
    {
        public static Combatant CreateProtagonist()
        {
            return new Combatant("yomi", "天野 詠", true,
                maxHp: 180, maxSp: 80, atk: 22, def: 18,
                matk: 25, mdef: 20, agi: 20,
                AffinityTable.AllNormal(),
                new List<Skill>
                {
                    new("agi", "アギ", Element.Fire, 35, 4, TargetType.SingleEnemy),
                    new("zio", "ジオ", Element.Thunder, 35, 4, TargetType.SingleEnemy),
                    new("dia", "ディア", Element.Light, 30, 4, TargetType.SingleAlly),
                });
        }

        public static Combatant CreateAkari()
        {
            return new Combatant("akari", "望月 灯里", true,
                maxHp: 150, maxSp: 90, atk: 18, def: 16,
                matk: 28, mdef: 24, agi: 18,
                AffinityTable.Build(weak: new[] { Element.Dark }, resist: new[] { Element.Light }),
                new List<Skill>
                {
                    new("kouha", "コウハ", Element.Light, 35, 4, TargetType.SingleEnemy),
                    new("dia", "ディア", Element.Light, 30, 4, TargetType.SingleAlly),
                    new("media", "メディア", Element.Light, 20, 8, TargetType.AllAllies),
                });
        }

        public static Combatant CreateRitsu()
        {
            return new Combatant("ritsu", "久遠 律", true,
                maxHp: 130, maxSp: 100, atk: 14, def: 14,
                matk: 32, mdef: 26, agi: 22,
                AffinityTable.Build(weak: new[] { Element.Wind }, resist: new[] { Element.Thunder }),
                new List<Skill>
                {
                    new("zionga", "ジオンガ", Element.Thunder, 55, 8, TargetType.SingleEnemy),
                    new("eiha", "エイハ", Element.Dark, 35, 4, TargetType.SingleEnemy),
                    new("analyze", "解析", Element.Almighty, 0, 2, TargetType.SingleEnemy),
                });
        }

        public static Combatant CreateRen()
        {
            return new Combatant("ren", "朝霧 蓮", true,
                maxHp: 220, maxSp: 50, atk: 30, def: 28,
                matk: 12, mdef: 16, agi: 15,
                AffinityTable.Build(weak: new[] { Element.Ice }, resist: new[] { Element.Fire, Element.Strike }),
                new List<Skill>
                {
                    new("assault", "猛突進", Element.Strike, 50, 6, TargetType.SingleEnemy, isPhysical: true, critRate: 0.15f),
                    new("agilao", "アギラオ", Element.Fire, 55, 8, TargetType.SingleEnemy),
                    new("taunt", "挑発", Element.Almighty, 0, 4, TargetType.Self),
                });
        }

        public static List<Combatant> CreateDefaultParty()
        {
            return new List<Combatant>
            {
                CreateProtagonist(),
                CreateAkari(),
                CreateRitsu(),
                CreateRen()
            };
        }
    }

    public static class EnemyFactory
    {
        public static Combatant CreateOri(string id, string name, Element weakness)
        {
            return new Combatant(id, name, false,
                maxHp: 100, maxSp: 30, atk: 16, def: 14,
                matk: 18, mdef: 14, agi: 12,
                AffinityTable.WithWeak(weakness),
                new List<Skill>
                {
                    new("bash", "叩きつけ", Element.Strike, 30, 0, TargetType.SingleEnemy, isPhysical: true),
                });
        }

        public static Combatant CreateSilentOri()
        {
            return new Combatant("silent_ori", "沈黙の澱", false,
                maxHp: 160, maxSp: 40, atk: 20, def: 18,
                matk: 22, mdef: 18, agi: 14,
                AffinityTable.Build(weak: new[] { Element.Light }, resist: new[] { Element.Dark }),
                new List<Skill>
                {
                    new("mudo", "ムド", Element.Dark, 40, 6, TargetType.SingleEnemy),
                    new("silence_pressure", "後悔の重圧", Element.Dark, 30, 4, TargetType.SingleEnemy),
                });
        }

        public static List<Combatant> CreateTutorialEncounter()
        {
            return new List<Combatant>
            {
                CreateOri("ori_a", "澱・怨嗟", Element.Fire),
                CreateOri("ori_b", "澱・沈黙", Element.Thunder),
                CreateSilentOri()
            };
        }
    }
}
