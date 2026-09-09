using System.Collections.Generic;
using UnityEngine;
using StationJam.Data;
using StationJam.Entities;

namespace StationJam.Factories
{
    public class WagonFactory : MonoBehaviour
    {
        [Header("Prefab")]
        [SerializeField]
        private TrainWagon _wagonPrefab;

        [Header("Wagon Materials")]
        [SerializeField]
        private List<ColorMaterialMapping> _materialsMap =
            new List<ColorMaterialMapping>();

        public TrainWagon Create(
            ColorType targetColor,
            int capacity,
            TrainWagon.DepartDirection departureDirection,
            Vector3 position,
            Quaternion rotation,
            Transform parent)
        {
            string wagonConfigurationError =
                GetWagonConfigurationError(
                    targetColor,
                    capacity);

            if (!string.IsNullOrEmpty(
                    wagonConfigurationError))
            {
                Debug.LogError(
                    $"[WagonFactory] " +
                    $"{wagonConfigurationError}");

                return null;
            }

            TrainWagon wagon =
                Instantiate(
                    _wagonPrefab,
                    position,
                    rotation,
                    parent);

            Renderer visualRenderer =
                wagon.GetComponentInChildren<
                    Renderer>();

            if (visualRenderer == null)
            {
                Debug.LogError(
                    "[WagonFactory] Renderer вагона " +
                    "не найден.");

                Destroy(
                    wagon.gameObject);

                return null;
            }

            Material material =
                GetMaterialByColor(
                    targetColor);

            visualRenderer.sharedMaterial =
                material;

            wagon.InitializeData(
                targetColor,
                capacity,
                departureDirection);

            return
                wagon;
        }

        public string GetConfigurationError()
        {
            if (_wagonPrefab == null)
            {
                return
                    "WagonFactory: prefab вагона " +
                    "не назначен.";
            }

            if (_wagonPrefab.SeatPoints == null ||
                _wagonPrefab.SeatPoints.Length == 0)
            {
                return
                    "WagonFactory: в prefab вагона " +
                    "не назначены Seat Points.";
            }

            if (_materialsMap == null ||
                _materialsMap.Count == 0)
            {
                return
                    "WagonFactory: список материалов " +
                    "вагонов пуст.";
            }

            foreach (ColorMaterialMapping mapping
                     in _materialsMap)
            {
                if (mapping.Material == null)
                {
                    return
                        $"WagonFactory: материал дл€ цвета " +
                        $"{mapping.Color} не назначен.";
                }
            }

            return
                string.Empty;
        }

        public string GetWagonConfigurationError(
            ColorType targetColor,
            int capacity)
        {
            if (!CanFitCapacity(
                    capacity))
            {
                return
                    $"текущий prefab не поддерживает " +
                    $"вместимость {capacity}.";
            }

            if (GetMaterialByColor(
                    targetColor) == null)
            {
                return
                    $"материал дл€ цвета " +
                    $"{targetColor} не найден.";
            }

            return
                string.Empty;
        }

        public bool CanFitCapacity(
            int capacity)
        {
            if (_wagonPrefab == null ||
                _wagonPrefab.SeatPoints == null)
            {
                return
                    false;
            }

            return
                capacity > 0 &&
                capacity <=
                _wagonPrefab.SeatPoints.Length;
        }

        private Material GetMaterialByColor(
            ColorType color)
        {
            if (_materialsMap == null)
            {
                return null;
            }

            foreach (ColorMaterialMapping mapping
                     in _materialsMap)
            {
                if (mapping.Color == color &&
                    mapping.Material != null)
                {
                    return
                        mapping.Material;
                }
            }

            return
                null;
        }
    }
}