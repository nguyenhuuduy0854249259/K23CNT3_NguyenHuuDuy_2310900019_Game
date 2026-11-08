using System.Collections;
using UnityEngine;
using ActionRPG.Combat;
using ActionRPG.Characters;
using ActionRPG.Core;

namespace ActionRPG.Enemies
{
    /// <summary>
    /// Bộ điều khiển Boss đa giai đoạn "Hắc Diệu Ma Viên" (Lv 20 - Hệ Hỏa/Ám):
    /// - Phase 1 (HP > 50%): Tấn công vung tay, ném đá dung nham kèm Circle Telegraph (delay 1.0s).
    /// - Transition (HP <= 50%): Gầm thét (Roar), bất tử trong 1.5s, hất văng người chơi xung quanh 4m.
    /// - Phase 2 (Cuồng Nộ - Enrage): Bốc cháy hào quang đỏ, tăng 30% tốc chạy, 25% sát thương,
    ///   thời gian nạp Telegraph giảm xuống 0.7s, liên tục nện đất tạo 3 vòng tròn nổ liên hoàn.
    /// </summary>
    public class BossController : EnemyController
    {
        [Header("Giai Đoạn Boss")]
        public bool isWorldBoss = false;
        public int currentPhase = 1;
        public bool isInvulnerable = false;
        public bool isTransitioning = false;

        [Header("Chiêu Thức Phase 1")]
        public float magmaBoulderCooldown = 5.0f;
        public float magmaRadius = 2.8f;
        public float magmaTelegraphDelay = 1.0f;
        private float magmaBoulderTimer = 2.0f;

        [Header("Chiêu Thức Phase 2 (Cuồng Nộ)")]
        public float tripleSlamCooldown = 4.5f;
        public float enrageTelegraphDelay = 0.7f;
        private float tripleSlamTimer = 3.0f;

        [Header("Âm Hưởng & Hiệu Ứng")]
        public Color enrageAuraColor = new Color(1f, 0.25f, 0.25f, 1f);

        protected override void Awake()
        {
            base.Awake();

            // Cấu hình thông số mặc định cho Boss "Hắc Diệu Ma Viên"
            if (string.IsNullOrEmpty(enemyName) || enemyName == "Quái Dã Ngoại")
            {
                enemyName = "Hắc Diệu Ma Viên";
                level = 20;
                element = ElementType.Fire;
                maxHp = 650f;
                currentHp = 650f;
                attackPower = 28f;
                defense = 8f;
                moveSpeed = 3.2f;
                attackRange = 1.8f;
                attackCooldown = 1.6f;
            }
        }

        protected override void Start()
        {
            base.Start();

            // Thông báo Boss xuất hiện lên HUD
            EventBus.OnBossSpawned?.Invoke(enemyName);
            NotifyBossHpChanged();
        }

        protected override void Update()
        {
            if (currentState == EnemyAIState.Dead || isTransitioning) return;

            // Kiểm tra điều kiện chuyển sang Phase 2 (Cuồng Nộ) khi HP <= 50%
            if (currentPhase == 1 && (currentHp / maxHp) <= 0.5f)
            {
                StartCoroutine(TransitionToPhase2Routine());
                return;
            }

            // Đếm ngược kỹ năng theo giai đoạn
            if (currentPhase == 1)
            {
                magmaBoulderTimer -= Time.deltaTime;
                if (magmaBoulderTimer <= 0f && playerTarget != null && Vector2.Distance(transform.position, playerTarget.position) <= detectionRadius)
                {
                    PerformMagmaBoulderSkill();
                }
            }
            else if (currentPhase == 2)
            {
                tripleSlamTimer -= Time.deltaTime;
                if (tripleSlamTimer <= 0f && playerTarget != null && Vector2.Distance(transform.position, playerTarget.position) <= detectionRadius)
                {
                    PerformTripleGroundSlamSkill();
                }
            }

            base.Update();
        }

        #region Kỹ Năng Phase 1: Ném Đá Dung Nham (Magma Boulder)
        private void PerformMagmaBoulderSkill()
        {
            magmaBoulderTimer = magmaBoulderCooldown;
            if (playerTarget == null) return;

            Vector3 targetPos = playerTarget.position;
            Debug.Log($"<color=orange>[Boss Skill] {enemyName} ném Đá Dung Nham vào vị trí {targetPos}!</color>");

            // Tạo vòng tròn cảnh báo đỏ Telegraph 1.0s
            TelegraphIndicator.CreateCircleTelegraph(
                targetPos,
                magmaRadius,
                magmaTelegraphDelay,
                attackPower * 1.5f,
                ElementType.Fire,
                gameObject
            );
        }
        #endregion

