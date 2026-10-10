using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using StationJam.Core;
using StationJam.Data;

namespace StationJam.Tests.EditMode
{
    public class ProjectLevelAssetsTests
    {
        private const string LevelCatalogPath =
            "Assets/Data/LevelCatalog.asset";

        private const string LevelsFolder =
            "Assets/Levels";

        [Test]
        public void LevelCatalogAsset_ExistsAndIsValid()
        {
            LevelCatalog levelCatalog =
                AssetDatabase.LoadAssetAtPath<LevelCatalog>(
                    LevelCatalogPath);

            Assert.IsNotNull(
                levelCatalog,
                $"LevelCatalog не найден по пути " +
                $"{LevelCatalogPath}.");

            LevelCatalogValidator validator =
                new LevelCatalogValidator();

            string validationError =
                validator.GetValidationError(
                    levelCatalog);

            Assert.AreEqual(
                string.Empty,
                validationError,
                $"LevelCatalog содержит ошибку: " +
                $"{validationError}");
        }

        [Test]
        public void AllLevelDataAssets_AreValid()
        {
            string[] levelGuids =
                AssetDatabase.FindAssets(
                    "t:LevelData",
                    new[]
                    {
                        LevelsFolder
                    });

            Assert.Greater(
                levelGuids.Length,
                0,
                $"В папке {LevelsFolder} " +
                "не найдено ни одного LevelData.");

            LevelValidator validator =
                new LevelValidator();

            foreach (string levelGuid
                     in levelGuids)
            {
                string levelPath =
                    AssetDatabase.GUIDToAssetPath(
                        levelGuid);

                LevelData levelData =
                    AssetDatabase.LoadAssetAtPath<
                        LevelData>(
                        levelPath);

                Assert.IsNotNull(
                    levelData,
                    $"Не удалось загрузить LevelData: " +
                    $"{levelPath}");

                string validationError =
                    validator.GetValidationError(
                        levelData);

                Assert.AreEqual(
                    string.Empty,
                    validationError,
                    $"Некорректный LevelData " +
                    $"'{levelPath}': " +
                    $"{validationError}");
            }
        }

        [Test]
        public void LevelCatalog_ContainsEveryLevelAssetExactlyOnce()
        {
            LevelCatalog levelCatalog =
                AssetDatabase.LoadAssetAtPath<LevelCatalog>(
                    LevelCatalogPath);

            Assert.IsNotNull(
                levelCatalog,
                $"LevelCatalog не найден по пути " +
                $"{LevelCatalogPath}.");

            string[] levelGuids =
                AssetDatabase.FindAssets(
                    "t:LevelData",
                    new[]
                    {
                        LevelsFolder
                    });

            HashSet<string> projectLevelPaths =
                new HashSet<string>();

            foreach (string levelGuid
                     in levelGuids)
            {
                string levelPath =
                    AssetDatabase.GUIDToAssetPath(
                        levelGuid);

                projectLevelPaths.Add(
                    levelPath);
            }

            HashSet<string> catalogLevelPaths =
                new HashSet<string>();

            for (int levelIndex = 0;
                 levelIndex < levelCatalog.Count;
                 levelIndex++)
            {
                LevelData levelData =
                    levelCatalog.GetLevel(
                        levelIndex);

                Assert.IsNotNull(
                    levelData,
                    $"В LevelCatalog отсутствует " +
                    $"уровень по индексу " +
                    $"{levelIndex}.");

                string levelPath =
                    AssetDatabase.GetAssetPath(
                        levelData);

                Assert.IsFalse(
                    string.IsNullOrEmpty(
                        levelPath),
                    $"LevelData по индексу " +
                    $"{levelIndex} не является " +
                    "сохранённым asset.");

                bool added =
                    catalogLevelPaths.Add(
                        levelPath);

                Assert.IsTrue(
                    added,
                    $"LevelData '{levelPath}' " +
                    "добавлен в LevelCatalog " +
                    "более одного раза.");
            }

            Assert.AreEqual(
                projectLevelPaths.Count,
                catalogLevelPaths.Count,
                "Количество LevelData в папке уровней " +
                "не совпадает с количеством уникальных " +
                "уровней в LevelCatalog.");

            foreach (string projectLevelPath
                     in projectLevelPaths)
            {
                Assert.IsTrue(
                    catalogLevelPaths.Contains(
                        projectLevelPath),
                    $"LevelData '{projectLevelPath}' " +
                    "существует в Assets/Levels, " +
                    "но отсутствует в LevelCatalog.");
            }
        }
    }
}