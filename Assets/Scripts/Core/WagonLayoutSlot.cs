using System;
using UnityEngine;
using StationJam.Entities;

namespace StationJam.Core
{
    [Serializable]
    public class WagonLayoutSlot
    {
        [SerializeField]
        private Transform _spawnPoint;

        [SerializeField]
        private TrainWagon.DepartDirection _departureDirection =
            TrainWagon.DepartDirection.Forward;

        public Transform SpawnPoint =>
            _spawnPoint;

        public TrainWagon.DepartDirection DepartureDirection =>
            _departureDirection;
    }
}