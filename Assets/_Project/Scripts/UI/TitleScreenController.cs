using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
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

        private void Start()
        {
            UIAnimator.SetVisible(titleLogoGroup, false);
            UIAnimator.SetVisible(menuGroup, false);

            newGameButton?.onClick.AddListener(OnNewGamePressed);
            continueButton?.onClick.AddListener(OnContinuePressed);
            settingsButton?.onClick.AddListener(OnSettingsPressed);

            if (continueButton != null)
                continueButton.interactable = SaveManager.Instance != null && SaveManager.Instance.SaveExists(0);

            AudioManager.Instance?.PlayBGM(BGMTrack.Title);

            PlayOpeningSequence();
        }

        private void PlayOpeningSequence()
        {
            var seq = DOTween.Sequence();
            seq.AppendInterval(0.3f);
            seq.Append(UIAnimator.FadeIn(titleLogoGroup, logoFadeInDuration));
            seq.AppendInterval(menuFadeInDelay - logoFadeInDuration);
            seq.Append(UIAnimator.FadeIn(menuGroup, 0.8f));
        }

        private void Update()
        {
            AnimateWaveEffect();
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
            AudioManager.Instance?.PlaySFX(SFXType.UI_Confirm);
            if (SceneLoader.Instance == null) return;

            UIAnimator.FadeOut(menuGroup, 0.3f);
            DOVirtual.DelayedCall(0.4f, () =>
            {
                GameEventBus.Publish(new NewGameStartedEvent());
                SceneLoader.Instance.LoadScene(gameSceneName, GamePhase.Field);
            });
        }

        private void OnContinuePressed()
        {
            AudioManager.Instance?.PlaySFX(SFXType.UI_Confirm);
            if (SaveManager.Instance == null) return;
            var saveData = SaveManager.Instance.Load(0);
            if (saveData == null) return;

            UIAnimator.FadeOut(menuGroup, 0.3f);
            DOVirtual.DelayedCall(0.4f, () =>
            {
                GameEventBus.Publish(new GameLoadedEvent(saveData));
                SceneLoader.Instance?.LoadScene(gameSceneName, GamePhase.Field);
            });
        }

        private void OnSettingsPressed()
        {
            AudioManager.Instance?.PlaySFX(SFXType.UI_Select);
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
