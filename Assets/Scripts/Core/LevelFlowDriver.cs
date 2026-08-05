using System;
using System.Collections.Generic;
using UnityEngine;
using StationJam.Entities;

namespace StationJam.Core
{
    public class LevelFlowDriver : MonoBehaviour
    {
        public event Action OnLevelCompleted;

        private readonly List<TrainWagon> _wagonsInLevel =
            new List<TrainWagon>();

        private int _departedWagonsCount;
        private bool _isInitialized;
        private bool _isLevelCompleted;

        public void Initialize(IReadOnlyList<TrainWagon> wagons)
        {
            UnsubscribeFromWagons();

            _wagonsInLevel.Clear();
            _departedWagonsCount = 0;
            _isInitialized = false;
            _isLevelCompleted = false;

            if (wagons == null)
            {
                Debug.LogError(
                    "[LevelFlowDriver] Получен пустой список вагонов.");

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

            Debug.Log(
                $"[LevelFlowDriver] Уровень инициализирован. " +
                $"Вагонов до победы: {_wagonsInLevel.Count}");
        }

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

        private void SubscribeToWagons()
        {
            foreach (TrainWagon wagon in _wagonsInLevel)
            {
                if (wagon == null)
                {
                    continue;
                }

                // Сначала отписываемся, чтобы исключить двойную подписку.
                wagon.OnWagonDeparted -= HandleWagonDeparted;
                wagon.OnWagonDeparted += HandleWagonDeparted;
            }
        }

        private void UnsubscribeFromWagons()
        {
            foreach (TrainWagon wagon in _wagonsInLevel)
            {
                if (wagon != null)
                {
                    wagon.OnWagonDeparted -= HandleWagonDeparted;
                }
            }
        }

        private void HandleWagonDeparted(TrainWagon wagon)
        {
            if (!_isInitialized || _isLevelCompleted)
            {
                return;
            }

            _departedWagonsCount++;

            int remainingWagons =
                _wagonsInLevel.Count - _departedWagonsCount;

            Debug.Log(
                $"[LevelFlowDriver] Вагон уехал. " +
                $"Осталось: {remainingWagons}");

            if (_departedWagonsCount >= _wagonsInLevel.Count)
            {
                CompleteLevel();
            }
        }

        private void CompleteLevel()
        {
            if (_isLevelCompleted)
            {
                return;
            }

            _isLevelCompleted = true;

            Debug.Log(
                "[LevelFlowDriver] Уровень пройден. " +
                "Все вагоны отправлены.");

            OnLevelCompleted?.Invoke();
        }
    }
}