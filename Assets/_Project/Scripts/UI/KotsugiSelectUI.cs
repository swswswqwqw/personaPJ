using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Amane.Battle;

namespace Amane.UI
{
    public sealed class KotsugiSelectUI : MonoBehaviour
    {
        [SerializeField] private GameObject _panel;
        [SerializeField] private Button[] _memberButtons;
        [SerializeField] private Text[] _memberLabels;
        [SerializeField] private Button _backButton;

        private List<Combatant> _candidates;

        public event Action<Combatant> OnMemberSelected;
        public event Action OnBack;

        private void Awake()
        {
            for (int i = 0; i < _memberButtons.Length; i++)
            {
                int idx = i;
                _memberButtons[i]?.onClick.AddListener(() => Select(idx));
            }
            _backButton?.onClick.AddListener(() => { Hide(); OnBack?.Invoke(); });
        }

        public void Show(List<Combatant> party, Combatant actor)
        {
            _candidates = party.Where(p => p.IsAlive && p != actor).ToList();
            if (_panel != null) _panel.SetActive(true);

            for (int i = 0; i < _memberButtons.Length; i++)
            {
                bool active = i < _candidates.Count;
                if (_memberButtons[i] != null) _memberButtons[i].gameObject.SetActive(active);
                if (active && i < _memberLabels.Length)
                    SetText(_memberLabels[i], _candidates[i].DisplayName);
            }
        }

        public void Hide()
        {
            if (_panel != null) _panel.SetActive(false);
        }

        private void Select(int index)
        {
            if (_candidates == null || index >= _candidates.Count) return;
            Hide();
            OnMemberSelected?.Invoke(_candidates[index]);
        }

        private static void SetText(Text t, string v) { if (t != null) t.text = v; }
    }
}
