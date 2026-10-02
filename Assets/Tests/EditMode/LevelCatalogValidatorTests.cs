using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using StationJam.Core;
using StationJam.Data;
using StationJam.Entities;

namespace StationJam.Tests.EditMode
{
    public class LevelCatalogValidatorTests
    {
        private LevelCatalogValidator _validator;
        private LevelCatalog _levelCatalog;

        private readonly List<LevelData> _createdLevels =
            new List<LevelData>();

        [SetUp]
        public void SetUp()
        {
            _validator =
                new LevelCatalogValidator();

            _levelCatalog =
                ScriptableObject.CreateInstance<
                    LevelCatalog>();
        }

        [TearDown]
        public void TearDown()
        {
            foreach (LevelData levelData
                     in _createdLevels)
            {
                if (levelData != null)
                {
                    Object.DestroyImmediate(
                        levelData);
                }
            }

            _createdLevels.Clear();

            if (_levelCatalog != null)
            {
                Object.DestroyImmediate(
                    _levelCatalog);
            }
        }

        [Test]
        public void GetValidationError_WhenCatalogIsNull_ReturnsError()
        {
            string validationError =
                _validator.GetValidationError(
                    null);

            Assert.AreEqual(
                "Передан пустой LevelCatalog.",
                validationError);
        }

        [Test]
        public void GetValidationError_WhenCatalogIsEmpty_ReturnsError()
        {
            string validationError =
                _validator.GetValidationError(
                    _levelCatalog);

            Assert.AreEqual(
                "Каталог уровней пуст.",
                validationError);
        }

        [Test]
        public void GetValidationError_WhenCatalogContainsNullLevel_ReturnsError()
        {
            LevelData firstLevel =
                CreateValidLevel(
                    1);

            AssignLevels(
                firstLevel,
                null);

            string validationError =
                _validator.GetValidationError(
                    _levelCatalog);

            Assert.AreEqual(
                "В каталоге отсутствует уровень " +
                "по индексу 1.",
                validationError);
        }

        [Test]
        public void GetValidationError_WhenLevelNumberIsInvalid_ReturnsError()
        {
            LevelData invalidLevel =
                CreateValidLevel(
                    0);

            AssignLevels(
                invalidLevel);

            string validationError =
                _validator.GetValidationError(
                    _levelCatalog);

            Assert.AreEqual(
                "Уровень каталога 0: " +
                "Номер уровня должен быть больше нуля.",
                validationError);
        }

        [Test]
        public void GetValidationError_WhenLevelNumbersAreDuplicated_ReturnsError()
        {
            LevelData firstLevel =
                CreateValidLevel(
                    1);

            LevelData secondLevel =
                CreateValidLevel(
                    1);

            AssignLevels(
                firstLevel,
                secondLevel);

            string validationError =
                _validator.GetValidationError(
                    _levelCatalog);

            Assert.AreEqual(
                "В каталоге повторяется номер " +
                "уровня 1.",
                validationError);
        }

        [Test]
        public void GetValidationError_WhenCatalogIsValid_ReturnsEmptyString()
        {
            LevelData firstLevel =
                CreateValidLevel(
                    1);

            LevelData secondLevel =
                CreateValidLevel(
                    2);

            AssignLevels(
                firstLevel,
                secondLevel);

            string validationError =
                _validator.GetValidationError(
                    _levelCatalog);

            Assert.AreEqual(
                string.Empty,
                validationError);
        }

        private LevelData CreateValidLevel(
            int levelNumber)
        {
            LevelData levelData =
                ScriptableObject.CreateInstance<
                    LevelData>();

            levelData.LevelNumber =
                levelNumber;

            levelData.InitialBufferColor =
                ColorType.Red;

            levelData.Wagons =
                new List<WagonSetup>
                {
                    new WagonSetup
                    {
                        TargetColor =
                            ColorType.Red,

                        Capacity =
                            1,

                        StartingPassengers =
                            new List<ColorType>
                            {
                                ColorType.Red
                            }
                    }
                };

            _createdLevels.Add(
                levelData);

            return
                levelData;
        }

        private void AssignLevels(
            params LevelData[] levels)
        {
            SerializedObject serializedCatalog =
                new SerializedObject(
                    _levelCatalog);

            SerializedProperty levelsProperty =
                serializedCatalog.FindProperty(
                    "_levels");

            Assert.IsNotNull(
                levelsProperty,
                "Поле _levels не найдено " +
                "в LevelCatalog.");

            levelsProperty.arraySize =
                levels.Length;

            for (int levelIndex = 0;
                 levelIndex < levels.Length;
                 levelIndex++)
            {
                levelsProperty
                    .GetArrayElementAtIndex(
                        levelIndex)
                    .objectReferenceValue =
                    levels[levelIndex];
            }

            serializedCatalog
                .ApplyModifiedPropertiesWithoutUndo();
        }
    }
}