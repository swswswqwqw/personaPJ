using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;

namespace EchoesOfArcadia.Core
{
    public class SceneLoader : MonoBehaviour
    {
        public static SceneLoader Instance { get; private set; }

        [SerializeField] private CanvasGroup fadeCanvasGroup;
        [SerializeField] private float fadeDuration = 0.5f;

        public bool IsTransitioning { get; private set; }

        public event Action<string> OnSceneLoadStarted;
        public event Action<string> OnSceneLoadCompleted;

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (fadeCanvasGroup != null)
            {
                fadeCanvasGroup.alpha = 0f;
                fadeCanvasGroup.blocksRaycasts = false;
            }
        }

        public async void LoadScene(string sceneName, GamePhase targetPhase)
        {
            if (IsTransitioning) return;
            IsTransitioning = true;

            OnSceneLoadStarted?.Invoke(sceneName);

            await FadeOut();

            var operation = SceneManager.LoadSceneAsync(sceneName);
            if (operation != null)
            {
                while (!operation.isDone)
                    await Task.Yield();
            }

            GameManager.Instance?.ChangePhase(targetPhase);

            await FadeIn();

            IsTransitioning = false;
            OnSceneLoadCompleted?.Invoke(sceneName);
        }

        public async void LoadSceneAdditive(string sceneName)
        {
            var operation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            if (operation != null)
            {
                while (!operation.isDone)
                    await Task.Yield();
            }
        }

        public async void UnloadScene(string sceneName)
        {
            var operation = SceneManager.UnloadSceneAsync(sceneName);
            if (operation != null)
            {
                while (!operation.isDone)
                    await Task.Yield();
            }
        }

        public async Task FadeOut()
        {
            if (fadeCanvasGroup == null) return;
            fadeCanvasGroup.blocksRaycasts = true;

            fadeCanvasGroup.DOKill();
            fadeCanvasGroup.DOFade(1f, fadeDuration).SetEase(Ease.InQuad).SetUpdate(true);
            await Task.Delay((int)(fadeDuration * 1000));
        }

        public async Task FadeIn()
        {
            if (fadeCanvasGroup == null) return;

            fadeCanvasGroup.DOKill();
            fadeCanvasGroup.DOFade(0f, fadeDuration).SetEase(Ease.OutQuad).SetUpdate(true)
                .OnComplete(() => fadeCanvasGroup.blocksRaycasts = false);
            await Task.Delay((int)(fadeDuration * 1000));
        }
    }
}
