using System.Collections.Generic;
using UnityEngine;
using StationJam.Entities;
using StationJam.Data;

namespace StationJam.Core
{
    [System.Serializable]
    public struct ColorMaterialMapping
    {
        public ColorType Color;
        public Material Material;
    }

    public class LevelBuilder : MonoBehaviour
    {
        [Header("Level Data")]
        [SerializeField] private LevelData _levelData;

        [Header("Scene References")]
        [SerializeField] private TransitSlot _transitSlot;
        [SerializeField] private List<TrainWagon> _sceneWagons;
        [SerializeField] private SwapEngine _swapEngine;
        [SerializeField] private LevelFlowDriver _levelFlowDriver;

        [Header("Prefabs & Visuals")]
        [SerializeField] private Passenger _passengerPrefab;
        [SerializeField]
        private List<ColorMaterialMapping> _materialsMap;

        private void Start()
        {
            BuildLevel();
        }

        private void BuildLevel()
        {
            if (!TryValidateLevel(out string validationError))
            {
                Debug.LogError(
                    $"[LevelBuilder] Уровень не создан: " +
                    $"{validationError}");

                return;
            }

            SetActiveWagonsCount(_levelData.Wagons.Count);

            Passenger initialBufferPassenger = SpawnPassenger(
                _levelData.InitialBufferColor,
                _transitSlot.GetPosition().position,
                null);

            _swapEngine.InitializeBuffer(
                initialBufferPassenger);

            var wagonsInLevel =
                new List<TrainWagon>();

            for (int wagonIndex = 0;
                 wagonIndex < _levelData.Wagons.Count;
                 wagonIndex++)
            {
                WagonSetup setup =
                    _levelData.Wagons[wagonIndex];

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
                        wagon.SeatPoints[passengerIndex];

                    Passenger passenger = SpawnPassenger(
                        setup.StartingPassengers[passengerIndex],
                        seatPoint.position,
                        wagon);

                    bool passengerAdded =
                        wagon.TryAddPassenger(passenger);

                    if (!passengerAdded)
                    {
                        Debug.LogError(
                            $"[LevelBuilder] Не удалось добавить " +
                            $"пассажира {passengerIndex} " +
                            $"в вагон {wagonIndex}.");
                    }
                }

                wagonsInLevel.Add(wagon);
            }

            _levelFlowDriver.Initialize(wagonsInLevel);

            Debug.Log(
                $"[LevelBuilder] Уровень " +
                $"{_levelData.LevelNumber} успешно создан.");
        }

        private bool TryValidateLevel(
            out string errorMessage)
        {
            if (!TryValidateReferences(out errorMessage))
            {
                return false;
            }

            if (!_levelData.TryValidate(out errorMessage))
            {
                return false;
            }

            if (_levelData.Wagons.Count >
                _sceneWagons.Count)
            {
                errorMessage =
                    $"В LevelData указано " +
                    $"{_levelData.Wagons.Count} вагонов, " +
                    $"но на сцене доступно только " +
                    $"{_sceneWagons.Count}.";

                return false;
            }

            if (!HasMaterialForColor(
                    _levelData.InitialBufferColor))
            {
                errorMessage =
                    $"Не назначен материал для цвета " +
                    $"{_levelData.InitialBufferColor}.";

                return false;
            }

            for (int wagonIndex = 0;
                 wagonIndex < _levelData.Wagons.Count;
                 wagonIndex++)
            {
                WagonSetup setup =
                    _levelData.Wagons[wagonIndex];

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
                        $"Вместимость вагона {wagonIndex} " +
                        $"равна {setup.Capacity}, но точек мест " +
                        $"только {wagon.SeatPoints.Length}.";

                    return false;
                }

                for (int seatIndex = 0;
                     seatIndex < setup.Capacity;
                     seatIndex++)
                {
                    if (wagon.SeatPoints[seatIndex] == null)
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
                        $"Не назначен материал для целевого " +
                        $"цвета {setup.TargetColor} " +
                        $"вагона {wagonIndex}.";

                    return false;
                }

                foreach (ColorType passengerColor
                         in setup.StartingPassengers)
                {
                    if (!HasMaterialForColor(passengerColor))
                    {
                        errorMessage =
                            $"Не назначен материал для цвета " +
                            $"{passengerColor}, используемого " +
                            $"в вагоне {wagonIndex}.";

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
            if (_levelData == null)
            {
                errorMessage = "LevelData не назначен.";
                return false;
            }

            if (_transitSlot == null)
            {
                errorMessage = "TransitSlot не назначен.";
                return false;
            }

            if (_swapEngine == null)
            {
                errorMessage = "SwapEngine не назначен.";
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

                if (wagon != null)
                {
                    wagon.gameObject.SetActive(
                        wagonIndex < activeWagonsCount);
                }
            }
        }

        private Passenger SpawnPassenger(
            ColorType color,
            Vector3 position,
            TrainWagon wagon)
        {
            Passenger newPassenger = Instantiate(
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

            newPassenger.SetData(color, material);

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