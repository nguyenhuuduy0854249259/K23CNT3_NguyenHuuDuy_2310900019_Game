using UnityEngine;
using ActionRPG.Visuals;

namespace ActionRPG.Town
{
    /// <summary>
    /// Điều khiển NPC hoặc Động Vật đi dạo tự do (free-roaming) quanh vị trí gốc (homePosition).
    /// - Tự động đi bộ ngẫu nhiên trong bán kính wanderRadius, nghỉ ngơi giữa các nhịp bước.
    /// - Tự động dừng lại và quay mặt về phía Người Chơi khi Người Chơi đến gần để giao tiếp/mua bán.
    /// - Tự động đổi hướng nếu gặp vật cản (tường, nhà, cây, nước).
    /// - Đồng bộ mượt mà với DirectionalWalkAnimator, LuminaFourDirectionCharacterAnimator và YSortOrder2D.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class NPCWanderController2D : MonoBehaviour
    {
        [Header("Cấu Hình Di Chuyển Tự Do")]
        public Vector2 homePosition;
        public bool useInitialPositionAsHome = true;
        public float wanderRadius = 3.2f;
        public float moveSpeed = 1.15f;

        [Header("Nhịp Nghỉ & Bước Đi")]
        public float minIdleTime = 1.8f;
        public float maxIdleTime = 4.2f;
        public float minWalkTime = 1.2f;
        public float maxWalkTime = 3.0f;

        [Header("Tương Tác Với Người Chơi")]
        public bool pauseWhenPlayerNearby = true;
        public float playerStopDistance = 2.2f;

        private Rigidbody2D rb;
        private SpriteRenderer sr;
        private DirectionalWalkAnimator dirWalkAnim;
        private LuminaFourDirectionCharacterAnimator luminaAnim;
        private YSortOrder2D ySort;
        private Transform playerTransform;

        private Vector2 targetPosition;
        private bool isWalking = false;
        private float stateTimer = 0f;

        private void Awake()
        {
            sr = GetComponent<SpriteRenderer>();
            rb = GetComponent<Rigidbody2D>();
            dirWalkAnim = GetComponent<DirectionalWalkAnimator>();
            luminaAnim = GetComponent<LuminaFourDirectionCharacterAnimator>();
            ySort = GetComponent<YSortOrder2D>();

            if (ySort != null)
            {
                // Cho phép cập nhật thứ tự vẽ động khi NPC di chuyển
                ySort.isStatic = false;
            }
        }

        private void Start()
        {
            if (useInitialPositionAsHome || homePosition.sqrMagnitude < 0.001f)
            {
                homePosition = transform.position;
            }

            // Khởi tạo ngẫu nhiên để các NPC không bước đi cùng một nhịp
            stateTimer = Random.Range(0.2f, maxIdleTime);
            isWalking = false;
        }

        private void Update()
        {
            if (pauseWhenPlayerNearby && CheckPlayerNearby())
            {
                StopWalkingAndFacePlayer();
                return;
            }

            stateTimer -= Time.deltaTime;

            if (isWalking)
            {
                Vector2 currentPos = transform.position;
                Vector2 toTarget = targetPosition - currentPos;
                float dist = toTarget.magnitude;

                if (dist <= 0.12f || stateTimer <= 0f)
                {
                    EnterIdleState();
                    return;
                }

                Vector2 moveDir = toTarget.normalized;

                // Kiểm tra vật cản phía trước
                RaycastHit2D hit = Physics2D.CircleCast(currentPos, 0.22f, moveDir, moveSpeed * Time.deltaTime + 0.18f);
                if (hit.collider != null && !hit.collider.isTrigger && hit.collider.gameObject != gameObject)
                {
                    EnterIdleState();
                    return;
                }

                Vector2 step = moveDir * (moveSpeed * Time.deltaTime);
                if (rb != null && rb.bodyType == RigidbodyType2D.Dynamic)
                {
                    rb.linearVelocity = moveDir * moveSpeed;
                }
                else
                {
                    transform.position = new Vector3(currentPos.x + step.x, currentPos.y + step.y, transform.position.z);
                }

                UpdateFacingVisual(moveDir, true);
            }
            else
            {
                if (rb != null && rb.bodyType == RigidbodyType2D.Dynamic)
                {
                    rb.linearVelocity = Vector2.zero;
                }

                if (stateTimer <= 0f)
                {
                    PickNewWanderTarget();
                }
            }
        }

        private void PickNewWanderTarget()
        {
            for (int attempt = 0; attempt < 6; attempt++)
            {
                Vector2 offset = Random.insideUnitCircle * wanderRadius;
                if (offset.magnitude < 0.6f)
                {
                    offset = offset.normalized * 0.9f;
                }

                Vector2 candidate = homePosition + offset;
                Vector2 dir = (candidate - (Vector2)transform.position);
                if (dir.magnitude < 0.4f) continue;

                RaycastHit2D hit = Physics2D.CircleCast(transform.position, 0.22f, dir.normalized, Mathf.Min(dir.magnitude, 1.5f));
                if (hit.collider == null || hit.collider.isTrigger || hit.collider.gameObject == gameObject)
                {
                    targetPosition = candidate;
                    isWalking = true;
                    stateTimer = Random.Range(minWalkTime, maxWalkTime);
                    return;
                }
            }

            // Nếu bị vướng xung quanh thì đứng nghỉ thêm một nhịp
            EnterIdleState();
        }

        private void EnterIdleState()
        {
            isWalking = false;
            stateTimer = Random.Range(minIdleTime, maxIdleTime);
            if (rb != null && rb.bodyType == RigidbodyType2D.Dynamic)
            {
                rb.linearVelocity = Vector2.zero;
            }
        }

        private bool CheckPlayerNearby()
        {
            if (playerTransform == null)
            {
                var p = GameObject.FindGameObjectWithTag("Player");
                if (p != null) playerTransform = p.transform;
                if (playerTransform == null) return false;
            }

            float d = Vector2.Distance(transform.position, playerTransform.position);
            return d <= playerStopDistance;
        }

        private void StopWalkingAndFacePlayer()
        {
            isWalking = false;
            if (rb != null && rb.bodyType == RigidbodyType2D.Dynamic)
            {
                rb.linearVelocity = Vector2.zero;
            }

            if (playerTransform != null)
            {
                Vector2 toPlayer = (Vector2)(playerTransform.position - transform.position);
                UpdateFacingVisual(toPlayer, false);
            }
        }

        /// <summary>
        /// Kích hoạt cơ chế hoảng loạn bỏ chạy khi bị Người Chơi tấn công hoặc bắt hụt.
        /// </summary>
        public void FleeFrom(Vector2 dangerSource)
        {
            Vector2 fleeDir = ((Vector2)transform.position - dangerSource).normalized;
            if (fleeDir == Vector2.zero) fleeDir = Random.insideUnitCircle.normalized;
            targetPosition = (Vector2)transform.position + fleeDir * Mathf.Max(3.5f, wanderRadius);
            isWalking = true;
            stateTimer = 2.5f;
            moveSpeed = Mathf.Max(moveSpeed, 3.2f);
        }

        private void UpdateFacingVisual(Vector2 dir, bool moving)
        {
            if (dir.sqrMagnitude < 0.001f) return;

            if (luminaAnim != null && luminaAnim.enabled)
            {
                luminaAnim.SetFacingDirection(dir);
            }
            else if (dirWalkAnim != null && dirWalkAnim.enabled)
            {
                if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y) * 0.7f)
                {
                    dirWalkAnim.currentFacing = FacingDirection2D.Side;
                    if (sr != null) sr.flipX = (dir.x < 0f);
                }
                else if (dir.y > 0.05f)
                {
                    dirWalkAnim.currentFacing = FacingDirection2D.Back;
                }
                else
                {
                    dirWalkAnim.currentFacing = FacingDirection2D.Front;
                }
            }
            else if (sr != null && Mathf.Abs(dir.x) > 0.05f)
            {
                sr.flipX = (dir.x < 0f);
            }
        }
    }
}
