using UnityEngine;
using TMPro;
using EchoesOfArcadia.Core;
using EchoesOfArcadia.Data;

namespace EchoesOfArcadia.UI
{
    public class StatRankUpUI : MonoBehaviour
    {
        [SerializeField] private CanvasGroup overlayGroup;
        [SerializeField] private TextMeshProUGUI statNameText;
        [SerializeField] private TextMeshProUGUI rankNameText;
        [SerializeField] private TextMeshProUGUI messageText;
        [SerializeField] private UnityEngine.UI.Image backgroundImage;

        private void OnEnable()
        {
            GameEventBus.Subscribe<StatRankUpEvent>(OnStatRankUp);
            SetVisible(false);
        }

        private void OnDisable()
        {
            GameEventBus.Unsubscribe<StatRankUpEvent>(OnStatRankUp);
        }

        private async void OnStatRankUp(StatRankUpEvent e)
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

            SetVisible(true);
            await System.Threading.Tasks.Task.Delay(2500);
            SetVisible(false);
        }

        private void SetVisible(bool visible)
        {
            if (overlayGroup == null) return;
            overlayGroup.alpha = visible ? 1f : 0f;
            overlayGroup.blocksRaycasts = visible;
        }
    }
}
