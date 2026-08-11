using System.Collections.Generic;
using UnityEngine;
using StationJam.Data;
using StationJam.Entities;

namespace StationJam.Factories
{
    public class PassengerFactory : MonoBehaviour
    {
        [Header("Prefab")]
        [SerializeField]
        private Passenger _passengerPrefab;

        [Header("Color Materials")]
        [SerializeField]
        private List<ColorMaterialMapping> _materialsMap =
            new List<ColorMaterialMapping>();

        public IReadOnlyList<ColorMaterialMapping> MaterialsMap =>
            _materialsMap;

        public Passenger Create(
            ColorType color,
            Vector3 position)
        {
            if (_passengerPrefab == null)
            {
                Debug.LogError(
                    "[PassengerFactory] Passenger prefab не назначен.");

                return null;
            }

            Material material =
                GetMaterialByColor(color);

            if (material == null)
            {
                Debug.LogError(
                    $"[PassengerFactory] Материал для цвета " +
                    $"{color} не найден.");

                return null;
            }

            Passenger passenger =
                Instantiate(
                    _passengerPrefab,
                    position,
                    Quaternion.identity);

            passenger.SetData(
                color,
                material);

            return passenger;
        }

        public string GetConfigurationError()
        {
            if (_passengerPrefab == null)
            {
                return
                    "PassengerFactory: префаб пассажира не назначен.";
            }

            if (_materialsMap == null ||
                _materialsMap.Count == 0)
            {
                return
                    "PassengerFactory: список материалов цветов пуст.";
            }

            foreach (ColorMaterialMapping mapping
                     in _materialsMap)
            {
                if (mapping.Material == null)
                {
                    return
                        $"PassengerFactory: материал для цвета " +
                        $"{mapping.Color} не назначен.";
                }
            }

            return string.Empty;
        }

        private Material GetMaterialByColor(
            ColorType color)
        {
            foreach (ColorMaterialMapping mapping
                     in _materialsMap)
            {
                if (mapping.Color == color)
                {
                    return mapping.Material;
                }
            }

            return null;
        }
    }
}