using System.Collections.Generic;
using DG.Tweening;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using StationJam.Core;
using StationJam.Entities;

namespace StationJam.Tests.EditMode
{
    public class SwapEngineTests
    {
        private const float PositionTolerance = 0.001f;

        private GameObject _swapEngineGameObject;
        private GameObject _flowGameObject;
        private GameObject _transitSlotGameObject;
        private GameObject _wagonGameObject;
        private GameObject _trainPassengerGameObject;
        private GameObject _bufferPassengerGameObject;

        private SwapEngine _swapEngine;
        private LevelFlowDriver _levelFlowDriver;
        private TransitSlot _transitSlot;
        private TrainWagon _wagon;
        private Passenger _trainPassenger;
        private Passenger _bufferPassenger;

        private Vector3 _trainStartPosition;
        private Vector3 _bufferPosition;

        [SetUp]
        public void SetUp()
        {
            DOTween.KillAll();

            CreateTransitSlot();
            CreateLevelFlowDriver();
            CreateWagon();
            CreatePassengers();
            CreateSwapEngine();

            _levelFlowDriver.Initialize(
                new List<TrainWagon>
                {
                    _wagon
                });

            _swapEngine.InitializeBuffer(
                _bufferPassenger);
        }

        [TearDown]
        public void TearDown()
        {
            DOTween.KillAll();

            if (_trainPassengerGameObject != null)
            {
                Object.DestroyImmediate(
                    _trainPassengerGameObject);
            }

            if (_bufferPassengerGameObject != null)
            {
                Object.DestroyImmediate(
                    _bufferPassengerGameObject);
            }

            if (_wagonGameObject != null)
            {
                Object.DestroyImmediate(
                    _wagonGameObject);
            }

            if (_swapEngineGameObject != null)
            {
                Object.DestroyImmediate(
                    _swapEngineGameObject);
            }

            if (_flowGameObject != null)
            {
                Object.DestroyImmediate(
                    _flowGameObject);
            }

            if (_transitSlotGameObject != null)
            {
                Object.DestroyImmediate(
                    _transitSlotGameObject);
            }
        }

        [Test]
        public void ProcessSwap_BeforeTweenCompletes_DoesNotChangeLogicalState()
        {
            _swapEngine.ProcessSwap(
                _trainPassenger);

            Assert.AreEqual(
                LevelState.Swapping,
                _levelFlowDriver.CurrentState);

            Assert.AreSame(
                _wagon,
                _trainPassenger.CurrentWagon);

            Assert.IsNull(
                _bufferPassenger.CurrentWagon);

            Assert.IsTrue(
                _wagon.IsFull);
        }

        [Test]
        public void ProcessSwap_WhenTweenCompletes_CommitsPassengerSwap()
        {
            _swapEngine.ProcessSwap(
                _trainPassenger);

            int completedTweens =
                DOTween.CompleteAll(true);

            Assert.GreaterOrEqual(
                completedTweens,
                1);

            Assert.AreEqual(
                LevelState.Playing,
                _levelFlowDriver.CurrentState);

            Assert.IsNull(
                _trainPassenger.CurrentWagon);

            Assert.AreSame(
                _wagon,
                _bufferPassenger.CurrentWagon);

            AssertPositionsAreEqual(
                _bufferPosition,
                _trainPassenger.transform.position);

            AssertPositionsAreEqual(
                _trainStartPosition,
                _bufferPassenger.transform.position);
        }

        [Test]
        public void ProcessSwap_WhenTweenIsCancelled_RollsBackSwap()
        {
            _swapEngine.ProcessSwap(
                _trainPassenger);

            Assert.AreEqual(
                LevelState.Swapping,
                _levelFlowDriver.CurrentState);

            Assert.AreSame(
                _wagon,
                _trainPassenger.CurrentWagon);

            Assert.IsNull(
                _bufferPassenger.CurrentWagon);

            DOTween.KillAll();

            Assert.AreEqual(
                LevelState.Playing,
                _levelFlowDriver.CurrentState);

            Assert.AreSame(
                _wagon,
                _trainPassenger.CurrentWagon);

            Assert.IsNull(
                _bufferPassenger.CurrentWagon);

            AssertPositionsAreEqual(
                _trainStartPosition,
                _trainPassenger.transform.position);

            AssertPositionsAreEqual(
                _bufferPosition,
                _bufferPassenger.transform.position);

            Assert.IsTrue(
                _wagon.IsFull);
        }

        [Test]
        public void ProcessSwap_WithEmptyBuffer_CommitsMoveToBuffer()
        {
            _swapEngine.InitializeBuffer(
                null);

            _swapEngine.ProcessSwap(
                _trainPassenger);

            Assert.AreEqual(
                LevelState.Swapping,
                _levelFlowDriver.CurrentState);

            Assert.AreSame(
                _wagon,
                _trainPassenger.CurrentWagon);

            DOTween.CompleteAll(true);

            Assert.AreEqual(
                LevelState.Playing,
                _levelFlowDriver.CurrentState);

            Assert.IsNull(
                _trainPassenger.CurrentWagon);

            Assert.IsFalse(
                _wagon.IsFull);

            AssertPositionsAreEqual(
                _bufferPosition,
                _trainPassenger.transform.position);
        }

