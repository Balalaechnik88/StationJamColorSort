using UnityEngine;

namespace StationJam.Core
{
    public class GameplayCameraFraming : MonoBehaviour
    {
        [Header("Scene Reference")]
        [SerializeField]
        private Transform _focusPoint;

        [Header("Reference Screen")]
        [SerializeField]
        private float _referenceWidth = 1920f;

        [SerializeField]
        private float _referenceHeight = 1080f;

        [Header("Settings")]
        [SerializeField]
        private bool _fitOnlyNarrowScreens = true;

        private Vector3 _referenceOffset;
        private Quaternion _referenceRotation;

        private int _lastScreenWidth;
        private int _lastScreenHeight;

        private bool _isInitialized;

        private void Awake()
        {
            Initialize();
        }

        private void OnEnable()
        {
            if (!_isInitialized)
            {
                Initialize();
            }

            ApplyFraming();
        }

        private void LateUpdate()
        {
            if (!_isInitialized)
            {
                return;
            }

            if (_lastScreenWidth == Screen.width &&
                _lastScreenHeight == Screen.height)
            {
                return;
            }

            ApplyFraming();
        }

        private void Initialize()
        {
            if (_focusPoint == null)
            {
                Debug.LogError(
                    "[GameplayCameraFraming] Focus Point " +
                    "не назначен.");

                enabled = false;
                return;
            }

            if (_referenceWidth <= 0f ||
                _referenceHeight <= 0f)
            {
                Debug.LogError(
                    "[GameplayCameraFraming] Reference Width " +
                    "и Reference Height должны быть больше нуля.");

                enabled = false;
                return;
            }

            _referenceOffset =
                transform.position -
                _focusPoint.position;

            _referenceRotation =
                transform.rotation;

            _isInitialized = true;
        }

        private void ApplyFraming()
        {
            if (Screen.width <= 0 ||
                Screen.height <= 0)
            {
                return;
            }

            float referenceAspect =
                _referenceWidth /
                _referenceHeight;

            float currentAspect =
                (float)Screen.width /
                Screen.height;

            float distanceMultiplier =
                referenceAspect /
                currentAspect;

            if (_fitOnlyNarrowScreens)
            {
                distanceMultiplier =
                    Mathf.Max(
                        1f,
                        distanceMultiplier);
            }

            transform.position =
                _focusPoint.position +
                _referenceOffset *
                distanceMultiplier;

            transform.rotation =
                _referenceRotation;

            _lastScreenWidth =
                Screen.width;

            _lastScreenHeight =
                Screen.height;
        }
    }
}