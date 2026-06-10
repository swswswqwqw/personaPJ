using System;
using UnityEngine;
using UnityEngine.UI;
using Amane.Core;

namespace Amane.UI
{
    public sealed class GameOverUI : MonoBehaviour
    {
        [SerializeField] private GameObject _panel;
        [SerializeField] private Text _titleText;
        [SerializeField] private Text _detailText;
        [SerializeField] private Button _titleButton;

        public void ShowGameOver(string reason)
        {
            if (_panel != null) _panel.SetActive(true);
            SetText(_titleText, "――意識が、遠のいていく……");
            SetText(_detailText, reason);
        }

        public void ShowEnding(string endingType, string message)
        {
            if (_panel != null) _panel.SetActive(true);
            SetText(_titleText, endingType);
            SetText(_detailText, message);
        }

        public void Hide()
        {
            if (_panel != null) _panel.SetActive(false);
        }

        private void Awake()
        {
            _titleButton?.onClick.AddListener(() =>
            {
                Hide();
                GameManager.Instance?.GoToTitle();
            });
            Hide();
        }

        private static void SetText(Text t, string v) { if (t != null) t.text = v; }
    }
}
