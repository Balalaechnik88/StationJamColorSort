using System.Collections.Generic;
using UnityEngine;
using StationJam.Entities; // Подключаем, чтобы скрипт видел наш ColorType

namespace StationJam.Data
{
    // Класс-настройка для конкретного вагона
    [System.Serializable]
    public class WagonSetup
    {
        [Tooltip("Цвет, который собирает этот вагон")]
        public ColorType TargetColor;

        [Tooltip("Количество мест в вагоне")]
        public int Capacity = 4;

        [Tooltip("Цвета пассажиров, которые сидят в вагоне на старте уровня")]
        public List<ColorType> StartingPassengers = new List<ColorType>();
    }

    // Главный файл настроек всего уровня
    [CreateAssetMenu(fileName = "Level_00", menuName = "StationJam/Level Data", order = 0)]
    public class LevelData : ScriptableObject
    {
        [Header("Level Info")]
        public int LevelNumber;

        [Header("Buffer Settings")]
        [Tooltip("Пассажир, который изначально стоит на перроне в лунке")]
        public ColorType InitialBufferColor;

        [Header("Wagons Configuration")]
        [Tooltip("Список вагонов на уровне")]
        public List<WagonSetup> Wagons = new List<WagonSetup>();
    }
}