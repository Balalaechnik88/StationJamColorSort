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

        private readonly List<Passenger> _spawnedPassengers =
            new List<Passenger>();

        private readonly List<TrainWagon> _spawnedWagons =
            new List<TrainWagon>();

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
                    levelData);

            if (!string.IsNullOrEmpty(
                    levelValidationError))
            {
                LogBuildError(
                    levelValidationError);

                return false;
            }

            string layoutValidationError =
                _levelLayout.GetConfigurationError(
                    levelData.Wagons.Count);

            if (!string.IsNullOrEmpty(
                    layoutValidationError))
            {
                LogBuildError(
                    layoutValidationError);

                return false;
            }

            string passengerFactoryValidationError =
                GetPassengerFactoryValidationError(
                    levelData);

            if (!string.IsNullOrEmpty(
                    passengerFactoryValidationError))
            {
                LogBuildError(
                    passengerFactoryValidationError);

                return false;
            }

            string wagonFactoryValidationError =
                GetWagonFactoryValidationError(
                    levelData);

            if (!string.IsNullOrEmpty(
                    wagonFactoryValidationError))
            {
                LogBuildError(
                    wagonFactoryValidationError);

                return false;
            }

            ClearBuiltLevel();

            bool buildCompleted =
                false;

            try
            {
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
                    BuildWagons(
                        levelData);

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
                    levelData.Wagons[
                        wagonIndex];

                WagonLayoutSlot layoutSlot =
                    _levelLayout.GetSlot(
                        wagonIndex);

                if (layoutSlot == null ||
                    layoutSlot.SpawnPoint == null)
                {
                    Debug.LogError(
                        $"[LevelBuilder] Layout-слот " +
                        $"{wagonIndex} недоступен.");

                    return null;
                }

                TrainWagon wagon =
                    CreateWagon(
                        setup,
                        layoutSlot);

                if (wagon == null)
                {
                    return null;
                }

                bool wagonFilled =
                    FillWagon(
                        wagon,
                        setup);

                if (!wagonFilled)
                {
                    return null;
                }

                wagonsInLevel.Add(
                    wagon);
            }

            return
                wagonsInLevel;
        }

        private TrainWagon CreateWagon(
            WagonSetup setup,
            WagonLayoutSlot layoutSlot)
        {
            TrainWagon wagon =
                _wagonFactory.Create(
                    setup.TargetColor,
                    setup.Capacity,
                    layoutSlot.DepartureDirection,
                    layoutSlot.SpawnPoint.position,
                    layoutSlot.SpawnPoint.rotation,
                    _levelLayout.transform);

            if (wagon == null)
            {
                Debug.LogError(
                    "[LevelBuilder] WagonFactory " +
                    "не смогла создать вагон.");

                return null;
            }

            _spawnedWagons.Add(
                wagon);

            return
                wagon;
        }

        private bool FillWagon(
            TrainWagon wagon,
            WagonSetup setup)
        {
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

            return
                true;
        }

        private bool CreatePassengerInWagon(
            TrainWagon wagon,
            ColorType passengerColor,
            int seatIndex)
        {
            Transform seatPoint =
                wagon.SeatPoints[
                    seatIndex];

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

            return
                false;
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

            return
                passenger;
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

        private string GetPassengerFactoryValidationError(
            LevelData levelData)
        {
            string bufferColorError =
                _passengerFactory
                    .GetColorConfigurationError(
                        levelData.InitialBufferColor);

            if (!string.IsNullOrEmpty(
                    bufferColorError))
            {
                return
                    bufferColorError;
            }

            foreach (WagonSetup setup
                     in levelData.Wagons)
            {
                foreach (ColorType passengerColor
                         in setup.StartingPassengers)
                {
                    string colorError =
                        _passengerFactory
                            .GetColorConfigurationError(
                                passengerColor);

                    if (!string.IsNullOrEmpty(
                            colorError))
                    {
                        return
                            colorError;
                    }
                }
            }

            return
                string.Empty;
        }

        private string GetWagonFactoryValidationError(
            LevelData levelData)
        {
            for (int wagonIndex = 0;
                 wagonIndex < levelData.Wagons.Count;
                 wagonIndex++)
            {
                WagonSetup setup =
                    levelData.Wagons[
                        wagonIndex];

                string wagonError =
                    _wagonFactory
                        .GetWagonConfigurationError(
                            setup.TargetColor,
                            setup.Capacity);

                if (string.IsNullOrEmpty(
                        wagonError))
                {
                    continue;
                }

                return
                    $"¬агон {wagonIndex}: " +
                    $"{wagonError}";
            }

            return
                string.Empty;
        }

        private void ClearBuiltLevel()
        {
            _levelFlowDriver.ResetLevel();

            _swapEngine.InitializeBuffer(
                null);

            ClearSpawnedPassengers();
            ClearSpawnedWagons();
        }

        private void RollbackFailedBuild()
        {
            Debug.LogWarning(
                "[LevelBuilder] ѕостроение уровн€ " +
                "не завершено. ¬ыполн€етс€ откат.");

            _levelFlowDriver.ResetLevel();

            _swapEngine.InitializeBuffer(
                null);

            ClearSpawnedPassengers();
            ClearSpawnedWagons();

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

        private void ClearSpawnedWagons()
        {
            foreach (TrainWagon wagon
                     in _spawnedWagons)
            {
                if (wagon == null)
                {
                    continue;
                }

                wagon.DisableForLevel();

                Destroy(
                    wagon.gameObject);
            }

            _spawnedWagons.Clear();
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

            if (_levelLayout == null)
            {
                return
                    "LevelLayout не назначен.";
            }

            if (_passengerFactory == null)
            {
                return
                    "PassengerFactory не назначен.";
            }

            if (_wagonFactory == null)
            {
                return
                    "WagonFactory не назначен.";
            }

            string passengerFactoryError =
                _passengerFactory
                    .GetConfigurationError();

            if (!string.IsNullOrEmpty(
                    passengerFactoryError))
            {
                return
                    passengerFactoryError;
            }

            string wagonFactoryError =
                _wagonFactory
                    .GetConfigurationError();

            if (!string.IsNullOrEmpty(
                    wagonFactoryError))
            {
                return
                    wagonFactoryError;
            }

            return
                string.Empty;
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