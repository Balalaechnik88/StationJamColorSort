using System.Collections.Generic;
using UnityEngine;
using StationJam.Data;
using StationJam.Entities;

namespace StationJam.Core
{
    public sealed class LevelValidator
    {
        public string GetValidationError(
            LevelData levelData,
            IReadOnlyList<TrainWagon> sceneWagons,
            IReadOnlyList<ColorMaterialMapping> materialsMap)
        {
            if (levelData == null)
            {
                return "Передан пустой LevelData.";
            }

            if (sceneWagons == null ||
                sceneWagons.Count == 0)
            {
                return "Список вагонов сцены пуст.";
            }

            if (materialsMap == null ||
                materialsMap.Count == 0)
            {
                return "Список материалов цветов пуст.";
            }

            if (levelData.Wagons == null ||
                levelData.Wagons.Count == 0)
            {
                return
                    "В конфигурации уровня отсутствуют вагоны.";
            }

            if (levelData.Wagons.Count >
                sceneWagons.Count)
            {
                return
                    $"В LevelData указано " +
                    $"{levelData.Wagons.Count} вагонов, " +
                    $"но на сцене доступно только " +
                    $"{sceneWagons.Count}.";
            }

            if (!HasMaterialForColor(
                    levelData.InitialBufferColor,
                    materialsMap))
            {
                return
                    $"Не назначен материал для цвета " +
                    $"{levelData.InitialBufferColor}.";
            }

            Dictionary<ColorType, int> availablePassengers =
                new Dictionary<ColorType, int>();

            Dictionary<ColorType, int> requiredPassengers =
                new Dictionary<ColorType, int>();

            AddColorCount(
                availablePassengers,
                levelData.InitialBufferColor,
                1);

            for (int wagonIndex = 0;
                 wagonIndex < levelData.Wagons.Count;
                 wagonIndex++)
            {
                WagonSetup setup =
                    levelData.Wagons[wagonIndex];

                string wagonValidationError =
                    GetWagonValidationError(
                        setup,
                        sceneWagons[wagonIndex],
                        wagonIndex,
                        materialsMap);

                if (!string.IsNullOrEmpty(
                        wagonValidationError))
                {
                    return wagonValidationError;
                }

                AddColorCount(
                    requiredPassengers,
                    setup.TargetColor,
                    setup.Capacity);

                foreach (ColorType passengerColor
                         in setup.StartingPassengers)
                {
                    AddColorCount(
                        availablePassengers,
                        passengerColor,
                        1);
                }
            }

            return GetPassengerBalanceValidationError(
                availablePassengers,
                requiredPassengers);
        }

        private string GetWagonValidationError(
            WagonSetup setup,
            TrainWagon wagon,
            int wagonIndex,
            IReadOnlyList<ColorMaterialMapping> materialsMap)
        {
            if (setup == null)
            {
                return
                    $"Настройка вагона " +
                    $"{wagonIndex} отсутствует.";
            }

            if (setup.Capacity <= 0)
            {
                return
                    $"Вместимость вагона " +
                    $"{wagonIndex} должна быть больше нуля.";
            }

            if (setup.StartingPassengers == null)
            {
                return
                    $"У вагона {wagonIndex} отсутствует " +
                    "список начальных пассажиров.";
            }

            if (setup.StartingPassengers.Count !=
                setup.Capacity)
            {
                return
                    $"Вагон {wagonIndex} имеет вместимость " +
                    $"{setup.Capacity}, но начальных " +
                    $"пассажиров " +
                    $"{setup.StartingPassengers.Count}. " +
                    "Вагон должен быть полностью заполнен.";
            }

            if (wagon == null)
            {
                return
                    $"Вагон сцены {wagonIndex} " +
                    "не назначен.";
            }

            if (wagon.SeatPoints == null)
            {
                return
                    $"У вагона {wagonIndex} " +
                    "не назначен массив Seat Points.";
            }

            if (setup.Capacity >
                wagon.SeatPoints.Length)
            {
                return
                    $"Вместимость вагона {wagonIndex} " +
                    $"равна {setup.Capacity}, но точек " +
                    $"мест только " +
                    $"{wagon.SeatPoints.Length}.";
            }

            for (int seatIndex = 0;
                 seatIndex < setup.Capacity;
                 seatIndex++)
            {
                if (wagon.SeatPoints[seatIndex] == null)
                {
                    return
                        $"У вагона {wagonIndex} " +
                        $"не назначена точка места " +
                        $"{seatIndex}.";
                }
            }

            if (!HasMaterialForColor(
                    setup.TargetColor,
                    materialsMap))
            {
                return
                    $"Не назначен материал для " +
                    $"целевого цвета " +
                    $"{setup.TargetColor} вагона " +
                    $"{wagonIndex}.";
            }

            foreach (ColorType passengerColor
                     in setup.StartingPassengers)
            {
                if (!HasMaterialForColor(
                        passengerColor,
                        materialsMap))
                {
                    return
                        $"Не назначен материал для цвета " +
                        $"{passengerColor}, используемого " +
                        $"в вагоне {wagonIndex}.";
                }
            }

            return string.Empty;
        }

        private string GetPassengerBalanceValidationError(
            Dictionary<ColorType, int> availablePassengers,
            Dictionary<ColorType, int> requiredPassengers)
        {
            foreach (
                KeyValuePair<ColorType, int> requirement
                in requiredPassengers)
            {
                availablePassengers.TryGetValue(
                    requirement.Key,
                    out int availableCount);

                if (availableCount <
                    requirement.Value)
                {
                    return
                        $"Недостаточно пассажиров цвета " +
                        $"{requirement.Key}. Нужно " +
                        $"{requirement.Value}, доступно " +
                        $"{availableCount}.";
                }
            }

            return string.Empty;
        }

        private bool HasMaterialForColor(
            ColorType color,
            IReadOnlyList<ColorMaterialMapping> materialsMap)
        {
            foreach (ColorMaterialMapping mapping
                     in materialsMap)
            {
                if (mapping.Color == color &&
                    mapping.Material != null)
                {
                    return true;
                }
            }

            return false;
        }

        private void AddColorCount(
            Dictionary<ColorType, int> colorCounts,
            ColorType color,
            int amount)
        {
            if (colorCounts.ContainsKey(color))
            {
                colorCounts[color] += amount;

                return;
            }

            colorCounts[color] = amount;
        }
    }
}