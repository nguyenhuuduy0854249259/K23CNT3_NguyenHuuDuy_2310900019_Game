using System;
using System.Collections.Generic;
using UnityEngine;
using ActionRPG.Combat;
using ActionRPG.Core;
using ActionRPG.Core.Services;
using ActionRPG.Items;

namespace ActionRPG.Characters
{
    public enum EvolutionTier
    {
        Tier1_Novice = 1,       // Cấp 1 - 9: Tân thủ, chưa có Pet, chỉ dùng vũ khí cơ bản
        Tier2_BasicClass = 2,   // Cấp 10 - 19: Mở khóa Pet, vũ khí cơ bản, bắt quái
        Tier3_LuminaClass = 3   // Cấp 20+: Mở khóa 10 chuyên phái Lumina & 3 Ấn nội tại
    }

    public class CharacterStats : MonoBehaviour
    {
        public static CharacterStats PlayerInstance { get; set; }

        [Header("Cấp Độ & Giai Đoạn Tiến Hóa")]
        public int level = 1;
        public int currentLevel => level;
        public int currentExp = 0;
        public int maxExp = 100;

        /// <summary>
        /// Giai đoạn tiến hóa nhân vật theo cấp độ chuẩn Sprint 2:
        /// Tier 1: Cấp 1-9 (Tân thủ)
        /// Tier 2: Cấp 10-19 (Class cơ bản + Đồng hành)
        /// Tier 3: Cấp 20+ (Chuyên phái Lumina)
        /// </summary>
        public EvolutionTier CurrentTier => level < 10 ? EvolutionTier.Tier1_Novice : (level < 20 ? EvolutionTier.Tier2_BasicClass : EvolutionTier.Tier3_LuminaClass);

        [Header("Mở Khóa Tính Năng Pet (Sprint 1 & 2)")]
        [Tooltip("Cấp độ yêu cầu để chính thức mở khóa hệ thống Pet và tính năng thu phục (Mặc định: Cấp 10)")]
        public int petUnlockLevel = 10;
        [Tooltip("Cờ trạng thái mở khóa Hệ thống Đồng Hành")]
        public bool isPetUnlocked = false;

        [Header("Chỉ Số Sinh Mệnh & Năng Lượng")]
        public float maxHp = 100f;
        public float currentHp = 100f;
        public float maxMp = 50f;
        public float currentMp = 50f;
        public float temporaryShieldHp = 0f; // Máu ảo / Barrier hấp thụ sát thương

        [Header("Chỉ Số Chiến Đấu (Công - Thủ - Tốc Độ)")]
        [Tooltip("Sát thương vật lý cơ bản (Physical Attack)")]
        public float basePhysicalAtk = 15f;

        [Tooltip("Sát thương phép cơ bản (Magic Attack / Spell Power)")]
        public float baseMagicAtk = 15f;

        [Tooltip("Giáp vật lý cơ bản (Physical Defense)")]
        public float basePhysicalDef = 5f;

        [Tooltip("Giáp phép / Kháng phép cơ bản (Magic Defense)")]
        public float baseMagicDef = 5f;

        [Tooltip("Tốc độ di chuyển cơ bản (Move Speed)")]
        public float baseMoveSpeed = 4.5f;

        [Tooltip("Tốc độ tấn công cơ bản (Attack Speed)")]
        public float baseAttackSpeed = 1.0f; // 1.0/s

        [Tooltip("Tỷ lệ bạo kích cơ bản (Crit Rate)")]
        public float baseCritRate = 0.05f; // 5%

        [Tooltip("Sát thương bạo kích cơ bản (Crit Damage)")]
        public float baseCritDamage = 1.5f; // 150%

        // Thuộc tính kế thừa tương thích ngược (Backward Compatibility)
        public float baseAtk { get => basePhysicalAtk; set => basePhysicalAtk = value; }
        public float baseDef { get => basePhysicalDef; set => basePhysicalDef = value; }

        [Header("Hồi Năng Lượng Tự Nhiên (Mana Regeneration)")]
        [Tooltip("Bật/Tắt cơ chế tự động hồi Mana liên tục")]
        public bool enableManaRegen = true;

        [Tooltip("Tốc độ hồi Mana cơ bản mỗi giây (MP/s)")]
        public float baseManaRegen = 15f;

        [Tooltip("Tỷ lệ % Max MP hồi mỗi giây thêm vào (Mặc định 0.1 = 10% Max MP/s)")]
        public float percentManaRegenPerSecond = 0.1f;

        [Tooltip("Hệ số hồi Mana theo Trí Tuệ (INT): mỗi điểm INT tăng thêm MP/s")]
        public float intManaRegenFactor = 0.1f;

