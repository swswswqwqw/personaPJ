using UnityEngine;
using ArcanaOfHollows.Data;

namespace ArcanaOfHollows.Echo
{
    [CreateAssetMenu(fileName = "FusionTable", menuName = "ArcanaOfHollows/Fusion Table")]
    public class FusionTable : ScriptableObject
    {
        [System.Serializable]
        public class FusionRule
        {
            public Arcana arcana1;
            public Arcana arcana2;
            public Arcana resultArcana;
            public int resultMinLevel;
        }

        [SerializeField] private FusionRule[] rules;

        public EchoData GetResult(Arcana arcana1, Arcana arcana2, EchoData[] allEchoes)
        {
            var resultArcana = FindResultArcana(arcana1, arcana2);
            if (resultArcana == null) return null;

            foreach (var echo in allEchoes)
            {
                if (echo.arcana == resultArcana.Value)
                    return echo;
            }
            return null;
        }

        private Arcana? FindResultArcana(Arcana a1, Arcana a2)
        {
            foreach (var rule in rules)
            {
                if ((rule.arcana1 == a1 && rule.arcana2 == a2) ||
                    (rule.arcana1 == a2 && rule.arcana2 == a1))
                {
                    return rule.resultArcana;
                }
            }
            return null;
        }
    }
}
