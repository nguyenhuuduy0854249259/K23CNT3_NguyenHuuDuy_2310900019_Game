using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ActionRPG.Combat;
using ActionRPG.Characters;
using ActionRPG.Core;
using ActionRPG.Core.Services;
using ActionRPG.Items;
using ActionRPG.Items.Loot;

namespace ActionRPG.Enemies
{
    /// <summary>
    /// Quản lý chỉ số, công thức tính sát thương khắc hệ chuẩn Lumina x2.0,
    /// hiệu ứng Hit Flash, hiển thị số nhảy sát thương và rơi đồ khi bị tiêu diệt.
    /// </summary>
    public class EnemyStats : MonoBehaviour
    {
        [Header("Dữ Liệu Cơ Sở")]
        public EnemyData enemyData;

        [Header("Chỉ Số Quái Vật")]
        public string enemyName = "Quái Rừng";
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

        [Header("Phần Thưởng Khi Bị Tiêu Diệt")]
        public int expReward = 25;
        public int goldReward = 15;
        public List<DropEntry> dropTable = new List<DropEntry>();

        private EnemyController controller;
        private SpriteRenderer spriteRenderer;
        private Rigidbody2D rb;
        private Coroutine flashCoroutine;
        private Color originalColor = Color.white;
        private bool isDead = false;

        private void Awake()
        {
            controller = GetComponent<EnemyController>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            rb = GetComponent<Rigidbody2D>();
            if (spriteRenderer != null) originalColor = spriteRenderer.color;

            // Đăng ký nhận sát thương trực tiếp từ Hurtbox2D nếu không có EnemyController
            var hurtbox = GetComponent<Hurtbox2D>();
            if (hurtbox != null && controller == null)
            {
                hurtbox.OnTakeDamageCustom += TakeDamage;
            }

            if (enemyData != null)
            {
                Initialize(enemyData);
            }
            else
            {
                SyncFromController();
            }
        }

        private void Start()
        {
            EnsureOverheadUI();
        }

        private void OnEnable()
        {
            EnsureOverheadUI();
        }

        public void EnsureOverheadUI()
        {
            if (GetComponent<ActionRPG.UI.EnemyOverheadUI>() == null)
            {
                gameObject.AddComponent<ActionRPG.UI.EnemyOverheadUI>();
            }
        }

        public void Initialize(EnemyData data)
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
            expReward = data.expReward;
            goldReward = data.goldReward;

            if (data.dropTable != null && data.dropTable.Count > 0)
            {
                dropTable = new List<DropEntry>(data.dropTable);
            }