        [Header("Chế Độ Thử Nghiệm Game Dev (Testing Mode)")]
        [Tooltip("Bật hồi Mana siêu tốc trong lúc phát triển để thoải mái test skill liên tục")]
        public bool devFastManaRegen = true;

        [Tooltip("Lượng Mana hồi thêm mỗi giây khi bật devFastManaRegen (MP/s)")]
        public float devExtraManaRegenRate = 35f;

        [Tooltip("Chế độ Vô Hạn Mana (khi bật, thi triển kỹ năng không tốn MP và luôn giữ đầy)")]
        public bool infiniteManaForTesting = false;

        [Tooltip("Phím tắt bật/tắt Vô Hạn Mana trong lúc chơi (Mặc định: F4)")]
        public KeyCode toggleInfiniteManaKey = KeyCode.F4;

        [Tooltip("Phím tắt nạp đầy 100% HP & MP tức thì (Mặc định: F3)")]
        public KeyCode instantRefillKey = KeyCode.F3;

        private float lastMpBroadcastTime = 0f;

        [Header("Chỉ Số Thuộc Tính")]
        public ElementType characterElement = ElementType.None;

        [Header("Thuộc Tính Gốc (Attributes)")]
        public int baseStr = 10;
        public int baseDex = 10;
        public int baseInt = 10;
        public int baseVit = 10;

        // Active Food / Potion Buffs
        private float buffAtk = 0f;
        private float buffMAtk = 0f;
        private float buffDef = 0f;
        private float buffMDef = 0f;
        private float buffSpeed = 0f;
        private float buffCrit = 0f;

        public int TotalStr => baseStr + (level - 1) * 2;
        public int TotalDex => baseDex + (level - 1) * 2;
        public int TotalInt => baseInt + (level - 1) * 2;
        public int TotalVit => baseVit + (level - 1) * 2;
        public float TotalCritDamage => baseCritDamage;
        public float TotalAttackSpeed => Mathf.Max(0.5f, baseAttackSpeed + buffSpeed * 0.05f);

        /// <summary>
        /// Tổng tốc độ hồi Mana mỗi giây (MP/s) kết hợp giữa hồi cơ bản, tỷ lệ % Max MP, điểm Trí Tuệ (INT)
        /// và chế độ Dev Fast Mana Regen (hỗ trợ test kỹ năng).
        /// </summary>
        public float TotalManaRegenRate
        {
            get
            {
                if (!enableManaRegen) return 0f;
                float rate = baseManaRegen + (maxMp * percentManaRegenPerSecond) + (TotalInt * intManaRegenFactor);
                if (devFastManaRegen) rate += devExtraManaRegenRate;
                return rate;
            }
        }

        private void Awake()
        {
            PlayerInstance = this;
            maxExp = GetRequiredExpForLevel(level);
            currentHp = maxHp;
            currentMp = maxMp;
            CheckPetUnlock(false);
        }

        private void OnDestroy()
        {
            if (PlayerInstance == this)
            {
                PlayerInstance = null;
            }
        }

        public void CheckPetUnlock(bool notify = true)
        {
            if (!isPetUnlocked && level >= petUnlockLevel)
            {
                isPetUnlocked = true;
                Debug.Log($"<color=cyan>[Pet System] Đạt Cấp {level}! Hệ thống Đồng Hành (Pet) đã được chính thức mở khóa!</color>");
                if (notify)
                {
                    EventBus.OnPetSystemUnlocked?.Invoke();
                }
            }
        }

        private void Start()
        {
            UpdateStatsUI();
        }

        private void Update()
        {
            if (!Application.isPlaying) return;

            if (PlayerInstance == this)
            {
                HandleDevKeyShortcuts();
            }

            HandleManaRegeneration();
        }

        private void HandleDevKeyShortcuts()
        {
            // Phím F4: Bật/Tắt chế độ Vô Hạn Mana để thoải mái test chuỗi kỹ năng
            if (toggleInfiniteManaKey != KeyCode.None && Input.GetKeyDown(toggleInfiniteManaKey))
            {
                infiniteManaForTesting = !infiniteManaForTesting;
                if (infiniteManaForTesting)
                {
                    currentMp = maxMp;
                    UpdateStatsUI();
                    EventBus.OnShowUIMessage?.Invoke("⚡ [DEV TEST] Vô Hạn Mana: ĐÃ BẬT (Thoải mái thử nghiệm kỹ năng!)");
                    Debug.Log("<color=cyan>[CharacterStats] Infinite Mana Mode: ON</color>");
                }
                else
                {
                    EventBus.OnShowUIMessage?.Invoke("⚡ [DEV TEST] Vô Hạn Mana: ĐÃ TẮT");
                    Debug.Log("<color=yellow>[CharacterStats] Infinite Mana Mode: OFF</color>");
                }
            }

            // Phím F3: Hồi đầy 100% HP & MP tức thì
            if (instantRefillKey != KeyCode.None && Input.GetKeyDown(instantRefillKey))
            {
                currentHp = maxHp;
                currentMp = maxMp;
                UpdateStatsUI();
                EventBus.OnShowUIMessage?.Invoke("❤ [DEV TEST] Đã hồi đầy 100% HP & MP!");
            }
        }

