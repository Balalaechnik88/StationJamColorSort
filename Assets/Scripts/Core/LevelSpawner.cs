using System.Collections.Generic;
using UnityEngine;
using StationJam.Data;
using StationJam.Entities;
using StationJam.Factories;

namespace StationJam.Core
{
    public sealed class LevelSpawner
    {
        private readonly LevelLayout _levelLayout;
        private readonly PassengerFactory _passengerFactory;
        private readonly WagonFactory _wagonFactory;
        private readonly LevelRuntimeSession _runtimeSession;

        public LevelSpawner(
            LevelLayout levelLayout,
            PassengerFactory passengerFactory,
            WagonFactory wagonFactory,
            LevelRuntimeSession runtimeSession)
        {
            _levelLayout =
                levelLayout;

            _passengerFactory =
                passengerFactory;

            _wagonFactory =
                wagonFactory;

            _runtimeSession =
                runtimeSession;
        }

        public bool TrySpawn(
            LevelData levelData,
            Vector3 bufferPosition,
            out LevelSpawnResult spawnResult)
        {
            spawnResult =
                null;

            Passenger bufferPassenger =
                CreatePassenger(
                    levelData.InitialBufferColor,
                    bufferPosition);

            if (bufferPassenger == null)
            {
                Debug.LogError(
                    "[LevelSpawner] Ќе удалось создать " +
                    "пассажира дл€ транзитного слота.");

                return false;
            }

            List<TrainWagon> wagons =
                BuildWagons(
                    levelData);

            if (wagons == null)
            {
                Debug.LogError(
                    "[LevelSpawner] Ќе удалось полностью " +
                    "создать вагоны уровн€.");

                return false;
            }

            spawnResult =
                new LevelSpawnResult(
                    bufferPassenger,
                    wagons);

            return true;
        }

        private List<TrainWagon> BuildWagons(
            LevelData levelData)
        {
            List<TrainWagon> wagons =
                new List<TrainWagon>();

            for (int wagonIndex = 0;
                 wagonIndex < levelData.Wagons.Count;
                 wagonIndex++)
            {
                WagonSetup wagonSetup =
                    levelData.Wagons[
                        wagonIndex];

                WagonLayoutSlot layoutSlot =
                    _levelLayout.GetSlot(
                        wagonIndex);

                if (layoutSlot == null ||
                    layoutSlot.SpawnPoint == null)
                {
                    Debug.LogError(
                        $"[LevelSpawner] Layout-слот " +
                        $"{wagonIndex} недоступен.");

                    return null;
                }

                TrainWagon wagon =
                    CreateWagon(
                        wagonSetup,
                        layoutSlot);

                if (wagon == null)
                {
                    return null;
                }

                bool wagonFilled =
                    FillWagon(
                        wagon,
                        wagonSetup);

                if (!wagonFilled)
                {
                    return null;
                }

                wagons.Add(
                    wagon);
            }

            return wagons;
        }

        private TrainWagon CreateWagon(
            WagonSetup wagonSetup,
            WagonLayoutSlot layoutSlot)
        {
            TrainWagon wagon =
                _wagonFactory.Create(
                    wagonSetup.TargetColor,
                    wagonSetup.Capacity,
                    layoutSlot.DepartureDirection,
                    layoutSlot.SpawnPoint.position,
                    layoutSlot.SpawnPoint.rotation,
                    _levelLayout.transform);

            if (wagon == null)
            {
                Debug.LogError(
                    "[LevelSpawner] WagonFactory " +
                    "не смогла создать вагон.");

                return null;
            }

            _runtimeSession.RegisterWagon(
                wagon);

            return wagon;
        }

        private bool FillWagon(
            TrainWagon wagon,
            WagonSetup wagonSetup)
        {
            for (int passengerIndex = 0;
                 passengerIndex <
                 wagonSetup.StartingPassengers.Count;
                 passengerIndex++)
            {
                bool passengerAdded =
                    CreatePassengerInWagon(
                        wagon,
                        wagonSetup.StartingPassengers[
                            passengerIndex],
                        passengerIndex);

                if (!passengerAdded)
                {
                    return false;
                }
            }

            return true;
        }

        private bool CreatePassengerInWagon(
            TrainWagon wagon,
            ColorType passengerColor,
            int seatIndex)
        {
            if (wagon.SeatPoints == null ||
                seatIndex < 0 ||
                seatIndex >= wagon.SeatPoints.Length)
            {
                Debug.LogError(
                    $"[LevelSpawner] ћесто {seatIndex} " +
                    $"недоступно в вагоне " +
                    $"'{wagon.gameObject.name}'.");

                return false;
            }

            Transform seatPoint =
                wagon.SeatPoints[
                    seatIndex];

            if (seatPoint == null)
            {
                Debug.LogError(
                    $"[LevelSpawner] SeatPoint " +
                    $"{seatIndex} не назначен в вагоне " +
                    $"'{wagon.gameObject.name}'.");

                return false;
            }

            Passenger passenger =
                CreatePassenger(
                    passengerColor,
                    seatPoint.position);

            if (passenger == null)
            {
                Debug.LogError(
                    $"[LevelSpawner] Ќе удалось создать " +
                    $"пассажира дл€ места {seatIndex} " +
                    $"вагона '{wagon.gameObject.name}'.");

                return false;
            }

            bool passengerAdded =
                wagon.TryAddPassenger(
                    passenger);

            if (!passengerAdded)
            {
                Debug.LogError(
                    $"[LevelSpawner] Ќе удалось добавить " +
                    $"пассажира на место {seatIndex} " +
                    $"вагона '{wagon.gameObject.name}'.");

                return false;
            }

            return true;
        }

        private Passenger CreatePassenger(
            ColorType color,
            Vector3 position)
        {
            Passenger passenger =
                _passengerFactory.Create(
                    color,
                    position);

            if (passenger == null)
            {
                return null;
            }

            _runtimeSession.RegisterPassenger(
                passenger);

            return passenger;
        }
    }
}