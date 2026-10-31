using TMPro;
using UnityEngine;
using StationJam.Core;

namespace StationJam.UI
{
    public class LevelNumberView : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField]
        private LevelSequence _levelSequence;

        [Header("UI")]
        [SerializeField]
        private TMP_Text _levelText;

        [SerializeField]
        private string _labelPrefix =
            "Уровень ";

        private void OnEnable()
        {
            Subscribe();
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        private void Subscribe()
        {
            if (_levelSequence == null)
            {
                Debug.LogError(
                    "[LevelNumberView] LevelSequence " +
                    "не назначен.");

                return;
            }

            _levelSequence.LevelStarted -=
                OnLevelStarted;

            _levelSequence.LevelStarted +=
                OnLevelStarted;
        }

        private void Unsubscribe()
        {
            if (_levelSequence != null)
            {
                _levelSequence.LevelStarted -=
                    OnLevelStarted;
            }
        }

        private void OnLevelStarted(
            int levelNumber)
        {
            if (_levelText == null)
            {
                Debug.LogError(
                    "[LevelNumberView] LevelText " +
                    "не назначен.");

                return;
            }

            _levelText.text =
                $"{_labelPrefix}{levelNumber}";
        }
    }
}