using UnityEngine;

namespace StationJam.Entities
{
    public class Passenger : MonoBehaviour
    {
        private const string IdleStateName = "Idle";
        private const string JumpStateName = "Jump";
        private const float IdleTransitionDuration = 0.2f;

        [Header("Passenger Data")]
        public ColorType PassengerColor;

        [Header("Visuals")]
        [SerializeField]
        private Animator _animator;

        [SerializeField]
        private SkinnedMeshRenderer _meshRenderer;

        public Vector3 SeatPosition { get; set; }

        public TrainWagon CurrentWagon { get; set; }

        private void Awake()
        {
            InitializeAnimator();
            InitializeMeshRenderer();
        }

        public void SetData(
            ColorType color,
            Material material)
        {
            PassengerColor = color;

            if (_meshRenderer != null &&
                material != null)
            {
                _meshRenderer.material =
                    material;
            }
        }

        public void PlayJump()
        {
            if (_animator == null)
            {
                return;
            }

            _animator.Play(
                JumpStateName,
                0,
                0f);
        }

        public void PlayIdle()
        {
            if (_animator == null)
            {
                return;
            }

            _animator.CrossFade(
                IdleStateName,
                IdleTransitionDuration);
        }

        private void InitializeAnimator()
        {
            if (_animator == null)
            {
                _animator =
                    GetComponent<Animator>();
            }

            if (_animator == null)
            {
                Debug.LogError(
                    $"[{gameObject.name}] Animator не найден.");

                return;
            }

            /*
             * Перемещением пассажира управляет SwapEngine через DOTween.
             * Animator должен отвечать только за движение скелета,
             * а не за изменение Transform объекта.
             */
            _animator.applyRootMotion = false;
        }

        private void InitializeMeshRenderer()
        {
            if (_meshRenderer != null)
            {
                return;
            }

            _meshRenderer =
                GetComponentInChildren<
                    SkinnedMeshRenderer>();

            if (_meshRenderer == null)
            {
                Debug.LogError(
                    $"[{gameObject.name}] " +
                    "SkinnedMeshRenderer не найден.");
            }
        }
    }
}