using System.Collections.Generic;
using UnityEngine;

namespace StationJam.Data
{
    [CreateAssetMenu(
        fileName = "LevelCatalog",
        menuName = "StationJam/Level Catalog")]
    public class LevelCatalog : ScriptableObject
    {
        [SerializeField]
        private List<LevelData> _levels =
            new List<LevelData>();

        public IReadOnlyList<LevelData> Levels =>
            _levels;

        public int Count =>
            _levels != null
                ? _levels.Count
                : 0;

        public LevelData GetLevel(
            int index)
        {
            if (_levels == null ||
                index < 0 ||
                index >= _levels.Count)
            {
                return null;
            }

            return
                _levels[index];
        }

        public bool HasLevel(
            int index)
        {
            return
                _levels != null &&
                index >= 0 &&
                index < _levels.Count;
        }
    }
}