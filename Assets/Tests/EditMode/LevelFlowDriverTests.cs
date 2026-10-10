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

            _wagon.InitializeData(
                ColorType.Red,
                1);
        }

        [TearDown]
        public void TearDown()
        {
            if (_wagon != null)
            {
                _wagon.DisableForLevel();
            }

            if (_wagonGameObject != null)
            {
                Object.DestroyImmediate(
                    _wagonGameObject);
            }

            if (_flowGameObject != null)
            {
                Object.DestroyImmediate(
                    _flowGameObject);
            }
        }

        [Test]
        public void InitialState_IsNotInitialized()
        {
            Assert.AreEqual(
                LevelState.NotInitialized,
                _levelFlowDriver.CurrentState);

            Assert.IsFalse(
                _levelFlowDriver.CanAcceptInput);
        }

        [Test]
        public void Initialize_WithWagon_ChangesStateToPlaying()
        {
            InitializeWithWagon();

            Assert.AreEqual(
                LevelState.Playing,
                _levelFlowDriver.CurrentState);

            Assert.IsTrue(
                _levelFlowDriver.CanAcceptInput);
        }

        [Test]
        public void Initialize_WithEmptyWagonList_RemainsNotInitialized()
        {
            _levelFlowDriver.Initialize(
                new List<TrainWagon>());

            Assert.AreEqual(
                LevelState.NotInitialized,
                _levelFlowDriver.CurrentState);

            Assert.IsFalse(
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
        public void TryStartSwap_WhenPlaying_ChangesStateToSwapping()
        {
            InitializeWithWagon();

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
        public void TryStartSwap_WhenAlreadySwapping_ReturnsFalse()
        {
            InitializeWithWagon();

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
        public void FinishSwap_WhenSwapping_ReturnsStateToPlaying()
        {
            InitializeWithWagon();

            _levelFlowDriver.TryStartSwap();

            _levelFlowDriver.FinishSwap();

            Assert.AreEqual(
                LevelState.Playing,
                _levelFlowDriver.CurrentState);

            Assert.IsTrue(
                _levelFlowDriver.CanAcceptInput);
        }

        [Test]
        public void ResetLevel_AfterInitialization_ReturnsToNotInitialized()
        {
            InitializeWithWagon();

            _levelFlowDriver.ResetLevel();

            Assert.AreEqual(
                LevelState.NotInitialized,
                _levelFlowDriver.CurrentState);

            Assert.IsFalse(
                _levelFlowDriver.CanAcceptInput);
        }

        [Test]
        public void TryPause_WhenPlaying_ChangesStateToPaused()
        {
            InitializeWithWagon();

            bool paused =
                _levelFlowDriver.TryPause();

            Assert.IsTrue(
                paused);

            Assert.AreEqual(
                LevelState.Paused,
                _levelFlowDriver.CurrentState);

            Assert.IsFalse(
                _levelFlowDriver.CanAcceptInput);
        }

        [Test]
        public void TryPause_WhenSwapping_ReturnsFalse()
        {
            InitializeWithWagon();

            _levelFlowDriver.TryStartSwap();

            bool paused =
                _levelFlowDriver.TryPause();

            Assert.IsFalse(
                paused);

            Assert.AreEqual(
                LevelState.Swapping,
                _levelFlowDriver.CurrentState);
        }

        [Test]
        public void TryResume_WhenPaused_ChangesStateToPlaying()
        {
            InitializeWithWagon();

            _levelFlowDriver.TryPause();

            bool resumed =
                _levelFlowDriver.TryResume();

            Assert.IsTrue(
                resumed);

            Assert.AreEqual(
                LevelState.Playing,
                _levelFlowDriver.CurrentState);

            Assert.IsTrue(
                _levelFlowDriver.CanAcceptInput);
        }

        [Test]
        public void TryResume_WhenPlaying_ReturnsFalse()
        {
            InitializeWithWagon();

            bool resumed =
                _levelFlowDriver.TryResume();

            Assert.IsFalse(
                resumed);

            Assert.AreEqual(
                LevelState.Playing,
                _levelFlowDriver.CurrentState);
        }

        [Test]
        public void TryFailLevel_WhenPlaying_ChangesStateToFailed()
        {
            InitializeWithWagon();

            bool failed =
                _levelFlowDriver.TryFailLevel();

            Assert.IsTrue(
                failed);

            Assert.AreEqual(
                LevelState.Failed,
                _levelFlowDriver.CurrentState);

            Assert.IsFalse(
                _levelFlowDriver.CanAcceptInput);
        }

        [Test]
        public void TryFailLevel_WhenAlreadyFailed_ReturnsFalse()
        {
            InitializeWithWagon();

            bool firstFailure =
                _levelFlowDriver.TryFailLevel();

            bool secondFailure =
                _levelFlowDriver.TryFailLevel();

            Assert.IsTrue(
                firstFailure);

            Assert.IsFalse(
                secondFailure);

            Assert.AreEqual(
                LevelState.Failed,
                _levelFlowDriver.CurrentState);
        }

        [Test]
        public void TryFailLevel_InvokesLevelFailedOnce()
        {
            InitializeWithWagon();

            int eventCount =
                0;

            _levelFlowDriver.LevelFailed +=
                () =>
                {
                    eventCount++;
                };

            _levelFlowDriver.TryFailLevel();
            _levelFlowDriver.TryFailLevel();

            Assert.AreEqual(
                1,
                eventCount);
        }

        private void InitializeWithWagon()
        {
            _levelFlowDriver.Initialize(
                new List<TrainWagon>
                {
                    _wagon
                });
        }
    }
}