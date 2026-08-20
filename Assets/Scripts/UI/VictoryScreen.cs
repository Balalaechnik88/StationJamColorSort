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

        [Header("Victory UI")]
        [SerializeField]
        private GameObject _victoryPanel;

        [SerializeField]
        private GameObject _levelCompletedView;

        [SerializeField]
        private GameObject _nextLevelButton;

        [SerializeField]
        private GameObject _sequenceCompletedView;

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

            if (!_levelSequence.HasNextLevel)
            {
                Debug.LogWarning(
                    "[VictoryScreen] Следующий уровень " +
                    "недоступен.");

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

        private void UpdateCompletionView()
        {
            if (_levelSequence == null)
            {
                Debug.LogError(
                    "[VictoryScreen] LevelSequence " +
                    "не назначен.");

                return;
            }

            bool hasNextLevel =
                _levelSequence.HasNextLevel;

            if (_levelCompletedView != null)
            {
                _levelCompletedView.SetActive(
                    hasNextLevel);
            }

            if (_nextLevelButton != null)
            {
                _nextLevelButton.SetActive(
                    hasNextLevel);
            }

            if (_sequenceCompletedView != null)
            {
                _sequenceCompletedView.SetActive(
                    !hasNextLevel);
            }
        }

        private void OnLevelCompleted()
        {
            UpdateCompletionView();
            ShowVictory();
        }

        private void OnLevelStateChanged(
            LevelState levelState)
        {
            if (levelState == LevelState.Completed)
            {
                return;
            }

            HideVictory();
        }
    }
}