using System.Collections.Generic;
using UnityEngine;
using StationJam.Entities;

namespace StationJam.Data
{
    [CreateAssetMenu(
        fileName = "Level_00",
        menuName = "StationJam/Level Data",
        order = 0)]
    public class LevelData : ScriptableObject
    {
        [Header("Level Info")]
        public int LevelNumber;

        [Header("Buffer Settings")]
        [Tooltip(
            "Пассажир, который изначально стоит " +
            "в транзитном слоте")]
        public ColorType InitialBufferColor;

        [Header("Wagons Configuration")]
        [Tooltip("Список вагонов на уровне")]
        public List<WagonSetup> Wagons =
            new List<WagonSetup>();

        public bool TryValidate(
            out string errorMessage)
        {
            if (Wagons == null ||
                Wagons.Count == 0)
            {
                errorMessage =
                    "В конфигурации уровня отсутствуют вагоны.";

                return false;
            }

            Dictionary<ColorType, int> availablePassengers =
                new Dictionary<ColorType, int>();

            Dictionary<ColorType, int> requiredPassengers =
                new Dictionary<ColorType, int>();

            AddColorCount(
                availablePassengers,
                InitialBufferColor,
                1);

            for (int wagonIndex = 0;
                 wagonIndex < Wagons.Count;
                 wagonIndex++)
            {
                WagonSetup setup =
                    Wagons[wagonIndex];

                if (setup == null)
                {
                    errorMessage =
                        $"Настройка вагона " +
                        $"{wagonIndex} отсутствует.";

                    return false;
                }

                if (setup.Capacity <= 0)
                {
                    errorMessage =
                        $"Вместимость вагона " +
                        $"{wagonIndex} должна быть " +
                        "больше нуля.";

                    return false;
                }

                if (setup.StartingPassengers == null)
                {
                    errorMessage =
                        $"У вагона {wagonIndex} " +
                        "отсутствует список начальных " +
                        "пассажиров.";

                    return false;
                }

                if (setup.StartingPassengers.Count !=
                    setup.Capacity)
                {
                    errorMessage =
                        $"Вагон {wagonIndex} имеет " +
                        $"вместимость {setup.Capacity}, " +
                        $"но начальных пассажиров " +
                        $"{setup.StartingPassengers.Count}. " +
                        "Вагон должен быть полностью " +
                        "заполнен.";

                    return false;
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
                    errorMessage =
                        $"Недостаточно пассажиров цвета " +
                        $"{requirement.Key}. Нужно " +
                        $"{requirement.Value}, доступно " +
                        $"{availableCount}.";

                    return false;
                }
            }

            errorMessage = string.Empty;

            return true;
        }

        private static void AddColorCount(
            Dictionary<ColorType, int> colorCounts,
            ColorType color,
            int amount)
        {
            if (colorCounts.ContainsKey(color))
            {
                colorCounts[color] += amount;
            }
            else
            {
                colorCounts[color] = amount;
            }
        }
    }
}