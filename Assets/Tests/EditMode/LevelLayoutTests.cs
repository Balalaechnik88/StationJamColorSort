using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using StationJam.Core;

namespace StationJam.Tests.EditMode
{
    public class LevelLayoutTests
    {
        private GameObject _layoutGameObject;
        private GameObject _firstSpawnPointGameObject;
        private GameObject _secondSpawnPointGameObject;

        private LevelLayout _levelLayout;

        private Transform _firstSpawnPoint;
        private Transform _secondSpawnPoint;

        [SetUp]
        public void SetUp()
        {
            _layoutGameObject =
                new GameObject(
                    "LevelLayout");

            _levelLayout =
                _layoutGameObject.AddComponent<
                    LevelLayout>();

            _firstSpawnPointGameObject =
                new GameObject(
                    "FirstSpawnPoint");

            _firstSpawnPoint =
                _firstSpawnPointGameObject.transform;

            _firstSpawnPoint.SetParent(
                _layoutGameObject.transform);

            _secondSpawnPointGameObject =
                new GameObject(
                    "SecondSpawnPoint");

            _secondSpawnPoint =
                _secondSpawnPointGameObject.transform;

            _secondSpawnPoint.SetParent(
                _layoutGameObject.transform);
        }

        [TearDown]
        public void TearDown()
        {
            if (_layoutGameObject != null)
            {
                Object.DestroyImmediate(
                    _layoutGameObject);
            }

            if (_firstSpawnPointGameObject != null)
            {
                Object.DestroyImmediate(
                    _firstSpawnPointGameObject);
            }

            if (_secondSpawnPointGameObject != null)
            {
                Object.DestroyImmediate(
                    _secondSpawnPointGameObject);
            }
        }

        [Test]
        public void GetConfigurationError_WhenSpawnPointsAreEmpty_ReturnsError()
        {
            string configurationError =
                _levelLayout.GetConfigurationError(
                    1);

            Assert.AreEqual(
                "LevelLayout: точки размещения " +
                "вагонов не назначены.",
                configurationError);
        }

        [Test]
        public void GetConfigurationError_WhenRequiredCountIsZero_ReturnsError()
        {
            AssignSpawnPoints(
                _firstSpawnPoint);

            string configurationError =
                _levelLayout.GetConfigurationError(
                    0);

            Assert.AreEqual(
                "LevelLayout: количество вагонов " +
                "должно быть больше нуля.",
                configurationError);
        }

        [Test]
        public void GetConfigurationError_WhenNotEnoughSpawnPoints_ReturnsError()
        {
            AssignSpawnPoints(
                _firstSpawnPoint);

            string configurationError =
                _levelLayout.GetConfigurationError(
                    2);

            Assert.AreEqual(
                "LevelLayout: требуется 2 вагонов, " +
                "но доступно только 1 точек размещения.",
                configurationError);
        }

        [Test]
        public void GetConfigurationError_WhenRequiredSpawnPointIsNull_ReturnsError()
        {
            AssignSpawnPoints(
                _firstSpawnPoint,
                null);

            string configurationError =
                _levelLayout.GetConfigurationError(
                    2);

            Assert.AreEqual(
                "LevelLayout: точка размещения 1 " +
                "не назначена.",
                configurationError);
        }

        [Test]
        public void GetConfigurationError_WhenConfigurationIsValid_ReturnsEmptyString()
        {
            AssignSpawnPoints(
                _firstSpawnPoint,
                _secondSpawnPoint);

            string configurationError =
                _levelLayout.GetConfigurationError(
                    2);

            Assert.AreEqual(
                string.Empty,
                configurationError);
        }

        [Test]
        public void GetSpawnPoint_WithValidIndex_ReturnsExpectedTransform()
        {
            AssignSpawnPoints(
                _firstSpawnPoint,
                _secondSpawnPoint);

            Transform spawnPoint =
                _levelLayout.GetSpawnPoint(
                    1);

            Assert.AreSame(
                _secondSpawnPoint,
                spawnPoint);
        }

        [Test]
        public void GetSpawnPoint_WithInvalidIndex_ReturnsNull()
        {
            AssignSpawnPoints(
                _firstSpawnPoint);

            Transform spawnPoint =
                _levelLayout.GetSpawnPoint(
                    5);

            Assert.IsNull(
                spawnPoint);
        }

        private void AssignSpawnPoints(
            params Transform[] spawnPoints)
        {
            SerializedObject serializedLayout =
                new SerializedObject(
                    _levelLayout);

            SerializedProperty spawnPointsProperty =
                serializedLayout.FindProperty(
                    "_wagonSpawnPoints");

            Assert.IsNotNull(
                spawnPointsProperty,
                "Поле _wagonSpawnPoints не найдено " +
                "в LevelLayout.");

            spawnPointsProperty.arraySize =
                spawnPoints.Length;

            for (int spawnPointIndex = 0;
                 spawnPointIndex < spawnPoints.Length;
                 spawnPointIndex++)
            {
                SerializedProperty elementProperty =
                    spawnPointsProperty
                        .GetArrayElementAtIndex(
                            spawnPointIndex);

                elementProperty.objectReferenceValue =
                    spawnPoints[spawnPointIndex];
            }

            serializedLayout
                .ApplyModifiedPropertiesWithoutUndo();
        }
    }
}