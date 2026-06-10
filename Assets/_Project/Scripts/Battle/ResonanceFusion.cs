using System;
using System.Collections.Generic;
using UnityEngine;
using AriaOfEchoes.Data;
using AriaOfEchoes.Core;

namespace AriaOfEchoes.Battle
{
    [CreateAssetMenu(fileName = "FusionRecipe", menuName = "AriaOfEchoes/FusionRecipe")]
    public class FusionRecipe : ScriptableObject
    {
        public ArcanaType arcana1;
        public ArcanaType arcana2;
        public ArcanaType resultArcana;
    }

    [CreateAssetMenu(fileName = "FusionTable", menuName = "AriaOfEchoes/FusionTable")]
    public class FusionTable : ScriptableObject
    {
        public List<FusionRecipe> recipes = new();
        public List<SpecialFusion> specialFusions = new();

        public ArcanaType? GetResultArcana(ArcanaType a, ArcanaType b)
        {
            var recipe = recipes.Find(r =>
                (r.arcana1 == a && r.arcana2 == b) ||
                (r.arcana1 == b && r.arcana2 == a));

            return recipe?.resultArcana;
        }
    }

    [Serializable]
    public class SpecialFusion
    {
        public string fusionName;
        public List<ResonanceFormData> materials;
        public ResonanceFormData result;
        public int requiredBondRank;
        public string requiredCharacterName;
    }

    public class ResonanceFusion : MonoBehaviour
    {
        public static ResonanceFusion Instance { get; private set; }

        [SerializeField] FusionTable fusionTable;
        [SerializeField] List<ResonanceFormData> allForms;

        List<ResonanceFormData> playerForms = new();

        public IReadOnlyList<ResonanceFormData> PlayerForms => playerForms;

        void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public void AddForm(ResonanceFormData form)
        {
            if (!playerForms.Contains(form))
            {
                playerForms.Add(form);
                EventBus.Publish(new FormObtainedEvent(form));
            }
        }

        public void RemoveForm(ResonanceFormData form)
        {
            playerForms.Remove(form);
        }

        public ResonanceFormData Fuse(ResonanceFormData form1, ResonanceFormData form2)
        {
            var resultArcana = fusionTable.GetResultArcana(form1.arcana, form2.arcana);
            if (resultArcana == null) return null;

            var resultForm = FindFormByArcanaAndLevel(
                resultArcana.Value,
                (form1.level + form2.level) / 2 + 1);

            if (resultForm == null) return null;

            RemoveForm(form1);
            RemoveForm(form2);
            AddForm(resultForm);

            EventBus.Publish(new FusionCompleteEvent(form1, form2, resultForm));
            return resultForm;
        }

        public List<SpecialFusion> GetAvailableSpecialFusions()
        {
            var available = new List<SpecialFusion>();
            foreach (var special in fusionTable.specialFusions)
            {
                bool hasAllMaterials = true;
                foreach (var material in special.materials)
                {
                    if (!playerForms.Contains(material))
                    {
                        hasAllMaterials = false;
                        break;
                    }
                }

                if (hasAllMaterials)
                    available.Add(special);
            }
            return available;
        }

        ResonanceFormData FindFormByArcanaAndLevel(ArcanaType arcana, int targetLevel)
        {
            ResonanceFormData best = null;
            int bestDiff = int.MaxValue;

            foreach (var form in allForms)
            {
                if (form.arcana != arcana) continue;
                int diff = Mathf.Abs(form.level - targetLevel);
                if (diff < bestDiff)
                {
                    bestDiff = diff;
                    best = form;
                }
            }
            return best;
        }
    }

    public struct FormObtainedEvent
    {
        public ResonanceFormData Form;
        public FormObtainedEvent(ResonanceFormData f) { Form = f; }
    }

    public struct FusionCompleteEvent
    {
        public ResonanceFormData Material1;
        public ResonanceFormData Material2;
        public ResonanceFormData Result;
        public FusionCompleteEvent(ResonanceFormData m1, ResonanceFormData m2, ResonanceFormData r)
        { Material1 = m1; Material2 = m2; Result = r; }
    }
}
