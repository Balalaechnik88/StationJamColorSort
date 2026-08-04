using UnityEngine;
using UnityEngine.EventSystems;
using StationJam.Core;
using StationJam.Entities;

namespace StationJam.InputSystem
{
    public class PlayerInputReader : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private SwapEngine _swapEngine;

        [Header("Settings")]
        [SerializeField] private LayerMask _passengerLayer;
        [SerializeField] private float _rayDistance = 100f;

        private Camera _mainCamera;

        private void Awake()
        {
            _mainCamera = Camera.main;
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                ProcessClick();
            }
        }

        private void ProcessClick()
        {
            // 1. Защита от пробития UI: игнорируем клик, если курсор над элементом интерфейса
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }

            Ray ray = _mainCamera.ScreenPointToRay(Input.mousePosition);

            // 2. Пускаем луч ограниченной длины и ТОЛЬКО по выбранному слою
            if (Physics.Raycast(ray, out RaycastHit hit, _rayDistance, _passengerLayer))
            {
                // 3. Безопасное извлечение компонента (TryGetComponent быстрее и чище)
                if (hit.collider.TryGetComponent(out Passenger clickedPassenger))
                {
                    _swapEngine.ProcessSwap(clickedPassenger);
                }
            }
        }
    }
}