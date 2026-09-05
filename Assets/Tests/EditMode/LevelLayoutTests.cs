using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using StationJam.Core;
using StationJam.Entities;

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
        }

        [Test]
        public void GetConfigurationError_WhenSlotsAreEmpty_ReturnsError()
        {
            string configurationError =
                _levelLayout.GetConfigurationError(
                    1);

            Assert.AreEqual(
                "LevelLayout: слоты размещения " +
                "вагонов не назначены.",
                configurationError);
        }

        [Test]
        public void GetConfigurationError_WhenRequiredCountIsZero_ReturnsError()
        {
            AssignForwardSlots(
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
        public void GetConfigurationError_WhenNotEnoughSlots_ReturnsError()
        {
            AssignForwardSlots(
                _firstSpawnPoint);

            string configurationError =
                _levelLayout.GetConfigurationError(
                    2);

            Assert.AreEqual(
                "LevelLayout: требуется 2 вагонов, " +
                "но доступно только 1 слотов.",
                configurationError);
        }

        [Test]
        public void GetConfigurationError_WhenRequiredSlotIsNull_ReturnsError()
        {
            AssignSlotsWithNullSecondSlot(
                _firstSpawnPoint);

            string configurationError =
                _levelLayout.GetConfigurationError(
                    2);

            Assert.AreEqual(
                "LevelLayout: слот вагона 1 " +
                "не настроен.",
                configurationError);
        }

        [Test]
        public void GetConfigurationError_WhenSpawnPointIsNull_ReturnsError()
        {
            AssignSlot(
                0,
                null,
                TrainWagon.DepartDirection.Forward,
                1);

            string configurationError =
                _levelLayout.GetConfigurationError(
                    1);

            Assert.AreEqual(
                "LevelLayout: точка размещения 0 " +
                "не назначена.",
                configurationError);
        }

        [Test]
        public void GetConfigurationError_WhenConfigurationIsValid_ReturnsEmptyString()
        {
            AssignForwardSlots(
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
        public void GetSlot_WithValidIndex_ReturnsExpectedSpawnPointAndDirection()
        {
            AssignSingleSlot(
                _firstSpawnPoint,
                TrainWagon.DepartDirection.Backward);

            WagonLayoutSlot slot =
                _levelLayout.GetSlot(
                    0);

            Assert.IsNotNull(
                slot);

            Assert.AreSame(
                _firstSpawnPoint,
                slot.SpawnPoint);

            Assert.AreEqual(
                TrainWagon.DepartDirection.Backward,
                slot.DepartureDirection);
        }

        [Test]
        public void GetSlot_WithInvalidIndex_ReturnsNull()
        {
            AssignForwardSlots(
                _firstSpawnPoint);

            WagonLayoutSlot slot =
                _levelLayout.GetSlot(
                    5);

            Assert.IsNull(
                slot);
        }

        private void AssignForwardSlots(
            params Transform[] spawnPoints)
        {
            SerializedObject serializedLayout =
                CreateSerializedLayout(
                    spawnPoints.Length);

            SerializedProperty slotsProperty =
                serializedLayout.FindProperty(
                    "_wagonSlots");

            for (int slotIndex = 0;
                 slotIndex < spawnPoints.Length;
                 slotIndex++)
            {
                SetSlotValues(
                    slotsProperty,
                    slotIndex,
                    spawnPoints[slotIndex],
                    TrainWagon.DepartDirection.Forward);
            }

            serializedLayout
                .ApplyModifiedPropertiesWithoutUndo();
        }

        private void AssignSingleSlot(
            Transform spawnPoint,
            TrainWagon.DepartDirection departureDirection)
        {
            SerializedObject serializedLayout =
                CreateSerializedLayout(1);

            SerializedProperty slotsProperty =
                serializedLayout.FindProperty(
                    "_wagonSlots");

            SetSlotValues(
                slotsProperty,
                0,
                spawnPoint,
                departureDirection);

            serializedLayout
                .ApplyModifiedPropertiesWithoutUndo();
        }

        private void AssignSlotsWithNullSecondSlot(
            Transform firstSpawnPoint)
        {
            WagonLayoutSlot firstSlot =
                CreateSlot(
                    firstSpawnPoint,
                    TrainWagon.DepartDirection.Forward);

            List<WagonLayoutSlot> slots =
                new List<WagonLayoutSlot>
                {
                    firstSlot,
                    null
                };

            FieldInfo slotsField =
                typeof(LevelLayout).GetField(
                    "_wagonSlots",
                    BindingFlags.Instance |
                    BindingFlags.NonPublic);

            Assert.IsNotNull(
                slotsField,
                "Поле _wagonSlots не найдено " +
                "в LevelLayout.");

            slotsField.SetValue(
                _levelLayout,
                slots);
        }

        private WagonLayoutSlot CreateSlot(
            Transform spawnPoint,
            TrainWagon.DepartDirection departureDirection)
        {
            WagonLayoutSlot slot =
                new WagonLayoutSlot();

            FieldInfo spawnPointField =
                typeof(WagonLayoutSlot).GetField(
                    "_spawnPoint",
                    BindingFlags.Instance |
                    BindingFlags.NonPublic);

            FieldInfo directionField =
                typeof(WagonLayoutSlot).GetField(
                    "_departureDirection",
                    BindingFlags.Instance |
                    BindingFlags.NonPublic);

            Assert.IsNotNull(
                spawnPointField,
                "Поле _spawnPoint не найдено " +
                "в WagonLayoutSlot.");

            Assert.IsNotNull(
                directionField,
                "Поле _departureDirection не найдено " +
                "в WagonLayoutSlot.");

            spawnPointField.SetValue(
                slot,
                spawnPoint);

            directionField.SetValue(
                slot,
                departureDirection);

            return slot;
        }

        private void AssignSlot(
            int slotIndex,
            Transform spawnPoint,
            TrainWagon.DepartDirection departureDirection,
            int slotsCount)
        {
            SerializedObject serializedLayout =
                CreateSerializedLayout(
                    slotsCount);

            SerializedProperty slotsProperty =
                serializedLayout.FindProperty(
                    "_wagonSlots");

            SetSlotValues(
                slotsProperty,
                slotIndex,
                spawnPoint,
                departureDirection);

            serializedLayout
                .ApplyModifiedPropertiesWithoutUndo();
        }

        private SerializedObject CreateSerializedLayout(
            int slotsCount)
        {
            SerializedObject serializedLayout =
                new SerializedObject(
                    _levelLayout);

            SerializedProperty slotsProperty =
                serializedLayout.FindProperty(
                    "_wagonSlots");

            Assert.IsNotNull(
                slotsProperty,
                "Поле _wagonSlots не найдено " +
                "в LevelLayout.");

            slotsProperty.arraySize =
                slotsCount;

            return serializedLayout;
        }

        private void SetSlotValues(
            SerializedProperty slotsProperty,
            int slotIndex,
            Transform spawnPoint,
            TrainWagon.DepartDirection departureDirection)
        {
            SerializedProperty slotProperty =
                slotsProperty.GetArrayElementAtIndex(
                    slotIndex);

            SerializedProperty spawnPointProperty =
                slotProperty.FindPropertyRelative(
                    "_spawnPoint");

            SerializedProperty directionProperty =
                slotProperty.FindPropertyRelative(
                    "_departureDirection");

            Assert.IsNotNull(
                spawnPointProperty);

            Assert.IsNotNull(
                directionProperty);

            spawnPointProperty.objectReferenceValue =
                spawnPoint;

            directionProperty.enumValueIndex =
                (int)departureDirection;
        }
    }
}