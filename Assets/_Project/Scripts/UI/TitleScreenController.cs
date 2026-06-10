using UnityEngine;
using UnityEngine.SceneManagement;
using AriaOfBacklight.Core;

namespace AriaOfBacklight.UI
{
    public class TitleScreenController : MonoBehaviour
    {
        [SerializeField] private CanvasGroup titleGroup;
        [SerializeField] private CanvasGroup menuGroup;
        [SerializeField] private float fadeInDuration = 2.0f;
        [SerializeField] private float logoDelay = 1.0f;

        private enum TitleState { FadeIn, WaitForInput, Menu }
        private TitleState state = TitleState.FadeIn;
        private float timer;

        private void Start()
        {
            if (titleGroup != null) titleGroup.alpha = 0f;
            if (menuGroup != null)
            {
                menuGroup.alpha = 0f;
                menuGroup.interactable = false;
            }
            timer = 0f;
        }

        private void Update()
        {
            switch (state)
            {
                case TitleState.FadeIn:
                    timer += UnityEngine.Time.deltaTime;
                    if (timer < logoDelay) return;
                    float t = (timer - logoDelay) / fadeInDuration;
                    if (titleGroup != null) titleGroup.alpha = Mathf.Clamp01(t);
                    if (t >= 1f)
                    {
                        state = TitleState.WaitForInput;
                    }
                    break;

                case TitleState.WaitForInput:
                    if (Input.anyKeyDown)
                    {
                        ShowMenu();
                    }
                    break;

                case TitleState.Menu:
                    break;
            }
        }

        private void ShowMenu()
        {
            state = TitleState.Menu;
            if (menuGroup != null)
            {
                menuGroup.alpha = 1f;
                menuGroup.interactable = true;
                menuGroup.blocksRaycasts = true;
            }
        }

        public void OnNewGame()
        {
            GameManager.Instance?.ChangePhase(GamePhase.Field);
            SceneManager.LoadScene("Field_School");
        }

        public void OnContinue()
        {
            var data = SaveLoadManager.Load(0);
            if (data == null) return;
            GameManager.Instance?.ChangePhase(GamePhase.Field);
            SceneManager.LoadScene(data.currentScene);
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
}
