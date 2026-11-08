using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ActionRPG.Characters;
using ActionRPG.Classes;
using ActionRPG.Core;
using ActionRPG.Enemies;
using ActionRPG.Items;
using ActionRPG.Player;
using ActionRPG.Visuals;

namespace ActionRPG.Combat
{
    /// <summary>
    /// Hệ thống Vung Chiêu Thức 2D 360 độ (2D 360 Combat & Skill System):
    /// - Đánh thường: Chuột Trái hoặc Phím [J].
    /// - Kỹ năng 1, 2, 3: Phím [K], [L], [O] hoặc [Q], [E], [R] hoặc [1], [2], [3].
    /// - Phân tích tên chiêu thức (String Parsing) từ ScriptableObject để quyết định loại hoạt ảnh & VFX:
    ///   * Cận chiến (Chém, Trảm, Slash, Cleave, Strike): Vệt chém hình cung 2D quét 360° theo hướng chuột, gây sát thương vùng.
    ///   * Xạ thủ (Bắn, Tiễn, Shot, Arrow, Snipe): Phóng mũi tên 360° theo vector hướng chuột.
    ///   * Pháp sư (Cầu, Lôi, Trận, Magic, Nova, Arcane): Phóng cầu ma pháp 360° hoặc tạo vòng tròn phép AOE tại vị trí chuột dưới đất.
    /// </summary>
    [DisallowMultipleComponent]
    public class PlayerSkillCombat2D : MonoBehaviour
    {
        [Header("Tham Chiếu Cốt Lõi")]
        [SerializeField] private PlayerAimController2D aimController;
        [SerializeField] private Animator animator;
        [SerializeField] private CharacterStats stats;
        [SerializeField] private ClassSystem classSystem;
        [SerializeField] private PlayerVisualSwitcher visualSwitcher;
        [SerializeField] private SpriteRenderer spriteRenderer;

        [Header("Mục Tiêu & Tầng Va Chạm")]
        [SerializeField] private LayerMask enemyLayers;

        [Header("Prefabs Hiệu Ứng VFX (2D Pixel Art)")]
        [SerializeField] private GameObject slashVfxPrefab;
        [SerializeField] private GameObject arrowPrefab;
        [SerializeField] private GameObject magicOrbPrefab;
        [SerializeField] private GameObject magicAoePrefab;
        [SerializeField] private GameObject hitSparkPrefab;
        [SerializeField] private GameObject fireOrbPrefab;
        [SerializeField] private GameObject fireOrbExplosionPrefab;
        [SerializeField] private ActionRPG.Visuals.LuminaFourDirectionCharacterAnimator fourDirAnimator;
        [SerializeField] private ActionRPG.Presentation.Views.WeaponSocketView weaponSocketView;

        [Header("Thời Gian Hồi Chiêu")]
        [SerializeField] private float basicAttackCooldown = 0.35f;
        private float basicAttackTimer = 0f;
        private Dictionary<string, float> skillCooldownTimers = new Dictionary<string, float>();
        private Dictionary<string, float> maxSkillCooldowns = new Dictionary<string, float>();

        // Cache danh sách kỹ năng mặc định theo từng Class nếu ScriptableObject chưa cấu hình
        private Dictionary<ClassBranch, List<SkillData>> fallbackSkills = new Dictionary<ClassBranch, List<SkillData>>();

        // Danh sách 6 kỹ năng đã nạp theo class hiện tại (slot 1, 2, 3, 4, 5, 6)
        private readonly List<SkillData> loadedSkills = new List<SkillData>(6);
        private ClassBranch loadedBranch = ClassBranch.Novice;
        private bool hasLoadedSkills = false;

        private void Awake()
        {
            if (aimController == null) aimController = GetComponent<PlayerAimController2D>();
            if (aimController == null) aimController = gameObject.AddComponent<PlayerAimController2D>();

            if (animator == null) animator = GetComponent<Animator>();
            if (fourDirAnimator == null) fourDirAnimator = GetComponent<ActionRPG.Visuals.LuminaFourDirectionCharacterAnimator>();
            if (stats == null) stats = GetComponent<CharacterStats>();
            if (classSystem == null) classSystem = GetComponent<ClassSystem>();
            if (visualSwitcher == null) visualSwitcher = GetComponent<PlayerVisualSwitcher>();
            if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
            EnsureWeaponSocketView();

            // Cấu hình mặc định cho enemyLayers (Layer 9: Enemy và Layer 0: Default) nếu chưa gán
            if (enemyLayers.value == 0)
            {
                int enemyLayer = LayerMask.NameToLayer("Enemy");
                int defaultLayer = LayerMask.NameToLayer("Default");
                if (enemyLayer != -1)
                {
                    enemyLayers = (1 << enemyLayer) | (1 << (defaultLayer != -1 ? defaultLayer : 0));
                }
                else
                {
                    enemyLayers = ~0; // Tạm thời nhận diện tất cả nếu chưa set layer
                }
            }

            LoadOrCreateVfxPrefabs();
            BuildFallbackClassSkills();
        }

        private ActionRPG.Presentation.Views.WeaponSocketView EnsureWeaponSocketView()
        {
            if (weaponSocketView == null)
                weaponSocketView = GetComponentInChildren<ActionRPG.Presentation.Views.WeaponSocketView>();
            if (weaponSocketView == null)
                weaponSocketView = GetComponent<ActionRPG.Presentation.Views.WeaponSocketView>();
            if (weaponSocketView == null)
                weaponSocketView = gameObject.AddComponent<ActionRPG.Presentation.Views.WeaponSocketView>();
            return weaponSocketView;
        }

        private int GetVfxSortingOrder(int offset = 15)
        {
            if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
            int baseOrder = spriteRenderer != null ? spriteRenderer.sortingOrder : 5010;
            return Mathf.Max(baseOrder, 5010) + offset;
        }

        private int GetVfxSortingLayerID()
        {
            if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
            return spriteRenderer != null ? spriteRenderer.sortingLayerID : 0;
        }

