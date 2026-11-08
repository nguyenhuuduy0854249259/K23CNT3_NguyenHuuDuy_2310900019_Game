using System;
using UnityEngine;
using ActionRPG.Combat;
using ActionRPG.Characters;
using ActionRPG.Classes;
using ActionRPG.Classes.Mechanics;
using ActionRPG.Core;

namespace ActionRPG.Pets
{
    public enum PetAIState
    {
        Idle,       // Thơ thẩn, ngửi hoa cỏ, ngồi cạnh chủ nhân
        Follow,     // Bám theo sát chủ nhân (Cự ly 1.5m - 2.5m)
        Attack,     // Hỗ trợ tấn công mục tiêu Player đang đánh
        Protect,    // Máu chủ nhân < 30%, che chắn làm bia đỡ đạn
        Skill       // Tung kỹ năng chủ động của Pet
    }

    /// <summary>
    /// Bộ não FSM & Dẫn đường thông minh cho Thú Cưng:
    /// - Duy trì khoảng cách an toàn 1.5m - 2.5m với PlayerController.
    /// - Raycast 2D Steering né tránh vật cản / góc tường.
    /// - Tự động Teleport về cạnh Player nếu khoảng cách > 12m.
    /// - Trạng thái Idle (Sniffing, ngắm nghía, nhún nhảy) khi Player đứng yên.
    /// - Hỗ trợ tấn công kẻ địch Player đang đánh.
    /// - Đồng bộ cơ chế 10 Class (nạp hàng đợi nguyên tố, cộng hưởng lưỡng thú).
    /// </summary>
    [RequireComponent(typeof(PetController))]
    [RequireComponent(typeof(Rigidbody2D))]
    public class PetAI : MonoBehaviour
    {
        [Header("Trạng Thái FSM")]
        public PetAIState currentState = PetAIState.Idle;
        [Tooltip("Pet hoang dã sinh sống tại Map, không tự động bám đuôi Player khi chưa được nhận")]
        public bool isWild = false;

        [Header("Khoảng Cách Định Tuyến")]
        public float minFollowDistance = 1.5f;   // Khoảng cách an toàn tối thiểu
        public float maxFollowDistance = 2.5f;   // Khoảng cách bắt đầu bám theo
        public float teleportDistance = 12.0f;   // Quá 12m tự động kéo lại gần (Teleport)
        public float attackRange = 1.8f;         // Tầm đánh của Pet
        public float scanRadius = 7.0f;          // Tầm quét kẻ địch

        [Header("Raycast 2D Steering (Né Vật Cản)")]
        [SerializeField] private float raycastDistance = 0.9f;
        [SerializeField] private LayerMask obstacleLayer;

        [Header("Thời Gian Chiêu & Tấn Công")]
        public float attackCooldown = 1.4f;
        public float skillCooldown = 6.0f;
        private float attackTimer = 0f;
        private float skillTimer = 0f;

        private PetController petController;
        private Rigidbody2D rb;
        private Animator animator;
        private Transform owner;
        private CharacterStats ownerStats;
        private ClassSystem ownerClassSystem;
        private Transform currentTarget;

        // Trạng thái nhàn rỗi (Idle Sniffing / Breathing) & Hướng nhìn 4 chiều (Animator)
        private Vector3 initialScale;
        private float idleAnimTimer = 0f;
        private Vector2 lastFacingDir = Vector2.down;

        private static readonly int HashMoveX = Animator.StringToHash("MoveX");
        private static readonly int HashMoveY = Animator.StringToHash("MoveY");
        private static readonly int HashIsMoving = Animator.StringToHash("IsMoving");

        private void Awake()
        {
            petController = GetComponent<PetController>();
            rb = GetComponent<Rigidbody2D>();
            animator = GetComponent<Animator>();
            initialScale = transform.localScale;
            if (initialScale == Vector3.zero) initialScale = Vector3.one;

            if (obstacleLayer.value == 0)
            {
                // Mặc định quét tường và vật thể chặn (Layer Default hoặc Obstacles)
                obstacleLayer = LayerMask.GetMask("Default", "Obstacle", "Wall");
            }
        }

