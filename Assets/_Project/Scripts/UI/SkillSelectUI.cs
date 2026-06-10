using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Amane.Battle;

namespace Amane.UI
{
    public sealed class SkillSelectUI : MonoBehaviour
    {
        [SerializeField] private GameObject _panel;
        [SerializeField] private Button[] _skillButtons;
        [SerializeField] private Text[] _skillLabels;
        [SerializeField] private Text[] _spCostLabels;
        [SerializeField] private Button _backButton;

        private List<Skill> _currentSkills;

        public event Action<Skill> OnSkillSelected;
        public event Action OnBack;

        private void Awake()
        {
            for (int i = 0; i < _skillButtons.Length; i++)
            {
                int idx = i;
                _skillButtons[i]?.onClick.AddListener(() => Select(idx));
            }
            _backButton?.onClick.AddListener(() => { Hide(); OnBack?.Invoke(); });
        }

        public void Show(Combatant actor)
        {
            _currentSkills = actor.Skills;
            if (_panel != null) _panel.SetActive(true);

            for (int i = 0; i < _skillButtons.Length; i++)
            {
                bool active = i < _currentSkills.Count;
                if (_skillButtons[i] != null)
                {
                    _skillButtons[i].gameObject.SetActive(active);
                    if (active)
                        _skillButtons[i].interactable = actor.Sp >= _currentSkills[i].SpCost;
                }
                if (active && i < _skillLabels.Length)
                    SetText(_skillLabels[i], _currentSkills[i].DisplayName);
                if (active && i < _spCostLabels.Length)
                    SetText(_spCostLabels[i], $"SP{_currentSkills[i].SpCost}");
            }
        }

        public void Hide()
        {
            if (_panel != null) _panel.SetActive(false);
        }

        private void Select(int index)
        {
            if (_currentSkills == null || index >= _currentSkills.Count) return;
            Hide();
            OnSkillSelected?.Invoke(_currentSkills[index]);
        }

        private static void SetText(Text t, string v) { if (t != null) t.text = v; }
    }
}
