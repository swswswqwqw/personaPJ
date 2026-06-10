using UnityEngine;
using UnityEngine.UI;
using Amane.Core;
using Amane.Stat;
using Amane.Social;

namespace Amane.UI
{
    public sealed class StatusUI : MonoBehaviour
    {
        [Header("Inner Stats")]
        [SerializeField] private Text _courageText;
        [SerializeField] private Text _intellectText;
        [SerializeField] private Text _empathyText;
        [SerializeField] private Text _expressionText;
        [SerializeField] private Text _composureText;

        [Header("Bonds")]
        [SerializeField] private Text _bondsText;

        private static readonly string[] StatLabels = { "度胸", "知性", "慈しみ", "ことのは", "静けさ" };
        private static readonly string[] RankNames = { "—", "★", "★★", "★★★", "★★★★", "★★★★★" };

        public void Toggle()
        {
            bool active = gameObject.activeSelf;
            gameObject.SetActive(!active);
            if (!active) Refresh();
        }

        public void Refresh()
        {
            var gm = GameManager.Instance;
            if (gm == null) return;

            SetStatText(_courageText, InnerStat.Courage, gm.Stats);
            SetStatText(_intellectText, InnerStat.Intellect, gm.Stats);
            SetStatText(_empathyText, InnerStat.Empathy, gm.Stats);
            SetStatText(_expressionText, InnerStat.Expression, gm.Stats);
            SetStatText(_composureText, InnerStat.Composure, gm.Stats);

            RefreshBonds(gm.Bonds);
        }

        private void SetStatText(Text text, InnerStat stat, InnerStatSet stats)
        {
            if (text == null) return;
            int rank = stats.GetRank(stat);
            int idx = (int)stat;
            string label = idx < StatLabels.Length ? StatLabels[idx] : stat.ToString();
            text.text = $"{label}: {RankNames[rank]}";
        }

        private void RefreshBonds(BondManager bonds)
        {
            if (_bondsText == null || bonds == null) return;
            var sb = new System.Text.StringBuilder();
            foreach (var bond in bonds.All)
            {
                sb.AppendLine($"{bond.DisplayName} Rank {bond.Rank}/{Bond.MaxRank}");
            }
            _bondsText.text = sb.ToString();
        }
    }
}
