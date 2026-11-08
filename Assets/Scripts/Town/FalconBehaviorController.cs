using UnityEngine;
using ActionRPG.Visuals;

namespace ActionRPG.Town
{
    /// <summary>
    /// Máy trạng thái 3 pha chuẩn của Chim Ưng (Pet_Falcon) đi cùng Quản Thú Kaelen:
    /// - Perched: Đậu yên trên găng tay trái của Kaelen tại điểm neo PerchPoint.
    /// - FlyAround: Cất cánh tách khỏi tay, bay lượn vòng tròn xung quanh Kaelen bằng quỹ đạo Mathf.Sin / Mathf.Cos.
    /// - Landing: Tự động bay khớp về đáp lại chính xác trên cổ tay (PerchPoint) của Kaelen.
    /// </summary>
    public enum FalconBehaviorState
    {
        Perched = 0,   // Đậu yên trên găng tay trái của Kaelen tại PerchPoint
        FlyAround = 1, // Bay lượn vòng tròn xung quanh Kaelen bằng Mathf.Sin / Mathf.Cos
        Landing = 2    // Tự động bay về đáp chính xác lên cổ tay Kaelen
    }

    [RequireComponent(typeof(SpriteRenderer))]
    public class FalconBehaviorController : MonoBehaviour
    {
        [Header("Tham Chiếu Chủ Nhân (Kaelen) & Điểm Neo Cổ Tay (PerchPoint)")]
        public Transform kaelenTransform;
        public Transform perchPoint;

        [Header("Bộ Sprite Hoạt Ảnh Chim Ưng (32x32, PPU = 16)")]
        public Sprite[] idlePerchedFrames;
        public Sprite[] flyLoopFrames;
        public float perchedFps = 2.5f;
        public float flyFps = 8.5f;

        [Header("Thông Số Quỹ Đạo Bay Lượn (Mathf.Sin / Mathf.Cos)")]
        public float orbitRadiusX = 2.2f;
        public float orbitRadiusY = 1.45f;
        public float flightHeightOffset = 0.55f;
        public float angularSpeed = 1.65f;
        public float landingSpeed = 3.2f;
        public float minPerchDuration = 4.0f;
        public float maxPerchDuration = 6.5f;
        public float minOrbitLoops = 1.5f;
        public float maxOrbitLoops = 2.5f;

        [Header("Trạng Thái Hiện Tại (State Machine 3 Pha)")]
        public FalconBehaviorState currentState = FalconBehaviorState.Perched;

        private SpriteRenderer sr;
        private SpriteRenderer kaelenSr;
        private DirectionalWalkAnimator kaelenAnim;

        private float stateTimer = 0f;
        private float currentOrbitAngle = 0f;
        private float accumulatedOrbitAngle = 0f;
        private float targetTotalOrbitAngle = 0f;
        private float orbitBlendWeight = 0f;

        private int currentFrameIndex = 0;
        private float animTimer = 0f;
        private Vector3 cachedLocalScale = Vector3.one;

        private void Awake()
        {
            sr = GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.color = Color.white;
            }
            cachedLocalScale = transform.localScale;
        }

        private void Start()
        {
            ResolveReferences();
            EnterState(FalconBehaviorState.Perched);
        }

        public void ResolveReferences()
        {
            if (kaelenTransform == null)
            {
                if (transform.parent != null)
                {
                    kaelenTransform = transform.parent;
                }
                else
                {
                    GameObject found = GameObject.Find("NPC_PetMaster_Kaelen");
                    if (found != null)
                    {
                        kaelenTransform = found.transform;
                    }
                }
            }

            if (kaelenTransform != null)
            {
                kaelenSr = kaelenTransform.GetComponent<SpriteRenderer>();
                kaelenAnim = kaelenTransform.GetComponent<DirectionalWalkAnimator>();

                if (perchPoint == null)
                {
                    Transform socket = kaelenTransform.Find("PerchPoint");
                    if (socket == null)
                    {
                        socket = kaelenTransform.Find("Socket_FalconPerch");
                    }
                    if (socket != null)
                    {
                        perchPoint = socket;
                    }
                }
            }
        }

