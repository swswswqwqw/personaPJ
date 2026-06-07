using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using EchoesOfArcadia.Core;
using EchoesOfArcadia.Data;
using EchoesOfArcadia.Social;

namespace EchoesOfArcadia.UI
{
    public class BondUIController : MonoBehaviour
    {
        [Header("Bond Map")]
        [SerializeField] private CanvasGroup bondMapGroup;
        [SerializeField] private RectTransform centerPoint;
        [SerializeField] private BondCharacterNode[] characterNodes;
        [SerializeField] private LineRenderer[] connectionLines;

        [Header("Rank Up Overlay")]
        [SerializeField] private CanvasGroup rankUpOverlay;
        [SerializeField] private TextMeshProUGUI rankUpCharacterName;
        [SerializeField] private TextMeshProUGUI rankUpArcanaText;
        [SerializeField] private TextMeshProUGUI rankUpNumberText;
        [SerializeField] private TextMeshProUGUI rankUpMessageText;
        [SerializeField] private Image rankUpBackground;

        [Header("Detail Panel")]
        [SerializeField] private CanvasGroup detailGroup;
        [SerializeField] private TextMeshProUGUI detailNameText;
        [SerializeField] private TextMeshProUGUI detailArcanaText;
        [SerializeField] private TextMeshProUGUI detailRankText;
        [SerializeField] private TextMeshProUGUI detailBioText;
        [SerializeField] private Slider detailProgressBar;

        [Header("Animation")]
        [SerializeField] private float pulseSpeed = 1.5f;
        [SerializeField] private float nodeOrbitSpeed = 0.1f;

        private float animTime;

        private void OnEnable()
        {
            GameEventBus.Subscribe<BondRankUpEvent>(OnBondRankUp);
        }

        private void OnDisable()
        {
            GameEventBus.Unsubscribe<BondRankUpEvent>(OnBondRankUp);
        }

        private void Update()
        {
            if (bondMapGroup == null || bondMapGroup.alpha < 0.5f) return;

            animTime += Time.deltaTime;
            AnimateNodes();
        }

        public void OpenBondMap()
        {
            RefreshBondMap();
            UIAnimator.FadeIn(bondMapGroup, 0.3f);
            UIAnimator.SetVisible(detailGroup, false);
            AudioManager.Instance?.PlaySFX(SFXType.UI_Open);
        }

        public void CloseBondMap()
        {
            UIAnimator.FadeOut(bondMapGroup, 0.2f);
            UIAnimator.FadeOut(detailGroup, 0.15f);
        }

        public void ShowCharacterDetail(Arcana arcana)
        {
            if (SocialLinkManager.Instance == null) return;

            var bond = SocialLinkManager.Instance.GetBondProgress(arcana);
            if (bond == null) return;

            if (detailNameText != null) detailNameText.text = bond.character.characterName;
            if (detailArcanaText != null) detailArcanaText.text = GetArcanaName(arcana);
            if (detailRankText != null) detailRankText.text = $"Rank {bond.currentRank}";
            if (detailBioText != null) detailBioText.text = bond.character.bondDescription;
            if (detailProgressBar != null)
            {
                detailProgressBar.maxValue = bond.PointsToNextRank;
                detailProgressBar.value = bond.currentPoints;
            }

            UIAnimator.SlideInFromRight(detailGroup, detailGroup.GetComponent<RectTransform>(), 0.25f);
        }

        private void RefreshBondMap()
        {
            if (SocialLinkManager.Instance == null || characterNodes == null) return;

            foreach (var node in characterNodes)
            {
                if (node == null) continue;
                int rank = SocialLinkManager.Instance.GetBondRank(node.Arcana);
                node.SetRank(rank);
            }
        }

        private void OnBondRankUp(BondRankUpEvent e)
        {
            if (rankUpOverlay == null) return;

            var bond = SocialLinkManager.Instance?.GetBondProgress(e.Arcana);
            if (bond == null) return;

            if (rankUpCharacterName != null) rankUpCharacterName.text = bond.character.characterName;
            if (rankUpArcanaText != null) rankUpArcanaText.text = GetArcanaName(e.Arcana);
            if (rankUpNumberText != null) rankUpNumberText.text = $"RANK {e.NewRank}";
            if (rankUpMessageText != null) rankUpMessageText.text = "絆の調べが深まった……";

            AudioManager.Instance?.PlaySFX(SFXType.Bond_RankUp);

            var rect = rankUpOverlay.GetComponent<RectTransform>();
            UIAnimator.PopIn(rankUpOverlay, rect, 0.4f);
            UIAnimator.PunchScale(rect, 0.1f, 0.5f);

            DOVirtual.DelayedCall(3f, () => UIAnimator.PopOut(rankUpOverlay, rect, 0.3f));
        }

        private void AnimateNodes()
        {
            if (characterNodes == null) return;

            for (int i = 0; i < characterNodes.Length; i++)
            {
                if (characterNodes[i] == null) continue;

                float pulse = 1f + Mathf.Sin(animTime * pulseSpeed + i * 0.7f) * 0.05f;
                characterNodes[i].transform.localScale = Vector3.one * pulse;
            }
        }

        private static string GetArcanaName(Arcana arcana) => arcana switch
        {
            Arcana.Fool => "0 愚者",
            Arcana.Magician => "I 魔術師",
            Arcana.Priestess => "II 女教皇",
            Arcana.Empress => "III 女帝",
            Arcana.Emperor => "IV 皇帝",
            Arcana.Hierophant => "V 法王",
            Arcana.Lovers => "VI 恋愛",
            Arcana.Chariot => "VII 戦車",
            Arcana.Justice => "VIII 正義",
            Arcana.Hermit => "IX 隠者",
            Arcana.Fortune => "X 運命",
            Arcana.Strength => "XI 力",
            Arcana.HangedMan => "XII 吊された男",
            Arcana.Death => "XIII 死神",
            Arcana.Temperance => "XIV 節制",
            Arcana.Devil => "XV 悪魔",
            Arcana.Tower => "XVI 塔",
            Arcana.Star => "XVII 星",
            Arcana.Moon => "XVIII 月",
            Arcana.Sun => "XIX 太陽",
            Arcana.Judgement => "XX 審判",
            Arcana.World => "XXI 世界",
            _ => ""
        };

    }

    public class BondCharacterNode : MonoBehaviour
    {
        [SerializeField] private Arcana arcana;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI rankText;
        [SerializeField] private Image nodeImage;
        [SerializeField] private Image connectionGlow;

        public Arcana Arcana => arcana;

        public void SetRank(int rank)
        {
            if (rankText != null) rankText.text = rank > 0 ? $"Rank {rank}" : "???";

            if (nodeImage != null)
            {
                float brightness = rank > 0 ? 0.5f + (rank / 10f) * 0.5f : 0.2f;
                nodeImage.color = new Color(UIColors.Cyan.r, UIColors.Cyan.g, UIColors.Cyan.b, brightness);
            }

            if (connectionGlow != null)
                connectionGlow.gameObject.SetActive(rank >= 5);
        }
    }
}
