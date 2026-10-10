using System;
using UnityEngine;

namespace StationJam.Core
{
    public class MoveBudget : MonoBehaviour
    {
        public event Action<int> MovesChanged;
        public event Action MovesExhausted;

        [Header("Dependencies")]
        [SerializeField]
        private SwapEngine _swapEngine;

        [Header("Runtime State")]
        [SerializeField]
        private bool _hasLimit;

        [SerializeField]
        private int _remainingMoves;

        public bool HasLimit =>
            _hasLimit;

        public int RemainingMoves =>
            _remainingMoves;

        private void OnEnable()
        {
            SubscribeToSwapEngine();
        }

        private void OnDisable()
        {
            UnsubscribeFromSwapEngine();
        }

        public void Initialize(
            int movesLimit)
        {
            if (movesLimit < 0)
            {
                Debug.LogError(
                    "[MoveBudget] Лимит ходов " +
                    "не может быть отрицательным.");

                ResetBudget();

                return;
            }

            _hasLimit =
                movesLimit > 0;

            _remainingMoves =
                _hasLimit
                    ? movesLimit
                    : 0;

            MovesChanged?.Invoke(
                _remainingMoves);

            Debug.Log(
                _hasLimit
                    ? $"[MoveBudget] Доступно ходов: " +
                      $"{_remainingMoves}."
                    : "[MoveBudget] Лимит ходов " +
                      "отключён.");
        }

        public bool TryConsumeMove()
        {
            if (!_hasLimit ||
                _remainingMoves <= 0)
            {
                return false;
            }

            _remainingMoves--;

            MovesChanged?.Invoke(
                _remainingMoves);

            if (_remainingMoves == 0)
            {
                Debug.Log(
                    "[MoveBudget] Ходы закончились.");

                MovesExhausted?.Invoke();
            }

            return true;
        }

        public void ResetBudget()
        {
            _hasLimit =
                false;

            _remainingMoves =
                0;

            MovesChanged?.Invoke(
                _remainingMoves);
        }

        private void SubscribeToSwapEngine()
        {
            if (_swapEngine == null)
            {
                return;
            }

            _swapEngine.MoveCommitted -=
                OnMoveCommitted;

            _swapEngine.MoveCommitted +=
                OnMoveCommitted;
        }

        private void UnsubscribeFromSwapEngine()
        {
            if (_swapEngine != null)
            {
                _swapEngine.MoveCommitted -=
                    OnMoveCommitted;
            }
        }

        private void OnMoveCommitted()
        {
            TryConsumeMove();
        }
    }
}