        private void LateUpdate()
        {
            if (kaelenTransform == null)
            {
                ResolveReferences();
            }

            UpdateSpriteAnimation();

            switch (currentState)
            {
                case FalconBehaviorState.Perched:
                    UpdatePerchedPhase();
                    break;

                case FalconBehaviorState.FlyAround:
                    UpdateFlyAroundPhase();
                    break;

                case FalconBehaviorState.Landing:
                    UpdateLandingPhase();
                    break;
            }
        }

        /// <summary>
        /// Pha 1 - Perched: Đậu yên trên găng tay trái của Kaelen tại điểm neo PerchPoint.
        /// Tự động cập nhật vị trí cổ tay và thứ tự lớp hiển thị (SortingOrder) theo 4 hướng đi của Kaelen.
        /// </summary>
        private void UpdatePerchedPhase()
        {
            Vector3 targetWristPos = GetPerchWorldPosition();
            transform.position = targetWristPos;

            if (kaelenSr != null)
            {
                bool behindBody = false;
                if (kaelenAnim != null)
                {
                    if (kaelenAnim.currentFacing == FacingDirection2D.Back)
                    {
                        behindBody = true;
                    }
                    else if (kaelenAnim.currentFacing == FacingDirection2D.Side && !kaelenAnim.isFacingLeft && !kaelenSr.flipX)
                    {
                        behindBody = false;
                    }
                }

                sr.sortingOrder = kaelenSr.sortingOrder + (behindBody ? -1 : 2);
                sr.flipX = (kaelenAnim != null && kaelenAnim.isFacingLeft) || kaelenSr.flipX;
            }

            stateTimer -= Time.deltaTime;
            if (stateTimer <= 0f)
            {
                EnterState(FalconBehaviorState.FlyAround);
            }
        }

        /// <summary>
        /// Pha 2 - FlyAround: Cất cánh tách khỏi tay Kaelen, bay lượn vòng tròn xung quanh Kaelen
        /// theo quỹ đạo lượng giác Mathf.Sin / Mathf.Cos mượt mà.
        /// </summary>
        private void UpdateFlyAroundPhase()
        {
            Vector3 prevPos = transform.position;

            float deltaAngle = angularSpeed * Time.deltaTime;
            currentOrbitAngle += deltaAngle;
            accumulatedOrbitAngle += deltaAngle;
            orbitBlendWeight = Mathf.MoveTowards(orbitBlendWeight, 1f, Time.deltaTime * 2.0f);

            Vector3 center = kaelenTransform != null ? kaelenTransform.position : transform.position;
            Vector3 perchPos = GetPerchWorldPosition();

            // Tính tọa độ quỹ đạo tròn/elip quanh Kaelen bằng Mathf.Cos và Mathf.Sin
            float cosVal = Mathf.Cos(currentOrbitAngle);
            float sinVal = Mathf.Sin(currentOrbitAngle);
            Vector3 orbitTarget = center + new Vector3(
                cosVal * orbitRadiusX,
                sinVal * orbitRadiusY + flightHeightOffset,
                0f
            );

            Vector3 nextPos = Vector3.Lerp(perchPos, orbitTarget, orbitBlendWeight);
            Vector3 velocity = nextPos - prevPos;

            if (Mathf.Abs(velocity.x) > 0.002f && sr != null)
            {
                sr.flipX = velocity.x < 0f;
            }

            transform.position = nextPos;

            if (kaelenSr != null && sr != null)
            {
                sr.sortingOrder = Mathf.Max(kaelenSr.sortingOrder + 10, 5025);
            }

            if (accumulatedOrbitAngle >= targetTotalOrbitAngle)
            {
                EnterState(FalconBehaviorState.Landing);
            }
        }

