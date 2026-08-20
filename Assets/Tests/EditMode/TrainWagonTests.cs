using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using StationJam.Entities;

namespace StationJam.Tests.EditMode
{
    public class TrainWagonTests
    {
        private GameObject _wagonGameObject;
        private TrainWagon _wagon;

        private GameObject _firstPassengerGameObject;
        private GameObject _secondPassengerGameObject;

        private Passenger _firstPassenger;
        private Passenger _secondPassenger;

        [SetUp]
        public void SetUp()
        {
            _wagonGameObject =
                new GameObject(
                    "TestWagon");

            _wagon =
                _wagonGameObject.AddComponent<
                    TrainWagon>();

            _wagon.InitializeData(
                ColorType.Red,
                1);

            _firstPassenger =
                CreatePassenger(
                    "FirstPassenger",
                    ColorType.Red);

            _firstPassengerGameObject =
                _firstPassenger.gameObject;

            _secondPassenger =
                CreatePassenger(
                    "SecondPassenger",
                    ColorType.Blue);

            _secondPassengerGameObject =
                _secondPassenger.gameObject;
        }

        [TearDown]
        public void TearDown()
        {
            if (_wagon != null)
            {
                _wagon.DisableForLevel();
            }

            if (_firstPassengerGameObject != null)
            {
                Object.DestroyImmediate(
                    _firstPassengerGameObject);
            }

            if (_secondPassengerGameObject != null)
            {
                Object.DestroyImmediate(
                    _secondPassengerGameObject);
            }

            if (_wagonGameObject != null)
            {
                Object.DestroyImmediate(
                    _wagonGameObject);
            }
        }

        [Test]
        public void InitializeData_SetsWagonToActiveState()
        {
            Assert.AreEqual(
                WagonState.Active,
                _wagon.CurrentState);

            Assert.IsTrue(
                _wagon.CanInteract);

            Assert.AreEqual(
                ColorType.Red,
                _wagon.TargetColor);

            Assert.AreEqual(
                1,
                _wagon.Capacity);
        }

        [Test]
        public void TryAddPassenger_WhenWagonIsActive_AddsPassenger()
        {
            bool passengerAdded =
                _wagon.TryAddPassenger(
                    _firstPassenger);

            Assert.IsTrue(
                passengerAdded);

            Assert.AreSame(
                _wagon,
                _firstPassenger.CurrentWagon);

            Assert.IsTrue(
                _wagon.IsFull);
        }

        [Test]
        public void TryAddPassenger_WhenWagonIsFull_ReturnsFalse()
        {
            bool firstPassengerAdded =
                _wagon.TryAddPassenger(
                    _firstPassenger);

            bool secondPassengerAdded =
                _wagon.TryAddPassenger(
                    _secondPassenger);

            Assert.IsTrue(
                firstPassengerAdded);

            Assert.IsFalse(
                secondPassengerAdded);

            Assert.AreSame(
                _wagon,
                _firstPassenger.CurrentWagon);

            Assert.IsNull(
                _secondPassenger.CurrentWagon);
        }

        [Test]
        public void TryRemovePassenger_WhenPassengerExists_RemovesPassenger()
        {
            bool passengerAdded =
                _wagon.TryAddPassenger(
                    _firstPassenger);

            Assert.IsTrue(
                passengerAdded);

            bool passengerRemoved =
                _wagon.TryRemovePassenger(
                    _firstPassenger);

            Assert.IsTrue(
                passengerRemoved);

            Assert.IsNull(
                _firstPassenger.CurrentWagon);

            Assert.IsFalse(
                _wagon.IsFull);
        }

        [Test]
        public void TryReplacePassenger_ReplacesPassengerAtomically()
        {
            Vector3 seatPosition =
                new Vector3(
                    1f,
                    2f,
                    3f);

            _firstPassenger.SeatPosition =
                seatPosition;

            bool passengerAdded =
                _wagon.TryAddPassenger(
                    _firstPassenger);

            Assert.IsTrue(
                passengerAdded);

            bool passengerReplaced =
                _wagon.TryReplacePassenger(
                    _firstPassenger,
                    _secondPassenger);

            Assert.IsTrue(
                passengerReplaced);

            Assert.IsNull(
                _firstPassenger.CurrentWagon);

            Assert.AreSame(
                _wagon,
                _secondPassenger.CurrentWagon);

            Assert.AreEqual(
                seatPosition,
                _secondPassenger.SeatPosition);

            Assert.IsTrue(
                _wagon.IsFull);
        }

        [Test]
        public void TryReplacePassenger_WhenWagonIsDeparting_ReturnsFalse()
        {
            bool passengerAdded =
                _wagon.TryAddPassenger(
                    _firstPassenger);

            Assert.IsTrue(
                passengerAdded);

            SetWagonState(
                WagonState.Departing);

            bool passengerReplaced =
                _wagon.TryReplacePassenger(
                    _firstPassenger,
                    _secondPassenger);

            Assert.IsFalse(
                passengerReplaced);

            Assert.AreSame(
                _wagon,
                _firstPassenger.CurrentWagon);

            Assert.IsNull(
                _secondPassenger.CurrentWagon);
        }

        [Test]
        public void CanInteract_WhenWagonIsDeparting_ReturnsFalse()
        {
            SetWagonState(
                WagonState.Departing);

            Assert.IsFalse(
                _wagon.CanInteract);
        }

        [Test]
        public void CheckCompletion_WhenPassengersMatchTarget_StartsDeparture()
        {
            bool passengerAdded =
                _wagon.TryAddPassenger(
                    _firstPassenger);

            Assert.IsTrue(
                passengerAdded);

            _wagon.CheckCompletion();

            Assert.AreEqual(
                WagonState.Departing,
                _wagon.CurrentState);

            Assert.IsFalse(
                _wagon.CanInteract);
        }

        [Test]
        public void CheckCompletion_WhenPassengerHasWrongColor_RemainsActive()
        {
            bool passengerAdded =
                _wagon.TryAddPassenger(
                    _secondPassenger);

            Assert.IsTrue(
                passengerAdded);

            _wagon.CheckCompletion();

            Assert.AreEqual(
                WagonState.Active,
                _wagon.CurrentState);

            Assert.IsTrue(
                _wagon.CanInteract);
        }

        private Passenger CreatePassenger(
            string objectName,
            ColorType color)
        {
            GameObject passengerGameObject =
                new GameObject(
                    objectName);

            Passenger passenger =
                passengerGameObject.AddComponent<
                    Passenger>();

            SerializedObject serializedPassenger =
                new SerializedObject(
                    passenger);

            SerializedProperty colorProperty =
                serializedPassenger.FindProperty(
                    "_passengerColor");

            Assert.IsNotNull(
                colorProperty,
                "Поле _passengerColor не найдено " +
                "в Passenger.");

            colorProperty.enumValueIndex =
                (int)color;

            serializedPassenger
                .ApplyModifiedPropertiesWithoutUndo();

            return passenger;
        }

        private void SetWagonState(
            WagonState wagonState)
        {
            SerializedObject serializedWagon =
                new SerializedObject(
                    _wagon);

            SerializedProperty stateProperty =
                serializedWagon.FindProperty(
                    "_currentState");

            Assert.IsNotNull(
                stateProperty,
                "Поле _currentState не найдено " +
                "в TrainWagon.");

            stateProperty.enumValueIndex =
                (int)wagonState;

            serializedWagon
                .ApplyModifiedPropertiesWithoutUndo();
        }
    }
}