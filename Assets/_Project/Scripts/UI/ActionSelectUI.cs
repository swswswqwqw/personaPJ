using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Amane.Core;
using Amane.Time;

namespace Amane.UI
{
    public enum FieldAction
    {
        Study,
        Socialize,
        PartTimeJob,
        Meditate,
        Dive,
        GoHome
    }

    public sealed class ActionSelectUI : MonoBehaviour
    {
        [SerializeField] private GameObject _panel;
        [SerializeField] private Button _studyButton;
        [SerializeField] private Button _socializeButton;
        [SerializeField] private Button _jobButton;
        [SerializeField] private Button _meditateButton;
        [SerializeField] private Button _diveButton;
        [SerializeField] private Button _goHomeButton;

        public event Action<FieldAction> OnActionSelected;

        private void Awake()
        {
            Bind(_studyButton, FieldAction.Study);
            Bind(_socializeButton, FieldAction.Socialize);
            Bind(_jobButton, FieldAction.PartTimeJob);
            Bind(_meditateButton, FieldAction.Meditate);
            Bind(_diveButton, FieldAction.Dive);
            Bind(_goHomeButton, FieldAction.GoHome);
        }

        public void Show(TimeSlot slot, int ap)
        {
            if (_panel != null) _panel.SetActive(true);

            bool canAct = ap > 0 && (slot == TimeSlot.AfterSchool || slot == TimeSlot.Evening);
            bool canDive = ap > 0 && slot != TimeSlot.Morning && slot != TimeSlot.Class;

            SetInteractable(_studyButton, canAct);
            SetInteractable(_socializeButton, canAct);
            SetInteractable(_jobButton, canAct);
            SetInteractable(_meditateButton, canAct);
            SetInteractable(_diveButton, canDive);
            SetInteractable(_goHomeButton, true);
        }

        public void Hide()
        {
            if (_panel != null) _panel.SetActive(false);
        }

        private void Bind(Button btn, FieldAction action)
        {
            btn?.onClick.AddListener(() => OnSelect(action));
        }

        private void OnSelect(FieldAction action)
        {
            Hide();
            OnActionSelected?.Invoke(action);
        }

        private static void SetInteractable(Button b, bool v) { if (b != null) b.interactable = v; }
    }
}
