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
        GoHome,
        LunchChat,    // 昼休み: 校内キャラとの短会話
        LunchLibrary, // 昼休み: 図書室で読書
        LunchSkip,    // 昼休み: 次の時間まで待機
        MidnightDive,  // 深夜強行潜行（静けさ4以上で解放・翌日疲労デバフ）
        LunchCanteen,  // 昼休み: 購買でアイテム入手（度胸+1）
        LunchRooftop   // 昼休み: 屋上でひとり（静けさ+5・特別演出）
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

        // 昼休み専用ボタン
        [SerializeField] private Button _lunchChatButton;
        [SerializeField] private Button _lunchLibraryButton;
        [SerializeField] private Button _lunchSkipButton;

        // 深夜専用ボタン（DESIGN.md 9-2: 強行潜行）
        [SerializeField] private Button _midnightDiveButton;

        // 昼休み拡張ボタン（購買・屋上）
        [SerializeField] private Button _lunchCanteenButton;
        [SerializeField] private Button _lunchRooftopButton;

        public event Action<FieldAction> OnActionSelected;

        private void Awake()
        {
            Bind(_studyButton, FieldAction.Study);
            Bind(_socializeButton, FieldAction.Socialize);
            Bind(_jobButton, FieldAction.PartTimeJob);
            Bind(_meditateButton, FieldAction.Meditate);
            Bind(_diveButton, FieldAction.Dive);
            Bind(_goHomeButton, FieldAction.GoHome);
            Bind(_lunchChatButton, FieldAction.LunchChat);
            Bind(_lunchLibraryButton, FieldAction.LunchLibrary);
            Bind(_lunchSkipButton, FieldAction.LunchSkip);
            Bind(_midnightDiveButton, FieldAction.MidnightDive);
            Bind(_lunchCanteenButton, FieldAction.LunchCanteen);
            Bind(_lunchRooftopButton, FieldAction.LunchRooftop);
        }

        public void Show(TimeSlot slot, int ap, bool lunchUsed = true, bool midnightDiveUnlocked = false)
        {
            if (_panel != null) _panel.SetActive(true);

            bool isLunch = slot == TimeSlot.Lunch;
            bool isLateNight = slot == TimeSlot.LateNight;
            bool canAct = ap > 0 && (slot == TimeSlot.AfterSchool || slot == TimeSlot.Evening);
            bool canDive = ap > 0 && slot != TimeSlot.Morning && slot != TimeSlot.Class && slot != TimeSlot.Lunch;
            bool canLunch = isLunch && !lunchUsed;

            // 通常行動: 昼休みと深夜は非表示
            bool showNormal = !isLunch && !isLateNight;
            SetVisible(_studyButton, showNormal);
            SetVisible(_socializeButton, showNormal);
            SetVisible(_jobButton, showNormal);
            SetVisible(_meditateButton, showNormal);
            SetVisible(_diveButton, showNormal);
            SetVisible(_goHomeButton, showNormal || isLateNight); // 深夜も「眠る」で帰宅可

            SetInteractable(_studyButton, canAct);
            SetInteractable(_socializeButton, canAct);
            SetInteractable(_jobButton, canAct);
            SetInteractable(_meditateButton, canAct);
            SetInteractable(_diveButton, canDive);
            SetInteractable(_goHomeButton, true);

            // 昼休み行動: 昼休み中のみ表示
            SetVisible(_lunchChatButton, isLunch);
            SetVisible(_lunchLibraryButton, isLunch);
            SetVisible(_lunchSkipButton, isLunch);
            SetVisible(_lunchCanteenButton, isLunch);
            SetVisible(_lunchRooftopButton, isLunch);

            SetInteractable(_lunchChatButton, canLunch);
            SetInteractable(_lunchLibraryButton, canLunch);
            SetInteractable(_lunchCanteenButton, canLunch);
            SetInteractable(_lunchRooftopButton, canLunch);
            SetInteractable(_lunchSkipButton, true); // スキップは常に可能

            // 深夜強行潜行: LateNight かつ静けさ4以上で表示（DESIGN.md 9-2）
            SetVisible(_midnightDiveButton, isLateNight && midnightDiveUnlocked);
            SetInteractable(_midnightDiveButton, isLateNight && midnightDiveUnlocked);
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
        private static void SetVisible(Button b, bool v) { if (b != null) b.gameObject.SetActive(v); }
    }
}
