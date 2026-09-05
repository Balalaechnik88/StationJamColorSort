using System.Collections.Generic;
using UnityEngine;

namespace StationJam.Core
{
    public class LevelLayout : MonoBehaviour
    {
        [Header("Wagon Layout Slots")]
        [SerializeField]
        private List<WagonLayoutSlot> _wagonSlots =
            new List<WagonLayoutSlot>();

        public int SlotsCount =>
            _wagonSlots != null
                ? _wagonSlots.Count
                : 0;

        public WagonLayoutSlot GetSlot(
            int wagonIndex)
        {
            if (_wagonSlots == null)
            {
                return null;
            }

            if (wagonIndex < 0 ||
                wagonIndex >= _wagonSlots.Count)
            {
                return null;
            }

            return _wagonSlots[wagonIndex];
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

            if (_wagonSlots == null ||
                _wagonSlots.Count == 0)
            {
                return
                    "LevelLayout: слоты размещения " +
                    "вагонов не назначены.";
            }

            if (requiredWagonsCount >
                _wagonSlots.Count)
            {
                return
                    $"LevelLayout: требуется " +
                    $"{requiredWagonsCount} вагонов, " +
                    $"но доступно только " +
                    $"{_wagonSlots.Count} слотов.";
            }

            for (int slotIndex = 0;
                 slotIndex < requiredWagonsCount;
                 slotIndex++)
            {
                WagonLayoutSlot slot =
                    _wagonSlots[slotIndex];

                if (slot == null)
                {
                    return
                        $"LevelLayout: слот вагона " +
                        $"{slotIndex} не настроен.";
                }

                if (slot.SpawnPoint == null)
                {
                    return
                        $"LevelLayout: точка размещения " +
                        $"{slotIndex} не назначена.";
                }
            }

            return string.Empty;
        }
    }
}