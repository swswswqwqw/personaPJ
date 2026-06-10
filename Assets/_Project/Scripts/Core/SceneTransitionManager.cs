using UnityEngine;
using UnityEngine.SceneManagement;
using Astra.Core;

namespace Astra.Core
{
    public class SceneTransitionManager : MonoBehaviour
    {
        public static SceneTransitionManager Instance { get; private set; }

        [SerializeField] private float transitionDuration = 0.5f;

        private bool _isTransitioning;

        public bool IsTransitioning => _isTransitioning;

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

        public void TransitionTo(string sceneName, GamePhase targetPhase)
        {
            if (_isTransitioning) return;
            _isTransitioning = true;

            GameManager.Instance?.ChangePhase(GamePhase.Transition);
            EventBus.Publish(new SceneTransitionStartedEvent(sceneName));

            SceneManager.LoadScene(sceneName);
            _isTransitioning = false;

            GameManager.Instance?.ChangePhase(targetPhase);
            EventBus.Publish(new SceneTransitionCompletedEvent(sceneName));
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
    }

    public readonly struct SceneTransitionStartedEvent
    {
        public readonly string TargetScene;
        public SceneTransitionStartedEvent(string scene) { TargetScene = scene; }
    }

    public readonly struct SceneTransitionCompletedEvent
    {
        public readonly string Scene;
        public SceneTransitionCompletedEvent(string scene) { Scene = scene; }
    }
}
