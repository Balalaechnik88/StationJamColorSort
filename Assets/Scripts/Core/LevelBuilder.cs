using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using StationJam.Data;
using StationJam.Entities;

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

        [Header("Prefabs & Visuals")]
        [SerializeField]
        private Passenger _passengerPrefab;

        [SerializeField]
        private List<ColorMaterialMapping> _materialsMap;

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
                    _materialsMap);

            if (!string.IsNullOrEmpty(
                    levelValidationError))
            {
                LogBuildError(
                    levelValidationError);

                return false;
            }

            ClearBuiltLevel();

            SetActiveWagonsCount(
                levelData.Wagons.Count);

            Passenger initialBufferPassenger =
                SpawnPassenger(
                    levelData.InitialBufferColor,
                    _transitSlot
                        .GetPosition()
                        .position,
                    null);

            _swapEngine.InitializeBuffer(
                initialBufferPassenger);

            List<TrainWagon> wagonsInLevel =
                BuildWagons(levelData);

            _levelFlowDriver.Initialize(
                wagonsInLevel);

            Debug.Log(
                $"[LevelBuilder] Уровень " +
                $"{levelData.LevelNumber} " +
                "успешно создан.");

            return true;
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

                BuildWagon(
                    wagon,
                    setup);

                wagonsInLevel.Add(
                    wagon);
            }

            return wagonsInLevel;
        }

        private void BuildWagon(
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
                SpawnPassengerInWagon(
                    wagon,
                    setup.StartingPassengers[
                        passengerIndex],
                    passengerIndex);
            }
        }

        private void SpawnPassengerInWagon(
            TrainWagon wagon,
            ColorType passengerColor,
            int seatIndex)
        {
            Transform seatPoint =
                wagon.SeatPoints[seatIndex];

            Passenger passenger =
                SpawnPassenger(
                    passengerColor,
                    seatPoint.position,
                    wagon);

            bool passengerAdded =
                wagon.TryAddPassenger(
                    passenger);

            if (passengerAdded)
            {
                return;
            }

            Debug.LogError(
                $"[LevelBuilder] Не удалось добавить " +
                $"пассажира на место {seatIndex} " +
                $"вагона {wagon.gameObject.name}.");
        }

        private void ClearBuiltLevel()
        {
            _levelFlowDriver.ResetLevel();

            _swapEngine.InitializeBuffer(null);

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

                Destroy(passenger.gameObject);
            }

            _spawnedPassengers.Clear();

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

        private Passenger SpawnPassenger(
            ColorType color,
            Vector3 position,
            TrainWagon wagon)
        {
            Passenger newPassenger =
                Instantiate(
                    _passengerPrefab,
                    position,
                    Quaternion.identity);

            if (wagon != null)
            {
                newPassenger.transform.SetParent(
                    wagon.transform);
            }

            Material material =
                GetMaterialByColor(color);

            newPassenger.SetData(
                color,
                material);

            _spawnedPassengers.Add(
                newPassenger);

            return newPassenger;
        }

        private Material GetMaterialByColor(
            ColorType color)
        {
            foreach (ColorMaterialMapping mapping
                     in _materialsMap)
            {
                if (mapping.Color == color)
                {
                    return mapping.Material;
                }
            }

            return null;
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

            if (_passengerPrefab == null)
            {
                return
                    "Префаб пассажира не назначен.";
            }

            return string.Empty;
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