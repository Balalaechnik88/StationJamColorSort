using UnityEngine;
using UnityEngine.EventSystems;
using StationJam.Core;
using StationJam.Entities;

namespace StationJam.InputSystem
{
    public class PlayerInputReader : MonoBehaviour
    {
        [Header("References")]
        [SerializeField]
        private SwapEngine _swapEngine;

        [SerializeField]
        private LevelFlowDriver _levelFlowDriver;

        [Header("Settings")]
        [SerializeField]
        private LayerMask _passengerLayer;

        [SerializeField]
        private float _rayDistance = 100f;

        private Camera _mainCamera;

        private void Awake()
        {
            _mainCamera = Camera.main;

            if (_mainCamera == null)
            {
                Debug.LogError(
                    "[PlayerInputReader] Main Camera не найдена.");
            }
        }

        private void Update()
        {
            if (_levelFlowDriver == null ||
                !_levelFlowDriver.CanAcceptInput)
            {
                return;
            }

            if (Input.GetMouseButtonDown(0))
            {
                ProcessClick();
            }
        }

        private void ProcessClick()
        {
            if (_mainCamera == null ||
                _swapEngine == null)
            {
                return;
            }

            if (EventSystem.current != null &&
                EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }

            Ray ray = _mainCamera.ScreenPointToRay(
                Input.mousePosition);

            bool passengerHit = Physics.Raycast(
                ray,
                out RaycastHit hit,
                _rayDistance,
                _passengerLayer);

            if (!passengerHit)
            {
                return;
            }

            if (hit.collider.TryGetComponent(
                    out Passenger clickedPassenger))
            {
                _swapEngine.ProcessSwap(clickedPassenger);
            }
        }
    }
}