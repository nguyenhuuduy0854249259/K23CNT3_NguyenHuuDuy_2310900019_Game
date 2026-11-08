using UnityEngine;
using ActionRPG.Visuals;

namespace ActionRPG.Town
{
    /// <summary>
    /// Hiệu ứng ánh sáng huyền bí nhấp nháy (Pulse / Glow) cho viên ngọc lam trên đỉnh quyền trượng
    /// của Trưởng Làng Eldrin (Prop_ElderStaff -> Prop_OrbGlow).
    /// Tự động đồng bộ Sprite 4 hướng, flipX và SortingOrder (+1 trên thân trượng) với Prop_ElderStaff
    /// để vầng sáng luôn khóa chặt 100% vào tâm viên ngọc lam trên cùng lưới Pixel 32x32 (PPU = 16).
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class EldrinStaffOrbGlow : MonoBehaviour
    {
        [Header("Sprites Phát Sáng Theo 4 Hướng (32x32, PPU = 16)")]
        public Sprite glowFront; // Down
        public Sprite glowBack;  // Up
        public Sprite glowSide;  // Right
        public Sprite glowLeft;  // Left

        [Header("Cấu Hình Nhịp Thở Ánh Sáng (Glow / Pulse)")]
        [Tooltip("Tốc độ nhịp thở hào quang ngọc lam")]
        public float pulseSpeed = 3.2f;

        [Tooltip("Độ trong suốt nhỏ nhất khi nhịp thở hạ xuống")]
        [Range(0.2f, 1f)]
        public float minAlpha = 0.45f;

        [Tooltip("Độ trong suốt lớn nhất khi viên ngọc rực sáng")]
        [Range(0.5f, 1f)]
        public float maxAlpha = 0.98f;

        [Tooltip("Hệ số dao động kích thước (đặt 0 cho sprite 32x32 đồng bộ lưới pixel để không lệch tâm)")]
        public float scalePulseFactor = 0f;

        private SpriteRenderer sr;
        private SpriteRenderer parentStaffSr;
        private DirectionalWalkAnimator rootWalkAnim;
        private Color dimCyan = new Color(0.20f, 0.85f, 1.00f, 1.0f);
        private Color brightCyanWhite = new Color(0.82f, 0.99f, 1.00f, 1.0f);

        private void Awake()
        {
            sr = GetComponent<SpriteRenderer>();
            if (transform.parent != null)
            {
                parentStaffSr = transform.parent.GetComponent<SpriteRenderer>();
            }
            rootWalkAnim = GetComponentInParent<DirectionalWalkAnimator>();
            transform.localPosition = Vector3.zero;
            transform.localScale = Vector3.one;
        }

        private void LateUpdate()
        {
            if (sr == null) return;

            if (parentStaffSr == null && transform.parent != null)
            {
                parentStaffSr = transform.parent.GetComponent<SpriteRenderer>();
            }
            if (rootWalkAnim == null)
            {
                rootWalkAnim = GetComponentInParent<DirectionalWalkAnimator>();
            }

            // 1. Khóa vị trí localPosition = (0,0,0) để trùng khít lưới 32x32 với Prop_ElderStaff
            transform.localPosition = Vector3.zero;
            transform.localScale = Vector3.one;

            // 2. Đồng bộ hướng hiển thị (Down, Up, Right, Left) và SortingOrder với cây trượng
            if (rootWalkAnim != null)
            {
                FacingDirection2D facing = rootWalkAnim.currentFacing;
                bool isLeft = rootWalkAnim.isFacingLeft;

                switch (facing)
                {
                    case FacingDirection2D.Front:
                        if (glowFront != null) sr.sprite = glowFront;
                        sr.flipX = false;
                        break;

                    case FacingDirection2D.Back:
                        if (glowBack != null) sr.sprite = glowBack;
                        else if (glowFront != null) sr.sprite = glowFront;
                        sr.flipX = false;
                        break;

                    case FacingDirection2D.Side:
                        if (isLeft)
                        {
                            if (glowLeft != null)
                            {
                                sr.sprite = glowLeft;
                                sr.flipX = false;
                            }
                            else if (glowSide != null)
                            {
                                sr.sprite = glowSide;
                                sr.flipX = true;
                            }
                        }
                        else
                        {
                            if (glowSide != null) sr.sprite = glowSide;
                            sr.flipX = false;
                        }
                        break;
                }
            }
            else if (parentStaffSr != null)
            {
                sr.flipX = parentStaffSr.flipX;
            }

            if (parentStaffSr != null)
            {
                sr.sortingLayerID = parentStaffSr.sortingLayerID;
                sr.sortingOrder = parentStaffSr.sortingOrder + 1;
            }

            // 3. Hiệu ứng nhịp thở phát sáng (Glow / Pulse) chuyển sắc độ và Alpha mượt mà
            float t = (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f;
            Color pulsedColor = Color.Lerp(dimCyan, brightCyanWhite, t);
            pulsedColor.a = Mathf.Lerp(minAlpha, maxAlpha, t);
            sr.color = pulsedColor;
        }
    }
}
