using UnityEngine;
using ActionRPG.Visuals;

namespace ActionRPG.Town
{
    /// <summary>
    /// Điều phối hiển thị đa tầng Modular Sprite cho NPC (Thân + Các đạo cụ bóc tách).
    /// Tự động đồng bộ vị trí tay cầm, socket neo, thứ tự Sorting Order (trước/sau thân)
    /// theo 4 hướng nhìn (Down, Up, Left, Right) và nhịp bước chân từ DirectionalWalkAnimator.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(DirectionalWalkAnimator))]
    public class NPCModularCharacter2D : MonoBehaviour
    {
        [Header("Danh Sách Đạo Cụ (Modular Props)")]
        public NPCModularProp[] props;

        [Header("Socket Neo Đặc Biệt (Tùy Chọn)")]
        public Transform perchSocket;    // Dành cho Kaelen (Điểm đậu chim ưng)
        public Transform staffTipSocket; // Dành cho Eldrin (Đầu trượng ngọc lam)

        [Header("Độ Lệch Socket Theo Hướng (Perch Socket)")]
        public Vector2 perchOffsetFront = new Vector2(-0.25f, 0.08f);
        public Vector2 perchOffsetBack = new Vector2(0.25f, 0.08f);
        public Vector2 perchOffsetSide = new Vector2(-0.28f, 0.06f);

        private SpriteRenderer bodySr;
        private DirectionalWalkAnimator walkAnim;

        private void Awake()
        {
            bodySr = GetComponent<SpriteRenderer>();
            walkAnim = GetComponent<DirectionalWalkAnimator>();

            if (bodySr != null)
            {
                bodySr.color = Color.white;
            }

            if (props == null || props.Length == 0)
            {
                props = GetComponentsInChildren<NPCModularProp>(true);
            }
        }

        private void LateUpdate()
        {
            if (bodySr == null || walkAnim == null) return;

            FacingDirection2D facing = walkAnim.currentFacing;
            bool isFacingLeft = walkAnim.isFacingLeft || bodySr.flipX;
            int bodyOrder = bodySr.sortingOrder;
            bool isMoving = walkAnim.isMoving;
            int frameIndex = walkAnim.CurrentFrameIndex;

            // Cập nhật tất cả các đạo cụ con theo đúng hướng và frame bước chân
            if (props != null)
            {
                for (int i = 0; i < props.Length; i++)
                {
                    if (props[i] != null && props[i].gameObject.activeSelf)
                    {
                        props[i].UpdateProp(facing, isFacingLeft, bodyOrder, isMoving, frameIndex);
                    }
                }
            }

            // Cập nhật Perch Socket nếu có (Kaelen)
            if (perchSocket != null)
            {
                UpdatePerchSocket(facing, isFacingLeft);
            }
        }

        private void UpdatePerchSocket(FacingDirection2D facing, bool isFacingLeft)
        {
            Vector2 offset = perchOffsetFront;
            switch (facing)
            {
                case FacingDirection2D.Front:
                    offset = perchOffsetFront;
                    break;
                case FacingDirection2D.Back:
                    offset = perchOffsetBack;
                    break;
                case FacingDirection2D.Side:
                    offset = isFacingLeft ? new Vector2(-perchOffsetSide.x, perchOffsetSide.y) : perchOffsetSide;
                    break;
            }
            perchSocket.localPosition = new Vector3(offset.x, offset.y, 0f);
        }

        /// <summary>
        /// Tìm hoặc làm mới danh sách đạo cụ con trong hierarchy.
        /// </summary>
        public void RefreshProps()
        {
            props = GetComponentsInChildren<NPCModularProp>(true);
        }
    }
}
