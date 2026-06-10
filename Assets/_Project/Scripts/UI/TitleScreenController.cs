using UnityEngine;
using UnityEngine.UI;
using Amane.Core;

namespace Amane.UI
{
    /// <summary>
    /// タイトル画面のコードビハインド。ロジックは持たず GameManager へ委譲する。
    /// DESIGN.md: 雨の窓ガラス→スワイプで結露が拭われタイトルが浮かぶ。
    /// NEW GAME＝「未読を開く」。本クラスはまずフロー配線のみを担う。
    /// </summary>
    public sealed class TitleScreenController : MonoBehaviour
    {
        [Header("Buttons")]
        [SerializeField] private Button _newGameButton;   // 「未読を開く」
        [SerializeField] private Button _continueButton;  // 「日記を読む」
        [SerializeField] private Button _quitButton;

        [Header("Presentation")]
        [SerializeField] private CanvasGroup _rootGroup;
        [SerializeField] private float _fadeInSeconds = 1.2f;

        private const int DefaultSaveSlot = 0;

        private void Start()
        {
            // Start()で配線。PrototypeBootstrapがAwake()後にSetPrivateFieldでボタンをセットするため、
            // Awake()時点ではボタンがnull。Start()なら確実にセット済み。
            _newGameButton?.onClick.AddListener(OnNewGame);
            _continueButton?.onClick.AddListener(OnContinue);
            _quitButton?.onClick.AddListener(OnQuit);
            RefreshContinueAvailability();
        }

        private void OnEnable()
        {
            FadeIn();
        }

        private void RefreshContinueAvailability()
        {
            bool hasSave = GameManager.Instance?.Save?.HasSave(DefaultSaveSlot) ?? false;
            if (_continueButton != null) _continueButton.interactable = hasSave;
        }

        private void FadeIn()
        {
            if (_rootGroup == null) return;
            // DOTween 導入後に結露ワイプ演出へ差し替える。まずは即時表示。
            _rootGroup.alpha = 1f;
        }

        private void OnNewGame()
        {
            // SE再生
            Core.Audio.AudioManager.Instance?.PlayUIClick();
            // 「未読を開く」: 新規ゲーム開始 → フィールドへ。
            GameManager.Instance?.StartNewGame();
        }

        private void OnContinue()
        {
            var game = GameManager.Instance;
            if (game == null) return;
            if (game.Save.Load(DefaultSaveSlot))
                game.StartNewGame(); // ロード成功後フィールドへ（暫定）。
        }

        private void OnQuit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void OnDestroy()
        {
            _newGameButton?.onClick.RemoveListener(OnNewGame);
            _continueButton?.onClick.RemoveListener(OnContinue);
            _quitButton?.onClick.RemoveListener(OnQuit);
        }
    }
}
