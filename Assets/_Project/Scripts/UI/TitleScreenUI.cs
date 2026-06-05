using AstralEchoes.Core;
using AstralEchoes.Data;

namespace AstralEchoes.UI
{
    /// <summary>
    /// タイトル画面の制御。Unityではこれを MonoBehaviour として
    /// Canvas の子オブジェクトにアタッチする。
    /// ここではロジックのみを Pure C# で記述。
    /// </summary>
    public class TitleScreenController
    {
        public enum TitleMenuOption
        {
            NewGame,
            Continue,
            Settings,
            Quit
        }

        public bool HasSaveData { get; set; }

        int _selectedIndex;
        readonly TitleMenuOption[] _availableOptions;

        public TitleMenuOption SelectedOption => _availableOptions[_selectedIndex];

        public TitleScreenController(bool hasSaveData)
        {
            HasSaveData = hasSaveData;

            _availableOptions = hasSaveData
                ? new[] { TitleMenuOption.Continue, TitleMenuOption.NewGame, TitleMenuOption.Settings, TitleMenuOption.Quit }
                : new[] { TitleMenuOption.NewGame, TitleMenuOption.Settings, TitleMenuOption.Quit };

            _selectedIndex = 0;
        }

        public void MoveSelection(int direction)
        {
            _selectedIndex += direction;
            if (_selectedIndex < 0) _selectedIndex = _availableOptions.Length - 1;
            if (_selectedIndex >= _availableOptions.Length) _selectedIndex = 0;
        }

        public void Confirm()
        {
            switch (SelectedOption)
            {
                case TitleMenuOption.NewGame:
                    StartNewGame();
                    break;
                case TitleMenuOption.Continue:
                    LoadGame();
                    break;
                case TitleMenuOption.Settings:
                    OpenSettings();
                    break;
                case TitleMenuOption.Quit:
                    QuitGame();
                    break;
            }
        }

        void StartNewGame()
        {
            GameManager.Instance.ChangeState(GameState.Loading);
            AstralEchoes.Time.TimeManager.Instance.Initialize(4, 1, TimeOfDay.Morning);
            SceneTransitionManager.Instance.TransitionTo("Field_SchoolEntrance");
            GameManager.Instance.ChangeState(GameState.Cutscene);
        }

        void LoadGame()
        {
            GameManager.Instance.ChangeState(GameState.Loading);
            // SaveManager will restore state
            // SceneTransitionManager.Instance.TransitionTo(savedScene);
        }

        void OpenSettings()
        {
            // Settings menu overlay
        }

        void QuitGame()
        {
            // Application.Quit() in Unity
        }
    }
}
