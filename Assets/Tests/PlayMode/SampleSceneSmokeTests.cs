using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using StationJam.Core;
using StationJam.Data;
using StationJam.Entities;

namespace StationJam.Tests.PlayMode
{
    public class SampleSceneSmokeTests
    {
        private const string SceneName =
            "SampleScene";

        [UnityTest]
        public IEnumerator SampleScene_LoadsFirstLevel_AndRestartRebuildsIt()
        {
            yield return SceneManager.LoadSceneAsync(
                SceneName,
                LoadSceneMode.Single);

            yield return null;
            yield return null;

            LevelSequence levelSequence =
                Object.FindObjectOfType<
                    LevelSequence>();

            LevelFlowDriver levelFlowDriver =
                Object.FindObjectOfType<
                    LevelFlowDriver>();

            LevelLayout levelLayout =
                Object.FindObjectOfType<
                    LevelLayout>();

            SwapEngine swapEngine =
                Object.FindObjectOfType<
                    SwapEngine>();

            Assert.IsNotNull(
                levelSequence,
                "В SampleScene не найден LevelSequence.");

            Assert.IsNotNull(
                levelFlowDriver,
                "В SampleScene не найден LevelFlowDriver.");

            Assert.IsNotNull(
                levelLayout,
                "В SampleScene не найден LevelLayout.");

            Assert.IsNotNull(
                swapEngine,
                "В SampleScene не найден SwapEngine.");

            Assert.AreEqual(
                0,
                levelSequence.CurrentLevelIndex,
                "После загрузки SampleScene должен " +
                "быть запущен первый уровень каталога.");

            Assert.AreEqual(
                LevelState.Playing,
                levelFlowDriver.CurrentState,
                "После построения первого уровня " +
                "LevelFlowDriver должен находиться " +
                "в состоянии Playing.");

            LevelData currentLevel =
                GetCurrentLevelData(
                    levelSequence);

            Assert.IsNotNull(
                currentLevel,
                "Не удалось получить текущий LevelData.");

            int expectedWagonCount =
                currentLevel.Wagons.Count;

            int expectedPassengerCount =
                GetExpectedPassengerCount(
                    currentLevel);

            TrainWagon[] firstBuildWagons =
                Object.FindObjectsOfType<
                    TrainWagon>();

            Passenger[] firstBuildPassengers =
                Object.FindObjectsOfType<
                    Passenger>();

            Assert.AreEqual(
                expectedWagonCount,
                firstBuildWagons.Length,
                "Количество созданных вагонов " +
                "не совпадает с LevelData.");

            Assert.AreEqual(
                expectedPassengerCount,
                firstBuildPassengers.Length,
                "Количество созданных пассажиров " +
                "не совпадает с LevelData.");

            AssertWagonsBelongToLayout(
                firstBuildWagons,
                levelLayout);

            Passenger passengerInBuffer =
                GetPassengerInBuffer(
                    swapEngine);

            Assert.IsNotNull(
                passengerInBuffer,
                "После построения уровня в буфере " +
                "должен находиться пассажир.");

            HashSet<int> firstWagonInstanceIds =
                firstBuildWagons
                    .Select(
                        wagon =>
                            wagon.GetInstanceID())
                    .ToHashSet();

            levelSequence.RestartCurrentLevel();

            yield return null;
            yield return null;

            Assert.AreEqual(
                0,
                levelSequence.CurrentLevelIndex,
                "RestartCurrentLevel не должен " +
                "изменять индекс текущего уровня.");

            Assert.AreEqual(
                LevelState.Playing,
                levelFlowDriver.CurrentState,
                "После RestartCurrentLevel уровень " +
                "должен снова находиться в Playing.");

            TrainWagon[] restartedWagons =
                Object.FindObjectsOfType<
                    TrainWagon>();

            Passenger[] restartedPassengers =
                Object.FindObjectsOfType<
                    Passenger>();

            Assert.AreEqual(
                expectedWagonCount,
                restartedWagons.Length,
                "После restart количество вагонов " +
                "должно остаться прежним.");

            Assert.AreEqual(
                expectedPassengerCount,
                restartedPassengers.Length,
                "После restart количество пассажиров " +
                "должно остаться прежним.");

            AssertWagonsBelongToLayout(
                restartedWagons,
                levelLayout);

            bool allWagonsWereRecreated =
                restartedWagons.All(
                    wagon =>
                        !firstWagonInstanceIds.Contains(
                            wagon.GetInstanceID()));

            Assert.IsTrue(
                allWagonsWereRecreated,
                "После RestartCurrentLevel должны быть " +
                "созданы новые экземпляры вагонов, " +
                "а старые должны быть очищены.");

            passengerInBuffer =
                GetPassengerInBuffer(
                    swapEngine);

            Assert.IsNotNull(
                passengerInBuffer,
                "После restart пассажир в буфере " +
                "должен быть создан заново.");
        }

        private LevelData GetCurrentLevelData(
            LevelSequence levelSequence)
        {
            FieldInfo catalogField =
                typeof(LevelSequence).GetField(
                    "_levelCatalog",
                    BindingFlags.Instance |
                    BindingFlags.NonPublic);

            Assert.IsNotNull(
                catalogField,
                "Поле _levelCatalog не найдено " +
                "в LevelSequence.");

            LevelCatalog levelCatalog =
                catalogField.GetValue(
                    levelSequence)
                as LevelCatalog;

            Assert.IsNotNull(
                levelCatalog,
                "LevelCatalog не назначен " +
                "в LevelSequence.");

            return
                levelCatalog.GetLevel(
                    levelSequence.CurrentLevelIndex);
        }

        private int GetExpectedPassengerCount(
            LevelData levelData)
        {
            int passengerCount =
                1;

            foreach (WagonSetup wagonSetup
                     in levelData.Wagons)
            {
                passengerCount +=
                    wagonSetup.StartingPassengers.Count;
            }

            return
                passengerCount;
        }

        private Passenger GetPassengerInBuffer(
            SwapEngine swapEngine)
        {
            FieldInfo bufferField =
                typeof(SwapEngine).GetField(
                    "_passengerInBuffer",
                    BindingFlags.Instance |
                    BindingFlags.NonPublic);

            Assert.IsNotNull(
                bufferField,
                "Поле _passengerInBuffer " +
                "не найдено в SwapEngine.");

            return
                bufferField.GetValue(
                    swapEngine)
                as Passenger;
        }

        private void AssertWagonsBelongToLayout(
            IEnumerable<TrainWagon> wagons,
            LevelLayout levelLayout)
        {
            foreach (TrainWagon wagon
                     in wagons)
            {
                Assert.IsNotNull(
                    wagon);

                Assert.AreSame(
                    levelLayout.transform,
                    wagon.transform.parent,
                    $"Вагон '{wagon.gameObject.name}' " +
                    "создан вне WagonLayout.");

                Assert.IsTrue(
                    wagon.gameObject.activeInHierarchy,
                    $"Вагон '{wagon.gameObject.name}' " +
                    "должен быть активен.");

                Assert.AreEqual(
                    WagonState.Active,
                    wagon.CurrentState,
                    $"Вагон '{wagon.gameObject.name}' " +
                    "не находится в состоянии Active.");
            }
        }
    }
}