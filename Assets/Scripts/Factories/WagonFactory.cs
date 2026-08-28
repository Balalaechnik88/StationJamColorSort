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
            Vector3 position,
            Quaternion rotation,
            Transform parent)
        {
            if (_wagonPrefab == null)
            {
                Debug.LogError(
                    "[WagonFactory] Wagon prefab не назначен.");

                return null;
            }

            Material material =
                GetMaterialByColor(
                    targetColor);

            if (material == null)
            {
                Debug.LogError(
                    $"[WagonFactory] Материал для цвета " +
                    $"{targetColor} не найден.");

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

            visualRenderer.sharedMaterial =
                material;

            wagon.InitializeData(
                targetColor,
                capacity);

            return wagon;
        }

        public string GetConfigurationError()
        {
            if (_wagonPrefab == null)
            {
                return
                    "WagonFactory: prefab вагона не назначен.";
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
                        $"WagonFactory: материал для цвета " +
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