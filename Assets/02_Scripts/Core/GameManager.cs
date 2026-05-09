using WaterGrow.Board;
using WaterGrow.UI;
using UnityEngine;

namespace WaterGrow.Core
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField] private SaveManager saveManager;
        [SerializeField] private BoardManager boardManager;
        [SerializeField] private UIManager uiManager;

        public SaveManager SaveManager => saveManager;

        private void Awake()
        {
            if (saveManager == null)
            {
                saveManager = FindObjectOfType<SaveManager>();
            }

            if (boardManager == null)
            {
                boardManager = FindObjectOfType<BoardManager>();
            }

            if (uiManager == null)
            {
                uiManager = FindObjectOfType<UIManager>();
            }
        }

        private void Start()
        {
            if (boardManager != null)
            {
                boardManager.ConfigureSave(saveManager);
            }

            if (saveManager != null && boardManager != null)
            {
                boardManager.Initialize(saveManager.Current);
            }

            if (uiManager != null)
            {
                uiManager.RefreshAll();
            }
        }

        public void SaveGame()
        {
            if (saveManager == null || boardManager == null)
            {
                return;
            }

            boardManager.WriteBoardState(saveManager.Current);
            saveManager.Save();
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                SaveGame();
            }
        }

        private void OnApplicationQuit()
        {
            SaveGame();
        }
    }
}
