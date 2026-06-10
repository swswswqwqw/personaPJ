using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ArcanaOfHollows.Core
{
    public class SceneTransitionManager : MonoBehaviour
    {
        public static SceneTransitionManager Instance { get; private set; }

        [SerializeField] private CanvasGroup fadeCanvasGroup;
        [SerializeField] private float fadeDuration = 0.5f;

        private bool isTransitioning;

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public async Task LoadScene(string sceneName, GamePhase targetPhase)
        {
            if (isTransitioning) return;
            isTransitioning = true;

            EventBus.Publish(new SceneTransitionEvent(sceneName, true));

            await FadeOut();

            var operation = SceneManager.LoadSceneAsync(sceneName);
            while (!operation.isDone)
                await Task.Yield();

            GameManager.Instance?.TransitionTo(targetPhase);

            await FadeIn();

            EventBus.Publish(new SceneTransitionEvent(sceneName, false));
            isTransitioning = false;
        }

        private async Task FadeOut()
        {
            if (fadeCanvasGroup == null) return;
            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += UnityEngine.Time.unscaledDeltaTime;
                fadeCanvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeDuration);
                await Task.Yield();
            }
            fadeCanvasGroup.alpha = 1f;
        }

        private async Task FadeIn()
        {
            if (fadeCanvasGroup == null) return;
            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += UnityEngine.Time.unscaledDeltaTime;
                fadeCanvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
                await Task.Yield();
            }
            fadeCanvasGroup.alpha = 0f;
        }
    }

    public readonly struct SceneTransitionEvent
    {
        public readonly string SceneName;
        public readonly bool IsStarting;

        public SceneTransitionEvent(string sceneName, bool isStarting)
        {
            SceneName = sceneName;
            IsStarting = isStarting;
        }
    }
}
