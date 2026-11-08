using UnityEngine;
using ActionRPG.Visuals;

namespace ActionRPG.Town
{
    public enum FalconState
    {
        Perched,    // Đang đậu trên găng tay của Kaelen
        TakeOff,    // Cất cánh bay lên
        Circling,   // Bay lượn vòng quanh Kaelen
        Returning,  // Bay về vị trí găng tay
        Landing     // Đáp xuống điểm neo
    }

    /// <summary>
    /// Điều khiển hành vi của Chim Ưng (Pet_Falcon) - Đồng hành của Quản Thú Kaelen:
    /// - Đậu yên trên găng tay trái (PerchPoint)
    /// - Tự động cất cánh bay lượn vòng cung mềm mại quanh Kaelen
    /// - Tự động định vị và bay về đáp chính xác vào PerchPoint dù Kaelen đang di chuyển
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class FalconPetController2D : MonoBehaviour
    {
        [Header("Tham Chiếu Chủ Nhân & Điểm Neo")]
        public Transform kaelenTransform;
        public Transform perchPoint;

        [Header("Animation Frames")]
        public Sprite[] idlePerchedFrames;
        public Sprite[] flyLoopFrames;
        public float perchFps = 2f;
        public float flyFps = 8f;

        [Header("Cấu Hình Quỹ Đạo Bay")]
        public float flightRadius = 2.4f;
        public float flightSpeed = 2.8f;
        public float minPerchTime = 4.0f;
        public float maxPerchTime = 7.0f;
        public float minFlightLoops = 1.5f;
        public float maxFlightLoops = 3.0f;

        [Header("Trạng Thái Hoạt Động")]
        public FalconState currentState = FalconState.Perched;

        private SpriteRenderer sr;
        private SpriteRenderer kaelenSr;
        private float stateTimer = 0f;
        private float flightAngle = 0f;
        private float totalAngleToFly = 0f;
        private float currentAngleFlown = 0f;
        private int currentFrameIndex = 0;
        private float animTimer = 0f;
        private Vector3 lastPos;

        private void Awake()
        {
            sr = GetComponent<SpriteRenderer>();
            sr.color = Color.white;
            lastPos = transform.position;
        }

        private void Start()
        {
            if (kaelenTransform == null)
            {
                if (transform.parent != null)
                {
                    kaelenTransform = transform.parent;
                }
                else
                {
                    var foundKaelen = GameObject.Find("NPC_PetMaster_Kaelen");
                    if (foundKaelen != null) kaelenTransform = foundKaelen.transform;
                }
            }

            if (kaelenTransform != null)
            {
                kaelenSr = kaelenTransform.GetComponent<SpriteRenderer>();
                if (perchPoint == null)
                {
                    Transform socket = kaelenTransform.Find("Socket_FalconPerch");
                    if (socket != null) perchPoint = socket;
                }
            }

            stateTimer = Random.Range(minPerchTime * 0.5f, minPerchTime);
            SetState(FalconState.Perched);
        }

        private void Update()
        {
            UpdateAnimation();

            switch (currentState)
            {
                case FalconState.Perched:
                    HandlePerchedState();
                    break;

                case FalconState.TakeOff:
                    HandleTakeOffState();
                    break;

                case FalconState.Circling:
                    HandleCirclingState();
                    break;

                case FalconState.Returning:
                    HandleReturningState();
                    break;

                case FalconState.Landing:
                    HandleLandingState();
                    break;
            }

            lastPos = transform.position;
        }

        private void HandlePerchedState()
        {
            // Bám sát vị trí PerchPoint trên găng tay của Kaelen
            if (perchPoint != null)
            {
                transform.position = perchPoint.position;
            }
            else if (kaelenTransform != null)
            {
                transform.position = kaelenTransform.position + new Vector3(-0.25f, 0.1f, 0f);
            }

            if (kaelenSr != null)
            {
                sr.sortingOrder = kaelenSr.sortingOrder + 2;
                sr.flipX = kaelenSr.flipX;
            }

            stateTimer -= Time.deltaTime;
            if (stateTimer <= 0f)
            {
                SetState(FalconState.TakeOff);
            }
        }

        private void HandleTakeOffState()
        {
            // Cất cánh bay vút chéo lên cao
            Vector3 targetUp = (perchPoint != null ? perchPoint.position : transform.position) + new Vector3(0.3f, 0.8f, 0f);
            transform.position = Vector3.MoveTowards(transform.position, targetUp, flightSpeed * 1.2f * Time.deltaTime);

            UpdateSortingDuringFlight();

            if (Vector3.Distance(transform.position, targetUp) < 0.1f)
            {
                // Bắt đầu lượn vòng từ góc hiện tại
                Vector3 center = kaelenTransform != null ? kaelenTransform.position : transform.position;
                Vector3 offset = transform.position - center;
                flightAngle = Mathf.Atan2(offset.y, offset.x);
                totalAngleToFly = Random.Range(minFlightLoops, maxFlightLoops) * Mathf.PI * 2f;
                currentAngleFlown = 0f;
                SetState(FalconState.Circling);
            }
        }

        private void HandleCirclingState()
        {
            float angleDelta = (flightSpeed / flightRadius) * Time.deltaTime;
            flightAngle += angleDelta;
            currentAngleFlown += angleDelta;

            Vector3 center = kaelenTransform != null ? kaelenTransform.position : transform.position;
            float rx = flightRadius;
            float ry = flightRadius * 0.65f; // Quỹ đạo elip nghiêng theo góc nhìn 2.5D
            Vector3 newPos = center + new Vector3(Mathf.Cos(flightAngle) * rx, Mathf.Sin(flightAngle) * ry + 0.6f, 0f);

            // Quay mặt theo hướng bay
            Vector3 flightDir = newPos - transform.position;
            if (Mathf.Abs(flightDir.x) > 0.01f)
            {
                sr.flipX = flightDir.x < 0f;
            }

            transform.position = newPos;
            UpdateSortingDuringFlight();

            if (currentAngleFlown >= totalAngleToFly)
            {
                SetState(FalconState.Returning);
            }
        }

        private void HandleReturningState()
        {
            Vector3 targetPerch = perchPoint != null ? perchPoint.position : (kaelenTransform != null ? kaelenTransform.position + new Vector3(-0.25f, 0.1f, 0f) : transform.position);

            Vector3 flyDir = (targetPerch - transform.position);
            float dist = flyDir.magnitude;

            if (Mathf.Abs(flyDir.x) > 0.01f)
            {
                sr.flipX = flyDir.x < 0f;
            }

            transform.position = Vector3.MoveTowards(transform.position, targetPerch, flightSpeed * Time.deltaTime);
            UpdateSortingDuringFlight();

            if (dist <= 0.15f)
            {
                SetState(FalconState.Landing);
            }
        }

        private void HandleLandingState()
        {
            Vector3 targetPerch = perchPoint != null ? perchPoint.position : (kaelenTransform != null ? kaelenTransform.position : transform.position);
            transform.position = Vector3.MoveTowards(transform.position, targetPerch, flightSpeed * 0.6f * Time.deltaTime);

            if (Vector3.Distance(transform.position, targetPerch) <= 0.03f)
            {
                transform.position = targetPerch;
                SetState(FalconState.Perched);
            }
        }

        private void UpdateSortingDuringFlight()
        {
            if (kaelenSr != null)
            {
                // Khi bay, chim ở tầng trên để không bị che khuất
                sr.sortingOrder = Mathf.Max(kaelenSr.sortingOrder + 10, 5020);
            }
        }

        private void SetState(FalconState newState)
        {
            currentState = newState;
            currentFrameIndex = 0;
            animTimer = 0f;

            if (newState == FalconState.Perched)
            {
                stateTimer = Random.Range(minPerchTime, maxPerchTime);
            }
        }

        private void UpdateAnimation()
        {
            Sprite[] activeFrames = (currentState == FalconState.Perched) ? idlePerchedFrames : flyLoopFrames;
            float fps = (currentState == FalconState.Perched) ? perchFps : flyFps;

            if (activeFrames == null || activeFrames.Length == 0) return;

            animTimer += Time.deltaTime;
            float interval = 1f / Mathf.Max(fps, 0.1f);
            if (animTimer >= interval)
            {
                animTimer -= interval;
                currentFrameIndex = (currentFrameIndex + 1) % activeFrames.Length;
            }

            int safeIdx = Mathf.Clamp(currentFrameIndex, 0, activeFrames.Length - 1);
            if (activeFrames[safeIdx] != null)
            {
                sr.sprite = activeFrames[safeIdx];
            }
        }
    }
}
