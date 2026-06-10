using System;
using UnityEngine;
using UnityEngine.UI;
using Amane.Battle;

namespace Amane.UI
{
    public sealed class BattleResultUI : MonoBehaviour
    {
        [SerializeField] private GameObject _panel;
        [SerializeField] private Text _resultTitle;
        [SerializeField] private Text _resultDetail;
        [SerializeField] private Button _continueButton;

        public event Action OnContinue;

        private void Awake()
        {
            _continueButton?.onClick.AddListener(() =>
            {
                Hide();
                OnContinue?.Invoke();
            });
            Hide();
        }

        public void ShowVictory(int expGained)
        {
            if (_panel != null) _panel.SetActive(true);
            SetText(_resultTitle, "――澱は静まった。");
            SetText(_resultDetail, $"経験値 +{expGained}\n\n言葉が、少しだけ戻った。");
        }

        public void ShowDefeat()
        {
            if (_panel != null) _panel.SetActive(true);
            SetText(_resultTitle, "――意識が、遠のいていく……");
            SetText(_resultDetail, "言葉は、まだ届かなかった。");
        }

        public void Hide()
        {
            if (_panel != null) _panel.SetActive(false);
        }

        private static void SetText(Text t, string v) { if (t != null) t.text = v; }
    }
}
