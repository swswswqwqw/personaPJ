using System;
using UnityEngine;
using UnityEngine.UI;
using Amane.Dialogue;
using Amane.UI.Effects;
using Amane.Core;

namespace Amane.UI
{
    public sealed class DialogueUI : MonoBehaviour
    {
        [SerializeField] private GameObject _panel;
        [SerializeField] private Text _speakerText;
        [SerializeField] private Text _bodyText;
        [SerializeField] private GameObject _choicePanel;
        [SerializeField] private Button[] _choiceButtons;
        [SerializeField] private Text[] _choiceTexts;
        [SerializeField] private TypewriterText _typewriter;

        private DialogueRunner _runner;
        private Action<int> _onChoiceSelected;

        public void Bind(DialogueRunner runner)
        {
            if (_runner != null)
            {
                _runner.OnLineShown -= ShowLine;
                _runner.OnChoicePresented -= ShowChoices;
                _runner.OnDialogueEnd -= OnEnd;
            }

            _runner = runner;
            _runner.OnLineShown += ShowLine;
            _runner.OnChoicePresented += ShowChoices;
            _runner.OnDialogueEnd += OnEnd;

            for (int i = 0; i < _choiceButtons.Length; i++)
            {
                int idx = i;
                _choiceButtons[i]?.onClick.AddListener(() => SelectChoice(idx));
            }
        }

        public void Show()
        {
            if (_panel != null) _panel.SetActive(true);
            HideChoices();
        }

        public void Hide()
        {
            if (_panel != null) _panel.SetActive(false);
            HideChoices();
        }

        public void OnAdvanceInput()
        {
            if (_runner == null || !_runner.IsRunning) return;
            if (_runner.IsWaitingForChoice) return;

            // タイプライター表示中ならスキップ、完了後なら次の行へ
            if (_typewriter != null && _typewriter.IsRevealing)
            {
                _typewriter.SkipToEnd();
                return;
            }

            _runner.Advance();
        }

        private void ShowLine(DialogueLine line)
        {
            SetText(_speakerText, SpeakerName(line.speakerId));

            // TypewriterTextが使えればタイプライター表示
            if (_typewriter != null)
            {
                _typewriter.Show(line.text, line.preSilence);
            }
            else
            {
                SetText(_bodyText, line.text);
            }
        }

        private void ShowChoices(DialogueChoice choice)
        {
            if (_choicePanel != null) _choicePanel.SetActive(true);
            for (int i = 0; i < _choiceButtons.Length; i++)
            {
                bool active = i < choice.options.Count;
                if (_choiceButtons[i] != null) _choiceButtons[i].gameObject.SetActive(active);
                if (active && i < _choiceTexts.Length && _choiceTexts[i] != null)
                    _choiceTexts[i].text = choice.options[i].text;
            }
        }

        private void SelectChoice(int index)
        {
            HideChoices();
            if (_runner == null) return;

            // bondBonus適用: SelectOption前にbondIdを取得（実行後はCurrentDataがnullになる場合がある）
            string bondId = _runner.CurrentData?.bondId;
            int bondBonus = _runner.SelectOption(index);

            if (bondBonus > 0 && !string.IsNullOrEmpty(bondId))
                GameManager.Instance?.Bonds.GivePoints(bondId, bondBonus);
        }

        private void HideChoices()
        {
            if (_choicePanel != null) _choicePanel.SetActive(false);
        }

        private void OnEnd(DialogueData data) => Hide();

        private static string SpeakerName(string id) => id switch
        {
            "yomi" => "天野 詠",
            "akari" => "望月 灯里",
            "ritsu" => "久遠 律",
            "ren" => "朝霧 蓮",
            "nagisa" => "鵠沼 渚",
            "yakumo" => "八雲",
            "kano" => "七尾 佳乃",
            "manabe" => "真鍋 刑事",
            "suzu" => "雛森 すず",
            "narrator" => "",
            _ => id
        };

        private static void SetText(Text t, string v) { if (t != null) t.text = v; }
    }
}
