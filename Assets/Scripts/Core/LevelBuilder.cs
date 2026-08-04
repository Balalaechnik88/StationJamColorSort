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
        [SerializeField] private SwapEngine _swapEngine; // Добавлена ссылка на ядро обмена

        [Header("Prefabs & Visuals")]
        [SerializeField] private Passenger _passengerPrefab;
        [SerializeField] private List<ColorMaterialMapping> _materialsMap;

        private void Start()
        {
            if (_levelData != null)
            {
                BuildLevel();
            }
            else
            {
                Debug.LogError("[LevelBuilder] Файл LevelData не назначен!");
            }
        }

        private void BuildLevel()
        {
            // 1. Спавним пассажира на перроне
            Passenger initialBufferPass = SpawnPassenger(_levelData.InitialBufferColor, _transitSlot.GetPosition().position, null);

            // ПЕРЕДАЕМ ПАССАЖИРА В ДВИЖОК
            if (_swapEngine != null)
            {
                _swapEngine.InitializeBuffer(initialBufferPass);
            }

            // 2. Настраиваем и заполняем вагоны
            for (int i = 0; i < _levelData.Wagons.Count; i++)
            {
                if (i >= _sceneWagons.Count) break;

                WagonSetup setup = _levelData.Wagons[i];
                TrainWagon wagon = _sceneWagons[i];

                wagon.InitializeData(setup.TargetColor, setup.Capacity);

                for (int j = 0; j < setup.StartingPassengers.Count; j++)
                {
                    if (j >= wagon.SeatPoints.Length) break;

                    Vector3 spawnPos = wagon.SeatPoints[j].position;
                    Passenger newPassenger = SpawnPassenger(setup.StartingPassengers[j], spawnPos, wagon);
                    wagon.TryAddPassenger(newPassenger);
                }
            }
        }

        private Passenger SpawnPassenger(ColorType color, Vector3 position, TrainWagon wagon)
        {
            Passenger newPass = Instantiate(_passengerPrefab, position, Quaternion.identity);

            if (wagon != null)
            {
                newPass.transform.SetParent(wagon.transform);
            }

            Material mat = GetMaterialByColor(color);
            newPass.SetData(color, mat);

            return newPass;
        }

        private Material GetMaterialByColor(ColorType color)
        {
            foreach (var mapping in _materialsMap)
            {
                if (mapping.Color == color) return mapping.Material;
            }
            return null;
        }
    }
}