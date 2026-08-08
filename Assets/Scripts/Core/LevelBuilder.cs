using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using StationJam.Entities;
using StationJam.Data;

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

        public bool BuildLevel(
            LevelData levelData)
        {
            if (!TryValidateLevel(
                    levelData,
                    out string validationError))
            {
                Debug.LogError(
                    $"[LevelBuilder] Уровень не создан: " +
                    $"{validationError}");

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
                new List<TrainWagon>();

            for (int wagonIndex = 0;
                 wagonIndex < levelData.Wagons.Count;
                 wagonIndex++)
            {
                WagonSetup setup =
                    levelData.Wagons[wagonIndex];

                TrainWagon wagon =
                    _sceneWagons[wagonIndex];

                wagon.InitializeData(
                    setup.TargetColor,
                    setup.Capacity);

                for (int passengerIndex = 0;
                     passengerIndex <
                     setup.StartingPassengers.Count;
                     passengerIndex++)
                {
                    Transform seatPoint =
                        wagon.SeatPoints[
                            passengerIndex];

                    Passenger passenger =
                        SpawnPassenger(
                            setup.StartingPassengers[
                                passengerIndex],
                            seatPoint.position,
                            wagon);

                    bool passengerAdded =
                        wagon.TryAddPassenger(
                            passenger);

                    if (!passengerAdded)
                    {
                        Debug.LogError(
                            $"[LevelBuilder] Не удалось " +
                            $"добавить пассажира " +
                            $"{passengerIndex} " +
                            $"в вагон {wagonIndex}.");
                    }
                }

                wagonsInLevel.Add(wagon);
            }

            _levelFlowDriver.Initialize(
                wagonsInLevel);

            Debug.Log(
                $"[LevelBuilder] Уровень " +
                $"{levelData.LevelNumber} " +
                "успешно создан.");

            return true;
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

        private bool TryValidateLevel(
            LevelData levelData,
            out string errorMessage)
        {
            if (!TryValidateReferences(
                    out errorMessage))
            {
                return false;
            }

            if (levelData == null)
            {
                errorMessage =
                    "Передан пустой LevelData.";

                return false;
            }

            if (!levelData.TryValidate(
                    out errorMessage))
            {
                return false;
            }

            if (levelData.Wagons.Count >
                _sceneWagons.Count)
            {
                errorMessage =
                    $"В LevelData указано " +
                    $"{levelData.Wagons.Count} вагонов, " +
                    $"но на сцене доступно только " +
                    $"{_sceneWagons.Count}.";

                return false;
            }

            if (!HasMaterialForColor(
                    levelData.InitialBufferColor))
            {
                errorMessage =
                    $"Не назначен материал для цвета " +
                    $"{levelData.InitialBufferColor}.";

                return false;
            }

            for (int wagonIndex = 0;
                 wagonIndex < levelData.Wagons.Count;
                 wagonIndex++)
            {
                WagonSetup setup =
                    levelData.Wagons[wagonIndex];

                TrainWagon wagon =
                    _sceneWagons[wagonIndex];

                if (wagon == null)
                {
                    errorMessage =
                        $"Вагон сцены {wagonIndex} " +
                        "не назначен.";

                    return false;
                }

                if (wagon.SeatPoints == null)
                {
                    errorMessage =
                        $"У вагона {wagonIndex} " +
                        "не назначен массив Seat Points.";

                    return false;
                }

                if (setup.Capacity >
                    wagon.SeatPoints.Length)
                {
                    errorMessage =
                        $"Вместимость вагона " +
                        $"{wagonIndex} равна " +
                        $"{setup.Capacity}, но точек " +
                        $"мест только " +
                        $"{wagon.SeatPoints.Length}.";

                    return false;
                }

                for (int seatIndex = 0;
                     seatIndex < setup.Capacity;
                     seatIndex++)
                {
                    if (wagon.SeatPoints[
                            seatIndex] == null)
                    {
                        errorMessage =
                            $"У вагона {wagonIndex} " +
                            $"не назначена точка места " +
                            $"{seatIndex}.";

                        return false;
                    }
                }

                if (!HasMaterialForColor(
                        setup.TargetColor))
                {
                    errorMessage =
                        $"Не назначен материал для " +
                        $"целевого цвета " +
                        $"{setup.TargetColor} " +
                        $"вагона {wagonIndex}.";

                    return false;
                }

                foreach (ColorType passengerColor
                         in setup.StartingPassengers)
                {
                    if (!HasMaterialForColor(
                            passengerColor))
                    {
                        errorMessage =
                            $"Не назначен материал для " +
                            $"цвета {passengerColor}, " +
                            $"используемого в вагоне " +
                            $"{wagonIndex}.";

                        return false;
                    }
                }
            }

            errorMessage = string.Empty;

            return true;
        }

        private bool TryValidateReferences(
            out string errorMessage)
        {
            if (_transitSlot == null)
            {
                errorMessage =
                    "TransitSlot не назначен.";

                return false;
            }

            if (_swapEngine == null)
            {
                errorMessage =
                    "SwapEngine не назначен.";

                return false;
            }

            if (_levelFlowDriver == null)
            {
                errorMessage =
                    "LevelFlowDriver не назначен.";

                return false;
            }

            if (_passengerPrefab == null)
            {
                errorMessage =
                    "Префаб пассажира не назначен.";

                return false;
            }

            if (_sceneWagons == null ||
                _sceneWagons.Count == 0)
            {
                errorMessage =
                    "Список вагонов сцены пуст.";

                return false;
            }

            if (_materialsMap == null ||
                _materialsMap.Count == 0)
            {
                errorMessage =
                    "Список материалов цветов пуст.";

                return false;
            }

            errorMessage = string.Empty;

            return true;
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
                }
                else
                {
                    wagon.DisableForLevel();
                }
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

        private bool HasMaterialForColor(
            ColorType color)
        {
            foreach (ColorMaterialMapping mapping
                     in _materialsMap)
            {
                if (mapping.Color == color &&
                    mapping.Material != null)
                {
                    return true;
                }
            }

            return false;
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
    }
}