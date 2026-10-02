using System.Collections.Generic;
using StationJam.Data;

namespace StationJam.Core
{
    public sealed class LevelCatalogValidator
    {
        private readonly LevelValidator _levelValidator =
            new LevelValidator();

        public string GetValidationError(
            LevelCatalog levelCatalog)
        {
            if (levelCatalog == null)
            {
                return
                    "Передан пустой LevelCatalog.";
            }

            if (levelCatalog.Count == 0)
            {
                return
                    "Каталог уровней пуст.";
            }

            HashSet<int> usedLevelNumbers =
                new HashSet<int>();

            for (int levelIndex = 0;
                 levelIndex < levelCatalog.Count;
                 levelIndex++)
            {
                LevelData levelData =
                    levelCatalog.GetLevel(
                        levelIndex);

                if (levelData == null)
                {
                    return
                        $"В каталоге отсутствует уровень " +
                        $"по индексу {levelIndex}.";
                }

                string levelValidationError =
                    _levelValidator.GetValidationError(
                        levelData);

                if (!string.IsNullOrEmpty(
                        levelValidationError))
                {
                    return
                        $"Уровень каталога {levelIndex}: " +
                        $"{levelValidationError}";
                }

                if (!usedLevelNumbers.Add(
                        levelData.LevelNumber))
                {
                    return
                        $"В каталоге повторяется номер " +
                        $"уровня {levelData.LevelNumber}.";
                }
            }

            return
                string.Empty;
        }
    }
}