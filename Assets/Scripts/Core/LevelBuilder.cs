using System;
using System.Collections.Generic;
using DG.Tweening;
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
        private List<TrainWagon> _sceneWagons;

        [SerializeField]
        private SwapEngine _swapEngine;

        [SerializeField]
        private LevelFlowDriver _levelFlowDriver;

        [SerializeField]
        private PassengerFactory _passengerFactory;

        private readonly List<Passenger> _spawnedPassengers =
            new List<Passenger>();

        private readonly LevelValidator _levelValidator =
            new LevelValidator();

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

            string levelValidationError =
                _levelValidator.GetValidationError(
                    levelData,
                    _sceneWagons,
                    _passengerFactory.MaterialsMap);

            if (!string.IsNullOrEmpty(
                    levelValidationError))
            {
                LogBuildError(
                    levelValidationError);

                return false;
            }

            ClearBuiltLevel();

            bool buildCompleted =
                false;

            try
            {
                SetActiveWagonsCount(
                    levelData.Wagons.Count);

                Passenger initialBufferPassenger =
                    CreatePassenger(
                        levelData.InitialBufferColor,
                        _transitSlot
                            .GetPosition()
                            .position);

                if (initialBufferPassenger == null)
                {
                    LogBuildError(
                        "Ќе удалось создать пассажира " +
                        "дл€ транзитного слота.");

                    return false;
                }

                _swapEngine.InitializeBuffer(
                    initialBufferPassenger);

                List<TrainWagon> wagonsInLevel =
                    BuildWagons(levelData);

                if (wagonsInLevel == null)
                {
                    LogBuildError(
                        "Ќе удалось полностью построить " +
                        "вагоны уровн€.");

                    return false;
                }

                _levelFlowDriver.Initialize(
                    wagonsInLevel);

                CheckInitialWagonsCompletion(
                    wagonsInLevel);

                buildCompleted =
                    true;

                Debug.Log(
                    $"[LevelBuilder] ”ровень " +
                    $"{levelData.LevelNumber} " +
                    "успешно создан.");

                return true;
            }
            catch (Exception exception)
            {
                Debug.LogException(
                    exception);

                LogBuildError(
                    "¬о врем€ построени€ уровн€ " +
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

        private List<TrainWagon> BuildWagons(
            LevelData levelData)
        {
            List<TrainWagon> wagonsInLevel =
                new List<TrainWagon>();

            for (int wagonIndex = 0;
                 wagonIndex < levelData.Wagons.Count;
                 wagonIndex++)
            {
                WagonSetup setup =
                    levelData.Wagons[wagonIndex];

                TrainWagon wagon =
                    _sceneWagons[wagonIndex];

                bool wagonBuilt =
                    BuildWagon(
                        wagon,
                        setup);

                if (!wagonBuilt)
                {
                    return null;
                }

                wagonsInLevel.Add(
                    wagon);
            }

            return wagonsInLevel;
        }

        private bool BuildWagon(
            TrainWagon wagon,
            WagonSetup setup)
        {
            wagon.InitializeData(
                setup.TargetColor,
                setup.Capacity);

            for (int passengerIndex = 0;
                 passengerIndex <
                 setup.StartingPassengers.Count;
                 passengerIndex++)
            {
                bool passengerAdded =
                    CreatePassengerInWagon(
                        wagon,
                        setup.StartingPassengers[
                            passengerIndex],
                        passengerIndex);

                if (!passengerAdded)
                {
                    return false;
                }
            }

            return true;
        }

        private bool CreatePassengerInWagon(
            TrainWagon wagon,
            ColorType passengerColor,
            int seatIndex)
        {
            Transform seatPoint =
                wagon.SeatPoints[seatIndex];

            Passenger passenger =
                CreatePassenger(
                    passengerColor,
                    seatPoint.position);

            if (passenger == null)
            {
                Debug.LogError(
                    $"[LevelBuilder] Ќе удалось создать " +
                    $"пассажира дл€ места {seatIndex} " +
                    $"вагона {wagon.gameObject.name}.");

                return false;
            }

            bool passengerAdded =
                wagon.TryAddPassenger(
                    passenger);

            if (passengerAdded)
            {
                return true;
            }

            Debug.LogError(
                $"[LevelBuilder] Ќе удалось добавить " +
                $"пассажира на место {seatIndex} " +
                $"вагона {wagon.gameObject.name}.");

            return false;
        }

        private Passenger CreatePassenger(
            ColorType color,
            Vector3 position)
        {
            Passenger passenger =
                _passengerFactory.Create(
                    color,
                    position);

            if (passenger != null)
            {
                _spawnedPassengers.Add(
                    passenger);
            }

            return passenger;
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

        private void ClearBuiltLevel()
        {
            _levelFlowDriver.ResetLevel();

            _swapEngine.InitializeBuffer(null);

            ClearSpawnedPassengers();

            DisableSceneWagons();
        }

        private void RollbackFailedBuild()
        {
            Debug.LogWarning(
                "[LevelBuilder] ѕостроение уровн€ " +
                "не завершено. ¬ыполн€етс€ откат.");

            _levelFlowDriver.ResetLevel();

            _swapEngine.InitializeBuffer(null);

            ClearSpawnedPassengers();

            DisableSceneWagons();

            Debug.LogWarning(
                "[LevelBuilder] „астично построенный " +
                "уровень полностью очищен.");
        }

        private void ClearSpawnedPassengers()
        {
            foreach (Passenger passenger
                     in _spawnedPassengers)
            {
                if (passenger == null)
                {
                    continue;
                }

                passenger.transform.DOKill();

                passenger.gameObject.SetActive(
                    false);

                Destroy(
                    passenger.gameObject);
            }

            _spawnedPassengers.Clear();
        }

        private void DisableSceneWagons()
        {
            foreach (TrainWagon wagon
                     in _sceneWagons)
            {
                if (wagon != null)
                {
                    wagon.DisableForLevel();
                }
            }
        }

        private void SetActiveWagonsCount(
            int activeWagonsCount)
        {
            for (int wagonIndex = 0;
                 wagonIndex < _sceneWagons.Count;
                 wagonIndex++)
            {
                TrainWagon wagon =
                    _sceneWagons[wagonIndex];

                if (wagon == null)
                {
                    continue;
                }

                if (wagonIndex <
                    activeWagonsCount)
                {
                    wagon.gameObject.SetActive(
                        true);

                    continue;
                }

                wagon.DisableForLevel();
            }
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

            if (_passengerFactory == null)
            {
                return
                    "PassengerFactory не назначен.";
            }

            string factoryConfigurationError =
                _passengerFactory
                    .GetConfigurationError();

            if (!string.IsNullOrEmpty(
                    factoryConfigurationError))
            {
                return
                    factoryConfigurationError;
            }

            return string.Empty;
        }

        private void LogBuildError(
            string validationError)
        {
            Debug.LogError(
                $"[LevelBuilder] ”ровень не создан: " +
                $"{validationError}");
        }
    }
}