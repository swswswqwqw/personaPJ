using UnityEngine;
using TMPro;
using DG.Tweening;
using EchoesOfArcadia.Core;
using EchoesOfArcadia.Data;

namespace EchoesOfArcadia.UI
{
    public class StatRankUpUI : MonoBehaviour
    {
        [SerializeField] private CanvasGroup overlayGroup;
        [SerializeField] private RectTransform overlayRect;
        [SerializeField] private TextMeshProUGUI statNameText;
        [SerializeField] private TextMeshProUGUI rankNameText;
        [SerializeField] private TextMeshProUGUI messageText;
        [SerializeField] private UnityEngine.UI.Image backgroundImage;

        private void OnEnable()
        {
            GameEventBus.Subscribe<StatRankUpEvent>(OnStatRankUp);
            UIAnimator.SetVisible(overlayGroup, false);
        }

        private void OnDisable()
        {
            GameEventBus.Unsubscribe<StatRankUpEvent>(OnStatRankUp);
        }

        private void OnStatRankUp(StatRankUpEvent e)
        {
            string statName = e.Stat switch
            {
                PersonalStat.Insight => "洞察",
                PersonalStat.Courage => "勇気",
                PersonalStat.Empathy => "共感",
                PersonalStat.Expression => "表現",
                PersonalStat.Endurance => "忍耐",
                _ => ""
            };

            string rankName = PlayerStats.Instance?.GetRankName(e.Stat) ?? "";

            if (statNameText != null) statNameText.text = statName;
            if (rankNameText != null) rankNameText.text = rankName;
            if (messageText != null) messageText.text = $"{statName}のランクが上がった！";

            AudioManager.Instance?.PlaySFX(SFXType.Stat_RankUp);
            UIAnimator.PopIn(overlayGroup, overlayRect, 0.35f);
            UIAnimator.PunchScale(overlayRect, 0.12f, 0.4f);

            DOVirtual.DelayedCall(2.5f, () => UIAnimator.PopOut(overlayGroup, overlayRect, 0.25f));
        }
    }
}
