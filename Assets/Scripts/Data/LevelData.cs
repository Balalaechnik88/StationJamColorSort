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

        [Header("Level Rules")]
        [Tooltip(
            "Максимальное количество ходов. " +
            "0 означает, что лимит ходов отключён.")]
        [Min(0)]
        public int MovesLimit;

        [Header("Buffer Settings")]
        [Tooltip(
            "Пассажир, который изначально " +
            "находится в транзитном слоте.")]
        public ColorType InitialBufferColor;

        [Header("Wagons Configuration")]
        public List<WagonSetup> Wagons =
            new List<WagonSetup>();
    }
}