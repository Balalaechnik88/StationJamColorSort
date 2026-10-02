using System.Collections.Generic;
using NUnit.Framework;
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

        [SetUp]
        public void SetUp()
        {
            _validator =
                new LevelValidator();

            _levelData =
                ScriptableObject.CreateInstance<
                    LevelData>();

            _levelData.LevelNumber =
                1;

            _levelData.InitialBufferColor =
                ColorType.Red;

            _levelData.Wagons =
                new List<WagonSetup>();
        }

        [TearDown]
        public void TearDown()
        {
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
                    null);

            Assert.AreEqual(
                "Передан пустой LevelData.",
                validationError);
        }

        [Test]
        public void GetValidationError_WhenLevelNumberIsZero_ReturnsError()
        {
            _levelData.LevelNumber =
                0;

            string validationError =
                _validator.GetValidationError(
                    _levelData);

            Assert.AreEqual(
                "Номер уровня должен быть больше нуля.",
                validationError);
        }

        [Test]
        public void GetValidationError_WhenLevelNumberIsNegative_ReturnsError()
        {
            _levelData.LevelNumber =
                -1;

            string validationError =
                _validator.GetValidationError(
                    _levelData);

            Assert.AreEqual(
                "Номер уровня должен быть больше нуля.",
                validationError);
        }

        [Test]
        public void GetValidationError_WhenBufferColorIsInvalid_ReturnsError()
        {
            _levelData.InitialBufferColor =
                (ColorType)999;

            string validationError =
                _validator.GetValidationError(
                    _levelData);

            Assert.AreEqual(
                "Недопустимый цвет пассажира " +
                "в буфере: 999.",
                validationError);
        }

        [Test]
        public void GetValidationError_WhenLevelHasNoWagons_ReturnsError()
        {
            string validationError =
                _validator.GetValidationError(
                    _levelData);

            Assert.AreEqual(
                "В конфигурации уровня отсутствуют вагоны.",
                validationError);
        }

        [Test]
        public void GetValidationError_WhenTargetColorIsInvalid_ReturnsError()
        {
            _levelData.Wagons.Add(
                new WagonSetup
                {
                    TargetColor =
                        (ColorType)999,

                    Capacity =
                        1,

                    StartingPassengers =
                        new List<ColorType>
                        {
                            ColorType.Red
                        }
                });

            string validationError =
                _validator.GetValidationError(
                    _levelData);

            Assert.AreEqual(
                "Вагон 0 имеет недопустимый " +
                "целевой цвет: 999.",
                validationError);
        }

        [Test]
        public void GetValidationError_WhenPassengerColorIsInvalid_ReturnsError()
        {
            _levelData.Wagons.Add(
                new WagonSetup
                {
                    TargetColor =
                        ColorType.Red,

                    Capacity =
                        1,

                    StartingPassengers =
                        new List<ColorType>
                        {
                            (ColorType)999
                        }
                });

            string validationError =
                _validator.GetValidationError(
                    _levelData);

            Assert.AreEqual(
                "Пассажир 0 вагона 0 имеет " +
                "недопустимый цвет: 999.",
                validationError);
        }

        [Test]
        public void GetValidationError_WhenPassengerCountDoesNotMatchCapacity_ReturnsError()
        {
            _levelData.Wagons.Add(
                new WagonSetup
                {
                    TargetColor =
                        ColorType.Red,

                    Capacity =
                        2,

                    StartingPassengers =
                        new List<ColorType>
                        {
                            ColorType.Red
                        }
                });

            string validationError =
                _validator.GetValidationError(
                    _levelData);

            Assert.AreEqual(
                "Вагон 0 имеет вместимость 2, " +
                "но начальных пассажиров 1. " +
                "Вагон должен быть полностью заполнен.",
                validationError);
        }

        [Test]
        public void GetValidationError_WhenCapacityIsZero_ReturnsError()
        {
            _levelData.Wagons.Add(
                new WagonSetup
                {
                    TargetColor =
                        ColorType.Red,

                    Capacity =
                        0,

                    StartingPassengers =
                        new List<ColorType>()
                });

            string validationError =
                _validator.GetValidationError(
                    _levelData);

            Assert.AreEqual(
                "Вместимость вагона 0 должна быть " +
                "больше нуля.",
                validationError);
        }

        [Test]
        public void GetValidationError_WhenRequiredColorIsMissing_ReturnsError()
        {
            _levelData.InitialBufferColor =
                ColorType.Blue;

            _levelData.Wagons.Add(
                new WagonSetup
                {
                    TargetColor =
                        ColorType.Red,

                    Capacity =
                        2,

                    StartingPassengers =
                        new List<ColorType>
                        {
                            ColorType.Blue,
                            ColorType.Blue
                        }
                });

            string validationError =
                _validator.GetValidationError(
                    _levelData);

            Assert.AreEqual(
                "Недостаточно пассажиров цвета Red. " +
                "Нужно 2, доступно 0.",
                validationError);
        }

        [Test]
        public void GetValidationError_WhenConfigurationIsValid_ReturnsEmptyString()
        {
            _levelData.Wagons.Add(
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
                });

            string validationError =
                _validator.GetValidationError(
                    _levelData);

            Assert.AreEqual(
                string.Empty,
                validationError);
        }
    }
}