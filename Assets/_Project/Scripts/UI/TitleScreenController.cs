using UnityEngine;
using UnityEngine.SceneManagement;
using ArcadiaOfEchoes.Core;

namespace ArcadiaOfEchoes.UI
{
    public class TitleScreenController : MonoBehaviour
    {
        [SerializeField] private CanvasGroup titleGroup;
        [SerializeField] private CanvasGroup menuGroup;
        [SerializeField] private float fadeSpeed = 2f;

        private enum TitleState { PressAny, Menu }
        private TitleState _state = TitleState.PressAny;
        private bool _isFading;

        private void Start()
        {
            if (menuGroup != null)
            {
                menuGroup.alpha = 0f;
                menuGroup.interactable = false;
                menuGroup.blocksRaycasts = false;
            }
        }

        private void Update()
        {
            if (_isFading) return;

            switch (_state)
            {
                case TitleState.PressAny:
                    if (Input.anyKeyDown)
                        TransitionToMenu();
                    break;
            }
        }

        private void TransitionToMenu()
        {
            _state = TitleState.Menu;
            if (titleGroup != null) titleGroup.interactable = false;
            if (menuGroup != null)
            {
                menuGroup.alpha = 1f;
                menuGroup.interactable = true;
                menuGroup.blocksRaycasts = true;
            }
        }

        public void OnNewGame()
        {
            GameManager.Instance?.TransitionTo(GamePhase.Field);
            SceneManager.LoadScene("Field");
        }

        public void OnContinue()
        {
            // セーブデータ選択UIを表示
            EventBus.Publish(new OpenSaveLoadUIEvent(isLoad: true));
        }

        public void OnSettings()
        {
            EventBus.Publish(new OpenSettingsUIEvent());
        }

        public void OnQuit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }

    public readonly struct OpenSaveLoadUIEvent
    {
        public readonly bool IsLoad;
        public OpenSaveLoadUIEvent(bool isLoad) { IsLoad = isLoad; }
    }

    public readonly struct OpenSettingsUIEvent { }
}
