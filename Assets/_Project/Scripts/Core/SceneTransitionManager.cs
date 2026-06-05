using System;
using AstralEchoes.Data;

namespace AstralEchoes.Core
{
    public sealed class SceneTransitionManager
    {
        static SceneTransitionManager _instance;
        public static SceneTransitionManager Instance => _instance ??= new SceneTransitionManager();

        public string CurrentScene { get; private set; } = "Title";
        public bool IsTransitioning { get; private set; }

        public event Action<string> OnTransitionStarted;
        public event Action<string> OnTransitionCompleted;

        SceneTransitionManager() { }

        public void TransitionTo(string sceneName)
        {
            if (IsTransitioning || sceneName == CurrentScene) return;

            IsTransitioning = true;
            var fromScene = CurrentScene;

            OnTransitionStarted?.Invoke(sceneName);

            EventBus.Publish(new SceneTransitionEvent
            {
                FromScene = fromScene,
                ToScene = sceneName
            });

            // In Unity, this would use SceneManager.LoadSceneAsync
            // For now, simulate the transition
            CompleteTransition(sceneName);
        }

        void CompleteTransition(string sceneName)
        {
            CurrentScene = sceneName;
            IsTransitioning = false;
            OnTransitionCompleted?.Invoke(sceneName);
        }
    }
}