        /// <summary>
        /// Pha 3 - Landing: Tự động định vị và bay khớp về đáp lại chính xác trên cổ tay (PerchPoint) của Kaelen.
        /// </summary>
        private void UpdateLandingPhase()
        {
            Vector3 targetWristPos = GetPerchWorldPosition();
            Vector3 toWrist = targetWristPos - transform.position;
            float distance = toWrist.magnitude;

            if (Mathf.Abs(toWrist.x) > 0.005f && sr != null)
            {
                sr.flipX = toWrist.x < 0f;
            }

            transform.position = Vector3.MoveTowards(transform.position, targetWristPos, landingSpeed * Time.deltaTime);

            if (kaelenSr != null && sr != null)
            {
                sr.sortingOrder = Mathf.Max(kaelenSr.sortingOrder + 4, 5015);
            }

            if (distance <= 0.03f)
            {
                transform.position = targetWristPos;
                if (kaelenTransform != null && transform.parent != kaelenTransform)
                {
                    transform.SetParent(kaelenTransform, true);
                    transform.localScale = cachedLocalScale;
                }
                EnterState(FalconBehaviorState.Perched);
            }
        }

        /// <summary>
        /// Chuyển pha trong State Machine (Perched -> FlyAround -> Landing -> Perched).
        /// </summary>
        public void EnterState(FalconBehaviorState nextState)
        {
            currentState = nextState;
            currentFrameIndex = 0;
            animTimer = 0f;

            switch (nextState)
            {
                case FalconBehaviorState.Perched:
                    if (kaelenTransform != null && transform.parent != kaelenTransform)
                    {
                        transform.SetParent(kaelenTransform, true);
                        transform.localScale = cachedLocalScale;
                    }
                    stateTimer = Random.Range(minPerchDuration, maxPerchDuration);
                    break;

                case FalconBehaviorState.FlyAround:
                    Vector3 center = kaelenTransform != null ? kaelenTransform.position : transform.position;
                    Vector3 rel = transform.position - center;
                    currentOrbitAngle = Mathf.Atan2(rel.y, rel.x);
                    accumulatedOrbitAngle = 0f;
                    orbitBlendWeight = 0f;
                    targetTotalOrbitAngle = Random.Range(minOrbitLoops, maxOrbitLoops) * Mathf.PI * 2f;
                    break;

                case FalconBehaviorState.Landing:
                    break;
            }
        }

        [ContextMenu("Force TakeOff (FlyAround)")]
        public void ForceFlyAround()
        {
            EnterState(FalconBehaviorState.FlyAround);
        }

        [ContextMenu("Force Return (Landing)")]
        public void ForceLanding()
        {
            EnterState(FalconBehaviorState.Landing);
        }

        private Vector3 GetPerchWorldPosition()
        {
            if (perchPoint != null)
            {
                return perchPoint.position;
            }
            if (kaelenTransform != null)
            {
                return kaelenTransform.position + new Vector3(0.28f, -0.06f, 0f);
            }
            return transform.position;
        }

        private void UpdateSpriteAnimation()
        {
            if (sr == null) return;

            Sprite[] activeFrames = (currentState == FalconBehaviorState.Perched) ? idlePerchedFrames : flyLoopFrames;
            float fps = (currentState == FalconBehaviorState.Perched) ? perchedFps : flyFps;

            if (activeFrames == null || activeFrames.Length == 0) return;

            animTimer += Time.deltaTime;
            float interval = 1f / Mathf.Max(fps, 0.1f);
            if (animTimer >= interval)
            {
                animTimer -= interval;
                currentFrameIndex = (currentFrameIndex + 1) % activeFrames.Length;
            }

            int safeIndex = Mathf.Clamp(currentFrameIndex, 0, activeFrames.Length - 1);
            if (activeFrames[safeIndex] != null)
            {
                sr.sprite = activeFrames[safeIndex];
            }
        }
    }
}
