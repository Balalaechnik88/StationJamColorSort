using UnityEngine;
using DG.Tweening;
using StationJam.Entities;

namespace StationJam.Core
{
    public class SwapEngine : MonoBehaviour
    {
        public enum EngineState
        {
            WaitInput,
            Swapping
        }

        [Header("Scene References")]
        [SerializeField] private TransitSlot _transitSlot;

        [Header("State")]
        [SerializeField] private Passenger _passengerInBuffer;
        [SerializeField] private EngineState _currentState = EngineState.WaitInput;

        [Header("Animation Settings")]
        [SerializeField] private float _jumpPower = 2f;
        [SerializeField] private float _jumpDuration = 0.5f;

        public void InitializeBuffer(Passenger initialPassenger)
        {
            _passengerInBuffer = initialPassenger;
            if (_passengerInBuffer != null)
            {
                _passengerInBuffer.SeatPosition = _transitSlot.GetPosition().position;
            }
        }

        public void ProcessSwap(Passenger clickedPassenger)
        {
            if (_currentState != EngineState.WaitInput || clickedPassenger == _passengerInBuffer)
            {
                return;
            }

            _currentState = EngineState.Swapping;

            if (_passengerInBuffer == null)
            {
                MoveToBuffer(clickedPassenger);
            }
            else
            {
                SwapPassengers(clickedPassenger, _passengerInBuffer);
            }
        }

        private void MoveToBuffer(Passenger trainPassenger)
        {
            TrainWagon sourceWagon = trainPassenger.CurrentWagon;

            trainPassenger.SeatPosition = trainPassenger.transform.position;
            Vector3 targetBufferPos = _transitSlot.GetPosition().position;

            if (sourceWagon != null)
            {
                sourceWagon.TryRemovePassenger(trainPassenger);
            }

            _passengerInBuffer = trainPassenger;

            trainPassenger.PlayJump();
            trainPassenger.transform.DOJump(targetBufferPos, _jumpPower, 1, _jumpDuration)
                .SetLink(trainPassenger.gameObject)
                .OnComplete(() =>
                {
                    // ФИКС: Принудительно впечатываем стикмена в пол
                    trainPassenger.transform.position = targetBufferPos;

                    trainPassenger.PlayIdle();
                    _currentState = EngineState.WaitInput;
                });
        }

        private void SwapPassengers(Passenger trainPassenger, Passenger bufferPassenger)
        {
            TrainWagon targetWagon = trainPassenger.CurrentWagon;

            trainPassenger.SeatPosition = trainPassenger.transform.position;
            Vector3 targetTrainSeat = trainPassenger.SeatPosition;
            Vector3 targetBufferPos = _transitSlot.GetPosition().position;

            if (targetWagon != null)
            {
                targetWagon.TryRemovePassenger(trainPassenger);
                targetWagon.TryAddPassenger(bufferPassenger);
            }

            Sequence swapSequence = DOTween.Sequence();

            trainPassenger.PlayJump();
            bufferPassenger.PlayJump();

            swapSequence.Join(trainPassenger.transform.DOJump(targetBufferPos, _jumpPower, 1, _jumpDuration).SetLink(trainPassenger.gameObject));
            swapSequence.Join(bufferPassenger.transform.DOJump(targetTrainSeat, _jumpPower, 1, _jumpDuration).SetLink(bufferPassenger.gameObject));

            swapSequence.OnComplete(() =>
            {
                // ФИКС: Принудительно впечатываем обоих стикменов в их финальные точки
                trainPassenger.transform.position = targetBufferPos;
                bufferPassenger.transform.position = targetTrainSeat;

                trainPassenger.PlayIdle();
                bufferPassenger.PlayIdle();

                _passengerInBuffer = trainPassenger;
                _currentState = EngineState.WaitInput;

                if (targetWagon != null)
                {
                    targetWagon.CheckCompletion();
                }
            });
        }
    }
}