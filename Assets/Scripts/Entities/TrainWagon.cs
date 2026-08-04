using System;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace StationJam.Entities
{
    public class TrainWagon : MonoBehaviour
    {
        public enum DepartDirection { Left, Right, Forward, Backward }

        [Header("Spawning Setup")]
        [SerializeField] private Transform[] _seatPoints;

        [Header("Wagon Settings")]
        [SerializeField] private ColorType _targetColor;
        [SerializeField] private int _capacity = 4;
        [SerializeField] private DepartDirection _departureDirection = DepartDirection.Right;

        [Header("State (Read Only)")]
        [SerializeField] private List<Passenger> _currentPassengers = new List<Passenger>();

        public event Action<TrainWagon> OnWagonDeparted;

        private bool _hasDeparted = false;

        public Transform[] SeatPoints => _seatPoints;
        public ColorType TargetColor => _targetColor;
        public int Capacity => _capacity;
        public bool IsFull => _currentPassengers.Count >= _capacity;

        public void InitializeData(ColorType targetColor, int capacity)
        {
            _targetColor = targetColor;
            _capacity = capacity;
            _currentPassengers.Clear();
        }

        public bool TryAddPassenger(Passenger passenger)
        {
            if (_hasDeparted || IsFull || _currentPassengers.Contains(passenger) || passenger == null)
            {
                return false;
            }

            _currentPassengers.Add(passenger);
            passenger.CurrentWagon = this;
            passenger.transform.SetParent(this.transform);
            return true;
        }

        public bool TryRemovePassenger(Passenger passenger)
        {
            if (_hasDeparted || !_currentPassengers.Contains(passenger) || passenger == null)
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
            if (_hasDeparted || !IsFull) return;

            foreach (var passenger in _currentPassengers)
            {
                if (passenger == null || passenger.PassengerColor != _targetColor)
                {
                    return;
                }
            }

            Depart();
        }

        private void Depart()
        {
            _hasDeparted = true;
            Debug.Log($"[{gameObject.name}] Ñîñòàâ ñîáðàí! Îòïðàâëåíèå...");

            // ÈÑÏÎËÜÇÓÅÌ ËÎÊÀËÜÍÛÅ ÎÑÈ ÑÀÌÎÃÎ ÂÀÃÎÍÀ
            Vector3 moveVector = transform.right;
            switch (_departureDirection)
            {
                case DepartDirection.Left: moveVector = -transform.right; break;
                case DepartDirection.Right: moveVector = transform.right; break;
                case DepartDirection.Forward: moveVector = transform.forward; break;
                case DepartDirection.Backward: moveVector = -transform.forward; break;
            }

            transform.DOMove(transform.position + moveVector * 25f, 2f)
                .SetEase(Ease.InQuad)
                .OnComplete(() =>
                {
                    OnWagonDeparted?.Invoke(this);
                    gameObject.SetActive(false);
                });
        }
    }
}