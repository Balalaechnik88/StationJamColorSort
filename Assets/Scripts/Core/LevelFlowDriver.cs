using System;
using System.Collections.Generic;
using UnityEngine;
using StationJam.Entities;

namespace StationJam.Core
{
    public class LevelFlowDriver : MonoBehaviour
    {
        [Header("Level Settings")]
        [SerializeField] private List<TrainWagon> _wagonsInLevel = new List<TrainWagon>();

        // Публикуем событие для UI и других систем
        public event Action OnLevelCompleted;

        private int _departedWagonsCount = 0;
        private int _totalWagons;

        private void Start()
        {
            _totalWagons = _wagonsInLevel.Count;
            Debug.Log($"[LevelFlowDriver] Уровень запущен. Вагонов до победы: {_totalWagons}");

            if (_totalWagons == 0)
            {
                Debug.LogWarning("[LevelFlowDriver] ВНИМАНИЕ: Список _wagonsInLevel пуст! Добавь вагоны в Инспекторе.");
            }
        }

        private void OnEnable()
        {
            // Симметричная подписка на события отъезда
            foreach (var wagon in _wagonsInLevel)
            {
                if (wagon != null)
                {
                    wagon.OnWagonDeparted += HandleWagonDeparted;
                }
            }
        }

        private void OnDisable()
        {
            // Симметричная отписка
            foreach (var wagon in _wagonsInLevel)
            {
                if (wagon != null)
                {
                    wagon.OnWagonDeparted -= HandleWagonDeparted;
                }
            }
        }

        private void HandleWagonDeparted(TrainWagon wagon)
        {
            _departedWagonsCount++;
            Debug.Log($"[LevelFlowDriver] Вагон уехал. Осталось: {_totalWagons - _departedWagonsCount}");

            if (_departedWagonsCount >= _totalWagons)
            {
                CompleteLevel();
            }
        }

        private void CompleteLevel()
        {
            Debug.Log("[LevelFlowDriver] Уровень пройден! Все вагоны отправлены.");
            OnLevelCompleted?.Invoke();
        }
    }
}