using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace StationJam.Entities
{
    public class TrainWagon : MonoBehaviour
    {
        public enum DepartDirection
        {
            Left,
            Right,
            Forward,
            Backward
        }

        [Header("Spawning Setup")]
        [SerializeField]
        private Transform[] _seatPoints;

        [Header("Wagon Settings")]
        [SerializeField]
        private ColorType _targetColor;

        [SerializeField]
        private int _capacity = 4;

        [SerializeField]
        private DepartDirection _departureDirection =
            DepartDirection.Right;

        [Header("Departure Animation")]
        [SerializeField]
        [Min(0f)]
        private float _departureDistance = 25f;

        [SerializeField]
        [Min(0f)]
        private float _departureDuration = 2f;

        [Header("State (Read Only)")]
        [SerializeField]
        private List<Passenger> _currentPassengers =
            new List<Passenger>();

        private bool _hasDeparted;
        private bool _initialTransformCached;
        private Vector3 _initialLocalPosition;
        private Quaternion _initialLocalRotation;

        public event Action<TrainWagon> WagonDeparted;

        public Transform[] SeatPoints =>
            _seatPoints;

        public ColorType TargetColor =>
            _targetColor;

        public int Capacity =>
            _capacity;

        public bool IsFull =>
            _currentPassengers.Count >= _capacity;

        private void Awake()
        {
            CacheInitialTransform();
        }

        public void InitializeData(
            ColorType targetColor,
            int capacity)
        {
            CacheInitialTransform();

            transform.DOKill();

            transform.localPosition =
                _initialLocalPosition;

            transform.localRotation =
                _initialLocalRotation;

            gameObject.SetActive(true);

            _targetColor = targetColor;
            _capacity = capacity;

            _currentPassengers.Clear();
            _hasDeparted = false;
        }

        public void DisableForLevel()
        {
            CacheInitialTransform();

            transform.DOKill();

            transform.localPosition =
                _initialLocalPosition;

            transform.localRotation =
                _initialLocalRotation;

            _currentPassengers.Clear();
            _hasDeparted = false;

            gameObject.SetActive(false);
        }

        public bool TryAddPassenger(
            Passenger passenger)
        {
            if (passenger == null ||
                _hasDeparted ||
                IsFull ||
                _currentPassengers.Contains(passenger))
            {
                return false;
            }

            _currentPassengers.Add(passenger);

            passenger.CurrentWagon = this;
            passenger.transform.SetParent(transform);

            return true;
        }

        public bool TryRemovePassenger(
            Passenger passenger)
        {
            if (passenger == null ||
                _hasDeparted ||
                !_currentPassengers.Contains(passenger))
            {
                return false;
            }

            _currentPassengers.Remove(passenger);

            passenger.CurrentWagon = null;
            passenger.transform.SetParent(null);

            return true;
        }

        public void CheckCompletion()
        {
            if (_hasDeparted || !IsFull)
            {
                return;
            }

            foreach (Passenger passenger
                     in _currentPassengers)
            {
                if (passenger == null ||
                    passenger.PassengerColor != _targetColor)
                {
                    return;
                }
            }

            Depart();
        }

        private void Depart()
        {
            _hasDeparted = true;

            Debug.Log(
                $"[{gameObject.name}] Состав собран. " +
                "Начинается отправление.");

            Vector3 moveVector =
                GetDepartureVector();

            Vector3 targetPosition =
                transform.position +
                moveVector * _departureDistance;

            transform.DOMove(
                    targetPosition,
                    _departureDuration)
                .SetEase(Ease.InQuad)
                .SetLink(gameObject)
                .OnComplete(() =>
                {
                    WagonDeparted?.Invoke(this);

                    gameObject.SetActive(false);
                });
        }

        private Vector3 GetDepartureVector()
        {
            switch (_departureDirection)
            {
                case DepartDirection.Left:
                    return -transform.right;

                case DepartDirection.Right:
                    return transform.right;

                case DepartDirection.Forward:
                    return transform.forward;

                case DepartDirection.Backward:
                    return -transform.forward;

                default:
                    return transform.right;
            }
        }

        private void CacheInitialTransform()
        {
            if (_initialTransformCached)
            {
                return;
            }

            _initialLocalPosition =
                transform.localPosition;

            _initialLocalRotation =
                transform.localRotation;

            _initialTransformCached = true;
        }
    }
}