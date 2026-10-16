using UnityEngine;
using StationJam.Core;
using TMPro;

namespace StationJam.UI
{
    public class MoveBudgetView : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField]
        private MoveBudget _moveBudget;

        [Header("UI")]
        [SerializeField]
        private GameObject _contentRoot;

        [SerializeField]
        private TMP_Text _movesText;

        [SerializeField]
        private string _labelPrefix =
            "Ходы: ";

        private void OnEnable()
        {
            Subscribe();
            Refresh();
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        private void Subscribe()
        {
            if (_moveBudget == null)
            {
                Debug.LogError(
                    "[MoveBudgetView] MoveBudget " +
                    "не назначен.");

                return;
            }

            _moveBudget.MovesChanged -=
                OnMovesChanged;

            _moveBudget.MovesChanged +=
                OnMovesChanged;
        }

        private void Unsubscribe()
        {
            if (_moveBudget != null)
            {
                _moveBudget.MovesChanged -=
                    OnMovesChanged;
            }
        }

        private void OnMovesChanged(
            int remainingMoves)
        {
            Refresh();
        }

        private void Refresh()
        {
            if (_contentRoot == null ||
                _movesText == null ||
                _moveBudget == null)
            {
                return;
            }

            bool shouldBeVisible =
                _moveBudget.HasLimit;

            _contentRoot.SetActive(
                shouldBeVisible);

            if (!shouldBeVisible)
            {
                return;
            }

            _movesText.text =
                $"{_labelPrefix}" +
                $"{_moveBudget.RemainingMoves}";
        }
    }
}