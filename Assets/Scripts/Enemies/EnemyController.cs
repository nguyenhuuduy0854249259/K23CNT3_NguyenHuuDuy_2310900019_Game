using System.Collections;
using UnityEngine;
using ActionRPG.Combat;
using ActionRPG.Characters;
using ActionRPG.Core;
using ActionRPG.Items;

namespace ActionRPG.Enemies
{
    public enum EnemyAIState
    {
        Patrol,     // Đi tuần quanh ổ bán kính 3m
        Chase,      // Đuổi theo Player khi vào tầm phát hiện 5-7m
        Attack,     // Dừng lại tung chiêu khi vào cự ly tấn công (CD 1.5s - 2s)
        Dead        // Bị tiêu diệt
    }

    /// <summary>
    /// Bộ điều khiển máy trạng thái (FSM) 4 trạng thái cho quái vật:
    /// - Patrol: Đi tuần ngẫu nhiên quanh điểm xuất hiện (bán kính 3m).
    /// - Chase: Truy đuổi Player khi vào tầm 5 - 7m.
    /// - Attack: Dừng lại tung đòn đánh (Cận chiến hoặc Tầm xa bắn đạn nguyên tố).
    /// - Dead: Tử trận.
    /// Tích hợp thanh máu World-space và liên kết chặt chẽ với EnemyStats.
    /// </summary>
    public class EnemyController : MonoBehaviour
    {
        [Header("Dữ Liệu Quái Vật")]
        public EnemyData enemyData;

        [Header("Thông Số Hiện Tại")]
        public string enemyName = "Quái Dã Ngoại";
        public int level = 1;
        public ElementType element = ElementType.None;
        public float maxHp = 50f;
        public float currentHp = 50f;

        [Tooltip("Sát thương vật lý (Physical Attack)")]
        public float physicalAtk = 8f;

        [Tooltip("Sát thương phép thuật (Magic Attack)")]
        public float magicAtk = 0f;

        [Tooltip("Giáp vật lý (Physical Defense)")]
        public float physicalDef = 2f;

        [Tooltip("Giáp phép / Kháng phép (Magic Defense)")]
        public float magicDef = 2f;

        [Tooltip("Tốc độ di chuyển")]
        public float moveSpeed = 2.8f;

        [Tooltip("Tốc độ tấn công")]
        public float attackSpeed = 1.0f;

        // Thuộc tính kế thừa tương thích ngược
        public float attackPower { get => physicalAtk; set => physicalAtk = value; }
        public float defense { get => physicalDef; set => physicalDef = value; }

        [Header("Tầm & Tần Suất Chiến Đấu")]
        public float detectionRadius = 6.0f;
        public float attackRange = 1.3f;
        public float attackCooldown = 1.8f;
        public bool isRanged = false;
        public float projectileSpeed = 6.0f;
        public GameObject projectilePrefab;

        [Header("Máy Trạng Thái FSM")]
        public EnemyAIState currentState = EnemyAIState.Patrol;
        public float patrolRadius = 3.0f;

        protected Transform playerTarget;
        protected Rigidbody2D rb;
        protected SpriteRenderer spriteRenderer;
        protected Hurtbox2D hurtbox;
        protected EnemyStats enemyStats;

        protected Vector2 spawnPosition;
        protected Vector2 currentPatrolTarget;
        protected float patrolWaitTimer = 0f;
        protected float attackTimer = 0f;
        protected bool isEnraged = false;

        [Header("Hiệu Ứng Bất Lợi (Debuffs)")]
        public float stunTimer = 0f;
        public float slowTimer = 0f;
        public float slowMultiplier = 1f;
        public float rootTimer = 0f;

        public void ApplyStun(float duration)
        {
            stunTimer = Mathf.Max(stunTimer, duration);
            if (rb != null) rb.linearVelocity = Vector2.zero;
        }

        public void ApplySlow(float percent, float duration)
        {
            slowTimer = Mathf.Max(slowTimer, duration);
            slowMultiplier = Mathf.Clamp(1f - percent, 0.1f, 1f);
        }

        public void ApplyRoot(float duration)
        {
            rootTimer = Mathf.Max(rootTimer, duration);
            if (rb != null) rb.linearVelocity = Vector2.zero;
        }

        protected virtual void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            hurtbox = GetComponent<Hurtbox2D>();
            enemyStats = GetComponent<EnemyStats>();

            if (hurtbox != null)
            {
                hurtbox.OnTakeDamageCustom += OnHurtboxDamaged;
            }

            spawnPosition = transform.position;
            currentPatrolTarget = spawnPosition;
        }

