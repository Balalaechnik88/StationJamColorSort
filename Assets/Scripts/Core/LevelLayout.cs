using System.Collections.Generic;
using UnityEngine;

namespace StationJam.Core
{
    public class LevelLayout : MonoBehaviour
    {
        [Header("Wagon Spawn Points")]
        [SerializeField]
        private List<Transform> _wagonSpawnPoints =
            new List<Transform>();

        public int SpawnPointsCount =>
            _wagonSpawnPoints != null
                ? _wagonSpawnPoints.Count
                : 0;

        public Transform GetSpawnPoint(
            int wagonIndex)
        {
            if (_wagonSpawnPoints == null)
            {
                return null;
            }

            if (wagonIndex < 0 ||
                wagonIndex >= _wagonSpawnPoints.Count)
            {
                return null;
            }

            return _wagonSpawnPoints[wagonIndex];
        }

        public string GetConfigurationError(
            int requiredWagonsCount)
        {
            if (requiredWagonsCount <= 0)
            {
                return
                    "LevelLayout: количество вагонов " +
                    "должно быть больше нуля.";
            }

            if (_wagonSpawnPoints == null ||
                _wagonSpawnPoints.Count == 0)
            {
                return
                    "LevelLayout: точки размещения " +
                    "вагонов не назначены.";
            }

            if (requiredWagonsCount >
                _wagonSpawnPoints.Count)
            {
                return
                    $"LevelLayout: требуется " +
                    $"{requiredWagonsCount} вагонов, " +
                    $"но доступно только " +
                    $"{_wagonSpawnPoints.Count} " +
                    "точек размещения.";
            }

            for (int spawnPointIndex = 0;
                 spawnPointIndex < requiredWagonsCount;
                 spawnPointIndex++)
            {
                if (_wagonSpawnPoints[
                        spawnPointIndex] == null)
                {
                    return
                        $"LevelLayout: точка размещения " +
                        $"{spawnPointIndex} не назначена.";
                }
            }

            return string.Empty;
        }
    }
}