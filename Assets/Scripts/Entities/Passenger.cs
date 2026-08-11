using UnityEngine;
using UnityEngine.Serialization;

namespace StationJam.Entities
{
    public class Passenger : MonoBehaviour
    {
        private const string IdleStateName = "Idle";
        private const string JumpStateName = "Jump";
        private const float IdleTransitionDuration = 0.2f;

        [Header("Passenger Data")]
        [FormerlySerializedAs("PassengerColor")]
        [SerializeField]
        private ColorType _passengerColor;

        [Header("Visuals")]
        [SerializeField]
        private Animator _animator;

        [SerializeField]
        private SkinnedMeshRenderer _meshRenderer;

        public ColorType PassengerColor =>
            _passengerColor;

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
            _passengerColor = color;

            if (_meshRenderer == null ||
                material == null)
            {
                return;
            }

            _meshRenderer.sharedMaterial =
                material;
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