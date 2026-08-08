using System;
using System.Collections.Generic;
using UnityEngine;
using StationJam.Data;

namespace StationJam.Core
{
    public class LevelSequence : MonoBehaviour
    {
        [Header("References")]
        [SerializeField]
        private LevelBuilder _levelBuilder;

        [Header("Levels")]
        [SerializeField]
        private List<LevelData> _levels =
            new List<LevelData>();

        [SerializeField]
        [Min(0)]
        private int _startLevelIndex;

        private int _currentLevelIndex = -1;

        public event Action<int, LevelData> LevelStarted;

        public int CurrentLevelIndex =>
            _currentLevelIndex;

        public bool HasNextLevel =>
            _currentLevelIndex >= 0 &&
            _currentLevelIndex + 1 < _levels.Count;

        private void Start()
        {
            LoadLevel(_startLevelIndex);
        }

        public void LoadNextLevel()
        {
            if (!HasNextLevel)
            {
                Debug.Log(
                    "[LevelSequence] Все доступные " +
                    "уровни пройдены.");

                return;
            }

            LoadLevel(
                _currentLevelIndex + 1);
        }

        public void RestartCurrentLevel()
        {
            if (_currentLevelIndex < 0)
            {
                Debug.LogWarning(
                    "[LevelSequence] Текущий уровень " +
                    "ещё не запущен.");

                return;
            }

            LoadLevel(
                _currentLevelIndex);
        }

        private void LoadLevel(
            int levelIndex)
        {
            if (_levelBuilder == null)
            {
                Debug.LogError(
                    "[LevelSequence] LevelBuilder " +
                    "не назначен.");

                return;
            }

            if (_levels == null ||
                _levels.Count == 0)
            {
                Debug.LogError(
                    "[LevelSequence] Список уровней пуст.");

                return;
            }

            if (levelIndex < 0 ||
                levelIndex >= _levels.Count)
            {
                Debug.LogError(
                    $"[LevelSequence] Индекс уровня " +
                    $"{levelIndex} находится за пределами " +
                    "списка.");

                return;
            }

            LevelData levelData =
                _levels[levelIndex];

            if (levelData == null)
            {
                Debug.LogError(
                    $"[LevelSequence] LevelData с индексом " +
                    $"{levelIndex} не назначен.");

                return;
            }

            bool levelBuilt =
                _levelBuilder.BuildLevel(
                    levelData);

            if (!levelBuilt)
            {
                return;
            }

            _currentLevelIndex =
                levelIndex;

            int displayedLevelIndex =
                _currentLevelIndex + 1;

            Debug.Log(
                $"[LevelSequence] Запущен уровень " +
                $"{levelData.LevelNumber}. " +
                $"Позиция в списке: " +
                $"{displayedLevelIndex}/" +
                $"{_levels.Count}.");

            LevelStarted?.Invoke(
                _currentLevelIndex,
                levelData);
        }
    }
}