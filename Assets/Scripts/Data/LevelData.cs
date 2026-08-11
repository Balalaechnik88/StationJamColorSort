using System.Collections.Generic;
using UnityEngine;
using StationJam.Entities;

namespace StationJam.Data
{
    [CreateAssetMenu(
        fileName = "Level_00",
        menuName = "StationJam/Level Data",
        order = 0)]
    public class LevelData : ScriptableObject
    {
        [Header("Level Info")]
        public int LevelNumber;

        [Header("Buffer Settings")]
        [Tooltip(
            "Пассажир, который изначально стоит " +
            "в транзитном слоте")]
        public ColorType InitialBufferColor;

        [Header("Wagons Configuration")]
        [Tooltip("Список вагонов на уровне")]
        public List<WagonSetup> Wagons =
            new List<WagonSetup>();
    }
}