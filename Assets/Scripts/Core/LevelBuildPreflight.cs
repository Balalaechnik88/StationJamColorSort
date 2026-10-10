using StationJam.Data;
using StationJam.Entities;
using StationJam.Factories;

namespace StationJam.Core
{
    public sealed class LevelBuildPreflight
    {
        private readonly LevelValidator _levelValidator =
            new LevelValidator();

        public string GetValidationError(
            LevelData levelData,
            LevelLayout levelLayout,
            PassengerFactory passengerFactory,
            WagonFactory wagonFactory)
        {
            if (levelLayout == null)
            {
                return
                    "LevelLayout не назначен.";
            }

            if (passengerFactory == null)
            {
                return
                    "PassengerFactory не назначен.";
            }

            if (wagonFactory == null)
            {
                return
                    "WagonFactory не назначен.";
            }

            string levelValidationError =
                _levelValidator.GetValidationError(
                    levelData);

            if (!string.IsNullOrEmpty(
                    levelValidationError))
            {
                return
                    levelValidationError;
            }

            string passengerFactoryError =
                passengerFactory
                    .GetConfigurationError();

            if (!string.IsNullOrEmpty(
                    passengerFactoryError))
            {
                return
                    passengerFactoryError;
            }

            string wagonFactoryError =
                wagonFactory
                    .GetConfigurationError();

            if (!string.IsNullOrEmpty(
                    wagonFactoryError))
            {
                return
                    wagonFactoryError;
            }

            string layoutValidationError =
                levelLayout.GetConfigurationError(
                    levelData.Wagons.Count);

            if (!string.IsNullOrEmpty(
                    layoutValidationError))
            {
                return
                    layoutValidationError;
            }

            string passengerColorsError =
                GetPassengerColorsValidationError(
                    levelData,
                    passengerFactory);

            if (!string.IsNullOrEmpty(
                    passengerColorsError))
            {
                return
                    passengerColorsError;
            }

            string wagonsError =
                GetWagonsValidationError(
                    levelData,
                    wagonFactory);

            if (!string.IsNullOrEmpty(
                    wagonsError))
            {
                return
                    wagonsError;
            }

            return
                string.Empty;
        }

        private string GetPassengerColorsValidationError(
            LevelData levelData,
            PassengerFactory passengerFactory)
        {
            string bufferColorError =
                passengerFactory
                    .GetColorConfigurationError(
                        levelData.InitialBufferColor);

            if (!string.IsNullOrEmpty(
                    bufferColorError))
            {
                return
                    bufferColorError;
            }

            foreach (WagonSetup wagonSetup
                     in levelData.Wagons)
            {
                foreach (ColorType passengerColor
                         in wagonSetup.StartingPassengers)
                {
                    string colorError =
                        passengerFactory
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

        private string GetWagonsValidationError(
            LevelData levelData,
            WagonFactory wagonFactory)
        {
            for (int wagonIndex = 0;
                 wagonIndex < levelData.Wagons.Count;
                 wagonIndex++)
            {
                WagonSetup wagonSetup =
                    levelData.Wagons[
                        wagonIndex];

                string wagonError =
                    wagonFactory
                        .GetWagonConfigurationError(
                            wagonSetup.TargetColor,
                            wagonSetup.Capacity);

                if (string.IsNullOrEmpty(
                        wagonError))
                {
                    continue;
                }

                return
                    $"Вагон {wagonIndex}: " +
                    $"{wagonError}";
            }

            return
                string.Empty;
        }
    }
}