        protected virtual void Start()
        {
            if (enemyData != null)
            {
                Initialize(enemyData);
            }

            FindPlayer();

            // Tự động gắn thanh máu World-space nếu chưa có
            EnsureOverheadUI();
        }

        public virtual void Initialize(EnemyData data)
        {
            if (data == null) return;
            enemyData = data;
            enemyName = data.enemyName;
            level = data.level;
            element = data.element;
            maxHp = data.maxHp;
            currentHp = maxHp;
            physicalAtk = data.physicalAtk > 0 ? data.physicalAtk : data.attackPower;
            magicAtk = data.magicAtk;
            physicalDef = data.physicalDef > 0 ? data.physicalDef : data.defense;
            magicDef = data.magicDef > 0 ? data.magicDef : data.defense;
            moveSpeed = data.moveSpeed;
            attackSpeed = data.attackSpeed > 0 ? data.attackSpeed : 1.0f;
            detectionRadius = data.detectionRadius > 0 ? data.detectionRadius : 6.0f;
            attackRange = data.attackRange > 0 ? data.attackRange : 1.3f;
            attackCooldown = data.attackCooldown > 0 ? data.attackCooldown : 1.8f;
            isRanged = data.isRanged;
            projectileSpeed = data.projectileSpeed > 0 ? data.projectileSpeed : 6.0f;

            if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null && data.sprite != null)
            {
                spriteRenderer.sprite = data.sprite;
            }

