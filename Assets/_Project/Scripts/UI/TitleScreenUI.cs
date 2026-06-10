using UnityEngine;
using ArcadiaOfEchoes.Core;

namespace ArcadiaOfEchoes.UI
{
    public class TitleScreenUI : UIPanel
    {
        [Header("Menu Buttons")]
        [SerializeField] private GameObject newGameButton;
        [SerializeField] private GameObject continueButton;
        [SerializeField] private GameObject optionsButton;

        [Header("Visuals")]
        [SerializeField] private CanvasGroup titleLogoGroup;
        [SerializeField] private CanvasGroup menuGroup;

        private bool _hasExistingSave;

        public override void OnActivate()
        {
            _hasExistingSave = Save.SaveManager.Instance?.SaveExists(0) ?? false;
            continueButton?.SetActive(_hasExistingSave);

            if (titleLogoGroup != null)
            {
                titleLogoGroup.alpha = 0f;
                // DOTween animation would go here:
                // titleLogoGroup.DOFade(1f, 1.5f).SetEase(Ease.OutQuad);
            }

            if (menuGroup != null)
            {
                menuGroup.alpha = 0f;
                // menuGroup.DOFade(1f, 0.8f).SetDelay(1.2f);
            }
        }

        public void OnNewGamePressed()
        {
            GameManager.Instance?.ChangePhase(GamePhase.Loading);
            // TODO: Initialize new game state and transition to opening scene
        }

        public void OnContinuePressed()
        {
            if (!_hasExistingSave) return;
            GameManager.Instance?.ChangePhase(GamePhase.Loading);
            Save.SaveManager.Instance?.LoadGame(0);
            // TODO: Transition to field scene
        }

        public void OnOptionsPressed()
        {
            // TODO: Open options panel
        }
    }
}
