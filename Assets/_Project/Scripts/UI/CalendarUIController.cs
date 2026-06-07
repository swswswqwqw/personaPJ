using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using EchoesOfArcadia.Core;
using EchoesOfArcadia.TimeSystem;

namespace EchoesOfArcadia.UI
{
    public class CalendarUIController : MonoBehaviour
    {
        [Header("Calendar Grid")]
        [SerializeField] private CanvasGroup calendarGroup;
        [SerializeField] private TextMeshProUGUI monthYearText;
        [SerializeField] private CalendarDayCell[] dayCells;
        [SerializeField] private Button prevMonthButton;
        [SerializeField] private Button nextMonthButton;
        [SerializeField] private Button closeButton;

        [Header("Event Info")]
        [SerializeField] private TextMeshProUGUI eventInfoText;
        [SerializeField] private Image deadlineMarker;

        private int displayMonth;
        private int displayYear;

        private void OnEnable()
        {
            prevMonthButton?.onClick.AddListener(ShowPreviousMonth);
            nextMonthButton?.onClick.AddListener(ShowNextMonth);
            closeButton?.onClick.AddListener(Close);
        }

        private void OnDisable()
        {
            prevMonthButton?.onClick.RemoveAllListeners();
            nextMonthButton?.onClick.RemoveAllListeners();
            closeButton?.onClick.RemoveAllListeners();
        }

        public void Open()
        {
            if (TimeManager.Instance == null) return;

            displayMonth = TimeManager.Instance.CurrentDate.Month;
            displayYear = TimeManager.Instance.CurrentDate.Year;
            RefreshCalendar();
            SetVisible(true);
        }

        public void Close()
        {
            SetVisible(false);
        }

        private void ShowPreviousMonth()
        {
            displayMonth--;
            if (displayMonth < 1) { displayMonth = 12; displayYear--; }
            RefreshCalendar();
        }

        private void ShowNextMonth()
        {
            displayMonth++;
            if (displayMonth > 12) { displayMonth = 1; displayYear++; }
            RefreshCalendar();
        }

        private void RefreshCalendar()
        {
            if (monthYearText != null)
                monthYearText.text = $"{displayYear}年 {displayMonth}月";

            int daysInMonth = DateTime.DaysInMonth(displayYear, displayMonth);
            var firstDay = new DateTime(displayYear, displayMonth, 1);
            int startDayOfWeek = (int)firstDay.DayOfWeek;

            GameDate today = TimeManager.Instance?.CurrentDate ?? new GameDate();
            GameDate? deadline = GetCurrentDeadline();

            for (int i = 0; i < dayCells.Length; i++)
            {
                if (dayCells[i] == null) continue;

                int dayNumber = i - startDayOfWeek + 1;

                if (dayNumber < 1 || dayNumber > daysInMonth)
                {
                    dayCells[i].SetEmpty();
                    continue;
                }

                var cellDate = new GameDate(displayYear, displayMonth, dayNumber);
                bool isToday = cellDate == today;
                bool isPast = cellDate < today;
                bool isDeadline = deadline.HasValue && cellDate == deadline.Value;
                bool isWeekend = cellDate.IsWeekend;

                dayCells[i].SetDay(dayNumber, isToday, isPast, isDeadline, isWeekend);
            }
        }

        private GameDate? GetCurrentDeadline()
        {
            var echoManager = Echo.EchoRealmManager.Instance;
            return echoManager?.CurrentDungeon != null
                ? echoManager.CurrentDungeon.deadline
                : null;
        }

        private void SetVisible(bool visible)
        {
            if (calendarGroup == null) return;
            calendarGroup.alpha = visible ? 1f : 0f;
            calendarGroup.interactable = visible;
            calendarGroup.blocksRaycasts = visible;
        }
    }

    [Serializable]
    public class CalendarDayCell : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI dayNumberText;
        [SerializeField] private Image background;
        [SerializeField] private Image deadlineIcon;
        [SerializeField] private Image todayIndicator;

        public void SetDay(int day, bool isToday, bool isPast, bool isDeadline, bool isWeekend)
        {
            gameObject.SetActive(true);

            if (dayNumberText != null)
            {
                dayNumberText.text = day.ToString();
                dayNumberText.color = isPast
                    ? new Color(0.5f, 0.5f, 0.5f)
                    : isWeekend ? UIColors.Crimson : UIColors.OffBlack;
            }

            if (background != null)
                background.color = isToday ? new Color(UIColors.Cyan.r, UIColors.Cyan.g, UIColors.Cyan.b, 0.3f) : Color.clear;

            if (todayIndicator != null)
                todayIndicator.gameObject.SetActive(isToday);

            if (deadlineIcon != null)
            {
                deadlineIcon.gameObject.SetActive(isDeadline);
                if (isDeadline) deadlineIcon.color = UIColors.Crimson;
            }
        }

        public void SetEmpty()
        {
            gameObject.SetActive(false);
        }
    }
}
