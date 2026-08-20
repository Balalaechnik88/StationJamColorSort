using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using StationJam.Core;
using StationJam.Entities;

namespace StationJam.Tests.EditMode
{
    public class LevelFlowDriverTests
    {
        private GameObject _flowGameObject;
        private GameObject _wagonGameObject;

        private LevelFlowDriver _levelFlowDriver;
        private TrainWagon _wagon;

        [SetUp]
        public void SetUp()
        {
            _flowGameObject =
                new GameObject(
                    "LevelFlowDriver");

            _levelFlowDriver =
                _flowGameObject.AddComponent<
                    LevelFlowDriver>();

            _wagonGameObject =
                new GameObject(
                    "TestWagon");

            _wagon =
                _wagonGameObject.AddComponent<
                    TrainWagon>();
        }

        [TearDown]
        public void TearDown()
        {
            if (_flowGameObject != null)
            {
                Object.DestroyImmediate(
                    _flowGameObject);
            }

            if (_wagonGameObject != null)
            {
                Object.DestroyImmediate(
                    _wagonGameObject);
            }
        }

        [Test]
        public void InitialState_IsNotInitialized()
        {
            Assert.AreEqual(
                LevelState.NotInitialized,
                _levelFlowDriver.CurrentState);
        }

        [Test]
        public void Initialize_WithWagon_ChangesStateToPlaying()
        {
            List<TrainWagon> wagons =
                CreateWagonList();

            _levelFlowDriver.Initialize(
                wagons);

            Assert.AreEqual(
                LevelState.Playing,
                _levelFlowDriver.CurrentState);

            Assert.IsTrue(
                _levelFlowDriver.CanAcceptInput);
        }

        [Test]
        public void TryStartSwap_WhenPlaying_ChangesStateToSwapping()
        {
            InitializeLevel();

            bool swapStarted =
                _levelFlowDriver.TryStartSwap();

            Assert.IsTrue(
                swapStarted);

            Assert.AreEqual(
                LevelState.Swapping,
                _levelFlowDriver.CurrentState);

            Assert.IsFalse(
                _levelFlowDriver.CanAcceptInput);
        }

        [Test]
        public void FinishSwap_WhenSwapping_ReturnsStateToPlaying()
        {
            InitializeLevel();

            bool swapStarted =
                _levelFlowDriver.TryStartSwap();

            Assert.IsTrue(
                swapStarted);

            _levelFlowDriver.FinishSwap();

            Assert.AreEqual(
                LevelState.Playing,
                _levelFlowDriver.CurrentState);

            Assert.IsTrue(
                _levelFlowDriver.CanAcceptInput);
        }

        [Test]
        public void TryStartSwap_WhenNotInitialized_ReturnsFalse()
        {
            bool swapStarted =
                _levelFlowDriver.TryStartSwap();

            Assert.IsFalse(
                swapStarted);

            Assert.AreEqual(
                LevelState.NotInitialized,
                _levelFlowDriver.CurrentState);
        }

        [Test]
        public void TryStartSwap_WhenAlreadySwapping_ReturnsFalse()
        {
            InitializeLevel();

            bool firstSwapStarted =
                _levelFlowDriver.TryStartSwap();

            bool secondSwapStarted =
                _levelFlowDriver.TryStartSwap();

            Assert.IsTrue(
                firstSwapStarted);

            Assert.IsFalse(
                secondSwapStarted);

            Assert.AreEqual(
                LevelState.Swapping,
                _levelFlowDriver.CurrentState);
        }

        [Test]
        public void ResetLevel_AfterInitialization_ReturnsToNotInitialized()
        {
            InitializeLevel();

            _levelFlowDriver.ResetLevel();

            Assert.AreEqual(
                LevelState.NotInitialized,
                _levelFlowDriver.CurrentState);

            Assert.IsFalse(
                _levelFlowDriver.CanAcceptInput);
        }

        [Test]
        public void Initialize_WithEmptyWagonList_RemainsNotInitialized()
        {
            List<TrainWagon> wagons =
                new List<TrainWagon>();

            _levelFlowDriver.Initialize(
                wagons);

            Assert.AreEqual(
                LevelState.NotInitialized,
                _levelFlowDriver.CurrentState);

            Assert.IsFalse(
                _levelFlowDriver.CanAcceptInput);
        }

        private void InitializeLevel()
        {
            _levelFlowDriver.Initialize(
                CreateWagonList());
        }

        private List<TrainWagon> CreateWagonList()
        {
            return new List<TrainWagon>
            {
                _wagon
            };
        }
    }
}