using System;
using System.Collections.Generic;
using UnityEngine;
using StationJam.Entities;

namespace StationJam.Core
{
    public class LevelFlowDriver : MonoBehaviour
    {
        [Header("State")]
        [SerializeField]
        private LevelState _currentState =
            LevelState.NotInitialized;

        private readonly List<TrainWagon> _wagonsInLevel =
            new List<TrainWagon>();

        private int _departedWagonsCount;
        private bool _isInitialized;

        public event Action LevelCompleted;
        public event Action<LevelState> StateChanged;

        public LevelState CurrentState =>
            _currentState;

        public bool CanAcceptInput =>
            _currentState == LevelState.Playing;

        private void OnEnable()
        {
            if (_isInitialized)
            {
                SubscribeToWagons();
            }
        }

        private void OnDisable()
        {
            UnsubscribeFromWagons();
        }

        public void Initialize(
            IReadOnlyList<TrainWagon> wagons)
        {
            UnsubscribeFromWagons();

            _wagonsInLevel.Clear();
            _departedWagonsCount = 0;
            _isInitialized = false;

            SetState(LevelState.NotInitialized);

            if (wagons == null)
            {
                Debug.LogError(
                    "[LevelFlowDriver] Получен null вместо списка вагонов.");

                return;
            }

            foreach (TrainWagon wagon in wagons)
            {
                if (wagon != null)
                {
                    _wagonsInLevel.Add(wagon);
                }
            }

            if (_wagonsInLevel.Count == 0)
            {
                Debug.LogWarning(
                    "[LevelFlowDriver] На уровне нет активных вагонов.");

                return;
            }

            SubscribeToWagons();

            _isInitialized = true;

            SetState(LevelState.Playing);

            Debug.Log(
                $"[LevelFlowDriver] Уровень запущен. " +
                $"Вагонов до победы: {_wagonsInLevel.Count}");
        }

        public void ResetLevel()
        {
            UnsubscribeFromWagons();

            _wagonsInLevel.Clear();
            _departedWagonsCount = 0;
            _isInitialized = false;

            SetState(LevelState.NotInitialized);

            Debug.Log(
                "[LevelFlowDriver] Состояние уровня сброшено.");
        }

        public bool TryStartSwap()
        {
            if (!_isInitialized ||
                _currentState != LevelState.Playing)
            {
                return false;
            }

            SetState(LevelState.Swapping);

            return true;
        }

        public void FinishSwap()
        {
            if (!_isInitialized ||
                _currentState != LevelState.Swapping)
            {
                return;
            }

            SetState(LevelState.Playing);
        }

        private void SubscribeToWagons()
        {
            foreach (TrainWagon wagon in _wagonsInLevel)
            {
                if (wagon == null)
                {
                    continue;
                }

                wagon.WagonDeparted -= OnWagonDeparted;
                wagon.WagonDeparted += OnWagonDeparted;
            }
        }

        private void UnsubscribeFromWagons()
        {
            foreach (TrainWagon wagon in _wagonsInLevel)
            {
                if (wagon != null)
                {
                    wagon.WagonDeparted -= OnWagonDeparted;
                }
            }
        }

        private void CompleteLevel()
        {
            if (_currentState == LevelState.Completed)
            {
                return;
            }

            SetState(LevelState.Completed);

            Debug.Log(
                "[LevelFlowDriver] Уровень пройден. " +
                "Все вагоны отправлены.");

            LevelCompleted?.Invoke();
        }

        private void SetState(
            LevelState newState)
        {
            if (_currentState == newState)
            {
                return;
            }

            _currentState = newState;

            Debug.Log(
                $"[LevelFlowDriver] Состояние уровня: " +
                $"{_currentState}");

            StateChanged?.Invoke(_currentState);
        }

        private void OnWagonDeparted(
            TrainWagon wagon)
        {
            if (!_isInitialized ||
                _currentState == LevelState.Completed)
            {
                return;
            }

            _departedWagonsCount++;

            int remainingWagons =
                _wagonsInLevel.Count -
                _departedWagonsCount;

            Debug.Log(
                $"[LevelFlowDriver] Вагон уехал. " +
                $"Осталось: {remainingWagons}");

            if (_departedWagonsCount >=
                _wagonsInLevel.Count)
            {
                CompleteLevel();
            }
        }
    }
}