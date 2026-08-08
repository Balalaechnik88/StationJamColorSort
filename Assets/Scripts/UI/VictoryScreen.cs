using UnityEngine;
using StationJam.Core;

namespace StationJam.UI
{
    public class VictoryScreen : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField]
        private LevelFlowDriver _levelFlowDriver;

        [SerializeField]
        private LevelSequence _levelSequence;

        [Header("UI Panels")]
        [SerializeField]
        private GameObject _victoryPanel;

        private void Awake()
        {
            HideVictory();
        }

        private void OnEnable()
        {
            if (_levelFlowDriver == null)
            {
                return;
            }

            _levelFlowDriver.LevelCompleted +=
                OnLevelCompleted;

            _levelFlowDriver.StateChanged +=
                OnLevelStateChanged;
        }

        private void OnDisable()
        {
            if (_levelFlowDriver == null)
            {
                return;
            }

            _levelFlowDriver.LevelCompleted -=
                OnLevelCompleted;

            _levelFlowDriver.StateChanged -=
                OnLevelStateChanged;
        }

        public void LoadNextLevel()
        {
            if (_levelSequence == null)
            {
                Debug.LogError(
                    "[VictoryScreen] LevelSequence " +
                    "не назначен.");

                return;
            }

            _levelSequence.LoadNextLevel();
        }

        private void ShowVictory()
        {
            if (_victoryPanel != null)
            {
                _victoryPanel.SetActive(true);
            }
        }

        private void HideVictory()
        {
            if (_victoryPanel != null)
            {
                _victoryPanel.SetActive(false);
            }
        }

        private void OnLevelCompleted()
        {
            ShowVictory();
        }

        private void OnLevelStateChanged(
            LevelState levelState)
        {
            if (levelState != LevelState.Completed)
            {
                HideVictory();
            }
        }
    }
}