        private void Start()
        {
            if (isWild || (petController != null && petController.isWild))
            {
                // Pet hoang dã dã ngoại trong Map: Không tự gán Player làm chủ
                return;
            }

            if (petController != null && petController.owner != null)
            {
                SetOwner(petController.owner);
            }
            else
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null) SetOwner(player.transform);
            }
        }

        public void SetOwner(Transform playerTransform)
        {
            owner = playerTransform;
            if (owner != null)
            {
                isWild = false;
                if (petController != null)
                {
                    petController.isWild = false;
                    petController.owner = owner;
                }
                ownerStats = owner.GetComponent<CharacterStats>();
                ownerClassSystem = owner.GetComponent<ClassSystem>();
            }
        }

        private void Update()
        {
            if (owner == null)
            {
                if (isWild || (petController != null && petController.isWild))
                {
                    // Hoang dã: đứng thở nhàn rỗi tại vị trí trên map
                    ExecuteIdle();
                    return;
                }

                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null) SetOwner(player.transform);
                return;
            }

            if (petController.isTransformedIntoWeapon)
            {
                if (rb != null) rb.linearVelocity = Vector2.zero;
                return;
            }

            // Đếm ngược cooldown
            if (attackTimer > 0f) attackTimer -= Time.deltaTime;
            if (skillTimer > 0f) skillTimer -= Time.deltaTime;

            UpdateFSMState();
            ExecuteFSMBehavior();
        }

        #region FSM State Machine
        private void UpdateFSMState()
        {
            float distToOwner = Vector2.Distance(transform.position, owner.position);

            // 1. Kiểm tra Dịch chuyển tức thời (Teleport > 12m)
            if (distToOwner > teleportDistance)
            {
                TeleportToOwner();
                currentState = PetAIState.Idle;
                return;
            }

            // 2. Chế độ BẢO VỆ (Máu chủ nhân tụt dưới 30%)
            if (ownerStats != null && (ownerStats.currentHp / ownerStats.maxHp) < 0.30f)
            {
                currentState = PetAIState.Protect;
                return;
            }

            // 3. Quét mục tiêu: Ưu tiên quái Player đang đánh hoặc quái gần nhất
            FindBestTarget();

            if (currentTarget != null)
            {
                float distToTarget = Vector2.Distance(transform.position, currentTarget.position);

                // Ưu tiên tung Skill nếu sẵn sàng
                if (skillTimer <= 0f && petController.petData != null && petController.petData.activeSkill != null)
                {
                    currentState = PetAIState.Skill;
                    return;
                }

                // Nếu trong tầm đánh -> Đánh
                if (distToTarget <= attackRange)
                {
                    currentState = PetAIState.Attack;
                    return;
                }
                else
                {
                    // Di chuyển áp sát mục tiêu (nhưng không đi quá xa chủ nhân > 8m)
                    if (distToOwner < 8.0f)
                    {
                        currentState = PetAIState.Follow;
                        return;
                    }
                }
            }

            // 4. Khi không có kẻ địch hoặc đi quá xa: Bám theo người chơi
            if (distToOwner > maxFollowDistance)
            {
                currentState = PetAIState.Follow;
            }
            else
            {
                currentState = PetAIState.Idle;
            }
        }

        private void ExecuteFSMBehavior()
        {
            switch (currentState)
            {
                case PetAIState.Idle:
                    ExecuteIdle();
                    break;
                case PetAIState.Follow:
                    ExecuteFollow();
                    break;
                case PetAIState.Attack:
                    ExecuteAttack();
                    break;
                case PetAIState.Protect:
                    ExecuteProtect();
                    break;
                case PetAIState.Skill:
                    ExecuteSkill();
                    break;
            }
        }
        #endregion

        #region Thực Thi Hành Vi
        private bool HasDirectionalAnimator()
        {
            if (animator == null) animator = GetComponent<Animator>();
            return animator != null && animator.runtimeAnimatorController != null;
        }

        public void UpdateAnimatorState(Vector2 moveDirection, bool isMoving)
        {
            if (moveDirection.sqrMagnitude > 0.001f)
            {
                if (Mathf.Abs(moveDirection.x) >= Mathf.Abs(moveDirection.y))
                {
                    lastFacingDir = new Vector2(Mathf.Sign(moveDirection.x), 0f);
                }
                else
                {
                    lastFacingDir = new Vector2(0f, Mathf.Sign(moveDirection.y));
                }
            }

            if (HasDirectionalAnimator())
            {
                animator.SetBool(HashIsMoving, isMoving);
                animator.SetFloat(HashMoveX, lastFacingDir.x);
                animator.SetFloat(HashMoveY, lastFacingDir.y);
            }
        }

        private void ExecuteIdle()
        {
            if (rb != null) rb.linearVelocity = Vector2.zero;
            UpdateAnimatorState(Vector2.zero, false);

            if (HasDirectionalAnimator())
            {
                transform.localScale = new Vector3(Mathf.Abs(initialScale.x), initialScale.y, initialScale.z);
                return;
            }

            // Hiệu ứng thở nhẹ và nhún nhảy nhàn rỗi (Idle Sniffing / Breathing) cho Pet 1 sprite tĩnh
            idleAnimTimer += Time.deltaTime * 3.5f;
            float bounceY = Mathf.Sin(idleAnimTimer) * 0.04f;
            float breathX = Mathf.Cos(idleAnimTimer) * 0.02f;

            float facing = transform.localScale.x >= 0 ? 1f : -1f;
            transform.localScale = new Vector3(facing * (Mathf.Abs(initialScale.x) + breathX), initialScale.y + bounceY, initialScale.z);
        }

        private void ExecuteFollow()
        {
            Vector2 destination = currentTarget != null ? (Vector2)currentTarget.position : (Vector2)owner.position;
            Vector2 currentPos = transform.position;
            float dist = Vector2.Distance(currentPos, destination);

            // Nếu theo chủ nhân và đã vào khoảng an toàn 1.5 - 2.5m thì dừng
            if (currentTarget == null && dist <= minFollowDistance)
            {
                if (rb != null) rb.linearVelocity = Vector2.zero;
                UpdateAnimatorState(Vector2.zero, false);
                return;
            }

            Vector2 desiredDirection = (destination - currentPos).normalized;

            // Áp dụng Raycast 2D Steering Avoidance để tránh kẹt tường
            Vector2 finalDirection = CalculateAvoidanceSteering(currentPos, desiredDirection);

            float speed = petController.petInstance != null ? petController.petInstance.MoveSpeed :
                          (petController.petData != null ? petController.petData.moveSpeed : 5.0f);

            if (rb != null)
            {
                rb.linearVelocity = finalDirection * speed;
            }

            UpdateAnimatorState(finalDirection, finalDirection.sqrMagnitude > 0.001f);

            // Quay mặt theo hướng di chuyển (chỉ lật X khi Pet không có Animator 4 hướng riêng biệt)
            if (HasDirectionalAnimator())
            {
                transform.localScale = new Vector3(Mathf.Abs(initialScale.x), initialScale.y, initialScale.z);
            }
            else if (finalDirection.x != 0)
            {
                transform.localScale = new Vector3(Mathf.Sign(finalDirection.x) * Mathf.Abs(initialScale.x), initialScale.y, initialScale.z);
            }
        }

        private void ExecuteAttack()
        {
            if (rb != null) rb.linearVelocity = Vector2.zero;
            if (currentTarget == null)
            {
                UpdateAnimatorState(Vector2.zero, false);
                return;
            }

            Vector2 dirToTarget = ((Vector2)currentTarget.position - (Vector2)transform.position).normalized;
            UpdateAnimatorState(dirToTarget, false);

            if (attackTimer <= 0f)
            {
                float cd = petController.petData != null ? petController.petData.attackCooldown : attackCooldown;
                attackTimer = cd;

                float atk = petController.petInstance != null ? petController.petInstance.AttackPower :
                            (petController.petData != null ? petController.petData.attackPower : 12f);
                ElementType elem = petController.petData != null ? petController.petData.element : ElementType.Fire;

                // Tấn công gây sát thương
                Hurtbox2D hurt = currentTarget.GetComponent<Hurtbox2D>();
                if (hurt != null)
                {
                    DamageInfo dmg = new DamageInfo(atk, elem, false, gameObject);
                    hurt.ReceiveDamage(dmg);
                    Debug.Log($"<color=orange>[Pet Combat] {petController.petData?.petName} cắn mục tiêu {currentTarget.name} gây {atk} sát thương {elem}!</color>");
                }

                // ĐỒNG BỘ CÁC CLASS ĐÃ CODE:
                TriggerClassSynergyOnHit(elem);
            }
        }

        private void ExecuteProtect()
        {
            // Pet di chuyển vào vị trí đứng giữa Player và kẻ địch để che chắn
            Vector2 targetPos = (Vector2)owner.position;
            if (currentTarget != null)
            {
                targetPos = Vector2.Lerp(owner.position, currentTarget.position, 0.45f);
            }
            else
            {
                targetPos = (Vector2)owner.position + Vector2.left * 1.0f;
            }

            float dist = Vector2.Distance(transform.position, targetPos);
            if (dist <= 0.2f)
            {
                if (rb != null) rb.linearVelocity = Vector2.zero;
                UpdateAnimatorState(Vector2.zero, false);
                return;
            }

            Vector2 dir = (targetPos - (Vector2)transform.position).normalized;
            Vector2 steerDir = CalculateAvoidanceSteering(transform.position, dir);

            float speed = (petController.petInstance != null ? petController.petInstance.MoveSpeed : 5f) * 1.25f;
            if (rb != null) rb.linearVelocity = steerDir * speed;
            UpdateAnimatorState(steerDir, steerDir.sqrMagnitude > 0.001f);
        }

        private void ExecuteSkill()
        {
            if (rb != null) rb.linearVelocity = Vector2.zero;
            if (currentTarget != null)
            {
                Vector2 dirToTarget = ((Vector2)currentTarget.position - (Vector2)transform.position).normalized;
                UpdateAnimatorState(dirToTarget, false);
            }
            else
            {
                UpdateAnimatorState(Vector2.zero, false);
            }
            skillTimer = skillCooldown;

            if (petController.petData != null && petController.petData.activeSkill != null)
            {
                string skillName = petController.petData.activeSkill.skillName;
                Debug.Log($"<color=cyan>[Pet Skill] 🌟 {petController.petData.petName} TUNG CHIÊU: {skillName}!</color>");
                EventBus.OnPetSkillTriggered?.Invoke(skillName, skillCooldown);

                if (currentTarget != null)
                {
                    Hurtbox2D hurt = currentTarget.GetComponent<Hurtbox2D>();
                    if (hurt != null)
                    {
                        float atk = petController.petInstance != null ? petController.petInstance.AttackPower : petController.petData.attackPower;
                        float skillDmg = atk * 2.2f;
                        DamageInfo dmg = new DamageInfo(skillDmg, petController.petData.element, true, gameObject);
                        hurt.ReceiveDamage(dmg);
                    }
                }

                TriggerClassSynergyOnHit(petController.petData.element);
            }

            currentState = PetAIState.Follow;
        }

        public void TeleportToOwner()
        {
            if (owner == null) return;
            Vector3 offset = (Vector3)UnityEngine.Random.insideUnitCircle.normalized * 1.6f;
            transform.position = owner.position + offset;
            if (rb != null) rb.linearVelocity = Vector2.zero;

            if (petController == null) petController = GetComponent<PetController>();
            if (petController != null) petController.PlaySummonEffect();
            Debug.Log($"<color=cyan>[Pet AI] Khoảng cách quá xa (> {teleportDistance}m)! Pet đã Teleport tức thì về cạnh Chủ Nhân!</color>");
        }
        #endregion

        #region Raycast 2D Steering Avoidance
        /// <summary>
        /// Thuật toán Raycast 2D Steering: Bắn chùm tia góc rộng để phát hiện chướng ngại vật/góc tường,
        /// uốn lượn hướng đi mượt mà tránh kẹt góc.
        /// </summary>
        private Vector2 CalculateAvoidanceSteering(Vector2 currentPos, Vector2 desiredDir)
        {
            if (desiredDir == Vector2.zero) return Vector2.zero;

            // Bắn 5 tia: Tia thẳng, lệch trái 30°, lệch phải 30°, lệch trái 60°, lệch phải 60°
            float[] testAngles = { 0f, 30f, -30f, 60f, -60f };
            Vector2 bestDir = desiredDir;
            float maxClearDistance = 0f;

            RaycastHit2D centerHit = Physics2D.Raycast(currentPos, desiredDir, raycastDistance, obstacleLayer);
            if (!centerHit || centerHit.collider.isTrigger)
            {
                // Đường thẳng phía trước hoàn toàn thông thoáng
                return desiredDir;
            }

            // Nếu gặp vật cản phía trước, tìm góc lệch thông thoáng nhất
            foreach (float angle in testAngles)
            {
                Vector2 rotatedDir = Quaternion.Euler(0, 0, angle) * desiredDir;
                RaycastHit2D hit = Physics2D.Raycast(currentPos, rotatedDir, raycastDistance, obstacleLayer);

                if (!hit || hit.collider.isTrigger)
                {
                    // Hướng này thông thoáng, chọn ngay!
                    return rotatedDir;
                }
                else if (hit.distance > maxClearDistance)
                {
                    maxClearDistance = hit.distance;
                    bestDir = rotatedDir;
                }
            }

            // Nếu bị vây kín, uốn theo pháp tuyến bề mặt va chạm để trượt dọc tường
            Vector2 slideDir = Vector2.Perpendicular(centerHit.normal);
            if (Vector2.Dot(slideDir, desiredDir) < 0) slideDir = -slideDir;

            return slideDir.normalized;
        }
        #endregion

        #region Tìm Kiếm Mục Tiêu Thông Minh
        private void FindBestTarget()
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, scanRadius, LayerMask.GetMask("Enemy"));
            float minDist = float.MaxValue;
            Transform nearest = null;

            foreach (var hit in hits)
            {
                if (hit.isTrigger) continue;
                float d = Vector2.Distance(transform.position, hit.transform.position);
                if (d < minDist)
                {
                    minDist = d;
                    nearest = hit.transform;
                }
            }

            currentTarget = nearest;
        }
        #endregion

        #region Đồng Bộ Cơ Chế Các Class (Class Synergy)
        private void TriggerClassSynergyOnHit(ElementType petElement)
        {
            if (ownerClassSystem == null) return;

            // 1. PHÁP SƯ NGUYÊN TỐ: 25% tỷ lệ tự động nạp hạt nguyên tố vào hàng đợi (Ấn 3)
            if (ownerClassSystem.ElementalQueue != null && UnityEngine.Random.value <= 0.25f)
            {
                ownerClassSystem.PushElementalQueue(petElement);
                Debug.Log($"<color=cyan>[Linh Thú Xúc Tác] Đòn đánh Pet nạp hạt nguyên tố {petElement} vào Hàng Đợi!</color>");
            }

            // 2. PHÁP SƯ TRIỆU HỒI: Hỗ trợ kích nổ Cộng Hưởng Lưỡng Thú nếu có 2 Pet
            if (ownerClassSystem.SummonerDualPet != null)
            {
                ownerClassSystem.SummonerDualPet.TriggerDualPetResonance(petElement, petElement);
            }
        }
        #endregion
    }
}