        private void HandleManaRegeneration()
        {
            if (!enableManaRegen) return;

            if (infiniteManaForTesting)
            {
                if (currentMp < maxMp)
                {
                    currentMp = maxMp;
                    EventBus.OnPlayerMpChanged?.Invoke(currentMp, maxMp);
                }
                return;
            }

            if (currentMp >= maxMp) return;

            float regenAmount = TotalManaRegenRate * Time.deltaTime;
            currentMp = Mathf.Min(maxMp, currentMp + regenAmount);

            // Cập nhật UI định kỳ mượt mà hoặc khi đầy
            if (Time.time - lastMpBroadcastTime >= 0.08f || currentMp >= maxMp)
            {
                lastMpBroadcastTime = Time.time;
                EventBus.OnPlayerMpChanged?.Invoke(currentMp, maxMp);
            }
        }

        public float TotalPhysicalAtk
        {
            get
            {
                float equipBonus = 0f;
                if (Inventory.Instance != null && Inventory.Instance.equippedItems != null)
                {
                    foreach (var item in Inventory.Instance.equippedItems.Values)
                    {
                        if (item != null) equipBonus += item.bonusAtk;
                    }
                }
                float statBonus = (TotalStr - 10) * 1.0f + (TotalDex - 10) * 0.5f;
                return basePhysicalAtk + equipBonus + buffAtk + Mathf.Max(0f, statBonus);
            }
        }

        public float TotalMagicAtk
        {
            get
            {
                float equipBonus = 0f;
                if (Inventory.Instance != null && Inventory.Instance.equippedItems != null)
                {
                    foreach (var item in Inventory.Instance.equippedItems.Values)
                    {
                        if (item != null) equipBonus += item.bonusMagicAtk > 0 ? item.bonusMagicAtk : item.bonusAtk * 0.8f;
                    }
                }
                float statBonus = (TotalInt - 10) * 1.5f;
                return baseMagicAtk + equipBonus + buffMAtk + Mathf.Max(0f, statBonus);
            }
        }

        public float TotalPhysicalDef
        {
            get
            {
                float equipBonus = 0f;
                if (Inventory.Instance != null && Inventory.Instance.equippedItems != null)
                {
                    foreach (var item in Inventory.Instance.equippedItems.Values)
                    {
                        if (item != null) equipBonus += item.bonusDef;
                    }
                }
                float statBonus = (TotalVit - 10) * 0.6f + (TotalStr - 10) * 0.3f;
                return basePhysicalDef + equipBonus + buffDef + Mathf.Max(0f, statBonus);
            }
        }

        public float TotalMagicDef
        {
            get
            {
                float equipBonus = 0f;
                if (Inventory.Instance != null && Inventory.Instance.equippedItems != null)
                {
                    foreach (var item in Inventory.Instance.equippedItems.Values)
                    {
                        if (item != null) equipBonus += item.bonusMagicDef > 0 ? item.bonusMagicDef : item.bonusDef * 0.7f;
                    }
                }
                float statBonus = (TotalInt - 10) * 0.8f + (TotalVit - 10) * 0.3f;
                return baseMagicDef + equipBonus + buffMDef + Mathf.Max(0f, statBonus);
            }
        }

        public float TotalAtk => TotalPhysicalAtk;
        public float TotalDef => TotalPhysicalDef;

        public float TotalMoveSpeed
        {
            get
            {
                float equipBonus = 0f;
                if (Inventory.Instance != null && Inventory.Instance.equippedItems != null)
                {
                    foreach (var item in Inventory.Instance.equippedItems.Values)
                    {
                        if (item != null) equipBonus += item.bonusSpeed;
                    }
                }
                float speed = baseMoveSpeed + equipBonus + buffSpeed;

                // Kiểm tra nếu đang bị đói giảm 30% tốc độ
                if (HungerSystem.Instance != null && HungerSystem.Instance.IsStarving)
                {
                    speed *= 0.7f;
                }
                return speed;
            }
        }

