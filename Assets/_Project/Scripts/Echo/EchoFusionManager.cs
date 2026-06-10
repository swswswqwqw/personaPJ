using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ArcanaOfHollows.Core;
using ArcanaOfHollows.Data;

namespace ArcanaOfHollows.Echo
{
    public class EchoFusionManager : MonoBehaviour
    {
        public static EchoFusionManager Instance { get; private set; }

        [SerializeField] private FusionTable fusionTable;
        [SerializeField] private EchoData[] allEchoes;

        private List<EchoData> playerEchoes = new();
        private const int MaxEchoSlots = 12;

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public bool AddEcho(EchoData echo)
        {
            if (playerEchoes.Count >= MaxEchoSlots) return false;
            if (playerEchoes.Contains(echo)) return false;

            playerEchoes.Add(echo);
            EventBus.Publish(new EchoObtainedEvent(echo));
            return true;
        }

        public void RemoveEcho(EchoData echo)
        {
            playerEchoes.Remove(echo);
        }

        public List<EchoData> GetPlayerEchoes() => new(playerEchoes);

        public EchoData GetFusionResult(EchoData echo1, EchoData echo2)
        {
            if (fusionTable == null) return null;
            return fusionTable.GetResult(echo1.arcana, echo2.arcana, allEchoes);
        }

        public FusionPreview PreviewFusion(EchoData echo1, EchoData echo2)
        {
            var result = GetFusionResult(echo1, echo2);
            if (result == null) return null;

            var inheritableSkills = new List<SkillData>();
            foreach (var ls in echo1.learnableSkills)
                if (ls.levelRequired <= echo1.level && !inheritableSkills.Contains(ls.skill))
                    inheritableSkills.Add(ls.skill);
            foreach (var ls in echo2.learnableSkills)
                if (ls.levelRequired <= echo2.level && !inheritableSkills.Contains(ls.skill))
                    inheritableSkills.Add(ls.skill);

            return new FusionPreview
            {
                Result = result,
                Material1 = echo1,
                Material2 = echo2,
                InheritableSkills = inheritableSkills,
                MaxInheritSlots = CalculateInheritSlots()
            };
        }

        public EchoData ExecuteFusion(EchoData echo1, EchoData echo2, List<SkillData> inheritedSkills)
        {
            var result = GetFusionResult(echo1, echo2);
            if (result == null) return null;

            RemoveEcho(echo1);
            RemoveEcho(echo2);
            AddEcho(result);

            EventBus.Publish(new EchoFusionEvent(echo1, echo2, result));
            return result;
        }

        private int CalculateInheritSlots()
        {
            int baseSlots = 2;
            var hsManager = Social.HeartStringManager.Instance;
            if (hsManager != null)
            {
                var hierophantRank = hsManager.GetRank("sakai_genichiro");
                if (hierophantRank >= 5) baseSlots++;
            }
            return baseSlots;
        }
    }

    public class FusionPreview
    {
        public EchoData Result;
        public EchoData Material1;
        public EchoData Material2;
        public List<SkillData> InheritableSkills;
        public int MaxInheritSlots;
    }

    public readonly struct EchoObtainedEvent
    {
        public readonly EchoData Echo;
        public EchoObtainedEvent(EchoData echo) { Echo = echo; }
    }

    public readonly struct EchoFusionEvent
    {
        public readonly EchoData Material1;
        public readonly EchoData Material2;
        public readonly EchoData Result;

        public EchoFusionEvent(EchoData mat1, EchoData mat2, EchoData result)
        {
            Material1 = mat1;
            Material2 = mat2;
            Result = result;
        }
    }
}
