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
        [SerializeField] private List<ColorMaterialMapping> _materialsMap;

        private void Start()
        {
            BuildLevel();
        }

        private void BuildLevel()
        {
            if (!ValidateReferences())
            {
                return;
            }

            if (_levelData.Wagons.Count > _sceneWagons.Count)
            {
                Debug.LogError(
                    $"[LevelBuilder] В LevelData указано " +
                    $"{_levelData.Wagons.Count} вагонов, " +
                    $"но на сцене доступно только {_sceneWagons.Count}.");

                return;
            }

            SetActiveWagonsCount(_levelData.Wagons.Count);

            Passenger initialBufferPassenger = SpawnPassenger(
                _levelData.InitialBufferColor,
                _transitSlot.GetPosition().position,
                null);

            _swapEngine.InitializeBuffer(initialBufferPassenger);

            List<TrainWagon> wagonsInLevel =
                new List<TrainWagon>();

            for (int i = 0; i < _levelData.Wagons.Count; i++)
            {
                WagonSetup setup = _levelData.Wagons[i];
                TrainWagon wagon = _sceneWagons[i];

                if (setup == null)
                {
                    Debug.LogError(
                        $"[LevelBuilder] Настройка вагона {i} отсутствует.");

                    continue;
                }

                if (wagon == null)
                {
                    Debug.LogError(
                        $"[LevelBuilder] Вагон сцены {i} не назначен.");

                    continue;
                }

                if (!ValidateWagonSetup(setup, wagon, i))
                {
                    continue;
                }

                wagon.InitializeData(
                    setup.TargetColor,
                    setup.Capacity);

                for (int passengerIndex = 0;
                     passengerIndex < setup.StartingPassengers.Count;
                     passengerIndex++)
                {
                    Vector3 spawnPosition =
                        wagon.SeatPoints[passengerIndex].position;

                    Passenger passenger = SpawnPassenger(
                        setup.StartingPassengers[passengerIndex],
                        spawnPosition,
                        wagon);

                    if (!wagon.TryAddPassenger(passenger))
                    {
                        Debug.LogError(
                            $"[LevelBuilder] Не удалось добавить " +
                            $"пассажира {passengerIndex} " +
                            $"в вагон {i}.");
                    }
                }

                wagonsInLevel.Add(wagon);
            }

            _levelFlowDriver.Initialize(wagonsInLevel);
        }

        private bool ValidateReferences()
        {
            if (_levelData == null)
            {
                Debug.LogError(
                    "[LevelBuilder] LevelData не назначен.");

                return false;
            }

            if (_transitSlot == null)
            {
                Debug.LogError(
                    "[LevelBuilder] TransitSlot не назначен.");

                return false;
            }

            if (_swapEngine == null)
            {
                Debug.LogError(
                    "[LevelBuilder] SwapEngine не назначен.");

                return false;
            }

            if (_levelFlowDriver == null)
            {
                Debug.LogError(
                    "[LevelBuilder] LevelFlowDriver не назначен.");

                return false;
            }

            if (_passengerPrefab == null)
            {
                Debug.LogError(
                    "[LevelBuilder] Префаб пассажира не назначен.");

                return false;
            }

            if (_sceneWagons == null || _sceneWagons.Count == 0)
            {
                Debug.LogError(
                    "[LevelBuilder] Список вагонов сцены пуст.");

                return false;
            }

            return true;
        }

        private bool ValidateWagonSetup(
            WagonSetup setup,
            TrainWagon wagon,
            int wagonIndex)
        {
            if (setup.Capacity <= 0)
            {
                Debug.LogError(
                    $"[LevelBuilder] У вагона {wagonIndex} " +
                    "вместимость должна быть больше нуля.");

                return false;
            }

            if (wagon.SeatPoints == null)
            {
                Debug.LogError(
                    $"[LevelBuilder] У вагона {wagonIndex} " +
                    "не назначены точки мест.");

                return false;
            }

            if (setup.Capacity > wagon.SeatPoints.Length)
            {
                Debug.LogError(
                    $"[LevelBuilder] Вместимость вагона {wagonIndex} " +
                    $"равна {setup.Capacity}, но точек мест только " +
                    $"{wagon.SeatPoints.Length}.");

                return false;
            }

            if (setup.StartingPassengers.Count > setup.Capacity)
            {
                Debug.LogError(
                    $"[LevelBuilder] В вагоне {wagonIndex} " +
                    "начальных пассажиров больше, чем мест.");

                return false;
            }

            return true;
        }

        private void SetActiveWagonsCount(int activeWagonsCount)
        {
            for (int i = 0; i < _sceneWagons.Count; i++)
            {
                TrainWagon wagon = _sceneWagons[i];

                if (wagon != null)
                {
                    wagon.gameObject.SetActive(i < activeWagonsCount);
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
                newPassenger.transform.SetParent(wagon.transform);
            }

            Material material = GetMaterialByColor(color);
            newPassenger.SetData(color, material);

            return newPassenger;
        }

        private Material GetMaterialByColor(ColorType color)
        {
            foreach (ColorMaterialMapping mapping in _materialsMap)
            {
                if (mapping.Color == color)
                {
                    return mapping.Material;
                }
            }

            Debug.LogWarning(
                $"[LevelBuilder] Материал для цвета {color} не найден.");

            return null;
        }
    }
}