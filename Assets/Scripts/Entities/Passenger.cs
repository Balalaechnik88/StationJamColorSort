using UnityEngine;

namespace StationJam.Entities
{
    public class Passenger : MonoBehaviour
    {
        [Header("Passenger Data")]
        public ColorType PassengerColor;

        [Header("Visuals")]
        [SerializeField] private Animator _animator;
        [SerializeField] private SkinnedMeshRenderer _meshRenderer;

        private const string IdleStateName = "Idle";
        private const string JumpStateName = "Jump";

        public Vector3 SeatPosition { get; set; }
        public TrainWagon CurrentWagon { get; set; }

        private void Awake()
        {
            if (_animator == null)
            {
                _animator = GetComponent<Animator>();
            }

            if (_meshRenderer == null)
            {
                _meshRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
            }

            // ЖЕСТКИЙ ФИКС: Отключаем Root Motion, чтобы анимации не ломали координаты
            if (_animator != null)
            {
                _animator.applyRootMotion = false;
            }
        }

        public void SetData(ColorType color, Material material)
        {
            PassengerColor = color;
            if (_meshRenderer != null && material != null)
            {
                _meshRenderer.material = material;
            }
        }

        public void PlayJump()
        {
            if (_animator != null)
            {
                _animator.Play(JumpStateName);
            }
        }

        public void PlayIdle()
        {
            if (_animator != null)
            {
                _animator.CrossFade(IdleStateName, 0.2f);
            }
        }
    }
}