        #region Chuyển Giai Đoạn (Transition Phase 1 -> Phase 2)
        public IEnumerator TransitionToPhase2Routine()
        {
            currentPhase = 2;
            isTransitioning = true;
            isInvulnerable = true;

            if (hurtbox != null) hurtbox.isInvulnerable = true;
            if (rb != null) rb.linearVelocity = Vector2.zero;

            Debug.Log($"<color=red>★★★ GẦM THÉT: {enemyName} PHÁT ĐỘNG CUỒNG NỘ! BẤT TỬ 1.5 GIÂY & ĐẨY LÙI MỌI THỨ! ★★★</color>");

            // 1. Hất văng người chơi xung quanh cự ly 4m (Knockback xung kích)
            KnockbackSurroundings(4.5f, 9.0f);

            // 2. Chớp sáng hào quang chuẩn bị biến hình
            if (spriteRenderer != null)
            {
                spriteRenderer.color = Color.white;
            }

            yield return new WaitForSeconds(1.5f);

            // 3. Hoàn tất chuyển Phase 2
            isInvulnerable = false;
            isTransitioning = false;
            if (hurtbox != null) hurtbox.isInvulnerable = false;

            ApplyPhase2Buffs();

            EventBus.OnBossPhaseChanged?.Invoke(enemyName, 2);
            NotifyBossHpChanged();
        }

        private void ApplyPhase2Buffs()
        {
            // Tăng 30% tốc độ chạy
            moveSpeed *= 1.30f;

            // Tăng 25% sát thương
            attackPower *= 1.25f;

            // Toàn thân bốc cháy hào quang đỏ rực
            if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                spriteRenderer.color = enrageAuraColor;
            }

            Debug.Log($"<color=red>[Boss Enrage] {enemyName}: Tốc chạy +30% ({moveSpeed}), Sát thương +25% ({attackPower}), Nạp Telegraph 0.7s!</color>");
        }

        private void KnockbackSurroundings(float radius, float force)
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, radius);
            foreach (var hit in hits)
            {
                if (hit.gameObject == gameObject) continue;

                if (hit.CompareTag("Player") || hit.CompareTag("Pet"))
                {
                    Rigidbody2D targetRb = hit.GetComponent<Rigidbody2D>();
                    if (targetRb != null)
                    {
                        Vector2 pushDir = ((Vector2)hit.transform.position - (Vector2)transform.position).normalized;
                        if (pushDir == Vector2.zero) pushDir = Vector2.right;
                        targetRb.AddForce(pushDir * force, ForceMode2D.Impulse);
                        Debug.Log($"<color=yellow>[Boss Roar] Sóng xung kích hất văng {hit.name} ra xa 4m!</color>");
                    }
                }
            }
        }
        #endregion

        #region Kỹ Năng Phase 2: Chấn Địa Liên Hoàn (Triple Ground Slam)
        private void PerformTripleGroundSlamSkill()
        {
            tripleSlamTimer = tripleSlamCooldown;
            if (playerTarget == null) return;

            Debug.Log($"<color=red>[Boss Cuồng Nộ] {enemyName} liên tục nện đất tạo 3 vòng tròn nổ liên hoàn!</color>");

            StartCoroutine(TripleGroundSlamRoutine());
        }

        private IEnumerator TripleGroundSlamRoutine()
        {
            Vector3 center = transform.position;
            Vector3 targetDir = playerTarget != null ? (playerTarget.position - center).normalized : Vector3.right;

            // 3 vòng nổ theo chuỗi thẳng hoặc bao quanh:
            Vector3[] slamPositions = new Vector3[]
            {
                center + targetDir * 1.5f,
                center + targetDir * 3.5f,
                center + targetDir * 5.5f
            };

            for (int i = 0; i < slamPositions.Length; i++)
            {
                Vector3 pos = slamPositions[i];
                TelegraphIndicator.CreateCircleTelegraph(
                    pos,
                    2.4f,
                    enrageTelegraphDelay, // Nạp nhanh 0.7s
                    attackPower * 1.35f,
                    ElementType.Fire,
                    gameObject
                );

                yield return new WaitForSeconds(0.25f);
            }
        }
        #endregion

        #region Sát Thương & Thông Báo HUD
        protected override void OnHurtboxDamaged(DamageInfo info)
        {
            if (isInvulnerable || isTransitioning)
            {
                Debug.Log($"<color=yellow>[Boss Bất Tử] {enemyName} đang trong thời gian gầm thét, miễn nhiễm mọi sát thương!</color>");
                return;
            }

            base.OnHurtboxDamaged(info);
            NotifyBossHpChanged();
        }

        private void NotifyBossHpChanged()
        {
            EventBus.OnBossHpChanged?.Invoke(enemyName, currentHp, maxHp, element);
        }

        protected override void Die()
        {
            Debug.Log($"<color=gold>★★★★★ CHÚC MỪNG! BẠN ĐÃ TIÊU DIỆT BOSS {enemyName}! ★★★★★</color>");
            EventBus.OnBossDefeated?.Invoke(enemyName);
            base.Die();
        }
        #endregion
    }
}
