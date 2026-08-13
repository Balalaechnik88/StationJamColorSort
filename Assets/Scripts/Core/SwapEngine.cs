using DG.Tweening;
using UnityEngine;
using StationJam.Entities;

namespace StationJam.Core
{
    public class SwapEngine : MonoBehaviour
    {
        private const int JumpCount = 1;

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
        [Min(0f)]
        private float _jumpPower = 2f;

        [SerializeField]
        [Min(0f)]
        private float _jumpDuration = 0.5f;

        private Tween _activeTween;

        private void OnDisable()
        {
            CancelActiveSwap();
        }

        public void InitializeBuffer(
            Passenger initialPassenger)
        {
            CancelActiveSwap();

            _passengerInBuffer =
                initialPassenger;

            if (_passengerInBuffer == null ||
                _transitSlot == null)
            {
                return;
            }

            Vector3 bufferPosition =
                _transitSlot
                    .GetPosition()
                    .position;

            _passengerInBuffer.SeatPosition =
                bufferPosition;
        }

        public void ProcessSwap(
            Passenger clickedPassenger)
        {
            if (clickedPassenger == null ||
                clickedPassenger == _passengerInBuffer)
            {
                return;
            }

            if (_transitSlot == null)
            {
                Debug.LogError(
                    "[SwapEngine] TransitSlot не назначен.");

                return;
            }

            if (_levelFlowDriver == null)
            {
                Debug.LogError(
                    "[SwapEngine] LevelFlowDriver " +
                    "не назначен.");

                return;
            }

            TrainWagon clickedPassengerWagon =
                clickedPassenger.CurrentWagon;

            if (clickedPassengerWagon == null)
            {
                Debug.LogWarning(
                    "[SwapEngine] Выбранный пассажир " +
                    "не находится в вагоне.");

                return;
            }

            if (!clickedPassengerWagon.CanInteract)
            {
                return;
            }

            if (!_levelFlowDriver.TryStartSwap())
            {
                return;
            }

            if (_passengerInBuffer == null)
            {
                MoveToBuffer(
                    clickedPassenger);

                return;
            }

            SwapPassengers(
                clickedPassenger,
                _passengerInBuffer);
        }

        private void MoveToBuffer(
            Passenger trainPassenger)
        {
            TrainWagon sourceWagon =
                trainPassenger.CurrentWagon;

            if (sourceWagon == null ||
                !sourceWagon.CanInteract)
            {
                _levelFlowDriver.FinishSwap();

                return;
            }

            Vector3 startPosition =
                trainPassenger.transform.position;

            Vector3 targetBufferPosition =
                _transitSlot
                    .GetPosition()
                    .position;

            trainPassenger.SeatPosition =
                startPosition;

            trainPassenger.PlayJump();

            bool operationResolved = false;

            Tween moveTween =
                trainPassenger.transform
                    .DOJump(
                        targetBufferPosition,
                        _jumpPower,
                        JumpCount,
                        _jumpDuration);

            _activeTween =
                moveTween;

            moveTween.OnComplete(() =>
            {
                bool passengerRemoved =
                    sourceWagon.TryRemovePassenger(
                        trainPassenger);

                if (!passengerRemoved)
                {
                    Debug.LogError(
                        "[SwapEngine] Не удалось завершить " +
                        "перемещение пассажира в буфер.");

                    RestorePassenger(
                        trainPassenger,
                        startPosition);

                    operationResolved = true;

                    _levelFlowDriver.FinishSwap();

                    return;
                }

                trainPassenger.transform.position =
                    targetBufferPosition;

                trainPassenger.SeatPosition =
                    targetBufferPosition;

                _passengerInBuffer =
                    trainPassenger;

                trainPassenger.PlayIdle();

                operationResolved = true;

                _levelFlowDriver.FinishSwap();
            });

            moveTween.OnKill(() =>
            {
                if (!operationResolved)
                {
                    RestorePassenger(
                        trainPassenger,
                        startPosition);

                    _levelFlowDriver.FinishSwap();
                }

                if (_activeTween == moveTween)
                {
                    _activeTween = null;
                }
            });
        }

        private void SwapPassengers(
            Passenger trainPassenger,
            Passenger bufferPassenger)
        {
            TrainWagon targetWagon =
                trainPassenger.CurrentWagon;

            if (targetWagon == null ||
                !targetWagon.CanInteract)
            {
                _levelFlowDriver.FinishSwap();

                return;
            }

            Vector3 trainStartPosition =
                trainPassenger.transform.position;

            Vector3 bufferStartPosition =
                bufferPassenger.transform.position;

            Vector3 targetTrainSeat =
                trainStartPosition;

            Vector3 targetBufferPosition =
                _transitSlot
                    .GetPosition()
                    .position;

            trainPassenger.SeatPosition =
                targetTrainSeat;

            trainPassenger.PlayJump();
            bufferPassenger.PlayJump();

            bool operationResolved = false;

            Sequence swapSequence =
                DOTween.Sequence();

            swapSequence.Join(
                trainPassenger.transform
                    .DOJump(
                        targetBufferPosition,
                        _jumpPower,
                        JumpCount,
                        _jumpDuration));

            swapSequence.Join(
                bufferPassenger.transform
                    .DOJump(
                        targetTrainSeat,
                        _jumpPower,
                        JumpCount,
                        _jumpDuration));

            _activeTween =
                swapSequence;

            swapSequence.OnComplete(() =>
            {
                bool passengersReplaced =
                    targetWagon.TryReplacePassenger(
                        trainPassenger,
                        bufferPassenger);

                if (!passengersReplaced)
                {
                    Debug.LogError(
                        "[SwapEngine] Не удалось завершить " +
                        "обмен пассажиров.");

                    RestorePassenger(
                        trainPassenger,
                        trainStartPosition);

                    RestorePassenger(
                        bufferPassenger,
                        bufferStartPosition);

                    operationResolved = true;

                    _levelFlowDriver.FinishSwap();

                    return;
                }

                trainPassenger.transform.position =
                    targetBufferPosition;

                trainPassenger.SeatPosition =
                    targetBufferPosition;

                bufferPassenger.transform.position =
                    targetTrainSeat;

                bufferPassenger.SeatPosition =
                    targetTrainSeat;

                _passengerInBuffer =
                    trainPassenger;

                trainPassenger.PlayIdle();
                bufferPassenger.PlayIdle();

                operationResolved = true;

                _levelFlowDriver.FinishSwap();

                targetWagon.CheckCompletion();
            });

            swapSequence.OnKill(() =>
            {
                if (!operationResolved)
                {
                    RestorePassenger(
                        trainPassenger,
                        trainStartPosition);

                    RestorePassenger(
                        bufferPassenger,
                        bufferStartPosition);

                    _levelFlowDriver.FinishSwap();
                }

                if (_activeTween == swapSequence)
                {
                    _activeTween = null;
                }
            });
        }

        private void CancelActiveSwap()
        {
            if (_activeTween == null)
            {
                return;
            }

            if (_activeTween.IsActive())
            {
                _activeTween.Kill();
            }

            _activeTween =
                null;
        }

        private void RestorePassenger(
            Passenger passenger,
            Vector3 position)
        {
            if (passenger == null)
            {
                return;
            }

            passenger.transform.position =
                position;

            passenger.PlayIdle();
        }
    }
}