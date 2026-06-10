using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Amane.Battle;

namespace Amane.UI
{
    public sealed class TargetSelectUI : MonoBehaviour
    {
        [SerializeField] private GameObject _panel;
        [SerializeField] private Button[] _targetButtons;
        [SerializeField] private Text[] _targetLabels;
        [SerializeField] private Button _backButton;

        private List<Combatant> _targets;

        public event Action<Combatant> OnTargetSelected;
        public event Action OnBack;

        private void Awake()
        {
            for (int i = 0; i < _targetButtons.Length; i++)
            {
                int idx = i;
                _targetButtons[i]?.onClick.AddListener(() => Select(idx));
            }
            _backButton?.onClick.AddListener(() => { Hide(); OnBack?.Invoke(); });
        }

        public void Show(List<Combatant> targets)
        {
            _targets = targets;
            if (_panel != null) _panel.SetActive(true);

            for (int i = 0; i < _targetButtons.Length; i++)
            {
                bool active = i < _targets.Count && _targets[i].IsAlive;
                if (_targetButtons[i] != null)
                {
                    _targetButtons[i].gameObject.SetActive(active);
                }
                if (active && i < _targetLabels.Length)
                {
                    var t = _targets[i];
                    string status = t.IsDown ? " [DOWN]" : "";
                    SetText(_targetLabels[i], $"{t.DisplayName} HP{t.Hp}/{t.MaxHp}{status}");
                }
            }
        }

        public void Hide()
        {
            if (_panel != null) _panel.SetActive(false);
        }

        private void Select(int index)
        {
            if (_targets == null || index >= _targets.Count) return;
            Hide();
            OnTargetSelected?.Invoke(_targets[index]);
        }

        private static void SetText(Text t, string v) { if (t != null) t.text = v; }
    }
}