        public float TotalCritRate
        {
            get
            {
                float equipBonus = 0f;
                if (Inventory.Instance != null && Inventory.Instance.equippedItems != null)
                {
                    foreach (var item in Inventory.Instance.equippedItems.Values)
                    {
                        if (item != null) equipBonus += item.bonusCritRate;
                    }
                }
                return Mathf.Clamp01(baseCritRate + equipBonus + buffCrit);
            }
        }

        /// <summary>
        /// Kiểm tra điều kiện cấp độ và giai đoạn tiến hóa (Tier) để trang bị vật phẩm.
        /// </summary>
        public bool CanEquip(EquipmentData equip, out string reason)
        {
            if (equip == null)
            {
                reason = string.Empty;
                return true;
            }

            if (currentLevel < equip.requiredLevel)
            {
                reason = $"Cần Cấp {equip.requiredLevel} để trang bị vật phẩm này!";
                return false;
            }

            if (CurrentTier < equip.requiredTier)
            {
                reason = $"Cần đạt giai đoạn {equip.requiredTier} để trang bị vật phẩm này!";
                return false;
            }

            // Ràng buộc Tier 1 (Lv 1 - 9): Không cho phép trang bị vũ khí chuyên phái (Cung, Gậy phép, Súng)
            if (CurrentTier == EvolutionTier.Tier1_Novice && equip.equipSlot == EquipSlot.Weapon && equip.isSpecializedWeapon)
            {
                reason = "Tân thủ (Tier 1) chưa thể trang bị vũ khí chuyên phái (Cung, Gậy phép, Súng)! Hãy đạt Cấp 10 để mở khóa.";
                return false;
            }

            reason = string.Empty;
            return true;
        }

        public void GainExp(int exp)
        {
            if (exp <= 0) return;
            currentExp += exp;
            Debug.Log($"[Stats] Nhận +{exp} EXP ({currentExp}/{maxExp})");

            while (currentExp >= maxExp)
            {
                currentExp -= maxExp;
                LevelUp();
            }

            EventBus.OnPlayerExpChanged?.Invoke(currentExp, maxExp);
        }

        public static int GetRequiredExpForLevel(int lvl)
        {
            // Từ Cấp 1 đến 30: Lượng kinh nghiệm yêu cầu được tăng gấp 1.5x - 2.0x (trung bình ~1.75x)
            // để tạo cảm giác thành tựu sâu sắc khi cày cuốc thăng cấp và tiến hóa class nhân vật cấp cao.
            if (lvl <= 1)
            {
                return 175; // Tăng ~1.75x so với mốc 100 EXP cũ
            }

            if (lvl <= 30)
            {
                // Công thức lũy tiến luỹ thừa, nhân hệ số 1.75x tạo độ dốc thử thách
                float baseExp = 100f * Mathf.Pow(1.36f, lvl - 1) + (lvl - 1) * 25f;
                return Mathf.RoundToInt(baseExp * 1.75f);
            }
            else
            {
                // Sau cấp 30 (Khi đã tiến cấp class cấp cao Tier-2)
                float lvl30Exp = (100f * Mathf.Pow(1.36f, 29) + 29 * 25f) * 1.75f;
                float post30 = lvl30Exp * Mathf.Pow(1.20f, lvl - 30);
                return Mathf.RoundToInt(post30);
            }
        }

        private void LevelUp()
        {
            level++;
            maxExp = GetRequiredExpForLevel(level);

            // Tăng chỉ số khi thăng cấp
            maxHp += 18f;
            maxMp += 10f;
            basePhysicalAtk += 3f;
            baseMagicAtk += 3f;
            basePhysicalDef += 1.5f;
            baseMagicDef += 1.5f;

            currentHp = maxHp; // Hồi đầy máu khi lên cấp
            currentMp = maxMp;

            // Kiểm tra và tự động kích hoạt mở khóa Hệ thống Pet nếu đạt cấp độ yêu cầu
            CheckPetUnlock(true);

            Debug.Log($"<color=green>[STATS] CHÚC MỪNG BẠN ĐÃ LÊN CẤP {level}! (EXP cần tiếp theo: {maxExp})</color>");
            EventBus.OnPlayerLevelUp?.Invoke(level);
            EventBus.OnShowDamageText?.Invoke(transform.position + Vector3.up * 0.8f, 0, false, true);
            UpdateStatsUI();
        }

