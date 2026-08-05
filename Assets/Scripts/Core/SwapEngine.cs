using UnityEngine;
using DG.Tweening;
using StationJam.Entities;

namespace StationJam.Core
{
    public class SwapEngine : MonoBehaviour
    {
        [Header("Scene References")]
        [SerializeField]
        private TransitSlot _transitSlot;

        [SerializeField]
        private LevelFlowDriver _levelFlowDriver;

        [Header("State")]
        [SerializeField]
        private Passenger _passengerInBuffer;

        [Header("Animation Settings")]
        [SerializeField]
        private float _jumpPower = 2f;

        [SerializeField]
        private float _jumpDuration = 0.5f;

        public void InitializeBuffer(
            Passenger initialPassenger)
        {
            _passengerInBuffer = initialPassenger;

            if (_passengerInBuffer != null &&
                _transitSlot != null)
            {
                _passengerInBuffer.SeatPosition =
                    _transitSlot.GetPosition().position;
            }
        }

        public void ProcessSwap(
            Passenger clickedPassenger)
        {
            if (clickedPassenger == null ||
                clickedPassenger == _passengerInBuffer)
            {
                return;
            }

            if (_levelFlowDriver == null)
            {
                Debug.LogError(
                    "[SwapEngine] LevelFlowDriver не назначен.");

                return;
            }

            if (!_levelFlowDriver.TryStartSwap())
            {
                return;
            }

            if (_passengerInBuffer == null)
            {
                MoveToBuffer(clickedPassenger);
            }
            else
            {
                SwapPassengers(
                    clickedPassenger,
                    _passengerInBuffer);
            }
        }

        private void MoveToBuffer(
            Passenger trainPassenger)
        {
            TrainWagon sourceWagon =
                trainPassenger.CurrentWagon;

            Vector3 targetBufferPosition =
                _transitSlot.GetPosition().position;

            trainPassenger.SeatPosition =
                trainPassenger.transform.position;

            if (sourceWagon != null &&
                !sourceWagon.TryRemovePassenger(
                    trainPassenger))
            {
                Debug.LogError(
                    "[SwapEngine] Ќе удалось удалить пассажира " +
                    "из исходного вагона.");

                _levelFlowDriver.FinishSwap();
                return;
            }

            _passengerInBuffer = trainPassenger;

            trainPassenger.PlayJump();

            Tween moveTween = trainPassenger.transform
                .DOJump(
                    targetBufferPosition,
                    _jumpPower,
                    1,
                    _jumpDuration)
                .SetLink(trainPassenger.gameObject);

            moveTween.OnComplete(() =>
            {
                trainPassenger.transform.position =
                    targetBufferPosition;

                trainPassenger.PlayIdle();

                _levelFlowDriver.FinishSwap();
            });

            moveTween.OnKill(() =>
            {
                _levelFlowDriver.FinishSwap();
            });
        }

        private void SwapPassengers(
            Passenger trainPassenger,
            Passenger bufferPassenger)
        {
            TrainWagon targetWagon =
                trainPassenger.CurrentWagon;

            if (targetWagon == null)
            {
                Debug.LogError(
                    "[SwapEngine] ¬ыбранный пассажир " +
                    "не находитс€ в вагоне.");

                _levelFlowDriver.FinishSwap();
                return;
            }

            Vector3 targetTrainSeat =
                trainPassenger.transform.position;

            Vector3 targetBufferPosition =
                _transitSlot.GetPosition().position;

            trainPassenger.SeatPosition =
                targetTrainSeat;

            bool removedFromWagon =
                targetWagon.TryRemovePassenger(
                    trainPassenger);

            if (!removedFromWagon)
            {
                Debug.LogError(
                    "[SwapEngine] Ќе удалось удалить выбранного " +
                    "пассажира из вагона.");

                _levelFlowDriver.FinishSwap();
                return;
            }

            bool addedToWagon =
                targetWagon.TryAddPassenger(
                    bufferPassenger);

            if (!addedToWagon)
            {
                Debug.LogError(
                    "[SwapEngine] Ќе удалось добавить пассажира " +
                    "из буфера в вагон.");

                // ¬озвращаем исходного пассажира обратно,
                // чтобы состо€ние вагона не оказалось сломанным.
                targetWagon.TryAddPassenger(
                    trainPassenger);

                _levelFlowDriver.FinishSwap();
                return;
            }

            trainPassenger.PlayJump();
            bufferPassenger.PlayJump();

            Sequence swapSequence =
                DOTween.Sequence();

            swapSequence.Join(
                trainPassenger.transform
                    .DOJump(
                        targetBufferPosition,
                        _jumpPower,
                        1,
                        _jumpDuration)
                    .SetLink(trainPassenger.gameObject));

            swapSequence.Join(
                bufferPassenger.transform
                    .DOJump(
                        targetTrainSeat,
                        _jumpPower,
                        1,
                        _jumpDuration)
                    .SetLink(bufferPassenger.gameObject));

            swapSequence.OnComplete(() =>
            {
                trainPassenger.transform.position =
                    targetBufferPosition;

                bufferPassenger.transform.position =
                    targetTrainSeat;

                trainPassenger.PlayIdle();
                bufferPassenger.PlayIdle();

                _passengerInBuffer =
                    trainPassenger;

                /*
                 * —начала завершаем обмен и возвращаем
                 * состо€ние Playing.
                 *
                 * «атем провер€ем вагон: эта проверка может
                 * запустить его отправление и победу.
                 */
                _levelFlowDriver.FinishSwap();

                targetWagon.CheckCompletion();
            });

            swapSequence.OnKill(() =>
            {
                _levelFlowDriver.FinishSwap();
            });
        }
    }
}