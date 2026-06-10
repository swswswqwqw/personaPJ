using UnityEngine;
using TMPro;

namespace ArcadiaOfEchoes.UI
{
    public class CalendarUI : UIPanel
    {
        [SerializeField] private TextMeshProUGUI dateText;
        [SerializeField] private TextMeshProUGUI dayOfWeekText;
        [SerializeField] private TextMeshProUGUI timeSlotText;
        [SerializeField] private TextMeshProUGUI weatherText;
        [SerializeField] private CanvasGroup calendarGroup;

        private static readonly string[] MonthNames =
        {
            "", "1月", "2月", "3月", "4月", "5月", "6月",
            "7月", "8月", "9月", "10月", "11月", "12月"
        };

        private static readonly string[] DayOfWeekNames =
        {
            "日", "月", "火", "水", "木", "金", "土"
        };

        private static readonly string[] TimeSlotNames =
        {
            "朝", "授業中", "放課後", "夜", "深夜"
        };

        private static readonly string[] WeatherNames =
        {
            "晴れ", "曇り", "雨", "雪"
        };

        public void UpdateDisplay(int month, int day, int dayOfWeek, int timeSlot, int weather)
        {
            if (dateText != null)
                dateText.text = $"{MonthNames[month]}{day}日";

            if (dayOfWeekText != null)
                dayOfWeekText.text = $"({DayOfWeekNames[dayOfWeek]})";

            if (timeSlotText != null)
                timeSlotText.text = TimeSlotNames[timeSlot];

            if (weatherText != null)
                weatherText.text = WeatherNames[weather];
        }

        public void PlayDayTransition()
        {
            if (calendarGroup == null) return;
            // DOTween animation:
            // calendarGroup.alpha = 0;
            // Sequence seq = DOTween.Sequence();
            // seq.Append(calendarGroup.DOFade(1f, 0.5f));
            // seq.AppendInterval(1.5f);
            // seq.Append(calendarGroup.DOFade(0f, 0.5f));
        }
    }
}
