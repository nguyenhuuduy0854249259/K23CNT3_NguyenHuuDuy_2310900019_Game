using UnityEngine;
using ActionRPG.Visuals;

namespace ActionRPG.Presentation.Views
{
    /// <summary>
    /// View chuyên trách toàn bộ hình ảnh 2D Pixel cho nhân vật:
    /// - Quản lý SpriteRenderer, lật mặt (FlipX).
    /// - Điều khiển Animator hoặc SpriteAnimator2D nội bộ dự án.
    /// - Gọi hiệu ứng nháy đỏ khi nhận sát thương qua HitFlashEffectView.
    /// TUYỆT ĐỐI KHÔNG chứa công thức tính toán logic máu hay vận tốc!
    /// </summary>
    public class CharacterVisualView : MonoBehaviour
    {
        [Header("Components Hình Ảnh Pixel")]
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Animator animator;
        [SerializeField] private SpriteAnimator2D spriteAnimator2D;
        [SerializeField] private HitFlashEffectView flashEffect;

        [Header("Tùy Chọn Lật Mặt Pixel")]
        [Tooltip("Nếu true: dùng spriteRenderer.flipX. Nếu false: lật bằng localScale.x")]
        [SerializeField] private bool useFlipX = true;

        private static readonly int IsMovingHash = Animator.StringToHash("IsMoving");
        private static readonly int DieTriggerHash = Animator.StringToHash("Die");
        private static readonly int DashTriggerHash = Animator.StringToHash("Dash");

        private void Awake()
        {
            if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
            if (animator == null) animator = GetComponent<Animator>();
            if (spriteAnimator2D == null) spriteAnimator2D = GetComponent<SpriteAnimator2D>();
            if (flashEffect == null) flashEffect = GetComponent<HitFlashEffectView>();
        }

        public System.Action<bool> OnFlipX;

        /// <summary>
        /// Đổi hướng nhìn: 1 = Phải, -1 = Trái
        /// </summary>
        public void SetFacingDirection(int directionX)
        {
            var fourDir = GetComponent<LuminaFourDirectionCharacterAnimator>();
            if (fourDir != null && fourDir.enabled)
            {
                if (spriteRenderer != null) spriteRenderer.flipX = false;
                return;
            }

            bool isFlipped = (directionX < 0);
            if (useFlipX && spriteRenderer != null)
            {
                spriteRenderer.flipX = isFlipped;
            }
            else
            {
                float sign = directionX >= 0 ? 1f : -1f;
                transform.localScale = new Vector3(sign, 1f, 1f);
            }
            OnFlipX?.Invoke(isFlipped);
        }

        /// <summary>
        /// Cập nhật trạng thái hoạt ảnh di chuyển
        /// </summary>
        public void SetMovingState(bool isMoving)
        {
            if (animator != null)
            {
                animator.SetBool(IsMovingHash, isMoving);
            }

            // Đồng bộ với SpriteAnimator2D nếu có
            if (spriteAnimator2D != null)
            {
                spriteAnimator2D.PlayAnimation(isMoving ? ActionRPG.Visuals.AnimationState.Walk : ActionRPG.Visuals.AnimationState.Idle);
            }
        }

        /// <summary>
        /// Kích hoạt visual lướt né đòn
        /// </summary>
        public void PlayDashVisual()
        {
            if (animator != null)
            {
                animator.SetTrigger(DashTriggerHash);
            }
        }

        /// <summary>
        /// Kích hoạt visual khi nhận sát thương
        /// </summary>
        public void PlayDamageVisual()
        {
            if (flashEffect != null)
            {
                flashEffect.PlayHitFlash();
            }
        }

        /// <summary>
        /// Kích hoạt visual khi tử trận
        /// </summary>
        public void PlayDeathVisual()
        {
            if (animator != null)
            {
                animator.SetTrigger(DieTriggerHash);
            }
        }
    }
}
