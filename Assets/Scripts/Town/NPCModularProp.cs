using UnityEngine;
using ActionRPG.Visuals;

namespace ActionRPG.Town
{
    /// <summary>
    /// Component quản lý một lớp đạo cụ (Prop) độc lập của NPC (Quyền trượng Prop_ElderStaff, Búa rèn, Túi thuốc, Áo choàng, Kiếm, Giỏ hàng, Mũ rơm).
    /// Tự động cập nhật Sprite 4 hướng (Down, Up, Right, Left), Socket Offset, SortingOrder (trước/sau thân)
    /// và đồng bộ nhịp nhún 1px (1/16 unit) theo đúng frame bước đi của thân nhân vật trên lưới 32x32 (PPU = 16).
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class NPCModularProp : MonoBehaviour
    {
        [Header("Sprites Theo 4 Hướng (32x32, PPU = 16)")]
        public Sprite spriteFront; // Down (Nhìn xuống)
        public Sprite spriteBack;  // Up (Nhìn lên)
        public Sprite spriteSide;  // Right (Nhìn sang phải)
        public Sprite spriteLeft;  // Left (Nhìn sang trái - tùy chọn, nếu null sẽ tự động lật spriteSide)

        [Header("Sorting Order Tương Đối So Với Thân")]
        public int sortingOffsetFront = 1;      // Down: ở trước thân (+1)
        public int sortingOffsetBack = -1;      // Up: ở sau thân (-1)
        public int sortingOffsetSideRight = 1;  // Right: ở trước thân (+1)
        public int sortingOffsetSideLeft = -1;  // Left: ở sau thân (-1)

        [Header("Điểm Neo / Socket Offset (World/Local Units)")]
        public Vector2 offsetFront = Vector2.zero;
        public Vector2 offsetBack = Vector2.zero;
        public Vector2 offsetSide = Vector2.zero;
        public Vector2 offsetLeft = Vector2.zero;

        [Header("Đồng Bộ Nhịp Bước Chân Trên Lưới Pixel (PPU = 16)")]
        [Tooltip("Tự động nhún 1px (-1/16 unit) khớp chính xác với frame bước chân lẻ của NPC")]
        public bool syncWithWalkStepBob = true;

        [Header("Chuyển Động Thứ Cấp (Secondary Motion)")]
        public bool hasSecondaryMotion = false;
        public float motionAmplitudeX = 0.03f;
        public float motionAmplitudeY = 0.04f;
        public float motionFrequency = 7f;
        public bool bounceOnlyWhenMoving = true;

        [HideInInspector]
        public SpriteRenderer sr;

        private void Awake()
        {
            sr = GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.color = Color.white;
            }
        }

        /// <summary>
        /// Cập nhật hướng hiển thị, sprite, sorting order và socket position (hỗ trợ tương thích ngược 4 tham số).
        /// </summary>
        public void UpdateProp(FacingDirection2D facing, bool flipX, int bodySortingOrder, bool isMoving)
        {
            UpdateProp(facing, flipX, bodySortingOrder, isMoving, 0);
        }

        /// <summary>
        /// Cập nhật hướng hiển thị 4 hướng, sprite, sorting order, đồng bộ nhịp bước chân 1px và chuyển động phụ.
        /// </summary>
        public void UpdateProp(FacingDirection2D facing, bool isFacingLeft, int bodySortingOrder, bool isMoving, int walkFrameIndex)
        {
            if (sr == null) sr = GetComponent<SpriteRenderer>();
            if (sr == null) return;

            Sprite targetSprite = spriteFront;
            int orderOffset = sortingOffsetFront;
            Vector2 baseOffset = offsetFront;
            bool shouldFlipPropX = false;

            switch (facing)
            {
                case FacingDirection2D.Front:
                    targetSprite = spriteFront != null ? spriteFront : spriteSide;
                    orderOffset = sortingOffsetFront;
                    baseOffset = offsetFront;
                    shouldFlipPropX = false;
                    break;

                case FacingDirection2D.Back:
                    targetSprite = spriteBack != null ? spriteBack : spriteSide;
                    orderOffset = sortingOffsetBack;
                    baseOffset = offsetBack;
                    shouldFlipPropX = false;
                    break;

                case FacingDirection2D.Side:
                    if (isFacingLeft)
                    {
                        orderOffset = sortingOffsetSideLeft;
                        if (spriteLeft != null)
                        {
                            targetSprite = spriteLeft;
                            baseOffset = offsetLeft;
                            shouldFlipPropX = false;
                        }
                        else
                        {
                            targetSprite = spriteSide != null ? spriteSide : spriteFront;
                            baseOffset = new Vector2(-offsetSide.x, offsetSide.y);
                            shouldFlipPropX = true;
                        }
                    }
                    else
                    {
                        targetSprite = spriteSide != null ? spriteSide : spriteFront;
                        orderOffset = sortingOffsetSideRight;
                        baseOffset = offsetSide;
                        shouldFlipPropX = false;
                    }
                    break;
            }

            sr.flipX = shouldFlipPropX;
            if (targetSprite != null)
            {
                sr.sprite = targetSprite;
            }

            sr.sortingOrder = bodySortingOrder + orderOffset;

            // Đồng bộ nhịp nhún 1px (1/16 = 0.0625f) theo frame bước chân (frame 0 và 2 trong chuỗi 4 frame walk_1..4 có bob = 1px)
            Vector2 stepBobDelta = Vector2.zero;
            if (syncWithWalkStepBob && isMoving)
            {
                if ((walkFrameIndex % 2) == 0)
                {
                    stepBobDelta = new Vector2(0f, -1f / PixelArtShadingHelper.StandardPixelsPerUnit);
                }
            }

            // Tính chuyển động phụ (nhịp lắc giỏ, nhịp nảy túi thuốc, búa dập bước đi, áo choàng bay)
            Vector2 motionDelta = Vector2.zero;
            if (hasSecondaryMotion)
            {
                if (!bounceOnlyWhenMoving || isMoving)
                {
                    float t = Time.time * motionFrequency;
                    float dx = Mathf.Sin(t) * motionAmplitudeX;
                    float dy = Mathf.Abs(Mathf.Sin(t)) * motionAmplitudeY;
                    motionDelta = new Vector2(dx, dy);
                }
            }

            transform.localPosition = new Vector3(
                baseOffset.x + stepBobDelta.x + motionDelta.x,
                baseOffset.y + stepBobDelta.y + motionDelta.y,
                0f);
        }
    }
}
