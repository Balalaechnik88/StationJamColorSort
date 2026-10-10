using System;
using UnityEngine;
using StationJam.Data;

namespace StationJam.Core
{
    public class LevelSequence : MonoBehaviour
    {
        public event Action<int> LevelStarted;

        [Header("References")]
        [SerializeField]
        private LevelBuilder _levelBuilder;

        [Header("Level Catalog")]
        [SerializeField]
        private LevelCatalog _levelCatalog;

        [SerializeField]
        [Min(0)]
        private int _startLevelIndex;

        private readonly LevelCatalogValidator _catalogValidator =
            new LevelCatalogValidator();

        private int _currentLevelIndex =
            -1;

        public int CurrentLevelIndex =>
            _currentLevelIndex;

        public bool HasNextLevel =>
            _levelCatalog != null &&
            _currentLevelIndex >= 0 &&
            _levelCatalog.HasLevel(
                _currentLevelIndex + 1);

        private void Start()
        {
            string configurationError =
                GetConfigurationError();

            if (!string.IsNullOrEmpty(
                    configurationError))
            {
                Debug.LogError(
                    $"[LevelSequence] ѕоследовательность " +
                    $"уровней не запущена: " +
                    $"{configurationError}");

                return;
            }

            LoadLevel(
                _startLevelIndex);
        }

        public void LoadNextLevel()
        {
            if (!HasNextLevel)
            {
                Debug.LogWarning(
                    "[LevelSequence] —ледующий уровень " +
                    "недоступен.");

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
                    "[LevelSequence] Ќельз€ перезапустить " +
                    "уровень: текущий уровень ещЄ " +
                    "не был запущен.");

                return;
            }

            LoadLevel(
                _currentLevelIndex);
        }

        private void LoadLevel(
            int levelIndex)
        {
            if (_levelCatalog == null)
            {
                Debug.LogError(
                    "[LevelSequence] LevelCatalog " +
                    "не назначен.");

                return;
            }

            if (!_levelCatalog.HasLevel(
                    levelIndex))
            {
                Debug.LogError(
                    $"[LevelSequence] ”ровень с индексом " +
                    $"{levelIndex} отсутствует в каталоге.");

                return;
            }

            LevelData levelData =
                _levelCatalog.GetLevel(
                    levelIndex);

            if (levelData == null)
            {
                Debug.LogError(
                    $"[LevelSequence] LevelData с индексом " +
                    $"{levelIndex} отсутствует.");

                return;
            }

            bool levelBuilt =
                _levelBuilder.BuildLevel(
                    levelData);

            if (!levelBuilt)
            {
                Debug.LogError(
                    $"[LevelSequence] Ќе удалось запустить " +
                    $"уровень {levelData.LevelNumber}.");

                return;
            }

            _currentLevelIndex =
                levelIndex;

            LevelStarted?.Invoke(
                levelData.LevelNumber);

            Debug.Log(
                $"[LevelSequence] «апущен уровень " +
                $"{levelData.LevelNumber}. " +
                $"ѕозици€ в каталоге: " +
                $"{_currentLevelIndex + 1}/" +
                $"{_levelCatalog.Count}.");
        }

        private string GetConfigurationError()
        {
            if (_levelBuilder == null)
            {
                return
                    "LevelBuilder не назначен.";
            }

            if (_levelCatalog == null)
            {
                return
                    "LevelCatalog не назначен.";
            }

            string catalogValidationError =
                _catalogValidator.GetValidationError(
                    _levelCatalog);

            if (!string.IsNullOrEmpty(
                    catalogValidationError))
            {
                return
                    catalogValidationError;
            }

            if (!_levelCatalog.HasLevel(
                    _startLevelIndex))
            {
                return
                    $"—тартовый индекс " +
                    $"{_startLevelIndex} отсутствует " +
                    "в LevelCatalog.";
            }

            return
                string.Empty;
        }
    }
}