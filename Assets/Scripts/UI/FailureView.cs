using UnityEngine;
using StationJam.Core;

namespace StationJam.UI
{
    public class FailureView : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField]
        private LevelFlowDriver _levelFlowDriver;

        [SerializeField]
        private LevelSequence _levelSequence;

        [Header("Failure UI")]
        [SerializeField]
        private GameObject _failurePanel;

        private void Awake()
        {
            HideFailure();
        }

        private void OnEnable()
        {
            Subscribe();

            if (_levelFlowDriver != null &&
                _levelFlowDriver.CurrentState ==
                LevelState.Failed)
            {
                ShowFailure();
            }
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        public void RetryLevel()
        {
            if (_levelSequence == null)
            {
                Debug.LogError(
                    "[FailureView] LevelSequence " +
                    "не назначен.");

                return;
            }

            HideFailure();

            _levelSequence.RestartCurrentLevel();
        }

        private void Subscribe()
        {
            if (_levelFlowDriver == null)
            {
                Debug.LogError(
                    "[FailureView] LevelFlowDriver " +
                    "не назначен.");

                return;
            }

            _levelFlowDriver.LevelFailed -=
                OnLevelFailed;

            _levelFlowDriver.LevelFailed +=
                OnLevelFailed;

            _levelFlowDriver.StateChanged -=
                OnStateChanged;

            _levelFlowDriver.StateChanged +=
                OnStateChanged;
        }

        private void Unsubscribe()
        {
            if (_levelFlowDriver == null)
            {
                return;
            }

            _levelFlowDriver.LevelFailed -=
                OnLevelFailed;

            _levelFlowDriver.StateChanged -=
                OnStateChanged;
        }

        private void OnLevelFailed()
        {
            ShowFailure();
        }

        private void OnStateChanged(
            LevelState levelState)
        {
            if (levelState != LevelState.Failed)
            {
                HideFailure();
            }
        }

        private void ShowFailure()
        {
            if (_failurePanel == null)
            {
                Debug.LogError(
                    "[FailureView] FailurePanel " +
                    "не назначен.");

                return;
            }

            _failurePanel.SetActive(
                true);
        }

        private void HideFailure()
        {
            if (_failurePanel != null)
            {
                _failurePanel.SetActive(
                    false);
            }
        }
    }
}