using UnityEngine;
using UnityEngine.SceneManagement;
using StationJam.Core;

namespace StationJam.UI
{
    public class VictoryScreen : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] private LevelFlowDriver _levelFlowDriver;

        [Header("UI Panels")]
        [SerializeField] private GameObject _victoryPanel;

        private void OnEnable()
        {
            if (_levelFlowDriver != null)
            {
                _levelFlowDriver.OnLevelCompleted += ShowVictory;
            }
        }

        private void OnDisable()
        {
            if (_levelFlowDriver != null)
            {
                _levelFlowDriver.OnLevelCompleted -= ShowVictory;
            }
        }

        private void ShowVictory()
        {
            if (_victoryPanel != null)
            {
                _victoryPanel.SetActive(true);
            }
        }

        // Этот метод по-прежнему висит на кнопке "Next" в Canvas
        public void LoadNextLevel()
        {
            int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
            SceneManager.LoadScene(currentSceneIndex);
        }
    }
}