        private void BoostVfxSortingOrder(GameObject vfxObj, int offset = 15)
        {
            if (vfxObj == null) return;
            int order = GetVfxSortingOrder(offset);
            int layerId = GetVfxSortingLayerID();
            SpriteRenderer[] renderers = vfxObj.GetComponentsInChildren<SpriteRenderer>(true);
            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] != null)
                {
                    renderers[i].sortingLayerID = layerId;
                    renderers[i].sortingOrder = order + i;
                }
            }
        }

        private void Start()
        {
            var activeCs = ClassSystem.Instance != null ? ClassSystem.Instance : classSystem;
            if (activeCs != null && activeCs.currentClass != null)
            {
                LoadClassSkills(activeCs.currentClass);
            }
            else
            {
                ClassBranch branch = GetCurrentBranch();
                fallbackSkills.TryGetValue(branch, out var list);
                ApplySkills(list, branch);
            }
        }

        private void Update()
        {
            if (basicAttackTimer > 0f) basicAttackTimer -= Time.deltaTime;

            // Đếm ngược Cooldown Kỹ Năng
            if (skillCooldownTimers.Count > 0)
            {
                var keys = new List<string>(skillCooldownTimers.Keys);
                foreach (var k in keys)
                {
                    if (skillCooldownTimers[k] > 0f)
                    {
                        skillCooldownTimers[k] = Mathf.Max(0f, skillCooldownTimers[k] - Time.deltaTime);
                        EventBus.OnSkillCooldown?.Invoke(k, skillCooldownTimers[k]);
                    }
                }
            }

            HandleCombatInputs();
        }

        private void HandleCombatInputs()
        {
            bool pointerOverUI = UnityEngine.EventSystems.EventSystem.current != null
                                 && UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();

            // 1. ĐÁNH THƯỜNG: Chuột Trái (khi không click vào nút UI) hoặc Phím J
            if (((Input.GetMouseButtonDown(0) && !pointerOverUI) || Input.GetKeyDown(KeyCode.J)) && basicAttackTimer <= 0f)
            {
                PerformBasicAttack();
            }

            // 2. KỸ NĂNG 1 ĐẾN 6 (PHÍM SỐ 1..6 / NUMPAD 1..6): Bắt sự kiện bàn phím trực tiếp
            if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1)) TryCastSkill(0);
            if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2)) TryCastSkill(1);
            if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3)) TryCastSkill(2);
            if (Input.GetKeyDown(KeyCode.Alpha4) || Input.GetKeyDown(KeyCode.Keypad4)) TryCastSkill(3);
            if (Input.GetKeyDown(KeyCode.Alpha5) || Input.GetKeyDown(KeyCode.Keypad5)) TryCastSkill(4);
            if (Input.GetKeyDown(KeyCode.Alpha6) || Input.GetKeyDown(KeyCode.Keypad6)) TryCastSkill(5);
        }

        #region Xử Lý Đánh Thường (Basic Attack) Cho 4 Phái (4 Hướng & 360°)
        private Vector2 GetCharacterFacingVector()
        {
            if (fourDirAnimator != null)
            {
                switch (fourDirAnimator.currentDirection)
                {
                    case CharacterDir4.Up: return Vector2.up;
                    case CharacterDir4.Down: return Vector2.down;
                    case CharacterDir4.Left: return Vector2.left;
                    case CharacterDir4.Right: return Vector2.right;
                }
            }
            if (aimController != null && aimController.AimDirection.sqrMagnitude > 0.001f)
                return aimController.AimDirection.normalized;
            return Vector2.right;
        }

        private Vector3 GetCurrentMouseWorld()
        {
            // Nếu người chơi bấm nút Kỹ năng bằng chuột trên thanh UI dưới đáy màn hình,
            // ưu tiên nhắm vào quái gần nhất hoặc hướng mặt hiện tại thay vì nhắm xuống đáy màn hình
            bool pointerOverUI = UnityEngine.EventSystems.EventSystem.current != null
                                 && UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();
            if (pointerOverUI)
            {
                Collider2D nearEnemy = FindClosestEnemyTarget(transform.position, 7.5f);
                if (nearEnemy != null) return nearEnemy.transform.position;
                return transform.position + (Vector3)(GetCharacterFacingVector() * 3.5f);
            }

            if (Camera.main != null)
            {
                Vector3 mw = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                mw.z = 0f;
                return mw;
            }
            if (aimController != null)
            {
                Vector3 mw = aimController.MouseWorldPosition;
                mw.z = 0f;
                return mw;
            }
            return transform.position + (Vector3)(GetCharacterFacingVector() * 3f);
        }

        private Vector2 GetCurrentAimDir(Vector3 mouseWorld)
        {
            Vector2 diff = (Vector2)(mouseWorld - transform.position);
            if (diff.sqrMagnitude > 0.001f)
                return diff.normalized;
            return GetCharacterFacingVector();
        }

        private void PerformBasicAttack()
        {
            basicAttackTimer = basicAttackCooldown;

            if (animator != null) animator.SetTrigger("Attack");
            if (visualSwitcher != null) visualSwitcher.TriggerAttack();

            ClassBranch branch = GetCurrentBranch();
            Vector3 mouseWorld = GetCurrentMouseWorld();
            Vector2 aimDir = GetCurrentAimDir(mouseWorld);
            Vector3 firePos = transform.position + (Vector3)(aimDir * 0.55f);

            float baseDamage = (stats != null) 
                ? (branch == ClassBranch.Mage ? stats.TotalMagicAtk : stats.TotalPhysicalAtk) 
                : 15f;
            bool isCrit = (stats != null) && (Random.value <= stats.TotalCritRate);
            if (isCrit && stats != null) baseDamage *= stats.TotalCritDamage;

            if (aimController != null) aimController.NotifyCombatAction(aimDir);
            if (fourDirAnimator != null) fourDirAnimator.SetFacingDirection(aimDir);

            var wsv = EnsureWeaponSocketView();

            switch (branch)
            {
                case ClassBranch.Archer:
                    // Xạ Thủ đánh thường: Hiện cung + hiện mũi tên -> kéo cung lùi lại -> bắn mũi tên về phía trước theo hướng con trỏ chuột
                    if (fourDirAnimator != null) fourDirAnimator.TriggerAttack(aimDir, 0.24f, false);
                    if (wsv != null)
                    {
                        wsv.ApplyClassWeapon(ClassBranch.Archer);
                        wsv.PlayBowDrawWithArrowAnimation(aimDir, 0.13f, releasePos =>
                        {
                            SpawnArcherArrowProjectile(releasePos, aimDir, baseDamage, ElementType.Wind, isCrit, 19f, 1, new Color(0.85f, 1f, 0.9f, 1f), 0.85f, DamageType.Physical);
                        }, new Color(0.85f, 1f, 0.9f, 1f));
                    }
                    else
                    {
                        SpawnArcherArrowProjectile(firePos, aimDir, baseDamage, ElementType.Wind, isCrit, 19f, 1, new Color(0.85f, 1f, 0.9f, 1f), 0.85f, DamageType.Physical);
                    }
                    break;

                case ClassBranch.Mage:
                    // Pháp Sư đánh thường: Hiện gậy phép -> trên đầu gậy phép hiện ánh sáng trắng -> ánh sáng trắng lao về phía trước theo hướng con trỏ chuột
                    if (fourDirAnimator != null) fourDirAnimator.TriggerCast(aimDir, 0.26f, false);
                    if (wsv != null)
                    {
                        wsv.ApplyClassWeapon(ClassBranch.Mage);
                        wsv.PlayStaffLightCastAnimation(aimDir, Color.white, 0.12f, 0.10f, tipPos =>
                        {
                            SpawnMageLightBolt(tipPos, aimDir, Color.white, baseDamage, ElementType.None, isCrit, 16f, DamageType.Magical);
                        });
                    }
                    else
                    {
                        SpawnMageLightBolt(firePos, aimDir, Color.white, baseDamage, ElementType.None, isCrit, 16f, DamageType.Magical);
                    }
                    break;

                case ClassBranch.Warrior:
                case ClassBranch.Novice:
                default:
                    // Tân Thủ / Kiếm Sĩ đánh thường: Hiện thanh kiếm vung chém theo hướng con trỏ chuột (đầy đủ 4 hướng)
                    if (fourDirAnimator != null) fourDirAnimator.TriggerAttack(aimDir, 0.22f, false);
                    if (wsv != null)
                    {
                        wsv.ApplyClassWeapon(branch == ClassBranch.Warrior ? ClassBranch.Warrior : ClassBranch.Novice);
                        wsv.PlayMeleeSlashAnimation(aimDir, 0.22f, 135f);
                    }
                    PerformMeleeSlash(firePos, aimDir, baseDamage, ElementType.None, isCrit, 2.1f, 4f);
                    break;
            }
        }
        #endregion

        #region Xử Lý Thi Triển Kỹ Năng (Skill Casting)
        public void ExecuteAttack()
        {
            PerformBasicAttack();
        }

        public void CastSkill(SkillData skillData)
        {
            if (skillData == null) return;
            ExecuteCastSkill(skillData);
        }

        public void CastSkillBySlot(int slotIndex)
        {
            TryCastSkill(slotIndex);
        }

        /// <summary>
        /// Kích hoạt tung chiêu theo số thứ tự Slot (0..5 khớp với phím 1..6).
        /// </summary>
        public bool TryCastSkill(int slotIndex)
        {
            var pc = GetComponent<PlayerController>();
            if (pc != null && pc.isSafeZone)
            {
                EventBus.OnShowUIMessage?.Invoke("🛡️ Vùng an toàn: Không thể tung chiêu!");
                return false;
            }
            SkillData skill = GetSkillForSlot(slotIndex);
            if (skill == null)
            {
                Debug.Log($"[Combat2D] Ô kỹ năng {slotIndex + 1} chưa gán kỹ năng.");
                return false;
            }
            return ExecuteCastSkill(skill);
        }

        private bool ExecuteCastSkill(SkillData skill)
        {
            if (skill == null) return false;

            // 1. Kiểm tra hồi chiêu
            if (skillCooldownTimers.TryGetValue(skill.skillId, out float cd) && cd > 0f)
            {
                EventBus.OnShowUIMessage?.Invoke($"⏳ {skill.skillName} đang hồi chiêu ({cd:F1}s)");
                return false;
            }

            // 2. Kiểm tra và tiêu hao năng lượng (MP)
            if (stats != null && !stats.ConsumeMp(skill.manaCost))
            {
                EventBus.OnShowUIMessage?.Invoke($"⚠️ Không đủ Mana ({skill.manaCost} MP)!");
                return false;
            }

            // Bắt đầu tính hồi chiêu
            skillCooldownTimers[skill.skillId] = skill.cooldownSeconds;
            EventBus.OnSkillCooldown?.Invoke(skill.skillId, skill.cooldownSeconds);

            // Kích hoạt trigger Attack của nhân vật
            if (animator != null) animator.SetTrigger("Attack");
            if (visualSwitcher != null) visualSwitcher.TriggerAttack();

            // Tính toán hướng ngắm aimDirection và góc xoay aimRotation từ vị trí nhân vật tới con trỏ chuột (đầy đủ 4 hướng & 360°)
            Vector3 mouseWorld = GetCurrentMouseWorld();
            Vector2 aimDir = GetCurrentAimDir(mouseWorld);

            float aimAngle = Mathf.Atan2(aimDir.y, aimDir.x) * Mathf.Rad2Deg;
            Quaternion aimRotation = Quaternion.Euler(0f, 0f, aimAngle);

            // Không tự động bật vũ khí ở đây; từng kỹ năng sẽ quyết định hiện vũ khí hay ẩn vũ khí (StopAndHideWeapon)
            if (aimController != null) aimController.NotifyCombatAction(aimDir);
            if (fourDirAnimator != null)
            {
                fourDirAnimator.SetFacingDirection(aimDir);
                fourDirAnimator.TriggerCast(aimDir, 0.3f, false);
            }

            EventBus.OnShowUIMessage?.Invoke($"✨ Thi triển: {skill.skillName}!");

            // Tính sát thương kỹ năng dựa trên loại sát thương (Vật Lý hoặc Phép)
            DamageType dmgType = skill.damageType;
            if (GetCurrentBranch() == ClassBranch.Mage && dmgType == DamageType.Physical)
            {
                dmgType = DamageType.Magical;
            }

            float atk = stats != null 
                ? (dmgType == DamageType.Magical ? stats.TotalMagicAtk : stats.TotalPhysicalAtk) 
                : 20f;
            float skillDamage = atk * (skill.damageMultiplier > 0 ? skill.damageMultiplier : 1.5f);
            bool isCrit = stats != null && (Random.value <= stats.TotalCritRate);
            if (isCrit && stats != null) skillDamage *= stats.TotalCritDamage;

            Vector3 firePos = (aimController != null && aimController.FirePoint != null)
                ? aimController.FirePoint.position
                : (transform.position + (Vector3)aimDir * 0.5f);

            // Thực thi hiệu ứng chiêu thức (VFX & Hitbox) theo từng kỹ năng
            ExecuteSkillByKeywords(skill, skillDamage, isCrit, aimDir, firePos, mouseWorld, aimRotation);
            return true;
        }

        private void ExecuteSkillByKeywords(
            SkillData skill, 
            float damage, 
            bool isCrit, 
            Vector2 aimDir, 
            Vector3 firePos, 
            Vector3 mousePos,
            Quaternion aimRotation)
        {
            string nameLower = (skill.skillName + " " + skill.skillId).ToLower();

            // ═════════════════════════════════════════════════════════════════
            // 1. KIẾM SĨ / ĐẤU SĨ (WARRIOR: 6 KỸ NĂNG)
            // ═════════════════════════════════════════════════════════════════
            // 1.1. Trảm Kích (Cleave): Vung kiếm hình bán nguyệt cự ly gần trước mặt, CÓ hiện thanh kiếm chém hình quạt
            if (nameLower.Contains("warrior_cleave") || nameLower.Contains("trảm kích") || nameLower.Contains("cleave"))
            {
                PerformCleaveSlash(skill, firePos, aimDir, aimRotation, damage, isCrit);
                return;
            }
            // 1.2. Xung Phong (Vanguard Charge): Lao vút về hướng chỉ định, ủi trúng mục tiêu đầu tiên gây choáng (KHÔNG hiện thanh kiếm)
            else if (nameLower.Contains("vanguard") || nameLower.Contains("xung phong"))
            {
                StartCoroutine(PerformVanguardChargeRoutine(skill, aimDir, aimRotation, damage, isCrit));
                return;
            }
            // 1.3. Thiết Giáp (Iron Bastion): Dựng lá chắn hộ thể xoay quanh thân mình (KHÔNG hiện thanh kiếm)
            else if (nameLower.Contains("iron_bastion") || nameLower.Contains("thiết giáp") || nameLower.Contains("bastion"))
            {
                PerformIronBastion(skill);
                return;
            }
            // 1.4. Địa Chấn (Tremor Slam): Cắm thanh kiếm xuống đất tạo sóng chấn động hình nón (CÓ hiện thanh kiếm cắm xuống đất)
            else if (nameLower.Contains("tremor") || nameLower.Contains("địa chấn"))
            {
                PerformTremorSlam(skill, firePos, aimDir, aimRotation, damage, isCrit);
                return;
            }
            // 1.5. Cuồng Nộ Kích (Frenzied Strike): Bao quanh cơ thể ánh sáng đỏ, chém tất sát 1 mục tiêu duy nhất rồi trở lại bình thường
            else if (nameLower.Contains("frenzied") || nameLower.Contains("cuồng nộ kích"))
            {
                StartCoroutine(PerformFrenziedStrikeRoutine(skill, firePos, aimDir, aimRotation, damage, isCrit));
                return;
            }
            // 1.6. Khai Sơn Bạt Địa (Tuyệt Kỹ): Bay người lên cao sau đó dậm mạnh xuống mặt đất diện rộng (KHÔNG hiện thanh kiếm)
            else if (nameLower.Contains("melee_ultimate_crush") || nameLower.Contains("khai sơn") || nameLower.Contains("bạt địa"))
            {
                StartCoroutine(PerformUltimateCrushJumpSlamRoutine(skill, mousePos, aimDir, damage, isCrit));
                return;
            }

            // ═════════════════════════════════════════════════════════════════
            // 2. PHÁP SƯ (MAGE: 6 KỸ NĂNG)
            // ═════════════════════════════════════════════════════════════════
            // 2.1. Hỏa Cầu (Fire Orb): Hiện gậy phép -> ánh sáng đỏ trên đầu gậy -> quả cầu lửa bay theo hướng con trỏ chuột
            else if (nameLower.Contains("fire_orb") || nameLower.Contains("hỏa cầu"))
            {
                SpawnFireOrbExplosive(skill, firePos, aimDir, aimRotation, damage, isCrit);
                return;
            }
            // 2.2. Băng Phong Thược (Frost Spikes): Hiện gậy phép -> ánh sáng xanh trên đầu gậy -> triệu hồi chông băng nhô lên tại vị trí chuột
            else if (nameLower.Contains("frost_spikes") || nameLower.Contains("băng phong"))
            {
                StartCoroutine(PerformFrostSpikesRoutine(skill, mousePos, aimRotation, damage, isCrit));
                return;
            }
            // 2.3. Thiểm Dịch (Blink / Arcane Shift): Biến ảnh tức thời về hướng chuột, để lại tàn ảnh tại vị trí cũ (KHÔNG hiện gậy phép)
            else if (nameLower.Contains("blink") || nameLower.Contains("thiểm dịch") || nameLower.Contains("arcane_shift"))
            {
                PerformMageBlink(skill, mousePos, aimRotation);
                return;
            }
            // 2.4. Lôi Điện Tỏa (Chain Lightning): Hiện gậy phép -> ánh sáng vàng trên đầu gậy -> bắn tia sét vàng vào mục tiêu và nảy sang 3 kẻ địch
            else if (nameLower.Contains("chain_lightning") || nameLower.Contains("lôi điện tỏa"))
            {
                PerformChainLightning(skill, mousePos, aimRotation, damage, isCrit);
                return;
            }
            // 2.5. Khiên Hộ Thuật (Mana Shield): Hiện gậy phép -> ánh sáng trắng trên đầu gậy -> tạo 1 lớp khiên trắng bao xung quanh cơ thể
            else if (nameLower.Contains("mana_shield") || nameLower.Contains("nguyên tố hộ thể") || nameLower.Contains("khiên hộ thuật"))
            {
                PerformManaShield(skill);
                return;
            }
            // 2.6. Bí Thuật Pháo / Đại Hồng Thủy (mage_arcane_blast): Hiện gậy phép -> ánh sáng đỏ trên đầu gậy -> phóng tia ánh sáng đỏ thẳng theo hướng chuột
            else if (nameLower.Contains("mage_arcane_blast") || nameLower.Contains("bí thuật pháo") || nameLower.Contains("đại hồng thủy"))
            {
                PerformMageArcaneLaserBeam(skill, aimDir, damage, isCrit);
                return;
            }

            // ═════════════════════════════════════════════════════════════════
            // 3. XẠ THỦ (ARCHER: 6 KỸ NĂNG)
            // ═════════════════════════════════════════════════════════════════
            // 3.1. Xuyên Vân Tiễn (Piercing Shot): Giữ lại như cũ (kéo cung bắn tên xuyên thấu)
            else if (nameLower.Contains("piercing_shot") || nameLower.Contains("xuyên vân"))
            {
                SpawnPiercingShot(skill, firePos, aimDir, aimRotation, damage, isCrit);
                return;
            }
            // 3.2. Mưa Tên (Volley / Barrage): Bắn 1 mũi tên lên cao -> hiện vòng tròn ma pháp trên cao -> mưa tên rơi xuống vùng nón phía trước
            else if (nameLower.Contains("archer_volley") || nameLower.Contains("mưa tên") || (nameLower.Contains("volley") && !nameLower.Contains("radiant")))
            {
                PerformArcherVolley(skill, firePos, aimDir, mousePos, aimRotation, damage, isCrit);
                return;
            }
            // 3.3. Nhào Lộn Thoát Thân (Tumble): Nhào lộn 360° theo hướng con trỏ chuột (KHÔNG hiện vũ khí)
            else if (nameLower.Contains("tumble") || nameLower.Contains("nhào lộn"))
            {
                StartCoroutine(PerformArcherTumbleRoutine(skill, aimDir, aimRotation));
                return;
            }
            // 3.4. Bẫy Dây (Snare Trap): Đặt bẫy tại vị trí chuột (giống bẫy Caitlyn, KHÔNG hiện vũ khí)
            else if (nameLower.Contains("snare_trap") || nameLower.Contains("bẫy dây") || nameLower.Contains("trap"))
            {
                StartCoroutine(PerformSnareTrapRoutine(skill, mousePos, aimRotation, damage, isCrit));
                return;
            }
            // 3.5. Ngắm Chuẩn Dứt Điểm (Deadeye): Hiện tâm ngắm trên mục tiêu gần con trỏ chuột, dồn lực bắn phát tên cực đại
            else if (nameLower.Contains("deadeye") || nameLower.Contains("ngắm chuẩn"))
            {
                StartCoroutine(PerformDeadeyeRoutine(skill, firePos, aimDir, mousePos, aimRotation, damage, isCrit));
                return;
            }
            // 3.6. Bão Tố Tiễn Vũ (arch_elemental_storm): Xả mưa tên năng lượng diện rộng trút xuống vùng chỉ định
            else if (nameLower.Contains("arch_elemental_storm") || nameLower.Contains("bão tên") || nameLower.Contains("bão tố tiễn vũ"))
            {
                StartCoroutine(PerformElementalArrowStormRoutine(skill, mousePos, aimDir, damage, isCrit));
                return;
            }

            // ═════════════════════════════════════════════════════════════════
            // 0. TÂN THỦ (NOVICE: 5 KỸ NĂNG)
            // ═════════════════════════════════════════════════════════════════
            // 0.1. Đoạn Phong Trảm (Fan Slash): Vung kiếm chém hình quạt (CÓ hiện thanh kiếm)
            if (nameLower.Contains("fan") || nameLower.Contains("hình quạt") || nameLower.Contains("đoạn phong"))
            {
                PerformFanSlash(firePos, aimDir, damage, skill.element, isCrit, 2.8f, 60f);
                return;
            }
            // 0.2. Loạn Vũ Liên Trảm (Flurry Slash): Chém liên tục 6 nhịp cuồng bạo (KHÔNG hiện thanh kiếm)
            else if (nameLower.Contains("flurry") || nameLower.Contains("loạn vũ") || nameLower.Contains("liên trảm"))
            {
                StartCoroutine(PerformFlurrySlashRoutine(firePos, aimDir, damage, skill.element, isCrit));
                return;
            }
            // 0.3. Kim Cang Giáp (Aegis Barrier): Kết giới hộ thể ánh vàng xung quanh cơ thể + lớp máu ảo trên thanh máu (KHÔNG hiện thanh kiếm)
            else if (nameLower.Contains("barrier") || nameLower.Contains("kim cang") || nameLower.Contains("máu ảo") || nameLower.Contains("aegis"))
            {
                PerformNoviceAegisBarrier(skill);
                return;
            }
            // 0.4. Tứ Bộ Du Long (Quad Dash): Lướt tới theo hướng con trỏ chuột (KHÔNG hiện thanh kiếm)
            else if (nameLower.Contains("quad") || nameLower.Contains("tứ bộ") || nameLower.Contains("4 đoạn") || nameLower.Contains("dash_quad"))
            {
                StartCoroutine(PerformQuadDashRoutine(aimDir));
                return;
            }
            // 0.5. Ánh Sáng Phục Hồi (Holy Heal): Thánh quang trị liệu xung quanh cơ thể (KHÔNG hiện thanh kiếm)
            else if (nameLower.Contains("novice_holy_heal") || nameLower.Contains("ánh sáng phục hồi") || nameLower.Contains("holy_heal"))
            {
                PerformNoviceHolyHeal(skill);
                return;
            }

            // ═════════════════════════════════════════════════════════════════
            // 4. CÁC NHÓM KỸ NĂNG MỞ RỘNG KHÁC
            // ═════════════════════════════════════════════════════════════════
            if (nameLower.Contains("nova") || nameLower.Contains("trận") || nameLower.Contains("zone") 
                || nameLower.Contains("blast") || nameLower.Contains("bộc phá") || nameLower.Contains("aoe"))
            {
                float radius = skill.radius > 0 ? skill.radius : 2.5f;
                SpawnGroundAoe(mousePos, damage, skill.element, isCrit, radius);
            }
            else if (skill.targetType == SkillTargetType.Projectile || nameLower.Contains("shot") || nameLower.Contains("arrow") || nameLower.Contains("bắn") 
                || nameLower.Contains("tiễn") || nameLower.Contains("snipe") || nameLower.Contains("bolt") || nameLower.Contains("đao"))
            {
                GameObject projPrefab = ResolveProjectilePrefab(skill, arrowPrefab);
                if (nameLower.Contains("triple") || nameLower.Contains("tam") || nameLower.Contains("chùm"))
                {
                    SpawnSpreadProjectiles(projPrefab, firePos, aimDir, damage * 0.7f, skill.element, isCrit, 3, 20f);
                }
                else
                {
                    float projSpeed = skill.range > 0 ? skill.range * 3f : 18f;
                    SpawnProjectile(projPrefab, firePos, aimDir, damage, skill.element, isCrit, projSpeed);
                }
            }
            else if (nameLower.Contains("fire") || nameLower.Contains("ice") || nameLower.Contains("magic") 
                || nameLower.Contains("phép") || nameLower.Contains("cầu") || nameLower.Contains("lôi") || nameLower.Contains("orb"))
            {
                GameObject projPrefab = ResolveProjectilePrefab(skill, magicOrbPrefab);
                SpawnProjectile(projPrefab, firePos, aimDir, damage, skill.element, isCrit, 14f);
            }
            else if (skill.targetType == SkillTargetType.SelfBuff || nameLower.Contains("aid") || nameLower.Contains("heal") || nameLower.Contains("skin") || nameLower.Contains("hồi"))
            {
                PerformNoviceHolyHeal(skill);
            }
            else
            {
                float range = skill.range > 0 ? skill.range : 2.2f;
                float kb = skill.knockback > 0 ? skill.knockback : 5f;
                var wsvFallback = EnsureWeaponSocketView();
                if (wsvFallback != null) wsvFallback.PlayMeleeSlashAnimation(aimDir, 0.25f, 120f);
                PerformMeleeSlash(firePos, aimDir, damage, skill.element, isCrit, range, kb);
            }
        }
        #endregion

        #region Kỹ Năng Tân Thủ (Novice - 5 Chiêu)
        /// <summary>
        /// Kỹ năng 1 Tân Thủ - Đoạn Phong Trảm (Fan Slash): Hiện thanh kiếm vung chém hình quạt rộng về hướng con trỏ chuột.
        /// </summary>
        private void PerformFanSlash(Vector3 center, Vector2 dir, float damage, ElementType element, bool isCrit, float radius, float arcAngle)
        {
            if (fourDirAnimator != null) fourDirAnimator.TriggerAttack(dir, 0.28f, false);
            var wsv = EnsureWeaponSocketView();
            if (wsv != null) wsv.PlayMeleeSlashAnimation(dir, 0.26f, 150f);

            PerformMeleeSlash(center, dir, damage * 1.4f, element, isCrit, radius, 5.5f);
            float baseAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            float leftRad = (baseAngle + 28f) * Mathf.Deg2Rad;
            float rightRad = (baseAngle - 28f) * Mathf.Deg2Rad;
            Vector2 leftDir = new Vector2(Mathf.Cos(leftRad), Mathf.Sin(leftRad));
            Vector2 rightDir = new Vector2(Mathf.Cos(rightRad), Mathf.Sin(rightRad));

            PerformMeleeSlash(center + (Vector3)leftDir * 0.35f, leftDir, damage * 0.6f, element, isCrit, radius * 0.85f, 3f);
            PerformMeleeSlash(center + (Vector3)rightDir * 0.35f, rightDir, damage * 0.6f, element, isCrit, radius * 0.85f, 3f);
            SpawnProceduralFanWave(center, dir, new Color(0.55f, 0.95f, 1f, 0.85f), radius * 1.15f, 0.28f);
        }

        /// <summary>
        /// Kỹ năng 2 Tân Thủ - Loạn Vũ Liên Trảm (Flurry Slash): Chém liên tục 6 nhịp cuồng bạo KHÔNG hiện thanh kiếm.
        /// </summary>
        private IEnumerator PerformFlurrySlashRoutine(Vector3 center, Vector2 dir, float damage, ElementType element, bool isCrit)
        {
            var wsv = EnsureWeaponSocketView();
            if (wsv != null) wsv.StopAndHideWeapon();

            int slashes = 6;
            float interval = 1.65f / slashes;
            float[] angleOffsets = { -18f, 18f, -26f, 24f, -12f, 0f };

            for (int i = 0; i < slashes; i++)
            {
                if (wsv != null) wsv.StopAndHideWeapon();

                Vector3 mw = GetCurrentMouseWorld();
                Vector2 curDir = GetCurrentAimDir(mw);
                if (fourDirAnimator != null) fourDirAnimator.TriggerAttack(curDir, interval * 0.9f, false);
                if (animator != null) animator.SetTrigger("Attack");

                float baseAng = Mathf.Atan2(curDir.y, curDir.x) * Mathf.Rad2Deg + angleOffsets[i % angleOffsets.Length];
                Vector2 slashDir = new Vector2(Mathf.Cos(baseAng * Mathf.Deg2Rad), Mathf.Sin(baseAng * Mathf.Deg2Rad));
                Vector3 slashPos = transform.position + (Vector3)curDir * 0.65f;

                PerformMeleeSlash(slashPos, slashDir, damage * 0.48f, element, isCrit, 2.2f, 2.5f);
                SpawnProceduralSlashWave(slashPos, slashDir, (i % 2 == 0) ? new Color(0.45f, 0.95f, 1f, 0.95f) : new Color(1f, 0.92f, 0.45f, 0.95f), 2.1f, 0.2f);
                yield return new WaitForSeconds(interval);
            }
        }

        /// <summary>
        /// Kỹ năng 3 Tân Thủ - Kim Cang Giáp (Aegis Barrier): Ánh vàng hộ thể xung quanh cơ thể + lớp máu ảo trên thanh máu (KHÔNG hiện thanh kiếm).
        /// </summary>
        private void PerformNoviceAegisBarrier(SkillData skill)
        {
            var wsv = EnsureWeaponSocketView();
            if (wsv != null) wsv.StopAndHideWeapon();

            if (stats != null)
            {
                stats.ApplyBuff(BuffStatType.DefensePower, 15f, 5.0f);
                stats.AddBarrier(50f, 5.0f);
            }
            StartCoroutine(SpawnGoldenBarrierAuraRoutine(5.0f));
        }

        /// <summary>
        /// Kỹ năng 4 Tân Thủ - Tứ Bộ Du Long (Quad Dash): Lướt theo hướng con trỏ chuột cả 4 hướng / 360° (KHÔNG hiện thanh kiếm).
        /// </summary>
        private IEnumerator PerformQuadDashRoutine(Vector2 initialDir)
        {
            var wsv = EnsureWeaponSocketView();
            if (wsv != null) wsv.StopAndHideWeapon();

            Rigidbody2D rb2d = GetComponent<Rigidbody2D>();

            for (int i = 0; i < 4; i++)
            {
                if (wsv != null) wsv.StopAndHideWeapon();

                Vector3 mw = GetCurrentMouseWorld();
                Vector2 dashVec = GetCurrentAimDir(mw);
                if (dashVec.sqrMagnitude < 0.01f) dashVec = (initialDir.sqrMagnitude > 0.01f ? initialDir.normalized : Vector2.right);

                if (fourDirAnimator != null) fourDirAnimator.SetFacingDirection(dashVec);
                SpawnCharacterAfterimage(transform.position, new Color(0.45f, 0.9f, 1f, 0.65f), 0.35f);

                float stepDuration = 0.075f;
                float elapsed = 0f;
                while (elapsed < stepDuration)
                {
                    elapsed += Time.deltaTime;
                    if (rb2d != null)
                        rb2d.linearVelocity = dashVec * 16f;
                    else
                        transform.position += (Vector3)(dashVec * 16f * Time.deltaTime);
                    yield return null;
                }

                if (rb2d != null) rb2d.linearVelocity = Vector2.zero;
                SpawnHitSpark(transform.position, new Color(0.4f, 0.9f, 1f, 0.85f));
                yield return new WaitForSeconds(0.06f);
            }
        }

        /// <summary>
        /// Kỹ năng 5 Tân Thủ - Ánh Sáng Phục Hồi (Holy Heal): Thánh quang trị liệu xung quanh cơ thể (KHÔNG hiện thanh kiếm).
        /// </summary>
        private void PerformNoviceHolyHeal(SkillData skill)
        {
            var wsv = EnsureWeaponSocketView();
            if (wsv != null) wsv.StopAndHideWeapon();

            if (stats != null) stats.Heal(60f);
            StartCoroutine(SpawnHolyHealAuraRoutine(1.6f));
        }
        #endregion

        #region Kỹ Năng 3 Chức Nghiệp Cơ Bản (Warrior, Mage, Archer)
        private static Hurtbox2D FindHurtbox(Collider2D col)
        {
            if (col == null) return null;
            Hurtbox2D h = col.GetComponent<Hurtbox2D>();
            if (h == null) h = col.GetComponentInParent<Hurtbox2D>();
            return h;
        }

        private static EnemyStats FindEnemyStats(Collider2D col)
        {
            if (col == null) return null;
            EnemyStats es = col.GetComponent<EnemyStats>();
            if (es == null) es = col.GetComponentInParent<EnemyStats>();
            return es;
        }

        private static EnemyController FindEnemyController(Collider2D col)
        {
            if (col == null) return null;
            EnemyController ec = col.GetComponent<EnemyController>();
            if (ec == null) ec = col.GetComponentInParent<EnemyController>();
            return ec;
        }

        private static SpriteRenderer FindSpriteRenderer(GameObject go)
        {
            if (go == null) return null;
            SpriteRenderer sr = go.GetComponent<SpriteRenderer>();
            if (sr == null) sr = go.GetComponentInChildren<SpriteRenderer>();
            return sr;
        }

        private static ProjectileBehavior GetOrAddProjectileBehavior(GameObject go)
        {
            if (go == null) return null;
            ProjectileBehavior pb = go.GetComponent<ProjectileBehavior>();
            if (pb == null) pb = go.AddComponent<ProjectileBehavior>();
            return pb;
        }

        private void SetupVfxAnimator(GameObject vfxObj, float defaultLifetime = 1.5f)
        {
            if (vfxObj == null) return;
            BoostVfxSortingOrder(vfxObj, 16);
            SimpleFrameAnimator2D anim = vfxObj.GetComponent<SimpleFrameAnimator2D>();
            if (anim == null) anim = vfxObj.GetComponentInChildren<SimpleFrameAnimator2D>();
            if (anim != null)
            {
                anim.loop = false;
                anim.destroyOnFinish = true;
            }
            else
            {
                Destroy(vfxObj, defaultLifetime);
            }
        }

        private GameObject ResolveHitVfx(SkillData skill, GameObject fallback = null)
        {
            if (skill != null && skill.hitVfxPrefab != null) return skill.hitVfxPrefab;
            if (fallback != null) return fallback;

#if UNITY_EDITOR
            if (skill != null && !string.IsNullOrEmpty(skill.skillId))
            {
                string cap = ClassEvolutionManager.GetCapitalizedSkillName(skill.skillId);
                var loaded = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>($"Assets/Prefabs/VFX/VFX_{cap}.prefab");
                if (loaded != null) return loaded;
            }
#endif
            return slashVfxPrefab;
        }

        private GameObject ResolveProjectilePrefab(SkillData skill, GameObject fallback = null)
        {
            if (skill != null && skill.projectilePrefab != null) return skill.projectilePrefab;
            if (fallback != null) return fallback;

#if UNITY_EDITOR
            if (skill != null && !string.IsNullOrEmpty(skill.skillId))
            {
                if (skill.skillId == "mage_fire_orb")
                {
                    var fo = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/VFX/FireOrb_Projectile.prefab");
                    if (fo != null) return fo;
                }
                else if (skill.skillId == "archer_volley")
                {
                    var av = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/VFX/VFX_Archer_Volley_Arrow.prefab");
                    if (av != null) return av;
                }
                else
                {
                    string cap = ClassEvolutionManager.GetCapitalizedSkillName(skill.skillId);
                    var loaded = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>($"Assets/Prefabs/VFX/VFX_{cap}.prefab");
                    if (loaded != null) return loaded;
                }
            }
#endif
            return arrowPrefab;
        }

        // ═══════════════════════════════════════════════════════════════════════════
        // KIẾM SĨ / ĐẤU SĨ (WARRIOR - 6 KỸ NĂNG)
        // ═══════════════════════════════════════════════════════════════════════════

        /// <summary>
        /// 1. Trảm Kích (Cleave): Hiện thanh kiếm vung chém hình bán nguyệt / hình quạt cự ly gần trước mặt.
        /// </summary>
        private void PerformCleaveSlash(SkillData skill, Vector3 center, Vector2 dir, Quaternion aimRotation, float damage, bool isCrit)
        {
            if (fourDirAnimator != null) fourDirAnimator.TriggerAttack(dir, 0.28f, false);
            var wsv = EnsureWeaponSocketView();
            if (wsv != null)
            {
                wsv.ApplyClassWeapon(ClassBranch.Warrior);
                wsv.PlayMeleeSlashAnimation(dir, 0.26f, 180f);
            }

            GameObject vfxPrefab = ResolveHitVfx(skill, slashVfxPrefab);
            if (vfxPrefab != null)
            {
                GameObject slash = Instantiate(vfxPrefab, center + (Vector3)dir * 0.75f, aimRotation);
                slash.transform.localScale = new Vector3(1.45f, 1.45f, 1f);
                SetupVfxAnimator(slash, 0.38f);
            }
            SpawnProceduralFanWave(center, dir, new Color(1f, 0.88f, 0.35f, 0.9f), 2.8f, 0.26f);

            Collider2D[] hitColliders = Physics2D.OverlapCircleAll(center, 2.8f, enemyLayers);
            var hitEntities = new HashSet<GameObject>();
            foreach (var col in hitColliders)
            {
                if (col == null || col.gameObject == gameObject) continue;
                Hurtbox2D hurt = FindHurtbox(col);
                EnemyStats enemy = FindEnemyStats(col);
                if (hurt == null && enemy == null) continue;
                GameObject targetRoot = hurt != null ? hurt.transform.root.gameObject : enemy.transform.root.gameObject;
                if (!hitEntities.Add(targetRoot)) continue;

                Vector2 toTarget = ((Vector2)col.transform.position - (Vector2)center);
                if (Vector2.Dot(dir.normalized, toTarget.normalized) >= -0.2f)
                {
                    ApplyDirectHit(col, damage, ElementType.None, isCrit, dir, 6f);
                    SpawnHitSpark(col.transform.position, Color.yellow);
                }
            }
        }

        /// <summary>
        /// 2. Xung Phong (Vanguard Charge): Lao vút về hướng chỉ định, ủi trúng mục tiêu đầu tiên gây choáng (KHÔNG hiện thanh kiếm).
        /// </summary>
        private IEnumerator PerformVanguardChargeRoutine(SkillData skill, Vector2 dir, Quaternion aimRotation, float damage, bool isCrit)
        {
            var wsv = EnsureWeaponSocketView();
            if (wsv != null) wsv.StopAndHideWeapon();
            if (fourDirAnimator != null) fourDirAnimator.TriggerAttack(dir, 0.28f, false);

            Rigidbody2D rb2d = GetComponent<Rigidbody2D>();
            Vector2 chargeVec = (dir.sqrMagnitude > 0.01f ? dir : Vector2.right).normalized;
            float duration = 0.24f;
            float elapsed = 0f;
            bool hitEnemy = false;
            float afterimageTimer = 0f;

            // Tạo vòm xung kích cản gió phía trước người khi lao tới
            GameObject chargeAura = CreateProceduralRingObject("VanguardCharge_Wave", transform.position + (Vector3)chargeVec * 0.5f, new Color(1f, 0.78f, 0.25f, 0.85f), 1.3f, 20);
            chargeAura.transform.SetParent(transform, true);
            chargeAura.transform.rotation = aimRotation;

            while (elapsed < duration)
            {
                if (wsv != null) wsv.StopAndHideWeapon();

                float dt = Time.deltaTime;
                elapsed += dt;
                afterimageTimer += dt;

                if (afterimageTimer >= 0.045f)
                {
                    afterimageTimer = 0f;
                    SpawnCharacterAfterimage(transform.position, new Color(1f, 0.75f, 0.3f, 0.6f), 0.25f);
                }

                if (rb2d != null)
                    rb2d.linearVelocity = chargeVec * 22f;
                else
                    transform.position += (Vector3)(chargeVec * 22f * dt);

                Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 1.45f, enemyLayers);
                foreach (var col in hits)
                {
                    if (col == null || col.gameObject == gameObject) continue;
                    Hurtbox2D hurt = FindHurtbox(col);
                    EnemyStats enemy = FindEnemyStats(col);
                    if (hurt == null && enemy == null) continue;

                    EnemyController enemyCtrl = FindEnemyController(col);
                    if (enemyCtrl != null) enemyCtrl.ApplyStun(0.75f);

                    DamageInfo info = new DamageInfo(damage, ElementType.None, isCrit, gameObject)
                    {
                        knockbackDirection = chargeVec,
                        knockbackForce = 7.5f,
                        statusEffect = StatusEffectType.Stun,
                        statusDuration = 0.75f
                    };
                    ApplyDirectHit(col, info);
                    SpawnHitSpark(col.transform.position, new Color(1f, 0.9f, 0.2f, 1f));
                    StartCoroutine(ExpandShockwaveRingRoutine(col.transform.position, new Color(1f, 0.85f, 0.25f, 0.95f), 0.3f, 2.0f, 0.25f));
                    hitEnemy = true;
                    break;
                }

                if (hitEnemy) break;
                yield return null;
            }

            if (chargeAura != null) Destroy(chargeAura);
            if (rb2d != null) rb2d.linearVelocity = Vector2.zero;
        }

        /// <summary>
        /// 3. Thiết Giáp (Iron Bastion): Dựng lá chắn hộ thể xoay quanh thân mình (KHÔNG hiện thanh kiếm).
        /// </summary>
        private void PerformIronBastion(SkillData skill)
        {
            var wsv = EnsureWeaponSocketView();
            if (wsv != null) wsv.StopAndHideWeapon();

            if (stats != null)
            {
                stats.AddBarrier(60f, 3.0f);
                stats.ApplyBuff(BuffStatType.DefensePower, 35f, 3.0f);
            }
            StartCoroutine(SpawnOrbitingShieldsRoutine(3.0f, 3));
            EventBus.OnShowUIMessage?.Invoke("🛡️ Thiết Giáp: Dựng lá chắn hộ thể xoay quanh thân & +60 Máu Ảo (3s)!");
        }

        /// <summary>
        /// 4. Địa Chấn (Tremor Slam): Cắm thanh kiếm xuống mặt đất tạo sóng chấn động hình nón (CÓ hiện thanh kiếm cắm xuống đất).
        /// </summary>
        private void PerformTremorSlam(SkillData skill, Vector3 center, Vector2 dir, Quaternion aimRotation, float damage, bool isCrit)
        {
            if (fourDirAnimator != null) fourDirAnimator.TriggerAttack(dir, 0.38f, false);

            var wsv = EnsureWeaponSocketView();
            if (wsv != null)
            {
                wsv.ApplyClassWeapon(ClassBranch.Warrior);
                wsv.PlaySwordPlungeAnimation(dir, 0.36f, impactPos =>
                {
                    ExecuteTremorGroundShockwave(skill, impactPos, dir, aimRotation, damage, isCrit);
                });
            }
            else
            {
                ExecuteTremorGroundShockwave(skill, center, dir, aimRotation, damage, isCrit);
            }
        }

        private void ExecuteTremorGroundShockwave(SkillData skill, Vector3 impactPos, Vector2 dir, Quaternion aimRotation, float damage, bool isCrit)
        {
            StartCoroutine(SpawnTremorConeWaveRoutine(transform.position, dir, 4.5f));

            GameObject vfxPrefab = ResolveHitVfx(skill);
            if (vfxPrefab != null)
            {
                GameObject vfx = Instantiate(vfxPrefab, transform.position + (Vector3)dir * 1.6f, aimRotation);
                SetupVfxAnimator(vfx, 0.6f);
            }

            Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, 4.5f, enemyLayers);
            var hitEntities = new HashSet<GameObject>();
            foreach (var col in hitColliders)
            {
                if (col == null || col.gameObject == gameObject) continue;
                Hurtbox2D hurt = FindHurtbox(col);
                EnemyStats enemy = FindEnemyStats(col);
                if (hurt == null && enemy == null) continue;
                GameObject targetRoot = hurt != null ? hurt.transform.root.gameObject : enemy.transform.root.gameObject;
                if (!hitEntities.Add(targetRoot)) continue;

                Vector2 toTarget = ((Vector2)col.transform.position - (Vector2)transform.position);
                if (toTarget.sqrMagnitude < 1.2f || Vector2.Dot(dir.normalized, toTarget.normalized) >= 0.30f)
                {
                    EnemyController enemyCtrl = FindEnemyController(col);
                    if (enemyCtrl != null) enemyCtrl.ApplySlow(0.40f, 2.0f);

                    DamageInfo info = new DamageInfo(damage, ElementType.Earth, isCrit, gameObject)
                    {
                        knockbackDirection = dir,
                        knockbackForce = 4.5f,
                        statusEffect = StatusEffectType.Slow,
                        statusDuration = 2.0f
                    };
                    ApplyDirectHit(col, info);
                    SpawnHitSpark(col.transform.position, new Color(0.9f, 0.6f, 0.2f, 1f));
                }
            }
        }

        /// <summary>
        /// 5. Cuồng Nộ Kích (Frenzied Strike): Bao quanh cơ thể ánh sáng đỏ, chém tất sát 1 mục tiêu duy nhất rồi ngay lập tức trở lại bình thường.
        /// </summary>
        private IEnumerator PerformFrenziedStrikeRoutine(SkillData skill, Vector3 center, Vector2 dir, Quaternion aimRotation, float damage, bool isCrit)
        {
            if (fourDirAnimator != null) fourDirAnimator.TriggerAttack(dir, 0.36f, false);

            // 1. Bật hào quang ánh sáng đỏ bao quanh cơ thể + nhuộm ánh đỏ nhân vật
            Color originalSpriteColor = spriteRenderer != null ? spriteRenderer.color : Color.white;
            if (spriteRenderer != null) spriteRenderer.color = new Color(1f, 0.32f, 0.32f, 1f);

            GameObject redAura = CreateProceduralRingObject("FrenziedRedAura", transform.position, new Color(1f, 0.12f, 0.12f, 0.85f), 1.65f, 18);
            redAura.transform.SetParent(transform, false);
            redAura.transform.localPosition = Vector3.zero;

            GameObject redCore = CreateProceduralDiscObject("FrenziedRedCore", transform.position, new Color(1f, 0.18f, 0.1f, 0.42f), 1.45f, 16);
            redCore.transform.SetParent(redAura.transform, false);
            redCore.transform.localPosition = Vector3.zero;

            // Tích lực ánh đỏ ngắn rồi vung kiếm chém 1 mục tiêu duy nhất
            float chargeTime = 0.15f;
            float elapsed = 0f;
            while (elapsed < chargeTime)
            {
                elapsed += Time.deltaTime;
                if (redAura != null)
                {
                    float pulse = 1.0f + Mathf.Sin(elapsed * 40f) * 0.18f;
                    redAura.transform.localScale = new Vector3(pulse, pulse, 1f);
                }
                yield return null;
            }

            var wsv = EnsureWeaponSocketView();
            if (wsv != null)
            {
                wsv.ApplyClassWeapon(ClassBranch.Warrior);
                wsv.PlayMeleeSlashAnimation(dir, 0.22f, 130f);
            }

            float missingHpPercent = (stats != null && stats.maxHp > 0) ? Mathf.Clamp01(1f - (stats.currentHp / stats.maxHp)) : 0f;
            float finalDamage = damage * (1f + missingHpPercent * 1.5f);

            // Tìm 1 mục tiêu duy nhất gần nhất theo hướng chém
            Collider2D singleTarget = FindClosestEnemyTarget(transform.position + (Vector3)dir * 1.1f, 2.8f);
            Vector3 strikePos = singleTarget != null ? singleTarget.transform.position : (transform.position + (Vector3)dir * 1.3f);

            // Hiệu ứng vệt chém huyết sắc chữ X trên mục tiêu duy nhất
            SpawnProceduralSlashWave(strikePos, dir, new Color(1f, 0.12f, 0.12f, 0.98f), 2.3f, 0.24f);
            Vector2 crossDir = new Vector2(-dir.y, dir.x);
            SpawnProceduralSlashWave(strikePos, (dir + crossDir * 0.6f).normalized, new Color(1f, 0.35f, 0.2f, 0.95f), 2.0f, 0.24f);

            if (singleTarget != null)
            {
                ApplyDirectHit(singleTarget, finalDamage, ElementType.Fire, true, dir, 8.0f);
                SpawnHitSpark(singleTarget.transform.position, new Color(1f, 0.15f, 0.1f, 1f));
            }

            EventBus.OnShowUIMessage?.Invoke($"💥 Cuồng Nộ Kích: Tất sát đơn mục tiêu (+{(missingHpPercent * 150f):F0}% ST)!");

            yield return new WaitForSeconds(0.12f);

            // 2. Sau khi chém xong 1 mục tiêu duy nhất thì tắt ánh sáng đỏ và trở lại bình thường ngay lập tức
            if (redAura != null) Destroy(redAura);
            if (spriteRenderer != null) spriteRenderer.color = originalSpriteColor;
        }

        /// <summary>
        /// 6. Khai Sơn Bạt Địa - Tuyệt Kỹ (melee_ultimate_crush): Bay người lên cao sau đó dậm mạnh xuống mặt đất diện rộng (KHÔNG hiện thanh kiếm).
        /// </summary>
        private IEnumerator PerformUltimateCrushJumpSlamRoutine(SkillData skill, Vector3 mousePos, Vector2 aimDir, float damage, bool isCrit)
        {
            var wsv = EnsureWeaponSocketView();
            if (wsv != null) wsv.StopAndHideWeapon();
            if (fourDirAnimator != null) fourDirAnimator.TriggerAttack(aimDir, 0.55f, false);

            Vector3 startPos = transform.position;
            Vector2 delta = (Vector2)mousePos - (Vector2)startPos;
            float jumpDist = Mathf.Min(delta.magnitude, 4.2f);
            Vector3 targetGroundPos = startPos + (Vector3)(aimDir * jumpDist);

            // Tạo bóng dưới mặt đất để thấy rõ độ cao khi nhân vật bay lên không trung
            GameObject groundShadow = CreateProceduralDiscObject("LeapShadow", startPos, new Color(0f, 0f, 0f, 0.45f), 1.1f, -2);
            groundShadow.transform.localScale = new Vector3(1.1f, 0.55f, 1f);

            // Pha 1: Bay vọt người lên cao (0.28s)
            float ascendDuration = 0.28f;
            float elapsed = 0f;
            float maxJumpHeight = 2.8f;

            while (elapsed < ascendDuration)
            {
                if (wsv != null) wsv.StopAndHideWeapon();
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / ascendDuration);
                float height = Mathf.Sin(t * Mathf.PI * 0.5f) * maxJumpHeight;

                Vector3 groundCur = Vector3.Lerp(startPos, targetGroundPos, t * 0.65f);
                transform.position = groundCur + Vector3.up * height;

                if (groundShadow != null)
                {
                    groundShadow.transform.position = groundCur + new Vector3(0f, -0.35f, 0f);
                    float shadowScale = Mathf.Lerp(1.1f, 0.55f, t);
                    groundShadow.transform.localScale = new Vector3(shadowScale, shadowScale * 0.5f, 1f);
                }
                yield return null;
            }

            // Pha 2: Lao thẳng dậm mạnh xuống mặt đất (0.14s)
            float slamDuration = 0.14f;
            elapsed = 0f;
            Vector3 apexGround = Vector3.Lerp(startPos, targetGroundPos, 0.65f);

            while (elapsed < slamDuration)
            {
                if (wsv != null) wsv.StopAndHideWeapon();
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / slamDuration);
                float height = Mathf.Lerp(maxJumpHeight, 0f, t * t);

                Vector3 groundCur = Vector3.Lerp(apexGround, targetGroundPos, t);
                transform.position = groundCur + Vector3.up * height;

                if (groundShadow != null)
                {
                    groundShadow.transform.position = groundCur + new Vector3(0f, -0.35f, 0f);
                    float shadowScale = Mathf.Lerp(0.55f, 1.3f, t);
                    groundShadow.transform.localScale = new Vector3(shadowScale, shadowScale * 0.5f, 1f);
                }
                yield return null;
            }

            transform.position = targetGroundPos;
            if (groundShadow != null) Destroy(groundShadow);

            // Pha 3: Dậm mạnh xuống đất tạo sóng chấn động nổ diện rộng 360°
            float slamRadius = 4.2f;
            StartCoroutine(SpawnEarthshatterShockwaveRoutine(targetGroundPos, slamRadius));
            DamageEnemiesInCircle(targetGroundPos, slamRadius, damage, ElementType.Earth, true);

            Collider2D[] hits = Physics2D.OverlapCircleAll(targetGroundPos, slamRadius, enemyLayers);
            foreach (var col in hits)
            {
                if (col == null || col.gameObject == gameObject) continue;
                EnemyController ec = FindEnemyController(col);
                if (ec != null) ec.ApplyStun(1.0f);
            }
            EventBus.OnShowUIMessage?.Invoke("🌋 Khai Sơn Bạt Địa: Giáng đòn dậm đất hủy diệt diện rộng!");
        }

        // ═══════════════════════════════════════════════════════════════════════════
        // PHÁI PHÁP SƯ (MAGE - 6 KỸ NĂNG)
        // ═══════════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Đòn đánh thường Pháp Sư: Phóng luồng ánh sáng trắng sắc nét (codeart2d mage_white_orb) từ đầu gậy phép bay theo hướng con trỏ chuột.
        /// </summary>
        private void SpawnMageLightBolt(Vector3 spawnPos, Vector2 aimDir, Color boltColor, float damage, ElementType elem, bool isCrit, float speed, DamageType dmgType = DamageType.Magical)
        {
            Vector2 normDir = aimDir.sqrMagnitude > 0.001f ? aimDir.normalized : Vector2.right;
            float angle = Mathf.Atan2(normDir.y, normDir.x) * Mathf.Rad2Deg;

            GameObject boltGo = new GameObject("MageWhiteLightBolt");
            boltGo.transform.position = spawnPos;
            boltGo.transform.rotation = Quaternion.Euler(0f, 0f, angle);
            boltGo.transform.localScale = new Vector3(0.85f, 0.85f, 1f);

            var sr = boltGo.AddComponent<SpriteRenderer>();
            Sprite whiteOrb = ActionRPG.Visuals.CodeArtSpriteBank.LoadSprite("mage_white_orb", new Vector2(0.56f, 0.5f), 32f);
            sr.sprite = whiteOrb != null ? whiteOrb : GetOrCreateDiscSprite();
            sr.color = Color.white;
            sr.sortingLayerID = GetVfxSortingLayerID();
            sr.sortingOrder = GetVfxSortingOrder(22);

            var col2d = boltGo.AddComponent<CircleCollider2D>();
            col2d.isTrigger = true;
            col2d.radius = 0.20f;

            var rb2d = boltGo.AddComponent<Rigidbody2D>();
            rb2d.gravityScale = 0f;
            rb2d.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            var pb = boltGo.AddComponent<ProjectileBehavior>();
            pb.Initialize(normDir, damage, elem, isCrit, gameObject, enemyLayers, speed, null, 1, dmgType);
        }

        /// <summary>
        /// 1. Hỏa Cầu (Fire Orb): Hiện gậy phép -> ánh sáng màu đỏ trên đầu gậy phép -> quả cầu lửa bay theo vị trí con trỏ chuột.
        /// </summary>
        private void SpawnFireOrbExplosive(SkillData skill, Vector3 firePos, Vector2 aimDir, Quaternion aimRotation, float damage, bool isCrit)
        {
            if (fourDirAnimator != null) fourDirAnimator.TriggerCast(aimDir, 0.34f, false);

            Color redLight = new Color(1f, 0.22f, 0.08f, 1f);
            var wsv = EnsureWeaponSocketView();
            if (wsv != null)
            {
                wsv.ApplyClassWeapon(ClassBranch.Mage);
                wsv.PlayStaffLightCastAnimation(aimDir, redLight, 0.14f, 0.14f, tipPos =>
                {
                    LaunchFireOrbFromTip(skill, tipPos, aimDir, aimRotation, damage, isCrit);
                });
            }
            else
            {
                LaunchFireOrbFromTip(skill, firePos, aimDir, aimRotation, damage, isCrit);
            }
        }

        private void LaunchFireOrbFromTip(SkillData skill, Vector3 spawnPos, Vector2 aimDir, Quaternion aimRotation, float damage, bool isCrit)
        {
            GameObject projGo = new GameObject("MageFireOrb_Proj");
            projGo.transform.position = spawnPos;
            projGo.transform.rotation = aimRotation;
            projGo.transform.localScale = new Vector3(0.95f, 0.95f, 1f);

            var sr = projGo.AddComponent<SpriteRenderer>();
            Sprite fireOrbSpr = ActionRPG.Visuals.CodeArtSpriteBank.LoadSprite("mage_fire_orb", new Vector2(0.56f, 0.5f), 32f);
            sr.sprite = fireOrbSpr != null ? fireOrbSpr : GetOrCreateDiscSprite();
            sr.color = Color.white;
            sr.sortingLayerID = GetVfxSortingLayerID();
            sr.sortingOrder = GetVfxSortingOrder(22);

            var c = projGo.AddComponent<CircleCollider2D>();
            c.isTrigger = true;
            c.radius = 0.24f;

            var r = projGo.AddComponent<Rigidbody2D>();
            r.gravityScale = 0f;
            r.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            ProjectileBehavior pb = GetOrAddProjectileBehavior(projGo);
            pb.Initialize(aimDir, damage, ElementType.Fire, isCrit, gameObject, enemyLayers, 15f, null, 1, DamageType.Magical);
            StartCoroutine(TrackProjectileExplosion(projGo, damage, isCrit, 2.2f));
        }

        private IEnumerator TrackProjectileExplosion(GameObject projGo, float damage, bool isCrit, float radius)
        {
            Vector3 lastPos = projGo != null ? projGo.transform.position : transform.position;
            while (projGo != null)
            {
                lastPos = projGo.transform.position;
                yield return null;
            }
            DamageEnemiesInCircle(lastPos, radius, damage * 0.85f, ElementType.Fire, isCrit, DamageType.Magical);
            StartCoroutine(ExpandShockwaveRingRoutine(lastPos, new Color(1f, 0.35f, 0.1f, 0.95f), 0.35f, radius * 0.85f, 0.25f));
            SpawnHitSpark(lastPos, new Color(1f, 0.35f, 0.1f, 1f));
        }

        /// <summary>
        /// 2. Băng Phong Thược (Frost Spikes):
        /// Hiện gậy phép thẳng đứng -> nghiêng đầu gậy nhẹ về phía trước -> tụ quang xanh băng trên đầu gậy -> triệu hồi cụm chông băng trồi từ đất.
        /// </summary>
        private IEnumerator PerformFrostSpikesRoutine(SkillData skill, Vector3 mousePos, Quaternion aimRotation, float damage, bool isCrit)
        {
            Vector2 aimDir = GetCurrentAimDir(mousePos);
            if (fourDirAnimator != null) fourDirAnimator.TriggerCast(aimDir, 0.44f, false);

            // Đảm bảo vị trí triệu hồi cụm chông băng luôn nằm trong tầm nhìn rõ ràng phía trước Pháp Sư (1.2m .. 4.8m)
            Vector2 deltaToMouse = (Vector2)(mousePos - transform.position);
            float distToMouse = deltaToMouse.magnitude;
            float spawnDist = (distToMouse >= 0.9f && distToMouse <= 5.2f) ? distToMouse : 2.3f;
            Vector3 targetGroundPos = transform.position + (Vector3)(aimDir * spawnDist);
            targetGroundPos.z = 0f;

            bool staffCastReleased = false;
            Vector3 releasedTipPos = transform.position + new Vector3(aimDir.x >= 0f ? 0.18f : -0.18f, 0.28f, 0f);
            Color iceBlueLight = new Color(0.22f, 0.90f, 1f, 1f);

            var wsv = EnsureWeaponSocketView();
            if (wsv != null)
            {
                wsv.ApplyClassWeapon(ClassBranch.Mage);
                // Bước 1 & 2 & 3: Hiện gậy phép thẳng đứng -> nghiêng đầu gậy nhẹ về phía trước -> tụ quang xanh băng trên đầu gậy
                wsv.PlayStaffLightCastAnimation(aimDir, iceBlueLight, 0.22f, 0.16f, tipPos =>
                {
                    releasedTipPos = tipPos;
                    staffCastReleased = true;
                });
            }
            else
            {
                staffCastReleased = true;
            }

            // Khi đầu gậy vừa nghiêng nhẹ về phía trước (~0.08s), tạo vòng tụ quang xanh băng thu nhỏ hội tụ vào đầu gậy
            float wait = 0f;
            bool spawnedGatherRing = false;
            while (!staffCastReleased && wait < 0.28f)
            {
                wait += Time.deltaTime;
                if (!spawnedGatherRing && wait >= 0.08f)
                {
                    spawnedGatherRing = true;
                    float sideSign = (aimDir.x < -0.05f) ? -1f : 1f;
                    Vector3 approxTipPos = transform.position + new Vector3(sideSign * 0.28f, 0.28f + aimDir.y * 0.04f, 0f);
                    StartCoroutine(ExpandShockwaveRingRoutine(approxTipPos, iceBlueLight, 0.52f, 0.12f, 0.13f));
                }
                yield return null;
            }

            // Loé tia băng nhỏ tại đầu gậy khi hoàn tất tụ quang xanh băng
            SpawnHitSpark(releasedTipPos, iceBlueLight);

            // Bước 4: Triệu hồi cụm chông băng trồi vọt từ mặt đất lên tại vị trí chỉ định
            StartCoroutine(SpawnEruptingIceSpikesVisualRoutine(targetGroundPos, 2.2f, 1.15f));

            yield return new WaitForSeconds(0.16f);

            Collider2D[] hits = Physics2D.OverlapCircleAll(targetGroundPos, 2.4f, enemyLayers);
            var hitEntities = new HashSet<GameObject>();
            foreach (var col in hits)
            {
                if (col == null || col.gameObject == gameObject) continue;
                Hurtbox2D hurt = FindHurtbox(col);
                EnemyStats enemy = FindEnemyStats(col);
                if (hurt == null && enemy == null) continue;
                GameObject targetRoot = hurt != null ? hurt.transform.root.gameObject : enemy.transform.root.gameObject;
                if (!hitEntities.Add(targetRoot)) continue;

                EnemyController enemyCtrl = FindEnemyController(col);
                if (enemyCtrl != null) enemyCtrl.ApplyRoot(1.5f);

                DamageInfo info = new DamageInfo(damage, ElementType.Water, isCrit, gameObject, DamageType.Magical)
                {
                    knockbackDirection = Vector2.zero,
                    knockbackForce = 1.0f
                };
                ApplyDirectHit(col, info);
                SpawnHitSpark(col.transform.position, iceBlueLight);
            }
        }

        /// <summary>
        /// 3. Thiểm Dịch (Blink / Arcane Shift): Biến ảnh tức thời về hướng chuột, để lại tàn ảnh tại vị trí trước khi biến (KHÔNG hiện gậy phép).
        /// </summary>
        private void PerformMageBlink(SkillData skill, Vector3 mousePos, Quaternion aimRotation)
        {
            var wsv = EnsureWeaponSocketView();
            if (wsv != null) wsv.StopAndHideWeapon();

            Vector2 delta = (Vector2)mousePos - (Vector2)transform.position;
            float dist = Mathf.Min(delta.magnitude, 5.5f);
            if (dist < 1.0f) dist = 3.5f;
            Vector2 dir = delta.sqrMagnitude > 0.01f ? delta.normalized : Vector2.right;
            Vector3 oldPos = transform.position;

            if (fourDirAnimator != null) fourDirAnimator.SetFacingDirection(dir);

            // Để lại tàn ảnh rõ nét của nhân vật tại vị trí trước khi biến
            SpawnCharacterAfterimage(oldPos, new Color(0.55f, 0.82f, 1f, 0.88f), 0.75f);
            StartCoroutine(ExpandShockwaveRingRoutine(oldPos, new Color(0.45f, 0.78f, 1f, 0.85f), 0.25f, 1.15f, 0.30f));

            transform.position = (Vector2)oldPos + dir * dist;

            StartCoroutine(ExpandShockwaveRingRoutine(transform.position, new Color(0.6f, 0.9f, 1f, 0.9f), 1.05f, 0.25f, 0.22f));

            Collider2D[] slowTargets = Physics2D.OverlapCircleAll(oldPos, 3.2f, enemyLayers);
            foreach (var col in slowTargets)
            {
                if (col == null) continue;
                EnemyController e = FindEnemyController(col);
                if (e != null) e.ApplySlow(0.35f, 2.0f);
            }
        }

        /// <summary>
        /// 4. Lôi Điện Tỏa (Chain Lightning): Hiện gậy phép -> ánh sáng màu vàng trên đầu gậy -> bắn ánh sáng sét vàng vào mục tiêu chỉ định và nảy sang tối đa 3 kẻ địch lân cận.
        /// </summary>
        private void PerformChainLightning(SkillData skill, Vector3 mousePos, Quaternion aimRotation, float damage, bool isCrit)
        {
            Vector2 aimDir = GetCurrentAimDir(mousePos);
            if (fourDirAnimator != null) fourDirAnimator.TriggerCast(aimDir, 0.36f, false);

            Color yellowLight = new Color(1f, 0.92f, 0.18f, 1f);
            var wsv = EnsureWeaponSocketView();
            if (wsv != null)
            {
                wsv.ApplyClassWeapon(ClassBranch.Mage);
                wsv.PlayStaffLightCastAnimation(aimDir, yellowLight, 0.14f, 0.16f, tipPos =>
                {
                    StartCoroutine(ChainLightningFromTipRoutine(skill, tipPos, mousePos, aimRotation, damage, isCrit));
                });
            }
            else
            {
                StartCoroutine(ChainLightningFromTipRoutine(skill, transform.position + (Vector3)aimDir * 0.45f, mousePos, aimRotation, damage, isCrit));
            }
        }

        private IEnumerator ChainLightningFromTipRoutine(SkillData skill, Vector3 staffTipPos, Vector3 mousePos, Quaternion aimRotation, float baseDamage, bool isCrit)
        {
            Collider2D firstCol = FindClosestEnemyTarget(mousePos, 4.5f);
            if (firstCol == null) firstCol = FindClosestEnemyTarget(transform.position, 6.5f);
            Color lightningYellow = new Color(1f, 0.95f, 0.22f, 1f);

            if (firstCol == null)
            {
                SpawnLightningBoltLine(staffTipPos, mousePos, lightningYellow, 0.28f);
                StartCoroutine(ExpandShockwaveRingRoutine(mousePos, lightningYellow, 0.2f, 0.95f, 0.22f));
                SpawnHitSpark(mousePos, lightningYellow);
                yield break;
            }

            var hitList = new HashSet<GameObject>();
            Transform curTarget = firstCol.transform;
            Vector3 prevPoint = staffTipPos;
            float curDamage = baseDamage;

            // Phóng vào mục tiêu đầu tiên + nảy sang tối đa 3 kẻ địch lân cận (tổng cộng tối đa 4 mục tiêu)
            for (int i = 0; i < 4; i++)
            {
                if (curTarget == null) break;
                GameObject rootObj = curTarget.root != null ? curTarget.root.gameObject : curTarget.gameObject;
                hitList.Add(rootObj);

                Vector3 targetPos = curTarget.position;
                SpawnLightningBoltLine(prevPoint, targetPos, lightningYellow, 0.28f);
                StartCoroutine(ExpandShockwaveRingRoutine(targetPos, lightningYellow, 0.2f, 0.88f, 0.18f));

                Collider2D targetCollider = curTarget.GetComponent<Collider2D>();
                if (targetCollider == null) targetCollider = curTarget.GetComponentInChildren<Collider2D>();
                ApplyDirectHit(targetCollider, curDamage, ElementType.Electric, isCrit, Vector2.zero, 2f, DamageType.Magical);
                SpawnHitSpark(targetPos, lightningYellow);

                prevPoint = targetPos;
                yield return new WaitForSeconds(0.08f);

                // Tìm kẻ địch lân cận tiếp theo chưa bị trúng sét
                Collider2D[] neighbors = Physics2D.OverlapCircleAll(prevPoint, 4.5f, enemyLayers);
                Transform nextTarget = null;
                float bestDist = float.MaxValue;
                foreach (var col in neighbors)
                {
                    if (col == null || col.gameObject == gameObject) continue;
                    Hurtbox2D h = FindHurtbox(col);
                    EnemyStats es = FindEnemyStats(col);
                    if (h == null && es == null) continue;
                    GameObject r = h != null ? h.transform.root.gameObject : es.transform.root.gameObject;
                    if (hitList.Contains(r)) continue;

                    float d = Vector2.Distance(prevPoint, col.transform.position);
                    if (d < bestDist)
                    {
                        bestDist = d;
                        nextTarget = col.transform;
                    }
                }

                curTarget = nextTarget;
                curDamage *= 0.85f;
            }
        }

        /// <summary>
        /// 5. Khiên Hộ Thuật (Mana Shield): Hiện gậy phép -> ánh sáng màu trắng trên đầu gậy phép -> tạo 1 lớp khiên trắng bao xung quanh cơ thể.
        /// </summary>
        private void PerformManaShield(SkillData skill)
        {
            Vector2 aimDir = GetCurrentAimDir(GetCurrentMouseWorld());
            if (fourDirAnimator != null) fourDirAnimator.TriggerCast(aimDir, 0.35f, false);

            if (stats != null)
            {
                stats.AddBarrier(70f, 5.0f);
                stats.RestoreMp(stats.maxMp * 0.25f);
            }

            var wsv = EnsureWeaponSocketView();
            if (wsv != null)
            {
                wsv.ApplyClassWeapon(ClassBranch.Mage);
                wsv.PlayStaffLightCastAnimation(aimDir, Color.white, 0.14f, 0.14f, _ =>
                {
                    StartCoroutine(SpawnWhiteManaShieldSphereRoutine(5.0f));
                });
            }
            else
            {
                StartCoroutine(SpawnWhiteManaShieldSphereRoutine(5.0f));
            }
            EventBus.OnShowUIMessage?.Invoke("🛡️ Khiên Hộ Thuật: Kích hoạt vòng khiên trắng ma pháp (+70 Máu Ảo)!");
        }

        /// <summary>
        /// 6. Bí Thuật Pháo / Đại Hồng Thủy (mage_arcane_blast): Hiện gậy phép -> ánh sáng màu đỏ trên đầu gậy -> phóng tia ánh sáng đỏ thành 1 đường thẳng theo hướng con trỏ chuột.
        /// </summary>
        private void PerformMageArcaneLaserBeam(SkillData skill, Vector2 aimDir, float damage, bool isCrit)
        {
            if (fourDirAnimator != null) fourDirAnimator.TriggerCast(aimDir, 0.48f, false);

            Color crimsonLight = new Color(1f, 0.15f, 0.18f, 1f);
            var wsv = EnsureWeaponSocketView();
            if (wsv != null)
            {
                wsv.ApplyClassWeapon(ClassBranch.Mage);
                wsv.PlayStaffLightCastAnimation(aimDir, crimsonLight, 0.16f, 0.26f, tipPos =>
                {
                    StartCoroutine(FireRedLaserBeamRoutine(tipPos, aimDir, damage * 1.6f, isCrit));
                });
            }
            else
            {
                StartCoroutine(FireRedLaserBeamRoutine(transform.position + (Vector3)aimDir * 0.45f, aimDir, damage * 1.6f, isCrit));
            }
        }

        private IEnumerator FireRedLaserBeamRoutine(Vector3 originPos, Vector2 aimDir, float damage, bool isCrit)
        {
            Vector2 dir = aimDir.sqrMagnitude > 0.001f ? aimDir.normalized : Vector2.right;
            float beamLength = 10.5f;
            float beamWidth = 0.36f;
            Vector3 endPos = originPos + (Vector3)(dir * beamLength);

            // Tạo tia laser đỏ sắc nét có lõi trắng rực ở giữa vừa vặn với tầm vóc nhân vật
            GameObject beamRoot = new GameObject("ArcaneRedLaserBeam");
            LineRenderer outerLine = beamRoot.AddComponent<LineRenderer>();
            ConfigureLaserLineRenderer(outerLine, beamWidth, new Color(1f, 0.14f, 0.18f, 0.95f), 22);
            outerLine.SetPosition(0, originPos);
            outerLine.SetPosition(1, endPos);

            GameObject innerObj = new GameObject("InnerCore");
            innerObj.transform.SetParent(beamRoot.transform, false);
            LineRenderer innerLine = innerObj.AddComponent<LineRenderer>();
            ConfigureLaserLineRenderer(innerLine, beamWidth * 0.42f, new Color(1f, 0.92f, 0.92f, 1f), 24);
            innerLine.SetPosition(0, originPos);
            innerLine.SetPosition(1, endPos);

            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            Vector2 boxCenter = (Vector2)originPos + dir * (beamLength * 0.5f);
            Collider2D[] hits = Physics2D.OverlapBoxAll(boxCenter, new Vector2(beamLength, 0.95f), angle, enemyLayers);
            var hitEntities = new HashSet<GameObject>();
            foreach (var col in hits)
            {
                if (col == null || col.gameObject == gameObject) continue;
                Hurtbox2D hurt = FindHurtbox(col);
                EnemyStats enemy = FindEnemyStats(col);
                if (hurt == null && enemy == null) continue;
                GameObject targetRoot = hurt != null ? hurt.transform.root.gameObject : enemy.transform.root.gameObject;
                if (!hitEntities.Add(targetRoot)) continue;

                ApplyDirectHit(col, damage, ElementType.Fire, isCrit, dir, 5.5f, DamageType.Magical);
                SpawnHitSpark(col.transform.position, new Color(1f, 0.2f, 0.2f, 1f));
            }

            float duration = 0.30f;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float curW = beamWidth * (1f - t * 0.80f);
                if (outerLine != null)
                {
                    outerLine.startWidth = curW;
                    outerLine.endWidth = curW;
                }
                if (innerLine != null)
                {
                    innerLine.startWidth = curW * 0.42f;
                    innerLine.endWidth = curW * 0.42f;
                }
                yield return null;
            }

            if (beamRoot != null) Destroy(beamRoot);
        }

        // ═══════════════════════════════════════════════════════════════════════════
        // PHÁI XẠ THỦ (ARCHER - 6 KỸ NĂNG + ĐÁNH THƯỜNG)
        // ═══════════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Tạo và phóng Mũi Tên Xạ Thủ nhỏ gọn, sắc nét (codeart2d archer_arrow 16x9px ở 32 PPU, FilterMode.Point).
        /// </summary>
        private GameObject SpawnArcherArrowProjectile(
            Vector3 spawnPos,
            Vector2 aimDir,
            float damage,
            ElementType elem,
            bool isCrit,
            float speed,
            int pierceCount,
            Color arrowColor,
            float scaleMultiplier = 0.85f,
            DamageType dmgType = DamageType.Physical)
        {
            Vector2 normDir = aimDir.sqrMagnitude > 0.001f ? aimDir.normalized : Vector2.right;
            float angle = Mathf.Atan2(normDir.y, normDir.x) * Mathf.Rad2Deg;
            Quaternion rot = Quaternion.Euler(0f, 0f, angle);

            GameObject projGo = CreateProceduralArrowObject("ArcherArrow_Projectile", spawnPos, arrowColor);
            projGo.transform.rotation = rot;
            projGo.transform.localScale = Vector3.one * scaleMultiplier;

            SpriteRenderer sr = FindSpriteRenderer(projGo);
            if (sr == null) sr = projGo.AddComponent<SpriteRenderer>();
            sr.sprite = GetOrCreateArrowSprite();
            sr.color = arrowColor;
            BoostVfxSortingOrder(projGo, 18);

            var c = projGo.AddComponent<CircleCollider2D>();
            c.isTrigger = true;
            c.radius = 0.18f;

            var r = projGo.AddComponent<Rigidbody2D>();
            r.gravityScale = 0f;
            r.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            ProjectileBehavior pb = GetOrAddProjectileBehavior(projGo);
            pb.Initialize(normDir, damage, elem, isCrit, gameObject, enemyLayers, speed, null, pierceCount, dmgType);
            return projGo;
        }

        /// <summary>
        /// 1. Xuyên Vân Tiễn (Piercing Shot): Giữ lại như cũ (kéo cung bắn mũi tên xuyên qua mọi mục tiêu trên đường thẳng).
        /// </summary>
        private void SpawnPiercingShot(SkillData skill, Vector3 firePos, Vector2 aimDir, Quaternion aimRotation, float damage, bool isCrit)
        {
            if (fourDirAnimator != null) fourDirAnimator.TriggerAttack(aimDir, 0.28f, false);

            var wsv = EnsureWeaponSocketView();
            if (wsv != null)
            {
                wsv.ApplyClassWeapon(ClassBranch.Archer);
                wsv.PlayBowDrawWithArrowAnimation(aimDir, 0.14f, releasePos =>
                {
                    FirePiercingArrowProjectile(skill, releasePos, aimDir, aimRotation, damage, isCrit);
                }, new Color(0.55f, 1f, 0.85f, 1f));
            }
            else
            {
                FirePiercingArrowProjectile(skill, firePos, aimDir, aimRotation, damage, isCrit);
            }
        }

        private void FirePiercingArrowProjectile(SkillData skill, Vector3 spawnPos, Vector2 aimDir, Quaternion aimRotation, float damage, bool isCrit)
        {
            StartCoroutine(ExpandShockwaveRingRoutine(spawnPos, new Color(0.45f, 1f, 0.8f, 0.85f), 0.18f, 0.75f, 0.16f));
            SpawnArcherArrowProjectile(spawnPos, aimDir, damage, ElementType.Wind, isCrit, 22f, 99, new Color(0.55f, 1f, 0.88f, 1f), 0.98f);
        }

        /// <summary>
        /// 2. Mưa Tên (Volley / Barrage): Bắn 1 mũi tên lên trên cao -> từ mũi tên đó hiện vòng tròn ma pháp trên cao -> nhiều mũi tên rơi xuống vùng hình nón phía trước.
        /// </summary>
        private void PerformArcherVolley(SkillData skill, Vector3 firePos, Vector2 aimDir, Vector3 mousePos, Quaternion aimRotation, float damage, bool isCrit)
        {
            if (fourDirAnimator != null) fourDirAnimator.TriggerAttack(aimDir, 0.34f, false);

            Vector2 delta = (Vector2)mousePos - (Vector2)transform.position;
            float targetDist = Mathf.Clamp(delta.magnitude, 2.5f, 5.5f);
            Vector3 targetZoneCenter = transform.position + (Vector3)(aimDir * targetDist);

            var wsv = EnsureWeaponSocketView();
            if (wsv != null)
            {
                wsv.ApplyClassWeapon(ClassBranch.Archer);
                wsv.PlayBowDrawWithArrowAnimation(Vector2.up, 0.14f, releasePos =>
                {
                    StartCoroutine(ArcherSkyCircleVolleyRoutine(skill, releasePos, targetZoneCenter, aimDir, damage, isCrit));
                }, new Color(0.45f, 1f, 0.75f, 1f));
            }
            else
            {
                StartCoroutine(ArcherSkyCircleVolleyRoutine(skill, firePos, targetZoneCenter, aimDir, damage, isCrit));
            }
        }

        private IEnumerator ArcherSkyCircleVolleyRoutine(SkillData skill, Vector3 bowPos, Vector3 targetCenter, Vector2 aimDir, float damage, bool isCrit)
        {
            // BƯỚC 1: Bắn 1 mũi tên nhỏ gọn sắc nét bay vút thẳng lên bầu trời
            Vector3 skyApex = targetCenter + new Vector3(0f, 3.8f, 0f);
            GameObject skyArrow = CreateProceduralArrowObject("SkyVolleyArrow", bowPos, new Color(0.55f, 1f, 0.78f, 1f));
            skyArrow.transform.localScale = Vector3.one * 0.90f;
            Vector2 upDir = (skyApex - bowPos).normalized;
            skyArrow.transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(upDir.y, upDir.x) * Mathf.Rad2Deg);

            float flyUpDuration = 0.20f;
            float elapsed = 0f;
            while (elapsed < flyUpDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / flyUpDuration);
                if (skyArrow != null)
                    skyArrow.transform.position = Vector3.Lerp(bowPos, skyApex, t);
                yield return null;
            }
            if (skyArrow != null) Destroy(skyArrow);

            // BƯỚC 2: Từ đỉnh mũi tên trên cao, bung ra Vòng Tròn Ma Pháp sắc nét trên không trung + vòng chỉ báo dưới đất
            GameObject skyMagicCircle = CreateProceduralRingObject("SkyMagicCircle", skyApex, new Color(0.35f, 1f, 0.75f, 0.95f), 1.6f, 24);
            skyMagicCircle.transform.localScale = new Vector3(1.55f, 0.75f, 1f);
            GameObject skyInnerRing = CreateProceduralRingObject("SkyInnerRing", skyApex, new Color(0.85f, 1f, 0.95f, 0.9f), 0.95f, 25);
            skyInnerRing.transform.SetParent(skyMagicCircle.transform, false);

            GameObject groundTargetRing = CreateProceduralRingObject("VolleyGroundZone", targetCenter, new Color(0.35f, 0.95f, 0.65f, 0.65f), 2.1f, 10);
            groundTargetRing.transform.localScale = new Vector3(1.9f, 1.15f, 1f);

            // BƯỚC 3: Từ vòng tròn ma pháp trên cao, mưa nhiều mũi tên sắc nét rơi dồn dập xuống khu vực mục tiêu
            int arrowRainCount = 12;
            for (int i = 0; i < arrowRainCount; i++)
            {
                Vector2 randOffset = Random.insideUnitCircle * 1.65f;
                Vector3 dropStart = skyApex + new Vector3(randOffset.x * 0.55f, 0f, 0f);
                Vector3 dropEnd = targetCenter + new Vector3(randOffset.x, randOffset.y * 0.65f, 0f);
                StartCoroutine(SpawnFallingArrowFromSkyRoutine(dropStart, dropEnd, new Color(0.55f, 1f, 0.78f, 1f), 0.15f));
                yield return new WaitForSeconds(0.028f);
            }

            // Gây sát thương và làm chậm kẻ địch trong vùng mưa tên rơi xuống
            DamageEnemiesInCircle(targetCenter, 3.0f, damage * 1.25f, ElementType.Wind, isCrit);
            Collider2D[] hits = Physics2D.OverlapCircleAll(targetCenter, 3.0f, enemyLayers);
            foreach (var col in hits)
            {
                if (col == null || col.gameObject == gameObject) continue;
                EnemyController e = FindEnemyController(col);
                if (e != null) e.ApplySlow(0.30f, 1.5f);
            }

            yield return new WaitForSeconds(0.20f);
            if (skyMagicCircle != null) Destroy(skyMagicCircle);
            if (groundTargetRing != null) Destroy(groundTargetRing);
        }

        /// <summary>
        /// 3. Nhào Lộn Thoát Thân (Tumble): Nhào lộn nhanh xoay 360° theo hướng con trỏ chuột (KHÔNG hiện vũ khí).
        /// </summary>
        private IEnumerator PerformArcherTumbleRoutine(SkillData skill, Vector2 aimDir, Quaternion aimRotation)
        {
            var wsv = EnsureWeaponSocketView();
            if (wsv != null) wsv.StopAndHideWeapon();

            Rigidbody2D rb2d = GetComponent<Rigidbody2D>();
            Vector2 rollVec = (aimDir.sqrMagnitude > 0.01f ? aimDir : Vector2.right).normalized;
            if (fourDirAnimator != null) fourDirAnimator.SetFacingDirection(rollVec);

            float duration = 0.25f;
            float elapsed = 0f;
            float spinDirection = (rollVec.x >= 0f) ? -360f : 360f;
            Quaternion originalRot = transform.localRotation;

            while (elapsed < duration)
            {
                if (wsv != null) wsv.StopAndHideWeapon();
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);

                transform.localRotation = Quaternion.Euler(0f, 0f, t * spinDirection);

                if (rb2d != null)
                    rb2d.linearVelocity = rollVec * 17f;
                else
                    transform.position += (Vector3)(rollVec * 17f * Time.deltaTime);

                if (Mathf.FloorToInt(t * 10f) != Mathf.FloorToInt((t - Time.deltaTime / duration) * 10f))
                {
                    SpawnCharacterAfterimage(transform.position, new Color(0.5f, 1f, 0.7f, 0.5f), 0.22f);
                }
                yield return null;
            }

            transform.localRotation = originalRot;
            if (rb2d != null) rb2d.linearVelocity = Vector2.zero;

            if (stats != null)
            {
                stats.ApplyBuff(BuffStatType.AttackPower, 20f, 3.0f);
                stats.ApplyBuff(BuffStatType.MoveSpeed, 1.2f, 3.0f);
            }
            EventBus.OnShowUIMessage?.Invoke("🏹 Nhào Lộn Thoát Thân: Lộn nhào né đòn & cường hóa tốc bắn (3s)!");
        }

        /// <summary>
        /// 4. Bẫy Dây (Snare Trap): Đặt bẫy tại vị trí chuột (giống bẫy Caitlyn trong Liên Minh Huyền Thoại, KHÔNG hiện vũ khí).
        /// </summary>
        private IEnumerator PerformSnareTrapRoutine(SkillData skill, Vector3 mousePos, Quaternion aimRotation, float damage, bool isCrit)
        {
            var wsv = EnsureWeaponSocketView();
            if (wsv != null) wsv.StopAndHideWeapon();

            Vector3 trapPos = mousePos;
            trapPos.z = 0f;

            // Tạo bẫy kẹp răng cưa sắc nét kiểu Caitlyn tại vị trí con trỏ chuột (không hiện vũ khí)
            GameObject trapObj = CreateCaitlynTrapVisual(trapPos);

            yield return new WaitForSeconds(0.35f);
            if (trapObj != null)
            {
                foreach (var sr in trapObj.GetComponentsInChildren<SpriteRenderer>())
                {
                    Color c = sr.color;
                    c.a *= 0.78f;
                    sr.color = c;
                }
            }

            float trapLife = 20f;
            while (trapLife > 0f && trapObj != null)
            {
                trapLife -= 0.08f;
                Collider2D[] stepped = Physics2D.OverlapCircleAll(trapPos, 1.35f, enemyLayers);
                Collider2D triggeredTarget = null;
                foreach (var col in stepped)
                {
                    if (col == null || col.gameObject == gameObject) continue;
                    Hurtbox2D h = FindHurtbox(col);
                    EnemyStats es = FindEnemyStats(col);
                    if (h != null || es != null)
                    {
                        triggeredTarget = col;
                        break;
                    }
                }

                if (triggeredTarget != null)
                {
                    foreach (var col in stepped)
                    {
                        if (col == null || col.gameObject == gameObject) continue;
                        EnemyController e = FindEnemyController(col);
                        if (e != null) e.ApplyRoot(1.25f);
                        ApplyDirectHit(col, damage, ElementType.Earth, isCrit, Vector2.zero, 2f);
                    }

                    StartCoroutine(AnimateTrapSnapAndRootRoutine(trapObj, triggeredTarget.transform.position, 1.25f));
                    EventBus.OnShowUIMessage?.Invoke("🕸️ Bẫy Dây Sập Hàm: Trói chân mục tiêu trong 1.25s!");
                    yield break;
                }
                yield return new WaitForSeconds(0.08f);
            }

            if (trapObj != null) Destroy(trapObj);
        }

        /// <summary>
        /// 5. Ngắm Chuẩn Dứt Điểm (Deadeye): Khóa mục tiêu tại con trỏ chuột, hiện tâm ngắm trên đầu/thân mục tiêu, kéo cung dồn lực rồi bắn phát tên cực đại.
        /// </summary>
        private IEnumerator PerformDeadeyeRoutine(SkillData skill, Vector3 firePos, Vector2 aimDir, Vector3 mousePos, Quaternion aimRotation, float damage, bool isCrit)
        {
            Rigidbody2D rb2d = GetComponent<Rigidbody2D>();
            if (rb2d != null) rb2d.linearVelocity = Vector2.zero;

            Collider2D lockedTarget = FindClosestEnemyTarget(mousePos, 4.5f);
            if (lockedTarget == null) lockedTarget = FindClosestEnemyTarget(transform.position, 8.0f);
            Vector3 lockPos = lockedTarget != null ? lockedTarget.transform.position : mousePos;
            Vector2 lockDir = GetCurrentAimDir(lockPos);

            if (fourDirAnimator != null) fourDirAnimator.TriggerAttack(lockDir, 0.85f, false);
            var wsv = EnsureWeaponSocketView();
            if (wsv != null)
            {
                wsv.ApplyClassWeapon(ClassBranch.Archer);
                wsv.PlayBowDrawWithArrowAnimation(lockDir, 0.68f, null, new Color(1f, 0.25f, 0.2f, 1f));
            }

            EventBus.OnShowUIMessage?.Invoke("🎯 Ngắm Chuẩn Dứt Điểm: Đã khóa tâm ngắm mục tiêu!");

            GameObject crosshairObj = CreateDeadeyeCrosshairVisual(lockPos + Vector3.up * 0.24f);

            float lockDuration = 0.65f;
            float elapsed = 0f;
            while (elapsed < lockDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / lockDuration);

                if (lockedTarget != null)
                {
                    lockPos = lockedTarget.transform.position;
                }
                else
                {
                    lockPos = GetCurrentMouseWorld();
                }

                lockDir = GetCurrentAimDir(lockPos);
                if (fourDirAnimator != null) fourDirAnimator.SetFacingDirection(lockDir);

                if (crosshairObj != null)
                {
                    crosshairObj.transform.position = lockPos + Vector3.up * 0.24f;
                    crosshairObj.transform.Rotate(0f, 0f, 180f * Time.deltaTime);
                    float scale = Mathf.Lerp(1.22f, 0.82f, t);
                    crosshairObj.transform.localScale = new Vector3(scale, scale, 1f);
                }
                yield return null;
            }

            if (crosshairObj != null) Destroy(crosshairObj);

            float finalDmg = damage * 2.2f;
            if (lockedTarget != null)
            {
                EnemyStats eStats = FindEnemyStats(lockedTarget);
                if (eStats != null && eStats.maxHp > 0)
                {
                    float missingHpPct = Mathf.Clamp01(1f - (eStats.currentHp / eStats.maxHp));
                    finalDmg *= (1f + missingHpPct * 1.5f);
                }
            }

            Vector3 spawnPos = transform.position + (Vector3)(lockDir * 0.42f);
            SpawnArcherArrowProjectile(spawnPos, lockDir, finalDmg, ElementType.Wind, true, 28f, 5, new Color(1f, 0.35f, 0.25f, 1f), 1.05f);

            if (lockedTarget != null)
            {
                ApplyDirectHit(lockedTarget, finalDmg, ElementType.Wind, true, lockDir, 7.5f);
                SpawnHitSpark(lockedTarget.transform.position, new Color(1f, 0.2f, 0.15f, 1f));
            }
            SpawnHitSpark(spawnPos, new Color(1f, 0.25f, 0.15f, 1f));
        }

        /// <summary>
        /// 6. Bão Tố Tiễn Vũ - Tuyệt Kỹ (arch_elemental_storm): Xả mưa tên năng lượng diện rộng trút xuống vùng chỉ định.
        /// </summary>
        private IEnumerator PerformElementalArrowStormRoutine(SkillData skill, Vector3 mousePos, Vector2 aimDir, float damage, bool isCrit)
        {
            if (fourDirAnimator != null) fourDirAnimator.TriggerAttack(aimDir, 0.4f, false);
            var wsv = EnsureWeaponSocketView();
            if (wsv != null)
            {
                wsv.ApplyClassWeapon(ClassBranch.Archer);
                wsv.PlayBowDrawWithArrowAnimation(Vector2.up, 0.18f, null, new Color(1f, 0.85f, 0.3f, 1f));
            }

            Vector3 stormCenter = mousePos;
            stormCenter.z = 0f;
            float stormRadius = 3.5f;

            // Vòng trận pháp bão tên dưới mặt đất và trên bầu trời
            GameObject groundCircle = CreateProceduralRingObject("ArrowStormGround", stormCenter, new Color(0.35f, 0.95f, 1f, 0.75f), 2.2f, 12);
            groundCircle.transform.localScale = new Vector3(2.1f, 1.25f, 1f);

            Vector3 skyCenter = stormCenter + new Vector3(0f, 4.2f, 0f);
            GameObject skyPortal = CreateProceduralRingObject("ArrowStormSkyPortal", skyCenter, new Color(1f, 0.82f, 0.28f, 0.92f), 1.8f, 24);
            skyPortal.transform.localScale = new Vector3(1.8f, 0.85f, 1f);

            Color[] energyColors =
            {
                new Color(1f, 0.35f, 0.15f, 1f), // Hỏa
                new Color(0.25f, 0.88f, 1f, 1f), // Băng
                new Color(0.4f, 1f, 0.55f, 1f),  // Phong
                new Color(1f, 0.92f, 0.25f, 1f)  // Lôi
            };

            int waves = 4;
            for (int w = 0; w < waves; w++)
            {
                for (int a = 0; a < 7; a++)
                {
                    Vector2 offset = Random.insideUnitCircle * (stormRadius * 0.78f);
                    Vector3 start = skyCenter + new Vector3(offset.x * 0.65f, Random.Range(-0.25f, 0.3f), 0f);
                    Vector3 end = stormCenter + new Vector3(offset.x, offset.y * 0.7f, 0f);
                    Color c = energyColors[(w + a) % energyColors.Length];
                    StartCoroutine(SpawnFallingArrowFromSkyRoutine(start, end, c, 0.14f));
                }

                DamageEnemiesInCircle(stormCenter, stormRadius, damage * 0.45f, ElementType.Wind, isCrit);
                StartCoroutine(ExpandShockwaveRingRoutine(stormCenter, energyColors[w % energyColors.Length], 0.35f, stormRadius * 0.65f, 0.22f));
                yield return new WaitForSeconds(0.26f);
            }

            if (groundCircle != null) Destroy(groundCircle);
            if (skyPortal != null) Destroy(skyPortal);
        }

        // ═══════════════════════════════════════════════════════════════════════════
        // PROCEDURAL 2D VFX HELPERS (NẠP TỪ CODEART2D PIXELSPEC & FX SẮC NÉT)
        // ═══════════════════════════════════════════════════════════════════════════

        private static Sprite s_cachedRingSprite;
        private static Sprite s_cachedDiscSprite;
        private static Sprite s_cachedSlashWaveSprite;
        private static Sprite s_cachedShieldSprite;
        private static Sprite s_cachedSpikeSprite;
        private static Sprite s_cachedRockSpikeSprite;
        private static Sprite s_cachedCrosshairSprite;
        private static Sprite s_cachedTrapSprite;
        private static Sprite s_cachedArrowSprite;
        private static Material s_cachedLineMaterial;

        private Collider2D FindClosestEnemyTarget(Vector3 searchCenter, float radius)
        {
            Collider2D[] cols = Physics2D.OverlapCircleAll(searchCenter, radius, enemyLayers);
            Collider2D best = null;
            float bestDist = float.MaxValue;
            foreach (var c in cols)
            {
                if (c == null || c.gameObject == gameObject) continue;
                Hurtbox2D h = FindHurtbox(c);
                EnemyStats es = FindEnemyStats(c);
                if (h == null && es == null) continue;
                float d = Vector2.Distance(searchCenter, c.transform.position);
                if (d < bestDist)
                {
                    bestDist = d;
                    best = c;
                }
            }
            return best;
        }

        private void SpawnCharacterAfterimage(Vector3 worldPos, Color tint, float duration)
        {
            if (spriteRenderer == null || spriteRenderer.sprite == null) return;
            GameObject ghost = new GameObject("Character_Afterimage");
            ghost.transform.position = worldPos;
            ghost.transform.rotation = transform.rotation;
            ghost.transform.localScale = transform.lossyScale;

            var sr = ghost.AddComponent<SpriteRenderer>();
            sr.sprite = spriteRenderer.sprite;
            sr.flipX = spriteRenderer.flipX;
            sr.sortingLayerID = GetVfxSortingLayerID();
            sr.sortingOrder = GetVfxSortingOrder(-1);
            sr.color = tint;

            StartCoroutine(FadeAndDestroySpriteRoutine(sr, duration));
        }

        private IEnumerator FadeAndDestroySpriteRoutine(SpriteRenderer sr, float duration)
        {
            if (sr == null) yield break;
            Color initColor = sr.color;
            float elapsed = 0f;
            while (elapsed < duration && sr != null)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                sr.color = new Color(initColor.r, initColor.g, initColor.b, Mathf.Lerp(initColor.a, 0f, t));
                yield return null;
            }
            if (sr != null) Destroy(sr.gameObject);
        }

        private IEnumerator SpawnGoldenBarrierAuraRoutine(float duration)
        {
            GameObject auraRoot = CreateProceduralRingObject("AegisGoldenBarrier", transform.position, new Color(1f, 0.88f, 0.22f, 0.88f), 1.65f, 18);
            auraRoot.transform.SetParent(transform, false);
            auraRoot.transform.localPosition = Vector3.zero;

            GameObject innerGlow = CreateProceduralDiscObject("GoldenCoreGlow", transform.position, new Color(1f, 0.92f, 0.35f, 0.28f), 1.55f, 16);
            innerGlow.transform.SetParent(auraRoot.transform, false);
            innerGlow.transform.localPosition = Vector3.zero;

            float elapsed = 0f;
            while (elapsed < duration && auraRoot != null)
            {
                elapsed += Time.deltaTime;
                auraRoot.transform.Rotate(0f, 0f, 95f * Time.deltaTime);
                float pulse = 1.0f + Mathf.Sin(elapsed * 8f) * 0.08f;
                auraRoot.transform.localScale = new Vector3(pulse, pulse, 1f);
                yield return null;
            }
            if (auraRoot != null) Destroy(auraRoot);
        }

        private IEnumerator SpawnHolyHealAuraRoutine(float duration)
        {
            GameObject healRoot = CreateProceduralRingObject("HolyHealAura", transform.position, new Color(0.55f, 1f, 0.65f, 0.9f), 0.95f, 19);
            healRoot.transform.SetParent(transform, false);
            healRoot.transform.localPosition = Vector3.zero;

            GameObject pillar = CreateProceduralDiscObject("HolyLightPillar", transform.position, new Color(1f, 0.98f, 0.65f, 0.45f), 0.72f, 18);
            pillar.transform.SetParent(healRoot.transform, false);
            pillar.transform.localPosition = new Vector3(0f, 0.2f, 0f);
            pillar.transform.localScale = new Vector3(0.65f, 1.45f, 1f);

            float elapsed = 0f;
            while (elapsed < duration && healRoot != null)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float alpha = 1f - t;
                foreach (var sr in healRoot.GetComponentsInChildren<SpriteRenderer>())
                {
                    Color c = sr.color;
                    sr.color = new Color(c.r, c.g, c.b, alpha * 0.75f);
                }
                healRoot.transform.localScale = Vector3.Lerp(Vector3.one * 0.75f, Vector3.one * 1.05f, t);
                yield return null;
            }
            if (healRoot != null) Destroy(healRoot);
        }

        private IEnumerator SpawnOrbitingShieldsRoutine(float duration, int shieldCount)
        {
            GameObject orbitRoot = new GameObject("IronBastion_OrbitShields");
            orbitRoot.transform.SetParent(transform, false);
            orbitRoot.transform.localPosition = Vector3.zero;

            float radius = 0.68f;
            var shields = new List<Transform>(shieldCount);
            for (int i = 0; i < shieldCount; i++)
            {
                GameObject sh = new GameObject($"Shield_{i}");
                sh.transform.SetParent(orbitRoot.transform, false);
                var sr = sh.AddComponent<SpriteRenderer>();
                sr.sprite = GetOrCreateShieldIconSprite();
                sr.color = Color.white;
                sr.sortingLayerID = GetVfxSortingLayerID();
                sr.sortingOrder = GetVfxSortingOrder(15);
                sh.transform.localScale = Vector3.one * 0.88f;
                shields.Add(sh.transform);
            }

            float elapsed = 0f;
            while (elapsed < duration && orbitRoot != null)
            {
                elapsed += Time.deltaTime;
                float baseAngle = elapsed * 260f;
                for (int i = 0; i < shields.Count; i++)
                {
                    if (shields[i] == null) continue;
                    float ang = (baseAngle + (360f / shields.Count) * i) * Mathf.Deg2Rad;
                    shields[i].localPosition = new Vector3(Mathf.Cos(ang) * radius, Mathf.Sin(ang) * radius * 0.8f, 0f);
                }
                yield return null;
            }

            if (orbitRoot != null) Destroy(orbitRoot);
        }

        private IEnumerator SpawnWhiteManaShieldSphereRoutine(float duration)
        {
            GameObject sphere = CreateProceduralRingObject("MageWhiteManaShield", transform.position, new Color(0.95f, 0.98f, 1f, 0.92f), 1.02f, 19);
            sphere.transform.SetParent(transform, false);
            sphere.transform.localPosition = Vector3.zero;

            GameObject inner = CreateProceduralDiscObject("ManaShieldInner", transform.position, new Color(0.88f, 0.96f, 1f, 0.28f), 0.92f, 17);
            inner.transform.SetParent(sphere.transform, false);
            inner.transform.localPosition = Vector3.zero;

            float elapsed = 0f;
            while (elapsed < duration && sphere != null)
            {
                elapsed += Time.deltaTime;
                sphere.transform.Rotate(0f, 0f, -80f * Time.deltaTime);
                float s = 1.0f + Mathf.Sin(elapsed * 7f) * 0.05f;
                sphere.transform.localScale = new Vector3(s, s, 1f);
                yield return null;
            }
            if (sphere != null) Destroy(sphere);
        }

        private IEnumerator SpawnTremorConeWaveRoutine(Vector3 origin, Vector2 dir, float maxDist)
        {
            Vector2 normDir = dir.sqrMagnitude > 0.001f ? dir.normalized : Vector2.right;
            float baseAngle = Mathf.Atan2(normDir.y, normDir.x) * Mathf.Rad2Deg;
            Sprite rockSprite = CodeArtSpriteBank.LoadSprite("vfx_rock_spike", new Vector2(0.5f, 0.09f), 32f) ?? GetOrCreateSpikeSprite();

            for (int step = 1; step <= 4; step++)
            {
                float d = (maxDist / 4f) * step;
                float spread = step * 0.35f;
                for (int s = -1; s <= 1; s++)
                {
                    float ang = (baseAngle + s * 18f) * Mathf.Deg2Rad;
                    Vector3 pos = origin + new Vector3(Mathf.Cos(ang), Mathf.Sin(ang), 0f) * d;
                    GameObject spike = CreateProceduralSpikeObject("TremorRockSpike", pos, Color.white, 0.60f + step * 0.08f);
                    var sr = spike.GetComponent<SpriteRenderer>();
                    if (sr != null) sr.sprite = rockSprite;
                    Destroy(spike, 0.42f);
                }
                StartCoroutine(ExpandShockwaveRingRoutine(origin + (Vector3)(normDir * d), new Color(0.92f, 0.68f, 0.28f, 0.85f), 0.25f, spread * 1.15f, 0.20f));
                yield return new WaitForSeconds(0.045f);
            }
        }

        private IEnumerator SpawnEarthshatterShockwaveRoutine(Vector3 center, float radius)
        {
            StartCoroutine(ExpandShockwaveRingRoutine(center, new Color(1f, 0.65f, 0.2f, 0.95f), 0.35f, radius * 0.85f, 0.30f));
            StartCoroutine(ExpandShockwaveRingRoutine(center, new Color(1f, 0.9f, 0.35f, 0.85f), 0.2f, radius * 0.58f, 0.24f));

            Sprite rockSprite = CodeArtSpriteBank.LoadSprite("vfx_rock_spike", new Vector2(0.5f, 0.09f), 32f) ?? GetOrCreateSpikeSprite();
            int rockCount = 8;
            for (int i = 0; i < rockCount; i++)
            {
                float ang = (360f / rockCount) * i * Mathf.Deg2Rad;
                Vector3 pos = center + new Vector3(Mathf.Cos(ang), Mathf.Sin(ang), 0f) * (radius * 0.58f);
                GameObject rock = CreateProceduralSpikeObject("EarthshatterRock", pos, Color.white, 0.82f);
                var sr = rock.GetComponent<SpriteRenderer>();
                if (sr != null) sr.sprite = rockSprite;
                Destroy(rock, 0.50f);
            }
            yield return null;
        }

        private IEnumerator SpawnEruptingIceSpikesVisualRoutine(Vector3 center, float radius, float duration)
        {
            Color iceCyan = new Color(0.28f, 0.92f, 1f, 0.95f);

            // Vết nứt mặt đất + vòng ấn băng giá nơi cụm chông băng phá đất trồi lên
            GameObject groundCrack = CreateProceduralDiscObject("FrostGroundCrack", center + new Vector3(0f, -0.06f, 0f), new Color(0.08f, 0.14f, 0.24f, 0.92f), 1.0f, 12);
            groundCrack.transform.localScale = new Vector3(1.38f, 0.26f, 1f);

            GameObject frostSeal = CreateProceduralRingObject("FrostGroundSeal", center + new Vector3(0f, -0.05f, 0f), iceCyan, 1.15f, 13);
            frostSeal.transform.localScale = new Vector3(1.28f, 0.65f, 1f);
            StartCoroutine(ExpandShockwaveRingRoutine(center, iceCyan, 0.35f, 1.35f, 0.24f));

            // Cấu trúc Cụm Chông Băng (7 mũi băng sắc nét kết cụm, mũi chính cao ở giữa, các mũi phụ nghiêng nhẹ tỏa ra xung quanh)
            Vector3[] offsets =
            {
                new Vector3(-0.36f,  0.08f, 0f), // Sau trái
                new Vector3( 0.36f,  0.08f, 0f), // Sau phải
                new Vector3(-0.19f,  0.03f, 0f), // Giữa trái
                new Vector3( 0.19f,  0.03f, 0f), // Giữa phải
                new Vector3( 0.00f,  0.00f, 0f), // Mũi chông băng chủ đạo chính giữa
                new Vector3(-0.26f, -0.06f, 0f), // Trước trái
                new Vector3( 0.26f, -0.06f, 0f)  // Trước phải
            };
            float[] tiltAngles = { 16f, -16f, 9f, -9f, 0f, 13f, -13f };
            float[] targetScales = { 0.98f, 0.98f, 1.22f, 1.22f, 1.52f, 0.92f, 0.92f };
            float[] staggerDelays = { 0.04f, 0.04f, 0.02f, 0.02f, 0.0f, 0.05f, 0.05f };
            int[] sortOffsets = { 18, 18, 19, 19, 20, 21, 21 };

            Sprite frostSpikeSpr = GetOrCreateSpikeSprite();
            var spikes = new List<GameObject>(offsets.Length);
            for (int i = 0; i < offsets.Length; i++)
            {
                Vector3 basePos = center + offsets[i] + new Vector3(0f, -0.14f, 0f);
                GameObject sp = CreateProceduralSpikeObject($"FrostSpikeCluster_{i}", basePos, Color.white, targetScales[i]);
                sp.transform.rotation = Quaternion.Euler(0f, 0f, tiltAngles[i]);
                sp.transform.localScale = new Vector3(targetScales[i] * 0.85f, 0.05f, 1f);
                var sr = sp.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    sr.sprite = frostSpikeSpr;
                    sr.sortingLayerID = GetVfxSortingLayerID();
                    sr.sortingOrder = GetVfxSortingOrder(sortOffsets[i]);
                    sr.color = (i == 4) ? Color.white : new Color(0.88f, 0.98f, 1f, 1.0f);
                }
                spikes.Add(sp);
            }

            // Giai đoạn 1: Các mũi chông băng trồi mạnh từ dưới vết nứt mặt đất lên (0.24s)
            float riseTime = 0.24f;
            float elapsed = 0f;
            while (elapsed < riseTime)
            {
                elapsed += Time.deltaTime;
                for (int i = 0; i < spikes.Count; i++)
                {
                    if (spikes[i] == null) continue;
                    float localT = Mathf.Clamp01((elapsed - staggerDelays[i]) / Mathf.Max(0.05f, riseTime - staggerDelays[i]));
                    // Đường cong bật nảy sắc nét khi mũi băng phá mặt đất chui lên
                    float overshoot = Mathf.Sin(localT * Mathf.PI) * 0.14f;
                    float curScaleY = Mathf.Clamp(Mathf.Max(0.05f, localT + overshoot), 0.05f, 1.14f) * targetScales[i];
                    float curScaleX = targetScales[i] * Mathf.Lerp(0.70f, 0.96f, localT);

                    spikes[i].transform.position = center + offsets[i] + new Vector3(0f, Mathf.Lerp(-0.14f, 0f, localT), 0f);
                    spikes[i].transform.localScale = new Vector3(curScaleX, curScaleY, 1f);
                }
                yield return null;
            }

            SpawnHitSpark(center + new Vector3(0f, 0.22f, 0f), iceCyan);

            // Giai đoạn 2: Giữ cụm chông băng sừng sững trên mặt đất
            float holdTime = Mathf.Max(0.25f, duration - riseTime - 0.18f);
            yield return new WaitForSeconds(holdTime);

            // Giai đoạn 3: Cụm chông băng lún nhanh xuống vết nứt mặt đất và tan biến
            float sinkTime = 0.18f;
            elapsed = 0f;
            while (elapsed < sinkTime)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / sinkTime);
                for (int i = 0; i < spikes.Count; i++)
                {
                    if (spikes[i] == null) continue;
                    float sY = Mathf.Lerp(targetScales[i], 0.0f, t);
                    spikes[i].transform.position = center + offsets[i] + new Vector3(0f, Mathf.Lerp(0f, -0.12f, t), 0f);
                    spikes[i].transform.localScale = new Vector3(targetScales[i] * 0.88f, sY, 1f);
                    var sr = spikes[i].GetComponent<SpriteRenderer>();
                    if (sr != null) sr.color = new Color(0.85f, 0.98f, 1f, 1f - t);
                }
                yield return null;
            }

            if (groundCrack != null) Destroy(groundCrack);
            if (frostSeal != null) Destroy(frostSeal);
            foreach (var sp in spikes)
            {
                if (sp != null) Destroy(sp);
            }
        }

        private IEnumerator SpawnFallingArrowFromSkyRoutine(Vector3 startPos, Vector3 endPos, Color arrowColor, float fallDuration)
        {
            GameObject arrow = CreateProceduralArrowObject("FallingVolleyArrow", startPos, arrowColor);
            arrow.transform.localScale = Vector3.one * 0.85f;
            Vector2 dir = (endPos - startPos).normalized;
            arrow.transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg);

            float elapsed = 0f;
            while (elapsed < fallDuration && arrow != null)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / fallDuration);
                arrow.transform.position = Vector3.Lerp(startPos, endPos, t);
                yield return null;
            }
            if (arrow != null) Destroy(arrow);
            SpawnHitSpark(endPos, arrowColor);
        }

        private IEnumerator AnimateTrapSnapAndRootRoutine(GameObject trapObj, Vector3 targetPos, float rootDuration)
        {
            if (trapObj != null)
            {
                trapObj.transform.position = targetPos;
                foreach (var sr in trapObj.GetComponentsInChildren<SpriteRenderer>())
                {
                    sr.color = new Color(1f, 0.35f, 0.2f, 1f);
                }
                trapObj.transform.localScale = Vector3.one * 0.98f;
            }
            StartCoroutine(ExpandShockwaveRingRoutine(targetPos, new Color(1f, 0.85f, 0.25f, 0.95f), 0.95f, 0.42f, 0.20f));
            yield return new WaitForSeconds(rootDuration);
            if (trapObj != null) Destroy(trapObj);
        }

        private void SpawnProceduralFanWave(Vector3 origin, Vector2 dir, Color color, float scale, float duration)
        {
            SpawnProceduralSlashWave(origin + (Vector3)(dir.normalized * 0.58f), dir, color, scale * 0.72f, duration);
        }

        private void SpawnProceduralSlashWave(Vector3 pos, Vector2 dir, Color color, float scale, float duration)
        {
            GameObject wave = new GameObject("ProceduralSlashWave");
            wave.transform.position = pos;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            wave.transform.rotation = Quaternion.Euler(0f, 0f, angle);
            float fitScale = Mathf.Clamp(scale * 0.56f, 0.65f, 1.35f);
            wave.transform.localScale = new Vector3(fitScale, fitScale, 1f);

            var sr = wave.AddComponent<SpriteRenderer>();
            Sprite[] seq = CodeArtSpriteBank.LoadFrameSequence("fx_slash_arc", 4, new Vector2(0.5f, 0.5f), 32f);
            sr.sprite = (seq != null && seq.Length > 0) ? seq[0] : GetOrCreateSlashWaveSprite();
            sr.color = color;
            sr.sortingLayerID = GetVfxSortingLayerID();
            sr.sortingOrder = GetVfxSortingOrder(16);

            if (seq != null && seq.Length > 1)
                StartCoroutine(PlaySpriteSequenceAndDestroyRoutine(sr, seq, duration));
            else
                StartCoroutine(FadeAndDestroySpriteRoutine(sr, duration));
        }

        private IEnumerator PlaySpriteSequenceAndDestroyRoutine(SpriteRenderer sr, Sprite[] frames, float duration)
        {
            if (sr == null || frames == null || frames.Length == 0) yield break;
            Color initCol = sr.color;
            float elapsed = 0f;
            while (elapsed < duration && sr != null)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / Mathf.Max(0.05f, duration));
                int idx = Mathf.Clamp(Mathf.FloorToInt(t * frames.Length), 0, frames.Length - 1);
                sr.sprite = frames[idx];
                if (t > 0.65f)
                {
                    float fade = 1f - ((t - 0.65f) / 0.35f);
                    sr.color = new Color(initCol.r, initCol.g, initCol.b, initCol.a * fade);
                }
                yield return null;
            }
            if (sr != null && sr.gameObject != null) Destroy(sr.gameObject);
        }

        private IEnumerator ExpandShockwaveRingRoutine(Vector3 center, Color color, float startScale, float endScale, float duration)
        {
            GameObject ring = CreateProceduralRingObject("ShockwaveRing", center, color, startScale * 0.72f, 20);
            var sr = ring.GetComponent<SpriteRenderer>();
            Sprite[] ringSeq = CodeArtSpriteBank.LoadFrameSequence("fx_shock_ring", 5, new Vector2(0.5f, 0.5f), 32f);
            float elapsed = 0f;
            while (elapsed < duration && ring != null)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float s = Mathf.Lerp(startScale * 0.72f, endScale * 0.72f, t);
                ring.transform.localScale = new Vector3(s, s, 1f);
                if (sr != null)
                {
                    if (ringSeq != null && ringSeq.Length > 0)
                    {
                        int idx = Mathf.Clamp(Mathf.FloorToInt(t * ringSeq.Length), 0, ringSeq.Length - 1);
                        sr.sprite = ringSeq[idx];
                    }
                    sr.color = new Color(color.r, color.g, color.b, Mathf.Lerp(color.a, 0f, t));
                }
                yield return null;
            }
            if (ring != null) Destroy(ring);
        }

        private void SpawnLightningBoltLine(Vector3 start, Vector3 end, Color color, float duration)
        {
            GameObject bolt = new GameObject("YellowLightningBolt");
            LineRenderer lr = bolt.AddComponent<LineRenderer>();
            ConfigureLaserLineRenderer(lr, 0.11f, color, 24);

            int segments = 6;
            lr.positionCount = segments + 1;
            Vector2 diff = end - start;
            Vector2 perp = new Vector2(-diff.y, diff.x).normalized;

            lr.SetPosition(0, start);
            for (int i = 1; i < segments; i++)
            {
                float t = i / (float)segments;
                Vector3 basePt = Vector3.Lerp(start, end, t);
                float jitter = Random.Range(-0.18f, 0.18f);
                lr.SetPosition(i, basePt + (Vector3)(perp * jitter));
            }
            lr.SetPosition(segments, end);

            Destroy(bolt, duration);
        }

        private void ConfigureLaserLineRenderer(LineRenderer lr, float width, Color color, int sortingOrderOffset)
        {
            if (s_cachedLineMaterial == null)
            {
                Shader sh = Shader.Find("Sprites/Default");
                if (sh != null) s_cachedLineMaterial = new Material(sh);
            }
            if (s_cachedLineMaterial != null) lr.material = s_cachedLineMaterial;
            lr.useWorldSpace = true;
            lr.startWidth = width;
            lr.endWidth = width;
            lr.startColor = color;
            lr.endColor = color;
            lr.numCapVertices = 2;
            lr.sortingLayerID = GetVfxSortingLayerID();
            lr.sortingOrder = GetVfxSortingOrder(sortingOrderOffset);
        }

        private GameObject CreateProceduralRingObject(string name, Vector3 pos, Color color, float scale, int sortOrderOffset)
        {
            GameObject go = new GameObject(name);
            go.transform.position = pos;
            go.transform.localScale = new Vector3(scale, scale, 1f);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = GetOrCreateRingSprite();
            sr.color = color;
            sr.sortingLayerID = GetVfxSortingLayerID();
            sr.sortingOrder = GetVfxSortingOrder(sortOrderOffset);
            return go;
        }

        private GameObject CreateProceduralDiscObject(string name, Vector3 pos, Color color, float scale, int sortOrderOffset)
        {
            GameObject go = new GameObject(name);
            go.transform.position = pos;
            go.transform.localScale = new Vector3(scale, scale, 1f);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = GetOrCreateDiscSprite();
            sr.color = color;
            sr.sortingLayerID = GetVfxSortingLayerID();
            sr.sortingOrder = GetVfxSortingOrder(sortOrderOffset);
            return go;
        }

        private GameObject CreateProceduralSpikeObject(string name, Vector3 pos, Color color, float scale)
        {
            GameObject go = new GameObject(name);
            go.transform.position = pos;
            go.transform.localScale = new Vector3(scale, scale, 1f);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = GetOrCreateSpikeSprite();
            sr.color = color;
            sr.sortingLayerID = GetVfxSortingLayerID();
            sr.sortingOrder = GetVfxSortingOrder(15);
            return go;
        }

        private GameObject CreateProceduralArrowObject(string name, Vector3 pos, Color color)
        {
            GameObject go = new GameObject(name);
            go.transform.position = pos;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = GetOrCreateArrowSprite();
            sr.color = color;
            sr.sortingLayerID = GetVfxSortingLayerID();
            sr.sortingOrder = GetVfxSortingOrder(18);
            return go;
        }

        private GameObject CreateDeadeyeCrosshairVisual(Vector3 pos)
        {
            GameObject go = new GameObject("Deadeye_Crosshair");
            go.transform.position = pos;
            go.transform.localScale = Vector3.one * 0.90f;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = GetOrCreateCrosshairSprite();
            sr.color = Color.white;
            sr.sortingLayerID = GetVfxSortingLayerID();
            sr.sortingOrder = GetVfxSortingOrder(30);
            return go;
        }

        private GameObject CreateCaitlynTrapVisual(Vector3 pos)
        {
            GameObject go = new GameObject("Caitlyn_SnareTrap");
            go.transform.position = pos;
            go.transform.localScale = Vector3.one * 0.88f;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = GetOrCreateTrapSprite();
            sr.color = Color.white;
            sr.sortingLayerID = GetVfxSortingLayerID();
            sr.sortingOrder = GetVfxSortingOrder(4);

            // Thêm vòng chỉ báo phạm vi bẫy dưới đất rõ nét
            GameObject trapRing = CreateProceduralRingObject("Trap_RadiusRing", pos, new Color(1f, 0.65f, 0.2f, 0.48f), 0.78f, 3);
            trapRing.transform.SetParent(go.transform, false);
            trapRing.transform.localPosition = Vector3.zero;
            return go;
        }

        private static Sprite GetOrCreateRingSprite()
        {
            if (s_cachedRingSprite != null) return s_cachedRingSprite;
            s_cachedRingSprite = CodeArtSpriteBank.LoadSprite("fx_shock_ring-03", new Vector2(0.5f, 0.5f), 32f);
            if (s_cachedRingSprite != null) return s_cachedRingSprite;

            int size = 32;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            Vector2 center = new Vector2(size * 0.5f, size * 0.5f);
            float maxR = size * 0.45f;
            float minR = size * 0.35f;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float d = Vector2.Distance(new Vector2(x, y), center);
                    tex.SetPixel(x, y, (d >= minR && d <= maxR) ? Color.white : Color.clear);
                }
            }
            tex.Apply();
            s_cachedRingSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 32f);
            return s_cachedRingSprite;
        }

        private static Sprite GetOrCreateDiscSprite()
        {
            if (s_cachedDiscSprite != null) return s_cachedDiscSprite;
            int size = 24;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            Vector2 center = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);
            float maxR = size * 0.45f;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float d = Vector2.Distance(new Vector2(x, y), center);
                    if (d <= maxR * 0.55f)
                        tex.SetPixel(x, y, Color.white);
                    else if (d <= maxR)
                        tex.SetPixel(x, y, new Color(1f, 1f, 1f, 0.65f));
                    else
                        tex.SetPixel(x, y, Color.clear);
                }
            }
            tex.Apply();
            s_cachedDiscSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 32f);
            return s_cachedDiscSprite;
        }

        private static Sprite GetOrCreateSlashWaveSprite()
        {
            if (s_cachedSlashWaveSprite != null) return s_cachedSlashWaveSprite;
            s_cachedSlashWaveSprite = CodeArtSpriteBank.LoadSprite("fx_slash_arc-01", new Vector2(0.5f, 0.5f), 32f);
            return s_cachedSlashWaveSprite;
        }

        private static Sprite GetOrCreateShieldIconSprite()
        {
            if (s_cachedShieldSprite != null) return s_cachedShieldSprite;
            s_cachedShieldSprite = CodeArtSpriteBank.LoadSprite("warrior_shield_icon", new Vector2(0.5f, 0.5f), 32f);
            return s_cachedShieldSprite;
        }

        private static Sprite GetOrCreateSpikeSprite()
        {
            if (s_cachedSpikeSprite != null) return s_cachedSpikeSprite;
            s_cachedSpikeSprite = CodeArtSpriteBank.LoadSprite("mage_frost_spike", new Vector2(0.5f, 0.05f), 32f);
            if (s_cachedSpikeSprite != null) return s_cachedSpikeSprite;

            // Fallback Pixel Art 14x22 sắc nét cho mũi chông băng (FilterMode.Point, 32 PPU)
            int w = 14, h = 22;
            Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            Color outline = new Color(0.10f, 0.11f, 0.17f, 1f);
            Color deepIce = new Color(0.25f, 0.65f, 0.96f, 1f);
            Color cyanIce = new Color(0.45f, 0.94f, 0.97f, 1f);
            for (int y = 0; y < h; y++)
            {
                float progress = y / (float)(h - 1);
                int halfW = Mathf.RoundToInt((1f - progress) * 5.2f);
                for (int x = 0; x < w; x++)
                {
                    int dx = x - w / 2;
                    int absDx = Mathf.Abs(dx);
                    if (y >= 1 && y <= 20 && absDx <= halfW)
                    {
                        if (absDx == halfW || y == 1 || y == 20)
                            tex.SetPixel(x, y, outline);
                        else if (dx == 0)
                            tex.SetPixel(x, y, Color.white);
                        else if (dx < 0)
                            tex.SetPixel(x, y, cyanIce);
                        else
                            tex.SetPixel(x, y, deepIce);
                    }
                    else
                    {
                        tex.SetPixel(x, y, Color.clear);
                    }
                }
            }
            tex.Apply();
            s_cachedSpikeSprite = Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.05f), 32f);
            return s_cachedSpikeSprite;
        }

        private static Sprite GetOrCreateCrosshairSprite()
        {
            if (s_cachedCrosshairSprite != null) return s_cachedCrosshairSprite;
            s_cachedCrosshairSprite = CodeArtSpriteBank.LoadSprite("archer_crosshair", new Vector2(0.5f, 0.5f), 32f);
            return s_cachedCrosshairSprite;
        }

        private static Sprite GetOrCreateTrapSprite()
        {
            if (s_cachedTrapSprite != null) return s_cachedTrapSprite;
            s_cachedTrapSprite = CodeArtSpriteBank.LoadSprite("archer_snare_trap", new Vector2(0.5f, 0.5f), 32f);
            return s_cachedTrapSprite;
        }

        private static Sprite GetOrCreateArrowSprite()
        {
            if (s_cachedArrowSprite != null) return s_cachedArrowSprite;
            s_cachedArrowSprite = CodeArtSpriteBank.LoadSprite("archer_arrow", new Vector2(0.5f, 4f / 9f), 32f);
            return s_cachedArrowSprite;
        }

        private void ApplyDirectHit(Collider2D col, float damage, ElementType element, bool isCrit, Vector2 knockDir, float knockForce, DamageType dmgType = DamageType.Physical)
        {
            if (col == null || col.gameObject == gameObject) return;
            DamageInfo info = new DamageInfo(damage, element, isCrit, gameObject, dmgType)
            {
                knockbackDirection = knockDir,
                knockbackForce = knockForce
            };
            ApplyDirectHit(col, info);
        }

        private void ApplyDirectHit(Collider2D col, DamageInfo info)
        {
            if (col == null || col.gameObject == gameObject) return;
            Hurtbox2D hurt = FindHurtbox(col);
            EnemyStats enemy = FindEnemyStats(col);
            if (hurt != null)
            {
                hurt.ReceiveDamage(info);
            }
            else if (enemy != null)
            {
                enemy.TakeDamage(info);
            }
        }

        /// <summary>
        /// Tạo vệt chém hình cung 2D (Slash Arc VFX) quét 360° theo hướng chuột và gây sát thương vùng
        /// </summary>
        private void PerformMeleeSlash(Vector3 center, Vector2 dir, float damage, ElementType element, bool isCrit, float radius, float knockback, DamageType dmgType = DamageType.Physical)
        {
            Color slashCol = GetElementColor(element, isCrit);

            // 1. Tạo chuỗi khung hình sóng kiếm khí Pixel Art sắc nét (fx_slash_arc) tỷ lệ chuẩn với nhân vật
            SpawnProceduralSlashWave(center, dir, slashCol, Mathf.Clamp(radius * 0.65f, 0.85f, 1.35f), 0.20f);

            // 2. Kiểm tra va chạm diện rộng (OverlapCircleAll) - Chống gây sát thương lặp nhiều lần lên cùng một quái
            Collider2D[] hitColliders = Physics2D.OverlapCircleAll(center, radius, enemyLayers);
            var hitEntities = new HashSet<GameObject>();
            foreach (var col in hitColliders)
            {
                if (col == null || col.gameObject == gameObject) continue;

                Hurtbox2D hurtbox = FindHurtbox(col);
                EnemyStats enemyStats = FindEnemyStats(col);
                if (hurtbox == null && enemyStats == null) continue;
                GameObject targetRoot = hurtbox != null ? hurtbox.transform.root.gameObject : enemyStats.transform.root.gameObject;
                if (!hitEntities.Add(targetRoot)) continue;

                if (hurtbox != null)
                {
                    DamageInfo info = new DamageInfo(damage, element, isCrit, gameObject, dmgType)
                    {
                        knockbackDirection = dir,
                        knockbackForce = knockback
                    };
                    hurtbox.ReceiveDamage(info);
                    SpawnHitSpark(col.transform.position, Color.yellow);
                }
                else if (enemyStats != null)
                {
                    enemyStats.TakeDamage(damage, element, isCrit, dir, dmgType);
                    SpawnHitSpark(col.transform.position, Color.yellow);
                }
            }
        }

        /// <summary>
        /// Phóng đạn/mũi tên bay 360 độ theo vector hướng
        /// </summary>
        private void SpawnProjectile(GameObject prefab, Vector3 spawnPos, Vector2 dir, float damage, ElementType element, bool isCrit, float speed, DamageType dmgType = DamageType.Physical)
        {
            SpawnArcherArrowProjectile(spawnPos, dir, damage, element, isCrit, speed, 1, GetElementColor(element, isCrit), 0.85f, dmgType);
        }

        /// <summary>
        /// Bắn chùm nhiều đạn theo hình nón (Spread)
        /// </summary>
        private void SpawnSpreadProjectiles(GameObject prefab, Vector3 spawnPos, Vector2 centerDir, float damage, ElementType element, bool isCrit, int count, float spreadAngle, DamageType dmgType = DamageType.Physical)
        {
            float baseAngle = Mathf.Atan2(centerDir.y, centerDir.x) * Mathf.Rad2Deg;
            float startAngle = baseAngle - spreadAngle * 0.5f;
            float step = (count > 1) ? (spreadAngle / (count - 1)) : 0f;

            for (int i = 0; i < count; i++)
            {
                float ang = startAngle + step * i;
                float rad = ang * Mathf.Deg2Rad;
                Vector2 dir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
                SpawnProjectile(prefab, spawnPos, dir, damage, element, isCrit, 16f, dmgType);
            }
        }

        /// <summary>
        /// Tạo vòng tròn ma pháp AOE nổ dưới đất tại vị trí chuột
        /// </summary>
        private void SpawnGroundAoe(Vector3 groundPos, float damage, ElementType element, bool isCrit, float radius, DamageType dmgType = DamageType.Physical)
        {
            StartCoroutine(ExpandShockwaveRingRoutine(groundPos, GetElementColor(element, isCrit), 0.35f, radius * 0.85f, 0.24f));
            DamageEnemiesInCircle(groundPos, radius, damage, element, isCrit, dmgType);
        }

        private void DamageEnemiesInCircle(Vector3 center, float radius, float damage, ElementType element, bool isCrit, DamageType dmgType = DamageType.Physical)
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(center, radius, enemyLayers);
            var hitEntities = new HashSet<GameObject>();
            foreach (var col in hits)
            {
                if (col == null || col.gameObject == gameObject) continue;
                Hurtbox2D hurt = FindHurtbox(col);
                EnemyStats enemy = FindEnemyStats(col);
                if (hurt == null && enemy == null) continue;
                GameObject targetRoot = hurt != null ? hurt.transform.root.gameObject : enemy.transform.root.gameObject;
                if (!hitEntities.Add(targetRoot)) continue;

                Vector2 knockDir = ((Vector2)col.transform.position - (Vector2)center).normalized;

                if (hurt != null)
                {
                    hurt.ReceiveDamage(new DamageInfo(damage, element, isCrit, gameObject, dmgType)
                    {
                        knockbackDirection = knockDir,
                        knockbackForce = 6f
                    });
                }
                else if (enemy != null)
                {
                    enemy.TakeDamage(damage, element, isCrit, knockDir, dmgType);
                }
            }
        }

        private void SpawnHitSpark(Vector3 pos, Color tint)
        {
            Sprite[] seq = CodeArtSpriteBank.LoadFrameSequence("fx_hit_spark", 3, new Vector2(0.5f, 0.5f), 32f);
            if (seq != null && seq.Length > 0)
            {
                GameObject spark = new GameObject("HitSpark_CodeArt");
                spark.transform.position = pos;
                spark.transform.localScale = Vector3.one * 0.65f;
                var sr = spark.AddComponent<SpriteRenderer>();
                sr.sprite = seq[0];
                sr.color = tint;
                sr.sortingLayerID = GetVfxSortingLayerID();
                sr.sortingOrder = GetVfxSortingOrder(24);
                StartCoroutine(PlaySpriteSequenceAndDestroyRoutine(sr, seq, 0.16f));
            }
            else
            {
                GameObject disc = CreateProceduralDiscObject("HitSpark_Proc", pos, tint, 0.35f, 24);
                Destroy(disc, 0.16f);
            }
        }

        private Color GetElementColor(ElementType elem, bool crit)
        {
            if (crit) return new Color(1f, 0.85f, 0.2f, 1f); // Vàng rực chí mạng

            switch (elem)
            {
                case ElementType.Fire: return new Color(1f, 0.35f, 0.1f, 1f);
                case ElementType.Water: return new Color(0.2f, 0.7f, 1f, 1f);
                case ElementType.Wind: return new Color(0.4f, 1f, 0.5f, 1f);
                case ElementType.Electric: return new Color(1f, 1f, 0.3f, 1f);
                case ElementType.Earth: return new Color(0.8f, 0.55f, 0.3f, 1f);
                case ElementType.Poison: return new Color(0.5f, 0.9f, 0.2f, 1f);
                case ElementType.Angel: return new Color(1f, 1f, 0.9f, 1f);
                case ElementType.Demon: return new Color(0.6f, 0.2f, 0.9f, 1f);
                default: return new Color(0.85f, 0.95f, 1f, 1f);
            }
        }
        #endregion

        #region Quản Lý Dữ Liệu Kỹ Năng & Class
        private ClassBranch GetCurrentBranch()
        {
            if (ClassSystem.Instance != null && ClassSystem.Instance.currentClass != null)
                return ClassSystem.Instance.currentClass.branch;
            if (classSystem != null && classSystem.currentClass != null)
                return classSystem.currentClass.branch;
            if (hasLoadedSkills)
                return loadedBranch;
            if (visualSwitcher != null)
                return visualSwitcher.CurrentBranch;
            return ClassBranch.Novice;
        }

        /// <summary>Nạp trực tiếp 6 kỹ năng của class vào các phím [1..6] và reset hồi chiêu.</summary>
        public void LoadClassSkills(CharacterClassData data)
        {
            if (data == null) return;
            List<SkillData> skills = ClassEvolutionManager.LoadSkillsForClass(data);
            ApplySkills(skills, data.branch);
        }

        public void ApplySkills(List<SkillData> skills, ClassBranch branch)
        {
            if (fallbackSkills.Count == 0) BuildFallbackClassSkills();

            // Reset toàn bộ hồi chiêu cũ
            var oldIds = new List<string>(skillCooldownTimers.Keys);
            skillCooldownTimers.Clear();
            maxSkillCooldowns.Clear();
            foreach (var id in oldIds) EventBus.OnSkillCooldown?.Invoke(id, 0f);

            fallbackSkills.TryGetValue(branch, out var fb);
            loadedSkills.Clear();
            for (int i = 0; i < 6; i++)
            {
                SkillData s = (skills != null && i < skills.Count) ? skills[i] : null;
                if (s == null && fb != null && i < fb.Count) s = fb[i];
                if (s != null && s.icon == null && fb != null && i < fb.Count)
                {
                    s.icon = fb[i].icon;
                }
                loadedSkills.Add(s);
            }
            loadedBranch = branch;
            hasLoadedSkills = true;

            // Bắn Event để ActionBarUI và GameplayHUD cập nhật ngay lập tức
            EventBus.RaiseSkillChanged(loadedSkills);

            var wsv = EnsureWeaponSocketView();
            if (wsv != null)
            {
                wsv.ApplyClassWeapon(branch);
            }
        }

        /// <summary>Danh sách 6 kỹ năng đang được gán vào [1..6] (dùng cho ActionBar).</summary>
        public List<SkillData> GetActiveSkills()
        {
            var result = new List<SkillData>(6);
            for (int i = 0; i < 6; i++) result.Add(GetSkillForSlot(i));
            return result;
        }

        private void OnEnable()
        {
            EventBus.OnClassApplied += HandleClassApplied;
            EventBus.OnSkillRequested += HandleSkillRequested;
        }

        private void OnDisable()
        {
            EventBus.OnClassApplied -= HandleClassApplied;
            EventBus.OnSkillRequested -= HandleSkillRequested;
        }

        private void HandleSkillRequested(string skillId)
        {
            if (string.IsNullOrEmpty(skillId)) return;
            SkillData skill = loadedSkills.Find(s => s != null && s.skillId == skillId);
            if (skill == null && classSystem != null && classSystem.currentClass != null && classSystem.currentClass.unlockedSkills != null)
            {
                skill = classSystem.currentClass.unlockedSkills.Find(s => s != null && s.skillId == skillId);
            }
            if (skill != null)
            {
                CastSkill(skill);
            }
        }

        private void HandleClassApplied(CharacterClassData data)
        {
            LoadClassSkills(data);
        }

        private SkillData GetSkillForSlot(int slotIndex)
        {
            // 0. Danh sách đã nạp trực tiếp khi đổi class
            if (hasLoadedSkills && slotIndex >= 0 && slotIndex < loadedSkills.Count && loadedSkills[slotIndex] != null)
            {
                return loadedSkills[slotIndex];
            }

            // 1. Kiểm tra trong ClassSystem
            if (classSystem != null && classSystem.currentClass != null)
            {
                var skills = classSystem.currentClass.unlockedSkills;
                if (skills != null && slotIndex >= 0 && slotIndex < skills.Count && skills[slotIndex] != null)
                {
                    return skills[slotIndex];
                }
            }

            // 2. Fallback từ danh sách kỹ năng dự phòng
            ClassBranch branch = GetCurrentBranch();
            if (fallbackSkills.TryGetValue(branch, out var list) && slotIndex >= 0 && slotIndex < list.Count)
            {
                return list[slotIndex];
            }

            return null;
        }

        private void BuildFallbackClassSkills()
        {
            // Novice Skills (6 kỹ năng Tân Thủ chuẩn: 1..6)
            var noviceList = new List<SkillData>
            {
                CreateSkillData("novice_fan_slash", "Đoạn Phong Trảm", "Chém hình quạt 90 độ cự ly rộng gây 140% sát thương.", 10f, 3.0f, 1.4f, SkillTargetType.DirectMelee, ElementType.Wind),
                CreateSkillData("novice_flurry_slash", "Loạn Vũ Liên Trảm", "Kích hoạt chém liên tục 6 nhịp trong 2 giây.", 25f, 7.0f, 2.7f, SkillTargetType.DirectMelee, ElementType.None),
                CreateSkillData("novice_aegis_barrier", "Kim Cang Giáp", "Tăng 15 điểm giáp và tạo 50 Máu Ảo hấp thụ sát thương trong 5s.", 20f, 14.0f, 0f, SkillTargetType.SelfBuff, ElementType.Earth),
                CreateSkillData("novice_quad_dash", "Tứ Bộ Du Long", "Lướt tới 4 đoạn ngắn liên tiếp né đòn với tốc độ cao.", 15f, 10.0f, 0f, SkillTargetType.SelfBuff, ElementType.Wind),
                CreateSkillData("novice_holy_heal", "Ánh Sáng Phục Hồi", "Kêu gọi thánh quang phục hồi tức thì 60 HP.", 35f, 22.0f, 1.0f, SkillTargetType.SelfBuff, ElementType.Angel),
                CreateSkillData("melee_ultimate_crush", "Khai Sơn Bạt Địa (Tuyệt Kỹ)", "Bay người lên cao sau đó dậm mạnh xuống mặt đất diện rộng.", 40f, 10.0f, 4.2f, SkillTargetType.AreaOfEffect, ElementType.Earth)
            };
            fallbackSkills[ClassBranch.Novice] = noviceList;

            // 1. Warrior Skills (6 kỹ năng Đấu Sĩ chuẩn: 1..6)
            var warriorList = new List<SkillData>
            {
                CreateSkillData("warrior_cleave", "Trảm Kích (Cleave)", "Vung vũ khí theo hình bán nguyệt phía trước, gây sát thương vật lý lên toàn bộ mục tiêu trúng đòn.", 12f, 2.5f, 1.8f, SkillTargetType.DirectMelee, ElementType.None),
                CreateSkillData("warrior_vanguard_charge", "Xung Phong (Vanguard Charge)", "Lao thẳng về hướng chỉ định một đoạn ngắn; nếu chạm kẻ địch đầu tiên sẽ gây choáng nhẹ (0.75s) và đẩy lùi nhẹ mục tiêu.", 16f, 4.5f, 1.5f, SkillTargetType.DirectMelee, ElementType.None),
                CreateSkillData("warrior_iron_bastion", "Thiết Giáp (Iron Bastion)", "Dựng lá chắn hộ thể xoay quanh thân mình, tạo một lớp giáp ảo hấp thụ sát thương trong 3 giây.", 18f, 7.0f, 1.0f, SkillTargetType.SelfBuff, ElementType.Earth),
                CreateSkillData("warrior_tremor_slam", "Địa Chấn (Tremor Slam)", "Cắm thanh kiếm xuống mặt đất tạo sóng chấn động hình nón, làm chậm 40% tốc độ di chuyển trong 2 giây.", 22f, 6.0f, 2.2f, SkillTargetType.AreaOfEffect, ElementType.Earth),
                CreateSkillData("warrior_frenzied_strike", "Cuồng Nộ Kích (Frenzied Strike)", "Bao quanh cơ thể ánh sáng đỏ rồi giáng đòn tất sát đơn mục tiêu.", 26f, 9.0f, 3.2f, SkillTargetType.DirectMelee, ElementType.Fire),
                CreateSkillData("melee_ultimate_crush", "Khai Sơn Bạt Địa (Tuyệt Kỹ)", "Bay người lên cao sau đó dậm mạnh xuống mặt đất diện rộng.", 40f, 10.0f, 4.2f, SkillTargetType.AreaOfEffect, ElementType.Earth)
            };
            fallbackSkills[ClassBranch.Warrior] = warriorList;

            // 2. Mage Skills (6 kỹ năng Pháp Sư chuẩn: 1..6)
            var mageList = new List<SkillData>
            {
                CreateSkillData("mage_fire_orb", "Hỏa Cầu (Fire Orb)", "Hiện gậy phép phát sáng đỏ và phóng quả cầu lửa bay thẳng theo vị trí con trỏ chuột.", 15f, 2.5f, 2.0f, SkillTargetType.Projectile, ElementType.Fire, DamageType.Magical),
                CreateSkillData("mage_frost_spikes", "Băng Phong Thược (Frost Spikes)", "Hiện gậy phép phát sáng xanh và triệu hồi chông băng nhô lên tại vị trí con trỏ chuột.", 20f, 4.5f, 2.2f, SkillTargetType.AreaOfEffect, ElementType.Water, DamageType.Magical),
                CreateSkillData("mage_blink", "Thiểm Dịch (Blink / Arcane Shift)", "Biến ảnh tức thời về hướng con trỏ chuột, để lại tàn ảnh tại vị trí trước khi biến.", 16f, 5.0f, 1.0f, SkillTargetType.SelfBuff, ElementType.Wind, DamageType.Magical),
                CreateSkillData("mage_chain_lightning", "Lôi Điện Tỏa (Chain Lightning)", "Hiện gậy phép phát sáng vàng và phóng tia sét vào mục tiêu, nảy sang tối đa 3 kẻ địch lân cận.", 26f, 7.5f, 2.5f, SkillTargetType.AreaOfEffect, ElementType.Electric, DamageType.Magical),
                CreateSkillData("mage_mana_shield", "Khiên Hộ Thuật (Mana Shield)", "Hiện gậy phép phát sáng trắng và tạo 1 lớp khiên trắng bao xung quanh cơ thể.", 22f, 10.0f, 1.0f, SkillTargetType.SelfBuff, ElementType.None, DamageType.Magical),
                CreateSkillData("mage_arcane_blast", "Bí Thuật Pháo / Đại Hồng Thủy", "Hiện gậy phép phát sáng đỏ và phóng tia ánh sáng đỏ thành 1 đường thẳng theo hướng con trỏ chuột.", 32f, 8.0f, 3.2f, SkillTargetType.Projectile, ElementType.Fire, DamageType.Magical)
            };
            fallbackSkills[ClassBranch.Mage] = mageList;

            // 3. Archer Skills (6 kỹ năng Xạ Thủ chuẩn: 1..6)
            var archerList = new List<SkillData>
            {
                CreateSkillData("archer_piercing_shot", "Xuyên Vân Tiễn (Piercing Shot)", "Kéo căng cung bắn một phát tên bay xuyên qua mọi kẻ địch trên đường thẳng.", 14f, 2.8f, 2.0f, SkillTargetType.Projectile, ElementType.Wind),
                CreateSkillData("archer_volley", "Mưa Tên (Volley / Barrage)", "Bắn 1 mũi tên lên cao tạo vòng tròn ma pháp trên bầu trời và trút mưa tên xuống phía trước.", 18f, 4.5f, 1.6f, SkillTargetType.Projectile, ElementType.Wind),
                CreateSkillData("archer_tumble", "Nhào Lộn Thoát Thân (Tumble)", "Nhào lộn 360 độ nhanh về hướng con trỏ chuột, cường hóa tốc bắn trong 3 giây.", 12f, 3.5f, 1.0f, SkillTargetType.SelfBuff, ElementType.Wind),
                CreateSkillData("archer_snare_trap", "Bẫy Dây (Snare Trap)", "Đặt bẫy kẹp tại vị trí con trỏ chuột; khi kẻ địch giẫm phải sẽ sập bẫy trói chân 1.25 giây.", 20f, 6.5f, 1.8f, SkillTargetType.AreaOfEffect, ElementType.Earth),
                CreateSkillData("archer_deadeye", "Ngắm Chuẩn Dứt Điểm (Deadeye)", "Hiện tâm ngắm khóa mục tiêu tại con trỏ chuột, dồn lực bắn phát tên uy lực cực đại.", 32f, 10.0f, 3.8f, SkillTargetType.Projectile, ElementType.None),
                CreateSkillData("arch_elemental_storm", "Bão Tố Tiễn Vũ (Tuyệt Kỹ)", "Xả mưa tên năng lượng diện rộng trút xuống vùng chỉ định.", 45f, 10.0f, 3.8f, SkillTargetType.AreaOfEffect, ElementType.Fire)
            };
            fallbackSkills[ClassBranch.Archer] = archerList;
        }

        private SkillData CreateSkillData(string id, string name, string desc, float mana, float cd, float mult, SkillTargetType targetType, ElementType elem, DamageType dmgType = DamageType.Physical)
        {
            var s = ScriptableObject.CreateInstance<SkillData>();
            s.skillId = id;
            s.skillName = name;
            s.description = desc;
            s.manaCost = mana;
            s.cooldownSeconds = cd;
            s.damageMultiplier = mult;
            s.targetType = targetType;
            s.element = elem;
            s.damageType = dmgType;
            return s;
        }

        private void LoadOrCreateVfxPrefabs()
        {
            // Tự động gán Prefabs hoặc nạp từ Resources/Art/VFX nếu trong Inspector để trống
#if UNITY_EDITOR
            if (slashVfxPrefab == null)
            {
                slashVfxPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/VFX/SlashArc_VFX.prefab");
            }
            if (arrowPrefab == null)
            {
                arrowPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/VFX/Arrow_Projectile.prefab");
            }
            if (magicOrbPrefab == null)
            {
                magicOrbPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/VFX/MagicOrb_Projectile.prefab");
            }
            if (magicAoePrefab == null)
            {
                magicAoePrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/VFX/MagicAOE_Circle.prefab");
            }
            if (hitSparkPrefab == null)
            {
                hitSparkPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/VFX/HitSpark_VFX.prefab");
            }
            if (fireOrbPrefab == null)
            {
                fireOrbPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/VFX/FireOrb_Projectile.prefab");
            }
            if (fireOrbExplosionPrefab == null)
            {
                fireOrbExplosionPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/VFX/FireOrb_Explosion_VFX.prefab");
            }
#endif
        }
        #endregion

        #region Public Setters & Inspectors
        public void SetAimController(PlayerAimController2D aim) => aimController = aim;
        public void SetEnemyLayers(LayerMask mask) => enemyLayers = mask;
        public void SetVfxPrefabs(GameObject slash, GameObject arrow, GameObject orb, GameObject aoe, GameObject spark)
        {
            slashVfxPrefab = slash;
            arrowPrefab = arrow;
            magicOrbPrefab = orb;
            magicAoePrefab = aoe;
            hitSparkPrefab = spark;
        }
        #endregion
    }
}
