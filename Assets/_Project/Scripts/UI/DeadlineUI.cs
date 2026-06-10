using UnityEngine;
using UnityEngine.UI;
using Amane.Core;

namespace Amane.UI
{
    public sealed class DeadlineUI : MonoBehaviour
    {
        [SerializeField] private GameObject _warningPanel;
        [SerializeField] private Text _warningText;

        private System.IDisposable _sub;

        private void OnEnable()
        {
            var gm = GameManager.Instance;
            if (gm == null) return;
            _sub = gm.Events.Subscribe<DeadlineApproachingEvent>(OnDeadlineApproaching);
            Hide();
        }

        private void OnDisable() => _sub?.Dispose();

        private void OnDeadlineApproaching(DeadlineApproachingEvent e)
        {
            if (_warningPanel != null) _warningPanel.SetActive(true);
            string urgency = e.DaysLeft switch
            {
                0 => "【今日が期限！】",
                1 => "【あと1日……】",
                _ => $"【あと{e.DaysLeft}日】"
            };
            SetText(_warningText, $"{urgency} {e.CaseId}");
        }

        public void Hide()
        {
            if (_warningPanel != null) _warningPanel.SetActive(false);
        }

        private static void SetText(Text t, string v) { if (t != null) t.text = v; }
    }
}
