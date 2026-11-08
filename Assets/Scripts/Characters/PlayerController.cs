using UnityEngine;
using ActionRPG.Core;
using ActionRPG.Combat;
using ActionRPG.Items;
using ActionRPG.Pets;

namespace ActionRPG.Characters
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(CharacterStats))]
    [RequireComponent(typeof(Hurtbox2D))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Tùy Chọn Di Chuyển")]
        private Rigidbody2D rb;
        private CharacterStats stats;
        private ClassSystem classSystem;
        private Hurtbox2D hurtbox;
        private SpriteRenderer spriteRenderer;
        private Animator animator;
        [Header("Khu Vực An Toàn")]
        public bool isSafeZone = false;

        [Header("Lướt Né Đòn (Dash)")]
        [SerializeField] private float dashSpeed = 10.5f;
        [SerializeField] private float dashDuration = 0.22f;
        [SerializeField] private float dashCooldown = 0.8f;
        private float dashTimer = 0f;
        private float dashCooldownTimer = 0f;
        private bool isDashing = false;
        public bool IsDashing => isDashing;
        private Vector2 dashDirection;

        [Header("Tấn Công & Vũ Khí")]
        [SerializeField] private Transform attackPoint;
        [SerializeField] private float attackRadius = 1.0f;
        [SerializeField] private LayerMask enemyLayers;
        [SerializeField] private GameObject basicProjectilePrefab;
        private float attackCooldownTimer = 0f;

        [Header("Gồng Lực (Bow Charge)")]
        private float chargeTimer = 0f;
        private bool isCharging = false;

        private Vector2 moveInput;
        private Vector2 lastFacingDirection = Vector2.right;
        private System.Collections.Generic.Dictionary<string, float> skillCooldownTimers = new System.Collections.Generic.Dictionary<string, float>();

        [Header("Tỉ Lệ Kích Thước Nhân Vật (Character Scale 1.5x)")]
        [SerializeField] private float characterScale = 1.5f;
        public float CharacterScale
        {
            get => characterScale;
            set
            {
                characterScale = Mathf.Max(0.1f, value);
                transform.localScale = new Vector3(characterScale, characterScale, 1f);
            }
        }

        [Header("Hệ Thống Thú Cưng Đồng Hành")]
        public PlayerPetRoster petRoster;
        public Transform lastTarget;

        private void OnEnable()
        {
            Core.EventBus.OnSkillRequested += HandleSkillRequested;
        }

        private void OnDisable()
        {
            Core.EventBus.OnSkillRequested -= HandleSkillRequested;
        }

        private void Awake()
        {
            if (Time.timeScale <= 0f) Time.timeScale = 1f;

            if (characterScale <= 0f) characterScale = 1.5f;
            transform.localScale = new Vector3(characterScale, characterScale, 1f);

            rb = GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.gravityScale = 0f;
                rb.freezeRotation = true;
                rb.bodyType = RigidbodyType2D.Dynamic;
            }

            stats = GetComponent<CharacterStats>();
            if (stats == null) stats = gameObject.AddComponent<CharacterStats>();

            classSystem = GetComponent<ClassSystem>();
            if (classSystem == null) classSystem = gameObject.AddComponent<ClassSystem>();
            hurtbox = GetComponent<Hurtbox2D>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            animator = GetComponent<Animator>();

            // Khởi tạo PlayerPetRoster
            if (petRoster == null) petRoster = GetComponent<PlayerPetRoster>();
            if (petRoster == null) petRoster = gameObject.AddComponent<PlayerPetRoster>();

            // Khởi tạo PlayerSkillCombat2D (Hệ thống kỹ năng 360 combat & chiêu thức)
            if (GetComponent<PlayerSkillCombat2D>() == null)
            {
                gameObject.AddComponent<PlayerSkillCombat2D>();
            }

            // Khởi tạo LuminaFourDirectionCharacterAnimator (Hoạt ảnh 4 hướng & chuyển spritesheet theo Class)
            if (GetComponent<ActionRPG.Visuals.LuminaFourDirectionCharacterAnimator>() == null)
            {
                gameObject.AddComponent<ActionRPG.Visuals.LuminaFourDirectionCharacterAnimator>();
            }

            // Khởi tạo WeaponSocketView nếu chưa có
            if (GetComponent<ActionRPG.Presentation.Views.WeaponSocketView>() == null)
            {
                gameObject.AddComponent<ActionRPG.Presentation.Views.WeaponSocketView>();
            }

            // Khởi tạo attackPoint nếu chưa có
            if (attackPoint == null)
            {
                GameObject ap = new GameObject("AttackPoint");
                ap.transform.SetParent(transform);
                ap.transform.localPosition = new Vector3(0.6f, 0, 0);
                attackPoint = ap.transform;
            }
        }

        private void Update()
        {
            HandleInputs();
            UpdateTimers();
            UpdateAnimation();
        }

        private void UpdateAnimation()
        {
            bool isMoving = moveInput.sqrMagnitude > 0.01f;
            float speed = isMoving ? (rb != null ? Mathf.Max(rb.linearVelocity.magnitude, 1f) : 1f) : 0f;

            if (animator != null)
            {
                animator.SetFloat("Speed", speed);
                animator.SetBool("IsMoving", isMoving);
            }

            var switcher = GetComponent<ActionRPG.Player.PlayerVisualSwitcher>();
            if (switcher != null)
            {
                switcher.SetMovingState(isMoving, speed);
            }
        }

        private void FixedUpdate()
        {
            HandleMovement();
        }

        private void HandleInputs()
        {
            // Di chuyển 8 hướng (Hỗ trợ Input Manager Axis + Phím cứng WASD / Mũi Tên dự phòng)
            float moveX = 0f;
            float moveY = 0f;

            try
            {
                moveX = Input.GetAxisRaw("Horizontal");
                moveY = Input.GetAxisRaw("Vertical");
            }
            catch { }

            // Fallback nhận diện phím bấm trực tiếp bằng KeyCode phòng trường hợp Axis Input Manager chưa nhạy
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) moveX = -1f;
            else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) moveX = 1f;

            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) moveY = -1f;
            else if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) moveY = 1f;

