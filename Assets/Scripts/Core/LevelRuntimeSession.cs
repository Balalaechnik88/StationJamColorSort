using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using StationJam.Entities;

namespace StationJam.Core
{
    public sealed class LevelRuntimeSession
    {
        private readonly List<Passenger> _passengers =
            new List<Passenger>();

        private readonly List<TrainWagon> _wagons =
            new List<TrainWagon>();

        public void RegisterPassenger(
            Passenger passenger)
        {
            if (passenger == null ||
                _passengers.Contains(passenger))
            {
                return;
            }

            _passengers.Add(
                passenger);
        }

        public void RegisterWagon(
            TrainWagon wagon)
        {
            if (wagon == null ||
                _wagons.Contains(wagon))
            {
                return;
            }

            _wagons.Add(
                wagon);
        }

        public void Clear(
            LevelFlowDriver levelFlowDriver,
            SwapEngine swapEngine)
        {
            if (levelFlowDriver != null)
            {
                levelFlowDriver.ResetLevel();
            }

            if (swapEngine != null)
            {
                swapEngine.InitializeBuffer(
                    null);
            }

            ClearPassengers();
            ClearWagons();
        }

        private void ClearPassengers()
        {
            foreach (Passenger passenger
                     in _passengers)
            {
                if (passenger == null)
                {
                    continue;
                }

                passenger.transform.DOKill();

                passenger.gameObject.SetActive(
                    false);

                Object.Destroy(
                    passenger.gameObject);
            }

            _passengers.Clear();
        }

        private void ClearWagons()
        {
            foreach (TrainWagon wagon
                     in _wagons)
            {
                if (wagon == null)
                {
                    continue;
                }

                wagon.DisableForLevel();

                Object.Destroy(
                    wagon.gameObject);
            }

            _wagons.Clear();
        }
    }
}