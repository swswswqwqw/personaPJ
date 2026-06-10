using UnityEngine;
using ArcadiaOfEchoes.Time;

namespace ArcadiaOfEchoes.Core
{
    public class GameInitializer : MonoBehaviour
    {
        [SerializeField] private bool autoInitialize = true;

        private void Start()
        {
            if (autoInitialize)
                InitializeNewGame();
        }

        public void InitializeNewGame()
        {
            TimeManager.Instance?.Initialize();
            PlayerStats.Instance?.Initialize();
            Social.ResonanceManager.Instance?.Initialize();

            GameManager.Instance?.ChangePhase(GamePhase.Field);

            Debug.Log("残響のアルカディア — New game initialized.");
            Debug.Log($"Date: {TimeManager.Instance?.CurrentMonth}月{TimeManager.Instance?.CurrentDay}日");
        }

        public void InitializeFromSave(Save.SaveData saveData)
        {
            // TODO: Apply save data to all systems
            Debug.Log("Game loaded from save.");
        }
    }
}