#if ENABLE_INPUT_SYSTEM
            if (UnityEngine.InputSystem.Keyboard.current != null)
            {
                var kb = UnityEngine.InputSystem.Keyboard.current;
                if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) moveX = -1f;
                else if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) moveX = 1f;

                if (kb.sKey.isPressed || kb.downArrowKey.isPressed) moveY = -1f;
                else if (kb.wKey.isPressed || kb.upArrowKey.isPressed) moveY = 1f;
            }
#endif
            moveInput = new Vector2(moveX, moveY).normalized;

            if (moveInput.sqrMagnitude > 0.01f)
            {
                lastFacingDirection = moveInput;
                if (attackPoint != null)
                {
                    attackPoint.localPosition = lastFacingDirection * 0.7f;
                }

                // Bảo đảm Transform.localScale của Player giữ nguyên theo tỷ lệ nhân vật (1.5x), không dùng scale âm
                transform.localScale = new Vector3(characterScale, characterScale, 1f);

                // Lật sprite theo hướng di chuyển (chỉ áp dụng nếu không dùng bộ 4 hướng riêng biệt)
                var fourDir = GetComponent<ActionRPG.Visuals.LuminaFourDirectionCharacterAnimator>();
                if (fourDir != null && fourDir.enabled)
                {
                    if (spriteRenderer != null) spriteRenderer.flipX = false;
                }
                else if (moveX != 0)
                {
                    if (spriteRenderer != null)
                    {
                        spriteRenderer.flipX = (moveX < 0);
                    }
                    var cvView = GetComponent<ActionRPG.Presentation.Views.CharacterVisualView>();
                    if (cvView != null)
                    {
                        cvView.SetFacingDirection(moveX < 0 ? -1 : 1);
                    }
                }
            }

            // Phím Space: Lướt né đòn (Dash / Roll với i-frame)
            if (Input.GetKeyDown(KeyCode.Space) && dashCooldownTimer <= 0f && !isDashing)
            {
                StartDash();
            }

            // Nếu GameObject đã có hệ thống PlayerSkillCombat2D (360 Combat mới), nhường quyền xử lý tấn công & kỹ năng
            var combat2D = GetComponent<ActionRPG.Combat.PlayerSkillCombat2D>();
            if (combat2D != null && combat2D.enabled)
            {
                // Phím V hoặc C: Thu phục quái vật gần đó nếu máu < 30% (Chỉ can thiệp khi có quái hợp lệ, không chặn mở UI Nhân Vật [C])
                if (Input.GetKeyDown(KeyCode.V) || (Input.GetKeyDown(KeyCode.C) && HasCapturableEnemyNearby()))
                {
                    TryCaptureNearbyEnemy();
                }

                // Phím F: Tương tác với NPC nếu đứng gần
                if (Input.GetKeyDown(KeyCode.F))
                {
                    TryInteractWithNPC();
                }
                return;
            }

            // Chuột Trái hoặc Phím J: Tấn công cơ bản / Vung kiếm (hoặc gồng lực bắn cung)
            bool attackPressed = Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.J);
            bool attackReleased = Input.GetMouseButtonUp(0) || Input.GetKeyUp(KeyCode.J);

            if (attackPressed)
            {
                if (isSafeZone)
                {
                    EventBus.OnShowUIMessage?.Invoke("🛡️ Vùng an toàn: Không thể vung vũ khí!");
                }
                else if (attackCooldownTimer <= 0f)
                {
                    isCharging = true;
                    chargeTimer = 0f;
                }
            }

            if (isCharging)
            {
                chargeTimer += Time.deltaTime;
                if (attackReleased)
                {
                    isCharging = false;
                    PerformAttack(chargeTimer);
                    chargeTimer = 0f;
                }
            }

            // Chuột Phải: Chặn đòn (Active Block cho Kiếm & Khiên)
            if (classSystem != null)
            {
                if (Input.GetMouseButton(1))
                {
                    classSystem.isBlocking = true;
                }
                else
                {
                    classSystem.isBlocking = false;
                }
            }

            // Phím X: Chuyển đổi Stance (Thiên Thần ↔ Ác Quỷ cho Mục Sư Thánh - Ma)
            if (Input.GetKeyDown(KeyCode.X) && classSystem != null)
            {
                classSystem.ToggleStance();
            }

            // Phím 1 đến 6 (Alpha1..Alpha6 hoặc Keypad1..Keypad6): Tung kỹ năng tương ứng với Class và Skill Bar
            if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1)) { if (isSafeZone) EventBus.OnShowUIMessage?.Invoke("🛡️ Vùng an toàn: Không thể tung chiêu!"); else CastSkillSlot(0); }
            if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2)) { if (isSafeZone) EventBus.OnShowUIMessage?.Invoke("🛡️ Vùng an toàn: Không thể tung chiêu!"); else CastSkillSlot(1); }
            if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3)) { if (isSafeZone) EventBus.OnShowUIMessage?.Invoke("🛡️ Vùng an toàn: Không thể tung chiêu!"); else CastSkillSlot(2); }
            if (Input.GetKeyDown(KeyCode.Alpha4) || Input.GetKeyDown(KeyCode.Keypad4)) { if (isSafeZone) EventBus.OnShowUIMessage?.Invoke("🛡️ Vùng an toàn: Không thể tung chiêu!"); else CastSkillSlot(3); }
            if (Input.GetKeyDown(KeyCode.Alpha5) || Input.GetKeyDown(KeyCode.Keypad5)) { if (isSafeZone) EventBus.OnShowUIMessage?.Invoke("🛡️ Vùng an toàn: Không thể tung chiêu!"); else CastSkillSlot(4); }
            if (Input.GetKeyDown(KeyCode.Alpha6) || Input.GetKeyDown(KeyCode.Keypad6)) { if (isSafeZone) EventBus.OnShowUIMessage?.Invoke("🛡️ Vùng an toàn: Không thể tung chiêu!"); else CastSkillSlot(5); }

            // Phím V hoặc C: Thu phục quái vật gần đó nếu máu < 30% (Chỉ can thiệp khi có quái hợp lệ, không chặn mở UI Nhân Vật [C])
            if (Input.GetKeyDown(KeyCode.V) || (Input.GetKeyDown(KeyCode.C) && HasCapturableEnemyNearby()))
            {
                TryCaptureNearbyEnemy();
            }

            // Phím F: Tương tác với NPC nếu đứng gần
            if (Input.GetKeyDown(KeyCode.F))
            {
                TryInteractWithNPC();
            }
        }

        private void HandleMovement()
        {
            if (rb == null) return;

            if (isDashing)
            {
                rb.linearVelocity = dashDirection * dashSpeed;
                return;
            }

            float currentSpeed = (stats != null) ? stats.TotalMoveSpeed : 4.5f;
            if (currentSpeed <= 0.1f) currentSpeed = 4.5f;

            if (classSystem != null && classSystem.isBlocking)
            {
                currentSpeed *= 0.35f; // Đi chậm khi giơ khiên
            }

            rb.linearVelocity = moveInput * currentSpeed;
        }

        private void StartDash()
        {
            isDashing = true;
            dashTimer = dashDuration;
            dashCooldownTimer = dashCooldown;
            dashDirection = moveInput.sqrMagnitude > 0.01f ? moveInput : lastFacingDirection;

            // Khung hình bất tử (i-frame) khi lướt
            if (hurtbox != null) hurtbox.isInvulnerable = true;

            Debug.Log("[Combat] Lướt né đòn (i-frame kích hoạt)!");
        }

        private void UpdateTimers()
        {
            if (attackCooldownTimer > 0f) attackCooldownTimer -= Time.deltaTime;
            if (dashCooldownTimer > 0f) dashCooldownTimer -= Time.deltaTime;

            if (isDashing)
            {
                dashTimer -= Time.deltaTime;
                if (dashTimer <= 0f)
                {
                    isDashing = false;
                    if (hurtbox != null) hurtbox.isInvulnerable = false;
                }
            }

            // Đếm ngược Cooldown Kỹ Năng
            if (skillCooldownTimers.Count > 0)
            {
                var keys = new System.Collections.Generic.List<string>(skillCooldownTimers.Keys);
                foreach (var k in keys)
                {
                    if (skillCooldownTimers[k] > 0f)
                    {
                        skillCooldownTimers[k] -= Time.deltaTime;
                        if (skillCooldownTimers[k] <= 0f)
                        {
                            skillCooldownTimers[k] = 0f;
                            Core.EventBus.OnSkillCooldown?.Invoke(k, 0f);
                        }
                    }
                }
            }
        }

        private void HandleSkillRequested(string skillId)
        {
            var combat2D = GetComponent<Combat.PlayerSkillCombat2D>();
            if (combat2D != null) return;

            if (classSystem != null && classSystem.currentClass != null)
            {
                var skill = classSystem.currentClass.unlockedSkills.Find(s => s != null && s.skillId == skillId);
                if (skill != null)
                {
                    ExecuteSkill(skill);
                }
            }
        }

        private void CastSkillSlot(int slotIndex)
        {
            var combat2D = GetComponent<Combat.PlayerSkillCombat2D>();
            if (combat2D != null)
            {
                combat2D.CastSkillBySlot(slotIndex);
                return;
            }

            if (classSystem != null && classSystem.currentClass != null)
            {
                var skills = classSystem.currentClass.unlockedSkills;
                if (skills != null && slotIndex >= 0 && slotIndex < skills.Count && skills[slotIndex] != null)
                {
                    ExecuteSkill(skills[slotIndex]);
                    return;
                }
            }

            // Fallback: Nếu chưa có kỹ năng gán vào slot, tung đòn đánh cơ bản hoặc cơ chế class
            Debug.Log($"[Skill] Slot {slotIndex + 1} chưa có kỹ năng hoặc Class chưa mở khóa!");
        }

        private void ExecuteSkill(Classes.SkillData skill)
        {
            if (skill == null) return;

            // Kiểm tra Cooldown
            if (skillCooldownTimers.TryGetValue(skill.skillId, out float cd) && cd > 0f)
            {
                Debug.LogWarning($"[Skill] {skill.skillName} đang hồi chiêu ({cd:F1}s còn lại)!");
                return;
            }

            // Kiểm tra và tiêu hao Mana
            if (!stats.ConsumeMp(skill.manaCost))
            {
                Debug.LogWarning($"[Skill] Không đủ Mana để thi triển {skill.skillName}! Cần {skill.manaCost} MP.");
                return;
            }

            // Đặt Cooldown
            skillCooldownTimers[skill.skillId] = skill.cooldownSeconds;
            Core.EventBus.OnSkillCooldown?.Invoke(skill.skillId, skill.cooldownSeconds);

            float damage = stats.TotalAtk * skill.damageMultiplier;
            bool isCrit = Random.value <= stats.TotalCritRate;
            if (isCrit) damage *= stats.TotalCritDamage;

            Combat.DamageInfo dmg = new Combat.DamageInfo(damage, skill.element != Combat.ElementType.None ? skill.element : stats.characterElement, isCrit, gameObject);
            dmg.knockbackForce = skill.knockback;

            Debug.Log($"<color=cyan>[Skill] Thi triển thành công {skill.skillName}! Sát thương: {damage:F0} (Crit: {isCrit}) - Tiêu hao {skill.manaCost} MP.</color>");

            // Xử lý cơ chế theo loại chiêu thức
            switch (skill.targetType)
            {
                case Classes.SkillTargetType.Projectile:
                    ShootProjectile(dmg, 2);
                    break;

                case Classes.SkillTargetType.AreaOfEffect:
                    Collider2D[] aoeTargets = Physics2D.OverlapCircleAll(transform.position, skill.radius > 0 ? skill.radius : 3.0f, enemyLayers);
                    foreach (var col in aoeTargets)
                    {
                        var hurt = col.GetComponent<Combat.Hurtbox2D>();
                        if (hurt != null) hurt.ReceiveDamage(dmg);
                    }
                    break;

                case Classes.SkillTargetType.SelfBuff:
                    stats.Heal(skill.manaCost * 1.5f);
                    break;

                default: // DirectMelee
                    Collider2D[] meleeTargets = Physics2D.OverlapCircleAll(attackPoint.position, skill.range > 0 ? skill.range : attackRadius, enemyLayers);
                    foreach (var col in meleeTargets)
                    {
                        var hurt = col.GetComponent<Combat.Hurtbox2D>();
                        if (hurt != null) hurt.ReceiveDamage(dmg);
                    }
                    break;
            }
        }

        private void PerformAttack(float chargeTime)
        {
            attackCooldownTimer = 0.45f;
            float damageMultiplier = 1f + Mathf.Min(chargeTime * 0.8f, 1.5f);
            float finalDamage = stats.TotalAtk * damageMultiplier;
            bool isCrit = Random.value <= stats.TotalCritRate;
            if (isCrit) finalDamage *= 1.8f;

            DamageInfo dmg = new DamageInfo(finalDamage, stats.characterElement, isCrit, gameObject);
            dmg.knockbackForce = 4.5f;

            // Kiểm tra nhánh class để tung đòn phù hợp
            if (classSystem != null && classSystem.currentClass != null && classSystem.currentClass.branch == Classes.ClassBranch.Archer)
            {
                // Bắn tên / đạn tầm xa
                ShootProjectile(dmg, chargeTime > 1.0f ? 3 : 1);
            }
            else
            {
                // Cận chiến chém quét: Vung kiếm vệt sáng + nhích nhẹ người
                SpawnMeleeSlashVFX(lastFacingDirection, isCrit);
                StartCoroutine(AttackLungeCoroutine(lastFacingDirection));

                Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRadius, enemyLayers);
                foreach (var col in hitEnemies)
                {
                    Hurtbox2D hurt = col.GetComponent<Hurtbox2D>();
                    if (hurt != null)
                    {
                        lastTarget = col.transform;
                        hurt.ReceiveDamage(dmg);
                        Debug.Log($"[Combat] Đánh trúng {col.name} gây {finalDamage:F0} sát thương (Crit: {isCrit})!");
                    }
                }
            }
        }

        private void SpawnMeleeSlashVFX(Vector2 dir, bool isCrit)
        {
            if (dir.sqrMagnitude < 0.01f) dir = Vector2.right;

            GameObject slashObj = new GameObject("PlayerSwordSlashVFX");
            Vector3 spawnPos = (attackPoint != null) ? attackPoint.position : (transform.position + (Vector3)dir * 0.7f);
            slashObj.transform.position = spawnPos;

            SpriteRenderer sr = slashObj.AddComponent<SpriteRenderer>();

            Sprite slashSprite = null;
#if UNITY_EDITOR
            slashSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/vfx_sword_slash.png");
            if (slashSprite == null)
            {
                slashSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/vfx_slash.png");
            }
#endif
            if (slashSprite != null) sr.sprite = slashSprite;

            sr.sortingOrder = 8;
            Color slashColor = isCrit ? new Color(1f, 0.85f, 0.2f, 1f) : new Color(0.85f, 0.95f, 1f, 0.95f);
            sr.color = slashColor;

            float baseAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            float scaleMult = isCrit ? 1.6f : 1.25f;

            StartCoroutine(AnimateSlashCoroutine(slashObj, sr, baseAngle, scaleMult));
        }

        private System.Collections.IEnumerator AnimateSlashCoroutine(GameObject slashObj, SpriteRenderer sr, float baseAngle, float baseScale)
        {
            float duration = 0.13f;
            float elapsed = 0f;
            float startAngle = baseAngle - 35f;
            float endAngle = baseAngle + 35f;

            while (elapsed < duration)
            {
                if (slashObj == null) yield break;

                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);

                // Quét vòng cung nhát kiếm
                float curAngle = Mathf.Lerp(startAngle, endAngle, t);
                slashObj.transform.rotation = Quaternion.Euler(0, 0, curAngle);

                // Mở rộng quy mô theo vệt chém
                float curScale = Mathf.Lerp(baseScale * 0.85f, baseScale * 1.2f, t);
                slashObj.transform.localScale = new Vector3(curScale, curScale, 1f);

                if (sr != null)
                {
                    Color c = sr.color;
                    c.a = Mathf.Lerp(1f, 0f, t * t);
                    sr.color = c;
                }

                yield return null;
            }

            if (slashObj != null) Destroy(slashObj);
        }

        private System.Collections.IEnumerator AttackLungeCoroutine(Vector2 dir)
        {
            if (rb != null && !isDashing)
            {
                rb.linearVelocity = dir.normalized * 2.2f;
                yield return new WaitForSeconds(0.08f);
                if (!isDashing && moveInput.sqrMagnitude < 0.01f)
                {
                    rb.linearVelocity = Vector2.zero;
                }
            }
        }

        private void ShootProjectile(DamageInfo dmg, int pierces)
        {
            GameObject projObj = new GameObject("PlayerArrow");
            projObj.transform.position = attackPoint.position;
            projObj.layer = LayerMask.NameToLayer("Default");

            SpriteRenderer sr = projObj.AddComponent<SpriteRenderer>();
            sr.color = Color.yellow;
            sr.sortingOrder = 6;
            sr.drawMode = SpriteDrawMode.Simple;

            CircleCollider2D col = projObj.AddComponent<CircleCollider2D>();
            col.radius = 0.2f;
            col.isTrigger = true;

            Rigidbody2D projRb = projObj.AddComponent<Rigidbody2D>();
            projRb.gravityScale = 0f;

            Projectile p = projObj.AddComponent<Projectile>();
            p.hitLayers = enemyLayers;
            p.speed = 12f;
            p.Setup(lastFacingDirection, dmg, pierces);
        }

        private bool HasCapturableEnemyNearby()
        {
            if (!PetCaptureSystem.IsPetFeatureUnlocked) return false;
            int enemyLayer = LayerMask.NameToLayer("Enemy");
            int defaultLayer = LayerMask.NameToLayer("Default");
            LayerMask searchMask = (enemyLayers.value != 0) ? enemyLayers : ((1 << (enemyLayer != -1 ? enemyLayer : 0)) | (1 << (defaultLayer != -1 ? defaultLayer : 0)));

            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 3.0f, searchMask);
            foreach (var hit in hits)
            {
                if (hit == null || hit.gameObject == gameObject) continue;
                var eStats = hit.GetComponent<Enemies.EnemyStats>() ?? hit.GetComponentInParent<Enemies.EnemyStats>();
                if (eStats != null && eStats.currentHp > 0 && (eStats.currentHp / Mathf.Max(1f, eStats.maxHp)) <= 0.35f)
                {
                    return true;
                }
            }
            return false;
        }

        private void TryCaptureNearbyEnemy()
        {
            if (!PetCaptureSystem.IsPetFeatureUnlocked)
            {
                int reqLevel = stats != null ? stats.petUnlockLevel : 10;
                string msg = $"Cần đạt Cấp {reqLevel} để mở khóa tính năng Thu Phục Thú Cưng!";
                Debug.LogWarning($"<color=orange>[Thu Phục] {msg}</color>");
                EventBus.OnShowUIMessage?.Invoke(msg);
                return;
            }

            // Quét mục tiêu trong bán kính 3.0m (bao gồm cả Động vật và Quái vật)
            int enemyLayer = LayerMask.NameToLayer("Enemy");
            int defaultLayer = LayerMask.NameToLayer("Default");
            LayerMask searchMask = (enemyLayers.value != 0) ? enemyLayers : ((1 << (enemyLayer != -1 ? enemyLayer : 0)) | (1 << (defaultLayer != -1 ? defaultLayer : 0)));

            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 3.0f, searchMask);
            foreach (var hit in hits)
            {
                if (hit.gameObject == gameObject) continue;

                var eStats = hit.GetComponent<Enemies.EnemyStats>() ?? hit.GetComponentInParent<Enemies.EnemyStats>();
                var eCtrl = hit.GetComponent<Enemies.EnemyController>() ?? hit.GetComponentInParent<Enemies.EnemyController>();

                if (eStats != null || eCtrl != null)
                {
                    if (PetCaptureSystem.Instance != null)
                    {
                        ItemData scroll = PetCaptureSystem.Instance.GetCaptureScrollReference();
                        if (eStats != null)
                        {
                            PetCaptureSystem.Instance.TryCaptureEnemy(eStats, scroll);
                        }
                        else if (eCtrl != null)
                        {
                            PetCaptureSystem.Instance.TryCaptureEnemy(eCtrl, scroll);
                        }
                    }
                    return;
                }
            }
            Debug.Log("[Thu Phục] Không có động vật hoặc quái vật nào trong tầm bắt!");
            EventBus.OnShowUIMessage?.Invoke("Không có sinh vật nào đủ yếu (<30% HP) gần đây để thu phục!");
        }

        private bool TryInteractWithNPC()
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 2.0f, LayerMask.GetMask("NPC"));
            foreach (var hit in hits)
            {
                Town.ShopNPC npc = hit.GetComponent<Town.ShopNPC>();
                if (npc != null)
                {
                    npc.Interact();
                    return true;
                }
            }
            return false;
        }
    }
}
