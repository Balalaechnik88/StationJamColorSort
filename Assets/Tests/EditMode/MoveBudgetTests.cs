using NUnit.Framework;
using UnityEngine;
using StationJam.Core;

namespace StationJam.Tests.EditMode
{
    public class MoveBudgetTests
    {
        private GameObject _gameObject;

        private MoveBudget _moveBudget;

        [SetUp]
        public void SetUp()
        {
            _gameObject =
                new GameObject(
                    "MoveBudget");

            _moveBudget =
                _gameObject.AddComponent<
                    MoveBudget>();
        }

        [TearDown]
        public void TearDown()
        {
            if (_gameObject != null)
            {
                Object.DestroyImmediate(
                    _gameObject);
            }
        }

        [Test]
        public void InitialState_HasNoLimitAndZeroRemainingMoves()
        {
            Assert.IsFalse(
                _moveBudget.HasLimit);

            Assert.AreEqual(
                0,
                _moveBudget.RemainingMoves);
        }

        [Test]
        public void Initialize_WithZero_DisablesLimit()
        {
            _moveBudget.Initialize(
                0);

            Assert.IsFalse(
                _moveBudget.HasLimit);

            Assert.AreEqual(
                0,
                _moveBudget.RemainingMoves);
        }

        [Test]
        public void Initialize_WithPositiveValue_SetsRemainingMoves()
        {
            _moveBudget.Initialize(
                10);

            Assert.IsTrue(
                _moveBudget.HasLimit);

            Assert.AreEqual(
                10,
                _moveBudget.RemainingMoves);
        }

        [Test]
        public void TryConsumeMove_WithActiveLimit_DecreasesRemainingMoves()
        {
            _moveBudget.Initialize(
                3);

            bool moveConsumed =
                _moveBudget.TryConsumeMove();

            Assert.IsTrue(
                moveConsumed);

            Assert.AreEqual(
                2,
                _moveBudget.RemainingMoves);
        }

        [Test]
        public void TryConsumeMove_WhenLimitDisabled_ReturnsFalse()
        {
            _moveBudget.Initialize(
                0);

            bool moveConsumed =
                _moveBudget.TryConsumeMove();

            Assert.IsFalse(
                moveConsumed);

            Assert.AreEqual(
                0,
                _moveBudget.RemainingMoves);
        }

        [Test]
        public void TryConsumeMove_WhenLastMoveConsumed_InvokesMovesExhausted()
        {
            _moveBudget.Initialize(
                1);

            int exhaustedEventCount =
                0;

            _moveBudget.MovesExhausted +=
                () =>
                {
                    exhaustedEventCount++;
                };

            _moveBudget.TryConsumeMove();

            Assert.AreEqual(
                0,
                _moveBudget.RemainingMoves);

            Assert.AreEqual(
                1,
                exhaustedEventCount);
        }

        [Test]
        public void TryConsumeMove_AfterExhaustion_DoesNotGoBelowZero()
        {
            _moveBudget.Initialize(
                1);

            bool firstMoveConsumed =
                _moveBudget.TryConsumeMove();

            bool secondMoveConsumed =
                _moveBudget.TryConsumeMove();

            Assert.IsTrue(
                firstMoveConsumed);

            Assert.IsFalse(
                secondMoveConsumed);

            Assert.AreEqual(
                0,
                _moveBudget.RemainingMoves);
        }

        [Test]
        public void Initialize_AfterMovesWereConsumed_RestoresFullLimit()
        {
            _moveBudget.Initialize(
                3);

            _moveBudget.TryConsumeMove();
            _moveBudget.TryConsumeMove();

            _moveBudget.Initialize(
                5);

            Assert.IsTrue(
                _moveBudget.HasLimit);

            Assert.AreEqual(
                5,
                _moveBudget.RemainingMoves);
        }
    }
}