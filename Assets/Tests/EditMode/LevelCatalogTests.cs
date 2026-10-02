using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using StationJam.Data;
using StationJam.Entities;

namespace StationJam.Tests.EditMode
{
    public class LevelCatalogTests
    {
        private LevelCatalog _levelCatalog;

        private LevelData _firstLevel;
        private LevelData _secondLevel;

        [SetUp]
        public void SetUp()
        {
            _levelCatalog =
                ScriptableObject.CreateInstance<
                    LevelCatalog>();

            _firstLevel =
                CreateValidLevel(
                    1);

            _secondLevel =
                CreateValidLevel(
                    2);
        }

        [TearDown]
        public void TearDown()
        {
            if (_firstLevel != null)
            {
                Object.DestroyImmediate(
                    _firstLevel);
            }

            if (_secondLevel != null)
            {
                Object.DestroyImmediate(
                    _secondLevel);
            }

            if (_levelCatalog != null)
            {
                Object.DestroyImmediate(
                    _levelCatalog);
            }
        }

        [Test]
        public void Count_WhenTwoLevelsAssigned_ReturnsTwo()
        {
            AssignLevels(
                _firstLevel,
                _secondLevel);

            Assert.AreEqual(
                2,
                _levelCatalog.Count);
        }

        [Test]
        public void GetLevel_WithValidIndex_ReturnsExpectedLevel()
        {
            AssignLevels(
                _firstLevel,
                _secondLevel);

            LevelData levelData =
                _levelCatalog.GetLevel(
                    1);

            Assert.AreSame(
                _secondLevel,
                levelData);
        }

        [Test]
        public void GetLevel_WithInvalidIndex_ReturnsNull()
        {
            AssignLevels(
                _firstLevel);

            LevelData levelData =
                _levelCatalog.GetLevel(
                    10);

            Assert.IsNull(
                levelData);
        }

        [Test]
        public void HasLevel_WithValidIndex_ReturnsTrue()
        {
            AssignLevels(
                _firstLevel);

            bool hasLevel =
                _levelCatalog.HasLevel(
                    0);

            Assert.IsTrue(
                hasLevel);
        }

        [Test]
        public void HasLevel_WithInvalidIndex_ReturnsFalse()
        {
            AssignLevels(
                _firstLevel);

            bool hasLevel =
                _levelCatalog.HasLevel(
                    1);

            Assert.IsFalse(
                hasLevel);
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