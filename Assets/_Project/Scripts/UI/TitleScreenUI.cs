using UnityEngine;
using Astra.Core;

namespace Astra.UI
{
    public class TitleScreenUI : MonoBehaviour
    {
        [SerializeField] private GameObject newGameButton;
        [SerializeField] private GameObject continueButton;
        [SerializeField] private GameObject optionsButton;

        private void Start()
        {
            if (continueButton != null)
                continueButton.SetActive(SaveLoadManager.Instance?.SaveExists(0) ?? false);
        }

        public void OnNewGame()
        {
            AudioManager.Instance?.PlaySE("ui_decide");
            SceneTransitionManager.Instance?.TransitionTo("Field", GamePhase.Field);
        }

        public void OnContinue()
        {
            AudioManager.Instance?.PlaySE("ui_decide");
            SaveLoadManager.Instance?.Load(0);
        }

        public void OnOptions()
        {
            AudioManager.Instance?.PlaySE("ui_select");
        }

        public void OnButtonHover()
        {
            AudioManager.Instance?.PlaySE("ui_cursor");
        }
    }
}
