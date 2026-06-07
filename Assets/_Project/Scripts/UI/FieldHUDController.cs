using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using EchoesOfArcadia.Core;
using EchoesOfArcadia.TimeSystem;

namespace EchoesOfArcadia.UI
{
    public class FieldHUDController : MonoBehaviour
    {
        [Header("Date & Time")]
        [SerializeField] private TextMeshProUGUI dateText;
        [SerializeField] private TextMeshProUGUI dayOfWeekText;
        [SerializeField] private TextMeshProUGUI timeOfDayText;
        [SerializeField] private Image timeOfDayIcon;

        [Header("Weather")]
        [SerializeField] private TextMeshProUGUI weatherText;
        [SerializeField] private Image weatherIcon;

        [Header("Action Points")]
        [SerializeField] private TextMeshProUGUI actionPointsText;
        [SerializeField] private Image[] actionPointDots;

        [Header("Deadline Warning")]
        [SerializeField] private CanvasGroup deadlineWarningGroup;
        [SerializeField] private TextMeshProUGUI deadlineText;
        [SerializeField] private Image deadlineBackground;

        [Header("Location")]
        [SerializeField] private TextMeshProUGUI locationText;

        [Header("Date Change Overlay")]
        [SerializeField] private CanvasGroup dateChangeOverlay;
        [SerializeField] private TextMeshProUGUI dateChangeDateText;
        [SerializeField] private TextMeshProUGUI dateChangeDayText;
        [SerializeField] private RectTransform dateChangeRect;

        [Header("Visual Settings")]
        [SerializeField] private Image hudBackground;

        private void OnEnable()
        {
            GameEventBus.Subscribe<TimeAdvancedEvent>(OnTimeAdvanced);
            GameEventBus.Subscribe<DayChangedEvent>(OnDayChanged);
        }

        private void OnDisable()
        {
            GameEventBus.Unsubscribe<TimeAdvancedEvent>(OnTimeAdvanced);
            GameEventBus.Unsubscribe<DayChangedEvent>(OnDayChanged);
        }

        private void Start()
        {
            RefreshAll();
        }

        private void OnTimeAdvanced(TimeAdvancedEvent e)
        {
            RefreshAll();
            if (TimeManager.Instance != null)
                UpdateTimeOfDayVisualAnimated(TimeManager.Instance.CurrentTimeOfDay);
        }

        private void OnDayChanged(DayChangedEvent e)
        {
            RefreshAll();
            PlayDateChangeAnimation();
        }

        public void RefreshAll()
        {
            if (TimeManager.Instance == null) return;

            var date = TimeManager.Instance.CurrentDate;
            var time = TimeManager.Instance.CurrentTimeOfDay;
            var weather = TimeManager.Instance.CurrentWeather;

            if (dateText != null) dateText.text = $"{date.Month}月{date.Day}日";
            if (dayOfWeekText != null) dayOfWeekText.text = GetJapaneseDayOfWeek(date.DayOfWeek);
            if (timeOfDayText != null) timeOfDayText.text = GetTimeOfDayName(time);
            if (weatherText != null) weatherText.text = GetWeatherName(weather);

            RefreshActionPoints();
            RefreshDeadlineWarning();
            UpdateTimeOfDayVisual(time);
        }

        public void SetLocation(string location)
        {
            if (locationText != null) locationText.text = location;
        }

        private void RefreshActionPoints()
        {
            if (TimeManager.Instance == null) return;
            int remaining = TimeManager.Instance.RemainingActionPoints;

            if (actionPointsText != null) actionPointsText.text = $"行動力: {remaining}";

            if (actionPointDots != null)
            {
                for (int i = 0; i < actionPointDots.Length; i++)
                {
                    if (actionPointDots[i] != null)
                        actionPointDots[i].color = i < remaining ? UIColors.Cyan : new Color(0.3f, 0.3f, 0.3f, 0.5f);
                }
            }
        }

        private void RefreshDeadlineWarning()
        {
            if (deadlineWarningGroup == null) return;

            var echoManager = Echo.EchoRealmManager.Instance;
            if (echoManager?.CurrentDungeon == null || !echoManager.IsDeadlineApproaching())
            {
                SetGroupVisible(deadlineWarningGroup, false);
                return;
            }

            int daysLeft = echoManager.GetDaysRemaining();
            SetGroupVisible(deadlineWarningGroup, true);

            if (deadlineText != null)
                deadlineText.text = daysLeft <= 1
                    ? "期限切れまで あと1日！"
                    : $"期限切れまで あと{daysLeft}日";

            if (deadlineBackground != null)
                deadlineBackground.color = daysLeft <= 1 ? UIColors.Crimson : UIColors.Amber;
        }

