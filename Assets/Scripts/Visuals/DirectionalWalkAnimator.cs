using UnityEngine;

namespace ActionRPG.Visuals
{
    public enum FacingDirection2D
    {
        Front, // Nhìn xuống (Down)
        Back,  // Nhìn lên (Up)
        Side   // Nhìn ngang (Trái / Phải)
    }

    /// <summary>
    /// Điều khiển hoạt ảnh 4 hướng (Down / Up / Right / Left) tự động theo vận tốc hoặc di chuyển.
    /// Hỗ trợ đầy đủ cả bộ sprite Left độc lập (idleLeft, walkLeftFrames) cho các NPC bất đối xứng
    /// (như Trưởng Làng Eldrin cầm quyền trượng tay phải), hoặc tự động lật flipX nếu không gán Left riêng.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class DirectionalWalkAnimator : MonoBehaviour
    {
        [Header("Frames Bước Đi (Walk Animations - 4 Hướng)")]
        public Sprite[] walkFrontFrames;
        public Sprite[] walkBackFrames;
        public Sprite[] walkSideFrames;
        public Sprite[] walkLeftFrames;

        [Header("Sprite Đứng Yên (Idle Sprites - 4 Hướng)")]
        public Sprite idleFront;
        public Sprite idleBack;
        public Sprite idleSide;
        public Sprite idleLeft;

        [Header("Tốc Độ Khung Hình")]
        [Tooltip("Số khung hình/giây khi bước đi")]
        public float fps = 8f;

        [Header("Trạng Thái Hiện Tại")]
        public FacingDirection2D currentFacing = FacingDirection2D.Front;
        public bool isFacingLeft = false;
        public bool isMoving = false;

        private SpriteRenderer sr;
        private Rigidbody2D rb;
        private Vector3 lastPosition;
        private float frameTimer = 0f;
        private int currentFrameIndex = 0;

        public int CurrentFrameIndex => currentFrameIndex;

        private void Awake()
        {
            sr = GetComponent<SpriteRenderer>();
            rb = GetComponent<Rigidbody2D>();
            lastPosition = transform.position;

            if (sr != null)
            {
                sr.color = Color.white;
                if (idleFront == null && sr.sprite != null)
                {
                    idleFront = sr.sprite;
                }
                if (sr.sprite == null && idleFront != null)
                {
                    sr.sprite = idleFront;
                }
            }
        }

        private void Update()
        {
            Vector2 moveVector = GetMovementVector();
            float speed = moveVector.magnitude;
            bool wasMoving = isMoving;
            isMoving = speed > 0.05f;

            if (isMoving)
            {
                FacingDirection2D newFacing = currentFacing;
                bool newFacingLeft = isFacingLeft;

                // Xác định hướng nhìn ưu tiên theo 4 hướng (Down, Up, Right, Left)
                if (Mathf.Abs(moveVector.x) > Mathf.Abs(moveVector.y) * 0.7f)
                {
                    newFacing = FacingDirection2D.Side;
                    newFacingLeft = (moveVector.x < 0f);
                    var cvView = GetComponent<ActionRPG.Presentation.Views.CharacterVisualView>();
                    if (cvView != null)
                    {
                        cvView.SetFacingDirection(newFacingLeft ? -1 : 1);
                    }
                }
                else if (moveVector.y > 0.05f)
                {
                    newFacing = FacingDirection2D.Back;
                    newFacingLeft = false;
                }
                else if (moveVector.y < -0.05f)
                {
                    newFacing = FacingDirection2D.Front;
                    newFacingLeft = false;
                }

                // Nếu vừa đổi hướng hoặc vừa bắt đầu đi, áp dụng ngay frame đầu tiên
                if (newFacing != currentFacing || newFacingLeft != isFacingLeft || !wasMoving)
                {
                    currentFacing = newFacing;
                    isFacingLeft = newFacingLeft;
                    currentFrameIndex = 0;
                    frameTimer = 0f;
                    ApplyCurrentWalkFrame();
                }

                // Cập nhật nhịp bước chân
                frameTimer += Time.deltaTime;
                float interval = 1f / Mathf.Max(fps, 1f);
                if (frameTimer >= interval)
                {
                    frameTimer -= interval;
                    AdvanceWalkFrame();
                }
            }
            else
            {
                frameTimer = 0f;
                currentFrameIndex = 0;
                SetIdleSprite();
            }

            lastPosition = transform.position;
        }

        private Vector2 GetMovementVector()
        {
            if (rb != null)
            {
                Vector2 vel = rb.linearVelocity;
                if (vel.sqrMagnitude > 0.01f)
                {
                    return vel;
                }
            }

            if (Time.deltaTime > 0.0001f)
            {
                Vector2 delta = (transform.position - lastPosition) / Time.deltaTime;
                if (delta.sqrMagnitude > 0.01f)
                {
                    return delta;
                }
            }

            return Vector2.zero;
        }

        private bool HasDedicatedLeftWalkFrames()
        {
            return walkLeftFrames != null && walkLeftFrames.Length > 0 && walkLeftFrames[0] != null;
        }

        private bool HasDedicatedLeftIdle()
        {
            return idleLeft != null;
        }

        private void ApplyCurrentWalkFrame()
        {
            Sprite[] frames = GetActiveWalkFrames(out bool shouldFlipX);
            if (sr != null)
            {
                sr.flipX = shouldFlipX;
            }

            if (frames != null && frames.Length > 0)
            {
                int safeIndex = Mathf.Clamp(currentFrameIndex, 0, frames.Length - 1);
                if (frames[safeIndex] != null)
                {
                    sr.sprite = frames[safeIndex];
                    return;
                }
            }
            SetIdleSprite();
        }

        private void AdvanceWalkFrame()
        {
            Sprite[] frames = GetActiveWalkFrames(out bool shouldFlipX);
            if (sr != null)
            {
                sr.flipX = shouldFlipX;
            }

            if (frames == null || frames.Length == 0)
            {
                SetIdleSprite();
                return;
            }

            currentFrameIndex = (currentFrameIndex + 1) % frames.Length;
            if (frames[currentFrameIndex] != null)
            {
                sr.sprite = frames[currentFrameIndex];
            }
        }

        private void SetIdleSprite()
        {
            if (sr == null) return;

            Sprite target = null;
            bool shouldFlipX = false;

            switch (currentFacing)
            {
                case FacingDirection2D.Back:
                    target = idleBack != null ? idleBack : idleFront;
                    shouldFlipX = false;
                    break;

                case FacingDirection2D.Side:
                    if (isFacingLeft)
                    {
                        if (HasDedicatedLeftIdle())
                        {
                            target = idleLeft;
                            shouldFlipX = false;
                        }
                        else
                        {
                            target = idleSide != null ? idleSide : idleFront;
                            shouldFlipX = true;
                        }
                    }
                    else
                    {
                        target = idleSide != null ? idleSide : idleFront;
                        shouldFlipX = false;
                    }
                    break;

                case FacingDirection2D.Front:
                default:
                    target = idleFront != null ? idleFront : (idleSide != null ? idleSide : idleBack);
                    shouldFlipX = false;
                    break;
            }

            sr.flipX = shouldFlipX;
            if (target != null)
            {
                sr.sprite = target;
            }
        }

        private Sprite[] GetActiveWalkFrames(out bool shouldFlipX)
        {
            shouldFlipX = false;
            switch (currentFacing)
            {
                case FacingDirection2D.Back:
                    return walkBackFrames != null && walkBackFrames.Length > 0 ? walkBackFrames : walkFrontFrames;

                case FacingDirection2D.Side:
                    if (isFacingLeft)
                    {
                        if (HasDedicatedLeftWalkFrames())
                        {
                            shouldFlipX = false;
                            return walkLeftFrames;
                        }
                        shouldFlipX = true;
                        return walkSideFrames != null && walkSideFrames.Length > 0 ? walkSideFrames : walkFrontFrames;
                    }
                    return walkSideFrames != null && walkSideFrames.Length > 0 ? walkSideFrames : walkFrontFrames;

                case FacingDirection2D.Front:
                default:
                    return walkFrontFrames;
            }
        }
    }
}
