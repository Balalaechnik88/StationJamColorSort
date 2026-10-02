using System;
using System.Collections.Generic;
using StationJam.Data;
using StationJam.Entities;

namespace StationJam.Core
{
    public sealed class LevelValidator
    {
        public string GetValidationError(
            LevelData levelData)
        {
            if (levelData == null)
            {
                return
                    "Передан пустой LevelData.";
            }

            if (levelData.LevelNumber <= 0)
            {
                return
                    "Номер уровня должен быть больше нуля.";
            }

            if (!IsValidColor(
                    levelData.InitialBufferColor))
            {
                return
                    $"Недопустимый цвет пассажира " +
                    $"в буфере: " +
                    $"{levelData.InitialBufferColor}.";
            }

            if (levelData.Wagons == null ||
                levelData.Wagons.Count == 0)
            {
                return
                    "В конфигурации уровня отсутствуют вагоны.";
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
                    levelData.Wagons[
                        wagonIndex];

                string wagonValidationError =
                    GetWagonValidationError(
                        setup,
                        wagonIndex);

                if (!string.IsNullOrEmpty(
                        wagonValidationError))
                {
                    return
                        wagonValidationError;
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

            return
                GetPassengerBalanceValidationError(
                    availablePassengers,
                    requiredPassengers);
        }

        private string GetWagonValidationError(
            WagonSetup setup,
            int wagonIndex)
        {
            if (setup == null)
            {
                return
                    $"Настройка вагона " +
                    $"{wagonIndex} отсутствует.";
            }

            if (!IsValidColor(
                    setup.TargetColor))
            {
                return
                    $"Вагон {wagonIndex} имеет " +
                    $"недопустимый целевой цвет: " +
                    $"{setup.TargetColor}.";
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

            for (int passengerIndex = 0;
                 passengerIndex <
                 setup.StartingPassengers.Count;
                 passengerIndex++)
            {
                ColorType passengerColor =
                    setup.StartingPassengers[
                        passengerIndex];

                if (IsValidColor(
                        passengerColor))
                {
                    continue;
                }

                return
                    $"Пассажир {passengerIndex} вагона " +
                    $"{wagonIndex} имеет недопустимый " +
                    $"цвет: {passengerColor}.";
            }

            return
                string.Empty;
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

            return
                string.Empty;
        }

        private bool IsValidColor(
            ColorType color)
        {
            return
                Enum.IsDefined(
                    typeof(ColorType),
                    color);
        }

        private void AddColorCount(
            Dictionary<ColorType, int> colorCounts,
            ColorType color,
            int amount)
        {
            if (colorCounts.ContainsKey(
                    color))
            {
                colorCounts[color] +=
                    amount;

                return;
            }

            colorCounts[color] =
                amount;
        }
    }
}