using UnityEngine;
using Astra.Core;

namespace Astra
{
    public class PlayerStats : MonoBehaviour
    {
        public static PlayerStats Instance { get; private set; }

        [Header("内面の器")]
        [SerializeField] private int listening = 1;   // 傾聴
        [SerializeField] private int courage = 1;     // 胆力
        [SerializeField] private int intellect = 1;   // 知性
        [SerializeField] private int empathy = 1;     // 共感
        [SerializeField] private int expression = 1;  // 表現

        public int Listening => listening;
        public int Courage => courage;
        public int Intellect => intellect;
        public int Empathy => empathy;
        public int Expression => expression;

        public const int MaxStatLevel = 5;

        private static readonly string[] ListeningRanks = { "", "聞き役", "傾聴者", "理解者", "共鳴者", "残響の器" };
        private static readonly string[] CourageRanks = { "", "臆病", "普通", "勇敢", "豪胆", "不屈" };
        private static readonly string[] IntellectRanks = { "", "凡庸", "秀才", "博識", "天才", "叡智" };
        private static readonly string[] EmpathyRanks = { "", "鈍感", "敏感", "共感", "慈愛", "共鳴の心" };
        private static readonly string[] ExpressionRanks = { "", "無口", "朴訥", "饒舌", "詩人", "残響の声" };

        public string ListeningRank => ListeningRanks[Mathf.Clamp(listening, 1, MaxStatLevel)];
        public string CourageRank => CourageRanks[Mathf.Clamp(courage, 1, MaxStatLevel)];
        public string IntellectRank => IntellectRanks[Mathf.Clamp(intellect, 1, MaxStatLevel)];
        public string EmpathyRank => EmpathyRanks[Mathf.Clamp(empathy, 1, MaxStatLevel)];
        public string ExpressionRank => ExpressionRanks[Mathf.Clamp(expression, 1, MaxStatLevel)];

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public void AddListening(int amount)
        {
            int prev = listening;
            listening = Mathf.Clamp(listening + amount, 1, MaxStatLevel);
            if (listening != prev)
                EventBus.Publish(new StatChangedEvent("傾聴", prev, listening, ListeningRank));
        }

        public void AddCourage(int amount)
        {
            int prev = courage;
            courage = Mathf.Clamp(courage + amount, 1, MaxStatLevel);
            if (courage != prev)
                EventBus.Publish(new StatChangedEvent("胆力", prev, courage, CourageRank));
        }

        public void AddIntellect(int amount)
        {
            int prev = intellect;
            intellect = Mathf.Clamp(intellect + amount, 1, MaxStatLevel);
            if (intellect != prev)
                EventBus.Publish(new StatChangedEvent("知性", prev, intellect, IntellectRank));
        }

        public void AddEmpathy(int amount)
        {
            int prev = empathy;
            empathy = Mathf.Clamp(empathy + amount, 1, MaxStatLevel);
            if (empathy != prev)
                EventBus.Publish(new StatChangedEvent("共感", prev, empathy, EmpathyRank));
        }

        public void AddExpression(int amount)
        {
            int prev = expression;
            expression = Mathf.Clamp(expression + amount, 1, MaxStatLevel);
            if (expression != prev)
                EventBus.Publish(new StatChangedEvent("表現", prev, expression, ExpressionRank));
        }
    }

    public readonly struct StatChangedEvent
    {
        public readonly string StatName;
        public readonly int PreviousLevel;
        public readonly int NewLevel;
        public readonly string NewRankName;

        public StatChangedEvent(string statName, int previousLevel, int newLevel, string newRankName)
        {
            StatName = statName;
            PreviousLevel = previousLevel;
            NewLevel = newLevel;
            NewRankName = newRankName;
        }
    }
}