        public void AddBarrier(float amount, float duration = 5f)
        {
            temporaryShieldHp += amount;
            Debug.Log($"<color=cyan>[Barrier] Kích hoạt Máu Ảo +{amount} (Tổng: {temporaryShieldHp}) trong {duration}s!</color>");
            EventBus.OnShowDamageText?.Invoke(transform.position + Vector3.up * 0.5f, amount, false, true);
            EventBus.OnPlayerHpChanged?.Invoke(currentHp, maxHp);
            StartCoroutine(ExpireBarrierRoutine(amount, duration));
        }

        private System.Collections.IEnumerator ExpireBarrierRoutine(float amount, float duration)
        {
            yield return new WaitForSeconds(duration);
            temporaryShieldHp = Mathf.Max(0f, temporaryShieldHp - amount);
            EventBus.OnPlayerHpChanged?.Invoke(currentHp, maxHp);
        }

        public void TakeDamage(DamageInfo damage)
        {
            float elementalMult = ElementalSynergy.GetElementalMultiplier(damage.element, characterElement);
            float finalDamage = DamageCalculationService.CalculateFinalDamage(
                damage.rawDamage,
                damage.damageType,
                TotalPhysicalDef,
                TotalMagicDef,
                elementalMult,
                damage.isCrit,
                TotalCritDamage
            );

            // Ưu tiên trừ vào Máu Ảo (Barrier) trước
            if (temporaryShieldHp > 0f)
            {
                if (temporaryShieldHp >= finalDamage)
                {
                    temporaryShieldHp -= finalDamage;
                    finalDamage = 0f;
                }
                else
                {
                    finalDamage -= temporaryShieldHp;
                    temporaryShieldHp = 0f;
                }
            }

            if (finalDamage > 0f)
            {
                currentHp = Mathf.Max(0f, currentHp - finalDamage);
            }

            EventBus.OnShowDamageText?.Invoke(transform.position, finalDamage, damage.isCrit, false);
            EventBus.OnPlayerHpChanged?.Invoke(currentHp, maxHp);

            if (currentHp <= 0f)
            {
                Die();
            }
        }

        public void Heal(float amount)
        {
            currentHp = Mathf.Min(maxHp, currentHp + amount);
            EventBus.OnShowDamageText?.Invoke(transform.position, amount, false, true);
            EventBus.OnPlayerHpChanged?.Invoke(currentHp, maxHp);
        }

        public void HealHp(float amount) => Heal(amount);

        public void RestoreMp(float amount)
        {
            currentMp = Mathf.Min(maxMp, currentMp + amount);
            EventBus.OnPlayerMpChanged?.Invoke(currentMp, maxMp);
        }

        public bool ConsumeMp(float amount)
        {
            if (infiniteManaForTesting)
            {
                currentMp = maxMp;
                EventBus.OnPlayerMpChanged?.Invoke(currentMp, maxMp);
                return true;
            }

            if (currentMp >= amount)
            {
                currentMp -= amount;
                EventBus.OnPlayerMpChanged?.Invoke(currentMp, maxMp);
                return true;
            }
            return false;
        }

        public void ApplyBuff(BuffStatType stat, float value, float duration)
        {
            StartCoroutine(BuffCoroutine(stat, value, duration));
        }

        private System.Collections.IEnumerator BuffCoroutine(BuffStatType stat, float value, float duration)
        {
            switch (stat)
            {
                case BuffStatType.AttackPower: buffAtk += value; buffMAtk += value; break;
                case BuffStatType.DefensePower: buffDef += value; buffMDef += value; break;
                case BuffStatType.MoveSpeed: buffSpeed += value; break;
                case BuffStatType.CritRate: buffCrit += value; break;
            }

            yield return new WaitForSeconds(duration);

            switch (stat)
            {
                case BuffStatType.AttackPower: buffAtk -= value; buffMAtk -= value; break;
                case BuffStatType.DefensePower: buffDef -= value; buffMDef -= value; break;
                case BuffStatType.MoveSpeed: buffSpeed -= value; break;
                case BuffStatType.CritRate: buffCrit -= value; break;
            }
        }

        private void Die()
        {
            Debug.Log("<color=red>[Stats] Người chơi đã bị hạ gục!</color>");
            // Hồi sinh tại Làng Tân Thủ
            currentHp = maxHp * 0.5f;
            transform.position = Vector3.zero;
            EventBus.OnPlayerHpChanged?.Invoke(currentHp, maxHp);
        }

        public void UpdateStatsUI()
        {
            EventBus.OnPlayerHpChanged?.Invoke(currentHp, maxHp);
            EventBus.OnPlayerMpChanged?.Invoke(currentMp, maxMp);
            EventBus.OnPlayerExpChanged?.Invoke(currentExp, maxExp);
            EventBus.OnStatsChanged?.Invoke();
        }
    }
}
