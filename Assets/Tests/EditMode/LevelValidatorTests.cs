using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using StationJam.Core;
using StationJam.Data;
using StationJam.Entities;

namespace StationJam.Tests.EditMode
{
    public class LevelValidatorTests
    {
        private LevelValidator _validator;
        private LevelData _levelData;

        private GameObject _wagonGameObject;
        private GameObject _seatGameObject;

        private TrainWagon _wagon;
        private Material _material;

        [SetUp]
        public void SetUp()
        {
            _validator =
                new LevelValidator();

            _levelData =
                ScriptableObject.CreateInstance<
                    LevelData>();

            _levelData.LevelNumber = 1;

            _levelData.InitialBufferColor =
                ColorType.Red;

            _levelData.Wagons =
                new List<WagonSetup>();

            _wagonGameObject =
                new GameObject(
                    "TestWagon");

            _wagon =
                _wagonGameObject.AddComponent<
                    TrainWagon>();

            _seatGameObject =
                new GameObject(
                    "SeatPoint");

            _seatGameObject.transform.SetParent(
                _wagonGameObject.transform);

            AssignSeatPoints(
                _wagon,
                new[]
                {
                    _seatGameObject.transform
                });

            Shader shader =
                Shader.Find(
                    "Sprites/Default");

            Assert.IsNotNull(
                shader,
                "Тестовый Shader не найден.");

            _material =
                new Material(shader);
        }

        [TearDown]
        public void TearDown()
        {
            if (_material != null)
            {
                Object.DestroyImmediate(
                    _material);
            }

            if (_wagonGameObject != null)
            {
                Object.DestroyImmediate(
                    _wagonGameObject);
            }

            if (_levelData != null)
            {
                Object.DestroyImmediate(
                    _levelData);
            }
        }

        [Test]
        public void GetValidationError_WhenLevelDataIsNull_ReturnsError()
        {
            string validationError =
                _validator.GetValidationError(
                    null,
                    CreateSceneWagons(),
                    CreateMaterialsMap());

            Assert.AreEqual(
                "Передан пустой LevelData.",
                validationError);
        }

        [Test]
        public void GetValidationError_WhenLevelHasNoWagons_ReturnsError()
        {
            string validationError =
                _validator.GetValidationError(
                    _levelData,
                    CreateSceneWagons(),
                    CreateMaterialsMap());

            Assert.AreEqual(
                "В конфигурации уровня отсутствуют вагоны.",
                validationError);
        }

        [Test]
        public void GetValidationError_WhenPassengerCountDoesNotMatchCapacity_ReturnsError()
        {
            WagonSetup wagonSetup =
                new WagonSetup
                {
                    TargetColor =
                        ColorType.Red,

                    Capacity = 2,

                    StartingPassengers =
                        new List<ColorType>
                        {
                            ColorType.Red
                        }
                };

            _levelData.Wagons.Add(
                wagonSetup);

            string validationError =
                _validator.GetValidationError(
                    _levelData,
                    CreateSceneWagons(),
                    CreateMaterialsMap());

            Assert.AreEqual(
                "Вагон 0 имеет вместимость 2, " +
                "но начальных пассажиров 1. " +
                "Вагон должен быть полностью заполнен.",
                validationError);
        }

        [Test]
        public void GetValidationError_WhenConfigurationIsValid_ReturnsEmptyString()
        {
            WagonSetup wagonSetup =
                new WagonSetup
                {
                    TargetColor =
                        ColorType.Red,

                    Capacity = 1,

                    StartingPassengers =
                        new List<ColorType>
                        {
                            ColorType.Red
                        }
                };

            _levelData.Wagons.Add(
                wagonSetup);

            string validationError =
                _validator.GetValidationError(
                    _levelData,
                    CreateSceneWagons(),
                    CreateMaterialsMap());

            Assert.AreEqual(
                string.Empty,
                validationError);
        }

        private List<TrainWagon> CreateSceneWagons()
        {
            return new List<TrainWagon>
            {
                _wagon
            };
        }

        private List<ColorMaterialMapping> CreateMaterialsMap()
        {
            return new List<ColorMaterialMapping>
            {
                new ColorMaterialMapping
                {
                    Color =
                        ColorType.Red,

                    Material =
                        _material
                }
            };
        }

        private void AssignSeatPoints(
            TrainWagon wagon,
            IReadOnlyList<Transform> seatPoints)
        {
            SerializedObject serializedWagon =
                new SerializedObject(wagon);

            SerializedProperty seatPointsProperty =
                serializedWagon.FindProperty(
                    "_seatPoints");

            Assert.IsNotNull(
                seatPointsProperty,
                "Поле _seatPoints не найдено " +
                "в TrainWagon.");

            seatPointsProperty.arraySize =
                seatPoints.Count;

            for (int seatIndex = 0;
                 seatIndex < seatPoints.Count;
                 seatIndex++)
            {
                SerializedProperty seatProperty =
                    seatPointsProperty
                        .GetArrayElementAtIndex(
                            seatIndex);

                seatProperty.objectReferenceValue =
                    seatPoints[seatIndex];
            }

            serializedWagon
                .ApplyModifiedPropertiesWithoutUndo();
        }
    }
}