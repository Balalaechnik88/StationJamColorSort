using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using StationJam.Core;
using StationJam.Entities;

namespace StationJam.Tests.EditMode
{
    public class MoveLimitRuleTests
    {
        private GameObject _flowGameObject;
        private GameObject _budgetGameObject;
        private GameObject _ruleGameObject;
        private GameObject _wagonGameObject;

        private LevelFlowDriver _levelFlowDriver;
        private MoveBudget _moveBudget;
        private MoveLimitRule _moveLimitRule;
        private TrainWagon _wagon;

        [SetUp]
        public void SetUp()
        {
            CreateFlow();
            CreateWagon();
            CreateBudget();
            CreateRule();

            _levelFlowDriver.Initialize(
                new List<TrainWagon>
                {
                    _wagon
                });
        }

        [TearDown]
        public void TearDown()
        {
            if (_ruleGameObject != null)
            {
                Object.DestroyImmediate(
                    _ruleGameObject);
            }

            if (_budgetGameObject != null)
            {
                Object.DestroyImmediate(
                    _budgetGameObject);
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
        public void MovesExhausted_WithActiveWagon_FailsLevel()
        {
            Assert.AreEqual(
                WagonState.Active,
                _wagon.CurrentState);

            Assert.IsFalse(
                _levelFlowDriver.IsCompletionPending);

            _moveBudget.Initialize(
                1);

            int exhaustedEventCount =
                0;

            _moveBudget.MovesExhausted +=
                () =>
                {
                    exhaustedEventCount++;
                };

            bool moveConsumed =
                _moveBudget.TryConsumeMove();

            Assert.IsTrue(
                moveConsumed);

            Assert.AreEqual(
                0,
                _moveBudget.RemainingMoves);

            Assert.AreEqual(
                1,
                exhaustedEventCount,
                "MovesExhausted должен быть вызван " +
                "ровно один раз.");

            Assert.AreEqual(
                LevelState.Failed,
                _levelFlowDriver.CurrentState);

            Assert.IsFalse(
                _levelFlowDriver.CanAcceptInput);
        }

        [Test]
        public void MovesExhausted_WhenCompletionIsPending_DoesNotFailLevel()
        {
            SetWagonState(
                WagonState.Departing);

            Assert.AreEqual(
                WagonState.Departing,
                _wagon.CurrentState);

            Assert.IsTrue(
                _levelFlowDriver.IsCompletionPending);

            _moveBudget.Initialize(
                1);

            bool moveConsumed =
                _moveBudget.TryConsumeMove();

            Assert.IsTrue(
                moveConsumed);

            Assert.AreEqual(
                0,
                _moveBudget.RemainingMoves);

            Assert.AreEqual(
                LevelState.Playing,
                _levelFlowDriver.CurrentState);
        }

        [Test]
        public void UnlimitedBudget_DoesNotFailLevel()
        {
            _moveBudget.Initialize(
                0);

            bool moveConsumed =
                _moveBudget.TryConsumeMove();

            Assert.IsFalse(
                moveConsumed);

            Assert.AreEqual(
                LevelState.Playing,
                _levelFlowDriver.CurrentState);
        }

        [Test]
        public void MovesExhausted_WhenLevelIsNotInitialized_DoesNotFailLevel()
        {
            _levelFlowDriver.ResetLevel();

            _moveBudget.Initialize(
                1);

            _moveBudget.TryConsumeMove();

            Assert.AreEqual(
                LevelState.NotInitialized,
                _levelFlowDriver.CurrentState);
        }

        private void CreateFlow()
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
                ColorType.Red,
                1);
        }

        private void CreateBudget()
        {
            _budgetGameObject =
                new GameObject(
                    "MoveBudget");

            _moveBudget =
                _budgetGameObject.AddComponent<
                    MoveBudget>();
        }

        private void CreateRule()
        {
            _ruleGameObject =
                new GameObject(
                    "MoveLimitRule");

            _ruleGameObject.SetActive(
                false);

            _moveLimitRule =
                _ruleGameObject.AddComponent<
                    MoveLimitRule>();

            SerializedObject serializedRule =
                new SerializedObject(
                    _moveLimitRule);

            SetObjectReference(
                serializedRule,
                "_moveBudget",
                _moveBudget);

            SetObjectReference(
                serializedRule,
                "_levelFlowDriver",
                _levelFlowDriver);

            serializedRule
                .ApplyModifiedPropertiesWithoutUndo();

            _ruleGameObject.SetActive(
                true);

            InvokeLifecycleMethod(
                _moveLimitRule,
                "OnEnable");
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

        private void InvokeLifecycleMethod(
            object target,
            string methodName)
        {
            MethodInfo method =
                target.GetType().GetMethod(
                    methodName,
                    BindingFlags.Instance |
                    BindingFlags.NonPublic);

            Assert.IsNotNull(
                method,
                $"Метод {methodName} не найден.");

            method.Invoke(
                target,
                null);
        }
    }
}