            if (enemyStats != null)
            {
                enemyStats.Initialize(data);
            }
        }

        protected virtual void Update()
        {
            if (currentState == EnemyAIState.Dead) return;

            // Xử lý đếm ngược Stun (Choáng)
            if (stunTimer > 0f)
            {
                stunTimer -= Time.deltaTime;
                if (rb != null) rb.linearVelocity = Vector2.zero;
                return;
            }

            // Xử lý đếm ngược Slow (Làm chậm)
            if (slowTimer > 0f)
            {
                slowTimer -= Time.deltaTime;
                if (slowTimer <= 0f) slowMultiplier = 1f;
            }

            // Xử lý đếm ngược Root (Trói chân)
            if (rootTimer > 0f)
            {
                rootTimer -= Time.deltaTime;
                if (rb != null) rb.linearVelocity = Vector2.zero;
            }

            if (attackTimer > 0f) attackTimer -= Time.deltaTime;

            if (playerTarget == null)
            {
                FindPlayer();
            }

            UpdateFSMState();
            ExecuteFSMBehavior();
        }

        protected void FindPlayer()
        {
            if (CharacterStats.PlayerInstance != null)
            {
                playerTarget = CharacterStats.PlayerInstance.transform;
            }
            else
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null) playerTarget = player.transform;
            }
        }

        #region FSM State Machine
        protected virtual void UpdateFSMState()
        {
            if (currentHp <= 0f)
            {
                currentState = EnemyAIState.Dead;
                return;
            }

            if (playerTarget == null)
            {
                currentState = EnemyAIState.Patrol;
                return;
            }

            // Ranh giới an toàn: Nếu Player chạy vào Làng Tân Thủ (cự ly < 16m tính từ tâm gốc (0,0)), quái vật lập tức mất dấu và quay đầu về
            float playerDistToVillage = Vector2.Distance(playerTarget.position, Vector2.zero);
            float selfDistToVillage = Vector2.Distance(transform.position, Vector2.zero);
            if (playerDistToVillage < 16f || selfDistToVillage < 16f)
            {
                currentState = EnemyAIState.Patrol;
                currentPatrolTarget = spawnPosition;
                return;
            }

            float distToPlayer = Vector2.Distance(transform.position, playerTarget.position);

            switch (currentState)
            {
                case EnemyAIState.Patrol:
                    if (distToPlayer <= detectionRadius)
                    {
                        currentState = EnemyAIState.Chase;
                    }
                    break;

                case EnemyAIState.Chase:
                    if (distToPlayer <= attackRange)
                    {
                        currentState = EnemyAIState.Attack;
                    }
                    else if (distToPlayer > detectionRadius * 1.6f)
                    {
                        // Người chơi thoát khỏi tầm truy đuổi, quay về tuần tra
                        currentState = EnemyAIState.Patrol;
                        currentPatrolTarget = spawnPosition;
                    }
                    break;

                case EnemyAIState.Attack:
                    if (distToPlayer > attackRange * 1.3f)
                    {
                        currentState = EnemyAIState.Chase;
                    }
                    break;
            }
        }

        protected virtual void ExecuteFSMBehavior()
        {
            switch (currentState)
            {
                case EnemyAIState.Patrol:
                    ExecutePatrol();
                    break;

                case EnemyAIState.Chase:
                    ExecuteChase();
                    break;

                case EnemyAIState.Attack:
                    ExecuteAttack();
                    break;

                case EnemyAIState.Dead:
                    if (rb != null) rb.linearVelocity = Vector2.zero;
                    break;
            }
        }

        protected virtual void ExecutePatrol()
        {
            if (patrolWaitTimer > 0f)
            {
                patrolWaitTimer -= Time.deltaTime;
                if (rb != null) rb.linearVelocity = Vector2.zero;
                return;
            }

            float dist = Vector2.Distance(transform.position, currentPatrolTarget);
            if (dist <= 0.3f)
            {
                // Đến điểm tuần tra -> Nghỉ ngơi 1.5 - 2.5s rồi chọn điểm mới
                patrolWaitTimer = Random.Range(1.5f, 2.5f);
                Vector2 randomOffset = Random.insideUnitCircle * patrolRadius;
                currentPatrolTarget = spawnPosition + randomOffset;
                if (rb != null) rb.linearVelocity = Vector2.zero;
                return;
            }

            if (rootTimer > 0f)
            {
                if (rb != null) rb.linearVelocity = Vector2.zero;
                return;
            }

            Vector2 moveDir = (currentPatrolTarget - (Vector2)transform.position).normalized;
            float patrolSpeed = moveSpeed * 0.6f * slowMultiplier;
            if (rb != null) rb.linearVelocity = moveDir * patrolSpeed;
            FlipSprite(moveDir.x);
        }

        protected virtual void ExecuteChase()
        {
            if (playerTarget == null) return;

            if (rootTimer > 0f)
            {
                if (rb != null) rb.linearVelocity = Vector2.zero;
                return;
            }

            Vector2 dir = ((Vector2)playerTarget.position - (Vector2)transform.position).normalized;
            float speed = (isEnraged ? moveSpeed * 1.35f : moveSpeed) * slowMultiplier;
            if (rb != null) rb.linearVelocity = dir * speed;
            FlipSprite(dir.x);
        }

        protected virtual void ExecuteAttack()
        {
            if (rb != null) rb.linearVelocity = Vector2.zero;

            if (playerTarget != null)
            {
                FlipSprite(playerTarget.position.x - transform.position.x);
            }

            if (attackTimer <= 0f)
            {
                PerformAttack();
            }
        }

        public virtual void PerformAttack()
        {
            attackTimer = attackCooldown;

            if (isRanged)
            {
                PerformRangedAttack();
            }
            else
            {
                PerformMeleeAttack();
            }
        }

        protected virtual void PerformMeleeAttack()
        {
            if (playerTarget == null) return;

            Hurtbox2D playerHurt = playerTarget.GetComponent<Hurtbox2D>();
            if (playerHurt != null)
            {
                DamageType dmgType = (magicAtk > physicalAtk && element != ElementType.None) ? DamageType.Magical : DamageType.Physical;
                float atkStat = dmgType == DamageType.Magical ? magicAtk : physicalAtk;
                float atk = isEnraged ? atkStat * 1.3f : atkStat;

                DamageInfo dmg = new DamageInfo(atk, element, false, gameObject, dmgType);
                dmg.knockbackForce = 3.5f;
                playerHurt.ReceiveDamage(dmg);
                Debug.Log($"<color=orange>[Enemy Melee] {enemyName} tấn công Player gây {atk} sát thương ({dmgType})!</color>");
            }
        }

        protected virtual void PerformRangedAttack()
        {
            if (playerTarget == null) return;

            Vector2 shootDir = ((Vector2)playerTarget.position - (Vector2)transform.position).normalized;
            Debug.Log($"<color=cyan>[Enemy Ranged] {enemyName} bắn đạn nguyên tố {element} về phía Player!</color>");

            // Tạo GameObject Projectile
            GameObject projObj = projectilePrefab != null ? Instantiate(projectilePrefab, transform.position, Quaternion.identity) : new GameObject($"Projectile_{enemyName}");
            projObj.transform.position = transform.position + (Vector3)shootDir * 0.5f;

            var projScript = projObj.GetComponent<EnemyProjectile>();
            if (projScript == null)
            {
                projScript = projObj.AddComponent<EnemyProjectile>();
            }

            DamageType dmgType = (magicAtk > 0 || element != ElementType.None) ? DamageType.Magical : DamageType.Physical;
            float atkStat = dmgType == DamageType.Magical && magicAtk > 0 ? magicAtk : physicalAtk;
            float atk = isEnraged ? atkStat * 1.25f : atkStat;
            projScript.Initialize(shootDir, projectileSpeed, atk, element, gameObject, dmgType);
        }

        protected void FlipSprite(float dirX)
        {
            if (Mathf.Abs(dirX) > 0.05f)
            {
                transform.localScale = new Vector3(Mathf.Sign(dirX) * Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            }
        }
        #endregion

        #region Nhận Đòn & Tử Trận
        protected virtual void OnHurtboxDamaged(DamageInfo info)
        {
            if (info.statusEffect == StatusEffectType.Stun)
            {
                ApplyStun(info.statusDuration > 0 ? info.statusDuration : 0.75f);
            }
            else if (info.statusEffect == StatusEffectType.Slow)
            {
                ApplySlow(0.4f, info.statusDuration > 0 ? info.statusDuration : 2.0f);
            }

            if (enemyStats != null)
            {
                enemyStats.TakeDamage(info);
                currentHp = enemyStats.currentHp;
            }
            else
            {
                // Fallback nếu không có EnemyStats đính kèm
                float defReduction = defense * 0.5f;
                float elemMult = ElementalSynergy.GetElementalMultiplier(info.element, element);
                float finalDamage = Mathf.Max(1f, (info.rawDamage - defReduction) * elemMult);

                currentHp = Mathf.Max(0f, currentHp - finalDamage);
                EventBus.OnShowDamageText?.Invoke(transform.position + Vector3.up * 0.5f, finalDamage, info.isCrit, false);

                if (currentHp <= 0f)
                {
                    Die();
                }
            }
        }

        public virtual void Enrage()
        {
            isEnraged = true;
            if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null) spriteRenderer.color = new Color(1f, 0.45f, 0.45f);
            Debug.Log($"<color=red>[Enemy] {enemyName} đã NỔI GIẬN (Cuồng Nộ)!</color>");
        }

        protected virtual void Die()
        {
            currentState = EnemyAIState.Dead;
            if (enemyStats != null)
            {
                enemyStats.Die();
            }
            else
            {
                int exp = enemyData != null ? enemyData.expReward : 20;
                int gold = enemyData != null ? enemyData.goldReward : 10;
                CharacterStats.PlayerInstance?.GainExp(exp);
                Inventory.Instance?.AddGold(gold);
                EventBus.OnEnemyKilled?.Invoke(gameObject, exp, gold);
                Destroy(gameObject);
            }
        }

        public void EnsureOverheadUI()
        {
            if (GetComponent<ActionRPG.UI.EnemyOverheadUI>() == null)
            {
                gameObject.AddComponent<ActionRPG.UI.EnemyOverheadUI>();
            }
        }

        protected void EnsureHealthBarWorldSpace()
        {
            EnsureOverheadUI();
        }
        #endregion
    }

    /// <summary>
    /// Đạn nguyên tố bắn ra từ quái vật tầm xa.
    /// </summary>
    public class EnemyProjectile : MonoBehaviour
    {
        private Vector2 direction;
        private float speed = 6f;
        private float damage = 10f;
        private ElementType element = ElementType.None;
        private GameObject attacker;
        private float lifetime = 4.0f;
        private DamageType damageType = DamageType.Magical;

        public void Initialize(Vector2 dir, float spd, float dmg, ElementType elem, GameObject source, DamageType dmgType = DamageType.Magical)
        {
            direction = dir.normalized;
            speed = spd;
            damage = dmg;
            element = elem;
            attacker = source;
            damageType = dmgType;

            // Xoay hướng đạn theo vector bay
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);

            // Gán hiển thị đơn giản nếu chưa có SpriteRenderer
            var sr = GetComponent<SpriteRenderer>();
            if (sr == null)
            {
                sr = gameObject.AddComponent<SpriteRenderer>();
                sr.color = ElementalSynergy.GetElementColor(element);
            }

            var col = GetComponent<CircleCollider2D>();
            if (col == null)
            {
                col = gameObject.AddComponent<CircleCollider2D>();
                col.isTrigger = true;
                col.radius = 0.25f;
            }
        }

        private void Update()
        {
            transform.position += (Vector3)(direction * speed * Time.deltaTime);

            lifetime -= Time.deltaTime;
            if (lifetime <= 0f)
            {
                Destroy(gameObject);
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.CompareTag("Player"))
            {
                Hurtbox2D hurt = collision.GetComponent<Hurtbox2D>();
                if (hurt != null)
                {
                    DamageInfo dmg = new DamageInfo(damage, element, false, attacker, damageType);
                    dmg.knockbackForce = 2.5f;
                    hurt.ReceiveDamage(dmg);
                }
                Destroy(gameObject);
            }
            else if (!collision.isTrigger && !collision.CompareTag("Enemy") && !collision.CompareTag("Pet"))
            {
                // Va chạm tường hoặc chướng ngại vật
                Destroy(gameObject);
            }
        }
    }
}