            if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null && data.sprite != null)
            {
                spriteRenderer.sprite = data.sprite;
            }
        }

        public void SyncFromController()
        {
            if (controller == null) controller = GetComponent<EnemyController>();
            if (controller != null)
            {
                enemyName = controller.enemyName;
                level = controller.level;
                element = controller.element;
                maxHp = controller.maxHp;
                currentHp = controller.currentHp;
                physicalAtk = controller.physicalAtk;
                magicAtk = controller.magicAtk;
                physicalDef = controller.physicalDef;
                magicDef = controller.magicDef;
                moveSpeed = controller.moveSpeed;
                attackSpeed = controller.attackSpeed;
            }
        }

        #region Tính Toán & Nhận Sát Thương (Lumina x2.0 Elemental Table & Giáp Vật Lý / Giáp Phép)
        /// <summary>
        /// Tính toán sát thương theo chuẩn tương khắc Lumina và giảm trừ theo Giáp tương ứng:
        /// - Sát thương Vật Lý (Physical) -> Giảm trừ theo Giáp Vật Lý (physicalDef).
        /// - Sát thương Phép (Magical) -> Giảm trừ theo Giáp Phép (magicDef).
        /// - Khắc hệ: x2.0 sát thương; Bị kháng: x0.5 sát thương.
        /// - Thanh máu đỏ sẽ rút tương ứng sau khi sát thương đã trừ giáp.
        /// </summary>
        public float TakeDamage(float amount, ElementType attackElement = ElementType.None, bool isCrit = false, Vector2 knockbackDir = default, DamageType dmgType = DamageType.Physical)
        {
            if (isDead) return 0f;

            // 1. Tính hệ số nguyên tố
            float elemMult = ElementalSynergy.GetElementalMultiplier(attackElement, element);

            // 2. Trừ giáp phòng thủ tương ứng (Vật lý hoặc Phép) qua DamageCalculationService
            float finalDamage = DamageCalculationService.CalculateFinalDamage(
                amount,
                dmgType,
                physicalDef,
                magicDef,
                elemMult,
                isCrit
            );

            // 3. Trừ máu
            currentHp = Mathf.Max(0f, currentHp - finalDamage);
            if (controller != null)
            {
                controller.currentHp = currentHp;
            }

            // 4. Số nhảy sát thương
            EventBus.OnShowDamageText?.Invoke(transform.position + Vector3.up * 0.6f, finalDamage, isCrit, false);

            // 5. Hiệu ứng Hit Flash (chớp trắng đỏ 0.1s)
            TriggerHitFlash();

            // 6. Đẩy lùi (Knockback) nhẹ khi trúng đòn
            if (knockbackDir != Vector2.zero && rb != null)
            {
                rb.AddForce(knockbackDir.normalized * 2.5f, ForceMode2D.Impulse);
            }

            // Kích hoạt phản ứng bỏ chạy cho động vật hoang dã
            var wander = GetComponent<ActionRPG.Town.NPCWanderController2D>();
            if (wander != null)
            {
                Vector2 dangerSrc = (Vector2)transform.position - (knockbackDir != Vector2.zero ? knockbackDir : Vector2.right);
                wander.FleeFrom(dangerSrc);
            }

            // 7. Kiểm tra tử vong
            if (currentHp <= 0f)
            {
                Die();
            }

            return finalDamage;
        }

        /// <summary>
        /// Overload tương thích với DamageInfo từ Hurtbox2D.
        /// </summary>
        public void TakeDamage(DamageInfo info)
        {
            Vector2 knockDir = Vector2.zero;
            if (info.attacker != null)
            {
                knockDir = (transform.position - info.attacker.transform.position).normalized;
            }
            TakeDamage(info.rawDamage, info.element, info.isCrit, knockDir, info.damageType);
        }
        #endregion

        #region Hiệu Ứng Trúng Đòn (Hit Flash)
        public void TriggerHitFlash()
        {
            if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null) return;

            if (flashCoroutine != null) StopCoroutine(flashCoroutine);
            flashCoroutine = StartCoroutine(HitFlashRoutine());
        }

        private IEnumerator HitFlashRoutine()
        {
            // Chớp màu trắng ánh đỏ báo hiệu trúng đòn
            spriteRenderer.color = new Color(1f, 0.35f, 0.35f, 1f);
            yield return new WaitForSeconds(0.1f);
            if (spriteRenderer != null)
            {
                spriteRenderer.color = originalColor;
            }
        }
        #endregion

        #region Xử Lý Tiêu Diệt & Rơi Vật Phẩm
        public void Die()
        {
            if (isDead) return;
            isDead = true;

            Debug.Log($"<color=red>[EnemyStats] {enemyName} đã bị hạ gục! Nhận {expReward} EXP.</color>");

            // 1. Thưởng EXP cho người chơi
            if (CharacterStats.PlayerInstance != null)
            {
                CharacterStats.PlayerInstance.GainExp(expReward);
            }
            else
            {
                var sceneStats = FindObjectOfType<CharacterStats>();
                if (sceneStats != null) sceneStats.GainExp(expReward);
            }

            // 2. Rơi Tiền Vàng & Vật Phẩm qua LootDropper hoặc WorldItem (Sprint 3)
            var dropper = GetComponent<LootDropper>();
            if (dropper != null)
            {
                dropper.DropLoot();
            }
            else
            {
                DropLoot();
            }

            // 3. Phát sự kiện toàn cục
            EventBus.OnEnemyKilled?.Invoke(gameObject, expReward, goldReward);

            // 4. Hủy GameObject
            Destroy(gameObject);
        }

        private void DropLoot()
        {
            Vector3 origin = transform.position;

            // Rơi tiền vàng dạng WorldItem
            if (goldReward > 0)
            {
                WorldItem.SpawnGold(origin, goldReward, 1.0f);
            }

            var drops = (dropTable != null && dropTable.Count > 0) ? dropTable : (enemyData != null ? enemyData.dropTable : null);
            if (drops == null || drops.Count == 0)
            {
                // Rơi nguyên liệu động vật mặc định (Da Thú Rừng) cho muông thú
                string lower = enemyName.ToLower();
                if (lower.Contains("hươu") || lower.Contains("thỏ") || lower.Contains("sói") || lower.Contains("deer") || lower.Contains("rabbit") || lower.Contains("wolf") || lower.Contains("boar"))
                {
                    var hideItem = Resources.Load<ItemData>("Items/material_animal_hide");
                    if (hideItem != null)
                    {
                        WorldItem.Spawn(origin, hideItem, Random.Range(1, 3), 1.0f);
                        Debug.Log($"<color=yellow>[Loot Rơi] {enemyName} rơi {hideItem.itemName}!</color>");
                    }
                }
                return;
            }

            foreach (var entry in drops)
            {
                if (entry == null || entry.item == null) continue;

                float roll = Random.Range(0f, 100f);
                if (roll <= entry.dropChancePercent)
                {
                    int qty = Random.Range(entry.minQuantity, entry.maxQuantity + 1);
                    if (qty > 0)
                    {
                        WorldItem.Spawn(origin, entry.item, qty, 1.0f);
                        Debug.Log($"<color=yellow>[Loot Rơi] {enemyName} rơi {entry.item.itemName} x{qty}!</color>");
                    }
                }
            }
        }
        #endregion
    }
}
