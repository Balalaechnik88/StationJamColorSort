using UnityEngine;

namespace StationJam.Core
{
    public class MoveLimitRule : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField]
        private MoveBudget _moveBudget;

        [SerializeField]
        private LevelFlowDriver _levelFlowDriver;

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
            if (_moveBudget == null)
            {
                return;
            }

            _moveBudget.MovesExhausted -=
                OnMovesExhausted;

            _moveBudget.MovesExhausted +=
                OnMovesExhausted;
        }

        private void Unsubscribe()
        {
            if (_moveBudget != null)
            {
                _moveBudget.MovesExhausted -=
                    OnMovesExhausted;
            }
        }

        private void OnMovesExhausted()
        {
            if (_levelFlowDriver == null)
            {
                Debug.LogError(
                    "[MoveLimitRule] LevelFlowDriver " +
                    "не назначен.");

                return;
            }

            if (_levelFlowDriver.IsCompletionPending)
            {
                Debug.Log(
                    "[MoveLimitRule] Ходы закончились, " +
                    "но завершение уровня уже ожидается.");

                return;
            }

            bool levelFailed =
                _levelFlowDriver.TryFailLevel();

            if (levelFailed)
            {
                Debug.Log(
                    "[MoveLimitRule] Лимит ходов " +
                    "исчерпан. Уровень проигран.");
            }
        }
    }
}