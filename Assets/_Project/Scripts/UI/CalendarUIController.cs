using UnityEngine;
using UnityEngine.UI;
using TMPro;
using AriaOfBacklight.Core;
using AriaOfBacklight.Time;

namespace AriaOfBacklight.UI
{
    public class CalendarUIController : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI dateText;
        [SerializeField] private TextMeshProUGUI timeSlotText;
        [SerializeField] private TextMeshProUGUI weatherText;
        [SerializeField] private Image weatherIcon;
        [SerializeField] private CanvasGroup transitionOverlay;
        [SerializeField] private float transitionDuration = 1.5f;

        private bool isTransitioning;
        private float transitionTimer;

        private void OnEnable()
        {
            EventBus.Subscribe<DayChangedEvent>(OnDayChanged);
            EventBus.Subscribe<TimeAdvancedEvent>(OnTimeAdvanced);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<DayChangedEvent>(OnDayChanged);
            EventBus.Unsubscribe<TimeAdvancedEvent>(OnTimeAdvanced);
        }

        private void Start()
        {
            UpdateDisplay();
        }

        private void Update()
        {
            if (!isTransitioning) return;

            transitionTimer += UnityEngine.Time.deltaTime;
            float t = transitionTimer / transitionDuration;

            if (t < 0.5f)
            {
                if (transitionOverlay != null)
                    transitionOverlay.alpha = t * 2f;
            }
            else
            {
                if (transitionOverlay != null)
                    transitionOverlay.alpha = 1f - (t - 0.5f) * 2f;
            }

            if (t >= 1f)
            {
                isTransitioning = false;
                if (transitionOverlay != null)
                    transitionOverlay.alpha = 0f;
            }
        }

        private void OnDayChanged(DayChangedEvent evt)
        {
            PlayDayTransition();
            UpdateDisplay();
        }

        private void OnTimeAdvanced(TimeAdvancedEvent evt)
        {
            UpdateDisplay();
        }

        private void UpdateDisplay()
        {
            var tm = TimeManager.Instance;
            if (tm == null) return;

            if (dateText != null) dateText.text = tm.GetDateString();
            if (timeSlotText != null) timeSlotText.text = tm.GetTimeSlotName();
            if (weatherText != null)
            {
                weatherText.text = tm.CurrentWeather switch
                {
                    Weather.Sunny => "晴れ",
                    Weather.Cloudy => "曇り",
                    Weather.Rainy => "雨",
                    Weather.Storm => "嵐",
                    _ => ""
                };
            }
        }

        private void PlayDayTransition()
        {
            isTransitioning = true;
            transitionTimer = 0f;
        }
    }
}
