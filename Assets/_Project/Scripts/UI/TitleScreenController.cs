using UnityEngine;
using UnityEngine.UI;
using TMPro;
using EchoesOfArcadia.Core;

namespace EchoesOfArcadia.UI
{
    public class TitleScreenController : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private CanvasGroup titleLogoGroup;
        [SerializeField] private CanvasGroup menuGroup;
        [SerializeField] private Button newGameButton;
        [SerializeField] private Button continueButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI subtitleText;

        [Header("Wave Effect")]
        [SerializeField] private RectTransform[] waveLines;
        [SerializeField] private float waveSpeed = 1.5f;
        [SerializeField] private float waveAmplitude = 20f;

        [Header("Settings")]
        [SerializeField] private string gameSceneName = "Field";
        [SerializeField] private float logoFadeInDuration = 2f;
        [SerializeField] private float menuFadeInDelay = 1.5f;

        private float elapsedTime;
        private bool menuVisible;

        private void Start()
        {
            if (titleLogoGroup != null) titleLogoGroup.alpha = 0f;
            if (menuGroup != null)
            {
                menuGroup.alpha = 0f;
                menuGroup.interactable = false;
            }

            newGameButton?.onClick.AddListener(OnNewGamePressed);
            continueButton?.onClick.AddListener(OnContinuePressed);
            settingsButton?.onClick.AddListener(OnSettingsPressed);

            if (continueButton != null)
                continueButton.interactable = SaveManager.Instance != null && SaveManager.Instance.SaveExists(0);
        }

        private void Update()
        {
            elapsedTime += Time.deltaTime;

            AnimateLogoFadeIn();
            AnimateMenuFadeIn();
            AnimateWaveEffect();
        }

        private void AnimateLogoFadeIn()
        {
            if (titleLogoGroup == null || titleLogoGroup.alpha >= 1f) return;
            titleLogoGroup.alpha = Mathf.Clamp01(elapsedTime / logoFadeInDuration);
        }

        private void AnimateMenuFadeIn()
        {
            if (menuGroup == null || menuVisible) return;
            if (elapsedTime < menuFadeInDelay) return;

            float menuElapsed = elapsedTime - menuFadeInDelay;
            menuGroup.alpha = Mathf.Clamp01(menuElapsed / 1f);

            if (menuGroup.alpha >= 1f)
            {
                menuGroup.interactable = true;
                menuVisible = true;
            }
        }

        private void AnimateWaveEffect()
        {
            if (waveLines == null) return;

            for (int i = 0; i < waveLines.Length; i++)
            {
                if (waveLines[i] == null) continue;
                float offset = i * 0.5f;
                float y = Mathf.Sin((elapsedTime + offset) * waveSpeed) * waveAmplitude;
                var pos = waveLines[i].anchoredPosition;
                pos.y = y;
                waveLines[i].anchoredPosition = pos;
            }
        }

        private void OnNewGamePressed()
        {
            if (SceneLoader.Instance == null) return;
            GameEventBus.Publish(new NewGameStartedEvent());
            SceneLoader.Instance.LoadScene(gameSceneName, GamePhase.Field);
        }

        private void OnContinuePressed()
        {
            if (SaveManager.Instance == null) return;
            var saveData = SaveManager.Instance.Load(0);
            if (saveData == null) return;
            GameEventBus.Publish(new GameLoadedEvent(saveData));
            SceneLoader.Instance?.LoadScene(gameSceneName, GamePhase.Field);
        }

        private void OnSettingsPressed()
        {
            // TODO: 設定画面を開く
        }

        private void OnDestroy()
        {
            newGameButton?.onClick.RemoveListener(OnNewGamePressed);
            continueButton?.onClick.RemoveListener(OnContinuePressed);
            settingsButton?.onClick.RemoveListener(OnSettingsPressed);
        }
    }

    public readonly struct NewGameStartedEvent { }

    public readonly struct GameLoadedEvent
    {
        public readonly SaveData Data;
        public GameLoadedEvent(SaveData data) { Data = data; }
    }
}