        [Test]
        public void ProcessSwap_WhenWagonIsDeparting_DoesNothing()
        {
            SetWagonState(
                WagonState.Departing);

            _swapEngine.ProcessSwap(
                _trainPassenger);

            Assert.AreEqual(
                LevelState.Playing,
                _levelFlowDriver.CurrentState);

            Assert.AreSame(
                _wagon,
                _trainPassenger.CurrentWagon);

            Assert.IsNull(
                _bufferPassenger.CurrentWagon);

            AssertPositionsAreEqual(
                _trainStartPosition,
                _trainPassenger.transform.position);

            AssertPositionsAreEqual(
                _bufferPosition,
                _bufferPassenger.transform.position);
        }

        private void CreateTransitSlot()
        {
            _transitSlotGameObject =
                new GameObject(
                    "TransitSlot");

            _bufferPosition =
                new Vector3(
                    10f,
                    0f,
                    0f);

            _transitSlotGameObject.transform.position =
                _bufferPosition;

            _transitSlot =
                _transitSlotGameObject.AddComponent<
                    TransitSlot>();
        }

        private void CreateLevelFlowDriver()
        {
            _flowGameObject =
                new GameObject(
                    "LevelFlowDriver");

            _levelFlowDriver =
                _flowGameObject.AddComponent<
                    LevelFlowDriver>();
        }

        private void CreateWagon()
        {
            _wagonGameObject =
                new GameObject(
                    "TestWagon");

            _wagon =
                _wagonGameObject.AddComponent<
                    TrainWagon>();

            _wagon.InitializeData(
                ColorType.Green,
                1);
        }

        private void CreatePassengers()
        {
            _trainStartPosition =
                new Vector3(
                    1f,
                    0f,
                    0f);

            _trainPassenger =
                CreatePassenger(
                    "TrainPassenger",
                    ColorType.Blue,
                    _trainStartPosition);

            _trainPassengerGameObject =
                _trainPassenger.gameObject;

            _trainPassenger.SeatPosition =
                _trainStartPosition;

            bool trainPassengerAdded =
                _wagon.TryAddPassenger(
                    _trainPassenger);

            Assert.IsTrue(
                trainPassengerAdded);

            _bufferPassenger =
                CreatePassenger(
                    "BufferPassenger",
                    ColorType.Red,
                    _bufferPosition);

            _bufferPassengerGameObject =
                _bufferPassenger.gameObject;

            _bufferPassenger.SeatPosition =
                _bufferPosition;
        }

        private void CreateSwapEngine()
        {
            _swapEngineGameObject =
                new GameObject(
                    "SwapEngine");

            _swapEngine =
                _swapEngineGameObject.AddComponent<
                    SwapEngine>();

            SerializedObject serializedSwapEngine =
                new SerializedObject(
                    _swapEngine);

            SetObjectReference(
                serializedSwapEngine,
                "_transitSlot",
                _transitSlot);

            SetObjectReference(
                serializedSwapEngine,
                "_levelFlowDriver",
                _levelFlowDriver);

            SetFloatValue(
                serializedSwapEngine,
                "_jumpPower",
                2f);

            SetFloatValue(
                serializedSwapEngine,
                "_jumpDuration",
                1f);

            serializedSwapEngine
                .ApplyModifiedPropertiesWithoutUndo();
        }

        private Passenger CreatePassenger(
            string objectName,
            ColorType color,
            Vector3 position)
        {
            GameObject passengerGameObject =
                new GameObject(
                    objectName);

            passengerGameObject.transform.position =
                position;

            passengerGameObject.AddComponent<
                Animator>();

            passengerGameObject.AddComponent<
                SkinnedMeshRenderer>();

            Passenger passenger =
                passengerGameObject.AddComponent<
                    Passenger>();

            passenger.SetData(
                color,
                null);

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

        private void SetObjectReference(
            SerializedObject serializedObject,
            string propertyName,
            Object value)
        {
            SerializedProperty property =
                serializedObject.FindProperty(
                    propertyName);

            Assert.IsNotNull(
                property,
                $"Поле {propertyName} не найдено.");

            property.objectReferenceValue =
                value;
        }

        private void SetFloatValue(
            SerializedObject serializedObject,
            string propertyName,
            float value)
        {
            SerializedProperty property =
                serializedObject.FindProperty(
                    propertyName);

            Assert.IsNotNull(
                property,
                $"Поле {propertyName} не найдено.");

            property.floatValue =
                value;
        }

        private void AssertPositionsAreEqual(
            Vector3 expectedPosition,
            Vector3 actualPosition)
        {
            float distance =
                Vector3.Distance(
                    expectedPosition,
                    actualPosition);

            Assert.LessOrEqual(
                distance,
                PositionTolerance);
        }
    }
}