using System;
using System.Collections.Generic;
using UnityEngine;
using StationJam.Data;
using StationJam.Entities;
using StationJam.Factories;

namespace StationJam.Core
{
    public class LevelBuilder : MonoBehaviour
    {
        [Header("Scene References")]
        [SerializeField]
        private TransitSlot _transitSlot;

        [SerializeField]
        private SwapEngine _swapEngine;

        [SerializeField]
        private LevelFlowDriver _levelFlowDriver;

        [SerializeField]
        private LevelLayout _levelLayout;

        [Header("Factories")]
        [SerializeField]
        private PassengerFactory _passengerFactory;

        [SerializeField]
        private WagonFactory _wagonFactory;

        private readonly LevelBuildPreflight _preflight =
            new LevelBuildPreflight();

        private readonly LevelRuntimeSession _runtimeSession =
            new LevelRuntimeSession();

        public bool BuildLevel(
            LevelData levelData)
        {
            string referencesValidationError =
                GetReferencesValidationError();

            if (!string.IsNullOrEmpty(
                    referencesValidationError))
            {
                LogBuildError(
                    referencesValidationError);

                return false;
            }

            string preflightError =
                _preflight.GetValidationError(
                    levelData,
                    _levelLayout,
                    _passengerFactory,
                    _wagonFactory);

            if (!string.IsNullOrEmpty(
                    preflightError))
            {
                LogBuildError(
                    preflightError);

                return false;
            }

            _runtimeSession.Clear(
                _levelFlowDriver,
                _swapEngine);

            bool buildCompleted =
                false;

            try
            {
                LevelSpawner levelSpawner =
                    new LevelSpawner(
                        _levelLayout,
                        _passengerFactory,
                        _wagonFactory,
                        _runtimeSession);

                bool levelSpawned =
                    levelSpawner.TrySpawn(
                        levelData,
                        _transitSlot
                            .GetPosition()
                            .position,
                        out LevelSpawnResult spawnResult);

                if (!levelSpawned ||
                    spawnResult == null)
                {
                    LogBuildError(
                        "Не удалось создать runtime " +
                        "объекты уровня.");

                    return false;
                }

                _swapEngine.InitializeBuffer(
                    spawnResult.BufferPassenger);

                _levelFlowDriver.Initialize(
                    spawnResult.Wagons);

                CheckInitialWagonsCompletion(
                    spawnResult.Wagons);

                buildCompleted =
                    true;

                Debug.Log(
                    $"[LevelBuilder] Уровень " +
                    $"{levelData.LevelNumber} " +
                    "успешно создан.");

                return true;
            }
            catch (Exception exception)
            {
                Debug.LogException(
                    exception);

                LogBuildError(
                    "Во время построения уровня " +
                    "возникло непредвиденное исключение.");

                return false;
            }
            finally
            {
                if (!buildCompleted)
                {
                    RollbackFailedBuild();
                }
            }
        }

        private void CheckInitialWagonsCompletion(
            IReadOnlyList<TrainWagon> wagons)
        {
            foreach (TrainWagon wagon in wagons)
            {
                if (wagon != null)
                {
                    wagon.CheckCompletion();
                }
            }
        }

        private void RollbackFailedBuild()
        {
            Debug.LogWarning(
                "[LevelBuilder] Построение уровня " +
                "не завершено. Выполняется откат.");

            _runtimeSession.Clear(
                _levelFlowDriver,
                _swapEngine);

            Debug.LogWarning(
                "[LevelBuilder] Частично построенный " +
                "уровень полностью очищен.");
        }

        private string GetReferencesValidationError()
        {
            if (_transitSlot == null)
            {
                return
                    "TransitSlot не назначен.";
            }

            if (_swapEngine == null)
            {
                return
                    "SwapEngine не назначен.";
            }

            if (_levelFlowDriver == null)
            {
                return
                    "LevelFlowDriver не назначен.";
            }

            return
                string.Empty;
        }

        private void LogBuildError(
            string validationError)
        {
            Debug.LogError(
                $"[LevelBuilder] Уровень не создан: " +
                $"{validationError}");
        }
    }
}