        private void UpdateTimeOfDayVisual(TimeOfDay time)
        {
            if (hudBackground == null) return;

            hudBackground.color = time switch
            {
                TimeOfDay.Morning => new Color(0.95f, 0.9f, 0.8f, 0.85f),
                TimeOfDay.Class => new Color(0.9f, 0.92f, 0.95f, 0.85f),
                TimeOfDay.Afternoon => new Color(0.95f, 0.85f, 0.7f, 0.85f),
                TimeOfDay.Evening => new Color(0.2f, 0.15f, 0.35f, 0.9f),
                TimeOfDay.LateNight => new Color(0.08f, 0.08f, 0.2f, 0.95f),
                _ => new Color(0.9f, 0.9f, 0.9f, 0.85f)
            };
        }

        private void PlayDateChangeAnimation()
        {
            if (dateChangeOverlay == null || TimeManager.Instance == null) return;

            var date = TimeManager.Instance.CurrentDate;
            if (dateChangeDateText != null)
                dateChangeDateText.text = $"{date.Month}月{date.Day}日";
            if (dateChangeDayText != null)
                dateChangeDayText.text = GetJapaneseDayOfWeek(date.DayOfWeek) + "曜日";

            AudioManager.Instance?.PlaySFX(SFXType.Calendar_DateChange);

            var seq = DOTween.Sequence();
            UIAnimator.SetVisible(dateChangeOverlay, false);
            if (dateChangeRect != null)
                dateChangeRect.localScale = Vector3.one * 1.3f;

            seq.Append(dateChangeOverlay.DOFade(1f, 0.3f).SetEase(Ease.OutQuad));
            if (dateChangeRect != null)
                seq.Join(dateChangeRect.DOScale(1f, 0.4f).SetEase(Ease.OutBack));

            seq.AppendInterval(1.2f);

            seq.Append(dateChangeOverlay.DOFade(0f, 0.4f).SetEase(Ease.InQuad));
            seq.OnComplete(() => UIAnimator.SetVisible(dateChangeOverlay, false));
        }

        private static string GetTimeOfDayName(TimeOfDay time) => time switch
        {
            TimeOfDay.Morning => "朝",
            TimeOfDay.Class => "授業中",
            TimeOfDay.Afternoon => "放課後",
            TimeOfDay.Evening => "夜",
            TimeOfDay.LateNight => "深夜",
            _ => ""
        };

        private static string GetWeatherName(Weather weather) => weather switch
        {
            Weather.Sunny => "晴れ",
            Weather.Cloudy => "曇り",
            Weather.Rainy => "雨",
            Weather.Stormy => "嵐",
            _ => ""
        };

        private static string GetJapaneseDayOfWeek(System.DayOfWeek day) => day switch
        {
            System.DayOfWeek.Sunday => "日",
            System.DayOfWeek.Monday => "月",
            System.DayOfWeek.Tuesday => "火",
            System.DayOfWeek.Wednesday => "水",
            System.DayOfWeek.Thursday => "木",
            System.DayOfWeek.Friday => "金",
            System.DayOfWeek.Saturday => "土",
            _ => ""
        };

        private void UpdateTimeOfDayVisualAnimated(TimeOfDay time)
        {
            if (hudBackground == null) return;

            Color targetColor = time switch
            {
                TimeOfDay.Morning => new Color(0.95f, 0.9f, 0.8f, 0.85f),
                TimeOfDay.Class => new Color(0.9f, 0.92f, 0.95f, 0.85f),
                TimeOfDay.Afternoon => new Color(0.95f, 0.85f, 0.7f, 0.85f),
                TimeOfDay.Evening => new Color(0.2f, 0.15f, 0.35f, 0.9f),
                TimeOfDay.LateNight => new Color(0.08f, 0.08f, 0.2f, 0.95f),
                _ => new Color(0.9f, 0.9f, 0.9f, 0.85f)
            };

            hudBackground.DOColor(targetColor, 0.8f).SetEase(Ease.InOutQuad);
        }
    }
}
