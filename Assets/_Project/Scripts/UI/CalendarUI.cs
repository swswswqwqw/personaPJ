using UnityEngine;
using UnityEngine.UI;
using Amane.Core;
using Amane.Time;

namespace Amane.UI
{
    public sealed class CalendarUI : MonoBehaviour
    {
        [SerializeField] private Text _dateText;
        [SerializeField] private Text _dayOfWeekText;
        [SerializeField] private Text _weatherText;
        [SerializeField] private Text _timeSlotText;
        [SerializeField] private Text _apText;

        private static readonly string[] DayNames = { "日", "月", "火", "水", "木", "金", "土" };
        private System.IDisposable _daySub;
        private System.IDisposable _timeSub;

        private void OnEnable()
        {
            var gm = GameManager.Instance;
            if (gm == null) return;
            _daySub = gm.Events.Subscribe<DayChangedEvent>(OnDayChanged);
            _timeSub = gm.Events.Subscribe<TimeAdvancedEvent>(OnTimeAdvanced);
            Refresh();
        }

        private void OnDisable()
        {
            _daySub?.Dispose();
            _timeSub?.Dispose();
        }

        private void OnDayChanged(DayChangedEvent e) => Refresh();
        private void OnTimeAdvanced(TimeAdvancedEvent e) => Refresh();

        public void Refresh()
        {
            var tm = GameManager.Instance?.Time;
            if (tm == null) return;

            var date = tm.Today;
            SetText(_dateText, date.ToString());
            SetText(_dayOfWeekText, DayNames[date.DayOfWeek % DayNames.Length]);
            SetText(_weatherText, WeatherLabel(tm.TodayWeather));
            SetText(_timeSlotText, SlotLabel(tm.CurrentSlot));
            SetText(_apText, $"AP {tm.ActionPoints}/{TimeManager.MaxActionPoints}");
        }

        private static void SetText(Text t, string v) { if (t != null) t.text = v; }

        private static string WeatherLabel(Weather w) => w switch
        {
            Weather.Clear => "晴",
            Weather.Rain => "雨",
            Weather.Fog => "霧",
            _ => ""
        };

        private static string SlotLabel(TimeSlot s) => s switch
        {
            TimeSlot.Morning => "朝",
            TimeSlot.Class => "授業中",
            TimeSlot.AfterSchool => "放課後",
            TimeSlot.Evening => "夜",
            TimeSlot.LateNight => "深夜",
            _ => ""
        };
    }
}
